﻿using System;
namespace aicup2019.Strategy.Services
{
    public static class AimService
    {
        public static MyPosition GetAimTarget(MyGame game)
        {
            // Om eg ikke kan sikt rett på. Sikt på nærmeste punkt eg kan skyt han fra?
            // Med mindre eg er på vei mot han? For da kan eg 
            if (!game.Me.HasWeapon) return game.Enemy.Center;
            var dist = game.Me.Center.Dist(game.Enemy.Center);
            var requested = game.Me.Weapon.Typ == AiCup2019.Model.WeaponType.RocketLauncher ? GetClosestGround(game) : game.Enemy.Center;
            if (dist < 3 || Math.Abs(game.Enemy.Center.Y-game.Me.Center.Y) < 0.1) requested = game.Enemy.Center;
            //requested = game.Enemy.GetEndPos(game);
            var angle = Math.Atan2(requested.Y - game.Me.Center.Y, requested.X - game.Me.Center.X);
            var prevAngle = game.Me.Unit.Weapon.Value.LastAngle.HasValue ? game.Me.Unit.Weapon.Value.LastAngle.Value : angle;
            if (Math.Abs(angle - prevAngle) < 0.01 || game.Me.Weapon.FireTimer > 0 && Math.Abs(angle - prevAngle) < 0.0) angle = prevAngle;
           //else if(!ShootService.CanShoot(game.Me.Center,game.Enemy.Center, game, game.Me.Weapon.Parameters.Bullet.Speed))
           //{
           //    angle = prevAngle;
           //}
            var dx = Math.Cos(angle)*dist;
            var dy = Math.Sin(angle)*dist;
            return new MyPosition(game.Me.Center.X + dx, game.Me.Center.Y + dy);
        }

        public static MyPosition GetClosestGround(MyGame game) 
        {
            var heights = game.GetHeights();
            var enemy = game.Enemy.Center;
            var d = 100000.0;
            var best = game.Enemy.Center;
            for(var i = 0; i < heights.Length; i++)
            {
                var p = new MyPosition(i, heights[i]);
                var dd = Math.Abs(p.Y - enemy.Y) + Math.Abs(p.X - enemy.X)*5;
                if (dd < d)
                {
                    best = p;
                    d = dd;
                }
            }
            return best;
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
