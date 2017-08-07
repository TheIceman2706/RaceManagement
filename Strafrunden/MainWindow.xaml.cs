using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
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
using System.Windows.Forms;

namespace Strafrunden
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Server.HttpServer _server;
        private SqlConnection sql;
        private ObservableCollection<int[]> data;
        private string dataQuery = "SELECT startnummer, SUM(fehler) FROM strafrunden GROUP BY startnummer;" ;

        private Timer timer;
        public MainWindow()
        {
            data = new ObservableCollection<int[]>();
            _server = new Server.HttpServer();
            _server.Start();
            bool newDatabaseFile = false;
            if (!System.IO.File.Exists(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "strafrunden.mdf")))
            {
                newDatabaseFile = true;
                CreateSqlDatabase(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "strafrunden.mdf"));
            }
            sql = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\""+ System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "strafrunden.mdf") + "\";Integrated Security=True");
            sql.Open();
            if (newDatabaseFile)
            {
                SqlCommand com = sql.CreateCommand();
                com.CommandText = @"CREATE TABLE [dbo].[strafrunden] ([Id]          INT IDENTITY (1, 1) NOT NULL,[startnummer] INT NOT NULL,[fehler]      INT NULL,PRIMARY KEY CLUSTERED ([Id] ASC));";
                com.ExecuteNonQuery();
                com.Dispose();
            }
            _server.Handler.RegisterResourceHandler(new StrafrundenPageHandler(sql));
            _server.Handler.RegisterResourceHandler(new FileResourceHandler("/strafrunden/main.css", "..\\..\\strafrunden.css", "text/css"));
            timer = new Timer();

            InitializeComponent();
            timer.Enabled = true;
            timer.Interval = 1000;
            timer.Tick += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, EventArgs e)
        {
            MenuItem_Click(sender, new RoutedEventArgs());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _server.Stop();
        }

        private void CombineFailsCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            dataQuery = "SELECT startnummer, SUM(fehler) FROM strafrunden GROUP BY startnummer;";
            if(sql != null)
                MenuItem_Click(sender, e);
            CombineFailsCheckbox.IsEnabled = false;
            if(ShowIndividual!= null)
                ShowIndividual.IsChecked = false;
        }

        private void CombineFailsCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            CombineFailsCheckbox.IsEnabled = true;
        }

        private void AutoUpdateCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if(timer != null)
                timer.Start();
        }

        private void AutoUpdateCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataOutput.CanUserAddRows = false;
            DataOutput.CanUserDeleteRows = false;
            DataOutput.ItemsSource = data;
            timer.Start();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SqlTransaction trans = sql.BeginTransaction();
            SqlCommand com = sql.CreateCommand();
            com.Transaction = trans;
            com.CommandText = dataQuery;
            SqlDataReader rd = com.ExecuteReader();
            data.Clear();
            if (rd.HasRows)
            {
                while (rd.Read())
                {
                    data.Add(new int[] { rd.GetInt32(0), rd.GetInt32(1) });
                }
            }
            rd.Close();

            trans.Commit();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            SqlTransaction trans = sql.BeginTransaction();
            SqlCommand com = sql.CreateCommand();
            com.Transaction = trans;
            com.CommandText = "DELETE FROM strafrunden WHERE 1=1;";
            com.ExecuteNonQuery();
            trans.Commit();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static void CreateSqlDatabase(string filename)
        {
            string databaseName = System.IO.Path.GetFileNameWithoutExtension(filename);
            using (var connection = new System.Data.SqlClient.SqlConnection(
                "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=master; Integrated Security=true;"))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        String.Format("CREATE DATABASE {0} ON PRIMARY (NAME={0}, FILENAME='{1}')", databaseName, filename);
                    command.ExecuteNonQuery();

                    command.CommandText =
                        String.Format("EXEC sp_detach_db '{0}', 'true'", databaseName);
                    command.ExecuteNonQuery();

                }
            }
        }

        private void ShowIndividual_Checked(object sender, RoutedEventArgs e)
        {
            CombineFailsCheckbox.IsChecked = false;
            dataQuery = "SELECT startnummer, fehler FROM strafrunden;";
            if (sql != null)
                MenuItem_Click(sender, e);
            ShowIndividual.IsEnabled = false;
        }

        private void ShowIndividual_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowIndividual.IsEnabled = true;
        }
    }
}
