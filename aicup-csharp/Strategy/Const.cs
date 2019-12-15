using System;
using AiCup2019.Model;

namespace aicup2019.Strategy
{
    public static class Const
    {
        public static Properties Properties;
        public static int Evals, Sims;
        public static int Steps = 5, Depth = 31;
        public static double Time;
        

        public static void Reset(Properties properties)
        {
            Properties = properties;
            Time = 1 / Const.Properties.TicksPerSecond / 5;
        }
    }
}
