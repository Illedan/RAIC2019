using AiCup2019.Model;
namespace aicup2019.Strategy
{
    public class MyUnit
    {
        public MyUnit(Unit unit)
        {
            Unit = unit;
            Size = Rect.FromUnit(unit);
        }

        public MyPosition Center => new MyPosition(Unit.Position.X, Unit.Position.Y + Unit.Size.Y / 2);
        public MyPosition Top => new MyPosition(Unit.Position.X, Unit.Position.Y + Unit.Size.Y);
        public MyPosition Bottom => new MyPosition(Unit.Position.X, Unit.Position.Y);
        public MyPosition LeftCorner => new MyPosition(Unit.Position.X - Unit.Size.X, Unit.Position.Y);
        public Unit Unit { get; }
        public Rect Size;
        public bool HasWeapon => Unit.Weapon.HasValue;
        public AiCup2019.Model.Weapon Weapon => Unit.Weapon.Value;
        public int Health => Unit.Health;
        public int MaxHealth => Const.Properties.UnitMaxHealth;
        public bool ShouldHeal => Health < MaxHealth * 0.7;

        public MyPosition GetEndPos(MyGame game)
        {
            var height = game.GetHeight(Size.X1, Size.X2, Bottom.Y);
            var heightPos = new MyPosition(Unit.Position.X, height);
            var dist = Bottom.Dist(heightPos);
            if (dist > 1 + Const.Properties.UnitSize.Y / 2) return new MyPosition(Unit.Position.X, Unit.Position.Y - 1);
            if (game.Me.HasWeapon && game.Me.Weapon.Typ == WeaponType.RocketLauncher)
                return heightPos;
            return heightPos.MoveTowards(game.Enemy.Center, Const.Properties.UnitSize.Y / 2);
        }
    }
}
