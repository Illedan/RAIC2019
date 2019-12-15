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
using aicup2019.Strategy.Sim;


 // LastEdited: 15/12/2019 13:47 


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
        public bool ShouldHeal => Health < MaxHealth ;

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

        public int GetDx(MyPosition p2) => p2.X > X ? 1 : ((int)p2.X == (int)X)?0:-1;

        public double Dist() => Math.Sqrt(X * X + Y * Y);
        public double Dist(MyPosition p2) => Math.Sqrt(Pow(X - p2.X) + Pow(Y - p2.Y));

        public static double Pow(double x) => x * x;

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

        public MyPosition Clone => new MyPosition(X, Y);

        public void UpdateFrom(MyPosition p)
        {
            X = p.X;
            Y = p.Y;
        }
    }
}

namespace aicup2019.Strategy
{
    public class MyAction
    {
        public bool JumpUp, JumpDown;
        public int Dx;
        public static double GetSpeed => Const.Properties.UnitMaxHorizontalSpeed;

        public static List<MyAction> Actions = new List<MyAction>
        {
            new  MyAction{ JumpUp = false, JumpDown = false, Dx = -1},
            new  MyAction{ JumpUp = false, JumpDown = false, Dx = 1},
            new  MyAction{ JumpUp = true, JumpDown = false, Dx = 1},
            new  MyAction{ JumpUp = true, JumpDown = false, Dx = -1},
            new  MyAction{ JumpUp = false, JumpDown = true, Dx = -1},
            new  MyAction{ JumpUp = false, JumpDown = true, Dx = 1},
            new  MyAction{ JumpUp = false, JumpDown = true, Dx = 0},
            new  MyAction{ JumpUp = true, JumpDown = false, Dx = 0},
        };

        public static List<MyAction> Dummy => new List<MyAction> { Actions[4] };
        public static MyAction DoNothing = new MyAction { JumpUp = false, JumpDown = false, Dx = 0 };
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
        public static int Evals, Sims;
        public static int Steps = 5, Depth = 31;
        public static double Time;
        

        public static void Reset(Properties properties)
        {
            Properties = properties;
            Time = 1 / Const.Properties.TicksPerSecond / 5;
        }
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
            if ((int)x >= Width || x < 0 || y < 0 || (int)y >= Width) return Tile.Wall;
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
        Const.Reset(game.Properties);

        Debug = debug;
        var myGame = new MyGame(game, unit);
        var me = myGame.Me;
        var aim = AimService.GetAimTarget(myGame);
        var shoot = ShootService.ShouldShoot(myGame, aim);
        var walkTarget = WalkService.FindWalkTarget(myGame);
        var jump = JumpService.GetDir(myGame, walkTarget);

        var stopwatch = Stopwatch.StartNew();
        var sim = new SimGame(myGame);
        var mySimUnit = sim.Units.First(u => u.unit.Id == me.Unit.Id);
        var selectedAction = MyAction.DoNothing;
        var bestScore = -10000.0;
        Const.Depth = sim.Bullets.Any(b => b.bullet.UnitId != mySimUnit.unit.Id) ? 30 : 6;

        foreach (var act in MyAction.Actions)
        {
            var score = SimService.ScoreDir(sim, act, walkTarget, mySimUnit, true);
            sim.Reset();
            if (score > bestScore)
            {
                selectedAction = act;
                bestScore = score;
            }
        }

        foreach (var act in MyAction.Actions)
        {
            var score = SimService.ScoreDir(sim, act, walkTarget, mySimUnit, false);
            sim.Reset();
            if(score > bestScore)
            {
                selectedAction = act;
                bestScore = score;
            }
        }
        Console.Error.WriteLine("Time: " + stopwatch.ElapsedMilliseconds + " BestScore: " + bestScore);

       // debug.Draw(new Line(aim.CreateFloatVec, me.Center.CreateFloatVec, 0.05f, new ColorFloat(1, 0, 0, 1)));
        //debug.Draw(new Line(walkTarget.CreateFloatVec, me.Center.CreateFloatVec, 0.1f, new ColorFloat(0, 0.5f, 0, 1)));
        //  Debug.Draw(new Log("Spread: " + (unit.Weapon.HasValue?unit.Weapon.Value.Spread:0) + " MAG: " + (unit.Weapon.HasValue ? unit.Weapon.Value.Magazine : 0) + " Reload: " + reload));
        UnitAction action = new UnitAction();
        //action.Velocity =me.Center.X < walkTarget.X ? game.Properties.UnitMaxHorizontalSpeed : -game.Properties.UnitMaxHorizontalSpeed;
        //action.Jump = jump > 0;
        //action.JumpDown = jump < 0;
        action.Velocity = selectedAction.Dx * game.Properties.UnitMaxHorizontalSpeed;
        action.Jump = selectedAction.JumpUp;
        action.JumpDown = selectedAction.JumpDown;
        action.Aim = new Vec2Double(aim.X - me.Center.X, aim.Y - me.Center.Y);
        action.Shoot = shoot;
        action.Reload = me.Center.Dist(myGame.Enemy.Center) > 5 && me.HasWeapon && me.Weapon.Magazine < me.Weapon.Parameters.MagazineSize*0.3;
        action.SwapWeapon = SwapService.ShouldSwap(myGame);
        action.PlantMine = myGame.Me.Center.Dist(myGame.Enemy.Center) < 3;

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
namespace aicup2019.Strategy.Sim
{
    public class SimUnit
    {
        public static double MaxJumpTime => Const.Properties.UnitJumpTime;
        public double JumpSpeed => Const.Properties.UnitJumpSpeed;

        public readonly Unit unit;
        public MyPosition Position, _position;
        public int Health, _health;
        public double HalfWidth, HalffHeight, JumpTime, _jumpTime;
        public bool IsDead;

        public SimUnit(Unit unit)
        {
            this.unit = unit;
            Health = _health = unit.Health;
            HalfWidth = unit.Size.X / 2;
            HalffHeight = unit.Size.Y / 2;
            Position = new MyPosition(unit.Position.X, unit.Position.Y + HalffHeight);
            _position = Position.Clone;
            JumpTime = _jumpTime = unit.JumpState.MaxTime > 0 ? unit.JumpState.MaxTime : (unit.JumpState.CanJump ? MaxJumpTime : 0);
        }

        public void Reset()
        {
            Position.UpdateFrom(_position);
            JumpTime = _jumpTime;
            Health = _health;

        }

        public void Draw(bool dmged)
        {
            LogService.DrawSquare(Position, HalfWidth * 2, HalffHeight * 2, dmged? 1f: 0.3f, 0.6f, 0);
        }

        public void ApplyAction(MyAction action, SimGame game, double dt)
        {
            var dy = -JumpSpeed;
            if(JumpTime > 0 && (action.JumpUp || !unit.JumpState.CanCancel))
            {
                JumpTime -= dt;
                dy = JumpSpeed;
            }
            else
            {
                JumpTime = 0;
            }

            var x = Position.X + action.Dx * MyAction.GetSpeed * dt;
            var y = Position.Y + dy * dt;

            var tile = game.GetTileD(Position.X-HalfWidth, y - HalffHeight);
            var otherTile = game.GetTileD(Position.X + HalfWidth, y - HalffHeight);
            var block = 0;
            if (tile == Tile.Wall || otherTile == Tile.Wall) block = 2;
            else if (tile == Tile.Platform || otherTile == Tile.Platform) 
                block = 1;

            var onLadder = game.GetTileD(Position.X, y) == Tile.Ladder || game.GetTileD(Position.X, y - HalffHeight) == Tile.Ladder;

            if(block == 2)
            {
                y = Position.Y;
                JumpTime = MaxJumpTime;
            }
            else if (onLadder)
            {
                JumpTime = MaxJumpTime;
                if (action.JumpUp)
                {
                    y = Position.Y + JumpSpeed * dt;
                }
                else if (action.JumpDown) y = Position.Y - JumpSpeed * dt;
                else y = Position.Y;
            }
            else if (block == 1 && !action.JumpDown && (int)(Position.Y-HalffHeight) > (int)(y-HalffHeight))
            {
                y = Position.Y;
                JumpTime = MaxJumpTime;
            }

            if(y > Position.Y)
            {
                var above = game.GetTileD(Position.X - HalfWidth, Position.Y + HalffHeight);
                var above1 = game.GetTileD(Position.X + HalfWidth, Position.Y + HalffHeight);
                if(above == Tile.Wall || above1 == Tile.Wall)
                {
                    y = Position.Y;
                    JumpTime = 0;
                }
            }


            if (game.GetTileD(x + HalfWidth, y - HalffHeight) == Tile.Wall
                || game.GetTileD(x - HalfWidth, y - HalffHeight) == Tile.Wall 
                || game.GetTileD(x + HalfWidth, y + HalffHeight) == Tile.Wall 
                || game.GetTileD(x - HalfWidth, y + HalffHeight) == Tile.Wall) x = Position.X;

            Position.X = x;
            Position.Y = y;
            //TODO: Collide with player
        }
    }
}
namespace aicup2019.Strategy.Sim
{
    public class SimBullet
    {
        public readonly Bullet bullet;
        public double Dx, Dy, HalfSize, ExplosionSize;
        public MyPosition Position, _position;
        public bool IsDead;

        public SimBullet(Bullet bullet)
        {
            this.bullet = bullet;
            Position = new MyPosition(bullet.Position.X, bullet.Position.Y);
            _position = Position.Clone;
            Dx = bullet.Velocity.X;
            Dy = bullet.Velocity.Y;
            HalfSize = bullet.Size / 2;
            ExplosionSize = bullet.ExplosionParameters.HasValue ? bullet.ExplosionParameters.Value.Radius : 0.0;
        }

        public void Reset()
        {
            IsDead = false;
            Position.UpdateFrom(_position);
        }

        public void Move(SimGame game, double dt)
        {
            if (IsDead) return;
            Position.X += Dx * dt;
            Position.Y += Dy * dt;
            foreach(var unit in game.Units)
            {
                if (IsCollidingWith(unit))
                {
                    if (bullet.ExplosionParameters != null) Explode(game);
                    else unit.Health -= bullet.Damage;
                    IsDead = true;
                    return;
                }
            }

            //TODO: Check mines
            var tile = game.GetTileD(Position.X, Position.Y);
            if (tile == Tile.Wall)
            {
                IsDead = true;
                if (bullet.ExplosionParameters != null) Explode(game);
            }
        }

        public bool IsCollidingWith(SimUnit unit)
        {
            if (unit.unit.Id == bullet.UnitId || unit.Health <= 0) return false;
            if (Math.Abs(Position.X - unit.Position.X) > HalfSize + unit.HalfWidth
                || Math.Abs(Position.Y - unit.Position.Y) > HalfSize + unit.HalffHeight) return false;

            return true;
        }

        public void Explode(SimGame game)
        {
            foreach (var unit in game.Units)
            {
                if (Math.Abs(Position.X - unit.Position.X) > bullet.ExplosionParameters.Value.Radius + unit.HalfWidth
                    || Math.Abs(Position.Y - unit.Position.Y) > bullet.ExplosionParameters.Value.Radius + unit.HalffHeight) continue;
                unit.Health -= bullet.ExplosionParameters.Value.Damage;
            }

            //TODO: Explode mines
        }
    }
}
namespace aicup2019.Strategy.Sim
{
    public class SimGame
    {
        private readonly MyGame game;
        public List<SimUnit> Units = new List<SimUnit>();
        public List<SimBullet> Bullets = new List<SimBullet>();

        public Tile[] Board;
        public SimGame(MyGame game)
        {
            this.game = game;
            Board = new Tile[game.Width * game.Height];
            for(var x = 0; x < game.Width; x++)
            {
                for(var y =0; y < game.Height; y++)
                {
                    Board[GetPos(x, y)] = game.Game.Level.Tiles[x][y];
                }
            }

            foreach(var u in game.Units)
            {
                Units.Add(new SimUnit(u.Unit));
            }

            foreach(var b in game.Bullets)
            {
                Bullets.Add(new SimBullet(b.Bullet));
            }
        }

        public Tile GetTileD(double x, double y) => GetTile((int)x, (int)y);
        public Tile GetTile(int x, int y) => OnBoard(x, y) ? Board[GetPos(x, y)] : Tile.Wall;
        public bool OnBoard(int x, int y) => game.OnBoard(x, y);
        public int GetPos(int x, int y) => game.Width * y + x;

        public void Reset()
        {
            foreach (var b in Bullets) b.Reset();
            foreach (var u in Units) u.Reset();
        }
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
    public static class SimService
    {
        public static double ScoreDir(SimGame game, MyAction action, MyPosition targetPos, SimUnit mySimUnit, bool waitFirst)
        {
            var actList = new List<MyAction> { action };
            var hp = mySimUnit.Health;
            if (waitFirst)
            {
                Simulate(game, new List<MyAction> { MyAction.DoNothing }, mySimUnit);
            }
            for (var i = waitFirst?1:0; i < Const.Depth; i++)
            {
                if (i % 5 == 0 && !waitFirst)
                {
                   //mySimUnit.Draw(hp != mySimUnit.Health);
                    hp = mySimUnit.Health;
                }
                SimService.Simulate(game, actList, mySimUnit);
            }

            return mySimUnit.Health * 1000 - mySimUnit.Position.Dist(targetPos);
        }

        public static void Simulate(SimGame game, List<MyAction> moves, SimUnit target)
        {
            Const.Sims++;
            for(var i = 0; i < moves.Count; i++)
            {
                for(var s = 0; s < Const.Steps; s++)
                {
                    foreach(var u in game.Units)
                    {
                        u.ApplyAction(u == target ? moves[i] : MyAction.DoNothing, game, Const.Time);
                    }

                    foreach(var b in game.Bullets)
                    {
                        b.Move(game, Const.Time);
                    }
                }
            }
        }
    }
}
namespace aicup2019.Strategy.Services
{
    public static class ShootService
    {
        public static bool IsDangerous(MyGame game, out MyPosition safe)
        {
            var me = game.Me.Bottom;
            var maxMS = Const.Properties.UnitMaxHorizontalSpeed / 60 * 20;
            var maxHeigth = game.Me.Unit.JumpState.CanJump ? (game.Me.Bottom.Y + Const.Properties.UnitJumpSpeed / 60 * 20) : game.Me.Bottom.Y;
            var posses = new List<MyPosition> { new MyPosition(me.X - maxMS, me.Y), new MyPosition(me.X + maxMS, me.Y), new MyPosition(me.X, maxHeigth), new MyPosition(me.X - maxMS, maxHeigth), new MyPosition(me.X + maxMS, maxHeigth) };
            safe = null;
            if (!IsDangerous(game, game.Me.Bottom)) return false;

            foreach(var p in posses)
            {
                var diff = game.Me.Bottom.GetDx(p);
                if (game.GetTile(me.X + diff, me.Y) == Tile.Wall) continue;
                safe = p;
                if (!IsDangerous(game, p)) return true;
            }

            return false;
        }

        private static bool IsDangerous(MyGame game, MyPosition pos)
        {
            var bullets = game.Bullets.Where(b => b.Bullet.UnitId != game.Me.Unit.Id).ToList();
            var rect = new Rect(pos.X - game.Me.Unit.Size.X * 0.5, pos.Y, pos.X + game.Me.Unit.Size.X * 0.5, pos.Y + game.Me.Unit.Size.Y);
            foreach(var b in bullets)
            {
                var time = GetShootTime(new MyPosition(b.Bullet.Position).Dist(pos), new MyPosition(b.Bullet.Velocity).Dist());
                if (time > 0.3) continue;
                var hitSpot = GetHitPos(new MyPosition(b.Bullet.Position), pos, game, new MyPosition(b.Bullet.Velocity).Dist(), false);
                if (hitSpot.Dist(pos) < 1) return true;
                if (b.Bullet.ExplosionParameters.HasValue && hitSpot.Dist(pos) < b.Bullet.ExplosionParameters.Value.Radius) return false;
            }

            return false;
        }

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

        public static MyPosition GetHitPos(MyPosition startPos, MyPosition endPos, MyGame game, double bulletSpeed, bool stopOnEnd = true)
        {
            var dist = endPos.Dist(startPos);
            var time = GetShootTime(dist, bulletSpeed) * Const.Properties.TicksPerSecond * 10;
            var dx = (endPos.X - startPos.X) / time;
            var dy = (endPos.Y - startPos.Y) / time;
            var x = startPos.X;
            var y = startPos.Y;
            var d = startPos.Dist(endPos);
            for (var i = 0; i < time*2; i++)
            {
                x += dx;
                y += dy;
                if (!game.OnBoard(x, y)) return new MyPosition(x, y);
                var tile = game.GetTile(x, y);
                if (tile == Tile.Wall) return new MyPosition(x, y);
                tile = game.GetTile(x-game.Me.Weapon.Parameters.Bullet.Size*0.5, y - game.Me.Weapon.Parameters.Bullet.Size * 0.5);
                if (tile == Tile.Wall) return new MyPosition(x, y);
                tile = game.GetTile(x + game.Me.Weapon.Parameters.Bullet.Size * 0.5, y - game.Me.Weapon.Parameters.Bullet.Size * 0.5);
                if (tile == Tile.Wall) return new MyPosition(x, y);
                var nextD = Math.Sqrt(MyPosition.Pow(x - endPos.X) + MyPosition.Pow(y - endPos.Y));
                if (nextD > d && stopOnEnd || nextD < 1) return endPos;
                d = nextD;
            }

            return endPos;
        }

        public static bool ShouldShoot(MyGame game, MyPosition aimPos)
        {
            var me = game.Me;
            if (!me.HasWeapon) return false;
            LogService.WriteLine("FireTimer: " + me.Unit.Weapon.Value.FireTimer);
            //if (me.Unit.Weapon.Value.Spread > me.Unit.Weapon.Value.Parameters.MinSpread + 0.1 && me.Center.Dist(aimPos) > 5) return false;
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

namespace aicup2019.Strategy.Services
{
    public static class WalkService
    {
        public static MyPosition GetRealTarget (MyGame game)
        {
            var me = game.Me;
            if (!me.HasWeapon) return GetWeapon(game);
            var weaps = game.Weapons.Where(w => (w.Item as Item.Weapon).WeaponType == WeaponType.RocketLauncher).ToList();
            if (me.Weapon.Typ != AiCup2019.Model.WeaponType.RocketLauncher && weaps.Any(w => me.Center.Dist(new MyPosition(w.Position)) < 4)) return new MyPosition(weaps.First(w => me.Center.Dist(new MyPosition(w.Position)) < 4).Position);
            if (me.ShouldHeal && game.HasHealing) return GetHealing(game);
            //return new MyPosition(game.Enemy.Center.MoveTowards(me.Center, 3).X, 50);
           if (me.Weapon.FireTimer > 0.2 && game.Me.Center.Dist(game.Enemy.Center) < 3) return Hide(game);
           LogService.WriteLine("Diff: " + game.ScoreDiff + " Tick: " + game.Game.CurrentTick + " " + game.Width + " " + game.Height);
           if(game.TargetDist < 4 && Math.Abs(game.Me.Center.Y-game.Enemy.Center.Y) < 1) return Attack(game);
           if (game.ScoreDiff > 0) return Hide(game);
           // if (game.ScoreDiff == 0 && game.Game.CurrentTick < 300 && game.Enemy.HasWeapon) return Hide(game);
           return Attack(game);
        }

        public static MyPosition FindWalkTarget(MyGame game)
        {
            var target = GetRealTarget(game);
            LogService.DrawLine(target, game.Me.Bottom, 1, 0, 0);
            if ((int)target.X == (int)game.Me.Center.X) return target;
            if (game.GetTile(target.X - 1, target.Y) == AiCup2019.Model.Tile.JumpPad) target = new MyPosition(target.X + 0.5, target.Y);
            else if (game.GetTile(target.X + 1, target.Y) == AiCup2019.Model.Tile.JumpPad) target = new MyPosition(target.X - 0.5, target.Y);
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
            if (target == null) target = game.HealthPacks.OrderBy(p => p.Dist(game.Me.Center)).First();
            return target;
        }

        private static MyPosition Hide(MyGame game)
        {
            LogService.WriteLine("HIDE");
            var heights = game.GetHeights();
            int xx = 0;
            var heightPositions = heights.Select(h => new MyPosition(xx++, h)).ToList();
            var target = heightPositions.Where(h => h.Y > heights.Min()+1).OrderByDescending(p => p.Dist(game.Enemy.Center) - game.Me.Center.Dist(p) / 2).FirstOrDefault();
            if (target == null) return heightPositions.OrderByDescending(p => p.Dist(game.Enemy.Center) - game.Me.Center.Dist(p) / 2).First();
            return target;
        }

        private static MyPosition Attack(MyGame game)
        {
            return new MyPosition(game.Enemy.Center.MoveTowards(game.Me.Center, 5).X, 50);
           //LogService.WriteLine("ATTACK");
           //var diff = 10;
           //if (game.Game.CurrentTick > 1000 && game.ScoreDiff < 0) diff = 0;
           //var target = new MyPosition(game.Enemy.Center.X + game.XDiff * -diff, game.Me.Center.Y);
           //if(target.X > game.Width || target.Y < 0) return new MyPosition(game.Enemy.Center.X + game.XDiff * diff, game.Me.Center.Y+50);
           //return target;
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

        public static void DrawLine(MyPosition p1, MyPosition p2, float r, float g, float b)
        {
            MyStrategy.Debug.Draw(new Line(p1.CreateFloatVec, p2.CreateFloatVec, 0.1f, new ColorFloat(r, g, b, 1)));
        }

        public static void DrawSquare(MyPosition position, double width, double height, float r, float g, float b)
        {
            var x1 = position.X - width / 2;
            var x2 = position.X + width / 2;
            var y1 = position.Y - height /2;
            var y2 = position.Y + height / 2;
            DrawLine(new MyPosition(x1, y1), new MyPosition(x2, y1), r, g, b);
            DrawLine(new MyPosition(x2, y1), new MyPosition(x2, y2), r, g, b);
            DrawLine(new MyPosition(x2, y2), new MyPosition(x1, y2), r, g, b);
            DrawLine(new MyPosition(x1, y2), new MyPosition(x1, y1), r, g, b);

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
            var requested = game.Enemy.Center;
            //if (dist < 7) requested = game.Enemy.Center;
            //requested = game.Enemy.GetEndPos(game);
            var angle = Math.Atan2(requested.Y - game.Me.Center.Y, requested.X - game.Me.Center.X);
            var prevAngle = game.Me.Unit.Weapon.Value.LastAngle.HasValue ? game.Me.Unit.Weapon.Value.LastAngle.Value : angle;
            if (Math.Abs(angle - prevAngle) < 0.1) angle = prevAngle;
           //else if(!ShootService.CanShoot(game.Me.Center,game.Enemy.Center, game, game.Me.Weapon.Parameters.Bullet.Speed))
           //{
           //    angle = prevAngle;
           //}
            var dx = Math.Cos(angle)*dist;
            var dy = Math.Sin(angle)*dist;
            return new MyPosition(game.Me.Center.X + dx, game.Me.Center.Y + dy);
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