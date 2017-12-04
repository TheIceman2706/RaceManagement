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
using MessageBox = System.Windows.MessageBox;
using System.Data;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Strafrunden.Server;
using Logging;


namespace Strafrunden
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Log log = Log.Instance;

        private Server.HttpServer _server;
        private SqlConnection sql;
        private ObservableCollection<int[]> data;
        private string dataQuery = "SELECT startnummer, SUM(fehler) FROM strafrunden GROUP BY startnummer;";
        

        private Timer timer;
        public MainWindow()
        {
            log.Info("Main window creating...");
            data = new ObservableCollection<int[]>();
            _server = new Server.HttpServer();
            _server.Start();
            bool newDatabaseFile = false;

            if (!System.IO.File.Exists(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "strafrunden.mdf")))
            {
                log.Info("Creating database file...");
                newDatabaseFile = true;
                CreateSqlDatabase(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "strafrunden.mdf"));
                log.Info("Database file created!");
            }
            try
            {
                log.Info("Checking SQL instances.");
                string instN = "";
                DataTable servers = System.Data.Sql.SqlDataSourceEnumerator.Instance.GetDataSources();
                if (servers.Rows.Count < 1)
                {
                    instN = "MSSQLLocalDB";
                }
                else
                {
                    instN = (string)servers.Rows[0].ItemArray[1];
                }
                log.Info("SQL server instance is "+instN);
                sql = new SqlConnection("Data Source=(LocalDB)\\"+instN+";AttachDbFilename=\"" + System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "strafrunden.mdf") + "\";Integrated Security=True");
                log.Info("Opening SQL connection...");
                sql.Open();
                log.Info("SQL connection open!");
            }
            catch (Exception ex)
            {
                log.Fail(ex.Message);
                MessageBox.Show("Ist die Datenbank bereits in einem anderen Programm geöffnet?", "Fehler");
                System.Windows.Application.Current.Shutdown();
            }
            if (newDatabaseFile)
            {
                log.Info("Setting up new database...");
                SqlCommand com = sql.CreateCommand();
                com.CommandText = @"CREATE TABLE [dbo].[strafrunden] ([Id] INT IDENTITY (1, 1) NOT NULL,[startnummer] INT NOT NULL,[fehler]      INT NULL,PRIMARY KEY CLUSTERED ([Id] ASC));";
                com.ExecuteNonQuery();
                com.Dispose();
                log.Info("Database set up!");
            }

            log.Info("registering Handlers...");
            Handlers.RegisterResourceHandler(new StrafrundenPageHandler(sql));
            Handlers.RegisterResourceHandler(new FileResourceHandler("/strafrunden/main.css", "strafrunden.css", "text/css"));

            log.Info("Creating timers...");
            timer = new Timer();
            log.Info("Initializing components...");
            InitializeComponent();
            log.Info("Creating timers...");
            timer.Enabled = true;
            timer.Interval = 1000;
            timer.Tick += Timer_Elapsed;
            log.Info("Main window created!");
        }
        

        private void Timer_Elapsed(object sender, EventArgs e)
        {
            if(AutoUpdateCheckbox.IsChecked)
                MenuItem_Click(sender, new RoutedEventArgs());
            if (ExportExcel.IsChecked)
                //System.Threading.ThreadPool.QueueUserWorkItem((o)=> { MenuItem_Click_3(((object[])o)[0], (RoutedEventArgs)((object[])o)[1]); }, new object[]{ sender, new RoutedEventArgs()});
                MenuItem_Click_3(sender, new RoutedEventArgs());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            log.Info("Main window closing...");
            _server.Stop();
            log.Info("Saving settings...");
            Properties.Settings.Default.Save();
        }

        private void CombineFailsCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            dataQuery = "SELECT startnummer, SUM(fehler) FROM strafrunden GROUP BY startnummer;";
            if (sql != null)
                MenuItem_Click(sender, e);
            CombineFailsCheckbox.IsEnabled = false;
            if (ShowIndividual != null)
                ShowIndividual.IsChecked = false;
            AnsichtStatus2.Content = "zusammengefasste Ergebnisse";

            DataOutput.Columns[2].Visibility = Visibility.Hidden;

            Properties.Settings.Default.CombineView = true;
             
        }

        private void CombineFailsCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            CombineFailsCheckbox.IsEnabled = true;
            Properties.Settings.Default.CombineView = false;
             
        }

        private void AutoUpdateCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            AnsichtStatus.Content = "automatische Aktualisierung,";
            Properties.Settings.Default.AutoUpdateView = true;
             
        }

        private void AutoUpdateCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            AnsichtStatus.Content = "manuelle Aktualisierung,";
            Properties.Settings.Default.AutoUpdateView = false;
             
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            log.Info("Main window loaded!");
            log.Info("Setting up UI.");

            DataOutput.CanUserAddRows = false;
            DataOutput.CanUserDeleteRows = false;
            DataOutput.ItemsSource = data;

            AutoUpdateCheckbox.IsChecked =  Properties.Settings.Default.AutoUpdateView;
            if (Properties.Settings.Default.CombineView)
            {
                ShowIndividual_Unchecked(this, new RoutedEventArgs());
                CombineFailsCheckbox_Checked(this, new RoutedEventArgs());
                CombineFailsCheckbox.IsChecked = true;
            }
            else
            {
                CombineFailsCheckbox_Unchecked(this, new RoutedEventArgs());
                ShowIndividual_Checked(this, new RoutedEventArgs());
                ShowIndividual.IsChecked = true;
            }

            ExcelCombine.IsChecked = Properties.Settings.Default.CombineExcel;
            ExcelFileName.Header = Properties.Settings.Default.ExcelFile;
            ExcelFileStatus.Content = Properties.Settings.Default.ExcelFile;
            ExportExcel.IsChecked = Properties.Settings.Default.AutoExportExcel;

            if (ExportExcel.IsChecked)
                ExcelModeStatus.Content = "automatisch" + (ExcelCombine.IsChecked ? ", zusammengefasst" : ", detailiert");
            else
                ExcelModeStatus.Content = "manuell" + (ExcelCombine.IsChecked ? ", zusammengefasst" : ", detailiert");
            Properties.Settings.Default.CombineExcel = ExcelCombine.IsChecked;

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
                    if (rd.FieldCount <= 2)
                        data.Add(new int[] { rd.GetInt32(0), rd.GetInt32(1) });
                    else
                        data.Add(new int[] { rd.GetInt32(0), rd.GetInt32(1), rd.GetInt32(2) });
                }
            }
            rd.Close();
           
            trans.Commit();

            LastUpdateStatus.Content = DateTime.Now.ToLongTimeString()+" ("+DateTime.Now.ToShortDateString()+")";
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("Sind sie sicher, dass sie alle Daten löschen möchten?", "Warnung", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                SqlTransaction trans = sql.BeginTransaction();
                SqlCommand com = sql.CreateCommand();
                com.Transaction = trans;
                com.CommandText = "DELETE FROM strafrunden WHERE 1=1;";
                com.ExecuteNonQuery();
                trans.Commit();
            }
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
            dataQuery = "SELECT startnummer, fehler, id FROM strafrunden;";
            if (sql != null)
                MenuItem_Click(sender, e);
            ShowIndividual.IsEnabled = false;
            AnsichtStatus2.Content = "einzelne Wurfrunden";
            DataOutput.Columns[2].Visibility = Visibility.Visible;
        }

        private void ShowIndividual_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowIndividual.IsEnabled = true;
        }

        private void HelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Falls der Server von aussen nicht zu erreichen ist, überprüfen Sie Ihre Firewall-Einstellungen.\nVersion:"+System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(), "Hilfe", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        bool auto;
        int[] oldData;
        private void DataOutput_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                if (e.EditAction == DataGridEditAction.Commit)
                {
                    int[] newData = (int[])((int[])e.Row.DataContext).Clone();
                    int dataIndex = Convert.ToInt32(e.Column.SortMemberPath.Replace('[', ' ').Replace(']', ' '));
                    if (dataIndex > 1 || dataIndex < 0)
                        return;
                    if (!(e.EditingElement is System.Windows.Controls.TextBox))
                        return;

                    newData[dataIndex] = Convert.ToInt32((e.EditingElement as System.Windows.Controls.TextBox).Text);

                    if (oldData.Length <= 2)
                    {
                        SqlTransaction trans = sql.BeginTransaction();
                        SqlCommand com = sql.CreateCommand();
                        com.Transaction = trans;
                        com.CommandText = "DELETE FROM strafrunden WHERE startnummer=" + oldData[0] + ";";
                        com.ExecuteNonQuery();
                        com.CommandText = "INSERT INTO strafrunden (startnummer,fehler) VALUES(" + newData[0] + "," + newData[1] + ");";
                        com.ExecuteNonQuery();
                        trans.Commit();
                    }
                    else
                    {

                        SqlTransaction trans = sql.BeginTransaction();
                        SqlCommand com = sql.CreateCommand();
                        com.Transaction = trans;
                        com.CommandText = "UPDATE strafrunden SET startnummer=" + newData[0] + ",fehler=" + newData[1] + " WHERE id = " + newData[2] + ";";
                        com.ExecuteNonQuery();
                        trans.Commit();
                    }
                }

            }
            catch (FormatException ex)
            {
                MessageBox.Show("Geben sie bitte nur Zahlen ein!\n"+ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Cancel = true;
                return;
            }
            finally
            {
                AutoUpdateCheckbox.IsChecked = auto;
                AnsichtMenuItem.IsEnabled = true;
            }

        }

        private void DataOutput_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            auto = AutoUpdateCheckbox.IsChecked;
            AutoUpdateCheckbox.IsChecked = false;
            oldData = (int[])e.Row.DataContext;
            AnsichtMenuItem.IsEnabled = false;
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {

                SqlTransaction trans = sql.BeginTransaction();
                SqlCommand com = sql.CreateCommand();
                com.Transaction = trans;
                com.CommandText = !ExcelCombine.IsChecked ? "SELECT startnummer, fehler, id FROM strafrunden;" : "SELECT startnummer, SUM(fehler) FROM strafrunden GROUP BY startnummer;";
                SqlDataReader rd = com.ExecuteReader();
                List<int[]> data = new List<int[]>();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        if (rd.FieldCount <= 2)
                            data.Add(new int[] { rd.GetInt32(0), rd.GetInt32(1) });
                        else
                            data.Add(new int[] { rd.GetInt32(0), rd.GetInt32(1), rd.GetInt32(2) });
                    }
                }
                rd.Close();

                trans.Commit();

                if (data.Count > 0)
                {
                    DataSet ds = new DataSet("Strafrunden");
                    DataTable dt = new DataTable("Strafrunden");

                    DataColumn startnr = new DataColumn("Startnummer");
                    DataColumn strafr = new DataColumn("Strafrunden");
                    DataColumn wurfid = data.ToArray()[0].Length > 2? new DataColumn("Wurf-ID") : null;

                    dt.Columns.Add(startnr);
                    dt.Columns.Add(strafr);
                    if(wurfid != null)
                        dt.Columns.Add(wurfid);

                    for(int i = 0; i < data.ToArray().Length; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dr.SetField<int>(startnr, data.ToArray()[i][0]);
                        dr.SetField<int>(strafr, data.ToArray()[i][1]);
                        if(wurfid != null)
                            dr.SetField<int>(wurfid, data.ToArray()[i][2]);
                        dt.Rows.Add(dr);
                    }

                    ds.Tables.Add(dt);

                    ExportDataSet(ds, ExcelFileName.Header.ToString());
                }
            }
            catch(System.IO.IOException ex)
            {
                MessageBox.Show("Ausgabefehler.\n"+ExcelFileName.Header.ToString()+" ist möglicherweise geöffnet!", "Fehler");
            }
            catch(Exception ex)
            {
                MessageBox.Show("Exception: "+ ex.Message,ex.ToString());
            }
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Excel-Datei wählen oder erstellen...";
            sfd.Filter = "Excel-Dateien | *.xlsx";
            if(sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ExcelFileName.Header = sfd.FileName;
                ExcelFileStatus.Content = ExcelFileName.Header.ToString();
                Properties.Settings.Default.ExcelFile = ExcelFileName.Header.ToString();
            }
        }

        private void ExportDataSet(DataSet ds, string destination)
        {
            using (var workbook = SpreadsheetDocument.Create(destination, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = workbook.AddWorkbookPart();

                workbook.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

                workbook.WorkbookPart.Workbook.Sheets = new DocumentFormat.OpenXml.Spreadsheet.Sheets();

                foreach (System.Data.DataTable table in ds.Tables)
                {

                    var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                    sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

                    DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                    string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                    uint sheetId = 1;
                    if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
                    {
                        sheetId =
                            sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                    }

                    DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = table.TableName };
                    sheets.Append(sheet);

                    DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

                    List<String> columns = new List<string>();
                    foreach (System.Data.DataColumn column in table.Columns)
                    {
                        columns.Add(column.ColumnName);

                        DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                        cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                        cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(column.ColumnName);
                        headerRow.AppendChild(cell);
                    }


                    sheetData.AppendChild(headerRow);

                    foreach (System.Data.DataRow dsrow in table.Rows)
                    {
                        DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                        foreach (String col in columns)
                        {
                            DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                            cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.Number;
                            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(dsrow[col].ToString()); //
                            newRow.AppendChild(cell);
                        }

                        sheetData.AppendChild(newRow);
                    }

                }
            }
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            if (ExportExcel.IsChecked)
                ExcelModeStatus.Content = "automatisch" + (ExcelCombine.IsChecked?", zusammengefasst":", detailiert");
            else
                ExcelModeStatus.Content = "manuell" + (ExcelCombine.IsChecked ? ", zusammengefasst" : ", detailiert");
            Properties.Settings.Default.AutoExportExcel = ExportExcel.IsChecked;
             
        }

        private void ExcelCombine_Click(object sender, RoutedEventArgs e)
        {
            if (ExportExcel.IsChecked)
                ExcelModeStatus.Content = "automatisch" + (ExcelCombine.IsChecked ? ", zusammengefasst" : ", detailiert");
            else
                ExcelModeStatus.Content = "manuell" + (ExcelCombine.IsChecked ? ", zusammengefasst" : ", detailiert");
            Properties.Settings.Default.CombineExcel = ExcelCombine.IsChecked;
             
        }
    }
}
