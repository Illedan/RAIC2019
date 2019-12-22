using System;
using System.Linq;
using AiCup2019.Model;
using aicup2019.Strategy.Sim;
namespace aicup2019.Strategy.Services
{
    public static class SwapService
    {
        public static bool ShouldSwap(SimGame game, SimUnit unit)
        {
            if (!unit.HasWeapon) return true;
            if (unit.WeaponType == WeaponType.AssaultRifle) return false;
            var weaponBoxes = game.game.Weapons;
            var closest = weaponBoxes.OrderBy(w => new MyPosition(w.Position).Dist(unit.Position)).Cast<LootBox?>().FirstOrDefault();
            if (closest == null) return false;
            var weapon = closest.Value.Item as Item.Weapon;
            var rect = Rect.FromMovingBullet(closest.Value.Position, closest.Value.Size.X);
            if (rect.Overlapping(unit.Rect) && unit.WeaponType != weapon.WeaponType)
            {
                if (weapon.WeaponType == WeaponType.AssaultRifle) return true;
                if (weapon.WeaponType == WeaponType.RocketLauncher) return true;
                //return weapon.WeaponType > game.Me.Weapon.Typ;
            }
            return false;
        }
    }
}
