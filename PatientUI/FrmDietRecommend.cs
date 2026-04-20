using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using BLL;
using Model;
using System.Collections.Generic;
using System.Linq;

namespace PatientUI
{
    public partial class FrmDietRecommend : Form
    {
        // ==============================================
        // 【原有UI参数 100%完全保留 无任何修改】
        // ==============================================
        private readonly int _pageTop = 100;
        private readonly int _pageLeft = 250;
        private readonly int _pageRight = 20;
        private readonly int _pageBottom = 20;
        private readonly int _suggestHeight = 220;
        private readonly int _tableTop = 100;
        private readonly int _tableLeft = 150;
        private readonly float _colScale = 0.75f;
        private readonly int _colFoodName = 180;
        private readonly int _colCategory = 130;
        private readonly int _colGI = 90;
        private readonly int _colEnergy = 140;
        private readonly int _colCarb = 160;
        private readonly int _fontTitle = 14;
        private readonly int _fontContent = 11;
        private readonly int _fontGrid = 9;

        // ==============================================
        // 高对比度模式 —— 和运动界面完全一致
        // ==============================================
        private bool IsHighContrast => Program.IsHighContrastMode;
        private Color BgColor => IsHighContrast ? Color.Black : Color.White;
        private Color FgColor => IsHighContrast ? Color.White : Color.FromArgb(64, 64, 64);
        private Color CardColor => IsHighContrast ? Color.FromArgb(30, 30, 30) : Color.FromArgb(248, 250, 252);
        private Color CardBlue => IsHighContrast ? Color.FromArgb(30, 30, 30) : Color.FromArgb(240, 248, 255);
        private Color BorderColor => IsHighContrast ? Color.Lime : Color.LightGray;
        private Color ThemeColor => IsHighContrast ? Color.Cyan : Color.FromArgb(0, 122, 204);
        private Color GridBg => IsHighContrast ? Color.Black : Color.White;
        private Color GridHeader => IsHighContrast ? Color.DarkBlue : Color.FromArgb(240, 240, 240);
        private Color ButtonNormal => IsHighContrast ? Color.DarkGreen : Color.FromArgb(0, 122, 204);
        private Color ButtonSuccess => IsHighContrast ? Color.DarkGreen : Color.FromArgb(28, 184, 45);
        private Color LightBlue => IsHighContrast ? Color.FromArgb(30, 30, 30) : Color.FromArgb(237, 246, 255);

        // ==============================================
        // 【修复1：删除硬编码1，改用系统登录用户ID】
        // ==============================================
        private int _currentUserId => Program.LoginUser?.user_id ?? 0;
        private readonly B_Diet _bDiet = new B_Diet();
        private readonly B_FoodNutrition _bFoodNutrition = new B_FoodNutrition();
        private readonly B_UserHealth _bUserHealth = new B_UserHealth();
        private readonly B_DietPlan _bDietPlan = new B_DietPlan();
        private DataGridView _dgvFood;
        private ComboBox _cboCategory;
        private TextBox _txtGiMin;
        private TextBox _txtGiMax;
        private TextBox _txtEnergyMax;
        private Label _lblStatistic;
        private Label _lblMealPlan;
        private Label _lblSuggestContent;
        private readonly int _pageSize = 10;
        private int _currentPageIndex = 1;
        private Label _lblPageInfo;
        private DataTable _foodSourceTable;

        public FrmDietRecommend()
        {
            InitializeComponent();
            this.TopLevel = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;
            this.BackColor = BgColor;
            this.Font = new Font("微软雅黑", 9F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.DoubleBuffered = true;
            this.VisibleChanged += (s, e) =>
            {
                if (this.Visible && _currentUserId > 0) BuildLayout();
            };
        }

        private void BuildLayout()
        {
            if (_currentUserId <= 0)
            {
                MessageBox.Show("请先登录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.Controls.Clear();
            this.SuspendLayout();
            Panel scrollContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgColor,
                AutoScroll = true
            };
            TableLayoutPanel root = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                RowCount = 4,
                ColumnCount = 1,
                BackColor = BgColor,
                Padding = new Padding(_pageLeft, _pageTop, _pageRight, _pageBottom),
                GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, _suggestHeight));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 220));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 450));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            root.Controls.Add(CreateStatisticPanel(), 0, 0);
            root.Controls.Add(CreateSuggestPanel(), 0, 1);
            root.Controls.Add(CreateMealPlanPanel(), 0, 2);
            root.Controls.Add(CreateFoodPanel(), 0, 3);
            scrollContainer.Controls.Add(root);
            this.Controls.Add(scrollContainer);
            this.ResumeLayout(true);
            this.PerformLayout();
            LoadFoodData();
            LoadFoodCategory();
        }

        private Panel CreateStatisticPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardColor,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(20, 10, 20, 10)
            };
            Label title = new Label
            {
                Text = "📊 近7天饮食摄入统计",
                Font = new Font("微软雅黑", _fontTitle, FontStyle.Bold),
                ForeColor = ThemeColor,
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = CardColor
            };
            _lblStatistic = new Label
            {
                Font = new Font("微软雅黑", _fontContent),
                ForeColor = FgColor,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 5, 0, 0),
                AutoSize = false,
                BackColor = CardColor
            };
            LoadStatisticData();
            panel.Controls.Add(_lblStatistic);
            panel.Controls.Add(title);
            return panel;
        }

        private Panel CreateSuggestPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBlue,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(20, 10, 20, 10)
            };
            Label title = new Label
            {
                Text = "📋 今日饮食核心建议",
                Font = new Font("微软雅黑", _fontTitle, FontStyle.Bold),
                ForeColor = ThemeColor,
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = CardBlue
            };
            _lblSuggestContent = new Label
            {
                Font = new Font("微软雅黑", _fontContent),
                ForeColor = FgColor,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 5, 0, 0),
                AutoSize = true,
                BackColor = CardBlue
            };
            LoadPersonalizedSuggestion();
            panel.Controls.Add(_lblSuggestContent);
            panel.Controls.Add(title);
            return panel;
        }

        private Panel CreateMealPlanPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgColor,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(20, 10, 20, 10)
            };
            Label title = new Label
            {
                Text = "🍱 今日饮食搭配方案",
                Font = new Font("微软雅黑", _fontTitle, FontStyle.Bold),
                ForeColor = ThemeColor,
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = BgColor
            };
            _lblMealPlan = new Label
            {
                Font = new Font("微软雅黑", _fontContent),
                ForeColor = FgColor,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 5, 0, 0),
                AutoSize = false,
                BackColor = BgColor
            };
            LoadMealPlanData();
            panel.Controls.Add(_lblMealPlan);
            panel.Controls.Add(title);
            return panel;
        }

        private Panel CreateFoodPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgColor,
                Padding = new Padding(0, 10, 0, 18)
            };
            Label title = new Label
            {
                Text = "🍎 低GI推荐食物（GI<55，适合糖尿病患者）",
                Font = new Font("微软雅黑", _fontTitle, FontStyle.Bold),
                ForeColor = ThemeColor,
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = BgColor
            };
            Panel pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = BgColor,
                Padding = new Padding(0, 5, 0, 5)
            };
            Panel pagePanel = CreatePagerPanel();
            BuildFilterPanel(pnlFilter);
            _dgvFood = new DataGridView
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(_tableLeft, _tableTop, 0, 0),
                BackgroundColor = GridBg,
                ForeColor = FgColor,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                ColumnHeadersVisible = true,
                GridColor = BorderColor,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToOrderColumns = true,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 40,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EnableHeadersVisualStyles = false,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = GridHeader, ForeColor = FgColor }
            };
            _dgvFood.Columns.AddRange(new[]
            {
                new DataGridViewTextBoxColumn {
                    Name = "FoodName", HeaderText = "食物名称", DataPropertyName = "FoodName",
                    Width = (int)(_colFoodName * _colScale), SortMode = DataGridViewColumnSortMode.Automatic
                },
                new DataGridViewTextBoxColumn {
                    Name = "FoodCategory", HeaderText = "食物分类", DataPropertyName = "FoodCategory",
                    Width = (int)(_colCategory * _colScale), SortMode = DataGridViewColumnSortMode.Automatic
                },
                new DataGridViewTextBoxColumn {
                    Name = "GI", HeaderText = "GI值", DataPropertyName = "GI",
                    Width = (int)(_colGI * _colScale), SortMode = DataGridViewColumnSortMode.Automatic
                },
                new DataGridViewTextBoxColumn {
                    Name = "Energy_kcal", HeaderText = "热量(kcal/100g)", DataPropertyName = "Energy_kcal",
                    Width = (int)(_colEnergy * _colScale), SortMode = DataGridViewColumnSortMode.Automatic
                },
                new DataGridViewTextBoxColumn {
                    Name = "Carbohydrate", HeaderText = "碳水化合物(g/100g)", DataPropertyName = "Carbohydrate",
                    Width = (int)(_colCarb * _colScale), SortMode = DataGridViewColumnSortMode.Automatic
                },
                new DataGridViewTextBoxColumn { Name = "FoodID", DataPropertyName = "FoodID", Visible = false },
                new DataGridViewTextBoxColumn { Name = "FoodCode", DataPropertyName = "FoodCode", Visible = false }
            });
            _dgvFood.DefaultCellStyle.Font = new Font("微软雅黑", _fontGrid);
            _dgvFood.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", _fontGrid, FontStyle.Bold);
            _dgvFood.RowTemplate.Height = 34;
            _dgvFood.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dgvFood.CellDoubleClick += DgvFood_CellDoubleClick;
            panel.Controls.Add(_dgvFood);
            panel.Controls.Add(pagePanel);
            panel.Controls.Add(pnlFilter);
            panel.Controls.Add(title);
            return panel;
        }

        private Panel CreatePagerPanel()
        {
            Panel pagePanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = BgColor,
                Padding = new Padding(8, 10, 8, 10)
            };
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2,
                BackColor = BgColor
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320f));

            _lblPageInfo = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = this.Font,
                Text = "第1页/共1页 总计0条",
                ForeColor = FgColor,
                BackColor = BgColor
            };

            FlowLayoutPanel btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = BgColor
            };

            Button CreatePageBtn(string text, Action onClick)
            {
                Button btn = new Button
                {
                    Text = text,
                    Width = 62,
                    Height = 34,
                    MinimumSize = new Size(62, 34),
                    Margin = new Padding(2, 0, 2, 0),
                    FlatStyle = FlatStyle.Standard,
                    BackColor = ButtonNormal,
                    ForeColor = Color.White
                };
                btn.Click += (s, e) => onClick();
                return btn;
            }

            btnPanel.Controls.Add(CreatePageBtn("首页", () => { _currentPageIndex = 1; BindCurrentFoodPage(); }));
            btnPanel.Controls.Add(CreatePageBtn("上一页", () => { _currentPageIndex--; BindCurrentFoodPage(); }));
            btnPanel.Controls.Add(CreatePageBtn("下一页", () => { _currentPageIndex++; BindCurrentFoodPage(); }));
            btnPanel.Controls.Add(CreatePageBtn("末页", () => { _currentPageIndex = GetFoodTotalPage(); BindCurrentFoodPage(); }));

            layout.Controls.Add(_lblPageInfo, 0, 0);
            layout.Controls.Add(btnPanel, 1, 0);
            pagePanel.Controls.Add(layout);
            return pagePanel;
        }

        private void BuildFilterPanel(Panel panel)
        {
            Label lblCategory = new Label
            {
                Text = "食物分类：",
                Font = new Font("微软雅黑", _fontContent),
                ForeColor = FgColor,
                AutoSize = true,
                Location = new Point(10, 10),
                BackColor = BgColor
            };
            _cboCategory = new ComboBox
            {
                Font = new Font("微软雅黑", _fontContent),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 120,
                Location = new Point(90, 8),
                BackColor = CardColor,
                ForeColor = FgColor
            };
            Label lblGi = new Label
            {
                Text = "GI范围：",
                Font = new Font("微软雅黑", _fontContent),
                ForeColor = FgColor,
                AutoSize = true,
                Location = new Point(220, 10),
                BackColor = BgColor
            };
            _txtGiMin = new TextBox
            {
                Font = new Font("微软雅黑", _fontContent),
                Width = 80,
                Location = new Point(292, 8),
                TextAlign = HorizontalAlignment.Left,
                Text = "最小值",
                BackColor = CardColor,
                ForeColor = FgColor
            };
            _txtGiMin.Enter += (s, e) => { if (_txtGiMin.Text == "最小值") _txtGiMin.Text = ""; };
            _txtGiMin.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(_txtGiMin.Text)) _txtGiMin.Text = "最小值"; };
            Label lblGiLine = new Label
            {
                Text = "-",
                Font = new Font("微软雅黑", _fontContent),
                ForeColor = FgColor,
                AutoSize = true,
                Location = new Point(378, 10),
                BackColor = BgColor
            };
            _txtGiMax = new TextBox
            {
                Font = new Font("微软雅黑", _fontContent),
                Width = 80,
                Location = new Point(392, 8),
                TextAlign = HorizontalAlignment.Left,
                Text = "最大值",
                BackColor = CardColor,
                ForeColor = FgColor
            };
            _txtGiMax.Enter += (s, e) => { if (_txtGiMax.Text == "最大值") _txtGiMax.Text = ""; };
            _txtGiMax.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(_txtGiMax.Text)) _txtGiMax.Text = "最大值"; };
            Label lblEnergy = new Label
            {
                Text = "热量上限：",
                Font = new Font("微软雅黑", _fontContent),
                ForeColor = FgColor,
                AutoSize = true,
                Location = new Point(478, 10),
                BackColor = BgColor
            };
            _txtEnergyMax = new TextBox
            {
                Font = new Font("微软雅黑", _fontContent),
                Width = 80,
                Location = new Point(558, 8),
                Text = "kcal/100g",
                BackColor = CardColor,
                ForeColor = FgColor
            };
            _txtEnergyMax.Enter += (s, e) => { if (_txtEnergyMax.Text == "kcal/100g") _txtEnergyMax.Text = ""; };
            _txtEnergyMax.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(_txtEnergyMax.Text)) _txtEnergyMax.Text = "kcal/100g"; };
            Button btnQuery = new Button
            {
                Text = "查询",
                Font = new Font("微软雅黑", _fontContent),
                BackColor = ButtonNormal,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 80,
                Height = 28,
                Location = new Point(648, 7)
            };
            btnQuery.FlatAppearance.BorderSize = 0;
            btnQuery.Click += BtnQuery_Click;
            Button btnReset = new Button
            {
                Text = "重置",
                Font = new Font("微软雅黑", _fontContent),
                BackColor = BgColor,
                ForeColor = ThemeColor,
                FlatStyle = FlatStyle.Flat,
                Width = 80,
                Height = 28,
                Location = new Point(738, 7)
            };
            btnReset.FlatAppearance.BorderColor = ThemeColor;
            btnReset.Click += BtnReset_Click;
            Button btnAddToDiet = new Button
            {
                Text = "添加到今日饮食",
                Font = new Font("微软雅黑", _fontContent),
                BackColor = ButtonSuccess,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 150,
                Height = 28,
                Location = new Point(828, 7)
            };
            btnAddToDiet.FlatAppearance.BorderSize = 0;
            btnAddToDiet.Click += BtnAddToDiet_Click;
            panel.Controls.AddRange(new Control[]
            {
                lblCategory, _cboCategory,
                lblGi, _txtGiMin, lblGiLine, _txtGiMax,
                lblEnergy, _txtEnergyMax,
                btnQuery, btnReset, btnAddToDiet
            });
            panel.Controls.Add(CreateCommonFoodShortcutButton("燕麦", 10, 38));
            panel.Controls.Add(CreateCommonFoodShortcutButton("苹果", 90, 38));
            panel.Controls.Add(CreateCommonFoodShortcutButton("西兰花", 170, 38));
            panel.Controls.Add(CreateCommonFoodShortcutButton("豆腐", 250, 38));
            panel.Controls.Add(CreateCommonFoodShortcutButton("酸奶", 330, 38));
        }

        private Button CreateCommonFoodShortcutButton(string keyword, int x, int y)
        {
            Button button = new Button
            {
                Text = keyword,
                Font = new Font("微软雅黑", 9F, FontStyle.Bold),
                BackColor = LightBlue,
                ForeColor = ThemeColor,
                FlatStyle = FlatStyle.Flat,
                Width = 72,
                Height = 24,
                Location = new Point(x, y)
            };
            button.FlatAppearance.BorderColor = ThemeColor;
            button.Click += (s, e) => AddCommonFoodToDiet(keyword);
            return button;
        }

        #region 数据加载方法
        private void LoadStatisticData()
        {
            var result = _bDiet.Get7DayDietStatistic(_currentUserId);
            if (!result.IsSuccess)
            {
                _lblStatistic.Text = "统计数据加载失败";
                MessageBox.Show(result.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Diet7DayStatistic statistic = result.Data as Diet7DayStatistic;
            string calorieStatus = statistic.AvgDailyCalorie > statistic.RecommendDailyCalorie
                ? $"**超出推荐值{Math.Round(statistic.AvgDailyCalorie - statistic.RecommendDailyCalorie, 0)}kcal，建议控制摄入量**"
                : $"符合推荐范围，继续保持";
            string carbStatus = statistic.CarbEnergyRatio < statistic.RecommendCarbRatioMin
                ? $"**低于推荐范围，建议适当增加优质碳水摄入**"
                : statistic.CarbEnergyRatio > statistic.RecommendCarbRatioMax
                    ? $"**超出推荐范围，建议控制碳水化合物摄入**"
                    : $"符合推荐范围，继续保持";
            _lblStatistic.Text = $"近7天总摄入热量：{statistic.TotalCalorie:F0} kcal | 日均摄入热量：{statistic.AvgDailyCalorie:F0} kcal（推荐值：{statistic.RecommendDailyCalorie:F0} kcal）{calorieStatus}\r\n" +
                                 $"近7天总摄入碳水：{statistic.TotalCarb:F0} g | 碳水化合物供能占比：{statistic.CarbEnergyRatio:F1}%（推荐范围：{statistic.RecommendCarbRatioMin}%-{statistic.RecommendCarbRatioMax}%）{carbStatus}";
        }

        private void LoadPersonalizedSuggestion()
        {
            Patient patient = B_Patient.GetPatientById(_currentUserId);
            var result = _bUserHealth.GeneratePersonalizedDietSuggestion(_currentUserId);
            if (!result.IsSuccess)
            {
                _lblSuggestContent.Text = "1. 优先选择低GI食物，控制每日碳水化合物摄入量；\r\n" +
                                          "2. 三餐定时定量，避免暴饮暴食；\r\n" +
                                          "3. 多吃绿叶蔬菜，减少精制米面、含糖饮料摄入；\r\n" +
                                          "4. 烹饪方式优先选择蒸、煮、清炒，避免油炸、红烧。";
                return;
            }
            _lblSuggestContent.Text = result.Data.ToString();
        }

        private void LoadMealPlanData()
        {
            var result = _bDietPlan.GenerateTodayMealPlan(_currentUserId);
            if (!result.IsSuccess)
            {
                _lblMealPlan.Text = "饮食方案生成失败";
                MessageBox.Show(result.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DietMealPlan plan = result.Data as DietMealPlan;
            string breakfast = string.Join("、", plan.BreakfastFoods.ConvertAll(f => f.FoodName));
            string lunch = string.Join("、", plan.LunchFoods.ConvertAll(f => f.FoodName));
            string dinner = string.Join("、", plan.DinnerFoods.ConvertAll(f => f.FoodName));
            _lblMealPlan.Text = $"【早餐】{breakfast} | 预计热量：{plan.BreakfastTotalCalorie:F0} kcal\r\n" +
                                 $"【午餐】{lunch} | 预计热量：{plan.LunchTotalCalorie:F0} kcal\r\n" +
                                 $"【晚餐】{dinner} | 预计热量：{plan.DinnerTotalCalorie:F0} kcal\r\n" +
                                 $"全天预计总热量：{plan.TotalDayCalorie:F0} kcal，符合糖尿病患者每日推荐摄入量，可根据自身情况调整食用量";
        }

        private void LoadFoodData()
        {
            var result = _bFoodNutrition.GetLowGIFoods(0);
            if (!result.IsSuccess)
            {
                MessageBox.Show(result.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SetFoodDataSource(result.Data as DataTable, true);
        }

        private void LoadFoodCategory()
        {
            var result = _bFoodNutrition.GetAllFoodCategory();
            if (!result.IsSuccess)
            {
                MessageBox.Show(result.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DataTable dt = result.Data as DataTable;
            DataRow dr = dt.NewRow();
            dr["FoodCategory"] = "全部";
            dt.Rows.InsertAt(dr, 0);
            _cboCategory.DataSource = dt;
            _cboCategory.DisplayMember = "FoodCategory";
            _cboCategory.ValueMember = "FoodCategory";
        }
        #endregion

        #region 控件事件处理
        private void DgvFood_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = _dgvFood.Rows[e.RowIndex];
            int foodId = Convert.ToInt32(row.Cells["FoodID"].Value);
            var result = _bFoodNutrition.GetFoodDetailById(foodId);
            if (!result.IsSuccess)
            {
                MessageBox.Show(result.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            FoodNutrition food = result.Data as FoodNutrition;
            FrmFoodDetail frmDetail = new FrmFoodDetail(food);
            frmDetail.ShowDialog(this);
        }

        private void BtnQuery_Click(object sender, EventArgs e)
        {
            string category = string.IsNullOrWhiteSpace(_cboCategory?.Text) ? "全部" : _cboCategory.Text.Trim();
            decimal.TryParse(_txtGiMin.Text.Trim() == "最小值" ? "0" : _txtGiMin.Text.Trim(), out decimal giMin);
            decimal.TryParse(_txtGiMax.Text.Trim() == "最大值" ? "55" : _txtGiMax.Text.Trim(), out decimal giMax);
            decimal.TryParse(_txtEnergyMax.Text.Trim() == "kcal/100g" ? "0" : _txtEnergyMax.Text.Trim(), out decimal energyMax);

            if (giMin > giMax)
            {
                MessageBox.Show("GI最小值不能大于最大值，请重新输入。", "查询提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = _bFoodNutrition.GetFoodsByFilter(category, giMin, giMax, energyMax);
            if (!result.IsSuccess)
            {
                MessageBox.Show(result.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DataTable dt = result.Data as DataTable;
            SetFoodDataSource(dt, true);
            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("未查询到符合条件的食物，请调整筛选条件后重试。", "查询结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            _cboCategory.SelectedIndex = 0;
            _txtGiMin.Text = "最小值";
            _txtGiMax.Text = "最大值";
            _txtEnergyMax.Text = "kcal/100g";
            LoadFoodData();
        }

        private void SetFoodDataSource(DataTable source, bool resetPage)
        {
            _foodSourceTable = source;
            if (resetPage)
            {
                _currentPageIndex = 1;
            }
            BindCurrentFoodPage();
        }

        private int GetFoodTotalPage()
        {
            int rowCount = _foodSourceTable?.Rows.Count ?? 0;
            if (rowCount == 0)
            {
                return 1;
            }
            return (int)Math.Ceiling(rowCount * 1.0 / _pageSize);
        }

        private void BindCurrentFoodPage()
        {
            if (_dgvFood == null) return;
            int totalRows = _foodSourceTable?.Rows.Count ?? 0;
            int totalPage = GetFoodTotalPage();
            if (_currentPageIndex < 1) _currentPageIndex = 1;
            if (_currentPageIndex > totalPage) _currentPageIndex = totalPage;

            DataTable pageTable = _foodSourceTable?.Clone();
            if (_foodSourceTable != null && _foodSourceTable.Rows.Count > 0)
            {
                int skip = (_currentPageIndex - 1) * _pageSize;
                foreach (DataRow row in _foodSourceTable.AsEnumerable().Skip(skip).Take(_pageSize))
                {
                    pageTable.ImportRow(row);
                }
            }

            _dgvFood.DataSource = null;
            _dgvFood.ColumnHeadersVisible = true;
            _dgvFood.DataSource = pageTable;
            _dgvFood.Invalidate();

            if (_lblPageInfo != null)
            {
                _lblPageInfo.Text = $"第{_currentPageIndex}页/共{totalPage}页 总计{totalRows}条";
            }
        }

        private void BtnAddToDiet_Click(object sender, EventArgs e)
        {
            if (_dgvFood.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要添加的食物", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            AddFoodRowToTodayDiet(_dgvFood.SelectedRows[0]);
        }

        private void AddCommonFoodToDiet(string keyword)
        {
            if (_dgvFood == null || _dgvFood.Rows.Count == 0)
            {
                MessageBox.Show("当前没有可选食物数据，请先查询后再试。", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DataGridViewRow targetRow = null;
            foreach (DataGridViewRow row in _dgvFood.Rows)
            {
                if (row.IsNewRow || row.Cells["FoodName"].Value == null) continue;
                if (row.Cells["FoodName"].Value.ToString().IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    targetRow = row;
                    break;
                }
            }
            if (targetRow == null)
            {
                MessageBox.Show($"当前推荐列表中未找到“{keyword}”，请先调整筛选条件。", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _dgvFood.ClearSelection();
            targetRow.Selected = true;
            if (_dgvFood.FirstDisplayedScrollingRowIndex >= 0 && targetRow.Index >= 0)
            {
                _dgvFood.FirstDisplayedScrollingRowIndex = targetRow.Index;
            }
            AddFoodRowToTodayDiet(targetRow);
        }

        private void AddFoodRowToTodayDiet(DataGridViewRow row)
        {
            if (row == null) return;
            string foodName = row.Cells["FoodName"].Value.ToString();
            string foodCode = row.Cells["FoodCode"]?.Value?.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(foodCode))
            {
                MessageBox.Show("当前食物缺少FoodCode，无法添加到今日饮食。请重新查询后重试。", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            decimal gi = Convert.ToDecimal(row.Cells["GI"].Value);
            decimal energy = Convert.ToDecimal(row.Cells["Energy_kcal"].Value);
            decimal carb = Convert.ToDecimal(row.Cells["Carbohydrate"].Value);
            string input = ShowInputDialog($"请输入【{foodName}】的食用重量（单位：g）", "添加到今日饮食", "100");
            if (string.IsNullOrEmpty(input)) return;
            if (!decimal.TryParse(input.Trim(), out decimal amount) || amount <= 0)
            {
                MessageBox.Show("请输入有效的食用重量（大于0的数字）", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string selectedMealType = ShowMealTypeDialog();
            if (string.IsNullOrEmpty(selectedMealType)) return;

            Diet diet = new Diet
            {
                user_id = _currentUserId,
                food_name = foodCode,
                local_food_name = foodName,
                food_gi = gi,
                food_calorie = Convert.ToInt32(energy),
                food_carb = carb,
                food_amount = amount,
                actual_calorie = Math.Round(energy * amount / 100, 1),
                actual_carb = Math.Round(carb * amount / 100, 1),
                meal_time = DateTime.Now,
                meal_type = selectedMealType,
                data_source = "手动录入",
                operator_id = _currentUserId,
                is_custom = 0,
                data_status = 1,
                data_version = 1,
                calorie_unit = "kcal",
                carb_unit = "g",
                create_time = DateTime.Now,
                update_time = DateTime.Now
            };
            var result = _bDiet.AddDietRecord(diet);
            if (result.IsSuccess)
            {
                MessageBox.Show("添加成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadStatisticData();
            }
            else
            {
                MessageBox.Show(result.Message, "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ShowInputDialog(string text, string caption, string defaultValue)
        {
            Form inputForm = new Form
            {
                Width = 400,
                Height = 180,
                Text = caption,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = BgColor,
                ForeColor = FgColor
            };
            Label lblText = new Label { Text = text, Location = new Point(20, 20), AutoSize = true, Font = new Font("微软雅黑", 9F), BackColor = BgColor, ForeColor = FgColor };
            TextBox txtInput = new TextBox { Text = defaultValue, Location = new Point(20, 50), Width = 340, Font = new Font("微软雅黑", 9F), BackColor = CardColor, ForeColor = FgColor };
            Button btnOk = new Button { Text = "确定", DialogResult = DialogResult.OK, Location = new Point(200, 90), Width = 80, Height = 30, BackColor = ButtonNormal, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            Button btnCancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel, Location = new Point(290, 90), Width = 80, Height = 30, BackColor = CardColor, ForeColor = FgColor };
            inputForm.Controls.AddRange(new Control[] { lblText, txtInput, btnOk, btnCancel });
            inputForm.AcceptButton = btnOk;
            inputForm.CancelButton = btnCancel;
            return inputForm.ShowDialog() == DialogResult.OK ? txtInput.Text : "";
        }

        private string ShowMealTypeDialog()
        {
            Form mealForm = new Form
            {
                Width = 300,
                Height = 200,
                Text = "选择用餐类型",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = BgColor,
                ForeColor = FgColor
            };
            Label lblTip = new Label { Text = "请选择用餐类型", Location = new Point(20, 20), AutoSize = true, Font = new Font("微软雅黑", 9F), BackColor = BgColor, ForeColor = FgColor };
            ComboBox cboMeal = new ComboBox
            {
                DataSource = new string[] { "早餐", "午餐", "晚餐", "加餐" },
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(20, 50),
                Width = 240,
                Font = new Font("微软雅黑", 9F),
                BackColor = CardColor,
                ForeColor = FgColor
            };
            Button btnOk = new Button { Text = "确定", DialogResult = DialogResult.OK, Location = new Point(100, 100), Width = 80, Height = 30, BackColor = ButtonNormal, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            Button btnCancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel, Location = new Point(190, 100), Width = 80, Height = 30, BackColor = CardColor, ForeColor = FgColor };
            mealForm.Controls.AddRange(new Control[] { lblTip, cboMeal, btnOk, btnCancel });
            mealForm.AcceptButton = btnOk;
            mealForm.CancelButton = btnCancel;
            return mealForm.ShowDialog() == DialogResult.OK ? cboMeal.SelectedValue.ToString() : "";
        }
        #endregion

        private void FrmDietRecommend_Load(object sender, EventArgs e) { }
    }
}