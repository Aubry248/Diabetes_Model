using BLL;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PatientUI
{
    public class FrmPatientChangePwd : Form
    {
        private readonly B_UserPwd _bllPwd = new B_UserPwd();
        private readonly Color _themePrimary = Color.FromArgb(0, 122, 204);
        private readonly Color _themeDanger = Color.FromArgb(220, 53, 69);
        private readonly Color _themeGray = Color.FromArgb(108, 117, 125);
        private readonly Color _themeBg = Color.FromArgb(245, 247, 250);

        private Panel _cardPanel;
        private Label _lblStrength;
        private Label _lblPolicy;
        private TextBox _txtOriginalPwd;
        private TextBox _txtNewPwd;
        private TextBox _txtConfirmPwd;
        private CheckBox _chkShowPwd;
        private Button _btnSave;
        private Button _btnCancel;

        public FrmPatientChangePwd()
        {
            Text = "修改密码";
            Size = new Size(540, 430);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = _themeBg;
            Font = new Font("微软雅黑", 9F);
            AutoScaleMode = AutoScaleMode.Dpi;

            BuildLayout();
            BindEvents();
        }

        private void BuildLayout()
        {
            SuspendLayout();

            _cardPanel = new Panel
            {
                BackColor = Color.White,
                Size = new Size(460, 330),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblTitle = new Label
            {
                Text = "患者密码修改",
                Font = new Font("微软雅黑", 15F, FontStyle.Bold),
                ForeColor = _themePrimary,
                AutoSize = false,
                Location = new Point(24, 20),
                Size = new Size(220, 32)
            };

            Label lblTip = new Label
            {
                Text = "请输入原密码并设置新的登录密码",
                Font = new Font("微软雅黑", 9F),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = false,
                Location = new Point(24, 56),
                Size = new Size(260, 24)
            };

            Label lblOriginal = CreateLabel("原密码", 24, 98);
            Label lblNew = CreateLabel("新密码", 24, 152);
            Label lblConfirm = CreateLabel("确认新密码", 24, 222);

            _txtOriginalPwd = CreateTextBox(136, 94);
            _txtNewPwd = CreateTextBox(136, 148);
            _txtConfirmPwd = CreateTextBox(136, 218);
            _txtOriginalPwd.PasswordChar = '*';
            _txtNewPwd.PasswordChar = '*';
            _txtConfirmPwd.PasswordChar = '*';

            _lblStrength = new Label
            {
                Text = "密码强度：未输入",
                Font = new Font("微软雅黑", 8.5F),
                ForeColor = _themeGray,
                AutoSize = false,
                Location = new Point(136, 184),
                Size = new Size(150, 20)
            };

            _lblPolicy = new Label
            {
                Text = "",
                Font = new Font("微软雅黑", 8.5F),
                ForeColor = _themeDanger,
                AutoSize = false,
                Location = new Point(290, 184),
                Size = new Size(130, 20),
                TextAlign = ContentAlignment.MiddleRight
            };

            _chkShowPwd = new CheckBox
            {
                Text = "显示密码",
                AutoSize = true,
                Location = new Point(136, 262),
                ForeColor = Color.FromArgb(71, 85, 105)
            };

            _btnSave = CreateButton("保存修改", _themePrimary, 214, 284);
            _btnCancel = CreateButton("取消", _themeGray, 330, 284);

            _cardPanel.Controls.AddRange(new Control[]
            {
                lblTitle, lblTip,
                lblOriginal, _txtOriginalPwd,
                lblNew, _txtNewPwd, _lblStrength, _lblPolicy,
                lblConfirm, _txtConfirmPwd,
                _chkShowPwd, _btnSave, _btnCancel
            });

            Controls.Add(_cardPanel);
            CenterCard();
            ResumeLayout(false);
        }

        private void BindEvents()
        {
            Resize += (s, e) => CenterCard();
            _chkShowPwd.CheckedChanged += (s, e) =>
            {
                char pwdChar = _chkShowPwd.Checked ? '\0' : '*';
                _txtOriginalPwd.PasswordChar = pwdChar;
                _txtNewPwd.PasswordChar = pwdChar;
                _txtConfirmPwd.PasswordChar = pwdChar;
            };
            _txtNewPwd.TextChanged += (s, e) => UpdatePasswordHints();
            _btnSave.Click += BtnSave_Click;
            _btnCancel.Click += (s, e) => Close();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (Program.LoginUser == null)
            {
                MessageBox.Show("登录状态已失效，请重新登录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            string originalPwd = _txtOriginalPwd.Text.Trim();
            string newPwd = _txtNewPwd.Text.Trim();
            string confirmPwd = _txtConfirmPwd.Text.Trim();

            if (string.IsNullOrWhiteSpace(originalPwd))
            {
                MessageBox.Show("请输入原密码。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtOriginalPwd.Focus();
                return;
            }

            if (!_bllPwd.CheckOriginalPassword(Program.LoginUser.user_id, originalPwd, out string originalError))
            {
                MessageBox.Show(originalError, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtOriginalPwd.Focus();
                return;
            }

            if (!_bllPwd.CheckNewPasswordPolicy(newPwd, out string policyError))
            {
                MessageBox.Show(policyError, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtNewPwd.Focus();
                return;
            }

            if (newPwd != confirmPwd)
            {
                MessageBox.Show("两次输入的新密码不一致。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtConfirmPwd.Focus();
                return;
            }

            if (string.Equals(originalPwd, newPwd, StringComparison.Ordinal))
            {
                MessageBox.Show("新密码不能与原密码相同。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtNewPwd.Focus();
                return;
            }

            Cursor = Cursors.WaitCursor;
            bool success = _bllPwd.ChangePassword(Program.LoginUser.user_id, newPwd, false, "127.0.0.1", out string changeError);
            Cursor = Cursors.Default;

            if (!success)
            {
                MessageBox.Show(changeError, "修改失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("密码修改成功，请使用新密码继续登录。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }

        private void UpdatePasswordHints()
        {
            string pwd = _txtNewPwd.Text.Trim();
            _lblStrength.Text = $"密码强度：{GetStrengthText(pwd)}";
            _lblStrength.ForeColor = GetStrengthColor(pwd);

            if (string.IsNullOrWhiteSpace(pwd))
            {
                _lblPolicy.Text = "";
                return;
            }

            _lblPolicy.Text = _bllPwd.CheckNewPasswordPolicy(pwd, out string errorMsg) ? "符合策略" : errorMsg;
            _lblPolicy.ForeColor = _bllPwd.CheckNewPasswordPolicy(pwd, out _) ? Color.FromArgb(22, 163, 74) : _themeDanger;
        }

        private string GetStrengthText(string pwd)
        {
            if (string.IsNullOrWhiteSpace(pwd))
                return "未输入";

            int score = 0;
            if (pwd.Length >= 6) score++;
            if (pwd.Length >= 10) score++;
            if (HasLower(pwd) && HasUpper(pwd)) score++;
            if (HasDigit(pwd)) score++;
            if (HasSpecial(pwd)) score++;

            if (score <= 2) return "弱";
            if (score <= 4) return "中";
            return "强";
        }

        private Color GetStrengthColor(string pwd)
        {
            string strength = GetStrengthText(pwd);
            if (strength == "强") return Color.FromArgb(22, 163, 74);
            if (strength == "中") return Color.FromArgb(234, 179, 8);
            if (strength == "弱") return _themeDanger;
            return _themeGray;
        }

        private bool HasLower(string pwd)
        {
            foreach (char c in pwd)
                if (char.IsLower(c))
                    return true;
            return false;
        }

        private bool HasUpper(string pwd)
        {
            foreach (char c in pwd)
                if (char.IsUpper(c))
                    return true;
            return false;
        }

        private bool HasDigit(string pwd)
        {
            foreach (char c in pwd)
                if (char.IsDigit(c))
                    return true;
            return false;
        }

        private bool HasSpecial(string pwd)
        {
            const string specialChars = "~!@#$%^&*()_+-=[]{}|;':\",./<>?";
            foreach (char c in pwd)
                if (specialChars.Contains(c.ToString()))
                    return true;
            return false;
        }

        private Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y + 3),
                Size = new Size(96, 24),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("微软雅黑", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85)
            };
        }

        private TextBox CreateTextBox(int x, int y)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(284, 30),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("微软雅黑", 10F)
            };
        }

        private Button CreateButton(string text, Color backColor, int x, int y)
        {
            Button button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(96, 38),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private void CenterCard()
        {
            if (_cardPanel == null)
                return;

            _cardPanel.Left = Math.Max((ClientSize.Width - _cardPanel.Width) / 2, 16);
            _cardPanel.Top = Math.Max((ClientSize.Height - _cardPanel.Height) / 2, 16);
        }
    }
}
