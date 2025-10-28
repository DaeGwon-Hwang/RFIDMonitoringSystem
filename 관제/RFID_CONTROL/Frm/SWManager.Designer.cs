namespace RFID_CONTROLLER.Frm
{
    partial class SWManager
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SWManager));
            this.headerPanel = new System.Windows.Forms.Panel();
            this.buttonClose = new DevExpress.XtraEditors.LabelControl();
            this.label1 = new System.Windows.Forms.Label();
            this.programMngTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.SWManagerGrid = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn3 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn4 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn5 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn6 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.btnSwAdd = new DevExpress.XtraEditors.LabelControl();
            this.btnSwSave = new DevExpress.XtraEditors.LabelControl();
            this.btnSwDel = new DevExpress.XtraEditors.LabelControl();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.cbEqType = new RFID_CONTROLLER.Controls.Editors.ComboBoxEditor();
            this.btnSWSearch = new System.Windows.Forms.Button();
            this.cbSwType = new RFID_CONTROLLER.Controls.Editors.ComboBoxEditor();
            this.txtSwNm = new RFID_CONTROLLER.Controls.Editors.TextEditor();
            this.txtSwSeq = new RFID_CONTROLLER.Controls.Editors.TextEditor();
            this.txtDescp = new RFID_CONTROLLER.Controls.Editors.TextEditor();
            this.txtFileName = new RFID_CONTROLLER.Controls.Editors.TextEditor();
            this.btnFile = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.headerPanel.SuspendLayout();
            this.programMngTableLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SWManagerGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            this.tableLayoutPanel3.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cbEqType.Editor.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbSwType.Editor.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSwNm.Editor.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSwSeq.Editor.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDescp.Editor.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtFileName.Editor.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // headerPanel
            // 
            this.headerPanel.BackColor = System.Drawing.SystemColors.Window;
            this.headerPanel.Controls.Add(this.buttonClose);
            this.headerPanel.Controls.Add(this.label1);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.Location = new System.Drawing.Point(0, 0);
            this.headerPanel.Margin = new System.Windows.Forms.Padding(2);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(1084, 50);
            this.headerPanel.TabIndex = 0;
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
            this.buttonClose.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("labelControl4.ImageOptions.SvgImage")));
            this.buttonClose.Location = new System.Drawing.Point(1044, 0);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(40, 50);
            this.buttonClose.TabIndex = 1;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Font = new System.Drawing.Font("Gulim", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this.label1.Size = new System.Drawing.Size(200, 50);
            this.label1.TabIndex = 0;
            this.label1.Text = "프로그램 관리";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // programMngTableLayout
            // 
            this.programMngTableLayout.BackColor = System.Drawing.SystemColors.Window;
            this.programMngTableLayout.ColumnCount = 1;
            this.programMngTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.programMngTableLayout.Controls.Add(this.SWManagerGrid, 0, 2);
            this.programMngTableLayout.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.programMngTableLayout.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.programMngTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.programMngTableLayout.Location = new System.Drawing.Point(0, 50);
            this.programMngTableLayout.Margin = new System.Windows.Forms.Padding(2);
            this.programMngTableLayout.Name = "programMngTableLayout";
            this.programMngTableLayout.RowCount = 3;
            this.programMngTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.programMngTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.programMngTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 0F));
            this.programMngTableLayout.Size = new System.Drawing.Size(1084, 511);
            this.programMngTableLayout.TabIndex = 1;
            // 
            // SWManagerGrid
            // 
            this.SWManagerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SWManagerGrid.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(2);
            this.SWManagerGrid.Location = new System.Drawing.Point(2, 117);
            this.SWManagerGrid.MainView = this.gridView1;
            this.SWManagerGrid.Margin = new System.Windows.Forms.Padding(2);
            this.SWManagerGrid.Name = "SWManagerGrid";
            this.SWManagerGrid.Size = new System.Drawing.Size(1080, 410);
            this.SWManagerGrid.TabIndex = 1;
            this.SWManagerGrid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn1,
            this.gridColumn2,
            this.gridColumn3,
            this.gridColumn4,
            this.gridColumn5,
            this.gridColumn6});
            this.gridView1.DetailHeight = 233;
            this.gridView1.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFullFocus;
            this.gridView1.GridControl = this.SWManagerGrid;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsEditForm.PopupEditFormWidth = 560;
            this.gridView1.OptionsView.ShowGroupPanel = false;
            // 
            // gridColumn1
            // 
            this.gridColumn1.Caption = "No";
            this.gridColumn1.FieldName = "SEQ";
            this.gridColumn1.Name = "gridColumn1";
            this.gridColumn1.Visible = true;
            this.gridColumn1.VisibleIndex = 0;
            this.gridColumn1.Width = 50;
            // 
            // gridColumn2
            // 
            this.gridColumn2.Caption = "장비 타입";
            this.gridColumn2.FieldName = "EQ_TYPE_NM";
            this.gridColumn2.Name = "gridColumn2";
            this.gridColumn2.Visible = true;
            this.gridColumn2.VisibleIndex = 1;
            this.gridColumn2.Width = 100;
            // 
            // gridColumn3
            // 
            this.gridColumn3.Caption = "프로그램 타입";
            this.gridColumn3.FieldName = "SW_TYPE_NM";
            this.gridColumn3.Name = "gridColumn3";
            this.gridColumn3.Visible = true;
            this.gridColumn3.VisibleIndex = 2;
            this.gridColumn3.Width = 100;
            // 
            // gridColumn4
            // 
            this.gridColumn4.Caption = "프로그램명";
            this.gridColumn4.FieldName = "SW_NM";
            this.gridColumn4.Name = "gridColumn4";
            this.gridColumn4.Visible = true;
            this.gridColumn4.VisibleIndex = 3;
            this.gridColumn4.Width = 150;
            // 
            // gridColumn5
            // 
            this.gridColumn5.Caption = "프로그램 파일명";
            this.gridColumn5.FieldName = "SW_FILENAME";
            this.gridColumn5.Name = "gridColumn5";
            this.gridColumn5.Visible = true;
            this.gridColumn5.VisibleIndex = 4;
            this.gridColumn5.Width = 150;
            // 
            // gridColumn6
            // 
            this.gridColumn6.Caption = "프로그램 설명";
            this.gridColumn6.FieldName = "DESCP";
            this.gridColumn6.Name = "gridColumn6";
            this.gridColumn6.Visible = true;
            this.gridColumn6.VisibleIndex = 5;
            this.gridColumn6.Width = 500;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 4;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 76F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8F));
            this.tableLayoutPanel3.Controls.Add(this.btnSwAdd, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnSwSave, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnSwDel, 3, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 80);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1084, 35);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // btnSwAdd
            // 
            this.btnSwAdd.Appearance.BackColor = System.Drawing.Color.DarkBlue;
            this.btnSwAdd.Appearance.ForeColor = System.Drawing.SystemColors.Window;
            this.btnSwAdd.Appearance.Options.UseBackColor = true;
            this.btnSwAdd.Appearance.Options.UseForeColor = true;
            this.btnSwAdd.Appearance.Options.UseTextOptions = true;
            this.btnSwAdd.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.btnSwAdd.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.btnSwAdd.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.btnSwAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSwAdd.ImageAlignToText = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            this.btnSwAdd.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnSwAdd.ImageOptions.SvgImage")));
            this.btnSwAdd.Location = new System.Drawing.Point(826, 3);
            this.btnSwAdd.Name = "btnSwAdd";
            this.btnSwAdd.Size = new System.Drawing.Size(80, 29);
            this.btnSwAdd.TabIndex = 0;
            this.btnSwAdd.Text = "추가하기";
            this.btnSwAdd.UseMnemonic = false;
            this.btnSwAdd.Click += new System.EventHandler(this.btnSwAdd_Click);
            // 
            // btnSwSave
            // 
            this.btnSwSave.Appearance.BackColor = System.Drawing.Color.DarkBlue;
            this.btnSwSave.Appearance.ForeColor = System.Drawing.SystemColors.Window;
            this.btnSwSave.Appearance.Options.UseBackColor = true;
            this.btnSwSave.Appearance.Options.UseForeColor = true;
            this.btnSwSave.Appearance.Options.UseTextOptions = true;
            this.btnSwSave.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.btnSwSave.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.btnSwSave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSwSave.ImageAlignToText = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            this.btnSwSave.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnSwSave.ImageOptions.SvgImage")));
            this.btnSwSave.Location = new System.Drawing.Point(912, 3);
            this.btnSwSave.Name = "btnSwSave";
            this.btnSwSave.Size = new System.Drawing.Size(80, 29);
            this.btnSwSave.TabIndex = 1;
            this.btnSwSave.Text = "저장하기";
            this.btnSwSave.Click += new System.EventHandler(this.btnSwSave_Click);
            // 
            // btnSwDel
            // 
            this.btnSwDel.Appearance.BackColor = System.Drawing.Color.DarkBlue;
            this.btnSwDel.Appearance.ForeColor = System.Drawing.SystemColors.Window;
            this.btnSwDel.Appearance.Options.UseBackColor = true;
            this.btnSwDel.Appearance.Options.UseForeColor = true;
            this.btnSwDel.Appearance.Options.UseTextOptions = true;
            this.btnSwDel.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.btnSwDel.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.btnSwDel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSwDel.ImageAlignToText = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            this.btnSwDel.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnSwDel.ImageOptions.SvgImage")));
            this.btnSwDel.Location = new System.Drawing.Point(998, 3);
            this.btnSwDel.Name = "btnSwDel";
            this.btnSwDel.Size = new System.Drawing.Size(83, 29);
            this.btnSwDel.TabIndex = 2;
            this.btnSwDel.Text = "삭제하기";
            this.btnSwDel.Click += new System.EventHandler(this.btnSwDel_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.BackColor = System.Drawing.SystemColors.Control;
            this.flowLayoutPanel1.Controls.Add(this.cbEqType);
            this.flowLayoutPanel1.Controls.Add(this.btnSWSearch);
            this.flowLayoutPanel1.Controls.Add(this.cbSwType);
            this.flowLayoutPanel1.Controls.Add(this.txtSwNm);
            this.flowLayoutPanel1.Controls.Add(this.txtSwSeq);
            this.flowLayoutPanel1.Controls.Add(this.txtDescp);
            this.flowLayoutPanel1.Controls.Add(this.txtFileName);
            this.flowLayoutPanel1.Controls.Add(this.btnFile);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1078, 74);
            this.flowLayoutPanel1.TabIndex = 3;
            // 
            // cbEqType
            // 
            this.cbEqType.AllCode = "";
            this.cbEqType.AllCodeName = "";
            this.cbEqType.AutoSize = true;
            this.cbEqType.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cbEqType.BackColor = System.Drawing.Color.Transparent;
            this.cbEqType.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbEqType.Caption = "장비 타입";
            this.cbEqType.DefaultCode = "";
            // 
            // 
            // 
            this.cbEqType.Editor.Location = new System.Drawing.Point(1, 1);
            this.cbEqType.Editor.Margin = new System.Windows.Forms.Padding(1);
            this.cbEqType.Editor.Name = "comboBoxEdit";
            this.cbEqType.Editor.Properties.AllowNullInput = DevExpress.Utils.DefaultBoolean.False;
            this.cbEqType.Editor.Properties.Appearance.Font = new System.Drawing.Font("Dotum", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbEqType.Editor.Properties.Appearance.Options.UseFont = true;
            this.cbEqType.Editor.Properties.AppearanceDropDown.Font = new System.Drawing.Font("Dotum", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbEqType.Editor.Properties.AppearanceDropDown.Options.UseFont = true;
            this.cbEqType.Editor.Properties.AppearanceReadOnly.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.cbEqType.Editor.Properties.AppearanceReadOnly.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.cbEqType.Editor.Properties.AppearanceReadOnly.Options.UseBackColor = true;
            this.cbEqType.Editor.Properties.AutoHeight = false;
            this.cbEqType.Editor.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.cbEqType.Editor.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbEqType.Editor.Properties.CloseUpKey = new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.F1);
            this.cbEqType.Editor.Properties.DropDownItemHeight = 30;
            this.cbEqType.Editor.Properties.DropDownRows = 20;
            this.cbEqType.Editor.Properties.ImmediatePopup = true;
            this.cbEqType.Editor.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cbEqType.Editor.Size = new System.Drawing.Size(148, 28);
            this.cbEqType.Editor.TabIndex = 0;
            this.cbEqType.EditorSize = new System.Drawing.Size(148, 28);
            this.cbEqType.FieldName = "";
            // 
            // 
            // 
            this.cbEqType.Label.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cbEqType.Label.Font = new System.Drawing.Font("Dotum", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.cbEqType.Label.Location = new System.Drawing.Point(0, 8);
            this.cbEqType.Label.Margin = new System.Windows.Forms.Padding(0);
            this.cbEqType.Label.Name = "DisplayLabel";
            this.cbEqType.Label.Size = new System.Drawing.Size(100, 13);
            this.cbEqType.Label.TabIndex = 1;
            this.cbEqType.Label.Text = "장비 타입";
            this.cbEqType.Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbEqType.LabelAutoSize = false;
            this.cbEqType.LabelSize = new System.Drawing.Size(100, 13);
            this.cbEqType.Location = new System.Drawing.Point(0, 5);
            this.cbEqType.Margin = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.cbEqType.MinimumSize = new System.Drawing.Size(0, 27);
            this.cbEqType.Name = "cbEqType";
            this.cbEqType.NameFieldName = "_NM";
            this.cbEqType.ParentContainer = this.flowLayoutPanel1;
            this.cbEqType.Size = new System.Drawing.Size(250, 30);
            this.cbEqType.TabIndex = 0;
            // 
            // btnSWSearch
            // 
            this.btnSWSearch.BackgroundImage = global::RFID_CONTROLLER.Properties.Resources.img_search;
            this.btnSWSearch.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSWSearch.Location = new System.Drawing.Point(250, 5);
            this.btnSWSearch.Margin = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.btnSWSearch.Name = "btnSWSearch";
            this.btnSWSearch.Size = new System.Drawing.Size(30, 30);
            this.btnSWSearch.TabIndex = 7;
            this.btnSWSearch.UseVisualStyleBackColor = true;
            this.btnSWSearch.Click += new System.EventHandler(this.btnSWSearch_Click);
            // 
            // cbSwType
            // 
            this.cbSwType.AllCode = "";
            this.cbSwType.AllCodeName = "";
            this.cbSwType.AutoSize = true;
            this.cbSwType.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cbSwType.BackColor = System.Drawing.Color.Transparent;
            this.cbSwType.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbSwType.Caption = "프로그램 타입";
            this.cbSwType.DefaultCode = "";
            // 
            // 
            // 
            this.cbSwType.Editor.Location = new System.Drawing.Point(1, 1);
            this.cbSwType.Editor.Margin = new System.Windows.Forms.Padding(1);
            this.cbSwType.Editor.Name = "comboBoxEdit";
            this.cbSwType.Editor.Properties.AllowNullInput = DevExpress.Utils.DefaultBoolean.False;
            this.cbSwType.Editor.Properties.Appearance.Font = new System.Drawing.Font("Dotum", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbSwType.Editor.Properties.Appearance.Options.UseFont = true;
            this.cbSwType.Editor.Properties.AppearanceDropDown.Font = new System.Drawing.Font("Dotum", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbSwType.Editor.Properties.AppearanceDropDown.Options.UseFont = true;
            this.cbSwType.Editor.Properties.AppearanceReadOnly.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.cbSwType.Editor.Properties.AppearanceReadOnly.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.cbSwType.Editor.Properties.AppearanceReadOnly.Options.UseBackColor = true;
            this.cbSwType.Editor.Properties.AutoHeight = false;
            this.cbSwType.Editor.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.cbSwType.Editor.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbSwType.Editor.Properties.CloseUpKey = new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.F1);
            this.cbSwType.Editor.Properties.DropDownItemHeight = 30;
            this.cbSwType.Editor.Properties.DropDownRows = 20;
            this.cbSwType.Editor.Properties.ImmediatePopup = true;
            this.cbSwType.Editor.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cbSwType.Editor.Size = new System.Drawing.Size(148, 28);
            this.cbSwType.Editor.TabIndex = 0;
            this.cbSwType.EditorSize = new System.Drawing.Size(148, 28);
            this.cbSwType.FieldName = "";
            // 
            // 
            // 
            this.cbSwType.Label.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cbSwType.Label.Font = new System.Drawing.Font("Dotum", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.cbSwType.Label.Location = new System.Drawing.Point(0, 8);
            this.cbSwType.Label.Margin = new System.Windows.Forms.Padding(0);
            this.cbSwType.Label.Name = "DisplayLabel";
            this.cbSwType.Label.Size = new System.Drawing.Size(100, 13);
            this.cbSwType.Label.TabIndex = 1;
            this.cbSwType.Label.Text = "프로그램 타입";
            this.cbSwType.Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbSwType.LabelAutoSize = false;
            this.cbSwType.LabelSize = new System.Drawing.Size(100, 13);
            this.cbSwType.Location = new System.Drawing.Point(280, 5);
            this.cbSwType.Margin = new System.Windows.Forms.Padding(0, 5, 15, 3);
            this.cbSwType.MinimumSize = new System.Drawing.Size(0, 27);
            this.cbSwType.Name = "cbSwType";
            this.cbSwType.NameFieldName = "_NM";
            this.cbSwType.ParentContainer = this.flowLayoutPanel1;
            this.cbSwType.Size = new System.Drawing.Size(250, 30);
            this.cbSwType.TabIndex = 2;
            // 
            // txtSwNm
            // 
            this.txtSwNm.AutoSize = true;
            this.txtSwNm.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.txtSwNm.BackColor = System.Drawing.Color.Transparent;
            this.txtSwNm.Caption = "프로그램명";
            this.txtSwNm.DefaultCode = "";
            // 
            // 
            // 
            this.txtSwNm.Editor.EditValue = "";
            this.txtSwNm.Editor.Location = new System.Drawing.Point(1, 1);
            this.txtSwNm.Editor.Margin = new System.Windows.Forms.Padding(1);
            this.txtSwNm.Editor.Name = "textEdit";
            this.txtSwNm.Editor.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.txtSwNm.Editor.Properties.Appearance.BackColor2 = System.Drawing.Color.White;
            this.txtSwNm.Editor.Properties.Appearance.Font = new System.Drawing.Font("Dotum", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtSwNm.Editor.Properties.Appearance.Options.UseBackColor = true;
            this.txtSwNm.Editor.Properties.Appearance.Options.UseFont = true;
            this.txtSwNm.Editor.Properties.AppearanceReadOnly.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.txtSwNm.Editor.Properties.AppearanceReadOnly.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.txtSwNm.Editor.Properties.AppearanceReadOnly.Options.UseBackColor = true;
            this.txtSwNm.Editor.Properties.AutoHeight = false;
            this.txtSwNm.Editor.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.txtSwNm.Editor.Size = new System.Drawing.Size(148, 28);
            this.txtSwNm.Editor.TabIndex = 0;
            this.txtSwNm.EditorSize = new System.Drawing.Size(148, 28);
            this.txtSwNm.FieldName = null;
            // 
            // 
            // 
            this.txtSwNm.Label.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.txtSwNm.Label.Font = new System.Drawing.Font("Dotum", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtSwNm.Label.Location = new System.Drawing.Point(0, 8);
            this.txtSwNm.Label.Margin = new System.Windows.Forms.Padding(0);
            this.txtSwNm.Label.Name = "DisplayLabel";
            this.txtSwNm.Label.Size = new System.Drawing.Size(100, 13);
            this.txtSwNm.Label.TabIndex = 1;
            this.txtSwNm.Label.Text = "프로그램명";
            this.txtSwNm.Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.txtSwNm.LabelAutoSize = false;
            this.txtSwNm.LabelSize = new System.Drawing.Size(100, 13);
            this.txtSwNm.Location = new System.Drawing.Point(545, 5);
            this.txtSwNm.Margin = new System.Windows.Forms.Padding(0, 5, 15, 3);
            this.txtSwNm.MinimumSize = new System.Drawing.Size(0, 27);
            this.txtSwNm.Name = "txtSwNm";
            this.txtSwNm.ParentContainer = this.flowLayoutPanel1;
            this.txtSwNm.Size = new System.Drawing.Size(250, 30);
            this.txtSwNm.TabIndex = 3;
            // 
            // txtSwSeq
            // 
            this.txtSwSeq.AutoSize = true;
            this.txtSwSeq.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.txtSwSeq.BackColor = System.Drawing.Color.Transparent;
            this.txtSwSeq.Caption = "순번";
            this.txtSwSeq.DefaultCode = "";
            // 
            // 
            // 
            this.txtSwSeq.Editor.Location = new System.Drawing.Point(1, 1);
            this.txtSwSeq.Editor.Margin = new System.Windows.Forms.Padding(1);
            this.txtSwSeq.Editor.Name = "textEdit";
            this.txtSwSeq.Editor.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.txtSwSeq.Editor.Properties.Appearance.BackColor2 = System.Drawing.Color.White;
            this.txtSwSeq.Editor.Properties.Appearance.Font = new System.Drawing.Font("Dotum", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtSwSeq.Editor.Properties.Appearance.Options.UseBackColor = true;
            this.txtSwSeq.Editor.Properties.Appearance.Options.UseFont = true;
            this.txtSwSeq.Editor.Properties.AppearanceReadOnly.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.txtSwSeq.Editor.Properties.AppearanceReadOnly.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.txtSwSeq.Editor.Properties.AppearanceReadOnly.Options.UseBackColor = true;
            this.txtSwSeq.Editor.Properties.AutoHeight = false;
            this.txtSwSeq.Editor.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.txtSwSeq.Editor.Size = new System.Drawing.Size(148, 28);
            this.txtSwSeq.Editor.TabIndex = 0;
            this.txtSwSeq.EditorSize = new System.Drawing.Size(148, 28);
            this.txtSwSeq.FieldName = null;
            // 
            // 
            // 
            this.txtSwSeq.Label.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.txtSwSeq.Label.Font = new System.Drawing.Font("Dotum", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtSwSeq.Label.Location = new System.Drawing.Point(0, 8);
            this.txtSwSeq.Label.Margin = new System.Windows.Forms.Padding(0);
            this.txtSwSeq.Label.Name = "DisplayLabel";
            this.txtSwSeq.Label.Size = new System.Drawing.Size(100, 13);
            this.txtSwSeq.Label.TabIndex = 1;
            this.txtSwSeq.Label.Text = "순번";
            this.txtSwSeq.Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.txtSwSeq.LabelAutoSize = false;
            this.txtSwSeq.LabelSize = new System.Drawing.Size(100, 13);
            this.txtSwSeq.Location = new System.Drawing.Point(810, 5);
            this.txtSwSeq.Margin = new System.Windows.Forms.Padding(0, 5, 15, 3);
            this.txtSwSeq.MinimumSize = new System.Drawing.Size(0, 27);
            this.txtSwSeq.Name = "txtSwSeq";
            this.txtSwSeq.ParentContainer = this.flowLayoutPanel1;
            this.txtSwSeq.Size = new System.Drawing.Size(250, 30);
            this.txtSwSeq.TabIndex = 6;
            // 
            // txtDescp
            // 
            this.txtDescp.AutoSize = true;
            this.txtDescp.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.txtDescp.BackColor = System.Drawing.Color.Transparent;
            this.txtDescp.Caption = "프로그램 설명";
            this.txtDescp.DefaultCode = "";
            // 
            // 
            // 
            this.txtDescp.Editor.Location = new System.Drawing.Point(1, 1);
            this.txtDescp.Editor.Margin = new System.Windows.Forms.Padding(1);
            this.txtDescp.Editor.Name = "textEdit";
            this.txtDescp.Editor.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.txtDescp.Editor.Properties.Appearance.BackColor2 = System.Drawing.Color.White;
            this.txtDescp.Editor.Properties.Appearance.Font = new System.Drawing.Font("Dotum", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtDescp.Editor.Properties.Appearance.Options.UseBackColor = true;
            this.txtDescp.Editor.Properties.Appearance.Options.UseFont = true;
            this.txtDescp.Editor.Properties.AppearanceReadOnly.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.txtDescp.Editor.Properties.AppearanceReadOnly.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.txtDescp.Editor.Properties.AppearanceReadOnly.Options.UseBackColor = true;
            this.txtDescp.Editor.Properties.AutoHeight = false;
            this.txtDescp.Editor.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.txtDescp.Editor.Size = new System.Drawing.Size(428, 28);
            this.txtDescp.Editor.TabIndex = 0;
            this.txtDescp.EditorSize = new System.Drawing.Size(428, 28);
            this.txtDescp.FieldName = null;
            // 
            // 
            // 
            this.txtDescp.Label.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.txtDescp.Label.Font = new System.Drawing.Font("Dotum", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtDescp.Label.Location = new System.Drawing.Point(0, 8);
            this.txtDescp.Label.Margin = new System.Windows.Forms.Padding(0);
            this.txtDescp.Label.Name = "DisplayLabel";
            this.txtDescp.Label.Size = new System.Drawing.Size(100, 13);
            this.txtDescp.Label.TabIndex = 1;
            this.txtDescp.Label.Text = "프로그램 설명";
            this.txtDescp.Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.txtDescp.LabelAutoSize = false;
            this.txtDescp.LabelSize = new System.Drawing.Size(100, 13);
            this.txtDescp.Location = new System.Drawing.Point(0, 38);
            this.txtDescp.Margin = new System.Windows.Forms.Padding(0, 0, 15, 12);
            this.txtDescp.MinimumSize = new System.Drawing.Size(0, 27);
            this.txtDescp.Name = "txtDescp";
            this.txtDescp.ParentContainer = this.flowLayoutPanel1;
            this.txtDescp.Size = new System.Drawing.Size(530, 30);
            this.txtDescp.TabIndex = 5;
            // 
            // txtFileName
            // 
            this.txtFileName.AutoSize = true;
            this.txtFileName.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.txtFileName.BackColor = System.Drawing.Color.Transparent;
            this.txtFileName.Caption = "파일 선택";
            this.txtFileName.DefaultCode = "";
            // 
            // 
            // 
            this.txtFileName.Editor.Location = new System.Drawing.Point(1, 1);
            this.txtFileName.Editor.Margin = new System.Windows.Forms.Padding(1);
            this.txtFileName.Editor.Name = "textEdit";
            this.txtFileName.Editor.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.txtFileName.Editor.Properties.Appearance.BackColor2 = System.Drawing.Color.White;
            this.txtFileName.Editor.Properties.Appearance.Font = new System.Drawing.Font("Dotum", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtFileName.Editor.Properties.Appearance.Options.UseBackColor = true;
            this.txtFileName.Editor.Properties.Appearance.Options.UseFont = true;
            this.txtFileName.Editor.Properties.AppearanceReadOnly.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.txtFileName.Editor.Properties.AppearanceReadOnly.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
            this.txtFileName.Editor.Properties.AppearanceReadOnly.Options.UseBackColor = true;
            this.txtFileName.Editor.Properties.AutoHeight = false;
            this.txtFileName.Editor.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.txtFileName.Editor.Properties.ReadOnly = true;
            this.txtFileName.Editor.Size = new System.Drawing.Size(384, 28);
            this.txtFileName.Editor.TabIndex = 0;
            this.txtFileName.EditorSize = new System.Drawing.Size(384, 28);
            this.txtFileName.FieldName = null;
            // 
            // 
            // 
            this.txtFileName.Label.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.txtFileName.Label.Font = new System.Drawing.Font("Dotum", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtFileName.Label.Location = new System.Drawing.Point(0, 8);
            this.txtFileName.Label.Margin = new System.Windows.Forms.Padding(0);
            this.txtFileName.Label.Name = "DisplayLabel";
            this.txtFileName.Label.Size = new System.Drawing.Size(100, 13);
            this.txtFileName.Label.TabIndex = 1;
            this.txtFileName.Label.Text = "파일 선택";
            this.txtFileName.Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.txtFileName.LabelAutoSize = false;
            this.txtFileName.LabelSize = new System.Drawing.Size(100, 13);
            this.txtFileName.Location = new System.Drawing.Point(545, 38);
            this.txtFileName.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.txtFileName.MinimumSize = new System.Drawing.Size(0, 27);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.ParentContainer = this.flowLayoutPanel1;
            this.txtFileName.ReadOnly = true;
            this.txtFileName.Size = new System.Drawing.Size(486, 30);
            this.txtFileName.TabIndex = 8;
            // 
            // btnFile
            // 
            this.btnFile.BackgroundImage = global::RFID_CONTROLLER.Properties.Resources.ico_btn_add_black;
            this.btnFile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnFile.Font = new System.Drawing.Font("Gulim", 9F);
            this.btnFile.Location = new System.Drawing.Point(1031, 38);
            this.btnFile.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.btnFile.Name = "btnFile";
            this.btnFile.Size = new System.Drawing.Size(30, 30);
            this.btnFile.TabIndex = 9;
            this.btnFile.UseVisualStyleBackColor = true;
            this.btnFile.Click += new System.EventHandler(this.btnFile_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // SWManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1084, 561);
            this.ControlBox = false;
            this.Controls.Add(this.programMngTableLayout);
            this.Controls.Add(this.headerPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "SWManager";
            this.Text = "ProgramMngPopUp";
            this.Load += new System.EventHandler(this.SWManager_Load);
            this.headerPanel.ResumeLayout(false);
            this.programMngTableLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SWManagerGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cbEqType.Editor.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cbSwType.Editor.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSwNm.Editor.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSwSeq.Editor.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDescp.Editor.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtFileName.Editor.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.TableLayoutPanel programMngTableLayout;
        private System.Windows.Forms.Label label1;
        private DevExpress.XtraGrid.GridControl SWManagerGrid;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn3;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn4;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn5;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn6;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private DevExpress.XtraEditors.LabelControl btnSwAdd;
        private DevExpress.XtraEditors.LabelControl btnSwSave;
        private DevExpress.XtraEditors.LabelControl btnSwDel;
        private DevExpress.XtraEditors.LabelControl buttonClose;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private Controls.Editors.ComboBoxEditor cbEqType;
        private Controls.Editors.ComboBoxEditor cbSwType;
        private Controls.Editors.TextEditor txtSwNm;
        private Controls.Editors.TextEditor txtDescp;
        private Controls.Editors.TextEditor txtSwSeq;
        private System.Windows.Forms.Button btnSWSearch;
        private Controls.Editors.TextEditor txtFileName;
        private System.Windows.Forms.Button btnFile;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}