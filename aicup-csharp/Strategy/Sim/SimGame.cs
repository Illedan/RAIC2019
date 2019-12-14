using System;
using System.Collections.Generic;
using AiCup2019.Model;
namespace aicup2019.Strategy.Sim
{
    public class SimGame
    {
        private readonly MyGame game;
        public List<SimUnit> Units = new List<SimUnit>();

        public Tile[] Board;
        public SimGame(MyGame game)
        {
            this.game = game;
            Board = new Tile[game.Width * game.Height];
            //TODO: Add board
        }

        public Tile GetTileD(double x, double y) => GetTile((int)x, (int)y);
        public Tile GetTile(int x, int y) => OnBoard(x, y) ? Board[GetPos(x, y)] : Tile.Wall;
        public bool OnBoard(int x, int y) => game.OnBoard(x, y);
        public int GetPos(int x, int y) => game.Width * y + x;
    }
}
