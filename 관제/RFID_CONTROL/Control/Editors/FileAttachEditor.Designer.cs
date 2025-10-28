namespace RFID_CONTROLLER.Controls.Editors {
	partial class FileAttachEditor {
		/// <summary> 
		/// 필수 디자이너 변수입니다.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// 사용 중인 모든 리소스를 정리합니다.
		/// </summary>
		/// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
		protected override void Dispose (bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region 구성 요소 디자이너에서 생성한 코드

		/// <summary> 
		/// 디자이너 지원에 필요한 메서드입니다. 
		/// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
		/// </summary>
		private void InitializeComponent () {
            this.DisplayLabel = new System.Windows.Forms.Label();
            this.MainPanel = new System.Windows.Forms.Panel();
            this.AttachBtn = new DevExpress.XtraEditors.SimpleButton();
            this.BorderPanel = new System.Windows.Forms.Panel();
            this.AttachPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.AttachFileList = new System.Windows.Forms.FlowLayoutPanel();
            this.LabelPanel = new System.Windows.Forms.Panel();
            this.MainPanel.SuspendLayout();
            this.BorderPanel.SuspendLayout();
            this.AttachPanel.SuspendLayout();
            this.LabelPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // DisplayLabel
            // 
            this.DisplayLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.DisplayLabel.Font = new System.Drawing.Font("Dotum", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.DisplayLabel.Location = new System.Drawing.Point(0, 7);
            this.DisplayLabel.Margin = new System.Windows.Forms.Padding(0);
            this.DisplayLabel.MaximumSize = new System.Drawing.Size(0, 13);
            this.DisplayLabel.Name = "DisplayLabel";
            this.DisplayLabel.Size = new System.Drawing.Size(100, 13);
            this.DisplayLabel.TabIndex = 1;
            this.DisplayLabel.Text = "첨부파일";
            this.DisplayLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MainPanel
            // 
            this.MainPanel.AutoSize = true;
            this.MainPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MainPanel.Controls.Add(this.AttachBtn);
            this.MainPanel.Controls.Add(this.BorderPanel);
            this.MainPanel.Controls.Add(this.LabelPanel);
            this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainPanel.Location = new System.Drawing.Point(0, 0);
            this.MainPanel.Margin = new System.Windows.Forms.Padding(0);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(230, 30);
            this.MainPanel.TabIndex = 25;
            // 
            // AttachBtn
            // 
            this.AttachBtn.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.AttachBtn.Appearance.Font = new System.Drawing.Font("Gulim", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.AttachBtn.Appearance.Options.UseFont = true;
            this.AttachBtn.Location = new System.Drawing.Point(203, 3);
            this.AttachBtn.Margin = new System.Windows.Forms.Padding(0);
            this.AttachBtn.MaximumSize = new System.Drawing.Size(25, 25);
            this.AttachBtn.MinimumSize = new System.Drawing.Size(20, 20);
            this.AttachBtn.Name = "AttachBtn";
            this.AttachBtn.Size = new System.Drawing.Size(25, 25);
            this.AttachBtn.TabIndex = 1;
            this.AttachBtn.TabStop = false;
            this.AttachBtn.Text = "+";
            this.AttachBtn.Click += new System.EventHandler(this.AttachBtn_Click);
            // 
            // BorderPanel
            // 
            this.BorderPanel.AutoSize = true;
            this.BorderPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BorderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.BorderPanel.Controls.Add(this.AttachPanel);
            this.BorderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BorderPanel.Location = new System.Drawing.Point(100, 0);
            this.BorderPanel.Margin = new System.Windows.Forms.Padding(0);
            this.BorderPanel.Name = "BorderPanel";
            this.BorderPanel.Size = new System.Drawing.Size(130, 30);
            this.BorderPanel.TabIndex = 26;
            // 
            // AttachPanel
            // 
            this.AttachPanel.AutoScroll = true;
            this.AttachPanel.AutoSize = true;
            this.AttachPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.AttachPanel.BackColor = System.Drawing.Color.White;
            this.AttachPanel.Controls.Add(this.AttachFileList);
            this.AttachPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AttachPanel.Location = new System.Drawing.Point(0, 0);
            this.AttachPanel.Margin = new System.Windows.Forms.Padding(0);
            this.AttachPanel.Name = "AttachPanel";
            this.AttachPanel.Size = new System.Drawing.Size(130, 30);
            this.AttachPanel.TabIndex = 12;
            this.AttachPanel.WrapContents = false;
            // 
            // AttachFileList
            // 
            this.AttachFileList.AutoSize = true;
            this.AttachFileList.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.AttachFileList.BackColor = System.Drawing.Color.Transparent;
            this.AttachFileList.Location = new System.Drawing.Point(0, 0);
            this.AttachFileList.Margin = new System.Windows.Forms.Padding(0);
            this.AttachFileList.MinimumSize = new System.Drawing.Size(0, 27);
            this.AttachFileList.Name = "AttachFileList";
            this.AttachFileList.Size = new System.Drawing.Size(0, 27);
            this.AttachFileList.TabIndex = 11;
            // 
            // LabelPanel
            // 
            this.LabelPanel.AutoSize = true;
            this.LabelPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.LabelPanel.Controls.Add(this.DisplayLabel);
            this.LabelPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.LabelPanel.Location = new System.Drawing.Point(0, 0);
            this.LabelPanel.Margin = new System.Windows.Forms.Padding(0);
            this.LabelPanel.MinimumSize = new System.Drawing.Size(0, 27);
            this.LabelPanel.Name = "LabelPanel";
            this.LabelPanel.Padding = new System.Windows.Forms.Padding(0, 7, 0, 0);
            this.LabelPanel.Size = new System.Drawing.Size(100, 30);
            this.LabelPanel.TabIndex = 25;
            // 
            // FileAttachEditor
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.MainPanel);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(0, 0, 15, 3);
            this.MinimumSize = new System.Drawing.Size(0, 27);
            this.Name = "FileAttachEditor";
            this.Size = new System.Drawing.Size(230, 30);
            this.Load += new System.EventHandler(this.FileAttachEditor_Load);
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.BorderPanel.ResumeLayout(false);
            this.BorderPanel.PerformLayout();
            this.AttachPanel.ResumeLayout(false);
            this.AttachPanel.PerformLayout();
            this.LabelPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label DisplayLabel;
		private System.Windows.Forms.Panel MainPanel;
		private System.Windows.Forms.Panel LabelPanel;
		private DevExpress.XtraEditors.SimpleButton AttachBtn;
		private System.Windows.Forms.Panel BorderPanel;
        private System.Windows.Forms.FlowLayoutPanel AttachPanel;
        private System.Windows.Forms.FlowLayoutPanel AttachFileList;
    }
}
