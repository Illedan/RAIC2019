using AiCup2019.Model;
using System;
using aicup2019.Strategy.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static AiCup2019.Model.CustomData;
using static AiCup2019.Model.Item;
using aicup2019.Strategy;
using AiCup2019;
using aicup2019.Strategy.Sim;
using System.Threading.Tasks;


// LastEdited: 18/12/2019 21:29 


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
        public bool ShouldHeal => Health < MaxHealth;

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

        public int GetDx(MyPosition p2) => p2.X > X ? 1 : ((int)p2.X == (int)X) ? 0 : -1;

        public double Dist() => Math.Sqrt(X * X + Y * Y);
        public double Dist(MyPosition p2) => Math.Sqrt(Pow(X - p2.X) + Pow(Y - p2.Y));
        public double XDist(MyPosition p2) => Math.Abs(X - p2.X);

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
    public class MyAction
    {
        public bool JumpUp, JumpDown;
        public int Dx;
        public static double GetSpeed => Const.Properties.UnitMaxHorizontalSpeed;

        public static List<MyAction> Actions = new List<MyAction>
        {
            new  MyAction{ JumpUp = true, JumpDown = false, Dx = 1},
            new  MyAction{ JumpUp = true, JumpDown = false, Dx = -1},
            new  MyAction{ JumpUp = false, JumpDown = false, Dx = -1},
            new  MyAction{ JumpUp = false, JumpDown = false, Dx = 1},
            new  MyAction{ JumpUp = false, JumpDown = true, Dx = -1},
            new  MyAction{ JumpUp = false, JumpDown = true, Dx = 1},
           // new  MyAction{ JumpUp = false, JumpDown = true, Dx = 0},
           // new  MyAction{ JumpUp = true, JumpDown = false, Dx = 0},
        };

        public static List<MyAction> Dummy => new List<MyAction> { Actions[4] };
        public static MyAction DoNothing = new MyAction { JumpUp = false, JumpDown = false, Dx = 0 };
    }
}

namespace aicup2019.Strategy
{
    public static class Const
    {
        public static Properties Properties;
        public static int Evals, Sims;
        public static int Steps = 5, Depth = 20, DepthPerMove = 1;
        public static double Time;
        public static Stopwatch Stopwatch;

        public static void Reset(Properties properties)
        {
            Evals = Sims = 0;
            Properties = properties;
            Time = 1 / Const.Properties.TicksPerSecond / Steps;
            Stopwatch = Stopwatch.StartNew();
            m_isDone = false;
            m_count = 0;
        }

        private static bool m_isDone;
        public static long m_timeout = 5;
        private static int m_count;

        public static bool IsDone()
        {
            if (m_isDone)
            {
                return true;
            }

            if (++m_count > 5)
            {
                var time = GetTime;
                if (time > m_timeout)
                {
                    m_isDone = true;
                }

                m_count = 0;
            }

            return m_isDone;
        }

        public static long GetTime => Stopwatch.ElapsedMilliseconds;
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
        }

        public void Init()
        {
            Enemy = Units.OrderBy(u => DistService.GetDist(u.Center, Me.Center)).First(u => u.Unit.PlayerId != Me.Unit.PlayerId);
        }

        public int XDiff => Me.Center.X < Enemy.Center.X ? 1 : -1;
        public double TargetDist => DistService.GetDist(Me.Center, Enemy.Center);
        public int ScoreDiff => MePlayer.Score - EnemyPlayer.Score;
        public bool HasHealing => HealthPacks.Any();

        public IEnumerable<MyPosition> HealthPacks => Game.LootBoxes.Where(l => l.Item is HealthPack).Select(h => new MyPosition(h.Position));

        public IEnumerable<LootBox> Weapons => Game.LootBoxes.Where(l => l.Item is Item.Weapon);

        public Game Game;

        public int GetHeight(double x0, double x1, double y)
        {
            var x = (int)x0;
            var x2 = (int)x1;
            for (var i = (int)y; i >= 0; i--)
            {
                if (Game.Level.Tiles[x][i] == Tile.Wall) return i + 1;
                if (Game.Level.Tiles[x2][i] == Tile.Wall) return i + 1;
            }

            return 0;
        }

        public List<MyPosition> GetHideouts()
        {
            var heights = GetHeights();
            var hideouts = new List<MyPosition>();
            for (var i = 4; i < heights.Length - 4; i++)
            {
                var h = heights[i];
                var dx = i > heights.Length / 2 ? -1 : 1;
                if (heights[i + dx] + 2 < h) hideouts.Add(new MyPosition(i, heights[i] + 1));
            }
            return hideouts;
        }

        public int[] GetHeights()
        {
            var heights = new int[Width];
            for (var x = 0; x < Width; x++)
            {
                var foundIt = false;
                for (var y = Height - 1; y >= 0; y--)
                {
                    var tile = Game.Level.Tiles[x][y];
                    if (tile == Tile.Empty) foundIt = true;
                    else if (tile != Tile.Empty && foundIt)
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


        public Vec2Float CalcBulletEnd(MyBullet bullet, double maxSpeed, out bool didHit)
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
        var sim = new SimGame(myGame);
        DistService.CalcDists(sim);
        myGame.Init();
        foreach (var b in sim.Bullets) b.CalcCollisionTime(sim);
        var aim = AimService.GetAimTarget(myGame);
        var shoot = ShootService.ShouldShoot(myGame, aim);
        var walkTarget = WalkService.FindWalkTarget(myGame);
        var jump = JumpService.GetDir(myGame, walkTarget.Clone);


        var mySimUnit = sim.Units.First(u => u.unit.Id == me.Unit.Id);
        var selectedAction = MyAction.DoNothing;
        Const.Depth = 10; //sim.Bullets.Any(b => b.bullet.UnitId != mySimUnit.unit.Id) || me.Center.Dist(walkTarget) > 3 ? 10 : 3;
        Const.DepthPerMove = 3;
        var sol = MonteCarlo.FindBest(sim, mySimUnit, walkTarget.Clone);
        selectedAction = sol[0];
        SimService.Simulate(sim, sol, mySimUnit, true);
        sim.Reset();
        if (game.CurrentTick % 600 == 0)
            Console.Error.WriteLine("Time: " + Const.GetTime + " Evals: " + Const.Evals + " Sims: " + Const.Sims);

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
        action.Reload = me.Center.Dist(myGame.Enemy.Center) > 5 && me.HasWeapon && me.Weapon.Magazine < me.Weapon.Parameters.MagazineSize * 0.3;
        action.SwapWeapon = SwapService.ShouldSwap(myGame);
        action.PlantMine = myGame.Me.Center.Dist(myGame.Enemy.Center) < 3;

        LogService.WriteLine("DIR: " + action.Velocity);
        var spread = AimService.GetSpread(myGame, aim);
        foreach (var point in spread)
        {
            debug.Draw(new Line(point.CreateFloatVec, me.Center.CreateFloatVec, 0.05f, new ColorFloat(0.1f, 0.1f, 0.4f, 1)));
        }

        foreach (var bullet in sim.Bullets)
        {
            var start = bullet.bullet.Position;
            var end = bullet.EndPosition;
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
        public double HalfWidth, HalffHeight, JumpTime, _jumpTime, Speed;
        public bool IsDead, CanJump, CanCancel;
        public int TeamId;

        public bool HasWeapon;

        public SimUnit(Unit unit)
        {
            this.unit = unit;
            TeamId = unit.PlayerId;
            HasWeapon = unit.Weapon.HasValue;
            Health = _health = unit.Health;
            HalfWidth = unit.Size.X / 2;
            HalffHeight = unit.Size.Y / 2;
            Position = new MyPosition(unit.Position.X, unit.Position.Y + HalffHeight);
            _position = Position.Clone;
            JumpTime = _jumpTime = unit.JumpState.MaxTime > 0 ? unit.JumpState.MaxTime : (unit.JumpState.CanJump ? MaxJumpTime : 0);
            CanJump = unit.JumpState.CanJump;
            Speed = unit.JumpState.Speed;
            CanCancel = unit.JumpState.CanCancel;
        }

        public void Reset()
        {
            Position.UpdateFrom(_position);
            JumpTime = _jumpTime;
            Health = _health;
            CanJump = unit.JumpState.CanJump;
            Speed = unit.JumpState.Speed;
            CanCancel = unit.JumpState.CanCancel;
        }

        public void Draw(bool dmged)
        {
            LogService.DrawSquare(Position, HalfWidth * 2, HalffHeight * 2, dmged ? 1f : 0.3f, 0.6f, 0);
        }

        public bool IsInside(double x, double y)
        {
            var x1 = Position.X - HalfWidth;
            var y1 = Position.Y - HalffHeight;
            var x2 = Position.X + HalfWidth;
            var y2 = Position.Y + HalffHeight;

            var xx1 = x - HalfWidth;
            var yy1 = y - HalffHeight;
            var xx2 = x + HalfWidth;
            var yy2 = y + HalffHeight;


            return (xx2 >= x1 && xx1 <= x2) && (yy2 >= y1 && yy1 <= y2);
        }

        private bool m_jumpUp;
        public void ApplyAction(MyAction action, SimGame game, double dt, bool canChange)
        {
            if (canChange) m_jumpUp = action.JumpUp;
            var dy = -JumpSpeed;
            if (game.GetTileD(Position.X + HalfWidth, Position.Y - HalffHeight) == Tile.JumpPad
                || game.GetTileD(Position.X - HalfWidth, Position.Y - HalffHeight) == Tile.JumpPad)
            {
                JumpTime = Const.Properties.JumpPadJumpTime;
                Speed = Const.Properties.JumpPadJumpSpeed;
                CanCancel = false;
            }
            if (JumpTime > 0 && (m_jumpUp || !CanCancel))
            {
                JumpTime -= dt;
                var speed = JumpSpeed;
                if (Speed > JumpSpeed) speed = Speed;
                dy = speed;
            }
            else
            {
                JumpTime = 0;
                CanCancel = true;
            }

            var y = Position.Y + dy * dt;

            var tile = game.GetTileD(Position.X - HalfWidth, y - HalffHeight);
            var otherTile = game.GetTileD(Position.X + HalfWidth, y - HalffHeight);
            var block = 0;
            if (tile == Tile.Wall || otherTile == Tile.Wall) block = 2;
            else if (tile == Tile.Platform || otherTile == Tile.Platform)
                block = 1;

            var onLadder = game.GetTileD(Position.X, y) == Tile.Ladder || game.GetTileD(Position.X, y - HalffHeight) == Tile.Ladder;

            if (block == 2)
            {
                y = Position.Y;
                JumpTime = MaxJumpTime;
            }
            else if (onLadder)
            {
                JumpTime = MaxJumpTime;
                if (m_jumpUp)
                {
                    y = Position.Y + JumpSpeed * dt;
                }
                else if (action.JumpDown) y = Position.Y - JumpSpeed * dt;
                else y = Position.Y;
            }
            else if (block == 1 && !action.JumpDown && (int)(Position.Y - HalffHeight) > (int)(y - HalffHeight))
            {
                y = Position.Y;
                JumpTime = MaxJumpTime;
            }

            if (y > Position.Y)
            {
                var above = game.GetTileD(Position.X - HalfWidth, Position.Y + HalffHeight);
                var above1 = game.GetTileD(Position.X + HalfWidth, Position.Y + HalffHeight);
                if (above == Tile.Wall || above1 == Tile.Wall)
                {
                    y = Position.Y;
                    JumpTime = 0;
                    Speed = JumpSpeed;
                    CanCancel = true;
                }
            }

            foreach (var u in game.Units)
            {
                if (u == this) continue;
                if (u.IsInside(Position.X, y))
                {
                    y = Position.Y;
                    break;
                }
            }

            var x = Position.X + action.Dx * MyAction.GetSpeed * dt;
            if (game.GetTileD(x + HalfWidth * action.Dx, Position.Y + HalffHeight) == Tile.Wall
                || game.GetTileD(x + HalfWidth * action.Dx, Position.Y - HalffHeight) == Tile.Wall) x = Position.X;

            foreach (var u in game.Units)
            {
                if (u == this) continue;
                if (u.IsInside(x, y))
                {
                    x = Position.X;
                    break;
                }
            }

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
        public double Dx, Dy, HalfSize, ExplosionSize, CollisionTime = 999999, _collisionTime = 999999;
        public MyPosition Position, _position, EndPosition;
        public bool IsDead;

        public SimBullet(Bullet bullet)
        {
            this.bullet = bullet;
            Position = new MyPosition(bullet.Position.X, bullet.Position.Y);
            _position = Position.Clone;
            Dx = bullet.Velocity.X;
            Dy = bullet.Velocity.Y;
            HalfSize = bullet.Size / 2 + bullet.Size * 0.1;
            ExplosionSize = bullet.ExplosionParameters.HasValue ? bullet.ExplosionParameters.Value.Radius : 0.0;
        }

        public void Reset()
        {
            IsDead = false;
            Position.UpdateFrom(_position);
            CollisionTime = _collisionTime;
        }

        public void CalcCollisionTime(SimGame game)
        {
            var t = 0.0;
            var dt = 1.0 / Const.Properties.TicksPerSecond / 10;
            while (true)
            {
                t += dt;
                Position.X += Dx * dt;
                Position.Y += Dy * dt;
                if (CollidesWithWall(game))
                {
                    CollisionTime = _collisionTime = t;
                    EndPosition = new MyPosition(Position.X, Position.Y);
                    break;
                }
            }

            Position.UpdateFrom(_position);
        }

        public bool CollidesWithWall(SimGame game)
        {
            var x = Position.X;
            var y = Position.Y;
            return CollidesWithWall(game, Position.X - HalfSize, Position.Y - HalfSize)
                || CollidesWithWall(game, Position.X + HalfSize, Position.Y - HalfSize)
                || CollidesWithWall(game, Position.X + HalfSize, Position.Y + HalfSize)
                || CollidesWithWall(game, Position.X - HalfSize, Position.Y + HalfSize);
        }

        public bool CollidesWithWall(SimGame game, double x, double y)
        {
            var tile = game.GetTileD(x, y);
            return tile == Tile.Wall;
        }

        public void Move(SimGame game, double dt)
        {
            if (IsDead) return;
            CollisionTime -= dt;
            Position.X += Dx * dt;
            Position.Y += Dy * dt;
            foreach (var unit in game.Units)
            {
                if (IsCollidingWith(unit))
                {
                    if (bullet.ExplosionParameters != null) Explode(game);
                    unit.Health -= bullet.Damage;
                    IsDead = true;
                    return;
                }
            }

            //TODO: Check mines
            if (CollidesWithWall(game) || CollisionTime <= 0)
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
                if (Math.Abs(Position.X - unit.Position.X) > bullet.ExplosionParameters.Value.Radius + HalfSize * 2 + unit.HalfWidth
                    || Math.Abs(Position.Y - unit.Position.Y) > bullet.ExplosionParameters.Value.Radius + HalfSize * 2 + unit.HalffHeight) continue;
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
        public readonly MyGame game;
        public List<SimUnit> Units = new List<SimUnit>();
        public List<SimBullet> Bullets = new List<SimBullet>();

        public Tile[] Board;
        public SimGame(MyGame game)
        {
            this.game = game;
            Board = new Tile[game.Width * game.Height];
            for (var x = 0; x < game.Width; x++)
            {
                for (var y = 0; y < game.Height; y++)
                {
                    Board[GetPos(x, y)] = game.Game.Level.Tiles[x][y];
                }
            }

            foreach (var u in game.Units)
            {
                Units.Add(new SimUnit(u.Unit));
            }

            foreach (var b in game.Bullets)
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
        private static MyPosition Target;
        public static double ScoreDir(SimGame game, MyAction[] action, MyPosition targetPos, SimUnit mySimUnit, bool draw = false)
        {
            Target = targetPos;
            Const.Evals++;
            var score = Simulate(game, action, mySimUnit, draw);
            return mySimUnit.Health * 100000 + score;
        }

        public static double Simulate(SimGame game, MyAction[] moves, SimUnit target, bool Draw = false)
        {
            var steps = Const.Steps * Const.DepthPerMove;
            var hp = target.Health;
            double d = 0.0; //target.HasWeapon ? DistService.GetDist(target.Position, Target) : target.Position.XDist(Target)*10;
            var score = 0;
            for (var i = 0; i < moves.Length; i++)
            {
                var move = moves[i];
                Const.Sims += Const.DepthPerMove;
                var timer = 0;
                for (var s = 0; s < steps; s++)
                {
                    foreach (var u in game.Units)
                    {
                        u.ApplyAction(u == target ? move : MyAction.DoNothing, game, Const.Time, timer == 0);
                    }

                    foreach (var b in game.Bullets)
                    {
                        b.Move(game, Const.Time);
                    }
                    if (timer-- <= 0) timer = Const.Steps;
                }
                if (target.HasWeapon)
                    foreach (var u in game.Units)
                    {
                        if (u == target) continue;
                        if (u.TeamId == target.TeamId)
                        {
                            if (Math.Abs(u.Position.X - target.Position.X) < 5 && Math.Abs(u.Position.Y - target.Position.Y) < 6)
                            {
                                score -= 1000;
                            }
                        }
                        else if (Math.Abs(u.Position.X - target.Position.X) < 3 && Math.Abs(u.Position.Y - target.Position.Y) < 5)
                        {
                            score -= 100;
                        }

                    }
                d += DistService.GetDist(target.Position, Target) + (target.Position.XDist(Target) * 1);

                if (Draw)
                {
                    target.Draw(hp != target.Health);
                }
            }
            return -d + score;
        }
    }
}
namespace aicup2019.Strategy.Services
{
    public static class DistService
    {
        private static int[] dxes = new int[] { -1, 1, 0, 0 };
        private static int[] dyes = new int[] { 0, 0, -1, 1 };
        public static bool m_isRun;
        public static int[,] Dists;
        private static SimGame Game;
        // Find all reachable tiles.

        public static void DrawPath(MyPosition p1, MyPosition p2)
        {
            var k = 0;
            LogService.WriteLine("DIST: " + DistService.GetDist(p1, p2));
            while ((int)p1.X != (int)p2.X || (int)p1.Y != (int)p2.Y)
            {
                k++;
                if (k > 30) return;
                var best = p1;
                var bestDist = 100000000.0;
                for (var i = 0; i < 4; i++)
                {
                    var xx = (int)p1.X + dxes[i];
                    var yy = (int)p1.Y + dyes[i];
                    if (!Game.OnBoard(xx, yy) || Game.GetTile(xx, yy) == AiCup2019.Model.Tile.Wall) continue;
                    LogService.DrawLineBetweenCenter(p1, new MyPosition(xx, yy), 0, 0, 1);
                    var newPos = new MyPosition(xx, yy);
                    var dist = GetDist(p2, newPos);
                    if (dist < bestDist)
                    {
                        best = newPos;
                        bestDist = dist;
                    }
                }
                //LogService.DrawLineBetweenCenter(p1, best, 0, 0, 1);
                p1 = best;
            }
        }

        public static int GetDist(MyPosition p1, MyPosition p2)
        {
            if (!Game.game.OnBoard(p1.X, p1.Y) || !Game.game.OnBoard(p2.X, p2.Y))
                return (int)p1.Dist(p2);
            return Dists[Game.GetPos((int)p1.X, (int)p1.Y), Game.GetPos((int)p2.X, (int)p2.Y)];
        }

        public static void CalcDists(SimGame game)
        {
            Game = game;
            if (m_isRun) return;
            for (var i = game.game.Height - 1; i >= 0; i--)
            {
                var line = "";
                for (var j = 0; j < game.game.Width; j++)
                {
                    var tile = game.game.Game.Level.Tiles[j][i];
                    if (tile == AiCup2019.Model.Tile.Wall) line += "#";
                    else if (tile == AiCup2019.Model.Tile.Ladder) line += "H";
                    else if (tile == AiCup2019.Model.Tile.Platform) line += "^";
                    else if (tile == AiCup2019.Model.Tile.JumpPad) line += "T";
                    else
                    {
                        var possible = ".";
                        foreach (var unit in game.game.Units)
                        {
                            var X = (int)unit.Bottom.X;
                            var Y = (int)unit.Bottom.Y;
                            if (X == j && Y == i) possible = "P";
                        }

                        line += possible;
                    }
                }
                Console.Error.WriteLine(line);
            }

            var t = Const.GetTime;
            m_isRun = true;
            var max = Game.game.Width * Game.game.Height;
            Dists = new int[max, max];
            for (var i = 0; i < max; i++)
            {
                for (var j = 0; j < max; j++)
                {
                    if (i == j) Dists[i, j] = 0;
                    else Dists[i, j] = 10000;
                }
            }
            for (var i = 0; i < Game.game.Width; i++)
            {
                for (var j = 0; j < Game.game.Height; j++)
                {
                    FindDists(i, j);
                }
            }
            Console.Error.WriteLine("Dist time: " + (Const.GetTime - t));
        }

        private static int FindDists(int x, int y)
        {
            var tested = 0;
            var p = Game.GetPos(x, y);
            // if (Game.Board[p] == AiCup2019.Model.Tile.Wall) return;
            var posses = new List<Node> { new Node { X = x, Y = y, Dist = 0 } };
            while (posses.Count > 0)
            {
                var next = posses[0];
                for (var i = 1; i < posses.Count; i++)
                {
                    var pp = posses[i];
                    if (pp.Dist < next.Dist)
                    {
                        next = pp;
                    }
                }

                posses.Remove(next);
                tested++;
                AddNeighbours(next.X, next.Y, next.Dist, posses, p);
            }
            return tested;
        }

        private static void AddNeighbours(int x, int y, int dist, List<Node> posses, int p)
        {
            for (var i = 0; i < 4; i++)
            {
                var xx = x + dxes[i];
                var yy = y + dyes[i];
                if (!Game.OnBoard(xx, yy) || xx == 0 || xx >= Game.game.Width - 1 || yy == 0 || yy >= Game.game.Height - 1) continue;
                var pos = Game.GetPos(xx, yy);
                var tile = Game.Board[pos];
                var stepCost = tile == AiCup2019.Model.Tile.Empty ? 3 : 1;
                var nextDist = dist + stepCost;
                if (tile == AiCup2019.Model.Tile.Wall)
                {
                    Dists[p, pos] = Math.Min(Dists[p, pos], nextDist);
                    continue;
                }
                var prevDist = Dists[p, pos];
                if (prevDist <= nextDist) continue;
                Dists[p, pos] = nextDist;
                posses.Add(new Node { X = xx, Y = yy, Dist = nextDist });
            }
        }

        //TODO: Add jumpHeight and reset if below is not empty. Then go to max of 4, keep max? Maybe set 8 with jumppad
        public class Node
        {
            public int X, Y, Dist;
        }
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

        public static bool CanShoot(MyPosition startPos, MyPosition endPos, MyGame game, MyUnit firering, double bulletSpeed)
        {
            var hitPos = GetHitPos(startPos, endPos, game, bulletSpeed, firering);
            if (game.Me.Weapon.Typ == WeaponType.RocketLauncher)
            {
                var spread = AimService.GetSpread(game, endPos);
                var posses = spread.Select(s => GetHitPos(startPos, s, game, bulletSpeed, firering)).ToArray();
                foreach (var p in posses)
                {
                    LogService.DrawLine(p, game.Me.Center, 0, 0, 1);
                }

                if (posses.Any(p => p.Dist(game.Enemy.Center) > p.Dist(game.Me.Center) && p.Dist(endPos) > game.Me.Weapon.Parameters.Explosion.Value.Radius - 1))
                    return false;

                if (game.Enemy.Center.Dist(endPos) - game.Me.Weapon.Parameters.Explosion.Value.Radius > game.Me.Center.Dist(endPos)) return false;
            }

            return hitPos.Dist(endPos) < 1;
        }

        public static MyPosition GetHitPos(MyPosition startPos, MyPosition endPos, MyGame game, double bulletSpeed, MyUnit firering, bool stopOnEnd = true)
        {
            var dist = endPos.Dist(startPos);
            var time = GetShootTime(dist, bulletSpeed) * Const.Properties.TicksPerSecond * 100;
            var dx = (endPos.X - startPos.X) / time;
            var dy = (endPos.Y - startPos.Y) / time;
            var x = startPos.X;
            var y = startPos.Y;
            var d = startPos.Dist(endPos);
            for (var i = 0; i < time * 2; i++)
            {
                x += dx;
                y += dy;
                if (!game.OnBoard(x, y)) return new MyPosition(x, y);
                var tile = game.GetTile(x, y);
                if (tile == Tile.Wall) return new MyPosition(x, y);
                tile = game.GetTile(x - game.Me.Weapon.Parameters.Bullet.Size * 0.5, y - game.Me.Weapon.Parameters.Bullet.Size * 0.5);
                if (tile == Tile.Wall) return new MyPosition(x, y);
                tile = game.GetTile(x + game.Me.Weapon.Parameters.Bullet.Size * 0.5, y - game.Me.Weapon.Parameters.Bullet.Size * 0.5);
                if (tile == Tile.Wall) return new MyPosition(x, y);
                var nextD = Math.Sqrt(MyPosition.Pow(x - endPos.X) + MyPosition.Pow(y - endPos.Y));
                if (nextD > d && stopOnEnd || nextD < 0.3) return endPos;
                d = nextD;
                foreach (var u in game.Units)
                {
                    if (u == firering || u.Unit.PlayerId != firering.Unit.PlayerId) continue;
                    var unit = u.Unit;
                    if (!(Math.Abs(x - unit.Position.X) > firering.Weapon.Parameters.Bullet.Size / 2 + unit.Size.X / 2
                            || Math.Abs(y - unit.Position.Y) > firering.Weapon.Parameters.Bullet.Size / 2 + unit.Size.Y / 2)) return new MyPosition(x, y);
                }
            }

            return endPos;
        }

        public static bool ShouldShoot(MyGame game, MyPosition aimPos)
        {
            var me = game.Me;
            if (!me.HasWeapon) return false;
            LogService.WriteLine("FireTimer: " + me.Unit.Weapon.Value.FireTimer);
            //if (me.Unit.Weapon.Value.Spread > me.Unit.Weapon.Value.Parameters.MinSpread + 0.1 && me.Center.Dist(aimPos) > 5) return false;
            if (!CanShoot(me.Center, aimPos, game, me, me.Unit.Weapon.Value.Parameters.Bullet.Speed)) return false;
            return true;
        }

        public static bool CanPotentiallyShoot(MyUnit me, MyUnit enemy, MyGame game)
        {
            if (!me.HasWeapon) return false;

            if (me.Unit.Weapon.Value.Typ == WeaponType.RocketLauncher)
            {
                if (!CanShoot(me.Center, enemy.Bottom, game, me, me.Unit.Weapon.Value.Parameters.Bullet.Speed)) return false;
                return true;
            }

            if (!CanShoot(me.Center, enemy.Center, game, me, me.Unit.Weapon.Value.Parameters.Bullet.Speed)) return false;
            // if (!CanShoot(me.Center, enemy.Top, game, me, me.Unit.Weapon.Value.Parameters.Bullet.Speed)) return false;
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
            if (!game.Me.HasWeapon) return true;
            if (game.Me.Weapon.Typ == WeaponType.AssaultRifle) return false;
            var weaponBoxes = game.Weapons;
            var closest = weaponBoxes.OrderBy(w => new MyPosition(w.Position).Dist(game.Me.Center)).Cast<LootBox?>().FirstOrDefault();
            if (closest == null) return false;
            var weapon = closest.Value.Item as Item.Weapon;
            var rect = Rect.FromMovingBullet(closest.Value.Position, closest.Value.Size.X);
            if (rect.Overlapping(game.Me.Size) && game.Me.Weapon.Typ != weapon.WeaponType)
            {
                if (weapon.WeaponType == WeaponType.AssaultRifle) return true;
                if (weapon.WeaponType == WeaponType.RocketLauncher) return true;
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
        public static MyPosition GetRealTarget(MyGame game)
        {
            var me = game.Me;
            var allied = game.Units.Where(u => u.Unit.PlayerId == me.Unit.PlayerId).OrderBy(u => u.Center.Dist(game.Enemy.Center)).ToList();
            //if (game.Game.CurrentTick > 1000) return Attack(game);
            if (!me.HasWeapon) return GetWeapon(game);
            //if (allied.Count == 2 && me == allied.Last() && allied[0].Health > 20) return allied[0].Center.MoveTowards(game.Me.Center, 100);
            var weaps = game.Weapons.Where(w => (w.Item as Item.Weapon).WeaponType == WeaponType.AssaultRifle).ToList();
            if (me.Weapon.Typ != AiCup2019.Model.WeaponType.AssaultRifle && weaps.Any(w => me.Center.Dist(new MyPosition(w.Position)) < 4)) return new MyPosition(weaps.First(w => me.Center.Dist(new MyPosition(w.Position)) < 4).Position);
            if (me.ShouldHeal && game.HasHealing) return GetHealing(game);
            //return new MyPosition(game.Enemy.Center.MoveTowards(me.Center, 3).X, game.Height-2);
            if (me.Weapon.FireTimer > 0.2 && game.Me.Center.Dist(game.Enemy.Center) < 3) return Hide(game);
            //LogService.WriteLine("Diff: " + game.ScoreDiff + " Tick: " + game.Game.CurrentTick + " " + game.Width + " " + game.Height);
            if (game.TargetDist < 4 && Math.Abs(game.Me.Center.Y - game.Enemy.Center.Y) < 1) return Attack(game);
            if (game.ScoreDiff > 0) return Hide(game);
            if (game.ScoreDiff == 0 && game.Game.CurrentTick < 300 && game.Enemy.HasWeapon) return Hide(game);
            return Attack(game);
        }

        public static MyPosition FindWalkTarget(MyGame game)
        {
            var target = GetRealTarget(game);
            LogService.DrawLineBetweenCenter(target, game.Me.Center, 1, 1, 1);
            for (var y = (int)target.Y; y < game.Height; y++)
            {
                var p = new MyPosition(target.X, y);
                var d = DistService.GetDist(p, game.Me.Center);
                if (d < game.Width * game.Height * 4)
                {
                    target = p;
                    break;
                }
            }

            LogService.DrawLine(target, game.Me.Bottom, 1, 0, 0);
            return target;
        }

        private static MyPosition GetWeapon(MyGame game)
        {
            LogService.WriteLine("WEAPON");
            return new MyPosition(game.Weapons.OrderBy(p => DistService.GetDist(new MyPosition(p.Position), game.Me.Center)).First().Position);
        }

        private static MyPosition GetHealing(MyGame game)
        {
            LogService.WriteLine("HEAL");
            var target = game.HealthPacks.OrderBy(p => DistService.GetDist(p, game.Me.Center)).FirstOrDefault(h => DistService.GetDist(h, game.Me.Center) < DistService.GetDist(h, game.Enemy.Center));
            if (target == null) target = game.HealthPacks.OrderBy(p => DistService.GetDist(p, game.Me.Center)).First();
            return target;
        }

        private static MyPosition Hide(MyGame game)
        {
            LogService.WriteLine("HIDE");
            var heights = game.GetHideouts();
            return heights.OrderByDescending(p => DistService.GetDist(p, game.Enemy.Center) - DistService.GetDist(game.Me.Center, p) * 0.5).FirstOrDefault() ?? game.Enemy.Center;
        }

        private static MyPosition Attack(MyGame game)
        {
            var diff = 10;
            if (game.Game.CurrentTick > 1000 && game.ScoreDiff <= 0) diff = 1;
            var target = game.Enemy.Center.MoveTowards(game.Me.Center, diff);
            if (target.X >= game.Width || target.X < 0) diff *= -1;
            target = game.Enemy.Center.MoveTowards(game.Me.Center, diff);
            return new MyPosition(target.X, Math.Min(target.Y + 20, game.Height - 2));
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
        public static bool m_debug = false;
        public static void WriteLine(this string line)
        {
            if (!m_debug) return;
            MyStrategy.Debug.Draw(new Log(line));
        }

        public static void DrawLine(MyPosition p1, MyPosition p2, float r, float g, float b)
        {
            if (!m_debug) return;
            MyStrategy.Debug.Draw(new Line(p1.CreateFloatVec, p2.CreateFloatVec, 0.1f, new ColorFloat(r, g, b, 1)));
        }

        public static void DrawLineBetweenCenter(MyPosition p1, MyPosition p2, float r, float g, float b)
        {
            if (!m_debug) return;
            p1 = new MyPosition(p1.X + 0.5, p1.Y + 0.5);
            p2 = new MyPosition(p2.X + 0.5, p2.Y + 0.5);
            MyStrategy.Debug.Draw(new Line(p1.CreateFloatVec, p2.CreateFloatVec, 0.1f, new ColorFloat(r, g, b, 1)));
        }

        public static void DrawSquare(MyPosition position, double width, double height, float r, float g, float b)
        {
            if (!m_debug) return;
            var x1 = position.X - width / 2;
            var x2 = position.X + width / 2;
            var y1 = position.Y - height / 2;
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
    public static class MonteCarlo
    {
        private static long m_lastBullet;
        private static double bestScore;
        private static MyAction[] Best, Temp;
        private static Random rnd = new Random(42);
        public static MyAction[] FindBest(SimGame game, SimUnit unit, MyPosition targetPos)
        {
            if (game.Bullets.Any()) m_lastBullet = game.game.Game.CurrentTick;
            if (unit.HasWeapon &&
                game.game.Game.CurrentTick - m_lastBullet > 600
                 && game.game.ScoreDiff > 0
                 && game.game.TargetDist > 5)
            {
                //Console.Error.WriteLine("Do nothing");
                return new MyAction[] { MyAction.DoNothing };
            }
            var depth = Const.Depth;
            Best = new MyAction[depth];
            Temp = new MyAction[depth];
            bestScore = -100000000;
            foreach (var act in MyAction.Actions)
            {
                Repeat(Temp, act);
                Score(game, unit, targetPos, false);
            }

            if (!game.Bullets.Any() && unit.HasWeapon) return Best;
            while (!Const.IsDone())
            {
                if (rnd.NextDouble() < 0.8) Randomize(Temp);
                else Mutate(Temp, Best);
                Score(game, unit, targetPos);
            }

            return Best;
        }

        public static void Score(SimGame game, SimUnit unit, MyPosition targetPos, bool draw = false)
        {
            var score = SimService.ScoreDir(game, Temp, targetPos, unit, draw);
            if (score > bestScore)
            {
                bestScore = score;
                var tmp = Best;
                Best = Temp;
                Temp = tmp;
                //Console.Error.WriteLine("BestScore: " + bestScore + " " + targetPos.X + " " + targetPos.Y + " " + unit.Position.X + " " + unit.Position.Y);
            }
            game.Reset();
        }

        public static void Randomize(MyAction[] actions)
        {
            for (var i = 0; i < Const.Depth; i++)
            {
                actions[i] = MyAction.Actions[rnd.Next(MyAction.Actions.Count)];
            }
        }

        public static void Mutate(MyAction[] actions, MyAction[] from)
        {
            for (var i = 0; i < Const.Depth; i++)
            {
                actions[i] = rnd.NextDouble() < 0.5 ? MyAction.Actions[rnd.Next(MyAction.Actions.Count)] : from[i];
            }
        }

        public static void Repeat(MyAction[] actions, MyAction target)
        {
            for (var i = 0; i < Const.Depth; i++)
            {
                actions[i] = target;
            }
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
            if (game.Me.Bottom.Y < target.Y)
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
            // Om eg ikke kan sikt rett på. Sikt på nærmeste punkt eg kan skyt han fra?
            // Med mindre eg er på vei mot han? For da kan eg 
            if (!game.Me.HasWeapon) return game.Enemy.Center;
            var dist = game.Me.Center.Dist(game.Enemy.Center);
            var requested = game.Me.Weapon.Typ == AiCup2019.Model.WeaponType.RocketLauncher ? GetClosestGround(game) : game.Enemy.Center;
            if (dist < 3 || Math.Abs(game.Enemy.Center.Y - game.Me.Center.Y) < 0.1) requested = game.Enemy.Center;
            //requested = game.Enemy.GetEndPos(game);
            var angle = Math.Atan2(requested.Y - game.Me.Center.Y, requested.X - game.Me.Center.X);
            var prevAngle = game.Me.Unit.Weapon.Value.LastAngle.HasValue ? game.Me.Unit.Weapon.Value.LastAngle.Value : angle;
            if (Math.Abs(angle - prevAngle) < 0.1 || game.Me.Weapon.FireTimer > 0 && Math.Abs(angle - prevAngle) < 0.2) angle = prevAngle;
            //else if(!ShootService.CanShoot(game.Me.Center,game.Enemy.Center, game, game.Me.Weapon.Parameters.Bullet.Speed))
            //{
            //    angle = prevAngle;
            //}
            var dx = Math.Cos(angle) * dist;
            var dy = Math.Sin(angle) * dist;
            return new MyPosition(game.Me.Center.X + dx, game.Me.Center.Y + dy);
        }

        public static MyPosition GetClosestGround(MyGame game)
        {
            var heights = game.GetHeights();
            var enemy = game.Enemy.Center;
            var d = 100000.0;
            var best = game.Enemy.Center;
            for (var i = 0; i < heights.Length; i++)
            {
                var p = new MyPosition(i, heights[i]);
                var dd = Math.Abs(p.Y - enemy.Y) + Math.Abs(p.X - enemy.X) * 5;
                if (dd < d)
                {
                    best = p;
                    d = dd;
                }
            }
            return best;
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