namespace AdminUI
{
    partial class FrmAdminMain
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
            this.statusStripMain = new System.Windows.Forms.StatusStrip();
            this.lblStatusTip = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStripMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStripMain
            // 
            this.statusStripMain.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.statusStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatusTip});
            this.statusStripMain.Location = new System.Drawing.Point(0, 689);
            this.statusStripMain.Name = "statusStripMain";
            this.statusStripMain.Size = new System.Drawing.Size(1163, 41);
            this.statusStripMain.TabIndex = 1;
            this.statusStripMain.Text = "statusStrip1";
            // 
            // lblStatusTip
            // 
            this.lblStatusTip.Name = "lblStatusTip";
            this.lblStatusTip.Size = new System.Drawing.Size(182, 31);
            this.lblStatusTip.Text = "操作提示：就绪";
            // 
            // FrmAdminMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(1163, 730);
            this.Controls.Add(this.statusStripMain);
            this.IsMdiContainer = true;
            this.Name = "FrmAdminMain";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Tag = "FrmUserInfoManage";
            this.Text = "糖尿病患者综合健康管理系统 - 管理员后台";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmAdminMain_FormClosed);
            this.statusStripMain.ResumeLayout(false);
            this.statusStripMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.StatusStrip statusStripMain;
        public System.Windows.Forms.ToolStripStatusLabel lblStatusTip;
    }
}