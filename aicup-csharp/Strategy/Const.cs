using System;
using AiCup2019.Model;
using System.Diagnostics;
using System.Linq;
using aicup2019.Strategy.Sim;

namespace aicup2019.Strategy
{
    public static class Const
    {
        public static Properties Properties;
        public static int Evals, Sims, Width, Height;
        public static int Steps = 5, Depth = 10, DepthPerMove = 5;
        public static double Time;
        public static Stopwatch Stopwatch;
        public static Random rnd = new Random();

        public static int GetPos(int x, int y) => Width * y + x;
        public static double HalfUnitHeight, HalfUnitWidth;
        public static int MyId;

        public static void Reset(Properties properties, Game game)
        {
            HalfUnitWidth = game.Units.First().Size.X / 2;
            HalfUnitHeight = game.Units.First().Size.Y / 2;
            Width = game.Level.Tiles.Length;
            Height = game.Level.Tiles[0].Length;
            Evals = Sims = 0;
            Properties = properties;
            Time = 1.0 / Properties.TicksPerSecond / Steps;
            Stopwatch = Stopwatch.StartNew();
            m_isDone = false;
            m_count = 0;
            BulletFactory.Initialize();
        }

        private static bool m_isDone;
        public static long m_timeout = 15;
        private static int m_count;

        public static bool IsDone()
        {
            if (m_isDone)
            {
                return true;
            }

            if (++m_count > 5)
            {
                var time = GetTime;
                if (time > m_timeout)
                {
                    m_isDone = true;
                }

                m_count = 0;
            }

            return m_isDone;
        }

        public static long GetTime => Stopwatch.ElapsedMilliseconds;
    }
}
