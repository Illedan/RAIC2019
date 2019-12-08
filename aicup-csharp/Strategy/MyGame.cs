﻿using System.Collections.Generic;
using AiCup2019.Model;
using System.Linq;
using System.Diagnostics;
using System;
using static AiCup2019.Model.CustomData;

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
