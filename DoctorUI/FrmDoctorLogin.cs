using System;
using System.Drawing;
using System.Windows.Forms;
using BLL;
using Model;

namespace DoctorUI
{
    public partial class FrmDoctorLogin : Form
    {
        // 复用BLL层实例（和患者端一致）
        private readonly B_User bllUser = new B_User();

        // 【可调节参数】
        private readonly int _formWidth = 720;
        private readonly int _formHeight = 430;
        private readonly int _controlTopMargin = 40;
        private readonly int _inputWidth = 300;
        private readonly int _inputHeight = 32;
        private readonly int _btnWidth = 120;
        private readonly int _btnHeight = 42;
        private readonly int _controlSpace = 20;
        private readonly Color _themeColor = Color.FromArgb(0, 122, 204);

        // 新增：错误提示标签（和患者端lblMsg一致）
        private Label _lblMsg;
        private TextBox _txtLoginAccount;
        private TextBox _txtPassword;
        private Button _btnLogin;
        private Button _btnExit;

        public FrmDoctorLogin()
        {
            InitializeComponent();

            InitLoginForm();
            BuildLayout();
            BindEvent();
        }

        private void InitLoginForm()
        {
            this.Text = "公卫医生端 - 登录";
            this.ClientSize = new Size(_formWidth, _formHeight);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("微软雅黑", 9.5F);
            this.BackColor = Color.FromArgb(241, 245, 249);
            this.MaximizeBox = false;
            this.MinimizeBox = false; // 和患者端一致，禁止最小化
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.AutoScaleMode = AutoScaleMode.Font;
            this.DoubleBuffered = true;
            // 新增：窗体关闭时彻底退出（和患者端一致）
            this.FormClosed += FrmDoctorLogin_FormClosed;
        }

        private void BuildLayout()
        {
            this.Controls.Clear();
            this.SuspendLayout();
            int buttonGap = 10;
            int firstButtonLeft = (500 - _btnWidth * 3 - buttonGap * 2) / 2;
            int secondColumnLeft = firstButtonLeft + _btnWidth + buttonGap;
            int thirdColumnLeft = secondColumnLeft + _btnWidth + buttonGap;
            Panel cardPanel = new Panel
            {
                Size = new Size(500, 360),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point((ClientSize.Width - 500) / 2, (ClientSize.Height - 360) / 2)
            };

            // 1. 登录标题
            Label lblTitle = new Label
            {
                Text = "公卫医生登录",
                Font = new Font("微软雅黑", 18F, FontStyle.Bold),
                ForeColor = _themeColor,
                Location = new Point(36, 28),
                Size = new Size(220, 36)
            };
            Label lblSubTitle = new Label
            {
                Text = "登录后可查看患者档案、随访与干预管理信息",
                Font = new Font("微软雅黑", 9.5F),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(36, 66),
                Size = new Size(360, 24)
            };

            // 2. 账号标签+输入框
            Label lblAccount = new Label
            {
                Text = "医生账号：",
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(40, 124),
                Size = new Size(92, 28)
            };
            _txtLoginAccount = new TextBox
            {
                Width = _inputWidth,
                Height = _inputHeight,
                Location = new Point(134, 122),
                Font = new Font("微软雅黑", 10.5F),
                BorderStyle = BorderStyle.FixedSingle
            };

            // 3. 密码标签+输入框
            Label lblPwd = new Label
            {
                Text = "登录密码：",
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(40, 184),
                Size = new Size(92, 28)
            };
            _txtPassword = new TextBox
            {
                Width = _inputWidth,
                Height = _inputHeight,
                Location = new Point(134, 182),
                Font = new Font("微软雅黑", 10.5F),
                PasswordChar = '●',
                BorderStyle = BorderStyle.FixedSingle
            };

            // 新增：错误提示标签（和患者端lblMsg一致）
            _lblMsg = new Label
            {
                Text = "",
                ForeColor = Color.FromArgb(220, 38, 38),
                AutoSize = false,
                Location = new Point(40, 228),
                Size = new Size(394, 42),
                Font = new Font("微软雅黑", 9F)
            };

            Button btnForgotPwd = new Button
            {
                Text = "忘记密码",
                Width = _btnWidth,
                Height = _btnHeight,
                Location = new Point(firstButtonLeft, 292)
            };
            StyleGhostButton(btnForgotPwd, Color.FromArgb(245, 158, 11));
            btnForgotPwd.Click += LblForgetPwd_Click;

            // 4. 登录按钮
            _btnLogin = new Button
            {
                Text = "登录",
                Width = _btnWidth,
                Height = _btnHeight,
                Location = new Point(secondColumnLeft, 292)
            };
            StylePrimaryButton(_btnLogin, _themeColor);

            // 5. 退出按钮
            _btnExit = new Button
            {
                Text = "退出",
                Width = _btnWidth,
                Height = _btnHeight,
                Location = new Point(thirdColumnLeft, 292)
            };
            StylePrimaryButton(_btnExit, Color.FromArgb(100, 116, 139));
            this.AcceptButton = _btnLogin;
            this.CancelButton = _btnExit;

            // 添加到窗体控件集合（放在ResumeLayout之前，和其他控件Add逻辑保持一致）
            this.Controls.Add(cardPanel);
            // 添加控件到窗体（包含错误提示标签）
            cardPanel.Controls.Add(lblTitle);
            cardPanel.Controls.Add(lblSubTitle);
            cardPanel.Controls.Add(lblAccount);
            cardPanel.Controls.Add(_txtLoginAccount);
            cardPanel.Controls.Add(lblPwd);
            cardPanel.Controls.Add(_txtPassword);
            cardPanel.Controls.Add(_lblMsg); // 新增错误提示
            cardPanel.Controls.Add(btnForgotPwd);
            cardPanel.Controls.Add(_btnLogin);
            cardPanel.Controls.Add(_btnExit);

            this.ResumeLayout(true);
        }

        private void StylePrimaryButton(Button button, Color backColor)
        {
            button.BackColor = backColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
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

        private void BindEvent()
        {
            // 登录按钮点击（核心逻辑对齐患者端）
            _btnLogin.Click += BtnLogin_Click;
            // 退出按钮点击（和患者端一致，彻底退出）
            _btnExit.Click += (s, e) => Application.Exit();
            // 回车快捷键（和患者端一致）
            _txtLoginAccount.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) _txtPassword.Focus(); };
            _txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) _btnLogin.PerformClick(); };
        }
        /// <summary>
        /// 忘记密码链接点击事件
        /// </summary>
        private void LblForgetPwd_Click(object sender, EventArgs e)
        {
            // 弹出医生端专属密码重置窗体，模态弹窗避免操作混乱
            FrmDoctorForgetPwd frmForgetPwd = new FrmDoctorForgetPwd();
            frmForgetPwd.ShowDialog();
            // 重置完成后，自动聚焦到账号输入框，优化登录体验
            _txtLoginAccount.Focus();
            _txtLoginAccount.Clear();
            _txtPassword.Clear();
        }
        // 核心登录逻辑（完全对齐患者端，补充所有缺失细节）
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            // 1. 基础输入校验（和患者端一致）
            string loginAccount = _txtLoginAccount.Text.Trim();
            string password = _txtPassword.Text.Trim();
            _lblMsg.Text = ""; // 清空错误提示

            if (string.IsNullOrEmpty(loginAccount))
            {
                MessageBox.Show("请输入医生账号！", "输入提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtLoginAccount.Focus();
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("请输入登录密码！", "输入提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtPassword.Focus();
                return;
            }

            // 2. 调用BLL层登录方法（和患者端逻辑完全一致）
            string msg;
            Users loginUser = bllUser.UserLogin(loginAccount, password, out msg);
            _lblMsg.Text = msg; // 显示BLL返回的具体错误（关键：定位失败原因）

            // 3. 处理登录结果（对齐患者端）
            if (loginUser != null)
            {
                // 权限校验：仅公卫医生（user_type=2）可登录（和患者端user_type=1对应）
                if (loginUser.user_type != 2)
                {
                    MessageBox.Show("您不是公卫医生账号，无法登录医生端！", "权限错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtLoginAccount.Clear();
                    _txtPassword.Clear();
                    _txtLoginAccount.Focus();
                    return;
                }

                // 登录成功：保存全局用户信息（和患者端一致）
                Program.LoginUser = loginUser;
                GlobalData.CurrentLoginUserId = Program.LoginUser.user_id;
                GlobalData.CurrentLoginUserName = Program.LoginUser.user_name;
                MessageBox.Show(string.Format("欢迎您，{0}！", loginUser.user_name), "登录成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                GlobalConfig.CurrentDoctorID = loginUser.user_id; // 取真实医生ID（t_user表主键）
                GlobalConfig.CurrentDoctorName = loginUser.user_name; // 取真实医生姓名
                // 打开医生端主窗体（和患者端FrmPatientMain对应）
                FrmDoctorMain mainForm = new FrmDoctorMain();
                mainForm.FormClosed += (s, args) => this.Close();
                mainForm.Show();
                this.Hide();
            }
            else
            {
                // 登录失败：清空密码框，聚焦（和患者端一致）
                _txtPassword.Clear();
                _txtPassword.Focus();
            }
        }

        // 窗体关闭事件（和患者端一致，彻底退出程序）
        private void FrmDoctorLogin_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        // 设计器方法（完善）
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FrmDoctorLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(480, 282);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "FrmDoctorLogin";
            this.Text = "公卫医生端-登录";
            this.Load += new System.EventHandler(this.FrmDoctorLogin_Load);
            this.ResumeLayout(false);

        }

        private void FrmDoctorLogin_Load(object sender, EventArgs e)
        {

        }
    }
}