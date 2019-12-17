using System;
using System.Collections.Generic;
using aicup2019.Strategy.Sim;
using System.Linq;
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
                    if(dist < bestDist)
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
            if(!Game.game.OnBoard(p1.X, p1.Y) || !Game.game.OnBoard(p2.X, p2.Y))
                return (int)p1.Dist(p2);
            return Dists[Game.GetPos((int)p1.X, (int)p1.Y), Game.GetPos((int)p2.X, (int)p2.Y)];
        }

        public static void CalcDists(SimGame game)
        {
            Game = game;
            if (m_isRun) return;
            for(var i = game.game.Height-1; i >= 0; i--)
            {
                var line = "";
                for(var j = 0; j < game.game.Width; j++)
                {
                    var tile = game.game.Game.Level.Tiles[j][i];
                    if (tile == AiCup2019.Model.Tile.Wall) line += "#";
                    else if (tile == AiCup2019.Model.Tile.Ladder) line += "H";
                    else if (tile == AiCup2019.Model.Tile.Platform) line += "^";
                    else if (tile == AiCup2019.Model.Tile.JumpPad) line += "T";
                    else
                    {
                        var possible = ".";
                        foreach(var unit in game.game.Units)
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
            for (var i = 0; i< max; i++)
            {
                for(var j = 0; j <max; j++)
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
            while(posses.Count > 0)
            {
                var next = posses[0];
                for(var i = 1; i < posses.Count; i++)
                {
                    var pp = posses[i];
                    if(pp.Dist < next.Dist)
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
            for(var i = 0; i < 4; i++)
            {
                var xx = x + dxes[i];
                var yy = y + dyes[i];
                if (!Game.OnBoard(xx, yy) || xx == 0 || xx >= Game.game.Width-1 || yy == 0 || yy >= Game.game.Height-1) continue;
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
