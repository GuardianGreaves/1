using System;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace diplom_lib_loskutova.Encryption
{
    public class ScramblerDecryptor
    {
        protected string key = "13371337";
        // УБРАЛ пробел в конце для синхронизации с шифратором
        protected string alphabet = "?><./,:';|{}[]+_=-()*&^%$#@!0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "abcdefghijklmnopqrstuvwxyzАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЬЫЭЮЯабвгдеёжзийклмнопрстуфхцчшщъьыэюя";

        public ScramblerDecryptor() { }

        public ScramblerDecryptor(string customKey)
        {
            if (!string.IsNullOrEmpty(customKey))
                key = customKey;
        }

        // Хэш-функция для синхронизации с шифратором
        public void SetKeyFromPassword(string password, byte[] salt = null)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            if (salt == null || salt.Length == 0)
            {
                salt = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }
            }

            byte[] saltedPassword = new byte[salt.Length + passwordBytes.Length];
            Array.Copy(salt, 0, saltedPassword, 0, salt.Length);
            Array.Copy(passwordBytes, 0, saltedPassword, salt.Length, passwordBytes.Length);

            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(saltedPassword);
                key = string.Join("", Array.ConvertAll(hash.Take(8).ToArray(), b =>
                    Math.Min((b % 10) + '0', '9')));
            }
        }

        public string Key
        {
            get { return key; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    key = value;
            }
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            string extendedKey = "", decryptedText = "";

            // Расширяем ключ
            int j = 0;
            for (int i = 0; i < encryptedText.Length; i++)
            {
                extendedKey += key[j];
                j++;
                if (j >= key.Length)
                    j = 0;
            }

            // Дешифрование: ВЫЧИТАНИЕ сдвига
            for (int i = 0; i < encryptedText.Length; i++)
            {
                for (int alphabetIndex = 0; alphabetIndex < alphabet.Length; alphabetIndex++)
                {
                    if (encryptedText[i] == alphabet[alphabetIndex])
                    {
                        int keyDigit = int.Parse(extendedKey[i].ToString());
                        int decryptedIndex = alphabetIndex - keyDigit;

                        if (decryptedIndex < 0)
                            decryptedIndex += alphabet.Length;

                        decryptedText += alphabet[decryptedIndex];
                        break;
                    }
                }
            }
            return decryptedText;
        }
    }
}
