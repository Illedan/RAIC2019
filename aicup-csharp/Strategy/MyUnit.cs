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

        public Unit Unit { get; }
        public Rect Size;
    }
}
