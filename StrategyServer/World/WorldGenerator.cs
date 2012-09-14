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
            world.IPBans = new List<IPBan>();

#if DEBUG
            System.Security.Cryptography.SHA256Managed sha256 = new System.Security.Cryptography.SHA256Managed();
            UTF8Encoding utf8Encoder = new UTF8Encoding();
            world.Players.Add(new Player("Majzlík", "Majzlík", sha256.ComputeHash(utf8Encoder.GetBytes("majzlik"))));
            world.Players.Add(new Player("Setal", "Setal", sha256.ComputeHash(utf8Encoder.GetBytes("setal"))));
#endif
            return world;
        }
    }
}