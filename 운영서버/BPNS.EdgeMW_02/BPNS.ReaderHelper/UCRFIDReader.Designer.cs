namespace BPNS.ReaderHelper
{
    partial class UCRFIDReader
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
            this.components = new System.ComponentModel.Container();
            this.iFormLoadingLoop1 = new iFormControls.iFormLoadingLoop(this.components);
            this.lbStatus = new System.Windows.Forms.Label();
            this.lbStartDT = new System.Windows.Forms.Label();
            this.lbConnectIP = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // iFormLoadingLoop1
            // 
            this.iFormLoadingLoop1.Color = System.Drawing.Color.Blue;
            this.iFormLoadingLoop1.InnerCircleRadius = 6;
            this.iFormLoadingLoop1.isRunning = false;
            this.iFormLoadingLoop1.Location = new System.Drawing.Point(12, 23);
            this.iFormLoadingLoop1.LoopDensity = 9;
            this.iFormLoadingLoop1.LoopLineThickness = 4;
            this.iFormLoadingLoop1.Name = "iFormLoadingLoop1";
            this.iFormLoadingLoop1.OuterCircleRadius = 7;
            this.iFormLoadingLoop1.RotationSpeed = 50;
            this.iFormLoadingLoop1.Size = new System.Drawing.Size(36, 41);
            this.iFormLoadingLoop1.Style = iFormControls.iFormLoadingLoop.StylePresets.Firefox;
            this.iFormLoadingLoop1.TabIndex = 15;
            this.iFormLoadingLoop1.Text = "iFormLoadingLoop1";
            // 
            // lbStatus
            // 
            this.lbStatus.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbStatus.ForeColor = System.Drawing.Color.Maroon;
            this.lbStatus.Location = new System.Drawing.Point(61, 74);
            this.lbStatus.Name = "lbStatus";
            this.lbStatus.Size = new System.Drawing.Size(183, 19);
            this.lbStatus.TabIndex = 11;
            this.lbStatus.Text = "Not Connected";
            this.lbStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbStartDT
            // 
            this.lbStartDT.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbStartDT.ForeColor = System.Drawing.Color.Black;
            this.lbStartDT.Location = new System.Drawing.Point(61, 53);
            this.lbStartDT.Name = "lbStartDT";
            this.lbStartDT.Size = new System.Drawing.Size(186, 19);
            this.lbStartDT.TabIndex = 12;
            this.lbStartDT.Text = "2017.06.13";
            this.lbStartDT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbConnectIP
            // 
            this.lbConnectIP.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbConnectIP.ForeColor = System.Drawing.Color.Lime;
            this.lbConnectIP.Location = new System.Drawing.Point(61, 32);
            this.lbConnectIP.Name = "lbConnectIP";
            this.lbConnectIP.Size = new System.Drawing.Size(186, 19);
            this.lbConnectIP.TabIndex = 13;
            this.lbConnectIP.Text = "192.168.14.102";
            this.lbConnectIP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblTitle.Location = new System.Drawing.Point(61, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(186, 19);
            this.lblTitle.TabIndex = 14;
            this.lblTitle.Text = "Gate 1 Edge M/W(SN2097E001)";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // UCRFIDReader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.iFormLoadingLoop1);
            this.Controls.Add(this.lbStatus);
            this.Controls.Add(this.lbStartDT);
            this.Controls.Add(this.lbConnectIP);
            this.Controls.Add(this.lblTitle);
            this.Name = "UCRFIDReader";
            this.Size = new System.Drawing.Size(258, 99);
            this.ResumeLayout(false);

        }

        #endregion

        private iFormControls.iFormLoadingLoop iFormLoadingLoop1;
        private System.Windows.Forms.Label lbStatus;
        private System.Windows.Forms.Label lbStartDT;
        private System.Windows.Forms.Label lbConnectIP;
        private System.Windows.Forms.Label lblTitle;
    }
}
