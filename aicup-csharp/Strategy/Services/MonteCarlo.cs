﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using aicup2019.Strategy.Sim;
//using AiCup2019.Model;

//namespace aicup2019.Strategy.Services
//{
//    public static class MonteCarlo
//    {
//        private static long m_lastBullet;
//        private static double bestScore;
//        private static MyAction[] Best, Temp;
//        private static Random rnd = new Random(42);
//        public static MyAction[] FindBest(SimGame game, SimUnit unit, MyPosition targetPos)
//        {
//            if (game.Bullets.Any()) m_lastBullet = game.game.Game.CurrentTick;
//            var depth = Const.Depth;
//            Best = new MyAction[depth];
//            Temp = new MyAction[depth];
//            bestScore = -100000000;
//            foreach (var act in MyAction.Actions.Take(7))
//            {
//                Repeat(Temp, act);
//                Score(game, unit, targetPos, false);
//            }
//            if (!unit.HasWeapon)
//            {
//                foreach (var act in MyAction.Actions.Take(3))
//                {
//                    Repeat(Temp, act);
//                    Temp[0] = MyAction.DoNothing;
//                    Score(game, unit, targetPos, false);
//                }
//            }

//            if (!game.Bullets.Any() && unit.HasWeapon) return Best;
//            while (!Const.IsDone())
//            {
//                if (rnd.NextDouble() < 0.8) Randomize(Temp);
//                else Mutate(Temp, Best);
//                Score(game, unit, targetPos);
//            }

//            return Best;
//        }

//        public static void Score(SimGame game, SimUnit unit, MyPosition targetPos, bool draw = false)
//        {
//            var score = SimService.ScoreDir(game, Temp, targetPos, unit, draw);
//            if (score > bestScore)
//            {
//                bestScore = score;
//                var tmp = Best;
//                Best = Temp;
//                Temp = tmp;
//                //Console.Error.WriteLine("BestScore: " + bestScore + " " + targetPos.X + " " + targetPos.Y + " " + unit.Position.X + " " + unit.Position.Y);
//            }
//            game.Reset();
//        }

//        public static void Randomize(MyAction[] actions)
//        {
//            for (var i = 0; i < Const.Depth; i++)
//            {
//                actions[i] = MyAction.Actions[rnd.Next(MyAction.Actions.Count)];
//            }
//        }

//        public static void Mutate(MyAction[] actions, MyAction[] from)
//        {
//            for (var i = 0; i < Const.Depth; i++)
//            {
//                actions[i] = rnd.NextDouble() < 0.5 ? MyAction.Actions[rnd.Next(MyAction.Actions.Count)] : from[i];
//            }
//        }

//        public static void Repeat(MyAction[] actions, MyAction target)
//        {
//            for (var i = 0; i < Const.Depth; i++)
//            {
//                actions[i] = target;
//            }
//        }
//    }
//}