using System;
using aicup2019.Strategy.Sim;
using System.Linq;

namespace aicup2019.Strategy.Services
{
    public static class AimService
    {
        public static MyPosition GetAimTarget(MyGame game, SimUnit unit)
        {
            var pos = unit.Position;
            var targetPos = unit.TargetEnemy.Position;
            if (!unit.HasWeapon) return targetPos;
            var dist = pos.Dist(targetPos);
            var requested = unit.WeaponType == AiCup2019.Model.WeaponType.RocketLauncher ? GetClosestGround(game, targetPos) : targetPos;
            if (dist < 3 || Math.Abs(targetPos.Y-pos.Y) < 0.1) requested = targetPos;

            var angle = Math.Atan2(requested.Y - pos.Y, requested.X - pos.X);
            var prevAngle = unit.AimAngle;
            if (Math.Abs(angle - prevAngle) < 0.05 || unit.FireTimer > 0 && Math.Abs(angle - prevAngle) < 0.1) angle = prevAngle;
            var dx = Math.Cos(angle)*dist;
            var dy = Math.Sin(angle)*dist;
            var target = new MyPosition(pos.X + dx, pos.Y + dy);
            //LogService.DrawLine(target, unit.Position, 1, 0, 0);
            return target;
        }

        public static MyPosition GetClosestGround(MyGame game, MyPosition targetPos) 
        {
            //Nærmeste eg kan skyte?
            var walls = game.AllWalls;
            var enemy = targetPos;
            var d = 100000.0;
            var best = targetPos;
            foreach(var wall in walls)
            {
                var dd = Math.Abs(wall.Y - enemy.Y) + Math.Abs(wall.X - enemy.X)*2;
                if (wall.Y > enemy.Y) dd += 3;
                if (dd < d)
                {
                    best = wall;
                    d = dd;
                }
            }
            return best;
        }

        public static MyPosition[] GetSpread(SimUnit unit)
        {
            var aim = unit.AimTarget;
            var pos = unit.Position;
            if (!unit.HasWeapon) return new MyPosition[0];
            var angle = Math.Atan2(aim.Y - pos.Y, aim.X - pos.X);
            var max = angle + unit.Spread;
            var min = angle - unit.Spread;
            return new MyPosition[] { pos.MoveTowards(max, 20), pos.MoveTowards(min, 20) };
        }
    }
}
