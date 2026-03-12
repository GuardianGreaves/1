using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace diplom_lib_loskutova.Captcha
{
        public class Captcha
    {
        private int _width; // Ширина изображения капчи
        private int _height; // Высота изображения капчи
        private Random _random; // Генератор случайных чисел для различных случайных операций

        // Свойство для хранения сгенерированного текста капчи
        public string CaptchaText { get; private set; }

        public Captcha(int width, int height)
        {
            _width = width;
            _height = height;
            _random = new Random();

            // Генерируем текст капчи при создании объекта
            CaptchaText = RandomText();
        }

        public Bitmap GenerateCaptcha(string fontFamilyName = "Comic Sans MS", float fontSize = 20F)
        {
            // Создание нового изображения капчи с заданными параметрами
            Bitmap captchaImage = new Bitmap(_width, _height, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(captchaImage))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Заполнение фона штриховой кистью
                using (HatchBrush hatchBrush = new HatchBrush(HatchStyle.SmallConfetti, Color.LightGray, Color.White))
                {
                    g.FillRectangle(hatchBrush, 0, 0, _width, _height);
                }

                // Настройка шрифта
                using (Font font = new Font(fontFamilyName, fontSize, FontStyle.Bold | FontStyle.Italic))
                {
                    // Настройка формата текста
                    StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                    // Получение упорядоченных позиций для символов текста
                    List<PointF> characterPositions = GetOrderedPositions(CaptchaText.Length, font, _width, _height);
                    for (int i = 0; i < CaptchaText.Length; i++)
                    {
                        // Применение случайного вращения к каждому символу
                        using (Matrix matrix = new Matrix())
                        {
                            matrix.RotateAt(_random.Next(-30, 30), characterPositions[i]);
                            g.Transform = matrix;

                            // Отрисовка текста серой кистью
                            g.DrawString(CaptchaText[i].ToString(), font, Brushes.Gray, characterPositions[i], format);

                            // Сброс матрицы трансформации
                            g.ResetTransform();
                        }
                    }
                }

                // Добавление кривых линий в качестве шума
                for (int i = 0; i < 7; i++)
                {
                    PointF startPoint = new PointF(_random.Next(_width), _random.Next(_height));
                    PointF controlPoint1 = new PointF(startPoint.X + _random.Next(-50, 50), startPoint.Y + _random.Next(-50, 50));
                    PointF controlPoint2 = new PointF(startPoint.X + _random.Next(-50, 50), startPoint.Y + _random.Next(-50, 50));
                    PointF endPoint = new PointF(startPoint.X + _random.Next(-50, 50), startPoint.Y + _random.Next(-50, 50));

                    using (Pen pen = new Pen(Color.Gray))  // Можно использовать другой цвет 
                    {
                        using (GraphicsPath path = new GraphicsPath())
                        {
                            path.AddBezier(startPoint, controlPoint1, controlPoint2, endPoint);

                            g.DrawPath(pen, path);
                        }
                    }
                }
            }

            return captchaImage;
        }

        private List<PointF> GetOrderedPositions(int count, Font font, int maxWidth, int maxHeight, float margin = 2F)
        {
            List<PointF> positions = new List<PointF>();
            float totalWidth = count * (font.Size - margin);
            float startX = (maxWidth - totalWidth) / 2;
            float startY = (maxHeight - font.Size) / 2;

            for (int i = 0; i < count; i++)
            {
                // Рассчет позиции для символов текста, учитывая отступы
                float x = startX + i * (font.Size - margin);
                float y = startY + 8;  // 8 - это константное смещение
                positions.Add(new PointF(x, y));
            }

            return positions;
        }

        // Генерация случайного текста капчи
        private string RandomText()
        {
            const string chars = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя0123456789";
            StringBuilder stringBuilder = new StringBuilder(5);

            Parallel.ForEach(Enumerable.Range(0, 5), _ =>
            {
                stringBuilder.Append(chars[_random.Next(chars.Length)]);
            });

            return stringBuilder.ToString();
        }

        // Генерация капчи и конвертация в BitmapImage
        public BitmapImage GenerateCaptchaBitmapImage()
        {
            Bitmap captchaImage = GenerateCaptcha();
            return ConvertBitmapToBitmapImage(captchaImage);
        }

        // Конвертация Bitmap в BitmapImage
        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Png);

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Оптимизация для ускорения загрузки

                return bitmapImage;
            }
        }
    }
}
