using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrategyServer
{
    [Serializable]
    class Player
    {
        public string Login { get; set; }
        public string Name { get; set; }
        public byte[] Password { get; set; }

        public Player(string name, string login, byte[] password)
        {
            Name = name;
            Login = login;
            Password = password;
        }
    }
}