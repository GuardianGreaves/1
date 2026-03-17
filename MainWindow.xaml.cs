using diplom_loskutova.Class;
using System;
using System.Windows;
using System.Windows.Input;

namespace diplom_loskutova
{
    public partial class MainWindow : Window
    {
        // Текущая роль пользователя
        private string role;

        // Конструктор: инициализация окна с именем и ролью пользователя
        public MainWindow(string _name, string _role)
        {
            InitializeComponent();

            nameUser.Text = _name;  // Установка имени пользователя
            role = _role;           // Сохранение роли

            RoleManager roleManager = new RoleManager();
            nameRole.Text = "(" + roleManager.GetNameById(Convert.ToInt32(_role)) + ")";  // Получение и отображение названия роли

            // Скрытие кнопок для роли "3" (гость/ограниченный доступ)
            if (_role == "3")
            {
                BtnOpenPageApplication.Visibility = Visibility.Collapsed;
                BtnOpenPageCitizen.Visibility = Visibility.Collapsed;
                BtnOpenPageUsers.Visibility = Visibility.Collapsed;
                BtnOpenPageStatus.Visibility = Visibility.Collapsed;
                BtnOpenPageTypeEvent.Visibility = Visibility.Collapsed;
                BtnOpenPageRoleUser.Visibility = Visibility.Collapsed;
                BtnOpenPageReports.Visibility = Visibility.Collapsed;
            }

            // Скрытие кнопок администрирования для роли "1" (обычный пользователь)
            if (_role == "1")
            {
                BtnOpenPageRoleUser.Visibility = Visibility.Collapsed;
                BtnOpenPageSettings.Visibility = Visibility.Collapsed;
            }
        }

        // Перетаскивание окна мышью
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        // Универсальный метод для открытия страниц в главном фрейме
        private void OpenPage(object page)
        {
            mainFrame.Navigate(page);
            mainFrame.Visibility = Visibility.Visible;  // Показываем Frame
            WelcomeText.Visibility = Visibility.Collapsed;  // Скрываем приветственный текст
        }

        // Вспомогательный метод для получения текущего окна
        private Window GetWindow() => Window.GetWindow(this);

        // Сворачивание окна
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            var window = GetWindow();
            if (window != null) window.WindowState = WindowState.Minimized;
        }

        // Развернуть/свернуть окно
        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            var window = GetWindow();
            if (window != null)
            {
                window.WindowState = window.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
            }
        }

        // Закрытие окна
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var window = GetWindow();
            if (window != null) window.Close();
        }

        // Навигаторы по страницам (передача роли пользователя)
        private void ButtonOpenPageApplication(object sender, RoutedEventArgs e) => OpenPage(new diplom_loskutova.Page.Applications(role));
        private void ButtonOpenPageEvent(object sender, RoutedEventArgs e) => OpenPage(new diplom_loskutova.Page.Events(role));
        private void ButtonOpenPageCitizen(object sender, RoutedEventArgs e) => OpenPage(new diplom_loskutova.Page.Citizens(role));
        private void ButtonOpenPageUsers(object sender, RoutedEventArgs e) => OpenPage(new diplom_loskutova.Page.Users(role));
        private void ButtonOpenPageStatus(object sender, RoutedEventArgs e) => OpenPage(new diplom_loskutova.Page.Status(role));
        private void ButtonOpenPageTypeEvent(object sender, RoutedEventArgs e) => OpenPage(new diplom_loskutova.Page.TypeEvent(role));
        private void ButtonOpenPageRoleUser(object sender, RoutedEventArgs e) => OpenPage(new diplom_loskutova.Page.RoleUser(role));

        // Отчеты (без передачи роли)
        private void ButtonOpenPageReports(object sender, RoutedEventArgs e) => OpenPage(new diplom_loskutova.Page.Reports());

        // Выход в окно авторизации
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowAuthorization newWindow = new WindowAuthorization();
            newWindow.Show();
            this.Close();
        }

        // Настройки (без передачи роли)
        private void ButtonOpenPageSettings(object sender, RoutedEventArgs e) => OpenPage(new diplom_loskutova.Page.Settings());
    }
}
