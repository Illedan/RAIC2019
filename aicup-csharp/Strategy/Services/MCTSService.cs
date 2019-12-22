using System;
using System.Collections.Generic;
using System.Linq;
using aicup2019.Strategy.Sim;
using AiCup2019.Model;

namespace aicup2019.Strategy.Services
{
    public static class MCTSService
    {
        public static void Search(SimGame game)
        {
            while (!Const.IsDone())
            {
                DoOneRound(game);
            }
        }

        public static void DoOneRound(SimGame game, bool draw = false)
        {
            for(var i = 0; i < Const.Depth; i++)
            {
                SimService.Simulate(game, i, draw);
            }

            foreach(var unit in game.Units)
            {
                unit.AfterRound();
            }

            game.Reset();
        }
    }
}
