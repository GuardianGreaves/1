using diplom_loskutova.Class;
using diplom_loskutova.Helpers;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;

namespace diplom_loskutova.Page.AddOrChange
{
    public partial class UsersAOC : System.Windows.Controls.Page
    {
        // DataSet для работы с таблицей 
        private DP_2025_LoskutovaDataSet db = new DP_2025_LoskutovaDataSet();

        // События для уведомления об изменениях данных и открытии страницы 
        public event EventHandler DataChanged;

        // Флаг для определения операции: изменение (true) или добавление (false)
        public bool ChangeOrAdd = false;

        // Представление текущей строки данных, редактируемой на форме
        private DataRowView currentRow = null;

        // Адаптер для работы с таблицей и менеджеры для получения ID по имени
        private DP_2025_LoskutovaDataSetTableAdapters.ПОЛЬЗОВАТЕЛЬTableAdapter adapter = new DP_2025_LoskutovaDataSetTableAdapters.ПОЛЬЗОВАТЕЛЬTableAdapter();
        private RoleManager roleManager = new RoleManager();

        // Конструктор страницы с передачей строки таблицы для редактирования.
        // Если строка передана, заполняет поля формы значениями из нее.
        public UsersAOC(DataRowView row = null)
        {
            InitializeComponent();
            LoadRolesToComboBox();

            if (row != null)
            {
                ChangeOrAdd = true;
                SetCurrentRow(row); // Заполнение формы данными строки
            }
            else
            {
                ChangeOrAdd = false;
                NamePage.Text = "Добавление записи в таблицу \"Пользователи\"";
            }
        }

        // Заполнить форму по переданной строке данных
        private void SetCurrentRow(DataRowView row)
        {
            currentRow = row;
            TextBoxName.Text = currentRow["Имя"].ToString();
            TextBoxSurname.Text = currentRow["Фамилия"].ToString();
            TextBoxPatronymic.Text = currentRow["Отчество"].ToString();
            TextBoxLogin.Text = currentRow["Логин"].ToString();
            TextBoxPassword.Text = currentRow["Пароль"].ToString();
            ComboBoxRole.SelectedValue = Convert.ToInt32(currentRow["ID_Роли"]);
            NamePage.Text = $"Редактирование записи №{Convert.ToInt32(currentRow["ID_Пользователя"])}";
        }

        // Получить ID пользователей, статуса и события по выбранным именам, с проверкой
        private bool TryGetIds(out int roleId)
        {
            roleId = roleManager.GetIdByName(ComboBoxRole.Text);
            if (roleId == -1)
            {
                MessageBox.Show("Запись не найдена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        // Проверка на дубликат записи
        private bool ExistsDuplicate(string login)
        {
            var existingRecords = adapter.GetData()
                    .Where(row => row.Логин == login);

            // При редактировании исключаем текущую запись по ее уникальному идентификатору, если он есть
            if (ChangeOrAdd && currentRow != null)
            {
                existingRecords = existingRecords.Where(row => row.ID_Пользователя != Convert.ToInt32(currentRow["ID_Пользователя"]));
            }

            return existingRecords.Any();
        }

        /// Обработчик кнопки сохранения.
        /// При изменении обновляет текущую строку DataRowView,
        /// При добавлении создает новую строку и сохраняет изменения.
        private void BtnSave(object sender, RoutedEventArgs e)
        {
            int roleId;
            string role = ComboBoxRole.Text;
            string name = TextBoxName.Text;
            string surname = TextBoxSurname.Text;
            string patronymic = TextBoxPatronymic.Text;
            string login = TextBoxLogin.Text;
            string password = TextBoxPassword.Text;

            if (!TryGetIds(out roleId))
                return;

            if (ExistsDuplicate(login))
            {
                MessageBox.Show("Запись с таким логином уже существует.", "Ошибка");
                return;
            }

            try
            {
                if (ChangeOrAdd)
                {
                    // Изменяем выбранную запись
                    currentRow["ID_Роли"] = roleId;
                    currentRow["Имя"] = name;
                    currentRow["Фамилия"] = surname;
                    currentRow["Отчество"] = patronymic;
                    currentRow["Логин"] = login;
                    currentRow["Пароль"] = password;
                    // Сохраняем изменения в базе
                    adapter.Update((DP_2025_LoskutovaDataSet.ПОЛЬЗОВАТЕЛЬDataTable)currentRow.DataView.Table);
                }
                else
                {
                    // Создаем новую запись в таблице
                    var newRow = db.ПОЛЬЗОВАТЕЛЬ.NewПОЛЬЗОВАТЕЛЬRow();
                    newRow.ID_Роли = roleId;
                    newRow.Имя = name;
                    newRow.Фамилия = surname;
                    newRow.Отчество = patronymic;
                    newRow.Логин = login;
                    newRow.Пароль = password;
                    db.ПОЛЬЗОВАТЕЛЬ.Rows.Add(newRow);

                    // Сохраняем новую запись в базе
                    adapter.Update(db.ПОЛЬЗОВАТЕЛЬ);
                }
                MessageBox.Show("Данные успешно сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DataChanged?.Invoke(this, EventArgs.Empty); // Вызываем событие, что данные изменились, чтобы загрузить их заново на предыдущей странице
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик кнопки возврата назад.
        // Возвращает пользователя на предыдущую страницу навигации.
        private void BtnBack(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }


        // Загрузить данные в соответствующие ComboBox
        private void LoadRolesToComboBox()
        {
            var roleAdapter = new DP_2025_LoskutovaDataSetTableAdapters.РОЛЬTableAdapter();
            ComboBoxHelper.LoadData(ComboBoxRole, roleAdapter.GetData(), "Название", "ID_Роли");
        }
    }
}