using System.Linq;
using System;
using AiCup2019.Model;

namespace aicup2019.Strategy.Services
{
    public static class WalkService
    {
        public static MyPosition GetRealTarget (MyGame game)
        {
            var me = game.Me;
            //if (game.Game.CurrentTick > 1000) return Attack(game);
            if (!me.HasWeapon) return GetWeapon(game);
            var weaps = game.Weapons.Where(w => (w.Item as Item.Weapon).WeaponType == WeaponType.RocketLauncher).ToList();
            if (me.Weapon.Typ != AiCup2019.Model.WeaponType.RocketLauncher && weaps.Any(w => me.Center.Dist(new MyPosition(w.Position)) < 4)) return new MyPosition(weaps.First(w => me.Center.Dist(new MyPosition(w.Position)) < 4).Position);
            if (me.ShouldHeal && game.HasHealing) return GetHealing(game);
            //return new MyPosition(game.Enemy.Center.MoveTowards(me.Center, 3).X, game.Height-2);
            if (me.Weapon.FireTimer > 0.2 && game.Me.Center.Dist(game.Enemy.Center) < 3) return Hide(game);
           //LogService.WriteLine("Diff: " + game.ScoreDiff + " Tick: " + game.Game.CurrentTick + " " + game.Width + " " + game.Height);
           if(game.TargetDist < 4 && Math.Abs(game.Me.Center.Y-game.Enemy.Center.Y) < 1) return Attack(game);
           if (game.ScoreDiff > 0) return Hide(game);
           // if (game.ScoreDiff == 0 && game.Game.CurrentTick < 300 && game.Enemy.HasWeapon) return Hide(game);
           return Attack(game);
        }

        public static MyPosition FindWalkTarget(MyGame game)
        {
            var target = GetRealTarget(game);
            LogService.DrawLineBetweenCenter(target, game.Me.Bottom, 1, 1, 1);
            for (var y = (int)target.Y; y < game.Height; y++)
            {
                var p = new MyPosition(target.X, y);
                if(DistService.GetDist(p, game.Me.Center) < game.Width * game.Height * 4)
                {
                    target = p;
                    break;
                }
            }

            LogService.DrawLine(target, game.Me.Bottom, 1, 0, 0);
            return target;
        }

        private static MyPosition GetWeapon(MyGame game)
        {
            LogService.WriteLine("WEAPON");
            return new MyPosition(game.Weapons.OrderBy(p => DistService.GetDist(new MyPosition(p.Position), game.Me.Center)).First().Position);
        }

        private static MyPosition GetHealing(MyGame game)
        {
            LogService.WriteLine("HEAL");
            var target = game.HealthPacks.OrderBy(p => DistService.GetDist(p,game.Me.Center)).FirstOrDefault(h => DistService.GetDist(h,game.Me.Center) < h.Dist(game.Enemy.Center));
            if (target == null) target = game.HealthPacks.OrderBy(p => DistService.GetDist(p,game.Me.Center)).First();
            return target;
        }

        private static MyPosition Hide(MyGame game)
        {
            LogService.WriteLine("HIDE");
            var heights = game.GetHideouts();

            return heights.OrderByDescending(p => -DistService.GetDist(p,game.Enemy.Center) - DistService.GetDist(game.Me.Center,p)*0.5).First();
        }

        private static MyPosition Attack(MyGame game)
        {
            var diff = 5;
            if (game.Game.CurrentTick > 1000 && game.ScoreDiff <= 0) diff = 0;
            return game.Enemy.Center.MoveTowards(game.Me.Center, diff);
           //LogService.WriteLine("ATTACK");
           //var diff = 10;
           //if (game.Game.CurrentTick > 1000 && game.ScoreDiff < 0) diff = 0;
           //var target = new MyPosition(game.Enemy.Center.X + game.XDiff * -diff, game.Me.Center.Y);
           //if(target.X > game.Width || target.Y < 0) return new MyPosition(game.Enemy.Center.X + game.XDiff * diff, game.Me.Center.Y+50);
           //return target;
        }
    }
}
