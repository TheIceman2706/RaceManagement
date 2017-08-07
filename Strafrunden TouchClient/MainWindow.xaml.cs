using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Strafrunden_TouchClient
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool firstSelected;
        private static readonly HttpClient client = new HttpClient();
        public MainWindow()
        {
            InitializeComponent();
            firstSelected = true;
        }
        
        private void Number1Button_Click(object sender, RoutedEventArgs e)
        {
            AddNumber(1);
        }

        private void AddNumber(int Nr)
        {
            int old = 0;
            if(firstSelected)
                old = Convert.ToInt32(StartnummerText.Text);
            else
                old = Convert.ToInt32(FehlwürfeText.Text);
            old *= 10;
            old += Nr;
            if (firstSelected)
                StartnummerText.Text = old.ToString();
            else
                FehlwürfeText.Text = old.ToString();
        }

        private void RemoveNumber()
        {

            int old = 0;
            if (firstSelected)
                old = Convert.ToInt32(StartnummerText.Text);
            else
                old = Convert.ToInt32(FehlwürfeText.Text);
            old /= 10;
            if (firstSelected)
                StartnummerText.Text = old.ToString();
            else
                FehlwürfeText.Text = old.ToString();
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            if (firstSelected)
            {
                firstSelected = false;
                StartnummerText.BorderThickness = new Thickness(1);
                FehlwürfeText.BorderThickness = new Thickness(3);
            }
            else
            {
                firstSelected = true;
                StartnummerText.BorderThickness = new Thickness(3);
                FehlwürfeText.BorderThickness = new Thickness(1);
                var values = new Dictionary<string, string>
                {
                   { "nr", StartnummerText.Text },
                   { "thrown", FehlwürfeText.Text }
                };

                var content = new FormUrlEncodedContent(values);

                client.PostAsync(UrlText.Text, content);

                StartnummerText.Text = "0";
                FehlwürfeText.Text = "0";
            }
        }

        private void Number2Button_Click(object sender, RoutedEventArgs e)
        {
            AddNumber(2);
        }

        private void Number0Button_Click(object sender, RoutedEventArgs e)
        {
            AddNumber(0);
        }

        private void Number3Button_Click(object sender, RoutedEventArgs e)
        {
            AddNumber(3);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveNumber();
        }

        private void Number4Button_Click(object sender, RoutedEventArgs e)
        {
            AddNumber(4);
        }

        private void Number5Button_Click(object sender, RoutedEventArgs e)
        {
            AddNumber(5);
        }

        private void Number6Button_Click(object sender, RoutedEventArgs e)
        {
            AddNumber(6);
        }

        private void Number7Button_Click(object sender, RoutedEventArgs e)
        {
            AddNumber(7);
        }

        private void Number8Button_Click(object sender, RoutedEventArgs e)
        {
            AddNumber(8);
        }

        private void Number9Button_Click(object sender, RoutedEventArgs e)
        {
            AddNumber(9);
        }
    }
}
