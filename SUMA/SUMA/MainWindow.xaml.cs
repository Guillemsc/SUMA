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
            bool exists = true;
            XDocument config = Managers.FileSystemManager.Instance.LoadConfigXML(out exists);

            if (exists && config != null)
            {
                string local_server_path = "";
                string factu_sol_server_path = "";
               
               List<XElement> servidors_el = config.Element("configuration").Elements("servers").ToList();

               if (servidors_el.Count > 1)
               {
                   local_server_path = servidors_el[0].Attribute("local_path").Value;

                   factu_sol_server_path = servidors_el[1].Attribute("factu_sol_path").Value;
               }
               
               Managers.LocalDBManager.Instance.SetDataBasePath(local_server_path);
               Managers.FactuSolDBManager.Instance.SetDataBasePath(factu_sol_server_path);
            }
            else
            {
                string message = "No s'ha pogut trobar l'arxiu 'Config.xml'";
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(message), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);

                Application.Current.Shutdown();
            }
        }

        private void LookForFileButton_Click(object sender, RoutedEventArgs e)
        {
            string file_path = Managers.FileSystemManager.Instance.LoadFileDialog("txt");

            if (file_path != "no_file" && File.Exists(file_path))
            {
                StartAsyncProcess();

                string[] text = Managers.FileSystemManager.Instance.GetFileDataText(file_path);

                Managers.ParserManager.Instance.ParseAsync(System.IO.Path.GetFileName(file_path), text, OnParseTick, OnParseFinish);
            }
        }

        private void OnParseTick(float progress)
        {
            ParseProgressBar.Visibility = Visibility.Visible;
            ParseProgressBar.Value = progress;
            LoadInfoText.Visibility = Visibility.Visible;
            LoadInfoText.Content = "Recuperant informació del .txt... (" + progress + "%)";
        }

        private void OnParseFinish()
        {
            Managers.Fitxer fit = Managers.DataManager.Instance.GetFitxer();

            ParseProgressBar.Visibility = Visibility.Hidden;

            DataImpLabel.Visibility = Visibility.Visible;

            bool correct = true;
           
           Managers.LocalDBManager.Instance.AddProductesAndEansAsync(fit.productes, fit.eans, 
               OnLocalProductesAndEansAddTick, OnLocalProductesAndEansAddFinish);
           
            if(correct)
            {
                correct = Managers.LocalDBManager.Instance.AddRegistre(fit);
            }

            if(!correct)
            {
                UrlText.Text = "";
            }

            LoadInfoText.Visibility = Visibility.Hidden;
        }

        private void OnLocalProductesAndEansAddTick(float progress)
        {
            ParseProgressBar.Visibility = Visibility.Visible;
            ParseProgressBar.Value = progress;
            LoadInfoText.Visibility = Visibility.Visible;
            LoadInfoText.Content = "Escrivint a la base de dades local... (" + progress + "%)";
        }

        private void OnLocalProductesAndEansAddFinish()
        {
            LoadInfoText.Visibility = Visibility.Hidden;
            ParseProgressBar.Visibility = Visibility.Hidden;

            Managers.LocalDBManager.Instance.LoadDataBase();
            CarregaArticlesDataGrid(Managers.DataManager.Instance.GetFitxer().productes);

            FinishAsyncProcess();
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
                StartAsyncProcess();

                List<Managers.Producte> selected = ArticlesDatGrid.SelectedItems.OfType<Managers.Producte>().ToList();

                Managers.FactuSolDBManager.Instance.AddEansAsync(selected, OnFactuSolEanAddTick, OnFactuSolEanFinish);
            }
        }

        private void ImportArticlesButton_Click(object sender, RoutedEventArgs e)
        {
            if (ArticlesDatGrid.SelectedItems != null && ArticlesDatGrid.SelectedItems.Count > 0)
            {
                StartAsyncProcess();

                List<Managers.Producte> selected = ArticlesDatGrid.SelectedItems.OfType<Managers.Producte>().ToList();
                Managers.FactuSolDBManager.Instance.AddProductesAsync(selected, OnFactuSolProducteAddTick, OnFactuSolProductesAddFinish, 
                    OnFactuSolEanAddTick, OnFactuSolEanFinish);
            }
        }

        private void OnFactuSolProducteAddTick(float progress)
        {
            StartAsyncProcess();

            ParseProgressBar.Visibility = Visibility.Visible;
            ParseProgressBar.Value = progress;
            LoadInfoText.Visibility = Visibility.Visible;
            LoadInfoText.Content = "Afegint productes... (" + progress + "%)";
        }

        private void OnFactuSolProductesAddFinish()
        {
            LoadInfoText.Visibility = Visibility.Hidden;
            ParseProgressBar.Visibility = Visibility.Hidden;

            FinishAsyncProcess();
        }

        private void OnFactuSolEanAddTick(float progress)
        {
            StartAsyncProcess();

            ParseProgressBar.Visibility = Visibility.Visible;
            ParseProgressBar.Value = progress;
            LoadInfoText.Visibility = Visibility.Visible;
            LoadInfoText.Content = "Afegint eans... (" + progress + "%)";
        }

        private void OnFactuSolEanFinish()
        {
            LoadInfoText.Visibility = Visibility.Hidden;
            ParseProgressBar.Visibility = Visibility.Hidden;

            FinishAsyncProcess();
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

        private bool async_process = false;
        private void StartAsyncProcess()
        {
            async_process = true;
            ImportArticlesButton.IsEnabled = false;
            ImportEansButton.IsEnabled = false;
            LookForFileButton.IsEnabled = false;
        }

        private void FinishAsyncProcess()
        {
            async_process = false;
            ImportArticlesButton.IsEnabled = true;
            ImportEansButton.IsEnabled = true;
            LookForFileButton.IsEnabled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(async_process)
            {
                string message = "Hi ha un process actiu, estas segur que vols tancar el programa? Tancar el programa" +
                    " pot finalitzar el process amb errors";
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(message), "Èxit", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);

                if (messageBoxResult == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
