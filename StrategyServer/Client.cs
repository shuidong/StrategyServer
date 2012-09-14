using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
using System.Net;
using System.Threading;

namespace StrategyServer
{
    class Client : IDisposable
    {
        private UTF8Encoding encoder;
        private Encryptor encryptor;

        public TcpClient TcpClient { get; set; }
        public Thread Thread { get; private set; }
        public Player Player { get; set; }

        public int BadLoginAttempts { get; set; }

        public Client(TcpClient tcpClient, UTF8Encoding encoder, Thread thread)
        {
            TcpClient = tcpClient;
            this.encoder = encoder;
            Thread = thread;
            BadLoginAttempts = 0;
        }

        public void SetupCommunication()
        {
            NetworkStream clientStream = TcpClient.GetStream();
            EndPoint endPoint = TcpClient.Client.RemoteEndPoint;

            Console.WriteLine("Client has connected [" + endPoint.ToString() + "]");

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                string keyString = rsa.ToXmlString(false);

                byte[] buffer = encoder.GetBytes(keyString);
                clientStream.Write(buffer, 0, buffer.Length);

                buffer = new byte[4096];
                int length = clientStream.Read(buffer, 0, buffer.Length);
                byte[] buffer2 = new byte[length];
                Array.Copy(buffer, buffer2, length);
                buffer = rsa.Decrypt(buffer2, true);

                encryptor = new Encryptor(buffer);
            }
        }

        public byte[] Encrypt(string text)
        {
            return encryptor.Encrypt(text);
        }
        public string Decrypt(byte[] buffer)
        {
            return encryptor.Decrypt(buffer);
        }

        public void Dispose()
        {
            encryptor.Dispose();
        }
    }
}