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
using System.Windows.Shapes;

namespace Strafrunden
{
    /// <summary>
    /// Interaktionslogik für TransponderEntryDialog.xaml
    /// </summary>
    public partial class TransponderEntryDialog : Window
    {
        public string Code { get; set; }
        public int Startnummer { get; set; }

        public bool CodeDisabled { get; set; } = false;
        public bool StartnummerDisabled { get; set; } = false;
        public TransponderEntryDialog()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
