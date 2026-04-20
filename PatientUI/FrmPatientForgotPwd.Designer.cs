namespace PatientUI
{
    partial class FrmPatientForgotPwd
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
            this.txtIdCard = new System.Windows.Forms.TextBox();
            this.txtIdPhone = new System.Windows.Forms.TextBox();
            this.txtNewPwd = new System.Windows.Forms.TextBox();
            this.txtConfirmPwd = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtIdCard
            // 
            this.txtIdCard.Location = new System.Drawing.Point(219, 64);
            this.txtIdCard.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtIdCard.Name = "txtIdCard";
            this.txtIdCard.Size = new System.Drawing.Size(225, 25);
            this.txtIdCard.TabIndex = 0;
            // 
            // txtIdPhone
            // 
            this.txtIdPhone.Location = new System.Drawing.Point(219, 112);
            this.txtIdPhone.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtIdPhone.Name = "txtIdPhone";
            this.txtIdPhone.Size = new System.Drawing.Size(225, 25);
            this.txtIdPhone.TabIndex = 1;
            // 
            // txtNewPwd
            // 
            this.txtNewPwd.Location = new System.Drawing.Point(219, 168);
            this.txtNewPwd.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtNewPwd.Name = "txtNewPwd";
            this.txtNewPwd.Size = new System.Drawing.Size(225, 25);
            this.txtNewPwd.TabIndex = 2;
            // 
            // txtConfirmPwd
            // 
            this.txtConfirmPwd.Location = new System.Drawing.Point(219, 222);
            this.txtConfirmPwd.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtConfirmPwd.Name = "txtConfirmPwd";
            this.txtConfirmPwd.Size = new System.Drawing.Size(225, 25);
            this.txtConfirmPwd.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(129, 229);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(67, 15);
            this.label5.TabIndex = 13;
            this.label5.Text = "确认密码";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(136, 170);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 15);
            this.label4.TabIndex = 12;
            this.label4.Text = "密码";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(136, 118);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 15);
            this.label3.TabIndex = 11;
            this.label3.Text = "手机号";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(136, 77);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 15);
            this.label2.TabIndex = 10;
            this.label2.Text = "身份证";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(149, 291);
            this.btnConfirm.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(91, 46);
            this.btnConfirm.TabIndex = 14;
            this.btnConfirm.Text = "确认重置";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(375, 291);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(112, 46);
            this.btnCancel.TabIndex = 15;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // FrmPatientForgotPwd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(688, 396);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtConfirmPwd);
            this.Controls.Add(this.txtNewPwd);
            this.Controls.Add(this.txtIdPhone);
            this.Controls.Add(this.txtIdCard);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "FrmPatientForgotPwd";
            this.Text = "FrmPatientForgotPwd";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtIdCard;
        private System.Windows.Forms.TextBox txtIdPhone;
        private System.Windows.Forms.TextBox txtNewPwd;
        private System.Windows.Forms.TextBox txtConfirmPwd;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnCancel;
    }
}