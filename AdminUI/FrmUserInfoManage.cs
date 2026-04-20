using BLL;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AdminUI
{
    public partial class FrmUserInfoManage : Form
    {
        // 业务层对象
        private readonly B_User bllUser = new B_User();
        private readonly B_Role bllRole = new B_Role();
        private void DgvUserList_Paint(object sender, PaintEventArgs e)
        {
            int radius = 12;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
                path.AddArc(dgvUserList.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
                path.AddArc(dgvUserList.Width - radius * 2, dgvUserList.Height - radius * 2, radius * 2, radius * 2, 0, 90);
                path.AddArc(0, dgvUserList.Height - radius * 2, radius * 2, radius * 2, 90, 90);
                path.CloseAllFigures();

                dgvUserList.Region = new Region(path);
            }
        }

        // 👇 这里就是你要放的位置！和类成员放在一起
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);

        // 当前选中的用户ID
        private int _currentSelectedUserId = 0;
        // 控件定义
        private TextBox txtQueryAccount, txtQueryName, txtQueryPhone;
        private ComboBox cboQueryRole;
        private DataGridView dgvUserList;
        private Button btnQuery, btnReset, btnAdd, btnEdit, btnDelete, btnStatus, btnResetPwd;
        // 【固定】右侧留给按钮的宽度（永远留200像素）
        private const int RIGHT_SPACE = 400;

        public FrmUserInfoManage()
        {
            InitializeComponent();
            this.Controls.Clear();
            // 【修改】窗体基础配置
            this.Text = "用户信息管理";
            this.Size = new Size(1400, 800);
            this.MinimumSize = new Size(1200, 700);

            // 重点修改：统一使用现代字体 "微软雅黑"，背景改为白色
            this.Font = new Font("微软雅黑", 9F);
            this.BackColor = Color.White;

            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.StartPosition = FormStartPosition.CenterParent;
            this.WindowState = FormWindowState.Maximized;

            CreateAllControls();
            this.Load += FrmUserInfoManage_Load;
            // 窗体缩放时统一重置布局
            this.Resize += (s, e) => { ResetLayout(); };
        }

        #region 控件创建（修复布局+层级，按钮必显）
        private void CreateAllControls()
        {
            this.SuspendLayout();
            int margin = 12;

            // ==============================================
            // 顶部查询区域
            // ==============================================
            Panel pnlQuery = new Panel();
            pnlQuery.Location = new Point(margin, margin);
            pnlQuery.Size = new Size(this.ClientSize.Width - 2 * margin, 50);
            pnlQuery.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlQuery.BackColor = Color.White;


            // 优化后的完整代码示例
            Label lblAccount = new Label { Text = "🔐登录账号：", Location = new Point(0, 10), AutoSize = true, Font = new Font("微软雅黑", 9F) };
            txtQueryAccount = new TextBox { Location = new Point(85, 8), Size = new Size(120, 25), Font = new Font("微软雅黑", 9F) };

            Label lblName = new Label { Text = "👤用户姓名：", Location = new Point(220, 10), AutoSize = true, Font = new Font("微软雅黑", 9F) };
            txtQueryName = new TextBox { Location = new Point(305, 8), Size = new Size(120, 25), Font = new Font("微软雅黑", 9F) };

            Label lblPhone = new Label { Text = "📞手机号：", Location = new Point(440, 10), AutoSize = true, Font = new Font("微软雅黑", 9F) };
            txtQueryPhone = new TextBox { Location = new Point(515, 8), Size = new Size(120, 25), Font = new Font("微软雅黑", 9F) };

            Label lblRole = new Label { Text = "👥用户角色：", Location = new Point(640, 10), AutoSize = true, Font = new Font("微软雅黑", 9F) };
            cboQueryRole = new ComboBox { Location = new Point(725, 8), Size = new Size(120, 25), Font = new Font("微软雅黑", 9F), DropDownStyle = ComboBoxStyle.DropDownList };

            btnQuery = new Button
            {
                Text = "🔍查询",
                Location = new Point(870, 6), // 位置完全不变
                Size = new Size(80, 28),
                Font = new Font("微软雅黑", 9F),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 },
                Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 80, 28, 14, 14))
            };

            btnQuery.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                Color top = Color.FromArgb(100, 160, 255);
                Color bottom = Color.FromArgb(22, 119, 255);

                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    btnQuery.ClientRectangle, top, bottom, System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, btnQuery.ClientRectangle);
                }

                TextRenderer.DrawText(e.Graphics, btnQuery.Text, btnQuery.Font, btnQuery.ClientRectangle,
                    btnQuery.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            btnReset = new Button
            {
                Text = "🔄 重置",
                Location = new Point(960, 6), // 位置完全不变
                Size = new Size(80, 28),
                Font = new Font("微软雅黑", 9F),
                ForeColor = Color.White, // 文字变白，绿色背景更清晰
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 },
                Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 80, 28, 14, 14)) // 圆角
            };

            btnReset.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // 🔥 渐变绿色（清爽好看）
                Color top = Color.FromArgb(80, 200, 120);
                Color bottom = Color.FromArgb(40, 160, 80);

                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    btnReset.ClientRectangle, top, bottom, System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, btnReset.ClientRectangle);
                }

                TextRenderer.DrawText(e.Graphics, btnReset.Text, btnReset.Font, btnReset.ClientRectangle,
                    btnReset.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            pnlQuery.Controls.AddRange(new Control[] { lblAccount, txtQueryAccount, lblName, txtQueryName, lblPhone, txtQueryPhone, lblRole, cboQueryRole, btnQuery, btnReset });
            this.Controls.Add(pnlQuery);

            // ==============================================
            // 右侧功能按钮（先创建，保证层级在顶层）
            // ==============================================
            int btnWidth = 160;
            int btnHeight = 38;
            btnAdd = new Button
            {
                Text = "新增用户",
                Size = new Size(btnWidth, btnHeight),
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                //BackColor = Color.FromArgb(22, 119, 255),  // 渐变模式下不再使用 BackColor
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 },
                // 👇 新增：让按钮自动变成圆角胶囊形状
                Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, btnWidth, btnHeight, 20, 20))
            };

            // 👇 新增：通过绘制实现渐变背景 + 玻璃高光效果
            btnAdd.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // 你的渐变色（你截图里是蓝+浅蓝，我给你配了舒服的科技蓝）
                Color topColor = Color.FromArgb(130, 210, 255);   // 上亮
                Color bottomColor = Color.FromArgb(22, 119, 255); // 下深

                // 1. 绘制渐变背景
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    btnAdd.ClientRectangle, topColor, bottomColor, System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, btnAdd.ClientRectangle);
                }


                // 3. 文字居中绘制
                TextRenderer.DrawText(e.Graphics, btnAdd.Text, btnAdd.Font, btnAdd.ClientRectangle,
                    btnAdd.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            btnEdit = new Button
            {
                Text = "修改用户",
                Size = new Size(btnWidth, btnHeight),
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 },
                Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnWidth, btnHeight, 20, 20))
            };

            btnEdit.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                Color top = Color.FromArgb(80, 140, 255);
                Color bottom = Color.FromArgb(20, 90, 220);

                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    btnEdit.ClientRectangle, top, bottom, System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, btnEdit.ClientRectangle);
                }

                TextRenderer.DrawText(e.Graphics, btnEdit.Text, btnEdit.Font, btnEdit.ClientRectangle,
                    btnEdit.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            btnResetPwd = new Button
            {
                Text = "重置密码",
                Size = new Size(btnWidth, btnHeight),
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 },
                Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnWidth, btnHeight, 20, 20)),
            };

            btnResetPwd.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // 浅黄色渐变（上亮下柔和）
                Color top = Color.FromArgb(255, 160, 80);
                Color bottom = Color.FromArgb(230, 100, 20);

                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    btnResetPwd.ClientRectangle, top, bottom, System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, btnResetPwd.ClientRectangle);
                }

                TextRenderer.DrawText(e.Graphics, btnResetPwd.Text, btnResetPwd.Font, btnResetPwd.ClientRectangle,
                    btnResetPwd.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            btnStatus = new Button
            {
                Text = "禁用账号",
                Size = new Size(btnWidth, btnHeight),
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 },
                Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnWidth, btnHeight, 20, 20)),
            };

            btnStatus.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                Color top = Color.FromArgb(255, 245, 180);
                Color bottom = Color.FromArgb(255, 220, 100);

                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    btnStatus.ClientRectangle, top, bottom, System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, btnStatus.ClientRectangle);
                }

                TextRenderer.DrawText(e.Graphics, btnStatus.Text, btnStatus.Font, btnStatus.ClientRectangle,
                    btnStatus.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            btnDelete = new Button
            {
                Text = "删除用户",
                Size = new Size(btnWidth, btnHeight),
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 },
                Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnWidth, btnHeight, 20, 20))
            };

            btnDelete.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                Color top = Color.FromArgb(255, 100, 100);
                Color bottom = Color.FromArgb(200, 30, 30);

                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    btnDelete.ClientRectangle, top, bottom, System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, btnDelete.ClientRectangle);
                }

                TextRenderer.DrawText(e.Graphics, btnDelete.Text, btnDelete.Font, btnDelete.ClientRectangle,
                    btnDelete.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            // 锚定顶部+右侧，提升控件层级（不被表格遮挡）
            btnAdd.Anchor = btnEdit.Anchor = btnStatus.Anchor = btnResetPwd.Anchor = btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnStatus, btnResetPwd, btnDelete });
            // 提升按钮层级，确保在表格上方
            this.Controls.SetChildIndex(btnAdd, 0);
            this.Controls.SetChildIndex(btnEdit, 0);
            this.Controls.SetChildIndex(btnStatus, 0);
            this.Controls.SetChildIndex(btnResetPwd, 0);
            this.Controls.SetChildIndex(btnDelete, 0);

            // ==============================================
            // 左侧用户列表（预留右侧200px按钮区）
            dgvUserList = new DataGridView();
            dgvUserList.Location = new Point(margin, 70);
            dgvUserList.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            dgvUserList.ScrollBars = ScrollBars.Both;
            dgvUserList.AutoGenerateColumns = false;
            dgvUserList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvUserList.BackgroundColor = Color.White;
            dgvUserList.ReadOnly = true;
            dgvUserList.RowHeadersVisible = false;
            dgvUserList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvUserList.EnableHeadersVisualStyles = false; // 必须关闭系统默认样式，才能画渐变
            dgvUserList.ColumnHeadersHeight = 40; // 表头加高，更现代
            dgvUserList.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", 9.5f, FontStyle.Bold);
            dgvUserList.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black; // 表头文字白色，和渐变搭配
            dgvUserList.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(170, 210, 255);
            // 2. 行样式 + 交替行
            dgvUserList.RowTemplate.Height = 35; // 行高加高，不拥挤
            dgvUserList.DefaultCellStyle.Font = new Font("微软雅黑", 9f);
            dgvUserList.DefaultCellStyle.BackColor = Color.White;
            dgvUserList.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 255); // 浅蓝交替行
            dgvUserList.GridColor = Color.White; // 表格线颜色调浅，不刺眼
            dgvUserList.CellBorderStyle = DataGridViewCellBorderStyle.None; // 彻底去掉单元格边框                                                                
            dgvUserList.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None; // 3. 表头边框也去掉，和示例一致
            dgvUserList.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dgvUserList.Paint += DgvUserList_Paint; // 圆角表格

            dgvUserList.Columns.AddRange(new DataGridViewColumn[]
            {
               new DataGridViewTextBoxColumn() { Name = "user_id", Visible = false },
                new DataGridViewTextBoxColumn() { Name = "login_account", HeaderText = "👤登录账号", Width = 120 },
                new DataGridViewTextBoxColumn() { Name = "user_name", HeaderText = "👤用户姓名", Width = 100 },
                new DataGridViewTextBoxColumn() { Name = "phone", HeaderText = "📞手机号", Width = 130 },
                new DataGridViewTextBoxColumn() { Name = "role_name", HeaderText = "⚙️所属角色", Width = 120 },
                new DataGridViewTextBoxColumn() { Name = "user_type_name", HeaderText = "👥用户类型", Width = 110 },
                new DataGridViewTextBoxColumn() { Name = "status_name", HeaderText = "🚦账号状态", Width = 100 },
                new DataGridViewTextBoxColumn() { Name = "create_time", HeaderText = "📅创建时间", Width = 150 }
            });
            // 让所有列按比例平分宽度，填满表格
            // 固定列宽：前几列按你设定的Width显示
            dgvUserList.Columns["login_account"].Width = 120;
            dgvUserList.Columns["user_name"].Width = 100;
            dgvUserList.Columns["phone"].Width = 130;
            dgvUserList.Columns["role_name"].Width = 120;
            dgvUserList.Columns["user_type_name"].Width = 110;
            dgvUserList.Columns["status_name"].Width = 100;

            // 最后一列自动填满剩余空间（不再出现右侧空白）
            dgvUserList.Columns["create_time"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.Controls.Add(dgvUserList);

            // 初始化统一布局
            ResetLayout();
            this.ResumeLayout(true);
        }
        #endregion

        #region 【核心修复】统一重置布局（表格+按钮）
        /// <summary>
        /// 统一重置布局：左侧表格自适应，右侧按钮固定显示
        /// </summary>
        private void ResetLayout()
        {
            int margin = 12;
            // 1. 重置表格尺寸：宽度=总宽-右侧200px-双边距，高度自适应
            dgvUserList.Size = new Size(
                this.ClientSize.Width - RIGHT_SPACE - margin * 2,
                this.ClientSize.Height - 120
            );

            // 2. 重置按钮位置
            ResetButtonPosition();
        }

        /// <summary>
        /// 重置右侧按钮位置
        /// </summary>
        private void ResetButtonPosition()
        {
            int margin = 12;
            int btnTop = 70;
            int btnGap = 50;
            // 按钮左侧坐标：窗体宽度-右侧留空+边距，永远在可视区
            int btnLeft = this.ClientSize.Width - RIGHT_SPACE + margin;

            btnAdd.Location = new Point(btnLeft, btnTop);
            btnEdit.Location = new Point(btnLeft, btnTop + btnGap);
            btnStatus.Location = new Point(btnLeft, btnTop + btnGap * 2);
            btnResetPwd.Location = new Point(btnLeft, btnTop + btnGap * 3);
            btnDelete.Location = new Point(btnLeft, btnTop + btnGap * 4);
        }
        #endregion

        // ====================== 业务逻辑完全保留 ======================
        #region 窗体加载事件
        private void FrmUserInfoManage_Load(object sender, EventArgs e)
        {
            btnQuery.Click += BtnQuery_Click;
            btnReset.Click += BtnReset_Click;
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnStatus.Click += BtnStatus_Click;
            btnResetPwd.Click += BtnResetPwd_Click;
            btnDelete.Click += BtnDelete_Click;
            dgvUserList.SelectionChanged += DgvUserList_SelectionChanged;
            dgvUserList.CellFormatting += DgvUserList_CellFormatting;

            InitRoleComboBox();
            LoadUserList();

            if (this.MdiParent is FrmAdminMain mainForm)
                mainForm.SetStatusTip("操作提示：已打开【用户信息管理】");
        }
        #endregion

        #region 核心业务方法
        private void InitRoleComboBox()
        {
            var roleList = bllRole.GetAllRoleList();
            roleList.Insert(0, new Role { role_id = 0, role_name = "全部" });
            cboQueryRole.DataSource = roleList;
            cboQueryRole.DisplayMember = "role_name";
            cboQueryRole.ValueMember = "role_id";
        }

        private void LoadUserList()
        {
            dgvUserList.Rows.Clear();
            string loginAccount = txtQueryAccount.Text.Trim();
            string userName = txtQueryName.Text.Trim();
            string phone = txtQueryPhone.Text.Trim();
            int? userType = null;

            if (cboQueryRole.SelectedItem is Role selectedRole && selectedRole.role_id > 0)
            {
                switch (selectedRole.role_code)
                {
                    case "admin":
                        userType = 3;
                        break;
                    case "doctor":
                        userType = 2;
                        break;
                    case "patient":
                        userType = 1;
                        break;
                    default:
                        userType = null;
                        break;
                }
            }

            List<Users> userList = bllUser.GetUserList(loginAccount, userName, phone, userType);
            foreach (Users user in userList)
            {
                int roleId = bllUser.GetUserRoleId(user.user_id);
                var role = bllRole.GetRoleById(roleId);
                string userTypeName;
                switch (user.user_type)
                {
                    case 1:
                        userTypeName = "普通患者";
                        break;
                    case 2:
                        userTypeName = "公卫医生";
                        break;
                    case 3:
                        userTypeName = "系统管理员";
                        break;
                    default:
                        userTypeName = "未知";
                        break;
                }

                string statusName = user.status == 1 ? "启用" : "禁用";
                string roleName = role?.role_name ?? "未绑定";

                dgvUserList.Rows.Add(user.user_id, user.login_account, user.user_name, user.phone,
                    roleName, userTypeName, statusName, user.create_time.ToString("yyyy-MM-dd HH:mm"));
            }

            dgvUserList.ClearSelection();
            _currentSelectedUserId = 0;
            SetButtonEnabled(false);
        }

        private void DgvUserList_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUserList.SelectedRows.Count == 0 || dgvUserList.SelectedRows[0].IsNewRow)
            {
                _currentSelectedUserId = 0;
                SetButtonEnabled(false);
                return;
            }

            DataGridViewRow row = dgvUserList.SelectedRows[0];
            _currentSelectedUserId = Convert.ToInt32(row.Cells["user_id"].Value);
            string status = row.Cells["status_name"].Value.ToString();
            string userType = row.Cells["user_type_name"].Value.ToString();
            bool isAdmin = userType == "系统管理员";

            SetButtonEnabled(!isAdmin);
            btnStatus.Text = status == "启用" ? "禁用账号" : "启用账号";
            btnStatus.BackColor = status == "启用" ? Color.FromArgb(255, 193, 7) : Color.FromArgb(40, 160, 40);
            btnStatus.ForeColor = status == "启用" ? Color.Black : Color.White;
        }

        private void DgvUserList_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = dgvUserList.Rows[e.RowIndex];
            if (row.Cells["user_type_name"].Value?.ToString() == "系统管理员")
                row.DefaultCellStyle.ForeColor = Color.Red;
        }

        private void FrmUserInfoManage_Load_1(object sender, EventArgs e)
        {

        }

        private void SetButtonEnabled(bool enabled)
        {
            btnEdit.Enabled = enabled;
            btnStatus.Enabled = enabled;
            btnResetPwd.Enabled = enabled;
            btnDelete.Enabled = enabled;
        }
        #endregion

        #region 按钮点击事件
        private void BtnQuery_Click(object sender, EventArgs e) => LoadUserList();

        private void BtnReset_Click(object sender, EventArgs e)
        {
            txtQueryAccount.Clear();
            txtQueryName.Clear();
            txtQueryPhone.Clear();
            cboQueryRole.SelectedIndex = 0;
            LoadUserList();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            FrmUserEdit frm = new FrmUserEdit();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadUserList();
                MessageBox.Show("新增成功！");
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (_currentSelectedUserId <= 0)
            {
                MessageBox.Show("请选择用户");
                return;
            }
            FrmUserEdit frm = new FrmUserEdit(_currentSelectedUserId);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadUserList();
                MessageBox.Show("修改成功！");
            }
        }

        private void BtnStatus_Click(object sender, EventArgs e)
        {
            if (_currentSelectedUserId <= 0) return;
            DataGridViewRow row = dgvUserList.SelectedRows[0];
            string currentStatus = row.Cells["status_name"].Value.ToString();
            int targetStatus = currentStatus == "启用" ? 0 : 1;
            string actionText = currentStatus == "启用" ? "禁用" : "启用";
            string userName = row.Cells["user_name"].Value.ToString();
            if (MessageBox.Show($"确定要{actionText}用户【{userName}】的账号吗？", "操作确认",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            string result = bllUser.UpdateUserStatus(_currentSelectedUserId, targetStatus, Program.LoginUser.user_id);
            if (result == "ok")
            {
                LoadUserList();
                MessageBox.Show($"账号{actionText}成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (this.MdiParent is FrmAdminMain mainForm)
                {
                    mainForm.SetStatusTip($"操作提示：用户【{userName}】账号{actionText}成功");
                }
            }
            else
            {
                MessageBox.Show(result, "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void BtnResetPwd_Click(object sender, EventArgs e)
        {
            if (_currentSelectedUserId <= 0) return;
            string userName = dgvUserList.SelectedRows[0].Cells["user_name"].Value.ToString();
            // 密码输入弹窗
            Form pwdForm = new Form();
            pwdForm.Text = "重置用户密码";
            pwdForm.Size = new Size(350, 180);
            pwdForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            pwdForm.StartPosition = FormStartPosition.CenterParent;
            pwdForm.MaximizeBox = false;
            pwdForm.MinimizeBox = false;
            pwdForm.Font = new Font("微软雅黑", 9F);
            Label lblTip = new Label { Text = $"请输入用户【{userName}】的新密码：", Location = new Point(20, 20), Size = new Size(300, 25) };
            TextBox txtNewPwd = new TextBox { Location = new Point(20, 50), Size = new Size(290, 25), PasswordChar = '*' };
            Button btnOk = new Button { Text = "确定", Location = new Point(60, 90), Size = new Size(100, 35), BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            Button btnCancel = new Button { Text = "取消", Location = new Point(180, 90), Size = new Size(100, 35), BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            pwdForm.Controls.AddRange(new Control[] { lblTip, txtNewPwd, btnOk, btnCancel });
            string newPassword = "";
            btnOk.Click += (s, ev) =>
            {
                if (string.IsNullOrEmpty(txtNewPwd.Text.Trim()) || txtNewPwd.Text.Trim().Length < 6)
                {
                    MessageBox.Show("密码长度不能少于6位！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                newPassword = txtNewPwd.Text.Trim();
                pwdForm.DialogResult = DialogResult.OK;
                pwdForm.Close();
            };
            btnCancel.Click += (s, ev) => { pwdForm.DialogResult = DialogResult.Cancel; pwdForm.Close(); };
            if (pwdForm.ShowDialog() != DialogResult.OK) return;
            // 执行密码重置
            string result = bllUser.ResetPassword(_currentSelectedUserId, newPassword, Program.LoginUser.user_id);
            if (result == "ok")
            {
                MessageBox.Show("密码重置成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (this.MdiParent is FrmAdminMain mainForm)
                {
                    mainForm.SetStatusTip($"操作提示：用户【{userName}】密码重置成功");
                }
            }
            else
            {
                MessageBox.Show(result, "重置失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_currentSelectedUserId <= 0) return;
            string userName = dgvUserList.SelectedRows[0].Cells["user_name"].Value.ToString();
            if (MessageBox.Show($"确定要删除用户【{userName}】吗？删除后该用户的所有健康数据都会被删除，不可恢复！", "删除确认",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            // 先删除用户关联的角色绑定，再删除用户
            string sql = "DELETE FROM t_user_role WHERE user_id = @user_id; DELETE FROM t_user WHERE user_id = @user_id;";
            int result = Tools.SqlHelper.ExecuteNonQuery(sql, new System.Data.SqlClient.SqlParameter("@user_id", _currentSelectedUserId));
            if (result > 0)
            {
                LoadUserList();
                MessageBox.Show("用户删除成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (this.MdiParent is FrmAdminMain mainForm)
                {
                    mainForm.SetStatusTip($"操作提示：用户【{userName}】删除成功");
                }
            }
            else
            {
                MessageBox.Show("用户删除失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}