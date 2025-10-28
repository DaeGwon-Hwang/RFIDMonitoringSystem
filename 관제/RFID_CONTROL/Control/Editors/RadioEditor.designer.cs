namespace RFID_CONTROLLER.Controls.Editors {
	partial class RadioEditor {
		/// <summary> 
		/// 필수 디자이너 변수입니다.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// 사용 중인 모든 리소스를 정리합니다.
		/// </summary>
		/// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
		protected override void Dispose(bool disposing) {
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
		private void InitializeComponent() {
            this.MainPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.DisplayLabel = new System.Windows.Forms.Label();
            this.RdoPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.MainPanel.SuspendLayout();
            this.RdoPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.AutoSize = true;
            this.MainPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MainPanel.BackColor = System.Drawing.Color.Transparent;
            this.MainPanel.Controls.Add(this.DisplayLabel);
            this.MainPanel.Controls.Add(this.RdoPanel);
            this.MainPanel.Location = new System.Drawing.Point(0, 0);
            this.MainPanel.Margin = new System.Windows.Forms.Padding(0);
            this.MainPanel.MinimumSize = new System.Drawing.Size(0, 27);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(187, 30);
            this.MainPanel.TabIndex = 0;
            // 
            // DisplayLabel
            // 
            this.DisplayLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.DisplayLabel.Font = new System.Drawing.Font("Dotum", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.DisplayLabel.Location = new System.Drawing.Point(0, 8);
            this.DisplayLabel.Margin = new System.Windows.Forms.Padding(0);
            this.DisplayLabel.Name = "DisplayLabel";
            this.DisplayLabel.Size = new System.Drawing.Size(100, 13);
            this.DisplayLabel.TabIndex = 1;
            this.DisplayLabel.Text = "레이블";
            this.DisplayLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // RdoPanel
            // 
            this.RdoPanel.BackColor = System.Drawing.Color.Transparent;
            this.RdoPanel.Controls.Add(this.radioButton1);
            this.RdoPanel.Controls.Add(this.radioButton2);
            this.RdoPanel.Location = new System.Drawing.Point(100, 0);
            this.RdoPanel.Margin = new System.Windows.Forms.Padding(0);
            this.RdoPanel.MinimumSize = new System.Drawing.Size(87, 27);
            this.RdoPanel.Name = "RdoPanel";
            this.RdoPanel.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.RdoPanel.Size = new System.Drawing.Size(87, 30);
            this.RdoPanel.TabIndex = 0;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.Font = new System.Drawing.Font("Dotum", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.radioButton1.Location = new System.Drawing.Point(5, 6);
            this.radioButton1.Margin = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(35, 20);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.TabStop = true;
            this.radioButton1.Tag = "Y";
            this.radioButton1.Text = "Y";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Font = new System.Drawing.Font("Dotum", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.radioButton2.Location = new System.Drawing.Point(40, 6);
            this.radioButton2.Margin = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(36, 20);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.Tag = "N";
            this.radioButton2.Text = "N";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // RadioEditor
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.MainPanel);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(0, 0, 15, 12);
            this.MinimumSize = new System.Drawing.Size(0, 27);
            this.Name = "RadioEditor";
            this.Size = new System.Drawing.Size(187, 30);
            this.Load += new System.EventHandler(this.RadioEditor_Load);
            this.Enter += new System.EventHandler(this.RadioEditor_Enter);
            this.Leave += new System.EventHandler(this.RadioEditor_Leave);
            this.MainPanel.ResumeLayout(false);
            this.RdoPanel.ResumeLayout(false);
            this.RdoPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel MainPanel;
		private System.Windows.Forms.FlowLayoutPanel RdoPanel;
		private System.Windows.Forms.Label DisplayLabel;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.RadioButton radioButton2;
	}
}
