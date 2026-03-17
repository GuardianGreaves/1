using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace diplom_lib_loskutova.Encryption
{
    public class ScramblerEncryptor
    {
        protected string key = "13371337";
        protected string alphabet = "?><./,:';|{}[]+_=-()*&^%$#@!0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "abcdefghijklmnopqrstuvwxyzАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЬЫЭЮЯабвгдеёжзийклмнопрстуфхцчшщъьыэюя";

        public ScramblerEncryptor() { }

        public ScramblerEncryptor(string customKey)
        {
            if (!string.IsNullOrEmpty(customKey))
                key = customKey;
        }

        // Свойство для ключа
        public string Key
        {
            get { return key; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    key = value;
            }
        }

        // НОВЫЙ МЕТОД: Генерация ключа из пароля с помощью SHA256
        public void SetKeyFromPassword(string password, byte[] salt = null)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Если соль не передана, генерируем случайную
            if (salt == null || salt.Length == 0)
            {
                salt = new byte[16]; // 128 бит соли
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }
            }

            // Объединяем пароль + соль
            byte[] saltedPassword = new byte[salt.Length + passwordBytes.Length];
            Array.Copy(salt, 0, saltedPassword, 0, salt.Length);
            Array.Copy(passwordBytes, 0, saltedPassword, salt.Length, passwordBytes.Length);

            // Генерируем SHA256 хеш
            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(saltedPassword);
                // Преобразуем первые 8 байт хеша в строку цифр для скремблера
                key = string.Join("", Array.ConvertAll(hash.Take(8).ToArray(), b =>
                    Math.Min((b % 10) + '0', '9')));
            }
        }

        // Метод шифрования - скремблер с подстановочным ключом
        public string Encrypt(string inputText)
        {
            if (string.IsNullOrEmpty(inputText))
                return string.Empty;

            string extendedKey = "", encryptedText = "";

            // Расширяем ключ до длины входного текста
            int j = 0;
            for (int i = 0; i < inputText.Length; i++)
            {
                extendedKey += key[j];
                j++;
                if (j >= key.Length)
                    j = 0;
            }

            // Шифрование каждой буквы методом скремблера
            for (int i = 0; i < inputText.Length; i++)
            {
                for (int alphabetIndex = 0; alphabetIndex < alphabet.Length; alphabetIndex++)
                {
                    if (inputText[i] == alphabet[alphabetIndex])
                    {
                        int keyDigit = int.Parse(extendedKey[i].ToString());
                        int encryptedIndex = alphabetIndex + keyDigit;
                        if (encryptedIndex >= alphabet.Length)
                            encryptedIndex -= alphabet.Length;

                        encryptedText += alphabet[encryptedIndex];
                        break;
                    }
                }
            }
            return encryptedText;
        }
    }
}
