using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.OleDb;
using System.Data.Linq;
using System.Xml.Linq;

namespace SUMA
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            ParseProgressBar.Visibility = Visibility.Hidden;
            LoadInfoText.Visibility = Visibility.Hidden;
            DataImpLabel.Visibility = Visibility.Hidden;

            LoadConfigData();

            if(Managers.LocalDBManager.Instance.LoadDataBase())
            {
                CarregaArticlesDataGrid(Managers.DataManager.Instance.GetFitxer().productes);
            }
            else
            {
                System.Environment.Exit(1);
            }
        }

        private void LoadConfigData()
        {
            XDocument config = Managers.FileSystemManager.Instance.LoadConfigXML();

            string local_server_path = "";
            string factu_sol_server_path = "";

            if (config != null)
            {
                List<XElement> servidors_el = config.Element("configuration").Elements("servers").ToList();

                if (servidors_el.Count > 1)
                {
                    local_server_path = servidors_el[0].Attribute("local").Value;
                    factu_sol_server_path = servidors_el[1].Attribute("factu_sol").Value;
                }
            }

            Managers.LocalDBManager.Instance.SetDataBasePath(local_server_path);
            Managers.FactuSolDBManager.Instance.SetDataBasePath(factu_sol_server_path);
        }

        private void LookForFileButton_Click(object sender, RoutedEventArgs e)
        {
            string file_path = Managers.FileSystemManager.Instance.LoadFileDialog("txt");

            if (file_path != "")
            {
                string[] text = Managers.FileSystemManager.Instance.GetFileDataText(file_path);

                Managers.ParserManager.Instance.ParseAsync(System.IO.Path.GetFileName(file_path), text, OnParseTick, OnParseFinish);

                LoadInfoText.Visibility = Visibility.Visible;
                LoadInfoText.Content = "Recuperant informació del .txt...";
            }
        }

        private void OnParseTick(float progress)
        {
            ParseProgressBar.Visibility = Visibility.Visible;
            ParseProgressBar.Value = progress;
        }

        private void OnParseFinish()
        {
            Managers.Fitxer fit = Managers.DataManager.Instance.GetFitxer();

            ParseProgressBar.Visibility = Visibility.Hidden;

            DataImpLabel.Visibility = Visibility.Visible;

            bool correct = Managers.LocalDBManager.Instance.ClearDataBase();

            if (correct)
            {
                LoadInfoText.Content = "Escrivint a la base de dades...";

                correct = Managers.LocalDBManager.Instance.AddProductes(fit.productes);
            }

            if(correct)
            {
                correct = Managers.LocalDBManager.Instance.AddEans(fit.eans);
            }

            if(correct)
            {
                correct = Managers.LocalDBManager.Instance.AddRegistre(fit);
            }

            if(correct)
            {
                CarregaArticlesDataGrid(fit.productes);
            }

            if(!correct)
            {
                UrlText.Text = "";
            }

            LoadInfoText.Visibility = Visibility.Hidden;
        }

        private void ArticlesDatGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ArticlesDatGrid.SelectedItems != null && ArticlesDatGrid.SelectedItems.Count > 0)
            {
                List<Managers.Producte> selected = ArticlesDatGrid.SelectedItems.OfType<Managers.Producte>().ToList();
                CarregaEansDataGrid(selected);
            }
        }

        private void CarregaArticlesDataGrid(List<Managers.Producte> productes)
        {
            Managers.Fitxer fit = Managers.DataManager.Instance.GetFitxer();

            if (fit != null)
            {
                DataImpLabel.Content = "Data importació: " + fit.data_importacio.ToString();
                DataImpLabel.Visibility = Visibility.Visible;

                if (fit != null)
                    UrlText.Text = fit.nom;

                ArticlesDatGrid.Columns.Clear();
                ArticlesDatGrid.Items.Clear();

                ArticlesDatGrid.IsReadOnly = true;

                DataGridTextColumn columnaCodiArt = new DataGridTextColumn();
                columnaCodiArt.Header = "Codi Art";
                columnaCodiArt.Binding = new Binding("codi_article");
                columnaCodiArt.IsReadOnly = true;
                ArticlesDatGrid.Columns.Add(columnaCodiArt);

                DataGridTextColumn columnaProveedor = new DataGridTextColumn();
                columnaProveedor.Header = "Descripcio";
                columnaProveedor.Binding = new Binding("descripcio");
                columnaProveedor.IsReadOnly = true;
                ArticlesDatGrid.Columns.Add(columnaProveedor);

                DataGridTextColumn columnaMarcaDeBaixa = new DataGridTextColumn();
                columnaMarcaDeBaixa.Header = "Marca Baixa";
                columnaMarcaDeBaixa.Binding = new Binding("marca_de_baixa_str");
                columnaMarcaDeBaixa.IsReadOnly = true;
                ArticlesDatGrid.Columns.Add(columnaMarcaDeBaixa);

                DataGridTextColumn columnaUnitatsCaixa = new DataGridTextColumn();
                columnaUnitatsCaixa.Header = "Unitats/Caixa";
                columnaUnitatsCaixa.Binding = new Binding("unitats_caixa");
                columnaUnitatsCaixa.IsReadOnly = true;
                ArticlesDatGrid.Columns.Add(columnaUnitatsCaixa);

                DataGridTextColumn columnaUnitatsFraccio = new DataGridTextColumn();
                columnaUnitatsFraccio.Header = "Unitats/Fraccio";
                columnaUnitatsFraccio.Binding = new Binding("unitats_fraccio");
                columnaUnitatsFraccio.IsReadOnly = true;
                ArticlesDatGrid.Columns.Add(columnaUnitatsFraccio);

                DataGridTextColumn columnaMarcaPes = new DataGridTextColumn();
                columnaMarcaPes.Header = "Marca de Pes";
                columnaMarcaPes.Binding = new Binding("marca_de_pes_str");
                columnaMarcaPes.IsReadOnly = true;
                ArticlesDatGrid.Columns.Add(columnaMarcaPes);

                DataGridTextColumn preuUnitari = new DataGridTextColumn();
                preuUnitari.Header = "Preu Unitari";
                preuUnitari.Binding = new Binding("preu_unitari");
                preuUnitari.IsReadOnly = true;
                ArticlesDatGrid.Columns.Add(preuUnitari);

                DataGridTextColumn preuVendaPublic = new DataGridTextColumn();
                preuVendaPublic.Header = "Preu Venda Publ Rec";
                preuVendaPublic.Binding = new Binding("preu_venta_public_recomanat");
                preuVendaPublic.IsReadOnly = true;
                ArticlesDatGrid.Columns.Add(preuVendaPublic);

                DataGridTextColumn preuFraccio = new DataGridTextColumn();
                preuFraccio.Header = "Preu Fracció";
                preuFraccio.Binding = new Binding("preu_de_fraccio");
                preuFraccio.IsReadOnly = true;
                ArticlesDatGrid.Columns.Add(preuFraccio);

                DataGridTextColumn tipusIva = new DataGridTextColumn();
                tipusIva.Header = "Tipus IVA";
                tipusIva.Binding = new Binding("tipus_iva");
                tipusIva.IsReadOnly = true;
                ArticlesDatGrid.Columns.Add(tipusIva);

                DataGridTextColumn codiFamilia = new DataGridTextColumn();
                codiFamilia.Header = "Codi Familia";
                codiFamilia.Binding = new Binding("codi_familia");
                codiFamilia.IsReadOnly = true;
                ArticlesDatGrid.Columns.Add(codiFamilia);

                DataGridTextColumn codiSubFamilia = new DataGridTextColumn();
                codiSubFamilia.Header = "Codi Sub Familia";
                codiSubFamilia.Binding = new Binding("codi_sub_familia");
                codiSubFamilia.IsReadOnly = true;
                ArticlesDatGrid.Columns.Add(codiSubFamilia);

                DataGridTextColumn unitatsMesura = new DataGridTextColumn();
                unitatsMesura.Header = "Unitats Mesura";
                unitatsMesura.Binding = new Binding("unitats_mesura");
                unitatsMesura.IsReadOnly = true;
                ArticlesDatGrid.Columns.Add(unitatsMesura);

                DataGridTextColumn factorConversio = new DataGridTextColumn();
                factorConversio.Header = "Factor Conversió";
                factorConversio.Binding = new Binding("factor_de_conversio");
                factorConversio.IsReadOnly = true;
                ArticlesDatGrid.Columns.Add(factorConversio);

                for(int i = 0; i < productes.Count; ++i)
                    ArticlesDatGrid.Items.Add(productes[i]);
                
            }
        }

        private void CarregaEansDataGrid(List<Managers.Producte> prods)
        {
            if(prods != null)
            {
                EansDatGrid.Columns.Clear();
                EansDatGrid.Items.Clear();

                DataGridTextColumn columnaCodiArt = new DataGridTextColumn();
                columnaCodiArt.Header = "Codi Art";
                columnaCodiArt.Binding = new Binding("codi_article");
                columnaCodiArt.IsReadOnly = true;
                EansDatGrid.Columns.Add(columnaCodiArt);

                DataGridTextColumn columnaCodiEan = new DataGridTextColumn();
                columnaCodiEan.Header = "Codi EAN";
                columnaCodiEan.Binding = new Binding("codi_ean");
                columnaCodiEan.IsReadOnly = true;
                EansDatGrid.Columns.Add(columnaCodiEan);

                EansDatGrid.IsReadOnly = true;

                for (int i = 0; i < prods.Count; ++i)
                    for(int e = 0; e < prods[i].codis_ean.Count; ++e)
                        EansDatGrid.Items.Add(prods[i].codis_ean[e]);
            }
        }

        private void ImportEansButton_Click(object sender, RoutedEventArgs e)
        {
            if (ArticlesDatGrid.SelectedItems != null && ArticlesDatGrid.SelectedItems.Count > 0)
            {
                List<Managers.Producte> selected = ArticlesDatGrid.SelectedItems.OfType<Managers.Producte>().ToList();

                Managers.FactuSolDBManager.Instance.AddEans(selected);
            }
        }

        private void ImportArticlesButton_Click(object sender, RoutedEventArgs e)
        {
            if (ArticlesDatGrid.SelectedItems != null && ArticlesDatGrid.SelectedItems.Count > 0)
            {
                List<Managers.Producte> selected = ArticlesDatGrid.SelectedItems.OfType<Managers.Producte>().ToList();
                Managers.FactuSolDBManager.Instance.AddProductes(selected);
            }
        }

        private void CercaButton_Click(object sender, RoutedEventArgs e)
        {
            Managers.Fitxer fit = Managers.DataManager.Instance.GetFitxer();

            List<Managers.Producte> prods = fit.productes.Where(f => FormatForSearch(f.descripcio)
            .Contains(FormatForSearch(BuscadorTextBox.Text)) || FormatForSearch(f.codi_article.ToString())
            .Contains(FormatForSearch(BuscadorTextBox.Text))).ToList();

            CarregaArticlesDataGrid(prods);
        }

        private string FormatForSearch(string txt)
        {
            return txt.ToLower().Replace(" ", "");
        }

        private void NetejaCerca_Click(object sender, RoutedEventArgs e)
        {
            BuscadorTextBox.Text = "";

            Managers.Fitxer fit = Managers.DataManager.Instance.GetFitxer();

            CarregaArticlesDataGrid(fit.productes);
        }
    }
}
