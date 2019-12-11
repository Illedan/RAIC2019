using System;
using AiCup2019.Model;
using aicup2019.Strategy.Services;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using static AiCup2019.Model.CustomData;
using aicup2019.Strategy;
using AiCup2019;
using static AiCup2019.Model.Item;


 // LastEdited: 11/12/2019 19:33 


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
        public Unit Unit { get; }
        public Rect Size;
    }
}

namespace aicup2019.Strategy
{
    public struct MyPosition
    {
        public double X, Y;

        public MyPosition(double x, double y)
        {
            X = x;
            Y = y;
        }

        public MyPosition(Vec2Double vect)
        {
            X = vect.X;
            Y = vect.Y;
        }

        public double Dist(MyPosition p2) => Math.Sqrt(Pow(X - p2.X) + Pow(Y - p2.Y));

        public double Pow(double x) => x * x;

        public Vec2Double CreateVec => new Vec2Double(X, Y);
        public Vec2Float CreateFloatVec => CreateVec.Conv();

        public MyPosition MoveTowards(MyPosition pos, double speed)
        {
            var dist = Dist(pos);
            if (dist < 0.1) return pos;
            var dx = (pos.X - X) / dist * speed;
            var dy = (pos.Y - Y) / dist * speed;
            return new MyPosition(X + dx, Y + dy);
        }
    }
}

namespace aicup2019.Strategy
{
    public struct Rect
    {
        public Rect(double x1, double y1, double x2, double y2)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }

        public Rect Clone => new Rect(X1, Y1, X2, Y2);

        public static Rect FromMovingBullet(Vec2Double pos, double size)
        {
            var halfsize = size / 2;
            return new Rect(pos.X - halfsize, pos.Y - halfsize, pos.X + halfsize, pos.Y + halfsize);
        }

        public static Rect FromBullet(Bullet bullet)
        {
            return FromMovingBullet(bullet.Position, bullet.Size);
        }

        public static Rect FromUnit(Unit unit)
        {
            var size = unit.Size;
            var pos = unit.Position;
            return new Rect(pos.X - size.X / 2, pos.Y, pos.X + size.X / 2, pos.Y + size.Y);
        }

        public double X1, Y1, X2, Y2;

        public bool Overlapping(Rect rect)
        {
            return (rect.X2 >= X1 && rect.X1 <= X2) && (rect.Y2 >= Y1 && rect.Y1 <= Y2);
        }
    }
}

namespace aicup2019.Strategy
{
    public static class Const
    {
        public static Properties Properties;
    }
}

namespace aicup2019.Strategy
{
    public class MyGame
    {
        // Find all potential positions of the enemy. Make a circle of all places he can be. Shoot if I can limit it.
        public Stopwatch s;
        public List<MyBullet> Bullets = new List<MyBullet>();
        public List<MyUnit> Units = new List<MyUnit>();
        public int Width, Height;
        public Player MePlayer, EnemyPlayer;
        public MyGame(Game game, Unit me)
        {
            s = Stopwatch.StartNew();
            Game = game;
            Width = Game.Level.Tiles.Length;
            Height = Game.Level.Tiles[0].Length;
            Units.AddRange(game.Units.Select(u => new MyUnit(u)));
            Bullets.AddRange(game.Bullets.Select(b => new MyBullet(b)));
            MePlayer = game.Players.First(p => p.Id == me.PlayerId);
            EnemyPlayer = game.Players.First(p => p.Id != me.PlayerId);
        }

        public MyUnit Enemy(MyUnit u) => Units.First(en => en.Unit.PlayerId != u.Unit.PlayerId);

        public Game Game;

        public int[] GetHeights()
        {
            var heights = new int[Width];
            for(var x = 0; x < Width; x++)
            {
                for(var y = 1; y < Height; y++)
                {
                    var tile = Game.Level.Tiles[x][y];
                    if(tile != Tile.Wall && Game.Level.Tiles[x][y-1] == Tile.Wall)
                    {
                        heights[x] = y;
                        break;
                    }
                }
            }
            return heights;
        }

        public bool OnBoard(double x, double y) => OnBoard((int)x, (int)y);

        public bool OnBoard(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public Tile GetTile(double x, double y)
        {
            return Game.Level.Tiles[(int)x][(int)y];
        }


        public Vec2Float CalcBulletEnd(MyBullet bullet, out bool didHit)
        {
            var potentialUnits = Units.Where(u => u.Unit.Id != bullet.Bullet.UnitId).ToArray();
            var pos = new Vec2Double(bullet.Bullet.Position.X, bullet.Bullet.Position.Y);
            var speed = bullet.Bullet.Velocity;
            var ticks = Const.Properties.TicksPerSecond;
            speed = new Vec2Double(speed.X / ticks, speed.Y / ticks);
            var i = 0;
            didHit = false;
            while (true)
            {
                pos = new Vec2Double(pos.X + speed.X, pos.Y + speed.Y);
                if (!OnBoard(pos.X, pos.Y)) break;
                var rect = Rect.FromMovingBullet(pos, bullet.Bullet.Size);
                foreach (var u in potentialUnits)
                {
                    if (u.Size.Overlapping(rect))
                    {
                        didHit = true;
                        break;
                    }
                }

                if (didHit) break;

                var tile = GetTile(pos.X, pos.Y);
                if (tile == Tile.Wall) break;
                i++;
            }

            //MyStrategy.Debug.Draw(new Log("TIME: " + s.ElapsedMilliseconds));
            return new Vec2Float((float)pos.X, (float)pos.Y);
        }
    }
}
namespace aicup2019.Strategy
{
    public class MyBullet
    {
        public MyBullet(Bullet bullet)
        {
            Bullet = bullet;
            Size = Rect.FromBullet(Bullet);
        }

        public Bullet Bullet { get; }
        public Rect Size;
    }
}

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
            else if(myGame.MePlayer.Score > myGame.EnemyPlayer.Score || (myGame.MePlayer.Score == myGame.EnemyPlayer.Score && game.CurrentTick > 300))
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
           //debug.Draw(new Line(sFloat, eFloat, 0.1f, new ColorFloat(1, 0, 0, 1)));
        }

        debug.Draw(new Line(aim.Conv(), me.Center.CreateFloatVec, 0.1f, new ColorFloat(1, 0, 0, 1)));
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
namespace aicup2019.Strategy.Services
{
    public static class UselessExtensions
    {
        public static Vec2Float Conv(this Vec2Double from) => new Vec2Float(Convert.ToSingle(from.X), Convert.ToSingle(from.Y));
    }
}
namespace aicup2019.Strategy.Services
{
    public static class ShootService
    {
        public static double GetShootTime(double dist, double speed)
        {
            return dist / speed;
        }

        public static double GetShootTime(MyPosition pos, MyPosition end, double bulletSpeed)
        {
            var dist = end.Dist(pos);
            return GetShootTime(dist, bulletSpeed);
        }

        public static bool CanShoot(MyPosition startPos, MyPosition endPos, MyGame game, double bulletSpeed)
        {
            var dist = endPos.Dist(startPos);
            var time = GetShootTime(dist, bulletSpeed) * Const.Properties.TicksPerSecond;
            var dx = (endPos.X - startPos.X) / time;
            var dy = (endPos.Y - startPos.Y) / time;
            var x = startPos.X;
            var y = startPos.Y;
            for(var i = 0; i < time-1; i++)
            {
                x += dx;
                y += dy;
                if (!game.OnBoard(x, y)) return false;
                var tile = game.GetTile(x, y);
                if (tile == Tile.Wall) return false;
            }

            return true;
        }

        public static bool CanPotentiallyShoot(MyUnit me, MyUnit enemy, MyGame game)
        {
            if (!me.Unit.Weapon.HasValue) return false;
            if (me.Unit.Weapon.Value.Spread > 0.1 && me.Center.Dist(enemy.Center) > 5) return false;
            if(me.Unit.Weapon.Value.Typ == WeaponType.RocketLauncher)
            {
                if (!CanShoot(me.Center, enemy.Bottom, game, me.Unit.Weapon.Value.Parameters.Bullet.Speed)) return false;
                return true;
            }

            if (!CanShoot(me.Center, enemy.Center, game, me.Unit.Weapon.Value.Parameters.Bullet.Speed)) return false;
            if (!CanShoot(me.Center, enemy.Top, game, me.Unit.Weapon.Value.Parameters.Bullet.Speed)) return false;
            return true;
        }
    }
}
namespace aicup2019.Strategy.Services
{
    public static class WalkService
    {
        public static MyPosition FindWalkTarget(MyGame game, MyUnit me, MyUnit enemy)
        {

        }
    }
}
namespace aicup2019.Strategy.Services
{
    public class AimService
    {
        public AimService()
        {
        }
    }
}