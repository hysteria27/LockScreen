using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace LockScreen
{
    public partial class MainWindow : Window
    {
        private Storyboard MoveBack, MoveUp;
        private IInputElement InputElement;
        private double MouseY, CurrentTranslate, ScreenTop;
        private bool IsDragging;
        private ImageLoader ImgLoader;

        public MainWindow()
        {
            InitializeComponent();

            ImgLoader = new ImageLoader(Properties.Settings.Default.ImagePath);
            GetImage();

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

        private ImageSource BuiltInImage
        {
            get
            {
                BitmapImage BMP = new BitmapImage();
                BMP.BeginInit();
                BMP.CacheOption = BitmapCacheOption.OnLoad;
                BMP.UriSource = new Uri("pack://application:,,,/Resources/wallpaper.jpg", UriKind.Absolute);
                BMP.EndInit();

                return BMP;
            }
        }

        private void GetImage()
        {
            if (ImgLoader.IsCatched)
            {
                if (ImgLoader.ImageSources.Count() == 0)
                    BackgroundBrush.ImageSource = BuiltInImage;
                else
                    BackgroundBrush.ImageSource = ImgLoader.ImageSources[0];
            }
            else
            {
                BackgroundBrush.ImageSource = BuiltInImage;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MoveUp.Begin();    // to make MoveUp controllable.
            MoveUp.Stop();     // then force it stop.

            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send,
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

    public class ImageLoader
    {
        private readonly string _root;
        private readonly string[] _supportedExtensions;
        private readonly bool _isCatched;
        private IEnumerable<string> _files;
        private List<ImageSource> _imageSources;

        public ImageLoader(object FileDir)
        {
            _root = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            _supportedExtensions = new[] { ".bmp", ".jpeg", ".jpg", ".png", ".tiff" };

            var DirPath = Path.Combine(Root, FileDir.ToString());
            if (Directory.Exists(DirPath))      // Check if directory is exists.
            {
                _isCatched = true;
                _files = Directory.GetFiles(Path.Combine(Root, FileDir.ToString()), "*.*").Where
                    ((ext) => SupportedExtensions.Contains(Path.GetExtension(ext).ToLower()));

                _imageSources = new List<ImageSource>();
                foreach (var file in Files)
                {
                    BitmapImage BMP = new BitmapImage();
                    BMP.BeginInit();
                    BMP.CacheOption = BitmapCacheOption.OnLoad;
                    BMP.UriSource = new Uri(file, UriKind.Absolute);
                    BMP.EndInit();

                    _imageSources.Add(BMP);
                }
            }
        }

        private string Root
        {
            get { return _root; }
        }

        private string[] SupportedExtensions
        {
            get { return _supportedExtensions; }
        }

        public bool IsCatched
        {
            get { return _isCatched; }
        }

        private IEnumerable<string> Files
        {
            get { return _files; }
        }

        public List<ImageSource> ImageSources
        {
            get { return _imageSources; }
        }
    }
}
