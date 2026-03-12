using System.Collections;
using System.Windows.Controls;

namespace diplom_loskutova.Helpers
{
    public static class ComboBoxHelper
    {
        public static void LoadData(ComboBox comboBox, IEnumerable dataSource, string displayMemberPath, string selectedValuePath)
        {
            comboBox.ItemsSource = dataSource;
            comboBox.DisplayMemberPath = displayMemberPath;
            comboBox.SelectedValuePath = selectedValuePath;
        }
    }
}
