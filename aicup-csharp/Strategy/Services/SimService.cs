using System;
using aicup2019.Strategy.Sim;
namespace aicup2019.Strategy.Services
{
    public static class SimService
    {
        public static void Simulate(SimGame game, int depth, bool draw = false)
        {
            foreach(var unit in game.Units)
            {
                unit.GetNextMove(depth);
            }

            var steps = Const.Steps * Const.DepthPerMove;
            Const.Sims += Const.DepthPerMove;
            var timer = 0;
            for (var s = 0; s < steps; s++)
            {
                foreach (var u in game.Units)
                {
                    u.ApplyAction(u.CurrentAction, game, Const.Time, timer == 0);
                }

                if(timer == 3)
                    foreach (var b in game.Bullets)
                    {
                        b.Move(game, Const.Time * Const.Steps);
                    }

                if (timer-- <= 0) timer = Const.Steps;
            }

            foreach(var u in game.Units)
            {
                if (draw)
                {
                    u.Draw(u.Health < u._health);
                }
                if (u.DidTouch)
                {
                    u.Score += DistService.GetDist(u.Position, u.WalkTarget);
                }
                else
                {
                    var d = DistService.GetDist(u.Position, u.WalkTarget) + Math.Abs(u.Position.X - u.WalkTarget.X)*0.5;
                    u.Score -= d;
                    if (d < 0.6) u.DidTouch = true;
                }
                if (!u.HasWeapon)
                {
                    continue;
                }

                foreach(var u2 in game.Units)
                {
                    if (u == u2) continue;
                    if (u.TeamId == u2.TeamId)
                    {
                        if (Math.Abs(u.Position.X - u2.Position.X) < 3 && Math.Abs(u.Position.Y - u2.Position.Y) < 4)
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
