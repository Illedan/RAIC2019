using System;
using System.Collections.Generic;

namespace aicup2019.Strategy
{
    public class UnitAction
    {
        public bool JumpUp, JumpDown;
        public int Dx;
        public double GetSpeed => Const.Properties.UnitMaxHorizontalSpeed * Dx;

        public static List<UnitAction> Actions = new List<UnitAction>
        {
            new UnitAction{ JumpUp = false, JumpDown = false, Dx = -1},
            new UnitAction{ JumpUp = false, JumpDown = false, Dx = 1},
            new UnitAction{ JumpUp = true, JumpDown = false, Dx = 1},
            new UnitAction{ JumpUp = true, JumpDown = false, Dx = -1},
            new UnitAction{ JumpUp = false, JumpDown = true, Dx = -1},
            new UnitAction{ JumpUp = false, JumpDown = true, Dx = 1},
        };

        public static UnitAction DoNothing = new UnitAction { JumpUp = false, JumpDown = false, Dx = 0 };
    }
}
