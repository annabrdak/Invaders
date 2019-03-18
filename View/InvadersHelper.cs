using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Invaders.View
{
    using Model;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Imaging;

    static class InvadersHelper
    {
        private static readonly Random _random = new Random();
                   

        public static FrameworkElement StarControlFactory(Point location, double scale)
        {
            FrameworkElement star;

            switch (_random.Next(3))
            {                
                case 0:
                    star = new Rectangle();
                    ((Rectangle)star).Fill = new SolidColorBrush(StarColorRandomizer());
                    ((Rectangle)star).Width = 2;
                    ((Rectangle)star).Height = 2;
                    break;
                case 1:
                    star = new Ellipse();
                    ((Ellipse)star).Fill= new SolidColorBrush(StarColorRandomizer());
                    ((Ellipse)star).Width = 2;
                    ((Ellipse)star).Height = 2;
                    break;
                default:
                    star = new StarControl();
                    ((StarControl)star).SetFill(new SolidColorBrush(StarColorRandomizer()));
                    break;
            }

            SetCanvasLocation(star, location.X * scale, location.Y * scale);
            Canvas.SetZIndex(star, -1000);
            return star;
        }

        public static FrameworkElement ShipControlFactory(Ship ship, double scale)
        {
            AnimatedImage shipControl = new AnimatedImage();

            if (ship is Invader)
            {
                Invader invaderShip = ship as Invader;
                shipControl = new AnimatedImage(CreateInvaderImageList(invaderShip.InvaderType), TimeSpan.FromMilliseconds(500));
            }
            else if (ship is Player)
            {
                Player playerShip = ship as Player;
                List<string> imageNames = new List<string>();
                imageNames.Add("player.png");
                shipControl = new AnimatedImage(imageNames, TimeSpan.FromSeconds(1));
            }

            shipControl.Width = ship.Size.Width * scale;
            shipControl.Height = ship.Size.Height * scale;
            SetCanvasLocation(shipControl, ship.Location.X *scale, ship.Location.Y *scale);
            return shipControl;
        }

        public static IEnumerable<string> CreateInvaderImageList(InvaderType invaderType)
        {
            string fileName = "";
            List<string> imageNames = new List<string>();

            switch (invaderType)
            {
                case InvaderType.Bug:
                    fileName = "bug";
                    break;
                case InvaderType.Saucer:
                    fileName = "flyingsaucer";                    
                    break;
                case InvaderType.Satellite:
                    fileName = "satellite";
                    break;
                case InvaderType.Spaceship:
                    fileName = "spaceship";
                    break;
                case InvaderType.Star:
                    fileName = "star";
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(fileName))
            {
                int numImages = 4;

                for (int i = 1; i <= numImages; i++)
                {
                    imageNames.Add(fileName + i + ".png");
                }
            }

            return imageNames;
        }

        public static FrameworkElement ShotControlFactory(Shot shot,double scale)
        {
            Rectangle shotControl = new Rectangle();
            shotControl.Width = Shot.ShotSize.Width;
            shotControl.Height = Shot.ShotSize.Height;
            shotControl.Fill = new SolidColorBrush(Colors.Yellow);
            SetCanvasLocation(shotControl, shot.Location.X*scale, shot.Location.Y*scale);
            return shotControl;
        }

        public static FrameworkElement ScanLineFactory(int y, int width, double scale)
        {
            Rectangle scanLine = new Rectangle();
            scanLine.Width = width * scale;
            scanLine.Height = 2;
            scanLine.Fill = new SolidColorBrush(Colors.White);
            scanLine.Opacity = .1;
            SetCanvasLocation(scanLine, 0, y * scale);           

            return scanLine;
        }

        private static Color StarColorRandomizer()
        {
            switch (_random.Next(9))
            {

                case 0:
                    return Colors.WhiteSmoke;
                case 1:
                    return Colors.Turquoise;
                case 2:
                    return Colors.Silver;
                case 3:
                    return Colors.PaleGoldenrod;
                case 4:
                    return Colors.OrangeRed;
                case 5:
                    return Colors.LightCyan;
                case 6:
                    return Colors.Fuchsia;
                case 7:
                    return Colors.AntiqueWhite;
                case 8:
                    return Colors.LightSkyBlue;
                default:
                    return Colors.Blue;
            }
        }



        public static void SetCanvasLocation(FrameworkElement control,double x, double y)
        {
            Canvas.SetLeft(control, x);
            Canvas.SetTop(control, y);
        }

        public static void MoveElementOnCanvas(FrameworkElement frameworkElement,double toX, double toY)
        {
            double fromX = Canvas.GetLeft(frameworkElement);
            double fromY = Canvas.GetTop(frameworkElement);

            Storyboard storyboard = new Storyboard();

            DoubleAnimation animationToX = CreateDoubleAnimation(frameworkElement, fromX, toX, new PropertyPath(Canvas.LeftProperty));
            DoubleAnimation animationToY = CreateDoubleAnimation(frameworkElement, fromY, toY, new PropertyPath(Canvas.TopProperty));

            storyboard.Children.Add(animationToX);
            storyboard.Children.Add(animationToY);
            storyboard.Begin();
        }

        public static DoubleAnimation CreateDoubleAnimation(FrameworkElement frameworkElement, double from, double to, PropertyPath propertyToAnimate,TimeSpan timespan)
        {
            DoubleAnimation animation = new DoubleAnimation();
            Storyboard.SetTarget(animation, frameworkElement);
            Storyboard.SetTargetProperty(animation, propertyToAnimate);
            animation.From = from;
            animation.To = to;
            animation.Duration = timespan;
            return animation;
        }

        public static DoubleAnimation CreateDoubleAnimation(FrameworkElement frameworkElement, double from, double to, PropertyPath propertyToAnimate)
        {
            return CreateDoubleAnimation(frameworkElement, from, to, propertyToAnimate, TimeSpan.FromMilliseconds(25));
        }

        internal static void ResizeElement(FrameworkElement control, double x, double y)
        {
            control.Width = x;
            control.Height = y;
        }
    }
}
