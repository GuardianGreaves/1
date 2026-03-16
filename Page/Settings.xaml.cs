using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace diplom_loskutova.Page
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : System.Windows.Controls.Page
    {
        public Settings()
        {
            InitializeComponent();
            LoadConnectionSettings();
        }

        private void LoadConnectionSettings()
        {
            var cs = ConfigurationManager.ConnectionStrings["diplom_loskutova.Properties.Settings.DP_2025_LoskutovaConnectionString"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
                return;

            var builder = new SqlConnectionStringBuilder(cs);

            DataSourceTextBox.Text = builder.DataSource;
            InitialCatalogTextBox.Text = builder.InitialCatalog;
            if (!builder.IntegratedSecurity)
            {
                UserTextBox.Text = builder.UserID;
                PasswordBox.Password = builder.Password;
            }
        }

        private void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = DataSourceTextBox.Text,
                InitialCatalog = InitialCatalogTextBox.Text,
                UserID = UserTextBox.Text,
                Password = PasswordBox.Password,
                IntegratedSecurity = false
            };

            using (var conn = new SqlConnection(builder.ToString()))
            {
                try
                {
                    conn.Open();
                    MessageBox.Show("Подключение успешно.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Ошибка подключения:\n" + ex.Message, "Проверка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = DataSourceTextBox.Text,
                InitialCatalog = InitialCatalogTextBox.Text,
                UserID = UserTextBox.Text,
                Password = PasswordBox.Password,
                IntegratedSecurity = false
            };

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var cs = config.ConnectionStrings.ConnectionStrings["diplom_loskutova.Properties.Settings.DP_2025_LoskutovaConnectionString"];
            if (cs == null)
            {
                cs = new ConnectionStringSettings("diplom_loskutova.Properties.Settings.DP_2025_LoskutovaConnectionString", builder.ToString(), "System.Data.SqlClient");
                config.ConnectionStrings.ConnectionStrings.Add(cs);
            }
            else
            {
                cs.ConnectionString = builder.ToString();
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("connectionStrings");

            MessageBox.Show("Строка подключения сохранена. Перезапустите приложение.", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void InstructionsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Путь к PDF файлу инструкций (положи инструкцию.pdf в папку с exe)
                string pdfPath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "1.pdf"
                );

                if (System.IO.File.Exists(pdfPath))
                {
                    // Открываем PDF в программе по умолчанию (Adobe Reader, браузер и т.д.)
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = pdfPath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    // Если файла нет - показываем сообщение
                    MessageBox.Show(
                        "Файл инструкции не найден!\n\n" +
                        "Ожидаемый путь: " + pdfPath + "\n\n" +
                        "Поместите файл 'инструкция.pdf' в папку с программой.",
                        "Инструкция",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось открыть инструкцию:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BtnBack(object sender, RoutedEventArgs e)
        {

        }

        private void TechnicalTaskButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UserManualButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OperatorManualButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}