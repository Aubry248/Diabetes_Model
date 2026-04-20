using System;
using System.Drawing;
using System.Windows.Forms;
using Model;
using BLL;

namespace AdminUI
{
    public partial class FrmRoleEdit : Form
    {
        #region 控件声明
        private TextBox txtRoleCode, txtRoleName, txtRoleDesc;
        private Button btnSave, btnCancel;
        #endregion

        #region 全局变量
        private readonly B_Role bllRole = new B_Role();
        private readonly int _roleId = 0; // 0=新增，>0=修改
        #endregion

        // 构造函数
        public FrmRoleEdit(int roleId)
        {
            _roleId = roleId;
            InitializeComponent();
            InitFormSetting();
            InitAllControls();
            if (_roleId > 0) LoadRoleInfo();
        }

        #region 1. 窗体基础设置
        private void InitFormSetting()
        {
            this.Text = _roleId == 0 ? "新增角色" : "修改角色";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(420, 320);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
        }
        #endregion

        #region 2. 动态创建控件（修复占位符逻辑）
        private void InitAllControls()
        {
            int labelLeft = 30;
            int inputLeft = 120;
            int topStart = 30;
            int lineHeight = 40;

            // 1. 角色编码
            Label lblRoleCode = new Label
            {
                Text = "角色编码：",
                Font = new Font("微软雅黑", 10F, FontStyle.Regular),
                AutoSize = true
            };
            lblRoleCode.Location = new Point(labelLeft, topStart + 5);
            this.Controls.Add(lblRoleCode);

            txtRoleCode = new TextBox
            {
                Font = new Font("微软雅黑", 10F),
                Size = new Size(240, 27),
                Location = new Point(inputLeft, topStart),
                MaxLength = 10 // 匹配数据库编码长度限制，避免超长
            };
            this.Controls.Add(txtRoleCode);
            topStart += lineHeight;

            // 2. 角色名称
            Label lblRoleName = new Label
            {
                Text = "角色名称：",
                Font = new Font("微软雅黑", 10F, FontStyle.Regular),
                AutoSize = true
            };
            lblRoleName.Location = new Point(labelLeft, topStart + 5);
            this.Controls.Add(lblRoleName);

            txtRoleName = new TextBox
            {
                Font = new Font("微软雅黑", 10F),
                Size = new Size(240, 27),
                Location = new Point(inputLeft, topStart),
                MaxLength = 20 // 严格匹配数据库role_name字段长度，避免触发约束
            };
            this.Controls.Add(txtRoleName);
            topStart += lineHeight;

            // 3. 角色描述
            Label lblRoleDesc = new Label
            {
                Text = "角色描述：",
                Font = new Font("微软雅黑", 10F, FontStyle.Regular),
                AutoSize = true
            };
            lblRoleDesc.Location = new Point(labelLeft, topStart + 5);
            this.Controls.Add(lblRoleDesc);

            txtRoleDesc = new TextBox
            {
                Font = new Font("微软雅黑", 10F),
                Size = new Size(240, 50),
                Location = new Point(inputLeft, topStart),
                Multiline = true,
                MaxLength = 100
            };
            this.Controls.Add(txtRoleDesc);
            topStart += lineHeight + 20;

            // 4. 保存按钮
            btnSave = new Button
            {
                Text = "保 存",
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                Size = new Size(100, 35),
                Location = new Point(inputLeft, topStart),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            // 5. 取消按钮
            btnCancel = new Button
            {
                Text = "取 消",
                Font = new Font("微软雅黑", 10F),
                Size = new Size(100, 35),
                Location = new Point(inputLeft + 120, topStart),
                BackColor = Color.FromArgb(230, 230, 230),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);
        }
        #endregion

        #region 3. 加载角色信息（修改时）
        private void LoadRoleInfo()
        {
            Role role = bllRole.GetRoleById(_roleId);
            if (role == null)
            {
                MessageBox.Show("角色信息不存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
            }
            txtRoleCode.Text = role.role_code;
            txtRoleName.Text = role.role_name;
            txtRoleDesc.Text = role.role_desc;
            txtRoleCode.Enabled = false; // 编码创建后不可修改，避免数据混乱
        }
        #endregion

        #region 4. 保存按钮核心逻辑（和数据库约束100%匹配）
        private void BtnSave_Click(object sender, EventArgs e)
        {
            // 1. 基础非空校验
            string code = txtRoleCode.Text.Trim();
            string name = txtRoleName.Text.Trim();
            string desc = txtRoleDesc.Text.Trim();

            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("请输入角色编码！", "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRoleCode.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("请输入角色名称！", "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRoleName.Focus();
                return;
            }

            // 2. 长度校验（和数据库字段长度严格一致）
            if (code.Length > 10)
            {
                MessageBox.Show("角色编码不能超过10个字符！", "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRoleCode.Focus();
                return;
            }
            if (name.Length > 20)
            {
                MessageBox.Show("角色名称不能超过20个字符！", "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRoleName.Focus();
                return;
            }

            // 3. 特殊字符校验（和数据库CHECK约束完全一致）
            string specialCharPattern = @"[<>/\\*&%$#@!(){}|`~]";
            if (System.Text.RegularExpressions.Regex.IsMatch(code, specialCharPattern))
            {
                MessageBox.Show("角色编码不能包含特殊字符！", "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRoleCode.Focus();
                return;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(name, specialCharPattern))
            {
                MessageBox.Show("角色名称不能包含特殊字符！", "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRoleName.Focus();
                return;
            }

            // 4. 构建实体（操作留痕）
            Role role = new Role
            {
                role_code = code,
                role_name = name,
                role_desc = desc,
                status = 1,
                create_by = Program.LoginUser.user_id,
                update_by = Program.LoginUser.user_id
            };

            // 5. 新增/修改分支
            string result;
            if (_roleId == 0)
            {
                result = bllRole.AddRole(role);
            }
            else
            {
                role.role_id = _roleId;
                result = bllRole.UpdateRole(role);
            }

            // 6. 结果处理
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