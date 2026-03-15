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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using MessageBox = System.Windows.Forms.MessageBox;

namespace diplom_loskutova.Page
{
    public partial class Applications : System.Windows.Controls.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["diplom_loskutova.Properties.Settings.DP_2025_LoskutovaConnectionString"].ConnectionString;
        private DP_2025_LoskutovaDataSetTableAdapters.ЗАЯВКАTableAdapter adapter = new DP_2025_LoskutovaDataSetTableAdapters.ЗАЯВКАTableAdapter();
        private DP_2025_LoskutovaDataSet db = new DP_2025_LoskutovaDataSet();

        public Applications(string _role)
        {
            InitializeComponent();

            LoadData();
            LoadToComboBox();
            LoadApplicationStats();
            SetupRoleVisibility(_role);
            Loaded += (s, e) => BuildApplicationStatusChart();
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
                //tbTotalUsers.Text = db.ЗАЯВКА.Count.ToString();
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
            double[] positions = Enumerable.Range(1, values.Length).Select(x => (double)x).ToArray();
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
        s.Название as StatusName,
        ISNULL(COUNT(a.ID_Заявки), 0) as StatusCount
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

        // Навигирует на страницу редактирования выбранного элемента.
        private void BtnChange(object sender, RoutedEventArgs e)
        {
            NavigatePageSelectedRow();
        }

        // Переходит на страницу редактирования выбранной записи, если она выбрана.
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

        // Универсальный метод для выбора строки из ListView как DataRowView.
        private bool TryGetSelectedRow(out DataRowView selectedRowView)
        {
            selectedRowView = listViewApplication.SelectedItem as DataRowView;
            return selectedRowView != null;
        }

        // Открывает страницу добавления или изменения записи.
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
            if (db.ЗАЯВКА.Rows.Count == 0)
            {
                listViewApplication.ItemsSource = null;
                return;
            }

            List<string> filters = new List<string>();

            if (ComboBoxSearchFIO.SelectedValue != null)
            {
                filters.Add($"ID_Гражданина = {ComboBoxSearchFIO.SelectedValue}");
            }

            if (ComboBoxSearchStatus.SelectedValue != null)
            {
                filters.Add($"ID_Статуса = {ComboBoxSearchStatus.SelectedValue}");
            }

            if (ComboBoxSearchEvent.SelectedValue != null)
            {
                filters.Add($"ID_Мероприятия = {ComboBoxSearchEvent.SelectedValue}");
            }

            if (DatePickerSearch.SelectedDate.HasValue)
            {
                string selectedDateStr = DatePickerSearch.SelectedDate.Value.ToString("yyyy-MM-dd");
                filters.Add($"Дата_Создания = '{selectedDateStr}'");
            }

            string filter = string.Join(" AND ", filters);

            DataView dv = db.ЗАЯВКА.DefaultView;
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

        private void ComboBoxSearchFIO_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ApplyFilter();

        }

        private void ComboBoxSearchStatus_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ApplyFilter();

        }

        private void ComboBoxSearchEvent_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
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

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}