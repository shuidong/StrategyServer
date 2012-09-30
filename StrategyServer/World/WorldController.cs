using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrategyServer
{
    class WorldController
    {
        private World world;

        public WorldController(World world)
        {
            this.world = world;
        }

        public World Generate()
        {
            Console.WriteLine("Generating new World...");
            world.Day = 1;
            world.Width = 50;
            world.Height = 50;
            world.Tiles = GenerateMap(world.Width, world.Height);
            world.Registrations = new List<Registration>();
            world.Players = new List<Player>();
            world.Villages = new List<Village>();
            world.IPBans = new List<IPBan>();

#if DEBUG
            System.Security.Cryptography.SHA256Managed sha256 = new System.Security.Cryptography.SHA256Managed();
            UTF8Encoding utf8Encoder = new UTF8Encoding();
            Player player = new Player();
            player.Name = "Majzlík";
            player.Login = "Majzlík";
            player.Password = sha256.ComputeHash(utf8Encoder.GetBytes("majzlik"));
            world.Players.Add(player);
            player = new Player();
            player.Name = "Setal";
            player.Login = "Setal";
            player.Password = sha256.ComputeHash(utf8Encoder.GetBytes("setal"));
            world.Players.Add(player);
#endif
            Console.WriteLine("New World Generated");
            return world;
        }

        public Tile[,] GenerateMap(byte width, byte height)
        {
            Console.Write("Generating map... ");
            Random random = Server.Random;
            Tile[,] map = new Tile[width, height];
            int blank = 0, lake = 0, forest = 0;
            for (int i = 0; i < width * height; i++)
            {
                map[i % width, i / width] = new Tile();
                Tile tile = map[i % width, i / width];
                tile.X = (byte)(i % width);
                tile.Y = (byte)(i / width);
                int value = random.Next(0, 5);
                if (value < 3)
                {
                    tile.Type = 0;
                    blank++;
                }
                else if (value == 3)
                {
                    tile.Type = 1;
                    lake++;
                }
                else
                {
                    tile.Type = 2;
                    forest++;
                }

            }
            Console.WriteLine("Done");
            Console.WriteLine("Generated map with {0} tiles total. Width: {1} Height: {2}", width * height, width, height);
            Console.WriteLine("Blank tiles: {0} Lake tiles: {1} Forest tiles: {2} ", blank, lake, forest);
            return map;
        }

        public string GetMap(Player player)
        {
            return GetMap(player, (byte)(world.Width / 2), (byte)(world.Height / 2));
        }

        public string GetMap(Player player, byte xCenter, byte yCenter)
        {
            if (xCenter < 5)
            {
                xCenter = 5;
            }
            if (xCenter > world.Width - 6)
            {
                xCenter = (byte)(world.Width - 6);
            }
            if (yCenter < 5)
            {
                yCenter = 5;
            }
            if (yCenter > world.Height - 6)
            {
                yCenter = (byte)(world.Height - 6);
            }

            string dataPartOne = string.Format("{0}~{1}~{2}~{3}~", world.Width, world.Height, xCenter, yCenter);
            string dataPartTwo = string.Empty;
            for (int i = 0; i < 121; i++)
            {
                Tile tile = world.Tiles[i % 11 + (xCenter - 5), i / 11 + (yCenter - 5)];
                if (tile is Village)
                {
                    Village village = tile as Village;
                    char villageState = 'E';
                    if (village.Owner == null)
                    {
                        villageState = 'I';
                    }
                    if (village.Owner == player)
                    {
                        villageState = 'M';
                    }
                    dataPartOne += string.Format("{0}~", (village.Buildings[0] / 5) + 128);
                    dataPartTwo += string.Format("{0}~{1}~{2}~{3}~", village.Name, village.Owner.Name, villageState, (int)village.Population);
                }
                else
                {
                    dataPartOne += string.Format("{0}~", tile.Type);
                }
            }
            return dataPartOne + dataPartTwo;
        }

        public string GetNotifications(Player player)
        {
            return string.Format("{0}~", world.Day);
        }
    }
}