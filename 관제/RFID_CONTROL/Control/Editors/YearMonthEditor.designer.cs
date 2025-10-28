namespace RFID_CONTROLLER.Controls.Editors {
	partial class YearMonthEditor {
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
			this.BorderPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.yearMonthEdit = new DevExpress.XtraEditors.DateEdit();
			this.HelpBtn = new DevExpress.XtraEditors.SimpleButton();
			this.MainPanel.SuspendLayout();
			this.BorderPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.yearMonthEdit.Properties.CalendarTimeProperties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.yearMonthEdit.Properties)).BeginInit();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.AutoSize = true;
			this.MainPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.MainPanel.BackColor = System.Drawing.Color.Transparent;
			this.MainPanel.Controls.Add(this.DisplayLabel);
			this.MainPanel.Controls.Add(this.BorderPanel);
			this.MainPanel.Controls.Add(this.HelpBtn);
			this.MainPanel.Location = new System.Drawing.Point(0, 0);
			this.MainPanel.Margin = new System.Windows.Forms.Padding(0);
			this.MainPanel.MinimumSize = new System.Drawing.Size(0, 27);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = new System.Drawing.Size(140, 27);
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
			// BorderPanel
			// 
			this.BorderPanel.AutoSize = true;
			this.BorderPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BorderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
			this.BorderPanel.Controls.Add(this.yearMonthEdit);
			this.BorderPanel.Location = new System.Drawing.Point(49, 0);
			this.BorderPanel.Margin = new System.Windows.Forms.Padding(0);
			this.BorderPanel.Name = "BorderPanel";
			this.BorderPanel.Size = new System.Drawing.Size(64, 27);
			this.BorderPanel.TabIndex = 0;
			// 
			// yearMonthEdit
			// 
			this.yearMonthEdit.EditValue = null;
			this.yearMonthEdit.Location = new System.Drawing.Point(0, 0);
			this.yearMonthEdit.Margin = new System.Windows.Forms.Padding(0);
			this.yearMonthEdit.Name = "yearMonthEdit";
			this.yearMonthEdit.Properties.AllowNullInput = DevExpress.Utils.DefaultBoolean.True;
			this.yearMonthEdit.Properties.Appearance.Font = new System.Drawing.Font("돋움", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.yearMonthEdit.Properties.Appearance.Options.UseFont = true;
			this.yearMonthEdit.Properties.Appearance.Options.UseTextOptions = true;
			this.yearMonthEdit.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
			this.yearMonthEdit.Properties.AppearanceReadOnly.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
			this.yearMonthEdit.Properties.AppearanceReadOnly.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
			this.yearMonthEdit.Properties.AppearanceReadOnly.Options.UseBackColor = true;
			this.yearMonthEdit.Properties.AutoHeight = false;
			this.yearMonthEdit.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.yearMonthEdit.Properties.ButtonsStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.yearMonthEdit.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.yearMonthEdit.Properties.CalendarTimeProperties.TouchUIMaxValue = new System.DateTime(9999, 12, 31, 23, 59, 0, 0);
			this.yearMonthEdit.Properties.Mask.EditMask = "([1-9][0-9][0-9][0-9])-(0?[1-9]|1[012])";
			this.yearMonthEdit.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.RegEx;
			this.yearMonthEdit.Properties.Mask.UseMaskAsDisplayFormat = true;
			this.yearMonthEdit.Properties.MaxValue = new System.DateTime(9999, 12, 1, 0, 0, 0, 0);
			this.yearMonthEdit.Properties.NullDate = "";
			this.yearMonthEdit.Properties.VistaCalendarInitialViewStyle = DevExpress.XtraEditors.VistaCalendarInitialViewStyle.YearView;
			this.yearMonthEdit.Properties.VistaCalendarViewStyle = DevExpress.XtraEditors.VistaCalendarViewStyle.YearView;
			this.yearMonthEdit.Size = new System.Drawing.Size(64, 27);
			this.yearMonthEdit.TabIndex = 0;
			this.yearMonthEdit.Modified += new System.EventHandler(this.Editor_Modified);
			this.yearMonthEdit.TextChanged += new System.EventHandler(this.Editor_TextChanged);
			this.yearMonthEdit.Enter += new System.EventHandler(this.Editor_Enter);
			this.yearMonthEdit.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyDownHandler);
			this.yearMonthEdit.Leave += new System.EventHandler(this.Editor_Leave);
			this.yearMonthEdit.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Editor_MouseUp);
			this.yearMonthEdit.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.Editor_PreviewKeyDown);
			// 
			// HelpBtn
			// 
			this.HelpBtn.Appearance.Font = new System.Drawing.Font("돋움", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.HelpBtn.Appearance.ForeColor = System.Drawing.Color.White;
			this.HelpBtn.Appearance.Image = global::RFID_CONTROLLER.Properties.Resources.btn_cal;
			this.HelpBtn.Appearance.Options.UseFont = true;
			this.HelpBtn.Appearance.Options.UseForeColor = true;
			this.HelpBtn.Appearance.Options.UseImage = true;
			this.HelpBtn.AppearanceHovered.Image = global::RFID_CONTROLLER.Properties.Resources.btn_cal_on;
			this.HelpBtn.AppearanceHovered.Options.UseImage = true;
			this.HelpBtn.AppearancePressed.Image = global::RFID_CONTROLLER.Properties.Resources.btn_cal_ov;
			this.HelpBtn.AppearancePressed.Options.UseImage = true;
			this.HelpBtn.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
			this.HelpBtn.Location = new System.Drawing.Point(113, 0);
			this.HelpBtn.LookAndFeel.UseDefaultLookAndFeel = false;
			this.HelpBtn.Margin = new System.Windows.Forms.Padding(0);
			this.HelpBtn.MinimumSize = new System.Drawing.Size(27, 27);
			this.HelpBtn.Name = "HelpBtn";
			this.HelpBtn.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
			this.HelpBtn.Size = new System.Drawing.Size(27, 27);
			this.HelpBtn.TabIndex = 21;
			this.HelpBtn.TabStop = false;
			// 
			// YearMonthEditor
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.MainPanel);
			this.DoubleBuffered = true;
			this.Margin = new System.Windows.Forms.Padding(0, 0, 15, 12);
			this.MinimumSize = new System.Drawing.Size(0, 27);
			this.Name = "YearMonthEditor";
			this.Size = new System.Drawing.Size(140, 27);
			this.Load += new System.EventHandler(this.YearMonthEditor_Load);
			this.SizeChanged += new System.EventHandler(this._SizeChanged);
			this.Enter += new System.EventHandler(this.YearMonthEditor_Enter);
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.BorderPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.yearMonthEdit.Properties.CalendarTimeProperties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.yearMonthEdit.Properties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel MainPanel;
		private System.Windows.Forms.FlowLayoutPanel BorderPanel;
		private DevExpress.XtraEditors.SimpleButton HelpBtn;
		private System.Windows.Forms.Label DisplayLabel;
		private DevExpress.XtraEditors.DateEdit yearMonthEdit;
	}
}
