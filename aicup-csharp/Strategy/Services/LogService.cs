using System;
using AiCup2019.Model;
using static AiCup2019.Model.CustomData;

namespace aicup2019.Strategy.Services
{
    public static class LogService
    {
        public static void WriteLine(this string line)
        {
            MyStrategy.Debug.Draw(new Log(line));
        }

        public static void DrawLine(MyPosition p1, MyPosition p2, float r, float g, float b)
        {
            MyStrategy.Debug.Draw(new Line(p1.CreateFloatVec, p2.CreateFloatVec, 0.1f, new ColorFloat(r, g, b, 1)));
        }

        public static void DrawSquare(MyPosition position, double width, double height, float r, float g, float b)
        {
            var x = position.X - width / 2;
            var y = position.Y - height /2;
            MyStrategy.Debug.Draw(new CustomData.Rect(new MyPosition(x, y).CreateFloatVec, new Vec2Float((float)width, (float)height), new ColorFloat(r, g, b, 1)));
        }
    }
}
