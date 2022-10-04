using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DDBook.EdgeTTS;
using NAudio.Wave;
using Newtonsoft.Json;
using PdfiumViewer;

namespace DDBook
{
    public partial class Form1 : Form
    {
        private Project _currentProject;
        private string _projFile;
        private readonly EdgeTTS.EdgeTTS _tts = new EdgeTTS.EdgeTTS();
        private Voice _choosedVoice;
        private readonly WaveOutEvent _wo = new WaveOutEvent();
        private AudioFileReader _audioFileReader;

        public Form1()
        {
            InitializeComponent();
            myPictureBox1.OnMessage += ShowInfo;
            myPictureBox1.OnOcr += MyPictureBox1_OnOcr;
            myPictureBox1.OnResult += MyPictureBox1_OnResult;
            _wo.PlaybackStopped += _wo_PlaybackStopped;
        }

        #region 事件处理

        private void _wo_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            Invoke(new Action(() =>
            {
                _audioFileReader?.Close();
                _audioFileReader?.Dispose();
                btnStop.Enabled = false;
            }));
        }

        private void MyPictureBox1_OnResult(string obj)
        {
            tbOcrResult.Text = obj;
            btnPlay.Enabled = !string.IsNullOrWhiteSpace(obj);
            ShowSuccess("识别完成");
        }

        private void MyPictureBox1_OnOcr(string obj)
        {
            pbOcr.ImageLocation = obj;
        }

        #endregion

        #region 打开项目

        async void ProcessProj(Project project)
        {
            if (!File.Exists(project.Pdf))
            {
                ShowError($"原始PDF文件不存在：{project.Pdf}");
                return;
            }

            _currentProject = project;

            ShowInfo("正在生成页面数据，请稍后...");
            var total = await CheckImage(project.Pdf, project.WorkingDir);

            if (project.Total != 0 && project.Total != total)
            {
                ShowError("检测到PDF文件页数和上次使用的不一致.");
                _currentProject = null;
                return;
            }

            _currentProject.Total = total;

            ShowCurrentPage();

            gbControl.Enabled = true;
        }

        void ShowCurrentPage()
        {
            var pageDir = Path.Combine(_currentProject.WorkingDir, $"{_currentProject.CurrentProcessPage + 1}");
            if (!myPictureBox1.LoadDir(pageDir))
            {
                ShowError("页面文件不存在！！！");
                return;
            }

            ShowInfo($"正在处理第{_currentProject.CurrentProcessPage + 1}页");
            lbPageInfo.Text = $"共计{_currentProject.Total}页，当前{_currentProject.CurrentProcessPage + 1}页";
            btnPrePage.Enabled = _currentProject.CurrentProcessPage > 0;
            btnNextPage.Enabled = _currentProject.CurrentProcessPage < _currentProject.Total - 1;
        }

        #endregion

        #region 拖拽打开项目或新建项目

        void CreateNewProj(string pdf)
        {
            var dlg = new FolderBrowserDialog()
            {
                Description = "选择点读教程项目保存位置",
                RootFolder = Environment.SpecialFolder.Desktop,
                ShowNewFolderButton = true
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            var proj = new Project()
            {
                Pdf = pdf,
                WorkingDir = Path.Combine(dlg.SelectedPath, "Pages")
            };

            _projFile = Path.Combine(dlg.SelectedPath, $"{Path.GetFileNameWithoutExtension(pdf)}.ddbook");

            ProcessProj(proj);
        }

        void OpenProj(string ddbook)
        {
            Project proj;

            try
            {
                var content = File.ReadAllText(ddbook);
                proj = JsonConvert.DeserializeObject<Project>(content);
                if (proj == null)
                {
                    ShowError("无效的项目文件");
                    return;
                }
            }
            catch (Exception ex)
            {
                ShowError($"无效的项目文件:{ex.Message}");
                return;
            }

            _projFile = ddbook;

            ProcessProj(proj);
        }


        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            //string[] FileName = new string[]
            //    { ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString() };
            if (!(e.Data.GetData(DataFormats.FileDrop) is string[] files))
            {
                return;
            }

            if (files.Length != 1) return;
            var file = files[0];
            var ext = Path.GetExtension(file).ToLower();
            switch (ext)
            {
                case ".pdf":
                    CreateNewProj(file);
                    break;
                case ".ddbook":
                    OpenProj(file);
                    break;
                default:
                    ShowError("拖入的文件必须是pdf格式");
                    break;
            }
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
            var ext = Path.GetExtension(file).ToLower();
            if (ext != ".pdf" && ext != ".ddbook")
            {
                ShowError("拖入的文件必须是pdf格式");
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = DragDropEffects.All; //重要代码：表明是所有类型的数据，比如文件路径
        }

        Task<int> CheckImage(string pdf, string workingDir)
        {
            return Task.Run(() =>
            {
                using var document = PdfDocument.Load(pdf);
                var pageCount = document.PageCount;
                for (var i = 0; i < pageCount; i++)
                {
                    var targetPic = Path.Combine(workingDir, $"{i + 1}", "pic.jpg");
                    if (File.Exists(targetPic)) continue;

                    var dir = Path.GetDirectoryName(targetPic);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                    var dpi = 300;
                    using var image = document.Render(i, dpi, dpi, PdfRenderFlags.CorrectFromDpi);
                    var encoder = ImageCodecInfo.GetImageEncoders()
                        .First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                    var encParams = new EncoderParameters(1);
                    encParams.Param[0] = new EncoderParameter(Encoder.Quality, 10L);

                    image.Save(targetPic, encoder, encParams);
                }

                return pageCount;
            });
        }

        #endregion

        #region 显示消息

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

        #endregion

        #region 保存项目

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("确定要退出么？", "温馨提示", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            if (_currentProject != null)
            {
                File.WriteAllText(_projFile, JsonConvert.SerializeObject(_currentProject));
            }

            _tts.Dispose();
        }

        #endregion

        #region 翻页

        private void btnPrePage_Click(object sender, EventArgs e)
        {
            if (_currentProject.CurrentProcessPage == 0) return;
            _currentProject.CurrentProcessPage--;
            ShowCurrentPage();
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            if (_currentProject.CurrentProcessPage >= _currentProject.Total - 1) return;
            _currentProject.CurrentProcessPage++;
            ShowCurrentPage();
        }


        #endregion

        #region 播放和停止

        private async void btnPlay_Click(object sender, EventArgs e)
        {
            try
            {
                btnPlay.Enabled = false;
                var data = await _tts.SynthesisAsync(tbOcrResult.Text, ShowInfo, _choosedVoice.ShortName);
                if (data?.Code == ResultCode.Success)
                {
                    var mp3 = myPictureBox1.SaveMp3(data.Data);
                    _audioFileReader = new AudioFileReader(mp3);
                    _wo.Init(_audioFileReader);
                    btnStop.Enabled = true;
                    _wo.Play();
                }
                else
                {
                    ShowError(data?.Message ?? "未知错误，请重试");
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
            finally
            {
                btnPlay.Enabled = true;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _wo.Stop();
        }

        #endregion

        #region TTS设置

        private void Form1_Load(object sender, EventArgs e)
        {
            var voices = _tts.GetVoiceList();
            if (voices == null) return;
            foreach (var voice in voices)
            {
                if (voice.Locale == "en-GB" || voice.Locale == "en-US")
                {
                    cbVoiceList.Items.Add(voice);
                }
            }

            cbVoiceList.SelectedItem =
                _choosedVoice = voices.FirstOrDefault(a => a.ShortName == "en-US-MichelleNeural");
        }

        private void cbVoiceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            _choosedVoice = cbVoiceList.SelectedItem as Voice;
        }

        #endregion

        #region 区块操作

        private void btnNewRect_Click(object sender, EventArgs e)
        {
            myPictureBox1.NewBlock();
            ShowInfo("可以在图片上绘制新点读区了");
        }

        private void btnSaveRect_Click(object sender, EventArgs e)
        {
            try
            {
                myPictureBox1.SaveBlock();
                ShowSuccess("点读区域保存成功");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            myPictureBox1.DeleteBlock();
            ShowInfo("点读区域已经删除");
        }

        private void btnSavePage_Click(object sender, EventArgs e)
        {
            try
            {
                myPictureBox1.SavePage();
                ShowInfo("保存页成功");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        #endregion


    }
}
