using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;

namespace diplom_loskutova.Page.AddOrChange
{
    public partial class StatusAOC : System.Windows.Controls.Page
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
        private DP_2025_LoskutovaDataSetTableAdapters.СТАТУСTableAdapter adapter = new DP_2025_LoskutovaDataSetTableAdapters.СТАТУСTableAdapter();

        // Конструктор страницы с передачей строки таблицы для редактирования.
        // Если строка передана, заполняет поля формы значениями из нее.
        public StatusAOC(DataRowView row = null)
        {
            InitializeComponent();

            if (row != null)
            {
                ChangeOrAdd = true;
                SetCurrentRow(row); // Заполнение формы данными строки
            }
            else
            {
                ChangeOrAdd = false;
                NamePage.Text = "Добавление записи в таблицу \"Статус\"";
            }
        }

        // Заполнить форму по переданной строке данных
        private void SetCurrentRow(DataRowView row)
        {
            currentRow = row;
            TextBoxName.Text = currentRow["Название"].ToString();
            TextBoxDescription.Text = currentRow["Описание"].ToString();
            NamePage.Text = $"Редактирование записи №{Convert.ToInt32(currentRow["ID_Статуса"])}";
        }

        // Проверка на дубликат записи
        private bool ExistsDuplicate()
        {
            var existingRecords = adapter.GetData()
                    .Where(row => row.Название == TextBoxName.Text && row.Описание == TextBoxDescription.Text);

            // При редактировании исключаем текущую запись по ее уникальному идентификатору, если он есть
            if (ChangeOrAdd && currentRow != null)
            {
                existingRecords = existingRecords.Where(row => row.ID_Статуса != Convert.ToInt32(currentRow["ID_Статуса"]));
            }

            return existingRecords.Any();
        }

        /// Обработчик кнопки сохранения.
        /// При изменении обновляет текущую строку DataRowView,
        /// При добавлении создает новую строку и сохраняет изменения.
        private void BtnSave(object sender, RoutedEventArgs e)
        {
            string name = TextBoxName.Text;
            string description = TextBoxDescription.Text;

            if (ExistsDuplicate())
            {
                MessageBox.Show("Такая запись уже существует.", "Ошибка");
                return;
            }

            try
            {
                if (ChangeOrAdd)
                {
                    // Изменяем выбранную запись
                    currentRow["Название"] = name;
                    currentRow["Описание"] = description;
                    // Сохраняем изменения в базе
                    adapter.Update((DP_2025_LoskutovaDataSet.СТАТУСDataTable)currentRow.DataView.Table);
                }
                else
                {
                    // Создаем новую запись в таблице
                    var newRow = db.СТАТУС.NewСТАТУСRow();
                    newRow.Название = name;
                    newRow.Описание = description;
                    db.СТАТУС.Rows.Add(newRow);

                    // Сохраняем новую запись в базе
                    adapter.Update(db.СТАТУС);
                }
                MessageBox.Show("Данные успешно сохранены", "Успех");
                DataChanged?.Invoke(this, EventArgs.Empty); // Вызываем событие, что данные изменились, чтобы загрузить их заново на предыдущей странице
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            // Вызываем событие, что данные изменились, чтобы загрузить их заново на предыдущей странице
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        // Обработчик кнопки возврата назад.
        // Возвращает пользователя на предыдущую страницу навигации.
        private void BtnBack(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}