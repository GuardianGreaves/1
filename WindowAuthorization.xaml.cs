using System.Windows;
using System.Windows.Input;

namespace diplom_loskutova
{
    public partial class WindowAuthorization : Window
    {
        public WindowAuthorization()
        {
            InitializeComponent();
            diplom_loskutova.Page.Authorization page = new diplom_loskutova.Page.Authorization();
            mainFrame.Navigate(new diplom_loskutova.Page.Authorization());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
