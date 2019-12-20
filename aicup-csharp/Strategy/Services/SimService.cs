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
            double d = 0.0; //target.HasWeapon ? DistService.GetDist(target.Position, Target) : target.Position.XDist(Target)*10;
            var score = 0;
            for (var i = 0; i < moves.Length; i++)
            {
                var move = moves[i];
                Const.Sims += Const.DepthPerMove;
                var timer = 0;
                for (var s = 0; s < steps; s++)
                {
                    foreach(var u in game.Units)
                    {
                        u.ApplyAction(u == target ? move : MyAction.DoNothing, game, Const.Time, timer == 0);
                    }

                    foreach(var b in game.Bullets)
                    {
                        b.Move(game, Const.Time);
                    }
                    if (timer-- <= 0) timer = Const.Steps;
                }
                if(target.HasWeapon)
                    foreach(var u in game.Units)
                    {
                        if (u == target) continue;
                        if (u.TeamId == target.TeamId)
                        {
                            if (Math.Abs(u.Position.X - target.Position.X) < 5 && Math.Abs(u.Position.Y - target.Position.Y) < 6)
                            {
                                score -= 1000;
                            }
                        }
                        else if (Math.Abs(u.Position.X - target.Position.X) < 1.5 && Math.Abs(u.Position.Y - target.Position.Y) < 2)
                        {
                            score -= 100;
                        }

                    }
                d +=  DistService.GetDist(target.Position, Target) + (target.Position.XDist(Target)*1);

                if (Draw)
                {
                    target.Draw(hp != target.Health);
                }
            }
            return -d + score;
        }
    }
}
