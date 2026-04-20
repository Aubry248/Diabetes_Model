using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BLL;

namespace AdminUI
{
    public partial class FrmSystemConfig : Form
    {
        #region 全局对象与控件声明
        // 业务层对象
        private readonly B_SystemConfig _bllConfig = new B_SystemConfig();
        // 主题色（和主窗体完全统一）
        private readonly Color _themePrimary = Color.FromArgb(0, 122, 204);
        private readonly Color _themeDanger = Color.FromArgb(220, 53, 69);
        private readonly Color _themeGray = Color.FromArgb(108, 117, 125);
        private readonly Color _themeSuccess = Color.FromArgb(40, 160, 40);

        // 全局控件声明
        private TabControl tabMain;
        #region Tab1 基础系统配置控件
        private TextBox txtSystemName, txtSystemVersion, txtSystemCopyright, txtLogoPath;
        private NumericUpDown nudPageSize, nudSessionTimeout, nudLogRetainDays, nudBackupRetainDays;
        private Button btnBaseSave, btnBaseReset;
        #endregion

        #region Tab2 糖尿病业务配置控件
        private NumericUpDown nudFastingMin, nudFastingMax, nudPostprandialMax, nudHypoglycemia, nudHyperglycemia;
        private NumericUpDown nudNormalFollowCycle, nudHighRiskFollowCycle, nudMedicalValidDays;
        private NumericUpDown nudMedicineRemind, nudGlucoseRemind, nudFollowRemind;
        private CheckBox chkEnableDietTemplate, chkEnableExerciseTemplate;
        private Button btnBizSave, btnBizReset;
        #endregion

        #region Tab3 安全权限配置控件
        private CheckBox chkFirstLoginForcePwd, chkAllowMultiLogin, chkEnableFullLog;
        private NumericUpDown nudLoginFailTimes, nudLockMinutes, nudNoOperationLogout;
        private Button btnSecuritySave, btnSecurityReset;
        #endregion

        #region Tab4 密码策略配置控件
        private NumericUpDown nudPwdMinLength, nudPwdChangeCycle, nudPwdHistoryCount;
        private CheckBox chkPwdUpperLower, chkPwdNumber, chkPwdSpecialChar;
        private Button btnPwdSave, btnPwdReset;
        #endregion
        #endregion

        #region 窗体初始化
        public FrmSystemConfig()
        {
            // 窗体基础配置（和MDI主窗体适配）
            this.Text = "系统参数配置";
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1280, 720);
            this.Font = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

            // 创建所有控件
            CreateAllControls();
            // 绑定所有事件
            BindAllEvents();
        }
        #endregion

        #region 页面布局创建（4个Tab页，分组规范）
        private void CreateAllControls()
        {
            this.SuspendLayout();
            int margin = 12;
            int formWidth = this.ClientSize.Width;
            int formHeight = this.ClientSize.Height;

            // 主TabControl
            tabMain = new TabControl();
            tabMain.Name = "tabMain";
            tabMain.Location = new Point(margin, margin);
            tabMain.Size = new Size(formWidth - 2 * margin, formHeight - 2 * margin);
            tabMain.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            tabMain.Font = new Font("微软雅黑", 9F);
            tabMain.Appearance = TabAppearance.Normal;
            tabMain.ItemSize = new Size(140, 30);
            tabMain.SizeMode = TabSizeMode.Fixed;

            // ===================== Tab1：基础系统配置 =====================
            TabPage tabBaseConfig = new TabPage("基础系统配置");
            tabBaseConfig.Name = "tabBaseConfig";
            tabBaseConfig.BackColor = Color.White;
            tabBaseConfig.Font = new Font("微软雅黑", 9F);
            CreateBaseConfigTab(tabBaseConfig);
            tabMain.TabPages.Add(tabBaseConfig);

            // ===================== Tab2：糖尿病业务参数配置 =====================
            TabPage tabBizConfig = new TabPage("糖尿病业务参数配置");
            tabBizConfig.Name = "tabBizConfig";
            tabBizConfig.BackColor = Color.White;
            tabBizConfig.Font = new Font("微软雅黑", 9F);
            CreateBizConfigTab(tabBizConfig);
            tabMain.TabPages.Add(tabBizConfig);

            // ===================== Tab3：安全权限配置 =====================
            TabPage tabSecurityConfig = new TabPage("安全权限配置");
            tabSecurityConfig.Name = "tabSecurityConfig";
            tabSecurityConfig.BackColor = Color.White;
            tabSecurityConfig.Font = new Font("微软雅黑", 9F);
            CreateSecurityConfigTab(tabSecurityConfig);
            tabMain.TabPages.Add(tabSecurityConfig);

            // ===================== Tab4：密码策略配置 =====================
            TabPage tabPwdPolicy = new TabPage("密码策略配置");
            tabPwdPolicy.Name = "tabPwdPolicy";
            tabPwdPolicy.BackColor = Color.White;
            tabPwdPolicy.Font = new Font("微软雅黑", 9F);
            CreatePwdPolicyTab(tabPwdPolicy);
            tabMain.TabPages.Add(tabPwdPolicy);

            this.Controls.Add(tabMain);
            this.ResumeLayout(true);
        }

        #region Tab1 基础系统配置布局
        private void CreateBaseConfigTab(TabPage tab)
        {
            int margin = 20;
            int tabWidth = tab.ClientSize.Width;
            int labelWidth = 120;
            int inputWidth = 300;
            int lineHeight = 35;
            int groupMargin = 15;

            // 分组1：系统基础信息
            GroupBox gbBaseInfo = new GroupBox();
            gbBaseInfo.Name = "gbBaseInfo";
            gbBaseInfo.Text = "系统基础信息";
            gbBaseInfo.Location = new Point(margin, margin);
            gbBaseInfo.Size = new Size(tabWidth - 2 * margin, 180);
            gbBaseInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbBaseInfo.Font = new Font("微软雅黑", 9F, FontStyle.Bold);

            // 系统名称
            Label lblSystemName = new Label { Text = "系统名称：", Location = new Point(groupMargin, 30), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            txtSystemName = new TextBox { Location = new Point(groupMargin + labelWidth, 27), Size = new Size(inputWidth * 2, 25), Font = new Font("微软雅黑", 9F) };

            // 版本号
            Label lblVersion = new Label { Text = "系统版本号：", Location = new Point(groupMargin, 30 + lineHeight), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            txtSystemVersion = new TextBox { Location = new Point(groupMargin + labelWidth, 27 + lineHeight), Size = new Size(inputWidth, 25), Font = new Font("微软雅黑", 9F) };

            // 版权信息
            Label lblCopyright = new Label { Text = "版权信息：", Location = new Point(groupMargin, 30 + lineHeight * 2), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            txtSystemCopyright = new TextBox { Location = new Point(groupMargin + labelWidth, 27 + lineHeight * 2), Size = new Size(inputWidth * 2, 25), Font = new Font("微软雅黑", 9F) };

            // Logo路径
            Label lblLogo = new Label { Text = "系统Logo路径：", Location = new Point(groupMargin, 30 + lineHeight * 3), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            txtLogoPath = new TextBox { Location = new Point(groupMargin + labelWidth, 27 + lineHeight * 3), Size = new Size(inputWidth * 2, 25), Font = new Font("微软雅黑", 9F) };
            Button btnSelectLogo = new Button { Text = "选择文件", Location = new Point(groupMargin + labelWidth + inputWidth * 2 + 10, 25), Size = new Size(80, 30), BackColor = _themeGray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            gbBaseInfo.Controls.AddRange(new Control[] { lblSystemName, txtSystemName, lblVersion, txtSystemVersion, lblCopyright, txtSystemCopyright, lblLogo, txtLogoPath, btnSelectLogo });
            tab.Controls.Add(gbBaseInfo);

            // 分组2：系统运行参数
            GroupBox gbRunParam = new GroupBox();
            gbRunParam.Name = "gbRunParam";
            gbRunParam.Text = "系统运行参数";
            gbRunParam.Location = new Point(margin, 210);
            gbRunParam.Size = new Size(tabWidth - 2 * margin, 180);
            gbRunParam.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbRunParam.Font = new Font("微软雅黑", 9F, FontStyle.Bold);

            // 分页默认条数
            Label lblPageSize = new Label { Text = "分页默认条数：", Location = new Point(groupMargin, 30), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            nudPageSize = new NumericUpDown { Location = new Point(groupMargin + labelWidth, 27), Size = new Size(inputWidth, 25), Minimum = 5, Maximum = 100, Value = 15, Font = new Font("微软雅黑", 9F) };

            // 会话超时时间
            Label lblSessionTimeout = new Label { Text = "会话超时时间(分钟)：", Location = new Point(groupMargin, 30 + lineHeight), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            nudSessionTimeout = new NumericUpDown { Location = new Point(groupMargin + labelWidth, 27 + lineHeight), Size = new Size(inputWidth, 25), Minimum = 5, Maximum = 120, Value = 30, Font = new Font("微软雅黑", 9F) };

            // 日志保留天数
            Label lblLogRetain = new Label { Text = "日志保留天数：", Location = new Point(groupMargin + inputWidth + labelWidth + 50, 30), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            nudLogRetainDays = new NumericUpDown { Location = new Point(groupMargin + inputWidth + labelWidth * 2 + 50, 27), Size = new Size(inputWidth, 25), Minimum = 7, Maximum = 365, Value = 90, Font = new Font("微软雅黑", 9F) };

            // 备份文件默认保留天数
            Label lblBackupRetain = new Label { Text = "备份文件保留天数：", Location = new Point(groupMargin + inputWidth + labelWidth + 50, 30 + lineHeight), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            nudBackupRetainDays = new NumericUpDown { Location = new Point(groupMargin + inputWidth + labelWidth * 2 + 50, 27 + lineHeight), Size = new Size(inputWidth, 25), Minimum = 7, Maximum = 365, Value = 30, Font = new Font("微软雅黑", 9F) };

            gbRunParam.Controls.AddRange(new Control[] { lblPageSize, nudPageSize, lblSessionTimeout, nudSessionTimeout, lblLogRetain, nudLogRetainDays, lblBackupRetain, nudBackupRetainDays });
            tab.Controls.Add(gbRunParam);

            // 操作按钮
            btnBaseSave = new Button
            {
                Text = "保存配置",
                Location = new Point(margin, 410),
                Size = new Size(120, 40),
                BackColor = _themePrimary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 10F, FontStyle.Bold)
            };
            btnBaseReset = new Button
            {
                Text = "重置默认值",
                Location = new Point(margin + 130, 410),
                Size = new Size(120, 40),
                BackColor = _themeGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 10F)
            };
            tab.Controls.AddRange(new Control[] { btnBaseSave, btnBaseReset });

            // Logo选择事件
            btnSelectLogo.Click += (s, e) =>
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp";
                    ofd.Title = "选择系统Logo图片";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        txtLogoPath.Text = ofd.FileName;
                    }
                }
            };
        }
        #endregion

        #region Tab2 糖尿病业务参数配置布局
        private void CreateBizConfigTab(TabPage tab)
        {
            int margin = 20;
            int tabWidth = tab.ClientSize.Width;
            int labelWidth = 180;
            int inputWidth = 120;
            int lineHeight = 35;
            int groupMargin = 15;

            // 分组1：血糖预警阈值配置
            GroupBox gbGlucoseThreshold = new GroupBox();
            gbGlucoseThreshold.Name = "gbGlucoseThreshold";
            gbGlucoseThreshold.Text = "血糖预警阈值配置（单位：mmol/L）";
            gbGlucoseThreshold.Location = new Point(margin, margin);
            gbGlucoseThreshold.Size = new Size(tabWidth - 2 * margin, 140);
            gbGlucoseThreshold.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbGlucoseThreshold.Font = new Font("微软雅黑", 9F, FontStyle.Bold);

            // 空腹血糖正常范围
            Label lblFasting = new Label { Text = "空腹血糖正常范围：", Location = new Point(groupMargin, 30), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            nudFastingMin = new NumericUpDown { Location = new Point(groupMargin + labelWidth, 27), Size = new Size(inputWidth, 25), Minimum = 2.0m, Maximum = 5.0m, DecimalPlaces = 1, Increment = 0.1m, Value = 3.9m, Font = new Font("微软雅黑", 9F) };
            Label lblFastingTo = new Label { Text = "~", Location = new Point(groupMargin + labelWidth + inputWidth + 10, 30), Size = new Size(20, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            nudFastingMax = new NumericUpDown { Location = new Point(groupMargin + labelWidth + inputWidth + 40, 27), Size = new Size(inputWidth, 25), Minimum = 5.0m, Maximum = 10.0m, DecimalPlaces = 1, Increment = 0.1m, Value = 6.1m, Font = new Font("微软雅黑", 9F) };

            // 餐后2小时血糖正常上限
            Label lblPostprandial = new Label { Text = "餐后2小时血糖正常上限：", Location = new Point(groupMargin + inputWidth * 2 + labelWidth + 100, 30), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            nudPostprandialMax = new NumericUpDown { Location = new Point(groupMargin + inputWidth * 3 + labelWidth + 100, 27), Size = new Size(inputWidth, 25), Minimum = 6.0m, Maximum = 15.0m, DecimalPlaces = 1, Increment = 0.1m, Value = 7.8m, Font = new Font("微软雅黑", 9F) };

            // 低血糖紧急阈值
            Label lblHypoglycemia = new Label { Text = "低血糖紧急阈值：", Location = new Point(groupMargin, 30 + lineHeight), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            nudHypoglycemia = new NumericUpDown { Location = new Point(groupMargin + labelWidth, 27 + lineHeight), Size = new Size(inputWidth, 25), Minimum = 2.0m, Maximum = 4.0m, DecimalPlaces = 1, Increment = 0.1m, Value = 3.9m, Font = new Font("微软雅黑", 9F) };

            // 高血糖紧急阈值
            Label lblHyperglycemia = new Label { Text = "高血糖紧急阈值：", Location = new Point(groupMargin + inputWidth + labelWidth + 50, 30 + lineHeight), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            nudHyperglycemia = new NumericUpDown { Location = new Point(groupMargin + inputWidth * 2 + labelWidth + 50, 27 + lineHeight), Size = new Size(inputWidth, 25), Minimum = 10.0m, Maximum = 30.0m, DecimalPlaces = 1, Increment = 0.1m, Value = 16.7m, Font = new Font("微软雅黑", 9F) };

            gbGlucoseThreshold.Controls.AddRange(new Control[] { lblFasting, nudFastingMin, lblFastingTo, nudFastingMax, lblPostprandial, nudPostprandialMax, lblHypoglycemia, nudHypoglycemia, lblHyperglycemia, nudHyperglycemia });
            tab.Controls.Add(gbGlucoseThreshold);

            // 分组2：患者管理配置
            GroupBox gbPatientManage = new GroupBox();
            gbPatientManage.Name = "gbPatientManage";
            gbPatientManage.Text = "患者管理配置";
            gbPatientManage.Location = new Point(margin, 160);
            gbPatientManage.Size = new Size(tabWidth - 2 * margin, 120);
            gbPatientManage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbPatientManage.Font = new Font("微软雅黑", 9F, FontStyle.Bold);

            // 普通患者随访周期
            Label lblNormalFollow = new Label { Text = "普通患者默认随访周期(天)：", Location = new Point(groupMargin, 30), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            nudNormalFollowCycle = new NumericUpDown { Location = new Point(groupMargin + labelWidth, 27), Size = new Size(inputWidth, 25), Minimum = 7, Maximum = 90, Value = 30, Font = new Font("微软雅黑", 9F) };

            // 高危患者随访周期
            Label lblHighRiskFollow = new Label { Text = "高危患者默认随访周期(天)：", Location = new Point(groupMargin + inputWidth + labelWidth + 50, 30), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            nudHighRiskFollowCycle = new NumericUpDown { Location = new Point(groupMargin + inputWidth * 2 + labelWidth + 50, 27), Size = new Size(inputWidth, 25), Minimum = 1, Maximum = 30, Value = 7, Font = new Font("微软雅黑", 9F) };

            // 体检报告有效期
            Label lblMedicalValid = new Label { Text = "体检报告有效期(天)：", Location = new Point(groupMargin, 30 + lineHeight), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            nudMedicalValidDays = new NumericUpDown { Location = new Point(groupMargin + labelWidth, 27 + lineHeight), Size = new Size(inputWidth, 25), Minimum = 30, Maximum = 365, Value = 180, Font = new Font("微软雅黑", 9F) };

            gbPatientManage.Controls.AddRange(new Control[] { lblNormalFollow, nudNormalFollowCycle, lblHighRiskFollow, nudHighRiskFollowCycle, lblMedicalValid, nudMedicalValidDays });
            tab.Controls.Add(gbPatientManage);

            // 分组3：提醒服务配置
            GroupBox gbRemind = new GroupBox();
            gbRemind.Name = "gbRemind";
            gbRemind.Text = "提醒服务配置";
            gbRemind.Location = new Point(margin, 290);
            gbRemind.Size = new Size(tabWidth - 2 * margin, 120);
            gbRemind.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbRemind.Font = new Font("微软雅黑", 9F, FontStyle.Bold);

            // 用药提醒提前时长
            Label lblMedicineRemind = new Label { Text = "用药提醒提前时长(分钟)：", Location = new Point(groupMargin, 30), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            nudMedicineRemind = new NumericUpDown { Location = new Point(groupMargin + labelWidth, 27), Size = new Size(inputWidth, 25), Minimum = 5, Maximum = 120, Value = 30, Font = new Font("微软雅黑", 9F) };

            // 血糖监测提醒提前时长
            Label lblGlucoseRemind = new Label { Text = "血糖监测提醒提前时长(分钟)：", Location = new Point(groupMargin + inputWidth + labelWidth + 50, 30), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            nudGlucoseRemind = new NumericUpDown { Location = new Point(groupMargin + inputWidth * 2 + labelWidth + 50, 27), Size = new Size(inputWidth, 25), Minimum = 5, Maximum = 120, Value = 15, Font = new Font("微软雅黑", 9F) };

            // 随访提醒提前天数
            Label lblFollowRemind = new Label { Text = "随访提醒提前天数：", Location = new Point(groupMargin, 30 + lineHeight), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            nudFollowRemind = new NumericUpDown { Location = new Point(groupMargin + labelWidth, 27 + lineHeight), Size = new Size(inputWidth, 25), Minimum = 1, Maximum = 30, Value = 3, Font = new Font("微软雅黑", 9F) };

            gbRemind.Controls.AddRange(new Control[] { lblMedicineRemind, nudMedicineRemind, lblGlucoseRemind, nudGlucoseRemind, lblFollowRemind, nudFollowRemind });
            tab.Controls.Add(gbRemind);

            // 分组4：模板配置
            GroupBox gbTemplate = new GroupBox();
            gbTemplate.Name = "gbTemplate";
            gbTemplate.Text = "模板配置";
            gbTemplate.Location = new Point(margin, 420);
            gbTemplate.Size = new Size(tabWidth - 2 * margin, 80);
            gbTemplate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbTemplate.Font = new Font("微软雅黑", 9F, FontStyle.Bold);

            chkEnableDietTemplate = new CheckBox { Text = "启用系统默认饮食模板", Location = new Point(groupMargin, 30), AutoSize = true, Font = new Font("微软雅黑", 10F) };
            chkEnableExerciseTemplate = new CheckBox { Text = "启用系统默认运动模板", Location = new Point(groupMargin + 300, 30), AutoSize = true, Font = new Font("微软雅黑", 10F) };

            gbTemplate.Controls.AddRange(new Control[] { chkEnableDietTemplate, chkEnableExerciseTemplate });
            tab.Controls.Add(gbTemplate);

            // 操作按钮
            btnBizSave = new Button
            {
                Text = "保存配置",
                Location = new Point(margin, 520),
                Size = new Size(120, 40),
                BackColor = _themePrimary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 10F, FontStyle.Bold)
            };
            btnBizReset = new Button
            {
                Text = "重置默认值",
                Location = new Point(margin + 130, 520),
                Size = new Size(120, 40),
                BackColor = _themeGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 10F)
            };
            tab.Controls.AddRange(new Control[] { btnBizSave, btnBizReset });
        }
        #endregion

        #region Tab3 安全权限配置布局
        private void CreateSecurityConfigTab(TabPage tab)
        {
            int margin = 20;
            int tabWidth = tab.ClientSize.Width;
            int labelWidth = 180;
            int inputWidth = 120;
            int lineHeight = 35;
            int groupMargin = 15;

            // 分组1：登录安全配置
            GroupBox gbLoginSecurity = new GroupBox();
            gbLoginSecurity.Name = "gbLoginSecurity";
            gbLoginSecurity.Text = "登录安全配置";
            gbLoginSecurity.Location = new Point(margin, margin);
            gbLoginSecurity.Size = new Size(tabWidth - 2 * margin, 140);
            gbLoginSecurity.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbLoginSecurity.Font = new Font("微软雅黑", 9F, FontStyle.Bold);

            // 首次登录强制改密
            chkFirstLoginForcePwd = new CheckBox { Text = "首次登录强制修改密码", Location = new Point(groupMargin, 30), AutoSize = true, Font = new Font("微软雅黑", 10F) };
            // 允许账号多地登录
            chkAllowMultiLogin = new CheckBox { Text = "允许账号多地同时登录", Location = new Point(groupMargin + 300, 30), AutoSize = true, Font = new Font("微软雅黑", 10F) };

            // 登录失败最大次数
            Label lblLoginFailTimes = new Label { Text = "登录失败最大次数：", Location = new Point(groupMargin, 30 + lineHeight), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            nudLoginFailTimes = new NumericUpDown { Location = new Point(groupMargin + labelWidth, 27 + lineHeight), Size = new Size(inputWidth, 25), Minimum = 3, Maximum = 10, Value = 5, Font = new Font("微软雅黑", 9F) };

            // 账号锁定时长
            Label lblLockMinutes = new Label { Text = "账号锁定时长(分钟)：", Location = new Point(groupMargin + inputWidth + labelWidth + 50, 30 + lineHeight), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            nudLockMinutes = new NumericUpDown { Location = new Point(groupMargin + inputWidth * 2 + labelWidth + 50, 27 + lineHeight), Size = new Size(inputWidth, 25), Minimum = 5, Maximum = 120, Value = 30, Font = new Font("微软雅黑", 9F) };

            gbLoginSecurity.Controls.AddRange(new Control[] { chkFirstLoginForcePwd, chkAllowMultiLogin, lblLoginFailTimes, nudLoginFailTimes, lblLockMinutes, nudLockMinutes });
            tab.Controls.Add(gbLoginSecurity);

            // 分组2：会话安全配置
            GroupBox gbSessionSecurity = new GroupBox();
            gbSessionSecurity.Name = "gbSessionSecurity";
            gbSessionSecurity.Text = "会话安全配置";
            gbSessionSecurity.Location = new Point(margin, 160);
            gbSessionSecurity.Size = new Size(tabWidth - 2 * margin, 120);
            gbSessionSecurity.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbSessionSecurity.Font = new Font("微软雅黑", 9F, FontStyle.Bold);

            // 无操作自动退出时长
            Label lblNoOperationLogout = new Label { Text = "无操作自动退出时长(分钟)：", Location = new Point(groupMargin, 30), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            nudNoOperationLogout = new NumericUpDown { Location = new Point(groupMargin + labelWidth, 27), Size = new Size(inputWidth, 25), Minimum = 5, Maximum = 120, Value = 15, Font = new Font("微软雅黑", 9F) };

            // 开启操作日志全记录
            chkEnableFullLog = new CheckBox { Text = "开启操作日志全记录（含敏感操作）", Location = new Point(groupMargin, 30 + lineHeight), AutoSize = true, Font = new Font("微软雅黑", 10F) };

            gbSessionSecurity.Controls.AddRange(new Control[] { lblNoOperationLogout, nudNoOperationLogout, chkEnableFullLog });
            tab.Controls.Add(gbSessionSecurity);

            // 操作按钮
            btnSecuritySave = new Button
            {
                Text = "保存配置",
                Location = new Point(margin, 300),
                Size = new Size(120, 40),
                BackColor = _themePrimary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 10F, FontStyle.Bold)
            };
            btnSecurityReset = new Button
            {
                Text = "重置默认值",
                Location = new Point(margin + 130, 300),
                Size = new Size(120, 40),
                BackColor = _themeGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 10F)
            };
            tab.Controls.AddRange(new Control[] { btnSecuritySave, btnSecurityReset });
        }
        #endregion

        #region Tab4 密码策略配置布局
        private void CreatePwdPolicyTab(TabPage tab)
        {
            int margin = 20;
            int tabWidth = tab.ClientSize.Width;
            int labelWidth = 180;
            int inputWidth = 120;
            int lineHeight = 35;
            int groupMargin = 15;

            // 分组1：密码强度配置
            GroupBox gbPwdStrength = new GroupBox();
            gbPwdStrength.Name = "gbPwdStrength";
            gbPwdStrength.Text = "密码强度配置";
            gbPwdStrength.Location = new Point(margin, margin);
            gbPwdStrength.Size = new Size(tabWidth - 2 * margin, 140);
            gbPwdStrength.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbPwdStrength.Font = new Font("微软雅黑", 9F, FontStyle.Bold);

            // 密码最小长度
            Label lblPwdMinLength = new Label { Text = "密码最小长度：", Location = new Point(groupMargin, 30), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            nudPwdMinLength = new NumericUpDown { Location = new Point(groupMargin + labelWidth, 27), Size = new Size(inputWidth, 25), Minimum = 6, Maximum = 20, Value = 8, Font = new Font("微软雅黑", 9F) };

            // 必须包含大小写字母
            chkPwdUpperLower = new CheckBox { Text = "必须包含大小写字母", Location = new Point(groupMargin, 30 + lineHeight), AutoSize = true, Font = new Font("微软雅黑", 10F) };
            // 必须包含数字
            chkPwdNumber = new CheckBox { Text = "必须包含数字", Location = new Point(groupMargin + 250, 30 + lineHeight), AutoSize = true, Font = new Font("微软雅黑", 10F) };
            // 必须包含特殊字符
            chkPwdSpecialChar = new CheckBox { Text = "必须包含特殊字符", Location = new Point(groupMargin + 450, 30 + lineHeight), AutoSize = true, Font = new Font("微软雅黑", 10F) };

            gbPwdStrength.Controls.AddRange(new Control[] { lblPwdMinLength, nudPwdMinLength, chkPwdUpperLower, chkPwdNumber, chkPwdSpecialChar });
            tab.Controls.Add(gbPwdStrength);

            // 分组2：密码生命周期配置
            GroupBox gbPwdLife = new GroupBox();
            gbPwdLife.Name = "gbPwdLife";
            gbPwdLife.Text = "密码生命周期配置";
            gbPwdLife.Location = new Point(margin, 160);
            gbPwdLife.Size = new Size(tabWidth - 2 * margin, 120);
            gbPwdLife.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbPwdLife.Font = new Font("微软雅黑", 9F, FontStyle.Bold);

            // 密码强制更换周期
            Label lblPwdChangeCycle = new Label { Text = "密码强制更换周期(天)：", Location = new Point(groupMargin, 30), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            nudPwdChangeCycle = new NumericUpDown { Location = new Point(groupMargin + labelWidth, 27), Size = new Size(inputWidth, 25), Minimum = 30, Maximum = 365, Value = 90, Font = new Font("微软雅黑", 9F) };

            // 历史密码禁止重复次数
            Label lblPwdHistoryCount = new Label { Text = "历史密码禁止重复次数：", Location = new Point(groupMargin + inputWidth + labelWidth + 50, 30), Size = new Size(labelWidth, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            nudPwdHistoryCount = new NumericUpDown { Location = new Point(groupMargin + inputWidth * 2 + labelWidth + 50, 27), Size = new Size(inputWidth, 25), Minimum = 0, Maximum = 10, Value = 3, Font = new Font("微软雅黑", 9F) };

            gbPwdLife.Controls.AddRange(new Control[] { lblPwdChangeCycle, nudPwdChangeCycle, lblPwdHistoryCount, nudPwdHistoryCount });
            tab.Controls.Add(gbPwdLife);

            // 操作按钮
            btnPwdSave = new Button
            {
                Text = "保存配置",
                Location = new Point(margin, 300),
                Size = new Size(120, 40),
                BackColor = _themePrimary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 10F, FontStyle.Bold)
            };
            btnPwdReset = new Button
            {
                Text = "重置默认值",
                Location = new Point(margin + 130, 300),
                Size = new Size(120, 40),
                BackColor = _themeGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 10F)
            };
            tab.Controls.AddRange(new Control[] { btnPwdSave, btnPwdReset });
        }
        #endregion
        #endregion

        #region 事件绑定
        private void BindAllEvents()
        {
            // 窗体加载事件
            this.Load += FrmSystemConfig_Load;
            this.FormClosed += FrmSystemConfig_FormClosed;

            // Tab1 基础配置事件
            btnBaseSave.Click += BtnBaseSave_Click;
            btnBaseReset.Click += BtnBaseReset_Click;

            // Tab2 业务配置事件
            btnBizSave.Click += BtnBizSave_Click;
            btnBizReset.Click += BtnBizReset_Click;

            // Tab3 安全配置事件
            btnSecuritySave.Click += BtnSecuritySave_Click;
            btnSecurityReset.Click += BtnSecurityReset_Click;

            // Tab4 密码策略事件
            btnPwdSave.Click += BtnPwdSave_Click;
            btnPwdReset.Click += BtnPwdReset_Click;

            // Tab切换事件
            tabMain.SelectedIndexChanged += TabMain_SelectedIndexChanged;
        }
        #endregion

        #region 窗体生命周期事件
        private void FrmSystemConfig_Load(object sender, EventArgs e)
        {
            // 加载所有配置到控件
            LoadAllConfigToControl();
            // 更新状态栏
            if (this.MdiParent is FrmAdminMain mainForm)
            {
                mainForm.SetStatusTip("操作提示：已打开【系统参数配置】");
            }
        }

        private void FrmSystemConfig_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 释放资源
            this.Dispose();
        }

        private void TabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 切换Tab时刷新配置
            LoadAllConfigToControl();
        }
        #endregion

        #region 配置加载方法（从数据库加载到控件）
        private void LoadAllConfigToControl()
        {
            // 加载基础配置
            LoadBaseConfig();
            // 加载业务配置
            LoadBizConfig();
            // 加载安全配置
            LoadSecurityConfig();
            // 加载密码策略
            LoadPwdPolicyConfig();
        }

        private void LoadBaseConfig()
        {
            txtSystemName.Text = SystemGlobalConfig.SystemName;
            txtSystemVersion.Text = SystemGlobalConfig.SystemVersion;
            txtSystemCopyright.Text = SystemGlobalConfig.SystemCopyright;
            txtLogoPath.Text = SystemGlobalConfig.SystemLogoPath;
            nudPageSize.Value = SystemGlobalConfig.PageDefaultSize;
            nudSessionTimeout.Value = SystemGlobalConfig.SessionTimeoutMinutes;
            nudLogRetainDays.Value = SystemGlobalConfig.LogRetainDays;
            nudBackupRetainDays.Value = SystemGlobalConfig.BackupDefaultRetainDays;
        }

        private void LoadBizConfig()
        {
            nudFastingMin.Value = SystemGlobalConfig.FastingGlucoseNormalMin;
            nudFastingMax.Value = SystemGlobalConfig.FastingGlucoseNormalMax;
            nudPostprandialMax.Value = SystemGlobalConfig.PostprandialGlucoseNormalMax;
            nudHypoglycemia.Value = SystemGlobalConfig.HypoglycemiaEmergencyThreshold;
            nudHyperglycemia.Value = SystemGlobalConfig.HyperglycemiaEmergencyThreshold;
            nudNormalFollowCycle.Value = SystemGlobalConfig.NormalPatientFollowUpCycle;
            nudHighRiskFollowCycle.Value = SystemGlobalConfig.HighRiskPatientFollowUpCycle;
            nudMedicalValidDays.Value = SystemGlobalConfig.MedicalReportValidDays;
            nudMedicineRemind.Value = SystemGlobalConfig.MedicineRemindAdvanceMinutes;
            nudGlucoseRemind.Value = SystemGlobalConfig.GlucoseRemindAdvanceMinutes;
            nudFollowRemind.Value = SystemGlobalConfig.FollowUpRemindAdvanceDays;
            chkEnableDietTemplate.Checked = SystemGlobalConfig.EnableDefaultDietTemplate;
            chkEnableExerciseTemplate.Checked = SystemGlobalConfig.EnableDefaultExerciseTemplate;
        }

        private void LoadSecurityConfig()
        {
            chkFirstLoginForcePwd.Checked = SystemGlobalConfig.FirstLoginForceChangePwd;
            chkAllowMultiLogin.Checked = SystemGlobalConfig.AllowMultiPlaceLogin;
            nudLoginFailTimes.Value = SystemGlobalConfig.LoginFailMaxTimes;
            nudLockMinutes.Value = SystemGlobalConfig.AccountLockMinutes;
            nudNoOperationLogout.Value = SystemGlobalConfig.NoOperationAutoLogoutMinutes;
            chkEnableFullLog.Checked = SystemGlobalConfig.EnableFullOperationLog;
        }

        private void LoadPwdPolicyConfig()
        {
            nudPwdMinLength.Value = SystemGlobalConfig.PwdMinLength;
            chkPwdUpperLower.Checked = SystemGlobalConfig.PwdRequireUpperLower;
            chkPwdNumber.Checked = SystemGlobalConfig.PwdRequireNumber;
            chkPwdSpecialChar.Checked = SystemGlobalConfig.PwdRequireSpecialChar;
            nudPwdChangeCycle.Value = SystemGlobalConfig.PwdForceChangeCycleDays;
            nudPwdHistoryCount.Value = SystemGlobalConfig.PwdHistoryForbidRepeatCount;
        }
        #endregion

        #region 配置保存与重置事件处理
        #region Tab1 基础配置保存与重置
        private void BtnBaseSave_Click(object sender, EventArgs e)
        {
            if (Program.LoginUser == null)
            {
                MessageBox.Show("登录信息失效，请重新登录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 校验
            if (string.IsNullOrWhiteSpace(txtSystemName.Text))
            {
                MessageBox.Show("系统名称不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 组装配置字典
            Dictionary<string, string> configDict = new Dictionary<string, string>
            {
                { "System_Name", txtSystemName.Text.Trim() },
                { "System_Version", txtSystemVersion.Text.Trim() },
                { "System_Copyright", txtSystemCopyright.Text.Trim() },
                { "System_LogoPath", txtLogoPath.Text.Trim() },
                { "Page_DefaultSize", nudPageSize.Value.ToString() },
                { "Session_TimeoutMinutes", nudSessionTimeout.Value.ToString() },
                { "Log_RetainDays", nudLogRetainDays.Value.ToString() },
                { "Backup_DefaultRetainDays", nudBackupRetainDays.Value.ToString() }
            };

            // 保存配置
            this.Cursor = Cursors.WaitCursor;
            bool result = _bllConfig.SaveConfigBatch(configDict, "基础配置", Program.LoginUser.user_id, out string errorMsg);
            this.Cursor = Cursors.Default;

            if (result)
            {
                // 刷新全局配置
                SystemGlobalConfig.RefreshConfig();
                // 写入审计日志
                _bllConfig.WriteAuditLog(Program.LoginUser.user_id, "修改系统配置", "修改基础系统配置", Program.ClientIP);
                MessageBox.Show("基础系统配置保存成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"保存失败：{errorMsg}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBaseReset_Click(object sender, EventArgs e)
        {
            if (Program.LoginUser == null)
            {
                MessageBox.Show("登录信息失效，请重新登录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("确定要重置基础系统配置为默认值吗？", "确认重置", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            // 重置配置
            this.Cursor = Cursors.WaitCursor;
            bool result = _bllConfig.ResetConfigByType("基础配置", Program.LoginUser.user_id, out string errorMsg);
            this.Cursor = Cursors.Default;

            if (result)
            {
                // 刷新全局配置
                SystemGlobalConfig.RefreshConfig();
                // 重新加载控件
                LoadBaseConfig();
                // 写入审计日志
                _bllConfig.WriteAuditLog(Program.LoginUser.user_id, "重置系统配置", "重置基础系统配置为默认值", Program.ClientIP);
                MessageBox.Show("基础系统配置已重置为默认值！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"重置失败：{errorMsg}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Tab2 业务配置保存与重置
        private void BtnBizSave_Click(object sender, EventArgs e)
        {
            if (Program.LoginUser == null)
            {
                MessageBox.Show("登录信息失效，请重新登录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 阈值合法性校验
            if (nudFastingMin.Value >= nudFastingMax.Value)
            {
                MessageBox.Show("空腹血糖正常下限必须小于上限！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (nudHypoglycemia.Value >= nudFastingMin.Value)
            {
                MessageBox.Show("低血糖阈值必须小于空腹血糖正常下限！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (nudHyperglycemia.Value <= nudPostprandialMax.Value)
            {
                MessageBox.Show("高血糖紧急阈值必须大于餐后血糖正常上限！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 组装配置字典
            Dictionary<string, string> configDict = new Dictionary<string, string>
            {
                { "Glucose_FastingNormalMin", nudFastingMin.Value.ToString() },
                { "Glucose_FastingNormalMax", nudFastingMax.Value.ToString() },
                { "Glucose_PostprandialNormalMax", nudPostprandialMax.Value.ToString() },
                { "Glucose_HypoglycemiaThreshold", nudHypoglycemia.Value.ToString() },
                { "Glucose_HyperglycemiaThreshold", nudHyperglycemia.Value.ToString() },
                { "FollowUp_NormalPatientCycle", nudNormalFollowCycle.Value.ToString() },
                { "FollowUp_HighRiskPatientCycle", nudHighRiskFollowCycle.Value.ToString() },
                { "MedicalReport_ValidDays", nudMedicalValidDays.Value.ToString() },
                { "Remind_MedicineAdvanceMinutes", nudMedicineRemind.Value.ToString() },
                { "Remind_GlucoseAdvanceMinutes", nudGlucoseRemind.Value.ToString() },
                { "Remind_FollowUpAdvanceDays", nudFollowRemind.Value.ToString() },
                { "Template_EnableDefaultDiet", chkEnableDietTemplate.Checked ? "1" : "0" },
                { "Template_EnableDefaultExercise", chkEnableExerciseTemplate.Checked ? "1" : "0" }
            };

            // 保存配置
            this.Cursor = Cursors.WaitCursor;
            bool result = _bllConfig.SaveConfigBatch(configDict, "业务配置", Program.LoginUser.user_id, out string errorMsg);
            this.Cursor = Cursors.Default;

            if (result)
            {
                SystemGlobalConfig.RefreshConfig();
                _bllConfig.WriteAuditLog(Program.LoginUser.user_id, "修改系统配置", "修改糖尿病业务参数配置", Program.ClientIP);
                MessageBox.Show("糖尿病业务参数配置保存成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"保存失败：{errorMsg}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBizReset_Click(object sender, EventArgs e)
        {
            if (Program.LoginUser == null)
            {
                MessageBox.Show("登录信息失效，请重新登录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("确定要重置糖尿病业务参数为指南默认值吗？", "确认重置", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            bool result = _bllConfig.ResetConfigByType("业务配置", Program.LoginUser.user_id, out string errorMsg);
            this.Cursor = Cursors.Default;

            if (result)
            {
                SystemGlobalConfig.RefreshConfig();
                LoadBizConfig();
                _bllConfig.WriteAuditLog(Program.LoginUser.user_id, "重置系统配置", "重置糖尿病业务参数为默认值", Program.ClientIP);
                MessageBox.Show("糖尿病业务参数已重置为指南默认值！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"重置失败：{errorMsg}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Tab3 安全配置保存与重置
        private void BtnSecuritySave_Click(object sender, EventArgs e)
        {
            if (Program.LoginUser == null)
            {
                MessageBox.Show("登录信息失效，请重新登录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 组装配置字典
            Dictionary<string, string> configDict = new Dictionary<string, string>
            {
                { "Security_FirstLoginForceChangePwd", chkFirstLoginForcePwd.Checked ? "1" : "0" },
                { "Security_AllowMultiPlaceLogin", chkAllowMultiLogin.Checked ? "1" : "0" },
                { "Security_LoginFailMaxTimes", nudLoginFailTimes.Value.ToString() },
                { "Security_AccountLockMinutes", nudLockMinutes.Value.ToString() },
                { "Security_NoOperationAutoLogoutMinutes", nudNoOperationLogout.Value.ToString() },
                { "Security_EnableFullOperationLog", chkEnableFullLog.Checked ? "1" : "0" }
            };

            // 保存配置
            this.Cursor = Cursors.WaitCursor;
            bool result = _bllConfig.SaveConfigBatch(configDict, "安全配置", Program.LoginUser.user_id, out string errorMsg);
            this.Cursor = Cursors.Default;

            if (result)
            {
                SystemGlobalConfig.RefreshConfig();
                _bllConfig.WriteAuditLog(Program.LoginUser.user_id, "修改系统配置", "修改安全权限配置", Program.ClientIP);
                MessageBox.Show("安全权限配置保存成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"保存失败：{errorMsg}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSecurityReset_Click(object sender, EventArgs e)
        {
            if (Program.LoginUser == null)
            {
                MessageBox.Show("登录信息失效，请重新登录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("确定要重置安全权限配置为默认值吗？", "确认重置", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            bool result = _bllConfig.ResetConfigByType("安全配置", Program.LoginUser.user_id, out string errorMsg);
            this.Cursor = Cursors.Default;

            if (result)
            {
                SystemGlobalConfig.RefreshConfig();
                LoadSecurityConfig();
                _bllConfig.WriteAuditLog(Program.LoginUser.user_id, "重置系统配置", "重置安全权限配置为默认值", Program.ClientIP);
                MessageBox.Show("安全权限配置已重置为默认值！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"重置失败：{errorMsg}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Tab4 密码策略保存与重置
        private void BtnPwdSave_Click(object sender, EventArgs e)
        {
            if (Program.LoginUser == null)
            {
                MessageBox.Show("登录信息失效，请重新登录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 组装配置字典
            Dictionary<string, string> configDict = new Dictionary<string, string>
            {
                { "PwdPolicy_MinLength", nudPwdMinLength.Value.ToString() },
                { "PwdPolicy_RequireUpperLower", chkPwdUpperLower.Checked ? "1" : "0" },
                { "PwdPolicy_RequireNumber", chkPwdNumber.Checked ? "1" : "0" },
                { "PwdPolicy_RequireSpecialChar", chkPwdSpecialChar.Checked ? "1" : "0" },
                { "PwdPolicy_ForceChangeCycleDays", nudPwdChangeCycle.Value.ToString() },
                { "PwdPolicy_HistoryForbidRepeatCount", nudPwdHistoryCount.Value.ToString() }
            };

            // 保存配置
            this.Cursor = Cursors.WaitCursor;
            bool result = _bllConfig.SaveConfigBatch(configDict, "密码策略", Program.LoginUser.user_id, out string errorMsg);
            this.Cursor = Cursors.Default;

            if (result)
            {
                SystemGlobalConfig.RefreshConfig();
                _bllConfig.WriteAuditLog(Program.LoginUser.user_id, "修改系统配置", "修改密码策略配置", Program.ClientIP);
                MessageBox.Show("密码策略配置保存成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"保存失败：{errorMsg}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPwdReset_Click(object sender, EventArgs e)
        {
            if (Program.LoginUser == null)
            {
                MessageBox.Show("登录信息失效，请重新登录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("确定要重置密码策略配置为默认值吗？", "确认重置", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            bool result = _bllConfig.ResetConfigByType("密码策略", Program.LoginUser.user_id, out string errorMsg);
            this.Cursor = Cursors.Default;

            if (result)
            {
                SystemGlobalConfig.RefreshConfig();
                LoadPwdPolicyConfig();
                _bllConfig.WriteAuditLog(Program.LoginUser.user_id, "重置系统配置", "重置密码策略配置为默认值", Program.ClientIP);
                MessageBox.Show("密码策略配置已重置为默认值！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"重置失败：{errorMsg}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
        #endregion
    }
}