using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace DDBook
{
    internal class MyPictureBox : Panel
    {
        private string _picFile;
        private string _pageDir;
        private int _curX, _curY, _lastX, _lastY;
        private bool _isMouseDown;
        private readonly Pen _pen;
        private Rectangle _lastDrawRectangle;
        private float _systemDpiX, _systemDpiY;


        public event Action<string> OnMessage;
        public event Action<string> OnOcr;
        public event Action<string> OnResult; 

        #region 构造函数

        public MyPictureBox()
        {
            _pen = new Pen(Color.Red);
            MouseDown += MyPictureBox_MouseDown;
            MouseMove += MyPictureBox_MouseMove;
            MouseUp += MyPictureBox_MouseUp;
        }

        #endregion

        #region 鼠标选取区域

        private void MyPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            _isMouseDown = false;
            ScreenShotAndOcr();
        }

        private void MyPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isMouseDown) return;
            _lastX = e.X;
            _lastY = e.Y;

            if (_lastDrawRectangle != Rectangle.Empty) Invalidate(_lastDrawRectangle);
            var x = _curX < _lastX ? _curX : _lastX;
            var y = _curY < _lastY ? _curY : _lastY;
            Invalidate(new Rectangle(x, y, Math.Abs(_curX - _lastX), Math.Abs(_curY - _lastY)));
        }

        private void MyPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            _curX = e.X;
            _curY = e.Y;
            _isMouseDown = true;
        }

        #endregion

        #region 加载点读页

        public bool LoadDir(string dir)
        {
            _picFile = Path.Combine(dir, "pic.jpg");
            if (!File.Exists(_picFile)) return false;
            _pageDir = dir;

            var currentGraphics = Graphics.FromHwnd(FindForm()!.Handle);
            _systemDpiX = currentGraphics.DpiX;
            _systemDpiY = currentGraphics.DpiY;
            using var image = Image.FromFile(_picFile);
            Width = (int)(image.Width * _systemDpiX / image.HorizontalResolution);
            Height = (int)(image.Height * _systemDpiY / image.VerticalResolution);

            Invalidate();

            return true;
        }

        #endregion

        #region 界面绘制

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (_picFile == null) return;
            if (!File.Exists(_picFile)) return;
            var gtx = BufferedGraphicsManager.Current;
            var buffer = gtx.Allocate(pe.Graphics, new Rectangle(0, 0, Width, Height));
            using var g = buffer.Graphics;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality; //高质量低速度呈现
            g.SmoothingMode = SmoothingMode.HighQuality; // 指定高质量、低速度呈现。
            g.Clear(Color.DarkCyan);
            using var image = Image.FromFile(_picFile);
            g.DrawImage(image, new Point(0, 0));

            if (_isMouseDown)
            {
                var x = _curX < _lastX ? _curX : _lastX;
                var y = _curY < _lastY ? _curY : _lastY;
                _lastDrawRectangle = new Rectangle(x, y, Math.Abs(_curX - _lastX), Math.Abs(_curY - _lastY));
                g.DrawRectangle(_pen, _lastDrawRectangle);
            }

            buffer.Render(pe.Graphics);
            buffer.Dispose();
        }

        #endregion

        async void ScreenShotAndOcr()
        {
            if (_lastDrawRectangle == Rectangle.Empty) return;
            OnMessage?.Invoke("正在处理...");
            using var img = new Bitmap(_lastDrawRectangle.Width, _lastDrawRectangle.Height);
            using var g = Graphics.FromImage(img);
            using var image = Image.FromFile(_picFile);
            /*
             *    Width = (int)(image.Width * currentGraphics.DpiX / image.HorizontalResolution);
            Height = (int)(image.Height * currentGraphics.DpiY / image.VerticalResolution);
             */
            var srcRectangle = new Rectangle(
                (int)(_lastDrawRectangle.X * image.HorizontalResolution / _systemDpiX),
                (int)(_lastDrawRectangle.Y * image.VerticalResolution / _systemDpiY),
                (int)(_lastDrawRectangle.Width * image.HorizontalResolution / _systemDpiX),
                (int)(_lastDrawRectangle.Height * image.VerticalResolution / _systemDpiY));
            g.DrawImage(image,
                new Rectangle(0, 0, _lastDrawRectangle.Width, _lastDrawRectangle.Height),
                srcRectangle,
                GraphicsUnit.Pixel);
            var ocrImage = Path.Combine(_pageDir, "tmp.jpg");
            img.Save(ocrImage, ImageFormat.Jpeg);
            OnMessage?.Invoke("正在识别...");
            OnOcr?.Invoke(ocrImage);
            var result = await PpOcr.Detect(ocrImage);
            OnResult?.Invoke(result);
        }
    }
}
