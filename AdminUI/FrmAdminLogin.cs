using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BLL;
using Model;

namespace AdminUI
{
    public partial class FrmAdminLogin : Form
    {

        // 实例化用户业务逻辑类（三层架构规范：UI层只调用BLL层，不直接操作数据库）
        private readonly B_User bllUser = new B_User();
        public FrmAdminLogin()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "糖尿病健康管理系统 - 管理员登录";
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
                Size = new Size(500, 390),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point((ClientSize.Width - 500) / 2, (ClientSize.Height - 390) / 2)
            };

            Label lblTitle = new Label
            {
                Text = "管理员端登录",
                Font = new Font("微软雅黑", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(36, 28),
                Size = new Size(220, 36)
            };

            Label lblSubTitle = new Label
            {
                Text = "登录后可进行用户、医生与系统数据的后台管理",
                Font = new Font("微软雅黑", 9.5F),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(36, 66),
                Size = new Size(360, 24)
            };

            label1.Text = "管理员账号";
            label1.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(51, 65, 85);
            label1.Location = new Point(40, 124);
            label1.Size = new Size(92, 28);

            txtLoginAccount.Location = new Point(134, 122);
            txtLoginAccount.Size = new Size(300, 32);
            txtLoginAccount.BorderStyle = BorderStyle.FixedSingle;
            txtLoginAccount.Font = new Font("微软雅黑", 10.5F);

            label2.Text = "密码";
            label2.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            label2.ForeColor = Color.FromArgb(51, 65, 85);
            label2.Location = new Point(40, 184);
            label2.Size = new Size(80, 28);

            txtPwd.Location = new Point(134, 182);
            txtPwd.Size = new Size(300, 32);
            txtPwd.BorderStyle = BorderStyle.FixedSingle;
            txtPwd.Font = new Font("微软雅黑", 10.5F);

            lblMsg.Location = new Point(40, 230);
            lblMsg.Size = new Size(394, 42);
            lblMsg.ForeColor = Color.FromArgb(220, 38, 38);
            lblMsg.Font = new Font("微软雅黑", 9F);

            btnLogin.Location = new Point(buttonLeft, 302);
            btnLogin.Size = new Size(buttonWidth, 42);
            StylePrimaryButton(btnLogin, Color.FromArgb(0, 122, 204));

            btnExit.Location = new Point(secondColumnLeft, 302);
            btnExit.Size = new Size(buttonWidth, 42);
            StylePrimaryButton(btnExit, Color.FromArgb(100, 116, 139));

            Controls.Add(cardPanel);
            cardPanel.Controls.Add(lblTitle);
            cardPanel.Controls.Add(lblSubTitle);
            cardPanel.Controls.Add(label1);
            cardPanel.Controls.Add(txtLoginAccount);
            cardPanel.Controls.Add(label2);
            cardPanel.Controls.Add(txtPwd);
            cardPanel.Controls.Add(lblMsg);
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

        private void btnLogin_Click(object sender, EventArgs e)
        {
            // 防抖
            if (!GlobalDebounce.Check()) return;
            string loginAccount = txtLoginAccount.Text.Trim();
            string loginPwd = txtPwd.Text.Trim();

            // 非空校验
            if (string.IsNullOrEmpty(loginAccount))
            {
                MessageBox.Show("请输入管理员登录账号！", "输入提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLoginAccount.Focus();
                return;
            }
            if (string.IsNullOrEmpty(loginPwd))
            {
                MessageBox.Show("请输入登录密码！", "输入提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPwd.Focus();
                return;
            }

            // 登录校验
            Users loginUser = bllUser.UserLogin(loginAccount, loginPwd, out string msg);
            if (loginUser == null)
            {
                MessageBox.Show(msg, "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 权限校验
            if (loginUser.user_type != 3)
            {
                MessageBox.Show("您不是管理员账号，无权登录本系统！", "权限不足", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtLoginAccount.Clear();
                txtPwd.Clear();
                txtLoginAccount.Focus();
                return;
            }

            // ===================== 修复核心：先给全局变量赋值，再使用！=====================
            // 1. 先给全局登录用户赋值，保证后续所有逻辑都能正常访问
            Program.LoginUser = loginUser;

            // 2. 强制改密校验（直接用已赋值的全局变量，或直接用loginUser对象，双重保险）
            B_UserPwd bllPwd = new B_UserPwd();
            bllPwd.CheckNeedForceChangePwd(loginUser.user_id, out bool isForceChange, out string forceReason, out bool isFirstLogin);

            // 3. 强制改密逻辑
            if (isForceChange)
            {
                FrmChangePwd changePwdForm = new FrmChangePwd
                {
                    IsForceChangeMode = true,
                    IsFirstLogin = isFirstLogin,
                    ForceChangeTip = forceReason
                };
                // 改密失败/取消，直接终止登录流程
                if (changePwdForm.ShowDialog() != DialogResult.OK)
                {
                    // 取消改密，清空全局登录信息，避免残留
                    Program.LoginUser = null;
                    txtLoginAccount.Clear();
                    txtPwd.Clear();
                    txtLoginAccount.Focus();
                    return;
                }
            }

            // 4. 所有校验通过，弹出登录成功提示
            MessageBox.Show($"欢迎您，管理员【{loginUser.user_name}】！", "登录成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // 5. 打开主窗体，隐藏登录页
            FrmAdminMain mainForm = new FrmAdminMain();
            mainForm.Show();
            this.Hide(); // 只隐藏，不销毁，避免闪退
        }
        // 保存全局登录用户信息（整个管理员端都能访问）
        //Program.LoginUser = loginUser;

        //// 打开管理员主窗体，隐藏当前登录窗体
        //FrmAdminMain mainForm = new FrmAdminMain();
        //mainForm.Show();
        //this.Hide();
        // }
        // #endregion

        #region 【基础功能】退出按钮点击事件
        private void btnExit_Click(object sender, EventArgs e)
        {
            // 直接退出整个应用程序
            Application.Exit();
        }
        #endregion

        #region 【体验优化】快捷键功能
        // 账号输入框按回车，自动跳转到密码输入框
        private void txtLoginAccount_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtPwd.Focus();
            }
        }

        // 密码输入框按回车，直接触发登录按钮
        private void txtPwd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin.PerformClick(); // 模拟点击登录按钮
            }
        }

        // 窗体关闭时，直接退出整个程序
        private void FrmAdminLogin_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
        #endregion
    }
}
