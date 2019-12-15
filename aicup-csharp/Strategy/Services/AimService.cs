using System;
namespace aicup2019.Strategy.Services
{
    public static class AimService
    {
        public static MyPosition GetAimTarget(MyGame game)
        {
            if (!game.Me.HasWeapon) return game.Enemy.Center;
            var dist = game.Me.Center.Dist(game.Enemy.Center);
            var requested = game.Enemy.Center;
            //if (dist < 7) requested = game.Enemy.Center;
            //requested = game.Enemy.GetEndPos(game);
            var angle = Math.Atan2(requested.Y - game.Me.Center.Y, requested.X - game.Me.Center.X);
            var prevAngle = game.Me.Unit.Weapon.Value.LastAngle.HasValue ? game.Me.Unit.Weapon.Value.LastAngle.Value : angle;
            if (Math.Abs(angle - prevAngle) < 0.20) angle = prevAngle;
            var dx = Math.Cos(angle)*dist;
            var dy = Math.Sin(angle)*dist;
            return new MyPosition(game.Me.Center.X + dx, game.Me.Center.Y + dy);
        }

        public static MyPosition[] GetSpread(MyGame game, MyPosition aim)
        {
            if (!game.Me.HasWeapon) return new MyPosition[0];
            var me = game.Me;
            var dist = aim.Dist(me.Center);
            var angle = Math.Atan2(aim.Y - me.Center.Y, aim.X - me.Center.X);
            var max = angle + game.Me.Weapon.Spread;
            var min = angle - game.Me.Weapon.Spread;
            return new MyPosition[] { me.Center.MoveTowards(max, 20), me.Center.MoveTowards(min, 20) };
        }
    }
}
