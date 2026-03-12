using diplom_loskutova.Helpers;
using ScottPlot;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using MessageBox = System.Windows.Forms.MessageBox;

namespace diplom_loskutova.Page
{
    public partial class Users : System.Windows.Controls.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["diplom_loskutova.Properties.Settings.DP_2025_LoskutovaConnectionString"].ConnectionString;
        private DP_2025_LoskutovaDataSetTableAdapters.ПОЛЬЗОВАТЕЛЬTableAdapter adapter = new DP_2025_LoskutovaDataSetTableAdapters.ПОЛЬЗОВАТЕЛЬTableAdapter();
        private DP_2025_LoskutovaDataSet db = new DP_2025_LoskutovaDataSet();

        public Users(string _role)
        {
            InitializeComponent();

            LoadData();
            LoadRolesToComboBox();

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

        private DataTable GetRoleStatistics()
        {
            string sql = @"
                        SELECT 
                            r.ID_Роли,
                            r.Название,
                            ISNULL(COUNT(u.ID_Пользователя), 0) as RoleCount
                        FROM [dbo].[РОЛЬ] r 
                        LEFT JOIN [dbo].[ПОЛЬЗОВАТЕЛЬ] u ON r.ID_Роли = u.ID_Роли 
                        GROUP BY r.ID_Роли, r.Название
                        ORDER BY r.ID_Роли";

            DataTable dt = new DataTable();
            using (var adapter = new SqlDataAdapter(sql, connectionString))
            {
                adapter.Fill(dt);
            }
            return dt;
        }

        private void LoadUserStats()
        {            
            var roleStats = GetRoleStatistics();

            try
            {
                adapter.FillBy(db.ПОЛЬЗОВАТЕЛЬ);
                tbTotalUsers.Text = db.ПОЛЬЗОВАТЕЛЬ.Count.ToString();
                listViewUsers.ItemsSource = db.ПОЛЬЗОВАТЕЛЬ.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
            }
            Loaded += (s, e) =>
            {
                double[] values = roleStats.AsEnumerable()
                    .Select(row => Convert.ToDouble(row["RoleCount"]))
                    .ToArray();

                string[] labels = roleStats.AsEnumerable()
                    .Select(row => row["Название"].ToString())
                    .ToArray();

                var pie = WpfPlot1.Plot.Add.Pie(values);
                pie.ExplodeFraction = .1;
                pie.SliceLabelDistance = 0.5;

                double total = values.Sum();
                double[] percentages = values.Select(v => v / total * 100).ToArray();

                for (int i = 0; i < pie.Slices.Count; i++)
                {
                    pie.Slices[i].Label = $"{percentages[i]:0.0}%\n{labels[i]}";
                    pie.Slices[i].LabelFontSize = 16;
                    pie.Slices[i].LabelBold = true;
                    pie.Slices[i].LabelFontColor = Colors.Black.WithAlpha(.7);
                }

                pie.Radius = 1.25;
                WpfPlot1.Plot.Axes.SetLimits(-1.5, 1.5, -1.5, 1.5);
                WpfPlot1.Plot.Title("Распределение пользователей по ролям");
                WpfPlot1.Plot.ShowLegend();
                WpfPlot1.Plot.Axes.Frameless();
                WpfPlot1.Plot.HideGrid();
                WpfPlot1.Refresh();
            };
        }

        private void LoadData()
        {
            try
            {
                adapter.FillBy(db.ПОЛЬЗОВАТЕЛЬ);
                listViewUsers.ItemsSource = db.ПОЛЬЗОВАТЕЛЬ.DefaultView;
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
                var result = MessageBox.Show("Вы уверены что хотите удалить запись ?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    selectedRowView.Row.Delete();

                    try
                    {
                        adapter.Update(db.ПОЛЬЗОВАТЕЛЬ);
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка");
                    }
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
            selectedRowView = listViewUsers.SelectedItem as DataRowView;
            return selectedRowView != null;
        }

        private void OpenPage(bool isChangeOrAdd, DataRowView rowView = null)
        {
            diplom_loskutova.Page.AddOrChange.UsersAOC page;
            if (rowView != null)
            {
                page = new diplom_loskutova.Page.AddOrChange.UsersAOC(rowView);
            }
            else
            {
                page = new diplom_loskutova.Page.AddOrChange.UsersAOC();
            }

            page.ChangeOrAdd = isChangeOrAdd;
            page.DataChanged += (s, ev) => LoadData();
            NavigationService.Navigate(page);
        }

        private void LoadRolesToComboBox()
        {
            var roleAdapter = new DP_2025_LoskutovaDataSetTableAdapters.РОЛЬTableAdapter();
            ComboBoxHelper.LoadData(ComboBoxSearchRole, roleAdapter.GetData(), "Название", "ID_Роли");
        }

        private void ComboBoxSearchRole_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (db.ПОЛЬЗОВАТЕЛЬ.Rows.Count == 0)
            {
                listViewUsers.ItemsSource = null;
                return;
            }

            string filter = "";

            if (ComboBoxSearchRole.SelectedValue != null)
            {
                filter += $"ID_Роли = {ComboBoxSearchRole.SelectedValue}";
            }

            var fio = TextBoxSearchFIO.Text.Trim();
            if (!string.IsNullOrEmpty(fio))
            {
                if (filter.Length > 0)
                    filter += " AND ";
                filter += $"(Фамилия LIKE '%{fio}%' OR Имя LIKE '%{fio}%' OR Отчество LIKE '%{fio}%')";
            }

            var login = TextBoxSearchLogin.Text.Trim();
            if (!string.IsNullOrEmpty(login))
            {
                if (filter.Length > 0)
                    filter += " AND ";
                filter += $"Логин LIKE '%{login}%'";
            }

            DataView dv = db.ПОЛЬЗОВАТЕЛЬ.DefaultView;
            dv.RowFilter = filter;
            listViewUsers.ItemsSource = dv;
        }

        private void BtnResetFilter_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxSearchRole.SelectedIndex = -1;
            TextBoxSearchFIO.Text = "";             
            TextBoxSearchLogin.Text = "";           
            ApplyFilter();                         
        }

        private void TextBoxSearchFIO_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void TextBoxSearchLogin_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ApplyFilter();
        }
    }
}