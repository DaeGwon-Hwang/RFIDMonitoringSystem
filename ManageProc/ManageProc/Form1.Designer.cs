namespace ManageProc
{
    partial class frmManageProc
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmManageProc));
            this.logListBox = new System.Windows.Forms.ListBox();
            this.TrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.TrayIconMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TrayIconMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // logListBox
            // 
            this.logListBox.BackColor = System.Drawing.Color.Black;
            this.logListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.logListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logListBox.Font = new System.Drawing.Font("돋움체", 9F);
            this.logListBox.ForeColor = System.Drawing.Color.White;
            this.logListBox.FormattingEnabled = true;
            this.logListBox.ItemHeight = 12;
            this.logListBox.Location = new System.Drawing.Point(0, 0);
            this.logListBox.Name = "logListBox";
            this.logListBox.Size = new System.Drawing.Size(861, 359);
            this.logListBox.TabIndex = 2;
            // 
            // TrayIcon
            // 
            this.TrayIcon.ContextMenuStrip = this.TrayIconMenu;
            this.TrayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("TrayIcon.Icon")));
            this.TrayIcon.Text = "ProcManage";
            this.TrayIcon.Visible = true;
            this.TrayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TrayIcon_MouseDoubleClick);
            // 
            // TrayIconMenu
            // 
            this.TrayIconMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.TrayIconMenu.Name = "TrayIconMenu";
            this.TrayIconMenu.Size = new System.Drawing.Size(105, 48);
            // 
            // showToolStripMenuItem
            // 
            this.showToolStripMenuItem.Name = "showToolStripMenuItem";
            this.showToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.showToolStripMenuItem.Text = "Show";
            this.showToolStripMenuItem.Click += new System.EventHandler(this.showToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // frmManageProc
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(861, 359);
            this.Controls.Add(this.logListBox);
            this.Name = "frmManageProc";
            this.Text = "관리 프로그램";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmManageProc_FormClosing);
            this.Load += new System.EventHandler(this.frmManageProc_Load);
            this.Shown += new System.EventHandler(this.frmManageProc_Shown);
            this.TrayIconMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox logListBox;
        private System.Windows.Forms.NotifyIcon TrayIcon;
        private System.Windows.Forms.ContextMenuStrip TrayIconMenu;
        private System.Windows.Forms.ToolStripMenuItem showToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    }
}

