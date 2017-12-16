using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Strafrunden
{
    /// <summary>
    /// Interaktionslogik für LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        Logging.Log log = Logging.Log.Instance;
        private BackgroundWorker loader;
        private MainWindow mw;
        private SqlConnection sql;

        private string SQLInstanceName;

        public LoadingWindow()
        {
            loader = new BackgroundWorker();
            loader.WorkerReportsProgress = true;
            loader.DoWork += Loader_DoWork;
            loader.ProgressChanged += Loader_ProgressChanged;
            loader.RunWorkerCompleted += Loader_RunWorkerCompleted;
            InitializeComponent();
            SQLInstanceName = "MSSQLLocalDB";
        }

        private void Loader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Progress.Value = 100;
            mw = new MainWindow(sql);
            mw.Closed += Mw_Closed;
            this.Close();
            mw.Show();
        }

        private void Mw_Closed(object sender, EventArgs e)
        {
            if (!CloseFirewall())
            {
                MessageBox.Show(Properties.strings.ErrorPortsNotFree, Properties.strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                Logging.Log.Instance.Warn("Firewall not configured!");
            }
        }

        private void Loader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(e.ProgressPercentage == 50)
                Progress.IsIndeterminate = true;
            if (e.ProgressPercentage == 75)
                Progress.IsIndeterminate = false;
            Progress.Value = e.ProgressPercentage;
            status(""+e.UserState);
        }

        private void Loader_DoWork(object sender, DoWorkEventArgs e)
        {
            loader.ReportProgress(1, Properties.strings.LoadingSettings);
            if (Strafrunden.Properties.Settings.Default.ApplicationVersion != System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString())
            {
                Strafrunden.Properties.Settings.Default.Upgrade();
                loader.ReportProgress(5, Properties.strings.SettingsUpgraded);
                Strafrunden.Properties.Settings.Default.ApplicationVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                loader.ReportProgress(10, Properties.strings.SettingsVersion+": " + Strafrunden.Properties.Settings.Default.ApplicationVersion);
            }
            loader.ReportProgress(15, Properties.strings.OpenFirewall);
            if (!OpenFirewall())
            {
                MessageBox.Show(Properties.strings.ErrorPortsNotFree, Properties.strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                Logging.Log.Instance.Warn("Firewall not configured!");
            }
            loader.ReportProgress(25, Properties.strings.SetURLACL);
            if (!AddUrlacl())
            {
                MessageBox.Show(Properties.strings.ErrorURLNotFree, Properties.strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                Logging.Log.Instance.Warn("URL not configured!");
            }
            loader.ReportProgress(35, Properties.strings.LoadingDatabase);

            LoadDatabase();


            loader.ReportProgress(90, Properties.strings.CreatingMainWindow);
        }

        private void LoadDatabase()
        {
            bool newDatabaseFile = false;


            loader.ReportProgress(40, Properties.strings.GettingSQLInstances);
            DataTable servers = System.Data.Sql.SqlDataSourceEnumerator.Instance.GetDataSources();
            if (servers.Rows.Count >= 1)
            { 
                SQLInstanceName = (string)servers.Rows[0].ItemArray[1];
            }
            loader.ReportProgress(60, Properties.strings.SQLServer + ": " + SQLInstanceName);

            if (!System.IO.File.Exists(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "strafrunden.mdf")))
            {
                loader.ReportProgress(61,"Creating database file...");
                newDatabaseFile = true;
                CreateSqlDatabase(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "strafrunden.mdf"));
                loader.ReportProgress(65, Properties.strings.DatabaseCreated);
            }
            try
            {
                
                sql = new SqlConnection("Data Source=(LocalDB)\\" + SQLInstanceName + ";AttachDbFilename=\"" + System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "strafrunden.mdf") + "\";Integrated Security=True");
                loader.ReportProgress(68, Properties.strings.OpeningSQL);
                sql.Open();
                loader.ReportProgress(75, Properties.strings.OpenedSQL);
            }
            catch (Exception ex)
            {
                log.Fail(ex.Message);
                MessageBox.Show("Ist die Datenbank bereits in einem anderen Programm geöffnet?", "Fehler");
                System.Windows.Application.Current.Shutdown();
            }
            if (newDatabaseFile)
            {
                loader.ReportProgress(85, Properties.strings.SettingUpDatabase);
                SqlCommand com = sql.CreateCommand();
                com.CommandText = @"CREATE TABLE [dbo].[strafrunden] ([Id] INT IDENTITY (1, 1) NOT NULL,[startnummer] INT NOT NULL,[fehler]      INT NULL,PRIMARY KEY CLUSTERED ([Id] ASC));";
                com.ExecuteNonQuery();
                com.Dispose();
                log.Info("Database set up!");
            }
        }

        private void status(string s)
        {
            StatusLabel.Content = s;

            Logging.Log.Instance.Info("[STATUS]" + s);
            log.SafeTo("loader.log.txt");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            Logging.Log.Instance.Info("App starting...");

            loader.RunWorkerAsync();

        }

        private bool OpenFirewall()
        {
            string output = "";
            using (Process p = new Process())
            {
                p.StartInfo.FileName = "netsh.exe";
                p.StartInfo.Arguments = "advfirewall firewall show rule name=\"Strafrundenserver\"";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();

                output = p.StandardOutput.ReadToEnd();

            }
            if (!output.Contains("Strafrundenserver"))
            {
                using (Process p = new Process())
                {
                    p.StartInfo.FileName = "netsh.exe";
                    p.StartInfo.Arguments = "advfirewall firewall add rule dir=in action=allow localport=80 protocol=tcp name=\"Strafrundenserver\"";
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.Verb = "runas";
                    p.StartInfo.RedirectStandardOutput = false;
                    try
                    {
                        p.Start();
                    }
                    catch (Win32Exception ex)
                    {
                        if (ex.NativeErrorCode == 1223)
                        {
                            return false;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                using (Process p = new Process())
                {
                    p.StartInfo.FileName = "netsh.exe";
                    p.StartInfo.Arguments = "advfirewall firewall add rule dir=out protocol=tcp action=allow localport=80  name=\"Strafrundenserver\"";
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.Verb = "runas";
                    p.StartInfo.RedirectStandardOutput = false;
                    try
                    {
                        p.Start();
                    }
                    catch (Win32Exception ex)
                    {
                        if (ex.NativeErrorCode == 1223)
                        {
                            return false;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            return true;
        }
        private bool CloseFirewall()
        {
            string output = "";
            using (Process p = new Process())
            {
                p.StartInfo.FileName = "netsh.exe";
                p.StartInfo.Arguments = "advfirewall firewall show rule name=\"Strafrundenserver\"";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();

                output = p.StandardOutput.ReadToEnd();

            }
            if (output.Contains("Strafrundenserver"))
            {
                using (Process p = new Process())
                {
                    p.StartInfo.FileName = "netsh.exe";
                    p.StartInfo.Arguments = "advfirewall firewall delete rule name=\"Strafrundenserver\"";
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.Verb = "runas";
                    p.StartInfo.RedirectStandardOutput = false;
                    try
                    {
                        p.Start();
                    }
                    catch (Win32Exception ex)
                    {
                        if (ex.NativeErrorCode == 1223)
                        {
                            return false;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            return true;
        }
        private bool AddUrlacl()
        {
            string output = "";

            using (Process p = new Process())
            {
                p.StartInfo.FileName = "netsh.exe";
                p.StartInfo.Arguments = "http show urlacl";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();

                output = p.StandardOutput.ReadToEnd();

            }
            if (!output.Contains(Strafrunden.Properties.Settings.Default.urlPrefix))
            {
                using (Process p = new Process())
                {
                    p.StartInfo.FileName = "netsh.exe";
                    p.StartInfo.Arguments = "http add urlacl " + Strafrunden.Properties.Settings.Default.urlPrefix + " user=\"" + System.Security.Principal.WindowsIdentity.GetCurrent().Name + "\"";
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.Verb = "runas";
                    p.StartInfo.RedirectStandardOutput = false;
                    try
                    {
                        p.Start();
                    }
                    catch (Win32Exception ex)
                    {
                        if (ex.NativeErrorCode == 1223)
                        {
                            return false;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            return true;
        }

        private void CreateSqlDatabase(string filename)
        {
            string databaseName = System.IO.Path.GetFileNameWithoutExtension(filename);
            using (var connection = new System.Data.SqlClient.SqlConnection(
                "Data Source=(LocalDB)\\"+SQLInstanceName+";Initial Catalog=master; Integrated Security=true;"))
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
    }
}
