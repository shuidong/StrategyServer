using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrategyServer
{
    static class WorldGenerator
    {
        public static World Generate()
        {
            World world = new World();
            world.Day = 1;
            world.Width = 50;
            world.Height = 50;
            world.Tiles = new Tile[world.Width, world.Height];
            world.Registrations = new List<Registration>();
            world.Players = new List<Player>();
            world.Villages = new List<Village>();

            return world;
        }
    }
}