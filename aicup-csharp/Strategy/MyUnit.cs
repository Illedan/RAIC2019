using System;
using AiCup2019.Model;
namespace aicup2019.Strategy
{
    public class MyUnit
    {
        public MyUnit(Unit unit)
        {
            Unit = unit;
            Size = Rect.FromUnit(unit);
        }

        public MyPosition Center => new MyPosition(Unit.Position.X, Unit.Position.Y + Unit.Size.Y / 2);
        public MyPosition Top => new MyPosition(Unit.Position.X, Unit.Position.Y + Unit.Size.Y);
        public MyPosition Bottom => new MyPosition(Unit.Position.X, Unit.Position.Y);
        public Unit Unit { get; }
        public Rect Size;
    }
}
