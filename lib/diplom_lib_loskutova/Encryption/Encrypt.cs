namespace diplom_lib_loskutova.Encryption
{
    public class ScramblerEncryptor
    {
        protected string key = "13371337";
        protected string alphabet = "?><./,:';|{}[]+_=-()*&^%$#@!0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "abcdefghijklmnopqrstuvwxyzАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЬЫЭЮЯабвгдеёжзийклмнопрстуфхцчшщъьыэюя ";

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
