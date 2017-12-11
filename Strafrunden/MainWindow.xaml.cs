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
        public MainWindow(SqlConnection con)
        {
            sql = con;
            log.Info("Main window creating...");
            data = new ObservableCollection<int[]>();
            _server = new Server.HttpServer();
            _server.Start();
            

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
            AnsichtStatus2.Content = Properties.strings.CombinedFails;

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
            AnsichtStatus.Content = Properties.strings.automatic+", ";
            Properties.Settings.Default.AutoUpdateView = true;
             
        }

        private void AutoUpdateCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            AnsichtStatus.Content = Properties.strings.manual+", ";
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
                ExcelModeStatus.Content = Properties.strings.automatic + (ExcelCombine.IsChecked ? ", " + Properties.strings.combined : ", " + Properties.strings.detailed);
            else
                ExcelModeStatus.Content = Properties.strings.manual + (ExcelCombine.IsChecked ? ", " + Properties.strings.combined : ", " + Properties.strings.detailed);
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
            if (System.Windows.MessageBox.Show(Properties.strings.QuestionDeleteData, Properties.strings.Warning,System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning) == MessageBoxResult.Yes)
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
        

        private void ShowIndividual_Checked(object sender, RoutedEventArgs e)
        {
            CombineFailsCheckbox.IsChecked = false;
            dataQuery = "SELECT startnummer, fehler, id FROM strafrunden;";
            if (sql != null)
                MenuItem_Click(sender, e);
            ShowIndividual.IsEnabled = false;
            AnsichtStatus2.Content = Properties.strings.detailed;
            DataOutput.Columns[2].Visibility = Visibility.Visible;
        }

        private void ShowIndividual_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowIndividual.IsEnabled = true;
        }

        private void HelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(String.Format(Properties.strings.HelpFormat,System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()), Properties.strings.Help, MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show(Properties.strings.OnlyNumbers+"\n"+ex.Message, Properties.strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show(String.Format(Properties.strings.FIleOpenErrorFromat, ExcelFileName.Header.ToString()), Properties.strings.Error);
            }
            catch(Exception ex)
            {
                MessageBox.Show(Properties.strings.Error+": "+ ex.Message,ex.ToString());
            }
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = Properties.strings.SelectExcelFile;
            sfd.Filter = Properties.strings.ExcelFile+" | *.xlsx";
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
                ExcelModeStatus.Content = Properties.strings.automatic + (ExcelCombine.IsChecked ? ", " + Properties.strings.combined : ", " + Properties.strings.detailed);
            else
                ExcelModeStatus.Content = Properties.strings.manual + (ExcelCombine.IsChecked ? ", " + Properties.strings.combined : ", " + Properties.strings.detailed);
            Properties.Settings.Default.AutoExportExcel = ExportExcel.IsChecked;
             
        }

        private void ExcelCombine_Click(object sender, RoutedEventArgs e)
        {
            if (ExportExcel.IsChecked)
                ExcelModeStatus.Content = Properties.strings.automatic + (ExcelCombine.IsChecked ? ", " + Properties.strings.combined : ", " + Properties.strings.detailed);
            else
                ExcelModeStatus.Content = Properties.strings.manual + (ExcelCombine.IsChecked ? ", " + Properties.strings.combined : ", " + Properties.strings.detailed);
            Properties.Settings.Default.CombineExcel = ExcelCombine.IsChecked;
             
        }
    }
}
