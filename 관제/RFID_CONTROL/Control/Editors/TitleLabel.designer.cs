namespace RFID_CONTROLLER.Controls.Editors {
	partial class TitleLabel {
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
			this.label = new System.Windows.Forms.Label();
			this.colorLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label
			// 
			this.label.AutoSize = true;
			this.label.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label.Font = new System.Drawing.Font("돋움", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.label.Location = new System.Drawing.Point(4, 0);
			this.label.Margin = new System.Windows.Forms.Padding(0);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(49, 13);
			this.label.TabIndex = 1;
			this.label.Text = "타이틀";
			// 
			// colorLabel
			// 
			this.colorLabel.AutoSize = true;
			this.colorLabel.BackColor = System.Drawing.Color.Red;
			this.colorLabel.Dock = System.Windows.Forms.DockStyle.Left;
			this.colorLabel.Font = new System.Drawing.Font("돋움", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.colorLabel.Location = new System.Drawing.Point(0, 0);
			this.colorLabel.Margin = new System.Windows.Forms.Padding(0);
			this.colorLabel.MinimumSize = new System.Drawing.Size(4, 0);
			this.colorLabel.Name = "colorLabel";
			this.colorLabel.Size = new System.Drawing.Size(4, 13);
			this.colorLabel.TabIndex = 2;
			this.colorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// TitleLabel
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.label);
			this.Controls.Add(this.colorLabel);
			this.DoubleBuffered = true;
			this.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
			this.Name = "TitleLabel";
			this.Size = new System.Drawing.Size(53, 13);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label;
		private System.Windows.Forms.Label colorLabel;
	}
}
