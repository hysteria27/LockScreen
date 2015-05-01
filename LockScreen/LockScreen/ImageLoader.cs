using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LockScreen
{
    public class ImageLoader
    {
        private readonly string _root;
        private readonly string[] _supportedExtensions;
        private readonly bool _isCatched;
        private IEnumerable<string> _files;
        private List<ImageSource> _imageSources;

        public ImageLoader(object FileDir)
        {
            _root = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);  // Get full exe app location on disk.
            
            var DirPath = Path.Combine(Root, FileDir.ToString());
            if (Directory.Exists(DirPath))  // Check if directory is exists.
            {
                _supportedExtensions = new[] { ".bmp", ".jpeg", ".jpg", ".png" };
                _isCatched = true;
                _files = Directory.GetFiles(Path.Combine(Root, FileDir.ToString()), "*.*").Where
                    ((ext) => SupportedExtensions.Contains(Path.GetExtension(ext).ToLower()));

                _imageSources = new List<ImageSource>();
                foreach (var file in Files)
                {
                    BitmapImage BMP = new BitmapImage();
                    BMP.BeginInit();
                    BMP.CacheOption = BitmapCacheOption.None;
                    BMP.UriSource = new Uri(file, UriKind.Absolute);
                    BMP.EndInit();
                    BMP.Freeze();

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
