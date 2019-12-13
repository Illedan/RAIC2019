using AiCup2019.Model;
using System;
using System.Collections.Generic;
using System.Linq;
namespace aicup2019.Strategy.Services
{
    public static class ShootService
    {
        public static bool IsDangerous(MyGame game, out MyPosition safe)
        {
            var me = game.Me.Bottom;
            var maxMS = Const.Properties.UnitMaxHorizontalSpeed / 60 * 20;
            var maxHeigth = game.Me.Unit.JumpState.CanJump ? (game.Me.Bottom.Y + Const.Properties.UnitJumpSpeed / 60 * 20) : game.Me.Bottom.Y;
            var posses = new List<MyPosition> { new MyPosition(me.X - maxMS, me.Y), new MyPosition(me.X + maxMS, me.Y), new MyPosition(me.X, maxHeigth), new MyPosition(me.X - maxMS, maxHeigth), new MyPosition(me.X + maxMS, maxHeigth) };
            safe = null;
            if (!IsDangerous(game, game.Me.Bottom)) return false;

            foreach(var p in posses)
            {
                var diff = game.Me.Bottom.GetDx(p);
                if (game.GetTile(me.X + diff, me.Y) == Tile.Wall) continue;
                safe = p;
                if (!IsDangerous(game, p)) return true;
            }

            return false;
        }

        private static bool IsDangerous(MyGame game, MyPosition pos)
        {
            var bullets = game.Bullets.Where(b => b.Bullet.UnitId != game.Me.Unit.Id).ToList();
            var rect = new Rect(pos.X - game.Me.Unit.Size.X * 0.5, pos.Y, pos.X + game.Me.Unit.Size.X * 0.5, pos.Y + game.Me.Unit.Size.Y);
            foreach(var b in bullets)
            {
                var time = GetShootTime(new MyPosition(b.Bullet.Position).Dist(pos), new MyPosition(b.Bullet.Velocity).Dist());
                if (time > 0.3) continue;
                var hitSpot = GetHitPos(new MyPosition(b.Bullet.Position), pos, game, new MyPosition(b.Bullet.Velocity).Dist(), false);
                if (hitSpot.Dist(pos) < 1) return true;
                if (b.Bullet.ExplosionParameters.HasValue && hitSpot.Dist(pos) < b.Bullet.ExplosionParameters.Value.Radius) return false;
            }

            return false;
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

        public static bool CanShoot(MyPosition startPos, MyPosition endPos, MyGame game, double bulletSpeed)
        {
            var hitPos = GetHitPos(startPos, endPos, game, bulletSpeed);
            if(game.Me.Weapon.Typ == WeaponType.RocketLauncher)
            {
                var spread = AimService.GetSpread(game, endPos);
                var posses = spread.Select(s => GetHitPos(startPos, s, game, bulletSpeed)).ToArray();
                foreach(var p in posses)
                {
                    LogService.DrawLine(p, game.Me.Center, 0, 0, 1);
                }

                if (posses.Any(p => p.Dist(startPos) < p.Dist(endPos) && p.Dist(endPos) > game.Me.Weapon.Parameters.Explosion.Value.Radius))
                    return false;
            }

            return hitPos.Dist(endPos) < 1;
        }

        public static MyPosition GetHitPos(MyPosition startPos, MyPosition endPos, MyGame game, double bulletSpeed, bool stopOnEnd = true)
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
            }

            return endPos;
        }

        public static bool ShouldShoot(MyGame game, MyPosition aimPos)
        {
            var me = game.Me;
            if (!me.HasWeapon) return false;
            LogService.WriteLine("FireTimer: " + me.Unit.Weapon.Value.FireTimer);
            //if (me.Unit.Weapon.Value.Spread > me.Unit.Weapon.Value.Parameters.MinSpread + 0.1 && me.Center.Dist(aimPos) > 5) return false;
            if (!CanShoot(me.Center, aimPos, game, me.Unit.Weapon.Value.Parameters.Bullet.Speed)) return false;
            return true;
        }

        public static bool CanPotentiallyShoot(MyUnit me, MyUnit enemy, MyGame game)
        {
            if (!me.HasWeapon) return false;
            if (me.Unit.Weapon.Value.Spread > me.Unit.Weapon.Value.Parameters.MinSpread && me.Center.Dist(enemy.Center) > 3) return false;
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
