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
        private Encryptor encryptor;

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

            buffer = new byte[4096];
            int length = clientStream.Read(buffer, 0, buffer.Length);
            byte[] buffer2 = new byte[length];
            Array.Copy(buffer, buffer2, length);
            buffer = rsa.Decrypt(buffer2, true);

            encryptor = new Encryptor(buffer);
        }

        public byte[] Encrypt(string text)
        {
            return encryptor.Encrypt(text);
        }
        public string Decrypt(byte[] buffer)
        {
            return encryptor.Decrypt(buffer);
        }
    }
}