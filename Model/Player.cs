using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Invaders.Model
{
    class Player : Ship
    {
        public static readonly Size PlayerSize = new Size(25, 15);

        public const double Speed = 10;
        const double PixelsToMove = 10;

        public Player() 
            : base(new Point(InvadersModel.PlayAreaSize.Width/2, InvadersModel.PlayAreaSize.Height - PlayerSize.Height * 3), Player.PlayerSize)
        {
            Location = new Point(Location.X- PlayerSize.Width/2, InvadersModel.PlayAreaSize.Height - PlayerSize.Height * 3);
        }

        public override void Move(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    if (Location.X > PlayerSize.Width /2)
                        Location = new Point(Location.X - PixelsToMove, Location.Y);
                    break;
                default:
                    if (Location.X < InvadersModel.PlayAreaSize.Width - PlayerSize.Width*1.5)
                        Location = new Point(Location.X + PixelsToMove, Location.Y);
                    break;
            }
        }
    }
}
