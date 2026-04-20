using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using BLL;
using Model;

namespace DoctorUI
{
    public partial class FrmInterventionPlan : Form
    {
        #region ========== 全局统一布局参数（与健康评估/档案管理页完全一致，无改动）==========
        /// <summary>
        /// 主容器内边距
        /// </summary>
        private readonly Padding _globalMainContainerPadding = new Padding(15, 15, 15, 15);
        /// <summary>
        /// 内容自动居中
        /// </summary>
        private readonly bool _globalContentAutoCenter = false;
        /// <summary>
        /// 整体偏移（往右/往下移动）
        /// </summary>
        private readonly int _globalContentOffsetX = 140;
        private readonly int _globalContentOffsetY = 75;
        /// <summary>
        /// 内容最小尺寸
        /// </summary>
        private readonly int _globalContentMinWidth = 1300;
        private readonly int _globalContentMinHeight = 800;
        /// <summary>
        /// 控件统一参数
        /// </summary>
        private readonly Padding _globalControlMargin = new Padding(5, 5, 5, 5);
        private readonly int _globalControlHeight = 28;
        private readonly int _globalButtonHeight = 36;
        private readonly int _globalButtonWidth = 110;
        private readonly int _globalLabelWidth = 120;
        private readonly int _globalRowHeight = 40;
        private readonly Padding _globalGroupBoxPadding = new Padding(15);
        #endregion

        #region 核心控件声明（原有控件无改动，仅新增健康信息展示控件）
        private Panel pnlMainContainer;
        private Panel pnlContentWrapper;
        private TabControl tabMain;
        private TabPage tab_PlanCreate, tab_PlanIssue, tab_PlanAdjust;

        // 干预方案制定 - 原有控件
        private GroupBox grp_PatientSelect, grp_PlanContent;
        private ComboBox cbo_Patient, cbo_PlanType;
        private TextBox txt_PlanContent;
        private Button btn_SavePlan, btn_ResetPlan;

        // 干预方案制定 - 新增：患者健康评估信息展示控件
        private GroupBox grp_PatientHealthInfo;
        private Label lbl_FastingGlucose, lbl_PostGlucose, lbl_Hba1c, lbl_BloodPressure, lbl_BMI, lbl_HealthLevel;

        // 方案下发管理 - 原有控件
        private GroupBox grp_PlanList;
        private DataGridView dgv_PlanList;
        private Button btn_IssuePlan, btn_ViewPlan;

        // 方案调整记录 - 原有控件
        private GroupBox grp_AdjustQuery, grp_AdjustList;
        private ComboBox cbo_AdjustPatient;
        private DateTimePicker dtp_AdjustStart, dtp_AdjustEnd;
        private DataGridView dgv_AdjustList;
        private Button btn_QueryAdjust, btn_ResetAdjust;

        /// <summary>
        /// 当前绑定的患者对象
        /// </summary>
        private PatientBindInfo _currentPatient;
        private Panel pnl_PatientBindBar;
        private Label lbl_CurrentPatient;
        private ComboBox cbo_PatientSelect;
        private Button btn_BindPatient;
        #endregion

        #region 业务层对象与全局变量
        private readonly B_InterventionPlan _planBll = new B_InterventionPlan();
        private readonly B_HealthAssessment _healthBll = new B_HealthAssessment();
        // 当前登录医生ID（请与项目全局登录逻辑保持一致，登录后赋值）
        private readonly int _currentDoctorId = 1;
        #endregion

        public FrmInterventionPlan()
        {
            // 窗体基础配置（与其他页面统一，无改动）
            this.Text = "个性化干预管理";
            this.Size = new Size(1300, 800);
            this.MinimumSize = new Size(_globalContentMinWidth + 40, _globalContentMinHeight + 100);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("微软雅黑", 9.5F);
            this.Dock = DockStyle.Fill;

            // 全局主容器初始化（无改动）
            InitMainContainer();
            // 动态创建控件（新增健康信息控件）
            InitializeDynamicControls();
            // 初始化下拉数据（改为真实数据库加载）
            InitControlData();
            // 绑定事件（完整业务逻辑）
            BindAllEvents();
        }

        #region 全局容器初始化（终极定格：切Tab完全不动）
        private void InitMainContainer()
        {
            // 主滚动容器
            pnlMainContainer = new Panel();
            pnlMainContainer.Dock = DockStyle.Fill;
            pnlMainContainer.BackColor = Color.White;
            pnlMainContainer.Padding = _globalMainContainerPadding;
            pnlMainContainer.AutoScroll = false; // ❌ 关闭自动滚动，杜绝跳动
            pnlMainContainer.HorizontalScroll.Visible = false;
            pnlMainContainer.VerticalScroll.Visible = false;
            this.Controls.Add(pnlMainContainer);

            // 内容包裹容器（绝对固定，死位置）
            pnlContentWrapper = new Panel();
            pnlContentWrapper.MinimumSize = new Size(_globalContentMinWidth, _globalContentMinHeight);
            pnlContentWrapper.Size = pnlContentWrapper.MinimumSize;
            pnlContentWrapper.BackColor = Color.White;
            // ✅ 死固定位置，绝不改变
            pnlContentWrapper.Location = new Point(_globalContentOffsetX, _globalContentOffsetY);
            pnlMainContainer.Controls.Add(pnlContentWrapper);

            // ✅ 只加载时定位1次，之后永不执行任何位置计算
            this.Load += (s, e) =>
            {
                pnlContentWrapper.Location = new Point(_globalContentOffsetX, _globalContentOffsetY);
            };

            // ❌ 彻底删除所有Resize/重绘相关绑定，杜绝跳动
        }
        #endregion

        #region 动态创建所有控件（仅新增健康信息分组，原有控件无改动）
        private void InitializeDynamicControls()
        {

            // 主标签控件（无改动）
            tabMain = new TabControl();
            tabMain.Dock = DockStyle.Fill;
            tabMain.Font = new Font("微软雅黑", 10F);
            tabMain.Padding = new Point(15, 8);
            tab_PlanCreate = new TabPage("干预方案制定") { BackColor = Color.White };
            tab_PlanIssue = new TabPage("方案下发管理") { BackColor = Color.White };
            tab_PlanAdjust = new TabPage("方案调整记录") { BackColor = Color.White };
            tabMain.TabPages.AddRange(new TabPage[] { tab_PlanCreate, tab_PlanIssue, tab_PlanAdjust });
            pnlContentWrapper.Controls.Add(tabMain);

            // 初始化三个功能页面
            InitPlanCreatePage();
            InitPlanIssuePage();
            InitPlanAdjustPage();
        }

        // 1. 干预方案制定页面（新增健康信息分组，原有布局顺序微调，控件无改动）
        private void InitPlanCreatePage()
        {
            // 1. 患者选择分组（原有控件，无改动）
            grp_PatientSelect = new GroupBox { Text = "患者与方案类型", Dock = DockStyle.Top, Height = 120, Padding = _globalGroupBoxPadding };
            tab_PlanCreate.Controls.Add(grp_PatientSelect);
            TableLayoutPanel tlp_Patient = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Patient.RowCount = 1;
            tlp_Patient.ColumnCount = 2;
            tlp_Patient.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_Patient.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_Patient.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_PatientSelect.Controls.Add(tlp_Patient);
            int row = 0;
            CreateEditItem<ComboBox>(tlp_Patient, out _, out cbo_Patient, "选择患者：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Patient, out _, out cbo_PlanType, "方案类型：", ref row, false);

            // 2. 新增：患者最新健康评估信息分组（放在空余区域，不影响原有布局）
            grp_PatientHealthInfo = new GroupBox { Text = "患者最新健康评估信息", Dock = DockStyle.Top, Height = 100, Padding = _globalGroupBoxPadding };
            tab_PlanCreate.Controls.Add(grp_PatientHealthInfo);
            TableLayoutPanel tlp_Health = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Health.RowCount = 1;
            tlp_Health.ColumnCount = 6;
            for (int i = 0; i < 6; i++)
            {
                tlp_Health.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.6F));
            }
            tlp_Health.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_PatientHealthInfo.Controls.Add(tlp_Health);

            // 创建健康信息只读标签
            lbl_FastingGlucose = CreateHealthLabel("空腹血糖：-- mmol/L");
            lbl_PostGlucose = CreateHealthLabel("餐后血糖：-- mmol/L");
            lbl_Hba1c = CreateHealthLabel("糖化血红蛋白：-- %");
            lbl_BloodPressure = CreateHealthLabel("血压：--/-- mmHg");
            lbl_BMI = CreateHealthLabel("BMI：--");
            lbl_HealthLevel = CreateHealthLabel("健康等级：--");

            tlp_Health.Controls.Add(lbl_FastingGlucose, 0, 0);
            tlp_Health.Controls.Add(lbl_PostGlucose, 1, 0);
            tlp_Health.Controls.Add(lbl_Hba1c, 2, 0);
            tlp_Health.Controls.Add(lbl_BloodPressure, 3, 0);
            tlp_Health.Controls.Add(lbl_BMI, 4, 0);
            tlp_Health.Controls.Add(lbl_HealthLevel, 5, 0);

            // 3. 方案内容分组（原有控件，无改动）
            grp_PlanContent = new GroupBox { Text = "干预方案内容", Dock = DockStyle.Top, Height = 300, Padding = _globalGroupBoxPadding };
            tab_PlanCreate.Controls.Add(grp_PlanContent);
            txt_PlanContent = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = this.Font,
                Margin = _globalControlMargin
            };
            grp_PlanContent.Controls.Add(txt_PlanContent);

            // 4. 按钮区（原有控件，无改动）
            Panel pnl_PlanBtn = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(15) };
            tab_PlanCreate.Controls.Add(pnl_PlanBtn);
            FlowLayoutPanel flp_PlanBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_SavePlan = CreateBtn("保存方案", Color.FromArgb(0, 122, 204));
            btn_ResetPlan = CreateBtn("重置", Color.Gray);
            flp_PlanBtn.Controls.AddRange(new Control[] { btn_SavePlan, btn_ResetPlan });
            pnl_PlanBtn.Controls.Add(flp_PlanBtn);
        }

        // 2. 方案下发管理页面（完全无改动）
        private void InitPlanIssuePage()
        {
            // 方案列表分组
            grp_PlanList = new GroupBox { Text = "待下发干预方案列表", Dock = DockStyle.Fill, Padding = _globalGroupBoxPadding };
            tab_PlanIssue.Controls.Add(grp_PlanList);
            dgv_PlanList = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false
            };
            grp_PlanList.Controls.Add(dgv_PlanList);

            // 添加列表列（无改动）
            dgv_PlanList.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { HeaderText = "患者姓名", DataPropertyName = "PatientName", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "方案类型", DataPropertyName = "PlanType", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "创建时间", DataPropertyName = "CreateTime", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "下发状态", DataPropertyName = "IssueStatus", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
            });

            // 操作按钮区（底部，无改动）
            Panel pnl_IssueBtn = new Panel { Dock = DockStyle.Bottom, Height = 80, Padding = new Padding(15) };
            tab_PlanIssue.Controls.Add(pnl_IssueBtn);
            FlowLayoutPanel flp_IssueBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            btn_ViewPlan = CreateBtn("查看详情", Color.FromArgb(0, 150, 136));
            btn_IssuePlan = CreateBtn("下发方案", Color.FromArgb(255, 152, 0));
            flp_IssueBtn.Controls.AddRange(new Control[] { btn_IssuePlan, btn_ViewPlan });
            pnl_IssueBtn.Controls.Add(flp_IssueBtn);
        }

        // 3. 方案调整记录页面（完全无改动）
        private void InitPlanAdjustPage()
        {
            // 查询条件分组
            grp_AdjustQuery = new GroupBox { Text = "查询条件", Dock = DockStyle.Top, Height = 120, Padding = _globalGroupBoxPadding };
            tab_PlanAdjust.Controls.Add(grp_AdjustQuery);
            TableLayoutPanel tlp_Query = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Query.RowCount = 1;
            tlp_Query.ColumnCount = 3;
            tlp_Query.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlp_Query.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlp_Query.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            tlp_Query.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_AdjustQuery.Controls.Add(tlp_Query);
            int row = 0;
            CreateEditItem<ComboBox>(tlp_Query, out _, out cbo_AdjustPatient, "选择患者：", ref row, false);

            // 时间范围控件
            Panel pnl_DateRange = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_Date = new Label { Text = "调整时间：", Size = new Size(80, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(0, 0) };
            dtp_AdjustStart = new DateTimePicker { Location = new Point(80, 0), Size = new Size(140, _globalControlHeight), Format = DateTimePickerFormat.Short };
            dtp_AdjustEnd = new DateTimePicker { Location = new Point(230, 0), Size = new Size(140, _globalControlHeight), Format = DateTimePickerFormat.Short };
            pnl_DateRange.Controls.AddRange(new Control[] { lbl_Date, dtp_AdjustStart, dtp_AdjustEnd });
            tlp_Query.Controls.Add(pnl_DateRange, 1, 0);

            // 查询按钮区
            Panel pnl_QueryBtn = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            FlowLayoutPanel flp_QueryBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            btn_QueryAdjust = CreateBtn("查询", Color.FromArgb(0, 122, 204));
            btn_ResetAdjust = CreateBtn("重置", Color.Gray);
            flp_QueryBtn.Controls.AddRange(new Control[] { btn_ResetAdjust, btn_QueryAdjust });
            pnl_QueryBtn.Controls.Add(flp_QueryBtn);
            tlp_Query.Controls.Add(pnl_QueryBtn, 2, 0);

            // 调整记录列表分组
            grp_AdjustList = new GroupBox { Text = "方案调整记录列表", Dock = DockStyle.Fill, Padding = _globalGroupBoxPadding };
            tab_PlanAdjust.Controls.Add(grp_AdjustList);
            dgv_AdjustList = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false
            };
            grp_AdjustList.Controls.Add(dgv_AdjustList);

            // 添加列表列（无改动）
            dgv_AdjustList.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { HeaderText = "患者姓名", DataPropertyName = "PatientName", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "方案类型", DataPropertyName = "PlanType", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "调整时间", DataPropertyName = "AdjustTime", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "调整内容", DataPropertyName = "AdjustContent", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "调整医生", DataPropertyName = "AdjustDoctor", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "版本号", DataPropertyName = "VersionNo", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells }
            });
        }
        #endregion

        #region 通用控件创建方法（与其他页面完全统一，无改动）
        private Button CreateBtn(string text, Color backColor)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.BackColor = backColor;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.Size = new Size(_globalButtonWidth, _globalButtonHeight);
            btn.Font = this.Font;
            btn.Margin = _globalControlMargin;
            return btn;
        }

        private void CreateEditItem<T>(TableLayoutPanel tlp, out Label lbl, out T ctrl, string text, ref int row, bool readOnly) where T : Control, new()
        {
            lbl = new Label
            {
                Text = text,
                Size = new Size(_globalLabelWidth, _globalControlHeight),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = _globalControlMargin
            };
            ctrl = new T();
            ctrl.Size = new Size(260, _globalControlHeight);
            ctrl.Margin = _globalControlMargin;
            if (ctrl is TextBox t)
            {
                t.ReadOnly = readOnly;
                t.BackColor = readOnly ? Color.FromArgb(245, 245, 245) : Color.White;
            }
            if (ctrl is DateTimePicker d)
            {
                d.Format = DateTimePickerFormat.Short;
                d.Enabled = !readOnly;
            }
            if (ctrl is ComboBox c)
            {
                c.DropDownStyle = ComboBoxStyle.DropDownList;
                c.Enabled = !readOnly;
            }
            Panel pairPanel = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            lbl.Location = new Point(0, 0);
            ctrl.Location = new Point(lbl.Width, 0);
            pairPanel.Controls.AddRange(new Control[] { lbl, ctrl });
            tlp.Controls.Add(pairPanel, row % 2 == 0 ? 0 : 1, row / 2);
            row++;
        }

        // 新增：健康信息标签创建辅助方法
        private Label CreateHealthLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("微软雅黑", 9F),
                ForeColor = Color.FromArgb(64, 64, 64)
            };
        }
        #endregion

        #region 下拉数据初始化（改为真实数据库加载，替换模拟数据）
        private void InitControlData()
        {
            // 1. 方案类型下拉框（与数据库约束一致，无改动）
            cbo_PlanType.Items.AddRange(new string[] { "饮食干预", "运动干预", "用药干预" });
            cbo_PlanType.SelectedIndex = 0;

            // 2. 患者下拉框 - 从数据库真实加载
            try
            {
                DataTable patientDt = _planBll.GetPatientList();
                // 绑定患者下拉框
                cbo_Patient.DataSource = patientDt.Copy();
                cbo_Patient.DisplayMember = "user_name";
                cbo_Patient.ValueMember = "user_id";

                // 绑定调整记录患者下拉框
                cbo_AdjustPatient.DataSource = patientDt.Copy();
                cbo_AdjustPatient.DisplayMember = "user_name";
                cbo_AdjustPatient.ValueMember = "user_id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载患者列表失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // 3. 时间范围默认值（无改动）
            dtp_AdjustStart.Value = DateTime.Now.AddMonths(-1);
            dtp_AdjustEnd.Value = DateTime.Now;

            // 4. 页面加载时，加载待下发方案列表
            LoadPlanList();
        }
        #endregion

        #region 核心业务方法
        /// <summary>
        /// 加载待下发方案列表
        /// </summary>
        private void LoadPlanList()
        {
            try
            {
                DataTable planDt = _planBll.GetWaitIssuePlanList();
                dgv_PlanList.DataSource = planDt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载方案列表失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 加载患者最新健康评估信息
        /// </summary>
        private void LoadPatientHealthInfo(int userId)
        {
            try
            {
                // 重置标签
                ResetHealthLabel();

                // 获取最新评估信息
                var healthInfo = _planBll.GetPatientLatestHealthInfo(userId);
                if (healthInfo == null)
                {
                    MessageBox.Show("该患者暂无健康评估记录，请先完成健康评估", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 赋值健康信息标签
                lbl_FastingGlucose.Text = $"空腹血糖：{healthInfo.avg_fasting_glucose?.ToString("F1") ?? "--"} mmol/L";
                lbl_PostGlucose.Text = $"餐后血糖：{healthInfo.avg_postprandial_glucose?.ToString("F1") ?? "--"} mmol/L";
                lbl_Hba1c.Text = $"糖化血红蛋白：{healthInfo.hba1c?.ToString("F1") ?? "--"} %";
                lbl_BloodPressure.Text = $"血压：{healthInfo.systolic_bp?.ToString() ?? "--"}/{healthInfo.diastolic_bp?.ToString() ?? "--"} mmHg";
                lbl_BMI.Text = $"BMI：{healthInfo.bmi?.ToString("F1") ?? "--"}";

                // 计算健康等级
                var (_, healthLevel, _) = _healthBll.CalcHealthComprehensiveScore(
                    healthInfo.avg_fasting_glucose, healthInfo.avg_postprandial_glucose, healthInfo.hba1c,
                    healthInfo.systolic_bp, healthInfo.diastolic_bp, healthInfo.bmi);
                // 替换为以下完整兼容代码
                Color healthColor;
                switch (healthLevel)
                {
                    case HealthLevel.优秀:
                        healthColor = Color.FromArgb(0, 128, 0);
                        break;
                    case HealthLevel.合格:
                        healthColor = Color.FromArgb(255, 152, 0);
                        break;
                    case HealthLevel.不合格:
                        healthColor = Color.FromArgb(220, 0, 0);
                        break;
                    default:
                        healthColor = Color.FromArgb(64, 64, 64);
                        break;
                }
                lbl_HealthLevel.ForeColor = healthColor;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载患者健康信息失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 重置健康信息标签
        /// </summary>
        private void ResetHealthLabel()
        {
            lbl_FastingGlucose.Text = "空腹血糖：-- mmol/L";
            lbl_PostGlucose.Text = "餐后血糖：-- mmol/L";
            lbl_Hba1c.Text = "糖化血红蛋白：-- %";
            lbl_BloodPressure.Text = "血压：--/-- mmHg";
            lbl_BMI.Text = "BMI：--";
            lbl_HealthLevel.Text = "健康等级：--";
            lbl_HealthLevel.ForeColor = Color.FromArgb(64, 64, 64);
        }
        #endregion

        #region 事件绑定（完整业务逻辑，替换原有模拟代码）
        private void BindAllEvents()
        {
            // ========== 干预方案制定页面事件 ==========
            // 患者选择联动 - 加载健康信息
            cbo_Patient.SelectedIndexChanged += (s, e) =>
            {
                if (cbo_Patient.SelectedValue != null && int.TryParse(cbo_Patient.SelectedValue.ToString(), out int userId))
                {
                    LoadPatientHealthInfo(userId);
                }
            };

            // 保存方案按钮
            btn_SavePlan.Click += (s, e) =>
            {
                try
                {
                    // 入参校验
                    if (cbo_Patient.SelectedValue == null || !int.TryParse(cbo_Patient.SelectedValue.ToString(), out int userId))
                    {
                        MessageBox.Show("请选择有效患者", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(txt_PlanContent.Text))
                    {
                        MessageBox.Show("请填写干预方案内容", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 构建方案实体
                    InterventionPlan plan = new InterventionPlan
                    {
                        user_id = userId,
                        plan_type = cbo_PlanType.SelectedItem.ToString(),
                        plan_content = txt_PlanContent.Text.Trim(),
                        expected_effect = "控制患者血糖、血压等核心指标达标，降低糖尿病并发症风险",
                        start_time = DateTime.Now.Date,
                        end_time = DateTime.Now.Date.AddMonths(1),
                        review_time = DateTime.Now.Date.AddDays(14),
                        create_by = _currentDoctorId
                    };

                    // 调用业务层保存
                    int planId = _planBll.SaveInterventionPlan(plan);
                    if (planId > 0)
                    {
                        MessageBox.Show($"干预方案保存成功！方案编号：{planId}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // 重置表单
                        btn_ResetPlan.PerformClick();
                        // 刷新方案列表
                        LoadPlanList();
                    }
                    else
                    {
                        MessageBox.Show("方案保存失败，请重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show(ex.Message, "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存方案失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 重置按钮
            btn_ResetPlan.Click += (s, e) =>
            {
                if (cbo_Patient.Items.Count > 0) cbo_Patient.SelectedIndex = 0;
                cbo_PlanType.SelectedIndex = 0;
                txt_PlanContent.Clear();
                ResetHealthLabel();
            };

            // ========== 方案下发管理页面事件 ==========
            // 查看详情按钮
            btn_ViewPlan.Click += (s, e) =>
            {
                if (dgv_PlanList.SelectedRows.Count == 0 || dgv_PlanList.SelectedRows[0].Cells["plan_id"].Value == null)
                {
                    MessageBox.Show("请先选择一条方案", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    int planId = Convert.ToInt32(dgv_PlanList.SelectedRows[0].Cells["plan_id"].Value);
                    var plan = _planBll.GetPlanById(planId);
                    if (plan == null)
                    {
                        MessageBox.Show("方案不存在或已被删除", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 弹出详情窗体
                    string detailContent = $"方案编号：{plan.plan_id}\r\n" +
                                           $"患者ID：{plan.user_id}\r\n" +
                                           $"方案类型：{plan.plan_type}\r\n" +
                                           $"方案内容：\r\n{plan.plan_content}\r\n\r\n" +
                                           $"预期效果：{plan.expected_effect}\r\n" +
                                           $"执行周期：{plan.start_time:yyyy-MM-dd} 至 {plan.end_time:yyyy-MM-dd}\r\n" +
                                           $"复查时间：{plan.review_time:yyyy-MM-dd}\r\n" +
                                           $"创建时间：{plan.create_time:yyyy-MM-dd HH:mm}\r\n" +
                                           $"当前状态：{(plan.status == 0 ? "待下发" : plan.status == 1 ? "已下发" : "已结束")}";

                    MessageBox.Show(detailContent, "方案详情", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"查看详情失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 下发方案按钮
            btn_IssuePlan.Click += (s, e) =>
            {
                if (dgv_PlanList.SelectedRows.Count == 0 || dgv_PlanList.SelectedRows[0].Cells["plan_id"].Value == null)
                {
                    MessageBox.Show("请先选择一条待下发方案", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    int planId = Convert.ToInt32(dgv_PlanList.SelectedRows[0].Cells["plan_id"].Value);
                    bool result = _planBll.IssueInterventionPlan(planId);
                    if (result)
                    {
                        MessageBox.Show("方案下发成功！已同步至患者端", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // 刷新列表
                        LoadPlanList();
                    }
                    else
                    {
                        MessageBox.Show("方案下发失败，请重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show(ex.Message, "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"下发方案失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // ========== 方案调整记录页面事件 ==========
            // 查询按钮
            btn_QueryAdjust.Click += (s, e) =>
            {
                try
                {
                    // 入参校验
                    if (cbo_AdjustPatient.SelectedValue == null || !int.TryParse(cbo_AdjustPatient.SelectedValue.ToString(), out int userId))
                    {
                        MessageBox.Show("请选择有效患者", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DateTime startTime = dtp_AdjustStart.Value;
                    DateTime endTime = dtp_AdjustEnd.Value;
                    if (startTime > endTime)
                    {
                        MessageBox.Show("开始时间不能晚于结束时间", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 查询数据
                    DataTable adjustDt = _planBll.GetPatientPlanAdjustRecords(userId, startTime, endTime);
                    dgv_AdjustList.DataSource = adjustDt;

                    if (adjustDt.Rows.Count == 0)
                    {
                        MessageBox.Show("该时间段内暂无方案调整记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show(ex.Message, "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"查询调整记录失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 重置按钮
            btn_ResetAdjust.Click += (s, e) =>
            {
                if (cbo_AdjustPatient.Items.Count > 0) cbo_AdjustPatient.SelectedIndex = 0;
                dtp_AdjustStart.Value = DateTime.Now.AddMonths(-1);
                dtp_AdjustEnd.Value = DateTime.Now;
                dgv_AdjustList.DataSource = null;
            };
        }
        #endregion

    }
}