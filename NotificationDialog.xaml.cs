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
        public NotificationDialog(string _title, string _text, string _desccription)
        {
            InitializeComponent();
            title.Text = _title;
            text.Text = _text;
            description.Text = _desccription;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
