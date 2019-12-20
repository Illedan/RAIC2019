using System;
using AiCup2019.Model;
namespace aicup2019.Strategy.Sim
{
    public class SimMine
    {
        public Mine Mine;
        public MyPosition Position, _position;
        public double Timer, _timer;
        public SimMine(Mine mine)
        {
            Mine = mine;
            Timer = _timer = Mine.Timer ?? 1000.0;
            Position = new MyPosition(mine.Position.X, mine.Position.Y);
            _position = Position.Clone;
        }

        public void Reset()
        {
            Timer = _timer;
            Position.UpdateFrom(_position);
        }

        public void Explode(SimGame game)
        {
            foreach (var unit in game.Units)
            {
                if (Math.Abs(Position.X - unit.Position.X) > Mine.ExplosionParameters.Radius + unit.HalfWidth
                    || Math.Abs(Position.Y - unit.Position.Y) > Mine.ExplosionParameters.Radius + unit.HalffHeight) continue;
                unit.Health -= Mine.ExplosionParameters.Damage;
            }
        }
    }
}
