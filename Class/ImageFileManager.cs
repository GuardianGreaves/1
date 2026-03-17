using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diplom_loskutova.Class
{
    public class ImageFileManager
    {
        private readonly string _projectImageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Image-citizen");
        private readonly string _backupFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Image-citizen");

        public string SaveImageWithUniqueName(string sourcePath)
        {
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException($"Файл не найден: {sourcePath}");

            // 1. Пробуем сохранить в проектную папку
            string targetPath = TryCopyToFolder(sourcePath, _projectImageFolder);
            if (targetPath != null) return targetPath;

            // 2. Пробуем сохранить на рабочий стол
            targetPath = TryCopyToFolder(sourcePath, _backupFolder);
            if (targetPath != null) return targetPath;

            throw new InvalidOperationException("Не удалось сохранить файл");
        }

        private string TryCopyToFolder(string sourcePath, string folderPath)
        {
            try
            {
                EnsureDirectoryExists(folderPath);

                // *** ГЕНЕРИРУЕМ имя ПОСЛЕ создания папки ***
                string uniqueName = GenerateUniqueFileName(sourcePath);
                string targetPath = Path.Combine(folderPath, uniqueName);

                // *** КОПИРУЕМ файл ***
                File.Copy(sourcePath, targetPath, true);

                return targetPath;  // ✅ Возвращаем реальный путь к СУЩЕСТВУЮЩЕМУ файлу
            }
            catch
            {
                return null;
            }
        }

        private string GenerateUniqueFileName(string sourcePath)
        {
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(sourcePath);
            string fileExt = Path.GetExtension(sourcePath);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            return $"{fileNameWithoutExt}_{timestamp}{fileExt}";
        }

        private bool EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return true;
        }
    }
}
