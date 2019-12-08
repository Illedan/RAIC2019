using System;
using AiCup2019.Model;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using static AiCup2019.Model.CustomData;
using aicup2019.Strategy;


 // LastEdited: 09/12/2019 0:34 


namespace aicup2019.Strategy
{
    public class MyUnit
    {
        public MyUnit(Unit unit)
        {
            Unit = unit;
            Size = Rect.FromUnit(unit);
        }

        public Unit Unit { get; }
        public Rect Size;
    }
}

namespace aicup2019.Strategy
{
    public struct Rect
    {
        public Rect(double x1, double y1, double x2, double y2)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }

        public Rect Clone => new Rect(X1, Y1, X2, Y2);

        public static Rect FromMovingBullet(Vec2Double pos, double size)
        {
            var halfsize = size / 2;
            return new Rect(pos.X - halfsize, pos.Y - halfsize, pos.X + halfsize, pos.Y + halfsize);
        }

        public static Rect FromBullet(Bullet bullet)
        {
            return FromMovingBullet(bullet.Position, bullet.Size);
        }

        public static Rect FromUnit(Unit unit)
        {
            var size = unit.Size;
            var pos = unit.Position;
            return new Rect(pos.X - size.X / 2, pos.Y, pos.X + size.X / 2, pos.Y + size.Y);
        }

        public double X1, Y1, X2, Y2;

        public bool Overlapping(Rect rect)
        {
            return (rect.X2 >= X1 && rect.X1 <= X2) && (rect.Y2 >= Y1 && rect.Y1 <= Y2);
        }
    }
}

namespace aicup2019.Strategy
{
    public class MyGame
    {
        // Find all potential positions of the enemy. Make a circle of all places he can be. Shoot if I can limit it.
        public Stopwatch s;
        public List<MyBullet> Bullets = new List<MyBullet>();
        public List<MyUnit> Units = new List<MyUnit>();
        public int Width, Height;
        public MyGame(Game game)
        {
            s = Stopwatch.StartNew();
            Game = game;
            Width = Game.Level.Tiles.Length;
            Height = Game.Level.Tiles[0].Length;
            Units.AddRange(game.Units.Select(u => new MyUnit(u)));
            Bullets.AddRange(game.Bullets.Select(b => new MyBullet(b)));
        }

        public Game Game;

        public bool OnBoard(double x, double y) => OnBoard((int)x, (int)y);

        public bool OnBoard(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public Tile GetTile(double x, double y)
        {
            return Game.Level.Tiles[(int)x][(int)y];
        }
        public static Vec2Double NextPos;
        public Vec2Float CalcBulletEnd(MyBullet bullet)
        {
            var potentialUnits = Units.Where(u => u.Unit.Id != bullet.Bullet.UnitId).ToArray();
            var pos = new Vec2Double(bullet.Bullet.Position.X, bullet.Bullet.Position.Y);
            var speed = bullet.Bullet.Velocity;
            speed = new Vec2Double(speed.X / 60.0 / 100.0, speed.Y / 60.0 / 100.0);
            var i = 0;
            while (true)
            {
                pos = new Vec2Double(pos.X + speed.X, pos.Y + speed.Y);
                if (i == 101) NextPos = pos;
                if (!OnBoard(pos.X, pos.Y)) break;
                var rect = Rect.FromMovingBullet(pos, bullet.Bullet.Size);
                var didHit = false;
                foreach (var u in potentialUnits)
                {
                    if (u.Size.Overlapping(rect)) didHit = true;
                }

                if (didHit) break;

                var tile = GetTile(pos.X, pos.Y);
                if (tile == Tile.Wall) break;
                i++;
            }

            MyStrategy.Debug.Draw(new Log("TIME: " + s.ElapsedMilliseconds));
            return new Vec2Float((float)pos.X, (float)pos.Y);
        }
    }
}
namespace aicup2019.Strategy
{
    public class MyBullet
    {
        public MyBullet(Bullet bullet)
        {
            Bullet = bullet;
            Size = Rect.FromBullet(Bullet);
        }

        public Bullet Bullet { get; }
        public Rect Size;
    }
}

namespace AiCup2019.Model
{
    public class MyStrategy
    {
        static double DistanceSqr(Vec2Double a, Vec2Double b)
        {
            return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
        }

        public static Debug Debug;

        public UnitAction GetAction(Unit unit, Game game, Debug debug)
        {
            Debug = debug;
            Unit? nearestEnemy = null;
            foreach (var other in game.Units)
            {
                if (other.PlayerId != unit.PlayerId)
                {
                    if (!nearestEnemy.HasValue || DistanceSqr(unit.Position, other.Position) < DistanceSqr(unit.Position, nearestEnemy.Value.Position))
                    {
                        nearestEnemy = other;
                    }
                }
            }
            LootBox? nearestWeapon = null;
            foreach (var lootBox in game.LootBoxes)
            {
                if (lootBox.Item is Item.Weapon)
                {
                    if (!nearestWeapon.HasValue || DistanceSqr(unit.Position, lootBox.Position) < DistanceSqr(unit.Position, nearestWeapon.Value.Position))
                    {
                        nearestWeapon = lootBox;
                    }
                }
            }
            Vec2Double targetPos = unit.Position;
            if (!unit.Weapon.HasValue && nearestWeapon.HasValue)
            {
                targetPos = nearestWeapon.Value.Position;
            }
            else if (nearestEnemy.HasValue)
            {
                targetPos = nearestEnemy.Value.Position;
            }
            Vec2Double aim = new Vec2Double(0, 0);
            if (nearestEnemy.HasValue)
            {
                aim = new Vec2Double(nearestEnemy.Value.Position.X - unit.Position.X, nearestEnemy.Value.Position.Y - unit.Position.Y);
            }
            bool jump = targetPos.Y > unit.Position.Y;
            if (targetPos.X > unit.Position.X && game.Level.Tiles[(int)(unit.Position.X + 1)][(int)(unit.Position.Y)] == Tile.Wall)
            {
                jump = true;
            }
            if (targetPos.X < unit.Position.X && game.Level.Tiles[(int)(unit.Position.X - 1)][(int)(unit.Position.Y)] == Tile.Wall)
            {
                jump = true;
            }
            UnitAction action = new UnitAction();
            action.Velocity = targetPos.X - unit.Position.X;
            action.Jump = jump;
            action.JumpDown = !jump;
            action.Aim = aim;
            action.Shoot = true;
            action.Reload = false;
            action.SwapWeapon = true;
            action.PlantMine = false;
            var myGame = new MyGame(game);
            foreach (var bullet in myGame.Bullets)
            {
                var start = bullet.Bullet.Position;
                var end = myGame.CalcBulletEnd(bullet);
                var sFloat = new Vec2Float((float)start.X, (float)start.Y);
                var eFloat = new Vec2Float((float)end.X, (float)end.Y);
                debug.Draw(new Line(sFloat, eFloat, 0.3f, new ColorFloat(0, 0, 0, 1)));
            }

            return action;
        }
    }
}