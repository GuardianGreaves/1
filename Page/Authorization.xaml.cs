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
                    "Строка подключения к БД не найдена в App.config!",
                    "Обратитесь к разработчику");
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

            if (string.IsNullOrEmpty(encryptLogin) || string.IsNullOrEmpty(encryptPassword))
            {
                errorLogIn++;
                if (errorLogIn >= 3)
                {
                    var msg = new diplom_loskutova.NotificationDialog(
                        "Ошибка",
                        "Больше 3 неудачных попыток",
                        "Для продолжения введите код с картинки");
                    msg.ShowDialog();
                    NavigationService.Navigate(new diplom_loskutova.Page.Captcha());
                }
                else
                {
                    var msg = new diplom_loskutova.NotificationDialog(
                        "Ошибка",
                        "Заполните поля",
                        "Введите логин и пароль для входа в систему");
                    msg.ShowDialog();
                }
                return;
            }

            // Передаём строку подключения в DbHelper
            DbHelper db = new DbHelper(_connectionString);
            var result = db.CheckUser(encryptLogin, encryptPassword);

            if (result.count > 0)
            {
                var msg = new diplom_loskutova.NotificationDialog(
                    "Успех",
                    "Вы успешно авторизировались",
                    "");
                msg.ShowDialog();
                MainWindow newWindow = new MainWindow(result.name, result.role);
                newWindow.Show();
                Application.Current.MainWindow.Close();
            }
            else
            {
                errorLogIn++;
                if (errorLogIn > 3)
                {
                    var msg = new diplom_loskutova.NotificationDialog(
                        "Ошибка",
                        "Больше 3 неудачных попыток",
                        "Для продолжения введите код с картинки");
                    msg.ShowDialog();
                    diplom_loskutova.Page.Captcha page = new diplom_loskutova.Page.Captcha();
                    NavigationService.Navigate(page);
                }
                else
                {
                    var msg = new diplom_loskutova.NotificationDialog(
                        "Ошибка",
                        "Заполните поля",
                        "Неверный логин или пароль");
                    msg.ShowDialog();

                }
            }
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


    }
}
