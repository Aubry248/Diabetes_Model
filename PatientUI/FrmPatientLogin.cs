using BLL;
using Model;
using System;
using System.Drawing;
using System.Windows.Forms;
using Tools;

namespace PatientUI
{
    public partial class FrmPatientLogin : Form
    {
        private readonly B_User bllUser = new B_User();

        public FrmPatientLogin()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "糖尿病健康管理系统 - 患者登录";
            ApplyVisualStyle();
        }

        private void ApplyVisualStyle()
        {
            BackColor = Color.FromArgb(241, 245, 249);
            Font = new Font("微软雅黑", 9.5F);
            ClientSize = new Size(960, 620);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            AcceptButton = btnLogin;
            CancelButton = btnExit;

            int buttonWidth = 120;
            int buttonGap = 20;
            int buttonLeft = (500 - buttonWidth * 2 - buttonGap) / 2;
            int secondColumnLeft = buttonLeft + buttonWidth + buttonGap;

            Panel cardPanel = new Panel
            {
                Size = new Size(500, 410),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point((ClientSize.Width - 500) / 2, (ClientSize.Height - 410) / 2)
            };

            Label lblTitle = new Label
            {
                Text = "患者端登录",
                Font = new Font("微软雅黑", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(36, 28),
                Size = new Size(200, 36)
            };

            Label lblSubTitle = new Label
            {
                Text = "登录后即可查看血糖、用药、饮食与运动管理信息",
                Font = new Font("微软雅黑", 9.5F),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(36, 66),
                Size = new Size(360, 24)
            };

            labelphone.Text = "手机号";
            labelphone.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            labelphone.ForeColor = Color.FromArgb(51, 65, 85);
            labelphone.Location = new Point(40, 122);
            labelphone.Size = new Size(80, 28);

            txtPhone.Location = new Point(134, 120);
            txtPhone.Size = new Size(300, 32);
            txtPhone.BorderStyle = BorderStyle.FixedSingle;
            txtPhone.Font = new Font("微软雅黑", 10.5F);

            label1.Text = "密码";
            label1.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(51, 65, 85);
            label1.Location = new Point(40, 182);
            label1.Size = new Size(80, 28);

            txtPwd.Location = new Point(134, 180);
            txtPwd.Size = new Size(300, 32);
            txtPwd.BorderStyle = BorderStyle.FixedSingle;
            txtPwd.Font = new Font("微软雅黑", 10.5F);

            lblMsg.Location = new Point(40, 228);
            lblMsg.Size = new Size(394, 42);
            lblMsg.ForeColor = Color.FromArgb(220, 38, 38);
            lblMsg.Font = new Font("微软雅黑", 9F);

            btnRegister.Location = new Point(buttonLeft, 278);
            btnRegister.Size = new Size(buttonWidth, 38);
            StyleGhostButton(btnRegister, Color.FromArgb(59, 130, 246));

            btnForgotPwd.Location = new Point(secondColumnLeft, 278);
            btnForgotPwd.Size = new Size(buttonWidth, 38);
            StyleGhostButton(btnForgotPwd, Color.FromArgb(245, 158, 11));

            btnLogin.Location = new Point(buttonLeft, 338);
            btnLogin.Size = new Size(buttonWidth, 42);
            StylePrimaryButton(btnLogin, Color.FromArgb(0, 122, 204));

            btnExit.Location = new Point(secondColumnLeft, 338);
            btnExit.Size = new Size(buttonWidth, 42);
            StylePrimaryButton(btnExit, Color.FromArgb(100, 116, 139));

            Controls.Add(cardPanel);
            cardPanel.Controls.Add(lblTitle);
            cardPanel.Controls.Add(lblSubTitle);
            cardPanel.Controls.Add(labelphone);
            cardPanel.Controls.Add(txtPhone);
            cardPanel.Controls.Add(label1);
            cardPanel.Controls.Add(txtPwd);
            cardPanel.Controls.Add(lblMsg);
            cardPanel.Controls.Add(btnRegister);
            cardPanel.Controls.Add(btnForgotPwd);
            cardPanel.Controls.Add(btnLogin);
            cardPanel.Controls.Add(btnExit);
        }

        private void StylePrimaryButton(Button button, Color backColor)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = backColor;
            button.ForeColor = Color.White;
            button.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
        }

        private void StyleGhostButton(Button button, Color accentColor)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = accentColor;
            button.BackColor = Color.White;
            button.ForeColor = accentColor;
            button.Font = new Font("微软雅黑", 9.5F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
        }

        // 登录按钮点击事件
        private void btnLogin_Click(object sender, EventArgs e)
        {
            // 1. 基础输入校验
            string phone = txtPhone.Text.Trim();
            string password = txtPwd.Text.Trim();
            lblMsg.Text = "";

            if (string.IsNullOrEmpty(phone) || phone.Length != 11)
            {
                MessageBox.Show("请输入正确的11位手机号！", "输入提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("请输入登录密码！", "输入提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPwd.Focus();
                return;
            }

            // 2. 调用BLL层登录方法
            Users loginUser = bllUser.UserLogin(phone, password, out string msg);
            lblMsg.Text = msg;

            // 3. 处理登录结果
            if (loginUser != null)
            {
                // 权限校验：只有患者才能登录患者端
                if (loginUser.user_type != 1)
                {
                    MessageBox.Show("您不是患者账号，无法登录患者端！", "权限错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPhone.Clear();
                    txtPwd.Clear();
                    txtPhone.Focus();
                    return;
                }

                // 登录成功：保存全局用户信息
                Program.LoginUser = loginUser;
                MessageBox.Show($"欢迎您，{loginUser.user_name}！", "登录成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 打开患者主窗体（提前创建好FrmPatientMain）
                FrmPatientMain mainForm = new FrmPatientMain();
                mainForm.FormClosed += (s, args) => this.Close();
                mainForm.Show();
                this.Hide();

         
            }
        }

        // 退出按钮点击事件
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // 快捷键优化
        private void txtPhone_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) txtPwd.Focus();
        }
        private void txtPwd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) btnLogin.PerformClick();
        }
        private void FrmPatientLogin_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
        //注册功能
        private void btnRegister_Click_1(object sender, EventArgs e)
        {
            FrmPatientRegister registerForm = new FrmPatientRegister();
            registerForm.ShowDialog();
        }

        private void btnForgotPwd_Click(object sender, EventArgs e)
        {
            FrmPatientForgotPwd FrmPatientForgotPwdForm = new FrmPatientForgotPwd();
            FrmPatientForgotPwdForm.ShowDialog();
        }
    }
}