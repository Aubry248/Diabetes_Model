using BLL;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace PatientUI
{
    public partial class FrmPatientMain : Form
    {
        // 主题色
        private Color _themePrimary = Color.FromArgb(0, 122, 204);
        private Color _menuBg = Color.FromArgb(248, 250, 252);
        private Color _menuHover = Color.FromArgb(230, 240, 255);

        // 全局控件引用（必须确保Name正确，子窗体才能识别）
        private TreeView _tvMenu;
        public Panel _panelMain; // 改成public，确保子窗体可以访问
        private Label _lblTitle;
        private Button _btnLogout;
        private Timer _reminderTimer;
        private readonly HashSet<string> _shownReminderKeys = new HashSet<string>();
        private readonly B_MedicineReminder _bllReminder = new B_MedicineReminder();

        public FrmPatientMain()
        {
            InitializeComponent();
            InitializeBaseForm();

            CreateAllControls();

            BindEvents();
        }

        #region 窗体基础属性（100%稳定）
        private void InitializeBaseForm()
        {
            ApplyThemeSettings();
            Text = "糖尿病健康管理系统 - 患者端";
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1280, 720);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("微软雅黑", Program.ScaleFont(9F));
            BackColor = Program.GetMainBackColor();
            IsMdiContainer = false;
            AutoScaleMode = AutoScaleMode.Font;
            DoubleBuffered = true;
        }
        #endregion

        #region 生成所有控件（层级绝对正确）
        private void CreateAllControls()
        {
            ApplyThemeSettings();
            SuspendLayout();
            Controls.Clear(); // 清空所有旧控件，彻底避免冲突

            // ====================== 【顺序1】先加顶部状态栏（Dock.Top，优先级最高） ======================
            Panel pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 76,
                BackColor = _themePrimary,

                Name = "pnlTop"
            };

            _lblTitle = new Label
            {
                Text = $"糖尿病健康综合管理系统 - 欢迎您，{Program.LoginUser?.user_name}",
                Font = new Font("微软雅黑", Program.ScaleFont(16F), FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,

                Padding = new Padding(20, 0, 0, 0),
                Size = new Size(800, 70),
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
            Controls.Add(pnlTop); // 先把顶部栏加到窗体

            // ====================== 【顺序2】再加左侧菜单（Dock.Left，优先级第二） ======================
            Panel pnlLeft = new Panel
            {
                Dock = DockStyle.Left,
                Width = 232,
                BackColor = _menuBg,
                Name = "pnlLeft"
            };

            _tvMenu = new TreeView
            {
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", Program.ScaleFont(11F)),
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
        new TreeNode("首页") { Tag = "FrmPatientHome", BackColor = _menuBg },
        new TreeNode("血糖记录管理") { Tag = "FrmBloodSugarManage", BackColor = _menuBg },
         new TreeNode("用药记录管理") { Tag = "FrmMedicineManage", BackColor = _menuBg },
        new TreeNode("饮食干预推荐") { Tag = "FrmDietRecommend", BackColor = _menuBg },
        new TreeNode("运动方案管理") { Tag = "FrmExerciseManage", BackColor = _menuBg },
        new TreeNode("个人中心") { Tag = "FrmPatientMyInfo", BackColor = _menuBg }
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
                    TextRenderer.DrawText(e.Graphics, e.Node.Text, _tvMenu.Font, textRect, Program.GetTextColor(), TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                }
            };

            pnlLeft.Controls.Add(_tvMenu);
            Controls.Add(pnlLeft); // 再把左侧菜单加到窗体

            // ====================== 【顺序3】最后加主容器（Dock.Fill，填充剩余空间，永远在菜单右侧！！！） ======================
            _panelMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Program.GetMainBackColor(),
                Name = "panelMain",
                BorderStyle = BorderStyle.None,
                Padding = new Padding(12)
            };

            Controls.Add(_panelMain); // 最后加主容器，它会自动填充顶部栏下方、菜单右侧的所有空间，绝对不会被遮挡！
            ApplyUnifiedStyles(this);

            ResumeLayout(true);
            PerformLayout();
        }
        #endregion

        #region 事件绑定
        private void BindEvents()
        {
            Load += FrmPatientMain_Load;
            FormClosed += FrmPatientMain_FormClosed;
            _tvMenu.NodeMouseClick += TvMenu_NodeMouseClick;
            _btnLogout.Click += (s, e) =>
            {
                if (MessageBox.Show("确定要退出登录吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Program.LoginUser = null;
                    new FrmPatientLogin().Show();
                    Close();
                }
            };
        }
        #endregion

        #region 打开子窗体（100%稳定，无任何花里胡哨）
        private void TvMenu_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == null || string.IsNullOrEmpty(e.Node.Tag?.ToString())) return;
            string formClassName = e.Node.Tag.ToString();

            // 先立即选中并重绘菜单，给用户立刻反馈
            _tvMenu.SelectedNode = e.Node;
            _tvMenu.Invalidate();
            _tvMenu.Update();

            // 重绘完成后再加载内容，避免“内容出来后才高亮”的卡顿感
            BeginInvoke(new Action(() => OpenChildForm(formClassName, e.Node.Text)));
        }

        private void OpenChildForm(string formClassName, string nodeText)
        {
            _panelMain.Controls.Clear();
            try
            {
                Type formType = Assembly.GetExecutingAssembly().GetType($"PatientUI.{formClassName}");
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
                childForm.Name = formClassName;
                childForm.AutoScaleMode = AutoScaleMode.Font;
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

        public void RefreshDisplaySettings(string selectedFormClassName = null)
        {
            string targetFormClassName = selectedFormClassName ?? _tvMenu?.SelectedNode?.Tag?.ToString() ?? "FrmPatientHome";
            InitializeBaseForm();
            CreateAllControls();

            _tvMenu.NodeMouseClick += TvMenu_NodeMouseClick;
            _btnLogout.Click += (s, e) =>
            {
                if (MessageBox.Show("确定要退出登录吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Program.LoginUser = null;
                    new FrmPatientLogin().Show();
                    Close();
                }
            };

            if (_tvMenu?.Nodes.Count > 0)
            {
                TreeNode targetNode = _tvMenu.Nodes.Cast<TreeNode>().FirstOrDefault(node => string.Equals(node.Tag?.ToString(), targetFormClassName, StringComparison.OrdinalIgnoreCase)) ?? _tvMenu.Nodes[0];
                _tvMenu.SelectedNode = targetNode;
                _tvMenu.Invalidate();
                _tvMenu.Update();
                BeginInvoke(new Action(() => OpenChildForm(targetNode.Tag?.ToString(), targetNode.Text)));
            }
        }

        private void ApplyThemeSettings()
        {
            _themePrimary = Program.GetPrimaryColor();
            _menuBg = Program.GetMenuBackColor();
            _menuHover = Program.GetMenuHoverColor();
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
            button.Font = new Font("微软雅黑", Program.ScaleFont(9.5F), button.Font.Style);
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
            textBox.Font = new Font("微软雅黑", Program.ScaleFont(9.5F), textBox.Font.Style);
            textBox.Margin = textBox.Margin.All <= 3 ? new Padding(6) : textBox.Margin;
            if (textBox is TextBox plainTextBox && !plainTextBox.Multiline)
                plainTextBox.MinimumSize = new Size(0, 30);
        }

        private void StyleComboBox(ComboBox comboBox)
        {
            comboBox.Font = new Font("微软雅黑", Program.ScaleFont(9.5F), comboBox.Font.Style);
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.IntegralHeight = false;
            comboBox.Height = Math.Max(comboBox.Height, 32);
            comboBox.Margin = comboBox.Margin.All <= 3 ? new Padding(6) : comboBox.Margin;
        }

        private void StyleDateTimePicker(DateTimePicker dateTimePicker)
        {
            dateTimePicker.Font = new Font("微软雅黑", Program.ScaleFont(9.5F), dateTimePicker.Font.Style);
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
            dataGridView.DefaultCellStyle.Font = new Font("微软雅黑", Program.ScaleFont(9F));
            dataGridView.DefaultCellStyle.SelectionBackColor = _themePrimary;
            dataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", Program.ScaleFont(9F), FontStyle.Bold);
            dataGridView.ColumnHeadersDefaultCellStyle.BackColor = _themePrimary;
            dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        }

        private void StyleChart(Chart chart)
        {
            chart.BackColor = Color.White;
            foreach (ChartArea chartArea in chart.ChartAreas)
            {
                chartArea.BackColor = Color.White;
                chartArea.AxisX.LabelStyle.Font = new Font("微软雅黑", Program.ScaleFont(8.5F));
                chartArea.AxisY.LabelStyle.Font = new Font("微软雅黑", Program.ScaleFont(8.5F));
                chartArea.AxisX.TitleFont = new Font("微软雅黑", Program.ScaleFont(9F), FontStyle.Bold);
                chartArea.AxisY.TitleFont = new Font("微软雅黑", Program.ScaleFont(9F), FontStyle.Bold);
                chartArea.AxisX.LineColor = Color.FromArgb(148, 163, 184);
                chartArea.AxisY.LineColor = Color.FromArgb(148, 163, 184);
            }

            foreach (Legend legend in chart.Legends)
                legend.Font = new Font("微软雅黑", Program.ScaleFont(8.5F));
        }

        private void StyleGroupBox(GroupBox groupBox)
        {
            groupBox.Font = new Font("微软雅黑", Program.ScaleFont(10F), FontStyle.Bold);
            groupBox.Padding = new Padding(Math.Max(groupBox.Padding.Left, 12), Math.Max(groupBox.Padding.Top, 12), Math.Max(groupBox.Padding.Right, 12), Math.Max(groupBox.Padding.Bottom, 12));
        }

        private void StyleTabControl(TabControl tabControl)
        {
            tabControl.Padding = new Point(Math.Max(tabControl.Padding.X, 18), Math.Max(tabControl.Padding.Y, 8));
            tabControl.ItemSize = new Size(Math.Max(tabControl.ItemSize.Width, 96), Math.Max(tabControl.ItemSize.Height, 32));
        }

        private void StartReminderMonitor()
        {
            if (_reminderTimer == null)
            {
                _reminderTimer = new Timer { Interval = 30000 };
                _reminderTimer.Tick += (s, e) => CheckMedicineReminderAlerts();
            }
            _reminderTimer.Start();
            CheckMedicineReminderAlerts();
        }

        private void StopReminderMonitor()
        {
            if (_reminderTimer == null)
                return;

            _reminderTimer.Stop();
            _reminderTimer.Dispose();
            _reminderTimer = null;
        }

        private void CheckMedicineReminderAlerts()
        {
            if (Program.LoginUser == null || Program.LoginUser.user_id <= 0)
                return;

            DateTime now = DateTime.Now;
            var reminderList = _bllReminder.GetUserReminders(Program.LoginUser.user_id)
                .Where(item => item.is_enabled)
                .ToList();
            foreach (var reminder in reminderList)
            {
                if (!TimeSpan.TryParse(reminder.reminder_time, out TimeSpan reminderTime))
                    continue;

                DateTime targetTime = now.Date.Add(reminderTime);
                if (Math.Abs((targetTime - now).TotalMinutes) > 0.6d)
                    continue;

                string reminderKey = $"{now:yyyyMMdd}_{reminder.reminder_id}_{reminder.reminder_time}";
                if (_shownReminderKeys.Contains(reminderKey))
                    continue;

                _shownReminderKeys.Add(reminderKey);
                ShowMedicineReminderAlert(reminder);
            }
        }

        private void ShowMedicineReminderAlert(MedicineReminder reminder)
        {
            _reminderTimer?.Stop();
            try
            {
                using (var alertForm = new Form())
                {
                    alertForm.Text = "用药强提醒";
                    alertForm.StartPosition = FormStartPosition.CenterScreen;
                    alertForm.Size = new Size(600, 380);
                    alertForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    alertForm.MaximizeBox = false;
                    alertForm.MinimizeBox = false;
                    alertForm.TopMost = true;
                    alertForm.BackColor = Color.White;

                    var lblTitle = new Label
                    {
                        Dock = DockStyle.Top,
                        Height = 90,
                        Text = "⏰ 请立即按时用药",
                        Font = new Font("微软雅黑", Program.ScaleFont(22F), FontStyle.Bold),
                        ForeColor = Color.FromArgb(220, 53, 69),
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    var lblDrug = new Label
                    {
                        Dock = DockStyle.Top,
                        Height = 56,
                        Text = $"药物：{reminder.drug_name}    剂量：{reminder.drug_dosage}",
                        Font = new Font("微软雅黑", Program.ScaleFont(15F), FontStyle.Bold),
                        ForeColor = Color.FromArgb(33, 37, 41),
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    var lblTime = new Label
                    {
                        Dock = DockStyle.Top,
                        Height = 48,
                        Text = $"提醒时间：{reminder.reminder_time}    用药方式：{reminder.take_way}",
                        Font = new Font("微软雅黑", Program.ScaleFont(13F), FontStyle.Bold),
                        ForeColor = Color.FromArgb(0, 122, 204),
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    var lblRemark = new Label
                    {
                        Dock = DockStyle.Fill,
                        Text = string.IsNullOrWhiteSpace(reminder.remark) ? "请按医嘱完成本次用药，完成前请勿关闭此提醒。" : reminder.remark,
                        Font = new Font("微软雅黑", Program.ScaleFont(12F), FontStyle.Bold),
                        ForeColor = Color.FromArgb(64, 64, 64),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Padding = new Padding(36, 0, 36, 0)
                    };
                    var btnConfirm = new Button
                    {
                        Dock = DockStyle.Bottom,
                        Height = 60,
                        Text = "已确认，本次提醒关闭",
                        BackColor = Color.FromArgb(220, 53, 69),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("微软雅黑", Program.ScaleFont(12F), FontStyle.Bold)
                    };
                    btnConfirm.FlatAppearance.BorderSize = 0;
                    btnConfirm.Click += (s, e) => alertForm.Close();

                    alertForm.Controls.Add(lblRemark);
                    alertForm.Controls.Add(btnConfirm);
                    alertForm.Controls.Add(lblTime);
                    alertForm.Controls.Add(lblDrug);
                    alertForm.Controls.Add(lblTitle);
                    System.Media.SystemSounds.Exclamation.Play();
                    alertForm.ShowDialog(this);
                }
            }
            finally
            {
                _reminderTimer?.Start();
            }
        }

        #endregion

        #region 窗体加载/关闭事件
        private void FrmPatientMain_Load(object sender, EventArgs e)
        {
            // 默认打开首页
            if (_tvMenu?.Nodes.Count > 0)
            {
                TreeNode homeNode = _tvMenu.Nodes[0];
                _tvMenu.SelectedNode = homeNode;
                _tvMenu.Invalidate();
                _tvMenu.Update();
                BeginInvoke(new Action(() => OpenChildForm(homeNode.Tag?.ToString(), homeNode.Text)));
            }
            // 更新欢迎语
            if (Program.LoginUser != null)
            {
                _lblTitle.Text = $"糖尿病健康综合管理系统 - 欢迎您，{Program.LoginUser.user_name}";
            }
            StartReminderMonitor();
        }

        private void FrmPatientMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            StopReminderMonitor();
            Application.Exit();
        }
        #endregion
    }
}