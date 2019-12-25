using AiCup2019.Model;
using aicup2019.Strategy.Services;
using System.Collections.Generic;
using System.Linq;
using System;

namespace aicup2019.Strategy.Sim
{
    public class SimUnit
    {
        public static double MaxJumpTime => Const.Properties.UnitJumpTime;
        public double JumpSpeed => Const.Properties.UnitJumpSpeed;

        public List<SearchNode> Nodes = MyAction.Actions.Select(a => new SearchNode(a)).ToList();
        public SearchNode CurrentNode;
        public readonly Unit unit, Unit;
        public MyPosition Position, _position;
        public int Health, _health, MaxHealth;

        public WeaponType WeaponType;
        public double HalfWidth, HalffHeight, 
            JumpTime, _jumpTime, 
            Speed, 
            Score,
            ReloadTime,
            FireTimer, _fireTimer, MaxFireTimer, Spread, _spread, MaxSpread, MinSpread, SpreadChange, Recoil, AimAngle, _aimAngle;
        public bool IsDead, CanJump, CanCancel, debug, DidTouch;
        public int TeamId, Id, Magazine, Rounds, _rounds;
        public Rect Rect => Rect.FromUnit(unit);

        public MyPosition WalkTarget, AimTarget;
        public bool Shoot, IsMine;
        public bool NeedsHealing => Health < MaxHealth;

        public MyAction CurrentAction;
        public SimUnit Allied;
        public List<SimUnit> Enemies;
        public bool HasWeapon;
        public Weapon Weapon;
        public SimUnit TargetEnemy;

        public SimPlayer Player;

        public SimUnit(Unit unit)
        {
            MaxHealth = Const.Properties.UnitMaxHealth;
            Unit = this.unit = unit;
            Id = unit.Id;
            HasWeapon = unit.Weapon.HasValue;
            Weapon = HasWeapon ? unit.Weapon.Value : new Weapon();
            WeaponType = unit.Weapon.HasValue ? unit.Weapon.Value.Typ : WeaponType.Pistol;
            MaxFireTimer = unit.Weapon.HasValue ? unit.Weapon.Value.Parameters.FireRate : 10000;
            Rounds = _rounds = HasWeapon ? unit.Weapon.Value.Magazine : 0;
            Magazine = HasWeapon ? unit.Weapon.Value.Parameters.MagazineSize : 0;
            ReloadTime = HasWeapon ? unit.Weapon.Value.Parameters.ReloadTime : 1;

            FireTimer = _fireTimer = unit.Weapon.HasValue ? (unit.Weapon.Value.FireTimer ?? 0) : 10000;
            TeamId = unit.PlayerId;
            Health = _health = unit.Health;
            HalfWidth = unit.Size.X / 2;
            HalffHeight = unit.Size.Y / 2;
            Position = new MyPosition(unit.Position.X, unit.Position.Y + HalffHeight);
            _position = Position.Clone;
            JumpTime = _jumpTime = unit.JumpState.MaxTime > 0 ? unit.JumpState.MaxTime : (unit.JumpState.CanJump ? MaxJumpTime : 0);
            CanJump = unit.JumpState.CanJump;
            Speed = unit.JumpState.Speed;
            CanCancel = unit.JumpState.CanCancel;
            Spread = _spread = unit.Weapon.HasValue ? unit.Weapon.Value.Spread : 0;
            MaxSpread = unit.Weapon.HasValue ? unit.Weapon.Value.Parameters.MaxSpread : 0;
            MinSpread = unit.Weapon.HasValue ? unit.Weapon.Value.Parameters.MinSpread : 0;
            SpreadChange = unit.Weapon.HasValue ? unit.Weapon.Value.Parameters.AimSpeed : 0;
            Recoil = unit.Weapon.HasValue ? unit.Weapon.Value.Parameters.Recoil : 0;
            AimAngle = _aimAngle = unit.Weapon.HasValue ? unit.Weapon.Value.LastAngle ?? 0.0 : 0.0;
        }

        public void AfterRound()
        {
            var extra = 0.0;

            foreach (var enemy in Enemies)
            {
                extra -= enemy.Health * 100000;
            }

            if (Allied != null)
                extra += Allied.Score * 0.5 + Health * 50000;

            CurrentNode.Update(Score + extra);
        }

        public void GetNextMove(int depth)
        {
            if(depth == 0)
            {
                CurrentNode = GetRnd();
                if (Const.Evals < 20)
                {
                    for(var i = 0; i < Nodes.Count; i++)
                    {
                        var n = Nodes[i];
                        if (n.Usages < CurrentNode.Usages || n.Usages == CurrentNode.Usages && Const.rnd.NextDouble() < 0.10)
                        {
                            CurrentNode = n;
                        }
                    }
                }

                CurrentAction = CurrentNode.Action;
                return;
            }

           if(depth == 1 && Const.rnd.NextDouble() < 0.10) CurrentAction = GetRnd().Action;
           else CurrentAction = CurrentNode.Action;
            //else if (Const.rnd.NextDouble() < 0.90) CurrentAction = CurrentNode.Action;
            //else CurrentAction = GetRnd().Action;
        }

        public MyAction GetBestNode()
        {
            return Nodes.OrderByDescending(n => n.AvreageScore).First().Action;
        }

        public void Reset()
        {
            Position.UpdateFrom(_position);
            JumpTime = _jumpTime;
            Health = _health;
            CanJump = unit.JumpState.CanJump;
            Speed = unit.JumpState.Speed;
            CanCancel = unit.JumpState.CanCancel;
            Score = 0.0;
            FireTimer = _fireTimer;
            AimAngle = _aimAngle;
            Spread = _spread;
            Rounds = _rounds;
            DidTouch = false;
        }

        private SearchNode GetRnd() => Nodes[Const.rnd.Next(Nodes.Count)];

        public void Draw(bool dmged)
        {
            LogService.DrawSquare(Position, HalfWidth * 2, HalffHeight * 2, dmged? 1f: 0.3f, 0.6f, 0);
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

        public void Fire(MyPosition target, SimGame game)
        {
            if (!HasWeapon || FireTimer > 0 || Rounds <= 0) 
                return;

            var newAngle = Math.Atan2(target.Y - Position.Y, target.X - Position.X);
            var diff = Math.Abs(AimAngle - newAngle);
            if (diff > Math.PI)
            {
                var newDiff = Math.Abs(AimAngle - (newAngle - Math.PI * 2));
                if (newDiff < diff) diff = newDiff;
            }
            AimAngle = newAngle;
            if (AimAngle < 0) AimAngle += Math.PI * 2;
            else if (AimAngle > Math.PI * 2) AimAngle -= Math.PI * 2;
            Spread = Math.Max(MinSpread, Math.Min(MaxSpread, Spread + diff));

            if(debug)
                LogService.DrawLine(Position, new MyPosition(Position.X + Math.Cos(AimAngle) * 10, Position.Y + Math.Sin(AimAngle) * 10), 0, 0, 1);

            var bullets = BulletFactory.GetBullets(WeaponType, Position, newAngle, Id, Spread);
            game.Bullets.AddRange(bullets);
            Spread += Recoil;
            Rounds--;
            FireTimer = MaxFireTimer;
            if (Rounds <= 0)
            {
                FireTimer = ReloadTime;
                Rounds = Magazine;
            }
        }

        private bool m_jumpUp;
        public void ApplyAction(MyAction action, SimGame game, double dt, bool canChange)
        {
            Spread = Math.Max(MinSpread, Math.Min(MaxSpread, Spread - (SpreadChange * dt)));
            FireTimer -= dt;
            if (canChange) m_jumpUp = action.JumpUp;
            var dy = -JumpSpeed;
            if (game.GetTileD(Position.X + HalfWidth, Position.Y - HalffHeight) == Tile.JumpPad
                || game.GetTileD(Position.X - HalfWidth, Position.Y - HalffHeight) == Tile.JumpPad
                || game.GetTileD(Position.X + HalfWidth, Position.Y + HalffHeight) == Tile.JumpPad
                || game.GetTileD(Position.X - HalfWidth, Position.Y + HalffHeight) == Tile.JumpPad)
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
                if (m_jumpUp)
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

           //if (canChange && Position.Dist(TargetEnemy.Position) < 7 && ShootService.CanShootAt(Position, TargetEnemy.Position))
           //{
           //    //if (WeaponType != WeaponType.RocketLauncher || !IsMine)
           //    {
           //        Fire(TargetEnemy.Position, game);
           //    }
           //}
        }
    }
}
