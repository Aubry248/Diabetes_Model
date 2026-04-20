using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Model;

namespace DoctorUI
{
    public partial class FrmDoctorMain : Form
    {
        // ========== 【新增：静态单例实例，解决子窗体找不到主窗体的问题】 ==========
        public static FrmDoctorMain Instance { get; private set; }

        // ==============================================
        // 【可调节参数区】
        // ==============================================
        private readonly Color _themePrimary = Color.FromArgb(0, 122, 204);
        private readonly Color _menuBg = Color.FromArgb(248, 250, 252);
        private readonly Color _menuHover = Color.FromArgb(230, 240, 255);
        private readonly int _topBarHeight = 76;
        private readonly int _leftMenuWidth = 232;

        // 全局控件
        private TreeView _tvMenu;
        public Panel _panelMain;
        private Label _lblTitle;
        private Button _btnLogout;
        public int CurrentSelectedPatientId { get; set; } = 0;
        public string CurrentSelectedPatientName { get; set; } = string.Empty;

        public FrmDoctorMain()
        {
            InitializeComponent();
            // ========== 【新增：单例实例赋值，必须放在构造函数最前面】 ==========
            Instance = this;

            InitializeBaseForm();
            CreateAllControls();
            BindEvents();
        }

        #region 窗体基础属性
        private void InitializeBaseForm()
        {
            Text = "糖尿病健康管理系统 - 公卫医生端";
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1280, 720);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("微软雅黑", 9F);
            BackColor = Color.White;
            IsMdiContainer = false;
            AutoScaleMode = AutoScaleMode.Font;
            DoubleBuffered = true;
        }
        #endregion

        #region 生成所有控件
        private void CreateAllControls()
        {
            SuspendLayout();
            Controls.Clear();
            // 1. 顶部状态栏
            Panel pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = _topBarHeight,
                BackColor = _themePrimary,
                Name = "pnlTop"
            };
            _lblTitle = new Label
            {
                Text = $"糖尿病健康综合管理系统 - 医生端，欢迎您，{Program.LoginUser?.user_name}",
                Font = new Font("微软雅黑", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Size = new Size(800, _topBarHeight),
                Name = "lblTitle"
            };
            _btnLogout = new Button
            {
                Text = "安全退出",
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Right,
                Size = new Size(120, 40),
                Margin = new Padding(0, 15, 20, 15),
                Name = "btnLogout"
            };
            _btnLogout.FlatAppearance.BorderSize = 0;
            pnlTop.Controls.Add(_lblTitle);
            pnlTop.Controls.Add(_btnLogout);
            Controls.Add(pnlTop);
            // 2. 左侧菜单
            Panel pnlLeft = new Panel
            {
                Dock = DockStyle.Left,
                Width = _leftMenuWidth,
                BackColor = _menuBg,
                Name = "pnlLeft"
            };
            _tvMenu = new TreeView
            {
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 11F),
                BorderStyle = BorderStyle.None,
                FullRowSelect = true,
                ItemHeight = 44,
                ShowLines = false,
                ShowPlusMinus = false,
                HideSelection = false,
                BackColor = _menuBg,
                Padding = new Padding(10, 5, 0, 0),
                Name = "tvMenu"
            };
            // 菜单节点（Tag必须和窗体类名完全一致）
            _tvMenu.Nodes.AddRange(new[]
            {
                new TreeNode("首页") { Tag = "FrmDoctorHome", BackColor = _menuBg },
                new TreeNode("患者档案管理") { Tag = "FrmPatientRecordManage", BackColor = _menuBg },
                new TreeNode("健康评估") { Tag = "FrmHealthAssessment", BackColor = _menuBg },
                new TreeNode("干预方案制定") { Tag = "FrmInterventionPlan", BackColor = _menuBg },
                new TreeNode("用药调整") { Tag = "FrmMedicationAdjustment", BackColor = _menuBg },
                new TreeNode("随访管理") { Tag = "FrmFollowUpManage", BackColor = _menuBg },
                new TreeNode("异常数据处理") { Tag = "FrmAbnormalData", BackColor = _menuBg },
                new TreeNode("干预效果评估") { Tag = "FrmEffectEvaluation", BackColor = _menuBg },
                new TreeNode("个人中心") { Tag = "FrmDoctorMyProfile", BackColor = _menuBg }
            });
            // 菜单样式
            _tvMenu.DrawMode = TreeViewDrawMode.OwnerDrawText;
            _tvMenu.DrawNode += (s, e) =>
            {
                e.DrawDefault = false;
                Rectangle rowRect = new Rectangle(0, e.Bounds.Top, _tvMenu.ClientSize.Width, e.Bounds.Height);
                Rectangle textRect = new Rectangle(12, e.Bounds.Top, Math.Max(0, _tvMenu.ClientSize.Width - 12), e.Bounds.Height);
                if ((e.State & TreeNodeStates.Selected) != 0)
                {
                    using (var brush = new SolidBrush(_themePrimary))
                    {
                        e.Graphics.FillRectangle(brush, rowRect);
                    }
                    TextRenderer.DrawText(e.Graphics, e.Node.Text, _tvMenu.Font, textRect, Color.White, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                }
                else if ((e.State & TreeNodeStates.Hot) != 0)
                {
                    using (var brush = new SolidBrush(_menuHover))
                    {
                        e.Graphics.FillRectangle(brush, rowRect);
                    }
                    TextRenderer.DrawText(e.Graphics, e.Node.Text, _tvMenu.Font, textRect, _themePrimary, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                }
                else
                {
                    using (var brush = new SolidBrush(_menuBg))
                    {
                        e.Graphics.FillRectangle(brush, rowRect);
                    }
                    TextRenderer.DrawText(e.Graphics, e.Node.Text, _tvMenu.Font, textRect, Color.Black, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                }
            };
            pnlLeft.Controls.Add(_tvMenu);
            Controls.Add(pnlLeft);
            // 3. 主容器
            _panelMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250),
                Name = "panelMain",
                BorderStyle = BorderStyle.None,
                Padding = new Padding(12)
            };
            Controls.Add(_panelMain);
            ApplyUnifiedStyles(this);
            ResumeLayout(true);
            PerformLayout();
        }
        #endregion

        #region 事件绑定
        private void BindEvents()
        {
            Load += FrmDoctorMain_Load;
            FormClosed += FrmDoctorMain_FormClosed;
            _tvMenu.NodeMouseClick += TvMenu_NodeMouseClick;
            _btnLogout.Click += (s, e) =>
            {
                if (MessageBox.Show("确定要退出登录吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Program.LoginUser = null;
                    new FrmDoctorLogin().Show();
                    Close();
                }
            };
        }
        #endregion

        #region 【新增：公开的子窗体打开方法，给首页待办跳转用，完全兼容原有菜单逻辑】
        /// <summary>
        /// 公开方法：打开子窗体（首页待办跳转专用）
        /// </summary>
        /// <param name="childForm">要打开的子窗体</param>
        public void OpenChildForm(Form childForm)
        {
            if (childForm == null) return;
            try
            {
                // 清空原有窗体
                _panelMain.Controls.Clear();
                // 配置子窗体属性，和菜单打开逻辑完全一致
                childForm.TopLevel = false;
                childForm.FormBorderStyle = FormBorderStyle.None;
                childForm.Dock = DockStyle.Fill;
                childForm.Visible = true;
                // 添加到主容器
                _panelMain.Controls.Add(childForm);
                childForm.BringToFront();
                childForm.Show();
                childForm.Focus();
                ApplyUnifiedStyles(childForm);
                childForm.BeginInvoke(new Action(() => ApplyUnifiedStyles(childForm)));

                // 同步选中左侧菜单
                string formName = childForm.GetType().Name;
                foreach (TreeNode node in _tvMenu.Nodes)
                {
                    if (node.Tag?.ToString() == formName)
                    {
                        _tvMenu.SelectedNode = node;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开功能失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 打开子窗体
        private void TvMenu_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == null || string.IsNullOrEmpty(e.Node.Tag?.ToString())) return;
            string formClassName = e.Node.Tag.ToString();

            _tvMenu.SelectedNode = e.Node;
            _tvMenu.Invalidate();
            _tvMenu.Update();

            BeginInvoke(new Action(() => OpenChildFormByName(formClassName, e.Node.Text)));
        }

        private void OpenChildFormByName(string formClassName, string nodeText)
        {
            _panelMain.Controls.Clear();
            try
            {
                Type formType = Assembly.GetExecutingAssembly().GetType($"DoctorUI.{formClassName}");
                if (formType == null)
                {
                    MessageBox.Show($"功能【{nodeText}】正在开发中！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                Form childForm = Activator.CreateInstance(formType) as Form;
                if (childForm == null) return;
                childForm.TopLevel = false;
                childForm.FormBorderStyle = FormBorderStyle.None;
                childForm.Dock = DockStyle.Fill;
                childForm.Visible = true;
                _panelMain.Controls.Add(childForm);
                childForm.Show();
                childForm.BringToFront();
                childForm.Focus();
                ApplyUnifiedStyles(childForm);
                childForm.BeginInvoke(new Action(() => ApplyUnifiedStyles(childForm)));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开功能失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

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
            else if (control is Chart chart)
                StyleChart(chart);
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

        private void StyleChart(Chart chart)
        {
            chart.BackColor = Color.White;
            foreach (ChartArea chartArea in chart.ChartAreas)
            {
                chartArea.BackColor = Color.White;
                chartArea.AxisX.LabelStyle.Font = new Font("微软雅黑", 8.5F);
                chartArea.AxisY.LabelStyle.Font = new Font("微软雅黑", 8.5F);
                chartArea.AxisX.TitleFont = new Font("微软雅黑", 9F, FontStyle.Bold);
                chartArea.AxisY.TitleFont = new Font("微软雅黑", 9F, FontStyle.Bold);
                chartArea.AxisX.LineColor = Color.FromArgb(148, 163, 184);
                chartArea.AxisY.LineColor = Color.FromArgb(148, 163, 184);
            }

            foreach (Legend legend in chart.Legends)
                legend.Font = new Font("微软雅黑", 8.5F);
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

        #region 窗体加载/关闭事件
        private void FrmDoctorMain_Load(object sender, EventArgs e)
        {
            if (_tvMenu?.Nodes.Count > 0)
            {
                TreeNode homeNode = _tvMenu.Nodes[0];
                _tvMenu.SelectedNode = homeNode;
                _tvMenu.Invalidate();
                _tvMenu.Update();
                BeginInvoke(new Action(() => OpenChildFormByName(homeNode.Tag?.ToString(), homeNode.Text)));
            }
            if (Program.LoginUser != null)
            {
                _lblTitle.Text = $"糖尿病健康综合管理系统 - 医生端，欢迎您，{Program.LoginUser.user_name}";
            }
        }
        private void FrmDoctorMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 主窗体关闭时，全量清空会话
            GlobalConfig.ClearSession();
            Application.Exit();
        }
        #endregion
    }
}