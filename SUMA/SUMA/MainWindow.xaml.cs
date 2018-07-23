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
        }

        private void LookForFileButton_Click(object sender, RoutedEventArgs e)
        {
            string file_path = Managers.FileSystemManager.Instance.LoadFileDialog("txt");

            if (file_path != "")
            {
                UrlText.Text = file_path;

                string[] text = Managers.FileSystemManager.Instance.GetFileDataText(file_path);

                Managers.ParserManager.Instance.ParseAsync(text, OnParseTick, OnParseFinish);
            }
        }

        private void OnParseTick(float progress)
        {
            ParseProgressBar.Visibility = Visibility.Visible;
            ParseProgressBar.Value = progress;
        }

        private void OnParseFinish()
        {
            ParseProgressBar.Visibility = Visibility.Hidden;

            CarregaArticlesDataGrid();
        }

        private void CarregaArticlesDataGrid()
        {
             //DataGridTextColumn columnaProveedor = new DataGridTextColumn();
            //columnaProveedor.Header = "Descripcio";
            //columnaProveedor.Binding = new Binding("descripcio");
            //columnaProveedor.IsReadOnly = true;
            //ArticlesDatGrid.Columns.Add(columnaProveedor);
            ArticlesDatGrid.ItemsSource = Managers.DataManager.Instance.GetFitxer().productes;
            ArticlesDatGrid.IsReadOnly = true;
        }

        private void ImportEansButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ImportArticlesButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }
    }
}
