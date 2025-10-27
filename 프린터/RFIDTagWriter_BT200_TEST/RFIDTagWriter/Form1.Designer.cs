namespace RFIDTagWriter
{
	partial class Form1
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.설정ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.btnPrint = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnInternalCommand = new System.Windows.Forms.Button();
            this.btnSendStr = new System.Windows.Forms.Button();
            this.btnClosePort = new System.Windows.Forms.Button();
            this.lblMsg = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.bntAlive = new System.Windows.Forms.Button();
            this.btnSetup = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.tbIpAddr = new System.Windows.Forms.TextBox();
            this.rdoConnTCP = new System.Windows.Forms.RadioButton();
            this.rdoConnUSB = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.설정ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
            this.menuStrip1.Size = new System.Drawing.Size(1302, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 설정ToolStripMenuItem
            // 
            this.설정ToolStripMenuItem.Name = "설정ToolStripMenuItem";
            this.설정ToolStripMenuItem.Size = new System.Drawing.Size(79, 22);
            this.설정ToolStripMenuItem.Text = "프린터설정";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.button4);
            this.groupBox1.Controls.Add(this.button3);
            this.groupBox1.Controls.Add(this.btnPrint);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.btnInternalCommand);
            this.groupBox1.Controls.Add(this.btnSendStr);
            this.groupBox1.Controls.Add(this.btnClosePort);
            this.groupBox1.Controls.Add(this.lblMsg);
            this.groupBox1.Controls.Add(this.btnConnect);
            this.groupBox1.Controls.Add(this.bntAlive);
            this.groupBox1.Controls.Add(this.btnSetup);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.tbIpAddr);
            this.groupBox1.Controls.Add(this.rdoConnTCP);
            this.groupBox1.Controls.Add(this.rdoConnUSB);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(13, 28);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1280, 341);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "세팅";
            // 
            // button4
            // 
            this.button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.button4.Location = new System.Drawing.Point(6, 111);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(226, 35);
            this.button4.TabIndex = 26;
            this.button4.Text = "Print Test(-01)";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.button3.Location = new System.Drawing.Point(6, 63);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(226, 35);
            this.button3.TabIndex = 25;
            this.button3.Text = "Print Test";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // btnPrint
            // 
            this.btnPrint.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnPrint.Location = new System.Drawing.Point(930, 131);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(226, 35);
            this.btnPrint.TabIndex = 24;
            this.btnPrint.Text = "Print Test";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.button2.Location = new System.Drawing.Point(930, 91);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(226, 35);
            this.button2.TabIndex = 23;
            this.button2.Text = "InternalCommand_speed";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.button1.Location = new System.Drawing.Point(930, 51);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(226, 35);
            this.button1.TabIndex = 22;
            this.button1.Text = "InternalCommand_eth_ip";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnInternalCommand
            // 
            this.btnInternalCommand.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnInternalCommand.Location = new System.Drawing.Point(930, 11);
            this.btnInternalCommand.Name = "btnInternalCommand";
            this.btnInternalCommand.Size = new System.Drawing.Size(226, 35);
            this.btnInternalCommand.TabIndex = 21;
            this.btnInternalCommand.Text = "InternalCommand_print";
            this.btnInternalCommand.UseVisualStyleBackColor = true;
            this.btnInternalCommand.Click += new System.EventHandler(this.btnInternalCommand_Click);
            // 
            // btnSendStr
            // 
            this.btnSendStr.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnSendStr.Location = new System.Drawing.Point(817, 11);
            this.btnSendStr.Name = "btnSendStr";
            this.btnSendStr.Size = new System.Drawing.Size(108, 35);
            this.btnSendStr.TabIndex = 20;
            this.btnSendStr.Text = "SendStr";
            this.btnSendStr.UseVisualStyleBackColor = true;
            this.btnSendStr.Click += new System.EventHandler(this.btnSendStr_Click);
            // 
            // btnClosePort
            // 
            this.btnClosePort.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnClosePort.Location = new System.Drawing.Point(589, 11);
            this.btnClosePort.Name = "btnClosePort";
            this.btnClosePort.Size = new System.Drawing.Size(108, 35);
            this.btnClosePort.TabIndex = 19;
            this.btnClosePort.Text = "ClosePort";
            this.btnClosePort.UseVisualStyleBackColor = true;
            this.btnClosePort.Click += new System.EventHandler(this.btnClosePort_Click);
            // 
            // lblMsg
            // 
            this.lblMsg.AutoSize = true;
            this.lblMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblMsg.Location = new System.Drawing.Point(18, 300);
            this.lblMsg.Name = "lblMsg";
            this.lblMsg.Size = new System.Drawing.Size(0, 31);
            this.lblMsg.TabIndex = 18;
            // 
            // btnConnect
            // 
            this.btnConnect.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnConnect.Location = new System.Drawing.Point(391, 11);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(80, 35);
            this.btnConnect.TabIndex = 17;
            this.btnConnect.Text = "연결";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // bntAlive
            // 
            this.bntAlive.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.bntAlive.Location = new System.Drawing.Point(476, 11);
            this.bntAlive.Name = "bntAlive";
            this.bntAlive.Size = new System.Drawing.Size(108, 35);
            this.bntAlive.TabIndex = 16;
            this.bntAlive.Text = "연결확인";
            this.bntAlive.UseVisualStyleBackColor = true;
            this.bntAlive.Click += new System.EventHandler(this.bntAlive_Click);
            // 
            // btnSetup
            // 
            this.btnSetup.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnSetup.Location = new System.Drawing.Point(703, 11);
            this.btnSetup.Name = "btnSetup";
            this.btnSetup.Size = new System.Drawing.Size(108, 35);
            this.btnSetup.TabIndex = 15;
            this.btnSetup.Text = "Setup";
            this.btnSetup.UseVisualStyleBackColor = true;
            this.btnSetup.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(240, 29);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(40, 12);
            this.label11.TabIndex = 9;
            this.label11.Text = "IP주소";
            // 
            // tbIpAddr
            // 
            this.tbIpAddr.Location = new System.Drawing.Point(286, 24);
            this.tbIpAddr.Name = "tbIpAddr";
            this.tbIpAddr.Size = new System.Drawing.Size(100, 21);
            this.tbIpAddr.TabIndex = 8;
            this.tbIpAddr.Text = "192.168.200.224";
            // 
            // rdoConnTCP
            // 
            this.rdoConnTCP.AutoSize = true;
            this.rdoConnTCP.Checked = true;
            this.rdoConnTCP.Location = new System.Drawing.Point(163, 27);
            this.rdoConnTCP.Name = "rdoConnTCP";
            this.rdoConnTCP.Size = new System.Drawing.Size(59, 16);
            this.rdoConnTCP.TabIndex = 7;
            this.rdoConnTCP.TabStop = true;
            this.rdoConnTCP.Text = "TCPIP";
            this.rdoConnTCP.UseVisualStyleBackColor = true;
            this.rdoConnTCP.CheckedChanged += new System.EventHandler(this.rdoConnTCP_CheckedChanged);
            // 
            // rdoConnUSB
            // 
            this.rdoConnUSB.AutoSize = true;
            this.rdoConnUSB.Location = new System.Drawing.Point(96, 27);
            this.rdoConnUSB.Name = "rdoConnUSB";
            this.rdoConnUSB.Size = new System.Drawing.Size(47, 16);
            this.rdoConnUSB.TabIndex = 6;
            this.rdoConnUSB.Text = "USB";
            this.rdoConnUSB.UseVisualStyleBackColor = true;
            this.rdoConnUSB.Click += new System.EventHandler(this.rdoConnUSB_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "연결방법";
            // 
            // timer1
            // 
            this.timer1.Interval = 10;
            // 
            // timer2
            // 
            this.timer2.Interval = 2000;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1302, 378);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "RFID 태그발행 프로그램";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem 설정ToolStripMenuItem;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.TextBox tbIpAddr;
		private System.Windows.Forms.RadioButton rdoConnTCP;
		private System.Windows.Forms.RadioButton rdoConnUSB;
        private System.Windows.Forms.Button btnSetup;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Button bntAlive;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label lblMsg;
        private System.Windows.Forms.Button btnClosePort;
        private System.Windows.Forms.Button btnSendStr;
        private System.Windows.Forms.Button btnInternalCommand;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
    }
}

