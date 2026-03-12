using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

public static class DataGridHelper
{
    public static void SelectRow(DataGrid dataGrid, MouseButtonEventArgs e)
    {
        // Снять выделение со всех строк
        dataGrid.UnselectAll();

        DependencyObject dep = (DependencyObject)e.OriginalSource;

        // Поиск DataGridRow под курсором
        while ((dep != null) && !(dep is DataGridRow))
        {
            dep = VisualTreeHelper.GetParent(dep);
        }

        if (dep == null)
            return;

        DataGridRow row = dep as DataGridRow;
        row.IsSelected = true;
    }
}
