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

        private byte[] clientFileBuffer;

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
            LoadClient();
            World = WorldGenerator.Generate();
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
                    IPEndPoint ipEndPoint = tcpClient.Client.RemoteEndPoint as IPEndPoint;
                    IPAddress ip = ipEndPoint.Address;
                    foreach (IPBan ban in World.IPBans)
                    {
                        if (ban.IP.ToString() == ipEndPoint.Address.ToString())
                        {
                            tcpClient.Close();
                            throw new KickOutException("Warning! Attempt to conect from banned IP! " + ban.IP.ToString());
                        }
                    }

                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientCommunication));
                    Client client = new Client(tcpClient, encoder, clientThread);
                    Clients.Add(client);
                    clientThread.Start(client);
                }
                catch (KickOutException e)
                {
                    Console.WriteLine(e.Message);
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
                        Console.Write("Received message [" + endPoint.ToString() + "]: ");
                        byte[] buffer2 = new byte[length];
                        Array.Copy(buffer, buffer2, length);

                        string message = client.Decrypt(buffer2);
                        List<string> parameters = getParameters(message);
                        short requestType = short.Parse(parameters[0]);
                        Console.WriteLine((RequestType)requestType);
                        parameters.RemoveAt(0);
                        AnswerType answerType;
                        string answer = handleRequest((RequestType)requestType, parameters, client, out answerType);
                        buffer = client.Encrypt((short)answerType + "~" + answer);

                        clientStream.Write(buffer, 0, buffer.Length);
                        Console.WriteLine("Server answered: " + answerType);

                        if (answerType == AnswerType.Update)
                        {
                            clientStream.Write(clientFileBuffer, 0, clientFileBuffer.Length);
                        }
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
            catch (IOException)
            {
                Console.WriteLine("Client enforced disconnection [" + endPoint.ToString() + "]");
            }
            catch (KickOutException e)
            {
                Console.WriteLine("Client was kicked [" + endPoint.ToString() + "] " + e.Message);
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

        private string handleRequest(RequestType type, List<string> parameters, Client client, out AnswerType answerType) //This is actually giant switch
        {
            if (client.Player == null)
            {
                return handleUnloggedRequest(type, parameters, client, out answerType);
            }
            else
            {
                throw new KickOutException("Invalid Request from authorized Client");
            }
        }

        private string handleUnloggedRequest(RequestType type, List<string> parameters, Client client, out AnswerType answerType)
        {
            IPEndPoint ipEndPoint = client.TcpClient.Client.RemoteEndPoint as IPEndPoint;
            switch (type)
            {
                case RequestType.Welcome:
                    Version clientVersion = new Version(int.Parse(parameters[1]), int.Parse(parameters[2]), int.Parse(parameters[3]), int.Parse(parameters[4]));
                    bool isSupported = (clientVersion.Major == supportedVersion.Major && clientVersion.Minor == supportedVersion.Minor && clientVersion.Build == supportedVersion.Build);
                    answerType = AnswerType.Welcome;
                    return string.Format("{0}~{1}~{2}~", isSupported, name, Message);

                case RequestType.Update:
                    answerType = AnswerType.Update;
                    return string.Format("{0}~", clientFileBuffer.Length);

                case RequestType.Registration:
                    byte[] buffer = new byte[1024];
                    int length = client.TcpClient.GetStream().Read(buffer, 0, 1024);
                    byte[] passwordBuffer = new byte[length];
                    Array.Copy(buffer, passwordBuffer, length);

                    if (parameters[0].Length > 24 || parameters[1].Length > 16 || parameters[2].Length > 1024)
                    {
                        throw new KickOutException("Invalid Registration Input");
                    }

                    Registration newRegistration = new Registration(parameters[0], parameters[1], parameters[2], passwordBuffer, ipEndPoint.Address);

                    answerType = AnswerType.Registration;
                    if (World.Registrations.Count >= 10)
                    {
                        return string.Format("{0}~", 6);
                    }

                    int errorCode = 0;

                    foreach (Registration registration in World.Registrations)
                    {
                        if (registration.IP.ToString() == newRegistration.IP.ToString())
                        {
                            errorCode = 3;
                            break;
                        }
                        if (registration.Login == newRegistration.Login)
                        {
                            errorCode = 1;
                            break;
                        }
                        if (registration.Name == newRegistration.Name)
                        {
                            errorCode = 2;
                            break;
                        }
                    }

                    if (errorCode == 0)
                    {
                        foreach (Player player in World.Players)
                        {
                            if (player.Login == newRegistration.Login)
                            {
                                errorCode = 4;
                                break;
                            }
                            if (player.Name == newRegistration.Name)
                            {
                                errorCode = 5;
                                break;
                            }
                        }

                        if (errorCode == 0)
                        {
                            World.Registrations.Add(newRegistration);
                        }
                    }
                    return string.Format("{0}~", errorCode);

                case RequestType.Login:
                    answerType = AnswerType.Login;
                    buffer = new byte[1024];
                    length = client.TcpClient.GetStream().Read(buffer, 0, 1024);
                    passwordBuffer = new byte[length];
                    Array.Copy(buffer, passwordBuffer, length);

                    foreach (Player player in World.Players)
                    {
                        if (player.Login == parameters[0])
                        {
                            bool passwordMatch = passwordBuffer.Length == player.Password.Length;
                            for (int i = 0; i < player.Password.Length; i++)
                            {
                                if (passwordBuffer[i] != player.Password[i])
                                {
                                    passwordMatch = false;
                                    break;
                                }
                            }
                            if (passwordMatch)
                            {
                                client.Player = player;
                                return "0~";
                            }
                        }
                    }
                    if (client.BadLoginAttempts >= 2)
                    {
                        banThis(ipEndPoint.Address, 5, 0, "Bad Logins");
                        throw new KickOutException("Banned IP, too many bad login attemts");
                    }
                    else
                    {
                        client.BadLoginAttempts++;
                        return "1~";
                    }
            }
            throw new KickOutException("Invalid Request from unauthorized Client");
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
            Console.WriteLine("Loading configuration...\n");
            try
            {
                using (XmlTextReader reader = new XmlTextReader("config.xml"))
                {
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
                    Console.WriteLine("Configuration file loaded successfully.");
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Configuration file not found!");
            }
            catch (XmlException ex)
            {
                Console.WriteLine("Error in loading configuration: " + ex.Message + " .");
            }
            catch (Exception)
            {
                Console.WriteLine("Loading configuration failed!");
            }
        }

        private void LoadClient()
        {
            clientFileBuffer = File.ReadAllBytes("StrategyClient.exe");
        }

        private void banThis(IPAddress iPAddress, int duration, int thread, string reason)
        {
            foreach (IPBan ban in World.IPBans)
            {
                if (ban.IP == iPAddress)
                {
                    if (ban.Duration >= 0)
                    {
                        ban.Duration += duration;
                    }
                    ban.ThreadLevel += thread;
                    ban.Reason = reason;
                    return;
                }
            }
            IPBan newBan = new IPBan(iPAddress, duration, thread, reason);
            World.IPBans.Add(newBan);
        }
    }
}