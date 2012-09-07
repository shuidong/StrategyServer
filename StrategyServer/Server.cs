using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Security.Cryptography;

namespace StrategyServer
{
    class Server
    {
        public World World { get; set; }

        private TcpListener tcpListener;
        private Thread listenThread;
        private int port;
        private List<Client> Clients;

        public Server()
        {
            Clients = new List<Client>();
            port = 9050; //TODO load port from configuration
            tcpListener = new TcpListener(IPAddress.Any, port);
            listenThread = new Thread(new ThreadStart(ListenForClients));
            listenThread.Start();
        }

        public void Update()
        {

        }

        private void ListenForClients()
        {
            Console.WriteLine("Server Up... Listening at port " + port);
            tcpListener.Start();

            while (true)
            {
                //blocks until a client has connected to the server
                TcpClient tcpClient = tcpListener.AcceptTcpClient();

                //create a thread to handle communication with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                Client client = new Client(tcpClient);
                Clients.Add(client);
                clientThread.Start(client);
            }
        }

        private static void HandleClientComm(object obj)
        {
            Client client = (obj as Client);
            UTF8Encoding encoder = new UTF8Encoding();
            TcpClient tcpClient = client.TcpClient;
            NetworkStream clientStream = tcpClient.GetStream();


            EndPoint endPoint = tcpClient.Client.RemoteEndPoint;
            IPAddress address = (endPoint as IPEndPoint).Address;
            Console.WriteLine("Client has connected [" + endPoint.ToString() + "]");

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            string keyString = rsa.ToXmlString(false);

            byte[] buffer = encoder.GetBytes(keyString);
            clientStream.Write(buffer, 0, buffer.Length);

            int bytesRead;

            while (true)
            {
                bytesRead = 0;
                buffer = new byte[4096];

                try
                {
                    Console.WriteLine("Receiving message [" + endPoint.ToString() + "]");
                    bytesRead = clientStream.Read(buffer, 0, 4096);
                }

                catch (Exception e)
                {
                    if ((e.InnerException as SocketException).ErrorCode == 10054)
                    {
                        Console.WriteLine("Client has disconnected");
                        break;
                    }

                    Console.WriteLine("Unknown Error occured! " + e.Message);
                    break;
                }
                string message = encoder.GetString(buffer, 0, bytesRead);
                Console.WriteLine(message);
            }
            tcpClient.Close();
        }
    }
}