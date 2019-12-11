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

        public static void DrawLine(MyPosition p1, MyPosition p2, int r, int g, int b)
        {
            MyStrategy.Debug.Draw(new Line(p1.CreateFloatVec, p2.CreateFloatVec, 0.1f, new ColorFloat(r, g, b, 1)));
        }
    }
}
