using System;
using System.Linq;
using AiCup2019.Model;
namespace aicup2019.Strategy.Services
{
    public static class SwapService
    {
        public static bool ShouldSwap(MyGame game)
        {
            if (!game.Me.HasWeapon ) return true;
            if (game.Me.Weapon.Typ == WeaponType.RocketLauncher) return false;
            var weaponBoxes = game.Weapons;
            var closest = weaponBoxes.OrderBy(w => new MyPosition(w.Position).Dist(game.Me.Center)).Cast<LootBox?>().FirstOrDefault();
            if (closest == null) return false;
            var weapon = closest.Value.Item as Item.Weapon;
            var rect = Rect.FromMovingBullet(closest.Value.Position, closest.Value.Size.X);
            if (rect.Overlapping(game.Me.Size))
            {
                if (weapon.WeaponType == WeaponType.RocketLauncher) return true;
                if (weapon.WeaponType == WeaponType.Pistol) return true;
                //return weapon.WeaponType > game.Me.Weapon.Typ;
            }
            return false;
        }
    }
}
