using System;
using AiCup2019.Model;
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
            HalfSize = bullet.Size / 2;
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
            var dt = 1.0 / Const.Properties.TicksPerSecond / Const.Properties.UpdatesPerTick;
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
            foreach(var unit in game.Units)
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
                || Math.Abs(Position.Y - unit.Position.Y) > HalfSize+ unit.HalffHeight) return false;

            return true;
        }

        public void Explode(SimGame game)
        {
            foreach (var unit in game.Units)
            {
                if (Math.Abs(Position.X - unit.Position.X) > bullet.ExplosionParameters.Value.Radius + HalfSize*2 + unit.HalfWidth
                    || Math.Abs(Position.Y - unit.Position.Y) > bullet.ExplosionParameters.Value.Radius + HalfSize*2 + unit.HalffHeight) continue;
                unit.Health -= bullet.ExplosionParameters.Value.Damage;
            }

            //TODO: Explode mines
        }
    }
}
