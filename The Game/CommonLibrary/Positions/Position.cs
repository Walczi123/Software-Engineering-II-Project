﻿namespace GameMaster.Positions
{
    public class Position
    {
        public int x;
        public int y;

        public Position()
        {
        }

        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public void ChangePosition(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left: this.x -= 1;
                    break;
                case Direction.Right: this.x += 1;
                    break;
                case Direction.Up:  this.y -= 1;
                    break;
                case Direction.Down: this.y += 1;
                    break;
            }
        }
    }
}
