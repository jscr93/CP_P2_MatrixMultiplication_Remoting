using System;
using System.Collections.Generic;
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
using System.IO;
using System.Collections.ObjectModel;

namespace MatrixMultiplicationClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static ObservableCollection<string> lstDetails;

        string path1, path2;
        string path_result;
        string path_config;
        string directory;
        char separator;

        int rows, columns;
        int seed1, seed2;

        public MainWindow()
        {
            InitializeComponent();
            lstDetails = new ObservableCollection<string>();
            lstBxDetails.ItemsSource = lstDetails;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bool conectionSuccess = false;
            while (!conectionSuccess)
            {
                try
                {
                    Client.Register(string.IsNullOrEmpty(txtServerIp.Text) ? "localhost" : txtServerIp.Text);
                    conectionSuccess = true;
                }
                catch (Exception ex)
                {
                    MessageBoxResult mbr = MessageBox.Show("No se ha podido conectar al servidor. ¿Reintentar?", "Ha ocurrido un problema", MessageBoxButton.YesNo, MessageBoxImage.Hand);
                    //if (mbr == MessageBoxResult.No)
                        //return;
                }
            }

            Matrix.StartListening();
            path1 = @"C:\CP_P2\Matrix_1.txt";
            path2 = @"C:\CP_P2\Matrix_2.txt";
            path_result = @"C:\CP_P2\Matrix_Result.txt";
            path_config = @"C:\CP_P2\Matrix_Config.txt";
            separator = ',';
            directory = @"C:\CP_P2";
            System.IO.Directory.CreateDirectory(directory);
            if (File.Exists(path_config))
            {
                using (StreamReader sr = File.OpenText(path_config))
                {
                    txtRows.Text = sr.ReadLine();
                    txtColumns.Text = sr.ReadLine();
                    txtSeedM1.Text = sr.ReadLine();
                    txtSeedM2.Text = sr.ReadLine();
                    Int32.TryParse(txtRows.Text, out rows);
                    Int32.TryParse(txtColumns.Text, out columns);
                    Int32.TryParse(txtSeedM1.Text, out seed1);
                    Int32.TryParse(txtSeedM2.Text, out seed2);
                }
            }
            else
            {
                rows = 6;
                columns = 3;
                seed1 = 2;
                seed2 = 3;
                txtRows.Text = rows.ToString();
                txtColumns.Text = columns.ToString();
                txtSeedM1.Text = seed1.ToString();
                txtSeedM2.Text = seed2.ToString();
            }
        }

        private void btnNewMatrices_Click(object sender, RoutedEventArgs e)
        {
            int rows, columns, seed1, seed2;
            if (int.TryParse(txtRows.Text, out rows) && int.TryParse(txtColumns.Text, out columns) 
                && int.TryParse(txtSeedM1.Text, out seed1) && int.TryParse(txtSeedM2.Text, out seed2))
            {
                char separator = ',';
                DisableAll();
                File.Delete(path1);
                File.Delete(path2);
                Matrix.createMatrixFile(path1, rows, columns, separator, seed1);
                Matrix.createMatrixFile(path2, columns, rows, separator, seed2);
                EnableAll();
                File.Delete(path_config);
                using (StreamWriter sw = File.CreateText(path_config))
                {
                    sw.WriteLine(rows);
                    sw.WriteLine(columns);
                    sw.WriteLine(seed1);
                    sw.WriteLine(seed2);
                    this.rows = rows;
                    this.columns = columns;
                    this.seed1 = seed1;
                    this.seed2 = seed2;
                }
                MessageBox.Show("Se han creado las matrices exitosamente.", "Se han creado las matrices", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Los valores ingresados deben ser enteros", "No se ha podido realizar la operación", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void btnParallel_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(path1) && File.Exists(path2))
            {
                long elapsedTime = Matrix.multiplicationParallel(path1, path2, path_result, rows, columns, separator);
                MessageBox.Show(string.Format("Se ha concluido la operación en {0} milisegundos.", elapsedTime), "Operación completa", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("No se han generado las matrices", "No se ha podido realizar la operación", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void DisableAll()
        {
            btnNewMatrices.IsEnabled = false;
        }

        private void EnableAll()
        {
            btnNewMatrices.IsEnabled = true;
        }

    }
}
