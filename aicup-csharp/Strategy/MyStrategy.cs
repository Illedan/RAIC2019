using static AiCup2019.Model.CustomData;
using aicup2019.Strategy;
using System.Linq;
using AiCup2019.Model;
using AiCup2019;
using System;
using aicup2019.Strategy.Services;
using static AiCup2019.Model.Item;

public class MyStrategy
{
    // Simuler
    static double DistanceSqr(Vec2Double a, Vec2Double b)
    {
        return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
    }

    public static AiCup2019.Debug Debug;

    public UnitAction GetAction(Unit unit, Game game, AiCup2019.Debug debug)
    {

        Const.Properties = game.Properties;
        Debug = debug;
        Unit? nearestEnemy = null;
        var myGame = new MyGame(game, unit);
        var me = myGame.Units.First(u => u.Unit.Id == unit.Id);
        var enemy = myGame.Enemy(me);

        var heights = myGame.GetHeights();
        int xx = 0;
        var heightPositions = heights.Select(h => new MyPosition(xx++, h)).ToList();

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

        var healthBox = game.LootBoxes.Where(l => l.Item is HealthPack).OrderBy(l => me.Center.Dist(new MyPosition(l.Position))).FirstOrDefault();

        if (!unit.Weapon.HasValue && nearestWeapon.HasValue)
        {
            var pos = nearestWeapon.Value.Position;
            if (pos.X < unit.Position.X) targetPos = new Vec2Double(unit.Position.X - game.Properties.UnitMaxHorizontalSpeed, pos.Y);
            else targetPos = new Vec2Double(unit.Position.X + game.Properties.UnitMaxHorizontalSpeed, pos.Y);
        }
        else if (nearestEnemy.HasValue)
        {
            if(me.Unit.Health < Const.Properties.UnitMaxHealth && game.LootBoxes.Contains(healthBox))
            {
                var pos = healthBox.Position;
                if (pos.X < unit.Position.X) targetPos = new Vec2Double(unit.Position.X - game.Properties.UnitMaxHorizontalSpeed, pos.Y);
                else targetPos = new Vec2Double(unit.Position.X + game.Properties.UnitMaxHorizontalSpeed, pos.Y);
            }
            else if(myGame.MePlayer.Score > myGame.EnemyPlayer.Score)
            {
                var target = heightPositions.Where(h => h.Y > myGame.Height/2).OrderByDescending(p => p.Dist(enemy.Center) - me.Center.Dist(p)/2)?.FirstOrDefault();
                if(target == null) target = heightPositions.OrderByDescending(p => p.Dist(enemy.Center)).FirstOrDefault();
                targetPos = new Vec2Double(target.Value.X, target.Value.Y + 100);
            }
            else
            {
                targetPos = new Vec2Double(nearestEnemy.Value.Position.X, nearestEnemy.Value.Position.Y+100);
            }
        }
        Vec2Double aim = new Vec2Double(0, 0);
        if (nearestEnemy.HasValue)
        {
            var dist = enemy.Center.Dist(me.Center);
            Debug.Draw(new Log("DIST: " + dist));
            if(dist < 3)
            {
                aim = new Vec2Double(enemy.Center.X - unit.Position.X, enemy.Center.Y - unit.Position.Y);
            }
            else
            {
                var target = heightPositions.OrderBy(h => h.Dist(enemy.Center)).First();
                aim = new Vec2Double(target.X - unit.Position.X, target.Y - unit.Position.Y);
            }
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
        var shoot = false;
        var reload = false;
        if(myGame.Units.Count > 1)
        {
            shoot = ShootService.CanPotentiallyShoot(me, enemy, myGame);
            if(!shoot && 
                me.Unit.Weapon.HasValue &&
                 me.Unit.Weapon.Value.Magazine < me.Unit.Weapon.Value.Parameters.MagazineSize*0.3)
            {
                reload = true;
            }
        }

        for(var x = 0; x < heights.Length; x++) 
        {
            var sFloat = new Vec2Float((float)(x), (float)heights[x]);
            var eFloat = new Vec2Float((float)(x+1), (float)heights[x]);
            debug.Draw(new Line(sFloat, eFloat, 0.1f, new ColorFloat(1, 0, 0, 1)));
        }
        Debug.Draw(new Log("Spread: " + (unit.Weapon.HasValue?unit.Weapon.Value.Spread:0) + " MAG: " + (unit.Weapon.HasValue ? unit.Weapon.Value.Magazine : 0) + " Reload: " + reload));
        UnitAction action = new UnitAction();
        action.Velocity = targetPos.X - unit.Position.X;
        action.Jump = jump;
        action.JumpDown = !jump;
        action.Aim = aim;
        action.Shoot = shoot;
        action.Reload = reload;
        action.SwapWeapon = !unit.Weapon.HasValue || unit.Weapon.Value.Typ != WeaponType.AssaultRifle;
        action.PlantMine = false;
        foreach (var bullet in myGame.Bullets)
        {
            var start = bullet.Bullet.Position;
            var end = myGame.CalcBulletEnd(bullet, out bool didHit);
            var sFloat = new Vec2Float((float)start.X, (float)start.Y);
            var eFloat = new Vec2Float((float)end.X, (float)end.Y);
            debug.Draw(new Line(sFloat, eFloat, 0.1f, new ColorFloat(0, 0, 0, 1)));
        }

        return action;
    }
}
