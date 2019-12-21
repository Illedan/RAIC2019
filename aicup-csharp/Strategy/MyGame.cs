using System.Collections.Generic;
using AiCup2019.Model;
using System.Linq;
using static AiCup2019.Model.Item;

namespace aicup2019.Strategy
{
    //Contains stats about the game not needed in a sim.
    public class MyGame
    {
        public int Width, Height;
        public Player MePlayer, EnemyPlayer;

        public List<MyPosition> AllWalls = new List<MyPosition>();


        public MyGame(Game game, Unit me)
        {
            Game = game;
            Width = Game.Level.Tiles.Length;
            Height = Game.Level.Tiles[0].Length;
            MePlayer = game.Players.First(p => p.Id == me.PlayerId);
            EnemyPlayer = game.Players.First(p => p.Id != me.PlayerId);
            for(var x = 0; x < Width; x++)
            {
                for(var y = 0; y < Height; y++)
                {
                    if (Game.Level.Tiles[x][y] == Tile.Wall) AllWalls.Add(new MyPosition(x, y));
                }
            }
        }

        public bool HasHealing => HealthPacks.Any();

        public IEnumerable<MyPosition> HealthPacks => Game.LootBoxes.Where(l => l.Item is HealthPack).Select(h => new MyPosition(h.Position));

        public IEnumerable<LootBox> Weapons => Game.LootBoxes.Where(l => l.Item is Item.Weapon);

        public Game Game;

        public int GetHeight(double x0, double x1, double y)
        {
            var x = (int)x0;
            var x2 = (int)x1;
            for(var i = (int)y; i >= 0; i--)
            {
                if (Game.Level.Tiles[x][i] == Tile.Wall) return i + 1;
                if (Game.Level.Tiles[x2][i] == Tile.Wall) return i + 1;
            }

            return 0;
        }

        public List<MyPosition> GetHideouts()
        {
            var heights = GetHeights();
            var hideouts = new List<MyPosition>();
            for(var i = 4; i < heights.Length-4; i++)
            {
                var h = heights[i];
                var dx = i > heights.Length / 2 ? -1 : 1;
                if (heights[i + dx] + 2 < h) hideouts.Add(new MyPosition(i, heights[i]+1));
            }
            return hideouts;
        }

        public int[] GetHeights()
        {
            var heights = new int[Width];
            for(var x = 0; x < Width; x++)
            {
                var foundIt = false;
                for(var y = Height-1; y >= 0; y--)
                {
                    var tile = Game.Level.Tiles[x][y];
                    if (tile == Tile.Empty) foundIt = true;
                    else if (tile != Tile.Empty && foundIt)
                    {
                        heights[x] = y;
                        break;
                    }
                }
            }
            return heights;
        }

        public bool OnBoard(double x, double y) => OnBoard((int)x, (int)y);

        public bool OnBoard(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public Tile GetTile(double x, double y)
        {
            if ((int)x >= Width || x < 0 || y < 0 || (int)y >= Width) return Tile.Wall;
            return Game.Level.Tiles[(int)x][(int)y];
        }
    }
}
