using BLL;
using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace AdminUI
{
    public partial class FrmChangePwd : Form
    {
        #region 全局对象与控件声明
        private readonly B_UserPwd _bllPwd = new B_UserPwd();
        private readonly Color _themePrimary = Color.FromArgb(0, 122, 204);
        private readonly Color _themeDanger = Color.FromArgb(220, 53, 69);
        private readonly Color _themeGray = Color.FromArgb(108, 117, 125);
        private readonly Color _themeSuccess = Color.FromArgb(40, 160, 40);
        private readonly Color _themeWarning = Color.FromArgb(255, 193, 7);

        public bool IsForceChangeMode { get; set; } = false;
        public string ForceChangeTip { get; set; } = "";
        public bool IsFirstLogin { get; set; } = false;

        private Label lblLoginAccount;
        private Label lblForceTip;
        private TextBox txtOriginalPwd, txtNewPwd, txtConfirmPwd;
        private CheckBox chkShowPwd;
        private Label lblPwdPolicyTip, lblPwdStrengthTip;
        private Button btnSave, btnReset;
        #endregion

        #region 窗体初始化
        public FrmChangePwd()
        {
            this.Text = "修改密码";
            this.Size = new Size(520, 420);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Font = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this.BackColor = Color.White;
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.DoubleBuffered = true;

            CreateAllControls();
            BindAllEvents();
        }
        #endregion

        #region 页面布局创建
        private void CreateAllControls()
        {
            this.SuspendLayout();
            int margin = 20;
            int formWidth = this.ClientSize.Width;
            int labelWidth = 100;
            int inputWidth = 320;
            int lineHeight = 60;
            int topOffset = margin;

            // 顶部提示区
            lblLoginAccount = new Label
            {
                Text = $"当前登录账号：{Program.LoginUser?.login_account ?? "未知账号"}",
                Location = new Point(margin, topOffset),
                Size = new Size(formWidth - 2 * margin, 25),
                Font = new Font("微软雅黑", 9F, FontStyle.Bold),
                ForeColor = _themeGray
            };
            topOffset += 30;

            lblForceTip = new Label
            {
                Text = ForceChangeTip,
                Location = new Point(margin, topOffset),
                Size = new Size(formWidth - 2 * margin, 25),
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                ForeColor = _themeDanger,
                Visible = IsForceChangeMode
            };
            topOffset += 30;

            // 核心输入区分组框
            GroupBox gbInput = new GroupBox
            {
                Text = "密码修改",
                Location = new Point(margin, topOffset),
                Size = new Size(formWidth - 2 * margin, 220),
                Font = new Font("微软雅黑", 9F, FontStyle.Bold)
            };

            // 原密码输入
            Label lblOriginalPwd = new Label
            {
                Text = "原密码：",
                Location = new Point(20, 30),
                Size = new Size(labelWidth, 25),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("微软雅黑", 9F, FontStyle.Regular)
            };
            txtOriginalPwd = new TextBox
            {
                Location = new Point(20 + labelWidth, 28),
                Size = new Size(inputWidth, 25),
                PasswordChar = '*',
                Font = new Font("微软雅黑", 9F)
            };

            // 新密码输入
            Label lblNewPwd = new Label
            {
                Text = "新密码：",
                Location = new Point(20, 30 + lineHeight),
                Size = new Size(labelWidth, 25),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("微软雅黑", 9F, FontStyle.Regular)
            };
            txtNewPwd = new TextBox
            {
                Location = new Point(20 + labelWidth, 28 + lineHeight),
                Size = new Size(inputWidth, 25),
                PasswordChar = '*',
                Font = new Font("微软雅黑", 9F)
            };
            lblPwdStrengthTip = new Label
            {
                Text = "密码强度：未输入",
                Location = new Point(20 + labelWidth, 55 + lineHeight),
                Size = new Size(inputWidth / 2, 20),
                Font = new Font("微软雅黑", 8F),
                ForeColor = _themeGray
            };
            lblPwdPolicyTip = new Label
            {
                Text = "",
                Location = new Point(20 + labelWidth + inputWidth / 2, 55 + lineHeight),
                Size = new Size(inputWidth / 2, 20),
                Font = new Font("微软雅黑", 8F),
                ForeColor = _themeDanger,
                TextAlign = ContentAlignment.MiddleRight
            };

            // 确认新密码输入
            Label lblConfirmPwd = new Label
            {
                Text = "确认新密码：",
                Location = new Point(20, 30 + lineHeight * 2),
                Size = new Size(labelWidth, 25),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("微软雅黑", 9F, FontStyle.Regular)
            };
            txtConfirmPwd = new TextBox
            {
                Location = new Point(20 + labelWidth, 28 + lineHeight * 2),
                Size = new Size(inputWidth, 25),
                PasswordChar = '*',
                Font = new Font("微软雅黑", 9F)
            };

            // 显示密码复选框
            chkShowPwd = new CheckBox
            {
                Text = "显示密码明文",
                Location = new Point(20 + labelWidth, 55 + lineHeight * 2),
                AutoSize = true,
                Font = new Font("微软雅黑", 8F),
                ForeColor = _themeGray
            };

            gbInput.Controls.AddRange(new Control[] {
                lblOriginalPwd, txtOriginalPwd,
                lblNewPwd, txtNewPwd, lblPwdStrengthTip, lblPwdPolicyTip,
                lblConfirmPwd, txtConfirmPwd, chkShowPwd
            });
            this.Controls.Add(gbInput);
            topOffset += 230;

            // 底部操作区
            btnSave = new Button
            {
                Text = "保存修改",
                Location = new Point(formWidth - margin - 220, topOffset),
                Size = new Size(100, 40),
                BackColor = _themePrimary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 10F, FontStyle.Bold)
            };
            btnReset = new Button
            {
                Text = "重置",
                Location = new Point(formWidth - margin - 110, topOffset),
                Size = new Size(100, 40),
                BackColor = _themeGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 10F)
            };

            this.Controls.AddRange(new Control[] { lblLoginAccount, lblForceTip, btnSave, btnReset });
            this.ResumeLayout(true);
        }
        #endregion

        #region 事件绑定
        private void BindAllEvents()
        {
            this.Load += FrmChangePwd_Load;
            chkShowPwd.CheckedChanged += ChkShowPwd_CheckedChanged;
            txtNewPwd.TextChanged += TxtNewPwd_TextChanged;
            txtConfirmPwd.TextChanged += TxtConfirmPwd_TextChanged;
            btnSave.Click += BtnSave_Click;
            btnReset.Click += BtnReset_Click;
            this.FormClosing += FrmChangePwd_FormClosing;
        }
        #endregion

        #region 辅助方法：获取本机IP地址
        private string GetLocalIP()
        {
            try
            {
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                return "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }
        #endregion

        #region 窗体生命周期事件
        private void FrmChangePwd_Load(object sender, EventArgs e)
        {
            if (Program.LoginUser == null)
            {
                MessageBox.Show("登录信息失效，请重新登录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
            }

            lblLoginAccount.Text = $"当前登录账号：{Program.LoginUser.login_account}";

            if (IsForceChangeMode)
            {
                lblForceTip.Text = ForceChangeTip;
                lblForceTip.Visible = true;
                this.ControlBox = false;
                btnReset.Visible = false;
                this.Text = "强制修改密码";
            }
        }

        private void FrmChangePwd_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsForceChangeMode && this.DialogResult != DialogResult.OK)
            {
                e.Cancel = true;
                MessageBox.Show("您必须完成密码修改才能进入系统！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 实时校验事件处理
        private void ChkShowPwd_CheckedChanged(object sender, EventArgs e)
        {
            char pwdChar = chkShowPwd.Checked ? '\0' : '*';
            txtOriginalPwd.PasswordChar = pwdChar;
            txtNewPwd.PasswordChar = pwdChar;
            txtConfirmPwd.PasswordChar = pwdChar;
        }

        private void TxtNewPwd_TextChanged(object sender, EventArgs e)
        {
            string newPwd = txtNewPwd.Text.Trim();
            if (string.IsNullOrWhiteSpace(newPwd))
            {
                lblPwdStrengthTip.Text = "密码强度：未输入";
                lblPwdStrengthTip.ForeColor = _themeGray;
                lblPwdPolicyTip.Text = "";
                return;
            }

            // 策略合规性校验
            bool isPolicyValid = _bllPwd.CheckNewPasswordPolicy(newPwd, out string policyError);
            if (!isPolicyValid)
            {
                lblPwdPolicyTip.Text = policyError;
                lblPwdPolicyTip.ForeColor = _themeDanger;
            }
            else
            {
                lblPwdPolicyTip.Text = "密码符合策略要求";
                lblPwdPolicyTip.ForeColor = _themeSuccess;
            }

            // 密码强度计算
            int strengthScore = 0;
            if (newPwd.Length >= 8) strengthScore++;
            if (newPwd.Length >= 12) strengthScore++;
            if (newPwd.Length >= 16) strengthScore++;

            bool hasUpper = false;
            bool hasLower = false;
            foreach (char c in newPwd)
            {
                if (char.IsUpper(c)) hasUpper = true;
                if (char.IsLower(c)) hasLower = true;
            }
            if (hasUpper && hasLower) strengthScore++;

            bool hasNumber = false;
            foreach (char c in newPwd)
            {
                if (char.IsDigit(c))
                {
                    hasNumber = true;
                    break;
                }
            }
            if (hasNumber) strengthScore++;

            string specialChars = "~!@#$%^&*()_+-=[]{}|;':\",./<>?";
            bool hasSpecial = false;
            foreach (char c in newPwd)
            {
                if (specialChars.Contains(c.ToString()))
                {
                    hasSpecial = true;
                    break;
                }
            }
            if (hasSpecial) strengthScore++;

            // 强度提示（兼容C#7.3）
            if (strengthScore <= 2)
            {
                lblPwdStrengthTip.Text = "密码强度：弱";
                lblPwdStrengthTip.ForeColor = _themeDanger;
            }
            else if (strengthScore <= 4)
            {
                lblPwdStrengthTip.Text = "密码强度：中";
                lblPwdStrengthTip.ForeColor = _themeWarning;
            }
            else
            {
                lblPwdStrengthTip.Text = "密码强度：强";
                lblPwdStrengthTip.ForeColor = _themeSuccess;
            }
        }

        private void TxtConfirmPwd_TextChanged(object sender, EventArgs e)
        {
            string newPwd = txtNewPwd.Text.Trim();
            string confirmPwd = txtConfirmPwd.Text.Trim();

            if (!string.IsNullOrWhiteSpace(confirmPwd) && newPwd != confirmPwd)
            {
                lblPwdPolicyTip.Text = "两次密码输入不一致！";
                lblPwdPolicyTip.ForeColor = _themeDanger;
            }
            else if (!string.IsNullOrWhiteSpace(confirmPwd) && newPwd == confirmPwd)
            {
                lblPwdPolicyTip.Text = "两次密码输入一致";
                lblPwdPolicyTip.ForeColor = _themeSuccess;
            }
        }
        #endregion

        #region 按钮点击事件处理
        private void BtnReset_Click(object sender, EventArgs e)
        {
            txtOriginalPwd.Text = "";
            txtNewPwd.Text = "";
            txtConfirmPwd.Text = "";
            chkShowPwd.Checked = false;
            lblPwdPolicyTip.Text = "";
            lblPwdStrengthTip.Text = "密码强度：未输入";
            txtOriginalPwd.Focus();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (Program.LoginUser == null)
            {
                MessageBox.Show("登录信息失效，请重新登录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
            }

            int userId = Program.LoginUser.user_id;
            string originalPwd = txtOriginalPwd.Text.Trim();
            string newPwd = txtNewPwd.Text.Trim();
            string confirmPwd = txtConfirmPwd.Text.Trim();
            string clientIP = GetLocalIP();

            // 非空校验
            if (string.IsNullOrWhiteSpace(originalPwd))
            {
                MessageBox.Show("请输入原密码！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOriginalPwd.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(newPwd))
            {
                MessageBox.Show("请输入新密码！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPwd.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(confirmPwd))
            {
                MessageBox.Show("请确认新密码！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPwd.Focus();
                return;
            }

            // 两次密码一致性校验
            if (newPwd != confirmPwd)
            {
                MessageBox.Show("两次密码输入不一致，请重新输入！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPwd.Focus();
                return;
            }

            // 新密码不能和原密码一致
            if (originalPwd == newPwd)
            {
                MessageBox.Show("新密码不能和原密码一致！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPwd.Focus();
                return;
            }

            // 原密码校验
            this.Cursor = Cursors.WaitCursor;
            bool originalPwdValid = _bllPwd.CheckOriginalPassword(userId, originalPwd, out string originalError);
            this.Cursor = Cursors.Default;

            if (!originalPwdValid)
            {
                MessageBox.Show(originalError, "校验失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtOriginalPwd.Text = "";
                txtOriginalPwd.Focus();
                return;
            }

            // 新密码策略校验
            bool newPwdValid = _bllPwd.CheckNewPasswordPolicy(newPwd, out string policyError);
            if (!newPwdValid)
            {
                MessageBox.Show(policyError, "密码不合规", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPwd.Focus();
                return;
            }

            // 执行密码修改
            this.Cursor = Cursors.WaitCursor;
            bool changeSuccess = _bllPwd.ChangePassword(userId, newPwd, IsFirstLogin, clientIP, out string changeError);
            this.Cursor = Cursors.Default;

            if (changeSuccess)
            {
                MessageBox.Show("密码修改成功！程序将重启，请使用新密码重新登录。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 1. 清空登录信息
                Program.LoginUser = null;
                this.DialogResult = DialogResult.OK;

                // 2. ✅ 核心：重启整个应用（自动关闭所有窗体，重新打开登录界面）
                Application.Restart();

                // 3. 关闭当前进程，防止残留
                Environment.Exit(0);
            }
            else
            {
                MessageBox.Show(changeError, "修改失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            #endregion
        }
    }
}