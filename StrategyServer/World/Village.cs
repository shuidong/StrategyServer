using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrategyServer
{
    [Serializable]
    class Village : Tile
    {
        public Player Owner { get; set; }

        public int Wood { get; set; }
        public int Stone { get; set; }
        public int Iron { get; set; }

        public string Name { get; set; }
        public double Population { get; set; }
        public byte[] Buildings { get; set; }
    }
}