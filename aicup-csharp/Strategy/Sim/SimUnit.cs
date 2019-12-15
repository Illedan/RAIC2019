using AiCup2019.Model;
using aicup2019.Strategy.Services;
using System.Threading.Tasks;
namespace aicup2019.Strategy.Sim
{
    public class SimUnit
    {
        public static double MaxJumpTime => Const.Properties.UnitJumpTime;
        public double JumpSpeed => Const.Properties.UnitJumpSpeed;

        public readonly Unit unit;
        public MyPosition Position, _position;
        public int Health, _health;
        public double HalfWidth, HalffHeight, JumpTime, _jumpTime, Speed;
        public bool IsDead, CanJump, CanCancel;


        public SimUnit(Unit unit)
        {
            this.unit = unit;
            Health = _health = unit.Health;
            HalfWidth = unit.Size.X / 2;
            HalffHeight = unit.Size.Y / 2;
            Position = new MyPosition(unit.Position.X, unit.Position.Y + HalffHeight);
            _position = Position.Clone;
            JumpTime = _jumpTime = unit.JumpState.MaxTime > 0 ? unit.JumpState.MaxTime : (unit.JumpState.CanJump ? MaxJumpTime : 0);
            CanJump = unit.JumpState.CanJump;
            Speed = unit.JumpState.Speed;
            CanCancel = unit.JumpState.CanCancel;
        }

        public void Reset()
        {
            Position.UpdateFrom(_position);
            JumpTime = _jumpTime;
            Health = _health;
            CanJump = unit.JumpState.CanJump;
            Speed = unit.JumpState.Speed;
            CanCancel = unit.JumpState.CanCancel;
        }

        public void Draw(bool dmged)
        {
            LogService.DrawSquare(Position, HalfWidth * 2, HalffHeight * 2, dmged? 1f: 0.3f, 0.6f, 0);
        }

        public void ApplyAction(MyAction action, SimGame game, double dt)
        {
            var dy = -JumpSpeed;
            if (game.GetTileD(Position.X + HalfWidth, Position.Y - HalffHeight) == Tile.JumpPad
                || game.GetTileD(Position.X - HalfWidth, Position.Y - HalffHeight) == Tile.JumpPad)
            {
                JumpTime = Const.Properties.JumpPadJumpTime;
                Speed = Const.Properties.JumpPadJumpSpeed;
                CanCancel = false;
            }
            if (JumpTime > 0 && (action.JumpUp || !CanCancel))
            {
                JumpTime -= dt;
                var speed = JumpSpeed;
                if (Speed > JumpSpeed) speed = Speed;
                dy = speed;
            }
            else
            {
                JumpTime = 0;
            }

            var x = Position.X + action.Dx * MyAction.GetSpeed * dt;
            var y = Position.Y + dy * dt;

            var tile = game.GetTileD(Position.X-HalfWidth, y - HalffHeight);
            var otherTile = game.GetTileD(Position.X + HalfWidth, y - HalffHeight);
            var block = 0;
            if (tile == Tile.Wall || otherTile == Tile.Wall) block = 2;
            else if (tile == Tile.Platform || otherTile == Tile.Platform) 
                block = 1;

            var onLadder = game.GetTileD(Position.X, y) == Tile.Ladder || game.GetTileD(Position.X, y - HalffHeight) == Tile.Ladder;

            if(block == 2)
            {
                y = Position.Y;
                JumpTime = MaxJumpTime;
            }
            else if (onLadder)
            {
                JumpTime = MaxJumpTime;
                if (action.JumpUp)
                {
                    y = Position.Y + JumpSpeed * dt;
                }
                else if (action.JumpDown) y = Position.Y - JumpSpeed * dt;
                else y = Position.Y;
            }
            else if (block == 1 && !action.JumpDown && (int)(Position.Y-HalffHeight) > (int)(y-HalffHeight))
            {
                y = Position.Y;
                JumpTime = MaxJumpTime;
            }

            if(y > Position.Y)
            {
                var above = game.GetTileD(Position.X - HalfWidth, Position.Y + HalffHeight);
                var above1 = game.GetTileD(Position.X + HalfWidth, Position.Y + HalffHeight);
                if(above == Tile.Wall || above1 == Tile.Wall)
                {
                    y = Position.Y;
                    JumpTime = 0;
                    Speed = JumpSpeed;
                    CanCancel = true;
                }
            }


            if (game.GetTileD(x + HalfWidth, y - HalffHeight) == Tile.Wall
                || game.GetTileD(x - HalfWidth, y - HalffHeight) == Tile.Wall 
                || game.GetTileD(x + HalfWidth, y + HalffHeight) == Tile.Wall 
                || game.GetTileD(x - HalfWidth, y + HalffHeight) == Tile.Wall) x = Position.X;

            Position.X = x;
            Position.Y = y;
            //TODO: Collide with player
        }
    }
}
