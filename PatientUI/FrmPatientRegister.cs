using System;
using System.Drawing;
using System.Windows.Forms;
using BLL;
using Model;

namespace PatientUI
{
    public partial class FrmPatientRegister : Form
    {

        private readonly B_User bllUser = new B_User();

        public FrmPatientRegister()
        {
            InitializeComponent();

            this.Text = "患者账号注册";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Font = new Font("微软雅黑", 9.5F);
            this.BackColor = Color.FromArgb(241, 245, 249);
            this.ClientSize = new Size(980, 640);

            ApplyVisualStyle();

            if (cboGender.Items.Count == 0)
            {
                cboGender.Items.AddRange(new object[] { "男", "女" });
                cboGender.SelectedIndex = 0;
            }
            if (cboDiabetesType.Items.Count == 0)
            {
                cboDiabetesType.Items.AddRange(new object[] { "无", "1型", "2型", "妊娠", "其他" });
                cboDiabetesType.SelectedIndex = 0;
            }

        }

        private void ApplyVisualStyle()
        {
            Panel cardPanel = new Panel
            {
                Size = new Size(820, 500),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point((ClientSize.Width - 820) / 2, (ClientSize.Height - 500) / 2)
            };

            Label lblTitle = new Label
            {
                Text = "患者注册",
                Font = new Font("微软雅黑", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(34, 24),
                Size = new Size(180, 36)
            };

            Label lblSubTitle = new Label
            {
                Text = "填写基础资料并创建患者账号",
                Font = new Font("微软雅黑", 9.5F),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(34, 62),
                Size = new Size(280, 24)
            };

            StyleLabel(label1, "姓名", 44, 118, 80);
            StyleLabel(label2, "身份证号", 44, 176, 80);
            StyleLabel(label3, "手机号", 44, 234, 80);
            StyleLabel(label4, "密码", 44, 292, 80);
            StyleLabel(label5, "确认密码", 44, 350, 80);
            StyleLabel(label6, "性别", 430, 118, 88);
            StyleLabel(label7, "糖尿病类型", 430, 176, 88);
            StyleLabel(label8, "出生日期", 430, 234, 88);

            StyleTextBox(txtName, 140, 114, 230);
            StyleTextBox(txtIdCard, 140, 172, 230);
            StyleTextBox(txtPhone, 140, 230, 230);
            StyleTextBox(txtPwd, 140, 288, 230);
            StyleTextBox(txtConfirmPwd, 140, 346, 230);
            txtPwd.PasswordChar = '*';
            txtConfirmPwd.PasswordChar = '*';

            StyleComboBox(cboGender, 534, 114, 200);
            StyleComboBox(cboDiabetesType, 534, 172, 200);

            dtpBirthDate.Location = new Point(534, 230);
            dtpBirthDate.Size = new Size(232, 32);
            dtpBirthDate.Font = new Font("微软雅黑", 10F);

            StylePrimaryButton(btnRegister, Color.FromArgb(0, 122, 204), 540, 420, 110, 42, "确认注册");
            StylePrimaryButton(btnCancel, Color.FromArgb(100, 116, 139), 662, 420, 96, 42, "取消");

            Controls.Add(cardPanel);
            cardPanel.Controls.Add(lblTitle);
            cardPanel.Controls.Add(lblSubTitle);
            cardPanel.Controls.Add(label1);
            cardPanel.Controls.Add(label2);
            cardPanel.Controls.Add(label3);
            cardPanel.Controls.Add(label4);
            cardPanel.Controls.Add(label5);
            cardPanel.Controls.Add(label6);
            cardPanel.Controls.Add(label7);
            cardPanel.Controls.Add(label8);
            cardPanel.Controls.Add(txtName);
            cardPanel.Controls.Add(txtIdCard);
            cardPanel.Controls.Add(txtPhone);
            cardPanel.Controls.Add(txtPwd);
            cardPanel.Controls.Add(txtConfirmPwd);
            cardPanel.Controls.Add(cboGender);
            cardPanel.Controls.Add(cboDiabetesType);
            cardPanel.Controls.Add(dtpBirthDate);
            cardPanel.Controls.Add(btnRegister);
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

        private void StyleComboBox(ComboBox comboBox, int x, int y, int width)
        {
            comboBox.Location = new Point(x, y);
            comboBox.Size = new Size(width, 32);
            comboBox.Font = new Font("微软雅黑", 10F);
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void StylePrimaryButton(Button button, Color backColor, int x, int y, int width, int height, string text)
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

        private void btnRegister_Click(object sender, EventArgs e)
        {
            // 1. 基础输入校验
            if (string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                MessageBox.Show("请输入真实姓名！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtIdCard.Text.Trim()) || txtIdCard.Text.Trim().Length != 18)
            {
                MessageBox.Show("请输入正确的18位身份证号！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdCard.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtPhone.Text.Trim()) || txtPhone.Text.Trim().Length != 11)
            {
                MessageBox.Show("请输入正确的11位手机号！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtPwd.Text.Trim()) || txtPwd.Text.Trim().Length < 6)
            {
                MessageBox.Show("密码长度不能少于6位！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPwd.Focus();
                return;
            }
            if (txtPwd.Text.Trim() != txtConfirmPwd.Text.Trim())
            {
                MessageBox.Show("两次输入的密码不一致！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPwd.Focus();
                return;
            }

            // 2. 构建用户实体
            Users user = new Users
            {
                user_name = txtName.Text.Trim(),
                id_card = txtIdCard.Text.Trim(),
                phone = txtPhone.Text.Trim(),
                password = txtPwd.Text.Trim(),
                gender = cboGender.SelectedIndex == 0 ? 1 : 2,
                birth_date = dtpBirthDate.Value,
                diabetes_type = cboDiabetesType.Text == "无" ? null : cboDiabetesType.Text,
                age = DateTime.Now.Year - dtpBirthDate.Value.Year
            };

            // 3. 调用注册方法
            string result = bllUser.PatientRegister(user);
            if (result == "ok")
            {
                MessageBox.Show("注册成功！请使用手机号登录", "注册成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show(result, "注册失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}