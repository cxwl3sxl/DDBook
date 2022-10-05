using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace DDBook
{
    internal class MyPictureBox : Panel
    {
        private string _picFile;
        private string _pageDir;
        private int _curX, _curY, _lastX, _lastY;
        private bool _isMouseDown;
        private readonly Pen _pen;
        private float _systemDpiX, _systemDpiY;
        private readonly PageInfo _pageInfo = new PageInfo();
        private DdBlock _currentBlock;
        private bool _pageSaved;

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

            if (_currentBlock != null && _currentBlock?.Rectangle != Rectangle.Empty)
                Invalidate(_currentBlock!.Rectangle);
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
            _pageInfo.Width = Width;
            _pageInfo.Height = Height;

            _pageInfo.Blocks.Clear();
            var xyFile = Path.Combine(dir, "XY.json");
            if (File.Exists(xyFile))
            {
                var pageInfo = JsonConvert.DeserializeObject<PageInfo>(File.ReadAllText(xyFile));
                if (pageInfo != null)
                {
                    _pageInfo.Blocks.AddRange(pageInfo.Blocks);
                }
            }

            NewBlock();

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
            if (_pageInfo == null) return;
            var gtx = BufferedGraphicsManager.Current;
            var buffer = gtx.Allocate(pe.Graphics, new Rectangle(0, 0, Width, Height));
            using var g = buffer.Graphics;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality; //高质量低速度呈现
            g.SmoothingMode = SmoothingMode.HighQuality; // 指定高质量、低速度呈现。
            g.Clear(Color.DarkCyan);
            using var image = Image.FromFile(_picFile);
            g.DrawImage(image, new Point(0, 0));

            foreach (var block in _pageInfo.Blocks)
            {
                g.DrawRectangle(_pen, block.Rectangle);
            }

            if (_isMouseDown && _currentBlock != null)
            {
                var x = _curX < _lastX ? _curX : _lastX;
                var y = _curY < _lastY ? _curY : _lastY;
                _currentBlock.Rectangle = new Rectangle(x, y, Math.Abs(_curX - _lastX), Math.Abs(_curY - _lastY));
                g.DrawRectangle(_pen, _currentBlock.Rectangle);
            }

            buffer.Render(pe.Graphics);
            buffer.Dispose();
        }

        #endregion

        #region 截图OCR

        async void ScreenShotAndOcr()
        {
            if (_currentBlock == null) return;
            if (_currentBlock?.Rectangle == Rectangle.Empty) return;
            OnMessage?.Invoke("正在处理...");
            using var img = new Bitmap(_currentBlock!.Rectangle.Width, _currentBlock.Rectangle.Height);
            using var g = Graphics.FromImage(img);
            using var image = Image.FromFile(_picFile);
            var srcRectangle = new Rectangle(
                (int)(_currentBlock.Rectangle.X * image.HorizontalResolution / _systemDpiX),
                (int)(_currentBlock.Rectangle.Y * image.VerticalResolution / _systemDpiY),
                (int)(_currentBlock.Rectangle.Width * image.HorizontalResolution / _systemDpiX),
                (int)(_currentBlock.Rectangle.Height * image.VerticalResolution / _systemDpiY));
            g.DrawImage(image,
                new Rectangle(0, 0, _currentBlock.Rectangle.Width, _currentBlock.Rectangle.Height),
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
            if (_currentBlock == null) return null;
            var mp3 = Path.Combine(_pageDir, $"{_currentBlock.Id}.mp3");
            using var fs = File.Create(mp3);
            dataData.WriteTo(fs);
            fs.Close();
            return mp3;
        }

        #endregion

        #region 区块操作

        public void NewBlock()
        {
            _currentBlock = new DdBlock()
            {
                Id = Guid.NewGuid()
            };
            _pageInfo.Blocks.Add(_currentBlock);
        }

        public void SaveBlock()
        {
            if (_currentBlock == null) return;
            if (_currentBlock.Rectangle == Rectangle.Empty) return;
            if (!File.Exists(Path.Combine(_pageDir, $"{_currentBlock.Id}.mp3"))) throw new Exception("尚未合成录音，无法保存！");
        }

        public void DeleteBlock()
        {
            if (_currentBlock == null) return;
            var target = _pageInfo.Blocks.FirstOrDefault(a => a.Id == _currentBlock.Id);
            if (target == null) return;
            _pageInfo.Blocks.Remove(target);
        }

        public void SavePage()
        {
            if (_pageSaved) return;
            if (_pageDir == null) return;
            if (!Directory.Exists(_pageDir)) return;
            SaveBlock();
            //var sb = new StringBuilder();
            //var index = 1;
            //sb.AppendLine("#");
            //foreach (var block in _pageInfo.Blocks)
            //{
            //    var lx = block.Rectangle.X * 1.0f / Width;
            //    var ly = block.Rectangle.Y * 1.0f / Height;
            //    var rx = (block.Rectangle.X + block.Rectangle.Width) * 1.0f / Width;
            //    var ry = (block.Rectangle.Y + block.Rectangle.Height) * 1.0f / Height;

            //    sb.AppendLine(
            //        $"{FormatPoint(lx)},{FormatPoint(ly)},{FormatPoint(rx)},{FormatPoint(ry)}");
            //    index++;
            //}
            var emptyBlocks = _pageInfo.Blocks.Where(a => a.Rectangle == Rectangle.Empty).ToArray();
            foreach (var block in emptyBlocks)
            {
                _pageInfo.Blocks.Remove(block);
            }

            File.WriteAllText(Path.Combine(_pageDir, "XY.json"), JsonConvert.SerializeObject(_pageInfo));
            _pageSaved = true;
        }

        //string FormatPoint(float point)
        //{
        //    return $".{$"{point:F3}".Split('.')[1]}";
        //}

        #endregion

    }
}
