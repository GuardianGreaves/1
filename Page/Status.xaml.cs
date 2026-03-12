using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;

namespace diplom_loskutova.Page
{
    public partial class Status : System.Windows.Controls.Page
    {
        private DP_2025_LoskutovaDataSetTableAdapters.СТАТУСTableAdapter adapter = new DP_2025_LoskutovaDataSetTableAdapters.СТАТУСTableAdapter();
        private DP_2025_LoskutovaDataSet db = new DP_2025_LoskutovaDataSet();   // Объект для работы с данными из базы (DataSet)
        private SqlDataAdapter dataAdapter = new SqlDataAdapter();
        private DataTable statusTable = new DataTable();
        private int currentPage = 1;
        private int pageSize = 5;
        private int totalRecords = 0;
        private string connectionString = ConfigurationManager.ConnectionStrings["diplom_loskutova.Properties.Settings.DP_2025_LoskutovaConnectionString"].ConnectionString;
        private DP_2025_LoskutovaDataSetTableAdapters.ЗАЯВКАTableAdapter relatedAdapter = new DP_2025_LoskutovaDataSetTableAdapters.ЗАЯВКАTableAdapter();

        public Status(string _role)
        {
            InitializeComponent();
            LoadTotalCount();
            LoadPageData();

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

        private void LoadTotalCount()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = "SELECT COUNT(*) FROM СТАТУС";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    totalRecords = (int)cmd.ExecuteScalar();
                    UpdatePagingInfo();
                }
            }
        }

        private void LoadPageData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    dataAdapter.SelectCommand = new SqlCommand("GetStatusPaged", conn);
                    dataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    dataAdapter.SelectCommand.Parameters.AddWithValue("@PageNumber", currentPage);
                    dataAdapter.SelectCommand.Parameters.AddWithValue("@PageSize", pageSize);

                    statusTable.Clear();
                    dataAdapter.Fill(statusTable);
                    listViewStatus.ItemsSource = statusTable.DefaultView;
                }
                UpdatePagingInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void UpdatePagingInfo()
        {
            int shownRecords = statusTable.Rows.Count;
            int firstRecord = (currentPage - 1) * pageSize + 1;
            int lastRecord = Math.Min(firstRecord + shownRecords - 1, totalRecords);

            tbPageNumber.Text = $"Страница {currentPage}";
            tbRecordsInfo.Text = $"Отображаются строки с {firstRecord} по {lastRecord} из {totalRecords}";

            btnPrev.IsEnabled = currentPage > 1;
            btnNext.IsEnabled = currentPage * pageSize < totalRecords;
            tbTotalUsers.Text = totalRecords.ToString();
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadPageData();
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            if (currentPage < totalPages)
            {
                currentPage++;
                LoadPageData();
            }
        }

        private void LoadData()
        {
            try
            {
                adapter.Fill(db.СТАТУС);
                listViewStatus.ItemsSource = db.СТАТУС.DefaultView;
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
                int typeEventId = Convert.ToInt32(selectedRowView["ID_Статуса"]);
                var relatedRows = relatedAdapter.GetData();
                bool hasRelated = false;

                foreach (var row in relatedRows)
                {
                    if (row.ID_Статуса == typeEventId)
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
                try
                {
                    adapter.Update(db.СТАТУС);
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
            selectedRowView = listViewStatus.SelectedItem as DataRowView;
            return selectedRowView != null;
        }

        private void OpenPage(bool isChangeOrAdd, DataRowView rowView = null)
        {
            diplom_loskutova.Page.AddOrChange.StatusAOC page;
            if (rowView != null)
                page = new diplom_loskutova.Page.AddOrChange.StatusAOC(rowView);
            else
                page = new diplom_loskutova.Page.AddOrChange.StatusAOC();

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
            if (db.СТАТУС.Rows.Count == 0)
            {
                listViewStatus.ItemsSource = null;
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

            DataView dv = db.СТАТУС.DefaultView;
            dv.RowFilter = filter;
            listViewStatus.ItemsSource = dv;
        }

        private void BtnResetFilter_Click(object sender, RoutedEventArgs e)
        {
            TextBoxSearchName.Text = "";      
            ApplyFilter();                          
        }
    }
}