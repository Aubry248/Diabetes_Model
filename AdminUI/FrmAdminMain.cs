using BLL;
using Model;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace AdminUI
{
    public partial class FrmAdminMain : Form
    {
        private Panel pnlTop;
        private Label lblSystemTitle;
        private Label lblLoginUser;
        private Label lblLoginTime;
        private Button btnLogout;
        private Panel pnlLeft;
        private TreeView tvMenu;
        private ToolStripStatusLabel tsslOperateTip;
        private ToolStripStatusLabel tsslSystemTime;
        private ToolStripStatusLabel tsslVersion;
        private Timer timerTime;
        private MdiClient mdiClient;

        private readonly Color _themePrimary = Color.FromArgb(0, 122, 204);
        private readonly Color _themeDanger = Color.FromArgb(220, 53, 69);
        private readonly Color _menuBg = Color.FromArgb(248, 250, 252);
        private readonly Color _menuHover = Color.FromArgb(229, 243, 255);
        private readonly Color _menuSelected = Color.FromArgb(0, 122, 204);

        public FrmAdminMain()
        {
            InitializeComponent();
            Text = "糖尿病患者综合健康管理系统 - 管理员后台";
            IsMdiContainer = true;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1280, 720);
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            Font = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(245, 247, 250);

            InitMdiClientCore();
            InitAllControls();
            InitMenuTree();
            InitLoginUserInfo();
            StartSystemTimer();

            Resize += FrmAdminMain_Resize;
            Load += FrmAdminMain_Load;
        }

        private void InitMdiClientCore()
        {
            foreach (Control ctl in Controls)
            {
                if (ctl is MdiClient client)
                {
                    mdiClient = client;
                    break;
                }
            }

            if (mdiClient == null)
            {
                mdiClient = new MdiClient();
                Controls.Add(mdiClient);
            }

            mdiClient.Dock = DockStyle.None;
            mdiClient.BackColor = Color.FromArgb(245, 247, 250);
            mdiClient.SendToBack();
        }

        private void InitAllControls()
        {
            InitBottomStatusStrip();
            InitTopPanel();
            InitLeftMenuPanel();
            InitTimer();
            ApplyUnifiedStyles(this);
        }

        private void InitBottomStatusStrip()
        {
            if (statusStripMain == null)
            {
                statusStripMain = new StatusStrip();
                Controls.Add(statusStripMain);
            }

            statusStripMain.Dock = DockStyle.Bottom;
            statusStripMain.Font = new Font("微软雅黑", 9F);
            statusStripMain.BackColor = Color.FromArgb(245, 247, 250);
            statusStripMain.Height = 24;
            statusStripMain.SizingGrip = false;
            statusStripMain.Items.Clear();

            tsslOperateTip = lblStatusTip ?? new ToolStripStatusLabel();
            tsslOperateTip.Spring = true;
            tsslOperateTip.TextAlign = ContentAlignment.MiddleLeft;
            tsslOperateTip.ForeColor = Color.FromArgb(64, 64, 64);
            tsslOperateTip.Text = "操作提示：欢迎使用糖尿病健康管理系统";

            tsslSystemTime = new ToolStripStatusLabel { ForeColor = Color.FromArgb(64, 64, 64) };
            tsslVersion = new ToolStripStatusLabel { Text = "版本号：V1.0.0", ForeColor = Color.FromArgb(64, 64, 64) };

            statusStripMain.Items.AddRange(new ToolStripItem[]
            {
                tsslOperateTip,
                new ToolStripStatusLabel { Text = "|", Margin = new Padding(5, 0, 5, 0) },
                tsslSystemTime,
                new ToolStripStatusLabel { Text = "|", Margin = new Padding(5, 0, 5, 0) },
                tsslVersion
            });
        }

        private void InitTopPanel()
        {
            if (pnlTop != null)
                Controls.Remove(pnlTop);

            pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 76,
                BackColor = _themePrimary,
                Padding = new Padding(20)
            };

            lblSystemTitle = new Label { Text = "糖尿病患者综合健康管理系统 - 管理员后台", Font = new Font("微软雅黑", 16F, FontStyle.Bold), ForeColor = Color.White, AutoSize = true };
            btnLogout = new Button { Text = "安全退出", Font = new Font("微软雅黑", 10.5F), ForeColor = Color.White, BackColor = _themeDanger, FlatStyle = FlatStyle.Flat, Size = new Size(120, 40), Cursor = Cursors.Hand };
            lblLoginTime = new Label { Text = "登录时间：", Font = new Font("微软雅黑", 10.5F), ForeColor = Color.White, AutoSize = true };
            lblLoginUser = new Label { Text = "当前用户：", Font = new Font("微软雅黑", 10.5F), ForeColor = Color.White, AutoSize = true };

            btnLogout.Click += BtnLogout_Click;
            pnlTop.Controls.AddRange(new Control[] { lblSystemTitle, lblLoginUser, lblLoginTime, btnLogout });
            pnlTop.Resize += (s, e) => RefreshTopControlsPosition();
            Controls.Add(pnlTop);
            pnlTop.BringToFront();
        }

        private void InitLeftMenuPanel()
        {
            if (pnlLeft != null)
                Controls.Remove(pnlLeft);

            pnlLeft = new Panel
            {
                Dock = DockStyle.Left,
                Width = 232,
                BackColor = _menuBg,
                Padding = new Padding(8)
            };

            tvMenu = new TreeView
            {
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 10.5F),
                BackColor = _menuBg,
                ForeColor = Color.FromArgb(51, 51, 51),
                BorderStyle = BorderStyle.None,
                ShowLines = false,
                ShowPlusMinus = true,
                ShowRootLines = false,
                FullRowSelect = true,
                HideSelection = false,
                ItemHeight = 44,
                Indent = 20,
                Scrollable = true,
                DrawMode = TreeViewDrawMode.OwnerDrawAll
            };
            tvMenu.AfterExpand += TvMenu_AfterExpand;
            tvMenu.AfterCollapse += TvMenu_AfterCollapse;
            tvMenu.DrawNode += TvMenu_DrawNode;
            tvMenu.NodeMouseClick += TvMenu_NodeMouseClick;

            pnlLeft.Controls.Add(tvMenu);
            Controls.Add(pnlLeft);
            pnlLeft.BringToFront();
        }

        private void InitTimer()
        {
            timerTime = new Timer { Interval = 1000 };
            timerTime.Tick += TimerTime_Tick;
        }

        private void TvMenu_AfterExpand(object sender, TreeViewEventArgs e)
        {
            tvMenu?.Invalidate();
        }

        private void TvMenu_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            tvMenu?.Invalidate();
        }

        private void TvMenu_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            TreeNode node = e.Node;
            Rectangle bounds = e.Bounds;
            Rectangle rowRect = new Rectangle(0, bounds.Top, tvMenu.ClientSize.Width, bounds.Height);

            using (SolidBrush bgBrush = new SolidBrush(tvMenu.BackColor))
                e.Graphics.FillRectangle(bgBrush, rowRect);

            int textLeft = 28;
            int textTop = bounds.Top + 8;

            if (e.State.HasFlag(TreeNodeStates.Selected))
            {
                Rectangle bgRect = new Rectangle(2, bounds.Top + 2, Math.Max(0, tvMenu.ClientSize.Width - 4), Math.Max(0, bounds.Height - 4));
                using (SolidBrush brush = new SolidBrush(_menuSelected))
                    e.Graphics.FillRoundedRectangle(brush, bgRect, 4);
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                    e.Graphics.DrawString(node.Text, tvMenu.Font, textBrush, textLeft, textTop);
            }
            else if (e.State.HasFlag(TreeNodeStates.Hot))
            {
                Rectangle bgRect = new Rectangle(2, bounds.Top + 2, Math.Max(0, tvMenu.ClientSize.Width - 4), Math.Max(0, bounds.Height - 4));
                using (SolidBrush brush = new SolidBrush(_menuHover))
                    e.Graphics.FillRoundedRectangle(brush, bgRect, 4);
                using (SolidBrush textBrush = new SolidBrush(_themePrimary))
                    e.Graphics.DrawString(node.Text, tvMenu.Font, textBrush, textLeft, textTop);
            }
            else
            {
                Color textColor = node.Nodes.Count > 0 ? Color.FromArgb(20, 20, 20) : Color.FromArgb(60, 60, 60);
                Font font = node.Nodes.Count > 0 ? new Font(tvMenu.Font, FontStyle.Bold) : tvMenu.Font;
                using (SolidBrush textBrush = new SolidBrush(textColor))
                    e.Graphics.DrawString(node.Text, font, textBrush, textLeft, textTop);
                if (node.Nodes.Count > 0)
                    font.Dispose();
            }

            if (node.Nodes.Count > 0)
            {
                string symbol = node.IsExpanded ? "−" : "+";
                int symbolLeft = bounds.Left + 8;
                using (Font symbolFont = new Font("微软雅黑", 12F, FontStyle.Bold))
                using (SolidBrush symbolBrush = new SolidBrush(Color.FromArgb(80, 80, 80)))
                    e.Graphics.DrawString(symbol, symbolFont, symbolBrush, symbolLeft, bounds.Top + 7);
            }
        }

        private void InitMenuTree()
        {
            tvMenu.BeginUpdate();
            tvMenu.Nodes.Clear();

            TreeNode userManage = new TreeNode("用户管理");
            userManage.Nodes.Add(new TreeNode("用户信息管理") { Tag = "FrmUserInfoManage" });
            userManage.Nodes.Add(new TreeNode("角色权限管理") { Tag = "FrmRolePermissionManage" });

            TreeNode dataManage = new TreeNode("系统数据管理");
            dataManage.Nodes.Add(new TreeNode("食物库管理") { Tag = "FrmFoodLibraryManager" });
            dataManage.Nodes.Add(new TreeNode("运动库管理") { Tag = "FrmExerciseLibraryManager" });
            dataManage.Nodes.Add(new TreeNode("药物库管理") { Tag = "FrmDrugLibraryManager" });
            dataManage.Nodes.Add(new TreeNode("字典管理") { Tag = "FrmDictionaryManager" });

            TreeNode opsManage = new TreeNode("系统运维管理");
            opsManage.Nodes.Add(new TreeNode("日志查看") { Tag = "FrmAccessLog" });
            opsManage.Nodes.Add(new TreeNode("数据备份还原") { Tag = "FrmBackupRestore" });
            opsManage.Nodes.Add(new TreeNode("系统配置") { Tag = "FrmSystemConfig" });

            TreeNode personal = new TreeNode("个人中心");
            personal.Nodes.Add(new TreeNode("密码修改") { Tag = "FrmChangePwd" });
            personal.Nodes.Add(new TreeNode("个人信息维护") { Tag = "FrmMyInfo" });

            tvMenu.Nodes.AddRange(new[] { userManage, dataManage, opsManage, personal });
            tvMenu.Nodes[0].Expand();
            tvMenu.EndUpdate();
        }

        private void TvMenu_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (!GlobalDebounce.Check()) return;
            if (e.Node == null || string.IsNullOrEmpty(e.Node.Tag?.ToString()) || e.Node.Nodes.Count > 0)
                return;

            string formName = e.Node.Tag.ToString();
            tvMenu.SelectedNode = e.Node;
            tvMenu.Invalidate();
            tvMenu.Update();
            BeginInvoke(new Action(() => OpenChildForm(formName, e.Node.Text)));
        }

        private void OpenChildForm(string formName, string formText)
        {
            foreach (Form childForm in MdiChildren)
            {
                if (childForm.Name == formName && !childForm.IsDisposed)
                {
                    childForm.Activate();
                    childForm.WindowState = FormWindowState.Maximized;
                    childForm.BringToFront();
                    SetStatusTip($"操作提示：已切换到【{formText}】");
                    return;
                }
            }

            try
            {
                Type formType = Type.GetType($"AdminUI.{formName}") ?? Assembly.GetExecutingAssembly().GetType($"AdminUI.{formName}");
                if (formType == null)
                {
                    MessageBox.Show($"功能【{formText}】不存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Form childForm = (Form)Activator.CreateInstance(formType);
                childForm.Name = formName;
                childForm.Text = formText;
                childForm.MdiParent = this;
                childForm.WindowState = FormWindowState.Maximized;
                childForm.AutoScaleMode = AutoScaleMode.Dpi;
                childForm.Show();
                childForm.BringToFront();
                ApplyUnifiedStyles(childForm);
                childForm.BeginInvoke(new Action(() => ApplyUnifiedStyles(childForm)));
                SetStatusTip($"操作提示：已打开【{formText}】");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrmAdminMain_Resize(object sender, EventArgs e)
        {
            RefreshMdiClientBounds();
            RefreshTopControlsPosition();
        }

        private void FrmAdminMain_Load(object sender, EventArgs e)
        {
            RefreshTopControlsPosition();
            RefreshMdiClientBounds();

            TreeNode defaultLeafNode = GetFirstLeafNode(tvMenu?.Nodes);
            if (defaultLeafNode != null && !string.IsNullOrEmpty(defaultLeafNode.Tag?.ToString()))
            {
                tvMenu.SelectedNode = defaultLeafNode;
                tvMenu.Invalidate();
                tvMenu.Update();
                BeginInvoke(new Action(() => OpenChildForm(defaultLeafNode.Tag.ToString(), defaultLeafNode.Text)));
            }
        }

        private TreeNode GetFirstLeafNode(TreeNodeCollection nodes)
        {
            if (nodes == null || nodes.Count == 0)
                return null;

            foreach (TreeNode node in nodes)
            {
                if (node.Nodes.Count == 0 && !string.IsNullOrEmpty(node.Tag?.ToString()))
                    return node;

                TreeNode childLeaf = GetFirstLeafNode(node.Nodes);
                if (childLeaf != null)
                    return childLeaf;
            }

            return null;
        }

        private void RefreshMdiClientBounds()
        {
            if (mdiClient == null || pnlTop == null || pnlLeft == null || statusStripMain == null)
                return;

            int x = pnlLeft.Width;
            int y = pnlTop.Height;
            int width = ClientSize.Width - pnlLeft.Width;
            int height = ClientSize.Height - pnlTop.Height - statusStripMain.Height;
            mdiClient.Bounds = new Rectangle(x, y, Math.Max(width, 0), Math.Max(height, 0));
            mdiClient.SendToBack();
        }

        private void InitLoginUserInfo()
        {
            if (Program.LoginUser != null)
            {
                lblLoginUser.Text = $"当前用户：{Program.LoginUser.user_name}";
                lblLoginTime.Text = $"登录时间：{DateTime.Now:yyyy-MM-dd HH:mm}";
                RefreshTopControlsPosition();
            }
        }

        private void StartSystemTimer()
        {
            timerTime.Start();
            TimerTime_Tick(null, null);
        }

        private void TimerTime_Tick(object sender, EventArgs e)
        {
            if (tsslSystemTime != null)
                tsslSystemTime.Text = $"系统时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }

        public void BtnLogout_Click(object sender, EventArgs e)
        {
            if (!GlobalDebounce.Check()) return;

            if (MessageBox.Show("确定退出系统？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                timerTime.Stop();
                Program.LoginUser = null;

                foreach (Form child in MdiChildren)
                    child.Close();

                FrmAdminLogin login = Application.OpenForms["FrmAdminLogin"] as FrmAdminLogin;
                if (login != null) login.Show();

                Close();
            }
        }

        private void FrmAdminMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            timerTime?.Stop();
            Application.Exit();
        }

        private void RefreshTopControlsPosition()
        {
            if (pnlTop == null || btnLogout == null) return;

            int rightMargin = 20;
            int spacing = 30;
            btnLogout.Location = new Point(pnlTop.Width - btnLogout.Width - rightMargin, (pnlTop.Height - btnLogout.Height) / 2);
            lblLoginTime.Location = new Point(btnLogout.Left - lblLoginTime.Width - spacing, (pnlTop.Height - lblLoginTime.Height) / 2);
            lblLoginUser.Location = new Point(lblLoginTime.Left - lblLoginUser.Width - spacing, (pnlTop.Height - lblLoginUser.Height) / 2);
            lblSystemTitle.Location = new Point(20, (pnlTop.Height - lblSystemTitle.Height) / 2);
        }

        public void SetStatusTip(string tipText)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(SetStatusTip), tipText);
                return;
            }

            if (tsslOperateTip != null)
                tsslOperateTip.Text = tipText;
        }

        private void ApplyUnifiedStyles(Control root)
        {
            if (root == null)
                return;

            if (root is Form form)
                form.BackColor = Color.FromArgb(245, 247, 250);

            ApplyUnifiedStylesRecursive(root);
        }

        private void ApplyUnifiedStylesRecursive(Control control)
        {
            if (control is Button button)
                StyleButton(button);
            else if (control is TextBoxBase textBox)
                StyleTextBox(textBox);
            else if (control is ComboBox comboBox)
                StyleComboBox(comboBox);
            else if (control is DateTimePicker dateTimePicker)
                StyleDateTimePicker(dateTimePicker);
            else if (control is DataGridView dataGridView)
                StyleDataGridView(dataGridView);
            else if (control is GroupBox groupBox)
                StyleGroupBox(groupBox);
            else if (control is TabControl tabControl)
                StyleTabControl(tabControl);

            foreach (Control child in control.Controls)
                ApplyUnifiedStylesRecursive(child);
        }

        private void StyleButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("微软雅黑", 9.5F, button.Font.Style);
            button.Height = Math.Max(button.Height, 34);
            if (button.Margin.All <= 3)
                button.Margin = new Padding(6);
            if (button.BackColor == default(Color) || button.BackColor == SystemColors.Control)
            {
                button.BackColor = _themePrimary;
                button.ForeColor = Color.White;
            }
        }

        private void StyleTextBox(TextBoxBase textBox)
        {
            textBox.Font = new Font("微软雅黑", 9.5F, textBox.Font.Style);
            textBox.Margin = textBox.Margin.All <= 3 ? new Padding(6) : textBox.Margin;
            if (textBox is TextBox plainTextBox && !plainTextBox.Multiline)
                plainTextBox.MinimumSize = new Size(0, 30);
        }

        private void StyleComboBox(ComboBox comboBox)
        {
            comboBox.Font = new Font("微软雅黑", 9.5F, comboBox.Font.Style);
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.IntegralHeight = false;
            comboBox.Height = Math.Max(comboBox.Height, 32);
            comboBox.Margin = comboBox.Margin.All <= 3 ? new Padding(6) : comboBox.Margin;
        }

        private void StyleDateTimePicker(DateTimePicker dateTimePicker)
        {
            dateTimePicker.Font = new Font("微软雅黑", 9.5F, dateTimePicker.Font.Style);
            dateTimePicker.Height = Math.Max(dateTimePicker.Height, 32);
            dateTimePicker.Margin = dateTimePicker.Margin.All <= 3 ? new Padding(6) : dateTimePicker.Margin;
        }

        private void StyleDataGridView(DataGridView dataGridView)
        {
            dataGridView.BackgroundColor = Color.White;
            dataGridView.BorderStyle = BorderStyle.None;
            dataGridView.GridColor = Color.FromArgb(226, 232, 240);
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.ColumnHeadersHeight = Math.Max(dataGridView.ColumnHeadersHeight, 40);
            dataGridView.RowTemplate.Height = Math.Max(dataGridView.RowTemplate.Height, 34);
            dataGridView.DefaultCellStyle.Font = new Font("微软雅黑", 9F);
            dataGridView.DefaultCellStyle.SelectionBackColor = _themePrimary;
            dataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", 9F, FontStyle.Bold);
            dataGridView.ColumnHeadersDefaultCellStyle.BackColor = _themePrimary;
            dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        }

        private void StyleGroupBox(GroupBox groupBox)
        {
            groupBox.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            groupBox.Padding = new Padding(Math.Max(groupBox.Padding.Left, 12), Math.Max(groupBox.Padding.Top, 12), Math.Max(groupBox.Padding.Right, 12), Math.Max(groupBox.Padding.Bottom, 12));
        }

        private void StyleTabControl(TabControl tabControl)
        {
            tabControl.Padding = new Point(Math.Max(tabControl.Padding.X, 18), Math.Max(tabControl.Padding.Y, 8));
            tabControl.ItemSize = new Size(Math.Max(tabControl.ItemSize.Width, 96), Math.Max(tabControl.ItemSize.Height, 32));
        }
    }

    // 圆角扩展
    public static class GraphicsExtension
    {
        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int radius)
        {
            using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90);
                path.AddArc(bounds.Right - radius, bounds.Y, radius, radius, 270, 90);
                path.AddArc(bounds.Right - radius, bounds.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(bounds.X, bounds.Bottom - radius, radius, radius, 90, 90);
                path.CloseAllFigures();
                graphics.FillPath(brush, path);
            }
        }
        // 在 FrmAdminMain.cs 中添加
        public static void Logout()
        {
            // 这里放原来 BtnLogout_Click 里的登出代码
            // 比如：清空用户信息、跳转到登录页等
        }
    }

}