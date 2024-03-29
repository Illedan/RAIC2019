﻿using System.Collections.Generic;

namespace aicup2019.Strategy
{
    public class MyAction
    {
        public bool JumpUp, JumpDown;
        public int Dx;
        public static double GetSpeed => Const.Properties.UnitMaxHorizontalSpeed;

        public static List<MyAction> Actions = new List<MyAction>
        {
            new  MyAction{ JumpUp = true, JumpDown = false, Dx = 1},
            new  MyAction{ JumpUp = true, JumpDown = false, Dx = -1},
            new  MyAction{ JumpUp = true, JumpDown = false, Dx = 0},
            new  MyAction{ JumpUp = false, JumpDown = false, Dx = -1},
            new  MyAction{ JumpUp = false, JumpDown = false, Dx = 1},
            new  MyAction{ JumpUp = false, JumpDown = true, Dx = -1},
            new  MyAction{ JumpUp = false, JumpDown = true, Dx = 1},
            new  MyAction{ JumpUp = false, JumpDown = true, Dx = 0},
        };

        public static List<MyAction> Dummy => new List<MyAction> { Actions[4] };
        public static MyAction DoNothing = new MyAction { JumpUp = false, JumpDown = false, Dx = 0 };
    }
}
