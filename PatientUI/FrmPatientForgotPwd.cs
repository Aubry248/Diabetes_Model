using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using BLL;

namespace PatientUI
{
    public partial class FrmPatientForgotPwd : Form
    {
        private readonly B_User bllUser = new B_User();

        public FrmPatientForgotPwd()
        {
            InitializeComponent();
            this.Text = "忘记密码 - 重置";
            this.Size = new Size(620, 430);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Font = new Font("微软雅黑", 9.5F);
            this.BackColor = Color.FromArgb(241, 245, 249);
            ApplyVisualStyle();
        }

        private void ApplyVisualStyle()
        {
            Panel cardPanel = new Panel
            {
                Size = new Size(500, 360),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point((ClientSize.Width - 500) / 2, (ClientSize.Height - 360) / 2)
            };

            Label lblTitle = new Label
            {
                Text = "找回密码",
                Font = new Font("微软雅黑", 17F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(32, 22),
                Size = new Size(160, 34)
            };

            Label lblSubTitle = new Label
            {
                Text = "验证身份证与手机号后即可重置登录密码",
                Font = new Font("微软雅黑", 9.5F),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(32, 58),
                Size = new Size(300, 24)
            };

            StyleLabel(label2, "身份证号", 36, 110, 88);
            StyleLabel(label3, "手机号", 36, 162, 88);
            StyleLabel(label4, "新密码", 36, 214, 88);
            StyleLabel(label5, "确认密码", 36, 258, 88);

            StyleTextBox(txtIdCard, 138, 106, 300);
            StyleTextBox(txtIdPhone, 138, 158, 300);
            StyleTextBox(txtNewPwd, 138, 210, 300);
            StyleTextBox(txtConfirmPwd, 138, 254, 300);
            txtNewPwd.PasswordChar = '*';
            txtConfirmPwd.PasswordChar = '*';

            StyleButton(btnConfirm, Color.FromArgb(0, 122, 204), 230, 304, 100, 40, "确认重置");
            StyleButton(btnCancel, Color.FromArgb(100, 116, 139), 342, 304, 96, 40, "取消");

            Controls.Add(cardPanel);
            cardPanel.Controls.Add(lblTitle);
            cardPanel.Controls.Add(lblSubTitle);
            cardPanel.Controls.Add(label2);
            cardPanel.Controls.Add(label3);
            cardPanel.Controls.Add(label4);
            cardPanel.Controls.Add(label5);
            cardPanel.Controls.Add(txtIdCard);
            cardPanel.Controls.Add(txtIdPhone);
            cardPanel.Controls.Add(txtNewPwd);
            cardPanel.Controls.Add(txtConfirmPwd);
            cardPanel.Controls.Add(btnConfirm);
            cardPanel.Controls.Add(btnCancel);
        }

        private void StyleLabel(Label label, string text, int x, int y, int width)
        {
            label.Text = text;
            label.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            label.ForeColor = Color.FromArgb(51, 65, 85);
            label.Location = new Point(x, y);
            label.Size = new Size(width, 28);
        }

        private void StyleTextBox(TextBox textBox, int x, int y, int width)
        {
            textBox.Location = new Point(x, y);
            textBox.Size = new Size(width, 32);
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.Font = new Font("微软雅黑", 10.5F);
        }

        private void StyleButton(Button button, Color backColor, int x, int y, int width, int height, string text)
        {
            button.Text = text;
            button.Location = new Point(x, y);
            button.Size = new Size(width, height);
            button.BackColor = backColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            string idCard = txtIdCard.Text.Trim();   // 标签：身份证 → 对应 txtIdCard
            string phone = txtIdPhone.Text.Trim();    // 标签：手机号 → 对应 txtPhone
            string newPwd = txtNewPwd.Text.Trim();
            string confirmPwd = txtConfirmPwd.Text.Trim();

            // 手机号格式校验（对应下面的输入框）
            if (string.IsNullOrEmpty(phone) || !Regex.IsMatch(phone, @"^1[3-9]\d{9}$"))
            {
                MessageBox.Show("请输入正确的11位手机号！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdPhone.Focus(); // 焦点定位到手机号输入框
                return;
            }

            // 身份证号格式校验（对应上面的输入框）
            if (string.IsNullOrEmpty(idCard) || !Regex.IsMatch(idCard, @"^\d{17}[\dXx]$"))
            {
                MessageBox.Show("请输入正确的18位身份证号！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdCard.Focus(); // 焦点定位到身份证输入框
                return;
            }

            // 密码校验
            if (string.IsNullOrEmpty(newPwd) || newPwd.Length < 6)
            {
                MessageBox.Show("新密码长度不能少于6位！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPwd.Focus();
                return;
            }
            if (newPwd != confirmPwd)
            {
                MessageBox.Show("两次输入的新密码不一致！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPwd.Focus();
                return;
            }

            // 调用业务层（参数顺序不变：手机号、身份证号、新密码）
            string result = bllUser.ForgotPassword(phone, idCard, newPwd);
            if (result == "ok")
            {
                MessageBox.Show("密码重置成功！请使用新密码登录", "重置成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show(result, "重置失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}