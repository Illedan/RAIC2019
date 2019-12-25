using System;
using System.Collections.Generic;
using AiCup2019.Model;

namespace aicup2019.Strategy.Sim
{
    public static class BulletFactory
    { 
        private static int Amount = 1, CacheSize = 1000 ;
        private static int[] Counters = new int[3];
        private static SimBullet[][][] Bullets = new SimBullet[3][][];
        private static bool m_initialized = false;

        private static Dictionary<WeaponType, int> m_weapMap = new Dictionary<WeaponType, int>
        {
            { AiCup2019.Model.WeaponType.Pistol, 0},
            { AiCup2019.Model.WeaponType.AssaultRifle, 1},
            { AiCup2019.Model.WeaponType.RocketLauncher, 2},
        };

        public static SimBullet[] GetBullets(
            WeaponType weapon, 
            MyPosition start, 
            double angle,
            int unitId, 
            double spread)
        {
            //Is it slow to get bullets? Verify with Windows.
            var weapNum = m_weapMap[weapon];
            var bullets = Bullets[weapNum][Counters[weapNum]++];
            if (Amount == 1)
            {
                FlyToTarget(bullets[0], angle, start, unitId);
                return bullets;
            }
            FlyToTarget(bullets[0], angle - spread * 0.5, start, unitId);
            FlyToTarget(bullets[1], angle, start, unitId);
            FlyToTarget(bullets[2], angle + spread * 0.5, start, unitId);
            FlyToTarget(bullets[3], angle - spread, start, unitId);
            FlyToTarget(bullets[4], angle + spread, start, unitId);
            return bullets;
        }

        private static void FlyToTarget(SimBullet bullet, double angle, MyPosition start, int unitId)
        {
            bullet.UnitId = unitId;
            bullet.Position.UpdateFrom(start);
            var dx = Math.Cos(angle) * bullet.Speed;
            var dy = Math.Sin(angle) * bullet.Speed;
            bullet.Dx = dx;
            bullet.Dy = dy;
            bullet.IsDead = false;
            bullet.IsSimCreated = true;
        }

        public static void Initialize()
        {
            if (m_initialized) return;
            m_initialized = true;
            foreach (var weap in m_weapMap)
            {
                var bullet = new Bullet(weap.Key, 0, 0, new Vec2Double(0, 0), new Vec2Double(0, 0), Const.Properties.WeaponParameters[weap.Key].Bullet.Damage, Const.Properties.WeaponParameters[weap.Key].Bullet.Size, Const.Properties.WeaponParameters[weap.Key].Explosion);
                Bullets[weap.Value] = new SimBullet[CacheSize][];
                for (var i = 0; i < CacheSize; i++)
                {
                    Bullets[weap.Value][i] = new SimBullet[Amount];
                    for(var j = 0; j < Amount; j++)
                    {
                        Bullets[weap.Value][i][j] = new SimBullet(bullet);
                    }
                }
            }
        }

        public static void Reset()
        {
            Counters[0] = 0;
            Counters[1] = 0;
            Counters[2] = 0;
        }
    }
}
