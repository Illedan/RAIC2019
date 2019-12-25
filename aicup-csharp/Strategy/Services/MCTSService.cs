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
                if (Const.Evals < 20) Const.Depth = 5;
                else if (Const.Evals < 40) Const.Depth = 6;
                else if (Const.Evals < 80) Const.Depth = 7;
                DoOneRound(game);
            }
        }

        public static void DoOneRound(SimGame game, bool draw = false)
        {
            Const.Evals++;
            for(var i = 0; i < Const.Depth; i++)
            {
                SimService.Simulate(game, i, draw);
                if (draw)
                {
                    foreach(var b in game.Bullets)
                    {
                        if (b.IsDead || !b.IsSimCreated) continue;
                        b.Draw();
                    }
                }
            }

            foreach(var unit in game.Units)
            {
                unit.AfterRound();
            }

            game.Reset();
        }
    }
}
