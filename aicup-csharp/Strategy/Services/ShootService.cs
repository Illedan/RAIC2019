using AiCup2019.Model;
using System;
namespace aicup2019.Strategy.Services
{
    public static class ShootService
    {
        public static double GetShootTime(double dist, double speed)
        {
            return dist / speed;
        }

        public static double GetShootTime(MyPosition pos, MyPosition end, double bulletSpeed)
        {
            var dist = end.Dist(pos);
            return GetShootTime(dist, bulletSpeed);
        }

        public static bool CanShoot(MyPosition startPos, MyPosition endPos, MyGame game, double bulletSpeed)
        {
            var dist = endPos.Dist(startPos);
            var time = GetShootTime(dist, bulletSpeed) * Const.Properties.TicksPerSecond;
            var dx = (endPos.X - startPos.X) / time;
            var dy = (endPos.Y - startPos.Y) / time;
            var x = startPos.X;
            var y = startPos.Y;
            for(var i = 0; i < time-1; i++)
            {
                x += dx;
                y += dy;
                if (!game.OnBoard(x, y)) return false;
                var tile = game.GetTile(x, y);
                if (tile == Tile.Wall) return false;
            }

            return true;
        }

        public static bool CanPotentiallyShoot(MyUnit me, MyUnit enemy, MyGame game)
        {
            if (!me.Unit.Weapon.HasValue) return false;
            if (me.Unit.Weapon.Value.Spread > 0.1 && me.Center.Dist(enemy.Center) > 5) return false;
            if(me.Unit.Weapon.Value.Typ == WeaponType.RocketLauncher)
            {
                if (!CanShoot(me.Center, enemy.Bottom, game, me.Unit.Weapon.Value.Parameters.Bullet.Speed)) return false;
                return true;
            }

            if (!CanShoot(me.Center, enemy.Center, game, me.Unit.Weapon.Value.Parameters.Bullet.Speed)) return false;
            if (!CanShoot(me.Center, enemy.Top, game, me.Unit.Weapon.Value.Parameters.Bullet.Speed)) return false;
            return true;
        }
    }
}
