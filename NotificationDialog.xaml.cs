using System;
using System.Windows;
using System.Windows.Input;

namespace diplom_loskutova
{
    public partial class NotificationDialog : Window
    {
        public NotificationDialog(string _title, string _text, string _desccription, string _type = "btnOK")
        {
            InitializeComponent();
            title.Text = _title;
            text.Text = _text;
            description.Text = _desccription;

            if (_type == "btnYesCancel")
            {
                btnCancel.Visibility = Visibility.Visible;
                btnOK.Visibility = Visibility.Visible;
                btnOK.Content = "Да";
            }
            else {
                btnCancel.Visibility = Visibility.Collapsed;
                btnOK.Visibility = Visibility.Visible;
                btnOK.Content = "Ok";
            }

        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        public event EventHandler<bool> DialogClosed;  // true=OK, false=Cancel

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
