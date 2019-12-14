using System;
using AiCup2019.Model;
namespace aicup2019.Strategy.Sim
{
    public class SimUnit
    {
        private static MyPosition Dummy = new MyPosition(0, 0);
        public static double MaxJumpTime => Const.Properties.UnitJumpTime;
        public static double JumpSpeed => Const.Properties.UnitJumpSpeed;

        public readonly Unit unit;
        public MyPosition Position, _position;
        public int Health, _health;
        public double HalfWidth, HalffHeight, JumpTime, _jumpTime;
        public bool Jumping, JumpingDown, IsDead;


        public SimUnit(double x, double y, Unit unit)
        {
            this.unit = unit;
            Health = _health = unit.Health;
            HalfWidth = unit.Size.X / 2;
            HalffHeight = unit.Size.Y;
            Position = new MyPosition(x, y + HalffHeight);
            _position = Position.Clone;
            JumpTime = _jumpTime = unit.JumpState.MaxTime;
        }

        public void ApplyAction(UnitAction action, SimGame game, double dt)
        {

        }

        public void Reset()
        {
            Position.UpdateFrom(_position);

        }
    }
}
