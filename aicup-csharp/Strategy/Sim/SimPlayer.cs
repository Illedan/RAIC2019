using System;
using System.Collections.Generic;
using AiCup2019.Model;
using System.Linq;

namespace aicup2019.Strategy.Sim
{
    public class SimPlayer
    {
        public Player Player;
        public List<SimUnit> Units = new List<SimUnit>(2);
        public int Score, Id, _score;
        public SimPlayer(Player player, Game game)
        {
            Player = player;
            Id = player.Id;
            Score = _score = player.Score;
            Units.AddRange(game.Units.Where(u => u.PlayerId == Id).Select(u => new SimUnit(u)));
            if (Units.Count == 2)
            {
                Units[0].Allied = Units[1];
                Units[1].Allied = Units[0];
            }
        }

        public void Reset()
        {
            Score = _score;
            foreach (var u in Units) u.Reset();
        }
    }
}
