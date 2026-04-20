using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BLL;
using Model;

namespace AdminUI
{
    public partial class FrmMyInfo : Form
    {
        // 复用现有业务层
        private readonly B_User bllUser = new B_User();
        // 当前登录用户
        private readonly Users _currentUser;
        // 控件定义
        private TextBox txtLoginAccount, txtUserName, txtPhone, txtRole, txtUserType, txtCreateTime, txtStatus;
        private Button btnSave, btnReset;

        public FrmMyInfo()
        {
            InitializeComponent();
            this.Controls.Clear();

            // ===================== 【关键修复1：提前校验登录信息，从根源避免null】=====================
            _currentUser = Program.LoginUser;
            // 构造函数里提前校验，登录信息为空直接终止初始化，避免后续异常
            if (_currentUser == null)
            {
                MessageBox.Show("当前登录用户信息异常，请重新登录！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // 提前设置关闭标记，不执行后续控件创建
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Minimized;
                this.Load += (s, e) => this.Close();
                return;
            }

            // 窗体基础配置（和现有页面完全兼容）
            this.Text = "个人信息维护";
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1000, 600);
            this.Font = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // 创建页面控件
            CreateAllControls();
            // 绑定事件
            this.Load += FrmMyInfo_Load;
            // 绑定窗体Shown事件，确保句柄创建完成后再执行关闭逻辑
            this.Shown += FrmMyInfo_Shown;
        }

        #region 页面布局（稳定Anchor布局，控件永不消失）
        private void CreateAllControls()
        {
            this.SuspendLayout();
            int margin = 20;
            int formWidth = this.ClientSize.Width;
            int formHeight = this.ClientSize.Height;

            // ====================== 顶部标题栏 ======================
            Label lblTitle = new Label
            {
                Text = "管理员个人信息",
                Font = new Font("微软雅黑", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(margin, margin),
                AutoSize = true
            };
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            this.Controls.Add(lblTitle);

            // ====================== 中间信息卡片面板 ======================
            Panel pnlCard = new Panel
            {
                Location = new Point(margin, 80),
                Size = new Size(formWidth - 2 * margin, formHeight - 200),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;

            int labelWidth = 120;
            int inputWidth = 300;
            int rowHeight = 50;
            int startTop = 40;
            int startLeft = 80;

            // 1. 登录账号（只读）
            Label lbl1 = new Label { Text = "登录账号：", Location = new Point(startLeft, startTop), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 10F) };
            txtLoginAccount = new TextBox { Location = new Point(startLeft + labelWidth, startTop), Size = new Size(inputWidth, 28), Font = new Font("微软雅黑", 10F), ReadOnly = true, BackColor = Color.FromArgb(240, 240, 240) };
            pnlCard.Controls.AddRange(new Control[] { lbl1, txtLoginAccount });

            // 2. 用户姓名（可编辑）
            startTop += rowHeight;
            Label lbl2 = new Label { Text = "用户姓名：", Location = new Point(startLeft, startTop), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 10F) };
            txtUserName = new TextBox { Location = new Point(startLeft + labelWidth, startTop), Size = new Size(inputWidth, 28), Font = new Font("微软雅黑", 10F) };
            pnlCard.Controls.AddRange(new Control[] { lbl2, txtUserName });

            // 3. 手机号（可编辑）
            startTop += rowHeight;
            Label lbl3 = new Label { Text = "手机号：", Location = new Point(startLeft, startTop), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 10F) };
            txtPhone = new TextBox { Location = new Point(startLeft + labelWidth, startTop), Size = new Size(inputWidth, 28), Font = new Font("微软雅黑", 10F) };
            pnlCard.Controls.AddRange(new Control[] { lbl3, txtPhone });

            // 4. 所属角色（只读）
            startTop += rowHeight;
            Label lbl4 = new Label { Text = "所属角色：", Location = new Point(startLeft, startTop), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 10F) };
            txtRole = new TextBox { Location = new Point(startLeft + labelWidth, startTop), Size = new Size(inputWidth, 28), Font = new Font("微软雅黑", 10F), ReadOnly = true, BackColor = Color.FromArgb(240, 240, 240) };
            pnlCard.Controls.AddRange(new Control[] { lbl4, txtRole });

            // 5. 用户类型（只读）
            startTop += rowHeight;
            Label lbl5 = new Label { Text = "用户类型：", Location = new Point(startLeft, startTop), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 10F) };
            txtUserType = new TextBox { Location = new Point(startLeft + labelWidth, startTop), Size = new Size(inputWidth, 28), Font = new Font("微软雅黑", 10F), ReadOnly = true, BackColor = Color.FromArgb(240, 240, 240) };
            pnlCard.Controls.AddRange(new Control[] { lbl5, txtUserType });

            // 6. 创建时间（只读）
            startTop += rowHeight;
            Label lbl6 = new Label { Text = "创建时间：", Location = new Point(startLeft, startTop), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 10F) };
            txtCreateTime = new TextBox { Location = new Point(startLeft + labelWidth, startTop), Size = new Size(inputWidth, 28), Font = new Font("微软雅黑", 10F), ReadOnly = true, BackColor = Color.FromArgb(240, 240, 240) };
            pnlCard.Controls.AddRange(new Control[] { lbl6, txtCreateTime });

            // 7. 账号状态（只读）
            startTop += rowHeight;
            Label lbl7 = new Label { Text = "账号状态：", Location = new Point(startLeft, startTop), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 10F) };
            txtStatus = new TextBox { Location = new Point(startLeft + labelWidth, startTop), Size = new Size(inputWidth, 28), Font = new Font("微软雅黑", 10F), ReadOnly = true, BackColor = Color.FromArgb(240, 240, 240) };
            pnlCard.Controls.AddRange(new Control[] { lbl7, txtStatus });

            this.Controls.Add(pnlCard);

            // ====================== 底部操作按钮 ======================
            int btnWidth = 120;
            int btnHeight = 38;
            int btnTop = formHeight - 100;

            btnSave = new Button
            {
                Text = "保存修改",
                Location = new Point(formWidth - 2 * btnWidth - 3 * margin, btnTop),
                Size = new Size(btnWidth, btnHeight),
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnReset = new Button
            {
                Text = "重置内容",
                Location = new Point(formWidth - btnWidth - 2 * margin, btnTop),
                Size = new Size(btnWidth, btnHeight),
                Font = new Font("微软雅黑", 10F),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            // 按钮锚定，缩放不消失
            btnSave.Anchor = btnReset.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.Controls.AddRange(new Control[] { btnSave, btnReset });
            this.ResumeLayout(true);
        }
        #endregion

        #region 窗体生命周期事件
        // 【关键修复2：窗体完全显示、句柄创建完成后，再执行校验和关闭】
        private void FrmMyInfo_Shown(object sender, EventArgs e)
        {
            // 窗体完全显示后，再次校验用户信息，异常则安全关闭
            if (_currentUser == null)
            {
                this.Close();
                return;
            }
        }

        private void FrmMyInfo_Load(object sender, EventArgs e)
        {
            // 绑定按钮事件
            btnSave.Click += BtnSave_Click;
            btnReset.Click += BtnReset_Click;

            // 加载当前用户信息
            LoadUserInfo();

            // 更新状态栏
            if (this.MdiParent is FrmAdminMain mainForm)
            {
                mainForm.SetStatusTip("操作提示：已打开【个人信息维护】");
            }
        }
        #endregion

        #region 数据加载方法
        private void LoadUserInfo()
        {
            // 【关键修复3：移除Load事件里的直接Close()，用BeginInvoke安全延迟关闭】
            if (_currentUser == null)
            {
                MessageBox.Show("当前登录用户信息异常！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // 异步延迟执行Close()，确保窗体句柄创建完成后再关闭
                this.BeginInvoke(new Action(() => this.Close()));
                return;
            }

            // 绑定数据
            txtLoginAccount.Text = _currentUser.login_account;
            txtUserName.Text = _currentUser.user_name;
            txtPhone.Text = _currentUser.phone;

            // 用户类型文本转换
            string userTypeName;
            if (_currentUser.user_type == 1)
                userTypeName = "普通患者";
            else if (_currentUser.user_type == 2)
                userTypeName = "公卫医生";
            else if (_currentUser.user_type == 3)
                userTypeName = "系统管理员";
            else
                userTypeName = "未知";
            txtUserType.Text = userTypeName;

            txtStatus.Text = _currentUser.status == 1 ? "正常启用" : "已禁用";
            txtCreateTime.Text = _currentUser.create_time.ToString("yyyy-MM-dd HH:mm:ss");

            // 加载角色名称（增加异常捕获，避免局部错误导致整个窗体崩溃）
            try
            {
                B_Role bllRole = new B_Role();
                int roleId = bllUser.GetUserRoleId(_currentUser.user_id);
                var role = bllRole.GetRoleById(roleId);
                txtRole.Text = role?.role_name ?? "系统管理员";
            }
            catch (Exception ex)
            {
                txtRole.Text = "加载失败";
                MessageBox.Show($"角色信息加载异常：{ex.Message}", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 按钮事件
        private void BtnReset_Click(object sender, EventArgs e)
        {
            LoadUserInfo();
            MessageBox.Show("已重置为原始信息", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // 1. 基础校验
            string userName = txtUserName.Text.Trim();
            string phone = txtPhone.Text.Trim();

            if (string.IsNullOrEmpty(userName))
            {
                MessageBox.Show("请输入用户姓名！", "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUserName.Focus();
                return;
            }
            if (string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("请输入手机号！", "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return;
            }
            // 手机号格式校验
            if (!Regex.IsMatch(phone, @"^1[3-9]\d{9}$"))
            {
                MessageBox.Show("请输入正确的11位手机号！", "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return;
            }
            // 手机号唯一性校验
            if (!bllUser.CheckPhoneUnique(phone, _currentUser.user_id))
            {
                MessageBox.Show("该手机号已被其他用户使用！", "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return;
            }

            // 2. 提交修改
            try
            {
                // 更新用户信息
                _currentUser.user_name = userName;
                _currentUser.phone = phone;
                string result = bllUser.UpdateUserBaseInfo(_currentUser);

                if (result == "ok")
                {
                    // 同步全局登录用户信息
                    Program.LoginUser = _currentUser;
                    MessageBox.Show("个人信息修改成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadUserInfo();

                    // 更新主页面状态栏
                    if (this.MdiParent is FrmAdminMain mainForm)
                    {
                        mainForm.SetStatusTip("操作提示：个人信息修改成功");
                    }
                }
                else
                {
                    MessageBox.Show(result, "修改失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"系统异常：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}