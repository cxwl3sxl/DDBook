namespace DDBook
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pbOcr = new System.Windows.Forms.PictureBox();
            this.tbOcrResult = new System.Windows.Forms.TextBox();
            this.gbControl = new System.Windows.Forms.GroupBox();
            this.btnSavePage = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.cbVoiceList = new System.Windows.Forms.ComboBox();
            this.lbPageInfo = new System.Windows.Forms.Label();
            this.lbMessage = new System.Windows.Forms.Label();
            this.btnSaveRect = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnNewRect = new System.Windows.Forms.Button();
            this.btnNextPage = new System.Windows.Forms.Button();
            this.btnPrePage = new System.Windows.Forms.Button();
            this.myPictureBox1 = new DDBook.MyPictureBox();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbOcr)).BeginInit();
            this.gbControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(655, 610);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "当前页";
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.myPictureBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 17);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(649, 590);
            this.panel1.TabIndex = 1;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.splitContainer1);
            this.groupBox2.Location = new System.Drawing.Point(673, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(474, 451);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "识别结果";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 17);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pbOcr);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tbOcrResult);
            this.splitContainer1.Size = new System.Drawing.Size(468, 431);
            this.splitContainer1.SplitterDistance = 215;
            this.splitContainer1.TabIndex = 2;
            // 
            // pbOcr
            // 
            this.pbOcr.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbOcr.Location = new System.Drawing.Point(0, 0);
            this.pbOcr.Name = "pbOcr";
            this.pbOcr.Size = new System.Drawing.Size(468, 215);
            this.pbOcr.TabIndex = 0;
            this.pbOcr.TabStop = false;
            // 
            // tbOcrResult
            // 
            this.tbOcrResult.AcceptsReturn = true;
            this.tbOcrResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbOcrResult.Location = new System.Drawing.Point(0, 0);
            this.tbOcrResult.Multiline = true;
            this.tbOcrResult.Name = "tbOcrResult";
            this.tbOcrResult.Size = new System.Drawing.Size(468, 212);
            this.tbOcrResult.TabIndex = 0;
            // 
            // gbControl
            // 
            this.gbControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.gbControl.Controls.Add(this.btnSavePage);
            this.gbControl.Controls.Add(this.btnDelete);
            this.gbControl.Controls.Add(this.btnStop);
            this.gbControl.Controls.Add(this.cbVoiceList);
            this.gbControl.Controls.Add(this.lbPageInfo);
            this.gbControl.Controls.Add(this.lbMessage);
            this.gbControl.Controls.Add(this.btnSaveRect);
            this.gbControl.Controls.Add(this.btnPlay);
            this.gbControl.Controls.Add(this.btnNewRect);
            this.gbControl.Controls.Add(this.btnNextPage);
            this.gbControl.Controls.Add(this.btnPrePage);
            this.gbControl.Enabled = false;
            this.gbControl.Location = new System.Drawing.Point(673, 469);
            this.gbControl.Name = "gbControl";
            this.gbControl.Size = new System.Drawing.Size(474, 150);
            this.gbControl.TabIndex = 4;
            this.gbControl.TabStop = false;
            this.gbControl.Text = "控制区";
            // 
            // btnSavePage
            // 
            this.btnSavePage.Location = new System.Drawing.Point(392, 121);
            this.btnSavePage.Name = "btnSavePage";
            this.btnSavePage.Size = new System.Drawing.Size(75, 23);
            this.btnSavePage.TabIndex = 11;
            this.btnSavePage.Text = "保存页";
            this.btnSavePage.UseVisualStyleBackColor = true;
            this.btnSavePage.Click += new System.EventHandler(this.btnSavePage_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(89, 121);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 10;
            this.btnDelete.Text = "删除";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(393, 70);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 9;
            this.btnStop.Text = "停止";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // cbVoiceList
            // 
            this.cbVoiceList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbVoiceList.FormattingEnabled = true;
            this.cbVoiceList.Location = new System.Drawing.Point(6, 72);
            this.cbVoiceList.Name = "cbVoiceList";
            this.cbVoiceList.Size = new System.Drawing.Size(299, 20);
            this.cbVoiceList.TabIndex = 8;
            this.cbVoiceList.SelectedIndexChanged += new System.EventHandler(this.cbVoiceList_SelectedIndexChanged);
            // 
            // lbPageInfo
            // 
            this.lbPageInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbPageInfo.Location = new System.Drawing.Point(6, 43);
            this.lbPageInfo.Name = "lbPageInfo";
            this.lbPageInfo.Size = new System.Drawing.Size(299, 23);
            this.lbPageInfo.TabIndex = 7;
            this.lbPageInfo.Text = "共计N/A页，当前N/A页";
            this.lbPageInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbMessage
            // 
            this.lbMessage.Location = new System.Drawing.Point(6, 17);
            this.lbMessage.Name = "lbMessage";
            this.lbMessage.Size = new System.Drawing.Size(462, 23);
            this.lbMessage.TabIndex = 6;
            this.lbMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnSaveRect
            // 
            this.btnSaveRect.Location = new System.Drawing.Point(311, 121);
            this.btnSaveRect.Name = "btnSaveRect";
            this.btnSaveRect.Size = new System.Drawing.Size(75, 23);
            this.btnSaveRect.TabIndex = 5;
            this.btnSaveRect.Text = "保存读区";
            this.btnSaveRect.UseVisualStyleBackColor = true;
            this.btnSaveRect.Click += new System.EventHandler(this.btnSaveRect_Click);
            // 
            // btnPlay
            // 
            this.btnPlay.Enabled = false;
            this.btnPlay.Location = new System.Drawing.Point(311, 70);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(75, 23);
            this.btnPlay.TabIndex = 4;
            this.btnPlay.Text = "播放";
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // btnNewRect
            // 
            this.btnNewRect.Location = new System.Drawing.Point(8, 121);
            this.btnNewRect.Name = "btnNewRect";
            this.btnNewRect.Size = new System.Drawing.Size(75, 23);
            this.btnNewRect.TabIndex = 3;
            this.btnNewRect.Text = "新区";
            this.btnNewRect.UseVisualStyleBackColor = true;
            this.btnNewRect.Click += new System.EventHandler(this.btnNewRect_Click);
            // 
            // btnNextPage
            // 
            this.btnNextPage.Location = new System.Drawing.Point(392, 43);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(75, 23);
            this.btnNextPage.TabIndex = 2;
            this.btnNextPage.Text = "下一页";
            this.btnNextPage.UseVisualStyleBackColor = true;
            this.btnNextPage.Click += new System.EventHandler(this.btnNextPage_Click);
            // 
            // btnPrePage
            // 
            this.btnPrePage.Location = new System.Drawing.Point(311, 43);
            this.btnPrePage.Name = "btnPrePage";
            this.btnPrePage.Size = new System.Drawing.Size(75, 23);
            this.btnPrePage.TabIndex = 1;
            this.btnPrePage.Text = "上一页";
            this.btnPrePage.UseVisualStyleBackColor = true;
            this.btnPrePage.Click += new System.EventHandler(this.btnPrePage_Click);
            // 
            // myPictureBox1
            // 
            this.myPictureBox1.Location = new System.Drawing.Point(3, 3);
            this.myPictureBox1.Name = "myPictureBox1";
            this.myPictureBox1.Size = new System.Drawing.Size(643, 584);
            this.myPictureBox1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1159, 634);
            this.Controls.Add(this.gbControl);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "点读通教材制作工具";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            this.groupBox1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbOcr)).EndInit();
            this.gbControl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox gbControl;
        private System.Windows.Forms.Button btnNextPage;
        private System.Windows.Forms.Button btnPrePage;
        private System.Windows.Forms.Button btnNewRect;
        private System.Windows.Forms.Button btnSaveRect;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Label lbMessage;
        private System.Windows.Forms.Label lbPageInfo;
        private System.Windows.Forms.Panel panel1;
        private MyPictureBox myPictureBox1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox pbOcr;
        private System.Windows.Forms.TextBox tbOcrResult;
        private System.Windows.Forms.ComboBox cbVoiceList;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnSavePage;
    }
}

