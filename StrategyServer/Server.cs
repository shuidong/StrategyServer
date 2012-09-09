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
        public string Message { get; set; }

        public World World { get; set; }

        private int port;
        private string name;

        private TcpListener tcpListener;
        private Thread listenThread;
        private List<Client> Clients;
        private UTF8Encoding encoder;
        private Version supportedVersion;

        public Server()
        {
            Clients = new List<Client>();
            LoadConfig();
            encoder = new UTF8Encoding();
            tcpListener = new TcpListener(IPAddress.Any, port);
            listenThread = new Thread(new ThreadStart(ListenForClients));
            listenThread.Start();
            supportedVersion = new Version("0.0.1.0");
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
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientCommunication));
                    Client client = new Client(tcpClient, encoder);
                    Clients.Add(client);
                    clientThread.Start(client);
                }
                catch
                {
                    Console.WriteLine("Warning!!! Exception in ListenForClients!!!");
                }
            }
        }

        private void HandleClientCommunication(object obj)
        {
            Client client = (obj as Client);
            TcpClient tcpClient = client.TcpClient;
            EndPoint endPoint = tcpClient.Client.RemoteEndPoint;

            try
            {
                client.SetupCommunication();
                NetworkStream clientStream = tcpClient.GetStream();
                byte[] buffer;

                while (true)
                {
                    buffer = new byte[4096];
                    int length = clientStream.Read(buffer, 0, 4096);
                    if (length == 0)
                    {
                        Console.WriteLine("Client has disconnected [" + endPoint.ToString() + "]");
                        break;
                    }
                    lock (this)
                    {
                        Console.Write("Received message [" + endPoint.ToString() + "] ");
                        byte[] buffer2 = new byte[length];
                        Array.Copy(buffer, buffer2, length);

                        string message = client.Decrypt(buffer2);
                        List<string> parameters = getParameters(message);
                        short requestType = short.Parse(parameters[0]);
                        Console.WriteLine("Type: " + (RequestType)requestType);
                        parameters.RemoveAt(0);
                        AnswerType answerType;
                        string answer = HandleRequest((RequestType)requestType, parameters, client, out answerType);
                        buffer = client.Encrypt(answer);

                        clientStream.Write(buffer, 0, buffer.Length);
                        Console.WriteLine("Server answered: " + answerType);
                    }
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

        private string HandleRequest(RequestType type, List<string> parameters, Client client, out AnswerType answerType) //This is actually giant switch
        {
            switch (type)
            {
                case RequestType.Welcome:
                    Version clientVersion = new Version(int.Parse(parameters[1]), int.Parse(parameters[2]), int.Parse(parameters[3]), int.Parse(parameters[4]));
                    bool isSupported = (clientVersion.Major == supportedVersion.Major && clientVersion.Minor == supportedVersion.Minor && clientVersion.Build == supportedVersion.Build);
                    answerType = AnswerType.Welcome;
                    return string.Format("{0}~{1}~{2}~{3}~", (short)AnswerType.Welcome, isSupported, name, Message);
            }
            answerType = AnswerType.UnknownRequestError;
            return string.Format("{0}~", (short)AnswerType.UnknownRequestError);
        }

        private List<string> getParameters(string message)
        {
            int lastIndex = 0;
            List<string> arguments = new List<string>();
            for (int i = 0; i < message.Length; i++)
            {
                if (message[i] == '~')
                {
                    arguments.Add(message.Substring(lastIndex, i - lastIndex));
                    lastIndex = i + 1;
                }
            }
            return arguments;
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
                            case "Name":
                                reader.Read();
                                name = reader.Value;
                                continue;
                            case "Message":
                                reader.Read();
                                Message = reader.Value;
                                continue;
                            /*  case "WelcomeMessage":
                                  reader.Read();
                                  WelcomeMessage = reader.Value;
                                  continue;
                              */
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
                Console.Write("Erorr in loading configuration: " + ex.Message + "\n");
            }
            catch (Exception)
            {
                Console.Write("Loading configuration failed!\n");
            }
        }
    }
}