using BLL;
using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace PatientUI
{
    public partial class FrmExerciseManage : Form
    {
        // ==============================================
        // 【原有页面参数 完全保留 不做任何改动】
        // ==============================================
        /// <summary>页面顶部留白（整体下移像素）</summary>
        private readonly int _pageTopMargin = 100;
        /// <summary>页面左侧留白（整体右移像素）</summary>
        private readonly int _pageLeftMargin = 250;
        /// <summary>页面右侧留白</summary>
        private readonly int _pageRightMargin = 20;
        /// <summary>页面底部留白</summary>
        private readonly int _pageBottomMargin = 20;
        /// <summary>运动录入区高度</summary>
        private readonly int _inputAreaHeight = 160;
        /// <summary>干预方案区高度</summary>
        private readonly int _planAreaHeight = 160;
        /// <summary>运动记录表格区高度</summary>
        private readonly int _gridAreaHeight = 420;
        /// <summary>表格相对表格区顶部下移像素</summary>
        private readonly int _tableTopOffset = 40;
        /// <summary>表格相对表格区左侧右移像素</summary>
        private readonly int _tableLeftOffset = 0;
        /// <summary>列宽统一缩放因子（0.1~1.0）</summary>
        private readonly float _columnWidthScale = 0.85f;
        /// <summary>表格列原始宽度</summary>
        private readonly int _colWidth_Type = 12;    // 运动类型
        private readonly int _colWidth_Duration = 8;// 运动时长
        private readonly int _colWidth_Intensity = 10;// 运动强度
        private readonly int _colWidth_Time = 16;   // 运动时间
        private readonly int _colWidth_Source = 8;  // 数据来源
        private readonly int _colWidth_CreateTime = 16;// 录入时间
        /// <summary>输入框/下拉框外边距</summary>
        private readonly Padding _controlMargin = new Padding(5);
        /// <summary>按钮外边距</summary>
        private readonly Padding _btnMargin = new Padding(3);
        // ==============================================
        // 原有控件 完全保留 不做任何属性改动
        // ==============================================
        private ComboBox _cboExerciseType;    // 运动类型
        private TextBox _txtDuration;         // 运动时长
        private ComboBox _cboIntensity;       // 运动强度
        private DateTimePicker _dtpExerciseTime;// 运动时间
        private DataGridView _dgvExercise;    // 运动记录表格
        private RichTextBox _txtPlan;         // 干预方案文本
        private List<ExerciseRecord> _exerciseList;// 运动记录集合
        // ==============================================
        // 新增业务控件与全局对象
        // ==============================================
        private readonly B_Exercise _bll = new B_Exercise();
        private readonly B_BloodSugar _bllBloodSugar = new B_BloodSugar();
        // 适配新拆分架构：获取登录用户ID
        private int _currentUserId => Program.LoginUser?.user_id ?? 0;
        private Chart _chartExerciseTrend;    // 30天运动趋势图控件
        private SpeechSynthesizer _speechSynthesizer;
        // 筛选控件
        private DateTimePicker _dtpStartDate;
        private DateTimePicker _dtpEndDate;
        private ComboBox _cboFilterType;
        private ComboBox _cboFilterIntensity;
        private Button _btnQuery;
        private readonly int _pageSize = 10;
        private int _currentPageIndex = 1;
        private Label _lblPageInfo;
        private List<Exercise> _exerciseSourceList = new List<Exercise>();
        // 运动记录实体（内部使用，无需数据库）
        public class ExerciseRecord
        {
            public int exercise_id { get; set; }
            public int user_id { get; set; }
            public string exercise_type { get; set; }
            public string duration { get; set; }
            public string intensity { get; set; }
            public DateTime exercise_time { get; set; }
            public string data_source { get; set; }
            public DateTime create_time { get; set; }
        }

        public FrmExerciseManage()
        {
            _exerciseList = new List<ExerciseRecord>();
            // 子窗体基础配置 完全保留 不做改动
            this.TopLevel = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.Font = new Font("微软雅黑", 9F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.DoubleBuffered = true;
            // 显示时重建布局+加载全量数据
            this.VisibleChanged += (s, e) => {
                if (Visible)
                {
                    BuildLayout();
                    LoadAllUserData();
                }
            };
            this.Disposed += (s, e) => _speechSynthesizer?.Dispose();
        }

        // ==============================================
        // 原有水印方法 完全保留 不做改动
        // ==============================================
        private void SetWatermark(TextBox txt, string watermark)
        {
            txt.Text = watermark;
            txt.ForeColor = Color.Gray;
            txt.GotFocus += (s, e) =>
            {
                if (txt.Text == watermark) { txt.Text = ""; txt.ForeColor = Color.Black; }
            };
            txt.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txt.Text)) { txt.Text = watermark; txt.ForeColor = Color.Gray; }
            };
        }

        // ==============================================
        // 核心布局生成 原有控件完全保留，新增筛选/图表控件
        // ==============================================
        private void BuildLayout()
        {
            Controls.Clear();
            SuspendLayout();
            var scrollContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true
            };
            // 根容器 完全保留原有配置
            var rootPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                RowCount = 4,
                ColumnCount = 1,
                BackColor = Color.White,
                Padding = new Padding(_pageLeftMargin, _pageTopMargin, _pageRightMargin, _pageBottomMargin),
                GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            // 行高分配 完全保留原有配置
            rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, _inputAreaHeight));
            rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, _planAreaHeight));
            rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, _gridAreaHeight));
            rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 450));
            rootPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            // 添加区域 原有区域完全保留
            rootPanel.Controls.Add(CreateInputPanel(), 0, 0);
            rootPanel.Controls.Add(CreatePlanPanel(), 0, 1);
            rootPanel.Controls.Add(CreateGridPanel(), 0, 2);
            rootPanel.Controls.Add(CreateChartPanel(), 0, 3);
            scrollContainer.Controls.Add(rootPanel);
            Controls.Add(scrollContainer);
            ResumeLayout(true);
            PerformLayout();
        }

        // ==============================================
        // 1. 运动记录录入区 原有控件/属性完全保留，仅完善事件逻辑
        // ==============================================
        private Panel CreateInputPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 4
            };
            // 4列均分 完全保留
            for (int i = 0; i < 4; i++)
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            // 运动类型 原有控件完全保留
            _cboExerciseType = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Margin = _controlMargin };
            _cboExerciseType.Items.AddRange(new[] { "快走", "慢跑", "太极拳", "游泳", "其他" });
            _cboExerciseType.SelectedIndex = 0;
            layout.Controls.Add(new Label { Text = "运动类型", Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft }, 0, 0);
            layout.Controls.Add(_cboExerciseType, 0, 1);
            // 运动时长 原有控件完全保留
            _txtDuration = new TextBox { Dock = DockStyle.Fill, Margin = _controlMargin };
            SetWatermark(_txtDuration, "例：30分钟");
            layout.Controls.Add(new Label { Text = "运动时长", Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft }, 1, 0);
            layout.Controls.Add(_txtDuration, 1, 1);
            // 运动强度 原有控件完全保留
            _cboIntensity = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Margin = _controlMargin };
            _cboIntensity.Items.AddRange(new[] { "低强度", "中强度", "高强度" });
            _cboIntensity.SelectedIndex = 1;
            layout.Controls.Add(new Label { Text = "运动强度", Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft }, 2, 0);
            layout.Controls.Add(_cboIntensity, 2, 1);
            // 运动时间 原有控件完全保留
            _dtpExerciseTime = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm", Margin = _controlMargin };
            layout.Controls.Add(new Label { Text = "运动时间", Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft }, 3, 0);
            layout.Controls.Add(_dtpExerciseTime, 3, 1);
            // 按钮区 原有按钮完全保留，属性不改动
            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                AutoSize = false,
                Height = 44,
                Margin = new Padding(5),
                Padding = new Padding(0, 4, 0, 4),
                WrapContents = false
            };
            var btnSave = CreateBtn("保存记录", Color.FromArgb(0, 122, 204));
            var btnClear = CreateBtn("清空", Color.FromArgb(108, 117, 125));
            var btnImport = CreateBtn("模拟导入", Color.FromArgb(40, 167, 69));
            var btnVoice = CreateBtn("语音播报", Color.FromArgb(111, 66, 193));
            btnSave.Click += BtnSave_Click;
            btnClear.Click += BtnClear_Click;
            btnImport.Click += BtnImport_Click;
            btnVoice.Click += (s, e) => SpeakExercisePlan();
            btnPanel.Controls.Add(btnSave);
            btnPanel.Controls.Add(btnClear);
            btnPanel.Controls.Add(btnImport);
            btnPanel.Controls.Add(btnVoice);
            panel.Controls.Add(layout);
            panel.Controls.Add(btnPanel);
            return panel;
        }

        // ==============================================
        // 2. 糖尿病运动干预方案区 支持个性化动态渲染+异常标红
        // ==============================================
        private Panel CreatePlanPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };
            var lblTitle = new Label
            {
                Text = "糖尿病专属运动干预方案",
                Font = new Font("微软雅黑", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Dock = DockStyle.Top,
                Height = 30
            };
            // 仅改为RichTextBox支持富文本标红，其余原有属性完全保留
            _txtPlan = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Margin = _controlMargin,
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(248, 250, 252),
                Font = new Font("微软雅黑", 9F)
            };
            panel.Controls.Add(_txtPlan);
            panel.Controls.Add(lblTitle);
            return panel;
        }

        // ==============================================
        // 3. 运动历史记录表格区 新增筛选、操作列、排序功能
        // ==============================================
        private Panel CreateGridPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0, 10, 0, 18)
            };
            // 标题
            var lblTitle = new Label
            {
                Text = "运动历史记录",
                Font = new Font("微软雅黑", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Dock = DockStyle.Top,
                Height = 36
            };
            var pagePanel = CreatePagerPanel();
            // 新增筛选区面板
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42,
                BackColor = Color.White
            };
            // 筛选控件初始化
            _dtpStartDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 100, Location = new Point(80, 5) };
            _dtpEndDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 100, Location = new Point(220, 5) };
            _cboFilterType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100, Location = new Point(330, 5) };
            _cboFilterIntensity = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100, Location = new Point(550, 5) };
            _btnQuery = CreateBtn("查询", Color.FromArgb(0, 122, 204));
            _btnQuery.Location = new Point(770, 2);
            _btnQuery.Width = 80;
            _btnQuery.Height = 28;
            // 筛选控件赋值
            _cboFilterType.Items.AddRange(new[] { "全部", "快走", "慢跑", "太极拳", "游泳", "其他" });
            _cboFilterType.SelectedIndex = 0;
            _cboFilterIntensity.Items.AddRange(new[] { "全部", "低强度", "中强度", "高强度" });
            _cboFilterIntensity.SelectedIndex = 0;
            _dtpStartDate.Value = DateTime.Now.AddMonths(-1);
            _dtpEndDate.Value = DateTime.Now;
            _btnQuery.Click += BtnQueryFilter_Click;
            // 筛选控件加入面板
            filterPanel.Controls.Add(new Label { Text = "日期范围：", Location = new Point(0, 8), AutoSize = true });
            filterPanel.Controls.Add(_dtpStartDate);
            filterPanel.Controls.Add(new Label { Text = "至", Location = new Point(110, 8), AutoSize = true });
            filterPanel.Controls.Add(_dtpEndDate);
            filterPanel.Controls.Add(new Label { Text = "运动类型：", Location = new Point(440, 8), AutoSize = true });
            filterPanel.Controls.Add(_cboFilterType);
            filterPanel.Controls.Add(new Label { Text = "运动强度：", Location = new Point(660, 8), AutoSize = true });
            filterPanel.Controls.Add(_cboFilterIntensity);
            filterPanel.Controls.Add(_btnQuery);
            // 原有表格控件 核心属性完全保留，新增列配置
            _dgvExercise = new DataGridView
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(_tableLeftOffset, _tableTopOffset, 0, 0),
                BackgroundColor = Color.White,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                ColumnHeadersVisible = true,
                GridColor = Color.FromArgb(220, 220, 220),
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 40,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            // 列绑定 原有列完全保留，新增操作列
            _dgvExercise.Columns.AddRange(new DataGridViewColumn[]
             {
                new DataGridViewTextBoxColumn { Name = "exercise_id", HeaderText = "记录ID", DataPropertyName = "exercise_id", Visible = false },
                new DataGridViewTextBoxColumn { Name = "exercise_type", HeaderText = "运动类型", DataPropertyName = "exercise_type", Width = (int)(_colWidth_Type * _columnWidthScale), SortMode = DataGridViewColumnSortMode.Automatic },
                new DataGridViewTextBoxColumn { Name = "duration", HeaderText = "运动时长", DataPropertyName = "duration", Width = (int)(_colWidth_Duration * _columnWidthScale), SortMode = DataGridViewColumnSortMode.Automatic },
                new DataGridViewTextBoxColumn { Name = "intensity", HeaderText = "运动强度", DataPropertyName = "intensity", Width = (int)(_colWidth_Intensity * _columnWidthScale), SortMode = DataGridViewColumnSortMode.Automatic },
                new DataGridViewTextBoxColumn { Name = "exercise_time", HeaderText = "运动时间", DataPropertyName = "exercise_time", Width = (int)(_colWidth_Time * _columnWidthScale), SortMode = DataGridViewColumnSortMode.Automatic },
                new DataGridViewTextBoxColumn { Name = "data_source", HeaderText = "数据来源", DataPropertyName = "data_source", Width = (int)(_colWidth_Source * _columnWidthScale), SortMode = DataGridViewColumnSortMode.Automatic },
                new DataGridViewTextBoxColumn { Name = "create_time", HeaderText = "录入时间", DataPropertyName = "create_time", Width = (int)(_colWidth_CreateTime * _columnWidthScale), SortMode = DataGridViewColumnSortMode.Automatic },
                new DataGridViewButtonColumn { Name = "btnEdit", HeaderText = "操作", Text = "编辑", UseColumnTextForButtonValue = true, Width = 60 },
                new DataGridViewButtonColumn { Name = "btnDelete", HeaderText = "", Text = "删除", UseColumnTextForButtonValue = true, Width = 60 }
             });
            // 原有样式完全保留
            _dgvExercise.DefaultCellStyle.Font = new Font("微软雅黑", 9F);
            _dgvExercise.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", 9F, FontStyle.Bold);
            _dgvExercise.RowTemplate.Height = 34;
            _dgvExercise.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            // 绑定编辑删除事件
            _dgvExercise.CellContentClick += DgvExercise_CellContentClick;
            // 控件加入面板
            panel.Controls.Add(_dgvExercise);
            panel.Controls.Add(pagePanel);
            panel.Controls.Add(filterPanel);
            panel.Controls.Add(lblTitle);
            return panel;
        }

        private Panel CreatePagerPanel()
        {
            var pagePanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = Color.White,
                Padding = new Padding(8, 10, 8, 10)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320f));

            _lblPageInfo = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = this.Font,
                Text = "第1页/共1页 总计0条"
            };

            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            Button CreatePageBtn(string text, Action onClick)
            {
                var btn = new Button
                {
                    Text = text,
                    Width = 62,
                    Height = 34,
                    MinimumSize = new Size(62, 34),
                    Margin = new Padding(2, 0, 2, 0),
                    FlatStyle = FlatStyle.Standard
                };
                btn.Click += (s, e) => onClick();
                return btn;
            }

            btnPanel.Controls.Add(CreatePageBtn("首页", () => { _currentPageIndex = 1; BindCurrentExercisePage(); }));
            btnPanel.Controls.Add(CreatePageBtn("上一页", () => { _currentPageIndex--; BindCurrentExercisePage(); }));
            btnPanel.Controls.Add(CreatePageBtn("下一页", () => { _currentPageIndex++; BindCurrentExercisePage(); }));
            btnPanel.Controls.Add(CreatePageBtn("末页", () => { _currentPageIndex = GetExerciseTotalPage(); BindCurrentExercisePage(); }));

            layout.Controls.Add(_lblPageInfo, 0, 0);
            layout.Controls.Add(btnPanel, 1, 0);
            pagePanel.Controls.Add(layout);
            return pagePanel;
        }

        // ==============================================
        // 4. 30天运动时长趋势图 完整实现折线图+悬浮提示
        // ==============================================
        private Panel CreateChartPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(0, 0, 0, 16), MinimumSize = new Size(0, 260) };
            // 标题 完全保留原有配置
            panel.Controls.Add(new Label
            {
                Text = "30天运动时长趋势",
                Font = new Font("微软雅黑", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Dock = DockStyle.Top,
                Height = 36
            });
            // 初始化图表控件
            _chartExerciseTrend = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10, 6, 10, 20),
                Palette = ChartColorPalette.Pastel
            };
            // 图表区域配置
            var chartArea = new ChartArea("MainArea");
            chartArea.AxisX.Title = "日期";
            chartArea.AxisX.LabelStyle.Format = "MM-dd";
            chartArea.AxisX.LabelStyle.Angle = -30;
            chartArea.AxisX.Interval = 2;
            chartArea.AxisX.IsLabelAutoFit = true;
            chartArea.AxisY.Title = "运动时长(分钟)";
            chartArea.AxisY.Minimum = 0;
            chartArea.BackColor = Color.White;
            _chartExerciseTrend.ChartAreas.Add(chartArea);
            // 折线图系列配置
            var series = new Series("每日运动时长")
            {
                ChartType = SeriesChartType.Line,
                XValueType = ChartValueType.Date,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 8,
                BorderWidth = 3,
                Color = Color.FromArgb(0, 122, 204)
            };
            _chartExerciseTrend.Series.Add(series);
            // 图例配置
            var legend = new Legend();
            legend.Font = new Font("微软雅黑", 9F);
            _chartExerciseTrend.Legends.Add(legend);
            // 鼠标悬浮提示事件
            _chartExerciseTrend.GetToolTipText += ChartExerciseTrend_GetToolTipText;
            panel.Controls.Add(_chartExerciseTrend);
            return panel;
        }

        // ==============================================
        // 原有通用按钮创建方法 完全保留 不做改动
        // ==============================================
        private Button CreateBtn(string text, Color color)
        {
            return new Button
            {
                Text = text,
                Width = 110,
                Height = 33,
                Margin = _btnMargin,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
        }

        // ==============================================
        // 核心业务事件实现
        // ==============================================
        #region 1. 保存运动记录（含完整表单校验）
        private void BtnSave_Click(object sender, EventArgs e)
        {
            // 登录状态校验
            if (_currentUserId <= 0)
            {
                MessageBox.Show("用户登录状态异常，请重新登录！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!ValidateExerciseSafety())
            {
                return;
            }
            #region 表单校验
            if (string.IsNullOrWhiteSpace(_txtDuration.Text) || _txtDuration.Text == "例：30分钟")
            {
                MessageBox.Show("请输入运动时长！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(_txtDuration.Text.Replace("分钟", "").Trim(), out int duration) || duration <= 0)
            {
                MessageBox.Show("运动时长必须为大于0的正整数（单位：分钟）！", "校验失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_dtpExerciseTime.Value > DateTime.Now)
            {
                MessageBox.Show("运动时间不能晚于当前系统时间！", "校验失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(_cboExerciseType.Text))
            {
                MessageBox.Show("请选择运动类型！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(_cboIntensity.Text))
            {
                MessageBox.Show("请选择运动强度！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            #endregion
            #region 构建数据模型并保存
            decimal metValue = _bll.GetExerciseMetValue(_cboExerciseType.Text);
            // 适配新架构：统一data_status=1
            var model = new Exercise
            {
                user_id = _currentUserId,
                exercise_type = _cboExerciseType.Text,
                met_value = metValue,
                exercise_duration = duration,
                exercise_intensity = _cboIntensity.Text,
                exercise_time = _dtpExerciseTime.Value,
                data_source = "手动录入",
                device_id = null,
                related_bs_id = null,
                data_status = 1
            };
            var result = _bll.SaveExercise(model);
            if (result.success)
            {
                MessageBox.Show("运动记录保存成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAllUserData();
                BtnClear_Click(null, null);
                TrySpeak($"已保存{_cboExerciseType.Text}运动记录，时长{duration}分钟。请继续保持规律运动。");
            }
            else
            {
                MessageBox.Show(result.msg, "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            #endregion
        }
        #endregion

        #region 2. 清空录入框 原有逻辑完全保留
        private void BtnClear_Click(object sender, EventArgs e)
        {
            SetWatermark(_txtDuration, "例：30分钟");
            _cboExerciseType.SelectedIndex = 0;
            _cboIntensity.SelectedIndex = 1;
            _dtpExerciseTime.Value = DateTime.Now;
        }

        private bool ValidateExerciseSafety()
        {
            decimal latestBloodSugar = _bllBloodSugar.GetTodayLatestBloodSugar(_currentUserId);
            if (latestBloodSugar > 0m && latestBloodSugar < 3.9m)
            {
                string warningText = $"当前最新血糖 {latestBloodSugar:F1} mmol/L，低于 3.9 mmol/L，严禁运动，请立即补充碳水并复测血糖。";
                ShowExerciseWarningDialog(warningText);
                TrySpeak(warningText);
                return false;
            }
            return true;
        }

        private void SpeakExercisePlan()
        {
            string content = string.IsNullOrWhiteSpace(_txtPlan?.Text)
                ? "当前暂无运动方案，请先录入或刷新数据。"
                : _txtPlan.Text.Replace("\r", "，").Replace("\n", "，");
            TrySpeak(content);
        }

        private void TrySpeak(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }
            try
            {
                if (_speechSynthesizer == null)
                {
                    _speechSynthesizer = new SpeechSynthesizer();
                    _speechSynthesizer.SetOutputToDefaultAudioDevice();
                }
                _speechSynthesizer.SpeakAsyncCancelAll();
                _speechSynthesizer.SpeakAsync(text);
            }
            catch
            {
            }
        }

        private void ShowExerciseWarningDialog(string text)
        {
            using (var warningForm = new Form())
            {
                warningForm.Text = "运动风险警示";
                warningForm.StartPosition = FormStartPosition.CenterParent;
                warningForm.Size = new Size(560, 320);
                warningForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                warningForm.MaximizeBox = false;
                warningForm.MinimizeBox = false;
                warningForm.BackColor = Color.White;
                warningForm.TopMost = true;

                var lblTitle = new Label
                {
                    Dock = DockStyle.Top,
                    Height = 72,
                    Text = "⚠ 低血糖，禁止运动",
                    Font = new Font("微软雅黑", 22F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(220, 53, 69),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                var lblContent = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = text,
                    Font = new Font("微软雅黑", 14F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(64, 64, 64),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Padding = new Padding(28, 0, 28, 0)
                };
                var btnConfirm = new Button
                {
                    Dock = DockStyle.Bottom,
                    Height = 54,
                    Text = "我知道了",
                    BackColor = Color.FromArgb(220, 53, 69),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("微软雅黑", 12F, FontStyle.Bold)
                };
                btnConfirm.FlatAppearance.BorderSize = 0;
                btnConfirm.Click += (s, e) => warningForm.Close();

                warningForm.Controls.Add(lblContent);
                warningForm.Controls.Add(btnConfirm);
                warningForm.Controls.Add(lblTitle);
                warningForm.ShowDialog(this);
            }
        }
        #endregion

        #region 3. 模拟导入
        private void BtnImport_Click(object sender, EventArgs e)
        {
            if (_currentUserId <= 0)
            {
                MessageBox.Show("用户登录状态异常，请重新登录！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (MessageBox.Show("确定导入模拟运动设备数据？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            var result = _bll.BatchImportMockData(_currentUserId, 7);
            if (result.success)
            {
                MessageBox.Show(result.msg, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAllUserData();
            }
            else
            {
                MessageBox.Show(result.msg, "导入失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 4. 筛选查询
        private void BtnQueryFilter_Click(object sender, EventArgs e)
        {
            if (_currentUserId <= 0) return;
            DateTime startDate = _dtpStartDate.Value.Date;
            DateTime endDate = _dtpEndDate.Value.Date;
            string exerciseType = _cboFilterType.Text == "全部" ? "" : _cboFilterType.Text;
            string intensity = _cboFilterIntensity.Text == "全部" ? "" : _cboFilterIntensity.Text;
            var list = _bll.GetExerciseByFilter(_currentUserId, startDate, endDate, exerciseType, intensity);
            RefreshGrid(list);
        }
        #endregion

        #region 5. 表格编辑/删除事件
        private void DgvExercise_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int exerciseId = Convert.ToInt32(_dgvExercise.Rows[e.RowIndex].Cells["exercise_id"].Value);
            if (e.ColumnIndex == _dgvExercise.Columns["btnEdit"].Index)
            {
                OpenEditDialog(exerciseId);
            }
            else if (e.ColumnIndex == _dgvExercise.Columns["btnDelete"].Index)
            {
                DeleteExerciseRecord(exerciseId);
            }
        }

        private void OpenEditDialog(int exerciseId)
        {
            var model = _bll.GetExerciseById(exerciseId, _currentUserId);
            if (model == null)
            {
                MessageBox.Show("记录不存在或已被删除！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var editForm = new Form
            {
                Text = "编辑运动记录",
                Width = 400,
                Height = 300,
                StartPosition = FormStartPosition.CenterParent,
                Font = new Font("微软雅黑", 9F),
                MaximizeBox = false,
                MinimizeBox = false
            };
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5, ColumnCount = 2, Padding = new Padding(20) };
            for (int i = 0; i < 4; i++) table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            for (int i = 0; i < 2; i++) table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            var cboType = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cboType.Items.AddRange(new[] { "快走", "慢跑", "太极拳", "游泳", "其他" });
            cboType.Text = model.exercise_type;
            table.Controls.Add(new Label { Text = "运动类型", TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            table.Controls.Add(cboType, 1, 0);

            var txtDuration = new TextBox { Dock = DockStyle.Fill, Text = model.exercise_duration.ToString() };
            table.Controls.Add(new Label { Text = "运动时长(分钟)", TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            table.Controls.Add(txtDuration, 1, 1);

            var cboIntensity = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cboIntensity.Items.AddRange(new[] { "低强度", "中强度", "高强度" });
            cboIntensity.Text = model.exercise_intensity;
            table.Controls.Add(new Label { Text = "运动强度", TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
            table.Controls.Add(cboIntensity, 1, 2);

            var dtpTime = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm", Value = model.exercise_time };
            table.Controls.Add(new Label { Text = "运动时间", TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
            table.Controls.Add(dtpTime, 1, 3);

            var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(10) };
            var btnSave = new Button { Text = "保存修改", Width = 100, Height = 30, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnCancel = new Button { Text = "取消", Width = 100, Height = 30, BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.Click += (s, e) =>
            {
                if (!int.TryParse(txtDuration.Text.Trim(), out int duration) || duration <= 0)
                {
                    MessageBox.Show("运动时长必须为大于0的正整数！", "校验失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (dtpTime.Value > DateTime.Now)
                {
                    MessageBox.Show("运动时间不能晚于当前系统时间！", "校验失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                model.exercise_type = cboType.Text;
                model.exercise_duration = duration;
                model.exercise_intensity = cboIntensity.Text;
                model.exercise_time = dtpTime.Value;
                model.met_value = _bll.GetExerciseMetValue(cboType.Text);
                var result = _bll.UpdateExercise(model);
                if (result.success)
                {
                    MessageBox.Show("修改成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAllUserData();
                    editForm.Close();
                }
                else
                {
                    MessageBox.Show(result.msg, "修改失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            btnCancel.Click += (s, e) => editForm.Close();
            btnPanel.Controls.Add(btnSave);
            btnPanel.Controls.Add(btnCancel);
            editForm.Controls.Add(table);
            editForm.Controls.Add(btnPanel);
            editForm.ShowDialog();
        }

        private void DeleteExerciseRecord(int exerciseId)
        {
            if (MessageBox.Show("确定要删除该条运动记录？删除后无法恢复！", "二次确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                return;
            var result = _bll.DeleteExercise(exerciseId, _currentUserId);
            if (result.success)
            {
                MessageBox.Show("删除成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAllUserData();
            }
            else
            {
                MessageBox.Show(result.msg, "删除失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 6. 趋势图悬浮提示
        private void ChartExerciseTrend_GetToolTipText(object sender, ToolTipEventArgs e)
        {
            if (e.HitTestResult.ChartElementType == ChartElementType.DataPoint)
            {
                var point = e.HitTestResult.Series.Points[e.HitTestResult.PointIndex];
                DateTime date = DateTime.FromOADate(point.XValue);
                string totalMinutes = point.YValues[0].ToString("0") + "分钟";
                var detailList = _bll.GetDateExerciseDetail(_currentUserId, date);
                string detail = $"日期：{date:yyyy-MM-dd}\n总运动时长：{totalMinutes}\n\n运动明细：\n";
                foreach (var item in detailList)
                {
                    detail += $"{item.exercise_type} {item.exercise_duration}分钟 {item.exercise_intensity}\n";
                }
                e.Text = detail;
            }
        }
        #endregion

        // ==============================================
        // 通用数据加载与刷新方法
        // ==============================================
        #region 全量数据加载（页面打开时自动执行）
        private void LoadAllUserData()
        {
            if (_currentUserId <= 0) return;
            // 适配新架构：加载个性化运动方案
            LoadPersonalizedPlan();
            var allList = _bll.GetExerciseByFilter(_currentUserId);
            RefreshGrid(allList);
            LoadExerciseTrendChart();
        }
        #endregion

        #region 加载个性化运动干预方案（兼容患者扩展表）
        private void LoadPersonalizedPlan()
        {
            // 适配新拆分架构：获取患者扩展信息
            Patient patient = BLL.B_Patient.GetPatientById(_currentUserId);
            var planResult = _bll.GetPersonalizedExercisePlan(_currentUserId);
            _txtPlan.Clear();
            string[] lines = planResult.htmlContent.Split('\n');
            foreach (string line in lines)
            {
                _txtPlan.AppendText(line + "\n");
                if (line.Contains("mmol/L") && (line.Contains("⚠️") || line.Contains("❌")))
                {
                    int start = line.IndexOf(":") + 1;
                    int end = line.IndexOf("mmol/L") + 6;
                    if (start > 0 && end > start)
                    {
                        _txtPlan.Select(_txtPlan.Text.Length - line.Length - 1 + start, end - start);
                        _txtPlan.SelectionColor = Color.Red;
                        _txtPlan.SelectionFont = new Font("微软雅黑", 9F, FontStyle.Bold);
                    }
                }
            }
            _txtPlan.SelectionStart = 0;
            _txtPlan.ScrollToCaret();
        }
        #endregion

        #region 刷新表格数据
        private void RefreshGrid(List<Exercise> list)
        {
            _exerciseSourceList = list ?? new List<Exercise>();
            _currentPageIndex = 1;
            BindCurrentExercisePage();
        }

        private int GetExerciseTotalPage()
        {
            if (_exerciseSourceList == null || _exerciseSourceList.Count == 0)
            {
                return 1;
            }
            return (int)Math.Ceiling(_exerciseSourceList.Count * 1.0 / _pageSize);
        }

        private void BindCurrentExercisePage()
        {
            if (_dgvExercise == null)
            {
                return;
            }

            int totalPage = GetExerciseTotalPage();
            if (_currentPageIndex < 1)
            {
                _currentPageIndex = 1;
            }
            if (_currentPageIndex > totalPage)
            {
                _currentPageIndex = totalPage;
            }

            var bindList = (_exerciseSourceList ?? new List<Exercise>())
                .Skip((_currentPageIndex - 1) * _pageSize)
                .Take(_pageSize)
                .Select(m => new
            {
                exercise_id = m.exercise_id,
                exercise_type = m.exercise_type,
                duration = m.exercise_duration + "分钟",
                intensity = m.exercise_intensity,
                exercise_time = m.exercise_time.ToString("yyyy-MM-dd HH:mm"),
                data_source = m.data_source,
                create_time = m.create_time.ToString("yyyy-MM-dd HH:mm")
            }).ToList();
            _dgvExercise.DataSource = null;
            _dgvExercise.ColumnHeadersVisible = true;
            _dgvExercise.DataSource = bindList;
            _dgvExercise.Invalidate();

            if (_lblPageInfo != null)
            {
                _lblPageInfo.Text = $"第{_currentPageIndex}页/共{totalPage}页 总计{(_exerciseSourceList?.Count ?? 0)}条";
            }
        }
        #endregion

        #region 加载30天运动趋势图
        private void LoadExerciseTrendChart()
        {
            if (_currentUserId <= 0) return;
            var dt = _bll.Get30DayTrendData(_currentUserId);
            var series = _chartExerciseTrend.Series[0];
            series.Points.Clear();
            DateTime startDate = DateTime.Now.AddDays(-29);
            for (int i = 0; i < 30; i++)
            {
                DateTime currentDate = startDate.AddDays(i).Date;
                DataRow[] rows = dt.Select($"exercise_date = '{currentDate:yyyy-MM-dd}'");
                int totalMinutes = rows.Length > 0 ? Convert.ToInt32(rows[0]["total_minutes"]) : 0;
                series.Points.AddXY(currentDate, totalMinutes);
            }
            _chartExerciseTrend.ChartAreas[0].RecalculateAxesScale();
            _chartExerciseTrend.Invalidate();
        }
        #endregion

        private void FrmExerciseManage_Load(object sender, EventArgs e)
        {

        }
    }
}