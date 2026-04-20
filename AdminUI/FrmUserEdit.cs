using System;
using System.Drawing;
using System.Windows.Forms;
using BLL;
using Model;

namespace AdminUI
{
    public partial class FrmUserEdit : Form
    {
        private readonly B_User bllUser = new B_User();
        private readonly B_Role bllRole = new B_Role();
        private int _editUserId = 0;

        // 控件定义
        private TextBox txtAccount, txtPwd, txtName, txtIdCard, txtPhone;
        private ComboBox cboRole, cboGender, cboDiabetesType;
        private DateTimePicker dtpBirthDate;
        private Button btnSave, btnCancel;

        // 新增用户构造函数
        public FrmUserEdit()
        {
            // 【关键修复1】纯代码创建控件，必须注释掉设计器的InitializeComponent()
            // InitializeComponent();
            this.Text = "新增用户";
            this.Size = new Size(500, 580);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("微软雅黑", 9F);
            this.BackColor = Color.White;

            CreateControls();
            this.Load += FrmUserEdit_Load;
        }

        // 修改用户构造函数
        public FrmUserEdit(int userId)
        {
            _editUserId = userId;
            // 【关键修复2】纯代码创建控件，必须注释掉设计器的InitializeComponent()
            // InitializeComponent();
            this.Text = "修改用户信息";
            this.Size = new Size(500, 580);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("微软雅黑", 9F);
            this.BackColor = Color.White;

            CreateControls();
            this.Load += FrmUserEdit_Load;

            // 编辑模式禁用不可修改字段
            txtAccount.Enabled = false;
            txtPwd.Enabled = false;
            txtIdCard.Enabled = false;
        }

        #region 创建控件
        private void CreateControls()
        {
            int left = 30, top = 20, labelWidth = 80, inputWidth = 320, lineHeight = 38;

            // 登录账号
            Label lblAccount = new Label { Text = "登录账号：", Location = new Point(left, top), Size = new Size(labelWidth, 25) };
            txtAccount = new TextBox { Location = new Point(left + labelWidth, top), Size = new Size(inputWidth, 25) };
            top += lineHeight;

            // 登录密码
            Label lblPwd = new Label { Text = "登录密码：", Location = new Point(left, top), Size = new Size(labelWidth, 25) };
            txtPwd = new TextBox { Location = new Point(left + labelWidth, top), Size = new Size(inputWidth, 25), PasswordChar = '*' };
            top += lineHeight;

            // 用户姓名
            Label lblName = new Label { Text = "用户姓名：", Location = new Point(left, top), Size = new Size(labelWidth, 25) };
            txtName = new TextBox { Location = new Point(left + labelWidth, top), Size = new Size(inputWidth, 25) };
            top += lineHeight;

            // 身份证号
            Label lblIdCard = new Label { Text = "身份证号：", Location = new Point(left, top), Size = new Size(labelWidth, 25) };
            txtIdCard = new TextBox { Location = new Point(left + labelWidth, top), Size = new Size(inputWidth, 25) };
            top += lineHeight;

            // 手机号
            Label lblPhone = new Label { Text = "手机号：", Location = new Point(left, top), Size = new Size(labelWidth, 25) };
            txtPhone = new TextBox { Location = new Point(left + labelWidth, top), Size = new Size(inputWidth, 25) };
            top += lineHeight;

            // 所属角色
            Label lblRole = new Label { Text = "所属角色：", Location = new Point(left, top), Size = new Size(labelWidth, 25) };
            cboRole = new ComboBox { Location = new Point(left + labelWidth, top), Size = new Size(inputWidth, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            top += lineHeight;

            // 性别
            Label lblGender = new Label { Text = "性别：", Location = new Point(left, top), Size = new Size(labelWidth, 25) };
            cboGender = new ComboBox { Location = new Point(left + labelWidth, top), Size = new Size(inputWidth, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cboGender.Items.AddRange(new object[] { "男", "女" });
            cboGender.SelectedIndex = 0;
            top += lineHeight;

            // 出生日期
            Label lblBirth = new Label { Text = "出生日期：", Location = new Point(left, top), Size = new Size(labelWidth, 25) };
            dtpBirthDate = new DateTimePicker { Location = new Point(left + labelWidth, top), Size = new Size(inputWidth, 25), Format = DateTimePickerFormat.Short };
            top += lineHeight;

            // 糖尿病类型
            Label lblDiabetes = new Label { Text = "糖尿病类型：", Location = new Point(left, top), Size = new Size(labelWidth, 25) };
            cboDiabetesType = new ComboBox { Location = new Point(left + labelWidth, top), Size = new Size(inputWidth, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cboDiabetesType.Items.AddRange(new object[] { "无", "1型", "2型", "妊娠", "其他" });
            cboDiabetesType.SelectedIndex = 0;
            top += lineHeight;

            // 按钮
            btnSave = new Button
            {
                Text = "保存",
                Location = new Point(left + 100, top + 20),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnCancel = new Button
            {
                Text = "取消",
                Location = new Point(left + 250, top + 20),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };

            // 加入窗体
            this.Controls.AddRange(new Control[] {
                lblAccount, txtAccount, lblPwd, txtPwd, lblName, txtName, lblIdCard, txtIdCard,
                lblPhone, txtPhone, lblRole, cboRole, lblGender, cboGender, lblBirth, dtpBirthDate,
                lblDiabetes, cboDiabetesType, btnSave, btnCancel
            });

            // 事件绑定
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            cboRole.SelectedIndexChanged += CboRole_SelectedIndexChanged;
        }
        #endregion

        #region 窗体加载
        private void FrmUserEdit_Load(object sender, EventArgs e)
        {
            // 绑定角色下拉框
            var roleList = bllRole.GetAllRoleList();
            cboRole.DataSource = roleList;
            cboRole.DisplayMember = "role_name";
            cboRole.ValueMember = "role_id";

            // 编辑模式加载数据
            if (_editUserId > 0)
            {
                LoadUserData();
            }
        }
        #endregion

        #region 角色切换事件【关键修复：彻底解决类型转换报错】
        private void CboRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 【修复】替换 is not 为 C#7.3 兼容的 !(is) 语法
            if (!(cboRole.SelectedItem is Role selectedRole)) return;
            // 患者必填项显示（后续可扩展）
            bool isPatient = selectedRole.role_code == "patient";
        }
        #endregion

        #region 加载编辑用户数据
        private void LoadUserData()
        {
            var userList = bllUser.GetUserList();
            var user = userList.Find(u => u.user_id == _editUserId);
            if (user == null)
            {
                MessageBox.Show("用户不存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
            }

            txtAccount.Text = user.login_account;
            txtName.Text = user.user_name;
            txtIdCard.Text = user.id_card;
            txtPhone.Text = user.phone;
            cboGender.SelectedIndex = user.gender == 1 ? 0 : 1;
            if (user.birth_date.HasValue) dtpBirthDate.Value = user.birth_date.Value;
            cboDiabetesType.Text = user.diabetes_type ?? "无";

            // 绑定角色
            int roleId = bllUser.GetUserRoleId(_editUserId);
            // 【修复】临时解绑事件避免赋值时触发异常，赋值后重新绑定
            cboRole.SelectedIndexChanged -= CboRole_SelectedIndexChanged;
            cboRole.SelectedValue = roleId;
            cboRole.SelectedIndexChanged += CboRole_SelectedIndexChanged;
        }
        #endregion

        #region 保存按钮事件
        private void BtnSave_Click(object sender, EventArgs e)
        {
            // 基础校验
            if (string.IsNullOrEmpty(txtAccount.Text.Trim()))
            {
                MessageBox.Show("请输入登录账号！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAccount.Focus();
                return;
            }
            if (_editUserId == 0 && string.IsNullOrEmpty(txtPwd.Text.Trim()))
            {
                MessageBox.Show("请设置登录密码！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPwd.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                MessageBox.Show("请输入用户姓名！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            // 【修复】替换 is not 为 C#7.3 兼容的 !(is) 语法
            if (!(cboRole.SelectedItem is Role selectedRole))
            {
                MessageBox.Show("请选择所属角色！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboRole.Focus();
                return;
            }

            // 构建用户对象
            Users user = new Users
            {
                login_account = txtAccount.Text.Trim(),
                user_name = txtName.Text.Trim(),
                id_card = txtIdCard.Text.Trim(),
                phone = txtPhone.Text.Trim(),
                gender = cboGender.SelectedIndex == 0 ? 1 : 2,
                birth_date = dtpBirthDate.Value,
                diabetes_type = cboDiabetesType.Text == "无" ? null : cboDiabetesType.Text,
                user_type = 1
            };

            // 角色和用户类型匹配
            int roleId = selectedRole.role_id;
            if (selectedRole.role_code == "admin") user.user_type = 3;
            else if (selectedRole.role_code == "doctor") user.user_type = 2;
            else if (selectedRole.role_code == "patient") user.user_type = 1;
            else user.user_type = 1;

            string result;
            if (_editUserId == 0)
            {
                // 新增用户
                user.password = txtPwd.Text.Trim();
                result = bllUser.AddUser(user, roleId, Program.LoginUser.user_id);
            }
            else
            {
                // 修改用户
                user.user_id = _editUserId;
                result = bllUser.UpdateUser(user, roleId, Program.LoginUser.user_id);
            }

            if (result == "ok")
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(result, "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}