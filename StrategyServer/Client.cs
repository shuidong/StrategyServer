using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
using System.Net;

namespace StrategyServer
{
    class Client
    {
        private UTF8Encoding encoder;
        private AesManaged aes;
        private ICryptoTransform encryptor;
        private ICryptoTransform decryptor;

        public TcpClient TcpClient { get; set; }

        public Client(TcpClient tcpClient, UTF8Encoding encoder)
        {
            TcpClient = tcpClient;
            this.encoder = encoder;
        }

        public void SetupCommunication()
        {
            NetworkStream clientStream = TcpClient.GetStream();
            EndPoint endPoint = TcpClient.Client.RemoteEndPoint;

            Console.WriteLine("Client has connected [" + endPoint.ToString() + "]");

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            string keyString = rsa.ToXmlString(false);

            byte[] buffer = encoder.GetBytes(keyString);
            clientStream.Write(buffer, 0, buffer.Length);

            aes = new AesManaged();

            buffer = new byte[4096];
            int length = clientStream.Read(buffer, 0, buffer.Length);
            byte[] buffer2 = new byte[length];
            Array.Copy(buffer, buffer2, length);
            buffer = rsa.Decrypt(buffer2, true);
            buffer2 = new byte[16];
            Array.Copy(buffer, buffer2, 16);
            aes.IV = buffer2;
            buffer2 = new byte[32];
            Array.Copy(buffer, 16, buffer2, 0, 32);
            aes.Key = buffer2;

            encryptor = aes.CreateEncryptor();
            decryptor = aes.CreateDecryptor();
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