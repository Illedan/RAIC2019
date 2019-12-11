using System.Linq;

namespace aicup2019.Strategy.Services
{
    public static class WalkService
    {
        public static MyPosition FindWalkTarget(MyGame game)
        {
            var me = game.Me;
            if (!me.HasWeapon) return GetWeapon(game);
            if (me.ShouldHeal && game.HasHealing) return GetHealing(game);
            if (game.ScoreDiff > 0) return Hide(game);
            if (game.ScoreDiff == 0 && game.Game.CurrentTick < 300) Hide(game);
            return Attack(game);
        }

        private static MyPosition GetWeapon(MyGame game)
        {
            return new MyPosition(game.Weapons.OrderBy(p => new MyPosition(p.Position).Dist(game.Me.Center)).First().Position);
        }

        private static MyPosition GetHealing(MyGame game)
        {
            return game.HealthPacks.OrderBy(p => p.Dist(game.Me.Center)).First();
        }

        private static MyPosition Hide(MyGame game)
        {
            var heights = game.GetHeights();
            int xx = 0;
            var heightPositions = heights.Select(h => new MyPosition(xx++, h)).ToList();
            var target = heightPositions.Where(h => h.Y > game.Height / 2).OrderByDescending(p => p.Dist(game.Enemy.Center) - game.Me.Center.Dist(p) / 2)?.FirstOrDefault();
            if (target == null) return heightPositions.OrderByDescending(p => p.Dist(game.Enemy.Center)).First();
            return new MyPosition(target.Value.X, 500);
        }

        private static MyPosition Attack(MyGame game)
        {
            var target = game.Enemy.Center.MoveTowards(game.Me.Center, 5);
            if(target.X < 0 || target.X >= game.Width)
            {
                target = game.Enemy.Center.MoveTowards(game.Me.Center, -5);
            }
            return target;
        }
    }
}
