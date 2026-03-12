namespace diplom_lib_loskutova.Encryption
{
    public class ScramblerDecryptor
    {
        protected string key = "13371337";
        protected string alphabet = "?><./,:';|{}[]+_=-()*&^%$#@!0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "abcdefghijklmnopqrstuvwxyzАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЬЫЭЮЯабвгдеёжзийклмнопрстуфхцчшщъьыэюя ";

        public ScramblerDecryptor() { }

        public ScramblerDecryptor(string customKey)
        {
            if (!string.IsNullOrEmpty(customKey))
                key = customKey;
        }

        ~ScramblerDecryptor()
        {
            // Деструктор для освобождения ресурсов
        }

        // Свойство для ключа (должно совпадать с ключом шифратора!)
        public string Key
        {
            get { return key; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    key = value;
            }
        }

        // Метод дешифрования - обратный алгоритм скремблера
        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            string extendedKey = "", decryptedText = "";

            // Расширяем ключ до длины зашифрованного текста
            int j = 0;
            for (int i = 0; i < encryptedText.Length; i++)
            {
                extendedKey += key[j];
                j++;
                if (j >= key.Length)
                    j = 0;
            }

            // Дешифрование каждой буквы
            for (int i = 0; i < encryptedText.Length; i++)
            {
                for (int alphabetIndex = 0; alphabetIndex < alphabet.Length; alphabetIndex++)
                {
                    if (encryptedText[i] == alphabet[alphabetIndex])
                    {
                        int keyDigit = int.Parse(extendedKey[i].ToString());
                        int decryptedIndex = alphabetIndex - keyDigit;

                        // Если индекс стал отрицательным - переносим в конец алфавита
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
