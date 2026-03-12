using diplom_loskutova.Class;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using System.Windows.Navigation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace diplom_loskutova.Page.AddOrChange
{
    public partial class EventsAOC : System.Windows.Controls.Page
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
        private DP_2025_LoskutovaDataSetTableAdapters.МЕРОПРИЯТИЕTableAdapter adapter = new DP_2025_LoskutovaDataSetTableAdapters.МЕРОПРИЯТИЕTableAdapter();
        private UserManager userManager = new UserManager();
        private TypeEventsManager typeEventsManager = new TypeEventsManager();

        // Конструктор страницы с передачей строки таблицы для редактирования.
        // Если строка передана, заполняет поля формы значениями из нее.
        public EventsAOC(DataRowView row = null)
        {
            InitializeComponent();
            LoadToComboBox();
            if (row != null)
            {
                ChangeOrAdd = true;
                SetCurrentRow(row); // Заполнение формы данными строки
            }
            else
            {
                ChangeOrAdd = false;
                NamePage.Text = "Добавление записи в таблицу \"Заявки\"";
            }
        }

        // Заполнить форму по переданной строке данных
        private void SetCurrentRow(DataRowView row)
        {
            currentRow = row;
            ComboBoxUser.SelectedValue = Convert.ToInt32(currentRow["ID_Пользователя"]);
            ComboBoxTypeEvent.SelectedValue = Convert.ToInt32(currentRow["ID_Типа"]);
            TextBoxName.Text = currentRow["Название"].ToString();
            DatePickerDate.Text = currentRow["Дата_Мероприятия"].ToString();
            TextBoxBudget.Text = currentRow["Бюджет"].ToString();
            TextBoxDescription.Text = currentRow["Описание"].ToString();
            NamePage.Text = $"Редактирование записи №{Convert.ToInt32(currentRow["ID_Мероприятия"])}";
        }

        // Получить ID пользователей, статуса и события по выбранным именам, с проверкой
        private bool TryGetIds(out int userId, out int typeId)
        {
            userId = userManager.GetIdByName(ComboBoxUser.Text);
            typeId = typeEventsManager.GetIdByName(ComboBoxTypeEvent.Text);
            if (userId == -1 || typeId == -1)
            {
                MessageBox.Show("Запись не найдена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        // Проверка на дубликат записи
        private bool ExistsDuplicate(string name, string description)
        {
            var existingRecords = adapter.GetData()
                    .Where(row => row.Название == name && row.Описание == description);

            // При редактировании исключаем текущую запись по ее уникальному идентификатору, если он есть
            if (ChangeOrAdd && currentRow != null)
            {
                existingRecords = existingRecords.Where(row => row.ID_Мероприятия != Convert.ToInt32(currentRow["ID_Мероприятия"]));
            }

            return existingRecords.Any();
        }

        // Сохранение данных: обновление или добавление с проверкой и сообщениями
        private void BtnSave(object sender, RoutedEventArgs e)
        {
            int userId, typeId;

            string user = ComboBoxUser.Text;
            string type = ComboBoxTypeEvent.Text;
            string name = TextBoxName.Text;
            string description = TextBoxDescription.Text;
            decimal budget = Convert.ToDecimal(TextBoxBudget.Text);
            DateTime date = DateTime.Now;
            if (DatePickerDate.SelectedDate.HasValue)
            {
                date = DatePickerDate.SelectedDate.Value;
            }

            if (!TryGetIds(out userId, out typeId))
                return;

            if (ExistsDuplicate(name, description))
            {
                MessageBox.Show("Такая запись уже существует.", "Ошибка");
                return;
            }

            try
            {
                if (ChangeOrAdd)
                {
                    // Изменяем выбранную запись
                    currentRow["ID_Пользователя"] = userId;
                    currentRow["ID_Типа"] = typeId;
                    currentRow["Название"] = name;
                    currentRow["Описание"] = description;
                    currentRow["Дата_Мероприятия"] = date;
                    currentRow["Бюджет"] = budget;
                    // Сохраняем изменения в базе
                    adapter.Update((DP_2025_LoskutovaDataSet.МЕРОПРИЯТИЕDataTable)currentRow.DataView.Table);
                }
                else
                {
                    // Создаем новую запись в таблице
                    var newRow = db.МЕРОПРИЯТИЕ.NewМЕРОПРИЯТИЕRow();
                    newRow.ID_Пользователя = userId;
                    newRow.ID_Типа = typeId;
                    newRow.Название = name;
                    newRow.Описание = description;
                    newRow.Дата_Мероприятия = date;
                    newRow.Бюджет = budget;
                    db.МЕРОПРИЯТИЕ.Rows.Add(newRow);

                    // Сохраняем новую запись в базе
                    adapter.Update(db.МЕРОПРИЯТИЕ);
                }
                MessageBox.Show("Данные успешно сохранены", "Успех");
                DataChanged?.Invoke(this, EventArgs.Empty); // Вызываем событие, что данные изменились, чтобы загрузить их заново на предыдущей странице
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Кнопка назад, возвращает на предыдущую страницу
        private void BtnBack(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        // Загрузить данные в ComboBox, привязывая источник и названия полей
        private void LoadComboBox(System.Windows.Controls.ComboBox comboBox, System.Collections.IEnumerable dataSource, string displayMemberPath, string selectedValuePath)
        {
            comboBox.ItemsSource = dataSource;
            comboBox.DisplayMemberPath = displayMemberPath;
            comboBox.SelectedValuePath = selectedValuePath;
        }

        // Метод загрузки пользователей в ComboBox
        private void LoadToComboBox()
        {
            var userAdapter = new DP_2025_LoskutovaDataSetTableAdapters.ПОЛЬЗОВАТЕЛЬTableAdapter();
            var typeEventsAdapter = new DP_2025_LoskutovaDataSetTableAdapters.ТИП_МЕРОПРИЯТИЯTableAdapter();

            var usersTable = userAdapter.GetData();
            usersTable.Columns.Add("FullName", typeof(string), "Фамилия + ' ' + Имя + ' ' + Отчество");
            LoadComboBox(ComboBoxUser, usersTable, "FullName", "ID_Пользователя");

            LoadComboBox(ComboBoxTypeEvent, typeEventsAdapter.GetData(), "Название", "ID_Типа");
        }
    }
}
