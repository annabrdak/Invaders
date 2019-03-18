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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace Invaders.View
{
    using System.Diagnostics;
    using ViewModel;

    public partial class SpaceInvaders : Window
    {
        InvadersViewModel viewModel;

        public SpaceInvaders()
        {
            InitializeComponent();
            viewModel = FindResource("viewModel") as InvadersViewModel;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePlayAreaSize(new Size(e.NewSize.Width -40, e.NewSize.Height - 240));
        }

        private void PlayArea_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePlayAreaSize(playArea.RenderSize);
        }

        private void UpdatePlayAreaSize(Size newPlayAreaSize)
        {
            double targetWidth;
            double targetHeight;

            if (newPlayAreaSize.Width > newPlayAreaSize.Height)
            {
                targetWidth = newPlayAreaSize.Height * 4 / 3;
                targetHeight = newPlayAreaSize.Height;
                double leftRightMargin = (newPlayAreaSize.Width - targetWidth) / 2;
                playArea.Margin = new Thickness(leftRightMargin, 0, leftRightMargin, 0);                
            }
            else
            {
                targetHeight = newPlayAreaSize.Width * 3 / 4;
                targetWidth = newPlayAreaSize.Width;
                double topBottomMargin = (newPlayAreaSize.Height - targetHeight) / 2;
                playArea.Margin = new Thickness(0, topBottomMargin, 0, topBottomMargin);
            }
            playArea.Width = targetWidth;
            playArea.Height = targetHeight;
            viewModel.PlayAreaSize = playArea.RenderSize;
        }

        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            viewModel.KeyDown(e.Key);
        }
        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            viewModel.KeyUp(e.Key);
        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            viewModel.StartGame();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            PauseGame();
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            aboutPopup.IsOpen = true;
            if (!viewModel.Paused)
            {
                PauseGame();
            }
        }
        

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
            aboutPopup.IsOpen = false;            
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            aboutPopup.IsOpen = false;
        }

        private void PauseGame()
        {
            viewModel.PauseResumeGame();
            playArea.Focus();

            if (viewModel.Paused)
            {
                pauseButton.Content = "Resume";
            }
            else
            {
                pauseButton.Content = "Pause";
            }
        }
       
    }
}
