using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LockScreen
{
    public partial class MainWindow : Window
    {
        #region Fields...
        private DispatcherTimer Timer;
        private ImageLoader ImgLoader;
        private IInputElement InputElement;
        private Storyboard MoveBack, MoveUp, FadeIn, FadeOut;
        private bool IsImageMoreThanOne;
        private double MouseY, CurrentTranslate, ScreenTop;
        private int ImageIndex;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            ScreenTop = -SystemParameters.PrimaryScreenHeight;
            Translate.Y = ScreenTop;

            ImgLoader = new ImageLoader(Properties.Settings.Default.ImagePath);
            GetSetImage();

            Timer = new DispatcherTimer() { Interval = new TimeSpan(0,1,0), IsEnabled = false };
            Timer.Tick += Timer_Tick;

            MoveBack = (Storyboard)Resources["MoveBack"];
            MoveUp = (Storyboard)Resources["MoveUp"];
            FadeIn = (Storyboard)Resources["FadeIn"];
            FadeOut = (Storyboard)Resources["FadeOut"];

            MoveBack.Completed += MoveBack_Completed;
            MoveUp.Completed += MoveUp_Completed;

            this.Loaded += MainWindow_Loaded;
            this.PreviewMouseLeftButtonDown += MainWindow_PreviewMouseLeftButtonDown;
            this.PreviewMouseMove += MainWindow_PreviewMouseMove;
            this.PreviewMouseLeftButtonUp += MainWindow_PreviewMouseLeftButtonUp;
        }

        private ImageSource BuiltInImage
        {
            get
            {
                BitmapImage BMP = new BitmapImage();
                BMP.BeginInit();
                BMP.CacheOption = BitmapCacheOption.OnDemand;
                BMP.UriSource = new Uri("pack://application:,,,/Resources/wallpaper.jpg", UriKind.Absolute);
                BMP.EndInit();

                return BMP;
            }
        }

        private void GetSetImage()
        {
            if (ImgLoader.IsCatched)
            {
                if (ImgLoader.ImageSources.Count == 0)
                    FrontBackground.Source = BuiltInImage;
                else
                {
                    FrontBackground.Source = ImgLoader.ImageSources[0];
                    IsImageMoreThanOne = ImgLoader.ImageSources.Count > 1;
                }
            }
            else
            {
                FrontBackground.Source = BuiltInImage;
            }
        }

        private void PlayShow()
        {
            ImageIndex = (ImageIndex + 1) % ImgLoader.ImageSources.Count;

            if (FrontBackground.Opacity == 1)
            {
                BackBackground.Source = ImgLoader.ImageSources[ImageIndex];
                FadeOut.Begin(FrontBackground);
            }
            else
            {
                FrontBackground.Source = ImgLoader.ImageSources[ImageIndex];
                FadeIn.Begin(FrontBackground);
            }
        }

        #region Events...
        private void Timer_Tick(object sender, EventArgs e)
        {
            PlayShow();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MoveUp.Begin();    // To make MoveUp controllable.
            MoveUp.Stop();     // Then force it stop.

            this.Dispatcher.BeginInvoke(DispatcherPriority.Send,
                new Action(MoveBack.Begin));
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
            if (InputElement != null)
            {
                var NewY = Mouse.GetPosition((IInputElement)sender).Y;
                if (Translate.Y < 1)
                    Translate.Y = CurrentTranslate + (NewY - MouseY);

                if (Translate.Y > 0)
                    Translate.Y = 0;

                if (CurrentTranslate != Translate.Y)
                    if (Timer.IsEnabled)
                        Timer.IsEnabled = false;
            }
        }

        private void MainWindow_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (InputElement != null)
            {
                InputElement.ReleaseMouseCapture();
                InputElement = null;
                CurrentTranslate = 0;
            }

            if (Translate.Y < 0 && Translate.Y > (ScreenTop / 2))
                MoveBack.Begin();
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

            if (IsImageMoreThanOne)         // If image more than one in desired folder,
                Timer.IsEnabled = true;     // enable the timer to start slideshow.
        }

        private void MoveUp_Completed(object sender, EventArgs e)
        {
            Translate.Y = ScreenTop;
            MoveUp.Stop();
            Application.Current.Shutdown();
        }
        #endregion
    }
}
