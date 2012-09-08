using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Security.Cryptography;
using System.IO;
using System.Xml;

namespace StrategyServer
{
    class Server
    {
        public World World { get; set; }

        private TcpListener tcpListener;
        private Thread listenThread;
        private int port;
        private List<Client> Clients;
        private UTF8Encoding encoder;

        public Server()
        {
            Clients = new List<Client>();
            LoadConfig();
            port = 9050; //TODO: (load port from configuration) Create config.xml
            encoder = new UTF8Encoding();
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
                try
                {
                    //blocks until a client has connected to the server
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    //create a thread to handle communication with connected client
                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                    Client client = new Client(tcpClient);
                    Clients.Add(client);
                    clientThread.Start(client);
                }
                catch
                {
                    Console.WriteLine("Warning!!! Exception in ListenForClients!!!");
                }
            }
        }

        private void HandleClientComm(object obj)
        {
            Client client = (obj as Client);
            TcpClient tcpClient = client.TcpClient;
            EndPoint endPoint = tcpClient.Client.RemoteEndPoint;
            AesManaged aes = new AesManaged();
            try
            {
                NetworkStream clientStream = tcpClient.GetStream();

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
                buffer2 = new byte[16];
                Array.Copy(buffer, buffer2, 16);
                aes.IV = buffer2;
                buffer2 = new byte[32];
                Array.Copy(buffer, 16, buffer2, 0, 32);
                aes.Key = buffer2;

                int bytesRead;

                while (true)
                {
                    bytesRead = 0;
                    buffer = new byte[4096];
                    bytesRead = clientStream.Read(buffer, 0, 4096);
                    if (bytesRead == 0)
                    {
                        Console.WriteLine("Client has disconnected [" + endPoint.ToString() + "]");
                        break;
                    }

                    Console.WriteLine("Receiving message [" + endPoint.ToString() + "]");
                    //TODO: Handle request
                }
            }

            catch (SocketException e)
            {
                if ((e.InnerException as SocketException).ErrorCode == 10054)
                {
                    Console.WriteLine("Client has disconnected [" + endPoint.ToString() + "]");
                }
                Console.WriteLine("Unknown Socket Error occured! [" + endPoint.ToString() + "]" + e.Message);
            }
            catch (IOException e)
            {
                Console.WriteLine("Client enforced disconnection [" + endPoint.ToString() + "]");
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown Error occured! [" + endPoint.ToString() + "]" + e.Message);
            }
            finally
            {
                tcpClient.Close();
                Clients.Remove(client);
            }
        }

        private void LoadConfig()
        {
            Console.Write("Loading configuration...\n");
            try
            {
                XmlTextReader reader = new XmlTextReader("config.xml");
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "Port":
                                reader.Read();
                                port = int.Parse(reader.Value);
                                break;
                            /*  case "Name":
                                  reader.Read();
                                  name = reader.Value;
                                  continue;
                              case "Speed":
                                  reader.Read();
                                  Speed = int.Parse(reader.Value);
                                  continue;
                              case "Message":
                                  reader.Read();
                                  Message = reader.Value;
                                  continue;
                              case "WelcomeMessage":
                                  reader.Read();
                                  WelcomeMessage = reader.Value;
                                  continue;
                             * */
                        }
                    }
                }
                reader.Close();
            }
            catch (FileNotFoundException)
            {
                Console.Write("Configuration file not found!\n");
            }
            catch (XmlException ex)
            {
                Console.Write("Erorr in loading configuration: "+ ex.Message +"\n");
            }
            catch (Exception)
            {
                Console.Write("Loading configuration failed!\n");
            }
        }
    }
}