using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace diplom_loskutova.Page
{
    public partial class Captcha : System.Windows.Controls.Page
    {
        string CaptchaText;
        private static int captchaErrors = 0;  // Статический счётчик для всех экземпляров

        public Captcha()
        {
            InitializeComponent();
            CapthaGenerate();
        }

        void CapthaGenerate()
        {
            diplom_lib_loskutova.Captcha.Captcha captchaGenerator = new diplom_lib_loskutova.Captcha.Captcha(120, 36);
            CaptchaText = captchaGenerator.CaptchaText;
            BitmapImage _captchaImage = captchaGenerator.GenerateCaptchaBitmapImage();
            captchaImage.Source = _captchaImage;
        }

        private void Button_Click(object sender, RoutedEventArgs e)  // Новая капча
        {
            CapthaGenerate();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)  // Проверка капчи
        {
            if (CaptchaText == textBoxCaptcha.Text.Trim())
            {
                var msg = new diplom_loskutova.NotificationDialog(
                    "Ошибка",
                    "Каптча введена верно!",
                    "Введите каптчу заново");
                msg.ShowDialog();

                captchaErrors = 0;  // Сбрасываем счётчик при успехе

                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();  // Возврат на авторизацию
                }
            }
            else
            {
                captchaErrors++;  // Увеличиваем счётчик ошибок

                // Проверяем лимит (3 ошибки = закрытие)
                if (captchaErrors >= 3)
                {
                    var msg = new diplom_loskutova.NotificationDialog(
                        "Ошибка",
                        "Блокировка доступа",
                        "Слишком много ошибок капчи! Приложение закрывается.");
                    msg.ShowDialog();

                    System.Windows.Application.Current.Shutdown();
                    return;
                }
                else
                {
                    var msg = new diplom_loskutova.NotificationDialog(
                        "Ошибка",
                        "Каптча введена неверно!",
                        $"Попыток капчи осталось: {3 - captchaErrors}");
                    msg.ShowDialog();
                }

                CapthaGenerate();  // Новая капча
                textBoxCaptcha.Clear();
                textBoxCaptcha.Focus();
            }
        }

        private void textBoxCaptcha_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
