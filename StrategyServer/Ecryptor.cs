using System;
using System.IO;
using System.Security.Cryptography;

namespace StrategyServer
{
    class Encryptor
    {
        private AesManaged aes;
        private ICryptoTransform encryptor;
        private ICryptoTransform decryptor;

        public Encryptor()
        {
            aes = new AesManaged();
            encryptor = aes.CreateEncryptor();
            decryptor = aes.CreateDecryptor();
        }
        public Encryptor(byte[] key)
        {
            aes = new AesManaged();
            byte[] buffer = new byte[16];
            Array.Copy(key, buffer, 16);
            aes.IV = buffer;
            buffer = new byte[32];
            Array.Copy(key, 16, buffer, 0, 32);
            aes.Key = buffer;
            encryptor = aes.CreateEncryptor();
            decryptor = aes.CreateDecryptor();
        }

        public byte[] ExportKey()
        {
            byte[] buffer = new byte[48];
            Array.Copy(aes.IV, buffer, 16);
            Array.Copy(aes.Key, 0, buffer, 16, 32);
            return buffer;
        }

        public byte[] Encrypt(string text)
        {
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(text);
                    }
                }
                return msEncrypt.ToArray();
            }
        }
        public string Decrypt(byte[] buffer)
        {
            using (MemoryStream msDecrypt = new MemoryStream(buffer))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }
}