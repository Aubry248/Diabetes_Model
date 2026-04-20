using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Model;
using BLL;

namespace PatientUI
{
    public partial class FrmPatientMyInfo : Form
    {
        // ==============================================
        // 【1. 页面可调节参数区】—— 完全不动
        // ==============================================
        private readonly int _pageTopMargin = 100;
        private readonly int _pageLeftMargin = 250;
        private readonly int _pageRightMargin = 20;
        private readonly int _pageBottomMargin = 20;
        private readonly int _infoAreaHeight = 420;
        private readonly int _settingsAreaHeight = 170;
        private readonly int _btnAreaHeight = 84;
        private readonly Padding _controlMargin = new Padding(5);
        private readonly Padding _labelPadding = new Padding(0, 5, 10, 5);
        private readonly float _labelColumnWidth = 0.25f;
        private readonly float _valueColumnWidth = 0.75f;

        private bool _isEditing = false;

        public FrmPatientMyInfo()
        {
            this.TopLevel = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;
            this.BackColor = Program.GetMainBackColor();
            this.Font = new Font("微软雅黑", Program.ScaleFont(9F));
            this.AutoScaleMode = AutoScaleMode.Font;
            this.DoubleBuffered = true;

            this.VisibleChanged += (s, e) =>
            {
                if (this.Visible)
                {
                    BuildLayout();
                    LoadUserInfo(); // 只调用正确的加载方法
                    LoadDisplaySettingsControls();
                    ApplyHighContrastIfNeeded(); // 加这一行
                }
            };
        }

        // ==============================================
        // 布局方法 —— 完全不动
        // ==============================================
        private void BuildLayout()
        {
            this.Controls.Clear();
            this.SuspendLayout();
            var rootPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                BackColor = Program.GetMainBackColor(),
                Padding = new Padding(_pageLeftMargin, _pageTopMargin, _pageRightMargin, _pageBottomMargin),
                GrowStyle = TableLayoutPanelGrowStyle.FixedSize
            };
            rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, _infoAreaHeight));
            rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, _settingsAreaHeight));
            rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, _btnAreaHeight));
            rootPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            rootPanel.Controls.Add(CreateInfoPanel(), 0, 0);
            rootPanel.Controls.Add(CreateDisplaySettingsPanel(), 0, 1);
            rootPanel.Controls.Add(CreateBtnPanel(), 0, 2);
            this.Controls.Add(rootPanel);
            this.ResumeLayout(true);
            this.PerformLayout();
            ApplyHighContrastIfNeeded();
        }

        private Panel CreateInfoPanel()
        {
            var infoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(20),
                AutoScroll = true
            };
            var lblTitle = new Label
            {
                Text = "👤 个人基础信息",
                Font = new Font("微软雅黑", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 150, 243),
                Dock = DockStyle.Top,
                Height = 40,
                Margin = new Padding(0, 0, 0, 16),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var infoLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 9,
                BackColor = Color.White
            };
            infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75));
            for (int i = 0; i < 9; i++)
                infoLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            AddInfoRow(infoLayout, "姓名：", "lblUserName", "txtUserName", true);
            AddInfoRow(infoLayout, "手机号：", "lblPhone", "txtPhone", true);
            AddInfoRow(infoLayout, "身份证号：", "lblIdCard", "txtIdCard", true);
            AddInfoRow(infoLayout, "性别：", "lblGender", "txtGender", true);
            AddInfoRow(infoLayout, "年龄：", "lblAge", "txtAge", true);
            AddInfoRow(infoLayout, "糖尿病类型：", "lblDiabetesType", "txtDiabetesType", true);
            AddInfoRow(infoLayout, "确诊日期：", "lblDiagnoseDate", "txtDiagnoseDate", true);
            AddInfoRow(infoLayout, "空腹血糖基线：", "lblFastingGlucose", "txtFastingGlucose", true);
            AddInfoRow(infoLayout, "", "", "", true);

            infoPanel.Controls.Add(infoLayout);
            infoPanel.Controls.Add(lblTitle);
            return infoPanel;
        }

        private void AddInfoRow(TableLayoutPanel layout, string labelText, string labelName, string textBoxName, bool isReadOnly)
        {
            var lbl = new Label
            {
                Text = labelText,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 8, 10, 8),
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                Name = labelName,
                ForeColor = Color.FromArgb(85, 85, 85)
            };
            var txt = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(5),
                ReadOnly = isReadOnly,
                BackColor = Color.White,
                Name = textBoxName,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("微软雅黑", 10F),
                ForeColor = Color.FromArgb(33, 37, 41),
                TextAlign = HorizontalAlignment.Left
            };
            layout.Controls.Add(lbl);
            layout.Controls.Add(txt);
        }

        private Panel CreateDisplaySettingsPanel()
        {
            var settingsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Program.GetPanelBackColor(),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(20)
            };

            var lblTitle = new Label
            {
                Text = "🖥 显示设置",
                Font = new Font("微软雅黑", Program.ScaleFont(14F), FontStyle.Bold),
                ForeColor = Program.GetPrimaryColor(),
                Dock = DockStyle.Top,
                Height = 36,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                BackColor = Program.GetPanelBackColor()
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

            var lblFontScale = new Label
            {
                Text = "字体大小：",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("微软雅黑", Program.ScaleFont(10F), FontStyle.Bold),
                ForeColor = Program.GetTextColor()
            };
            var cboFontScale = new ComboBox
            {
                Name = "cboFontScale",
                Dock = DockStyle.Left,
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("微软雅黑", Program.ScaleFont(10F))
            };
            cboFontScale.Items.AddRange(new object[] { "标准", "偏大", "超大" });

            var lblMode = new Label
            {
                Text = "显示模式：",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("微软雅黑", Program.ScaleFont(10F), FontStyle.Bold),
                ForeColor = Program.GetTextColor()
            };
            var chkHighContrast = new CheckBox
            {
                Name = "chkHighContrast",
                Text = "高对比度模式",
                Dock = DockStyle.Left,
                Width = 200,
                Font = new Font("微软雅黑", Program.ScaleFont(10F)),
                ForeColor = Program.GetTextColor(),
                BackColor = Program.GetPanelBackColor()
            };

            var btnApply = new Button
            {
                Name = "btnApplyDisplaySettings",
                Text = "应用设置",
                Width = 110,
                Height = 34,
                BackColor = Program.GetPrimaryColor(),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", Program.ScaleFont(9.5F), FontStyle.Bold),
                Anchor = AnchorStyles.Right
            };
            btnApply.FlatAppearance.BorderSize = 0;
            btnApply.Click += BtnApplyDisplaySettings_Click;

            var lblHint = new Label
            {
                Name = "lblDisplaySettingHint",
                Text = "可调整患者端字体大小与高对比度显示，点击后立即刷新当前界面。",
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", Program.ScaleFont(9F)),
                ForeColor = Program.GetTextColor(),
                TextAlign = ContentAlignment.MiddleLeft
            };

            layout.Controls.Add(lblFontScale, 0, 0);
            layout.Controls.Add(cboFontScale, 1, 0);
            layout.Controls.Add(btnApply, 2, 0);
            layout.Controls.Add(lblMode, 0, 1);
            layout.Controls.Add(chkHighContrast, 1, 1);
            layout.Controls.Add(lblHint, 2, 1);

            settingsPanel.Controls.Add(layout);
            settingsPanel.Controls.Add(lblTitle);
            return settingsPanel;
        }

        private void LoadDisplaySettingsControls()
        {
            ComboBox cboFontScale = FindControlRecursive(this, "cboFontScale") as ComboBox;
            CheckBox chkHighContrast = FindControlRecursive(this, "chkHighContrast") as CheckBox;
            if (cboFontScale != null)
            {
                cboFontScale.SelectedIndex = Program.PatientFontScale >= 1.3f ? 2 : (Program.PatientFontScale >= 1.1f ? 1 : 0);
            }
            if (chkHighContrast != null)
            {
                chkHighContrast.Checked = Program.PatientHighContrastMode;
            }
        }

        private void BtnApplyDisplaySettings_Click(object sender, EventArgs e)
        {
            ComboBox cboFontScale = FindControlRecursive(this, "cboFontScale") as ComboBox;
            CheckBox chkHighContrast = FindControlRecursive(this, "chkHighContrast") as CheckBox;

            Program.PatientFontScale = cboFontScale != null && cboFontScale.SelectedIndex == 2 ? 1.3f : (cboFontScale != null && cboFontScale.SelectedIndex == 1 ? 1.15f : 1f);
            Program.PatientHighContrastMode = chkHighContrast != null && chkHighContrast.Checked;

            FrmPatientMain mainForm = Application.OpenForms.OfType<FrmPatientMain>().FirstOrDefault();
            if (mainForm != null)
            {
                mainForm.RefreshDisplaySettings("FrmPatientMyInfo");
            }
            else
            {
                BuildLayout();
                LoadUserInfo();
                LoadDisplaySettingsControls();
            }
        }

        private Control FindControlRecursive(Control root, string controlName)
        {
            if (root == null)
                return null;
            if (root.Name == controlName)
                return root;

            foreach (Control child in root.Controls)
            {
                Control match = FindControlRecursive(child, controlName);
                if (match != null)
                    return match;
            }

            return null;
        }

        private Panel CreateBtnPanel()
        {
            var btnPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10, 12, 10, 12)
            };
            var flowLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            // ✅ 修复：自定义圆角按钮（正确实现，不报错）
            Button btnLogout = new Button
            {
                Text = "🚪 退出登录",
                Width = 130,
                Height = 38,
                Margin = new Padding(8, 6, 8, 6),
                BackColor = Color.FromArgb(198, 40, 40),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 9.75F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnLogout.FlatAppearance.BorderSize = 0;

            Button btnChangePwd = new Button
            {
                Text = "🔐 修改密码",
                Width = 130,
                Height = 38,
                Margin = new Padding(8, 6, 8, 6),
                BackColor = Color.FromArgb(251, 140, 0),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 9.75F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnChangePwd.FlatAppearance.BorderSize = 0;

            Button btnEditInfo = new Button
            {
                Text = "✏️ 编辑信息",
                Width = 130,
                Height = 38,
                Margin = new Padding(8, 6, 8, 6),
                BackColor = Color.FromArgb(41, 98, 255),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 9.75F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnEditInfo.FlatAppearance.BorderSize = 0;

            btnLogout.Click += BtnLogout_Click;
            btnChangePwd.Click += BtnChangePwd_Click;
            btnEditInfo.Click += BtnEditInfo_Click;

            flowLayout.Controls.Add(btnLogout);
            flowLayout.Controls.Add(btnChangePwd);
            flowLayout.Controls.Add(btnEditInfo);
            btnPanel.Controls.Add(flowLayout);
            return btnPanel;
        }

        // ==============================================
        // ✅ 修复：加载信息（从t_patient取患者字段）
        // ==============================================
        private void LoadUserInfo()
        {
            if (Program.LoginUser == null)
            {
                MessageBox.Show("未获取到登录用户信息，请重新登录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Users user = Program.LoginUser;
            // ✅ 从扩展表获取患者信息
            Patient patient = B_Patient.GetPatientById(user.user_id);

            foreach (Control root in this.Controls)
            {
                if (root is TableLayoutPanel rootPanel)
                {
                    foreach (Control panel in rootPanel.Controls)
                    {
                        if (panel is Panel infoPanel)
                        {
                            foreach (Control layout in infoPanel.Controls)
                            {
                                if (layout is TableLayoutPanel infoLayout)
                                {
                                    foreach (Control ctrl in infoLayout.Controls)
                                    {
                                        switch (ctrl.Name)
                                        {
                                            case "txtUserName": ctrl.Text = user.user_name ?? ""; break;
                                            case "txtPhone": ctrl.Text = user.phone ?? ""; break;
                                            case "txtIdCard": ctrl.Text = user.id_card ?? ""; break;
                                            case "txtGender": ctrl.Text = user.gender == 1 ? "男" : "女"; break;
                                            case "txtAge": ctrl.Text = user.age.ToString(); break;
                                            // ✅ 从t_patient赋值
                                            case "txtDiabetesType": ctrl.Text = patient?.diabetes_type ?? "未填写"; break;
                                            case "txtDiagnoseDate": ctrl.Text = patient?.diagnose_date?.ToString("yyyy-MM-dd") ?? "未填写"; break;
                                            case "txtFastingGlucose": ctrl.Text = patient?.fasting_glucose_baseline?.ToString("F2") ?? "未填写"; break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // ==============================================
        // 编辑/保存 —— 完全不动
        // ==============================================
        private void BtnEditInfo_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            _isEditing = !_isEditing;
            btn.Text = _isEditing ? "保存信息" : "编辑信息";
            SetInfoEditMode(_isEditing);
            if (!_isEditing) SaveUserInfo();
        }

        private void SetInfoEditMode(bool isEditing)
        {
            foreach (Control root in this.Controls)
            {
                if (root is TableLayoutPanel rootPanel)
                {
                    foreach (Control panel in rootPanel.Controls)
                    {
                        if (panel is Panel infoPanel)
                        {
                            foreach (Control layout in infoPanel.Controls)
                            {
                                if (layout is TableLayoutPanel infoLayout)
                                {
                                    foreach (Control ctrl in infoLayout.Controls)
                                    {
                                        if (ctrl is TextBox txt && txt.Name != "txtUserName" && txt.Name != "txtIdCard")
                                        {
                                            txt.ReadOnly = !isEditing;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // ==============================================
        // ✅ 修复：保存（患者字段存t_patient，不改动t_user）
        // ==============================================
        private void SaveUserInfo()
        {
            if (Program.LoginUser == null) return;
            Users user = Program.LoginUser;
            Patient patient = B_Patient.GetPatientById(user.user_id) ?? new Patient { patient_id = user.user_id };

            try
            {
                foreach (Control root in this.Controls)
                {
                    if (root is TableLayoutPanel rootPanel)
                    {
                        foreach (Control panel in rootPanel.Controls)
                        {
                            if (panel is Panel infoPanel)
                            {
                                foreach (Control layout in infoPanel.Controls)
                                {
                                    if (layout is TableLayoutPanel infoLayout)
                                    {
                                        foreach (Control ctrl in infoLayout.Controls)
                                        {
                                            switch (ctrl.Name)
                                            {
                                                case "txtPhone": user.phone = ctrl.Text; break;
                                                case "txtGender": user.gender = ctrl.Text == "男" ? 1 : 2; break;
                                                case "txtAge": int.TryParse(ctrl.Text, out int age); user.age = age; break;
                                                // ✅ 患者字段赋值给patient
                                                case "txtDiabetesType": patient.diabetes_type = ctrl.Text; break;
                                                case "txtDiagnoseDate":
                                                    if (DateTime.TryParse(ctrl.Text, out DateTime date))
                                                        patient.diagnose_date = date; break;
                                                case "txtFastingGlucose":
                                                    if (decimal.TryParse(ctrl.Text, out decimal val))
                                                        patient.fasting_glucose_baseline = val; break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // ✅ 分别更新：基础表 + 扩展表
                B_User.UpdateUser(user);
                B_Patient.UpdatePatient(patient);

                MessageBox.Show("保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadUserInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==============================================
        // 按钮事件 —— 完全不动
        // ==============================================
        private void BtnChangePwd_Click(object sender, EventArgs e)
        {
            if (Program.LoginUser == null)
            {
                MessageBox.Show("未获取到登录用户信息，请重新登录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (FrmPatientChangePwd changePwdForm = new FrmPatientChangePwd())
            {
                changePwdForm.ShowDialog(this);
            }
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定退出？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Program.LoginUser = null;
                Application.OpenForms.OfType<FrmPatientMain>().FirstOrDefault()?.Close();
                new FrmPatientLogin().Show();
            }
        }

        private void FrmPatientMyInfo_Load(object sender, EventArgs e)
        {

        }
        // ==============================================
        // 高对比度模式自动适配（新增）
        // ==============================================
        private void ApplyHighContrastIfNeeded()
        {
            bool isHighContrast = Program.PatientHighContrastMode;

            foreach (Control c in this.Controls)
            {
                ApplyTheme(c, isHighContrast);
            }
        }

        private void ApplyTheme(Control control, bool isHighContrast)
        {
            if (control == null) return;

            // 高对比度配色
            Color darkBg = Color.FromArgb(20, 20, 20);
            Color lightBg = Color.FromArgb(45, 45, 45);
            Color whiteText = Color.White;
            Color inputBorder = Color.White;

            // 普通模式配色
            Color normalBg = Color.White;
            Color normalText = Color.FromArgb(33, 37, 41);
            Color grayText = Color.FromArgb(85, 85, 85);

            try
            {
                // 面板/卡片
                if (control is Panel || control is TableLayoutPanel || control is GroupBox)
                {
                    control.BackColor = isHighContrast ? darkBg : Program.GetMainBackColor();
                }

                // 标签
                if (control is Label lbl)
                {
                    lbl.ForeColor = isHighContrast ? whiteText : (lbl.Name.Contains("lbl") ? grayText : normalText);
                }

                // 输入框
                if (control is TextBox txt)
                {
                    txt.ForeColor = isHighContrast ? whiteText : normalText;
                    txt.BackColor = isHighContrast ? lightBg : normalBg;
                    txt.BorderStyle = isHighContrast ? BorderStyle.FixedSingle : BorderStyle.FixedSingle;
                }

                // 下拉框
                if (control is ComboBox cbo)
                {
                    cbo.ForeColor = isHighContrast ? whiteText : normalText;
                    cbo.BackColor = isHighContrast ? lightBg : normalBg;
                }

                // 复选框
                if (control is CheckBox chk)
                {
                    chk.ForeColor = isHighContrast ? whiteText : normalText;
                    chk.BackColor = isHighContrast ? darkBg : Program.GetPanelBackColor();
                }
            }
            catch { }

            // 递归子控件
            foreach (Control child in control.Controls)
            {
                ApplyTheme(child, isHighContrast);
            }
        }
    }

}