using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DDBook
{
    internal class MyPictureBox : Panel
    {
        private string _picFile;

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
            g.Clear(Color.BurlyWood);
            using var image = System.Drawing.Image.FromFile(_picFile);
            g.DrawImage(image, new Point(0, 0));
        }
    }
}
