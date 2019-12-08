using System;
using AiCup2019.Model;
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
