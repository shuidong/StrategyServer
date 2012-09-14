using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace StrategyServer
{
    [Serializable]
    class IPBan
    {
        public IPAddress IP { get; set; }
        public int Duration { get; set; }
        public int ThreadLevel { get; set; }
        public string Reason { get; set; }

        public IPBan(IPAddress ip, int duration, int threadLevel, string reason)
        {
            IP = ip;
            Duration = duration;
            ThreadLevel = threadLevel;
            Reason = reason;
        }
    }
}