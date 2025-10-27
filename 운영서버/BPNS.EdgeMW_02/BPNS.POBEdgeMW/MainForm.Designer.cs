namespace BPNS.EdgeMW
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pnlTit = new System.Windows.Forms.Panel();
            this.chkLog = new System.Windows.Forms.CheckBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.pnlArea = new System.Windows.Forms.Panel();
            this.logListBox = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.pnlTit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pnlTit);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(2377, 796);
            this.splitContainer1.SplitterDistance = 112;
            this.splitContainer1.SplitterWidth = 6;
            this.splitContainer1.TabIndex = 0;
            // 
            // pnlTit
            // 
            this.pnlTit.BackColor = System.Drawing.Color.Black;
            this.pnlTit.Controls.Add(this.chkLog);
            this.pnlTit.Controls.Add(this.btnStop);
            this.pnlTit.Controls.Add(this.btnStart);
            this.pnlTit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTit.Location = new System.Drawing.Point(0, 0);
            this.pnlTit.Margin = new System.Windows.Forms.Padding(4);
            this.pnlTit.Name = "pnlTit";
            this.pnlTit.Size = new System.Drawing.Size(2377, 112);
            this.pnlTit.TabIndex = 5;
            // 
            // chkLog
            // 
            this.chkLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkLog.AutoSize = true;
            this.chkLog.ForeColor = System.Drawing.SystemColors.Window;
            this.chkLog.Location = new System.Drawing.Point(1861, 39);
            this.chkLog.Margin = new System.Windows.Forms.Padding(4);
            this.chkLog.Name = "chkLog";
            this.chkLog.Size = new System.Drawing.Size(168, 22);
            this.chkLog.TabIndex = 4;
            this.chkLog.Text = "로그 정보 SHOW";
            this.chkLog.UseVisualStyleBackColor = true;
            // 
            // btnStop
            // 
            this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnStop.Enabled = false;
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnStop.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStop.ForeColor = System.Drawing.Color.Lime;
            this.btnStop.Location = new System.Drawing.Point(2213, 29);
            this.btnStop.Margin = new System.Windows.Forms.Padding(4);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(153, 38);
            this.btnStop.TabIndex = 3;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnStart.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStart.ForeColor = System.Drawing.Color.Lime;
            this.btnStart.Location = new System.Drawing.Point(2037, 29);
            this.btnStart.Margin = new System.Windows.Forms.Padding(4);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(153, 38);
            this.btnStart.TabIndex = 2;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.pnlArea);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.logListBox);
            this.splitContainer2.Size = new System.Drawing.Size(2377, 678);
            this.splitContainer2.SplitterDistance = 1177;
            this.splitContainer2.SplitterWidth = 6;
            this.splitContainer2.TabIndex = 0;
            // 
            // pnlArea
            // 
            this.pnlArea.AutoScroll = true;
            this.pnlArea.BackColor = System.Drawing.Color.Black;
            this.pnlArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlArea.Location = new System.Drawing.Point(0, 0);
            this.pnlArea.Margin = new System.Windows.Forms.Padding(4);
            this.pnlArea.Name = "pnlArea";
            this.pnlArea.Size = new System.Drawing.Size(1177, 678);
            this.pnlArea.TabIndex = 3;
            // 
            // logListBox
            // 
            this.logListBox.BackColor = System.Drawing.Color.Black;
            this.logListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.logListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logListBox.Font = new System.Drawing.Font("돋움체", 9F);
            this.logListBox.ForeColor = System.Drawing.Color.White;
            this.logListBox.FormattingEnabled = true;
            this.logListBox.ItemHeight = 18;
            this.logListBox.Location = new System.Drawing.Point(0, 0);
            this.logListBox.Margin = new System.Windows.Forms.Padding(4);
            this.logListBox.Name = "logListBox";
            this.logListBox.Size = new System.Drawing.Size(1194, 678);
            this.logListBox.TabIndex = 5;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2377, 796);
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "Edge M/W";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.pnlTit.ResumeLayout(false);
            this.pnlTit.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel pnlTit;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Panel pnlArea;
        private System.Windows.Forms.CheckBox chkLog;
        private System.Windows.Forms.ListBox logListBox;
    }
}

