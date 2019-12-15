using System.Collections.Generic;
using AiCup2019.Model;
using System.Linq;
using System.Diagnostics;
using System;
using static AiCup2019.Model.CustomData;
using static AiCup2019.Model.Item;
using aicup2019.Strategy.Services;

namespace aicup2019.Strategy
{
    public class MyGame
    {
        // Find all potential positions of the enemy. Make a circle of all places he can be. Shoot if I can limit it.
        public Stopwatch s;
        public List<MyBullet> Bullets = new List<MyBullet>();
        public List<MyUnit> Units = new List<MyUnit>();
        public int Width, Height;
        public Player MePlayer, EnemyPlayer;
        public MyUnit Me, Enemy;
        public MyGame(Game game, Unit me)
        {
            s = Stopwatch.StartNew();
            Game = game;
            Width = Game.Level.Tiles.Length;
            Height = Game.Level.Tiles[0].Length;
            Units.AddRange(game.Units.Select(u => new MyUnit(u)));
            Bullets.AddRange(game.Bullets.Select(b => new MyBullet(b)));
            MePlayer = game.Players.First(p => p.Id == me.PlayerId);
            EnemyPlayer = game.Players.First(p => p.Id != me.PlayerId);
            Me = Units.First(u => u.Unit.Id == me.Id);
        }

        public void Init()
        {
            Enemy = Units.OrderBy(u => DistService.GetDist(u.Center, Me.Center)).First(u => u.Unit.PlayerId != Me.Unit.PlayerId);
        }

        public int XDiff => Me.Center.X < Enemy.Center.X ? 1 : -1;
        public double TargetDist => DistService.GetDist(Me.Center, Enemy.Center);
        public int ScoreDiff => MePlayer.Score - EnemyPlayer.Score;
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
            for(var i = 2; i < heights.Length-2; i++)
            {
                var h = heights[i];
                if (heights[i - 1] + 1 < h || heights[i + 1] < h) hideouts.Add(new MyPosition(i, heights[i]+1));
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


        public Vec2Float CalcBulletEnd(MyBullet bullet, out bool didHit)
        {
            var potentialUnits = Units.Where(u => u.Unit.Id != bullet.Bullet.UnitId).ToArray();
            var pos = new Vec2Double(bullet.Bullet.Position.X, bullet.Bullet.Position.Y);
            var speed = bullet.Bullet.Velocity;
            var ticks = Const.Properties.TicksPerSecond;
            speed = new Vec2Double(speed.X / ticks, speed.Y / ticks);
            var i = 0;
            didHit = false;
            while (true)
            {
                pos = new Vec2Double(pos.X + speed.X, pos.Y + speed.Y);
                if (!OnBoard(pos.X, pos.Y)) break;
                var rect = Rect.FromMovingBullet(pos, bullet.Bullet.Size);
                foreach (var u in potentialUnits)
                {
                    if (u.Size.Overlapping(rect))
                    {
                        didHit = true;
                        break;
                    }
                }

                if (didHit) break;

                var tile = GetTile(pos.X, pos.Y);
                if (tile == Tile.Wall) break;
                i++;
            }

            return new Vec2Float((float)pos.X, (float)pos.Y);
        }
    }
}
