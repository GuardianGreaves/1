using diplom_loskutova.Class;
using Microsoft.Win32;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace diplom_loskutova.Page.AddOrChange
{

    public partial class CitizensAOC : System.Windows.Controls.Page
    {
        private string fileName;
        private string fullPath;

        // DataSet для работы с таблицей 
        private DP_2025_LoskutovaDataSet db = new DP_2025_LoskutovaDataSet();

        // События для уведомления об изменениях данных и открытии страницы 
        public event EventHandler DataChanged;

        // Флаг для определения операции: изменение (true) или добавление (false)
        public bool ChangeOrAdd = false;

        // Представление текущей строки данных, редактируемой на форме
        private DataRowView currentRow = null;

        // Адаптер для работы с таблицей
        private DP_2025_LoskutovaDataSetTableAdapters.ГРАЖДАНИНTableAdapter adapter = new DP_2025_LoskutovaDataSetTableAdapters.ГРАЖДАНИНTableAdapter();

        // Конструктор страницы с передачей строки таблицы для редактирования.
        // Если строка передана, заполняет поля формы значениями из нее.
        public CitizensAOC(DataRowView row = null)
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
                NamePage.Text = "Добавление записи в таблицу \"Граждане\"";
            }
        }

        // Заполнить форму по переданной строке данных
        private void SetCurrentRow(DataRowView row)
        {
            currentRow = row;
            DatePickerBirthday.Text = currentRow["Дата_Рождения"].ToString();
            TextBoxTelephone.Text = currentRow["Контактный_Телефон"].ToString();
            TextBoxName.Text = currentRow["Имя"].ToString();
            TextBoxSurname.Text = currentRow["Фамилия"].ToString();
            TextBoxPatronymic.Text = currentRow["Отчество"].ToString();

            // ✅ Загрузка фото из БД с проверкой двух путей
            fileName = currentRow["Фото"].ToString(); // "photo_202602041234567.jpg"

            photo(fileName);

            NamePage.Text = $"Редактирование записи №{Convert.ToInt32(currentRow["ID_Гражданина"])}";
        }

        public void photo(string fileName)
        {
            // 1. Пробуем путь проекта
            string projectPath = Path.Combine(AppContext.BaseDirectory + "\\Image-citizen", fileName);
            if (File.Exists(projectPath))
            {
                PhotoImage.Source = new BitmapImage(new Uri(projectPath, UriKind.Absolute));
            }
            // 2. Если нет - пробуем рабочий стол
            else
            {
                string desktopPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "Image-citizen",
                    fileName
                );

                if (File.Exists(desktopPath))
                {
                    PhotoImage.Source = new BitmapImage(new Uri(desktopPath, UriKind.Absolute));
                }
                else
                {
                    PhotoImage.Source = null;
                    MessageBox.Show($"Фото не найдено:\n{projectPath}\n{desktopPath}"); // Отладка
                }
            }
        }

        // Проверка на дубликат записи
        private bool ExistsDuplicate(string telephone)
        {
            var existingRecords = adapter.GetData()
                    .Where(row => row.Контактный_Телефон == telephone);

            // При редактировании исключаем текущую запись по ее уникальному идентификатору, если он есть
            if (ChangeOrAdd && currentRow != null)
            {
                existingRecords = existingRecords.Where(row => row.ID_Гражданина != Convert.ToInt32(currentRow["ID_Гражданина"]));
            }

            return existingRecords.Any();
        }

        /// Обработчик кнопки сохранения.
        /// При изменении обновляет текущую строку DataRowView,
        /// При добавлении создает новую строку и сохраняет изменения.
        private void BtnSave(object sender, RoutedEventArgs e)
        {
            DateTime birthday = DatePickerBirthday.DisplayDate;
            string telephone = TextBoxTelephone.Text;
            string name = TextBoxName.Text;
            string surname = TextBoxSurname.Text;
            string patronymic = TextBoxPatronymic.Text;
            string photo = fileName;

            if (ExistsDuplicate(telephone))
            {
                var msg = new diplom_loskutova.NotificationDialog(
                "Ошибка",
                $"Запись с таким номером уже существует",
                "");
                msg.ShowDialog();
                return;
            }

            try
            {
                if (ChangeOrAdd)
                {
                    // Изменяем выбранную запись
                    currentRow["Дата_Рождения"] = birthday;
                    currentRow["Контактный_Телефон"] = telephone;
                    currentRow["Имя"] = name;
                    currentRow["Фамилия"] = surname;
                    currentRow["Отчество"] = patronymic;
                    currentRow["Фото"] = photo;
                    // Сохраняем изменения в базе
                    adapter.Update((DP_2025_LoskutovaDataSet.ГРАЖДАНИНDataTable)currentRow.DataView.Table);
                }
                else
                {
                    // Создаем новую запись в таблице
                    var newRow = db.ГРАЖДАНИН.NewГРАЖДАНИНRow();
                    newRow.Дата_Рождения = birthday;
                    newRow.Контактный_Телефон = telephone;
                    newRow.Имя = name;
                    newRow.Фамилия = surname;
                    newRow.Отчество = patronymic;
                    newRow.Фото = photo;
                    db.ГРАЖДАНИН.Rows.Add(newRow);

                    // Сохраняем новую запись в базе
                    adapter.Update(db.ГРАЖДАНИН);
                }
                var msg = new diplom_loskutova.NotificationDialog(
                "Выполнено",
                $"Данные успешно сохранены",
                "");
                msg.ShowDialog();
                DataChanged?.Invoke(this, EventArgs.Empty); // Вызываем событие, что данные изменились, чтобы загрузить их заново на предыдущей странице
            }
            catch (Exception ex)
            {
                var msg = new diplom_loskutova.NotificationDialog(
                "Ошибка",
                $"Ошибка: {ex.Message}",
                "");
                msg.ShowDialog();
            }
            NavigationService.GoBack();
        }

        // Кнопка назад, возвращает на предыдущую страницу
        private void BtnBack(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string photosSubdirectory = "Image";
        string selectedPhotoFullPath;  // полный путь к выбранному файлу

        private readonly ImageFileManager _imageManager = new ImageFileManager();

        private void BtnSelectPhoto(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog() { Filter = "Изображения|*.jpg;*.png" };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // 1. ✅ СОХРАНЯЕМ ФАЙЛ (получаем реальный путь)
                    string savedFilePath = _imageManager.SaveImageWithUniqueName(dialog.FileName);
                    string fileNameOnly = Path.GetFileName(savedFilePath);  // только имя файла

                    // 2. ✅ СОХРАНЯЕМ В БД (файл УЖЕ существует!)
                    SavePhotoToDatabase(fileNameOnly);
                    var msg = new diplom_loskutova.NotificationDialog(
                        "Выполнено",
                        $"Файл сохранен",
                        $"Путь к файлу: {savedFilePath}");
                    msg.ShowDialog();
                }
                catch (Exception ex)
                {
                    var msg = new diplom_loskutova.NotificationDialog(
                    "Ошибка",
                    $"Ошибка: {ex.Message}",
                    "");
                    msg.ShowDialog();
                }
            }
        }

        private void SavePhotoToDatabase(string relativePath)
        {
            fileName = relativePath;
            photo(relativePath);
        }

    }
}
