using System;
using System.Data;

namespace diplom_loskutova.Page
{
    public partial class Reports : System.Windows.Controls.Page
    {
        public Reports()
        {
            InitializeComponent();
        }
        private void OpenPage(int numberReport)
        {
            diplom_loskutova.UserControlReport page;
            page = new diplom_loskutova.UserControlReport(numberReport);
            NavigationService.Navigate(page);
        }

        private void Report1_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenPage(1);
        }

        private void Report2_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenPage(2);
        }

        private void Report3_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenPage(3);
        }

        private void Report4_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenPage(4);
        }

        private void Report5_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenPage(5);
        }

        private void Report6_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenPage(6);
        }

        private void Report7_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenPage(7);
        }

        private void Report8_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenPage(8);
        }

        private void Report9_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenPage(9);
        }

        private void Report10_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenPage(10);
        }

        private void Report11_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenPage(11);
        }

        private void Report12_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenPage(12);
        }

        private void Report13_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenPage(13);
        }
    }
}
