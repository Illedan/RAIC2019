﻿using System;
using AiCup2019.Model;
using aicup2019.Strategy.Services;

namespace aicup2019.Strategy
{
    public class MyPosition
    {
        public double X, Y;

        public MyPosition(double x, double y)
        {
            X = x;
            Y = y;
        }

        public MyPosition(Vec2Double vect)
        {
            X = vect.X;
            Y = vect.Y;
        }

        public int GetDx(MyPosition p2) => p2.X > X ? 1 : ((int)p2.X == (int)X)?0:-1;

        public double Dist() => Math.Sqrt(X * X + Y * Y);
        public double Dist(MyPosition p2) => Math.Sqrt(Pow(X - p2.X) + Pow(Y - p2.Y));
        public double XDist(MyPosition p2) => Math.Abs(X - p2.X);

        public static double Pow(double x) => x * x;

        public Vec2Double CreateVec => new Vec2Double(X, Y);
        public Vec2Float CreateFloatVec => CreateVec.Conv();

        public MyPosition MoveTowards(MyPosition pos, double speed)
        {
            var dist = Dist(pos);
            if (dist < 0.1) return pos;
            var dx = (pos.X - X) / dist * speed;
            var dy = (pos.Y - Y) / dist * speed;
            return new MyPosition(X + dx, Y + dy);
        }

        public MyPosition MoveTowards(double angle, double speed)
        {
            var dx = Math.Cos(angle);
            var dy = Math.Sin(angle);
            return new MyPosition(dx * speed + X, dy * speed + Y);
        }

        public MyPosition Clone => new MyPosition(X, Y);

        public void UpdateFrom(MyPosition p)
        {
            X = p.X;
            Y = p.Y;
        }
    }
}
