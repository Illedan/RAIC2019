using AiCup2019.Model;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static bool CanShoot(MyPosition startPos, MyPosition endPos, MyGame game, MyUnit firering, double bulletSpeed)
        {
            var hitPos = GetHitPos(startPos, endPos, game, bulletSpeed, firering);
            if(game.Me.Weapon.Typ == WeaponType.RocketLauncher)
            {
                var spread = AimService.GetSpread(game, endPos);
                var posses = spread.Select(s => GetHitPos(startPos, s, game, bulletSpeed, firering)).ToArray();
                foreach(var p in posses)
                {
                    LogService.DrawLine(p, game.Me.Center, 0, 0, 1);
                }

                if (posses.Any(p => p.Dist(startPos) < p.Dist(endPos) && p.Dist(endPos) > game.Me.Weapon.Parameters.Explosion.Value.Radius))
                    return false;
            }

            return hitPos.Dist(endPos) < 1;
        }

        public static MyPosition GetHitPos(MyPosition startPos, MyPosition endPos, MyGame game, double bulletSpeed, MyUnit firering, bool stopOnEnd = true)
        {
            var dist = endPos.Dist(startPos);
            var time = GetShootTime(dist, bulletSpeed) * Const.Properties.TicksPerSecond * 10;
            var dx = (endPos.X - startPos.X) / time;
            var dy = (endPos.Y - startPos.Y) / time;
            var x = startPos.X;
            var y = startPos.Y;
            var d = startPos.Dist(endPos);
            for (var i = 0; i < time*2; i++)
            {
                x += dx;
                y += dy;
                if (!game.OnBoard(x, y)) return new MyPosition(x, y);
                var tile = game.GetTile(x, y);
                if (tile == Tile.Wall) return new MyPosition(x, y);
                tile = game.GetTile(x-game.Me.Weapon.Parameters.Bullet.Size*0.5, y - game.Me.Weapon.Parameters.Bullet.Size * 0.5);
                if (tile == Tile.Wall) return new MyPosition(x, y);
                tile = game.GetTile(x + game.Me.Weapon.Parameters.Bullet.Size * 0.5, y - game.Me.Weapon.Parameters.Bullet.Size * 0.5);
                if (tile == Tile.Wall) return new MyPosition(x, y);
                var nextD = Math.Sqrt(MyPosition.Pow(x - endPos.X) + MyPosition.Pow(y - endPos.Y));
                if (nextD > d && stopOnEnd || nextD < 1) return endPos;
                d = nextD;
                foreach(var u in game.Units)
                {
                    if (u == firering || u.Unit.PlayerId != firering.Unit.PlayerId) continue;
                    var unit = u.Unit;
                    if (!(Math.Abs(x - unit.Position.X) > firering.Weapon.Parameters.Bullet.Size/2 + unit.Size.X/2
                            || Math.Abs(y - unit.Position.Y) > firering.Weapon.Parameters.Bullet.Size / 2 + unit.Size.Y / 2)) return new MyPosition(x, y);
                }
            }

            return endPos;
        }

        public static bool ShouldShoot(MyGame game, MyPosition aimPos)
        {
            var me = game.Me;
            if (!me.HasWeapon) return false;
            LogService.WriteLine("FireTimer: " + me.Unit.Weapon.Value.FireTimer);
            //if (me.Unit.Weapon.Value.Spread > me.Unit.Weapon.Value.Parameters.MinSpread + 0.1 && me.Center.Dist(aimPos) > 5) return false;
            if (!CanShoot(me.Center, aimPos, game, me,me.Unit.Weapon.Value.Parameters.Bullet.Speed)) return false;
            return true;
        }

        public static bool CanPotentiallyShoot(MyUnit me, MyUnit enemy, MyGame game)
        {
            if (!me.HasWeapon) return false;

           //if(me.Unit.Weapon.Value.Typ == WeaponType.RocketLauncher)
           //{
           //    if (!CanShoot(me.Center, enemy.Bottom, game, me,me.Unit.Weapon.Value.Parameters.Bullet.Speed)) return false;
           //    return true;
           //}

            if (!CanShoot(me.Center, enemy.Center, game, me,me.Unit.Weapon.Value.Parameters.Bullet.Speed)) return false;
           // if (!CanShoot(me.Center, enemy.Top, game, me, me.Unit.Weapon.Value.Parameters.Bullet.Speed)) return false;
            return true;
        }
    }
}
