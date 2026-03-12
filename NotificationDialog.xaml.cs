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

namespace diplom_loskutova
{
    /// <summary>
    /// Логика взаимодействия для NotificationDialog.xaml
    /// </summary>
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
