using BLL;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DoctorUI
{
    public partial class FrmPatientRecordManage : Form
    {
        #region ========== 全局可调节布局参数（一键修改全界面，无需改动下方代码）==========
        /// <summary>
        /// 主容器内边距（上下左右）- 控制窗体与内容区的整体留白
        /// </summary>
        private readonly Padding _globalMainContainerPadding = new Padding(10, 10, 10, 10);
        /// <summary>
        /// 内容区是否在主容器中自动居中（开启后偏移参数会叠加在居中位置上）
        /// </summary>
        private readonly bool _globalContentAutoCenter = false;
        /// <summary>
        /// 【重点：控制整体往右/往下偏移】正数往右/往下，负数往左/往上，改这里直接生效
        /// </summary>
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
        /// 控件之间的统一外边距（上下左右）- 控制控件间的间隔
        /// </summary>
        private readonly Padding _globalControlMargin = new Padding(5, 5, 5, 5);
        /// <summary>
        /// 统一输入控件高度（文本框、下拉框、日期选择器）
        /// </summary>
        private readonly int _globalControlHeight = 28;
        /// <summary>
        /// 统一按钮高度
        /// </summary>
        private readonly int _globalButtonHeight = 50;
        /// <summary>
        /// 统一按钮宽度
        /// </summary>
        private readonly int _globalButtonWidth = 110;
        /// <summary>
        /// 统一标签宽度（所有Label固定宽度，保证对齐）
        /// </summary>
        private readonly int _globalLabelWidth = 80;
        /// <summary>
        /// 全局行高（垂直方向控件的标准行间距）
        /// </summary>
        private readonly int _globalRowHeight = 40;
        /// <summary>
        /// 分组框(GroupBox)统一内边距
        /// </summary>
        private readonly Padding _globalGroupBoxPadding = new Padding(15);
        /// <summary>
        /// 分页控件统一高度
        /// </summary>
        private readonly int _globalPageControlHeight = 45;
        #endregion

        #region 全局业务对象（完全保留原业务逻辑，无修改）
        private readonly B_PatientRecord _bllPatient = new B_PatientRecord();
        private int _pageIndex = 1;
        private readonly int _pageSize = 20;
        private int _totalCount = 0;
        private int _selectUserId = 0;
        private bool _isViewDetailMode = false;
        #endregion

        #region 核心控件声明（完全保留原控件名称，保证业务逻辑兼容）
        private Panel pnlMainContainer;
        private Panel pnlContentWrapper; // 全局内容容器，所有子页面挂载于此
        private TabControl tabMain;
        private TabPage tabPage_List, tabPage_Edit, tabPage_History;

        // 列表页控件
        private GroupBox grp_Query;
        private Label lbl_Name, lbl_Phone, lbl_DiabetesType, lbl_ControlStatus, lbl_DateRange;
        private TextBox txt_Name, txt_Phone;
        private ComboBox cbo_DiabetesType, cbo_ControlStatus;
        private DateTimePicker dtp_StartDate, dtp_EndDate;
        private CheckBox chk_OnlyValid;
        private Button btn_Query, btn_Reset, btn_Detail, btn_Edit, btn_History;
        private DataGridView dgv_PatientList;
        private Label lbl_PageInfo;
        private Button btn_Prev, btn_Next;

        // 维护页控件
        private SplitContainer split_Edit;
        private GroupBox grp_BaseInfo, grp_Disease, grp_Assessment;
        private TextBox txt_Base_Name, txt_Base_ID, txt_Base_Phone, txt_Base_Age, txt_Base_Emergency, txt_Base_EmerPhone;
        private ComboBox cbo_Gender, cbo_Diabetes, cbo_ControlStatusEdit;
        private DateTimePicker dtp_Diagnose, dtp_AssessDate;
        private TextBox txt_Baseline, txt_Duration, txt_Height, txt_Weight, txt_BMI, txt_Systolic, txt_Diastolic, txt_Heart, txt_HbA1c, txt_AvgFast, txt_AvgPost;
        private TextBox txt_Complication, txt_Comorbidity;
        private Button btn_Save, btn_Cancel;

        // 历史页控件
        private GroupBox grp_HistoryQuery;
        private DateTimePicker dtp_HistoryStart, dtp_HistoryEnd;
        private ComboBox cbo_RecordType;
        private Button btn_HistoryQuery, btn_HistoryReset, btn_Export;
        private DataGridView dgv_HistoryList;
        private Label lbl_HistoryPatient;

        private ErrorProvider errorProviderMain;
        /// <summary>
        /// 列表页-日期范围快捷选择下拉框
        /// </summary>
        private ComboBox cbo_ListDateQuick;
        /// <summary>
        /// 历史追溯页-日期范围快捷选择下拉框
        /// </summary>
        private ComboBox cbo_HistoryDateQuick;
        /// <summary>
        /// 历史追溯页-患者选择相关控件
        /// </summary>
        private Label lbl_HistorySelectPatient;
        private TextBox txt_HistoryPatientSearch;
        private ComboBox cbo_HistoryPatientList;
        #endregion

        public FrmPatientRecordManage()
        {
            // 窗体基础属性（适配嵌入场景，不强制固定尺寸）
            this.Text = "患者档案管理";
            this.Size = new Size(1400, 850);
            this.MinimumSize = new Size(_globalContentMinWidth + 40, _globalContentMinHeight + 100);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("微软雅黑", 9.5F);
            this.Dock = DockStyle.Fill; // 适配嵌入主窗体场景，自动填满父容器

            // ========== 新增：校验控件初始化 ==========
            errorProviderMain = new ErrorProvider();
            errorProviderMain.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            errorProviderMain.ContainerControl = this;
            // ========== 修复：主容器初始化，解决Padding不生效问题 ==========
            pnlMainContainer = new Panel();
            pnlMainContainer.Dock = DockStyle.Fill;
            pnlMainContainer.BackColor = Color.White;
            pnlMainContainer.Padding = _globalMainContainerPadding;
            pnlMainContainer.Margin = Padding.Empty;
            pnlMainContainer.AutoScroll = true; // 内容超出自动出现滚动条
            this.Controls.Add(pnlMainContainer);
            // ========== 修复：内容容器初始化，初始位置直接赋值，解决左上角贴边问题 ==========
            pnlContentWrapper = new Panel();
            pnlContentWrapper.MinimumSize = new Size(_globalContentMinWidth, _globalContentMinHeight);
            pnlContentWrapper.Size = pnlContentWrapper.MinimumSize;
            pnlContentWrapper.BackColor = Color.White;
            // 初始位置直接按偏移参数赋值，不管Resize事件有没有触发，都不会贴左上角
            pnlContentWrapper.Location = new Point(_globalContentOffsetX, _globalContentOffsetY);
            pnlMainContainer.Controls.Add(pnlContentWrapper);
            // ========== 修复：窗体大小变化时，自动更新位置，适配缩放 ==========
            Action updateContentLocation = () =>
            {
                if (_globalContentAutoCenter)
                {
                    // 开启居中：居中位置 + 偏移量
                    pnlContentWrapper.Left = Math.Max(0, (pnlMainContainer.ClientSize.Width - pnlContentWrapper.Width) / 2 + _globalContentOffsetX);
                    pnlContentWrapper.Top = Math.Max(0, (pnlMainContainer.ClientSize.Height - pnlContentWrapper.Height) / 2 + _globalContentOffsetY);
                }
                else
                {
                    // 关闭居中：直接用偏移量控制位置
                    pnlContentWrapper.Left = Math.Max(0, _globalContentOffsetX);
                    pnlContentWrapper.Top = Math.Max(0, _globalContentOffsetY);
                }
            };
            // 绑定所有可能触发尺寸变化的事件，保证位置始终正确
            this.Load += (s, e) => updateContentLocation();

            // ========== 【关键】严格遵循初始化顺序：先实例化控件 → 再初始化数据 → 最后绑定事件 ==========
            InitializeDynamicControls(); // 实例化所有页面控件，包括lbl_HistoryPatient
            InitControlData(); // 给下拉框填充选项
            BindAllEvents(); // 最后绑定事件，保证控件全部实例化完成
        }

        #region 动态创建控件（重构布局，彻底抛弃绝对定位）
        private void InitializeDynamicControls()
        {
            // 主TabControl：全部挂载到全局内容容器
            tabMain = new TabControl();
            tabMain.Dock = DockStyle.Fill;
            tabMain.Font = new Font("微软雅黑", 10F);
            tabMain.Padding = new Point(15, 8);
            tabPage_List = new TabPage("患者档案列表") { BackColor = Color.White };
            tabPage_Edit = new TabPage("档案信息维护") { BackColor = Color.White };
            tabPage_History = new TabPage("健康档案追溯") { BackColor = Color.White };
            tabMain.TabPages.AddRange(new[] { tabPage_List, tabPage_Edit, tabPage_History });
            pnlContentWrapper.Controls.Add(tabMain);

            // 初始化各子页面
            InitPatientListPage();
            InitArchiveEditPage();
            InitHistoryPage();
        }

        /// <summary>
        /// 患者列表页：网格布局，无绝对定位
        /// </summary>
        /// <summary>
        /// 患者列表页：升级-增加日期快捷选择、行内操作按钮
        /// </summary>
        /// <summary>
        /// 患者列表页：修复布局顺序、表头遮挡、列表下移控制
        /// </summary>
        private void InitPatientListPage()
        {
            // ======================================
            // 【核心修复1：正确的Dock控件添加顺序】
            // 1. 先加 列表专属容器（Dock.Fill，最底层）
            // 2. 再加 底部分页栏（Dock.Bottom，中层）
            // 3. 最后加 顶部查询栏（Dock.Top，最上层）
            // ======================================

            // ========== 1. 先创建【列表专属容器】：控制列表下移的核心，改这里的Padding直接生效 ==========
            Panel pnl_ListContainer = new Panel
            {
                Dock = DockStyle.Fill,
                // 👇 【控制列表下移的唯一参数】Padding(左, 上, 右, 下)
                // 第二个数字=列表往下移多少，改成50就下移50，改成80就下移80，100%生效
                Padding = new Padding(0, 50, 0, 0),
                Margin = Padding.Empty,
                BackColor = Color.White
            };
            // 【第一步就把容器加到TabPage，保证Dock.Fill层级正确】
            tabPage_List.Controls.Add(pnl_ListContainer);

            // ========== 2. 再创建分页控件（Dock.Bottom，原有代码完全保留） ==========
            Panel pnl_Page = new Panel { Dock = DockStyle.Bottom, Height = _globalPageControlHeight, BackColor = Color.FromArgb(245, 245, 245) };
            tabPage_List.Controls.Add(pnl_Page);
            lbl_PageInfo = new Label { Text = "第1页/共0页 总计0条", Location = new Point(20, 12), Size = new Size(300, 20) };
            btn_Prev = CreateBtn("上一页", Color.White);
            btn_Next = CreateBtn("下一页", Color.White);
            btn_Prev.ForeColor = btn_Next.ForeColor = Color.Black;
            pnl_Page.Controls.AddRange(new Control[] { lbl_PageInfo, btn_Prev, btn_Next });
            Action updatePaginationButtons = () =>
            {
                btn_Prev.Location = new Point(pnl_Page.Width - 240, 8);
                btn_Next.Location = new Point(pnl_Page.Width - 120, 8);
            };
            pnl_Page.Resize += (s, e) => updatePaginationButtons();
            updatePaginationButtons();

            // ========== 3. 最后创建查询条件分组框（Dock.Top，原有代码完全保留，仅修复高度计算） ==========
            grp_Query = new GroupBox { Text = "查询条件", Dock = DockStyle.Top, Padding = _globalGroupBoxPadding };
            // 修复高度：2行内容，每行_globalRowHeight，加边距，避免高度溢出遮挡列表
            grp_Query.Height = _globalRowHeight * 2 + 60;
            tabPage_List.Controls.Add(grp_Query);
            TableLayoutPanel tlp_Query = new TableLayoutPanel();
            tlp_Query.Dock = DockStyle.Fill;
            tlp_Query.RowCount = 2;
            tlp_Query.ColumnCount = 4;
            tlp_Query.Margin = Padding.Empty;
            tlp_Query.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlp_Query.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlp_Query.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlp_Query.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlp_Query.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            tlp_Query.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_Query.Controls.Add(tlp_Query);

            // 第一行控件（原有代码完全保留，无修改）
            CreateLabelTextBox(out lbl_Name, out txt_Name, "患者姓名：");
            tlp_Query.Controls.Add(CreateControlPairPanel(lbl_Name, txt_Name), 0, 0);
            CreateLabelTextBox(out lbl_Phone, out txt_Phone, "联系电话：");
            tlp_Query.Controls.Add(CreateControlPairPanel(lbl_Phone, txt_Phone), 1, 0);
            CreateLabelCombo(out lbl_DiabetesType, out cbo_DiabetesType, "糖尿病类型：");
            tlp_Query.Controls.Add(CreateControlPairPanel(lbl_DiabetesType, cbo_DiabetesType), 2, 0);
            FlowLayoutPanel flp_QueryBtns = new FlowLayoutPanel() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Margin = Padding.Empty };
            btn_Reset = CreateBtn("重置", Color.Gray);
            btn_Query = CreateBtn("查询", Color.FromArgb(0, 122, 204));
            flp_QueryBtns.Controls.AddRange(new Control[] { btn_Reset, btn_Query });
            tlp_Query.Controls.Add(flp_QueryBtns, 3, 0);

            // 第二行控件（原有代码完全保留，无修改）
            CreateLabelCombo(out lbl_ControlStatus, out cbo_ControlStatus, "控制状态：");
            tlp_Query.Controls.Add(CreateControlPairPanel(lbl_ControlStatus, cbo_ControlStatus), 0, 1);
            Panel pnl_DateRange = new Panel() { Dock = DockStyle.Fill, Margin = Padding.Empty };
            lbl_DateRange = new Label { Text = "确诊日期：", Size = new Size(_globalLabelWidth, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(0, 0) };
            dtp_StartDate = new DateTimePicker { Location = new Point(_globalLabelWidth, 0), Size = new Size(120, _globalControlHeight), Format = DateTimePickerFormat.Short, Checked = false };
            Label lbl_DateSplit = new Label { Text = "至", Size = new Size(20, _globalControlHeight), TextAlign = ContentAlignment.MiddleCenter, Location = new Point(_globalLabelWidth + 125, 0) };
            dtp_EndDate = new DateTimePicker { Location = new Point(_globalLabelWidth + 150, 0), Size = new Size(120, _globalControlHeight), Format = DateTimePickerFormat.Short, Checked = false };
            cbo_ListDateQuick = new ComboBox { Location = new Point(_globalLabelWidth + 275, 0), Size = new Size(100, _globalControlHeight), DropDownStyle = ComboBoxStyle.DropDownList };
            cbo_ListDateQuick.Items.AddRange(new[] { "自定义", "今天", "近7天", "近1个月", "近3个月" });
            cbo_ListDateQuick.SelectedIndex = 0;
            pnl_DateRange.Controls.AddRange(new Control[] { lbl_DateRange, dtp_StartDate, lbl_DateSplit, dtp_EndDate, cbo_ListDateQuick });
            tlp_Query.Controls.Add(pnl_DateRange, 1, 1);
            Panel pnl_Valid = new Panel() { Dock = DockStyle.Fill, Margin = Padding.Empty };
            chk_OnlyValid = new CheckBox { Text = "仅有效患者", Location = new Point(0, 0), Size = new Size(120, _globalControlHeight), Checked = true };
            pnl_Valid.Controls.Add(chk_OnlyValid);
            tlp_Query.Controls.Add(pnl_Valid, 2, 1);
            FlowLayoutPanel flp_OperBtns = new FlowLayoutPanel() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Margin = Padding.Empty };
            btn_History = CreateBtn("历史追溯", Color.FromArgb(103, 58, 183));
            btn_Edit = CreateBtn("编辑档案", Color.FromArgb(255, 152, 0));
            btn_Detail = CreateBtn("查看详情", Color.FromArgb(0, 150, 136));
            flp_OperBtns.Controls.AddRange(new Control[] { btn_History, btn_Edit, btn_Detail });
            tlp_Query.Controls.Add(flp_OperBtns, 3, 1);

            // ========== 4. 初始化列表控件，放到【列表专属容器】里，修复表头显示 ==========
            dgv_PatientList = new DataGridView
            {
                Dock = DockStyle.Fill, // 保留Fill，稳定适配窗体缩放
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,

                // 【强制表头显示，彻底解决表头隐藏问题】
                ColumnHeadersVisible = true,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing,
                ColumnHeadersHeight = 35, // 固定表头高度，不会被挤压
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Padding = new Padding(8, 6, 8, 6),
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    BackColor = Color.FromArgb(240, 240, 240),
                    Font = new Font("微软雅黑", 9.5F, FontStyle.Bold),
                    ForeColor = Color.Black
                },
                // 单元格样式优化
                //CellPadding = new Padding(5, 3, 5, 3),
                //RowTemplate.Height = 30, // 固定行高，更美观

                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = true,
                MultiSelect = false
            };
            // 列定义（原有代码完全保留，补全DataPropertyName，避免数据绑定异常）
            dgv_PatientList.Columns.AddRange(new DataGridViewColumn[] {
        new DataGridViewTextBoxColumn {
            Name = "user_name",
            HeaderText = "姓名",
            DataPropertyName = "user_name",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 15
        },
        new DataGridViewTextBoxColumn {
            Name = "phone",
            HeaderText = "电话",
            DataPropertyName = "phone",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 15
        },
        new DataGridViewTextBoxColumn {
            Name = "diabetes_type",
            HeaderText = "糖尿病类型",
            DataPropertyName = "diabetes_type",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 20
        },
        new DataGridViewTextBoxColumn {
            Name = "glycemic_control_status",
            HeaderText = "控制状态",
            DataPropertyName = "glycemic_control_status",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 20
        },
        new DataGridViewButtonColumn
        {
            Name = "btn_Detail_Row",
            HeaderText = "详情",
            Text = "详情",
            UseColumnTextForButtonValue = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
        },
        new DataGridViewButtonColumn
        {
            Name = "btn_Edit_Row",
            HeaderText = "编辑",
            Text = "编辑",
            UseColumnTextForButtonValue = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
        },
        new DataGridViewButtonColumn
        {
            Name = "btn_History_Row",
            HeaderText = "追溯",
            Text = "追溯",
            UseColumnTextForButtonValue = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
        }
    });
            dgv_PatientList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // 【关键】把列表加到专属容器里，而不是直接加到TabPage
            pnl_ListContainer.Controls.Add(dgv_PatientList);
        }

        /// <summary>
        /// 档案维护页：升级-必填项标记、实时校验绑定
        /// </summary>
        private void InitArchiveEditPage()
        {
            // 原有代码全部保留，仅修改CreateEditItem的必填项参数
            split_Edit = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 650 };
            tabPage_Edit.Controls.Add(split_Edit);
            Panel pnl_Left = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            split_Edit.Panel1.Controls.Add(pnl_Left);
            Panel pnl_Right = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            split_Edit.Panel2.Controls.Add(pnl_Right);

            #region 基础信息分组-升级：必填项标记
            grp_BaseInfo = new GroupBox { Text = "患者基础信息", Dock = DockStyle.Top, Padding = _globalGroupBoxPadding };
            pnl_Left.Controls.Add(grp_BaseInfo);
            TableLayoutPanel tlp_BaseInfo = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty, AutoScroll = false };
            tlp_BaseInfo.ColumnCount = 1;
            tlp_BaseInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            grp_BaseInfo.Controls.Add(tlp_BaseInfo);

            // 新增必填项参数isRequired=true
            CreateEditItem<TextBox>(tlp_BaseInfo, out _, out txt_Base_Name, "姓名：", true, isRequired: true);
            CreateEditItem<TextBox>(tlp_BaseInfo, out _, out txt_Base_ID, "身份证号：", true, isRequired: true);
            CreateEditItem<TextBox>(tlp_BaseInfo, out _, out txt_Base_Phone, "联系电话：", true, isRequired: true);
            CreateEditItem<ComboBox>(tlp_BaseInfo, out _, out cbo_Gender, "性别：", true, isRequired: true);
            CreateEditItem<TextBox>(tlp_BaseInfo, out _, out txt_Base_Age, "年龄：", true);
            CreateEditItem<TextBox>(tlp_BaseInfo, out _, out txt_Base_Emergency, "紧急联系人：", false);
            CreateEditItem<TextBox>(tlp_BaseInfo, out _, out txt_Base_EmerPhone, "紧急电话：", false);
            grp_BaseInfo.Height = tlp_BaseInfo.RowCount * _globalRowHeight + 30;
            #endregion

            #region 患病信息分组-升级：必填项标记
            grp_Disease = new GroupBox { Text = "糖尿病患病信息", Dock = DockStyle.Top, Padding = _globalGroupBoxPadding };
            pnl_Left.Controls.Add(grp_Disease);
            TableLayoutPanel tlp_Disease = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty, AutoScroll = false };
            tlp_Disease.ColumnCount = 1;
            tlp_Disease.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            grp_Disease.Controls.Add(tlp_Disease);

            CreateEditItem<ComboBox>(tlp_Disease, out _, out cbo_Diabetes, "糖尿病类型：", false, isRequired: true);
            CreateEditItem<DateTimePicker>(tlp_Disease, out _, out dtp_Diagnose, "确诊日期：", false, isRequired: true);
            CreateEditItem<TextBox>(tlp_Disease, out _, out txt_Baseline, "空腹血糖基线：", false);
            CreateEditItem<TextBox>(tlp_Disease, out _, out txt_Duration, "病程（年）：", true);
            grp_Disease.Height = tlp_Disease.RowCount * _globalRowHeight + 30;
            #endregion

            #region 健康评估分组-升级：必填项标记
            grp_Assessment = new GroupBox { Text = "健康评估信息", Dock = DockStyle.Fill, Padding = _globalGroupBoxPadding };
            pnl_Right.Controls.Add(grp_Assessment);
            TableLayoutPanel tlp_Assessment = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty, AutoScroll = false };
            tlp_Assessment.ColumnCount = 1;
            tlp_Assessment.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            grp_Assessment.Controls.Add(tlp_Assessment);

            CreateEditItem<DateTimePicker>(tlp_Assessment, out _, out dtp_AssessDate, "评估日期：", false, isRequired: true);
            CreateEditItem<TextBox>(tlp_Assessment, out _, out txt_Height, "身高(cm)：", false);
            CreateEditItem<TextBox>(tlp_Assessment, out _, out txt_Weight, "体重(kg)：", false);
            CreateEditItem<TextBox>(tlp_Assessment, out _, out txt_BMI, "BMI：", true);
            CreateEditItem<TextBox>(tlp_Assessment, out _, out txt_Systolic, "收缩压：", false);
            CreateEditItem<TextBox>(tlp_Assessment, out _, out txt_Diastolic, "舒张压：", false);
            CreateEditItem<TextBox>(tlp_Assessment, out _, out txt_Heart, "心率：", false);
            CreateEditItem<TextBox>(tlp_Assessment, out _, out txt_HbA1c, "糖化血红蛋白：", false);
            CreateEditItem<TextBox>(tlp_Assessment, out _, out txt_AvgFast, "平均空腹血糖：", true);
            CreateEditItem<TextBox>(tlp_Assessment, out _, out txt_AvgPost, "平均餐后血糖：", true);
            CreateEditItem<ComboBox>(tlp_Assessment, out _, out cbo_ControlStatusEdit, "控制状态：", false);

            int rowIndex = tlp_Assessment.RowCount;
            tlp_Assessment.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            Label lbl_C = new Label { Text = "并发症：", Size = new Size(_globalLabelWidth, 70), TextAlign = ContentAlignment.MiddleLeft, Margin = _globalControlMargin };
            txt_Complication = new TextBox { Size = new Size(320, 70), Multiline = true, ScrollBars = ScrollBars.Vertical, Margin = _globalControlMargin };
            tlp_Assessment.Controls.Add(CreateControlPairPanel(lbl_C, txt_Complication), 0, rowIndex);

            rowIndex++;
            tlp_Assessment.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            Label lbl_Co = new Label { Text = "合并症：", Size = new Size(_globalLabelWidth, 70), TextAlign = ContentAlignment.MiddleLeft, Margin = _globalControlMargin };
            txt_Comorbidity = new TextBox { Size = new Size(320, 70), Multiline = true, ScrollBars = ScrollBars.Vertical, Margin = _globalControlMargin };
            tlp_Assessment.Controls.Add(CreateControlPairPanel(lbl_Co, txt_Comorbidity), 0, rowIndex);
            #endregion

            #region 底部按钮（原有代码保留，无修改）
            Panel pnl_Btn = new Panel { Dock = DockStyle.Bottom, Height = 90, BackColor = Color.FromArgb(245, 245, 245) };
            tabPage_Edit.Controls.Add(pnl_Btn);
            FlowLayoutPanel flp_EditBtns = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 20, 20, 0) };
            btn_Cancel = CreateBtn("取消编辑", Color.Gray);
            btn_Save = CreateBtn("保存档案", Color.FromArgb(0, 122, 204));
            flp_EditBtns.Controls.AddRange(new Control[] { btn_Cancel, btn_Save });
            pnl_Btn.Controls.Add(flp_EditBtns);
            #endregion

            txt_Height.TextChanged += (s, e) => CalculateBMI();
            txt_Weight.TextChanged += (s, e) => CalculateBMI();
            dtp_Diagnose.ValueChanged += (s, e) => CalculateDiseaseDuration();
        }


        /// <summary>
        /// 历史追溯页：完整升级-患者选择、日期快捷选项、记录类型优化、查询功能完善
        /// </summary>

        /// <summary>
        /// 历史追溯页：完整升级-患者选择、日期快捷选项、记录类型优化、查询功能完善
        /// </summary>
        private void InitHistoryPage()
        {
            // 调大分组框高度，适配3行布局
            grp_HistoryQuery = new GroupBox { Text = "追溯条件", Dock = DockStyle.Top, Height = 240, Padding = _globalGroupBoxPadding };
            tabPage_History.Controls.Add(grp_HistoryQuery);

            TableLayoutPanel tlp_HistoryQuery = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_HistoryQuery.RowCount = 3;
            tlp_HistoryQuery.ColumnCount = 4;
            tlp_HistoryQuery.RowStyles.Clear();
            tlp_HistoryQuery.ColumnStyles.Clear();
            // 行高设置
            tlp_HistoryQuery.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            tlp_HistoryQuery.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            tlp_HistoryQuery.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            // 列宽设置
            tlp_HistoryQuery.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlp_HistoryQuery.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlp_HistoryQuery.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlp_HistoryQuery.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            grp_HistoryQuery.Controls.Add(tlp_HistoryQuery);

            // 第一行：患者选择（支持手动更换，默认带入选中患者）
            Panel pnl_HistoryPatientSelect = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            lbl_HistorySelectPatient = new Label { Text = "选择患者：", Size = new Size(80, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(0, 0) };
            txt_HistoryPatientSearch = new TextBox { Location = new Point(80, 0), Size = new Size(150, _globalControlHeight), Text = "搜索姓名/电话" };
            cbo_HistoryPatientList = new ComboBox { Location = new Point(235, 0), Size = new Size(200, _globalControlHeight), DropDownStyle = ComboBoxStyle.DropDownList };
            pnl_HistoryPatientSelect.Controls.AddRange(new Control[] { lbl_HistorySelectPatient, txt_HistoryPatientSearch, cbo_HistoryPatientList });
            tlp_HistoryQuery.Controls.Add(pnl_HistoryPatientSelect, 0, 0);
            tlp_HistoryQuery.SetColumnSpan(pnl_HistoryPatientSelect, 4);

            // 第二行：时间范围+日期快捷选择
            Panel pnl_HistoryDate = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_Time = new Label { Text = "时间范围：", Size = new Size(80, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(0, 0) };
            dtp_HistoryStart = new DateTimePicker { Location = new Point(80, 0), Size = new Size(120, _globalControlHeight), Format = DateTimePickerFormat.Short };
            Label lbl_HistoryDateSplit = new Label { Text = "至", Size = new Size(20, _globalControlHeight), TextAlign = ContentAlignment.MiddleCenter, Location = new Point(205, 0) };
            dtp_HistoryEnd = new DateTimePicker { Location = new Point(230, 0), Size = new Size(120, _globalControlHeight), Format = DateTimePickerFormat.Short };
            cbo_HistoryDateQuick = new ComboBox { Location = new Point(355, 0), Size = new Size(100, _globalControlHeight), DropDownStyle = ComboBoxStyle.DropDownList };
            cbo_HistoryDateQuick.Items.AddRange(new[] { "自定义", "今天", "近7天", "近1个月", "近3个月" });
            cbo_HistoryDateQuick.SelectedIndex = 0;
            pnl_HistoryDate.Controls.AddRange(new Control[] { lbl_Time, dtp_HistoryStart, lbl_HistoryDateSplit, dtp_HistoryEnd, cbo_HistoryDateQuick });
            tlp_HistoryQuery.Controls.Add(pnl_HistoryDate, 0, 1);
            tlp_HistoryQuery.SetColumnSpan(pnl_HistoryDate, 2);

            // 第二行：记录类型（按需求预设选项）
            Panel pnl_RecordType = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_Type = new Label { Text = "记录类型：", Size = new Size(80, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(0, 0) };
            cbo_RecordType = new ComboBox { Location = new Point(80, 0), Size = new Size(200, _globalControlHeight), DropDownStyle = ComboBoxStyle.DropDownList };
            cbo_RecordType.Items.AddRange(new[] { "全部", "血糖监测", "随访记录", "干预方案", "用药调整", "体检记录", "评估记录" });
            cbo_RecordType.SelectedIndex = 0;
            pnl_RecordType.Controls.AddRange(new Control[] { lbl_Type, cbo_RecordType });
            tlp_HistoryQuery.Controls.Add(pnl_RecordType, 2, 1);
            tlp_HistoryQuery.SetColumnSpan(pnl_RecordType, 2);

            // 第三行：操作按钮
            FlowLayoutPanel flp_HistoryBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Margin = Padding.Empty,
                Padding = new Padding(5),
            };
            btn_Export = CreateBtn("导出", Color.FromArgb(0, 150, 136));
            btn_HistoryReset = CreateBtn("重置", Color.Gray);
            btn_HistoryQuery = CreateBtn("查询", Color.FromArgb(0, 122, 204));
            btn_Export.Size = new Size(90, 30);
            btn_HistoryReset.Size = new Size(90, 30);
            btn_HistoryQuery.Size = new Size(90, 30);
            btn_Export.Margin = new Padding(3, 3, 3, 3);
            btn_HistoryReset.Margin = new Padding(3, 3, 3, 3);
            btn_HistoryQuery.Margin = new Padding(3, 3, 3, 3);
            flp_HistoryBtns.Controls.AddRange(new Control[] { btn_Export, btn_HistoryReset, btn_HistoryQuery });
            tlp_HistoryQuery.Controls.Add(flp_HistoryBtns, 0, 2);
            tlp_HistoryQuery.SetColumnSpan(flp_HistoryBtns, 4);

            // ========== 【核心修复】补全 lbl_HistoryPatient 控件的实例化与挂载 ==========
            Panel pnl_HistoryPatientInfo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = Color.FromArgb(245, 245, 245),
                Margin = Padding.Empty
            };
            lbl_HistoryPatient = new Label
            {
                Name = "lbl_HistoryPatient",
                Text = "患者：未选择",
                Location = new Point(20, 8),
                Size = new Size(400, 20),
                Font = new Font("微软雅黑", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnl_HistoryPatientInfo.Controls.Add(lbl_HistoryPatient);
            tabPage_History.Controls.Add(pnl_HistoryPatientInfo);
            // ==========================================================================

            // 历史列表表格
            dgv_HistoryList = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, ReadOnly = true };
            dgv_HistoryList.AutoGenerateColumns = false;
            dgv_HistoryList.Columns.AddRange(new[] {
        new DataGridViewTextBoxColumn { HeaderText = "记录时间", DataPropertyName = "record_time", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
        new DataGridViewTextBoxColumn { HeaderText = "记录类型", DataPropertyName = "record_type", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
        new DataGridViewTextBoxColumn { HeaderText = "记录摘要", DataPropertyName = "record_summary", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
        new DataGridViewTextBoxColumn { HeaderText = "操作人", DataPropertyName = "operator_name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
    });
            dgv_HistoryList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv_HistoryList.AllowUserToAddRows = false;
            tabPage_History.Controls.Add(dgv_HistoryList);
        }
        #endregion
        #region 新增：业务辅助方法
        /// <summary>
        /// 加载追溯页患者下拉列表，支持搜索
        /// </summary>
        /// <param name="searchText">姓名/电话搜索关键词</param>
        /// <summary>
        /// 加载追溯页患者下拉列表，支持搜索
        /// </summary>
        /// <param name="searchText">姓名/电话搜索关键词</param>
        private void LoadHistoryPatientList(string searchText = "")
        {
            try
            {
                // 【修复】先解绑事件，避免绑定数据源时提前触发，导致空引用
                cbo_HistoryPatientList.SelectedIndexChanged -= cbo_HistoryPatientList_SelectedIndexChanged;

                var patientList = _bllPatient.GetPatientSimpleList(searchText);
                cbo_HistoryPatientList.DataSource = patientList;
                cbo_HistoryPatientList.DisplayMember = "user_name";
                cbo_HistoryPatientList.ValueMember = "user_id";

                // 绑定完成后清空选中，再重新绑定事件，保证安全
                cbo_HistoryPatientList.SelectedIndex = -1;
                cbo_HistoryPatientList.SelectedIndexChanged += cbo_HistoryPatientList_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载患者列表失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 同步选中患者到追溯页下拉框
        /// </summary>
        /// <param name="userId">患者ID</param>
        private void SyncHistoryPatientSelect(int userId)
        {
            try
            {
                if (cbo_HistoryPatientList.Items.Count == 0) LoadHistoryPatientList();
                cbo_HistoryPatientList.SelectedValue = userId;
            }
            catch
            {
                // 忽略选中异常，不影响主流程
            }
        }
        #endregion
        #region 通用布局方法
        /// <summary>
        /// 自动计算BMI
        /// </summary>
        private void CalculateBMI()
        {
            if (decimal.TryParse(txt_Height.Text, out decimal height) && decimal.TryParse(txt_Weight.Text, out decimal weight) && height > 0)
            {
                decimal heightM = height / 100; // 厘米转米
                decimal bmi = Math.Round(weight / (heightM * heightM), 2);
                txt_BMI.Text = bmi.ToString();
            }
            else
            {
                txt_BMI.Clear();
            }
        }

        /// <summary>
        /// 自动计算病程（年）
        /// </summary>
        private void CalculateDiseaseDuration()
        {
            if (dtp_Diagnose.Checked && dtp_Diagnose.Value <= DateTime.Now)
            {
                int years = DateTime.Now.Year - dtp_Diagnose.Value.Year;
                if (DateTime.Now.Month < dtp_Diagnose.Value.Month || (DateTime.Now.Month == dtp_Diagnose.Value.Month && DateTime.Now.Day < dtp_Diagnose.Value.Day))
                {
                    years--;
                }
                txt_Duration.Text = years >= 0 ? years.ToString() : "0";
            }
            else
            {
                txt_Duration.Clear();
            }
        }
        private Button CreateBtn(string text, Color backColor)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.BackColor = backColor;
            btn.ForeColor = backColor == Color.White ? Color.Black : Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.Size = new Size(_globalButtonWidth, _globalButtonHeight);
            btn.Font = this.Font;
            btn.Margin = _globalControlMargin;
            return btn;
        }

        /// <summary>
        /// 扩展：支持必填项标记，兼容原有调用
        /// </summary>
        private void CreateLabelTextBox(out Label lbl, out TextBox txt, string labelText, bool isRequired = false)
        {
            lbl = new Label
            {
                Text = isRequired ? labelText.TrimEnd('：') + "*：" : labelText,
                Size = new Size(_globalLabelWidth, _globalControlHeight),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = _globalControlMargin,
                ForeColor = isRequired ? Color.Red : Color.Black
            };
            txt = new TextBox
            {
                Size = new Size(240, _globalControlHeight),
                Margin = _globalControlMargin
            };
        }

        /// <summary>
        /// 扩展：支持必填项标记，兼容原有调用
        /// </summary>
        private void CreateLabelCombo(out Label lbl, out ComboBox cbo, string labelText, bool isRequired = false)
        {
            lbl = new Label
            {
                Text = isRequired ? labelText.TrimEnd('：') + "*：" : labelText,
                Size = new Size(_globalLabelWidth, _globalControlHeight),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = _globalControlMargin,
                ForeColor = isRequired ? Color.Red : Color.Black
            };
            cbo = new ComboBox
            {
                Size = new Size(240, _globalControlHeight),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = _globalControlMargin
            };
        }

        private Panel CreateControlPairPanel(Label lbl, Control ctrl)
        {
            Panel panel = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            lbl.Location = new Point(0, 0);
            ctrl.Location = new Point(lbl.Width, 0);
            panel.Controls.AddRange(new Control[] { lbl, ctrl });
            return panel;
        }

        /// <summary>
        /// 扩展：支持必填项标记，兼容原有调用
        /// </summary>
        private void CreateEditItem<T>(TableLayoutPanel tlp, out Label lbl, out T ctrl, string labelText, bool readOnly, bool isRequired = false) where T : Control, new()
        {
            int rowIndex = tlp.RowCount;
            tlp.RowCount++;
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            lbl = new Label
            {
                Text = isRequired ? labelText.TrimEnd('：') + "*：" : labelText,
                Size = new Size(_globalLabelWidth, _globalControlHeight),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = _globalControlMargin,
                ForeColor = isRequired ? Color.Red : Color.Black
            };
            ctrl = new T();
            ctrl.Size = new Size(240, _globalControlHeight);
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
            tlp.Controls.Add(CreateControlPairPanel(lbl, ctrl), 0, rowIndex);
        }
        #endregion

        #region 业务逻辑代码（完全保留，无任何修改）
        private void InitControlData()
        {
            // ========== 第一步：先给所有下拉框添加选项（补全缺失的cbo_ControlStatusEdit） ==========
            cbo_DiabetesType.Items.AddRange(new[] { "全部", "1型", "2型", "妊娠", "其他" });
            cbo_ControlStatus.Items.AddRange(new[] { "全部", "良好", "需调整饮食", "需调整用药", "需转诊" });
            cbo_Gender.Items.AddRange(new[] { "男", "女" });
            cbo_Diabetes.Items.AddRange(new[] { "1型", "2型", "妊娠", "其他" });
            cbo_RecordType.Items.AddRange(new[] { "全部", "健康评估", "血糖检测", "干预方案", "用药记录" });
            // 【核心修复】给编辑页控制状态下拉框添加业务选项（编辑页不含"全部"，仅保留具体状态）
            cbo_ControlStatusEdit.Items.AddRange(new[] { "良好", "需调整饮食", "需调整用药", "需转诊" });

            // ========== 第二步：所有选项添加完成后，再统一设置默认选中索引（顺序不能颠倒） ==========
            cbo_DiabetesType.SelectedIndex = 0;
            cbo_ControlStatus.SelectedIndex = 0;
            // 【容错修复】先判断下拉框有选项，再设置索引，彻底避免同类异常
            if (cbo_ControlStatusEdit.Items.Count > 0)
                cbo_ControlStatusEdit.SelectedIndex = 0;

            dgv_HistoryList.Columns.AddRange(new[] {
        new DataGridViewTextBoxColumn { HeaderText = "记录时间", DataPropertyName = "record_time", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
        new DataGridViewTextBoxColumn { HeaderText = "记录类型", DataPropertyName = "record_type", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
        new DataGridViewTextBoxColumn { HeaderText = "记录摘要", DataPropertyName = "record_summary", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
        new DataGridViewTextBoxColumn { HeaderText = "操作人", DataPropertyName = "operator_name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
    });
            dgv_HistoryList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv_HistoryList.AllowUserToAddRows = false;
        }

        private void BindAllEvents()
        {
            #region 原有事件全部保留，无任何修改
            dgv_PatientList.CellFormatting += (s, e) =>
            {
                if (dgv_PatientList.Rows[e.RowIndex].DataBoundItem is PatientArchive archive)
                {
                    switch (dgv_PatientList.Columns[e.ColumnIndex].Name)
                    {
                        case "user_name":
                            e.Value = archive.BaseInfo?.user_name ?? "";
                            e.FormattingApplied = true;
                            break;
                        case "phone":
                            e.Value = archive.BaseInfo?.phone ?? "";
                            e.FormattingApplied = true;
                            break;
                        case "diabetes_type":
                            e.Value = archive.BaseInfo?.diabetes_type ?? "";
                            e.FormattingApplied = true;
                            break;
                        case "glycemic_control_status":
                            e.Value = archive.GlycemicControlStatus ?? "未评估";
                            e.FormattingApplied = true;
                            break;
                    }
                }
            };

            dgv_PatientList.DataBindingComplete += (s, e) =>
            {
                if (dgv_PatientList.Rows.Count > 0)
                {
                    dgv_PatientList.Rows[0].Selected = true;
                    _selectUserId = (dgv_PatientList.Rows[0].DataBoundItem as PatientArchive)?.BaseInfo.user_id ?? 0;
                }
                else
                {
                    _selectUserId = 0;
                }
            };

            btn_Query.Click += (s, e) => { _pageIndex = 1; LoadList(); };

            btn_Reset.Click += (s, e) => {
                txt_Name.Clear();
                txt_Phone.Clear();
                if (cbo_DiabetesType.Items.Count > 0) cbo_DiabetesType.SelectedIndex = 0;
                if (cbo_ControlStatus.Items.Count > 0) cbo_ControlStatus.SelectedIndex = 0;
                dtp_StartDate.Value = DateTime.Now.AddMonths(-1);
                dtp_StartDate.Checked = false;
                dtp_EndDate.Value = DateTime.Now;
                dtp_EndDate.Checked = false;
                chk_OnlyValid.Checked = true;
                cbo_ListDateQuick.SelectedIndex = 0;
                _pageIndex = 1;
                LoadList();
            };

            btn_Prev.Click += (s, e) => { if (_pageIndex > 1) { _pageIndex--; LoadList(); } };
            btn_Next.Click += (s, e) => {
                int totalPage = (int)Math.Ceiling(_totalCount * 1.0 / _pageSize);
                if (_pageIndex < totalPage) { _pageIndex++; LoadList(); }
            };

            dgv_PatientList.SelectionChanged += (s, e) => {
                if (dgv_PatientList.SelectedRows.Count > 0 && dgv_PatientList.SelectedRows[0].DataBoundItem is PatientArchive a)
                    _selectUserId = a.BaseInfo.user_id;
                else _selectUserId = 0;
            };

            btn_Cancel.Click += (s, e) => { ClearEditControls(); tabMain.SelectedTab = tabPage_List; errorProviderMain.Clear(); };
            #endregion

            #region 新增：日期快捷选择事件
            cbo_ListDateQuick.SelectedIndexChanged += (s, e) =>
            {
                if (cbo_ListDateQuick.SelectedIndex == 0) return;
                DateTime now = DateTime.Now.Date;
                DateTime startDate = now;
                DateTime endDate = now;
                switch (cbo_ListDateQuick.SelectedItem.ToString())
                {
                    case "今天":
                        startDate = now;
                        endDate = now;
                        break;
                    case "近7天":
                        startDate = now.AddDays(-6);
                        endDate = now;
                        break;
                    case "近1个月":
                        startDate = now.AddMonths(-1);
                        endDate = now;
                        break;
                    case "近3个月":
                        startDate = now.AddMonths(-3);
                        endDate = now;
                        break;
                }
                dtp_StartDate.Value = startDate;
                dtp_StartDate.Checked = true;
                dtp_EndDate.Value = endDate;
                dtp_EndDate.Checked = true;
            };

            cbo_HistoryDateQuick.SelectedIndexChanged += (s, e) =>
            {
                if (cbo_HistoryDateQuick.SelectedIndex == 0) return;
                DateTime now = DateTime.Now.Date;
                DateTime startDate = now;
                DateTime endDate = now;
                switch (cbo_HistoryDateQuick.SelectedItem.ToString())
                {
                    case "今天":
                        startDate = now;
                        endDate = now;
                        break;
                    case "近7天":
                        startDate = now.AddDays(-6);
                        endDate = now;
                        break;
                    case "近1个月":
                        startDate = now.AddMonths(-1);
                        endDate = now;
                        break;
                    case "近3个月":
                        startDate = now.AddMonths(-3);
                        endDate = now;
                        break;
                }
                dtp_HistoryStart.Value = startDate;
                dtp_HistoryEnd.Value = endDate;
            };
            #endregion

            #region 新增：行内操作按钮点击事件
            dgv_PatientList.CellContentClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                if (!( dgv_PatientList.Rows[e.RowIndex].DataBoundItem is PatientArchive archive)) return;

                _selectUserId = archive.BaseInfo.user_id;
                switch (dgv_PatientList.Columns[e.ColumnIndex].Name)
                {
                    case "btn_Detail_Row":
                        _isViewDetailMode = true;
                        tabMain.SelectedTab = tabPage_Edit;
                        LoadPatientDetail(_selectUserId);
                        errorProviderMain.Clear();
                        break;
                    case "btn_Edit_Row":
                        _isViewDetailMode = false;
                        tabMain.SelectedTab = tabPage_Edit;
                        LoadPatientDetail(_selectUserId);
                        errorProviderMain.Clear();
                        break;
                    case "btn_History_Row":
                        tabMain.SelectedTab = tabPage_History;
                        SyncHistoryPatientSelect(_selectUserId);
                        LoadPatientHistory(_selectUserId);
                        break;
                }
            };
            #endregion

            #region 新增：编辑/详情/追溯按钮优化（自动切换标签页+全量回填）
            btn_Detail.Click += (s, e) => {
                if (_selectUserId > 0)
                {
                    _isViewDetailMode = true;
                    tabMain.SelectedTab = tabPage_Edit;
                    LoadPatientDetail(_selectUserId);
                    errorProviderMain.Clear();
                }
                else MessageBox.Show("请先选择患者！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };

            btn_Edit.Click += (s, e) => {
                if (_selectUserId > 0)
                {
                    _isViewDetailMode = false;
                    tabMain.SelectedTab = tabPage_Edit;
                    LoadPatientDetail(_selectUserId);
                    errorProviderMain.Clear();
                }
                else MessageBox.Show("请先选择患者！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };

            btn_History.Click += (s, e) => {
                if (_selectUserId > 0)
                {
                    tabMain.SelectedTab = tabPage_History;
                    SyncHistoryPatientSelect(_selectUserId);
                    LoadPatientHistory(_selectUserId);
                }
                else MessageBox.Show("请先选择患者！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };
            #endregion

            #region 新增：实时表单校验事件
            // 身份证号格式校验
            txt_Base_ID.Validating += (s, e) =>
            {
                string idCard = txt_Base_ID.Text.Trim();
                if (string.IsNullOrEmpty(idCard))
                {
                    errorProviderMain.SetError(txt_Base_ID, "身份证号不能为空");
                    e.Cancel = true;
                    return;
                }
                System.Text.RegularExpressions.Regex idCardRegex = new System.Text.RegularExpressions.Regex(@"^[1-9]\d{5}(18|19|20)\d{2}((0[1-9])|(1[0-2]))(([0-2][1-9])|10|20|30|31)\d{3}[0-9Xx]$");
                if (!idCardRegex.IsMatch(idCard))
                {
                    errorProviderMain.SetError(txt_Base_ID, "请输入有效的18位身份证号");
                    e.Cancel = true;
                    return;
                }
                errorProviderMain.SetError(txt_Base_ID, "");
            };

            // 手机号格式校验
            txt_Base_Phone.Validating += (s, e) =>
            {
                string phone = txt_Base_Phone.Text.Trim();
                if (string.IsNullOrEmpty(phone))
                {
                    errorProviderMain.SetError(txt_Base_Phone, "手机号不能为空");
                    e.Cancel = true;
                    return;
                }
                System.Text.RegularExpressions.Regex phoneRegex = new System.Text.RegularExpressions.Regex(@"^1[3-9]\d{9}$");
                if (!phoneRegex.IsMatch(phone))
                {
                    errorProviderMain.SetError(txt_Base_Phone, "请输入有效的11位手机号");
                    e.Cancel = true;
                    return;
                }
                errorProviderMain.SetError(txt_Base_Phone, "");
            };

            // 身高数值范围校验
            txt_Height.Validating += (s, e) =>
            {
                if (string.IsNullOrEmpty(txt_Height.Text)) return;
                if (!decimal.TryParse(txt_Height.Text, out decimal height) || height < 100 || height > 250)
                {
                    errorProviderMain.SetError(txt_Height, "请输入100-250之间的有效身高(cm)");
                    e.Cancel = true;
                    return;
                }
                errorProviderMain.SetError(txt_Height, "");
            };

            // 体重数值范围校验
            txt_Weight.Validating += (s, e) =>
            {
                if (string.IsNullOrEmpty(txt_Weight.Text)) return;
                if (!decimal.TryParse(txt_Weight.Text, out decimal weight) || weight < 30 || weight > 200)
                {
                    errorProviderMain.SetError(txt_Weight, "请输入30-200之间的有效体重(kg)");
                    e.Cancel = true;
                    return;
                }
                errorProviderMain.SetError(txt_Weight, "");
            };

            // 血压数值范围&逻辑校验
            txt_Systolic.Validating += (s, e) =>
            {
                if (string.IsNullOrEmpty(txt_Systolic.Text)) return;
                if (!short.TryParse(txt_Systolic.Text, out short sbp) || sbp < 50 || sbp > 250)
                {
                    errorProviderMain.SetError(txt_Systolic, "请输入50-250之间的有效收缩压");
                    e.Cancel = true;
                    return;
                }
                errorProviderMain.SetError(txt_Systolic, "");
            };

            txt_Diastolic.Validating += (s, e) =>
            {
                if (string.IsNullOrEmpty(txt_Diastolic.Text)) return;
                if (!short.TryParse(txt_Diastolic.Text, out short dbp) || dbp < 30 || dbp > 150)
                {
                    errorProviderMain.SetError(txt_Diastolic, "请输入30-150之间的有效舒张压");
                    e.Cancel = true;
                    return;
                }
                if (short.TryParse(txt_Systolic.Text, out short sbp) && sbp <= dbp)
                {
                    errorProviderMain.SetError(txt_Diastolic, "舒张压必须小于收缩压");
                    e.Cancel = true;
                    return;
                }
                errorProviderMain.SetError(txt_Diastolic, "");
            };

            // 心率数值范围校验
            txt_Heart.Validating += (s, e) =>
            {
                if (string.IsNullOrEmpty(txt_Heart.Text)) return;
                if (!short.TryParse(txt_Heart.Text, out short heart) || heart < 30 || heart > 200)
                {
                    errorProviderMain.SetError(txt_Heart, "请输入30-200之间的有效心率");
                    e.Cancel = true;
                    return;
                }
                errorProviderMain.SetError(txt_Heart, "");
            };

            // 糖化血红蛋白数值范围校验
            txt_HbA1c.Validating += (s, e) =>
            {
                if (string.IsNullOrEmpty(txt_HbA1c.Text)) return;
                if (!decimal.TryParse(txt_HbA1c.Text, out decimal hba1c) || hba1c < 2 || hba1c > 20)
                {
                    errorProviderMain.SetError(txt_HbA1c, "请输入2-20之间的有效糖化血红蛋白值");
                    e.Cancel = true;
                    return;
                }
                errorProviderMain.SetError(txt_HbA1c, "");
            };

            // 空腹血糖基线数值范围校验
            txt_Baseline.Validating += (s, e) =>
            {
                if (string.IsNullOrEmpty(txt_Baseline.Text)) return;
                if (!decimal.TryParse(txt_Baseline.Text, out decimal baseline) || baseline < 3.9m || baseline > 15m)
                {
                    errorProviderMain.SetError(txt_Baseline, "请输入3.9-15之间的有效空腹血糖基线值");
                    e.Cancel = true;
                    return;
                }
                errorProviderMain.SetError(txt_Baseline, "");
            };

            // 日期逻辑校验
            dtp_Diagnose.Validating += (s, e) =>
            {
                if (!dtp_Diagnose.Checked) return;
                if (dtp_Diagnose.Value > DateTime.Now)
                {
                    errorProviderMain.SetError(dtp_Diagnose, "确诊日期不能晚于当前时间");
                    e.Cancel = true;
                    return;
                }
                errorProviderMain.SetError(dtp_Diagnose, "");
            };

            dtp_AssessDate.Validating += (s, e) =>
            {
                if (!dtp_AssessDate.Checked) return;
                if (dtp_AssessDate.Value > DateTime.Now)
                {
                    errorProviderMain.SetError(dtp_AssessDate, "评估日期不能晚于当前时间");
                    e.Cancel = true;
                    return;
                }
                errorProviderMain.SetError(dtp_AssessDate, "");
            };
            #endregion
            this.Load += FrmPatientRecordManage_Load;

            #region 新增：追溯页功能事件
            // 窗体加载时初始化患者列表
            this.Load += (s, e) =>
            {
                LoadHistoryPatientList();
                LoadList();
            };

            // 患者搜索事件
            txt_HistoryPatientSearch.TextChanged += (s, e) =>
            {
                LoadHistoryPatientList(txt_HistoryPatientSearch.Text.Trim());
            };

            // 患者选择切换事件
            cbo_HistoryPatientList.SelectedIndexChanged += (s, e) =>
            {
                if (cbo_HistoryPatientList.SelectedValue is int userId && userId > 0)
                {
                    _selectUserId = userId;
                    var patient = _bllPatient.GetPatientDetail(userId);
                    lbl_HistoryPatient.Text = patient?.BaseInfo != null ? $"患者：{patient.BaseInfo.user_name}" : "患者：未找到";
                }
            };

            // 追溯页查询按钮事件
            btn_HistoryQuery.Click += (s, e) =>
            {
                if (cbo_HistoryPatientList.SelectedValue is int userId && userId > 0)
                {
                    LoadPatientHistory(userId);
                }
                else
                {
                    MessageBox.Show("请先选择患者！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            // 追溯页重置按钮事件
            btn_HistoryReset.Click += (s, e) => {
                dtp_HistoryStart.Value = DateTime.Now.AddMonths(-3);
                dtp_HistoryEnd.Value = DateTime.Now;
                cbo_HistoryDateQuick.SelectedIndex = 0;
                if (cbo_RecordType.Items.Count > 0) cbo_RecordType.SelectedIndex = 0;
                txt_HistoryPatientSearch.Clear();
                LoadHistoryPatientList();
                if (_selectUserId > 0) SyncHistoryPatientSelect(_selectUserId);
                LoadPatientHistory(_selectUserId);
            };
            #endregion

            #region 原有保存事件优化（增加前置校验）
            btn_Save.Click += (s, e) => {
                try
                {
                    // 前置校验：清空之前的错误
                    errorProviderMain.Clear();
                    // 基础数据校验
                    if (_selectUserId <= 0)
                    {
                        MessageBox.Show("患者信息无效，无法保存！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (string.IsNullOrEmpty(cbo_Diabetes.Text))
                    {
                        errorProviderMain.SetError(cbo_Diabetes, "请选择糖尿病类型");
                        MessageBox.Show("请完善必填项信息！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!dtp_Diagnose.Checked)
                    {
                        errorProviderMain.SetError(dtp_Diagnose, "请选择确诊日期");
                        MessageBox.Show("请完善必填项信息！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!dtp_AssessDate.Checked)
                    {
                        errorProviderMain.SetError(dtp_AssessDate, "请选择评估日期");
                        MessageBox.Show("请完善必填项信息！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 原有实体组装代码全部保留
                    Users user = new Users()
                    {
                        user_id = _selectUserId,
                        diabetes_type = cbo_Diabetes.Text,
                        diagnose_date = dtp_Diagnose.Checked ? dtp_Diagnose.Value : (DateTime?)null,
                        fasting_glucose_baseline = decimal.TryParse(txt_Baseline.Text, out decimal baseline) ? baseline : (decimal?)null,
                        emergency_contact = txt_Base_Emergency.Text.Trim(),
                        emergency_phone = txt_Base_EmerPhone.Text.Trim()
                    };

                    HealthAssessment assessment = new HealthAssessment()
                    {
                        user_id = _selectUserId,
                        assessment_date = dtp_AssessDate.Value,
                        assessment_type = "常规评估",
                        assessment_by = 1,
                        height = decimal.TryParse(txt_Height.Text, out decimal height) ? height : (decimal?)null,
                        weight = decimal.TryParse(txt_Weight.Text, out decimal weight) ? weight : (decimal?)null,
                        bmi = decimal.TryParse(txt_BMI.Text, out decimal bmi) ? bmi : (decimal?)null,
                        systolic_bp = short.TryParse(txt_Systolic.Text, out short sbp) ? sbp : (short?)null,
                        diastolic_bp = short.TryParse(txt_Diastolic.Text, out short dbp) ? dbp : (short?)null,
                        heart_rate = short.TryParse(txt_Heart.Text, out short heart) ? heart : (short?)null,
                        hba1c = decimal.TryParse(txt_HbA1c.Text, out decimal hba1c) ? hba1c : (decimal?)null,
                        avg_fasting_glucose = decimal.TryParse(txt_AvgFast.Text, out decimal avgFast) ? avgFast : (decimal?)null,
                        avg_postprandial_glucose = decimal.TryParse(txt_AvgPost.Text, out decimal avgPost) ? avgPost : (decimal?)null,
                        disease_duration_years = decimal.TryParse(txt_Duration.Text, out decimal duration) ? duration : (decimal?)null,
                        glycemic_control_status = cbo_ControlStatusEdit.Text,
                        diabetes_complications = txt_Complication.Text.Trim(),
                        comorbidities = txt_Comorbidity.Text.Trim(),
                        data_completeness = 100
                    };

                    var result = _bllPatient.SavePatientDiseaseInfo(user, assessment);
                    if (result.Success)
                    {
                        MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearEditControls();
                        tabMain.SelectedTab = tabPage_List;
                        LoadList();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("保存档案异常：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            #endregion
        }

        #region 新增：患者下拉框选中事件（支持安全解绑，避免提前触发异常）
        #region 【修复新增】历史页控件事件处理方法（支持安全解绑，避免提前触发异常）
        /// <summary>
        /// 患者下拉框选中切换事件（安全处理，无空引用风险）
        /// </summary>
        private void cbo_HistoryPatientList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 前置空校验：控件未初始化/无选中值，直接返回，彻底规避空引用
            if (lbl_HistoryPatient == null || cbo_HistoryPatientList.SelectedValue == null)
                return;

            if (cbo_HistoryPatientList.SelectedValue is int userId && userId > 0)
            {
                _selectUserId = userId;
                var patient = _bllPatient.GetPatientDetail(userId);
                // 多层空值穿透校验，兜底所有null场景
                string displayName = patient?.BaseInfo?.user_name?.Trim();
                lbl_HistoryPatient.Text = !string.IsNullOrEmpty(displayName)
                    ? $"患者：{displayName}"
                    : "患者：未找到";
            }
            else
            {
                lbl_HistoryPatient.Text = "患者：未选择";
            }
        }

        /// <summary>
        /// 窗体加载完成后的统一初始化逻辑（合并重复的Load事件，避免重复执行）
        /// </summary>
        private void FrmPatientRecordManage_Load(object sender, EventArgs e)
        {
            // 窗体加载完成后，再执行数据加载，保证控件全部初始化完成
            LoadHistoryPatientList();
            LoadList();
        }
        #endregion
        #endregion


        private void ClearEditControls()
        {
            // 递归清空控件的通用方法
            void ClearControls(Control parent)
            {
                foreach (Control item in parent.Controls)
                {
                    if (item is TextBox t && !t.ReadOnly) t.Clear();
                    if (item is ComboBox c && c.Enabled) c.SelectedIndex = -1;
                    if (item is DateTimePicker d && d.Enabled) d.Checked = false;
                    if (item.HasChildren) ClearControls(item); // 递归查找子容器内的控件
                }
            }

            // 清空三个分组框内的所有控件
            ClearControls(grp_BaseInfo);
            ClearControls(grp_Disease);
            ClearControls(grp_Assessment);

            // 重置模式状态
            _isViewDetailMode = false;
        }

        private void LoadList()
        {
            try
            {
                DateTime? start = null;
                if (dtp_StartDate.Checked) start = dtp_StartDate.Value;
                DateTime? end = null;
                if (dtp_EndDate.Checked) end = dtp_EndDate.Value;
                var list = _bllPatient.GetPatientPageList(_pageIndex, _pageSize, out _totalCount,
                    txt_Name.Text.Trim(), txt_Phone.Text.Trim(),
                    cbo_DiabetesType.Text == "全部" ? "" : cbo_DiabetesType.Text,
                    cbo_ControlStatus.Text == "全部" ? "" : cbo_ControlStatus.Text,
                    start, end, chk_OnlyValid.Checked);
                dgv_PatientList.DataSource = list;
                int totalPage = (int)Math.Ceiling(_totalCount * 1.0 / _pageSize);
                lbl_PageInfo.Text = $"第{_pageIndex}页/共{totalPage}页 总计{_totalCount}条";
                btn_Prev.Enabled = _pageIndex > 1;
                btn_Next.Enabled = _pageIndex < totalPage;
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载列表失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 替换原有LoadPatientDetail方法
        private void LoadPatientDetail(int userId)
        {
            try
            {
                var patient = _bllPatient.GetPatientDetail(userId);
                if (patient == null || patient.BaseInfo == null)
                {
                    MessageBox.Show("未找到该患者的档案信息！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                #region 填充基础信息
                txt_Base_Name.Text = patient.BaseInfo.user_name;
                txt_Base_ID.Text = patient.BaseInfo.id_card;
                txt_Base_Phone.Text = patient.BaseInfo.phone;
                cbo_Gender.Text = patient.BaseInfo.gender == 1 ? "男" : "女";
                txt_Base_Age.Text = patient.BaseInfo.age.ToString();
                txt_Base_Emergency.Text = patient.BaseInfo.emergency_contact;
                txt_Base_EmerPhone.Text = patient.BaseInfo.emergency_phone;
                #endregion

                #region 填充患病信息
                cbo_Diabetes.Text = patient.BaseInfo.diabetes_type;
                if (patient.BaseInfo.diagnose_date.HasValue)
                {
                    dtp_Diagnose.Value = patient.BaseInfo.diagnose_date.Value;
                    dtp_Diagnose.Checked = true;
                }
                else
                {
                    dtp_Diagnose.Checked = false;
                }
                txt_Baseline.Text = patient.BaseInfo.fasting_glucose_baseline?.ToString();
                txt_Duration.Text = patient.LatestAssessment?.disease_duration_years?.ToString();
                #endregion

                #region 填充健康评估信息
                if (patient.LatestAssessment != null)
                {
                    dtp_AssessDate.Value = patient.LatestAssessment.assessment_date;
                    dtp_AssessDate.Checked = true;
                    txt_Height.Text = patient.LatestAssessment.height?.ToString();
                    txt_Weight.Text = patient.LatestAssessment.weight?.ToString();
                    txt_BMI.Text = patient.LatestAssessment.bmi?.ToString();
                    txt_Systolic.Text = patient.LatestAssessment.systolic_bp?.ToString();
                    txt_Diastolic.Text = patient.LatestAssessment.diastolic_bp?.ToString();
                    txt_Heart.Text = patient.LatestAssessment.heart_rate?.ToString();
                    txt_HbA1c.Text = patient.LatestAssessment.hba1c?.ToString();
                    txt_AvgFast.Text = patient.LatestAssessment.avg_fasting_glucose?.ToString() ?? patient.AvgFastingGlucose?.ToString();
                    txt_AvgPost.Text = patient.LatestAssessment.avg_postprandial_glucose?.ToString() ?? patient.AvgPostprandialGlucose?.ToString();
                    cbo_ControlStatusEdit.Text = patient.LatestAssessment.glycemic_control_status;
                    txt_Complication.Text = patient.LatestAssessment.diabetes_complications;
                    txt_Comorbidity.Text = patient.LatestAssessment.comorbidities;
                }
                else
                {
                    dtp_AssessDate.Checked = false;
                    txt_Height.Clear();
                    txt_Weight.Clear();
                    txt_BMI.Clear();
                    txt_Systolic.Clear();
                    txt_Diastolic.Clear();
                    txt_Heart.Clear();
                    txt_HbA1c.Clear();
                    txt_AvgFast.Clear();
                    txt_AvgPost.Clear();
                    cbo_ControlStatusEdit.SelectedIndex = 0;
                    txt_Complication.Clear();
                    txt_Comorbidity.Clear();
                }
                #endregion

                #region 区分查看/编辑状态，设置控件只读权限
                bool isReadOnly = _isViewDetailMode;
                // 基础信息控件（仅紧急联系人/电话可编辑，其他核心信息只读）
                txt_Base_Name.ReadOnly = true;
                txt_Base_ID.ReadOnly = true;
                txt_Base_Phone.ReadOnly = true;
                cbo_Gender.Enabled = false;
                txt_Base_Age.ReadOnly = true;
                txt_Base_Emergency.ReadOnly = isReadOnly;
                txt_Base_EmerPhone.ReadOnly = isReadOnly;

                // 患病信息控件
                cbo_Diabetes.Enabled = !isReadOnly;
                dtp_Diagnose.Enabled = !isReadOnly;
                txt_Baseline.ReadOnly = isReadOnly;
                txt_Duration.ReadOnly = true; // 病程自动计算，始终只读

                // 健康评估控件
                dtp_AssessDate.Enabled = !isReadOnly;
                txt_Height.ReadOnly = isReadOnly;
                txt_Weight.ReadOnly = isReadOnly;
                txt_BMI.ReadOnly = true; // BMI自动计算，始终只读
                txt_Systolic.ReadOnly = isReadOnly;
                txt_Diastolic.ReadOnly = isReadOnly;
                txt_Heart.ReadOnly = isReadOnly;
                txt_HbA1c.ReadOnly = isReadOnly;
                txt_AvgFast.ReadOnly = isReadOnly;
                txt_AvgPost.ReadOnly = isReadOnly;
                cbo_ControlStatusEdit.Enabled = !isReadOnly;
                txt_Complication.ReadOnly = isReadOnly;
                txt_Comorbidity.ReadOnly = isReadOnly;

                // 保存按钮仅编辑模式可见
                btn_Save.Visible = !isReadOnly;
                #endregion

                // 同步更新历史追溯页的患者名称（替换原有代码）
                if (lbl_HistoryPatient != null)
                {
                    string displayName = patient?.BaseInfo?.user_name?.Trim();
                    lbl_HistoryPatient.Text = !string.IsNullOrEmpty(displayName)
                        ? $"患者：{displayName}"
                        : "患者：未找到";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载患者详情失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 替换原有LoadPatientHistory方法
        private void LoadPatientHistory(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    MessageBox.Show("请先选择患者！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // 处理时间范围
                DateTime startTime = dtp_HistoryStart.Checked ? dtp_HistoryStart.Value : DateTime.Now.AddMonths(-3);
                DateTime endTime = dtp_HistoryEnd.Checked ? dtp_HistoryEnd.Value : DateTime.Now;
                // 处理记录类型
                string recordType = cbo_RecordType.Text == "全部" ? "" : cbo_RecordType.Text;
                // 调用BLL查询
                var historyList = _bllPatient.GetPatientHistoryList(userId, startTime, endTime, recordType);
                // 绑定数据到表格
                dgv_HistoryList.DataSource = null;
                dgv_HistoryList.DataSource = historyList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载患者历史档案失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}