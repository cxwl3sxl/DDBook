using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DDBook
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] FileName = new string[]
                { ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString() };
            lbResult.Text = string.Join(",", FileName);
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            ShowInfo("");
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            if (!(e.Data.GetData(DataFormats.FileDrop) is string[] files))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            if (files.Length != 1)
            {
                ShowError("同时只支持处理一个文件");
                e.Effect = DragDropEffects.None;
                return;
            }

            var file = files[0];
            if (Path.GetExtension(file) != ".pdf")
            {
                ShowError("拖入的文件必须是pdf格式");
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = DragDropEffects.All; //重要代码：表明是所有类型的数据，比如文件路径
        }

        void ShowMessage(string msg, Color color)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => { ShowMessage(msg, color); }));
                return;
            }

            lbMessage.ForeColor = color;
            lbMessage.Text = msg;
        }

        void ShowError(string msg)
        {
            ShowMessage(msg, Color.Red);
        }

        void ShowSuccess(string msg)
        {
            ShowMessage(msg, Color.Green);
        }

        void ShowInfo(string msg)
        {
            ShowMessage(msg, Color.Black);
        }
    }
}
