namespace RFID_CONTROLLER.Frm
{
    partial class SignManager
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SignManager));
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonClose = new DevExpress.XtraEditors.LabelControl();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonOk = new DevExpress.XtraEditors.LabelControl();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.textBoxID = new RFID_CONTROLLER.Controls.Editors.TextEditor();
            this.textBoxPW = new RFID_CONTROLLER.Controls.Editors.TextEditor();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxID.Editor.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxPW.Editor.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Window;
            this.panel1.Controls.Add(this.buttonClose);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(2, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(380, 50);
            this.panel1.TabIndex = 5;
            // 
            // buttonClose
            // 
            this.buttonClose.Appearance.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClose.Appearance.Options.UseFont = true;
            this.buttonClose.Appearance.Options.UseTextOptions = true;
            this.buttonClose.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.buttonClose.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.buttonClose.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.buttonClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.buttonClose.ImageAlignToText = DevExpress.XtraEditors.ImageAlignToText.RightCenter;
            this.buttonClose.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("buttonClose.ImageOptions.SvgImage")));
            this.buttonClose.Location = new System.Drawing.Point(340, 0);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(40, 50);
            this.buttonClose.TabIndex = 2;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Font = new System.Drawing.Font("Gulim", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this.label1.Size = new System.Drawing.Size(200, 50);
            this.label1.TabIndex = 1;
            this.label1.Text = "로그인";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.Control;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(2, 52);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(380, 157);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel2.Controls.Add(this.buttonOk, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(380, 39);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // buttonOk
            // 
            this.buttonOk.Appearance.BackColor = System.Drawing.Color.DarkBlue;
            this.buttonOk.Appearance.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonOk.Appearance.ForeColor = System.Drawing.SystemColors.Window;
            this.buttonOk.Appearance.Options.UseBackColor = true;
            this.buttonOk.Appearance.Options.UseFont = true;
            this.buttonOk.Appearance.Options.UseForeColor = true;
            this.buttonOk.Appearance.Options.UseTextOptions = true;
            this.buttonOk.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.buttonOk.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.buttonOk.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.buttonOk.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonOk.ImageAlignToText = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            this.buttonOk.Location = new System.Drawing.Point(303, 3);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(74, 33);
            this.buttonOk.TabIndex = 5;
            this.buttonOk.Text = "확인";
            this.buttonOk.UseMnemonic = false;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.textBoxID);
            this.flowLayoutPanel1.Controls.Add(this.textBoxPW);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 42);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(374, 112);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // textBoxID
            // 
            this.textBoxID.AutoSize = true;
            this.textBoxID.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.textBoxID.BackColor = System.Drawing.Color.Transparent;
            this.textBoxID.Caption = "사용자 ID";
            this.textBoxID.DefaultCode = "";
            // 
            // 
            // 
            this.textBoxID.Editor.Location = new System.Drawing.Point(1, 1);
            this.textBoxID.Editor.Margin = new System.Windows.Forms.Padding(1);
            this.textBoxID.Editor.Name = "textEdit";
            this.textBoxID.Editor.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.textBoxID.Editor.Properties.Appearance.BackColor2 = System.Drawing.Color.White;
            this.textBoxID.Editor.Properties.Appearance.Font = new System.Drawing.Font("Dotum", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.textBoxID.Editor.Properties.Appearance.Options.UseBackColor = true;
            this.textBoxID.Editor.Properties.Appearance.Options.UseFont = true;
            this.textBoxID.Editor.Properties.Appearance.Options.UseTextOptions = true;
            this.textBoxID.Editor.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.textBoxID.Editor.Properties.AppearanceReadOnly.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.textBoxID.Editor.Properties.AppearanceReadOnly.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.textBoxID.Editor.Properties.AppearanceReadOnly.Options.UseBackColor = true;
            this.textBoxID.Editor.Properties.AutoHeight = false;
            this.textBoxID.Editor.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.textBoxID.Editor.Size = new System.Drawing.Size(240, 28);
            this.textBoxID.Editor.TabIndex = 0;
            this.textBoxID.EditorSize = new System.Drawing.Size(240, 28);
            this.textBoxID.FieldName = null;
            this.textBoxID.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            // 
            // 
            // 
            this.textBoxID.Label.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.textBoxID.Label.Font = new System.Drawing.Font("Dotum", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.textBoxID.Label.Location = new System.Drawing.Point(0, 8);
            this.textBoxID.Label.Margin = new System.Windows.Forms.Padding(0);
            this.textBoxID.Label.Name = "DisplayLabel";
            this.textBoxID.Label.Size = new System.Drawing.Size(100, 13);
            this.textBoxID.Label.TabIndex = 1;
            this.textBoxID.Label.Text = "사용자 ID";
            this.textBoxID.Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.textBoxID.LabelAutoSize = false;
            this.textBoxID.LabelSize = new System.Drawing.Size(100, 13);
            this.textBoxID.Location = new System.Drawing.Point(0, 20);
            this.textBoxID.Margin = new System.Windows.Forms.Padding(0, 20, 15, 3);
            this.textBoxID.MinimumSize = new System.Drawing.Size(0, 27);
            this.textBoxID.Name = "textBoxID";
            this.textBoxID.ParentContainer = this.flowLayoutPanel1;
            this.textBoxID.Size = new System.Drawing.Size(342, 30);
            this.textBoxID.TabIndex = 0;
            // 
            // textBoxPW
            // 
            this.textBoxPW.AutoSize = true;
            this.textBoxPW.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.textBoxPW.BackColor = System.Drawing.Color.Transparent;
            this.textBoxPW.Caption = "비밀번호";
            this.textBoxPW.DefaultCode = "";
            // 
            // 
            // 
            this.textBoxPW.Editor.Location = new System.Drawing.Point(1, 1);
            this.textBoxPW.Editor.Margin = new System.Windows.Forms.Padding(1);
            this.textBoxPW.Editor.Name = "textEdit";
            this.textBoxPW.Editor.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.textBoxPW.Editor.Properties.Appearance.BackColor2 = System.Drawing.Color.White;
            this.textBoxPW.Editor.Properties.Appearance.Font = new System.Drawing.Font("Dotum", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.textBoxPW.Editor.Properties.Appearance.Options.UseBackColor = true;
            this.textBoxPW.Editor.Properties.Appearance.Options.UseFont = true;
            this.textBoxPW.Editor.Properties.Appearance.Options.UseTextOptions = true;
            this.textBoxPW.Editor.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.textBoxPW.Editor.Properties.AppearanceReadOnly.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.textBoxPW.Editor.Properties.AppearanceReadOnly.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.textBoxPW.Editor.Properties.AppearanceReadOnly.Options.UseBackColor = true;
            this.textBoxPW.Editor.Properties.AutoHeight = false;
            this.textBoxPW.Editor.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.textBoxPW.Editor.Properties.PasswordChar = '*';
            this.textBoxPW.Editor.Size = new System.Drawing.Size(240, 28);
            this.textBoxPW.Editor.TabIndex = 0;
            this.textBoxPW.EditorSize = new System.Drawing.Size(240, 28);
            this.textBoxPW.FieldName = null;
            this.textBoxPW.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            // 
            // 
            // 
            this.textBoxPW.Label.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.textBoxPW.Label.Font = new System.Drawing.Font("Dotum", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.textBoxPW.Label.Location = new System.Drawing.Point(0, 8);
            this.textBoxPW.Label.Margin = new System.Windows.Forms.Padding(0);
            this.textBoxPW.Label.Name = "DisplayLabel";
            this.textBoxPW.Label.Size = new System.Drawing.Size(100, 13);
            this.textBoxPW.Label.TabIndex = 1;
            this.textBoxPW.Label.Text = "비밀번호";
            this.textBoxPW.Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.textBoxPW.LabelAutoSize = false;
            this.textBoxPW.LabelSize = new System.Drawing.Size(100, 13);
            this.textBoxPW.Location = new System.Drawing.Point(0, 53);
            this.textBoxPW.Margin = new System.Windows.Forms.Padding(0, 0, 15, 3);
            this.textBoxPW.MinimumSize = new System.Drawing.Size(0, 27);
            this.textBoxPW.Name = "textBoxPW";
            this.textBoxPW.ParentContainer = this.flowLayoutPanel1;
            this.textBoxPW.Size = new System.Drawing.Size(342, 30);
            this.textBoxPW.TabIndex = 1;
            this.textBoxPW.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxPW_KeyDown);
            // 
            // SignManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlText;
            this.ClientSize = new System.Drawing.Size(384, 211);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "SignManager";
            this.Padding = new System.Windows.Forms.Padding(2);
            this.Text = "SignManager";
            this.Load += new System.EventHandler(this.SignManager_Load);
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxID.Editor.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxPW.Editor.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private DevExpress.XtraEditors.LabelControl buttonOk;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private Controls.Editors.TextEditor textBoxID;
        private Controls.Editors.TextEditor textBoxPW;
        private DevExpress.XtraEditors.LabelControl buttonClose;
    }
}