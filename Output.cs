using AiCup2019.Model;
using System;
using aicup2019.Strategy.Services;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using static AiCup2019.Model.CustomData;
using static AiCup2019.Model.Item;
using aicup2019.Strategy;
using AiCup2019;


 // LastEdited: 12/12/2019 0:55 


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

namespace aicup2019.Strategy
{
    public class MyPosition
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

        public MyPosition MoveTowards(double angle, double speed)
        {
            var dx = Math.Cos(angle);
            var dy = Math.Sin(angle);
            return new MyPosition(dx * speed + X, dy * speed + Y);
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
        public MyUnit Me, Enemy;
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
            Me = Units.First(u => u.Unit.Id == me.Id);
            Enemy = Units.OrderBy(u => u.Center.Dist(Me.Center)).First(u => u.Unit.PlayerId != me.PlayerId);
        }

        public int XDiff => Me.Center.X < Enemy.Center.X ? 1 : -1;
        public double TargetDist => Me.Center.Dist(Enemy.Center);
        public int ScoreDiff => MePlayer.Score - EnemyPlayer.Score;
        public bool HasHealing => HealthPacks.Any();

        public IEnumerable<MyPosition> HealthPacks => Game.LootBoxes.Where(l => l.Item is HealthPack).Select(h => new MyPosition(h.Position));

        public IEnumerable<LootBox> Weapons => Game.LootBoxes.Where(l => l.Item is Item.Weapon);

        public Game Game;

        public int GetHeight(double x0, double x1, double y)
        {
            var x = (int)x0;
            var x2 = (int)x1;
            for(var i = (int)y; i >= 0; i--)
            {
                if (Game.Level.Tiles[x][i] == Tile.Wall) return i + 1;
                if (Game.Level.Tiles[x2][i] == Tile.Wall) return i + 1;
            }

            return 0;
        }

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
    public static AiCup2019.Debug Debug;

    public UnitAction GetAction(Unit unit, Game game, AiCup2019.Debug debug)
    {
        Const.Properties = game.Properties;
        Debug = debug;
        var myGame = new MyGame(game, unit);
        var me = myGame.Me;
        var aim = AimService.GetAimTarget(myGame);
        var shoot = ShootService.ShouldShoot(myGame, aim);
        var walkTarget = WalkService.FindWalkTarget(myGame);
        var jump = JumpService.GetDir(myGame, walkTarget);

       // debug.Draw(new Line(aim.CreateFloatVec, me.Center.CreateFloatVec, 0.05f, new ColorFloat(1, 0, 0, 1)));
        //debug.Draw(new Line(walkTarget.CreateFloatVec, me.Center.CreateFloatVec, 0.1f, new ColorFloat(0, 0.5f, 0, 1)));
        //  Debug.Draw(new Log("Spread: " + (unit.Weapon.HasValue?unit.Weapon.Value.Spread:0) + " MAG: " + (unit.Weapon.HasValue ? unit.Weapon.Value.Magazine : 0) + " Reload: " + reload));
        UnitAction action = new UnitAction();
        action.Velocity =me.Center.X < walkTarget.X ? game.Properties.UnitMaxHorizontalSpeed : -game.Properties.UnitMaxHorizontalSpeed;
        action.Jump = jump > 0;
        action.JumpDown = jump < 0;
        action.Aim = new Vec2Double(aim.X - me.Center.X, aim.Y - me.Center.Y);
        action.Shoot = shoot;
        action.Reload = me.Center.Dist(myGame.Enemy.Center) > 5 && me.HasWeapon && me.Weapon.Magazine < me.Weapon.Parameters.MagazineSize*0.3;
        action.SwapWeapon = !unit.Weapon.HasValue || me.HasWeapon && me.Weapon.Magazine < me.Weapon.Parameters.MagazineSize * 0.3;
        action.PlantMine = false;

        var spread = AimService.GetSpread(myGame, aim);
        foreach(var point in spread)
        {
           debug.Draw(new Line(point.CreateFloatVec, me.Center.CreateFloatVec, 0.05f, new ColorFloat(0.1f, 0.1f, 0.4f, 1)));
        }

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
            var hitPos = GetHitPos(startPos, endPos, game, bulletSpeed);
            if(game.Me.Weapon.Typ == WeaponType.RocketLauncher)
            {
                var spread = AimService.GetSpread(game, endPos);
                var posses = spread.Select(s => GetHitPos(startPos, s, game, bulletSpeed)).ToArray();
                foreach(var p in posses)
                {
                    LogService.DrawLine(p, game.Me.Center, 0, 0, 1);
                }
                if (posses.Any(p => p.Dist(startPos) < p.Dist(endPos) && p.Dist(endPos) > game.Me.Weapon.Parameters.Explosion.Value.Radius))
                    return false;
            }

            return hitPos.Dist(endPos) < 1;
        }

        public static MyPosition GetHitPos(MyPosition startPos, MyPosition endPos, MyGame game, double bulletSpeed)
        {
            var dist = endPos.Dist(startPos);
            var time = GetShootTime(dist, bulletSpeed) * Const.Properties.TicksPerSecond;
            var dx = (endPos.X - startPos.X) / time;
            var dy = (endPos.Y - startPos.Y) / time;
            var x = startPos.X;
            var y = startPos.Y;
            for (var i = 0; i < time - 1; i++)
            {
                x += dx;
                y += dy;
                if (!game.OnBoard(x, y)) return new MyPosition(x, y);
                var tile = game.GetTile(x, y);
                if (tile == Tile.Wall) return new MyPosition(x, y);
            }

            return endPos;
        }

        public static bool ShouldShoot(MyGame game, MyPosition aimPos)
        {
            var me = game.Me;
            if (!me.HasWeapon) return false;
            if (me.Unit.Weapon.Value.Spread > me.Unit.Weapon.Value.Parameters.MinSpread + 0.3 && me.Center.Dist(aimPos) > 3) return false;
            if (!CanShoot(me.Center, aimPos, game, me.Unit.Weapon.Value.Parameters.Bullet.Speed)) return false;
            return true;
        }

        public static bool CanPotentiallyShoot(MyUnit me, MyUnit enemy, MyGame game)
        {
            if (!me.HasWeapon) return false;
            if (me.Unit.Weapon.Value.Spread > me.Unit.Weapon.Value.Parameters.MinSpread && me.Center.Dist(enemy.Center) > 3) return false;
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

namespace aicup2019.Strategy.Services
{
    public static class LogService
    {
        public static void WriteLine(this string line)
        {
            MyStrategy.Debug.Draw(new Log(line));
        }

        public static void DrawLine(MyPosition p1, MyPosition p2, int r, int g, int b)
        {
            MyStrategy.Debug.Draw(new Line(p1.CreateFloatVec, p2.CreateFloatVec, 0.1f, new ColorFloat(r, g, b, 1)));
        }
    }
}
namespace aicup2019.Strategy.Services
{
    public static class JumpService
    {
        public static int GetDir(MyGame game, MyPosition target)
        {
            var me = game.Me;
            if(game.Me.Bottom.Y < target.Y)
            {
                if (me.Unit.JumpState.MaxTime * me.Unit.JumpState.Speed < 2.5) return 0;
            }

            return game.Me.Bottom.Y < target.Y ? 1 : -1;
        }
    }
}
namespace aicup2019.Strategy.Services
{
    public static class AimService
    {
        public static MyPosition GetAimTarget(MyGame game)
        {
            if (!game.Me.HasWeapon) return game.Enemy.Center;
            var dist = game.Me.Center.Dist(game.Enemy.Center);
            if (dist < 3) return game.Enemy.Center;
            return game.Enemy.GetEndPos(game);
        }

        public static MyPosition[] GetSpread(MyGame game, MyPosition aim)
        {
            if (!game.Me.HasWeapon) return new MyPosition[0];
            var me = game.Me;
            var dist = aim.Dist(me.Center);
            var angle = Math.Atan2(aim.Y - me.Center.Y, aim.X - me.Center.X);
            var max = angle + game.Me.Weapon.Spread;
            var min = angle - game.Me.Weapon.Spread;
            return new MyPosition[] { me.Center.MoveTowards(max, 20), me.Center.MoveTowards(min, 20) };
        }
    }
}