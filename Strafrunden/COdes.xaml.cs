using System;
using System.Collections.Generic;
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
using TLookup= Strafrunden.Resources.TransponderLookup;

namespace Strafrunden
{
    /// <summary>
    /// Interaktionslogik für COdes.xaml
    /// </summary>
    public partial class COdes : Window
    {
        public COdes()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataOutput.ItemsSource = TLookup.List();
            LastUpdateStatus.Content = DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToShortDateString();
        }

        private void AddMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var ted = new TransponderEntryDialog();
            bool? res = ted.ShowDialog();
            if (res.HasValue && res.Value)
                TLookup.Add(ted.Code, ted.Startnummer);
            MenuItem_Click(sender, e);
        }

        private void RemoveMeuItem_Click(object sender, RoutedEventArgs e)
        {
            var ted = new TransponderEntryDialog();
            bool? res = ted.ShowDialog();
            if (res.HasValue && res.Value)
            {
                if(!string.IsNullOrWhiteSpace(ted.Code))
                    TLookup.Remove(ted.Code);
                else if(ted.Startnummer != 0)
                    TLookup.Remove(ted.Startnummer);
            }
            MenuItem_Click(sender, e);
        }

        private void ClearMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var mbr = System.Windows.MessageBox.Show("Wirklich alle Transpondercodes löschen?", "Warnung", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if(mbr == MessageBoxResult.Yes)
            {
                TLookup.Clear();
            }
            MenuItem_Click(sender, e);
        }

        private void CSVMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.Filter = "CSV-Dateien|*.csv";
            var res = ofd.ShowDialog();
            if(res == System.Windows.Forms.DialogResult.OK)
            {
                string[] lines = System.IO.File.ReadAllLines(ofd.FileName);
                foreach (string line in lines)
                {
                    try
                    {
                        TLookup.Add(line.Split(',')[0], int.Parse(line.Split(',')[1]));
                    }
                    catch (Exception ex)
                    {
                        Logging.Log.Instance.Warn("Adding csv based transponder failed:");
                        Logging.Log.Instance.Error(ex.ToString());
                    }
                }
            }
            MenuItem_Click(sender, e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            MenuItem_Click(sender, e);
        }

        private void TCEquivMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            Strafrunden.Resources.TransponderLookup.UseIdentity = true;
        }

        private void TCEquivMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            Strafrunden.Resources.TransponderLookup.UseIdentity = false;
        }
    }
}
