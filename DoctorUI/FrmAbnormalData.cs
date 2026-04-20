using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using BLL;
using Model;

namespace DoctorUI
{
    public partial class FrmAbnormalData : Form
    {
        #region ========== 全局统一布局参数（与随访管理页完全一致）==========
        private readonly Padding _globalMainContainerPadding = new Padding(15, 15, 15, 15);
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
        private readonly Padding _globalControlMargin = new Padding(5, 5, 5, 5);
        private readonly int _globalControlHeight = 28;
        private readonly int _globalButtonHeight = 36;
        private readonly int _globalButtonWidth = 110;
        private readonly int _globalLabelWidth = 120;
        private readonly int _globalRowHeight = 40;
        private readonly Padding _globalGroupBoxPadding = new Padding(15);
        private readonly Color _themeColor = Color.FromArgb(0, 122, 204);
        #endregion

        #region 核心控件声明（完全匹配截图4个标签页，删除废弃ComboBox）
        private Panel pnlMainContainer;
        private Panel pnlContentWrapper;
        private TabControl tabMain;
        private TabPage tab_Warn, tab_Mark, tab_Intervention, tab_Review;
        // 1. 异常数据预警标签页（删除cbo_WarnPatient）
        private GroupBox grp_WarnQuery;
        private ComboBox cbo_WarnAbnormalType; // 仅保留异常类型下拉框
        private DateTimePicker dtp_WarnStart, dtp_WarnEnd;
        private DataGridView dgv_WarnList;
        private Button btn_QueryWarn, btn_ResetWarn;
        // 2. 异常原因标注标签页（删除cbo_MarkPatient）
        private GroupBox grp_MarkContent, grp_MarkPatient;
        private TextBox txt_AbnormalReason, txt_ClinicalSuggestion;
        private ComboBox cbo_MarkAbnormalData; // 仅保留异常数据下拉框
        private Button btn_SaveMark, btn_ResetMark;
        // 3. 异常干预处理标签页（删除cbo_InterventionPatient）
        private GroupBox grp_InterventionContent, grp_InterventionPatient;
        private TextBox txt_InterventionContent, txt_InterventionEffect;
        private ComboBox cbo_InterventionType; // 仅保留干预类型下拉框
        private Button btn_SaveIntervention, btn_ResetIntervention;
        // 4. 复查提醒管理标签页（删除cbo_ReviewPatient）
        private GroupBox grp_ReviewQuery;
        private DateTimePicker dtp_ReviewDate, dtp_ReviewStart, dtp_ReviewEnd;
        private DataGridView dgv_ReviewList;
        private Button btn_SetReview, btn_QueryReview, btn_ResetReview;
        #endregion

        #region 全局业务变量
        private readonly B_Abnormal _bllAbnormal = new B_Abnormal();
        private List<PatientSimpleInfo> _allPatientList = new List<PatientSimpleInfo>();
        // 追加：保存各标签页选中的患者ID（完全兼容原有业务逻辑）
        private int _selectedWarnPatientId = 0; // 异常预警选中的患者ID（0=全部）
        private int _selectedMarkPatientId = 0; // 异常标注选中的患者ID
        private int _selectedInterventionPatientId = 0; // 干预处理选中的患者ID
        private int _selectedReviewPatientId = 0; // 复查提醒选中的患者ID（0=全部）
        /// <summary>
        /// 选中的异常记录ID
        /// </summary>
        private int _selectAbnormalId = 0;
        /// <summary>
        /// 分页页码
        /// </summary>
        private int _pageIndex = 1;
        /// <summary>
        /// 分页每页条数
        /// </summary>
        private readonly int _pageSize = 20;
        /// <summary>
        /// 总记录数
        /// </summary>
        private int _totalCount = 0;
        /// <summary>
        /// 异常数据表格控件
        /// </summary>
        private DataGridView dgv_AbnormalList;
        #endregion

        public FrmAbnormalData()
        {
            // 窗体基础配置（与系统其他页面完全统一，原有代码完全保留）
            this.Text = "异常数据处理";
            this.Size = new Size(1300, 800);
            this.MinimumSize = new Size(_globalContentMinWidth + 40, _globalContentMinHeight + 100);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("微软雅黑", 9.5F);
            this.Dock = DockStyle.Fill;

            // 【执行顺序优化】先创建所有控件
            InitMainContainer();
            InitializeDynamicControls(); // 所有控件在这里完成实例化创建
            InitControlData();
            BindAllEvents();

            // 【关键优化】控件创建完成后，立即执行一次初始化，避免Load事件延迟执行导致的潜在问题
            InitDefaultValue();

            // 原有Load事件注册保留，不影响原有逻辑
            this.Load += FrmAbnormalDataManage_Load;
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

        #region 动态创建所有控件（完全匹配截图布局）
        private void InitializeDynamicControls()
        {
            // 主标签控件（4个标签页与截图完全一致）
            tabMain = new TabControl();
            tabMain.Dock = DockStyle.Fill;
            tabMain.Font = new Font("微软雅黑", 10F);
            tabMain.Padding = new Point(15, 8);

            tab_Warn = new TabPage("异常数据预警") { BackColor = Color.White };
            tab_Mark = new TabPage("异常原因标注") { BackColor = Color.White };
            tab_Intervention = new TabPage("异常干预处理") { BackColor = Color.White };
            tab_Review = new TabPage("复查提醒管理") { BackColor = Color.White };

            tabMain.TabPages.AddRange(new TabPage[] { tab_Warn, tab_Mark, tab_Intervention, tab_Review });
            pnlContentWrapper.Controls.Add(tabMain);

            // 初始化4个标签页
            InitWarnPage();
            InitMarkPage();
            InitInterventionPage();
            InitReviewPage();
        }
        #region 页面初始化
        private void InitializePage()
        {
            // 这里写你原有界面布局代码，必须给dgv_AbnormalList实例化
            // ========== 核心：必须实例化dgv_AbnormalList，否则报错 ==========
            dgv_AbnormalList = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeight = 35,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                ScrollBars = ScrollBars.Both,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                Name = "dgv_AbnormalList"
            };

            // 给表格添加列（和你数据库表字段对应）
            dgv_AbnormalList.Columns.AddRange(new[]
            {
                new DataGridViewTextBoxColumn { Name = "abnormal_id", HeaderText = "异常ID", DataPropertyName = "abnormal_id", Visible = false },
                new DataGridViewTextBoxColumn { HeaderText = "患者姓名", DataPropertyName = "PatientName", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "异常类型", DataPropertyName = "abnormal_type", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "异常原因", DataPropertyName = "abnormal_reason", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "处理状态", DataPropertyName = "HandleStatusDesc", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "创建时间", DataPropertyName = "create_time", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm" } }
            });

            // 把表格添加到窗体
            this.Controls.Add(dgv_AbnormalList);
        }
        #endregion

        #region 事件绑定
        private void BindEvents()
        {
            this.Load += (s, e) => LoadAbnormalList();
            dgv_AbnormalList.SelectionChanged += (s, e) =>
            {
                if (dgv_AbnormalList.SelectedRows.Count > 0 && dgv_AbnormalList.SelectedRows[0].DataBoundItem is AbnormalWarnViewModel model)
                {
                    _selectAbnormalId = model.abnormal_id;
                }
            };
        }
        #endregion

        #region 【核心：补全列表加载方法，解决LoadAbnormalList找不到的报错】
        /// <summary>
        /// 加载异常数据列表
        /// </summary>
        public void LoadAbnormalList()
        {
            try
            {
                var result = _bllAbnormal.GetAbnormalWarnList(null, "", DateTime.Now.AddMonths(-3), DateTime.Now);
                if (result.Success)
                {
                    dgv_AbnormalList.DataSource = null;
                    dgv_AbnormalList.DataSource = result.Data;
                }
                else
                {
                    MessageBox.Show($"加载数据失败：{result.Msg}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载异常数据失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 【核心：待办跳转专用方法，必须写在窗体类内部，解决作用域错误】
        /// <summary>
        /// 根据异常ID加载对应异常数据（首页待办跳转专用）
        /// </summary>
        /// <param name="abnormalId">异常记录ID</param>
        public void LoadAbnormalById(int abnormalId)
        {
            try
            {
                // 1. 定位到对应异常记录
                _selectAbnormalId = abnormalId;
                _pageIndex = 1;
                // 2. 重新加载列表
                LoadAbnormalList();
                // 3. 自动选中目标行
                if (dgv_AbnormalList.Rows.Count > 0)
                {
                    foreach (DataGridViewRow row in dgv_AbnormalList.Rows)
                    {
                        if (row.Cells["abnormal_id"].Value?.ToString() == abnormalId.ToString())
                        {
                            row.Selected = true;
                            dgv_AbnormalList.FirstDisplayedScrollingRowIndex = row.Index;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载异常数据失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
        #region 1. 异常数据预警页面（修复后，补全异常类型控件，布局完全匹配原设计）
        private void InitWarnPage()
        {
            // 查询条件分组（原有代码完全保留）
            grp_WarnQuery = new GroupBox { Text = "预警查询条件", Dock = DockStyle.Top, Height = 160, Padding = _globalGroupBoxPadding };
            tab_Warn.Controls.Add(grp_WarnQuery);
            TableLayoutPanel tlp_Warn = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Warn.RowCount = 2;
            tlp_Warn.ColumnCount = 3;
            tlp_Warn.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlp_Warn.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlp_Warn.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            tlp_Warn.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            tlp_Warn.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_WarnQuery.Controls.Add(tlp_Warn);

            int row = 0; // 行计数初始化

            // ========== 1. 选择患者控件（原有改造代码完全保留）==========
            Panel pnl_WarnPatient = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_WarnPatient = new Label { Text = "选择患者：", Size = new Size(_globalLabelWidth, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(0, 0) };
            TextBox txt_WarnPatient = new TextBox { Name = "txt_WarnPatient", Text = "全部患者", Size = new Size(100, _globalControlHeight), Location = new Point(_globalLabelWidth, 0), ReadOnly = true, BackColor = Color.FromArgb(245, 245, 245) };
            Button btn_SelectWarnPatient = new Button { Name = "btn_SelectWarnPatient", Text = "选择患者", BackColor = _themeColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(90, _globalControlHeight), Location = new Point(_globalLabelWidth +120, 0) };
            pnl_WarnPatient.Controls.AddRange(new Control[] { lbl_WarnPatient, txt_WarnPatient, btn_SelectWarnPatient });
            tlp_Warn.Controls.Add(pnl_WarnPatient, 0, 0); // 第一行第0列
            row++; // 【关键修复】行计数自增，和原有CreateEditItem逻辑保持一致

            // ========== 【核心修复】补全异常类型下拉框创建（解决null异常）==========
            CreateEditItem<ComboBox>(tlp_Warn, out _, out cbo_WarnAbnormalType, "异常类型：", ref row, false);
            tlp_Warn.SetCellPosition(cbo_WarnAbnormalType.Parent, new TableLayoutPanelCellPosition(1, 0)); // 放到第一行第1列，匹配原3列布局

            // ========== 3. 数据时间范围控件（原有代码完全保留，调整行计数）==========
            Panel pnl_DateRange = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_Date = new Label { Text = "数据时间：", Size = new Size(80, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(0, 0) };
            dtp_WarnStart = new DateTimePicker { Location = new Point(80, 0), Size = new Size(140, _globalControlHeight), Format = DateTimePickerFormat.Short };
            dtp_WarnEnd = new DateTimePicker { Location = new Point(200, 0), Size = new Size(140, _globalControlHeight), Format = DateTimePickerFormat.Short };
            pnl_DateRange.Controls.AddRange(new Control[] { lbl_Date, dtp_WarnStart, dtp_WarnEnd });
            tlp_Warn.Controls.Add(pnl_DateRange, 2, 0); // 第一行第2列
            row++;

            // ========== 查询/重置按钮区（原有代码完全保留）==========
            Panel pnl_WarnBtn = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            FlowLayoutPanel flp_WarnBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            btn_QueryWarn = CreateBtn("查询预警", _themeColor);
            btn_ResetWarn = CreateBtn("重置", Color.Gray);
            flp_WarnBtn.Controls.AddRange(new Control[] { btn_ResetWarn, btn_QueryWarn });
            pnl_WarnBtn.Controls.Add(flp_WarnBtn);
            tlp_Warn.Controls.Add(pnl_WarnBtn, 2, 1); // 第二行第2列

            // ========== 异常预警列表分组（原有代码100%完全保留）==========
            GroupBox grp_WarnList = new GroupBox { Text = "异常预警数据列表", Dock = DockStyle.Fill, Padding = _globalGroupBoxPadding };
            tab_Warn.Controls.Add(grp_WarnList);
            dgv_WarnList = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };
            grp_WarnList.Controls.Add(dgv_WarnList);
            // 列表列定义（原有代码完全保留）
            dgv_WarnList.Columns.AddRange(new DataGridViewColumn[] {
        new DataGridViewTextBoxColumn { HeaderText = "异常ID", DataPropertyName = "AbnormalId", Visible = false },
        new DataGridViewTextBoxColumn { HeaderText = "患者姓名", DataPropertyName = "PatientName", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
        new DataGridViewTextBoxColumn { HeaderText = "数据类型", DataPropertyName = "DataType", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
        new DataGridViewTextBoxColumn { HeaderText = "异常类型", DataPropertyName = "AbnormalType", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
        new DataGridViewTextBoxColumn { HeaderText = "异常描述", DataPropertyName = "AbnormalDesc", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
        new DataGridViewTextBoxColumn { HeaderText = "异常时间", DataPropertyName = "AbnormalTime", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm" } },
        new DataGridViewTextBoxColumn { HeaderText = "处理状态", DataPropertyName = "HandleStatusDesc", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
        new DataGridViewTextBoxColumn { HeaderText = "标注医生", DataPropertyName = "MarkDoctor", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
        new DataGridViewTextBoxColumn { HeaderText = "标注时间", DataPropertyName = "MarkTime", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm" } }
    });
        }
        #endregion
        #region 2. 异常原因标注页面（与截图完全一致）
        private void InitMarkPage()
        {
            // 按钮区
            Panel pnl_MarkBtn = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(15) };
            tab_Mark.Controls.Add(pnl_MarkBtn);
            FlowLayoutPanel flp_MarkBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_SaveMark = CreateBtn("保存标注", _themeColor);
            btn_ResetMark = CreateBtn("重置", Color.Gray);
            flp_MarkBtn.Controls.AddRange(new Control[] { btn_SaveMark, btn_ResetMark });
            pnl_MarkBtn.Controls.Add(flp_MarkBtn);

            // 异常原因标注分组
            grp_MarkContent = new GroupBox { Text = "异常原因标注", Dock = DockStyle.Top, Height = 300, Padding = _globalGroupBoxPadding };
            tab_Mark.Controls.Add(grp_MarkContent);

            TableLayoutPanel tlp_MarkContent = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_MarkContent.RowCount = 2;
            tlp_MarkContent.ColumnCount = 1;
            tlp_MarkContent.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlp_MarkContent.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            grp_MarkContent.Controls.Add(tlp_MarkContent);

            // 异常原因输入框
            txt_AbnormalReason = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = this.Font,
                Margin = _globalControlMargin,
               Text = "请输入异常原因（如：饮食不规律、药物未按时服用、并发症影响等）"
            };
            tlp_MarkContent.Controls.Add(txt_AbnormalReason, 0, 0);

            // 临床备注输入框
            txt_ClinicalSuggestion = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = this.Font,
                Margin = _globalControlMargin,
               Text = "临床备注（如：建议调整饮食、监测血压等）"
            };
            tlp_MarkContent.Controls.Add(txt_ClinicalSuggestion, 0, 1);

            // 患者与异常数据分组
            grp_MarkPatient = new GroupBox { Text = "患者与异常数据", Dock = DockStyle.Top, Height = 160, Padding = _globalGroupBoxPadding };
            tab_Mark.Controls.Add(grp_MarkPatient);

            TableLayoutPanel tlp_MarkPatient = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_MarkPatient.RowCount = 1;
            tlp_MarkPatient.ColumnCount = 2;
            tlp_MarkPatient.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_MarkPatient.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_MarkPatient.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_MarkPatient.Controls.Add(tlp_MarkPatient);

            int row = 0;
            // ========== 核心改造：替换原有ComboBox为文本框+选择按钮 ==========
            Panel pnl_MarkPatient = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_MarkPatient = new Label { Text = "选择患者：", Size = new Size(_globalLabelWidth, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(0, 0) };
            TextBox txt_MarkPatient = new TextBox { Name = "txt_MarkPatient", Text = "请选择患者", Size = new Size(200, _globalControlHeight), Location = new Point(_globalLabelWidth, 0), ReadOnly = true, BackColor = Color.FromArgb(245, 245, 245) };
            Button btn_SelectMarkPatient = new Button { Name = "btn_SelectMarkPatient", Text = "选择患者", BackColor = _themeColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(90, _globalControlHeight), Location = new Point(_globalLabelWidth + 210, 0) };
            pnl_MarkPatient.Controls.AddRange(new Control[] { lbl_MarkPatient, txt_MarkPatient, btn_SelectMarkPatient });
            tlp_MarkPatient.Controls.Add(pnl_MarkPatient, 0, 0);
            CreateEditItem<ComboBox>(tlp_MarkPatient, out _, out cbo_MarkAbnormalData, "选择异常数据：", ref row, false);
        }
        #endregion

        #region 3. 异常干预处理页面（与截图完全一致）
        private void InitInterventionPage()
        {
            // 按钮区
            Panel pnl_InterventionBtn = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(15) };
            tab_Intervention.Controls.Add(pnl_InterventionBtn);
            FlowLayoutPanel flp_InterventionBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_SaveIntervention = CreateBtn("保存干预方案", _themeColor);
            btn_ResetIntervention = CreateBtn("重置", Color.Gray);
            flp_InterventionBtn.Controls.AddRange(new Control[] { btn_SaveIntervention, btn_ResetIntervention });
            pnl_InterventionBtn.Controls.Add(flp_InterventionBtn);

            // 干预处理内容分组
            grp_InterventionContent = new GroupBox { Text = "干预处理内容", Dock = DockStyle.Top, Height = 300, Padding = _globalGroupBoxPadding };
            tab_Intervention.Controls.Add(grp_InterventionContent);

            TableLayoutPanel tlp_InterventionContent = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_InterventionContent.RowCount = 2;
            tlp_InterventionContent.ColumnCount = 1;
            tlp_InterventionContent.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlp_InterventionContent.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            grp_InterventionContent.Controls.Add(tlp_InterventionContent);

            // 干预措施输入框
            txt_InterventionContent = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = this.Font,
                Margin = _globalControlMargin,
                Text = "请输入干预处理措施（如：调整用药剂量、饮食干预方案、运动建议等）"
            };
            tlp_InterventionContent.Controls.Add(txt_InterventionContent, 0, 0);

            // 干预效果预期输入框
            txt_InterventionEffect = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = this.Font,
                Margin = _globalControlMargin,
                Text = "干预效果预期（如：血糖控制在7.0mmol/L以下、血压稳定在130/80mmHg以下）"
            };
            tlp_InterventionContent.Controls.Add(txt_InterventionEffect, 0, 1);

            // 患者与干预类型分组
            grp_InterventionPatient = new GroupBox { Text = "患者与干预类型", Dock = DockStyle.Top, Height = 160, Padding = _globalGroupBoxPadding };
            tab_Intervention.Controls.Add(grp_InterventionPatient);

            TableLayoutPanel tlp_InterventionPatient = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_InterventionPatient.RowCount = 1;
            tlp_InterventionPatient.ColumnCount = 2;
            tlp_InterventionPatient.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_InterventionPatient.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_InterventionPatient.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_InterventionPatient.Controls.Add(tlp_InterventionPatient);

            int row = 0;
            Panel pnl_InterventionPatient = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_InterventionPatient = new Label { Text = "选择患者：", Size = new Size(_globalLabelWidth, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(0, 0) };
            TextBox txt_InterventionPatient = new TextBox { Name = "txt_InterventionPatient", Text = "请选择患者", Size = new Size(200, _globalControlHeight), Location = new Point(_globalLabelWidth, 0), ReadOnly = true, BackColor = Color.FromArgb(245, 245, 245) };
            Button btn_SelectInterventionPatient = new Button { Name = "btn_SelectInterventionPatient", Text = "选择患者", BackColor = _themeColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(90, _globalControlHeight), Location = new Point(_globalLabelWidth + 210, 0) };
            pnl_InterventionPatient.Controls.AddRange(new Control[] { lbl_InterventionPatient, txt_InterventionPatient, btn_SelectInterventionPatient });
            tlp_InterventionPatient.Controls.Add(pnl_InterventionPatient, 0, 0);
            CreateEditItem<ComboBox>(tlp_InterventionPatient, out _, out cbo_InterventionType, "干预类型：", ref row, false);
        }
        #endregion

        #region 4. 复查提醒管理页面（与截图完全一致）
        private void InitReviewPage()
        {
            // 查询条件分组
            grp_ReviewQuery = new GroupBox { Text = "复查提醒设置/查询", Dock = DockStyle.Top, Height = 160, Padding = _globalGroupBoxPadding };
            tab_Review.Controls.Add(grp_ReviewQuery);

            TableLayoutPanel tlp_Review = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Review.RowCount = 2;
            tlp_Review.ColumnCount = 3;
            tlp_Review.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlp_Review.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlp_Review.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            tlp_Review.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            tlp_Review.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_ReviewQuery.Controls.Add(tlp_Review);

            int row = 0;
            Panel pnl_ReviewPatient = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_ReviewPatient = new Label { Text = "选择患者：", Size = new Size(_globalLabelWidth, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(0, 0) };
            TextBox txt_ReviewPatient = new TextBox { Name = "txt_ReviewPatient", Text = "全部患者", Size = new Size(50, _globalControlHeight), Location = new Point(_globalLabelWidth, 0), ReadOnly = true, BackColor = Color.FromArgb(245, 245, 245) };
            Button btn_SelectReviewPatient = new Button { Name = "btn_SelectReviewPatient", Text = "选择患者", BackColor = _themeColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(90, _globalControlHeight), Location = new Point(_globalLabelWidth + 210, 0) };
            pnl_ReviewPatient.Controls.AddRange(new Control[] { lbl_ReviewPatient, txt_ReviewPatient, btn_SelectReviewPatient });
            tlp_Review.Controls.Add(pnl_ReviewPatient, 0, 0);
            CreateEditItem<DateTimePicker>(tlp_Review, out _, out dtp_ReviewDate, "复查日期：", ref row, false);

            // 查询时间范围
            Panel pnl_ReviewDateRange = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_ReviewDate = new Label { Text = "查询时间：", Size = new Size(80, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(0, 0) };
            dtp_ReviewStart = new DateTimePicker { Location = new Point(80, 0), Size = new Size(140, _globalControlHeight), Format = DateTimePickerFormat.Short };
            dtp_ReviewEnd = new DateTimePicker { Location = new Point(230, 0), Size = new Size(140, _globalControlHeight), Format = DateTimePickerFormat.Short };
            pnl_ReviewDateRange.Controls.AddRange(new Control[] { lbl_ReviewDate, dtp_ReviewStart, dtp_ReviewEnd });
            tlp_Review.Controls.Add(pnl_ReviewDateRange, 2, 0);

            // 按钮区
            Panel pnl_ReviewBtn = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            FlowLayoutPanel flp_ReviewBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            btn_SetReview = CreateBtn("设置提醒", Color.FromArgb(255, 193, 7));
            btn_QueryReview = CreateBtn("查询提醒", _themeColor);
            btn_ResetReview = CreateBtn("重置", Color.Gray);
            flp_ReviewBtn.Controls.AddRange(new Control[] { btn_ResetReview, btn_QueryReview, btn_SetReview });
            pnl_ReviewBtn.Controls.Add(flp_ReviewBtn);
            tlp_Review.Controls.Add(pnl_ReviewBtn, 2, 1);

            // 复查提醒列表分组
            GroupBox grp_ReviewList = new GroupBox { Text = "复查提醒列表", Dock = DockStyle.Fill, Padding = _globalGroupBoxPadding };
            tab_Review.Controls.Add(grp_ReviewList);

            dgv_ReviewList = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };
            grp_ReviewList.Controls.Add(dgv_ReviewList);

            // 列表列定义
            dgv_ReviewList.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { HeaderText = "提醒ID", DataPropertyName = "RemindId", Visible = false },
                new DataGridViewTextBoxColumn { HeaderText = "患者姓名", DataPropertyName = "PatientName", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "复查日期", DataPropertyName = "ReviewDate", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd" } },
                new DataGridViewTextBoxColumn { HeaderText = "提醒内容", DataPropertyName = "RemindContent", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "创建医生", DataPropertyName = "CreateDoctor", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "创建时间", DataPropertyName = "CreateTime", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm" } },
                new DataGridViewTextBoxColumn { HeaderText = "状态", DataPropertyName = "StatusDesc", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
            });
        }
        #endregion
        #endregion

        #region 原有通用控件创建方法（完全保留，无修改）
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
        #endregion

        #region 窗体加载与数据初始化
        private void FrmAbnormalDataManage_Load(object sender, EventArgs e)
        {
            // 登录信息校验（复用系统GlobalConfig，原有代码完全保留）
            if (GlobalConfig.CurrentDoctorID <= 0)
            {
                MessageBox.Show("登录信息失效，请重新登录系统！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }
            // 【优化】移除重复的InitDefaultValue()调用，避免重复执行
        }

        #region 追加：患者下拉框模糊搜索通用方法
        /// <summary>
        /// 为患者下拉框添加模糊搜索功能（输入姓名/手机号过滤）
        /// </summary>
        private void AddPatientSearchFunction(ComboBox cbo)
        {
            cbo.TextUpdate += (s, e) =>
            {
                string searchKey = cbo.Text.Trim();
                if (string.IsNullOrEmpty(searchKey))
                {
                    // 清空搜索时恢复全量列表
                    if (cbo.Name == "cbo_WarnPatient" || cbo.Name == "cbo_ReviewPatient")
                    {
                        var allList = new List<PatientSimpleInfo>
                        {
                            new PatientSimpleInfo { UserId = 0, DisplayText = "全部患者" }
                        };
                        allList.AddRange(_allPatientList);
                        cbo.DataSource = allList;
                    }
                    else
                    {
                        cbo.DataSource = _allPatientList;
                    }
                    return;
                }

                // 多维度模糊匹配（姓名/手机号/糖尿病类型）
                var filterList = _allPatientList.Where(p =>
                    p.UserName.Contains(searchKey) ||
                    !string.IsNullOrEmpty(p.Phone) && p.Phone.Contains(searchKey) ||
                    !string.IsNullOrEmpty(p.DiabetesType) && p.DiabetesType.Contains(searchKey)
                ).ToList();

                // 带"全部"选项的下拉框特殊处理
                if (cbo.Name == "cbo_WarnPatient" || cbo.Name == "cbo_ReviewPatient")
                {
                    var allList = new List<PatientSimpleInfo>
                    {
                        new PatientSimpleInfo { UserId = 0, DisplayText = "全部患者" }
                    };
                    allList.AddRange(filterList);
                    cbo.DataSource = allList;
                }
                else
                {
                    cbo.DataSource = filterList;
                }

                // 自动展开下拉框，提升体验
                cbo.DroppedDown = true;
            };
        }
        #endregion
        /// <summary>
        /// 初始化下拉框默认值
        /// </summary>
        private void InitDefaultValue()
        {
            // 异常类型下拉框
            cbo_WarnAbnormalType.Items.Clear();
            cbo_WarnAbnormalType.Items.AddRange(new string[] { "全部", "高血糖", "低血糖", "饮食超标", "运动不足", "用药不规范" });
            cbo_WarnAbnormalType.SelectedIndex = 0;

            // 干预类型下拉框
            cbo_InterventionType.Items.Clear();
            cbo_InterventionType.Items.AddRange(new string[] { "饮食干预", "运动干预", "用药调整", "综合干预" });
            cbo_InterventionType.SelectedIndex = 0;

            // 日期默认值
            dtp_WarnStart.Value = DateTime.Now.AddMonths(-1);
            dtp_WarnEnd.Value = DateTime.Now;
            dtp_ReviewDate.Value = DateTime.Now.AddDays(7);
            dtp_ReviewStart.Value = DateTime.Now.AddMonths(-3);
            dtp_ReviewEnd.Value = DateTime.Now;

            //// 默认选中第一项
            //cbo_WarnPatient.SelectedIndex = 0;
            //cbo_MarkPatient.SelectedIndex = 0;
            //cbo_InterventionPatient.SelectedIndex = 0;
            //cbo_ReviewPatient.SelectedIndex = 0;
        }

        private void InitControlData()
        {

        }
        #endregion

        #region 事件绑定与核心业务逻辑实现
        private void BindAllEvents()
        {
            #region 【核心修复】4个选择患者按钮 完整绑定弹窗事件（无重复+空值防护）
            // ==================================
            // 1. 异常数据预警页 - 选择患者按钮
            // ==================================
            var btn_SelectWarnPatient = this.Controls.Find("btn_SelectWarnPatient", true).FirstOrDefault() as Button;
            if (btn_SelectWarnPatient != null)
            {
                // 先解绑再绑定，防止重复注册
                btn_SelectWarnPatient.Click -= Btn_SelectWarnPatient_Click;
                btn_SelectWarnPatient.Click += Btn_SelectWarnPatient_Click;
            }

            // ==================================
            // 2. 异常原因标注页 - 选择患者按钮
            // ==================================
            var btn_SelectMarkPatient = this.Controls.Find("btn_SelectMarkPatient", true).FirstOrDefault() as Button;
            if (btn_SelectMarkPatient != null)
            {
                btn_SelectMarkPatient.Click -= Btn_SelectMarkPatient_Click;
                btn_SelectMarkPatient.Click += Btn_SelectMarkPatient_Click;
            }

            // ==================================
            // 3. 异常干预处理页 - 选择患者按钮
            // ==================================
            var btn_SelectInterventionPatient = this.Controls.Find("btn_SelectInterventionPatient", true).FirstOrDefault() as Button;
            if (btn_SelectInterventionPatient != null)
            {
                btn_SelectInterventionPatient.Click -= Btn_SelectInterventionPatient_Click;
                btn_SelectInterventionPatient.Click += Btn_SelectInterventionPatient_Click;
            }

            // ==================================
            // 4. 复查提醒管理页 - 选择患者按钮
            // ==================================
            var btn_SelectReviewPatient = this.Controls.Find("btn_SelectReviewPatient", true).FirstOrDefault() as Button;
            if (btn_SelectReviewPatient != null)
            {
                btn_SelectReviewPatient.Click -= Btn_SelectReviewPatient_Click;
                btn_SelectReviewPatient.Click += Btn_SelectReviewPatient_Click;
            }
            #endregion

            #region 异常数据预警 按钮事件（仅绑定1次，无重复）
            // 查询预警按钮
            if (btn_QueryWarn != null)
            {
                btn_QueryWarn.Click -= Btn_QueryWarn_Click;
                btn_QueryWarn.Click += Btn_QueryWarn_Click;
            }
            // 重置按钮
            if (btn_ResetWarn != null)
            {
                btn_ResetWarn.Click -= Btn_ResetWarn_Click;
                btn_ResetWarn.Click += Btn_ResetWarn_Click;
            }
            #endregion

            #region 异常原因标注 按钮事件（仅绑定1次，无重复）
            // 保存标注按钮
            if (btn_SaveMark != null)
            {
                btn_SaveMark.Click -= Btn_SaveMark_Click;
                btn_SaveMark.Click += Btn_SaveMark_Click;
            }
            // 重置按钮
            if (btn_ResetMark != null)
            {
                btn_ResetMark.Click -= Btn_ResetMark_Click;
                btn_ResetMark.Click += Btn_ResetMark_Click;
            }
            #endregion

            #region 异常干预处理 按钮事件（仅绑定1次，无重复）
            // 保存干预方案按钮
            if (btn_SaveIntervention != null)
            {
                btn_SaveIntervention.Click -= Btn_SaveIntervention_Click;
                btn_SaveIntervention.Click += Btn_SaveIntervention_Click;
            }
            // 重置按钮
            if (btn_ResetIntervention != null)
            {
                btn_ResetIntervention.Click -= Btn_ResetIntervention_Click;
                btn_ResetIntervention.Click += Btn_ResetIntervention_Click;
            }
            #endregion

            #region 复查提醒管理 按钮事件（仅绑定1次，无重复）
            // 设置提醒按钮
            if (btn_SetReview != null)
            {
                btn_SetReview.Click -= Btn_SetReview_Click;
                btn_SetReview.Click += Btn_SetReview_Click;
            }
            // 查询提醒按钮
            if (btn_QueryReview != null)
            {
                btn_QueryReview.Click -= Btn_QueryReview_Click;
                btn_QueryReview.Click += Btn_QueryReview_Click;
            }
            // 重置按钮
            if (btn_ResetReview != null)
            {
                btn_ResetReview.Click -= Btn_ResetReview_Click;
                btn_ResetReview.Click += Btn_ResetReview_Click;
            }
            #endregion
        }

        #region 【拆分独立方法】按钮点击事件处理（避免Lambda闭包问题，更易维护）
        /// <summary>
        /// 异常预警页-选择患者按钮点击事件
        /// </summary>
        private void Btn_SelectWarnPatient_Click(object sender, EventArgs e)
        {
            using (FrmAbPatientSelector frmWarn = new FrmAbPatientSelector())
            {
                frmWarn.AllowSelectAll = true;
                frmWarn.OnlyLoadPatientWithAbnormal = true;
                if (frmWarn.ShowDialog() == DialogResult.OK)
                {
                    var txt_WarnPatient = this.Controls.Find("txt_WarnPatient", true).FirstOrDefault() as TextBox;
                    if (txt_WarnPatient == null) return;

                    if (frmWarn.IsSelectAll)
                    {
                        _selectedWarnPatientId = 0;
                        txt_WarnPatient.Text = "全部患者";
                        dgv_WarnList.DataSource = null;
                    }
                    else
                    {
                        _selectedWarnPatientId = frmWarn.SelectedPatient.UserId;
                        txt_WarnPatient.Text = frmWarn.SelectedPatient.DisplayText;
                        AutoLoadPatientAbnormalData(frmWarn.SelectedPatient.UserId);
                    }
                }
            }
        }

        /// <summary>
        /// 异常标注页-选择患者按钮点击事件
        /// </summary>
        private void Btn_SelectMarkPatient_Click(object sender, EventArgs e)
        {
            using (FrmAbPatientSelector frmMark = new FrmAbPatientSelector())
            {
                frmMark.AllowSelectAll = false;
                frmMark.OnlyLoadPatientWithAbnormal = true;
                if (frmMark.ShowDialog() == DialogResult.OK)
                {
                    var txt_MarkPatient = this.Controls.Find("txt_MarkPatient", true).FirstOrDefault() as TextBox;
                    if (txt_MarkPatient == null) return;

                    _selectedMarkPatientId = frmMark.SelectedPatient.UserId;
                    txt_MarkPatient.Text = frmMark.SelectedPatient.DisplayText;
                    BindWaitMarkAbnormalData(_selectedMarkPatientId);
                }
            }
        }

        /// <summary>
        /// 干预处理页-选择患者按钮点击事件
        /// </summary>
        private void Btn_SelectInterventionPatient_Click(object sender, EventArgs e)
        {
            using (FrmAbPatientSelector frmInter = new FrmAbPatientSelector())
            {
                frmInter.AllowSelectAll = false;
                frmInter.OnlyLoadPatientWithAbnormal = true;
                if (frmInter.ShowDialog() == DialogResult.OK)
                {
                    var txt_InterventionPatient = this.Controls.Find("txt_InterventionPatient", true).FirstOrDefault() as TextBox;
                    if (txt_InterventionPatient == null) return;

                    _selectedInterventionPatientId = frmInter.SelectedPatient.UserId;
                    txt_InterventionPatient.Text = frmInter.SelectedPatient.DisplayText;
                }
            }
        }

        /// <summary>
        /// 复查提醒页-选择患者按钮点击事件
        /// </summary>
        private void Btn_SelectReviewPatient_Click(object sender, EventArgs e)
        {
            using (FrmAbPatientSelector frmReview = new FrmAbPatientSelector())
            {
                frmReview.AllowSelectAll = true;
                frmReview.OnlyLoadPatientWithAbnormal = false;
                if (frmReview.ShowDialog() == DialogResult.OK)
                {
                    var txt_ReviewPatient = this.Controls.Find("txt_ReviewPatient", true).FirstOrDefault() as TextBox;
                    if (txt_ReviewPatient == null) return;

                    if (frmReview.IsSelectAll)
                    {
                        _selectedReviewPatientId = 0;
                        txt_ReviewPatient.Text = "全部患者";
                    }
                    else
                    {
                        _selectedReviewPatientId = frmReview.SelectedPatient.UserId;
                        txt_ReviewPatient.Text = frmReview.SelectedPatient.DisplayText;
                    }
                }
            }
        }

        /// <summary>
        /// 异常预警页-查询按钮点击事件
        /// </summary>
        private void Btn_QueryWarn_Click(object sender, EventArgs e)
        {
            LoadAbnormalWarnData();
        }

        /// <summary>
        /// 异常预警页-重置按钮点击事件
        /// </summary>
        private void Btn_ResetWarn_Click(object sender, EventArgs e)
        {
            _selectedWarnPatientId = 0;
            var txt_WarnPatient = this.Controls.Find("txt_WarnPatient", true).FirstOrDefault() as TextBox;
            if (txt_WarnPatient != null)
                txt_WarnPatient.Text = "全部患者";
            cbo_WarnAbnormalType.SelectedIndex = 0;
            dtp_WarnStart.Value = DateTime.Now.AddMonths(-1);
            dtp_WarnEnd.Value = DateTime.Now;
            dgv_WarnList.DataSource = null;
        }

        /// <summary>
        /// 异常标注页-重置按钮点击事件
        /// </summary>
        private void Btn_ResetMark_Click(object sender, EventArgs e)
        {
            _selectedMarkPatientId = 0;
            var txt_MarkPatient = this.Controls.Find("txt_MarkPatient", true).FirstOrDefault() as TextBox;
            if (txt_MarkPatient != null)
                txt_MarkPatient.Text = "请选择患者";
            cbo_MarkAbnormalData.Items.Clear();
            txt_AbnormalReason.Clear();
            txt_ClinicalSuggestion.Clear();
        }

        /// <summary>
        /// 干预处理页-重置按钮点击事件
        /// </summary>
        private void Btn_ResetIntervention_Click(object sender, EventArgs e)
        {
            _selectedInterventionPatientId = 0;
            var txt_InterventionPatient = this.Controls.Find("txt_InterventionPatient", true).FirstOrDefault() as TextBox;
            if (txt_InterventionPatient != null)
                txt_InterventionPatient.Text = "请选择患者";
            cbo_InterventionType.SelectedIndex = 0;
            txt_InterventionContent.Clear();
            txt_InterventionEffect.Clear();
        }

        /// <summary>
        /// 复查提醒页-查询按钮点击事件
        /// </summary>
        private void Btn_QueryReview_Click(object sender, EventArgs e)
        {
            LoadReviewRemindData();
        }

        /// <summary>
        /// 复查提醒页-重置按钮点击事件
        /// </summary>
        private void Btn_ResetReview_Click(object sender, EventArgs e)
        {
            _selectedReviewPatientId = 0;
            var txt_ReviewPatient = this.Controls.Find("txt_ReviewPatient", true).FirstOrDefault() as TextBox;
            if (txt_ReviewPatient != null)
                txt_ReviewPatient.Text = "全部患者";
            dtp_ReviewDate.Value = DateTime.Now.AddDays(7);
            dtp_ReviewStart.Value = DateTime.Now.AddMonths(-3);
            dtp_ReviewEnd.Value = DateTime.Now;
            dgv_ReviewList.DataSource = null;
        }
        #endregion

        #region 核心业务方法
        /// <summary>
        /// 加载异常预警数据列表
        /// </summary>
        private void LoadAbnormalWarnData()
        {
            // 给null显式指定类型为int?
            int? userId = _selectedWarnPatientId == 0 ? (int?)null : _selectedWarnPatientId;
            if (userId == 0) userId = null;
            string abnormalType = cbo_WarnAbnormalType.SelectedItem?.ToString();
            DateTime startDate = dtp_WarnStart.Value;
            DateTime endDate = dtp_WarnEnd.Value;

            var result = _bllAbnormal.GetAbnormalWarnList(userId, abnormalType, startDate, endDate);
            if (!result.Success)
            {
                MessageBox.Show(result.Msg, "查询失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            dgv_WarnList.DataSource = result.Data;
        }

        /// <summary>
        /// 绑定待标注的异常数据下拉框
        /// </summary>
        private void BindWaitMarkAbnormalData(int userId)
        {
            if (userId <= 0)
            {
                cbo_MarkAbnormalData.Items.Clear();
                cbo_MarkAbnormalData.Text = "请先选择患者";
                return;
            }
            cbo_MarkAbnormalData.Items.Clear();
            var abnormalList = _bllAbnormal.GetWaitMarkAbnormalByUserId(userId);
            if (abnormalList == null || !abnormalList.Any())
            {
                cbo_MarkAbnormalData.Text = "该患者暂无待标注的异常数据";
                return;
            }

            cbo_MarkAbnormalData.DisplayMember = "DisplayText";
            cbo_MarkAbnormalData.ValueMember = "abnormal_id";
            cbo_MarkAbnormalData.DataSource = abnormalList.Select(a => new
            {
                a.abnormal_id,
                DisplayText = $"{a.create_time:yyyy-MM-dd} {a.abnormal_type} - {a.data_type}"
            }).ToList();
            cbo_MarkAbnormalData.SelectedIndex = 0;
        }

        /// <summary>
        /// 保存异常原因标注
        /// </summary>
        private void Btn_SaveMark_Click(object sender, EventArgs e)
        {
            // 1. 校验异常数据选择（原有逻辑完全保留，cbo_MarkAbnormalData未被替换）
            if (cbo_MarkAbnormalData.SelectedValue == null || !(cbo_MarkAbnormalData.SelectedValue is int abnormalId) || abnormalId <= 0)
            {
                MessageBox.Show("请选择有效异常数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. 构建标注模型（原有逻辑完全保留）
            Abnormal model = new Abnormal
            {
                abnormal_id = abnormalId,
                abnormal_reason = txt_AbnormalReason.Text.Trim(),
                suggestion = txt_ClinicalSuggestion.Text.Trim(),
                mark_by = GlobalConfig.CurrentDoctorID
            };

            // 3. 调用业务层保存（原有逻辑完全保留）
            var result = _bllAbnormal.SaveAbnormalMark(model);
            if (result.Success)
            {
                MessageBox.Show(result.Msg, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // ========== 核心修复：替换废弃控件为全局选中ID ==========
                // 原废弃代码（删除）：
                // if (cbo_MarkPatient.SelectedValue is int userId && userId > 0)
                // {
                //     BindWaitMarkAbnormalData(userId);
                // }
                // 修复后代码：
                if (_selectedMarkPatientId > 0)
                {
                    BindWaitMarkAbnormalData(_selectedMarkPatientId);
                }
                // ======================================================

                // 4. 原有清空/刷新逻辑完全保留
                txt_AbnormalReason.Clear();
                txt_ClinicalSuggestion.Clear();
                LoadAbnormalWarnData();
            }
            else
            {
                MessageBox.Show(result.Msg, "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 保存异常干预方案
        /// </summary>
        private void Btn_SaveIntervention_Click(object sender, EventArgs e)
        {
            if (cbo_MarkAbnormalData.SelectedValue == null || !(cbo_MarkAbnormalData.SelectedValue is int abnormalId) || abnormalId <= 0)
            {
                MessageBox.Show("请先在异常原因标注页选择对应异常数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 仅修改这1行：从_selectedInterventionPatientId取值，其余完全保留
            InterventionPlan plan = new InterventionPlan
            {
                user_id = _selectedInterventionPatientId,
                related_abnormal_id = abnormalId,
                plan_type = cbo_InterventionType.SelectedItem.ToString(),
                plan_content = txt_InterventionContent.Text.Trim(),
                expected_effect = txt_InterventionEffect.Text.Trim(),
                start_time = DateTime.Now.Date,
                end_time = DateTime.Now.Date.AddMonths(1),
                review_time = DateTime.Now.Date.AddDays(15),
                create_by = GlobalConfig.CurrentDoctorID
            };
            var result = _bllAbnormal.SaveInterventionPlan(plan, abnormalId);
            if (result.Success)
            {
                MessageBox.Show(result.Msg, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txt_InterventionContent.Clear();
                txt_InterventionEffect.Clear();
                LoadAbnormalWarnData();
            }
            else
            {
                MessageBox.Show(result.Msg, "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 设置复查提醒
        /// </summary>
        private void Btn_SetReview_Click(object sender, EventArgs e)
        {
            // 仅修改这1行：从_selectedReviewPatientId取值，其余完全保留
            if (_selectedReviewPatientId <= 0)
            {
                MessageBox.Show("请选择有效患者", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            FollowUp model = new FollowUp
            {
                user_id = _selectedReviewPatientId,
                follow_up_time = dtp_ReviewDate.Value,
                follow_up_content = $"血糖异常复查提醒，复查日期：{dtp_ReviewDate.Value:yyyy-MM-dd}",
                follow_up_by = GlobalConfig.CurrentDoctorID
            };
            var result = _bllAbnormal.AddReviewRemind(model);
            if (result.Success)
            {
                MessageBox.Show(result.Msg, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadReviewRemindData();
            }
            else
            {
                MessageBox.Show(result.Msg, "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #region 【追加新方法】确认选择患者后，自动加载异常数据到列表
        /// <summary>
        /// 自动加载指定患者的异常预警数据
        /// </summary>
        private void AutoLoadPatientAbnormalData(int patientId)
        {
            // 复用页面已选的时间范围，保证查询条件一致
            DateTime startDate = dtp_WarnStart.Value;
            DateTime endDate = dtp_WarnEnd.Value;

            // 调用BLL层方法获取数据
            var result = _bllAbnormal.GetAbnormalWarnByPatientId(patientId, startDate, endDate);
            if (!result.Success)
            {
                MessageBox.Show(result.Msg, "加载失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 绑定数据到列表
            dgv_WarnList.DataSource = result.Data;

            // 可选：自动切换到异常数据预警标签页，提升体验
            tabMain.SelectedTab = tab_Warn;
        }
        #endregion
        /// <summary>
        /// 加载复查提醒列表
        /// </summary>
        private void LoadReviewRemindData()
        {
            // 给null显式指定类型为int?
            int? userId = _selectedReviewPatientId == 0 ? (int?)null : _selectedReviewPatientId;
            if (userId == 0) userId = null;
            DateTime startDate = dtp_ReviewStart.Value;
            DateTime endDate = dtp_ReviewEnd.Value;

            var result = _bllAbnormal.GetReviewRemindList(userId, startDate, endDate);
            if (!result.Success)
            {
                MessageBox.Show(result.Msg, "查询失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            dgv_ReviewList.DataSource = result.Data;
        }
        #endregion
        #endregion
    }
}