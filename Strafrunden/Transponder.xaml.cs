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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Strafrunden
{
    /// <summary>
    /// Interaktionslogik für Transponder.xaml
    /// </summary>
    public partial class Transponder : Window
    {
        /// <summary>
        /// all registrations
        /// </summary>
        public ObservableCollection<int[]> data;
        private string dataQuery = "SELECT startnummer, COUNT(timestamp) FROM registrations GROUP BY startnummer;";

        /// <summary>
        /// missing penalties
        /// </summary>
        public ObservableCollection<int[]> data2;
        private string data2Query = "select fails.stnr, fails.err - regs.reg as miss from (select s.startnummer as stnr, sum(s.fehler) as err from strafrunden as s group by  s.startnummer) as fails left join (select startnummer as stnr, count(timestamp) as reg from registrations group by startnummer) as regs on fails.stnr = regs.stnr ;";

        private Timer timer;
        private SqlConnection sql;
        public Transponder(SqlConnection s)
        {

            data = new ObservableCollection<int[]>();

            data2 = new ObservableCollection<int[]>();
            timer = new Timer();
            InitializeComponent();
            DataOutput.ItemsSource = data;
            DataOutput2.ItemsSource = data2;
            DataContext = this;
            timer.Enabled = true;
            timer.Interval = 1000;
            timer.Tick += Timer_Elapsed;
            sql = s;
        }

        private void Timer_Elapsed(object sender, EventArgs e)
        {
            if (AutoUpdateMenuItem.IsChecked)
                MenuItem_Click(sender, new RoutedEventArgs());
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlCommand com = sql.CreateCommand())
                {
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
                }
                using (SqlCommand com = sql.CreateCommand())
                {
                    com.CommandText = data2Query;
                    SqlDataReader rd = com.ExecuteReader();
                    data2.Clear();
                    if (rd.HasRows)
                    {
                        while (rd.Read())
                        {
                            if (!rd.IsDBNull(1) && rd.GetInt32(1) != 0)
                            {
                                if (rd.FieldCount <= 2)
                                    data2.Add(new int[] { rd.GetInt32(0), rd.GetInt32(1) });
                                else
                                    data2.Add(new int[] { rd.GetInt32(0), rd.GetInt32(1), rd.GetInt32(2) });
                            }
                        }
                    }
                    rd.Close();
                }

                LastUpdateStatus.Content = System.DateTime.Now.ToLongTimeString() + " " + System.DateTime.Now.ToShortDateString();
            }
            catch(Exception ex)
            {
                Logging.Log.Instance.Warn("[Transponder-Window][Exception]" + ex.ToString());
            }

        }
    }
}
