using BLL;
using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Tools;
// 【必做配置】右键项目→引用→程序集→勾选「System.Windows.Forms.DataVisualization」
namespace PatientUI
{
    public partial class FrmBloodSugarManage : Form
    {
        #region ====================== 【动态自适应布局参数区】已修复所有不合理配置 ======================
        // 1. 页面边距（修复：左侧边距加大，避让侧边栏遮挡；顶部边距加大，避让主窗体标题栏）
        private readonly int _pageTopMargin = 80;
        private readonly int _pageLeftMargin = 230;
        private readonly int _pageRightMargin = 40;
        private readonly int _pageBottomMargin = 25;

        // 2. 各区域【最小高度】（修复：加大最小高度，确保控件完整显示，不被压缩）
        private readonly int _statsAreaMinHeight = 100;    // 统计卡片最小高度，避免被标题栏遮挡
        private readonly int _inputAreaMinHeight = 138;   // 录入区最小高度，确保输入框完整显示
        private readonly int _filterAreaMinHeight = 60;    // 筛选区最小高度
     /*   private readonly int _pageAreaMinHeight = 40;   */   // 分页区最小高度，避免和图表挤压

        // 3. 弹性区域高度占比（剩余可用高度按比例分配给表格和图表，总和100%）
        private readonly float _gridAreaHeightRatio = 45f;  // 表格区占剩余高度的45%
        private readonly float _chartAreaHeightRatio = 55f; // 图表区占剩余高度的55%

        // 4. 表格列【最小宽度】（确保关键列永远不会被压缩消失）
        private readonly int _colMinWidth_Value = 100;
        private readonly int _colMinWidth_Scenario = 90;
        private readonly int _colMinWidth_Time = 150;
        private readonly int _colMinWidth_Status = 60;
        private readonly int _colMinWidth_Source = 90;
        //private readonly int _colMinWidth_Operate = 150;

        // 5. 按钮基础尺寸
        private readonly int _btnSaveWidth = 100;
        private readonly int _btnClearWidth = 70;
        private readonly int _btnQueryWidth = 80;
        private readonly int _btnPageWidth = 60;
        private readonly int _btnHeight = 32;

        // 6. 字体大小
        private readonly float _globalFontSize = 9f;
        private readonly float _titleFontSize = 12f;
        private readonly float _tableFontSize = 9f;

        // 7. 临床常量（与数据库、BLL层保持一致）
        private const decimal FastingMin = 3.9m;
        private const decimal FastingMax = 6.1m;
        private const decimal PostprandialMax = 7.8m;
        private const decimal SugarMinValid = 0.5m;
        private const decimal SugarMaxValid = 30.0m;

        // 8. 分页配置（修复：删除重复声明，确保赋值生效）
        private readonly int _pageSize = 10;

        // 9. 颜色主题
        private readonly Color _themePrimaryColor = Color.FromArgb(0, 122, 204);
        private readonly Color _themeSecondaryColor = Color.FromArgb(108, 117, 125);
        private readonly Color _themeSuccessColor = Color.FromArgb(40, 167, 69);
        private readonly Color _themeDangerColor = Color.FromArgb(220, 53, 69);
        private readonly Color _cardBgColor = Color.FromArgb(248, 250, 252);
        private readonly Padding _controlMargin = new Padding(5);
        private readonly Padding _btnMargin = new Padding(4);
        #endregion

        #region 全局私有变量（修复：删除重复的_pageSize声明）
        private readonly B_BloodSugar bllBs = new B_BloodSugar();
        private int _currentPageIndex = 1;
        private int _totalCount = 0;
        private DataGridView _dgvBloodSugar;
        private Chart _chartTrend;
        private TextBox _txtSugarValue;
        private ComboBox _cboScenario;
        private DateTimePicker _dtpMeasureTime;
        private ComboBox _cboFilterScenario;
        private DateTimePicker _dtpFilterStart;
        private DateTimePicker _dtpFilterEnd;
        private Label _lblPageInfo;
        private Font _globalFont;
        private TableLayoutPanel _rootPanel;
        private bool _isInitialized = false;
        #endregion

        #region 构造函数（新增高DPI适配，彻底解决缩放导致的表头文字裁剪）
        public FrmBloodSugarManage()
        {
            // 【高DPI适配必须放在最前面】
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            AutoScaleMode = AutoScaleMode.Dpi; // 替换原有的Font模式，适配系统缩放
            Font = new Font("微软雅黑", 9f, FontStyle.Regular, GraphicsUnit.Point);

            InitializeComponent();
            _globalFont = new Font("微软雅黑", _globalFontSize, FontStyle.Regular, GraphicsUnit.Point);

            // 子窗体嵌入核心配置
            this.TopLevel = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.Font = _globalFont;
            this.AutoScroll = true;

            // 事件绑定
            this.SizeChanged += FrmBloodSugarManage_SizeChanged;
            this.Load += FrmBloodSugarManage_Load;
            this.VisibleChanged += FrmBloodSugarManage_VisibleChanged;
            this.Disposed += (s, e) =>
            {
                _globalFont?.Dispose();
                _dgvBloodSugar?.Dispose();
                _chartTrend?.Dispose();
            };
        }

        // 尺寸变化时，重新计算布局
        private void FrmBloodSugarManage_SizeChanged(object sender, EventArgs e)
        {
            if (_isInitialized && _rootPanel != null && this.ClientSize.Width > 0 && this.ClientSize.Height > 0)
            {
                AdaptiveLayout();
            }
        }
        #endregion
        #region 核心：动态适配父容器可用尺寸的布局方法
        /// <summary>
        /// 基于当前可用尺寸，动态调整各区域高度
        /// </summary>
        private void AdaptiveLayout()
        {
            if (_rootPanel == null || this.ClientSize.Width <= 0 || this.ClientSize.Height <= 0) return;
            // 1. 获取当前可用的宽高
            int availableWidth = this.ClientSize.Width - _pageLeftMargin - _pageRightMargin;
            int availableHeight = this.ClientSize.Height - _pageTopMargin - _pageBottomMargin;
            // 2. 计算固定区域的总高度
            int fixedTotalHeight = _statsAreaMinHeight + _inputAreaMinHeight + _filterAreaMinHeight;
            // 3. 剩余高度分配给表格和图表，🔥 表格最小高度从180px加大到250px，确保表头+数据行完整显示
            int remainHeight = Math.Max(availableHeight - fixedTotalHeight, 650);
            int gridAreaHeight = Math.Max((int)(remainHeight * _gridAreaHeightRatio / 100), 360);
            int chartAreaHeight = Math.Max((int)(remainHeight * _chartAreaHeightRatio / 100), 260);
            // 4. 动态更新根容器的行高
            _rootPanel.RowStyles[0].Height = _statsAreaMinHeight;
            _rootPanel.RowStyles[1].Height = _inputAreaMinHeight;
            _rootPanel.RowStyles[2].Height = _filterAreaMinHeight;
            _rootPanel.RowStyles[3].Height = gridAreaHeight;
            _rootPanel.RowStyles[4].Height = chartAreaHeight;
            // 5. 强制刷新布局
            _rootPanel.PerformLayout();
            this.PerformLayout();
        }
        #endregion

        #region 窗体生命周期事件
        private void FrmBloodSugarManage_Load(object sender, EventArgs e)
        {
            if (!this.DesignMode)
            {
                BuildLayout();
                LoadAllData();
            }
        }

        private bool _isLoading = false;
        private void FrmBloodSugarManage_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible && !this.DesignMode && _isInitialized && !_isLoading)
            {
                _isLoading = true;
                AdaptiveLayout();
                LoadAllData();
                _isLoading = false;
            }
        }
        #endregion

        #region 布局初始化方法（已修复所有UI布局问题）
        private void BuildLayout()
        {
            this.Controls.Clear();
            this.SuspendLayout();

            var scrollContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true
            };

            // 1. 第一步：初始化根容器（已从6行改为5行，移除单独的分页行）
            _rootPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                RowCount = 5,
                ColumnCount = 1,
                BackColor = Color.White,
                Padding = new Padding(_pageLeftMargin, _pageTopMargin, _pageRightMargin, _pageBottomMargin),
                GrowStyle = TableLayoutPanelGrowStyle.AddRows,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            // 行样式初始化（最小高度兜底）
            _rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, _statsAreaMinHeight));
            _rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, _inputAreaMinHeight));
            _rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, _filterAreaMinHeight));
            _rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 360));
            _rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 260));
            _rootPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            // 2. 第二步：根容器加入窗体，先执行动态适配
            scrollContainer.Controls.Add(_rootPanel);
            this.Controls.Add(scrollContainer);
            _isInitialized = true;
            AdaptiveLayout();

            #region 1. 统计卡片区域（无修改，保留原有逻辑）
            var statsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 4,
                BackColor = Color.White
            };
            for (int i = 0; i < 4; i++)
            {
                statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            }
            statsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            string[] cardTitles = { "今日平均血糖", "今日最高血糖", "今日最低血糖", "今日异常次数" };
            string[] cardTags = { "avg", "max", "min", "abnormal" };
            string[] cardUnits = { "mmol/L", "mmol/L", "mmol/L", "次" };
            for (int i = 0; i < 4; i++)
            {
                var card = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = _cardBgColor,
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = _controlMargin
                };
                var cardLayout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    RowCount = 2,
                    ColumnCount = 1
                };
                cardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40f));
                cardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60f));
                cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

                var lblTitle = new Label
                {
                    Text = cardTitles[i],
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.BottomCenter,
                    Font = new Font(_globalFont.FontFamily, 9f, FontStyle.Regular),
                    ForeColor = _themeSecondaryColor
                };
                var lblValue = new Label
                {
                    Name = $"lblStats_{cardTags[i]}",
                    Text = "0",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.TopCenter,
                    Font = new Font(_globalFont.FontFamily, 16f, FontStyle.Bold),
                    ForeColor = _themePrimaryColor
                };
                var lblUnit = new Label
                {
                    Text = cardUnits[i],
                    Dock = DockStyle.Right,
                    Font = new Font(_globalFont.FontFamily, 8f),
                    ForeColor = _themeSecondaryColor
                };

                cardLayout.Controls.Add(lblTitle, 0, 0);
                cardLayout.Controls.Add(lblValue, 0, 1);
                lblValue.Controls.Add(lblUnit);
                card.Controls.Add(cardLayout);
                statsPanel.Controls.Add(card, i, 0);
            }
            _rootPanel.Controls.Add(statsPanel, 0, 0);
            #endregion

            #region 2. 血糖录入区域（无修改，保留原有逻辑）
            var inputPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _cardBgColor,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(12)
            };
            var inputLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 4
            };
            // 列宽优化：确保每列有足够空间，血糖值输入框不会消失
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22f));
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22f));
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26f));
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
            inputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25f));
            inputLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // 1. 血糖值输入框
            _txtSugarValue = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = _controlMargin,
                Font = _globalFont,
                MinimumSize = new Size(80, 25)
            };
            // 限制仅能输入数字、小数点、退格键
            _txtSugarValue.KeyPress += (s, e) =>
            {
                if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != (char)8)
                    e.Handled = true;
                if (e.KeyChar == '.' && (s as TextBox).Text.Contains("."))
                    e.Handled = true;
            };
            var sugarValuePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Margin = new Padding(0)
            };
            sugarValuePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            sugarValuePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36f));
            sugarValuePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            sugarValuePanel.Controls.Add(_txtSugarValue, 0, 0);
            sugarValuePanel.Controls.Add(CreateQuickSugarPanel(), 0, 1);
            inputLayout.Controls.Add(new Label
            {
                Text = "血糖值(mmol/L)",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                Font = _globalFont
            }, 0, 0);
            inputLayout.Controls.Add(sugarValuePanel, 0, 1);

            // 2. 测量场景下拉框
            _cboScenario = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = _controlMargin,
                Font = _globalFont
            };
            _cboScenario.Items.AddRange(new[] { "空腹", "餐后2小时", "餐前", "睡前", "随机" });
            _cboScenario.SelectedIndex = 0;
            inputLayout.Controls.Add(new Label
            {
                Text = "测量场景",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                Font = _globalFont
            }, 1, 0);
            inputLayout.Controls.Add(_cboScenario, 1, 1);

            // 3. 测量时间选择框
            _dtpMeasureTime = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm",
                Margin = _controlMargin,
                Font = _globalFont,
                Value = DateTime.Now,
                MinDate = new DateTime(1900, 1, 1),
                MaxDate = DateTime.Now
            };
            inputLayout.Controls.Add(new Label
            {
                Text = "测量时间",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                Font = _globalFont
            }, 2, 0);
            inputLayout.Controls.Add(_dtpMeasureTime, 2, 1);

            // 4. 按钮区
            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Margin = _controlMargin
            };
            var btnSave = new Button
            {
                Text = "保存录入",
                Width = _btnSaveWidth,
                Height = _btnHeight,
                Margin = _btnMargin,
                BackColor = _themePrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font(_globalFont.FontFamily, 9f, FontStyle.Bold)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            var btnClear = new Button
            {
                Text = "清空",
                Width = _btnClearWidth,
                Height = _btnHeight,
                Margin = _btnMargin,
                BackColor = _themeSecondaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = _globalFont
            };
            btnClear.FlatAppearance.BorderSize = 0;

            btnSave.Click += BtnSave_Click;
            btnClear.Click += (s, e) =>
            {
                _txtSugarValue.Clear();
                _cboScenario.SelectedIndex = 0;
                ResetMeasureTimePickerToNow();
            };

            btnPanel.Controls.Add(btnClear);
            btnPanel.Controls.Add(btnSave);
            inputLayout.Controls.Add(btnPanel, 3, 1);
            inputPanel.Controls.Add(inputLayout);
            _rootPanel.Controls.Add(inputPanel, 0, 1);
            #endregion

            #region 3. 筛选区域（【问题1修复】调整“至”字位置，放在两日期中间）
            var filterPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0, 8, 0, 0)
            };
            var filterLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 6
            };
            // 【问题1核心修复】列宽重新分配，列顺序调整为：标签→下拉→开始日期→至→结束日期→查询
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));  // 0: 测量场景标签
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); // 1: 场景下拉框
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130)); // 2: 开始日期控件
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 25));  // 3: 至标签（两日期中间）
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130)); // 4: 结束日期控件
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f)); // 5: 查询按钮
            filterLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // 1. 场景筛选
            _cboFilterScenario = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = _globalFont
            };
            _cboFilterScenario.Items.AddRange(new[] { "全部", "空腹", "餐后2小时", "餐前", "睡前", "随机" });
            _cboFilterScenario.SelectedIndex = 0;
            filterLayout.Controls.Add(new Label
            {
                Text = "测量场景",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = _globalFont
            }, 0, 0);
            filterLayout.Controls.Add(_cboFilterScenario, 1, 0);

            // 【问题1修复】时间范围筛选，调整控件顺序：开始日期→至→结束日期
            _dtpFilterStart = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Font = _globalFont,
                Value = DateTime.Now.AddDays(-30),
                MinDate = new DateTime(1900, 1, 1),
                MaxDate = DateTime.Now
            };
            _dtpFilterEnd = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Font = _globalFont,
                Value = DateTime.Now,
                MinDate = new DateTime(1900, 1, 1),
                MaxDate = DateTime.Now
            };
            filterLayout.Controls.Add(_dtpFilterStart, 2, 0); // 开始日期在第2列
            filterLayout.Controls.Add(new Label
            {
                Text = "至",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = _globalFont
            }, 3, 0); // 至标签在第3列，两日期中间
            filterLayout.Controls.Add(_dtpFilterEnd, 4, 0); // 结束日期在第4列

            // 查询按钮
            var btnQuery = new Button
            {
                Text = "查询",
                Width = _btnQueryWidth,
                Height = _btnHeight,
                BackColor = _themePrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = _globalFont
            };
            btnQuery.FlatAppearance.BorderSize = 0;
            btnQuery.Click += BtnQuery_Click;
            filterLayout.Controls.Add(btnQuery, 5, 0);
            filterPanel.Controls.Add(filterLayout);
            _rootPanel.Controls.Add(filterPanel, 0, 2);
            #endregion

            #region 4. 血糖历史记录表格区域（表头100%显示终极版））
            // 改为TableLayout固定分区，避免Dock叠放导致标题/分页覆盖表格
            var gridOuterPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                MinimumSize = new Size(0, 360),
                RowCount = 2,
                ColumnCount = 1
            };
            gridOuterPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35f));
            gridOuterPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            gridOuterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            // 1. 表格标题（固定35高度，保证表头不被遮挡）
            var lblGridTitle = new Label
            {
                Text = "血糖历史记录",
                Font = new Font(_globalFont.FontFamily, _titleFontSize, FontStyle.Bold),
                ForeColor = _themePrimaryColor,
                Dock = DockStyle.Fill,
                Height = 35,
                Padding = new Padding(0, 8, 0, 8) // 上下留边，避免和表头贴紧
            };
            gridOuterPanel.Controls.Add(lblGridTitle, 0, 0);

            // 2. 表格+分页的内层容器（行1：表格，行2：分页）
            var gridInnerPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = Color.White,
                Padding = new Padding(0)
            };
            gridInnerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            gridInnerPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 64f));
            gridInnerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            gridOuterPanel.Controls.Add(gridInnerPanel, 0, 1);

            // 2.1 分页控件（固定在内层第2行，避免遮挡最后一行数据）
            var pagePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252),
                BorderStyle = BorderStyle.None,
                Height = 64,
                Padding = new Padding(8, 10, 8, 10)
            };

            // 固定两列布局：左侧页码文本，右侧分页按钮，避免Flow布局在高DPI下丢控件
            var pageLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2,
                BackColor = Color.Transparent
            };
            pageLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            pageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            pageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320f));

            var pageButtonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = false,
                Margin = new Padding(0),
                Padding = new Padding(0),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            // 分页按钮（保留原有样式和事件，完全不变）
            var pageBtnMargin = new Padding(2, 0, 2, 0);
            var btnFirstPage = new Button { Text = "首页", Width = _btnPageWidth, Height = 34, Margin = pageBtnMargin, Font = _globalFont, FlatStyle = FlatStyle.Standard, BackColor = Color.White, ForeColor = _themePrimaryColor, MinimumSize = new Size(_btnPageWidth, 34) };
            var btnPrevPage = new Button { Text = "上一页", Width = _btnPageWidth, Height = 34, Margin = pageBtnMargin, Font = _globalFont, FlatStyle = FlatStyle.Standard, BackColor = Color.White, ForeColor = _themePrimaryColor, MinimumSize = new Size(_btnPageWidth, 34) };
            var btnNextPage = new Button { Text = "下一页", Width = _btnPageWidth, Height = 34, Margin = pageBtnMargin, Font = _globalFont, FlatStyle = FlatStyle.Standard, BackColor = Color.White, ForeColor = _themePrimaryColor, MinimumSize = new Size(_btnPageWidth, 34) };
            var btnLastPage = new Button { Text = "末页", Width = _btnPageWidth, Height = 34, Margin = pageBtnMargin, Font = _globalFont, FlatStyle = FlatStyle.Standard, BackColor = Color.White, ForeColor = _themePrimaryColor, MinimumSize = new Size(_btnPageWidth, 34) };
            foreach (var btn in new[] { btnFirstPage, btnPrevPage, btnNextPage, btnLastPage })
            {
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(245, 247, 249);
                btn.BackColor = Color.FromArgb(230, 240, 255);
                btn.ForeColor = _themePrimaryColor;
                btn.TextAlign = ContentAlignment.MiddleCenter;
            }
            _lblPageInfo = new Label
            {
                Text = "第1页/共0页 总计0条",
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Font = _globalFont,
                TextAlign = ContentAlignment.MiddleRight
            };
            // 分页事件（完全保留原有逻辑，一字不变）
            btnFirstPage.Click += (s, e) => { _currentPageIndex = 1; LoadBloodSugarList(); };
            btnPrevPage.Click += (s, e) => { if (_currentPageIndex > 1) { _currentPageIndex--; LoadBloodSugarList(); } };
            btnNextPage.Click += (s, e) =>
            {
                int totalPage = GetTotalPage();
                if (_currentPageIndex < totalPage) { _currentPageIndex++; LoadBloodSugarList(); }
            };
            btnLastPage.Click += (s, e) =>
            {
                int totalPage = GetTotalPage();
                _currentPageIndex = totalPage > 0 ? totalPage : 1; LoadBloodSugarList();
            };

            pageButtonPanel.Controls.Add(btnFirstPage);
            pageButtonPanel.Controls.Add(btnPrevPage);
            pageButtonPanel.Controls.Add(btnNextPage);
            pageButtonPanel.Controls.Add(btnLastPage);

            pageLayout.Controls.Add(_lblPageInfo, 0, 0);
            pageLayout.Controls.Add(pageButtonPanel, 1, 0);
            pagePanel.Controls.Add(pageLayout);
            gridInnerPanel.Controls.Add(pagePanel, 0, 1);

            // 2.2 表格控件（固定在内层第1行，完整显示表头和数据）
            _dgvBloodSugar = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = true,
                ReadOnly = true,
                RowHeadersVisible = false,
                // ========== 表头100%显示核心修复，缺一不可 ==========
                ColumnHeadersVisible = true, // 强制显示表头
                                             // 🔥 核心修复2：禁用AutoSize，固定表头高度，彻底解决高度压缩问题
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 40, // 固定表头高度40px，绝对不会被压缩
                EnableHeadersVisualStyles = false, // 禁用系统样式，自定义样式100%生效
                                                   // ==================================================
                GridColor = Color.FromArgb(220, 220, 220),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                DefaultCellStyle = {
        Font = _globalFont,
        WrapMode = DataGridViewTriState.False,
        Alignment = DataGridViewContentAlignment.MiddleCenter,
        BackColor = Color.White,
        ForeColor = Color.Black,
        SelectionBackColor = _themePrimaryColor,
        SelectionForeColor = Color.White
    },
                // 表头高对比度样式，蓝底白字，绝对不会和背景融合
                ColumnHeadersDefaultCellStyle = {
        Font = new Font(_globalFont.FontFamily, _tableFontSize, FontStyle.Bold),
        BackColor = _themePrimaryColor,
        ForeColor = Color.White,
        Alignment = DataGridViewContentAlignment.MiddleCenter,
        WrapMode = DataGridViewTriState.False
    },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 30 }
            };
            // 列配置（完全保留原有配置，一字不变，确保数据绑定正常）
            _dgvBloodSugar.Columns.AddRange(new DataGridViewColumn[]
            {
    new DataGridViewTextBoxColumn {
        Name = "blood_sugar_id",
        HeaderText = "记录ID",
        DataPropertyName = "blood_sugar_id",
        Visible = false,
        SortMode = DataGridViewColumnSortMode.NotSortable
    },
    new DataGridViewTextBoxColumn {
        Name = "blood_sugar_value",
        HeaderText = "血糖值(mmol/L)",
        MinimumWidth = _colMinWidth_Value,
        DataPropertyName = "blood_sugar_value",
        SortMode = DataGridViewColumnSortMode.NotSortable,
        FillWeight = 18
    },
    new DataGridViewTextBoxColumn {
        Name = "measurement_scenario",
        HeaderText = "测量场景",
        MinimumWidth = _colMinWidth_Scenario,
        DataPropertyName = "measurement_scenario",
        SortMode = DataGridViewColumnSortMode.NotSortable,
        FillWeight = 17
    },
    new DataGridViewTextBoxColumn {
        Name = "measurement_time",
        HeaderText = "测量时间",
        MinimumWidth = _colMinWidth_Time,
        DataPropertyName = "measurement_time",
        DefaultCellStyle = { Format = "yyyy-MM-dd HH:mm" },
        SortMode = DataGridViewColumnSortMode.NotSortable,
        FillWeight = 25
    },
    new DataGridViewTextBoxColumn {
        Name = "is_abnormal",
        HeaderText = "状态",
        MinimumWidth = _colMinWidth_Status,
        DataPropertyName = "is_abnormal",
        SortMode = DataGridViewColumnSortMode.NotSortable,
        FillWeight = 10
    },
    new DataGridViewTextBoxColumn {
        Name = "data_source",
        HeaderText = "数据来源",
        MinimumWidth = _colMinWidth_Source,
        DataPropertyName = "data_source",
        SortMode = DataGridViewColumnSortMode.NotSortable,
        FillWeight = 15
    },
    new DataGridViewLinkColumn {
        Name = "operate",
        HeaderText = "操作",
        MinimumWidth = 160,
        Text = "编辑 | 删除",
        UseColumnTextForLinkValue = true,
        LinkColor = _themePrimaryColor,
        ActiveLinkColor = _themeDangerColor,
        VisitedLinkColor = _themePrimaryColor,
        SortMode = DataGridViewColumnSortMode.NotSortable,
        FillWeight = 15
    }
            });
            // 事件绑定（完全保留原有逻辑，一字不变）
            _dgvBloodSugar.CellContentClick += DgvBloodSugar_CellContentClick;
            _dgvBloodSugar.CellFormatting += DgvBloodSugar_CellFormatting;
            _dgvBloodSugar.DataError += DgvBloodSugar_DataError;
            gridInnerPanel.Controls.Add(_dgvBloodSugar, 0, 0);

            // 3. 把外层容器加入根布局
            _rootPanel.Controls.Add(gridOuterPanel, 0, 3);
            #endregion

            #region 5. 30天血糖趋势图区域（修复日期显示不全+旧版兼容）
            var chartPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0, 8, 0, 35), // 加大底部Padding至35，给日期标签留空间
                MinimumSize = new Size(0, 260)
            };
            // 图表标题（保留原有逻辑）
            var lblChartTitle = new Label
            {
                Text = "30天血糖趋势图",
                Font = new Font(_globalFont.FontFamily, _titleFontSize, FontStyle.Bold),
                ForeColor = _themePrimaryColor,
                Dock = DockStyle.Top,
                Height = 35,
                Padding = new Padding(0, 8, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };
            chartPanel.Controls.Add(lblChartTitle);

            // 图表控件（保留原有逻辑）
            _chartTrend = new Chart
            {
                Dock = DockStyle.Fill,
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High,
                Padding = new Padding(10, 5, 10, 15)
            };

            // 【核心修复】图表区域配置（旧版兼容+日期完整显示）
            var chartArea = new ChartArea("MainArea")
            {
                BackColor = Color.White,
                BorderColor = Color.FromArgb(220, 220, 220),
                InnerPlotPosition = new ElementPosition
                {
                    X = 12,
                    Y = 15,
                    Width = 85,
                    Height = 70 // 降低绘图区高度，给X轴留足空间
                },
                AxisX =
    {
        Title = "日期",
        TitleFont = new Font(_globalFont.FontFamily, 9f, FontStyle.Bold),
        TitleForeColor = Color.Black,
        LabelStyle = {
            Format = "MM-dd",
            Font = _globalFont,
            Angle = -45, // 倾斜45度，减少横向占用
            ForeColor = Color.Black
        },
        MajorGrid = { LineColor = Color.FromArgb(240, 240, 240) },
        Interval = 3, // 每3天显示一个日期，降低密度
        IsMarginVisible = true
    },
                AxisY =
    {
        Title = "血糖值(mmol/L)",
        TitleFont = new Font(_globalFont.FontFamily, 9f, FontStyle.Bold),
        TitleForeColor = Color.Black,
        LabelStyle = { Font = _globalFont, ForeColor = Color.Black },
        MajorGrid = { LineColor = Color.FromArgb(240, 240, 240) },
        Minimum = 3,
        Maximum = 15,
        Interval = 2,
        TextOrientation = TextOrientation.Rotated270,
        IsMarginVisible = true
    }
            };

            // 正常范围参考线（保留原有逻辑）
            var fastingLine = new StripLine
            {
                IntervalOffset = (double)FastingMin,
                StripWidth = (double)(FastingMax - FastingMin),
                BackColor = Color.FromArgb(30, _themeSuccessColor),
                Text = "空腹正常范围",
                TextAlignment = StringAlignment.Far,
                Font = new Font(_globalFont.FontFamily, 7f),
                ForeColor = _themeSecondaryColor,
                TextLineAlignment = StringAlignment.Center
            };
            var postprandialLine = new StripLine
            {
                IntervalOffset = (double)FastingMax,
                StripWidth = (double)(PostprandialMax - FastingMax),
                BackColor = Color.FromArgb(20, _themeSuccessColor),
                Text = "餐后正常范围",
                TextAlignment = StringAlignment.Far,
                Font = new Font(_globalFont.FontFamily, 7f),
                ForeColor = _themeSecondaryColor,
                TextLineAlignment = StringAlignment.Center
            };
            chartArea.AxisY.StripLines.Add(fastingLine);
            chartArea.AxisY.StripLines.Add(postprandialLine);
            _chartTrend.ChartAreas.Add(chartArea);

            // 图例+数据系列（保留原有逻辑）
            var legend = new Legend("MainLegend")
            {
                Docking = Docking.Top,
                Alignment = StringAlignment.Center,
                Font = new Font(_globalFont.FontFamily, 8f),
                BorderColor = Color.FromArgb(220, 220, 220),
                BackColor = Color.Transparent
            };
            _chartTrend.Legends.Add(legend);

            var seriesFasting = new Series("空腹血糖")
            {
                ChartType = SeriesChartType.Line,
                ChartArea = "MainArea",
                Color = _themePrimaryColor,
                BorderWidth = 2,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 5,
                MarkerColor = _themePrimaryColor,
                IsVisibleInLegend = true
            };
            var seriesPostprandial = new Series("餐后2小时血糖")
            {
                ChartType = SeriesChartType.Line,
                ChartArea = "MainArea",
                Color = _themeDangerColor,
                BorderWidth = 2,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 5,
                MarkerColor = _themeDangerColor,
                IsVisibleInLegend = true
            };
            _chartTrend.Series.Add(seriesFasting);
            _chartTrend.Series.Add(seriesPostprandial);

            chartPanel.Controls.Add(_chartTrend);
            _rootPanel.Controls.Add(chartPanel, 0, 4);
            #endregion

            // 最终渲染
            this.ResumeLayout(true);
            this.PerformLayout();
        }
        #endregion

        #region 工具方法
        private int GetTotalPage()
        {
            if (_totalCount <= 0 || _pageSize <= 0) return 1;
            return (int)Math.Ceiling((double)_totalCount / _pageSize);
        }
        #endregion

        #region 业务事件处理方法
        /// <summary>
        /// 保存录入按钮事件（修复时间校验+异常捕获）
        /// </summary>
        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (Program.LoginUser == null || Program.LoginUser.user_id <= 0)
                {
                    MessageBox.Show("用户登录信息异常，请重新登录", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 1. 血糖值校验（兼容11.0等有效数值）
                if (string.IsNullOrWhiteSpace(_txtSugarValue.Text.Trim()))
                {
                    MessageBox.Show("请输入血糖值", "输入提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtSugarValue.Focus();
                    return;
                }
                if (!decimal.TryParse(_txtSugarValue.Text.Trim(), out decimal sugarValue))
                {
                    MessageBox.Show("请输入有效的血糖值（数字格式）", "输入提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtSugarValue.Focus();
                    return;
                }
                if (sugarValue < SugarMinValid || sugarValue > SugarMaxValid)
                {
                    MessageBox.Show($"请输入{SugarMinValid}-{SugarMaxValid}之间的有效血糖值", "输入提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtSugarValue.Focus();
                    return;
                }

                // 2. 测量时间校验（核心修复：避免超出Min/Max范围）
                DateTime measureTime = _dtpMeasureTime.Value;
                if (measureTime < _dtpMeasureTime.MinDate || measureTime > _dtpMeasureTime.MaxDate)
                {
                    MessageBox.Show($"测量时间需在{_dtpMeasureTime.MinDate:yyyy-MM-dd}至{_dtpMeasureTime.MaxDate:yyyy-MM-dd}之间", "时间提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ResetMeasureTimePickerToNow();
                    return;
                }

                // 3. 构建实体
                var bs = new BloodSugar
                {
                    user_id = Program.LoginUser.user_id,
                    blood_sugar_value = sugarValue,
                    measurement_scenario = _cboScenario.SelectedItem?.ToString() ?? "空腹",
                    measurement_time = measureTime,
                    data_source = "手动录入",
                    operator_id = Program.LoginUser.user_id,
                    create_time = DateTime.Now,
                    update_time = DateTime.Now
                };

                // 4. 保存数据
                var result = bllBs.AddBloodSugar(bs);
                if (result.Success)
                {
                    MessageBox.Show(result.Msg, "操作成功", MessageBoxButtons.OK, result.Msg.Contains("异常") ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                    _txtSugarValue.Clear();
                    _cboScenario.SelectedIndex = 0;
                    ResetMeasureTimePickerToNow();
                    LoadAllData();
                }
                else
                {
                    MessageBox.Show(result.Msg, "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (ArgumentOutOfRangeException ex) when (ex.Message.Contains("MinDate") || ex.Message.Contains("MaxDate"))
            {
                // 捕获时间范围异常，友好提示
                MessageBox.Show("测量时间超出允许范围，请重新选择", "时间错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ResetMeasureTimePickerToNow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败：{ex.Message}", "系统异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetMeasureTimePickerToNow()
        {
            if (_dtpMeasureTime == null)
                return;
            DateTime now = DateTime.Now;
            if (now < _dtpMeasureTime.MinDate)
                now = _dtpMeasureTime.MinDate;

            _dtpMeasureTime.MaxDate = DateTime.Now;
            if (now > _dtpMeasureTime.MaxDate)
                now = _dtpMeasureTime.MaxDate;

            _dtpMeasureTime.Value = now;
        }

        private FlowLayoutPanel CreateQuickSugarPanel()
        {
            var quickPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                WrapContents = true,
                Margin = new Padding(5, 0, 5, 0),
                Padding = new Padding(0)
            };
            foreach (var value in new[] { "3.9", "5.6", "7.8", "11.1" })
            {
                quickPanel.Controls.Add(CreateQuickSugarButton(value));
            }
            return quickPanel;
        }

        private Button CreateQuickSugarButton(string value)
        {
            var button = new Button
            {
                Text = value,
                Width = 52,
                Height = 26,
                Margin = new Padding(0, 0, 6, 4),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(230, 240, 255),
                ForeColor = _themePrimaryColor,
                Font = new Font(_globalFont.FontFamily, 8.5f, FontStyle.Bold)
            };
            button.FlatAppearance.BorderColor = _themePrimaryColor;
            button.Click += (s, e) =>
            {
                _txtSugarValue.Text = value;
                _txtSugarValue.Focus();
                _txtSugarValue.SelectionStart = _txtSugarValue.TextLength;
            };
            return button;
        }

        /// <summary>
        /// 查询按钮事件
        /// </summary>
        private void BtnQuery_Click(object sender, EventArgs e)
        {
            _currentPageIndex = 1;
            LoadBloodSugarList();
        }

        /// <summary>
        /// 表格操作按钮事件（零报错修复版）
        /// </summary>
        private void DgvBloodSugar_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // 1. 先校验点击的是不是操作列
                if (e.RowIndex < 0 || e.ColumnIndex != _dgvBloodSugar.Columns["operate"].Index)
                    return;

                // 2. 校验用户登录状态
                if (Program.LoginUser == null || Program.LoginUser.user_id <= 0)
                {
                    MessageBox.Show("用户登录信息异常，请重新登录", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 3. 【核心容错】先判断ID列是否存在，再取值，彻底杜绝找不到列报错
                if (!_dgvBloodSugar.Columns.Contains("blood_sugar_id"))
                {
                    MessageBox.Show("表格配置异常，请重启页面", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var idCell = _dgvBloodSugar.Rows[e.RowIndex].Cells["blood_sugar_id"];
                if (idCell.Value == null || Convert.IsDBNull(idCell.Value) || !int.TryParse(idCell.Value.ToString(), out int recordId))
                {
                    MessageBox.Show("记录ID异常，无法操作", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                int userId = Program.LoginUser.user_id;

                // 4. 精准判断点击的是编辑还是删除（单元格内前半部分=编辑，后半部分=删除）
                Rectangle cellRect = _dgvBloodSugar.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                Point clientPoint = _dgvBloodSugar.PointToClient(System.Windows.Forms.Cursor.Position);
                int clickXInCell = clientPoint.X - cellRect.X;

                // 点击删除（单元格后半部分）
                if (clickXInCell > cellRect.Width / 2)
                {
                    if (MessageBox.Show("确定要删除这条血糖记录吗？删除后不可恢复", "删除确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        var result = bllBs.DeleteBloodSugar(recordId, userId);
                        if (result.Success)
                        {
                            MessageBox.Show(result.Msg, "操作成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadAllData();
                        }
                        else
                        {
                            MessageBox.Show(result.Msg, "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                // 点击编辑（单元格前半部分）
                else
                {
                    var result = bllBs.GetBloodSugarDetail(recordId, userId);
                    if (!result.Success || result.Data == null)
                    {
                        MessageBox.Show(result.Msg, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    var detail = result.Data;

                    using (var editForm = new Form())
                    {
                        editForm.Text = "编辑血糖记录";
                        editForm.StartPosition = FormStartPosition.CenterParent;
                        editForm.Width = 420;
                        editForm.Height = 380;
                        editForm.FormBorderStyle = FormBorderStyle.FixedSingle;
                        editForm.MaximizeBox = false;
                        editForm.MinimizeBox = false;
                        editForm.BackColor = Color.White;
                        editForm.Font = _globalFont;

                        var layout = new TableLayoutPanel
                        {
                            Dock = DockStyle.Fill,
                            RowCount = 5,
                            ColumnCount = 2,
                            Padding = new Padding(20),
                            GrowStyle = TableLayoutPanelGrowStyle.AddRows
                        };
                        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
                        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                        for (int i = 0; i < 5; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

                        // 血糖值输入框
                        var txtEditValue = new TextBox { Dock = DockStyle.Fill, Text = detail.blood_sugar_value.ToString("0.0"), Font = _globalFont };
                        layout.Controls.Add(new Label { Text = "血糖值(mmol/L)", TextAlign = ContentAlignment.MiddleLeft, Font = _globalFont }, 0, 0);
                        layout.Controls.Add(txtEditValue, 1, 0);

                        // 测量场景下拉框
                        var cboEditScenario = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = _globalFont };
                        cboEditScenario.Items.AddRange(new[] { "空腹", "餐后2小时", "餐前", "睡前", "随机" });
                        cboEditScenario.SelectedItem = detail.measurement_scenario ?? "空腹";
                        layout.Controls.Add(new Label { Text = "测量场景", TextAlign = ContentAlignment.MiddleLeft, Font = _globalFont }, 0, 1);
                        layout.Controls.Add(cboEditScenario, 1, 1);

                        // 测量时间选择框
                        var dtpEditTime = new DateTimePicker
                        {
                            Dock = DockStyle.Fill,
                            Format = DateTimePickerFormat.Custom,
                            CustomFormat = "yyyy-MM-dd HH:mm",
                            Value = detail.measurement_time ?? DateTime.Now,
                            MinDate = new DateTime(1900, 1, 1),
                            MaxDate = DateTime.Now,
                            Font = _globalFont
                        };
                        layout.Controls.Add(new Label { Text = "测量时间", TextAlign = ContentAlignment.MiddleLeft, Font = _globalFont }, 0, 2);
                        layout.Controls.Add(dtpEditTime, 1, 2);

                        // 异常备注
                        var txtEditRemark = new TextBox { Dock = DockStyle.Fill, Text = detail.abnormal_note ?? string.Empty, Font = _globalFont, Multiline = true, Height = 50 };
                        layout.Controls.Add(new Label { Text = "异常备注", TextAlign = ContentAlignment.MiddleLeft, Font = _globalFont }, 0, 3);
                        layout.Controls.Add(txtEditRemark, 1, 3);

                        // 按钮区
                        var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 10, 0, 0) };
                        var btnSaveEdit = new Button { Text = "保存", Width = 90, Height = 35, Font = _globalFont, BackColor = _themePrimaryColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                        var btnCancelEdit = new Button { Text = "取消", Width = 90, Height = 35, Font = _globalFont, BackColor = _themeSecondaryColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                        btnSaveEdit.FlatAppearance.BorderSize = 0;
                        btnCancelEdit.FlatAppearance.BorderSize = 0;

                        btnSaveEdit.Click += (s, ev) =>
                        {
                            try
                            {
                                if (!decimal.TryParse(txtEditValue.Text.Trim(), out decimal newValue))
                                {
                                    MessageBox.Show("请输入有效的血糖值", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                                if (newValue < 0.5m || newValue > 30.0m)
                                {
                                    MessageBox.Show("请输入0.5-30.0之间的有效血糖值", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                detail.blood_sugar_value = newValue;
                                detail.measurement_scenario = cboEditScenario.SelectedItem?.ToString() ?? "空腹";
                                detail.measurement_time = dtpEditTime.Value;
                                detail.abnormal_note = txtEditRemark.Text.Trim();

                                var updateResult = bllBs.UpdateBloodSugar(detail);
                                if (updateResult.Success)
                                {
                                    MessageBox.Show(updateResult.Msg, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    editForm.DialogResult = DialogResult.OK;
                                    editForm.Close();
                                }
                                else
                                {
                                    MessageBox.Show(updateResult.Msg, "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"编辑失败：{ex.Message}", "系统异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        };
                        btnCancelEdit.Click += (s, ev) => { editForm.Close(); };

                        btnPanel.Controls.Add(btnCancelEdit);
                        btnPanel.Controls.Add(btnSaveEdit);
                        layout.Controls.Add(btnPanel, 1, 4);
                        editForm.Controls.Add(layout);

                        if (editForm.ShowDialog() == DialogResult.OK)
                        {
                            LoadAllData();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"操作失败：{ex.Message}", "系统异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void DgvBloodSugar_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                // 1. 状态列显示处理（保留原有逻辑）
                if (_dgvBloodSugar.Columns[e.ColumnIndex].Name == "is_abnormal" && e.RowIndex >= 0)
                {
                    if (e.Value != null && !Convert.IsDBNull(e.Value))
                    {
                        int isAbnormal = Convert.ToInt32(e.Value);
                        e.Value = isAbnormal == 1 ? "异常" : "正常";
                        e.FormattingApplied = true;

                        var row = _dgvBloodSugar.Rows[e.RowIndex];
                        row.DefaultCellStyle.ForeColor = isAbnormal == 1 ? _themeDangerColor : Color.Black;
                        row.DefaultCellStyle.BackColor = isAbnormal == 1 ? Color.FromArgb(255, 245, 245) : Color.White;
                        row.DefaultCellStyle.SelectionForeColor = isAbnormal == 1 ? _themeDangerColor : Color.White;
                    }
                }

                // 2. 操作列：选中时强制链接颜色为白色（新增修复）
                if (_dgvBloodSugar.Columns[e.ColumnIndex].Name == "operate" && e.RowIndex >= 0)
                {
                    var linkCell = _dgvBloodSugar[e.ColumnIndex, e.RowIndex] as DataGridViewLinkCell;
                    if (linkCell == null) return;

                    if (linkCell.Selected)
                    {
                        linkCell.LinkColor = Color.White;
                        linkCell.ActiveLinkColor = Color.White;
                        linkCell.VisitedLinkColor = Color.White;
                    }
                    else
                    {
                        linkCell.LinkColor = _themePrimaryColor;
                        linkCell.ActiveLinkColor = _themeDangerColor;
                        linkCell.VisitedLinkColor = _themePrimaryColor;
                    }
                }
            }
            catch { }
        }

        #region 拦截表格数据绑定错误，避免弹窗
        private void DgvBloodSugar_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
        }
        #endregion
        #endregion

        #region 数据加载方法
        /// <summary>
        /// 加载所有数据
        /// </summary>
        private void LoadAllData()
        {
            if (Program.LoginUser == null || Program.LoginUser.user_id <= 0) return;
            if (_dgvBloodSugar == null || _chartTrend == null || _lblPageInfo == null) return;

            LoadStatsData();
            LoadBloodSugarList();
            LoadTrendChartData();
        }

        /// <summary>
        /// 加载统计卡片数据
        /// </summary>
        /// <summary>
        /// 加载统计卡片数据（修复后）
        /// </summary>
        private void LoadStatsData()
        {
            try
            {
                if (Program.LoginUser == null || Program.LoginUser.user_id <= 0) return;
                int userId = Program.LoginUser.user_id;
                Patient patient = BLL.B_Patient.GetPatientById(userId);
                // 修复：明确查询今日0点到当前时间的数据
                string sql = @"
SELECT 
ISNULL(AVG(blood_sugar_value),0) as avg_value,
ISNULL(MAX(blood_sugar_value),0) as max_value,
ISNULL(MIN(blood_sugar_value),0) as min_value,
ISNULL(SUM(is_abnormal),0) as abnormal_count
FROM t_blood_sugar 
WHERE user_id = @user_id 
AND measurement_time >= @todayStart 
AND measurement_time <= @now;
";
                SqlParameter[] parameters = {
            new SqlParameter("@user_id", userId),
            new SqlParameter("@todayStart", DateTime.Now.Date),
            new SqlParameter("@now", DateTime.Now)
        };

                DataTable dt = SqlHelper.ExecuteDataTable(sql, parameters);
                if (dt == null || dt.Rows.Count == 0) return;
                DataRow dr = dt.Rows[0];

                void UpdateLabel(string labelName, string text, Color? foreColor = null)
                {
                    var lbl = this.Controls.Find($"lblStats_{labelName}", true).FirstOrDefault() as Label;
                    if (lbl == null || lbl.IsDisposed) return;

                    if (lbl.InvokeRequired)
                    {
                        lbl.Invoke(new Action(() =>
                        {
                            lbl.Text = text;
                            if (foreColor.HasValue) lbl.ForeColor = foreColor.Value;
                        }));
                    }
                    else
                    {
                        lbl.Text = text;
                        if (foreColor.HasValue) lbl.ForeColor = foreColor.Value;
                    }
                }

                UpdateLabel("avg", dr["avg_value"] != DBNull.Value ? Convert.ToDecimal(dr["avg_value"]).ToString("0.0") : "0");
                UpdateLabel("max", dr["max_value"] != DBNull.Value ? Convert.ToDecimal(dr["max_value"]).ToString("0.0") : "0");
                UpdateLabel("min", dr["min_value"] != DBNull.Value ? Convert.ToDecimal(dr["min_value"]).ToString("0.0") : "0");
                int abnormalCount = dr["abnormal_count"] != DBNull.Value ? Convert.ToInt32(dr["abnormal_count"]) : 0;
                UpdateLabel("abnormal", abnormalCount.ToString(), abnormalCount > 0 ? _themeDangerColor : _themeSuccessColor);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"统计数据加载失败：{ex.Message}", "系统异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 加载血糖记录列表（表头不显示终极修复版）
        /// </summary>
        /// <summary>
        /// 加载血糖记录列表（表头强制重绘修复版）
        /// </summary>
        private void LoadBloodSugarList()
        {
            try
            {
                if (Program.LoginUser == null || Program.LoginUser.user_id <= 0) return;
                int userId = Program.LoginUser.user_id;
                DateTime? startTime = _dtpFilterStart?.Value.Date;
                DateTime? endTime = _dtpFilterEnd?.Value.Date.AddDays(1).AddSeconds(-1);
                string scenario = _cboFilterScenario?.SelectedItem?.ToString() ?? "全部";

                // 获取数据源
                var list = bllBs.GetBloodSugarPageList(userId, _currentPageIndex, _pageSize, out _totalCount, startTime, endTime, scenario) ?? new List<BloodSugar>();

                // 【核心修复：数据绑定+表头强制重绘，顺序绝对不能改】
                void BindData()
                {
                    // 1. 先清空数据源，断开绑定
                    _dgvBloodSugar.DataSource = null;
                    // 2. 再次强制锁死自动生成列+表头显示，防止系统篡改
                    _dgvBloodSugar.AutoGenerateColumns = false;
                    _dgvBloodSugar.ColumnHeadersVisible = true;
                    // 3. 绑定数据源
                    _dgvBloodSugar.DataSource = list;
                    // 4. 🔥 强制重绘表头，彻底解决渲染失败问题
                    //_dgvBloodSugar.InvalidateColumnHeaders();
                    // 5. 全局刷新布局
                    _dgvBloodSugar.Invalidate();
                    _dgvBloodSugar.Update();
                }

                // 跨线程处理
                if (_dgvBloodSugar.InvokeRequired)
                {
                    _dgvBloodSugar.Invoke(new Action(BindData));
                }
                else
                {
                    BindData();
                }

                // 分页信息更新（原有逻辑完全不变）
                int totalPage = GetTotalPage();
                _currentPageIndex = _currentPageIndex < 1 ? 1 : (_currentPageIndex > totalPage ? totalPage : _currentPageIndex);
                string pageText = $"第{_currentPageIndex}页/共{totalPage}页 总计{_totalCount}条";
                if (_lblPageInfo.InvokeRequired)
                {
                    _lblPageInfo.Invoke(new Action(() => { _lblPageInfo.Text = pageText; }));
                }
                else
                {
                    _lblPageInfo.Text = pageText;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载数据失败：{ex.Message}", "系统异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// 加载趋势图数据
        /// </summary>
        /// <summary>
        /// 加载趋势图数据（修复后，显示近30天完整数据）
        /// </summary>
        private void LoadTrendChartData()
        {
            try
            {
                if (Program.LoginUser == null || Program.LoginUser.user_id <= 0) return;
                int userId = Program.LoginUser.user_id;

                // 修复：查询近30天的数据，包含最新记录
                DateTime startDate = DateTime.Now.Date.AddDays(-30);
                DateTime endDate = DateTime.Now.Date.AddDays(1).AddSeconds(-1);

                string sql = @"
                SELECT 
                CONVERT(date, measurement_time) as record_date,
                measurement_scenario,
                AVG(blood_sugar_value) as avg_value
                FROM t_blood_sugar 
                WHERE user_id = @user_id 
                AND measurement_time >= @startDate 
                AND measurement_time <= @endDate
                AND measurement_scenario IN ('空腹','餐后2小时')
                GROUP BY CONVERT(date, measurement_time), measurement_scenario
                ORDER BY record_date;
                ";
                SqlParameter[] parameters = {
            new SqlParameter("@user_id", userId),
            new SqlParameter("@startDate", startDate),
            new SqlParameter("@endDate", endDate)
        };

                DataTable dt = SqlHelper.ExecuteDataTable(sql, parameters);

                // 清空原有数据
                if (_chartTrend.InvokeRequired)
                {
                    _chartTrend.Invoke(new Action(() =>
                    {
                        _chartTrend.Series["空腹血糖"].Points.Clear();
                        _chartTrend.Series["餐后2小时血糖"].Points.Clear();
                    }));
                }
                else
                {
                    _chartTrend.Series["空腹血糖"].Points.Clear();
                    _chartTrend.Series["餐后2小时血糖"].Points.Clear();
                }

                // 无数据处理
                if (dt == null || dt.Rows.Count == 0)
                {
                    if (_chartTrend.InvokeRequired)
                    {
                        _chartTrend.Invoke(new Action(() =>
                        {
                            _chartTrend.Series["空腹血糖"].Points.AddXY(DateTime.Now, 0);
                            _chartTrend.Series["餐后2小时血糖"].Points.AddXY(DateTime.Now, 0);
                            _chartTrend.Series["空腹血糖"].MarkerStyle = MarkerStyle.None;
                            _chartTrend.Series["餐后2小时血糖"].MarkerStyle = MarkerStyle.None;
                            // 修复：X轴范围适配近30天
                            _chartTrend.ChartAreas["MainArea"].AxisX.Minimum = DateTime.Now.AddDays(-30).ToOADate();
                            _chartTrend.ChartAreas["MainArea"].AxisX.Maximum = DateTime.Now.ToOADate();
                        }));
                    }
                    else
                    {
                        _chartTrend.Series["空腹血糖"].Points.AddXY(DateTime.Now, 0);
                        _chartTrend.Series["餐后2小时血糖"].Points.AddXY(DateTime.Now, 0);
                        _chartTrend.Series["空腹血糖"].MarkerStyle = MarkerStyle.None;
                        _chartTrend.Series["餐后2小时血糖"].MarkerStyle = MarkerStyle.None;
                        _chartTrend.ChartAreas["MainArea"].AxisX.Minimum = DateTime.Now.AddDays(-30).ToOADate();
                        _chartTrend.ChartAreas["MainArea"].AxisX.Maximum = DateTime.Now.ToOADate();
                    }
                    return;
                }

                // 绑定数据
                foreach (DataRow dr in dt.Rows)
                {
                    if (Convert.IsDBNull(dr["record_date"]) || Convert.IsDBNull(dr["avg_value"]) || Convert.IsDBNull(dr["measurement_scenario"]))
                        continue;

                    DateTime date = Convert.ToDateTime(dr["record_date"]).Date;
                    decimal value = Convert.ToDecimal(dr["avg_value"]);
                    string scenario = dr["measurement_scenario"].ToString();
                    double chartValue = Convert.ToDouble(value);

                    if (_chartTrend.InvokeRequired)
                    {
                        _chartTrend.Invoke(new Action(() =>
                        {
                            if (scenario == "空腹")
                            {
                                _chartTrend.Series["空腹血糖"].Points.AddXY(date, chartValue);
                            }
                            else if (scenario == "餐后2小时")
                            {
                                _chartTrend.Series["餐后2小时血糖"].Points.AddXY(date, chartValue);
                            }
                        }));
                    }
                    else
                    {
                        if (scenario == "空腹")
                        {
                            _chartTrend.Series["空腹血糖"].Points.AddXY(date, chartValue);
                        }
                        else if (scenario == "餐后2小时")
                        {
                            _chartTrend.Series["餐后2小时血糖"].Points.AddXY(date, chartValue);
                        }
                    }
                }

                // 修复：X轴范围自动适配近30天
                if (_chartTrend.InvokeRequired)
                {
                    _chartTrend.Invoke(new Action(() =>
                    {
                        _chartTrend.ChartAreas["MainArea"].AxisX.Minimum = DateTime.Now.AddDays(-30).ToOADate();
                        _chartTrend.ChartAreas["MainArea"].AxisX.Maximum = DateTime.Now.ToOADate();
                        _chartTrend.Series["空腹血糖"].MarkerStyle = MarkerStyle.Circle;
                        _chartTrend.Series["餐后2小时血糖"].MarkerStyle = MarkerStyle.Circle;
                    }));
                }
                else
                {
                    _chartTrend.ChartAreas["MainArea"].AxisX.Minimum = DateTime.Now.AddDays(-30).ToOADate();
                    _chartTrend.ChartAreas["MainArea"].AxisX.Maximum = DateTime.Now.ToOADate();
                    _chartTrend.Series["空腹血糖"].MarkerStyle = MarkerStyle.Circle;
                    _chartTrend.Series["餐后2小时血糖"].MarkerStyle = MarkerStyle.Circle;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"趋势图加载失败：{ex.Message}", "系统异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}