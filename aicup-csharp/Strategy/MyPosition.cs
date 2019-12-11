using System;
using AiCup2019.Model;

namespace aicup2019.Strategy
{
    public struct MyPosition
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

        public double Dist(MyPosition p2) => Math.Sqrt(Pow(X - p2.X) + Pow(Y - p2.Y));

        public double Pow(double x) => x * x;
    }
}
