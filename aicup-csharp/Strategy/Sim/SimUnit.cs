using System;
using System.Collections.Generic;
using AiCup2019.Model;
using aicup2019.Strategy.Services;
namespace aicup2019.Strategy.Sim
{
    public class SimUnit
    {
        public static double MaxJumpTime => Const.Properties.UnitJumpTime;
        public double JumpSpeed => !unit.JumpState.CanJump ? Const.Properties.JumpPadJumpSpeed : Const.Properties.UnitJumpSpeed;

        public readonly Unit unit;
        public MyPosition Position, _position;
        public int Health, _health, PrevY;
        public double HalfWidth, HalffHeight, JumpTime, _jumpTime;
        public bool IsDead;

        public SimUnit(Unit unit)
        {
            this.unit = unit;
            Health = _health = unit.Health;
            HalfWidth = unit.Size.X / 2;
            HalffHeight = unit.Size.Y;
            Position = new MyPosition(unit.Position.X, unit.Position.Y + HalffHeight);
            _position = Position.Clone;
            JumpTime = _jumpTime = unit.JumpState.MaxTime;
            PrevY = (int)(Position.Y-HalffHeight);
        }

        public void Draw()
        {
            LogService.DrawSquare(Position, HalfWidth * 2, HalffHeight * 2, 0.3f, 0.3f, 0);
        }

        public void ApplyAction(MyAction action, SimGame game, double dt)
        {
            var dy = -JumpSpeed;
            if(JumpTime > 0 && (action.JumpUp || !unit.JumpState.CanCancel))
            {
                JumpTime -= dt;
                dy = JumpSpeed;
            }
            else
            {
                JumpTime = 0;
            }

            var x = Position.X + action.Dx * MyAction.GetSpeed * dt;
            var y = Position.Y + dy * dt;

            var yInt = (int)(y-HalffHeight);

            if(yInt != PrevY)
            {
                var tile = game.GetTileD(Position.X-HalfWidth, yInt);
                var otherTile = game.GetTileD(Position.X + HalfWidth, yInt);
                var block = 0;
                if (tile == Tile.Wall || otherTile == Tile.Wall) block = 2;
                else if (tile == Tile.Platform || otherTile == Tile.Platform) block = 1;
                if(block == 2 || (block == 1 && !action.JumpDown))
                {
                    y = (int)Position.Y;
                }

                PrevY = yInt;
            }

            var xTile = game.GetTileD(x+ HalfWidth*action.Dx, y+HalffHeight);
            var xTile1 = game.GetTileD(x + HalfWidth * action.Dx, y - HalffHeight);
            if (xTile == Tile.Wall || xTile1 == Tile.Wall) x = Position.X;

            Position.X = x;
            Position.Y = y;
        }

        public void Reset()
        {
            Position.UpdateFrom(_position);
            PrevY = (int)Position.Y;
            JumpTime = _jumpTime;
        }
    }
}
