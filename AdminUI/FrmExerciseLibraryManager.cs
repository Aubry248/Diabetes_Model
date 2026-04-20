using BLL;
using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace AdminUI
{
    public partial class FrmExerciseLibraryManager : Form
    {
        #region ========== 全局统一布局参数（与干预效果评估页完全一致）==========
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
        private readonly int _globalContentOffsetX = 20;
        private readonly int _globalContentOffsetY = 20;
        /// <summary>
        /// 内容最小尺寸（加宽适配运动库多字段）
        /// </summary>
        private readonly int _globalContentMinWidth = 1200;
        private readonly int _globalContentMinHeight = 700;
        /// <summary>
        /// 控件统一参数（加宽标签适配长文本）
        /// </summary>
        private readonly Padding _globalControlMargin = new Padding(5, 5, 5, 5);
        private readonly int _globalControlHeight = 28;
        private readonly int _globalButtonHeight = 36;
        private readonly int _globalButtonWidth = 110;
        private readonly int _globalLabelWidth = 160;
        private readonly int _globalRowHeight = 40;
        private readonly Padding _globalGroupBoxPadding = new Padding(15);
        #endregion
        #region 业务逻辑全局变量
        private readonly B_ExerciseLibrary _bExerciseLib = new B_ExerciseLibrary();
        private readonly string _currentUser = "系统管理员1"; // 可从登录全局变量动态获取
        private int _currentEditExerciseId = 0; // 当前编辑的运动ID
        #endregion
        #region 核心控件声明（贴合糖尿病运动干预场景，补全缺失控件）
        private Panel pnlMainContainer;
        private Panel pnlContentWrapper;
        private TabControl tabMain;
        private TabPage tab_ExerciseList, tab_ExerciseEdit, tab_BatchOperate, tab_Audit, tab_Version;
        // 1. 运动库列表页（核心）
        private GroupBox grp_SearchFilter, grp_ExerciseList;
        private TextBox txt_SearchKey; // 运动名称/编码检索
        private ComboBox cbo_ExerciseType, cbo_Intensity, cbo_Suitable人群, cbo_EnableStatus;
        private NumericUpDown nud_CalorieMin, nud_CalorieMax; // 热量消耗区间
        private DateTimePicker dtp_UpdateStart, dtp_UpdateEnd;
        private DataGridView dgv_ExerciseList;
        private Button btn_Search, btn_ResetFilter, btn_AddSingle, btn_EditSelected, btn_DeleteSelected;
        // 2. 运动详情/编辑页
        private GroupBox grp_BaseInfo, grp_CoreParam, grp_DiabetesSafety, grp_ActionGuide, grp_StatusVersion;
        // 基础信息
        private TextBox txt_ExerciseCode, txt_ExerciseName, txt_Alias, txt_DemoUrl, txt_DataSource, txt_Remark; // 补全txt_Remark
        private ComboBox cbo_EditType;
        // 运动核心参数（MET值为核心）
        private TextBox txt_METValue, txt_CaloriePerKgH, txt_RecommendDuration, txt_RecommendFrequency, txt_Scene, txt_Equipment;
        // 糖尿病专属安全适配（核心必填）
        private TextBox txt_Suitable人群, txt_SafetyTip, txt_AdvancedAdapt;
        // 动作指导
        private TextBox txt_ActionSteps, txt_ForcePoint, txt_ErrorCorrect;
        // 状态版本管理
        private ComboBox cbo_EditEnableStatus;
        private TextBox txt_Version, txt_UpdateLog, txt_AuditRecord;
        private Button btn_SaveEdit, btn_CancelEdit;
        // 3. 批量操作页
        private GroupBox grp_BatchImport, grp_BatchOperate;
        private Button btn_ImportExcel, btn_ExportExcel, btn_BatchEnable, btn_BatchDisable;
        // 4. 审核页
        private GroupBox grp_AuditFilter, grp_AuditList;
        private ComboBox cbo_AuditStatus, cbo_Uploader;
        private DataGridView dgv_AuditList;
        private Button btn_QueryAudit, btn_AuditPass, btn_AuditReject, btn_ResetAudit;
        // 5. 版本管理页
        private GroupBox grp_VersionFilter, grp_VersionList;
        private ComboBox cbo_VersionExercise;
        private DataGridView dgv_VersionList;
        private Button btn_QueryVersion, btn_RollbackVersion, btn_ResetVersion;
        #endregion

        public FrmExerciseLibraryManager()
        {
            // 窗体基础配置（管理员端风格统一，贴合运动库定位）
            this.Text = "运动库管理（糖尿病运动干预核心库）";
            this.Size = new Size(1400, 850); // 加宽适配多字段展示
            this.MinimumSize = new Size(_globalContentMinWidth + 40, _globalContentMinHeight + 100);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("微软雅黑", 9.5F);
            this.Dock = DockStyle.Fill;

            // 全局主容器初始化（沿用统一架构）
            InitMainContainer();
            // 动态创建控件（贴合运动库业务）
            InitializeDynamicControls();
            // 初始化下拉数据（遵循《中国糖尿病患者运动指南》）
            InitControlData();
            // 绑定事件（核心业务逻辑入口）
            BindAllEvents();
            // 窗体加载时绑定下拉框数据和列表数据
            this.Load += (s, e) =>
            {
                BindComboBoxData();
                BindExerciseListData();
            };

            // 标签页切换时刷新对应数据
            tabMain.SelectedIndexChanged += (s, e) =>
            {
                if (tabMain.SelectedTab == tab_Audit)
                {
                    BindAuditListData();
                }
            };
            // 【修复8】子窗体关闭释放资源（防止内存泄漏导致跳转不稳定）
            this.FormClosed += (s, e) => { this.Dispose(); };
        }

        #region 全局容器初始化（统一布局架构，保证视觉一致性）
         private void InitMainContainer()
        {
            pnlMainContainer = new Panel();
            pnlMainContainer.Dock = DockStyle.Fill;
            pnlMainContainer.BackColor = Color.White;
            pnlMainContainer.Padding = _globalMainContainerPadding;
            pnlMainContainer.AutoScroll = true;
            this.Controls.Add(pnlMainContainer);

            pnlContentWrapper = new Panel();
            pnlContentWrapper.MinimumSize = new Size(_globalContentMinWidth, _globalContentMinHeight);
            pnlContentWrapper.Size = pnlContentWrapper.MinimumSize;
            pnlContentWrapper.BackColor = Color.White;
            pnlContentWrapper.Location = new Point(_globalContentOffsetX, _globalContentOffsetY);
            pnlMainContainer.Controls.Add(pnlContentWrapper);

            Action updateLocation = () =>
            {
                if (_globalContentAutoCenter)
                {
                    pnlContentWrapper.Left = Math.Max(0, (pnlMainContainer.ClientSize.Width - pnlContentWrapper.Width) / 2 + _globalContentOffsetX);
                    pnlContentWrapper.Top = Math.Max(0, (pnlMainContainer.ClientSize.Height - pnlContentWrapper.Height) / 2 + _globalContentOffsetY);
                }
                else
                {
                    pnlContentWrapper.Left = Math.Max(0, _globalContentOffsetX);
                    pnlContentWrapper.Top = Math.Max(0, _globalContentOffsetY);
                }
            };

            pnlMainContainer.Resize += (s, e) => updateLocation();
            this.Resize += (s, e) => updateLocation();
            this.Load += (s, e) => updateLocation();
        }
        #endregion

        #region 动态创建所有控件（贴合运动库业务场景）
        private void InitializeDynamicControls()
        {
            // 主标签控件（5大核心功能标签）
            tabMain = new TabControl();
            tabMain.Dock = DockStyle.Fill;
            tabMain.Font = new Font("微软雅黑", 10F);
            tabMain.Padding = new Point(15, 8);

            tab_ExerciseList = new TabPage("运动库列表") { BackColor = Color.White };
            tab_ExerciseEdit = new TabPage("运动详情/编辑") { BackColor = Color.White };
            tab_BatchOperate = new TabPage("批量操作") { BackColor = Color.White };
            tab_Audit = new TabPage("数据审核") { BackColor = Color.White };
            tab_Version = new TabPage("版本管理") { BackColor = Color.White };

            tabMain.TabPages.AddRange(new TabPage[] { tab_ExerciseList, tab_ExerciseEdit, tab_BatchOperate, tab_Audit, tab_Version });
            pnlContentWrapper.Controls.Add(tabMain);

            // 初始化各功能页面
            InitExerciseListPage();
            InitExerciseEditPage();
            InitBatchOperatePage();
            InitAuditPage();
            InitVersionPage();
        }

        // 1. 运动库列表页（核心检索筛选+列表展示）
        private void InitExerciseListPage()
        {
            // 检索与筛选区（贴合运动库检索需求）
            grp_SearchFilter = new GroupBox { Text = "检索与筛选条件", Dock = DockStyle.Top, Height = 220, Padding = _globalGroupBoxPadding };
            tab_ExerciseList.Controls.Add(grp_SearchFilter);

            TableLayoutPanel tlp_Filter = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Filter.RowCount = 4;
            tlp_Filter.ColumnCount = 3;
            for (int i = 0; i < 3; i++) tlp_Filter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
            for (int i = 0; i < 4; i++) tlp_Filter.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_SearchFilter.Controls.Add(tlp_Filter);

            // 第一行：全局检索（运动名称/编码）
            Label lbl_Search = new Label { Text = "运动名称/编码：", Size = new Size(_globalLabelWidth, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Margin = _globalControlMargin };
            txt_SearchKey = new TextBox { Size = new Size(280, _globalControlHeight), Margin = _globalControlMargin, Text = "输入运动名称/唯一编码检索" };
            Panel pnl_Search = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            lbl_Search.Location = new Point(0, 0);
            txt_SearchKey.Location = new Point(lbl_Search.Width, 0);
            pnl_Search.Controls.AddRange(new Control[] { lbl_Search, txt_SearchKey });
            tlp_Filter.Controls.Add(pnl_Search, 0, 0);
            tlp_Filter.SetColumnSpan(pnl_Search, 2);

            // 筛选条件：运动类型 + 运动强度
            int row = 0;
            CreateEditItem<ComboBox>(tlp_Filter, out _, out cbo_ExerciseType, "运动类型：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Filter, out _, out cbo_Intensity, "运动强度：", ref row, false);

            // 筛选条件：适用人群 + 热量消耗区间
            row = 1;
            CreateEditItem<ComboBox>(tlp_Filter, out _, out cbo_Suitable人群, "适用人群：", ref row, false);
            Panel pnl_CalorieRange = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_Calorie = new Label { Text = "消耗热量区间(kcal/kg·h)：", Size = new Size(140, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(0, 0) };
            nud_CalorieMin = new NumericUpDown { Location = new Point(140, 0), Size = new Size(80, _globalControlHeight), Minimum = 0, Maximum = 50, DecimalPlaces = 1 };
            Label lbl_To = new Label { Text = "~", Location = new Point(230, 5), AutoSize = true };
            nud_CalorieMax = new NumericUpDown { Location = new Point(250, 0), Size = new Size(80, _globalControlHeight), Minimum = 0, Maximum = 50, DecimalPlaces = 1, Value = 50 };
            pnl_CalorieRange.Controls.AddRange(new Control[] { lbl_Calorie, nud_CalorieMin, lbl_To, nud_CalorieMax });
            tlp_Filter.Controls.Add(pnl_CalorieRange, 2, 1);

            // 筛选条件：启用状态 + 更新时间范围
            row = 2;
            CreateEditItem<ComboBox>(tlp_Filter, out _, out cbo_EnableStatus, "启用状态：", ref row, false);
            Panel pnl_UpdateDate = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_Date = new Label { Text = "更新时间：", Size = new Size(80, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(0, 0) };
            dtp_UpdateStart = new DateTimePicker { Location = new Point(80, 0), Size = new Size(120, _globalControlHeight), Format = DateTimePickerFormat.Short };
            dtp_UpdateEnd = new DateTimePicker { Location = new Point(210, 0), Size = new Size(120, _globalControlHeight), Format = DateTimePickerFormat.Short };
            pnl_UpdateDate.Controls.AddRange(new Control[] { lbl_Date, dtp_UpdateStart, dtp_UpdateEnd });
            tlp_Filter.Controls.Add(pnl_UpdateDate, 2, 2);

            // 操作按钮区
            row = 3;
            Panel pnl_FilterBtn = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            FlowLayoutPanel flp_FilterBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_Search = CreateBtn("检索", Color.FromArgb(0, 122, 204));
            btn_ResetFilter = CreateBtn("重置筛选", Color.Gray);
            btn_AddSingle = CreateBtn("新增运动", Color.FromArgb(0, 150, 136));
            btn_EditSelected = CreateBtn("编辑选中", Color.FromArgb(255, 152, 0));
            btn_DeleteSelected = CreateBtn("删除选中", Color.FromArgb(244, 67, 54));
            flp_FilterBtn.Controls.AddRange(new Control[] { btn_Search, btn_ResetFilter, btn_AddSingle, btn_EditSelected, btn_DeleteSelected });
            pnl_FilterBtn.Controls.Add(flp_FilterBtn);
            tlp_Filter.Controls.Add(pnl_FilterBtn, 0, 3);
            tlp_Filter.SetColumnSpan(pnl_FilterBtn, 3);

            // 运动列表区（核心字段贴合糖尿病运动干预）
            grp_ExerciseList = new GroupBox { Text = "运动库列表（遵循《中国糖尿病患者运动指南》）", Dock = DockStyle.Fill, Padding = new Padding(15, 220, 15, 15) };
            tab_ExerciseList.Controls.Add(grp_ExerciseList);

            dgv_ExerciseList = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ColumnHeadersHeight = 35
            };
            grp_ExerciseList.Controls.Add(dgv_ExerciseList);
            // 列表核心字段（糖尿病运动干预专属，修正列名匹配数据库）
            dgv_ExerciseList.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { HeaderText = "运动唯一ID", DataPropertyName = "ExerciseID", Width = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "运动唯一编码", DataPropertyName = "ExerciseCode", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "运动名称", DataPropertyName = "ExerciseName", Width = 150 },
                new DataGridViewTextBoxColumn { HeaderText = "运动类型", DataPropertyName = "ExerciseCategory", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "运动强度", DataPropertyName = "IntensityCategory", Width = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "MET值", DataPropertyName = "MET_Value", Width = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "推荐单次时长(min)", DataPropertyName = "RecommendDuration", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "适用人群", DataPropertyName = "SuitablePeople", Width = 150 },
                new DataGridViewTextBoxColumn { HeaderText = "启用状态", DataPropertyName = "EnableStatus", Width = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "审核状态", DataPropertyName = "AuditStatus", Width = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "创建时间", DataPropertyName = "CreateTime", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "最后更新人", DataPropertyName = "UpdateUser", Width = 100 },
                new DataGridViewButtonColumn { HeaderText = "操作", Text = "编辑", UseColumnTextForButtonValue = true, Width = 80 }
            });
        }

        // 2. 运动详情/编辑页（分5大核心模块）
        private void InitExerciseEditPage()
        {
            // 基础信息区
            grp_BaseInfo = new GroupBox { Text = "基础信息", Dock = DockStyle.Top, Height = 180, Padding = _globalGroupBoxPadding };
            tab_ExerciseEdit.Controls.Add(grp_BaseInfo);

            TableLayoutPanel tlp_Base = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Base.RowCount = 3;
            tlp_Base.ColumnCount = 2;
            for (int i = 0; i < 2; i++) tlp_Base.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (int i = 0; i < 3; i++) tlp_Base.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_BaseInfo.Controls.Add(tlp_Base);

            int row = 0;
            CreateEditItem<TextBox>(tlp_Base, out _, out txt_ExerciseCode, "运动唯一编码：", ref row, true); // 编码只读
            CreateEditItem<TextBox>(tlp_Base, out _, out txt_ExerciseName, "运动名称：", ref row, false);
            CreateEditItem<TextBox>(tlp_Base, out _, out txt_Alias, "别名：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Base, out _, out cbo_EditType, "运动分类：", ref row, false);
            CreateEditItem<TextBox>(tlp_Base, out _, out txt_DemoUrl, "演示素材链接：", ref row, false);
            CreateEditItem<TextBox>(tlp_Base, out _, out txt_DataSource, "数据来源/参考指南：", ref row, false);

            // 运动核心参数区（MET值为核心，遵循指南）
            grp_CoreParam = new GroupBox { Text = "运动核心参数（MET值为强度核心指标）", Dock = DockStyle.Top, Height = 200, Padding = _globalGroupBoxPadding };
            tab_ExerciseEdit.Controls.Add(grp_CoreParam);

            TableLayoutPanel tlp_Core = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Core.RowCount = 3;
            tlp_Core.ColumnCount = 2;
            for (int i = 0; i < 2; i++) tlp_Core.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (int i = 0; i < 3; i++) tlp_Core.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_CoreParam.Controls.Add(tlp_Core);

            row = 0;
            CreateEditItem<TextBox>(tlp_Core, out _, out txt_METValue, "运动强度分级(MET值)：", ref row, false);
            CreateEditItem<TextBox>(tlp_Core, out _, out txt_CaloriePerKgH, "单位时间消耗热量(kcal/kg·h)：", ref row, false);
            CreateEditItem<TextBox>(tlp_Core, out _, out txt_RecommendDuration, "推荐单次时长(min)：", ref row, false);
            CreateEditItem<TextBox>(tlp_Core, out _, out txt_RecommendFrequency, "推荐运动频次(次/周)：", ref row, false);
            CreateEditItem<TextBox>(tlp_Core, out _, out txt_Scene, "运动场景：", ref row, false);
            CreateEditItem<TextBox>(tlp_Core, out _, out txt_Equipment, "所需器械：", ref row, false);

            // 糖尿病专属安全与适配信息区（核心必填，规避风险）
            grp_DiabetesSafety = new GroupBox { Text = "糖尿病专属安全与适配信息（核心必填）", Dock = DockStyle.Top, Height = 300, Padding = _globalGroupBoxPadding };
            tab_ExerciseEdit.Controls.Add(grp_DiabetesSafety);

            TableLayoutPanel tlp_Diabetes = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Diabetes.RowCount = 4;
            tlp_Diabetes.ColumnCount = 1;
            for (int i = 0; i < 4; i++) tlp_Diabetes.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            grp_DiabetesSafety.Controls.Add(tlp_Diabetes);

            // 适用/禁忌人群（多行文本）
            Label lbl_Suitable = new Label { Text = "适用/禁忌人群：", Size = new Size(_globalLabelWidth, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Margin = _globalControlMargin };
            txt_Suitable人群 = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, Margin = _globalControlMargin, Text = "适配：2型糖尿病、无严重合并症；禁忌：1型糖尿病血糖波动大、严重心血管并发症、糖尿病足等" };
            Panel pnl_Suitable = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            lbl_Suitable.Location = new Point(0, 0);
            txt_Suitable人群.Location = new Point(0, 30);
            txt_Suitable人群.Size = new Size(pnl_Suitable.Width - 10, 40);
            pnl_Suitable.Controls.AddRange(new Control[] { lbl_Suitable, txt_Suitable人群 });
            tlp_Diabetes.Controls.Add(pnl_Suitable, 0, 0);

            // 安全提示（多行文本，核心风险规避）
            Label lbl_Safety = new Label { Text = "安全提示：", Size = new Size(_globalLabelWidth, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Margin = _globalControlMargin };
            txt_SafetyTip = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, Margin = _globalControlMargin, Text = "1. 运动前血糖＜3.9mmol/L禁止运动；2. 运动中随身携带糖果，出现低血糖立即补充；3. 运动后30分钟监测血糖" };
            Panel pnl_Safety = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            lbl_Safety.Location = new Point(0, 0);
            txt_SafetyTip.Location = new Point(0, 30);
            txt_SafetyTip.Size = new Size(pnl_Safety.Width - 10, 40);
            pnl_Safety.Controls.AddRange(new Control[] { lbl_Safety, txt_SafetyTip });
            tlp_Diabetes.Controls.Add(pnl_Safety, 0, 1);

            // 进阶适配（多行文本）
            Label lbl_Advanced = new Label { Text = "进阶适配建议：", Size = new Size(_globalLabelWidth, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Margin = _globalControlMargin };
            txt_AdvancedAdapt = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, Margin = _globalControlMargin, Text = "合并肾病患者：降低运动强度；病程＞10年患者：缩短单次时长，增加频次" };
            Panel pnl_Advanced = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            lbl_Advanced.Location = new Point(0, 0);
            txt_AdvancedAdapt.Location = new Point(0, 30);
            txt_AdvancedAdapt.Size = new Size(pnl_Advanced.Width - 10, 40);
            pnl_Advanced.Controls.AddRange(new Control[] { lbl_Advanced, txt_AdvancedAdapt });
            tlp_Diabetes.Controls.Add(pnl_Advanced, 0, 2);

            // 饮食搭配建议
            Label lbl_Diet = new Label { Text = "运动前后饮食搭配：", Size = new Size(_globalLabelWidth, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Margin = _globalControlMargin };
            TextBox txt_DietSuggest = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, Margin = _globalControlMargin, Text = "运动前1小时可摄入15g碳水（如半片面包）；运动后30分钟补充20g碳水，避免低血糖" };
            Panel pnl_Diet = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            lbl_Diet.Location = new Point(0, 0);
            txt_DietSuggest.Location = new Point(0, 30);
            txt_DietSuggest.Size = new Size(pnl_Diet.Width - 10, 40);
            pnl_Diet.Controls.AddRange(new Control[] { lbl_Diet, txt_DietSuggest });
            tlp_Diabetes.Controls.Add(pnl_Diet, 0, 3);

            // 动作指导区
            grp_ActionGuide = new GroupBox { Text = "动作指导", Dock = DockStyle.Top, Height = 220, Padding = _globalGroupBoxPadding };
            tab_ExerciseEdit.Controls.Add(grp_ActionGuide);

            TableLayoutPanel tlp_Action = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Action.RowCount = 3;
            tlp_Action.ColumnCount = 1;
            for (int i = 0; i < 3; i++) tlp_Action.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            grp_ActionGuide.Controls.Add(tlp_Action);

            CreateEditItem<TextBox>(tlp_Action, out _, out txt_ActionSteps, "标准动作步骤：", ref row, false);
            txt_ActionSteps.Multiline = true;
            txt_ActionSteps.Height = 50;
            CreateEditItem<TextBox>(tlp_Action, out _, out txt_ForcePoint, "发力要点：", ref row, false);
            txt_ForcePoint.Multiline = true;
            txt_ForcePoint.Height = 50;
            CreateEditItem<TextBox>(tlp_Action, out _, out txt_ErrorCorrect, "常见错误纠正：", ref row, false);
            txt_ErrorCorrect.Multiline = true;
            txt_ErrorCorrect.Height = 50;

            // 状态与版本管理区
            grp_StatusVersion = new GroupBox { Text = "状态与版本管理", Dock = DockStyle.Top, Height = 180, Padding = _globalGroupBoxPadding };
            tab_ExerciseEdit.Controls.Add(grp_StatusVersion);

            TableLayoutPanel tlp_Status = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Status.RowCount = 2;
            tlp_Status.ColumnCount = 2;
            for (int i = 0; i < 2; i++) tlp_Status.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (int i = 0; i < 2; i++) tlp_Status.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_StatusVersion.Controls.Add(tlp_Status);

            row = 0;
            CreateEditItem<ComboBox>(tlp_Status, out _, out cbo_EditEnableStatus, "启用状态：", ref row, false);
            CreateEditItem<TextBox>(tlp_Status, out _, out txt_Version, "版本号：", ref row, true); // 版本号只读
            CreateEditItem<TextBox>(tlp_Status, out _, out txt_UpdateLog, "更新日志：", ref row, false);
            CreateEditItem<TextBox>(tlp_Status, out _, out txt_AuditRecord, "审核记录：", ref row, true); // 审核记录只读

            // 编辑操作按钮
            Panel pnl_EditBtn = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            tab_ExerciseEdit.Controls.Add(pnl_EditBtn);
            FlowLayoutPanel flp_EditBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_SaveEdit = CreateBtn("保存修改", Color.FromArgb(0, 122, 204));
            btn_CancelEdit = CreateBtn("取消编辑", Color.Gray);
            flp_EditBtn.Controls.AddRange(new Control[] { btn_SaveEdit, btn_CancelEdit });
            pnl_EditBtn.Controls.Add(flp_EditBtn);
        }

        // 3. 批量操作页（导入导出+批量状态操作）
        private void InitBatchOperatePage()
        {
            // 批量导入/导出区
            grp_BatchImport = new GroupBox { Text = "批量导入/导出（带合规校验）", Dock = DockStyle.Top, Height = 150, Padding = _globalGroupBoxPadding };
            tab_BatchOperate.Controls.Add(grp_BatchImport);

            Panel pnl_Import = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_ImportTip = new Label { Text = "支持Excel格式批量导入（模板含MET值、安全提示等必填项），导出所有/选中运动数据", Location = new Point(10, 10), AutoSize = true, Font = new Font("微软雅黑", 9F) };
            btn_ImportExcel = CreateBtn("批量导入", Color.FromArgb(0, 150, 136));
            btn_ImportExcel.Location = new Point(10, 50);
            btn_ExportExcel = CreateBtn("批量导出", Color.FromArgb(255, 152, 0));
            btn_ExportExcel.Location = new Point(130, 50);
            pnl_Import.Controls.AddRange(new Control[] { lbl_ImportTip, btn_ImportExcel, btn_ExportExcel });
            grp_BatchImport.Controls.Add(pnl_Import);

            // 批量操作区
            grp_BatchOperate = new GroupBox { Text = "批量状态操作", Dock = DockStyle.Top, Height = 150, Padding = _globalGroupBoxPadding };
            tab_BatchOperate.Controls.Add(grp_BatchOperate);

            Panel pnl_Operate = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_OperateTip = new Label { Text = "请先在【运动库列表】选中需要操作的记录，再执行以下批量操作", Location = new Point(10, 10), AutoSize = true, Font = new Font("微软雅黑", 9F) };
            btn_BatchEnable = CreateBtn("批量启用", Color.FromArgb(0, 122, 204));
            btn_BatchEnable.Location = new Point(10, 50);
            btn_BatchDisable = CreateBtn("批量禁用", Color.FromArgb(255, 152, 0));
            btn_BatchDisable.Location = new Point(130, 50);
            pnl_Operate.Controls.AddRange(new Control[] { lbl_OperateTip, btn_BatchEnable, btn_BatchDisable });
            grp_BatchOperate.Controls.Add(pnl_Operate);
        }

        // 4. 数据审核页（营养师/教练上传数据审核）
        private void InitAuditPage()
        {
            // 审核筛选区
            grp_AuditFilter = new GroupBox { Text = "审核筛选条件", Dock = DockStyle.Top, Height = 150, Padding = _globalGroupBoxPadding };
            tab_Audit.Controls.Add(grp_AuditFilter);

            TableLayoutPanel tlp_AuditFilter = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_AuditFilter.RowCount = 1;
            tlp_AuditFilter.ColumnCount = 3;
            for (int i = 0; i < 3; i++) tlp_AuditFilter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
            tlp_AuditFilter.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_AuditFilter.Controls.Add(tlp_AuditFilter);

            int row = 0;
            CreateEditItem<ComboBox>(tlp_AuditFilter, out _, out cbo_AuditStatus, "审核状态：", ref row, false);
            CreateEditItem<ComboBox>(tlp_AuditFilter, out _, out cbo_Uploader, "上传人类型：", ref row, false);

            // 审核按钮区
            Panel pnl_AuditBtn = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            FlowLayoutPanel flp_AuditBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            btn_QueryAudit = CreateBtn("查询待审核", Color.FromArgb(0, 122, 204));
            btn_ResetAudit = CreateBtn("重置", Color.Gray);
            flp_AuditBtn.Controls.AddRange(new Control[] { btn_ResetAudit, btn_QueryAudit });
            pnl_AuditBtn.Controls.Add(flp_AuditBtn);
            tlp_AuditFilter.Controls.Add(pnl_AuditBtn, 2, 0);

            // 审核列表区
            grp_AuditList = new GroupBox { Text = "待审核运动数据列表", Dock = DockStyle.Fill, Padding = _globalGroupBoxPadding };
            tab_Audit.Controls.Add(grp_AuditList);

            dgv_AuditList = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            grp_AuditList.Controls.Add(dgv_AuditList);
            dgv_AuditList.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { HeaderText = "运动编码", DataPropertyName = "ExerciseCode", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "运动名称", DataPropertyName = "ExerciseName", Width = 150 },
                new DataGridViewTextBoxColumn { HeaderText = "上传人", DataPropertyName = "Uploader", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "上传时间", DataPropertyName = "UploadTime", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "审核状态", DataPropertyName = "AuditStatus", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "核心校验提示", DataPropertyName = "CheckTip", Width = 200 }
            });

            // 审核操作按钮
            Panel pnl_AuditOperate = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(15) };
            tab_Audit.Controls.Add(pnl_AuditOperate);
            FlowLayoutPanel flp_AuditOperate = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_AuditPass = CreateBtn("审核通过", Color.FromArgb(0, 150, 136));
            btn_AuditReject = CreateBtn("审核驳回", Color.FromArgb(244, 67, 54));
            flp_AuditOperate.Controls.AddRange(new Control[] { btn_AuditPass, btn_AuditReject });
            pnl_AuditOperate.Controls.Add(flp_AuditOperate);
        }

        // 5. 版本管理页（版本追溯+回滚）
        private void InitVersionPage()
        {
            // 版本筛选区
            grp_VersionFilter = new GroupBox { Text = "版本筛选条件", Dock = DockStyle.Top, Height = 150, Padding = _globalGroupBoxPadding };
            tab_Version.Controls.Add(grp_VersionFilter);

            TableLayoutPanel tlp_VersionFilter = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_VersionFilter.RowCount = 1;
            tlp_VersionFilter.ColumnCount = 2;
            for (int i = 0; i < 2; i++) tlp_VersionFilter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_VersionFilter.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_VersionFilter.Controls.Add(tlp_VersionFilter);

            int row = 0;
            CreateEditItem<ComboBox>(tlp_VersionFilter, out _, out cbo_VersionExercise, "选择运动：", ref row, false);

            // 版本按钮区
            Panel pnl_VersionBtn = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            FlowLayoutPanel flp_VersionBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            btn_QueryVersion = CreateBtn("查询版本", Color.FromArgb(0, 122, 204));
            btn_ResetVersion = CreateBtn("重置", Color.Gray);
            flp_VersionBtn.Controls.AddRange(new Control[] { btn_ResetVersion, btn_QueryVersion });
            pnl_VersionBtn.Controls.Add(flp_VersionBtn);
            tlp_VersionFilter.Controls.Add(pnl_VersionBtn, 1, 0);

            // 版本列表区
            grp_VersionList = new GroupBox { Text = "运动版本记录（支持追溯/回滚）", Dock = DockStyle.Fill, Padding = _globalGroupBoxPadding };
            tab_Version.Controls.Add(grp_VersionList);

            dgv_VersionList = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            grp_VersionList.Controls.Add(dgv_VersionList);
            dgv_VersionList.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { HeaderText = "版本号", DataPropertyName = "Version", Width = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "更新时间", DataPropertyName = "UpdateTime", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "更新人", DataPropertyName = "UpdateUser", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "更新内容", DataPropertyName = "UpdateContent", Width = 200 },
                new DataGridViewTextBoxColumn { HeaderText = "审核状态", DataPropertyName = "AuditStatus", Width = 100 }
            });

            // 版本操作按钮
            Panel pnl_VersionOperate = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(15) };
            tab_Version.Controls.Add(pnl_VersionOperate);
            FlowLayoutPanel flp_VersionOperate = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_RollbackVersion = CreateBtn("版本回滚", Color.FromArgb(255, 152, 0));
            flp_VersionOperate.Controls.Add(btn_RollbackVersion);
            pnl_VersionOperate.Controls.Add(flp_VersionOperate);
        }
        #endregion

        #region 通用控件创建方法（适配运动库长标签）
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

        private void CreateEditItem<T>(TableLayoutPanel tlp, out Label lbl, out T ctrl, string text, ref int row, bool readOnly = false) where T : Control, new()
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
                // 占位提示贴合运动库场景
                t.Text = text.Contains("编码") ? "系统自动生成" : (text.Contains("MET") ? "如：3.5（低强度）、7.0（中强度）" : "");
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
        #endregion

        #region 下拉数据初始化（严格遵循《中国糖尿病患者运动指南》）
        private void InitControlData()
        {
            // 运动类型（指南标准分类）
            string[] exerciseTypes = new string[] { "全部", "有氧运动", "力量训练", "柔韧性训练", "平衡训练", "混合训练" };
            // 运动强度（按MET值分级，指南标准）
            string[] intensities = new string[] { "全部", "低强度(MET<3.0)", "中强度(3.0≤MET<6.0)", "高强度(MET≥6.0)" };
            // 适用人群（糖尿病分型+合并症）
            string[] suitableGroups = new string[] { "全部", "2型糖尿病无合并症", "2型糖尿病合并轻度高血压", "1型糖尿病血糖稳定", "老年糖尿病患者", "糖尿病前期人群" };
            // 启用状态
            string[] enableStatus = new string[] { "全部", "启用", "禁用" };
            // 审核状态
            string[] auditStatus = new string[] { "全部", "待审核", "审核通过", "审核驳回" };
            // 上传人类型
            string[] uploaders = new string[] { "全部", "营养师", "运动教练", "管理员", "用户" };
            // 运动名称（示例）
            string[] exerciseNames = new string[] { "全部", "快走", "慢跑", "游泳", "哑铃训练", "太极拳", "瑜伽" };

            // 列表页下拉
            cbo_ExerciseType.Items.AddRange(exerciseTypes);
            cbo_Intensity.Items.AddRange(intensities);
            cbo_Suitable人群.Items.AddRange(suitableGroups);
            cbo_EnableStatus.Items.AddRange(enableStatus);

            // 编辑页下拉
            cbo_EditType.Items.AddRange(exerciseTypes);
            cbo_EditEnableStatus.Items.AddRange(enableStatus);

            // 审核页下拉
            cbo_AuditStatus.Items.AddRange(auditStatus);
            cbo_Uploader.Items.AddRange(uploaders);

            // 版本页下拉
            cbo_VersionExercise.Items.AddRange(exerciseNames);

            // 默认选中（贴合常用场景）
            cbo_ExerciseType.SelectedIndex = 0;
            cbo_Intensity.SelectedIndex = 0;
            cbo_Suitable人群.SelectedIndex = 0;
            cbo_EnableStatus.SelectedIndex = 0;
            cbo_EditType.SelectedIndex = 0;
            cbo_EditEnableStatus.SelectedIndex = 1; // 默认启用
            cbo_AuditStatus.SelectedIndex = 1; // 默认待审核
            cbo_Uploader.SelectedIndex = 0;
            cbo_VersionExercise.SelectedIndex = 0;

            // 默认时间
            dtp_UpdateStart.Value = DateTime.Now.AddMonths(-3);
            dtp_UpdateEnd.Value = DateTime.Now;

            // 默认热量区间
            nud_CalorieMin.Value = 0;
            nud_CalorieMax.Value = 50;
        }
        #endregion

        #region 事件绑定（核心业务逻辑入口，贴合运动库风险管控）
        #region 事件绑定（真实业务逻辑实现）
        private void BindAllEvents()
        {
            #region 运动库列表页按钮事件
            // 检索按钮
            btn_Search.Click += (s, e) => BindExerciseListData();

            // 重置筛选按钮
            btn_ResetFilter.Click += (s, e) =>
            {
                txt_SearchKey.Text = "输入运动名称/唯一编码检索";
                cbo_ExerciseType.SelectedIndex = 0;
                cbo_Intensity.SelectedIndex = 0;
                cbo_Suitable人群.SelectedIndex = 0;
                cbo_EnableStatus.SelectedIndex = 0;
                nud_CalorieMin.Value = 0;
                nud_CalorieMax.Value = 50;
                dtp_UpdateStart.Value = DateTime.Now.AddMonths(-3);
                dtp_UpdateEnd.Value = DateTime.Now;
                BindExerciseListData();
            };

            // 新增运动按钮
            btn_AddSingle.Click += (s, e) =>
            {
                // 清空编辑页
                _currentEditExerciseId = 0;
                txt_ExerciseCode.Text = "系统自动生成";
                txt_ExerciseName.Clear();
                txt_Alias.Clear();
                cbo_EditType.SelectedIndex = 0;
                txt_DataSource.Text = "中国糖尿病患者运动指南";
                txt_METValue.Clear();
                txt_RecommendDuration.Text = "30分钟/次";
                txt_RecommendFrequency.Text = "5次/周";
                txt_Suitable人群.Text = "2型糖尿病、糖尿病前期人群";
                txt_SafetyTip.Text = "1. 运动前监测血糖，血糖＜3.9mmol/L禁止运动；2. 随身携带糖果，预防低血糖";
                txt_AdvancedAdapt.Clear();
                txt_ActionSteps.Clear();
                txt_ForcePoint.Clear();
                txt_ErrorCorrect.Clear();
                cbo_EditEnableStatus.SelectedIndex = 1;
                txt_Version.Text = "V1.0.0";
                txt_UpdateLog.Clear();
                txt_AuditRecord.Clear();
                txt_Remark.Clear();

                // 切换到编辑页
                tabMain.SelectedTab = tab_ExerciseEdit;
            };

            // 编辑选中按钮
            btn_EditSelected.Click += (s, e) =>
            {
                if (dgv_ExerciseList.SelectedRows.Count == 0 || dgv_ExerciseList.SelectedRows[0].IsNewRow)
                {
                    MessageBox.Show("请先选中一条要编辑的运动记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 获取选中的运动ID（修正列名）
                DataGridViewRow selectedRow = dgv_ExerciseList.SelectedRows[0];
                int exerciseId = Convert.ToInt32(selectedRow.Cells["ExerciseID"].Value);
                LoadExerciseDetailToEdit(exerciseId);
            };


            // 删除选中按钮
            btn_DeleteSelected.Click += (s, e) =>
            {
                if (dgv_ExerciseList.SelectedRows.Count == 0 || dgv_ExerciseList.SelectedRows[0].IsNewRow)
                {
                    MessageBox.Show("请先选中一条要删除的运动记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataGridViewRow selectedRow = dgv_ExerciseList.SelectedRows[0];
                int exerciseId = Convert.ToInt32(selectedRow.Cells["ExerciseID"].Value);
                string exerciseName = selectedRow.Cells["ExerciseName"].Value.ToString();

                if (MessageBox.Show($"确认删除运动【{exerciseName}】？删除后不可恢复！", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var result = _bExerciseLib.DeleteExercise(exerciseId);
                    if (result.IsSuccess)
                    {
                        MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        BindExerciseListData();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            // 列表行双击编辑
            dgv_ExerciseList.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && !dgv_ExerciseList.Rows[e.RowIndex].IsNewRow)
                {
                    int exerciseId = Convert.ToInt32(dgv_ExerciseList.Rows[e.RowIndex].Cells["ExerciseID"].Value);
                    LoadExerciseDetailToEdit(exerciseId);
                }
            };
            #endregion

            #region 运动编辑页按钮事件
            // 保存修改按钮
            btn_SaveEdit.Click += (s, e) =>
            {
                try
                {
                    // 基础校验
                    if (string.IsNullOrWhiteSpace(txt_ExerciseName.Text))
                    {
                        MessageBox.Show("运动名称不能为空", "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!decimal.TryParse(txt_METValue.Text, out decimal metValue) || metValue <= 0)
                    {
                        MessageBox.Show("MET值必须为大于0的数字", "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 构建实体
                    ExerciseEnergyExpenditure model = new ExerciseEnergyExpenditure
                    {
                        ExerciseID = _currentEditExerciseId,
                        ExerciseName = txt_ExerciseName.Text.Trim(),
                        Alias = txt_Alias.Text.Trim(),
                        ExerciseCategory = cbo_EditType.SelectedItem?.ToString() ?? "有氧运动",
                        MET_Value = metValue,
                        IntensityCategory = metValue < 3.0m ? "低强度" : metValue < 6.0m ? "中强度" : "高强度",
                        IsDiabetesFriendly = txt_Suitable人群.Text.Contains("禁用") ? "否" : "是",
                        ExerciseDesc = txt_ActionSteps.Text.Trim(),
                        StandardSource = txt_DataSource.Text.Trim(),
                        RecommendDuration = txt_RecommendDuration.Text.Trim(),
                        RecommendFrequency = txt_RecommendFrequency.Text.Trim(),
                        SuitablePeople = txt_Suitable人群.Text.Trim(),
                        ForbiddenPeople = txt_AdvancedAdapt.Text.Trim(),
                        SafetyTip = txt_SafetyTip.Text.Trim(),
                        Remark = txt_Remark.Text.Trim(),
                        EnableStatus = cbo_EditEnableStatus.SelectedItem?.ToString() ?? "启用",
                        Version = txt_Version.Text.Trim()
                    };

                    BizResult result;
                    if (_currentEditExerciseId == 0)
                    {
                        // 新增
                        result = _bExerciseLib.AddExercise(model, _currentUser);
                    }
                    else
                    {
                        // 修改
                        result = _bExerciseLib.UpdateExercise(model, _currentUser, txt_UpdateLog.Text.Trim());
                    }

                    if (result.IsSuccess)
                    {
                        MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        tabMain.SelectedTab = tab_ExerciseList;
                        BindExerciseListData();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 取消编辑按钮
            btn_CancelEdit.Click += (s, e) =>
            {
                tabMain.SelectedTab = tab_ExerciseList;
            };
            #endregion

            #region 批量操作页按钮事件
            // 批量启用按钮
            btn_BatchEnable.Click += (s, e) =>
            {
                if (dgv_ExerciseList.SelectedRows.Count == 0)
                {
                    MessageBox.Show("请先在运动库列表选中要操作的记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                List<int> idList = new List<int>();
                foreach (DataGridViewRow row in dgv_ExerciseList.SelectedRows)
                {
                    if (!row.IsNewRow)
                    {
                        idList.Add(Convert.ToInt32(row.Cells["ExerciseID"].Value));
                    }
                }

                var result = _bExerciseLib.BatchUpdateEnableStatus(idList, "启用", _currentUser);
                if (result.IsSuccess)
                {
                    MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    BindExerciseListData();
                }
                else
                {
                    MessageBox.Show(result.Message, "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 批量禁用按钮
            btn_BatchDisable.Click += (s, e) =>
            {
                if (dgv_ExerciseList.SelectedRows.Count == 0)
                {
                    MessageBox.Show("请先在运动库列表选中要操作的记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                List<int> idList = new List<int>();
                foreach (DataGridViewRow row in dgv_ExerciseList.SelectedRows)
                {
                    if (!row.IsNewRow)
                    {
                        idList.Add(Convert.ToInt32(row.Cells["ExerciseID"].Value));
                    }
                }

                var result = _bExerciseLib.BatchUpdateEnableStatus(idList, "禁用", _currentUser);
                if (result.IsSuccess)
                {
                    MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    BindExerciseListData();
                }
                else
                {
                    MessageBox.Show(result.Message, "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 批量导入/导出按钮保留原有逻辑，可扩展Excel导入导出
            #endregion

            #region 审核页按钮事件
            // 查询待审核按钮
            btn_QueryAudit.Click += (s, e) => BindAuditListData();

            // 审核通过按钮
            btn_AuditPass.Click += (s, e) =>
            {
                if (dgv_AuditList.SelectedRows.Count == 0)
                {
                    MessageBox.Show("请先选中要审核的记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                List<int> idList = new List<int>();
                foreach (DataGridViewRow row in dgv_AuditList.SelectedRows)
                {
                    idList.Add(Convert.ToInt32(row.Cells["ExerciseID"].Value));
                }

                var result = _bExerciseLib.BatchAuditExercise(idList, "审核通过", $"审核通过，操作人：{_currentUser}", _currentUser);
                if (result.IsSuccess)
                {
                    MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    BindAuditListData();
                    BindExerciseListData();
                }
                else
                {
                    MessageBox.Show(result.Message, "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 审核驳回按钮
            btn_AuditReject.Click += (s, e) =>
            {
                if (dgv_AuditList.SelectedRows.Count == 0)
                {
                    MessageBox.Show("请先选中要审核的记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string rejectReason = Microsoft.VisualBasic.Interaction.InputBox("请输入驳回原因：", "审核驳回", "", 0, 0);
                if (string.IsNullOrWhiteSpace(rejectReason))
                {
                    MessageBox.Show("驳回原因不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                List<int> idList = new List<int>();
                foreach (DataGridViewRow row in dgv_AuditList.SelectedRows)
                {
                    idList.Add(Convert.ToInt32(row.Cells["ExerciseID"].Value));
                }

                var result = _bExerciseLib.BatchAuditExercise(idList, "审核驳回", $"驳回原因：{rejectReason}，操作人：{_currentUser}", _currentUser);
                if (result.IsSuccess)
                {
                    MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    BindAuditListData();
                }
                else
                {
                    MessageBox.Show(result.Message, "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 重置审核筛选按钮
            btn_ResetAudit.Click += (s, e) =>
            {
                cbo_AuditStatus.SelectedIndex = 1;
                cbo_Uploader.SelectedIndex = 0;
                BindAuditListData();
            };
            #endregion

            #region 版本管理页按钮事件
            // 查询版本按钮
            btn_QueryVersion.Click += (s, e) =>
            {
                if (cbo_VersionExercise.SelectedIndex <= 0)
                {
                    MessageBox.Show("请先选择要查询的运动", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 获取运动ID
                var nameResult = _bExerciseLib.GetAllExerciseNameList();
                if (nameResult.IsSuccess)
                {
                    DataTable dt = nameResult.Data as DataTable;
                    DataRow[] rows = dt.Select($"ExerciseName = '{cbo_VersionExercise.SelectedItem}'");
                    if (rows.Length > 0)
                    {
                        int exerciseId = Convert.ToInt32(rows[0]["ExerciseID"]);
                        BindVersionHistoryData(exerciseId);
                    }
                }
            };

            // 重置版本筛选按钮
            btn_ResetVersion.Click += (s, e) =>
            {
                cbo_VersionExercise.SelectedIndex = 0;
                dgv_VersionList.DataSource = null;
            };
            #endregion
        }
        #endregion
        #endregion

        #region 核心业务数据绑定方法
        /// <summary>
        /// 绑定运动库列表数据
        /// </summary>
        private void BindExerciseListData()
        {
            try
            {
                // 获取筛选条件
                string searchKey = txt_SearchKey.Text.Trim();
                string category = cbo_ExerciseType.SelectedItem?.ToString() ?? "全部";
                string intensity = cbo_Intensity.SelectedItem?.ToString() ?? "全部";
                string suitablePeople = cbo_Suitable人群.SelectedItem?.ToString() ?? "全部";
                string enableStatus = cbo_EnableStatus.SelectedItem?.ToString() ?? "全部";
                DateTime updateStart = dtp_UpdateStart.Value.Date;
                DateTime updateEnd = dtp_UpdateEnd.Value.Date;

                // 调用BLL查询数据
                var result = _bExerciseLib.GetExerciseList(searchKey, category, intensity, suitablePeople, enableStatus, updateStart, updateEnd);
                if (!result.IsSuccess)
                {
                    MessageBox.Show(result.Message, "数据加载失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dgv_ExerciseList.DataSource = null;
                    return;
                }

                // 绑定数据到DataGridView
                DataTable dt = result.Data as DataTable;
                dgv_ExerciseList.DataSource = dt;
                dgv_ExerciseList.ClearSelection();

                // 更新状态栏
                this.Text = $"糖尿病患者综合健康管理系统 - 管理员后台 - 运动库管理 | 共{result.TotalCount}条数据";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载运动库数据失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dgv_ExerciseList.DataSource = null;
            }
        }

        /// <summary>
        /// 绑定下拉框数据源
        /// </summary>
        private void BindComboBoxData()
        {
            try
            {
                // 绑定运动分类下拉框
                var categoryResult = _bExerciseLib.GetAllExerciseCategory();
                if (categoryResult.IsSuccess)
                {
                    DataTable dt = categoryResult.Data as DataTable;
                    cbo_ExerciseType.Items.Clear();
                    cbo_ExerciseType.Items.Add("全部");
                    cbo_EditType.Items.Clear();
                    foreach (DataRow dr in dt.Rows)
                    {
                        cbo_ExerciseType.Items.Add(dr["ExerciseCategory"].ToString());
                        cbo_EditType.Items.Add(dr["ExerciseCategory"].ToString());
                    }
                    cbo_ExerciseType.SelectedIndex = 0;
                    cbo_EditType.SelectedIndex = 0;
                }

                // 绑定版本管理运动名称下拉框
                var nameResult = _bExerciseLib.GetAllExerciseNameList();
                if (nameResult.IsSuccess)
                {
                    DataTable dt = nameResult.Data as DataTable;
                    cbo_VersionExercise.Items.Clear();
                    cbo_VersionExercise.Items.Add("全部");
                    foreach (DataRow dr in dt.Rows)
                    {
                        cbo_VersionExercise.Items.Add(dr["ExerciseName"].ToString());
                    }
                    cbo_VersionExercise.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载下拉框数据失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 加载运动详情到编辑页
        /// </summary>
        private void LoadExerciseDetailToEdit(int exerciseId)
        {
            try
            {
                var result = _bExerciseLib.GetExerciseDetail(exerciseId);
                if (!result.IsSuccess)
                {
                    MessageBox.Show(result.Message, "加载失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var model = result.Data as ExerciseEnergyExpenditure;
                _currentEditExerciseId = model.ExerciseID;

                // 填充基础信息
                txt_ExerciseCode.Text = model.ExerciseCode;
                txt_ExerciseName.Text = model.ExerciseName;
                txt_Alias.Text = model.Alias;
                cbo_EditType.SelectedItem = model.ExerciseCategory;
                txt_DataSource.Text = model.StandardSource;

                // 填充核心参数
                txt_METValue.Text = model.MET_Value.ToString();
                txt_RecommendDuration.Text = model.RecommendDuration;
                txt_RecommendFrequency.Text = model.RecommendFrequency;

                // 填充安全信息
                txt_Suitable人群.Text = model.SuitablePeople;
                txt_SafetyTip.Text = model.SafetyTip;
                txt_AdvancedAdapt.Text = model.ForbiddenPeople;

                // 填充动作指导
                txt_ActionSteps.Text = model.ExerciseDesc;
                txt_Remark.Text = model.Remark;

                // 填充状态版本
                cbo_EditEnableStatus.SelectedItem = model.EnableStatus;
                txt_Version.Text = model.Version;
                txt_AuditRecord.Text = model.AuditRecord;

                // 切换到编辑页
                tabMain.SelectedTab = tab_ExerciseEdit;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载运动详情失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 绑定审核列表数据
        /// </summary>
        private void BindAuditListData()
        {
            try
            {
                string auditStatus = cbo_AuditStatus.SelectedItem?.ToString() ?? "全部";
                string uploader = cbo_Uploader.SelectedItem?.ToString() ?? "全部";
                var result = _bExerciseLib.GetAuditList(auditStatus, uploader);
                if (!result.IsSuccess)
                {
                    MessageBox.Show(result.Message, "加载失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dgv_AuditList.DataSource = null;
                    return;
                }
                dgv_AuditList.DataSource = result.Data as DataTable;
                dgv_AuditList.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载审核列表失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 绑定版本历史数据
        /// </summary>
        private void BindVersionHistoryData(int exerciseId)
        {
            try
            {
                var result = _bExerciseLib.GetVersionHistory(exerciseId);
                if (!result.IsSuccess)
                {
                    MessageBox.Show(result.Message, "加载失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dgv_VersionList.DataSource = null;
                    return;
                }
                dgv_VersionList.DataSource = result.Data as DataTable;
                dgv_VersionList.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载版本历史失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}