using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace StrategyServer
{
    class Client
    {
        public TcpClient TcpClient { get; set; }

        public Client(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
        }
    }
}