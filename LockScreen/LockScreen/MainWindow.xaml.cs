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
        private readonly ScreenCapture WindowBackground;
        private Storyboard MoveBack, MoveUp, FadeIn, FadeOut;
        private bool IsImageMoreThanOne;
        private static double MouseY, CurrentTranslate;
        private static readonly double
            ScreenWidth = SystemParameters.PrimaryScreenWidth,
            ScreenHeight = SystemParameters.PrimaryScreenHeight;
        private int ImageIndex;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.LowQuality);
            RenderOptions.SetCachingHint(this, CachingHint.Cache);
            //Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline),
            //    new FrameworkPropertyMetadata { DefaultValue = 24 });     // Default is 60.

            // Use desktop screenshot as a window background instead set the window transparency to true,
            // that will causing a poor performance.
            WindowBackground = new ScreenCapture(0, 0, (int)ScreenWidth, (int)ScreenHeight);

            ImgLoader = new ImageLoader(Properties.Settings.Default.ImagePath);
            GetSetImage();

            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                Timer = new DispatcherTimer() { Interval = TimeSpan.FromMinutes(1), IsEnabled = false };
                Timer.Tick += delegate { PlayShow(); };
            }));

            MoveBack = (Storyboard)Resources["MoveBack"];
            MoveUp = (Storyboard)Resources["MoveUp"];
            FadeIn = (Storyboard)Resources["FadeIn"];
            FadeOut = (Storyboard)Resources["FadeOut"];

            MoveBack.Completed += MoveBack_Completed;
            MoveUp.Completed += MoveUp_Completed;

            this.Loaded += MainWindow_Loaded;
            RootPanel.MouseLeftButtonDown += RootPanel_MouseLeftButtonDown;
            RootPanel.MouseLeftButtonUp += RootPanel_MouseLeftButtonUp;
        }

        private static ImageSource BuiltInImage
        {
            get
            {
                BitmapImage BMP = new BitmapImage();
                BMP.BeginInit();
                BMP.CacheOption = BitmapCacheOption.OnLoad;
                BMP.UriSource = new Uri("pack://application:,,,/Resources/wallpaper.jpg", UriKind.Absolute);
                BMP.EndInit();
                BMP.Freeze();

                return BMP;
            }
        }

        private void GetSetImage()
        {
            this.Background = new ImageBrush(WindowBackground.Captured);

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
                ImgLoader = null;
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
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MoveUp.Begin();    // To make MoveUp controllable.
            MoveUp.Stop();     // Then force it stop.

            Translate.Y = -ScreenHeight;
            this.Dispatcher.BeginInvoke(new Action(MoveBack.Begin));
        }

        private void RootPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MouseY = e.GetPosition(this).Y;

            CurrentTranslate = Translate.Y;
            Translate.Y = CurrentTranslate;

            InputElement = (IInputElement)sender;
            InputElement.CaptureMouse();

            if (MoveBack.GetCurrentState() == ClockState.Active)
                MoveBack.Stop();

            if (MoveUp.GetCurrentState() == ClockState.Active)
                MoveUp.Stop();

            RootPanel.MouseMove += RootPanel_MouseMove;
        }

        private void RootPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (InputElement != null)
            {
                if (Translate.Y < 1)
                {
                    double NewY = e.GetPosition(this).Y;
                    Translate.Y = CurrentTranslate + (NewY - MouseY);

                    if (Translate.Y > 0)
                        Translate.Y = 0;
                }

                if (Timer.IsEnabled)
                    if (CurrentTranslate != Translate.Y)
                        Timer.IsEnabled = false;
            }
        }

        private void RootPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (InputElement != null)
            {
                RootPanel.MouseMove -= RootPanel_MouseMove;
                InputElement.ReleaseMouseCapture();
                InputElement = null;
                CurrentTranslate = 0;

                if (Translate.Y < 0 && Translate.Y > (-ScreenHeight / 2))
                    MoveBack.Begin();
                else if (Translate.Y <= (-ScreenHeight / 2))
                {
                    ((DoubleAnimation)MoveUp.Children[0]).To = -ScreenHeight;
                    MoveUp.Begin();
                }
            }
        }

        private void MoveBack_Completed(object sender, EventArgs e)
        {
            Translate.Y = 0;

            if (IsImageMoreThanOne)         // If image more than one in desired folder,
                Timer.IsEnabled = true;     // enable the timer to start slideshow.
        }

        private void MoveUp_Completed(object sender, EventArgs e)
        {
            Translate.Y = -ScreenHeight;
            Application.Current.Shutdown();
        }
        #endregion
    }
}
