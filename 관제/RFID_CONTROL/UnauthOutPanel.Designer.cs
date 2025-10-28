namespace RFID_CONTROLLER
{
    partial class UnauthOutPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.roundBorderPanel = new RFID_CONTROLLER.Controls.Editors.RoundBorderPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblDate = new System.Windows.Forms.Label();
            this.lblFloorGate = new System.Windows.Forms.Label();
            this.lblMgmtId = new System.Windows.Forms.Label();
            this.lblInOut = new System.Windows.Forms.Label();
            this.roundBorderPanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // roundBorderPanel
            // 
            this.roundBorderPanel.BackColor = System.Drawing.Color.Transparent;
            this.roundBorderPanel.Controls.Add(this.tableLayoutPanel1);
            this.roundBorderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.roundBorderPanel.Location = new System.Drawing.Point(0, 0);
            this.roundBorderPanel.Name = "roundBorderPanel";
            this.roundBorderPanel.Padding = new System.Windows.Forms.Padding(3);
            this.roundBorderPanel.Size = new System.Drawing.Size(140, 100);
            this.roundBorderPanel.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.lblDate, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblFloorGate, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblMgmtId, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblInOut, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(134, 94);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // lblDate
            // 
            this.lblDate.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblDate, 2);
            this.lblDate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDate.Font = new System.Drawing.Font("Gulim", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblDate.ForeColor = System.Drawing.SystemColors.Control;
            this.lblDate.Location = new System.Drawing.Point(0, 0);
            this.lblDate.Margin = new System.Windows.Forms.Padding(0);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(134, 28);
            this.lblDate.TabIndex = 0;
            this.lblDate.Text = "lblDate";
            this.lblDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFloorGate
            // 
            this.lblFloorGate.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblFloorGate, 2);
            this.lblFloorGate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFloorGate.Font = new System.Drawing.Font("Gulim", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblFloorGate.ForeColor = System.Drawing.SystemColors.Control;
            this.lblFloorGate.Location = new System.Drawing.Point(0, 28);
            this.lblFloorGate.Margin = new System.Windows.Forms.Padding(0);
            this.lblFloorGate.Name = "lblFloorGate";
            this.lblFloorGate.Size = new System.Drawing.Size(134, 18);
            this.lblFloorGate.TabIndex = 1;
            this.lblFloorGate.Text = "lblFloorGate";
            this.lblFloorGate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblMgmtId
            // 
            this.lblMgmtId.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblMgmtId, 2);
            this.lblMgmtId.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMgmtId.Font = new System.Drawing.Font("Gulim", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblMgmtId.ForeColor = System.Drawing.SystemColors.Control;
            this.lblMgmtId.Location = new System.Drawing.Point(0, 46);
            this.lblMgmtId.Margin = new System.Windows.Forms.Padding(0);
            this.lblMgmtId.Name = "lblMgmtId";
            this.lblMgmtId.Size = new System.Drawing.Size(134, 18);
            this.lblMgmtId.TabIndex = 2;
            this.lblMgmtId.Text = "lblMgmtId";
            this.lblMgmtId.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblInOut
            // 
            this.lblInOut.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblInOut, 2);
            this.lblInOut.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblInOut.Font = new System.Drawing.Font("Gulim", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblInOut.ForeColor = System.Drawing.SystemColors.Control;
            this.lblInOut.Location = new System.Drawing.Point(0, 64);
            this.lblInOut.Margin = new System.Windows.Forms.Padding(0);
            this.lblInOut.Name = "lblInOut";
            this.lblInOut.Size = new System.Drawing.Size(134, 30);
            this.lblInOut.TabIndex = 3;
            this.lblInOut.Text = "lblInOut";
            this.lblInOut.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // UnauthOutPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.roundBorderPanel);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "UnauthOutPanel";
            this.Size = new System.Drawing.Size(140, 100);
            this.roundBorderPanel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.Editors.RoundBorderPanel roundBorderPanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblDate;
        private System.Windows.Forms.Label lblFloorGate;
        private System.Windows.Forms.Label lblMgmtId;
        private System.Windows.Forms.Label lblInOut;
    }
}
