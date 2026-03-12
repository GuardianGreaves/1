using diplom_loskutova.Class;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;

namespace diplom_loskutova.Page.AddOrChange
{
    public partial class ApplicationsAOC : System.Windows.Controls.Page
    {
        // Основной DataSet для работы с данными
        private DP_2025_LoskutovaDataSet db = new DP_2025_LoskutovaDataSet();

        // События для оповещения об изменении данных и открытии страницы
        public event EventHandler DataChanged;

        // Флаг: true - редактируем запись, false - добавляем новую
        public bool ChangeOrAdd = false;

        // Текущая редактируемая строка данных
        private DataRowView currentRow = null;

        // Адаптер для работы с таблицей и менеджеры для получения ID по имени
        private DP_2025_LoskutovaDataSetTableAdapters.ЗАЯВКАTableAdapter adapter = new DP_2025_LoskutovaDataSetTableAdapters.ЗАЯВКАTableAdapter();
        private CitizenManager userManager = new CitizenManager();
        private StatusManager statusManager = new StatusManager();
        private Class.EventsManager eventManager = new Class.EventsManager();

        // Конструктор страницы с параметром для редактируемой строки (если null - добавляем новую)
        public ApplicationsAOC(DataRowView row = null)
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
            ComboBoxUser.SelectedValue = Convert.ToInt32(currentRow["ID_Гражданина"]);
            ComboBoxStatus.SelectedValue = Convert.ToInt32(currentRow["ID_Статуса"]);
            ComboBoxEvents.SelectedValue = Convert.ToInt32(currentRow["ID_Мероприятия"]);
            NamePage.Text = $"Редактирование записи №{Convert.ToInt32(currentRow["ID_Заявки"])}";
        }

        // Получить ID пользователей, статуса и события по выбранным именам, с проверкой
        private bool TryGetIds(out int userId, out int statusId, out int eventId)
        {
            userId = userManager.GetIdByName(ComboBoxUser.Text);
            statusId = statusManager.GetIdByName(ComboBoxStatus.Text);
            eventId = eventManager.GetIdByName(ComboBoxEvents.Text);
            if (userId == -1 || statusId == -1 || eventId == -1)
            {
                MessageBox.Show("Запись не найдена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        // Проверка на дубликат записи
        private bool ExistsDuplicate(int userId, int statusId, int eventId)
        {
            var existingRecords = adapter.GetData()
                    .Where(row => row.ID_Гражданина == userId && row.ID_Статуса == statusId && row.ID_Мероприятия == eventId);

            // При редактировании исключаем текущую запись по ее уникальному идентификатору, если он есть
            if (ChangeOrAdd && currentRow != null)
            {
                existingRecords = existingRecords.Where(row => row.ID_Заявки != Convert.ToInt32(currentRow["ID_Заявки"]));
            }

            return existingRecords.Any();
        }

        // Сохранение данных: обновление или добавление с проверкой и сообщениями
        private void BtnSave(object sender, RoutedEventArgs e)
        {
            int userId, statusId, eventId;
            if (!TryGetIds(out userId, out statusId, out eventId))
                return;

            if (ExistsDuplicate(userId, statusId, eventId))
            {
                MessageBox.Show("Такая запись уже существует.", "Ошибка");
                return;
            }

            try
            {
                if (ChangeOrAdd)
                {
                    currentRow["ID_Гражданина"] = userId;
                    currentRow["ID_Статуса"] = statusId;
                    currentRow["ID_Мероприятия"] = eventId;
                    adapter.Update((DP_2025_LoskutovaDataSet.ЗАЯВКАDataTable)currentRow.DataView.Table);
                }
                else
                {
                    var newRow = db.ЗАЯВКА.NewЗАЯВКАRow();
                    newRow.ID_Гражданина = userId;
                    newRow.ID_Статуса = statusId;
                    newRow.ID_Мероприятия = eventId;
                    db.ЗАЯВКА.Rows.Add(newRow);
                    adapter.Update(db.ЗАЯВКА);
                }
                MessageBox.Show("Данные успешно сохранены", "Успех");
                DataChanged?.Invoke(this, EventArgs.Empty); // Вызываем событие, что данные изменились, чтобы загрузить их заново на предыдущей странице
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
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

        // Загрузить данные в соответствующие ComboBox
        private void LoadToComboBox()
        {
            var userAdapter = new DP_2025_LoskutovaDataSetTableAdapters.ГРАЖДАНИНTableAdapter();
            var statusAdapter = new DP_2025_LoskutovaDataSetTableAdapters.СТАТУСTableAdapter();
            var eventAdapter = new DP_2025_LoskutovaDataSetTableAdapters.МЕРОПРИЯТИЕTableAdapter();

            var usersTable = userAdapter.GetData();
            usersTable.Columns.Add("FullName", typeof(string), "Фамилия + ' ' + Имя + ' ' + Отчество");
            LoadComboBox(ComboBoxUser, usersTable, "FullName", "ID_Гражданина");

            LoadComboBox(ComboBoxStatus, statusAdapter.GetData(), "Название", "ID_Статуса");

            LoadComboBox(ComboBoxEvents, eventAdapter.GetData(), "Название", "ID_Мероприятия");
        }
    }
}
