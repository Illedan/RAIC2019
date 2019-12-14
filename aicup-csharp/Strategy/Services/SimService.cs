using System.Collections.Generic;
using aicup2019.Strategy.Sim;
namespace aicup2019.Strategy.Services
{
    public static class SimService
    {
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
