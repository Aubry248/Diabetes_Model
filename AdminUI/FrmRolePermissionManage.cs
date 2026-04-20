using BLL;
using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace AdminUI
{
    public partial class FrmRolePermissionManage : Form
    {
        // 核心业务控件
        private DataGridView dgvRoleList;
        private Button btnAddRole, btnEditRole, btnDeleteRole;
        private CheckBox chkSelectAll;
        private Button btnSavePermission;
        private TreeView tvPermission;
        private Label lblRoleTitle, lblPermissionTitle;

        // 业务层对象（全局初始化，避免重复创建）
        private readonly B_Role bllRole = new B_Role();
        private readonly B_Permission bllPermission = new B_Permission();
        private readonly B_RolePermission bllRolePermission = new B_RolePermission();

        // 当前选中的角色ID
        private int _currentSelectedRoleId = 0;

        public FrmRolePermissionManage()
        {
            // 【关键修复】清空设计器里的旧控件/旧事件绑定，彻底解决CS0103错误
            SuspendLayout();
            InitializeComponent();
            this.Controls.Clear();

            this.Controls.Clear(); // 清空设计器里的所有旧控件，避免和动态创建的冲突

            // 窗体基础配置
            this.Text = "角色权限管理";
            this.Size = new Size(1400, 800);
            this.MinimumSize = new Size(1200, 700);
            this.Font = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.StartPosition = FormStartPosition.CenterParent;
            this.WindowState = FormWindowState.Maximized;

            // 事件绑定
            this.Load += FrmRolePermissionManage_Load;
            this.Resize += (s, e) => {

                this.Refresh();
                // 表格布局刷新，防止缩放后列宽错乱
                if (dgvRoleList != null && !dgvRoleList.IsDisposed)
                {
                    dgvRoleList.Update();
                    dgvRoleList.Refresh();
                }
            };

            // 创建控件
            CreateAllControls();
            ResumeLayout(false);
            PerformLayout();
        }

        #region 窗体加载事件
        private void FrmRolePermissionManage_Load(object sender, EventArgs e)
        {
            // 绑定按钮事件（【关键修复】确保控件创建完成后再绑定，不会找不到方法）
            btnAddRole.Click -= BtnAddRole_Click;
            btnAddRole.Click += BtnAddRole_Click;
            btnEditRole.Click += BtnEditRole_Click;
            btnDeleteRole.Click += BtnDeleteRole_Click;
            btnSavePermission.Click += BtnSavePermission_Click;
            chkSelectAll.CheckedChanged += ChkSelectAll_CheckedChanged;
            tvPermission.AfterCheck += TvPermission_AfterCheck;
            dgvRoleList.SelectionChanged += DgvRoleList_SelectionChanged;

            // 加载业务数据（从数据库读取真实数据）
            InitRoleList();
            InitPermissionTree();
        }
        #endregion

        #region 控件创建与布局（已修复两个核心问题）
        private void CreateAllControls()
        {
            SuspendLayout();
            int formWidth = this.ClientSize.Width;
            int formHeight = this.ClientSize.Height;
            // ==============【修复1：调大左侧面板宽度，解决列表太小、中间空白大】==============
            int leftPanelWidth = 600; // 从380改为600，适配按钮显示+列表宽度
            int margin = 12;
            // ==============================================
            // 左侧：角色列表区域
            // ==============================================
            // 1. 角色列表标题
            lblRoleTitle = new Label();
            lblRoleTitle.Text = "角色列表";
            lblRoleTitle.Font = new Font("微软雅黑", 12F, FontStyle.Bold);
            lblRoleTitle.ForeColor = Color.Black;
            lblRoleTitle.Location = new Point(margin, margin);
            lblRoleTitle.Size = new Size(100, 25);
            lblRoleTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            this.Controls.Add(lblRoleTitle);

            // 2. 按钮行
            Panel pnlBtn = new Panel();
            pnlBtn.Location = new Point(margin, 40);
            pnlBtn.Size = new Size(leftPanelWidth - 2 * margin, 38);
            pnlBtn.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // ==============【修复2：调整按钮间距，确保删除按钮不被隐藏】==============
            btnAddRole = new Button();
            btnAddRole.Text = "新增角色";
            btnAddRole.Font = new Font("微软雅黑", 9F);
            btnAddRole.ForeColor = Color.White;
            btnAddRole.BackColor = Color.FromArgb(0, 122, 204);
            btnAddRole.FlatStyle = FlatStyle.Flat;
            btnAddRole.FlatAppearance.BorderSize = 0;
            btnAddRole.Size = new Size(90, 35);
            btnAddRole.Location = new Point(0, 0);
            btnAddRole.Cursor = Cursors.Hand;
            pnlBtn.Controls.Add(btnAddRole);

            btnEditRole = new Button();
            btnEditRole.Text = "修改角色";
            btnEditRole.Font = new Font("微软雅黑", 9F);
            btnEditRole.ForeColor = Color.White;
            btnEditRole.BackColor = Color.FromArgb(40, 160, 40);
            btnEditRole.FlatStyle = FlatStyle.Flat;
            btnEditRole.FlatAppearance.BorderSize = 0;
            btnEditRole.Size = new Size(90, 35);
            btnEditRole.Location = new Point(95, 0); // 间距5px，避免重叠
            btnEditRole.Cursor = Cursors.Hand;
            pnlBtn.Controls.Add(btnEditRole);

            btnDeleteRole = new Button();
            btnDeleteRole.Text = "删除角色";
            btnDeleteRole.Font = new Font("微软雅黑", 9F);
            btnDeleteRole.ForeColor = Color.White;
            btnDeleteRole.BackColor = Color.FromArgb(220, 53, 69);
            btnDeleteRole.FlatStyle = FlatStyle.Flat;
            btnDeleteRole.FlatAppearance.BorderSize = 0;
            btnDeleteRole.Size = new Size(90, 35);
            btnDeleteRole.Location = new Point(190, 0); // 间距5px，完全在面板可视范围内
            btnDeleteRole.Cursor = Cursors.Hand;
            pnlBtn.Controls.Add(btnDeleteRole);

            this.Controls.Add(pnlBtn);

            // 3. 角色列表表格（彻底解决显示不全问题）
            dgvRoleList = new DataGridView();
            dgvRoleList.Location = new Point(margin, 88);
            dgvRoleList.Size = new Size(leftPanelWidth - 2 * margin, formHeight - 135);
            dgvRoleList.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;

            // ===================== 核心显示修复配置（必须全设置）=====================
            // 基础样式配置
            dgvRoleList.BackgroundColor = Color.White;
            dgvRoleList.BorderStyle = BorderStyle.FixedSingle;
            dgvRoleList.GridColor = Color.LightGray;
            dgvRoleList.EnableHeadersVisualStyles = false;
            // 双缓冲消除闪烁
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                dgvRoleList,
                new object[] { true }
            );
            // 操作权限控制（防止用户误操作打乱布局）
            dgvRoleList.AllowUserToAddRows = false;
            dgvRoleList.AllowUserToDeleteRows = false;
            dgvRoleList.AllowUserToResizeRows = false; // 禁止手动改行高
            dgvRoleList.AllowUserToResizeColumns = true; // 允许手动调列宽，可选关闭
            dgvRoleList.ReadOnly = true;
            dgvRoleList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRoleList.MultiSelect = false;
            dgvRoleList.RowHeadersVisible = false; // 隐藏行头，释放横向空间

            // 表头样式（规范美观）
            dgvRoleList.ColumnHeadersHeight = 35;
            dgvRoleList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvRoleList.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black,
                Font = new Font("微软雅黑", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                WrapMode = DataGridViewTriState.False
            };

            // 单元格样式（核心解决换行、显示不全）
            dgvRoleList.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("微软雅黑", 9F, FontStyle.Regular),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                WrapMode = DataGridViewTriState.True, // 允许文字自动换行（核心！）
                Padding = new Padding(6, 4, 6, 4) // 单元格内边距，避免文字贴边
            };

            // 行高自动适配（核心！换行后自动撑开行高）
            dgvRoleList.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvRoleList.RowTemplate.MinimumHeight = 30; // 最小行高，避免太挤

            // 列宽自适应模式（核心！列宽自动填充表格，不会留空白/挤压）
            dgvRoleList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // ===================== 列定义（合理分配宽度，彻底解决挤压）=====================
            dgvRoleList.Columns.Clear();
            dgvRoleList.Columns.AddRange(new DataGridViewColumn[]
            {
                // 隐藏列：角色ID
                new DataGridViewTextBoxColumn()
                {
                    Name = "role_id",
                    HeaderText = "角色ID",
                    Visible = false
                },
                // 编码列：窄宽度，固定最小宽度
                new DataGridViewTextBoxColumn()
                {
                    Name = "role_code",
                    HeaderText = "编码",
                    FillWeight = 15, // 占比15%
                    MinimumWidth = 60, // 最小宽度，防止缩到看不见
                    SortMode = DataGridViewColumnSortMode.NotSortable
                },
                // 角色名称列：中等宽度
                new DataGridViewTextBoxColumn()
                {
                    Name = "role_name",
                    HeaderText = "角色名称",
                    FillWeight = 20, // 占比20%
                    MinimumWidth = 80,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                },
                // 角色描述列：最宽，放长文本
                new DataGridViewTextBoxColumn()
                {
                    Name = "role_desc",
                    HeaderText = "角色描述",
                    FillWeight = 45, // 占比45%，给长文本留足空间
                    MinimumWidth = 120,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                },
                // 创建时间列：固定宽度
                new DataGridViewTextBoxColumn()
                {
                    Name = "create_time",
                    HeaderText = "创建时间",
                    FillWeight = 20, // 占比20%
                    MinimumWidth = 120,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter // 时间居中显示
                    }
                }
            });
            // 把表格添加到窗体
            this.Controls.Add(dgvRoleList);

            // ==============================================
            // 右侧：权限分配区域（自动适配左侧宽度，消除空白）
            // ==============================================
            int rightStartX = leftPanelWidth + margin * 2;
            int rightWidth = this.ClientSize.Width - rightStartX - margin;

            // 1. 权限分配标题
            lblPermissionTitle = new Label();
            lblPermissionTitle.Text = "权限分配（选中角色后可分配权限）";
            lblPermissionTitle.Font = new Font("微软雅黑", 12F, FontStyle.Bold);
            lblPermissionTitle.ForeColor = Color.Black;
            lblPermissionTitle.Location = new Point(rightStartX, margin);
            lblPermissionTitle.Size = new Size(350, 25);
            lblPermissionTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            this.Controls.Add(lblPermissionTitle);

            // 2. 全选复选框
            chkSelectAll = new CheckBox();
            chkSelectAll.Text = "全选/取消全选";
            chkSelectAll.Font = new Font("微软雅黑", 10F);
            chkSelectAll.Location = new Point(rightStartX, 45);
            chkSelectAll.Size = new Size(150, 25);
            chkSelectAll.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            chkSelectAll.Cursor = Cursors.Hand;
            this.Controls.Add(chkSelectAll);

            // 3. 保存权限分配按钮
            btnSavePermission = new Button();
            btnSavePermission.Text = "保存权限分配";
            btnSavePermission.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            btnSavePermission.ForeColor = Color.White;
            btnSavePermission.BackColor = Color.FromArgb(0, 122, 204);
            btnSavePermission.FlatStyle = FlatStyle.Flat;
            btnSavePermission.FlatAppearance.BorderSize = 0;
            btnSavePermission.Location = new Point(rightStartX + 350, 40);
            btnSavePermission.Size = new Size(160, 38);
            btnSavePermission.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnSavePermission.Cursor = Cursors.Hand;
            btnSavePermission.Enabled = false; // 默认禁用，选中角色后启用
            this.Controls.Add(btnSavePermission);

            // 4. 权限树形控件
            tvPermission = new TreeView();
            tvPermission.Location = new Point(rightStartX, 88);
            tvPermission.Size = new Size(rightWidth, formHeight - 135);
            tvPermission.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            tvPermission.Font = new Font("微软雅黑", 10F);
            tvPermission.BackColor = Color.White;
            tvPermission.BorderStyle = BorderStyle.FixedSingle;
            tvPermission.CheckBoxes = true;
            tvPermission.ShowLines = true;
            tvPermission.ShowPlusMinus = true;
            tvPermission.ShowRootLines = true;
            tvPermission.FullRowSelect = true;
            tvPermission.ItemHeight = 30;
            tvPermission.Indent = 25;
            tvPermission.Scrollable = true;
            this.Controls.Add(tvPermission);

            ResumeLayout(true);
            PerformLayout();
        }
        #endregion

        #region 【核心修复1】加载角色列表（从数据库读取真实数据，替换硬编码假数据）
        /// <summary>
        /// 从数据库t_role表读取所有角色，刷新列表
        /// </summary>
        private void InitRoleList()
        {
            dgvRoleList.Rows.Clear();
            // 从BLL层读取数据库真实数据
            List<Role> roleList = bllRole.GetAllRoleList();

            // 绑定到表格
            foreach (Role role in roleList)
            {
                dgvRoleList.Rows.Add(
                    role.role_id,
                    role.role_code,
                    role.role_name,
                    role.role_desc,
                    role.create_time.ToString("yyyy-MM-dd HH:mm") // 统一时间格式，避免过长
                );
            }

            // 清空选中状态
            dgvRoleList.ClearSelection();
            _currentSelectedRoleId = 0;
            btnSavePermission.Enabled = false;
            ClearPermissionCheck();
        }
        #endregion

        #region 【核心修复2】加载权限树（从数据库t_permission表读取，绑定permission_id）
        /// <summary>
        /// 从数据库t_permission表读取权限，构建树形结构，绑定permission_id到节点Tag
        /// </summary>
        private void InitPermissionTree()
        {
            tvPermission.Nodes.Clear();
            // 从BLL层读取数据库真实权限数据
            List<Permission> permissionList = bllPermission.GetAllPermissionList();

            if (permissionList == null || permissionList.Count == 0)
            {
                MessageBox.Show("权限表无数据，请先执行SQL初始化t_permission表的权限数据！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 按permission_type分组构建树形结构
            var groupList = permissionList.GroupBy(p => p.permission_type).ToList();
            foreach (var group in groupList)
            {
                TreeNode parentNode = new TreeNode(group.Key);
                parentNode.Tag = "parent";

                // 子节点绑定permission_id到Tag（核心：保存权限时读取）
                foreach (Permission permission in group)
                {
                    TreeNode childNode = new TreeNode(permission.permission_name);
                    childNode.Tag = permission.permission_id; // 绑定权限主键ID
                    parentNode.Nodes.Add(childNode);
                }
                tvPermission.Nodes.Add(parentNode);
            }
            tvPermission.ExpandAll(); // 展开所有节点
        }
        #endregion

        #region 【核心修复3】选中角色后，加载该角色已分配的权限
        /// <summary>
        /// 加载指定角色的已有权限，自动勾选对应节点
        /// </summary>
        private void LoadRolePermission(int roleId)
        {
            ClearPermissionCheck();
            // 从数据库获取该角色已分配的权限ID列表
            List<int> permissionIdList = bllRolePermission.GetPermissionIdListByRoleId(roleId);
            if (permissionIdList == null || permissionIdList.Count == 0) return;

            // 遍历权限树，勾选对应权限
            tvPermission.BeginUpdate();
            foreach (TreeNode parentNode in tvPermission.Nodes)
            {
                foreach (TreeNode childNode in parentNode.Nodes)
                {
                    if (childNode.Tag is int permissionId && permissionIdList.Contains(permissionId))
                    {
                        childNode.Checked = true;
                    }
                }
                // 更新父节点勾选状态
                UpdateParentNodeCheckStatus(parentNode);
            }
            tvPermission.EndUpdate();
            // 更新全选复选框状态
            UpdateSelectAllCheckStatus();
        }
        #endregion

        #region 【核心修复4】完整实现保存权限分配业务逻辑
        /// <summary>
        /// 保存权限分配按钮点击事件（完整持久化逻辑）
        /// </summary>
        private void BtnSavePermission_Click(object sender, EventArgs e)
        {
            // 1. 校验是否选中角色
            if (_currentSelectedRoleId <= 0)
            {
                MessageBox.Show("请先在左侧选中要分配权限的角色！", "操作提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. 收集所有勾选的权限ID
            List<int> checkedPermissionIdList = new List<int>();
            foreach (TreeNode parentNode in tvPermission.Nodes)
            {
                foreach (TreeNode childNode in parentNode.Nodes)
                {
                    if (childNode.Checked && childNode.Tag is int permissionId)
                    {
                        checkedPermissionIdList.Add(permissionId);
                    }
                }
            }

            // 3. 二次确认
            string roleName = dgvRoleList.SelectedRows[0].Cells["role_name"].Value.ToString();
            if (MessageBox.Show($"确定要为角色【{roleName}】分配 {checkedPermissionIdList.Count} 项权限吗？\n保存后将覆盖该角色原有所有权限",
                "权限分配确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            // 4. 调用业务层，事务化保存权限
            string result = bllRolePermission.SaveRolePermission(
                roleId: _currentSelectedRoleId,
                permissionIdList: checkedPermissionIdList,
                operateUserId: Program.LoginUser.user_id
            );

            // 5. 处理结果
            if (result == "ok")
            {
                if (this.MdiParent is FrmAdminMain mainForm)
                {
                    mainForm.SetStatusTip($"操作提示：角色【{roleName}】权限分配保存成功");
                }
                MessageBox.Show("权限分配保存成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // 重新加载权限，刷新勾选状态
                LoadRolePermission(_currentSelectedRoleId);
            }
            else
            {
                MessageBox.Show(result, "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 按钮事件：新增/修改/删除角色
        /// <summary>
        /// 新增角色按钮点击事件
        /// </summary>
        private void BtnAddRole_Click(object sender, EventArgs e)
        {
            FrmRoleEdit frmEdit = new FrmRoleEdit(0);
            if (frmEdit.ShowDialog() == DialogResult.OK)
            {
                InitRoleList();
                // 正确调用主窗体的公共方法
                if (this.MdiParent is FrmAdminMain mainForm)
                {
                    mainForm.SetStatusTip("操作提示：新增角色成功");
                }
                MessageBox.Show("新增角色成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 修改角色按钮点击事件
        /// </summary>
        private void BtnEditRole_Click(object sender, EventArgs e)
        {
            if (_currentSelectedRoleId <= 0)
            {
                MessageBox.Show("请先选择要修改的角色！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            FrmRoleEdit frmEdit = new FrmRoleEdit(_currentSelectedRoleId);
            if (frmEdit.ShowDialog() == DialogResult.OK)
            {
                InitRoleList();
                if (this.MdiParent is FrmAdminMain mainForm)
                {
                    mainForm.SetStatusTip("操作提示：修改角色成功");
                }
                MessageBox.Show("修改角色成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 删除角色按钮点击事件
        /// </summary>
        private void BtnDeleteRole_Click(object sender, EventArgs e)
        {
            if (_currentSelectedRoleId <= 0)
            {
                MessageBox.Show("请先选择要删除的角色！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 校验角色是否被用户关联
            if (bllRole.CheckRoleHasUser(_currentSelectedRoleId))
            {
                MessageBox.Show("该角色已被用户关联，无法删除！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (MessageBox.Show("确定要删除该角色吗？删除后不可恢复！", "确认删除",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            // 执行删除
            string result = bllRole.DeleteRole(_currentSelectedRoleId, Program.LoginUser.user_id);
            if (result == "ok")
            {
                InitRoleList();
                if (this.MdiParent is FrmAdminMain mainForm)
                {
                    mainForm.SetStatusTip("操作提示：删除角色成功");
                }
                MessageBox.Show("删除角色成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(result, "删除失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 权限树联动逻辑（父子节点勾选联动）
        // 角色列表选中事件
        private void DgvRoleList_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvRoleList.SelectedRows.Count == 0) return;
            DataGridViewRow row = dgvRoleList.SelectedRows[0];
            _currentSelectedRoleId = Convert.ToInt32(row.Cells["role_id"].Value);

            // 加载该角色的已有权限
            LoadRolePermission(_currentSelectedRoleId);
            // 启用保存按钮
            btnSavePermission.Enabled = true;
        }

        // 权限树节点勾选联动
        private void TvPermission_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.Unknown) return;

            // 父节点勾选→子节点全选
            if (e.Node.Nodes.Count > 0)
            {
                foreach (TreeNode childNode in e.Node.Nodes)
                {
                    childNode.Checked = e.Node.Checked;
                }
            }
            // 子节点勾选变化→更新父节点
            else
            {
                UpdateParentNodeCheckStatus(e.Node.Parent);
            }
            // 更新全选复选框状态
            UpdateSelectAllCheckStatus();
        }

        // 更新父节点勾选状态
        private void UpdateParentNodeCheckStatus(TreeNode parentNode)
        {
            if (parentNode == null) return;
            bool allChecked = true;
            foreach (TreeNode childNode in parentNode.Nodes)
            {
                if (!childNode.Checked)
                {
                    allChecked = false;
                    break;
                }
            }
            parentNode.Checked = allChecked;
        }

        // 全选/取消全选
        private void ChkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            tvPermission.BeginUpdate();
            foreach (TreeNode parentNode in tvPermission.Nodes)
            {
                parentNode.Checked = chkSelectAll.Checked;
                foreach (TreeNode childNode in parentNode.Nodes)
                {
                    childNode.Checked = chkSelectAll.Checked;
                }
            }
            tvPermission.EndUpdate();
        }

        // 更新全选复选框状态
        private void UpdateSelectAllCheckStatus()
        {
            bool allChecked = true;
            foreach (TreeNode parentNode in tvPermission.Nodes)
            {
                if (!parentNode.Checked)
                {
                    allChecked = false;
                    break;
                }
            }
            chkSelectAll.CheckedChanged -= ChkSelectAll_CheckedChanged;
            chkSelectAll.Checked = allChecked;
            chkSelectAll.CheckedChanged += ChkSelectAll_CheckedChanged;
        }

        // 清空所有勾选
        private void ClearPermissionCheck()
        {
            tvPermission.BeginUpdate();
            foreach (TreeNode parentNode in tvPermission.Nodes)
            {
                parentNode.Checked = false;
                foreach (TreeNode childNode in parentNode.Nodes)
                {
                    childNode.Checked = false;
                }
            }
            tvPermission.EndUpdate();
            chkSelectAll.Checked = false;
        }
        #endregion
    }

    // 双缓冲DataGridView辅助类
    public class DoubleBufferedDataGridView : DataGridView
    {
        public DoubleBufferedDataGridView()
        {
            this.DoubleBuffered = true;
        }
    }
}