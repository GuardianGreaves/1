using ScottPlot;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace diplom_loskutova.Page
{
    public partial class Citizens : System.Windows.Controls.Page
    {
        private DP_2025_LoskutovaDataSetTableAdapters.ЗАЯВКАTableAdapter relatedAdapter = new DP_2025_LoskutovaDataSetTableAdapters.ЗАЯВКАTableAdapter();
        private DP_2025_LoskutovaDataSetTableAdapters.ГРАЖДАНИНTableAdapter adapter = new DP_2025_LoskutovaDataSetTableAdapters.ГРАЖДАНИНTableAdapter();
        private DP_2025_LoskutovaDataSet db = new DP_2025_LoskutovaDataSet();   // Объект для работы с данными из базы (DataSet)
        public Citizens(string _role)
        {
            InitializeComponent();
            LoadUserStats();
            LoadData();

            var visibilityManager = new Class.RoleVisibilityManager(_role);
            visibilityManager.SetButtonVisibility(btnDelete, btnAdd, btnChange);
        }

        private void LoadUserStats()
        {
            try
            {
                adapter.Fill(db.ГРАЖДАНИН);
                tbTotalCitizen.Text = db.ГРАЖДАНИН.Count.ToString();
                listViewCitizen.ItemsSource = db.ГРАЖДАНИН.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
                return;
            }
            BuildAgeHistogram();
        }

        private void BuildAgeHistogram()
        {
            var ageData = GetAgeGroupStatistics();
            if (ageData.Rows.Count == 0)
                return;
            double[] values = ageData.AsEnumerable()
                .Select(row => Convert.ToDouble(row["Count"]))
                .ToArray();
            string[] labels = ageData.AsEnumerable()
                .Select(row => row["Возрастная_группа"].ToString())
                .ToArray();
            double[] positions = Enumerable.Range(1, values.Length).Select(x => (double)x).ToArray();
            WpfPlot1.Plot.Clear();

            for (int i = 0; i < values.Length; i++)
            {
                double[] xs = { positions[i] };
                double[] ys = { values[i] };

                var bar = WpfPlot1.Plot.Add.Bars(xs, ys);
                bar.LegendText = labels[i];
            }

            WpfPlot1.Plot.ShowLegend(Alignment.UpperLeft);
            WpfPlot1.Plot.Axes.Margins(bottom: 0);
            WpfPlot1.Plot.Title("Распределение граждан по возрастным группам");
            WpfPlot1.Refresh();
        }
        private DataTable GetAgeGroupStatistics()
        {
            string sql = @"
                        SELECT 
                            Возрастная_группа,
                            COUNT(*) as Count
                        FROM (
                            SELECT 
                                CASE 
                                    WHEN DATEDIFF(YEAR, Дата_Рождения, GETDATE()) BETWEEN 40 AND 50 THEN '40-50'
                                    WHEN DATEDIFF(YEAR, Дата_Рождения, GETDATE()) BETWEEN 51 AND 60 THEN '51-60'
                                    WHEN DATEDIFF(YEAR, Дата_Рождения, GETDATE()) BETWEEN 61 AND 70 THEN '61-70'
                                    WHEN DATEDIFF(YEAR, Дата_Рождения, GETDATE()) BETWEEN 71 AND 80 THEN '71-80'
                                    WHEN DATEDIFF(YEAR, Дата_Рождения, GETDATE()) BETWEEN 81 AND 90 THEN '81-90'
                                    WHEN DATEDIFF(YEAR, Дата_Рождения, GETDATE()) >= 91 THEN '91+'
                                    ELSE 'До 40'
                                END as Возрастная_группа
                            FROM [dbo].[ГРАЖДАНИН]
                            WHERE Дата_Рождения IS NOT NULL
                        ) AS AgeGroups
                        GROUP BY Возрастная_группа
                        ORDER BY 
                            CASE Возрастная_группа
                                WHEN 'До 40' THEN 0
                                WHEN '40-50' THEN 1
                                WHEN '51-60' THEN 2
                                WHEN '61-70' THEN 3
                                WHEN '71-80' THEN 4
                                WHEN '81-90' THEN 5
                                WHEN '90+' THEN 6
                            END";

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
                adapter.Fill(db.ГРАЖДАНИН);
                listViewCitizen.ItemsSource = db.ГРАЖДАНИН.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
            }
            LoadUserStats();
        }

        private void BtnAdd(object sender, RoutedEventArgs e)
        {
            OpenPage(false);
        }

        private void BtnDelete(object sender, RoutedEventArgs e)
        {
            if (TryGetSelectedRow(out DataRowView selectedRowView))
            {
                int typeEventId = Convert.ToInt32(selectedRowView["ID_Гражданина"]);
                var relatedRows = relatedAdapter.GetData();
                bool hasRelated = false;

                foreach (var row in relatedRows)
                {
                    if (row.ID_Гражданина == typeEventId)
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
                    adapter.Update(db.ГРАЖДАНИН);
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

        // Переходит на страницу редактирования выбранной записи, если она выбрана.
        private void ListViewStatus_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            NavigatePageSelectedRow();
        }

        // Универсальный метод для выбора строки из ListView как DataRowView.
        private bool TryGetSelectedRow(out DataRowView selectedRowView)
        {
            selectedRowView = listViewCitizen.SelectedItem as DataRowView;
            return selectedRowView != null;
        }

        private void NavigatePageSelectedRow()
        {
            if (TryGetSelectedRow(out DataRowView selectedRowView))
                OpenPage(true, selectedRowView);
            else
                MessageBox.Show("Выберите строку для редактирования.");
        }

        // Открывает страницу добавления или изменения записи.
        private void OpenPage(bool isChangeOrAdd, DataRowView rowView = null)
        {
            diplom_loskutova.Page.AddOrChange.CitizensAOC page;
            if (rowView != null)
                page = new diplom_loskutova.Page.AddOrChange.CitizensAOC(rowView);
            else
                page = new diplom_loskutova.Page.AddOrChange.CitizensAOC();

            page.ChangeOrAdd = isChangeOrAdd;
            page.DataChanged += (s, ev) => LoadData();
            NavigationService.Navigate(page);
        }

        private void BtnResetFilter_Click(object sender, RoutedEventArgs e)
        {
            DatePickerSearch.Text = "";
            TextBoxSearchFIO.Text = "";             
            TextBoxSearchTelephone.Text = "";          
            ApplyFilter();                          
        }
        private void ApplyFilter()
        {
            if (db.ГРАЖДАНИН.Rows.Count == 0)
            {
                listViewCitizen.ItemsSource = null;
                return;
            }

            string filter = "";

            // Фильтр по ФИО
            var fio = TextBoxSearchFIO.Text.Trim();
            if (!string.IsNullOrEmpty(fio))
            {
                if (filter.Length > 0)
                    filter += " AND ";
                filter += $"(Фамилия LIKE '%{fio}%' OR Имя LIKE '%{fio}%' OR Отчество LIKE '%{fio}%')";
            }

            // Фильтр по ФИО
            var telephone = TextBoxSearchTelephone.Text.Trim();
            if (!string.IsNullOrEmpty(telephone))
            {
                if (filter.Length > 0)
                    filter += " AND ";
                filter += $"(Контактный_Телефон LIKE '%{telephone}%')";
            }

            // Фильтр по дате
            if (DatePickerSearch.SelectedDate.HasValue)
            {
                if (filter.Length > 0)
                    filter += " AND ";
                string selectedDateStr = DatePickerSearch.SelectedDate.Value.ToString("yyyy-MM-dd");
                filter += $"Дата_Рождения = #{selectedDateStr}#";
            }

            DataView dv = db.ГРАЖДАНИН.DefaultView;
            dv.RowFilter = filter;
            listViewCitizen.ItemsSource = dv;
        }

        private void DatePickerSearch_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void TextBoxSearchTelephone_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void TextBoxSearchFIO_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        public void photo(string fileName)
        {
            // 1. Пробуем путь проекта
            string projectPath = Path.Combine(AppContext.BaseDirectory + "\\Image-citizen", fileName);
            if (File.Exists(projectPath))
            {
                PhotoImage.Source = new BitmapImage(new Uri(projectPath, UriKind.Absolute));
            }
            // 2. Если нет - пробуем рабочий стол
            else
            {
                string desktopPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "Image-citizen",
                    fileName
                );

                if (File.Exists(desktopPath))
                {
                    PhotoImage.Source = new BitmapImage(new Uri(desktopPath, UriKind.Absolute));
                }
                else
                {
                    PhotoImage.Source = null;
                    // MessageBox.Show($"Фото не найдено:\n{projectPath}\n{desktopPath}"); // Отладка
                }
            }
        }

        private void listViewCitizen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TryGetSelectedRow(out DataRowView selectedRowView))
                photo(selectedRowView["Фото"].ToString());
        }
    }
}