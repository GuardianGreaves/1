using diplom_loskutova.Class;
using diplom_loskutova.Page;
using System;
using System.Web.Configuration;
using System.Windows;

namespace diplom_loskutova
{
    public partial class MainWindow : Window
    {
        private string role;

        public MainWindow(string _name, string _role)
        {
            InitializeComponent();

            nameUser.Text = _name;
            role = _role;
            RoleManager roleManager = new RoleManager();
            nameRole.Text = "(" + roleManager.GetNameById(Convert.ToInt32(_role)) + ")";

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

            if (_role == "1")
            {
                BtnOpenPageRoleUser.Visibility = Visibility.Collapsed;
            }

        }

        // Универсальный метод для открытия страниц
        private void OpenPage(object page)
        {
            mainFrame.Navigate(page);
            mainFrame.Visibility = Visibility.Visible;  // Показываем Frame
            WelcomeText.Visibility = Visibility.Collapsed;  // Скрываем текст
        }

        // КНОПКИ ОКНА
        private Window GetWindow() => Window.GetWindow(this);
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            var window = GetWindow();
            if (window != null) window.WindowState = WindowState.Minimized;
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            var window = GetWindow();
            if (window != null)
            {
                window.WindowState = window.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var window = GetWindow();
            if (window != null) window.Close();
        }

        // ОТКРЫТИЕ СТРАНИЦЫ ЗАЯВКИ
        private void ButtonOpenPageApplication(object sender, RoutedEventArgs e)
        {
            OpenPage(new diplom_loskutova.Page.Applications(role));
        }

        // ОТКРЫТИЕ СТРАНИЦЫ МЕРОПРИЯТИЯ
        private void ButtonOpenPageEvent(object sender, RoutedEventArgs e)
        {
            OpenPage(new diplom_loskutova.Page.Events(role));
        }

        // ОТКРЫТИЕ СТРАНИЦЫ ГРАЖДАНЕ
        private void ButtonOpenPageCitizen(object sender, RoutedEventArgs e)
        {
            OpenPage(new diplom_loskutova.Page.Citizens(role));
        }

        // ОТКРЫТИЕ СТРАНИЦЫ ПОЛЬЗОВАТЕЛИ
        private void ButtonOpenPageUsers(object sender, RoutedEventArgs e)
        {
            OpenPage(new diplom_loskutova.Page.Users(role));
        }

        // ОТКРЫТИЕ СТРАНИЦЫ СТАТУС
        private void ButtonOpenPageStatus(object sender, RoutedEventArgs e)
        {
            OpenPage(new diplom_loskutova.Page.Status(role));
        }

        // ОТКРЫТИЕ СТРАНИЦЫ ТИП МЕРОПРИЯТИЙ
        private void ButtonOpenPageTypeEvent(object sender, RoutedEventArgs e)
        {
            OpenPage(new diplom_loskutova.Page.TypeEvent(role));
        }

        // ОТКРЫТИЕ СТРАНИЦЫ РОЛЬ ПОЛЬЗОВАТЕЛЯ
        private void ButtonOpenPageRoleUser(object sender, RoutedEventArgs e)
        {
            OpenPage(new diplom_loskutova.Page.RoleUser(role));
        }

        // ОТКРЫТИЕ СТРАНИЦЫ ОТЧЕТЫ
        private void ButtonOpenPageReports(object sender, RoutedEventArgs e)
        {
            OpenPage(new diplom_loskutova.Page.Reports());
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowAuthorization newWindow = new WindowAuthorization();
            newWindow.Show();
            this.Close();
        }

        private void ButtonOpenPageSettings(object sender, RoutedEventArgs e)
        {
            diplom_loskutova.Page.Settings page = new diplom_loskutova.Page.Settings();
            mainFrame.Navigate(page);
        }
    }
}
