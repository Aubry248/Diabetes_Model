namespace PatientUI
{
    partial class FrmPatientRegister
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
            this.txtName = new System.Windows.Forms.TextBox();
            this.txtPhone = new System.Windows.Forms.TextBox();
            this.txtIdCard = new System.Windows.Forms.TextBox();
            this.txtPwd = new System.Windows.Forms.TextBox();
            this.txtConfirmPwd = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cboGender = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.cboDiabetesType = new System.Windows.Forms.ComboBox();
            this.dtpBirthDate = new System.Windows.Forms.DateTimePicker();
            this.label8 = new System.Windows.Forms.Label();
            this.btnRegister = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(172, 45);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(231, 35);
            this.txtName.TabIndex = 0;
            // 
            // txtPhone
            // 
            this.txtPhone.Location = new System.Drawing.Point(172, 178);
            this.txtPhone.Name = "txtPhone";
            this.txtPhone.Size = new System.Drawing.Size(231, 35);
            this.txtPhone.TabIndex = 1;
            // 
            // txtIdCard
            // 
            this.txtIdCard.Location = new System.Drawing.Point(172, 105);
            this.txtIdCard.Name = "txtIdCard";
            this.txtIdCard.Size = new System.Drawing.Size(231, 35);
            this.txtIdCard.TabIndex = 2;
            // 
            // txtPwd
            // 
            this.txtPwd.Location = new System.Drawing.Point(172, 248);
            this.txtPwd.Name = "txtPwd";
            this.txtPwd.Size = new System.Drawing.Size(231, 35);
            this.txtPwd.TabIndex = 3;
            // 
            // txtConfirmPwd
            // 
            this.txtConfirmPwd.Location = new System.Drawing.Point(172, 324);
            this.txtConfirmPwd.Name = "txtConfirmPwd";
            this.txtConfirmPwd.Size = new System.Drawing.Size(231, 35);
            this.txtConfirmPwd.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(52, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 24);
            this.label1.TabIndex = 5;
            this.label1.Text = "姓名";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(52, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 24);
            this.label2.TabIndex = 6;
            this.label2.Text = "身份证";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(52, 181);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 24);
            this.label3.TabIndex = 7;
            this.label3.Text = "手机号";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(52, 248);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 24);
            this.label4.TabIndex = 8;
            this.label4.Text = "密码";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(52, 327);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(106, 24);
            this.label5.TabIndex = 9;
            this.label5.Text = "确认密码";
            // 
            // cboGender
            // 
            this.cboGender.FormattingEnabled = true;
            this.cboGender.Items.AddRange(new object[] {
            "男",
            "女"});
            this.cboGender.Location = new System.Drawing.Point(632, 63);
            this.cboGender.Name = "cboGender";
            this.cboGender.Size = new System.Drawing.Size(196, 32);
            this.cboGender.TabIndex = 10;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(486, 63);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(58, 24);
            this.label6.TabIndex = 11;
            this.label6.Text = "性别";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(486, 152);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(130, 24);
            this.label7.TabIndex = 13;
            this.label7.Text = "糖尿病类型";
            // 
            // cboDiabetesType
            // 
            this.cboDiabetesType.FormattingEnabled = true;
            this.cboDiabetesType.Items.AddRange(new object[] {
            "1型",
            "2型",
            "妊娠",
            "其他"});
            this.cboDiabetesType.Location = new System.Drawing.Point(632, 144);
            this.cboDiabetesType.Name = "cboDiabetesType";
            this.cboDiabetesType.Size = new System.Drawing.Size(196, 32);
            this.cboDiabetesType.TabIndex = 12;
            // 
            // dtpBirthDate
            // 
            this.dtpBirthDate.Location = new System.Drawing.Point(632, 244);
            this.dtpBirthDate.Name = "dtpBirthDate";
            this.dtpBirthDate.Size = new System.Drawing.Size(253, 35);
            this.dtpBirthDate.TabIndex = 14;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(486, 248);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(106, 24);
            this.label8.TabIndex = 15;
            this.label8.Text = "出生日期";
            // 
            // btnRegister
            // 
            this.btnRegister.Location = new System.Drawing.Point(172, 442);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(136, 76);
            this.btnRegister.TabIndex = 16;
            this.btnRegister.Text = "确认";
            this.btnRegister.UseVisualStyleBackColor = true;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(535, 442);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(123, 76);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // FrmPatientRegister
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(938, 578);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnRegister);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.dtpBirthDate);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.cboDiabetesType);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.cboGender);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtConfirmPwd);
            this.Controls.Add(this.txtPwd);
            this.Controls.Add(this.txtIdCard);
            this.Controls.Add(this.txtPhone);
            this.Controls.Add(this.txtName);
            this.Name = "FrmPatientRegister";
            this.Text = "患者注册";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.TextBox txtPhone;
        private System.Windows.Forms.TextBox txtIdCard;
        private System.Windows.Forms.TextBox txtPwd;
        private System.Windows.Forms.TextBox txtConfirmPwd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cboGender;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cboDiabetesType;
        private System.Windows.Forms.DateTimePicker dtpBirthDate;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.Button btnCancel;
    }
}