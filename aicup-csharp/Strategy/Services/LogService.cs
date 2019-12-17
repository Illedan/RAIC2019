using System;
using AiCup2019.Model;
using static AiCup2019.Model.CustomData;

namespace aicup2019.Strategy.Services
{
    public static class LogService
    {
        public static bool m_debug = false;
        public static void WriteLine(this string line)
        {
            if (!m_debug) return;
            MyStrategy.Debug.Draw(new Log(line));
        }

        public static void DrawLine(MyPosition p1, MyPosition p2, float r, float g, float b)
        {
            if (!m_debug) return;
            MyStrategy.Debug.Draw(new Line(p1.CreateFloatVec, p2.CreateFloatVec, 0.1f, new ColorFloat(r, g, b, 1)));
        }

        public static void DrawLineBetweenCenter(MyPosition p1, MyPosition p2, float r, float g, float b)
        {
            if (!m_debug) return;
            p1 = new MyPosition(p1.X + 0.5, p1.Y + 0.5);
            p2 = new MyPosition(p2.X + 0.5, p2.Y + 0.5);
            MyStrategy.Debug.Draw(new Line(p1.CreateFloatVec, p2.CreateFloatVec, 0.1f, new ColorFloat(r, g, b, 1)));
        }

        public static void DrawSquare(MyPosition position, double width, double height, float r, float g, float b)
        {
            if (!m_debug) return;
            var x1 = position.X - width / 2;
            var x2 = position.X + width / 2;
            var y1 = position.Y - height /2;
            var y2 = position.Y + height / 2;
            DrawLine(new MyPosition(x1, y1), new MyPosition(x2, y1), r, g, b);
            DrawLine(new MyPosition(x2, y1), new MyPosition(x2, y2), r, g, b);
            DrawLine(new MyPosition(x2, y2), new MyPosition(x1, y2), r, g, b);
            DrawLine(new MyPosition(x1, y2), new MyPosition(x1, y1), r, g, b);

        }
    }
}
