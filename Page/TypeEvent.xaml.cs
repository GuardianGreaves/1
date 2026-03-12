using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace diplom_loskutova.Page
{
    public partial class TypeEvent : System.Windows.Controls.Page
    {
        private DP_2025_LoskutovaDataSetTableAdapters.ТИП_МЕРОПРИЯТИЯTableAdapter adapter = new DP_2025_LoskutovaDataSetTableAdapters.ТИП_МЕРОПРИЯТИЯTableAdapter();
        private DP_2025_LoskutovaDataSet db = new DP_2025_LoskutovaDataSet();
        private DP_2025_LoskutovaDataSetTableAdapters.МЕРОПРИЯТИЕTableAdapter relatedAdapter = new DP_2025_LoskutovaDataSetTableAdapters.МЕРОПРИЯТИЕTableAdapter();

        public TypeEvent(string _role)
        {
            InitializeComponent();
            LoadData(); 

            var visibilityManager = new Class.RoleVisibilityManager(_role);
            if (_role == "1")
            {
                btnDelete.Visibility = Visibility.Collapsed;
                btnAdd.Visibility = Visibility.Collapsed;
                btnChange.Visibility = Visibility.Collapsed;
            }
            else
            {
                visibilityManager.SetButtonVisibility(btnDelete, btnAdd, btnChange);
            }
        }

        private void LoadData()
        {
            try
            {
                adapter.Fill(db.ТИП_МЕРОПРИЯТИЯ);
                listViewTypeEvent.ItemsSource = db.ТИП_МЕРОПРИЯТИЯ.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
            }
            LoadStats();
        }

        private void LoadStats()
        {
            try
            {
                adapter.Fill(db.ТИП_МЕРОПРИЯТИЯ);
                tbTotalUsers.Text = db.ТИП_МЕРОПРИЯТИЯ.Count.ToString();
                listViewTypeEvent.ItemsSource = db.ТИП_МЕРОПРИЯТИЯ.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
            }
        }

        private void BtnAdd(object sender, RoutedEventArgs e)
        {
            OpenPage(false);
        }

        private void BtnDelete(object sender, RoutedEventArgs e)
        {
            if (TryGetSelectedRow(out DataRowView selectedRowView))
            {
                int typeEventId = Convert.ToInt32(selectedRowView["ID_Типа"]);
                var relatedRows = relatedAdapter.GetData();
                bool hasRelated = false;

                foreach (var row in relatedRows)
                {
                    if (row.ID_Типа == typeEventId)
                    {
                        hasRelated = true;
                        break;
                    }
                }

                if (hasRelated)
                {
                    var result = MessageBox.Show("Существуют связанные мероприятия. Вы уверены, что хотите удалить эту запись?",
                        "Подтверждение удаления", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                    if (result != MessageBoxResult.OK)
                        return; 
                }
                else
                {
                    var result = MessageBox.Show("Вы уверены что хотите удалить запись ?", "Подтверждение удаления", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                    if (result != MessageBoxResult.OK)
                        return; 
                }

                selectedRowView.Row.Delete();

                try
                {
                    adapter.Update(db.ТИП_МЕРОПРИЯТИЯ);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка");
                }
            }
            else
            {
                MessageBox.Show("Выберите строку для удаления.");
            }
        }

        private void BtnChange(object sender, RoutedEventArgs e)
        {
            NavigatePageSelectedRow();
        }

        private void ListViewStatus_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            NavigatePageSelectedRow();
        }

        private void NavigatePageSelectedRow()
        {
            if (TryGetSelectedRow(out DataRowView selectedRowView))
                OpenPage(true, selectedRowView);
            else
                MessageBox.Show("Выберите строку для редактирования.");
        }

        private bool TryGetSelectedRow(out DataRowView selectedRowView)
        {
            selectedRowView = listViewTypeEvent.SelectedItem as DataRowView;
            return selectedRowView != null;
        }

        private void OpenPage(bool isChangeOrAdd, DataRowView rowView = null)
        {
            diplom_loskutova.Page.AddOrChange.TypeEventAOC page;
            if (rowView != null)
                page = new diplom_loskutova.Page.AddOrChange.TypeEventAOC(rowView);
            else
                page = new diplom_loskutova.Page.AddOrChange.TypeEventAOC();

            page.ChangeOrAdd = isChangeOrAdd;
            page.DataChanged += (s, ev) => LoadData();
            NavigationService.Navigate(page);
        }

        private void TextBoxSearchName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ApplyFilter();                        
        }

        private void ApplyFilter()
        {
            if (db.ТИП_МЕРОПРИЯТИЯ.Rows.Count == 0)
            {
                listViewTypeEvent.ItemsSource = null;
                return;
            }

            string filter = "";
            var name = TextBoxSearchName.Text.Trim();

            if (!string.IsNullOrEmpty(name))
            {
                if (filter.Length > 0)
                    filter += " AND ";
                filter += $"(Название LIKE '%{name}%')";
            }

            DataView dv = db.ТИП_МЕРОПРИЯТИЯ.DefaultView;
            dv.RowFilter = filter;
            listViewTypeEvent.ItemsSource = dv;
        }

        private void BtnResetFilter_Click(object sender, RoutedEventArgs e)
        {
            TextBoxSearchName.Text = "";          
            ApplyFilter();                          
        }
    }
}