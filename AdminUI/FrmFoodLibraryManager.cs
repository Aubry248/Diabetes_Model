using BLL;
using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AdminUI
{
    // 唯一的窗体类，无任何重复！
    public partial class FrmFoodLibraryManager : Form
    {
        #region ========== 全局统一布局参数 ==========
        private readonly Padding _globalMainContainerPadding = new Padding(15, 15, 15, 15);
        private readonly bool _globalContentAutoCenter = false;
        private readonly int _globalContentOffsetX = 1;
        private readonly int _globalContentOffsetY = 1;
        private readonly int _globalContentMinWidth = 1200;
        private readonly int _globalContentMinHeight = 700;
        private readonly Padding _globalControlMargin = new Padding(5, 5, 5, 5);
        private readonly int _globalControlHeight = 28;
        private readonly int _globalButtonHeight = 36;
        private readonly int _globalButtonWidth = 110;
        private readonly int _globalLabelWidth = 140;
        private readonly int _globalRowHeight = 40;
        private readonly Padding _globalGroupBoxPadding = new Padding(15);
        // 在全局变量区域添加缓存变量
        private DataTable _cachedFoodList;
        private DateTime _cacheExpireTime;
        private readonly Color _colorLow = Color.Green;
        private readonly Color _colorMiddle = Color.Orange;
        private readonly Color _colorHigh = Color.Red;
        #endregion

        #region 核心控件声明
        private Panel pnlMainContainer;
        private Panel pnlContentWrapper;
        private TabControl tabMain;
        private TabPage tab_FoodList, tab_FoodEdit, tab_BatchOperate, tab_Audit, tab_Version;

        // 食物库列表页
        private GroupBox grp_SearchFilter, grp_FoodList;
        private TextBox txt_SearchKey;
        private ComboBox cbo_FoodCategory, cbo_GILevel, cbo_GLLevel, cbo_EnableStatus, cbo_DataSource;
        private DateTimePicker dtp_UpdateStart, dtp_UpdateEnd;
        private DataGridView dgv_FoodList;
        private Button btn_Search, btn_ResetFilter, btn_AddSingle, btn_EditSelected, btn_DeleteSelected;

        // 食物详情/编辑页
        private GroupBox grp_BaseInfo, grp_Nutrition, grp_DiabetesIndex, grp_ClinicalAdapt, grp_StatusVersion;
        private TextBox txt_FoodCode, txt_FoodName, txt_Alias, txt_EdibleRatio, txt_DataSourceInfo, txt_Reference;
        private ComboBox cbo_EditCategory;
        private PictureBox pb_FoodImage;
        private TextBox txt_Calorie, txt_Carb, txt_Protein, txt_Fat, txt_Fiber, txt_Sodium;
        private TextBox txt_GI, txt_GL, txt_ExchangeUnit, txt_GlycemicFeature;
        private TextBox txt_SuitablePeople, txt_ForbiddenPeople, txt_RecommendAmount, txt_CookingSuggest, txt_GlucoseTip;
        private ComboBox cbo_EditEnableStatus;
        private TextBox txt_Version, txt_UpdateLog, txt_AuditRecord;
        private Button btn_SaveEdit, btn_CancelEdit, btn_UploadImage;

        // 批量操作页
        private GroupBox grp_BatchImport, grp_BatchOperate;
        private Button btn_ImportExcel, btn_ExportExcel, btn_BatchEnable, btn_BatchDisable, btn_Deduplicate;

        // 审核页
        private GroupBox grp_AuditFilter, grp_AuditList;
        private ComboBox cbo_AuditStatus, cbo_Uploader;
        private DataGridView dgv_AuditList;
        private Button btn_QueryAudit, btn_AuditPass, btn_AuditReject, btn_ResetAudit;

        // 版本管理页
        private GroupBox grp_VersionFilter, grp_VersionList;
        private ComboBox cbo_VersionFood;
        private DataGridView dgv_VersionList;
        private Button btn_QueryVersion, btn_RollbackVersion, btn_ResetVersion;
        #endregion

        #region 全局业务对象（新增）
        private readonly B_FoodNutrition _bFood = new B_FoodNutrition();
        private readonly B_FoodAudit _bAudit = new B_FoodAudit();
        private readonly B_FoodVersion _bVersion = new B_FoodVersion();
        private int _currentEditFoodId = 0; // 当前编辑的食物主键ID
        private string _currentLoginUser = "系统管理员1"; // 可对接登录全局变量
        private string _currentFoodImagePath = string.Empty; // 食物图片路径
        #endregion
        public FrmFoodLibraryManager()

        {
            InitializeComponent();
            this.Controls.Clear();

            // 初始化动态控件
            InitMainContainer();
            InitializeDynamicControls();
            InitControlData();
            BindAllEvents();
            // 新增：窗体加载时绑定真实食物数据
            this.Load += (s, e) => BindFoodListData();
            // 【修复8】子窗体关闭释放资源（防止内存泄漏导致跳转不稳定）
            this.FormClosed += (s, e) => { this.Dispose(); };
        }

        #region 全局容器初始化
        private void InitMainContainer()
        {
            pnlMainContainer = new Panel();
            pnlMainContainer.Dock = DockStyle.Fill;
            pnlMainContainer.BackColor = Color.White;
            pnlMainContainer.Padding = _globalMainContainerPadding;
            pnlMainContainer.AutoScroll =true;
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

        #region 动态创建所有控件
        private void InitializeDynamicControls()
        {
            tabMain = new TabControl();
            tabMain.Dock = DockStyle.Fill;
            tabMain.Font = new Font("微软雅黑", 10F);
            tabMain.Padding = new Point(15, 8);

            tab_FoodList = new TabPage("食物库列表") { BackColor = Color.White };
            tab_FoodEdit = new TabPage("食物详情/编辑") { BackColor = Color.White };
            tab_BatchOperate = new TabPage("批量操作") { BackColor = Color.White };
            tab_Audit = new TabPage("数据审核") { BackColor = Color.White };
            tab_Version = new TabPage("版本管理") { BackColor = Color.White };

            tabMain.TabPages.AddRange(new[] { tab_FoodList, tab_FoodEdit, tab_BatchOperate, tab_Audit, tab_Version });
            pnlContentWrapper.Controls.Add(tabMain);

            InitFoodListPage();
            InitFoodEditPage();
            InitBatchOperatePage();
            InitAuditPage();
            InitVersionPage();
        }

        private void InitFoodListPage()
        {
            grp_SearchFilter = new GroupBox { Text = "检索与筛选条件", Dock = DockStyle.Top, Height = 180, Padding = _globalGroupBoxPadding };
            tab_FoodList.Controls.Add(grp_SearchFilter);
            TableLayoutPanel tlp_Filter = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Filter.RowCount = 3;
            tlp_Filter.ColumnCount = 4;
            tlp_Filter.ColumnStyles.Clear();
            tlp_Filter.RowStyles.Clear();
            for (int i = 0; i < 4; i++)
                tlp_Filter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            for (int i = 0; i < 3; i++)
                tlp_Filter.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_SearchFilter.Controls.Add(tlp_Filter);
            Label lbl_Search = new Label
            {
                Text = "名称/拼音/编码：",
                Size = new Size(_globalLabelWidth, _globalControlHeight),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = _globalControlMargin
            };
            txt_SearchKey = new TextBox
            {
                Size = new Size(280, _globalControlHeight),
                Margin = _globalControlMargin,
                Text = "输入食物名称/拼音/唯一编码检索"
            };
            Panel pnl_Search = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            lbl_Search.Location = new Point(0, 0);
            txt_SearchKey.Location = new Point(lbl_Search.Width, 0);
            pnl_Search.Controls.Add(lbl_Search);
            pnl_Search.Controls.Add(txt_SearchKey);
            tlp_Filter.Controls.Add(pnl_Search, 0, 0);
            tlp_Filter.SetColumnSpan(pnl_Search, 2);
            int row = 0;
            CreateEditItem<ComboBox>(tlp_Filter, out _, out cbo_FoodCategory, "食物分类：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Filter, out _, out cbo_GILevel, "GI值区间：", ref row, false);
            row = 1;
            CreateEditItem<ComboBox>(tlp_Filter, out _, out cbo_GLLevel, "GL值区间：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Filter, out _, out cbo_EnableStatus, "启用状态：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Filter, out _, out cbo_DataSource, "数据来源：", ref row, false);
            Panel pnl_UpdateDate = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_Date = new Label { Text = "更新时间：", Size = new Size(80, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(0, 0) };
            dtp_UpdateStart = new DateTimePicker { Location = new Point(80, 0), Size = new Size(120, _globalControlHeight), Format = DateTimePickerFormat.Short };
            dtp_UpdateEnd = new DateTimePicker { Location = new Point(210, 0), Size = new Size(120, _globalControlHeight), Format = DateTimePickerFormat.Short };
            pnl_UpdateDate.Controls.Add(lbl_Date);
            pnl_UpdateDate.Controls.Add(dtp_UpdateStart);
            pnl_UpdateDate.Controls.Add(dtp_UpdateEnd);
            tlp_Filter.Controls.Add(pnl_UpdateDate, 3, 1);
            row = 2;
            Panel pnl_FilterBtn = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            FlowLayoutPanel flp_FilterBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_Search = CreateBtn("检索", Color.FromArgb(0, 122, 204));
            btn_ResetFilter = CreateBtn("重置筛选", Color.Gray);
            btn_AddSingle = CreateBtn("新增食物", Color.FromArgb(0, 150, 136));
            btn_EditSelected = CreateBtn("编辑选中", Color.FromArgb(255, 152, 0));
            btn_DeleteSelected = CreateBtn("删除选中", Color.FromArgb(244, 67, 54));
            flp_FilterBtn.Controls.Add(btn_Search);
            flp_FilterBtn.Controls.Add(btn_ResetFilter);
            flp_FilterBtn.Controls.Add(btn_AddSingle);
            flp_FilterBtn.Controls.Add(btn_EditSelected);
            flp_FilterBtn.Controls.Add(btn_DeleteSelected);
            pnl_FilterBtn.Controls.Add(flp_FilterBtn);
            tlp_Filter.Controls.Add(pnl_FilterBtn, 0, 2);
            tlp_Filter.SetColumnSpan(pnl_FilterBtn, 4);
            grp_FoodList = new GroupBox { Text = "食物库列表（糖尿病饮食标准）", Dock = DockStyle.Fill, Padding = new Padding(15, 190, 15, 15) };
            tab_FoodList.Controls.Add(grp_FoodList);
            dgv_FoodList = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ColumnHeadersHeight = 35

            };
            grp_FoodList.Controls.Add(dgv_FoodList);

            dgv_FoodList.Columns.AddRange(new DataGridViewColumn[]
            {
    // 隐藏主键列（必须保留）
    new DataGridViewTextBoxColumn
    {
        HeaderText = "食物ID",
        DataPropertyName = "FoodID",
        Visible = false,
        Width = 0
    },
    // 基础信息列
    new DataGridViewTextBoxColumn { HeaderText = "食物编码", DataPropertyName = "FoodCode", Width = 100 },
    new DataGridViewTextBoxColumn { HeaderText = "食物名称", DataPropertyName = "FoodName", Width = 140 },
    new DataGridViewTextBoxColumn { HeaderText = "食物分类", DataPropertyName = "FoodCategory", Width = 100 },
    new DataGridViewTextBoxColumn { HeaderText = "可食部(%)", DataPropertyName = "EdibleRate", Width = 80, DefaultCellStyle = { Format = "0.0" } },
    new DataGridViewTextBoxColumn { HeaderText = "水分(g)", DataPropertyName = "WaterContent", Width = 80, DefaultCellStyle = { Format = "0.0" } },
    
    // 能量与核心营养列
    new DataGridViewTextBoxColumn { HeaderText = "热量(kcal)", DataPropertyName = "Energy_kcal", Width = 90, DefaultCellStyle = { Format = "0.0" } },
    new DataGridViewTextBoxColumn { HeaderText = "热量(kJ)", DataPropertyName = "Energy_kJ", Width = 90, DefaultCellStyle = { Format = "0.0" } },
    new DataGridViewTextBoxColumn { HeaderText = "碳水(g)", DataPropertyName = "Carbohydrate", Width = 80, DefaultCellStyle = { Format = "0.0" } },
    new DataGridViewTextBoxColumn { HeaderText = "蛋白质(g)", DataPropertyName = "Protein", Width = 80, DefaultCellStyle = { Format = "0.0" } },
    new DataGridViewTextBoxColumn { HeaderText = "脂肪(g)", DataPropertyName = "Fat", Width = 80, DefaultCellStyle = { Format = "0.0" } },
    new DataGridViewTextBoxColumn { HeaderText = "膳食纤维(g)", DataPropertyName = "DietaryFiber", Width = 90, DefaultCellStyle = { Format = "0.0" } },
    
    // 微量营养列
    new DataGridViewTextBoxColumn { HeaderText = "胆固醇(mg)", DataPropertyName = "Cholesterol", Width = 90, DefaultCellStyle = { Format = "0.0" } },
    new DataGridViewTextBoxColumn { HeaderText = "维生素C(mg)", DataPropertyName = "VitaminC", Width = 90, DefaultCellStyle = { Format = "0.0" } },
    new DataGridViewTextBoxColumn { HeaderText = "胡萝卜素(μg)", DataPropertyName = "Carotene", Width = 90, DefaultCellStyle = { Format = "0.0" } },
    new DataGridViewTextBoxColumn { HeaderText = "钠(mg)", DataPropertyName = "Sodium", Width = 80, DefaultCellStyle = { Format = "0.0" } },
    new DataGridViewTextBoxColumn { HeaderText = "钾(mg)", DataPropertyName = "Potassium", Width = 80, DefaultCellStyle = { Format = "0.0" } },
    
    // 糖尿病专属指标列（保留原有颜色标识）
    new DataGridViewTextBoxColumn { HeaderText = "GI值", DataPropertyName = "GI", Width = 70, DefaultCellStyle = { Format = "0.0" } },
    new DataGridViewTextBoxColumn { HeaderText = "GL值", DataPropertyName = "GL", Width = 70, DefaultCellStyle = { Format = "0.0" } },
    new DataGridViewTextBoxColumn { HeaderText = "交换份", DataPropertyName = "ExchangeUnit", Width = 80, DefaultCellStyle = { Format = "0.0" } },
    
    // 管理状态列
    new DataGridViewTextBoxColumn { HeaderText = "启用状态", DataPropertyName = "EnableStatus", Width = 80 },
    new DataGridViewTextBoxColumn { HeaderText = "审核状态", DataPropertyName = "AuditStatus", Width = 80 },
    new DataGridViewTextBoxColumn { HeaderText = "版本号", DataPropertyName = "Version", Width = 80 },
    new DataGridViewTextBoxColumn { HeaderText = "数据来源", DataPropertyName = "DataSourceInfo", Width = 110 },
    
    // 时间与操作人列
    new DataGridViewTextBoxColumn { HeaderText = "创建时间", DataPropertyName = "CreateTime", Width = 120 },
    new DataGridViewTextBoxColumn { HeaderText = "创建人", DataPropertyName = "CreateUser", Width = 90 },
    new DataGridViewTextBoxColumn { HeaderText = "更新时间", DataPropertyName = "UpdateTime", Width = 120 },
    new DataGridViewTextBoxColumn { HeaderText = "更新人", DataPropertyName = "UpdateUser", Width = 90 },
    
    // 操作列
    new DataGridViewButtonColumn { HeaderText = "操作", Text = "编辑", UseColumnTextForButtonValue = true, Width = 70 }
            });

            // 新增：允许用户调整列宽，适配多列显示
            dgv_FoodList.AllowUserToResizeColumns = true;
            dgv_FoodList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        }


        private void InitFoodEditPage()
        {
            try
            {
                // ===================== 基础信息区域 =====================
                grp_BaseInfo = new GroupBox();
                grp_BaseInfo.Text = "基础信息";
                grp_BaseInfo.Dock = DockStyle.Top;
                grp_BaseInfo.Height = 200;
                grp_BaseInfo.Padding = _globalGroupBoxPadding;
                tab_FoodEdit.Controls.Add(grp_BaseInfo);

                TableLayoutPanel tlp_Base = new TableLayoutPanel();
                tlp_Base.Dock = DockStyle.Fill;
                tlp_Base.Margin = Padding.Empty;
                tlp_Base.RowCount = 3;
                tlp_Base.ColumnCount = 2;
                tlp_Base.ColumnStyles.Clear();
                tlp_Base.RowStyles.Clear();
                for (int i = 0; i < 2; i++)
                    tlp_Base.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                for (int i = 0; i < 3; i++)
                    tlp_Base.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
                grp_BaseInfo.Controls.Add(tlp_Base);

                int row = 0;
                CreateEditItem<TextBox>(tlp_Base, out _, out txt_FoodCode, "食物唯一编码：", ref row, true);
                CreateEditItem<TextBox>(tlp_Base, out _, out txt_FoodName, "食物名称：", ref row, false);
                CreateEditItem<TextBox>(tlp_Base, out _, out txt_Alias, "别名：", ref row, false);
                CreateEditItem<ComboBox>(tlp_Base, out _, out cbo_EditCategory, "食物分类：", ref row, false);
                CreateEditItem<TextBox>(tlp_Base, out _, out txt_EdibleRatio, "可食部比例(%)：", ref row, false);
                CreateEditItem<TextBox>(tlp_Base, out _, out txt_DataSourceInfo, "数据来源：", ref row, false);

                // 图片和参考依据区域（补全缺失控件）
                Panel pnl_ImageRef = new Panel();
                pnl_ImageRef.Dock = DockStyle.Bottom;
                pnl_ImageRef.Height = 80;
                pnl_ImageRef.Margin = Padding.Empty;

                pb_FoodImage = new PictureBox();
                pb_FoodImage.Size = new Size(60, 60);
                pb_FoodImage.Location = new Point(10, 10);
                pb_FoodImage.BorderStyle = BorderStyle.FixedSingle;
                pb_FoodImage.SizeMode = PictureBoxSizeMode.Zoom;

                btn_UploadImage = CreateBtn("上传图片", Color.FromArgb(0, 122, 204));
                btn_UploadImage.Size = new Size(80, 28);
                btn_UploadImage.Location = new Point(80, 25);

                Label lbl_Reference = new Label();
                lbl_Reference.Text = "参考依据：";
                lbl_Reference.Size = new Size(_globalLabelWidth, _globalControlHeight);
                lbl_Reference.TextAlign = ContentAlignment.MiddleLeft;
                lbl_Reference.Location = new Point(170, 10);

                txt_Reference = new TextBox();
                txt_Reference.Size = new Size(280, _globalControlHeight);
                txt_Reference.Location = new Point(290, 10);
                txt_Reference.Margin = _globalControlMargin;
                txt_Reference.Text = "《中国食物成分表》";

                pnl_ImageRef.Controls.Add(pb_FoodImage);
                pnl_ImageRef.Controls.Add(btn_UploadImage);
                pnl_ImageRef.Controls.Add(lbl_Reference);
                pnl_ImageRef.Controls.Add(txt_Reference);
                grp_BaseInfo.Controls.Add(pnl_ImageRef);

                // ===================== 营养成分区域 =====================
                grp_Nutrition = new GroupBox();
                grp_Nutrition.Text = "营养成分（每100g可食部）";
                grp_Nutrition.Dock = DockStyle.Top;
                grp_Nutrition.Height = 180;
                grp_Nutrition.Padding = _globalGroupBoxPadding;
                tab_FoodEdit.Controls.Add(grp_Nutrition);

                TableLayoutPanel tlp_Nutrition = new TableLayoutPanel();
                tlp_Nutrition.Dock = DockStyle.Fill;
                tlp_Nutrition.Margin = Padding.Empty;
                tlp_Nutrition.RowCount = 2;
                tlp_Nutrition.ColumnCount = 3;
                tlp_Nutrition.ColumnStyles.Clear();
                tlp_Nutrition.RowStyles.Clear();
                for (int i = 0; i < 3; i++)
                    tlp_Nutrition.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
                for (int i = 0; i < 2; i++)
                    tlp_Nutrition.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
                grp_Nutrition.Controls.Add(tlp_Nutrition);

                row = 0;
                CreateEditItem<TextBox>(tlp_Nutrition, out _, out txt_Calorie, "热量(kcal)：", ref row, false);
                CreateEditItem<TextBox>(tlp_Nutrition, out _, out txt_Carb, "碳水化合物(g)：", ref row, false);
                CreateEditItem<TextBox>(tlp_Nutrition, out _, out txt_Protein, "蛋白质(g)：", ref row, false);
                CreateEditItem<TextBox>(tlp_Nutrition, out _, out txt_Fat, "脂肪(g)：", ref row, false);
                CreateEditItem<TextBox>(tlp_Nutrition, out _, out txt_Fiber, "膳食纤维(g)：", ref row, false);
                CreateEditItem<TextBox>(tlp_Nutrition, out _, out txt_Sodium, "钠(mg)：", ref row, false);

                // ===================== 糖尿病专属指标区域 =====================
                grp_DiabetesIndex = new GroupBox();
                grp_DiabetesIndex.Text = "糖尿病专属指标";
                grp_DiabetesIndex.Dock = DockStyle.Top;
                grp_DiabetesIndex.Height = 180;
                grp_DiabetesIndex.Padding = _globalGroupBoxPadding;
                tab_FoodEdit.Controls.Add(grp_DiabetesIndex);

                TableLayoutPanel tlp_Diabetes = new TableLayoutPanel();
                tlp_Diabetes.Dock = DockStyle.Fill;
                tlp_Diabetes.Margin = Padding.Empty;
                tlp_Diabetes.RowCount = 2;
                tlp_Diabetes.ColumnCount = 2;
                tlp_Diabetes.ColumnStyles.Clear();
                tlp_Diabetes.RowStyles.Clear();
                for (int i = 0; i < 2; i++)
                    tlp_Diabetes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                for (int i = 0; i < 2; i++)
                    tlp_Diabetes.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
                grp_DiabetesIndex.Controls.Add(tlp_Diabetes);

                row = 0;
                CreateEditItem<TextBox>(tlp_Diabetes, out _, out txt_GI, "GI值(血糖生成指数)：", ref row, false);
                CreateEditItem<TextBox>(tlp_Diabetes, out _, out txt_GL, "GL值(血糖负荷)：", ref row, false);
                CreateEditItem<TextBox>(tlp_Diabetes, out _, out txt_ExchangeUnit, "糖尿病交换份：", ref row, false);
                CreateEditItem<TextBox>(tlp_Diabetes, out _, out txt_GlycemicFeature, "升糖特点标注：", ref row, false);

                // GL自动计算按钮
                Panel pnl_GLCalculate = new Panel();
                Button btn_CalculateGL = new Button();
                pnl_GLCalculate.Dock = DockStyle.Fill;
                pnl_GLCalculate.Margin = Padding.Empty;
                btn_CalculateGL.Text = "自动计算GL";
                btn_CalculateGL.BackColor = Color.FromArgb(103, 58, 183);
                btn_CalculateGL.ForeColor = Color.White;
                btn_CalculateGL.Size = new Size(100, 28);
                btn_CalculateGL.Location = new Point(260, 0);
                pnl_GLCalculate.Controls.Add(txt_GL);
                pnl_GLCalculate.Controls.Add(btn_CalculateGL);
                tlp_Diabetes.Controls.Remove(txt_GL);
                tlp_Diabetes.Controls.Add(pnl_GLCalculate, 1, 0);

                // 自动计算GL事件
                btn_CalculateGL.Click += (s, e) =>
                {
                    try
                    {
                        if (!decimal.TryParse(txt_GI.Text.Trim(), out decimal gi) || gi <= 0)
                        {
                            MessageBox.Show("请先填写有效的GI值", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        if (!decimal.TryParse(txt_Carb.Text.Trim(), out decimal carb) || carb < 0)
                        {
                            MessageBox.Show("请先填写有效的碳水化合物值", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        if (!decimal.TryParse(txt_Fiber.Text.Trim(), out decimal fiber) || fiber < 0)
                        {
                            MessageBox.Show("请先填写有效的膳食纤维值", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        decimal availableCarb = Math.Max(0, carb - fiber);
                        decimal gl = Math.Round((gi * availableCarb) / 100, 2);
                        txt_GL.Text = gl.ToString();

                        if (decimal.TryParse(txt_Calorie.Text.Trim(), out decimal calorie) && calorie > 0)
                        {
                            decimal exchangeUnit = Math.Round(90 / calorie * 100, 2);
                            txt_ExchangeUnit.Text = exchangeUnit.ToString();
                            MessageBox.Show($"计算完成！\nGL值：{gl}\n交换份：{exchangeUnit}g/份", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show($"GL值计算完成：{gl}\n（请填写热量值以自动计算交换份）", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"计算失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                // ===================== 临床适配信息区域 =====================
                grp_ClinicalAdapt = new GroupBox();
                grp_ClinicalAdapt.Text = "糖尿病临床适配信息";
                grp_ClinicalAdapt.Dock = DockStyle.Top;
                grp_ClinicalAdapt.Height = 220;
                grp_ClinicalAdapt.Padding = _globalGroupBoxPadding;
                tab_FoodEdit.Controls.Add(grp_ClinicalAdapt);

                TableLayoutPanel tlp_Clinical = new TableLayoutPanel();
                tlp_Clinical.Dock = DockStyle.Fill;
                tlp_Clinical.Margin = Padding.Empty;
                tlp_Clinical.RowCount = 3;
                tlp_Clinical.ColumnCount = 2;
                tlp_Clinical.ColumnStyles.Clear();
                tlp_Clinical.RowStyles.Clear();
                for (int i = 0; i < 2; i++)
                    tlp_Clinical.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                for (int i = 0; i < 3; i++)
                    tlp_Clinical.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
                grp_ClinicalAdapt.Controls.Add(tlp_Clinical);

                row = 0;
                CreateEditItem<TextBox>(tlp_Clinical, out _, out txt_SuitablePeople, "适用人群：", ref row, false);
                CreateEditItem<TextBox>(tlp_Clinical, out _, out txt_ForbiddenPeople, "禁忌人群：", ref row, false);
                CreateEditItem<TextBox>(tlp_Clinical, out _, out txt_RecommendAmount, "推荐食用量：", ref row, false);
                CreateEditItem<TextBox>(tlp_Clinical, out _, out txt_CookingSuggest, "烹饪方式建议：", ref row, false);
                CreateEditItem<TextBox>(tlp_Clinical, out _, out txt_GlucoseTip, "血糖影响提示：", ref row, false);

                // ===================== 状态与版本管理区域 =====================
                grp_StatusVersion = new GroupBox();
                grp_StatusVersion.Text = "状态与版本管理";
                grp_StatusVersion.Dock = DockStyle.Top;
                grp_StatusVersion.Height = 180;
                grp_StatusVersion.Padding = _globalGroupBoxPadding;
                tab_FoodEdit.Controls.Add(grp_StatusVersion);

                TableLayoutPanel tlp_Status = new TableLayoutPanel();
                tlp_Status.Dock = DockStyle.Fill;
                tlp_Status.Margin = Padding.Empty;
                tlp_Status.RowCount = 2;
                tlp_Status.ColumnCount = 2;
                tlp_Status.ColumnStyles.Clear();
                tlp_Status.RowStyles.Clear();
                for (int i = 0; i < 2; i++)
                    tlp_Status.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                for (int i = 0; i < 2; i++)
                    tlp_Status.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
                grp_StatusVersion.Controls.Add(tlp_Status);

                row = 0;
                CreateEditItem<ComboBox>(tlp_Status, out _, out cbo_EditEnableStatus, "启用状态：", ref row, false);
                CreateEditItem<TextBox>(tlp_Status, out _, out txt_Version, "版本号：", ref row, true);
                CreateEditItem<TextBox>(tlp_Status, out _, out txt_UpdateLog, "更新日志：", ref row, false);
                CreateEditItem<TextBox>(tlp_Status, out _, out txt_AuditRecord, "审核记录：", ref row, true);

                // ===================== 底部按钮 =====================
                Panel pnl_EditBtn = new Panel();
                FlowLayoutPanel flp_EditBtn = new FlowLayoutPanel();
                pnl_EditBtn.Dock = DockStyle.Top;
                pnl_EditBtn.Height = 60;
                pnl_EditBtn.Padding = new Padding(15);
                tab_FoodEdit.Controls.Add(pnl_EditBtn);

                flp_EditBtn.Dock = DockStyle.Fill;
                flp_EditBtn.FlowDirection = FlowDirection.LeftToRight;

                btn_SaveEdit = CreateBtn("保存修改", Color.FromArgb(0, 122, 204));
                btn_CancelEdit = CreateBtn("取消编辑", Color.Gray);

                flp_EditBtn.Controls.Add(btn_SaveEdit);
                flp_EditBtn.Controls.Add(btn_CancelEdit);
                pnl_EditBtn.Controls.Add(flp_EditBtn);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化编辑页面失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitBatchOperatePage()
        {
            grp_BatchImport = new GroupBox { Text = "批量导入/导出", Dock = DockStyle.Top, Height = 150, Padding = _globalGroupBoxPadding };
            tab_BatchOperate.Controls.Add(grp_BatchImport);

            Panel pnl_Import = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_ImportTip = new Label { Text = "支持Excel格式批量导入（模板可下载），导出所有/选中食物数据", Location = new Point(10, 10), AutoSize = true, Font = new Font("微软雅黑", 9F) };
            btn_ImportExcel = CreateBtn("批量导入", Color.FromArgb(0, 150, 136));
            btn_ImportExcel.Location = new Point(10, 50);
            btn_ExportExcel = CreateBtn("批量导出", Color.FromArgb(255, 152, 0));
            btn_ExportExcel.Location = new Point(130, 50);
            pnl_Import.Controls.Add(lbl_ImportTip);
            pnl_Import.Controls.Add(btn_ImportExcel);
            pnl_Import.Controls.Add(btn_ExportExcel);
            grp_BatchImport.Controls.Add(pnl_Import);

            grp_BatchOperate = new GroupBox { Text = "批量状态操作", Dock = DockStyle.Top, Height = 150, Padding = _globalGroupBoxPadding };
            tab_BatchOperate.Controls.Add(grp_BatchOperate);

            Panel pnl_Operate = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_OperateTip = new Label { Text = "请先在【食物库列表】选中需要操作的记录，再执行以下批量操作", Location = new Point(10, 10), AutoSize = true, Font = new Font("微软雅黑", 9F) };
            btn_BatchEnable = CreateBtn("批量启用", Color.FromArgb(0, 122, 204));
            btn_BatchEnable.Location = new Point(10, 50);
            btn_BatchDisable = CreateBtn("批量禁用", Color.FromArgb(255, 152, 0));
            btn_BatchDisable.Location = new Point(130, 50);
            btn_Deduplicate = CreateBtn("重复数据去重", Color.FromArgb(244, 67, 54));
            btn_Deduplicate.Location = new Point(250, 50);
            pnl_Operate.Controls.Add(lbl_OperateTip);
            pnl_Operate.Controls.Add(btn_BatchEnable);
            pnl_Operate.Controls.Add(btn_BatchDisable);
            pnl_Operate.Controls.Add(btn_Deduplicate);
            grp_BatchOperate.Controls.Add(pnl_Operate);
        }

        private void InitAuditPage()
        {
            grp_AuditFilter = new GroupBox { Text = "审核筛选条件", Dock = DockStyle.Top, Height = 150, Padding = _globalGroupBoxPadding };
            tab_Audit.Controls.Add(grp_AuditFilter);

            TableLayoutPanel tlp_AuditFilter = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_AuditFilter.RowCount = 1;
            tlp_AuditFilter.ColumnCount = 3;
            tlp_AuditFilter.ColumnStyles.Clear();
            tlp_AuditFilter.RowStyles.Clear();
            for (int i = 0; i < 3; i++) tlp_AuditFilter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
            tlp_AuditFilter.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_AuditFilter.Controls.Add(tlp_AuditFilter);

            int row = 0;
            CreateEditItem<ComboBox>(tlp_AuditFilter, out _, out cbo_AuditStatus, "审核状态：", ref row, false);
            CreateEditItem<ComboBox>(tlp_AuditFilter, out _, out cbo_Uploader, "上传人：", ref row, false);

            Panel pnl_AuditBtn = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            FlowLayoutPanel flp_AuditBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            btn_QueryAudit = CreateBtn("查询待审核", Color.FromArgb(0, 122, 204));
            btn_ResetAudit = CreateBtn("重置", Color.Gray);
            flp_AuditBtn.Controls.Add(btn_ResetAudit);
            flp_AuditBtn.Controls.Add(btn_QueryAudit);
            pnl_AuditBtn.Controls.Add(flp_AuditBtn);
            tlp_AuditFilter.Controls.Add(pnl_AuditBtn, 2, 0);

            grp_AuditList = new GroupBox { Text = "待审核数据列表", Dock = DockStyle.Fill, Padding = _globalGroupBoxPadding };
            tab_Audit.Controls.Add(grp_AuditList);

            dgv_AuditList = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToOrderColumns = false,
                // 👇 新增
                ScrollBars = ScrollBars.Both,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
            };
            grp_AuditList.Controls.Add(dgv_AuditList);

            dgv_AuditList.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { HeaderText = "食物编码", DataPropertyName = "FoodCode", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "食物名称", DataPropertyName = "FoodName", Width = 150 },
                new DataGridViewTextBoxColumn { HeaderText = "上传人", DataPropertyName = "Uploader", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "上传时间", DataPropertyName = "UploadTime", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "审核状态", DataPropertyName = "AuditStatus", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "备注", DataPropertyName = "Remark", Width = 200 }
            });

            Panel pnl_AuditOperate = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(15) };
            tab_Audit.Controls.Add(pnl_AuditOperate);
            FlowLayoutPanel flp_AuditOperate = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_AuditPass = CreateBtn("审核通过", Color.FromArgb(0, 150, 136));
            btn_AuditReject = CreateBtn("审核驳回", Color.FromArgb(244, 67, 54));
            flp_AuditOperate.Controls.Add(btn_AuditPass);
            flp_AuditOperate.Controls.Add(btn_AuditReject);
            pnl_AuditOperate.Controls.Add(flp_AuditOperate);
        }

        private void InitVersionPage()
        {
            grp_VersionFilter = new GroupBox { Text = "版本筛选条件", Dock = DockStyle.Top, Height = 150, Padding = _globalGroupBoxPadding };
            tab_Version.Controls.Add(grp_VersionFilter);

            TableLayoutPanel tlp_VersionFilter = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_VersionFilter.RowCount = 1;
            tlp_VersionFilter.ColumnCount = 2;
            tlp_VersionFilter.ColumnStyles.Clear();
            tlp_VersionFilter.RowStyles.Clear();
            for (int i = 0; i < 2; i++) tlp_VersionFilter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_VersionFilter.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_VersionFilter.Controls.Add(tlp_VersionFilter);

            int row = 0;
            CreateEditItem<ComboBox>(tlp_VersionFilter, out _, out cbo_VersionFood, "选择食物：", ref row, false);

            Panel pnl_VersionBtn = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            FlowLayoutPanel flp_VersionBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            btn_QueryVersion = CreateBtn("查询版本", Color.FromArgb(0, 122, 204));
            btn_ResetVersion = CreateBtn("重置", Color.Gray);
            flp_VersionBtn.Controls.Add(btn_ResetVersion);
            flp_VersionBtn.Controls.Add(btn_QueryVersion);
            pnl_VersionBtn.Controls.Add(flp_VersionBtn);
            tlp_VersionFilter.Controls.Add(pnl_VersionBtn, 1, 0);

            grp_VersionList = new GroupBox { Text = "食物版本记录", Dock = DockStyle.Fill, Padding = _globalGroupBoxPadding };
            tab_Version.Controls.Add(grp_VersionList);

            dgv_VersionList = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToOrderColumns = false,
                // 👇 新增
                ScrollBars = ScrollBars.Both,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
            };
            grp_VersionList.Controls.Add(dgv_VersionList);

            dgv_VersionList.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { HeaderText = "版本号", DataPropertyName = "Version", Width = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "更新时间", DataPropertyName = "UpdateTime", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "更新人", DataPropertyName = "UpdateUser", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "更新内容", DataPropertyName = "UpdateContent", Width = 200 },
                new DataGridViewTextBoxColumn { HeaderText = "审核状态", DataPropertyName = "AuditStatus", Width = 100 }
            });

            Panel pnl_VersionOperate = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(15) };
            tab_Version.Controls.Add(pnl_VersionOperate);
            FlowLayoutPanel flp_VersionOperate = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_RollbackVersion = CreateBtn("版本回滚", Color.FromArgb(255, 152, 0));
            flp_VersionOperate.Controls.Add(btn_RollbackVersion);
            pnl_VersionOperate.Controls.Add(flp_VersionOperate);
        }
        #endregion

        #region 通用控件创建方法
        private Button CreateBtn(string text, Color backColor)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.BackColor = backColor;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Size = new Size(_globalButtonWidth, _globalButtonHeight);
            btn.Font = this.Font;
            btn.Margin = _globalControlMargin;
            return btn;
        }

        private void CreateEditItem<T>(TableLayoutPanel tlp, out Label lbl, out T ctrl, string text, ref int row, bool readOnly = false)
            where T : Control, new()
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
            ctrl.Name = $"ctrl_{text.Replace("：", "").Replace(" ", "")}";

            if (ctrl is TextBox t)
            {
                t.ReadOnly = readOnly;
                t.BackColor = readOnly ? Color.FromArgb(245, 245, 245) : Color.White;
                if (text.Contains("编码") || text.Contains("版本") || text.Contains("审核"))
                {
                    t.ReadOnly = true;
                    t.Enabled = false;
                }
                t.Text = text.Contains("编码") ? "系统自动生成" : (text.Contains("参考") ? "《中国食物成分表》" : "");
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
            pairPanel.Controls.Add(lbl);
            pairPanel.Controls.Add(ctrl);

            int currentColumn = row % 2;
            int currentRow = row / 2;
            tlp.Controls.Add(pairPanel, currentColumn, currentRow);
            row++;
        }
        #endregion

        #region 下拉数据初始化
        private void InitControlData()
        {
            string[] foodCategories = { "全部", "谷薯类", "蔬菜类", "水果类", "肉蛋类", "水产类", "豆类及制品", "奶及奶制品", "油脂类", "坚果类", "调味品" };
            string[] giLevels = { "全部", "低GI(<55)", "中GI(55-70)", "高GI(>70)" };
            string[] glLevels = { "全部", "低GL(<10)", "中GL(10-20)", "高GL(>20)" };
            string[] enableStatus = { "全部", "启用", "禁用" };
            string[] dataSources = { "全部", "中国食物成分表", "临床实测", "营养师录入", "用户上传" };
            string[] auditStatus = { "全部", "待审核", "审核通过", "审核驳回" };
            string[] uploaders = { "全部", "营养师A", "营养师B", "管理员", "用户1", "用户2" };
            string[] foodNames = { "全部", "米饭", "馒头", "西兰花", "苹果", "鸡蛋", "三文鱼", "豆腐", "牛奶", "花生油", "核桃" };

            cbo_FoodCategory.Items.AddRange(foodCategories);
            cbo_GILevel.Items.AddRange(giLevels);
            cbo_GLLevel.Items.AddRange(glLevels);
            cbo_EnableStatus.Items.AddRange(enableStatus);
            cbo_DataSource.Items.AddRange(dataSources);
            cbo_EditCategory.Items.AddRange(foodCategories);
            cbo_EditEnableStatus.Items.AddRange(enableStatus);
            cbo_AuditStatus.Items.AddRange(auditStatus);
            cbo_Uploader.Items.AddRange(uploaders);
            cbo_VersionFood.Items.AddRange(foodNames);

            cbo_FoodCategory.SelectedIndex = 0;
            cbo_GILevel.SelectedIndex = 0;
            cbo_GLLevel.SelectedIndex = 0;
            cbo_EnableStatus.SelectedIndex = 0;
            cbo_DataSource.SelectedIndex = 0;
            cbo_EditCategory.SelectedIndex = 0;
            cbo_EditEnableStatus.SelectedIndex = 1;
            cbo_AuditStatus.SelectedIndex = 1;
            cbo_Uploader.SelectedIndex = 0;
            cbo_VersionFood.SelectedIndex = 0;

            dtp_UpdateStart.Value = DateTime.Now.AddMonths(-3);
            dtp_UpdateEnd.Value = DateTime.Now;
        }
        #endregion

        #region 事件绑定（重写，实现真实业务逻辑）
        private void BindAllEvents()
        {
            #region 食物库列表页 - 核心事件
            #region 食物库列表页 - 核心事件
            // 检索按钮：按筛选条件查询数据（带缓存清除）
            btn_Search.Click += (s, e) =>
            {
                try
                {
                    // 清除缓存，强制重新查询
                    _cachedFoodList = null;
                    BindFoodListData();
                    int rowCount = dgv_FoodList.RowCount;
                    MessageBox.Show($"食物库检索完成！共查询到{rowCount}条数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"检索失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 重置筛选按钮：清空筛选条件，刷新列表（带缓存清除）
            btn_ResetFilter.Click += (s, e) =>
            {
                try
                {
                    txt_SearchKey.Text = "输入食物名称/拼音/唯一编码检索";
                    cbo_FoodCategory.SelectedIndex = 0;
                    cbo_GILevel.SelectedIndex = 0;
                    cbo_GLLevel.SelectedIndex = 0;
                    cbo_EnableStatus.SelectedIndex = 0;
                    cbo_DataSource.SelectedIndex = 0;
                    dtp_UpdateStart.Value = DateTime.Now.AddMonths(-3);
                    dtp_UpdateEnd.Value = DateTime.Now;

                    // 清除缓存
                    _cachedFoodList = null;
                    BindFoodListData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"重置筛选失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 新增食物按钮：清空编辑页，跳转到编辑tab
            btn_AddSingle.Click += (s, e) =>
            {
                try
                {
                    ClearEditForm(); // 清空编辑页
                    tabMain.SelectedTab = tab_FoodEdit;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"打开新增页面失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 编辑选中按钮：加载选中行数据，跳转到编辑页
            btn_EditSelected.Click += (s, e) =>
            {
                try
                {
                    if (dgv_FoodList.SelectedRows.Count == 0)
                    {
                        MessageBox.Show("请先选中一条要编辑的食物记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 获取选中行的主键ID
                    DataGridViewRow selectedRow = dgv_FoodList.SelectedRows[0];
                    int foodId = Convert.ToInt32(selectedRow.Cells["FoodID"].Value);
                    // 加载食物详情到编辑页
                    LoadFoodDetailToEdit(foodId);
                    // 跳转到编辑页
                    tabMain.SelectedTab = tab_FoodEdit;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"打开编辑页面失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 删除选中按钮：逻辑删除选中的食物
            btn_DeleteSelected.Click += (s, e) =>
            {
                try
                {
                    if (dgv_FoodList.SelectedRows.Count == 0)
                    {
                        MessageBox.Show("请先选中要删除的食物数据！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 获取选中行信息
                    DataGridViewRow selectedRow = dgv_FoodList.SelectedRows[0];
                    int foodId = Convert.ToInt32(selectedRow.Cells["FoodID"].Value);
                    string foodName = selectedRow.Cells["FoodName"].Value.ToString();

                    // 二次确认
                    if (MessageBox.Show($"确认删除食物【{foodName}】？删除后可在版本管理中恢复！", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        // 调用BLL层执行删除
                        var result = _bFood.DeleteFood(foodId, _currentLoginUser);
                        if (result.IsSuccess)
                        {
                            MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            // 删除后刷新列表
                            BindFoodListData();
                        }
                        else
                        {
                            MessageBox.Show(result.Message, "删除失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除操作失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 列表行内编辑按钮点击事件
            dgv_FoodList.CellContentClick += (s, e) =>
            {
                try
                {
                    // 点击的是操作列的编辑按钮
                    if (e.ColumnIndex == dgv_FoodList.Columns["操作"].Index && e.RowIndex >= 0)
                    {
                        int foodId = Convert.ToInt32(dgv_FoodList.Rows[e.RowIndex].Cells["FoodID"].Value);
                        LoadFoodDetailToEdit(foodId);
                        tabMain.SelectedTab = tab_FoodEdit;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"编辑操作失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            #endregion

            #region 食物编辑页 - 核心事件
            // 保存修改按钮：新增/编辑食物，提交审核
            btn_SaveEdit.Click += (s, e) =>
            {
                try
                {
                    // 1. 基础校验
                    if (string.IsNullOrWhiteSpace(txt_FoodName.Text))
                    {
                        MessageBox.Show("食物名称不能为空", "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (cbo_EditCategory.SelectedIndex == 0)
                    {
                        MessageBox.Show("请选择食物分类", "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(txt_UpdateLog.Text))
                    {
                        MessageBox.Show("请填写更新日志，说明修改/新增内容", "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 2. 构建食物实体对象
                    FoodNutrition food = new FoodNutrition
                    {
                        FoodID = _currentEditFoodId,
                        FoodName = txt_FoodName.Text.Trim(),
                        Alias = txt_Alias.Text.Trim(),
                        FoodCategory = cbo_EditCategory.SelectedItem.ToString(),
                        EdibleRate = decimal.TryParse(txt_EdibleRatio.Text.Trim(), out decimal er) ? er : (decimal?)null,
                        DataSourceInfo = txt_DataSourceInfo.Text.Trim(),
                        Reference = txt_Reference.Text.Trim(),
                        FoodImagePath = _currentFoodImagePath,
                        Energy_kcal = decimal.TryParse(txt_Calorie.Text.Trim(), out decimal cal) ? cal : (decimal?)null,
                        Carbohydrate = decimal.TryParse(txt_Carb.Text.Trim(), out decimal carb) ? carb : (decimal?)null,
                        Protein = decimal.TryParse(txt_Protein.Text.Trim(), out decimal pro) ? pro : (decimal?)null,
                        Fat = decimal.TryParse(txt_Fat.Text.Trim(), out decimal fat) ? fat : (decimal?)null,
                        DietaryFiber = decimal.TryParse(txt_Fiber.Text.Trim(), out decimal fiber) ? fiber : (decimal?)null,
                        Sodium = decimal.TryParse(txt_Sodium.Text.Trim(), out decimal na) ? na : (decimal?)null,
                        GI = decimal.TryParse(txt_GI.Text.Trim(), out decimal gi) ? gi : (decimal?)null,
                        GL = decimal.TryParse(txt_GL.Text.Trim(), out decimal gl) ? gl : (decimal?)null,
                        ExchangeUnit = decimal.TryParse(txt_ExchangeUnit.Text.Trim(), out decimal eu) ? eu : (decimal?)null,
                        GlycemicFeature = txt_GlycemicFeature.Text.Trim(),
                        SuitablePeople = txt_SuitablePeople.Text.Trim(),
                        ForbiddenPeople = txt_ForbiddenPeople.Text.Trim(),
                        RecommendAmount = txt_RecommendAmount.Text.Trim(),
                        CookingSuggest = txt_CookingSuggest.Text.Trim(),
                        GlucoseTip = txt_GlucoseTip.Text.Trim(),
                        EnableStatus = cbo_EditEnableStatus.SelectedItem.ToString(),
                        UpdateLog = txt_UpdateLog.Text.Trim()
                    };

                    // 3. 调用BLL层，区分新增/编辑
                    BizResult result;
                    if (_currentEditFoodId == 0)
                    {
                        // 新增食物
                        result = _bFood.AddFood(food, _currentLoginUser);
                    }
                    else
                    {
                        // 编辑食物
                        result = _bFood.UpdateFood(food, _currentLoginUser, txt_UpdateLog.Text.Trim());
                    }

                    // 4. 处理结果
                    if (result.IsSuccess)
                    {
                        MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        tabMain.SelectedTab = tab_FoodList;
                        // 刷新列表和审核列表
                        BindFoodListData();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存操作失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 取消编辑按钮：清空编辑页，返回列表
            btn_CancelEdit.Click += (s, e) =>
            {
                try
                {
                    ClearEditForm();
                    tabMain.SelectedTab = tab_FoodList;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"取消编辑失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 上传图片按钮：选择图片，保存路径
            btn_UploadImage.Click += (s, e) =>
            {
                try
                {
                    using (OpenFileDialog ofd = new OpenFileDialog { Filter = "图片文件|*.png;*.jpg;*.jpeg", Title = "选择食物图片" })
                    {
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            _currentFoodImagePath = ofd.FileName;
                            pb_FoodImage.Image = Image.FromFile(ofd.FileName);
                            MessageBox.Show("图片上传成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"图片上传失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            #endregion

            #region 批量操作页 - 核心事件
            // 批量启用按钮
            btn_BatchEnable.Click += (s, e) =>
            {
                try
                {
                    if (dgv_FoodList.SelectedRows.Count == 0)
                    {
                        MessageBox.Show("请先在食物库列表选中需要启用的记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 收集选中的食物ID
                    List<int> foodIdList = new List<int>();
                    foreach (DataGridViewRow row in dgv_FoodList.SelectedRows)
                    {
                        foodIdList.Add(Convert.ToInt32(row.Cells["FoodID"].Value));
                    }

                    // 调用BLL层
                    var result = _bFood.BatchUpdateEnableStatus(foodIdList, "启用", _currentLoginUser);
                    if (result.IsSuccess)
                    {
                        MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        BindFoodListData();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"批量启用失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 批量禁用按钮
            btn_BatchDisable.Click += (s, e) =>
            {
                try
                {
                    if (dgv_FoodList.SelectedRows.Count == 0)
                    {
                        MessageBox.Show("请先在食物库列表选中需要禁用的记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    List<int> foodIdList = new List<int>();
                    foreach (DataGridViewRow row in dgv_FoodList.SelectedRows)
                    {
                        foodIdList.Add(Convert.ToInt32(row.Cells["FoodID"].Value));
                    }

                    var result = _bFood.BatchUpdateEnableStatus(foodIdList, "禁用", _currentLoginUser);
                    if (result.IsSuccess)
                    {
                        MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        BindFoodListData();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"批量禁用失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 重复数据去重按钮
            btn_Deduplicate.Click += (s, e) =>
            {
                try
                {
                    if (MessageBox.Show("确认执行重复数据去重？将按食物名称+分类去重，保留最新版本数据", "确认去重", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        var result = _bFood.DeduplicateFoods();
                        if (result.IsSuccess)
                        {
                            MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            BindFoodListData();
                        }
                        else
                        {
                            MessageBox.Show(result.Message, "去重失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"去重操作失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 批量导入/导出按钮：保留原有弹窗，可扩展Excel逻辑（NPOI/EPPlus）
            btn_ImportExcel.Click += (s, e) =>
            {
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Excel文件|*.xlsx;*.xls", Title = "选择食物数据Excel文件" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        MessageBox.Show("导入功能已对接，可扩展Excel解析逻辑，当前文件：" + ofd.FileName, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };
            btn_ExportExcel.Click += (s, e) =>
            {
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "Excel文件|*.xlsx", Title = "保存食物数据", FileName = $"食物库数据_{DateTime.Now:yyyyMMddHHmmss}.xlsx" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        MessageBox.Show("导出功能已对接，可扩展Excel生成逻辑，保存路径：" + sfd.FileName, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };
            #endregion

            #region 审核页、版本管理页事件（可按相同逻辑扩展，这里补充核心逻辑）
            // 审核页-查询按钮
            btn_QueryAudit.Click += (s, e) =>
            {
                try
                {
                    string auditStatus = cbo_AuditStatus.SelectedItem?.ToString() ?? "全部";
                    string uploader = cbo_Uploader.SelectedItem?.ToString() ?? "全部";
                    var result = _bAudit.GetAuditList(auditStatus, uploader);
                    if (result.IsSuccess)
                    {
                        dgv_AuditList.DataSource = result.Data as DataTable;
                        dgv_AuditList.ClearSelection();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "查询失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"查询审核列表失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 版本管理页-查询按钮
            btn_QueryVersion.Click += (s, e) =>
            {
                try
                {
                    if (cbo_VersionFood.SelectedIndex == 0)
                    {
                        MessageBox.Show("请选择要查询版本的食物", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    string foodName = cbo_VersionFood.SelectedItem.ToString();
                    var foodResult = _bFood.GetFoodList(foodName, "全部", "全部", "全部", "全部", "全部", DateTime.Now.AddYears(-10), DateTime.Now);
                    if (!foodResult.IsSuccess || (foodResult.Data as DataTable).Rows.Count == 0)
                    {
                        MessageBox.Show("未找到该食物数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    int foodId = Convert.ToInt32((foodResult.Data as DataTable).Rows[0]["FoodID"]);
                    var versionResult = _bVersion.GetVersionList(foodId);
                    if (versionResult.IsSuccess)
                    {
                        dgv_VersionList.DataSource = versionResult.Data as DataTable;
                        dgv_VersionList.ClearSelection();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"查询版本失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // 剩余审核通过、驳回、版本回滚、重置按钮，可按上面的逻辑，对接BLL层对应方法即可
            btn_ResetAudit.Click += (s, e) => { cbo_AuditStatus.SelectedIndex = 1; cbo_Uploader.SelectedIndex = 0; };
            btn_ResetVersion.Click += (s, e) => { cbo_VersionFood.SelectedIndex = 0; dgv_VersionList.DataSource = null; };
            #endregion
        }
        #endregion
        #endregion

        #region 系统自带的初始化方法
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "食物库管理";
            this.ResumeLayout(false);
        }

        /// <summary>
        /// 绑定食物库列表数据（优化版：加载更稳 + GI/GL颜色标识）
        /// 完全保留原有逻辑，仅做稳定性与显示优化
        /// </summary>
        private void BindFoodListData()
        {
            try
            {
                // 1. 获取当前页面所有筛选条件（完全沿用你原有取值逻辑）
                string searchKey = txt_SearchKey.Text.Trim();
                string category = cbo_FoodCategory.SelectedItem == null ? "全部" : cbo_FoodCategory.SelectedItem.ToString();
                string giLevel = cbo_GILevel.SelectedItem == null ? "全部" : cbo_GILevel.SelectedItem.ToString();
                string glLevel = cbo_GLLevel.SelectedItem == null ? "全部" : cbo_GLLevel.SelectedItem.ToString();
                string enableStatus = cbo_EnableStatus.SelectedItem == null ? "全部" : cbo_EnableStatus.SelectedItem.ToString();
                string dataSource = cbo_DataSource.SelectedItem == null ? "全部" : cbo_DataSource.SelectedItem.ToString();
                DateTime updateStart = dtp_UpdateStart.Value;
                DateTime updateEnd = dtp_UpdateEnd.Value;

                // 2. 调用业务层获取数据（完全沿用你原有调用方式）
                var result = _bFood.GetFoodList(
                    searchKey,
                    category,
                    giLevel,
                    glLevel,
                    enableStatus,
                    dataSource,
                    updateStart,
                    updateEnd);

                // 3. 结果处理（完全保留你原有提示逻辑）
                if (!result.IsSuccess || result.Data == null)
                {
                    dgv_FoodList.DataSource = null;
                    MessageBox.Show(result.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 4. 绑定数据到表格
                DataTable dt = result.Data as DataTable;
                dgv_FoodList.DataSource = dt;
                dgv_FoodList.ClearSelection();

                // ========== 4. 安全设置GI/GL颜色（修复：不重复绑定、不空引用）==========
                if (dt == null || dt.Rows.Count == 0) return;
                if (!dgv_FoodList.Columns.Contains("GI") || !dgv_FoodList.Columns.Contains("GL")) return;

                foreach (DataGridViewRow row in dgv_FoodList.Rows)
                {
                    if (row.IsNewRow) continue;

                    // 处理GI颜色（安全判断：DBNull + 格式转换）
                    if (row.Cells["GI"].Value != null && row.Cells["GI"].Value != DBNull.Value)
                    {
                        if (decimal.TryParse(row.Cells["GI"].Value.ToString(), out decimal gi))
                        {
                            row.Cells["GI"].Style.ForeColor = gi < 55 ? _colorLow : (gi <= 70 ? _colorMiddle : _colorHigh);
                        }
                    }

                    // 处理GL颜色
                    if (row.Cells["GL"].Value != null && row.Cells["GL"].Value != DBNull.Value)
                    {
                        if (decimal.TryParse(row.Cells["GL"].Value.ToString(), out decimal gl))
                        {
                            row.Cells["GL"].Style.ForeColor = gl < 10 ? _colorLow : (gl <= 20 ? _colorMiddle : _colorHigh);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载食物列表失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dgv_FoodList.DataSource = null;
            }
        }
        #endregion

        #region 编辑页辅助方法（新增）
        /// <summary>
        /// 加载食物详情到编辑表单
        /// </summary>
        private void LoadFoodDetailToEdit(int foodId)
        {
            try
            {
                var result = _bFood.GetFoodDetailById(foodId);
                if (!result.IsSuccess)
                {
                    MessageBox.Show(result.Message, "加载失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                FoodNutrition food = result.Data as FoodNutrition;
                _currentEditFoodId = foodId;
                _currentFoodImagePath = food.FoodImagePath;

                // 基础信息赋值
                txt_FoodCode.Text = food.FoodCode;
                txt_FoodName.Text = food.FoodName;
                txt_Alias.Text = food.Alias;
                cbo_EditCategory.SelectedItem = food.FoodCategory;
                txt_EdibleRatio.Text = food.EdibleRate?.ToString() ?? "";
                txt_DataSourceInfo.Text = food.DataSourceInfo;
                txt_Reference.Text = food.Reference;

                // 营养成分赋值
                txt_Calorie.Text = food.Energy_kcal?.ToString() ?? "";
                txt_Carb.Text = food.Carbohydrate?.ToString() ?? "";
                txt_Protein.Text = food.Protein?.ToString() ?? "";
                txt_Fat.Text = food.Fat?.ToString() ?? "";
                txt_Fiber.Text = food.DietaryFiber?.ToString() ?? "";
                txt_Sodium.Text = food.Sodium?.ToString() ?? "";

                // 糖尿病指标赋值
                txt_GI.Text = food.GI?.ToString() ?? "";
                txt_GL.Text = food.GL?.ToString() ?? "";
                txt_ExchangeUnit.Text = food.ExchangeUnit?.ToString() ?? "";
                txt_GlycemicFeature.Text = food.GlycemicFeature;

                // 临床适配信息赋值
                txt_SuitablePeople.Text = food.SuitablePeople;
                txt_ForbiddenPeople.Text = food.ForbiddenPeople;
                txt_RecommendAmount.Text = food.RecommendAmount;
                txt_CookingSuggest.Text = food.CookingSuggest;
                txt_GlucoseTip.Text = food.GlucoseTip;

                // 状态版本赋值
                cbo_EditEnableStatus.SelectedItem = food.EnableStatus;
                txt_Version.Text = food.Version;
                txt_AuditRecord.Text = food.AuditRecord;
                txt_UpdateLog.Clear();

                // 图片加载
                if (!string.IsNullOrEmpty(food.FoodImagePath) && File.Exists(food.FoodImagePath))
                {
                    pb_FoodImage.Image = Image.FromFile(food.FoodImagePath);
                }
                else
                {
                    pb_FoodImage.Image = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载食物详情失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 清空编辑表单，用于新增食物
        /// </summary>
        private void ClearEditForm()
        {
            _currentEditFoodId = 0;
            _currentFoodImagePath = string.Empty;

            // 清空所有可编辑文本框
            txt_FoodName.Clear();
            txt_Alias.Clear();
            txt_EdibleRatio.Clear();
            txt_DataSourceInfo.Clear();
            txt_Reference.Clear();
            txt_Calorie.Clear();
            txt_Carb.Clear();
            txt_Protein.Clear();
            txt_Fat.Clear();
            txt_Fiber.Clear();
            txt_Sodium.Clear();
            txt_GI.Clear();
            txt_GL.Clear();
            txt_ExchangeUnit.Clear();
            txt_GlycemicFeature.Clear();
            txt_SuitablePeople.Clear();
            txt_ForbiddenPeople.Clear();
            txt_RecommendAmount.Clear();
            txt_CookingSuggest.Clear();
            txt_GlucoseTip.Clear();
            txt_UpdateLog.Clear();
            txt_AuditRecord.Clear();

            // 重置下拉框和只读字段
            txt_FoodCode.Text = "系统自动生成";
            txt_Version.Text = "V1.0.0";
            cbo_EditCategory.SelectedIndex = 0;
            cbo_EditEnableStatus.SelectedIndex = 1;
            pb_FoodImage.Image = null;
        }
        #endregion
    }
}