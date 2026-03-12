using System.Windows;
using System.Windows.Controls;

namespace diplom_loskutova.Class
{
    public class RoleVisibilityManager
    {
        private readonly string _role;

        public RoleVisibilityManager(string role)
        {
            _role = role;
        }

        public void SetButtonVisibility(Button btnDelete, Button btnAdd, Button btnChange)
        {
            if (_role == "3")
            {
                btnDelete.Visibility = Visibility.Collapsed;
                btnAdd.Visibility = Visibility.Collapsed;
                btnChange.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnDelete.Visibility = Visibility.Visible;
                btnAdd.Visibility = Visibility.Visible;
                btnChange.Visibility = Visibility.Visible;
            }
        }
    }
}