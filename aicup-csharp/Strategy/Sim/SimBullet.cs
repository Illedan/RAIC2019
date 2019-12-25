using System;
using aicup2019.Strategy.Services;
using AiCup2019.Model;
namespace aicup2019.Strategy.Sim
{
    public class SimBullet
    {
        public readonly Bullet bullet;
        public double Dx, Dy, HalfSize, ExplosionSize, CollisionTime = 999999, _collisionTime = 999999,
            TempAngle, Speed;
        public MyPosition Position, _position, EndPosition;
        public int UnitId;
        public bool IsDead, IsSimCreated;

        public SimBullet(Bullet bullet)
        {
            this.bullet = bullet;
            UnitId = bullet.UnitId;
            Speed = Math.Sqrt(bullet.Velocity.X * bullet.Velocity.X + bullet.Velocity.Y * bullet.Velocity.Y);
            Position = new MyPosition(bullet.Position.X, bullet.Position.Y);
            _position = Position.Clone;
            Dx = bullet.Velocity.X;
            Dy = bullet.Velocity.Y;
            HalfSize = bullet.Size / 2 + bullet.Size * 0.1;
            ExplosionSize = bullet.ExplosionParameters.HasValue ? bullet.ExplosionParameters.Value.Radius : 0.0;
        }

        public void Draw()
        {
            LogService.DrawSquare(Position, HalfSize, HalfSize, 0.3f, 1, 0);
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
            foreach(var unit in game.Units)
            {
                if (IsCollidingWith(unit))
                {
                    if (bullet.ExplosionParameters != null) Explode(game);
                    unit.Health -= IsSimCreated ? bullet.Damage / 10 : bullet.Damage;
                    IsDead = true;
                    return;
                }
            }

            //TODO: Check mines

            if ((_collisionTime < 999999 && CollidesWithWall(game)) || CollisionTime <= 0)
            {
                IsDead = true;
                if (bullet.ExplosionParameters != null) Explode(game);
            }
        }

        public bool IsCollidingWith(SimUnit unit)
        {
            if (unit.Id == UnitId || unit.Health <= 0) return false;
            if (Math.Abs(Position.X - unit.Position.X) > HalfSize + unit.HalfWidth
                || Math.Abs(Position.Y - unit.Position.Y) > HalfSize+ unit.HalffHeight) return false;

            return true;
        }

        public void Explode(SimGame game)
        {
            var dmg = IsSimCreated ? bullet.ExplosionParameters.Value.Damage / 10 : bullet.ExplosionParameters.Value.Damage;
            foreach (var unit in game.Units)
            {
                if (Math.Abs(Position.X - unit.Position.X) > bullet.ExplosionParameters.Value.Radius + HalfSize*2 + unit.HalfWidth
                    || Math.Abs(Position.Y - unit.Position.Y) > bullet.ExplosionParameters.Value.Radius + HalfSize*2 + unit.HalffHeight) continue;
                unit.Health -= dmg;
            }

            //TODO: Explode mines
        }
    }
}
