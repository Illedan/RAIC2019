using System;
using AiCup2019.Model;
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
