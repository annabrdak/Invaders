using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Invaders.Model
{
    class Invader : Ship
    {
        public static readonly Size InvaderSize = new Size(15, 15);

        public const double HorizontalPixelsPerMove = 5;
        public const double VerticalPixelsPerMove = 15;


        public InvaderType InvaderType { get; private set; }

        public int Score { get; private set; }

        public Invader(InvaderType type, Point location, int score) 
            : base(location, Invader.InvaderSize)
        {
            InvaderType = type;
            Score = score;
        }


        public override void Move(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    Location = new Point(Location.X - HorizontalPixelsPerMove, Location.Y);
                    break;
                case Direction.Right:
                    Location = new Point(Location.X + HorizontalPixelsPerMove, Location.Y);
                    break;
                case Direction.Down:
                    Location = new Point(Location.X,Location.Y + VerticalPixelsPerMove);
                    break;              
            }
        }
    }
}
