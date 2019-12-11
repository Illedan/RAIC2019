using System;
using AiCup2019.Model;
namespace aicup2019.Strategy.Services
{
    public static class UselessExtensions
    {
        public static Vec2Float Conv(this Vec2Double from) => new Vec2Float(Convert.ToSingle(from.X), Convert.ToSingle(from.Y));
    }
}
