using diplom_loskutova.Helpers;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MessageBox = System.Windows.Forms.MessageBox;

namespace diplom_loskutova.Page
{
    public partial class Applications : System.Windows.Controls.Page
    {
        private string connectionString =
            ConfigurationManager.ConnectionStrings[
                "diplom_loskutova.Properties.Settings.DP_2025_LoskutovaConnectionString"
            ].ConnectionString;

        private DP_2025_LoskutovaDataSetTableAdapters.ЗАЯВКАTableAdapter adapter =
            new DP_2025_LoskutovaDataSetTableAdapters.ЗАЯВКАTableAdapter();

        private DP_2025_LoskutovaDataSet db = new DP_2025_LoskutovaDataSet();

        private int currentPage = 1;
        private int pageSize = 5;
        private int totalRecords = 0;
        private SqlDataAdapter dataAdapter = new SqlDataAdapter();
        private DataTable statusTable = new DataTable();

        public Applications(string _role)
        {
            InitializeComponent();

            LoadTotalCount();
            LoadPageData();
            LoadToComboBox();
            SetupRoleVisibility(_role);
            Loaded += (s, e) => BuildApplicationStatusChart();
        }

        private void LoadTotalCount()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = "SELECT COUNT(*) FROM ЗАЯВКА";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    totalRecords = (int)cmd.ExecuteScalar();
                    UpdatePagingInfo();
                }
            }
        }

        private void SetupRoleVisibility(string role)
        {
            var visibilityManager = new Class.RoleVisibilityManager(role);
            visibilityManager.SetButtonVisibility(btnDelete, btnAdd, btnChange);
        }

        private void LoadApplicationStats()
        {
            try
            {
                adapter.FillBy(db.ЗАЯВКА);
                listViewApplication.ItemsSource = db.ЗАЯВКА.DefaultView;
            }
            catch (Exception ex)
            {
                var msg = new diplom_loskutova.NotificationDialog(
                    "Ошибка",
                    "Ошибка загрузки данных:",
                    $"{ex.Message}");
                msg.ShowDialog();
            }
        }

        private void BuildApplicationStatusChart()
        {
            var statusStats = GetApplicationStatusStatistics();

            if (statusStats.Rows.Count == 0)
                return;

            WpfPlot1.Plot.Clear();

            double[] values = statusStats.AsEnumerable()
                .Select(row => Convert.ToDouble(row["StatusCount"]))
                .ToArray();

            string[] labels = statusStats.AsEnumerable()
                .Select(row => row["StatusName"].ToString())
                .ToArray();

            double[] positions = Enumerable.Range(1, values.Length)
                .Select(x => (double)x)
                .ToArray();

            for (int i = 0; i < values.Length; i++)
            {
                double[] xs = { positions[i] };
                double[] ys = { values[i] };

                var bar = WpfPlot1.Plot.Add.Bars(xs, ys);
                bar.LegendText = labels[i];
            }

            WpfPlot1.Plot.Title("Распределение заявок по статусам");
            WpfPlot1.Plot.ShowLegend(Alignment.UpperRight);
            WpfPlot1.Plot.Axes.Margins(bottom: 0.1);
            WpfPlot1.Plot.Axes.SetLimitsY(0, values.Max() * 1.1);

            WpfPlot1.Plot.Axes.Bottom.Label.Text = "Статусы заявок";
            WpfPlot1.Plot.Axes.Left.Label.Text = "Количество заявок";

            WpfPlot1.Refresh();
        }

        private DataTable GetApplicationStatusStatistics()
        {
            string sql = @"
                SELECT DISTINCT
                    s.ID_Статуса,
                    s.Название AS StatusName,
                    ISNULL(COUNT(a.ID_Заявки), 0) AS StatusCount
                FROM [dbo].[СТАТУС] s
                LEFT JOIN [dbo].[ЗАЯВКА] a ON s.ID_Статуса = a.ID_Статуса
                GROUP BY s.ID_Статуса, s.Название
                ORDER BY s.ID_Статуса";

            DataTable dt = new DataTable();

            using (var adapter = new SqlDataAdapter(sql, connectionString))
            {
                adapter.Fill(dt);
            }

            return dt;
        }

        private void LoadData()
        {
            try
            {
                adapter.FillBy(db.ЗАЯВКА);
                listViewApplication.ItemsSource = db.ЗАЯВКА.DefaultView;
            }
            catch (Exception ex)
            {
                var msg = new diplom_loskutova.NotificationDialog(
                    "Ошибка",
                    "Ошибка загрузки данных:",
                    $"{ex.Message}");
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
                    "Оповещение",
                    "Подтверждение удаления",
                    "Вы уверены что хотите удалить запись ?",
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

                        adapter.Update(db.ЗАЯВКА);
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
                    "Выделите нужную строку и нажмите Удалить");
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

        private void NavigatePageSelectedRow()
        {
            var msg = new diplom_loskutova.NotificationDialog(
                "Ошибка",
                "Выберите строку для редактирования",
                "Вы можете дважды кликнуть или выделить нужную строку и нажать Редактировать");

            if (TryGetSelectedRow(out DataRowView selectedRowView))
                OpenPage(true, selectedRowView);
            else
                msg.ShowDialog();
        }

        private bool TryGetSelectedRow(out DataRowView selectedRowView)
        {
            selectedRowView = listViewApplication.SelectedItem as DataRowView;
            return selectedRowView != null;
        }

        private void OpenPage(bool isChangeOrAdd, DataRowView rowView = null)
        {
            diplom_loskutova.Page.AddOrChange.ApplicationsAOC page;

            if (rowView != null)
                page = new diplom_loskutova.Page.AddOrChange.ApplicationsAOC(rowView);
            else
                page = new diplom_loskutova.Page.AddOrChange.ApplicationsAOC();

            page.ChangeOrAdd = isChangeOrAdd;
            page.DataChanged += (s, ev) => LoadData();
            NavigationService.Navigate(page);
        }

        private void ApplyFilter()
        {
            if (statusTable.Rows.Count == 0)
            {
                listViewApplication.ItemsSource = null;
                return;
            }

            var conditions = new List<string>();

            // ФИЛЬТР ПО ID_Гражданина (из комбобокса)
            if (ComboBoxSearchFIO.SelectedValue != null)
            {
                int idGr = (int)ComboBoxSearchFIO.SelectedValue;
                conditions.Add($"ID_Гражданина = {idGr}");
            }

            // ФИЛЬТР ПО ID_Статуса
            if (ComboBoxSearchStatus.SelectedValue != null)
            {
                int idSt = (int)ComboBoxSearchStatus.SelectedValue;
                conditions.Add($"ID_Статуса = {idSt}");
            }

            // ФИЛЬТР ПО ID_Мероприятия
            if (ComboBoxSearchEvent.SelectedValue != null)
            {
                int idEv = (int)ComboBoxSearchEvent.SelectedValue;
                conditions.Add($"ID_Мероприятия = {idEv}");
            }

            // ФИЛЬТР ПО ДАТЕ
            if (DatePickerSearch.SelectedDate.HasValue)
            {
                string selectedDateStr = DatePickerSearch.SelectedDate.Value.ToString("yyyy-MM-dd");
                conditions.Add($"Дата_Создания = '{selectedDateStr}'");
            }

            string filter = string.Join(" AND ", conditions);
            DataView dv = statusTable.DefaultView;
            dv.RowFilter = filter;
            listViewApplication.ItemsSource = dv;
        }

        private void BtnResetFilter_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxSearchEvent.SelectedIndex = -1;
            ComboBoxSearchFIO.SelectedIndex = -1;
            ComboBoxSearchStatus.SelectedIndex = -1;
            ApplyFilter();
        }

        private void ComboBoxSearchFIO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ComboBoxSearchStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ComboBoxSearchEvent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void DatePickerSearch_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void LoadToComboBox()
        {
            var userAdapter = new DP_2025_LoskutovaDataSetTableAdapters.ГРАЖДАНИНTableAdapter();
            
            var usersTable = userAdapter.GetData();
            usersTable.Columns.Add("FullName", typeof(string), "Фамилия + ' ' + Имя + ' ' + Отчество");
            ComboBoxHelper.LoadData(ComboBoxSearchFIO, usersTable, "FullName", "ID_Гражданина");

            var statusAdapter = new DP_2025_LoskutovaDataSetTableAdapters.СТАТУСTableAdapter();
            ComboBoxHelper.LoadData(ComboBoxSearchStatus, statusAdapter.GetData(), "Название", "ID_Статуса");

            var eventAdapter = new DP_2025_LoskutovaDataSetTableAdapters.МЕРОПРИЯТИЕTableAdapter();
            ComboBoxHelper.LoadData(ComboBoxSearchEvent, eventAdapter.GetData(), "Название", "ID_Мероприятия");
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
        }

        private void LoadPageData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string sqlPage = @"
                        SELECT 
                            ЗАЯВКА.ID_Заявки,
                            ЗАЯВКА.ID_Гражданина,
                            ГРАЖДАНИН.Фамилия + ' ' + ГРАЖДАНИН.Имя + ' ' + ГРАЖДАНИН.Отчество AS ИмяПользователя,
                            ЗАЯВКА.ID_Статуса,
                            СТАТУС.Название AS СтатусМероприятия,
                            ЗАЯВКА.ID_Мероприятия,
                            МЕРОПРИЯТИЕ.Название,
                            ЗАЯВКА.Дата_Создания
                        FROM dbo.ЗАЯВКА
                        INNER JOIN dbo.ГРАЖДАНИН ON ЗАЯВКА.ID_Гражданина = ГРАЖДАНИН.ID_Гражданина
                        INNER JOIN dbo.СТАТУС ON ЗАЯВКА.ID_Статуса = СТАТУС.ID_Статуса
                        INNER JOIN dbo.МЕРОПРИЯТИЕ ON ЗАЯВКА.ID_Мероприятия = МЕРОПРИЯТИЕ.ID_Мероприятия
                        ORDER BY ЗАЯВКА.ID_Заявки
                        OFFSET (@Offset) ROWS
                        FETCH NEXT @PageSize ROWS ONLY;
                    ";

                    dataAdapter.SelectCommand = new SqlCommand(sqlPage, conn);
                    dataAdapter.SelectCommand.Parameters.AddWithValue("@Offset", (currentPage - 1) * pageSize);
                    dataAdapter.SelectCommand.Parameters.AddWithValue("@PageSize", pageSize);

                    statusTable.Clear();
                    dataAdapter.Fill(statusTable);
                    listViewApplication.ItemsSource = statusTable.DefaultView;

                    string sqlCount = "SELECT COUNT(*) AS TotalCount FROM dbo.ЗАЯВКА";

                    using (SqlCommand cmdCount = new SqlCommand(sqlCount, conn))
                    {
                        conn.Open();
                        object result = cmdCount.ExecuteScalar();
                        conn.Close();

                        if (result != null && int.TryParse(result.ToString(), out int totalCount))
                        {
                            totalRecords = totalCount;
                        }
                    }
                }

                UpdatePagingInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
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
    }
}
