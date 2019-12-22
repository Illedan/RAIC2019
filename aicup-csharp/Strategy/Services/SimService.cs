using System;
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
            double d = 0.0;
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

                    if (timer-- <= 0) timer = Const.Steps-1;
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
                d +=  DistService.GetDist(target.Position, Target) + (target.Position.XDist(Target)*5);

                if (Draw)
                {
                    target.Draw(hp != target.Health);
                }
            }
            return -d + score;
        }


        public static void Simulate(SimGame game, int depth, bool draw = false)
        {
            foreach(var unit in game.Units)
            {
                unit.GetNextMove(depth);
            }

            var dpm = depth == 0 ? 2 : (depth == 1) ? 3 : 5;
            var steps = Const.Steps * dpm;
            Const.Sims += Const.DepthPerMove;
            var timer = 0;
            for (var s = 0; s < steps; s++)
            {
                foreach (var u in game.Units)
                {
                    u.ApplyAction(u.CurrentAction, game, Const.Time, timer == 0);
                }

                foreach (var b in game.Bullets)
                {
                    b.Move(game, Const.Time);
                }

                if (timer-- <= 0) timer = Const.Steps;
            }

            foreach(var u in game.Units)
            {
                if (draw)
                {
                    u.Draw(u.Health < u._health);
                }
                u.Score -= DistService.GetDist(u.Position, u.WalkTarget) + (u.Position.XDist(u.WalkTarget) * 5);
                if (!u.HasWeapon || u.Health <= 0)
                {
                    continue;
                }

                foreach(var u2 in game.Units)
                {
                    if (u == u2 || u2.Health <= 0) continue;
                    if (u.TeamId == u2.TeamId)
                    {
                        if (Math.Abs(u.Position.X - u2.Position.X) < 5 && Math.Abs(u.Position.Y - u2.Position.Y) < 6)
                        {
                            u.Score -= 1000;
                        }
                    }
                    else if (Math.Abs(u.Position.X - u2.Position.X) < 3 && Math.Abs(u.Position.Y - u2.Position.Y) < 4)
                    {
                        u.Score -= 100;
                    }
                }
            }
        }
    }
}
