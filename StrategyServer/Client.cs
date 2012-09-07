using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrategyServer
{
    class Client
    {
        private System.Net.Sockets.TcpClient tcpClient;

        public Client(System.Net.Sockets.TcpClient tcpClient)
        {
            // TODO: Complete member initialization
            this.tcpClient = tcpClient;
        }

        public System.Net.Sockets.TcpClient TcpClient { get; set; }
    }
}