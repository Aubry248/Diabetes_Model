using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using BLL;
using Model;

namespace DoctorUI
{
    public partial class FrmEffectEvaluation : Form
    {
        #region ========== 原有全局统一布局参数（100%保留，无任何修改）==========
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

        #region 原有核心控件声明（100%保留，仅追加Chart控件声明）
        private Panel pnlMainContainer;
        private Panel pnlContentWrapper;
        private TabControl tabMain;
        private TabPage tab_GlucoseTrend, tab_ComplicationProgress, tab_EffectScore, tab_ReportGenerate;

        // 血糖趋势分析
        private GroupBox grp_GlucoseQuery, grp_GlucoseChart, grp_GlucoseList;
        private ComboBox cbo_GlucosePatient;
        private DateTimePicker dtp_GlucoseStart, dtp_GlucoseEnd;
        private Chart chart_GlucoseTrend; // 追加：真实血糖趋势图控件
        private DataGridView dgv_GlucoseList;
        private Button btn_QueryGlucose, btn_ResetGlucose;

        // 并发症进展跟踪
        private GroupBox grp_ComplicationQuery, grp_ComplicationList;
        private ComboBox cbo_ComplicationPatient, cbo_ComplicationType;
        private DateTimePicker dtp_ComplicationStart, dtp_ComplicationEnd;
        private DataGridView dgv_ComplicationList;
        private Button btn_QueryComplication, btn_ResetComplication;

        // 干预效果评分
        private GroupBox grp_ScorePatient, grp_ScoreItems, grp_ScoreResult;
        private ComboBox cbo_ScorePatient, cbo_InterventionType;
        private TextBox txt_DietScore, txt_ExerciseScore, txt_MedicationScore, txt_ComplianceScore;
        private Label lbl_TotalScore, lbl_EffectLevel;
        private Button btn_CalcScore, btn_ResetScore;

        // 报告生成
        private GroupBox grp_ReportQuery, grp_ReportPreview;
        private ComboBox cbo_ReportPatient;
        private DateTimePicker dtp_ReportStart, dtp_ReportEnd;
        private TextBox txt_ReportPreview;
        private Button btn_PreviewReport, btn_ExportReport, btn_ResetReport;
        #endregion

        #region 全局业务变量（新增，不影响原有代码）
        private readonly B_EffectEvaluation _bllEvaluation = new B_EffectEvaluation();
        private List<PatientSimpleInfo> _allPatientList = new List<PatientSimpleInfo>();
        #endregion

        // 原有构造函数（100%保留，仅追加窗体加载登录校验）
        public FrmEffectEvaluation()
        {
            this.Text = "干预效果评估";
            this.Size = new Size(1300, 800);
            this.MinimumSize = new Size(_globalContentMinWidth + 40, _globalContentMinHeight + 100);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("微软雅黑", 9.5F);
            this.Dock = DockStyle.Fill;

            InitMainContainer();
            InitializeDynamicControls();
            InitControlData();
            BindAllEvents();

            // 追加：窗体加载登录校验
            this.Load += FrmEffectEvaluation_Load;
        }

        #region 原有全局容器初始化（100%完全保留，无任何修改）
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
        #endregion

        #region 原有动态创建控件方法（仅修改血糖趋势图部分，其余100%保留）
        private void InitializeDynamicControls()
        {
            tabMain = new TabControl();
            tabMain.Dock = DockStyle.Fill;
            tabMain.Font = new Font("微软雅黑", 10F);
            tabMain.Padding = new Point(15, 8);

            tab_GlucoseTrend = new TabPage("血糖趋势分析") { BackColor = Color.White };
            tab_ComplicationProgress = new TabPage("并发症进展跟踪") { BackColor = Color.White };
            tab_EffectScore = new TabPage("干预效果评分") { BackColor = Color.White };
            tab_ReportGenerate = new TabPage("报告生成") { BackColor = Color.White };

            tabMain.TabPages.AddRange(new TabPage[] { tab_GlucoseTrend, tab_ComplicationProgress, tab_EffectScore, tab_ReportGenerate });
            pnlContentWrapper.Controls.Add(tabMain);

            InitGlucoseTrendPage();
            InitComplicationProgressPage();
            InitEffectScorePage();
            InitReportGeneratePage();
        }

        // 1. 血糖趋势分析页面（仅替换模拟标签为真实Chart控件，其余布局100%保留）
        private void InitGlucoseTrendPage()
        {
            grp_GlucoseQuery = new GroupBox { Text = "查询条件", Dock = DockStyle.Top, Height = 120, Padding = _globalGroupBoxPadding };
            tab_GlucoseTrend.Controls.Add(grp_GlucoseQuery);
            TableLayoutPanel tlp_Glucose = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Glucose.RowCount = 1;
            tlp_Glucose.ColumnCount = 3;
            tlp_Glucose.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlp_Glucose.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlp_Glucose.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            tlp_Glucose.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_GlucoseQuery.Controls.Add(tlp_Glucose);
            int row = 0;
            CreateEditItem<ComboBox>(tlp_Glucose, out _, out cbo_GlucosePatient, "选择患者：", ref row, false);

            Panel pnl_DateRange = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_Date = new Label { Text = "时间范围：", Size = new Size(80, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(0, 0) };
            dtp_GlucoseStart = new DateTimePicker { Location = new Point(80, 0), Size = new Size(140, _globalControlHeight), Format = DateTimePickerFormat.Short };
            dtp_GlucoseEnd = new DateTimePicker { Location = new Point(230, 0), Size = new Size(140, _globalControlHeight), Format = DateTimePickerFormat.Short };
            pnl_DateRange.Controls.AddRange(new Control[] { lbl_Date, dtp_GlucoseStart, dtp_GlucoseEnd });
            tlp_Glucose.Controls.Add(pnl_DateRange, 1, 0);

            Panel pnl_GlucoseBtn = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            FlowLayoutPanel flp_GlucoseBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            btn_QueryGlucose = CreateBtn("查询趋势", Color.FromArgb(0, 122, 204));
            btn_ResetGlucose = CreateBtn("重置", Color.Gray);
            flp_GlucoseBtn.Controls.AddRange(new Control[] { btn_ResetGlucose, btn_QueryGlucose });
            pnl_GlucoseBtn.Controls.Add(flp_GlucoseBtn);
            tlp_Glucose.Controls.Add(pnl_GlucoseBtn, 2, 0);

            // 替换原有模拟标签为真实Chart控件，布局位置、尺寸100%保留
            grp_GlucoseChart = new GroupBox { Text = "血糖趋势图", Dock = DockStyle.Top, Height = 250, Padding = _globalGroupBoxPadding };
            tab_GlucoseTrend.Controls.Add(grp_GlucoseChart);
            chart_GlucoseTrend = new Chart { Dock = DockStyle.Fill };
            ChartArea chartArea = new ChartArea("MainArea");
            chartArea.AxisX.Title = "测量时间";
            chartArea.AxisY.Title = "血糖值(mmol/L)";
            chartArea.AxisY.Minimum = 2;
            chartArea.AxisY.Maximum = 25;
            chart_GlucoseTrend.ChartAreas.Add(chartArea);
            chart_GlucoseTrend.Series.Add(new Series("空腹血糖") { ChartType = SeriesChartType.Line, Color = Color.Red, BorderWidth = 2 });
            chart_GlucoseTrend.Series.Add(new Series("餐后2小时血糖") { ChartType = SeriesChartType.Line, Color = Color.Blue, BorderWidth = 2 });
            chart_GlucoseTrend.Legends.Add(new Legend("MainLegend") { Docking = Docking.Top });
            grp_GlucoseChart.Controls.Add(chart_GlucoseTrend);

            grp_GlucoseList = new GroupBox { Text = "血糖数据明细", Dock = DockStyle.Fill, Padding = _globalGroupBoxPadding };
            tab_GlucoseTrend.Controls.Add(grp_GlucoseList);
            dgv_GlucoseList = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };
            grp_GlucoseList.Controls.Add(dgv_GlucoseList);

            dgv_GlucoseList.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { HeaderText = "测量时间", DataPropertyName = "MeasureTime", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm" } },
                new DataGridViewTextBoxColumn { HeaderText = "测量场景", DataPropertyName = "MeasureScenario", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "血糖值(mmol/L)", DataPropertyName = "BloodSugarValue", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "控制状态", DataPropertyName = "ControlStatus", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "是否异常", DataPropertyName = "IsAbnormal", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
            });
        }

        // 2. 并发症进展跟踪页面（100%保留原有布局，无任何修改）
        private void InitComplicationProgressPage()
        {
            grp_ComplicationQuery = new GroupBox { Text = "查询条件", Dock = DockStyle.Top, Height = 160, Padding = _globalGroupBoxPadding };
            tab_ComplicationProgress.Controls.Add(grp_ComplicationQuery);
            TableLayoutPanel tlp_Complication = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Complication.RowCount = 2;
            tlp_Complication.ColumnCount = 3;
            tlp_Complication.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlp_Complication.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlp_Complication.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            tlp_Complication.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            tlp_Complication.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_ComplicationQuery.Controls.Add(tlp_Complication);
            int row = 0;
            CreateEditItem<ComboBox>(tlp_Complication, out _, out cbo_ComplicationPatient, "选择患者：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Complication, out _, out cbo_ComplicationType, "并发症类型：", ref row, false);

            Panel pnl_DateRange = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_Date = new Label { Text = "随访时间：", Size = new Size(80, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(0, 0) };
            dtp_ComplicationStart = new DateTimePicker { Location = new Point(80, 0), Size = new Size(140, _globalControlHeight), Format = DateTimePickerFormat.Short };
            dtp_ComplicationEnd = new DateTimePicker { Location = new Point(230, 0), Size = new Size(140, _globalControlHeight), Format = DateTimePickerFormat.Short };
            pnl_DateRange.Controls.AddRange(new Control[] { lbl_Date, dtp_ComplicationStart, dtp_ComplicationEnd });
            tlp_Complication.Controls.Add(pnl_DateRange, 2, 0);

            Panel pnl_ComplicationBtn = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            FlowLayoutPanel flp_ComplicationBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            btn_QueryComplication = CreateBtn("查询进展", Color.FromArgb(0, 122, 204));
            btn_ResetComplication = CreateBtn("重置", Color.Gray);
            flp_ComplicationBtn.Controls.AddRange(new Control[] { btn_ResetComplication, btn_QueryComplication });
            pnl_ComplicationBtn.Controls.Add(flp_ComplicationBtn);
            tlp_Complication.Controls.Add(pnl_ComplicationBtn, 2, 1);

            grp_ComplicationList = new GroupBox { Text = "并发症进展记录", Dock = DockStyle.Fill, Padding = _globalGroupBoxPadding };
            tab_ComplicationProgress.Controls.Add(grp_ComplicationList);
            dgv_ComplicationList = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };
            grp_ComplicationList.Controls.Add(dgv_ComplicationList);

            dgv_ComplicationList.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { HeaderText = "随访日期", DataPropertyName = "RecordDate", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd" } },
                new DataGridViewTextBoxColumn { HeaderText = "并发症类型", DataPropertyName = "ComplicationType", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "指标变化", DataPropertyName = "IndexChangeDesc", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "进展程度", DataPropertyName = "ProgressDegree", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "干预措施", DataPropertyName = "InterventionMeasures", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "随访医生", DataPropertyName = "FollowUpDoctor", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
            });
        }

        // 3. 干预效果评分页面（100%保留原有布局，无任何修改）
        private void InitEffectScorePage()
        {
            Panel pnl_ScoreBtn = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(15) };
            tab_EffectScore.Controls.Add(pnl_ScoreBtn);
            FlowLayoutPanel flp_ScoreBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_CalcScore = CreateBtn("计算评分", Color.FromArgb(0, 150, 136));
            btn_ResetScore = CreateBtn("重置", Color.Gray);
            flp_ScoreBtn.Controls.AddRange(new Control[] { btn_CalcScore, btn_ResetScore });
            pnl_ScoreBtn.Controls.Add(flp_ScoreBtn);

            grp_ScoreResult = new GroupBox { Text = "评分结果", Dock = DockStyle.Top, Height = 120, Padding = _globalGroupBoxPadding };
            tab_EffectScore.Controls.Add(grp_ScoreResult);
            TableLayoutPanel tlp_ScoreResult = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_ScoreResult.RowCount = 2;
            tlp_ScoreResult.ColumnCount = 2;
            tlp_ScoreResult.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_ScoreResult.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_ScoreResult.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            tlp_ScoreResult.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_ScoreResult.Controls.Add(tlp_ScoreResult);

            Label lbl_TotalTitle = new Label { Text = "综合评分：", Font = new Font("微软雅黑", 11, FontStyle.Bold), AutoSize = true };
            lbl_TotalScore = new Label { Text = "0", Font = new Font("微软雅黑", 12, FontStyle.Bold), ForeColor = Color.Green, AutoSize = true };
            Label lbl_LevelTitle = new Label { Text = "效果等级：", Font = new Font("微软雅黑", 11, FontStyle.Bold), AutoSize = true };
            lbl_EffectLevel = new Label { Text = "未评分", Font = new Font("微软雅黑", 12, FontStyle.Bold), ForeColor = Color.Orange, AutoSize = true };
            tlp_ScoreResult.Controls.Add(lbl_TotalTitle, 0, 0);
            tlp_ScoreResult.Controls.Add(lbl_TotalScore, 1, 0);
            tlp_ScoreResult.Controls.Add(lbl_LevelTitle, 0, 1);
            tlp_ScoreResult.Controls.Add(lbl_EffectLevel, 1, 1);

            grp_ScoreItems = new GroupBox { Text = "干预效果评分项（满分100）", Dock = DockStyle.Top, Height = 240, Padding = _globalGroupBoxPadding };
            tab_EffectScore.Controls.Add(grp_ScoreItems);
            TableLayoutPanel tlp_ScoreItems = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_ScoreItems.RowCount = 4;
            tlp_ScoreItems.ColumnCount = 2;
            tlp_ScoreItems.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_ScoreItems.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (int i = 0; i < 4; i++) tlp_ScoreItems.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_ScoreItems.Controls.Add(tlp_ScoreItems);
            int row = 0;
            CreateEditItem<TextBox>(tlp_ScoreItems, out _, out txt_DietScore, "饮食干预评分：", ref row, false);
            CreateEditItem<TextBox>(tlp_ScoreItems, out _, out txt_ExerciseScore, "运动干预评分：", ref row, false);
            CreateEditItem<TextBox>(tlp_ScoreItems, out _, out txt_MedicationScore, "用药干预评分：", ref row, false);
            CreateEditItem<TextBox>(tlp_ScoreItems, out _, out txt_ComplianceScore, "依从性评分：", ref row, false);

            grp_ScorePatient = new GroupBox { Text = "患者与干预类型", Dock = DockStyle.Top, Height = 120, Padding = _globalGroupBoxPadding };
            tab_EffectScore.Controls.Add(grp_ScorePatient);
            TableLayoutPanel tlp_ScorePatient = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_ScorePatient.RowCount = 1;
            tlp_ScorePatient.ColumnCount = 2;
            tlp_ScorePatient.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_ScorePatient.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_ScorePatient.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_ScorePatient.Controls.Add(tlp_ScorePatient);
            row = 0;
            CreateEditItem<ComboBox>(tlp_ScorePatient, out _, out cbo_ScorePatient, "选择患者：", ref row, false);
            CreateEditItem<ComboBox>(tlp_ScorePatient, out _, out cbo_InterventionType, "干预类型：", ref row, false);
        }

        // 4. 报告生成页面（100%保留原有布局，无任何修改）
        private void InitReportGeneratePage()
        {
            grp_ReportQuery = new GroupBox { Text = "报告生成条件", Dock = DockStyle.Top, Height = 120, Padding = _globalGroupBoxPadding };
            tab_ReportGenerate.Controls.Add(grp_ReportQuery);
            TableLayoutPanel tlp_Report = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Report.RowCount = 1;
            tlp_Report.ColumnCount = 3;
            tlp_Report.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlp_Report.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlp_Report.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            tlp_Report.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_ReportQuery.Controls.Add(tlp_Report);
            int row = 0;
            CreateEditItem<ComboBox>(tlp_Report, out _, out cbo_ReportPatient, "选择患者：", ref row, false);

            Panel pnl_DateRange = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_Date = new Label { Text = "评估周期：", Size = new Size(80, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(0, 0) };
            dtp_ReportStart = new DateTimePicker { Location = new Point(80, 0), Size = new Size(140, _globalControlHeight), Format = DateTimePickerFormat.Short };
            dtp_ReportEnd = new DateTimePicker { Location = new Point(230, 0), Size = new Size(140, _globalControlHeight), Format = DateTimePickerFormat.Short };
            pnl_DateRange.Controls.AddRange(new Control[] { lbl_Date, dtp_ReportStart, dtp_ReportEnd });
            tlp_Report.Controls.Add(pnl_DateRange, 1, 0);

            Panel pnl_ReportBtn = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            FlowLayoutPanel flp_ReportBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            btn_PreviewReport = CreateBtn("预览报告", Color.FromArgb(0, 122, 204));
            btn_ExportReport = CreateBtn("导出报告", Color.FromArgb(255, 152, 0));
            btn_ResetReport = CreateBtn("重置", Color.Gray);
            flp_ReportBtn.Controls.AddRange(new Control[] { btn_ResetReport, btn_ExportReport, btn_PreviewReport });
            pnl_ReportBtn.Controls.Add(flp_ReportBtn);
            tlp_Report.Controls.Add(pnl_ReportBtn, 2, 0);

            grp_ReportPreview = new GroupBox { Text = "干预效果评估报告预览", Dock = DockStyle.Fill, Padding = _globalGroupBoxPadding };
            tab_ReportGenerate.Controls.Add(grp_ReportPreview);
            txt_ReportPreview = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("微软雅黑", 9F),
                Margin = _globalControlMargin,
                ReadOnly = true,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            grp_ReportPreview.Controls.Add(txt_ReportPreview);
        }
        #endregion

        #region 原有通用控件创建方法（100%完全保留，无任何修改）
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

        #region 窗体加载与下拉数据初始化（替换原有模拟数据为真实BLL调用）
        // 新增：窗体加载登录校验
        private void FrmEffectEvaluation_Load(object sender, EventArgs e)
        {
            if (GlobalConfig.CurrentDoctorID <= 0)
            {
                MessageBox.Show("登录信息失效，请重新登录系统！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }
        }

        // 替换原有模拟数据为真实BLL调用
        private void InitControlData()
        {
            // 1. 加载有效患者列表
            _allPatientList = _bllEvaluation.GetAllValidPatient();
            if (_allPatientList == null || !_allPatientList.Any())
            {
                MessageBox.Show("未查询到有效患者数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. 绑定所有患者下拉框
            cbo_GlucosePatient.DataSource = new BindingSource(_allPatientList, null);
            cbo_GlucosePatient.DisplayMember = "UserName";
            cbo_GlucosePatient.ValueMember = "UserId";

            cbo_ComplicationPatient.DataSource = new BindingSource(_allPatientList, null);
            cbo_ComplicationPatient.DisplayMember = "UserName";
            cbo_ComplicationPatient.ValueMember = "UserId";

            cbo_ScorePatient.DataSource = new BindingSource(_allPatientList, null);
            cbo_ScorePatient.DisplayMember = "UserName";
            cbo_ScorePatient.ValueMember = "UserId";

            cbo_ReportPatient.DataSource = new BindingSource(_allPatientList, null);
            cbo_ReportPatient.DisplayMember = "UserName";
            cbo_ReportPatient.ValueMember = "UserId";

            // 3. 并发症类型下拉框
            cbo_ComplicationType.Items.Clear();
            cbo_ComplicationType.Items.AddRange(new string[] { "全部", "糖尿病肾病", "糖尿病视网膜病变", "糖尿病神经病变", "糖尿病足", "心血管病变" });
            cbo_ComplicationType.SelectedIndex = 0;

            // 4. 干预类型下拉框
            cbo_InterventionType.Items.Clear();
            cbo_InterventionType.Items.AddRange(new string[] { "综合干预", "饮食干预", "运动干预", "用药干预" });
            cbo_InterventionType.SelectedIndex = 0;

            // 5. 默认时间范围
            dtp_GlucoseStart.Value = DateTime.Now.AddMonths(-3);
            dtp_GlucoseEnd.Value = DateTime.Now;
            dtp_ComplicationStart.Value = DateTime.Now.AddMonths(-6);
            dtp_ComplicationEnd.Value = DateTime.Now;
            dtp_ReportStart.Value = DateTime.Now.AddMonths(-3);
            dtp_ReportEnd.Value = DateTime.Now;

            // 6. 默认选中第一项
            cbo_GlucosePatient.SelectedIndex = 0;
            cbo_ComplicationPatient.SelectedIndex = 0;
            cbo_ScorePatient.SelectedIndex = 0;
            cbo_ReportPatient.SelectedIndex = 0;
        }
        #endregion

        #region 事件绑定（替换原有模拟事件为真实BLL业务逻辑）
        private void BindAllEvents()
        {
            #region 血糖趋势分析：查询/重置按钮事件
            btn_QueryGlucose.Click += Btn_QueryGlucose_Click;
            btn_ResetGlucose.Click += (s, e) =>
            {
                cbo_GlucosePatient.SelectedIndex = 0;
                dtp_GlucoseStart.Value = DateTime.Now.AddMonths(-3);
                dtp_GlucoseEnd.Value = DateTime.Now;
                dgv_GlucoseList.DataSource = null;
                chart_GlucoseTrend.Series[0].Points.Clear();
                chart_GlucoseTrend.Series[1].Points.Clear();
            };
            #endregion

            #region 并发症进展跟踪：查询/重置按钮事件
            btn_QueryComplication.Click += Btn_QueryComplication_Click;
            btn_ResetComplication.Click += (s, e) =>
            {
                cbo_ComplicationPatient.SelectedIndex = 0;
                cbo_ComplicationType.SelectedIndex = 0;
                dtp_ComplicationStart.Value = DateTime.Now.AddMonths(-6);
                dtp_ComplicationEnd.Value = DateTime.Now;
                dgv_ComplicationList.DataSource = null;
            };
            #endregion

            #region 干预效果评分：计算/重置按钮事件
            btn_CalcScore.Click += Btn_CalcScore_Click;
            btn_ResetScore.Click += (s, e) =>
            {
                cbo_ScorePatient.SelectedIndex = 0;
                cbo_InterventionType.SelectedIndex = 0;
                txt_DietScore.Clear();
                txt_ExerciseScore.Clear();
                txt_MedicationScore.Clear();
                txt_ComplianceScore.Clear();
                lbl_TotalScore.Text = "0";
                lbl_EffectLevel.Text = "未评分";
                lbl_EffectLevel.ForeColor = Color.Orange;
            };
            #endregion

            #region 报告生成：预览/导出/重置按钮事件
            btn_PreviewReport.Click += Btn_PreviewReport_Click;
            btn_ExportReport.Click += Btn_ExportReport_Click;
            btn_ResetReport.Click += (s, e) =>
            {
                cbo_ReportPatient.SelectedIndex = 0;
                dtp_ReportStart.Value = DateTime.Now.AddMonths(-3);
                dtp_ReportEnd.Value = DateTime.Now;
                txt_ReportPreview.Clear();
            };
            #endregion
        }

        #region 核心按钮事件业务实现
        /// <summary>
        /// 血糖趋势查询按钮事件
        /// </summary>
        private void Btn_QueryGlucose_Click(object sender, EventArgs e)
        {
            if (!(cbo_GlucosePatient.SelectedValue is int userId) || userId <= 0)
            {
                MessageBox.Show("请选择有效患者", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime startDate = dtp_GlucoseStart.Value;
            DateTime endDate = dtp_GlucoseEnd.Value;
            var result = _bllEvaluation.GetBloodSugarTrendData(userId, startDate, endDate);

            if (!result.Success)
            {
                MessageBox.Show(result.Msg, "查询失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 绑定明细列表
            dgv_GlucoseList.DataSource = result.TrendList;

            // 渲染趋势图
            chart_GlucoseTrend.Series[0].Points.Clear();
            chart_GlucoseTrend.Series[1].Points.Clear();

            if (result.TrendList != null && result.TrendList.Any())
            {
                // 空腹血糖数据
                var fastingList = result.TrendList.Where(x => x.MeasureScenario == "空腹").ToList();
                foreach (var item in fastingList)
                {
                    chart_GlucoseTrend.Series[0].Points.AddXY(item.MeasureTime, item.BloodSugarValue);
                }

                // 餐后血糖数据
                var postList = result.TrendList.Where(x => x.MeasureScenario == "餐后2小时").ToList();
                foreach (var item in postList)
                {
                    chart_GlucoseTrend.Series[1].Points.AddXY(item.MeasureTime, item.BloodSugarValue);
                }

                // 显示统计信息
                MessageBox.Show($"查询成功！\n平均空腹血糖：{result.Statistics.AvgFastingGlucose:F1} mmol/L\n平均餐后血糖：{result.Statistics.AvgPostprandialGlucose:F1} mmol/L\n血糖达标率：{result.Statistics.StandardReachRate:F2}%", "查询完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(result.Msg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 并发症进展查询按钮事件
        /// </summary>
        private void Btn_QueryComplication_Click(object sender, EventArgs e)
        {
            if (!(cbo_ComplicationPatient.SelectedValue is int userId) || userId <= 0)
            {
                MessageBox.Show("请选择有效患者", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string complicationType = cbo_ComplicationType.SelectedItem?.ToString();
            DateTime startDate = dtp_ComplicationStart.Value;
            DateTime endDate = dtp_ComplicationEnd.Value;

            var result = _bllEvaluation.GetComplicationProgressList(userId, complicationType, startDate, endDate);
            if (!result.Success)
            {
                MessageBox.Show(result.Msg, "查询失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            dgv_ComplicationList.DataSource = result.Data;
            if (result.Data == null || !result.Data.Any())
            {
                MessageBox.Show(result.Msg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 计算并保存评分按钮事件
        /// </summary>
        private void Btn_CalcScore_Click(object sender, EventArgs e)
        {
            // 输入校验
            if (!int.TryParse(txt_DietScore.Text, out int dietScore) || !int.TryParse(txt_ExerciseScore.Text, out int exerciseScore) ||
                !int.TryParse(txt_MedicationScore.Text, out int medicationScore) || !int.TryParse(txt_ComplianceScore.Text, out int complianceScore))
            {
                MessageBox.Show("请输入有效的数字评分！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!(cbo_ScorePatient.SelectedValue is int userId) || userId <= 0)
            {
                MessageBox.Show("请选择有效患者", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 构建评分模型
            var scoreModel = new InterventionEffectScore
            {
                UserId = userId,
                InterventionType = cbo_InterventionType.SelectedItem?.ToString(),
                DietScore = dietScore,
                ExerciseScore = exerciseScore,
                MedicationScore = medicationScore,
                ComplianceScore = complianceScore,
                AssessmentBy = GlobalConfig.CurrentDoctorID
            };

            // 调用BLL计算并保存
            var result = _bllEvaluation.CalcAndSaveEffectScore(scoreModel);
            if (!result.Success)
            {
                MessageBox.Show(result.Msg, "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 更新界面显示
            lbl_TotalScore.Text = result.TotalScore.ToString();
            lbl_EffectLevel.Text = result.EffectLevel;
            lbl_EffectLevel.ForeColor = result.TotalScore >= 80 ? Color.Green : result.TotalScore >= 60 ? Color.Orange : Color.Red;

            MessageBox.Show(result.Msg, "操作成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 报告预览按钮事件
        /// </summary>
        private void Btn_PreviewReport_Click(object sender, EventArgs e)
        {
            if (!(cbo_ReportPatient.SelectedValue is int userId) || userId <= 0)
            {
                MessageBox.Show("请选择有效患者", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime startDate = dtp_ReportStart.Value;
            DateTime endDate = dtp_ReportEnd.Value;
            string doctorName = GlobalConfig.CurrentDoctorName;

            var result = _bllEvaluation.GenerateEvaluationReport(userId, startDate, endDate, doctorName);
            if (!result.Success)
            {
                MessageBox.Show(result.Msg, "报告生成失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 生成报告文本并显示
            string reportText = _bllEvaluation.BuildReportText(result.ReportData);
            txt_ReportPreview.Text = reportText;
            MessageBox.Show("报告预览生成成功！", "操作成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 报告导出按钮事件
        /// </summary>
        private void Btn_ExportReport_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txt_ReportPreview.Text))
            {
                MessageBox.Show("请先预览报告再执行导出", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 打开保存对话框
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Title = "导出干预效果评估报告",
                Filter = "文本文件(*.txt)|*.txt|Word文档(*.doc)|*.doc|所有文件(*.*)|*.*",
                FileName = $"{cbo_ReportPatient.Text}_糖尿病干预效果评估报告_{DateTime.Now:yyyyMMddHHmmss}",
                DefaultExt = ".txt"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(saveDialog.FileName, txt_ReportPreview.Text, System.Text.Encoding.UTF8);
                    MessageBox.Show($"报告导出成功！\n保存路径：{saveDialog.FileName}", "导出成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion
        #endregion
    }
}