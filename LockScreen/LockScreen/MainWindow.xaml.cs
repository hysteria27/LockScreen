using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace LockScreen
{
    public partial class MainWindow : Window
    {
        private Storyboard Jump, MoveBack, MoveUp;
        private IInputElement InputElement;
        private double MouseY, ScreenTop;
        private bool IsDragging;

        public MainWindow()
        {
            InitializeComponent();
            this.ScreenTop = -SystemParameters.PrimaryScreenHeight;
            this.Translate.Y = ScreenTop;

            this.Loaded += MainWindow_Loaded;

            this.Jump = (Storyboard)Resources["Jump"];
            this.MoveBack = (Storyboard)Resources["MoveBack"];
            this.MoveUp = (Storyboard)Resources["MoveUp"];

            this.MainGrid.MouseLeftButtonDown += MainGrid_MouseLeftButtonDown;
            this.MainGrid.MouseMove += MainGrid_MouseMove;
            this.MainGrid.MouseLeftButtonUp += MainGrid_MouseLeftButtonUp;

            this.Jump.Completed += Jump_Completed;
            this.MoveBack.Completed += MoveBack_Completed;
            this.MoveUp.Completed += MoveUp_Completed;
        }

        private void MainGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.MouseY = Mouse.GetPosition((IInputElement)sender).Y;
            this.InputElement = (IInputElement)sender;
            this.InputElement.CaptureMouse();
        }

        private void MainGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton.Equals(MouseButtonState.Pressed))
                this.IsDragging = true;

            if (this.IsDragging && this.InputElement != null)
            {
                var NewY = Mouse.GetPosition((IInputElement)sender).Y;
                if (NewY < this.MouseY)
                    Translate.Y = NewY - this.MouseY;
            }
        }

        private void MainGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.InputElement != null)
            {
                this.InputElement.ReleaseMouseCapture();
                this.InputElement = null;
                this.IsDragging = false;
            }

            if (Translate.Y == 0)
            {
                this.Jump.Begin();
            }
            else if (Translate.Y > (ScreenTop/2))
            {
                this.MoveBack.Begin();
            }
            else
            {
                ((DoubleAnimation)MoveUp.Children[0]).To = ScreenTop;
                this.MoveUp.Begin();
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.MoveBack.Begin();
        }

        private void Jump_Completed(object sender, EventArgs e)
        {
            this.Translate.Y = 0;
            this.Jump.Stop();
        }

        private void MoveBack_Completed(object sender, EventArgs e)
        {
            this.Translate.Y = 0;
            this.MoveBack.Stop();
        }

        private void MoveUp_Completed(object sender, EventArgs e)
        {
            this.Translate.Y = ScreenTop;
            this.MoveUp.Stop();
            Application.Current.Shutdown();
        }
    }
}
