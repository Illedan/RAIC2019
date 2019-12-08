using AiCup2019.Model;

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
