using diplom_lib_loskutova.Encryption;
using System.Configuration;  // ← добавить
using System.Data.SqlClient; // ← добавить, если нет
using System.Windows;
using System.Windows.Interop;

namespace diplom_loskutova.Page
{
    public partial class Authorization : System.Windows.Controls.Page
    {
        int errorLogIn;
        private readonly string _connectionString;

        public Authorization()
        {
            InitializeComponent();

            // Читаем строку подключения из App.config
            _connectionString = ConfigurationManager.ConnectionStrings[
                "diplom_loskutova.Properties.Settings.DP_2025_LoskutovaConnectionString"
            ]?.ConnectionString;

            if (string.IsNullOrEmpty(_connectionString))
            {
                var msg = new diplom_loskutova.NotificationDialog(
                    "Ошибка",
                    "Строка подключения к БД не найдена",
                    "Строка подключения к базе данных не найдена в App.config! Обратитесь к разработчику");
                msg.ShowDialog();

                return;
            }
        }


        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            // Используем новый класс шифрования скремблер
            ScramblerEncryptor encryptor = new ScramblerEncryptor("13371337");
            var encryptLogin = encryptor.Encrypt(TextBoxLogin.Text.Trim());
            var encryptPassword = encryptor.Encrypt(TextBoxPassword.Password.Trim());

            // Проверка пустых полей ДОНАЙТИ
            if (string.IsNullOrEmpty(encryptLogin) || string.IsNullOrEmpty(encryptPassword))
            {
                ShowError("Заполните поля", "Введите логин и пароль для входа в систему");
                return;
            }

            // Проверка пользователя в БД
            DbHelper db = new DbHelper(_connectionString);
            var result = db.CheckUser(encryptLogin, encryptPassword);

            if (result.count > 0)
            {
                // Успешный вход
                var msg = new diplom_loskutova.NotificationDialog(
                    "Успех", "Вы успешно авторизировались", "");
                msg.ShowDialog();

                MainWindow newWindow = new MainWindow(result.name, result.role);
                newWindow.Show();
                Application.Current.MainWindow.Close();
                errorLogIn = 0; // Сброс счётчика
            }
            else
            {
                errorLogIn++;

                if (errorLogIn >= 3) // ИСПРАВЛЕНО: >= 3
                {
                    var msg = new diplom_loskutova.NotificationDialog(
                        "Ошибка", "Больше 3 неудачных попыток",
                        "Для продолжения введите код с картинки");
                    msg.ShowDialog();
                    NavigationService.Navigate(new diplom_loskutova.Page.Captcha());
                }
                else
                {
                    ShowError("Неверные данные", "Неверный логин или пароль");
                }
            }
        }

        // ВЫНЕСЕННАЯ МЕТОД ДЛЯ УБИРАНИЯ ДУБЛИРОВАНИЯ
        private void ShowError(string title, string message)
        {
            errorLogIn++;
            var msg = new diplom_loskutova.NotificationDialog("Ошибка", title, message);
            msg.ShowDialog();
        }

        private void TextBoxLogin_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBlockLogin.Text) && TextBoxLogin.Text.Length > 0)
            {
                TextBlockLogin.Visibility = Visibility.Hidden;
            }
            else
            {
                TextBlockLogin.Visibility = Visibility.Visible;
            }
        }

        private void TextBoxPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBlockPassword.Text) && TextBoxPassword.Password.Length > 0)
            {
                TextBlockPassword.Visibility = Visibility.Hidden;
            }
            else
            {
                TextBlockPassword.Visibility = Visibility.Visible;
            }
        }

        private void GuestIn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow newWindow = new MainWindow("Гость", "3");
            newWindow.Show();
            Application.Current.MainWindow.Close();
        }
    }
}
