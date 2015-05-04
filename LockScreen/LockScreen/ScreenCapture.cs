using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace LockScreen
{
    public class ScreenCapture
    {
        private int x, y, width, height;

        public ScreenCapture(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public BitmapSource Captured
        {
            get
            {
                Bitmap BMP = new Bitmap(width, height);
                Graphics graphic = Graphics.FromImage(BMP);
                graphic.CopyFromScreen(x, y, x, y, BMP.Size);
                var result = Imaging.CreateBitmapSourceFromHBitmap(BMP.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                return result;
            }
        }
    }
}
