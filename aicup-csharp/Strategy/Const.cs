using System;
using AiCup2019.Model;
using System.Diagnostics;

namespace aicup2019.Strategy
{
    public static class Const
    {
        public static Properties Properties;
        public static int Evals, Sims;
        public static int Steps = 5, Depth = 7, DepthPerMove = 5;
        public static double Time;
        public static Stopwatch Stopwatch;
        public static Random rnd = new Random();

        public static void Reset(Properties properties)
        {
            Evals = Sims = 0;
            Properties = properties;
            Time = 1.0 / Properties.TicksPerSecond / Steps;
            Stopwatch = Stopwatch.StartNew();
            m_isDone = false;
            m_count = 0;
        }

        private static bool m_isDone;
        public static long m_timeout = 10;
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
