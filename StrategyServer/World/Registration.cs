using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace StrategyServer
{
    [Serializable]
    class Registration
    {
        public string Login { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public byte[] Password { get; set; }
        public IPAddress IP { get; set; }

        public Registration(string login, string name, string description, byte[] password, IPAddress ip)
        {
            Login = login;
            Name = name;
            Description = description;
            Password = password;
            IP = ip;
        }
    }
}