namespace RFID_CONTROLLER.Controls.Editors
{
	partial class ComboBoxEditor
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

		#region 구성 요소 디자이너에서 생성한 코드

		/// <summary> 
		/// 디자이너 지원에 필요한 메서드입니다. 
		/// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
		/// </summary>
		private void InitializeComponent()
		{
            this.MainPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.DisplayLabel = new System.Windows.Forms.Label();
            this.BorderPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.comboBoxEdit = new DevExpress.XtraEditors.ComboBoxEdit();
            this.MainPanel.SuspendLayout();
            this.BorderPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.AutoSize = true;
            this.MainPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MainPanel.BackColor = System.Drawing.Color.Transparent;
            this.MainPanel.Controls.Add(this.DisplayLabel);
            this.MainPanel.Controls.Add(this.BorderPanel);
            this.MainPanel.Location = new System.Drawing.Point(0, 0);
            this.MainPanel.Margin = new System.Windows.Forms.Padding(0);
            this.MainPanel.MinimumSize = new System.Drawing.Size(0, 27);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(250, 30);
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
            this.DisplayLabel.SizeChanged += new System.EventHandler(this._SizeChanged);
            // 
            // BorderPanel
            // 
            this.BorderPanel.AutoSize = true;
            this.BorderPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BorderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.BorderPanel.Controls.Add(this.comboBoxEdit);
            this.BorderPanel.Location = new System.Drawing.Point(100, 0);
            this.BorderPanel.Margin = new System.Windows.Forms.Padding(0);
            this.BorderPanel.Name = "BorderPanel";
            this.BorderPanel.Size = new System.Drawing.Size(150, 30);
            this.BorderPanel.TabIndex = 0;
            // 
            // comboBoxEdit
            // 
            this.comboBoxEdit.Location = new System.Drawing.Point(0, 0);
            this.comboBoxEdit.Margin = new System.Windows.Forms.Padding(0);
            this.comboBoxEdit.Name = "comboBoxEdit";
            this.comboBoxEdit.Properties.AllowNullInput = DevExpress.Utils.DefaultBoolean.False;
            this.comboBoxEdit.Properties.Appearance.Font = new System.Drawing.Font("Dotum", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxEdit.Properties.Appearance.Options.UseFont = true;
            this.comboBoxEdit.Properties.AppearanceDropDown.Font = new System.Drawing.Font("Dotum", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxEdit.Properties.AppearanceDropDown.Options.UseFont = true;
            this.comboBoxEdit.Properties.AppearanceReadOnly.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.comboBoxEdit.Properties.AppearanceReadOnly.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.comboBoxEdit.Properties.AppearanceReadOnly.Options.UseBackColor = true;
            this.comboBoxEdit.Properties.AutoHeight = false;
            this.comboBoxEdit.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.comboBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.comboBoxEdit.Properties.CloseUpKey = new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.F1);
            this.comboBoxEdit.Properties.DropDownItemHeight = 30;
            this.comboBoxEdit.Properties.DropDownRows = 20;
            this.comboBoxEdit.Properties.ImmediatePopup = true;
            this.comboBoxEdit.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.comboBoxEdit.Size = new System.Drawing.Size(150, 30);
            this.comboBoxEdit.TabIndex = 0;
            this.comboBoxEdit.Modified += new System.EventHandler(this.Editor_Modified);
            this.comboBoxEdit.EditValueChanging += new DevExpress.XtraEditors.Controls.ChangingEventHandler(this.Editor_EditValueChanging);
            this.comboBoxEdit.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyDownHandler);
            this.comboBoxEdit.Leave += new System.EventHandler(this.Editor_Leave);
            this.comboBoxEdit.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.Editor_PreviewKeyDown);
            // 
            // ComboBoxEditor
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.MainPanel);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(0, 0, 15, 3);
            this.MinimumSize = new System.Drawing.Size(0, 27);
            this.Name = "ComboBoxEditor";
            this.Size = new System.Drawing.Size(250, 30);
            this.Load += new System.EventHandler(this.ComboBoxEditor_Load);
            this.SizeChanged += new System.EventHandler(this._SizeChanged);
            this.Enter += new System.EventHandler(this.ComboBoxEditor_Enter);
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.BorderPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel MainPanel;
		private System.Windows.Forms.FlowLayoutPanel BorderPanel;
		private System.Windows.Forms.Label DisplayLabel;
		private DevExpress.XtraEditors.ComboBoxEdit comboBoxEdit;
	}
}
