using AiCup2019.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using aicup2019.Strategy.Sim;
namespace aicup2019.Strategy.Services
{
    public static class ShootService
    {
        public static bool[,] canShootMap;
        public static bool m_isRun;

        public static void Initialize(SimGame game)
        {
            if (m_isRun) return;
            m_isRun = true;
            var max = Const.Width * Const.Height;
            canShootMap = new bool[max, max];

        }


        public static bool ShouldShoot(SimGame game, SimUnit unit)
        {
            if (!unit.HasWeapon || unit.FireTimer > 0) return false;
            LogService.WriteLine("FireTimer: " + unit.FireTimer);
            //if (me.Unit.Weapon.Value.Spread > me.Unit.Weapon.Value.Parameters.MinSpread + 0.1 && me.Center.Dist(aimPos) > 5) return false;
            if (!CanShoot(unit.Position, unit.AimTarget, game, unit, unit.Weapon.Parameters.Bullet.Speed)) return false;
            return true;
        }

        public static bool CanShootAt(MyPosition pos, MyPosition target)
        {
            return true;
           //var y =(int)target.Y;
           //var y2 = (int)(target.Y + Const.HalfUnitHeight);
           //if (y == y2) return canShootMap[Const.GetPos((int)pos.X, (int)pos.Y), Const.GetPos((int)target.X, y)];
		   //
           //return canShootMap[Const.GetPos((int)pos.X, (int)pos.Y), Const.GetPos((int)target.X, y)] ||
           //    canShootMap[Const.GetPos((int)pos.X, (int)pos.Y), Const.GetPos((int)target.X, y2)];
        }

        public static double GetShootTime(double dist, double speed)
        {
            return dist / speed;
        }

        public static double GetShootTime(MyPosition pos, MyPosition end, double bulletSpeed)
        {
            var dist = end.Dist(pos);
            return GetShootTime(dist, bulletSpeed);
        }

        public static bool CanShoot(MyPosition startPos, MyPosition endPos, SimGame game, SimUnit unit, double bulletSpeed)
        {
            var hitPos = GetHitPos(startPos, endPos, game, bulletSpeed, unit);
            if(unit.WeaponType == WeaponType.RocketLauncher)
            {
                var spread = AimService.GetSpread(unit);
                var posses = spread.Select(s => GetHitPos(startPos, s, game, bulletSpeed, unit)).ToArray();
                foreach(var p in posses)
                {
                    LogService.DrawLine(p, unit.Position, 0, 0, 1);
                }

                if (posses.Any(p => p.Dist(unit.TargetEnemy.Position) > p.Dist(unit.Position) && p.Dist(endPos) > unit.Weapon.Parameters.Explosion.Value.Radius-1))
                    return false;

                if (unit.TargetEnemy.Position.Dist(endPos) - unit.Weapon.Parameters.Explosion.Value.Radius > unit.Position.Dist(endPos)) return false;
            }

            return hitPos.Dist(endPos) < 1;
        }

        public static MyPosition GetHitPos(MyPosition startPos, MyPosition endPos, SimGame game, double bulletSpeed, SimUnit firering, bool stopOnEnd = true)
        {
            var dist = endPos.Dist(startPos);
            var time = GetShootTime(dist, bulletSpeed) * Const.Properties.TicksPerSecond * 100;
            var dx = (endPos.X - startPos.X) / time;
            var dy = (endPos.Y - startPos.Y) / time;
            var x = startPos.X;
            var y = startPos.Y;
            var d = startPos.Dist(endPos);
            for (var i = 0; i < time*2; i++)
            {
                x += dx;
                y += dy;
                if (!game.game.OnBoard(x, y)) return new MyPosition(x, y);
                var tile = game.GetTileD(x, y);
                if (tile == Tile.Wall) return new MyPosition(x, y);
                tile = game.GetTileD(x- firering.Weapon.Parameters.Bullet.Size*0.5, y - firering.Weapon.Parameters.Bullet.Size * 0.5);
                if (tile == Tile.Wall) return new MyPosition(x, y);
                tile = game.GetTileD(x + firering.Weapon.Parameters.Bullet.Size * 0.5, y - firering.Weapon.Parameters.Bullet.Size * 0.5);
                if (tile == Tile.Wall) return new MyPosition(x, y);
                var nextD = Math.Sqrt(MyPosition.Pow(x - endPos.X) + MyPosition.Pow(y - endPos.Y));
                if (nextD > d && stopOnEnd || nextD < 0.3) return endPos;
                d = nextD;
                foreach(var u in game.Units)
                {
                    if (u == firering || u.unit.PlayerId != firering.unit.PlayerId) continue;
                    var unit = u.unit;
                    if (!(Math.Abs(x - unit.Position.X) > firering.Weapon.Parameters.Bullet.Size/2 + unit.Size.X/2
                            || Math.Abs(y - unit.Position.Y) > firering.Weapon.Parameters.Bullet.Size / 2 + unit.Size.Y / 2)) return new MyPosition(x, y);
                }
            }

            return endPos;
        }

    }
}
