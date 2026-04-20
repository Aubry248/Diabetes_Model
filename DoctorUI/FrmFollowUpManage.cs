using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using BLL;
using Model;

namespace DoctorUI
{
    public partial class FrmFollowUpManage : Form
    {
        #region ========== 原有全局统一布局参数（完全保留）==========
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

        #region 原有核心控件声明（完全保留）
        private Panel pnlMainContainer;
        private Panel pnlContentWrapper;
        private TabControl tabMain;
        private TabPage tab_FollowUpPlan, tab_FollowUpRecord, tab_FollowUpHistory;
        // 随访计划制定
        private GroupBox grp_PlanPatient, grp_PlanContent;
        private ComboBox cbo_PlanPatient, cbo_PlanType, cbo_FollowUpWay;
        private DateTimePicker dtp_FollowUpDate;
        private TextBox txt_PlanContent;
        private Button btn_SavePlan, btn_ResetPlan;
        // 随访记录录入与提醒
        private GroupBox grp_RecordPatient, grp_RecordContent, grp_Remind;
        private ComboBox cbo_RecordPatient, cbo_RecordPlan;
        private TextBox txt_FollowUpResult, txt_NextFollowUpNote;
        private DateTimePicker dtp_NextFollowUpDate;
        private Button btn_SubmitRecord, btn_ResetRecord;
        // 随访历史查询
        private GroupBox grp_HistoryQuery, grp_HistoryList;
        private ComboBox cbo_HistoryPatient, cbo_HistoryType;
        private DateTimePicker dtp_HistoryStart, dtp_HistoryEnd;
        private DataGridView dgv_FollowUpHistory;
        private Button btn_QueryHistory, btn_ResetHistory;
        #endregion

        #region ========== 新增：全局业务变量 ==========
        /// <summary>
        /// 当前登录医生ID（窗体实例化时传入）
        /// </summary>
        public int CurrentLoginDoctorId { get; set; }

        /// <summary>
        /// 随访业务逻辑对象
        /// </summary>
        private readonly B_FollowUp _bllFollowUp = new B_FollowUp();

        /// <summary>
        /// 全局患者列表缓存
        /// </summary>
        private List<PatientSimpleInfo> _allPatientList = new List<PatientSimpleInfo>();
        #endregion

        #region 原有构造函数（完全保留，仅追加窗体加载事件）
        public FrmFollowUpManage()
        {
            // 窗体基础配置（与其他页面统一）
            this.Text = "随访管理";
            this.Size = new Size(1300, 800);
            this.MinimumSize = new Size(_globalContentMinWidth + 40, _globalContentMinHeight + 100);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("微软雅黑", 9.5F);
            this.Dock = DockStyle.Fill;
            // 全局主容器初始化
            InitMainContainer();
            // 动态创建控件
            InitializeDynamicControls();
            // 初始化下拉数据
            InitControlData();
            // 绑定事件
            BindAllEvents();

            // 新增：窗体加载时绑定数据
            this.Load += FrmFollowUpManage_Load;
        }
        #endregion

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


        #region 原有动态创建所有控件（完全保留，无修改）
        private void InitializeDynamicControls()
        {
            // 主标签控件
            tabMain = new TabControl();
            tabMain.Dock = DockStyle.Fill;
            tabMain.Font = new Font("微软雅黑", 10F);
            tabMain.Padding = new Point(15, 8);
            tab_FollowUpPlan = new TabPage("随访计划制定") { BackColor = Color.White };
            tab_FollowUpRecord = new TabPage("随访记录录入") { BackColor = Color.White };
            tab_FollowUpHistory = new TabPage("随访历史查询") { BackColor = Color.White };
            tabMain.TabPages.AddRange(new TabPage[] { tab_FollowUpPlan, tab_FollowUpRecord, tab_FollowUpHistory });
            pnlContentWrapper.Controls.Add(tabMain);
            // 初始化三个功能页面
            InitFollowUpPlanPage();
            InitFollowUpRecordPage();
            InitFollowUpHistoryPage();
        }

        // 1. 随访计划制定页面（完全保留，无修改）
        private void InitFollowUpPlanPage()
        {
            // 患者与基础信息分组
            grp_PlanPatient = new GroupBox { Text = "患者与计划基础信息", Dock = DockStyle.Top, Height = 160, Padding = _globalGroupBoxPadding };
            tab_FollowUpPlan.Controls.Add(grp_PlanPatient);
            TableLayoutPanel tlp_Plan = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Plan.RowCount = 2;
            tlp_Plan.ColumnCount = 2;
            tlp_Plan.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_Plan.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_Plan.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            tlp_Plan.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_PlanPatient.Controls.Add(tlp_Plan);
            int row = 0;
            CreateEditItem<ComboBox>(tlp_Plan, out _, out cbo_PlanPatient, "选择患者：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Plan, out _, out cbo_PlanType, "计划类型：", ref row, false);
            CreateEditItem<DateTimePicker>(tlp_Plan, out _, out dtp_FollowUpDate, "随访日期：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Plan, out _, out cbo_FollowUpWay, "随访方式：", ref row, false);
            // 计划内容分组
            grp_PlanContent = new GroupBox { Text = "随访计划内容", Dock = DockStyle.Top, Height = 260, Padding = _globalGroupBoxPadding };
            tab_FollowUpPlan.Controls.Add(grp_PlanContent);
            txt_PlanContent = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = this.Font,
                Margin = _globalControlMargin
            };
            grp_PlanContent.Controls.Add(txt_PlanContent);
            // 按钮区（独立行，不遮挡）
            Panel pnl_PlanBtn = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(15) };
            tab_FollowUpPlan.Controls.Add(pnl_PlanBtn);
            FlowLayoutPanel flp_PlanBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_SavePlan = CreateBtn("保存计划", Color.FromArgb(0, 122, 204));
            btn_ResetPlan = CreateBtn("重置", Color.Gray);
            flp_PlanBtn.Controls.AddRange(new Control[] { btn_SavePlan, btn_ResetPlan });
            pnl_PlanBtn.Controls.Add(flp_PlanBtn);
        }

        // 2. 随访记录录入与提醒页面（完全保留，无修改）
        private void InitFollowUpRecordPage()
        {
            // 患者与计划选择分组
            grp_RecordPatient = new GroupBox { Text = "患者与随访计划", Dock = DockStyle.Top, Height = 120, Padding = _globalGroupBoxPadding };
            tab_FollowUpRecord.Controls.Add(grp_RecordPatient);
            TableLayoutPanel tlp_Record = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Record.RowCount = 1;
            tlp_Record.ColumnCount = 2;
            tlp_Record.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_Record.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_Record.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_RecordPatient.Controls.Add(tlp_Record);
            int row = 0;
            CreateEditItem<ComboBox>(tlp_Record, out _, out cbo_RecordPatient, "选择患者：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Record, out _, out cbo_RecordPlan, "选择随访计划：", ref row, false);
            // 随访结果录入分组
            grp_RecordContent = new GroupBox { Text = "随访结果记录", Dock = DockStyle.Top, Height = 200, Padding = _globalGroupBoxPadding };
            tab_FollowUpRecord.Controls.Add(grp_RecordContent);
            txt_FollowUpResult = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = this.Font,
                Margin = _globalControlMargin
            };
            grp_RecordContent.Controls.Add(txt_FollowUpResult);
            // 下次随访提醒分组
            grp_Remind = new GroupBox { Text = "下次随访提醒设置", Dock = DockStyle.Top, Height = 120, Padding = _globalGroupBoxPadding };
            tab_FollowUpRecord.Controls.Add(grp_Remind);
            TableLayoutPanel tlp_Remind = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Remind.RowCount = 1;
            tlp_Remind.ColumnCount = 2;
            tlp_Remind.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_Remind.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_Remind.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_Remind.Controls.Add(tlp_Remind);
            row = 0;
            CreateEditItem<DateTimePicker>(tlp_Remind, out _, out dtp_NextFollowUpDate, "下次随访日期：", ref row, false);
            CreateEditItem<TextBox>(tlp_Remind, out _, out txt_NextFollowUpNote, "提醒备注：", ref row, false);
            // 提交按钮区
            Panel pnl_RecordBtn = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(15) };
            tab_FollowUpRecord.Controls.Add(pnl_RecordBtn);
            FlowLayoutPanel flp_RecordBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_SubmitRecord = CreateBtn("提交随访记录", Color.FromArgb(0, 122, 204));
            btn_ResetRecord = CreateBtn("重置", Color.Gray);
            flp_RecordBtn.Controls.AddRange(new Control[] { btn_SubmitRecord, btn_ResetRecord });
            pnl_RecordBtn.Controls.Add(flp_RecordBtn);
        }

        // 3. 随访历史查询页面（完全保留，仅追加DataGridView列）
        private void InitFollowUpHistoryPage()
        {
            // 查询条件分组
            grp_HistoryQuery = new GroupBox { Text = "查询条件", Dock = DockStyle.Top, Height = 160, Padding = _globalGroupBoxPadding };
            tab_FollowUpHistory.Controls.Add(grp_HistoryQuery);
            TableLayoutPanel tlp_History = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_History.RowCount = 2;
            tlp_History.ColumnCount = 3;
            tlp_History.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlp_History.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlp_History.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            tlp_History.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            tlp_History.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_HistoryQuery.Controls.Add(tlp_History);
            int row = 0;
            CreateEditItem<ComboBox>(tlp_History, out _, out cbo_HistoryPatient, "选择患者：", ref row, false);
            CreateEditItem<ComboBox>(tlp_History, out _, out cbo_HistoryType, "随访类型：", ref row, false);
            // 时间范围控件
            Panel pnl_DateRange = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_Date = new Label { Text = "随访时间：", Size = new Size(80, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(0, 0) };
            dtp_HistoryStart = new DateTimePicker { Location = new Point(80, 0), Size = new Size(140, _globalControlHeight), Format = DateTimePickerFormat.Short };
            dtp_HistoryEnd = new DateTimePicker { Location = new Point(230, 0), Size = new Size(140, _globalControlHeight), Format = DateTimePickerFormat.Short };
            pnl_DateRange.Controls.AddRange(new Control[] { lbl_Date, dtp_HistoryStart, dtp_HistoryEnd });
            tlp_History.Controls.Add(pnl_DateRange, 2, 0);
            // 查询按钮区
            Panel pnl_HistoryBtn = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            FlowLayoutPanel flp_HistoryBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            btn_QueryHistory = CreateBtn("查询", Color.FromArgb(0, 122, 204));
            btn_ResetHistory = CreateBtn("重置", Color.Gray);
            flp_HistoryBtn.Controls.AddRange(new Control[] { btn_ResetHistory, btn_QueryHistory });
            pnl_HistoryBtn.Controls.Add(flp_HistoryBtn);
            tlp_History.Controls.Add(pnl_HistoryBtn, 2, 1);
            // 随访历史列表分组
            grp_HistoryList = new GroupBox { Text = "随访历史记录列表", Dock = DockStyle.Fill, Padding = _globalGroupBoxPadding };
            tab_FollowUpHistory.Controls.Add(grp_HistoryList);
            dgv_FollowUpHistory = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };
            grp_HistoryList.Controls.Add(dgv_FollowUpHistory);
            // 新增：完善列表列（原有基础上追加，适配业务）
            dgv_FollowUpHistory.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { HeaderText = "随访日期", DataPropertyName = "FollowUpDate", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd" } },
                new DataGridViewTextBoxColumn { HeaderText = "患者姓名", DataPropertyName = "PatientName", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "随访方式", DataPropertyName = "FollowUpWay", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "随访结果", DataPropertyName = "FollowUpResult", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "随访状态", DataPropertyName = "StatusDesc", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "随访医生", DataPropertyName = "FollowUpDoctor", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
            });
        }
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
                if (text == "提醒备注：") t.Multiline = true; // 提醒备注允许多行
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

        #region ========== 新增：窗体加载数据绑定方法 ==========
        /// <summary>
        /// 窗体加载时初始化数据
        /// </summary>
        private void FrmFollowUpManage_Load(object sender, EventArgs e)
        {
            // 直接从全局配置获取医生ID，无需主窗体传参
            this.CurrentLoginDoctorId = GlobalConfig.CurrentDoctorID;

            // 兜底校验：如果全局配置仍无值，提示重新登录
            if (this.CurrentLoginDoctorId <= 0)
            {
                MessageBox.Show("登录信息失效，请重新登录系统！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }
            // 绑定患者列表
            BindPatientList();
            // 初始化默认值
            InitDefaultValue();
        }

        /// <summary>
        /// 绑定所有患者下拉框
        /// </summary>
        private void BindPatientList()
        {
            _allPatientList = _bllFollowUp.GetAllValidPatient();
            if (_allPatientList == null || !_allPatientList.Any())
            {
                MessageBox.Show("未查询到有效患者数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 统一绑定三个患者下拉框
            var bindSource = new BindingSource(_allPatientList, null);
            cbo_PlanPatient.DataSource = bindSource;
            cbo_PlanPatient.DisplayMember = "UserName";
            cbo_PlanPatient.ValueMember = "UserId";

            cbo_RecordPatient.DataSource = new BindingSource(_allPatientList, null);
            cbo_RecordPatient.DisplayMember = "UserName";
            cbo_RecordPatient.ValueMember = "UserId";

            // 历史查询下拉框追加"全部"选项
            var historyPatientList = new List<PatientSimpleInfo>
            {
                new PatientSimpleInfo { UserId = 0, UserName = "全部" }
            };
            historyPatientList.AddRange(_allPatientList);
            cbo_HistoryPatient.DataSource = historyPatientList;
            cbo_HistoryPatient.DisplayMember = "UserName";
            cbo_HistoryPatient.ValueMember = "UserId";
        }

        /// <summary>
        /// 初始化控件默认值
        /// </summary>
        private void InitDefaultValue()
        {
            // 计划类型下拉框
            cbo_PlanType.Items.Clear();
            cbo_PlanType.Items.AddRange(new string[] { "定期随访", "临时随访", "紧急随访" });
            cbo_PlanType.SelectedIndex = 0;

            // 随访方式下拉框
            cbo_FollowUpWay.Items.Clear();
            cbo_FollowUpWay.Items.AddRange(new string[] { "门诊", "电话", "线上", "上门" });
            cbo_FollowUpWay.SelectedIndex = 0;

            // 随访类型下拉框（历史查询用）
            cbo_HistoryType.Items.Clear();
            cbo_HistoryType.Items.AddRange(new string[] { "全部", "定期随访", "临时随访", "紧急随访" });
            cbo_HistoryType.SelectedIndex = 0;

            // 日期默认值
            dtp_FollowUpDate.Value = DateTime.Now.AddDays(7);
            dtp_NextFollowUpDate.Value = DateTime.Now.AddDays(30);
            dtp_HistoryStart.Value = DateTime.Now.AddMonths(-3);
            dtp_HistoryEnd.Value = DateTime.Now;
        }

        /// <summary>
        /// 根据患者ID绑定待随访计划
        /// </summary>
        private void BindWaitFollowUpPlan(int userId)
        {
            cbo_RecordPlan.Items.Clear();
            var planList = _bllFollowUp.GetWaitFollowUpPlanByUserId(userId);
            if (planList == null || !planList.Any())
            {
                cbo_RecordPlan.Text = "该患者暂无待随访计划";
                return;
            }

            // 绑定下拉框，显示文本，值为随访ID
            cbo_RecordPlan.DisplayMember = "DisplayText";
            cbo_RecordPlan.ValueMember = "follow_up_id";
            cbo_RecordPlan.DataSource = planList.Select(p => new
            {
                p.follow_up_id,
                DisplayText = $"{p.follow_up_time:yyyy-MM-dd} {cbo_FollowUpWay.Text}"
            }).ToList();
            cbo_RecordPlan.SelectedIndex = 0;
        }

        /// <summary>
        /// 加载随访历史数据到列表
        /// </summary>
        private void LoadFollowUpHistoryData()
        {
            // 获取查询条件
            int? userId = cbo_HistoryPatient.SelectedValue as int?;
            if (userId == 0) userId = null;
            string followUpType = cbo_HistoryType.SelectedItem?.ToString();
            DateTime startDate = dtp_HistoryStart.Value;
            DateTime endDate = dtp_HistoryEnd.Value;

            // 调用业务层查询
            var result = _bllFollowUp.GetFollowUpHistoryList(userId, followUpType, startDate, endDate);
            if (!result.Success)
            {
                MessageBox.Show(result.Msg, "查询失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 绑定数据到列表
            dgv_FollowUpHistory.DataSource = result.Data;
        }
        #endregion

        #region 原有下拉数据初始化（修改为真实业务绑定，原有模拟代码注释）
        private void InitControlData()
        {
            // 原有模拟代码注释，改为窗体加载时真实绑定
            //// 模拟患者数据（实际项目对接后端获取）
            //string[] patients = new string[] { "张三", "李四", "王五" };
            //cbo_PlanPatient.Items.AddRange(patients);
            //cbo_RecordPatient.Items.AddRange(patients);
            //cbo_HistoryPatient.Items.AddRange(patients);
            //// 计划类型下拉框
            //cbo_PlanType.Items.AddRange(new string[] { "定期随访", "临时随访", "紧急随访" });
            //// 随访方式下拉框
            //cbo_FollowUpWay.Items.AddRange(new string[] { "门诊", "电话", "微信", "上门" });
            //// 随访计划下拉框（模拟数据）
            //cbo_RecordPlan.Items.AddRange(new string[] { "2026-03-15 定期随访", "2026-03-20 临时随访" });
            //// 随访类型下拉框（历史查询用）
            //cbo_HistoryType.Items.AddRange(new string[] { "全部", "定期随访", "临时随访", "紧急随访" });
            //// 默认选中项
            //cbo_PlanPatient.SelectedIndex = 0;
            //cbo_RecordPatient.SelectedIndex = 0;
            //cbo_HistoryPatient.SelectedIndex = 0;
            //cbo_PlanType.SelectedIndex = 0;
            //cbo_FollowUpWay.SelectedIndex = 0;
            //cbo_RecordPlan.SelectedIndex = 0;
            //cbo_HistoryType.SelectedIndex = 0;
            //dtp_FollowUpDate.Value = DateTime.Now.AddDays(7);
            //dtp_NextFollowUpDate.Value = DateTime.Now.AddDays(30);
            //dtp_HistoryStart.Value = DateTime.Now.AddMonths(-3);
            //dtp_HistoryEnd.Value = DateTime.Now;
        }
        #endregion

        #region 原有事件绑定（完全重构为真实业务逻辑，原有模拟代码保留注释）
        private void BindAllEvents()
        {
            #region 原有模拟事件代码注释
            //// 随访计划制定：保存/重置
            //btn_SavePlan.Click += (s, e) => MessageBox.Show("随访计划保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //btn_ResetPlan.Click += (s, e) => { cbo_PlanPatient.SelectedIndex = 0; cbo_PlanType.SelectedIndex = 0; dtp_FollowUpDate.Value = DateTime.Now.AddDays(7); cbo_FollowUpWay.SelectedIndex = 0; txt_PlanContent.Clear(); };
            //// 随访记录录入：提交/重置
            //btn_SubmitRecord.Click += (s, e) => MessageBox.Show("随访记录提交成功，下次随访提醒已设置！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //btn_ResetRecord.Click += (s, e) => { cbo_RecordPatient.SelectedIndex = 0; cbo_RecordPlan.SelectedIndex = 0; txt_FollowUpResult.Clear(); dtp_NextFollowUpDate.Value = DateTime.Now.AddDays(30); txt_NextFollowUpNote.Clear(); };
            //// 随访历史查询：查询/重置
            //btn_QueryHistory.Click += (s, e) => MessageBox.Show("随访历史查询完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //btn_ResetHistory.Click += (s, e) => { cbo_HistoryPatient.SelectedIndex = 0; cbo_HistoryType.SelectedIndex = 0; dtp_HistoryStart.Value = DateTime.Now.AddMonths(-3); dtp_HistoryEnd.Value = DateTime.Now; };
            #endregion

            #region 新增：真实业务事件绑定
            // ========== 随访计划制定事件 ==========
            // 保存计划按钮
            btn_SavePlan.Click += Btn_SavePlan_Click;
            // 重置计划按钮
            btn_ResetPlan.Click += (s, e) =>
            {
                cbo_PlanPatient.SelectedIndex = 0;
                cbo_PlanType.SelectedIndex = 0;
                dtp_FollowUpDate.Value = DateTime.Now.AddDays(7);
                cbo_FollowUpWay.SelectedIndex = 0;
                txt_PlanContent.Clear();
            };

            // ========== 随访记录录入事件 ==========
            // 患者切换时加载待随访计划
            cbo_RecordPatient.SelectedIndexChanged += (s, e) =>
            {
                if (cbo_RecordPatient.SelectedValue is int userId && userId > 0)
                {
                    BindWaitFollowUpPlan(userId);
                }
            };
            // 提交随访记录按钮
            btn_SubmitRecord.Click += Btn_SubmitRecord_Click;
            // 重置记录按钮
            btn_ResetRecord.Click += (s, e) =>
            {
                cbo_RecordPatient.SelectedIndex = 0;
                txt_FollowUpResult.Clear();
                dtp_NextFollowUpDate.Value = DateTime.Now.AddDays(30);
                txt_NextFollowUpNote.Clear();
            };

            // ========== 随访历史查询事件 ==========
            // 查询按钮
            btn_QueryHistory.Click += (s, e) => LoadFollowUpHistoryData();
            // 重置按钮
            btn_ResetHistory.Click += (s, e) =>
            {
                cbo_HistoryPatient.SelectedIndex = 0;
                cbo_HistoryType.SelectedIndex = 0;
                dtp_HistoryStart.Value = DateTime.Now.AddMonths(-3);
                dtp_HistoryEnd.Value = DateTime.Now;
                dgv_FollowUpHistory.DataSource = null;
            };
            #endregion
        }

        #region 新增：按钮点击业务方法
        /// <summary>
        /// 保存随访计划
        /// </summary>
        private void Btn_SavePlan_Click(object sender, EventArgs e)
        {
            // 校验登录医生ID
            if (CurrentLoginDoctorId <= 0)
            {
                MessageBox.Show("当前登录信息异常，请重新登录系统", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 构建实体
            FollowUp model = new FollowUp
            {
                user_id = (int)cbo_PlanPatient.SelectedValue,
                follow_up_time = dtp_FollowUpDate.Value,
                follow_up_way = cbo_FollowUpWay.SelectedItem.ToString(),
                follow_up_content = $"【{cbo_PlanType.SelectedItem}】{txt_PlanContent.Text.Trim()}",
                follow_up_by = CurrentLoginDoctorId
            };

            // 调用业务层
            var result = _bllFollowUp.AddFollowUpPlan(model);
            if (result.Success)
            {
                MessageBox.Show(result.Msg, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txt_PlanContent.Clear();
                // 刷新随访计划下拉框
                if (cbo_RecordPatient.SelectedValue is int userId && userId == model.user_id)
                {
                    BindWaitFollowUpPlan(userId);
                }
            }
            else
            {
                MessageBox.Show(result.Msg, "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 提交随访记录
        /// </summary>
        private void Btn_SubmitRecord_Click(object sender, EventArgs e)
        {
            if (cbo_RecordPlan.SelectedValue == null || !(cbo_RecordPlan.SelectedValue is int followUpId) || followUpId <= 0)
            {
                MessageBox.Show("请选择有效随访计划", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 构建实体
            FollowUp model = new FollowUp
            {
                follow_up_id = followUpId,
                follow_up_result = txt_FollowUpResult.Text.Trim(),
                next_follow_up_time = dtp_NextFollowUpDate.Value
            };
            // 追加备注到结果
            if (!string.IsNullOrEmpty(txt_NextFollowUpNote.Text.Trim()))
            {
                model.follow_up_result += $"\r\n【下次随访提醒备注】：{txt_NextFollowUpNote.Text.Trim()}";
            }

            // 调用业务层
            var result = _bllFollowUp.SubmitFollowUpRecord(model);
            if (result.Success)
            {
                MessageBox.Show(result.Msg, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // 重置表单
                txt_FollowUpResult.Clear();
                txt_NextFollowUpNote.Clear();
                // 刷新计划下拉框
                if (cbo_RecordPatient.SelectedValue is int userId && userId > 0)
                {
                    BindWaitFollowUpPlan(userId);
                }
            }
            else
            {
                MessageBox.Show(result.Msg, "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
        #endregion
    }
}