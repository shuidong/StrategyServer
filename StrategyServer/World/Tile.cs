using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrategyServer
{
    [Serializable]
    class Tile
    {
        public byte Type { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }
    }
}