
namespace RFIDTagWriter
{
    partial class MainForm
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
            this.listID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.socketID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Connection = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ConnectionTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LastCommunicationTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.logListBox = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
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
            // logListBox
            // 
            this.logListBox.BackColor = System.Drawing.Color.Black;
            this.logListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.logListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logListBox.Font = new System.Drawing.Font("돋움체", 9F);
            this.logListBox.ForeColor = System.Drawing.Color.White;
            this.logListBox.FormattingEnabled = true;
            this.logListBox.ItemHeight = 18;
            this.logListBox.Location = new System.Drawing.Point(0, 0);
            this.logListBox.Margin = new System.Windows.Forms.Padding(4);
            this.logListBox.Name = "logListBox";
            this.logListBox.Size = new System.Drawing.Size(1162, 1089);
            this.logListBox.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1162, 1089);
            this.Controls.Add(this.logListBox);
            this.Name = "MainForm";
            this.Text = "Print Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ColumnHeader listID;
        private System.Windows.Forms.ColumnHeader socketID;
        private System.Windows.Forms.ColumnHeader ID;
        private System.Windows.Forms.ColumnHeader Connection;
        private System.Windows.Forms.ColumnHeader ConnectionTime;
        private System.Windows.Forms.ColumnHeader LastCommunicationTime;
        private System.Windows.Forms.ListBox logListBox;
    }
}