namespace RFID_CONTROLLER.Controls.Editors
{
    partial class CheckBoxEditor
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
            this.CheckBoxPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.MainPanel.SuspendLayout();
            this.CheckBoxPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.AutoSize = true;
            this.MainPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MainPanel.BackColor = System.Drawing.Color.Transparent;
            this.MainPanel.Controls.Add(this.DisplayLabel);
            this.MainPanel.Controls.Add(this.CheckBoxPanel);
            this.MainPanel.Location = new System.Drawing.Point(0, 0);
            this.MainPanel.Margin = new System.Windows.Forms.Padding(0);
            this.MainPanel.MinimumSize = new System.Drawing.Size(0, 27);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(117, 27);
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
            // 
            // CheckBoxPanel
            // 
            this.CheckBoxPanel.AutoSize = true;
            this.CheckBoxPanel.BackColor = System.Drawing.Color.Transparent;
            this.CheckBoxPanel.Controls.Add(this.checkBox1);
            this.CheckBoxPanel.Location = new System.Drawing.Point(49, 0);
            this.CheckBoxPanel.Margin = new System.Windows.Forms.Padding(0);
            this.CheckBoxPanel.MinimumSize = new System.Drawing.Size(0, 27);
            this.CheckBoxPanel.Name = "CheckBoxPanel";
            this.CheckBoxPanel.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.CheckBoxPanel.Size = new System.Drawing.Size(68, 27);
            this.CheckBoxPanel.TabIndex = 0;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Font = new System.Drawing.Font("돋움", 9.75F);
            this.checkBox1.Location = new System.Drawing.Point(5, 6);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(58, 17);
            this.checkBox1.TabIndex = 0;
            this.checkBox1.Text = "체크1";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // CheckBoxEditor
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.MainPanel);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(0, 0, 15, 12);
            this.MinimumSize = new System.Drawing.Size(0, 27);
            this.Name = "CheckBoxEditor";
            this.Size = new System.Drawing.Size(117, 27);
            this.Load += new System.EventHandler(this.CheckBoxEditor_Load);
            this.Enter += new System.EventHandler(this.CheckBoxEditor_Enter);
            this.Leave += new System.EventHandler(this.CheckBoxEditor_Leave);
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.CheckBoxPanel.ResumeLayout(false);
            this.CheckBoxPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel MainPanel;
        private System.Windows.Forms.FlowLayoutPanel CheckBoxPanel;
        private System.Windows.Forms.Label DisplayLabel;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}
