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
            if (!me.HasWeapon) return GetWeapon(game);
            var weaps = game.Weapons.Where(w => (w.Item as Item.Weapon).WeaponType == WeaponType.RocketLauncher).ToList();
            if (me.Weapon.Typ != AiCup2019.Model.WeaponType.RocketLauncher && weaps.Any(w => me.Center.Dist(new MyPosition(w.Position)) < 4)) return new MyPosition(weaps.First(w => me.Center.Dist(new MyPosition(w.Position)) < 4).Position);
            if (me.ShouldHeal && game.HasHealing) return GetHealing(game);
            return new MyPosition(game.Enemy.Center.MoveTowards(me.Center, 3).X, 50);
           //if (me.Weapon.FireTimer > 0.2 && game.Me.Center.Dist(game.Enemy.Center) < 3) return Hide(game);
           //LogService.WriteLine("Diff: " + game.ScoreDiff + " Tick: " + game.Game.CurrentTick + " " + game.Width + " " + game.Height);
           //if(game.TargetDist < 4 && Math.Abs(game.Me.Center.Y-game.Enemy.Center.Y) < 1) return Attack(game);
           //if (game.ScoreDiff > 0) return Hide(game);
           /// if (game.ScoreDiff == 0 && game.Game.CurrentTick < 300 && game.Enemy.HasWeapon) return Hide(game);
           //return Attack(game);
        }

        public static MyPosition FindWalkTarget(MyGame game)
        {
            var target = GetRealTarget(game);
            LogService.DrawLine(target, game.Me.Bottom, 1, 0, 0);
            if ((int)target.X == (int)game.Me.Center.X) return target;
            if (game.GetTile(target.X - 1, target.Y) == AiCup2019.Model.Tile.JumpPad) target = new MyPosition(target.X + 0.5, target.Y);
            else if (game.GetTile(target.X + 1, target.Y) == AiCup2019.Model.Tile.JumpPad) target = new MyPosition(target.X - 0.5, target.Y);
            var diff = game.Me.Center.X < target.X ? 1 : -1;
            var height = game.GetHeights()[((int)game.Me.Center.X) + diff];
            if (height > game.Me.Bottom.Y) return new MyPosition(target.X, 50);
            return target;
        }

        private static MyPosition GetWeapon(MyGame game)
        {
            LogService.WriteLine("WEAPON");
            return new MyPosition(game.Weapons.OrderBy(p => new MyPosition(p.Position).Dist(game.Me.Center)).First().Position);
        }

        private static MyPosition GetHealing(MyGame game)
        {
            LogService.WriteLine("HEAL");
            var target = game.HealthPacks.OrderBy(p => p.Dist(game.Me.Center)).FirstOrDefault(h => h.Dist(game.Me.Center) < h.Dist(game.Enemy.Center));
            if (target == null) target = game.HealthPacks.OrderBy(p => p.Dist(game.Me.Center)).First();
            return target;
        }

        private static MyPosition Hide(MyGame game)
        {
            LogService.WriteLine("HIDE");
            var heights = game.GetHeights();
            int xx = 0;
            var heightPositions = heights.Select(h => new MyPosition(xx++, h)).ToList();
            var target = heightPositions.Where(h => h.Y > heights.Min()+1).OrderByDescending(p => p.Dist(game.Enemy.Center) - game.Me.Center.Dist(p) / 2).FirstOrDefault();
            if (target == null) return heightPositions.OrderByDescending(p => p.Dist(game.Enemy.Center) - game.Me.Center.Dist(p) / 2).First();
            return target;
        }

        private static MyPosition Attack(MyGame game)
        {
            LogService.WriteLine("ATTACK");
            var diff = 10;
            if (game.Game.CurrentTick > 1000 && game.MePlayer.Score == 0) diff = 0;
            var target = new MyPosition(game.Enemy.Center.X + game.XDiff * -diff, game.Me.Center.Y);
            if(target.X > game.Width || target.Y < 0) return new MyPosition(game.Enemy.Center.X + game.XDiff * diff, game.Me.Center.Y+50);
            return target;
        }
    }
}
