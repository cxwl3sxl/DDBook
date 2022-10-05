using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
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
        private readonly List<DDBlock> _blocks = new List<DDBlock>();
        private string _lastMp3;
        private bool _pageSaved = false;

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
            SavePage();

            _pageSaved = false;
            _picFile = Path.Combine(dir, "pic.jpg");
            if (!File.Exists(_picFile)) return false;
            _pageDir = dir;

            var currentGraphics = Graphics.FromHwnd(FindForm()!.Handle);
            _systemDpiX = currentGraphics.DpiX;
            _systemDpiY = currentGraphics.DpiY;
            using var image = Image.FromFile(_picFile);
            Width = (int)(image.Width * _systemDpiX / image.HorizontalResolution);
            Height = (int)(image.Height * _systemDpiY / image.VerticalResolution);

            _blocks.Clear();
            var xyFile = Path.Combine(dir, "XY.txt");
            if (File.Exists(xyFile))
            {
                var sr = new StreamReader(xyFile);
                string line;
                do
                {
                    line = sr.ReadLine();
                } while (!string.IsNullOrWhiteSpace(line));
                sr.Close();
                sr.Dispose();
            }

            Invalidate();

            return true;
        }

        public string GetPic()
        {
            return _picFile;
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

            foreach (var block in _blocks)
            {
                g.DrawRectangle(_pen, block.Rectangle);
            }

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

        #region 截图OCR

        async void ScreenShotAndOcr()
        {
            if (_lastDrawRectangle == Rectangle.Empty) return;
            OnMessage?.Invoke("正在处理...");
            using var img = new Bitmap(_lastDrawRectangle.Width, _lastDrawRectangle.Height);
            using var g = Graphics.FromImage(img);
            using var image = Image.FromFile(_picFile);
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

        #endregion

        #region 保存当前区块对应的MP3

        public string SaveMp3(MemoryStream dataData)
        {
            _lastMp3 = Path.Combine(_pageDir, $"{Guid.NewGuid()}.mp3");
            using var fs = File.Create(_lastMp3);
            dataData.WriteTo(fs);
            fs.Close();
            return _lastMp3;
        }

        #endregion

        #region 区块操作

        public void NewBlock()
        {
            _lastMp3 = null;
            _lastDrawRectangle = Rectangle.Empty;
        }

        public void SaveBlock()
        {
            if (_lastDrawRectangle == Rectangle.Empty) return;
            if (string.IsNullOrWhiteSpace(_lastMp3)) throw new Exception("尚未合成录音，无法保存！");
            if (!File.Exists(_lastMp3)) throw new Exception("尚未合成录音，无法保存！");
            var lx = _lastDrawRectangle.X * 1.0f / Width;
            var ly = _lastDrawRectangle.Y * 1.0f / Height;
            var rx = (_lastDrawRectangle.X + _lastDrawRectangle.Width) * 1.0f / Width;
            var ry = (_lastDrawRectangle.Y + _lastDrawRectangle.Height) * 1.0f / Height;
            _blocks.Add(new DDBlock()
            {
                Mp3 = _lastMp3,
                Rectangle = _lastDrawRectangle,
                LeftTop = new DDPoint()
                {
                    X = lx,
                    Y = ly
                },
                RightTop = new DDPoint()
                {
                    X = rx,
                    Y = ry
                }
            });
            _lastDrawRectangle = Rectangle.Empty;
        }

        public void DeleteBlock()
        {
            _lastMp3 = null;
            _lastDrawRectangle = Rectangle.Empty;
        }

        public void SavePage()
        {
            if (_pageSaved) return;
            if (_pageDir == null) return;
            if (!Directory.Exists(_pageDir)) return;
            SaveBlock();
            var sb = new StringBuilder();
            var index = 1;
            foreach (var block in _blocks)
            {
                sb.AppendLine(
                    $"#{block.Rectangle.X},{block.Rectangle.Y},{block.Rectangle.Width},{block.Rectangle.Height}");
                sb.AppendLine($"{block.LeftTop.X},{block.LeftTop.Y},{block.RightTop.X},{block.RightTop.Y}");
                var targetMp3 = Path.Combine(Path.GetDirectoryName(block.Mp3)!, $"{index}.mp3");
                if (File.Exists(targetMp3)) File.Delete(targetMp3);
                File.Move(block.Mp3, targetMp3);
                block.Mp3 = targetMp3;
                index++;
            }

            File.WriteAllText(Path.Combine(_pageDir, "XY.txt"), sb.ToString());
            _pageSaved = true;
        }

        #endregion

    }

    class DDBlock
    {
        public DDPoint LeftTop { get; set; }

        public DDPoint RightTop { get; set; }

        public Rectangle Rectangle { get; set; }

        public string Mp3 { get; set; }
    }

    class DDPoint
    {

        public float X { get; set; }

        public float Y { get; set; }
    }
}
