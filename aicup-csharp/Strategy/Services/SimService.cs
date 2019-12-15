using System.Collections.Generic;
using aicup2019.Strategy.Sim;
namespace aicup2019.Strategy.Services
{
    public static class SimService
    {
        public static double ScoreDir(SimGame game, MyAction action, MyPosition targetPos, SimUnit mySimUnit)
        {
            var actList = new List<MyAction> { action };
            var hp = mySimUnit.Health;
            for (var i = 0; i < Const.Depth; i++)
            {
                if (i % 5 == 0)
                {
                    mySimUnit.Draw(hp != mySimUnit.Health);
                    hp = mySimUnit.Health;
                }
                SimService.Simulate(game, actList, mySimUnit);
            }

            return mySimUnit.Health * 1000 - mySimUnit.Position.Dist(targetPos);
        }

        public static void Simulate(SimGame game, List<MyAction> moves, SimUnit target)
        {
            Const.Sims++;
            for(var i = 0; i < moves.Count; i++)
            {
                for(var s = 0; s < Const.Steps; s++)
                {
                    foreach(var u in game.Units)
                    {
                        u.ApplyAction(u == target ? moves[i] : MyAction.DoNothing, game, Const.Time);
                    }

                    foreach(var b in game.Bullets)
                    {
                        b.Move(game, Const.Time);
                    }
                }
            }
        }
    }
}
