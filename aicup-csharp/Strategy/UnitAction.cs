using System;
using System.Collections.Generic;

namespace aicup2019.Strategy
{
    public class MyAction
    {
        public bool JumpUp, JumpDown;
        public int Dx;
        public static double GetSpeed => Const.Properties.UnitMaxHorizontalSpeed * Dx;

        public static List<MyAction> Actions = new List<MyAction>
        {
            new  MyAction{ JumpUp = false, JumpDown = false, Dx = -1},
            new  MyAction{ JumpUp = false, JumpDown = false, Dx = 1},
            new  MyAction{ JumpUp = true, JumpDown = false, Dx = 1},
            new  MyAction{ JumpUp = true, JumpDown = false, Dx = -1},
            new  MyAction{ JumpUp = false, JumpDown = true, Dx = -1},
            new  MyAction{ JumpUp = false, JumpDown = true, Dx = 1},
        };

        public static MyAction DoNothing = new MyAction { JumpUp = false, JumpDown = false, Dx = 0 };
    }
}
