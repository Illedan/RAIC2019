using System.Linq;

namespace aicup2019.Strategy.Services
{
    public static class WalkService
    {
        public static MyPosition GetRealTarget (MyGame game)
        {
            var me = game.Me;
            if (!me.HasWeapon) return GetWeapon(game);
            if (me.ShouldHeal && game.HasHealing) return GetHealing(game);
            LogService.WriteLine("Diff: " + game.ScoreDiff + " Tick: " + game.Game.CurrentTick + " " + game.Width + " " + game.Height);
            if (game.ScoreDiff > 0) return Hide(game);
            if (game.ScoreDiff == 0 && game.Game.CurrentTick < 300 && game.Enemy.HasWeapon) return Hide(game);
            return Attack(game);
        }

        public static MyPosition FindWalkTarget(MyGame game)
        {
            var target = GetRealTarget(game);
            LogService.DrawLine(target, game.Me.Bottom, 1, 0, 0);
            if ((int)target.X == (int)game.Me.Center.X) return target;
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
            if (target == null) return game.HealthPacks.OrderBy(p => p.Dist(game.Me.Center)).First();
            if ((int)target.X == (int)game.Me.Center.X) return target;
            return new MyPosition(target.X, 50);
        }

        private static MyPosition Hide(MyGame game)
        {
            LogService.WriteLine("HIDE");
            var heights = game.GetHeights();
            int xx = 0;
            var heightPositions = heights.Select(h => new MyPosition(xx++, h)).ToList();
            var target = heightPositions.Where(h => h.Y > game.Height / 2).OrderByDescending(p => p.Dist(game.Enemy.Center) - game.Me.Center.Dist(p) / 2).FirstOrDefault();
            if (target == null) return heightPositions.OrderByDescending(p => p.Dist(game.Enemy.Center) - game.Me.Center.Dist(p) / 2).First();
            return target;
        }

        private static MyPosition Attack(MyGame game)
        {
            LogService.WriteLine("ATTACK");
            var heights = game.GetHeights();

            var target = new MyPosition(game.Enemy.Center.X + game.XDiff * -5, game.Me.Center.Y);

            //LogService.DrawLine(game.Me.Center, new MyPosition(game.Me.LeftCorner.X + game.XDiff, heights[(int)(game.Me.LeftCorner.X + game.XDiff*2)]), 1, 1, 1);
            return target;
        }
    }
}
