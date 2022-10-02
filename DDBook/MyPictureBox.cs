using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DDBook
{
    internal class MyPictureBox : Panel
    {
        private string _picFile;
        private int _curX, _curY, _lastX, _lastY;
        private bool _isMouseDown;
        private readonly Pen _pen;

        public MyPictureBox()
        {
            _pen = new Pen(Color.Red);
            MouseDown += MyPictureBox_MouseDown;
            MouseMove += MyPictureBox_MouseMove;
            MouseUp += MyPictureBox_MouseUp;
        }

        private void MyPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            _isMouseDown = false;
        }

        private void MyPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isMouseDown) return;
            _lastX = e.X;
            _lastY = e.Y;
            Invalidate();
        }

        private void MyPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            _curX = e.X;
            _curY = e.Y;
            _isMouseDown = true;
        }

        public bool LoadDir(string dir)
        {
            _picFile = Path.Combine(dir, "pic.jpg");
            if (!File.Exists(_picFile)) return false;

            var currentGraphics = Graphics.FromHwnd(FindForm()!.Handle);
            using var image = Image.FromFile(_picFile);
            Width = (int)(image.Width * currentGraphics.DpiX / image.HorizontalResolution);
            Height = (int)(image.Height * currentGraphics.DpiY / image.VerticalResolution);

            Invalidate();

            return true;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (_picFile == null) return;
            if (!File.Exists(_picFile)) return;
            using var g = pe.Graphics;
            g.Clear(Color.DarkCyan);
            using var image = System.Drawing.Image.FromFile(_picFile);
            g.DrawImage(image, new Point(0, 0));

            if (_isMouseDown)
            {
                var x = _curX < _lastX ? _curX : _lastX;
                var y = _curY < _lastY ? _curY : _lastY;

                g.DrawRectangle(_pen, x, y, Math.Abs(_curX - _lastX), Math.Abs(_curY - _lastY));
            }
        }
    }
}
