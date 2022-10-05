using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DDBook.EdgeTTS;
using ICSharpCode.SharpZipLib.Zip;
using NAudio.Wave;
using Newtonsoft.Json;
using PdfiumViewer;
using Encoder = System.Drawing.Imaging.Encoder;
// ReSharper disable All

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
            tbBookName.Text = _currentProject.BookName;

            ShowInfo("正在生成页面数据，请稍后...");
            SetBusy(true);

            try
            {
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
            finally
            {
                SetBusy(false);
            }
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
                    ShowInfo($"正在生成页面数据[第{i + 1}页]，请稍后...");
                    var targetPic = Path.Combine(workingDir, $"{i + 1}", "pic.jpg");
                    if (File.Exists(targetPic)) continue;

                    var dir = Path.GetDirectoryName(targetPic)!;
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

        void SetBusy(bool isBusy)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    SetBusy(isBusy);
                }));
                return;
            }

            toolStripProgressBar1.Visible = isBusy;
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
                SetBusy(true);
                btnPlay.Enabled = false;
                var data = await _tts.SynthesisAsync(tbOcrResult.Text, ShowInfo, _choosedVoice.ShortName);
                if (data?.Code == ResultCode.Success)
                {
                    var mp3 = myPictureBox1.SaveMp3(data.Data);
                    if (string.IsNullOrWhiteSpace(mp3)) return;
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
                SetBusy(false);
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
                _choosedVoice = voices.FirstOrDefault(a => a.ShortName == "en-GB-LibbyNeural");
        }

        private void cbVoiceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            _choosedVoice = cbVoiceList.SelectedItem as Voice;
        }

        #endregion

        #region 区块操作

        private void btnSaveRect_Click(object sender, EventArgs e)
        {
            try
            {
                myPictureBox1.SaveBlock();
                ShowSuccess("点读区域保存成功");
                myPictureBox1.NewBlock();
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

        #region 课本相关

        private void btnSetAsCover_Click(object sender, EventArgs e)
        {
            var img = myPictureBox1.GetPic();
            if (string.IsNullOrWhiteSpace(img))
            {
                ShowError("当前未选中任何页");
                return;
            }

            _currentProject.CoverImgPage = _currentProject.CurrentProcessPage + 1;

            ShowSuccess("封面设置成功！");
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentProject.BookName))
            {
                ShowError("请设置教材名称");
                return;
            }

            try
            {
                SetBusy(true);
                tbBookName.Enabled = false;
                ShowInfo("正在导出");

                var zipFile = Path.Combine(_currentProject.WorkingDir,
                    $"{_currentProject.BookName}.ddt");
                using var zip = new ZipOutputStream(File.Create(zipFile));
                var bookCover = Path.Combine(_currentProject.WorkingDir, $"{_currentProject.CoverImgPage}", "pic.jpg");
                var newBookCover = Path.Combine(_currentProject.WorkingDir, $"{_currentProject.BookName}.jpg");
                if (File.Exists(bookCover))
                {
                    if (File.Exists(newBookCover)) File.Delete(newBookCover);
                    File.Move(bookCover, newBookCover);
                }

                ShowInfo("正在导出[正在写入封面]");
                if (!File.Exists(newBookCover))
                {
                    if (MessageBox.Show("当前教材尚未设置封面，是否继续？", "温馨提示", MessageBoxButtons.YesNo) == DialogResult.No) return;
                }

                WriteFile(zip, newBookCover);

                var dirInfo = new StringBuilder();

                for (var i = 1;; i++)
                {
                    var pageDir = Path.Combine(_currentProject.WorkingDir, $"{i}");
                    if (!Directory.Exists(pageDir)) break;

                    ShowInfo($"正在导出[正在写入第{i}页]");
                    WriteFile(zip, Path.Combine(pageDir, "pic.jpg"));

                    var pageInfoFile = Path.Combine(pageDir, "XY.json");
                    var blocks = new StringBuilder();
                    blocks.AppendLine("#");

                    if (File.Exists(pageInfoFile))
                    {
                        var pageInfo = JsonConvert.DeserializeObject<PageInfo>(File.ReadAllText(pageInfoFile));
                        if (pageInfo != null)
                        {
                            var index = 1;
                            foreach (var block in pageInfo.Blocks)
                            {
                                var mp3 = Path.Combine(pageDir, $"{block.Id}.mp3");
                                if (!File.Exists(mp3)) continue;
                                var lx = block.Rectangle.X * 1.0f / pageInfo.Width;
                                var ly = block.Rectangle.Y * 1.0f / pageInfo.Height;
                                var rx = (block.Rectangle.X + block.Rectangle.Width) * 1.0f / pageInfo.Width;
                                var ry = (block.Rectangle.Y + block.Rectangle.Height) * 1.0f / pageInfo.Height;

                                blocks.AppendLine(
                                    $"{FormatPoint(lx)},{FormatPoint(ly)},{FormatPoint(rx)},{FormatPoint(ry)}");

                                WriteFile(zip,
                                    mp3,
                                    Path
                                        .Combine(pageDir, $"{index}.mp3")
                                        .Replace(_currentProject.WorkingDir, _currentProject.BookName));

                                index++;
                            }
                        }
                    }

                    var xyTxt = Path.Combine(pageDir, "XY.txt");
                    File.WriteAllText(xyTxt, blocks.ToString());

                    WriteFile(zip, xyTxt);

                    dirInfo.AppendLine($"{i}");
                }

                ShowInfo("正在导出[正在写入目录信息]");
                var dirFile = Path.Combine(_currentProject.WorkingDir, "DirList.txt");
                File.WriteAllText(dirFile, dirInfo.ToString());
                WriteFile(zip, dirFile);
                zip.Close();
                zip.Dispose();

                var targetFile = Path.Combine(Path.GetDirectoryName(_projFile), Path.GetFileName(zipFile));
                if (File.Exists(targetFile))
                {
                    if (MessageBox.Show("指定的课本已经存在，是否覆盖？", "询问", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    {
                        ShowInfo("导出课本已经取消");
                        return;
                    }

                    File.Delete(targetFile);
                }

                File.Move(zipFile,targetFile );

                ShowSuccess("教材写入成功");
            }
            finally
            {
                SetBusy(false);
                tbBookName.Enabled = true;
            }
        }

        string FormatPoint(float point)
        {
            return $".{$"{point:F3}".Split('.')[1]}";
        }

        void WriteFile(ZipOutputStream zip, string file, string fileInZip = null)
        {
            if (!File.Exists(file)) return;
            var buffer = new byte[4096]; //缓冲区大小
            fileInZip ??= file.Replace(_currentProject.WorkingDir, _currentProject.BookName);
            var entry = new ZipEntry(fileInZip)
            {
                DateTime = DateTime.Now
            };
            zip.PutNextEntry(entry);
            using var fs = File.OpenRead(file);
            int sourceBytes;
            do
            {
                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                zip.Write(buffer, 0, sourceBytes);
            } while (sourceBytes > 0);

            fs.Close();
        }


        #endregion

        private void tbBookName_TextChanged(object sender, EventArgs e)
        {
            _currentProject.BookName = tbBookName.Text;
        }
    }
}
