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
                    // MessageBox.Show($"Фото не найдено:\n{projectPath}\n{desktopPath}"); // Отладка
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
                MessageBox.Show("Запись с таким номером уже существует.", "Ошибка");
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
                MessageBox.Show("Данные успешно сохранены", "Успех");
                DataChanged?.Invoke(this, EventArgs.Empty); // Вызываем событие, что данные изменились, чтобы загрузить их заново на предыдущей странице
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void BtnSelectPhoto(object sender, RoutedEventArgs e)
        {
            // Открываем диалог выбора файла
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Выберите изображение гражданина",
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Все файлы|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string sourceFile = openFileDialog.FileName;
                string projectFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Image-citizen");
                string desktopFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Image-citizen");
                string targetFolder = null;
                string targetFile = null;

                try
                {
                    // Пробуем создать/использовать папку в проекте
                    if (!Directory.Exists(projectFolder))
                    {
                        Directory.CreateDirectory(projectFolder);
                    }

                    // Создаем уникальное имя файла с timestamp
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(sourceFile);
                    string fileExt = Path.GetExtension(sourceFile);
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"); // 20260204_152323_456
                    string uniqueFileName = $"{fileNameWithoutExt}_{timestamp}{fileExt}";
                    targetFile = Path.Combine(projectFolder, uniqueFileName);
                    targetFolder = projectFolder;

                    // Копируем файл
                    File.Copy(sourceFile, targetFile, true);

                    MessageBox.Show($"Файл успешно скопирован в проект:\n{targetFile}", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    SavePhotoToDatabase(uniqueFileName);
                }
                catch (UnauthorizedAccessException)
                {
                    try
                    {
                        // Если нет прав на папку проекта, используем рабочий стол
                        if (!Directory.Exists(desktopFolder))
                        {
                            Directory.CreateDirectory(desktopFolder);
                        }

                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(sourceFile);
                        string fileExt = Path.GetExtension(sourceFile);
                        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                        string uniqueFileName = $"{fileNameWithoutExt}_{timestamp}{fileExt}";
                        targetFile = Path.Combine(desktopFolder, uniqueFileName);
                        targetFolder = desktopFolder;

                        File.Copy(sourceFile, targetFile, true);

                        MessageBox.Show($"Файл скопирован на рабочий стол:\n{targetFile}", "Сохранено на Рабочий стол",
                                      MessageBoxButton.OK, MessageBoxImage.Information);

                        SavePhotoToDatabase(uniqueFileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка копирования: {ex.Message}", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Неожиданная ошибка: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
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
