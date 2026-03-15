using diplom_loskutova.Helpers;
using ScottPlot;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using MessageBox = System.Windows.Forms.MessageBox;

namespace diplom_loskutova.Page
{
    public partial class Events : System.Windows.Controls.Page
    {
        private DP_2025_LoskutovaDataSetTableAdapters.МЕРОПРИЯТИЕTableAdapter adapter = new DP_2025_LoskutovaDataSetTableAdapters.МЕРОПРИЯТИЕTableAdapter();
        private DP_2025_LoskutovaDataSet db = new DP_2025_LoskutovaDataSet();

        public Events(string _role)
        {
            InitializeComponent();
            LoadData();
            LoadToComboBox();
            LoadUserStats();

            var visibilityManager = new Class.RoleVisibilityManager(_role);
            visibilityManager.SetButtonVisibility(btnDelete, btnAdd, btnChange);
        }

        private void LoadUserStats()
        {
            try
            {
                adapter.Fill(db.МЕРОПРИЯТИЕ);
                //tbTotalEvent.Text = db.МЕРОПРИЯТИЕ.Count.ToString();
                listViewEvents.ItemsSource = db.МЕРОПРИЯТИЕ.DefaultView;
            }
            catch (Exception ex)
            {
                var msg = new diplom_loskutova.NotificationDialog(
                    "Ошибка",
                    "Ошибка загрузки данных:",
                    $"{ex.Message}");
                msg.ShowDialog();
                return;
            }
            BuildEventTypeHistogram();
        }

        private void BuildEventTypeHistogram()
        {
            var eventData = GetEventTypeStatistics();

            if (eventData.Rows.Count == 0)
                return;
            WpfPlot1.Plot.Clear();
            var values = eventData.AsEnumerable()
                .Select(row => Convert.ToDouble(row["Count"]))
                .ToArray();
            var labels = eventData.AsEnumerable()
                .Select(row => row["TypeName"].ToString())
                .ToArray();
            double[] positions = Enumerable.Range(1, values.Length).Select(x => (double)x).ToArray();

            for (int i = 0; i < values.Length; i++)
            {
                double[] xs = { positions[i] };
                double[] ys = { values[i] };

                var bar = WpfPlot1.Plot.Add.Bars(xs, ys);
                bar.LegendText = labels[i];
            }

            WpfPlot1.Plot.Title("Распределение мероприятий по типам");
            WpfPlot1.Plot.ShowLegend(Alignment.UpperLeft);
            WpfPlot1.Plot.Axes.Margins(bottom: 0);
            WpfPlot1.Refresh();
        }
        private DataTable GetEventTypeStatistics()
        {
            string sql = @"
                        SELECT 
                            t.Название as TypeName,
                            ISNULL(COUNT(m.ID_Мероприятия), 0) as Count
                        FROM [dbo].[ТИП_МЕРОПРИЯТИЯ] t
                        LEFT JOIN [dbo].[МЕРОПРИЯТИЕ] m ON t.ID_Типа = m.ID_Типа
                        GROUP BY t.ID_Типа, t.Название
                        ORDER BY t.ID_Типа";

            DataTable dt = new DataTable();
            using (var adapter = new SqlDataAdapter(sql, ConfigurationManager.ConnectionStrings["diplom_loskutova.Properties.Settings.DP_2025_LoskutovaConnectionString"].ConnectionString))
            {
                adapter.Fill(dt);
            }
            return dt;
        }

        private void LoadData()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings[
                    "diplom_loskutova.Properties.Settings.DP_2025_LoskutovaConnectionString"
                ].ConnectionString;

                string query = @"
            SELECT М.ID_Мероприятия, М.ID_Пользователя, М.ID_Типа, 
                   П.Имя + ' ' + П.Фамилия AS q, 
                   Т.Название AS qq, 
                   М.Название, М.Описание, М.Дата_Мероприятия, М.Бюджет
            FROM МЕРОПРИЯТИЕ AS М 
            INNER JOIN ПОЛЬЗОВАТЕЛЬ AS П ON М.ID_Пользователя = П.ID_Пользователя 
            INNER JOIN ТИП_МЕРОПРИЯТИЯ AS Т ON М.ID_Типа = Т.ID_Типа";

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();

                    conn.Open();
                    adapter.Fill(dt);

                    listViewEvents.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\n\nStackTrace: {ex.StackTrace}", "Ошибка загрузки");
                var msg = new diplom_loskutova.NotificationDialog(
                "Ошибка",
                "Ошибка загрузки данных:",
                $"Ошибка: {ex.Message}\n\nStackTrace: {ex.StackTrace}");
                msg.ShowDialog();

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
                var msg = new diplom_loskutova.NotificationDialog(
                "Внимание",
                "Вы уверены, что хотите удалить эту запись?",
                $"Существуют связанные мероприятия",
                "btnYesCancel");
                var result = msg.ShowDialog();
                if (result == true)
                {
                    selectedRowView.Row.Delete();

                    try
                    {
                        msg = new diplom_loskutova.NotificationDialog(
                            "Выполнено",
                            "Запись успешно удалена из базы данных",
                            "");
                        msg.ShowDialog();

                        adapter.Update(db.МЕРОПРИЯТИЕ);
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        msg = new diplom_loskutova.NotificationDialog(
                            "Ошибка",
                            "Ошибка загрузки данных:",
                            $"{ex.Message}");
                        msg.ShowDialog();
                    }
                }
            }
            else
            {
                var msg = new diplom_loskutova.NotificationDialog(
                    "Ошибка",
                    "Выберите строку для удаления",
                    "Вы можете выделить нужную строку и нажать Удалить");
                msg.ShowDialog();
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

        private bool TryGetSelectedRow(out DataRowView selectedRowView)
        {
            selectedRowView = listViewEvents.SelectedItem as DataRowView;
            return selectedRowView != null;
        }

        private void NavigatePageSelectedRow()
        {
            if (TryGetSelectedRow(out DataRowView selectedRowView))
                OpenPage(true, selectedRowView);
            else
            {
                var msg = new diplom_loskutova.NotificationDialog(
                    "Ошибка",
                    "Выберите строку для редактирования",
                    "Вы можете дважды кликнуть или выделить нужную строку и нажать Редактировать");
                msg.ShowDialog();
            }
        }

        private void OpenPage(bool isChangeOrAdd, DataRowView rowView = null)
        {
            diplom_loskutova.Page.AddOrChange.EventsAOC page;
            if (rowView != null)
                page = new diplom_loskutova.Page.AddOrChange.EventsAOC(rowView);
            else
                page = new diplom_loskutova.Page.AddOrChange.EventsAOC();

            page.ChangeOrAdd = isChangeOrAdd;
            page.DataChanged += (s, ev) => LoadData();
            NavigationService.Navigate(page);
        }

        private void ApplyFilter()
        {
            if (db.МЕРОПРИЯТИЕ.Rows.Count == 0)
            {
                listViewEvents.ItemsSource = null;
                return;
            }

            string filter = "";

            var fio = ComboBoxSearchFIO.Text.Trim();
            if (!string.IsNullOrEmpty(fio))
            {
                if (filter.Length > 0)
                    filter += " AND ";
                filter += $"(Фамилия LIKE '%{fio}%' OR Имя LIKE '%{fio}%' OR Отчество LIKE '%{fio}%')";
            }

            if (ComboBoxSearchTypeEvent.SelectedValue != null)
            {
                filter += $"ID_Типа = {ComboBoxSearchTypeEvent.SelectedValue}";
            }

            var login = TextBoxSearchName.Text.Trim();
            if (!string.IsNullOrEmpty(login))
            {
                if (filter.Length > 0)
                    filter += " AND ";
                filter += $"Название LIKE '%{login}%'";
            }

            if (DatePickerSearchDate.SelectedDate.HasValue)
            {
                if (filter.Length > 0)
                    filter += " AND ";
                string selectedDateStr = DatePickerSearchDate.SelectedDate.Value.ToString("yyyy-MM-dd");
                filter += $"Дата_Мероприятия = #{selectedDateStr}#";
            }

            if (decimal.TryParse(TextBoxMinBudget.Text.Trim(), out decimal minBudget))
            {
                if (filter.Length > 0)
                    filter += " AND ";
                filter += $"Бюджет >= {minBudget.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            }

            if (decimal.TryParse(TextBoxMaxBudget.Text.Trim(), out decimal maxBudget))
            {
                if (filter.Length > 0)
                    filter += " AND ";
                filter += $"Бюджет <= {maxBudget.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            }

            DataView dv = db.МЕРОПРИЯТИЕ.DefaultView;
            dv.RowFilter = filter;
            listViewEvents.ItemsSource = dv;
        }

        private void ComboBoxSearchTypeEvent_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void TextBoxSearchName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void TextBoxSearchDate_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ComboBoxSearchDate_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void BtnResetFilter_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxSearchTypeEvent.SelectedIndex = -1;
            ComboBoxSearchFIO.SelectedIndex = -1;
            TextBoxSearchName.Text = "";
            DatePickerSearchDate.SelectedDate = null;
            TextBoxMinBudget.Text = "";
            TextBoxMaxBudget.Text = "";
            ApplyFilter();
        }

        private void ComboBoxSearchFIO_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ComboBoxSearchMoney_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void TextBoxMinBudget_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void TextBoxMaxBudget_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ApplyFilter();
        }
        private void DatePickerSearchDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void LoadToComboBox()
        {
            var userAdapter = new DP_2025_LoskutovaDataSetTableAdapters.ПОЛЬЗОВАТЕЛЬTableAdapter();

            var usersTable = userAdapter.GetData();
            usersTable.Columns.Add("FullName", typeof(string), "Фамилия + ' ' + Имя + ' ' + Отчество");
            ComboBoxHelper.LoadData(ComboBoxSearchFIO, usersTable, "FullName", "ID_Пользователя");

            var typeEventAdapter = new DP_2025_LoskutovaDataSetTableAdapters.ТИП_МЕРОПРИЯТИЯTableAdapter();
            ComboBoxHelper.LoadData(ComboBoxSearchTypeEvent, typeEventAdapter.GetData(), "Название", "ID_Типа");
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}