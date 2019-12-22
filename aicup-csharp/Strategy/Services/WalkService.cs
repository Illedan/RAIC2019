using System.Linq;
using System;
using AiCup2019.Model;
using aicup2019.Strategy.Sim;

namespace aicup2019.Strategy.Services
{
    public static class WalkService
    {
        public static MyPosition GetRealTarget (SimGame game, SimUnit unit)
        {
            //if (game.Game.CurrentTick > 1000) return Attack(game);
            if (!unit.HasWeapon) return GetWeapon(game.game, unit);
            var weaps = game.game.Weapons.Where(w => (w.Item as Item.Weapon).WeaponType == WeaponType.AssaultRifle).ToList();
            if (unit.Weapon.Typ != AiCup2019.Model.WeaponType.AssaultRifle && weaps.Any(w => unit.Position.Dist(new MyPosition(w.Position)) < 4))
                return new MyPosition(weaps.First(w => unit.Position.Dist(new MyPosition(w.Position)) < 4).Position);
            if (unit.NeedsHealing && game.game.HasHealing) return GetHealing(game.game, unit);
            //return new MyPosition(game.Enemy.Center.MoveTowards(me.Center, 3).X, game.Height-2);
            if (unit.Weapon.FireTimer > 0.2 && unit.Position.Dist(unit.TargetEnemy.Position) < 3) return Hide(game, unit);
           //LogService.WriteLine("Diff: " + game.ScoreDiff + " Tick: " + game.Game.CurrentTick + " " + game.Width + " " + game.Height);
           if (unit.Position.Dist(unit.TargetEnemy.Position) < 4 && Math.Abs(unit.Position.Y-unit.TargetEnemy.Position.Y) < 1) return Attack(game.game, unit);
           if (unit.Player.ScoreDiff > 0) return Hide(game, unit);
           if (unit.Player.ScoreDiff == 0 && game.game.Game.CurrentTick < 300 && unit.TargetEnemy.HasWeapon) return Hide(game, unit);
           return Attack(game.game, unit);
        }

        public static MyPosition FindWalkTarget(SimGame game, SimUnit unit)
        {
            var target = GetRealTarget(game, unit);
            for (var y = (int)target.Y; y < game.game.Height; y++)
            {
                var p = new MyPosition(target.X, y);
                var d = DistService.GetDist(p, unit.Position);
                if (d < game.game.Width * game.game.Height * 4)
                {
                    target = p;
                    break;
                }
            }

           //LogService.DrawLine(target, unit.Position, 1, 0, 0);
            return target;
        }

        private static MyPosition GetWeapon(MyGame game, SimUnit unit)
        {
            LogService.WriteLine("WEAPON");
            return new MyPosition(game.Weapons.OrderBy(p => DistService.GetDist(new MyPosition(p.Position), unit.Position)).First().Position);
        }

        private static MyPosition GetHealing(MyGame game, SimUnit unit)
        {
            LogService.WriteLine("HEAL");
            var target = game.HealthPacks.OrderBy(p => DistService.GetDist(p, unit.Position)).FirstOrDefault(h => DistService.GetDist(h, unit.Position) < DistService.GetDist(h, unit.TargetEnemy.Position));
            if (target == null) target = game.HealthPacks.OrderBy(p => DistService.GetDist(p, unit.Position)).First();
            return target;
        }

        private static MyPosition Hide(SimGame game, SimUnit unit)
        {
            LogService.WriteLine("HIDE");
            var heights = game.game.GetHideouts();
            return heights.OrderByDescending(p => DistService.GetDist(p, unit.TargetEnemy.Position) - DistService.GetDist(unit.Position,p)*0.5).FirstOrDefault() ?? unit.TargetEnemy.Position;
        }

        private static MyPosition Attack(MyGame game, SimUnit unit)
        {
            LogService.WriteLine("ATTACK");
            var diff = 10;
            if (game.Game.CurrentTick > 3000 && unit.Player.ScoreDiff <= 0) diff = 3;
            var target= unit.TargetEnemy.Position.MoveTowards(unit.Position, diff);
            if (target.X >= game.Width || target.X < 0) diff *= -1;
            target = unit.TargetEnemy.Position.MoveTowards(unit.Position, diff);
            return new MyPosition(target.X, Math.Min(target.Y+20, game.Height - 2));
        }
    }
}
