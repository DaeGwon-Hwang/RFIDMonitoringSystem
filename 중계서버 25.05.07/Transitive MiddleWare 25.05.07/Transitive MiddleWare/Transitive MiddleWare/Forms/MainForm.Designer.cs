namespace BPNS.TransitiveMiddleware
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

            if (disposing && (listener != null))
            {
                listener.Dispose();
            }
            if (disposing && (Mwlistener != null))
            {
                Mwlistener.Dispose();
            }
            if (disposing && (Pamslistener != null))
            {
                Pamslistener.Dispose();
            }
            if (disposing && (runCheckMutex != null))
            {
                runCheckMutex.Dispose();
            }
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.logListBox = new System.Windows.Forms.ListBox();
            this.mwStateCheckTimer = new System.Windows.Forms.Timer(this.components);
            this.ScheduleCheckTimer = new System.Windows.Forms.Timer(this.components);
            this.CommDataTimer = new System.Windows.Forms.Timer(this.components);
            this.txtConnCount = new System.Windows.Forms.TextBox();
            this.CrAllTimer = new System.Windows.Forms.Timer(this.components);
            this.mwListView = new BPNS.TransitiveMiddleware.DoubleBufferedListView();
            this.listID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.socketID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Connection = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ConnectionTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LastCommunicationTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // logListBox
            // 
            this.logListBox.BackColor = System.Drawing.Color.Black;
            this.logListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.logListBox.Font = new System.Drawing.Font("돋움체", 9F);
            this.logListBox.ForeColor = System.Drawing.Color.White;
            this.logListBox.FormattingEnabled = true;
            this.logListBox.ItemHeight = 18;
            this.logListBox.Location = new System.Drawing.Point(1, 192);
            this.logListBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.logListBox.Name = "logListBox";
            this.logListBox.Size = new System.Drawing.Size(990, 1262);
            this.logListBox.TabIndex = 0;
            // 
            // mwStateCheckTimer
            // 
            this.mwStateCheckTimer.Tick += new System.EventHandler(this.mwStateCheckTimer_Tick);
            // 
            // ScheduleCheckTimer
            // 
            this.ScheduleCheckTimer.Interval = 1000;
            this.ScheduleCheckTimer.Tick += new System.EventHandler(this.ScheduleCheckTimer_Tick);
            // 
            // CommDataTimer
            // 
            this.CommDataTimer.Interval = 10000;
            // 
            // txtConnCount
            // 
            this.txtConnCount.Location = new System.Drawing.Point(168, 150);
            this.txtConnCount.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtConnCount.Name = "txtConnCount";
            this.txtConnCount.ReadOnly = true;
            this.txtConnCount.Size = new System.Drawing.Size(444, 28);
            this.txtConnCount.TabIndex = 3;
            // 
            // CrAllTimer
            // 
            this.CrAllTimer.Interval = 600000;
            // 
            // mwListView
            // 
            this.mwListView.BackColor = System.Drawing.Color.Black;
            this.mwListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.listID,
            this.socketID,
            this.ID,
            this.Connection,
            this.ConnectionTime,
            this.LastCommunicationTime});
            this.mwListView.Font = new System.Drawing.Font("돋움체", 9F);
            this.mwListView.ForeColor = System.Drawing.Color.White;
            this.mwListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.mwListView.HideSelection = false;
            this.mwListView.LabelWrap = false;
            this.mwListView.Location = new System.Drawing.Point(21, 220);
            this.mwListView.Margin = new System.Windows.Forms.Padding(4);
            this.mwListView.MultiSelect = false;
            this.mwListView.Name = "mwListView";
            this.mwListView.Size = new System.Drawing.Size(968, 626);
            this.mwListView.TabIndex = 1;
            this.mwListView.UseCompatibleStateImageBehavior = false;
            this.mwListView.View = System.Windows.Forms.View.Details;
            this.mwListView.Visible = false;
            // 
            // listID
            // 
            this.listID.Text = "listID";
            this.listID.Width = 0;
            // 
            // socketID
            // 
            this.socketID.Text = "Socket ID";
            this.socketID.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.socketID.Width = 90;
            // 
            // ID
            // 
            this.ID.Text = "M/W ID";
            this.ID.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ID.Width = 90;
            // 
            // Connection
            // 
            this.Connection.Text = "Connection";
            this.Connection.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Connection.Width = 90;
            // 
            // ConnectionTime
            // 
            this.ConnectionTime.Text = "Connection Time";
            this.ConnectionTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ConnectionTime.Width = 134;
            // 
            // LastCommunicationTime
            // 
            this.LastCommunicationTime.Text = "Communication Time";
            this.LastCommunicationTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.LastCommunicationTime.Width = 134;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(990, 1462);
            this.Controls.Add(this.txtConnCount);
            this.Controls.Add(this.mwListView);
            this.Controls.Add(this.logListBox);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "MainForm";
            this.Text = "데이터 미들웨어";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_Closing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox logListBox;
        //private System.Windows.Forms.ListView mwListView;
        private DoubleBufferedListView mwListView;
        private System.Windows.Forms.ColumnHeader listID;
        private System.Windows.Forms.ColumnHeader socketID;
        private System.Windows.Forms.ColumnHeader ID;
        private System.Windows.Forms.ColumnHeader Connection;
        private System.Windows.Forms.ColumnHeader ConnectionTime;
        private System.Windows.Forms.Timer mwStateCheckTimer;
        private System.Windows.Forms.ColumnHeader LastCommunicationTime;
        private System.Windows.Forms.Timer ScheduleCheckTimer;
        private System.Windows.Forms.Timer CommDataTimer;
        private System.Windows.Forms.TextBox txtConnCount;
        private System.Windows.Forms.Timer CrAllTimer;
    }
}

