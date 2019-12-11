using System;
namespace aicup2019.Strategy.Services
{
    public static class JumpService
    {
        public static int GetDir(MyGame game, MyPosition target)
        {
            var me = game.Me;
            if(game.Me.Bottom.Y < target.Y)
            {
                if (me.Unit.JumpState.MaxTime * me.Unit.JumpState.Speed < 2.5) return 0;
            }

            return game.Me.Bottom.Y < target.Y ? 1 : -1;
        }
    }
}
