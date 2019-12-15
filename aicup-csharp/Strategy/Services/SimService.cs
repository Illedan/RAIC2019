using System;
using System.Collections.Generic;
using aicup2019.Strategy.Sim;
namespace aicup2019.Strategy.Services
{
    public static class SimService
    {
        private static MyPosition Target;
        public static double ScoreDir(SimGame game, MyAction[] action, MyPosition targetPos, SimUnit mySimUnit, bool draw = false)
        {
            Target = targetPos;
            Const.Evals++; 
            var score = Simulate(game, action, mySimUnit, draw);
            return mySimUnit.Health * 100000 + score;
        }

        public static double Simulate(SimGame game, MyAction[] moves, SimUnit target, bool Draw = false)
        {
            var steps = Const.Steps * Const.DepthPerMove;
            var hp = target.Health;
            double d = DistService.GetDist(target.Position, Target) ;
            for (var i = 0; i < moves.Length; i++)
            {
                var move = moves[i];
                Const.Sims += Const.DepthPerMove;
                for (var s = 0; s < steps; s++)
                {
                    foreach(var u in game.Units)
                    {
                        u.ApplyAction(u == target ? move : MyAction.DoNothing, game, Const.Time);
                    }

                    foreach(var b in game.Bullets)
                    {
                        b.Move(game, Const.Time);
                    }
                }
                d = Math.Min(d, DistService.GetDist(target.Position, Target) + (i+1)*0.1);

                if (Draw)
                {
                    target.Draw(hp != target.Health);
                }
            }
            return -d;
        }
    }
}
