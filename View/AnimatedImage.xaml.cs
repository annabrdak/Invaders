using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Invaders.View
{
    
    public partial class AnimatedImage : UserControl
    {
        Storyboard invaderShotStoryboard;
        Storyboard flashStoryboard;

        public AnimatedImage()
        {
            InitializeComponent();

            invaderShotStoryboard = FindResource("invaderShotStoryboard") as Storyboard;
            flashStoryboard = FindResource("flashStoryboard") as Storyboard;
        }

        public AnimatedImage(IEnumerable<string> imageNames, TimeSpan interval)
            : this()
        {
            StartAnimation(imageNames, interval);
        }


        private void StartAnimation(IEnumerable<string> imageNames, TimeSpan interval)
        {
            Storyboard storyboard = new Storyboard();
            ObjectAnimationUsingKeyFrames animation = new ObjectAnimationUsingKeyFrames();
            Storyboard.SetTarget(animation, image);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Image.SourceProperty));
            TimeSpan currentInterval = TimeSpan.FromMilliseconds(0);

            foreach (string imageName in imageNames)
            {
                ObjectKeyFrame keyFrame = new DiscreteObjectKeyFrame();
                keyFrame.Value = CreateImageFromAssets(imageName);
                keyFrame.KeyTime = currentInterval;
                animation.KeyFrames.Add(keyFrame);
                currentInterval = currentInterval.Add(interval);
            }

            storyboard.RepeatBehavior = RepeatBehavior.Forever;
            storyboard.AutoReverse = true;
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        private static BitmapImage CreateImageFromAssets(string imageFilename)
        {
            try
            {
                Uri uri = new Uri("pack://application:,,,/Assets/"+imageFilename, UriKind.RelativeOrAbsolute);
                return new BitmapImage(uri);

            }
            catch (System.IO.IOException)
            {
                return new BitmapImage();
            }
        }
               

        public void InvaderShot()
        {
            invaderShotStoryboard.Begin();
        }

        public void StartFlashing()
        {
            flashStoryboard.Begin();
        }

        public void StopFlashing()
        {
            flashStoryboard.Stop();
        }
    }
}
