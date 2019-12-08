using static AiCup2019.Model.CustomData;
using aicup2019.Strategy;
using System.Linq;
using AiCup2019.Model;
using static AiCup2019.Debug;
using AiCup2019;

public class MyStrategy
{
    static double DistanceSqr(Vec2Double a, Vec2Double b)
    {
        return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
    }

    public static AiCup2019.Debug Debug;

    public UnitAction GetAction(Unit unit, Game game, AiCup2019.Debug debug)
    {
        Debug = debug;
        Unit? nearestEnemy = null;
        foreach (var other in game.Units)
        {
            if (other.PlayerId != unit.PlayerId)
            {
                if (!nearestEnemy.HasValue || DistanceSqr(unit.Position, other.Position) < DistanceSqr(unit.Position, nearestEnemy.Value.Position))
                {
                    nearestEnemy = other;
                }
            }
        }
        LootBox? nearestWeapon = null;
        foreach (var lootBox in game.LootBoxes)
        {
            if (lootBox.Item is Item.Weapon)
            {
                if (!nearestWeapon.HasValue || DistanceSqr(unit.Position, lootBox.Position) < DistanceSqr(unit.Position, nearestWeapon.Value.Position))
                {
                    nearestWeapon = lootBox;
                }
            }
        }
        Vec2Double targetPos = unit.Position;
        if (!unit.Weapon.HasValue && nearestWeapon.HasValue)
        {
            targetPos = nearestWeapon.Value.Position;
        }
        else if (nearestEnemy.HasValue)
        {
            targetPos = nearestEnemy.Value.Position;
        }
        Vec2Double aim = new Vec2Double(0, 0);
        if (nearestEnemy.HasValue)
        {
            aim = new Vec2Double(nearestEnemy.Value.Position.X - unit.Position.X, nearestEnemy.Value.Position.Y - unit.Position.Y);
        }
        bool jump = targetPos.Y > unit.Position.Y;
        if (targetPos.X > unit.Position.X && game.Level.Tiles[(int)(unit.Position.X + 1)][(int)(unit.Position.Y)] == Tile.Wall)
        {
            jump = true;
        }
        if (targetPos.X < unit.Position.X && game.Level.Tiles[(int)(unit.Position.X - 1)][(int)(unit.Position.Y)] == Tile.Wall)
        {
            jump = true;
        }
        UnitAction action = new UnitAction();
        action.Velocity = targetPos.X - unit.Position.X;
        action.Jump = jump;
        action.JumpDown = !jump;
        action.Aim = aim;
        action.Shoot = true;
        action.Reload = false;
        action.SwapWeapon = true;
        action.PlantMine = false;
        var myGame = new MyGame(game);
        foreach (var bullet in myGame.Bullets)
        {
            var start = bullet.Bullet.Position;
            var end = myGame.CalcBulletEnd(bullet);
            var sFloat = new Vec2Float((float)start.X, (float)start.Y);
            var eFloat = new Vec2Float((float)end.X, (float)end.Y);
            debug.Draw(new Line(sFloat, eFloat, 0.3f, new ColorFloat(0, 0, 0, 1)));
        }

        return action;
    }
}
