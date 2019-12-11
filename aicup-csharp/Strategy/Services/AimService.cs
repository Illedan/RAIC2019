using System;
namespace aicup2019.Strategy.Services
{
    public static class AimService
    {
        public static MyPosition GetAimTarget(MyGame game)
        {
            if (!game.Me.HasWeapon) return game.Enemy.Center;
            var dist = game.Me.Center.Dist(game.Enemy.Center);
            return game.Enemy.GetEndPos(game);
           // if (game.Me.Weapon.Typ == AiCup2019.Model.WeaponType.RocketLauncher && dist > 3) return GetClosestGround(game);
           // return game.Enemy.Center;
        }
    }
}
