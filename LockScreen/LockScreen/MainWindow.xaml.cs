using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace LockScreen
{
    public partial class MainWindow : Window
    {
        private Storyboard MoveBack, MoveUp;
        private IInputElement InputElement;
        private double MouseY, CurrentTranslate, ScreenTop;
        private bool IsDragging;

        public MainWindow()
        {
            InitializeComponent();
            ScreenTop = -SystemParameters.PrimaryScreenHeight;
            Translate.Y = ScreenTop;

            MoveBack = (Storyboard)Resources["MoveBack"];
            MoveUp = (Storyboard)Resources["MoveUp"];

            this.Loaded += MainWindow_Loaded;
            this.PreviewMouseLeftButtonDown += MainWindow_PreviewMouseLeftButtonDown;
            this.PreviewMouseMove += MainWindow_PreviewMouseMove;
            this.PreviewMouseLeftButtonUp += MainWindow_PreviewMouseLeftButtonUp;

            MoveBack.Completed += MoveBack_Completed;
            MoveUp.Completed += MoveUp_Completed;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MoveUp.Begin();    // to make MoveUp controllable.
            MoveUp.Stop();     // then force it stop.

            this.Dispatcher.BeginInvoke(new Action(MoveBack.Begin));
        }

        private void MainWindow_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MouseY = Mouse.GetPosition((IInputElement)sender).Y;
            InputElement = (IInputElement)sender;
            InputElement.CaptureMouse();

            CurrentTranslate = Translate.Y;
            Translate.Y = CurrentTranslate;

            if (MoveBack.GetCurrentState() == ClockState.Active)
                MoveBack.Stop();

            if (MoveUp.GetCurrentState() == ClockState.Active)
                MoveUp.Stop();
        }

        private void MainWindow_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton.Equals(MouseButtonState.Pressed))
                IsDragging = true;

            if (IsDragging && InputElement != null)
            {
                var NewY = Mouse.GetPosition((IInputElement)sender).Y;
                if (Translate.Y <= 0)
                    Translate.Y = CurrentTranslate + (NewY - MouseY);

                if (Translate.Y > 0)
                    Translate.Y = 0;
            }
        }

        private void MainWindow_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (InputElement != null)
            {
                InputElement.ReleaseMouseCapture();
                InputElement = null;
                IsDragging = false;
                CurrentTranslate = 0;
            }

            if (Translate.Y < 0 && Translate.Y > (ScreenTop / 2))
            {
                MoveBack.Begin();
            }
            else if (Translate.Y <= (ScreenTop / 2))
            {
                ((DoubleAnimation)MoveUp.Children[0]).To = ScreenTop;
                MoveUp.Begin();
            }
        }

        private void MoveBack_Completed(object sender, EventArgs e)
        {
            Translate.Y = 0;
            MoveBack.Stop();
        }

        private void MoveUp_Completed(object sender, EventArgs e)
        {
            Translate.Y = ScreenTop;
            MoveUp.Stop();
            Application.Current.Shutdown();
        }
    }
}
