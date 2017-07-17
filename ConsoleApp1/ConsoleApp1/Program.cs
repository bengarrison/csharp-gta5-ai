using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using Point = OpenCvSharp.Point;
using Timer = System.Windows.Forms.Timer;

namespace ConsoleApp1
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static Rectangle screenCaptureArea = new Rectangle(0, 40, 800, 640);

        [STAThread]
        static void Main(string[] args)
        {
            HideConsoleWindow();

            Application.EnableVisualStyles();
            Application.Run(new Form1());


        }


        private static Mat processImage()
        {
            var bmp = new Bitmap(screenCaptureArea.Width, screenCaptureArea.Height, PixelFormat.Format32bppArgb);
            var g = Graphics.FromImage(bmp);

            g.CopyFromScreen(screenCaptureArea.Left, screenCaptureArea.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);

            var src = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmp);
            var dst = new Mat();

            Cv2.Canny(src, dst, 75, 100);

            return dst;
        }



        private static void HideConsoleWindow()
        {
            var handle = GetConsoleWindow();

            // Hide
            ShowWindow(handle, 0);
        }
    }

    public class Form1 : Form
    {
        private Rectangle screenCaptureArea = new Rectangle(0, 40, 800, 640);
        private PictureBox pictureBox = new PictureBox();

        protected override bool DoubleBuffered => true;

        public Form1()
        {
            StartPosition = FormStartPosition.CenterScreen;
            Width = screenCaptureArea.Width;
            Height = screenCaptureArea.Height;

            pictureBox = new PictureBox
            {
                Image = ScreenCaptureHelper.CaptureScreenArea(screenCaptureArea),
                Width = screenCaptureArea.Width,
                Height = screenCaptureArea.Height
            };

            this.Controls.Add(pictureBox);

            var timer = new Timer { Interval = 30 };

            timer.Tick += TimerOnTick;

            timer.Start();
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            pictureBox.Image = ScreenCaptureHelper.CaptureScreenArea(screenCaptureArea);

            GC.Collect();
        }
    }

    public static class ScreenCaptureHelper
    {
        public static Bitmap CaptureScreenArea(Rectangle captureArea)
        {
            var bmp = new Bitmap(captureArea.Width, captureArea.Height, PixelFormat.Format32bppArgb);
            var g = Graphics.FromImage(bmp);

            g.CopyFromScreen(captureArea.Left, captureArea.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);

            var src = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmp);
            var dst = new Mat();
            src.CopyTo(dst);
            //src = src.GaussianBlur(new OpenCvSharp.Size(7, 9),0);
            src = src.CvtColor(ColorConversionCodes.BGR2GRAY);
            src = src.Canny(50, 75);

            var lines = src.HoughLinesP(1, Math.PI / 180, 50);
            //var Blank = new Mat(src.Rows, src.Cols, MatType.CV_8UC3, new Scalar(0, 0, 0));

            foreach (var line in lines)
            {
                //Cv2.Line(src, line.P1, line.P2, new Scalar(0, 0, 0), 2, LineTypes.AntiAlias);
                Cv2.Line(dst, line.P1, line.P2, new Scalar(255, 255, 255), 2, LineTypes.AntiAlias);
            }

            //src = src.AdjustROI(100, 640, 100, 700);

            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
        }
    }
}
