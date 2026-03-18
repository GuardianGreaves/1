using diplom_lib_loskutova.Export;
using ScottPlot;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace diplom_loskutova
{
    public partial class UserControlReport : System.Windows.Controls.UserControl
    {
        private readonly string queryReport1 = @"
SELECT 
        г.Фамилия + ' ' + г.Имя AS Гражданин,
        COUNT(z.ID_Заявки) AS [Количество заявок],
        CASE 
            WHEN MAX(z.Дата_Создания) IS NULL THEN N'Никогда'
            WHEN DATEDIFF(MONTH, MAX(z.Дата_Создания), GETDATE()) = 0 THEN N'Текущий месяц'
            WHEN DATEDIFF(MONTH, MAX(z.Дата_Создания), GETDATE()) = 1 THEN N'1 месяц назад'
            WHEN DATEDIFF(MONTH, MAX(z.Дата_Создания), GETDATE()) <= 12 THEN 
                CAST(DATEDIFF(MONTH, MAX(z.Дата_Создания), GETDATE()) AS NVARCHAR(10)) + N' мес. назад'
            ELSE 
                CAST(DATEDIFF(YEAR, MAX(z.Дата_Создания), GETDATE()) AS NVARCHAR(10)) + N' лет назад'
        END AS [С последней заявки]
    FROM dbo.ГРАЖДАНИН г
    LEFT JOIN dbo.ЗАЯВКА z ON г.ID_Гражданина = z.ID_Гражданина
    GROUP BY г.ID_Гражданина, г.Фамилия, г.Имя
    ORDER BY [Количество заявок] DESC;";

        private readonly string queryReport2 = @"
        SELECT 
            м.[Название] AS Название_Мероприятия,
            г.[Фамилия] + ' ' + г.[Имя] + ' ' + г.[Отчество] AS ФИО_Гражданина
        FROM [ФИКСАЦИЯ_ЯВКИ] ф
        LEFT JOIN [МЕРОПРИЯТИЕ] м ON ф.[ID_Мероприятия] = м.[ID_Мероприятия]
        LEFT JOIN [ГРАЖДАНИН] г ON ф.[ID_Гражданина] = г.[ID_Гражданина]";

        private readonly string queryReport3 = @"
        SELECT 
            m.Название,
            t.Название AS [Тип мероприятия],
            p.Имя + ' ' + p.Фамилия AS Пользователь,
            m.Описание,
            m.Дата_Мероприятия AS Дата,
            m.Бюджет
        FROM dbo.МЕРОПРИЯТИЕ m
        LEFT JOIN dbo.ТИП_МЕРОПРИЯТИЯ t ON m.ID_Типа = t.ID_Типа
        LEFT JOIN dbo.ПОЛЬЗОВАТЕЛЬ p ON m.ID_Пользователя = p.ID_Пользователя
        WHERE m.Дата_Мероприятия >= CAST(GETDATE() AS DATE)
        ORDER BY m.Дата_Мероприятия";

        private readonly string queryReport3_2 = @"
        SELECT 
            m.Название,
            t.Название AS [Тип мероприятия],
            p.Имя + ' ' + p.Фамилия AS Пользователь,
            m.Описание,
            m.Дата_Мероприятия AS Дата,
            m.Бюджет
        FROM dbo.МЕРОПРИЯТИЕ m
        LEFT JOIN dbo.ТИП_МЕРОПРИЯТИЯ t ON m.ID_Типа = t.ID_Типа
        LEFT JOIN dbo.ПОЛЬЗОВАТЕЛЬ p ON m.ID_Пользователя = p.ID_Пользователя
        WHERE m.Дата_Мероприятия >= @ДатаНачала
        ORDER BY m.Дата_Мероприятия";


        private readonly string queryReport4 = @"
        SELECT 
            С.Название AS Статус,
            CAST(SUM(CASE WHEN З.ID_Статуса = С.ID_Статуса THEN 1 ELSE 0 END) AS FLOAT) / 
            (SELECT COUNT(*) FROM ЗАЯВКА) AS ДоляЗаявок
        FROM СТАТУС С
        LEFT JOIN ЗАЯВКА З ON З.ID_Статуса = С.ID_Статуса
        GROUP BY С.Название";

        private readonly string queryReport5 = @"
        SELECT 
            СТАТУС.Название,
            ROUND(CAST(COUNT(ЗАЯВКА.ID_Заявки) AS FLOAT) / (SELECT COUNT(*) FROM [ЗАЯВКА]) * 100, 2) AS Процент
        FROM [ЗАЯВКА]
        JOIN [СТАТУС] ON ЗАЯВКА.ID_Статуса = СТАТУС.ID_Статуса
        GROUP BY СТАТУС.Название
        ORDER BY Процент DESC";

        private readonly string queryReport6 = @"
        SELECT 
            ГРАЖДАНИН.Фамилия + ' ' + ГРАЖДАНИН.Имя + ' ' + ГРАЖДАНИН.Отчество AS ФИО,
            COUNT(*) AS Всего_заявок,
            SUM(CASE WHEN ЗАЯВКА.ID_Статуса = 4 THEN 1 ELSE 0 END) AS Выполнено_услуг,
            ROUND(
                CAST(SUM(CASE WHEN ЗАЯВКА.ID_Статуса = 4 THEN 1 ELSE 0 END) AS FLOAT) / COUNT(*) * 100, 2
            ) AS Доля_выполненных_услуг_в_процентах
        FROM [ЗАЯВКА]
        JOIN [ГРАЖДАНИН] ON ЗАЯВКА.ID_Гражданина = ГРАЖДАНИН.ID_Гражданина
        GROUP BY ГРАЖДАНИН.Фамилия, ГРАЖДАНИН.Имя, ГРАЖДАНИН.Отчество
        ORDER BY Доля_выполненных_услуг_в_процентах DESC";

        private readonly string queryReport7 = @"
        SELECT 
            p.Имя + ' ' + p.Фамилия + ' ' + ISNULL(p.Отчество, '') AS ФИО_Сотрудника,
            m.Название AS Название_Мероприятия,
            t.Название AS Тип_Мероприятия,
            m.Дата_Мероприятия,
            m.Бюджет
        FROM МЕРОПРИЯТИЕ m
        JOIN ПОЛЬЗОВАТЕЛЬ p ON m.ID_Пользователя = p.ID_Пользователя
        JOIN РОЛЬ r ON p.ID_Роли = r.ID_Роли
        JOIN ТИП_МЕРОПРИЯТИЯ t ON m.ID_Типа = t.ID_Типа;
        ";

        private readonly string queryReport8 = @"
        USE DP_2025_Loskutova
        SELECT TOP 10
            ROW_NUMBER() OVER (ORDER BY cnt DESC) AS Место,
            Название,
            Дата_Мероприятия,
            Организатор,
            КоличествоУчастников
        FROM (
            SELECT 
                m.Название,
                m.Дата_Мероприятия,
                p.Имя + ' ' + p.Фамилия AS Организатор,
                COUNT(f.ID_Гражданина) AS КоличествоУчастников,
                COUNT(f.ID_Гражданина) as cnt
            FROM МЕРОПРИЯТИЕ m
            JOIN ПОЛЬЗОВАТЕЛЬ p ON m.ID_Пользователя = p.ID_Пользователя
            LEFT JOIN ФИКСАЦИЯ_ЯВКИ f ON m.ID_Мероприятия = f.ID_Мероприятия
            GROUP BY m.ID_Мероприятия, m.Название, m.Дата_Мероприятия, p.Имя, p.Фамилия
        ) AS Рейтинг
        ORDER BY cnt DESC, Название;
        ";


        private readonly string queryReport9 = @"
            USE DP_2025_Loskutova;
            WITH БюджетПоТипам AS (
                SELECT 
                    t.Название AS Тип_Мероприятия,
                    t.Описание AS Описание_Типа,
                    COUNT(m.ID_Мероприятия) AS Количество_Мероприятий,
                    ISNULL(SUM(m.Бюджет), 0) AS Общий_Бюджет,
                    AVG(ISNULL(m.Бюджет, 0)) AS Средний_Бюджет,
                    MIN(ISNULL(m.Бюджет, 0)) AS Мин_Бюджет,
                    MAX(ISNULL(m.Бюджет, 0)) AS Макс_Бюджет
                FROM ТИП_МЕРОПРИЯТИЯ t
                LEFT JOIN МЕРОПРИЯТИЕ m ON t.ID_Типа = m.ID_Типа
                GROUP BY t.ID_Типа, t.Название, t.Описание
            )
            SELECT 
                ROW_NUMBER() OVER (ORDER BY Общий_Бюджет DESC) AS Рейтинг,
                Тип_Мероприятия,
                Описание_Типа,
                Количество_Мероприятий,
                Общий_Бюджет,
                CAST(Средний_Бюджет AS DECIMAL(10,2)) AS Средний_Бюджет,
                Мин_Бюджет,
                Макс_Бюджет,
                CAST((Общий_Бюджет * 100.0 / NULLIF(SUM(Общий_Бюджет) OVER(), 0)) AS DECIMAL(5,2)) AS Доля_в_бюджете_проц
            FROM БюджетПоТипам
            ORDER BY Общий_Бюджет DESC;";

        private readonly string queryReport10 = @"
                SELECT 
                m.Название AS Мероприятие,
                m.Дата_Мероприятия,
                ISNULL(COUNT(z.ID_Гражданина), 0) AS Заявки,
                ISNULL(COUNT(f.ID_Гражданина), 0) AS Явилось,
                ISNULL(COUNT(z.ID_Гражданина), 0) - ISNULL(COUNT(f.ID_Гражданина), 0) AS Не_явилось,
                CAST(
                    CASE 
                        WHEN COUNT(z.ID_Гражданина) = 0 THEN 0 
                        ELSE COUNT(f.ID_Гражданина) * 100.0 / COUNT(z.ID_Гражданина) 
                    END AS DECIMAL(5,1)
                ) AS Процент_явки
            FROM МЕРОПРИЯТИЕ m
            LEFT JOIN ЗАЯВКА z ON m.ID_Мероприятия = z.ID_Мероприятия
            LEFT JOIN ФИКСАЦИЯ_ЯВКИ f ON m.ID_Мероприятия = f.ID_Мероприятия
            GROUP BY m.ID_Мероприятия, m.Название, m.Дата_Мероприятия
            ORDER BY Заявки DESC, Процент_явки DESC;";

        private readonly string queryReport11 = @"
                        SELECT 
                YEAR(z.Дата_Создания) AS Год,
                MONTH(z.Дата_Создания) AS Месяц,
                DATENAME(MONTH, z.Дата_Создания) + ' ' + CAST(YEAR(z.Дата_Создания) AS VARCHAR(4)) AS Период,
                COUNT(*) AS Количество_заявок
            FROM ЗАЯВКА z
            WHERE z.Дата_Создания IS NOT NULL
            GROUP BY YEAR(z.Дата_Создания), MONTH(z.Дата_Создания), DATENAME(MONTH, z.Дата_Создания)
            ORDER BY Год DESC, Месяц DESC;";

        private readonly string queryReport12 = @"
                        SELECT TOP 10
            g.ID_Гражданина,
            g.Фамилия + ' ' + g.Имя + ISNULL(' ' + g.Отчество, '') AS ФИО,
            COUNT(f.ID_Гражданина) AS Количество_Посещений,
            STRING_AGG(m.Название, ', ') AS Мероприятия
        FROM [dbo].[ГРАЖДАНИН] g
        INNER JOIN [dbo].[ФИКСАЦИЯ_ЯВКИ] f ON g.ID_Гражданина = f.ID_Гражданина
        INNER JOIN [dbo].[МЕРОПРИЯТИЕ] m ON f.ID_Мероприятия = m.ID_Мероприятия
        GROUP BY g.ID_Гражданина, g.Фамилия, g.Имя, g.Отчество
        ORDER BY Количество_Посещений DESC;";

        private readonly string queryReport13 = @"
                        SELECT
    p.ID_Пользователя,
    p.Фамилия + ' ' + p.Имя + ISNULL(' ' + p.Отчество, '') AS ФИО,
    COUNT(m.ID_Мероприятия) AS Количество_Мероприятий
FROM dbo.ПОЛЬЗОВАТЕЛЬ p
LEFT JOIN dbo.МЕРОПРИЯТИЕ m
    ON m.ID_Пользователя = p.ID_Пользователя
GROUP BY
    p.ID_Пользователя,
    p.Фамилия,
    p.Имя,
    p.Отчество
ORDER BY
    Количество_Мероприятий DESC;
";

        DataTable info;
        private string currentSqlQuery;
        public UserControlReport(int numberReport)
        {
            InitializeComponent();
            InitializeReport(numberReport);
            LoadEvents();
        }

        private void LoadEvents()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["diplom_loskutova.Properties.Settings.DP_2025_LoskutovaConnectionString"].ConnectionString))
                {
                    string query = @"
                        SELECT 
                            m.ID_Мероприятия,
                            m.Название,
                            m.Дата_Мероприятия,
                            p.Имя + ' ' + p.Фамилия AS Организатор,
                            t.Название AS Тип
                        FROM МЕРОПРИЯТИЕ m
                        JOIN ПОЛЬЗОВАТЕЛЬ p ON m.ID_Пользователя = p.ID_Пользователя
                        JOIN ТИП_МЕРОПРИЯТИЯ t ON m.ID_Типа = t.ID_Типа
                        ORDER BY m.Дата_Мероприятия DESC";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable eventsTable = new DataTable();
                    adapter.Fill(eventsTable);

                    // Создаем колонку для отображения в ComboBox
                    eventsTable.Columns.Add("DisplayText", typeof(string));
                    foreach (DataRow row in eventsTable.Rows)
                    {
                        row["DisplayText"] = $"{row["Название"]} ({row["Организатор"]}, {((DateTime)row["Дата_Мероприятия"]).ToString("dd.MM")})";
                    }

                    cbEvents.ItemsSource = eventsTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки мероприятий: {ex.Message}");
            }
        }

        private void cbEvents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbEvents.SelectedValue != null)
            {
                int eventId = (int)cbEvents.SelectedValue;
                LoadParticipants(eventId);
            }
        }

        private void LoadParticipants(int eventId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["diplom_loskutova.Properties.Settings.DP_2025_LoskutovaConnectionString"].ConnectionString))
                {
                    string query = @"
                SELECT 
                    g.Имя + ' ' + g.Фамилия + ISNULL(' ' + g.Отчество, '') AS ФИО,
                    g.Контактный_Телефон AS Телефон
                FROM ГРАЖДАНИН g
                JOIN ФИКСАЦИЯ_ЯВКИ f ON g.ID_Гражданина = f.ID_Гражданина
                WHERE f.ID_Мероприятия = @EventId
                ORDER BY g.Фамилия, g.Имя";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@EventId", eventId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable participantsTable = new DataTable();
                    adapter.Fill(participantsTable);

                    dataGridReport.ItemsSource = participantsTable.DefaultView;
                    UpdateEventInfo(eventId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки участников: {ex.Message}");
            }
        }

        private void UpdateEventInfo(int eventId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["diplom_loskutova.Properties.Settings.DP_2025_LoskutovaConnectionString"].ConnectionString))
                {
                    string query = @"
                        SELECT 
                            m.Название,
                            m.Дата_Мероприятия,
                            p.Имя + ' ' + p.Фамилия AS Организатор
                        FROM МЕРОПРИЯТИЕ m
                        JOIN ПОЛЬЗОВАТЕЛЬ p ON m.ID_Пользователя = p.ID_Пользователя
                        WHERE m.ID_Мероприятия = @EventId";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@EventId", eventId);
                    conn.Open();
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void LoadDataGrid(DateTime датаНачала)
        {
            string query = queryReport3_2;

            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["diplom_loskutova.Properties.Settings.DP_2025_LoskutovaConnectionString"].ConnectionString); // Полная типизация
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ДатаНачала", датаНачала.Date);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            conn.Open();
            adapter.Fill(dt);
            conn.Close();

            dataGridReport.ItemsSource = dt.DefaultView;
        }

        string namereport;
        private void InitializeReport(int _numberReport)
        {
            DbHelper dbHelper = new DbHelper();
            DataTable dt;

            switch (_numberReport)
            {
                case 1:
                    NameReport.Text = "Уровень активности граждан";
                    currentSqlQuery = queryReport1;
                    break;
                case 2:
                    NameReport.Text = "Реестр участников мероприятий";
                    filterEvent.Visibility = Visibility.Visible;
                    break;
                case 3:
                    currentSqlQuery = queryReport3;
                    NameReport.Text = "Список текущих мероприятий";
                    filter.Visibility = Visibility.Visible;
                    break;
                case 4:
                    NameReport.Text = "Статистика удовлетворенности заявок";
                    currentSqlQuery = queryReport4;
                    break;
                case 5:
                    NameReport.Text = "Анализ персональных заявок";
                    currentSqlQuery = queryReport5;
                    break;
                case 6:
                    NameReport.Text = "Статистика предоставленных услуг";
                    currentSqlQuery = queryReport6;
                    break;
                case 7:
                    NameReport.Text = "Статистика предоставленных услуг конкретным сотрудником";
                    currentSqlQuery = queryReport7;
                    break;
                case 8:
                    NameReport.Text = "Рейтинг мероприятий по популярности";
                    currentSqlQuery = queryReport8;
                    break;
                case 9:
                    NameReport.Text = "Бюджетный отчет по мероприятиям";

                    chartColumn.Width = new GridLength(350);
                    BuildBudgetByEventTypeChart();
                    currentSqlQuery = queryReport9;
                    break;
                case 10:
                    NameReport.Text = "Эффективность мероприятий";
                    currentSqlQuery = queryReport10;
                    break;
                case 11:
                    NameReport.Text = "Динамика подачи заявок";
                    currentSqlQuery = queryReport11;
                    break;
                case 12:
                    NameReport.Text = "Топ активных участников";
                    currentSqlQuery = queryReport12;
                    break;
                case 13:
                    NameReport.Text = "Нагрузка на социальных работников";
                    currentSqlQuery = queryReport13;
                    break;
                default:
                    currentSqlQuery = "";
                    filter.Visibility = Visibility.Collapsed;
                    break;
            }

            if (!string.IsNullOrEmpty(currentSqlQuery))
            {
                dt = dbHelper.ExecuteQuery(currentSqlQuery);
                info = dt;
                FillListViewWPF(dataGridReport, dt);
            }
        }
        private string connectionString = ConfigurationManager.ConnectionStrings["diplom_loskutova.Properties.Settings.DP_2025_LoskutovaConnectionString"].ConnectionString;

        private void BuildBudgetByEventTypeChart()
        {
            var budgetStats = GetBudgetByEventTypeStatistics();

            if (budgetStats.Rows.Count == 0)
                return;

            WpfPlot1.Plot.Clear();

            double[] values = budgetStats.AsEnumerable()
                .Select(row => Convert.ToDouble(row["Общий_Бюджет"]))
                .ToArray();

            string[] labels = budgetStats.AsEnumerable()
                .Select(row => row["Тип_Мероприятия"].ToString())
                .ToArray();

            double[] positions = Enumerable.Range(1, values.Length).Select(x => (double)x).ToArray();

            for (int i = 0; i < values.Length; i++)
            {
                double[] xs = { positions[i] };
                double[] ys = { values[i] };

                var bar = WpfPlot1.Plot.Add.Bars(xs, ys);
                bar.LegendText = labels[i];
            }

            WpfPlot1.Plot.Title("Бюджет по типам мероприятий");
            WpfPlot1.Plot.ShowLegend(Alignment.UpperRight);
            WpfPlot1.Plot.Axes.Margins(bottom: 0.1);
            WpfPlot1.Plot.Axes.SetLimitsY(0, values.Max() * 1.1);

            WpfPlot1.Plot.Axes.Bottom.Label.Text = "Типы мероприятий";
            WpfPlot1.Plot.Axes.Left.Label.Text = "Общий бюджет";

            WpfPlot1.Refresh();
        }

        private DataTable GetBudgetByEventTypeStatistics()
        {
            DataTable dt = new DataTable();
            using (var adapter = new SqlDataAdapter(queryReport9, connectionString))
            {
                adapter.Fill(dt);
            }
            return dt;
        }


        public void FillListViewWPF(System.Windows.Controls.DataGrid listView, DataTable dt)
        {
            listView.ItemsSource = dt.DefaultView;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Excel файлы (*.xlsx)|*.xlsx|Word файлы (*.docx)|*.docx",
                FileName = $"Отчет_{DateTime.Now:ddMMyyyy_HHmmss}.docx",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (saveDialog.ShowDialog() == true)
            {
                string filePath = saveDialog.FileName;

                if (Path.GetExtension(filePath).ToLower() == ".docx")
                    new ExportWord().ExportDataTableToWord(info, filePath, "Отчет");
                else
                    new ExportExcel().ExportDataTableToExcel(info, filePath, "Отчет");
            }
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Excel файлы (*.xlsx)|*.xlsx|Word файлы (*.docx)|*.docx",
                FileName = $"Отчет_{DateTime.Now:ddMMyyyy_HHmmss}.xlsx",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (saveDialog.ShowDialog() == true)
            {
                string filePath = saveDialog.FileName;

                if (Path.GetExtension(filePath).ToLower() == ".docx")
                    new ExportWord().ExportDataTableToWord(info, filePath, "Отчет");
                else
                    new ExportExcel().ExportDataTableToExcel(info, filePath, "Отчет");
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDlg = new PrintDialog();
            if (printDlg.ShowDialog() == true)
            {
                // Создаем FlowDocument для форматированной таблицы
                FlowDocument doc = new FlowDocument();
                Table table = new Table();
                table.CellSpacing = 0;
                table.Margin = new Thickness(10); // Отступы таблицы внутри полей

                // Настройка документа под страницу
                doc.PagePadding = new Thickness(20);
                double pageWidth = printDlg.PrintableAreaWidth -
                                  doc.PagePadding.Left - doc.PagePadding.Right -
                                  table.Margin.Left - table.Margin.Right;
                doc.ColumnWidth = pageWidth;

                // Заголовки столбцов
                TableRowGroup header = new TableRowGroup();
                TableRow headerRow = new TableRow();
                foreach (var col in dataGridReport.Columns)
                {
                    // Пропорциональная ширина колонок от DataGrid
                    double colWidth = col.ActualWidth / dataGridReport.ActualWidth;
                    table.Columns.Add(new TableColumn()
                    {
                        Width = new GridLength(colWidth, GridUnitType.Star)
                    });

                    TableCell cell = new TableCell(new Paragraph(new Run(col.Header.ToString())))
                    {
                        Background = System.Windows.Media.Brushes.LightGray,
                        FontWeight = FontWeights.Bold,
                        BorderBrush = System.Windows.Media.Brushes.Black,
                        BorderThickness = new Thickness(0.5),
                        Padding = new Thickness(3),
                        FontSize = 10,
                        TextAlignment = TextAlignment.Center
                    };
                    headerRow.Cells.Add(cell);
                }
                header.Rows.Add(headerRow);
                table.RowGroups.Add(header);

                // Данные из DataGrid
                TableRowGroup body = new TableRowGroup();
                foreach (var item in dataGridReport.Items)
                {
                    if (item is DataRowView rowView)
                    {
                        TableRow dataRow = new TableRow();
                        for (int colIndex = 0; colIndex < dataGridReport.Columns.Count; colIndex++)
                        {
                            string cellValue = rowView.Row.ItemArray[colIndex]?.ToString() ?? "";
                            TableCell cell = new TableCell(new Paragraph(new Run(cellValue)))
                            {
                                BorderBrush = System.Windows.Media.Brushes.Black,
                                BorderThickness = new Thickness(0.5),
                                Padding = new Thickness(2),
                                FontSize = 9,
                                TextAlignment = TextAlignment.Left
                            };
                            dataRow.Cells.Add(cell);
                        }
                        body.Rows.Add(dataRow);
                    }
                }
                table.RowGroups.Add(body);
                doc.Blocks.Add(table);

                // Печать документа
                IDocumentPaginatorSource paginator = doc;
                printDlg.PrintDocument(paginator.DocumentPaginator, "Данные из DataGrid");
            }
        }

        private void filterDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadDataGrid(filterDate.SelectedDate ?? DateTime.Now.Date);
        }

        private void dataGridReport_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}