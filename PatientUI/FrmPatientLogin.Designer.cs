namespace PatientUI
{
    partial class FrmPatientLogin
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.txtPhone = new System.Windows.Forms.TextBox();
            this.labelphone = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPwd = new System.Windows.Forms.TextBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.lblMsg = new System.Windows.Forms.Label();
            this.btnRegister = new System.Windows.Forms.Button();
            this.btnForgotPwd = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtPhone
            // 
            this.txtPhone.Location = new System.Drawing.Point(377, 75);
            this.txtPhone.Name = "txtPhone";
            this.txtPhone.Size = new System.Drawing.Size(339, 35);
            this.txtPhone.TabIndex = 0;
            // 
            // labelphone
            // 
            this.labelphone.AutoSize = true;
            this.labelphone.Location = new System.Drawing.Point(268, 78);
            this.labelphone.Name = "labelphone";
            this.labelphone.Size = new System.Drawing.Size(82, 24);
            this.labelphone.TabIndex = 1;
            this.labelphone.Text = "手机号";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(292, 176);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 24);
            this.label1.TabIndex = 3;
            this.label1.Text = "密码";
            // 
            // txtPwd
            // 
            this.txtPwd.Location = new System.Drawing.Point(377, 165);
            this.txtPwd.Name = "txtPwd";
            this.txtPwd.PasswordChar = '*';
            this.txtPwd.Size = new System.Drawing.Size(339, 35);
            this.txtPwd.TabIndex = 2;
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(277, 447);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(132, 69);
            this.btnLogin.TabIndex = 4;
            this.btnLogin.Text = "登录";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(542, 447);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(125, 68);
            this.btnExit.TabIndex = 5;
            this.btnExit.Text = "退出";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // lblMsg
            // 
            this.lblMsg.AutoSize = true;
            this.lblMsg.Location = new System.Drawing.Point(277, 248);
            this.lblMsg.Name = "lblMsg";
            this.lblMsg.Size = new System.Drawing.Size(0, 24);
            this.lblMsg.TabIndex = 6;
            // 
            // btnRegister
            // 
            this.btnRegister.Location = new System.Drawing.Point(263, 307);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(156, 51);
            this.btnRegister.TabIndex = 7;
            this.btnRegister.Text = "注册账号";
            this.btnRegister.UseVisualStyleBackColor = true;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click_1);
            // 
            // btnForgotPwd
            // 
            this.btnForgotPwd.Location = new System.Drawing.Point(528, 307);
            this.btnForgotPwd.Name = "btnForgotPwd";
            this.btnForgotPwd.Size = new System.Drawing.Size(165, 54);
            this.btnForgotPwd.TabIndex = 8;
            this.btnForgotPwd.Text = "忘记密码";
            this.btnForgotPwd.UseVisualStyleBackColor = true;
            this.btnForgotPwd.Click += new System.EventHandler(this.btnForgotPwd_Click);
            // 
            // FrmPatientLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1059, 589);
            this.Controls.Add(this.btnForgotPwd);
            this.Controls.Add(this.btnRegister);
            this.Controls.Add(this.lblMsg);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtPwd);
            this.Controls.Add(this.labelphone);
            this.Controls.Add(this.txtPhone);
            this.Name = "FrmPatientLogin";
            this.Text = "患者端";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtPhone;
        private System.Windows.Forms.Label labelphone;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPwd;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Label lblMsg;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.Button btnForgotPwd;
    }
}

