namespace RFID_CONTROLLER.Controls.Editors {
	partial class PhoneNumberEditor {
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
			this.MainPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.DisplayLabel = new System.Windows.Forms.Label();
			this.BorderPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.phoneNumberEdit1 = new DevExpress.XtraEditors.TextEdit();
			this.BorderPanel2 = new System.Windows.Forms.FlowLayoutPanel();
			this.phoneNumberEdit2 = new DevExpress.XtraEditors.TextEdit();
			this.BorderPanel3 = new System.Windows.Forms.FlowLayoutPanel();
			this.phoneNumberEdit3 = new DevExpress.XtraEditors.TextEdit();
			this.MainPanel.SuspendLayout();
			this.BorderPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.phoneNumberEdit1.Properties)).BeginInit();
			this.BorderPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.phoneNumberEdit2.Properties)).BeginInit();
			this.BorderPanel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.phoneNumberEdit3.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.AutoSize = true;
			this.MainPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.MainPanel.BackColor = System.Drawing.Color.Transparent;
			this.MainPanel.Controls.Add(this.DisplayLabel);
			this.MainPanel.Controls.Add(this.BorderPanel1);
			this.MainPanel.Controls.Add(this.BorderPanel2);
			this.MainPanel.Controls.Add(this.BorderPanel3);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = new System.Drawing.Point(0, 0);
			this.MainPanel.Margin = new System.Windows.Forms.Padding(0);
			this.MainPanel.MinimumSize = new System.Drawing.Size(0, 27);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = new System.Drawing.Size(223, 27);
			this.MainPanel.TabIndex = 0;
			// 
			// DisplayLabel
			// 
			this.DisplayLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.DisplayLabel.AutoSize = true;
			this.DisplayLabel.Font = new System.Drawing.Font("돋움", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.DisplayLabel.Location = new System.Drawing.Point(0, 7);
			this.DisplayLabel.Margin = new System.Windows.Forms.Padding(0);
			this.DisplayLabel.Name = "DisplayLabel";
			this.DisplayLabel.Size = new System.Drawing.Size(49, 13);
			this.DisplayLabel.TabIndex = 1;
			this.DisplayLabel.Text = "레이블";
			this.DisplayLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.DisplayLabel.SizeChanged += new System.EventHandler(this._SizeChanged);
			// 
			// BorderPanel1
			// 
			this.BorderPanel1.AutoSize = true;
			this.BorderPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BorderPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
			this.BorderPanel1.Controls.Add(this.phoneNumberEdit1);
			this.BorderPanel1.Location = new System.Drawing.Point(49, 0);
			this.BorderPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.BorderPanel1.Name = "BorderPanel1";
			this.BorderPanel1.Size = new System.Drawing.Size(50, 27);
			this.BorderPanel1.TabIndex = 0;
			// 
			// phoneNumberEdit1
			// 
			this.phoneNumberEdit1.Location = new System.Drawing.Point(0, 0);
			this.phoneNumberEdit1.Margin = new System.Windows.Forms.Padding(0);
			this.phoneNumberEdit1.Name = "phoneNumberEdit1";
			this.phoneNumberEdit1.Properties.Appearance.BackColor = System.Drawing.Color.White;
			this.phoneNumberEdit1.Properties.Appearance.BackColor2 = System.Drawing.Color.White;
			this.phoneNumberEdit1.Properties.Appearance.Font = new System.Drawing.Font("돋움", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.phoneNumberEdit1.Properties.Appearance.Options.UseBackColor = true;
			this.phoneNumberEdit1.Properties.Appearance.Options.UseFont = true;
			this.phoneNumberEdit1.Properties.Appearance.Options.UseTextOptions = true;
			this.phoneNumberEdit1.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.phoneNumberEdit1.Properties.AppearanceReadOnly.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
			this.phoneNumberEdit1.Properties.AppearanceReadOnly.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
			this.phoneNumberEdit1.Properties.AppearanceReadOnly.Options.UseBackColor = true;
			this.phoneNumberEdit1.Properties.AutoHeight = false;
			this.phoneNumberEdit1.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.phoneNumberEdit1.Properties.Mask.EditMask = "([0-9][0-9]?[0-9])";
			this.phoneNumberEdit1.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.RegEx;
			this.phoneNumberEdit1.Properties.Mask.UseMaskAsDisplayFormat = true;
			this.phoneNumberEdit1.Size = new System.Drawing.Size(50, 27);
			this.phoneNumberEdit1.TabIndex = 0;
			this.phoneNumberEdit1.Modified += new System.EventHandler(this.Editor_Modified);
			this.phoneNumberEdit1.TextChanged += new System.EventHandler(this.Editor_TextChanged);
			this.phoneNumberEdit1.Enter += new System.EventHandler(this.Editor_Enter);
			this.phoneNumberEdit1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyDownHandler);
			this.phoneNumberEdit1.Leave += new System.EventHandler(this.Editor_Leave);
			this.phoneNumberEdit1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Editor_MouseUp);
			this.phoneNumberEdit1.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.Editor_PreviewKeyDown);
			// 
			// BorderPanel2
			// 
			this.BorderPanel2.AutoSize = true;
			this.BorderPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BorderPanel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
			this.BorderPanel2.Controls.Add(this.phoneNumberEdit2);
			this.BorderPanel2.Location = new System.Drawing.Point(101, 0);
			this.BorderPanel2.Margin = new System.Windows.Forms.Padding(2, 0, 0, 0);
			this.BorderPanel2.Name = "BorderPanel2";
			this.BorderPanel2.Size = new System.Drawing.Size(60, 27);
			this.BorderPanel2.TabIndex = 1;
			// 
			// phoneNumberEdit2
			// 
			this.phoneNumberEdit2.Location = new System.Drawing.Point(0, 0);
			this.phoneNumberEdit2.Margin = new System.Windows.Forms.Padding(0);
			this.phoneNumberEdit2.Name = "phoneNumberEdit2";
			this.phoneNumberEdit2.Properties.Appearance.BackColor = System.Drawing.Color.White;
			this.phoneNumberEdit2.Properties.Appearance.BackColor2 = System.Drawing.Color.White;
			this.phoneNumberEdit2.Properties.Appearance.Font = new System.Drawing.Font("돋움", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.phoneNumberEdit2.Properties.Appearance.Options.UseBackColor = true;
			this.phoneNumberEdit2.Properties.Appearance.Options.UseFont = true;
			this.phoneNumberEdit2.Properties.Appearance.Options.UseTextOptions = true;
			this.phoneNumberEdit2.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.phoneNumberEdit2.Properties.AppearanceReadOnly.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
			this.phoneNumberEdit2.Properties.AppearanceReadOnly.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
			this.phoneNumberEdit2.Properties.AppearanceReadOnly.Options.UseBackColor = true;
			this.phoneNumberEdit2.Properties.AutoHeight = false;
			this.phoneNumberEdit2.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.phoneNumberEdit2.Properties.Mask.EditMask = "([0-9][0-9][0-9]?[0-9])";
			this.phoneNumberEdit2.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.RegEx;
			this.phoneNumberEdit2.Properties.Mask.UseMaskAsDisplayFormat = true;
			this.phoneNumberEdit2.Size = new System.Drawing.Size(60, 27);
			this.phoneNumberEdit2.TabIndex = 0;
			this.phoneNumberEdit2.Modified += new System.EventHandler(this.Editor_Modified);
			this.phoneNumberEdit2.TextChanged += new System.EventHandler(this.Editor_TextChanged);
			this.phoneNumberEdit2.Enter += new System.EventHandler(this.Editor_Enter);
			this.phoneNumberEdit2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyDownHandler);
			this.phoneNumberEdit2.Leave += new System.EventHandler(this.Editor_Leave);
			this.phoneNumberEdit2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Editor_MouseUp);
			this.phoneNumberEdit2.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.Editor_PreviewKeyDown);
			// 
			// BorderPanel3
			// 
			this.BorderPanel3.AutoSize = true;
			this.BorderPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BorderPanel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
			this.BorderPanel3.Controls.Add(this.phoneNumberEdit3);
			this.BorderPanel3.Location = new System.Drawing.Point(163, 0);
			this.BorderPanel3.Margin = new System.Windows.Forms.Padding(2, 0, 0, 0);
			this.BorderPanel3.Name = "BorderPanel3";
			this.BorderPanel3.Size = new System.Drawing.Size(60, 27);
			this.BorderPanel3.TabIndex = 2;
			// 
			// phoneNumberEdit3
			// 
			this.phoneNumberEdit3.Location = new System.Drawing.Point(0, 0);
			this.phoneNumberEdit3.Margin = new System.Windows.Forms.Padding(0);
			this.phoneNumberEdit3.Name = "phoneNumberEdit3";
			this.phoneNumberEdit3.Properties.Appearance.BackColor = System.Drawing.Color.White;
			this.phoneNumberEdit3.Properties.Appearance.BackColor2 = System.Drawing.Color.White;
			this.phoneNumberEdit3.Properties.Appearance.Font = new System.Drawing.Font("돋움", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.phoneNumberEdit3.Properties.Appearance.Options.UseBackColor = true;
			this.phoneNumberEdit3.Properties.Appearance.Options.UseFont = true;
			this.phoneNumberEdit3.Properties.Appearance.Options.UseTextOptions = true;
			this.phoneNumberEdit3.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.phoneNumberEdit3.Properties.AppearanceReadOnly.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
			this.phoneNumberEdit3.Properties.AppearanceReadOnly.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
			this.phoneNumberEdit3.Properties.AppearanceReadOnly.Options.UseBackColor = true;
			this.phoneNumberEdit3.Properties.AutoHeight = false;
			this.phoneNumberEdit3.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.phoneNumberEdit3.Properties.Mask.EditMask = "([0-9][0-9][0-9][0-9])";
			this.phoneNumberEdit3.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.RegEx;
			this.phoneNumberEdit3.Properties.Mask.UseMaskAsDisplayFormat = true;
			this.phoneNumberEdit3.Size = new System.Drawing.Size(60, 27);
			this.phoneNumberEdit3.TabIndex = 0;
			this.phoneNumberEdit3.Modified += new System.EventHandler(this.Editor_Modified);
			this.phoneNumberEdit3.TextChanged += new System.EventHandler(this.Editor_TextChanged);
			this.phoneNumberEdit3.Enter += new System.EventHandler(this.Editor_Enter);
			this.phoneNumberEdit3.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyDownHandler);
			this.phoneNumberEdit3.Leave += new System.EventHandler(this.Editor_Leave);
			this.phoneNumberEdit3.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Editor_MouseUp);
			this.phoneNumberEdit3.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.Editor_PreviewKeyDown);
			// 
			// PhoneNumberEditor
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.MainPanel);
			this.DoubleBuffered = true;
			this.Margin = new System.Windows.Forms.Padding(0, 0, 15, 12);
			this.MinimumSize = new System.Drawing.Size(0, 27);
			this.Name = "PhoneNumberEditor";
			this.Size = new System.Drawing.Size(223, 27);
			this.Load += new System.EventHandler(this.PhoneNumberOldEditor_Load);
			this.SizeChanged += new System.EventHandler(this._SizeChanged);
			this.Enter += new System.EventHandler(this.PhoneNumberOldEditor_Enter);
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.BorderPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.phoneNumberEdit1.Properties)).EndInit();
			this.BorderPanel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.phoneNumberEdit2.Properties)).EndInit();
			this.BorderPanel3.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.phoneNumberEdit3.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel MainPanel;
		private DevExpress.XtraEditors.TextEdit phoneNumberEdit1;
		private System.Windows.Forms.FlowLayoutPanel BorderPanel1;
		private System.Windows.Forms.Label DisplayLabel;
		private System.Windows.Forms.FlowLayoutPanel BorderPanel2;
		private DevExpress.XtraEditors.TextEdit phoneNumberEdit2;
		private System.Windows.Forms.FlowLayoutPanel BorderPanel3;
		private DevExpress.XtraEditors.TextEdit phoneNumberEdit3;
	}
}
