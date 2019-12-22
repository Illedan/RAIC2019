using System;
using System.Collections.Generic;
using AiCup2019.Model;
using System.Linq;
namespace aicup2019.Strategy.Sim
{
    public class SimGame
    {
        public readonly MyGame game; 
        public List<SimUnit> Units = new List<SimUnit>();
        public List<SimBullet> Bullets = new List<SimBullet>();
        public SimPlayer[] Players = new SimPlayer[2];

        public Tile[] Board;

        public SimGame(MyGame game, Unit myUnit)
        {
            this.game = game;
            Board = new Tile[game.Width * game.Height];
            for(var x = 0; x < game.Width; x++)
            {
                for(var y =0; y < game.Height; y++)
                {
                    Board[GetPos(x, y)] = game.Game.Level.Tiles[x][y];
                }
            }

            Players[0] = new SimPlayer(game.Game.Players.First(p => p.Id == myUnit.PlayerId), game.Game);
            Players[1] = new SimPlayer(game.Game.Players.First(p => p.Id != myUnit.PlayerId), game.Game);

            foreach (var player in Players)
            {
                Units.AddRange(player.Units);
                foreach (var u in player.Units)
                    u.Enemies = Players.First(p => p != player).Units;
            }

            foreach(var b in game.Game.Bullets)
            {
                Bullets.Add(new SimBullet(b));
            }
            Players[0].EnemyPlayer = Players[1];
            Players[1].EnemyPlayer = Players[0];
            foreach(var u in Units)
            {
                u.TargetEnemy = u.Enemies.OrderBy(e => e.Position.Dist(u.Position) + e.Health * 0.01).First();
            }
        }

        public Tile GetTileD(double x, double y) => GetTile((int)x, (int)y);
        public Tile GetTile(int x, int y) => OnBoard(x, y) ? Board[GetPos(x, y)] : Tile.Wall;
        public bool OnBoard(int x, int y) => game.OnBoard(x, y);
        public int GetPos(int x, int y) => game.Width * y + x;

        public void Reset()
        {
            for(var i = Bullets.Count-1; i>= 0; i--)
            {
                if (Bullets[i].IsSimCreated)
                {
                    Bullets.RemoveAt(i);
                }
                else break;
            }

            foreach (var b in Bullets) b.Reset();
            Players[0].Reset();
            Players[1].Reset();
            BulletFactory.Reset();  
        }
    }
}
