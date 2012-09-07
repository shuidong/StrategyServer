using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrategyServer
{
    [Serializable]
    class World
    {
        public int Day { get; set; }
        public byte Width { get; set; }
        public byte Height { get; set; }
        public Tile[,] Tiles { get; set; }
        public List<Player> Players { get; set; }
        public List<Village> Villages { get; set; }
    }
}