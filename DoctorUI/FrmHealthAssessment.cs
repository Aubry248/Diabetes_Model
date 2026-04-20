using BLL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DoctorUI
{
    public partial class FrmHealthAssessment : Form
    {
        #region ========== 全局统一布局参数（核心：所有控件尺寸/位置的全局基准，一键适配全页面）==========
        /// <summary>
        /// 【最外层主容器内边距】控制窗体边缘与内容区域的留白
        /// 格式：Padding(左, 上, 右, 下)
        /// 调整说明：顶部内边距从默认15改为5，解决页面整体下移、顶部留白过多的核心问题
        /// 左右/下保留10-15，保证内容不贴窗体边缘，视觉更舒适
        /// 后续修改：如需缩小左右留白，减小左右数值；如需调整页面顶部距离，修改第二个数值
        /// </summary>
        private readonly Padding _globalMainContainerPadding = new Padding(10, 1, 10, 10);
        /// <summary>
        /// 【内容全局居中开关】控制整个页面内容是否在窗体中水平+垂直居中
        /// 调整说明：false=靠左固定偏移，true=窗体缩放时自动居中
        /// 后续修改：如需页面始终居中，改为true即可
        /// </summary>
        private readonly bool _globalContentAutoCenter = false;
        /// <summary>
        /// 【内容全局偏移量】控制整个页面内容的强制偏移，是页面整体右移/下移的根源参数
        /// 调整说明：X=水平偏移（数值越大越靠右），Y=垂直偏移（数值越大越靠下）
        /// 修复逻辑：Y从100改为0，彻底解决页面整体下移问题；X从250改为10，解决页面整体右移问题
        /// 后续修改：如需页面整体右移，增大X值；如需整体下移，增大Y值（不建议超过20）
        /// </summary>
        private readonly int _globalContentOffsetX = 120;
        private readonly int _globalContentOffsetY = 80;
        /// <summary>
        /// 【内容最小尺寸】保证窗体缩小时，内容不会被挤压变形
        /// 调整说明：最小宽度1100、高度700，适配主流电脑分辨率，避免控件被挤到看不见
        /// 后续修改：如需适配更小分辨率，可减小数值；适配大屏可增大数值
        /// </summary>
        private readonly int _globalContentMinWidth = 700;
        private readonly int _globalContentMinHeight = 800;
        /// <summary>
        /// 【控件统一外边距】所有输入框、按钮、标签之间的默认间距
        /// 调整说明：上下左右5px，保证控件之间不贴边，视觉更整洁
        /// 后续修改：如需控件间距更大，增大数值；更紧凑则减小数值
        /// </summary>
        private readonly Padding _globalControlMargin = new Padding(5, 5, 5, 5);
        /// <summary>
        /// 【输入控件统一高度】所有文本框、下拉框、日期控件的默认高度
        /// 调整说明：28px适配9.5号字体，避免文字被上下截断
        /// 后续修改：字体放大时，同步增大该数值，保证文字完整显示
        /// </summary>
        private readonly int _globalControlHeight = 28;
        /// <summary>
        /// 【按钮统一高度】全页面所有按钮的固定高度，保证样式统一
        /// 调整说明：36px比输入框略高，突出按钮层级，点击区域更大更易用
        /// </summary>
        private readonly int _globalButtonHeight = 50;
        /// <summary>
        /// 【按钮统一宽度】全页面所有按钮的固定宽度，保证样式统一
        /// 调整说明：110px适配中文按钮文字，避免文字被左右截断
        /// 后续修改：按钮文字更长时，增大该数值
        /// </summary>
        private readonly int _globalButtonWidth = 110;
        /// <summary>
        /// 【标签统一宽度】所有输入项前的说明标签固定宽度
        /// 调整说明：120px保证最长的标签文字（如"糖化血红蛋白："）完整显示，同时让所有输入框左对齐
        /// 后续修改：标签文字更长时，增大该数值
        /// </summary>
        private readonly int _globalLabelWidth = 120;
        /// <summary>
        /// 【表格行统一高度】所有TableLayoutPanel的行高，保证每行输入项垂直间距一致
        /// 调整说明：40px比控件高度多12px，上下留白均匀，不会出现行与行贴在一起的问题
        /// </summary>
        private readonly int _globalRowHeight = 40;
        /// <summary>
        /// 【分组框统一内边距】所有GroupBox的内部留白，保证分组框内的控件不贴边框
        /// 调整说明：上下左右15px，视觉更透气，避免控件贴分组框边缘
        /// </summary>
        private readonly Padding _globalGroupBoxPadding = new Padding(15);
        /// <summary>
        /// 【可折叠卡片折叠后高度】卡片收起时的高度，仅显示标题栏
        /// 调整说明：40px刚好容纳标题和折叠按钮，不会过高浪费空间
        /// </summary>
        private readonly int _cardCollapsedHeight = 40;
        /// <summary>
        /// 【可折叠卡片展开后高度】卡片展开时的高度，保证内部操作栏、图表、列表完整显示
        /// 调整说明：320px适配内部所有控件，避免内容被截断
        /// 后续修改：卡片内内容更多时，增大该数值
        /// </summary>
        private readonly int _cardExpandedHeight = 320;
        #endregion

        #region 核心控件声明
        // 顶部患者选择区域（新增核心模块）
        private GroupBox grp_PatientSelector;
        private ComboBox cbo_PatientSearch;
        private Label lbl_PatientTip;
        private Button btn_RefreshPatient;

        // 主容器与标签控件
        private Panel pnlMainContainer;
        private Panel pnlContentWrapper;
        private TabControl tabMain;
        private TabPage tab_BaselineOverview, tab_DiabetesRisk, tab_ComplicationRisk, tab_HealthData;

        #region 标签页1：评估基线数据总览控件
        // 1.1 患者基本信息卡片（固定不可折叠）
        private GroupBox grp_PatientBaseInfo;
        private Button btn_EditArchive;
        private Label lbl_PatientName, lbl_PatientGender, lbl_PatientAge, lbl_PatientIdCard, lbl_PatientArchiveNo;
        private Label lbl_DiagnosisTime, lbl_DiseaseDuration, lbl_FamilyHistory, lbl_PastHistory, lbl_AllergyHistory;
        private Label lbl_Height, lbl_Weight, lbl_deibetetype,lbl_BMI;

        // 1.2 血糖监测记录卡片（可折叠）
        private GroupBox grp_BloodGlucoseRecord;
        private Button btn_ToggleGlucose, btn_FilterGlucose;
        private RadioButton rbtn_3Month, rbtn_6Month;
        private DataGridView dgv_GlucoseDetail;

        // 1.3 饮食行为记录卡片（可折叠）
        private GroupBox grp_DietRecord;
        private Button btn_ToggleDiet;
        private DataGridView dgv_DietDetail;

        // 1.4 运动行为记录卡片（可折叠）
        private GroupBox grp_ExerciseRecord;
        private Button btn_ToggleExercise, btn_ExportExercise;
        private DataGridView dgv_ExerciseDetail;

        // 1.5 用药治疗记录卡片（可折叠）
        private GroupBox grp_MedicationRecord;
        private Button btn_ToggleMedication, btn_ViewFullMedication;
        private DataGridView dgv_MedicationDetail;

        // 1.6 既往健康评估与干预效果记录卡片（可折叠）
        private GroupBox grp_PastAssessment;
        private Button btn_TogglePastAssessment;
        private DataGridView dgv_PastAssessment;
        #endregion

        #region 标签页2：糖尿病风险评估控件（优化原有）
        private GroupBox grp_DiabetesBase, grp_DiabetesResult;
        private TextBox txt_DiabetesAge, txt_DiabetesWeight, txt_DiabetesHeight, txt_DiabetesBMI;
        private ComboBox cbo_DiabetesFamily, cbo_DiabetesDiet, cbo_DiabetesExercise;
        private Button btn_CalcDiabetes, btn_ResetDiabetes;
        private Label lbl_DiabetesScore, lbl_DiabetesLevel;
        private TextBox txt_DiabetesConclusion;
        #endregion

        #region 标签页3：并发症风险评估控件（优化原有）
        private GroupBox grp_ComplicationBase, grp_ComplicationResult;
        private ComboBox cbo_Duration, cbo_HbA1cLevel, cbo_BloodPressure, cbo_Lipid;
        private Button btn_CalcComplication, btn_ResetComplication;
        private Label lbl_ComplicationScore, lbl_ComplicationLevel;
        private TextBox txt_ComplicationSuggestion;
        #endregion

        #region 标签页4：健康数据录入与评分控件（优化原有）
        private GroupBox grp_HealthInput, grp_HealthScore;
        private TextBox txt_FastingGlucose, txt_PostGlucose, txt_HbA1c, txt_Systolic, txt_Diastolic, txt_BMI;
        private DateTimePicker dtp_AssessDate;
        private Button btn_SaveHealth, btn_CalcHealth, btn_ResetHealth, btn_GenerateReport, btn_SyncToIntervention;
        private Label lbl_HealthTotalScore, lbl_HealthLevel;
        #endregion

        #region 新增：患者选择相关字段
        private readonly B_PatientRecord _bllPatient = new B_PatientRecord();
        private readonly B_HealthAssessment _bllHealthAssessment = new B_HealthAssessment();
        /// <summary>
        /// 当前选中的患者ID
        /// </summary>
        private int _currentSelectedPatientId = 0;
        #endregion
        #endregion

        public FrmHealthAssessment()
        {
            #region 【窗体级尺寸/缩放适配】解决高DPI下文字截断、控件错位问题
            // 按96DPI（100%缩放）为基准适配，开启系统DPI自动缩放
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            // 缩放模式设为Dpi，适配Windows系统不同缩放比例（125%/150%/200%）
            this.AutoScaleMode = AutoScaleMode.Dpi;
            // 全局统一字体，避免不同控件字体不一致导致的尺寸错位
            this.Font = new Font("微软雅黑", 9F);
            #endregion

            #region 窗体基础尺寸配置
            this.Text = "健康评估管理";
            // 窗体默认打开尺寸，比最小尺寸大200x100，默认打开更舒适
            this.Size = new Size(1300, 800);
            // 窗体最小尺寸，保证缩放到该尺寸时，控件不会被挤压消失
            this.MinimumSize = new Size(_globalContentMinWidth + 40, _globalContentMinHeight + 100);
            // 窗体打开时默认居中屏幕
            this.StartPosition = FormStartPosition.CenterScreen;
            // 填充父容器（如果是MDI子窗体，该属性生效）
            //this.Dock = DockStyle.Fill;
            #endregion

            // 全局主容器初始化
            InitMainContainer();
            // 动态创建所有控件
            InitializeDynamicControls();
            // 初始化下拉数据
            InitControlData();
            // 绑定事件
            BindAllEvents();
        }

        #region 全局容器初始化（页面根布局，控制所有内容的整体位置/尺寸）
        #region 全局容器初始化（根治横向溢出：只允许垂直滚动，禁止横向滚动）
        private void InitMainContainer()
        {
            // 主滚动容器：只开垂直滚动，彻底禁止横向滚动
            pnlMainContainer = new Panel();
            pnlMainContainer.Dock = DockStyle.Fill;
            pnlMainContainer.BackColor = Color.White;
            pnlMainContainer.Padding = _globalMainContainerPadding;
            // ✅ 关键：关闭自动滚动，手动只开垂直滚动，彻底杜绝横向溢出
            pnlMainContainer.AutoScroll = false;
            pnlMainContainer.HorizontalScroll.Enabled = false;
            pnlMainContainer.HorizontalScroll.Visible = false;
            pnlMainContainer.VerticalScroll.Enabled = true;
            pnlMainContainer.VerticalScroll.Visible = true;
            this.Controls.Add(pnlMainContainer);

            // 内容包裹容器：宽度锁死最大适配宽度，绝对不超过主容器
            pnlContentWrapper = new Panel();
            // ✅ 关键：宽度=主容器可视宽度-边距，永远不溢出
            pnlContentWrapper.Size = new Size(
                Math.Max(pnlMainContainer.ClientSize.Width - 40, _globalContentMinWidth),
                _globalContentMinHeight
            );
            pnlContentWrapper.MinimumSize = pnlContentWrapper.Size;
            pnlContentWrapper.BackColor = Color.White;
            pnlContentWrapper.Location = new Point(_globalContentOffsetX, _globalContentOffsetY);
            pnlMainContainer.Controls.Add(pnlContentWrapper);

            // 只在加载时定位1次，彻底删除所有Resize绑定，杜绝跳动+溢出
            this.Load += (s, e) =>
            {
                pnlContentWrapper.Left = Math.Max(0, _globalContentOffsetX);
                pnlContentWrapper.Top = Math.Max(0, _globalContentOffsetY);
                // 加载时再锁一次宽度，绝对不溢出
                pnlContentWrapper.Width = Math.Max(pnlMainContainer.ClientSize.Width - 5, _globalContentMinWidth);
            };

            // ❌ 彻底删除所有Resize、居中、动态计算代码，杜绝一切溢出可能
        }
        #endregion
        #endregion

        #region 动态创建所有控件（核心布局实现，每个控件的位置/尺寸都有注释）
        private void InitializeDynamicControls()
        {
            // 1. 初始化顶部患者选择区域（页面最顶部，最先创建保证在最上层）
            InitPatientSelector();
            // 2. 主标签控件（4个流程标签页，在患者选择区下方）
            tabMain = new TabControl();
            // Dock=Fill：标签控件填满内容容器的剩余空间，窗体缩放时自动适配
            tabMain.Dock = DockStyle.Fill;
            tabMain.Font = new Font("微软雅黑", 10F);
            // 标签页头部内边距，让标签文字不贴边，更美观
            tabMain.Padding = new Point(15, 12);
            // 切换标签页时自动聚焦，避免键盘操作错位
            tabMain.SelectedIndexChanged += (s, e) => { tabMain.SelectedTab?.Focus(); };

            // 按操作流程创建标签页，顺序决定标签页的显示顺序
            tab_BaselineOverview = new TabPage("评估基线数据总览") { BackColor = Color.White };
            tab_DiabetesRisk = new TabPage("糖尿病风险评估") { BackColor = Color.White };
            tab_ComplicationRisk = new TabPage("并发症风险评估") { BackColor = Color.White };
            tab_HealthData = new TabPage("健康数据录入与评分") { BackColor = Color.White };

            // 把标签页添加到标签控件
            tabMain.TabPages.AddRange(new TabPage[] { tab_BaselineOverview, tab_DiabetesRisk, tab_ComplicationRisk, tab_HealthData });
            // 标签控件添加到内容容器，在患者选择区下方
            pnlContentWrapper.Controls.Add(tabMain);

            // 3. 初始化四个标签页的内部控件
            InitBaselineOverviewPage();
            InitDiabetesRiskPage();
            InitComplicationRiskPage();
            InitHealthDataPage();
        }

        #region 顶部患者选择区域初始化（页面最顶部，第一个显示的模块）
        private void InitPatientSelector()
        {
            #region 分组框容器
            grp_PatientSelector = new GroupBox();
            grp_PatientSelector.Text = "患者选择";
            // Dock=Top：靠容器顶部停靠，保证在页面最上方
            grp_PatientSelector.Dock = DockStyle.Top;
            // 高度设为50，原70，减少顶部空间占用，避免把下方标签页挤到可视区域外
            grp_PatientSelector.Height = 70;
            // 内边距上下改为5，适配缩小后的高度，保证控件垂直居中
            grp_PatientSelector.Padding = new Padding(15, 5, 15, 5);
            grp_PatientSelector.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            // 添加到内容容器，最顶部位置
            pnlContentWrapper.Controls.Add(grp_PatientSelector);
            #endregion

            #region 【表格布局容器】替代手动Location定位，彻底解决窗体缩放时控件错位问题
            TableLayoutPanel tlp_Patient = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            // 3列布局，适配「标签+下拉框+按钮」的结构
            tlp_Patient.ColumnCount = 3;
            tlp_Patient.RowCount = 1;
            // 第一列：固定宽度，放「检索患者：」标签，宽度和全局标签宽度统一
            tlp_Patient.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, _globalLabelWidth));
            // 第二列：百分比自适应，放下拉检索框，占剩余空间的60%，保证下拉框足够宽
            tlp_Patient.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            // 第三列：固定宽度，放刷新按钮，宽度比按钮宽20px，留边距
            tlp_Patient.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, _globalButtonWidth + 20));
            // 行高100%填充，保证控件垂直居中
            tlp_Patient.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            // 添加到分组框
            grp_PatientSelector.Controls.Add(tlp_Patient);
            #endregion

            #region 标签控件
            lbl_PatientTip = new Label();
            lbl_PatientTip.Text = "检索患者：";
            // Dock=Fill：填满整个单元格，保证文字垂直居中
            lbl_PatientTip.Dock = DockStyle.Fill;
            lbl_PatientTip.TextAlign = ContentAlignment.MiddleLeft;
            lbl_PatientTip.Font = new Font("微软雅黑", 9.5F, FontStyle.Regular);
            // 添加到表格第0列第0行
            tlp_Patient.Controls.Add(lbl_PatientTip, 0, 0);
            #endregion

            #region 患者检索下拉框
            cbo_PatientSearch = new ComboBox();
            // Dock=Fill：填满整个单元格，窗体缩放时自动适配宽度
            cbo_PatientSearch.Dock = DockStyle.Fill;
            cbo_PatientSearch.Font = new Font("微软雅黑", 9.5F, FontStyle.Regular);
            // DropDown=可输入+下拉，支持检索
            cbo_PatientSearch.DropDownStyle = ComboBoxStyle.DropDown;
            // 开启自动补全，输入时自动匹配列表项
            cbo_PatientSearch.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cbo_PatientSearch.AutoCompleteSource = AutoCompleteSource.ListItems;
            // 添加到表格第1列第0行
            tlp_Patient.Controls.Add(cbo_PatientSearch, 1, 0);
            #endregion

            #region 
            btn_RefreshPatient = CreateBtn("选择患者", Color.FromArgb(0, 122, 204));
            // 靠右停靠，保证在单元格最右侧
            btn_RefreshPatient.Dock = DockStyle.Right;
            // 添加到表格第2列第0行
            tlp_Patient.Controls.Add(btn_RefreshPatient, 2, 0);
            #endregion
        }
        #endregion

        #region 标签页1：评估基线数据总览（核心模块，多卡片垂直排列）
        private void InitBaselineOverviewPage()
        {
            #region 标签页内滚动容器（修复：内容整体下移，100%生效，调多少移多少）
            Panel pnl_BaselineScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
            // ✅【唯一要改的】顶部内边距 = 下移距离（单位：像素），比如60就是下移60px，改成你要的数值
            pnl_BaselineScroll.Padding = new Padding(0, 30, 0, 0);
            tab_BaselineOverview.Controls.Add(pnl_BaselineScroll);
            #endregion

            #region 卡片流布局容器（修复宽度适配 + 单独下移）
            FlowLayoutPanel flp_BaselineContainer = new FlowLayoutPanel();
            flp_BaselineContainer.Dock = DockStyle.Top;
            flp_BaselineContainer.AutoSize = true;
            flp_BaselineContainer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flp_BaselineContainer.WrapContents = false;
            flp_BaselineContainer.FlowDirection = FlowDirection.TopDown;
            flp_BaselineContainer.BackColor = Color.White;

            // 👇 关键修复：初始宽度正确适配滚动容器
            flp_BaselineContainer.Width = pnl_BaselineScroll.ClientSize.Width - 20;
            pnl_BaselineScroll.Controls.Add(flp_BaselineContainer);
            #endregion

            #region 容器宽度自适应（修复绑定对象，正确计算）
            pnl_BaselineScroll.Resize += (s, e) =>
            {
                // 👇 关键修复：宽度=滚动容器可视宽度-40，保证卡片不溢出、按钮全显
                flp_BaselineContainer.Width = Math.Max(pnl_BaselineScroll.ClientSize.Width - 40, _globalContentMinWidth - 100);
            };
            #endregion

            // 按顺序创建卡片（不变）
            InitPatientBaseInfoCard(flp_BaselineContainer);
            InitBloodGlucoseCard(flp_BaselineContainer);
            InitDietRecordCard(flp_BaselineContainer);
            InitExerciseRecordCard(flp_BaselineContainer);
            InitMedicationRecordCard(flp_BaselineContainer);
            InitPastAssessmentCard(flp_BaselineContainer);
        }

        // 1.1 患者基本信息卡片（固定不可折叠，第一个显示的卡片）
        private void InitPatientBaseInfoCard(FlowLayoutPanel parent)
        {
            #region 卡片分组框
            grp_PatientBaseInfo = new GroupBox();
            grp_PatientBaseInfo.Text = "患者基本信息";
            // 宽度=父容器宽度-20px，左右各留10px边距，和其他卡片宽度统一
            grp_PatientBaseInfo.Width = parent.Width - 50;
            // 高度220px，适配5行3列的信息，保证所有内容完整显示
            grp_PatientBaseInfo.Height = 250;
            // 外边距10px，卡片之间不贴边
            grp_PatientBaseInfo.Margin = new Padding(10);
            // 内边距使用全局统一参数
            grp_PatientBaseInfo.Padding = _globalGroupBoxPadding;
            grp_PatientBaseInfo.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            // 添加到流容器
            parent.Controls.Add(grp_PatientBaseInfo);
            // 父容器宽度变化时，自动适配卡片宽度
            parent.Resize += (s, e) => { grp_PatientBaseInfo.Width = parent.Width - 20; };
            #endregion

            #region 编辑档案按钮
            btn_EditArchive = CreateBtn("编辑档案", Color.FromArgb(255, 152, 0));
            // 位置固定在卡片右上角，距离右边缘20px，上边缘10px
            btn_EditArchive.Location = new Point(grp_PatientBaseInfo.Width - btn_EditArchive.Width - 20, 10);
            // Anchor=Top|Right：窗体缩放时，按钮始终固定在右上角，不会错位
            btn_EditArchive.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            // 添加到卡片
            grp_PatientBaseInfo.Controls.Add(btn_EditArchive);
            #endregion

            #region 信息表格布局
            // 表格填满卡片剩余空间，顶部Margin=30px，避开右上角的编辑按钮
            TableLayoutPanel tlp_BaseInfo = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 30, 0, 0) };
            // 5行3列，和实际信息行数完全匹配，解决之前行数不匹配导致控件被挤压的问题
            tlp_BaseInfo.RowCount = 5;
            tlp_BaseInfo.ColumnCount = 3;
            // 3列均分宽度，每列33.33%，保证布局工整
            for (int i = 0; i < 3; i++)
            {
                tlp_BaseInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            }
            // 给每一行都设置固定行高，和全局行高统一，保证每行垂直间距一致
            for (int i = 0; i < 5; i++)
            {
                tlp_BaseInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            }
            // 添加到卡片
            grp_PatientBaseInfo.Controls.Add(tlp_BaseInfo);
            #endregion

            #region 信息项创建（按行添加，保证布局正确）
            int row = 0;
            // 第一行：姓名、性别、年龄
            CreateInfoItem(tlp_BaseInfo, "姓名：", out lbl_PatientName, ref row, 0);
            CreateInfoItem(tlp_BaseInfo, "性别：", out lbl_PatientGender, ref row, 1);
            CreateInfoItem(tlp_BaseInfo, "年龄：", out lbl_PatientAge, ref row, 2);
            row += 3; // 行号+3，跳到下一行
            // 第二行：身份证号、档案编号、确诊时间
            CreateInfoItem(tlp_BaseInfo, "身份证号：", out lbl_PatientIdCard, ref row, 0);
            CreateInfoItem(tlp_BaseInfo, "档案编号：", out lbl_PatientArchiveNo, ref row, 1);
            CreateInfoItem(tlp_BaseInfo, "确诊时间：", out lbl_DiagnosisTime, ref row, 2);
            row += 3;
            // 第三行：病程年限、家族史、既往病史
            CreateInfoItem(tlp_BaseInfo, "病程年限：", out lbl_DiseaseDuration, ref row, 0);
            CreateInfoItem(tlp_BaseInfo, "家族史：", out lbl_FamilyHistory, ref row, 1);
            CreateInfoItem(tlp_BaseInfo, "既往病史：", out lbl_PastHistory, ref row, 2);
            row += 3;
            // 第四行：过敏史、身高、体重
            CreateInfoItem(tlp_BaseInfo, "过敏史：", out lbl_AllergyHistory, ref row, 0);
            CreateInfoItem(tlp_BaseInfo, "身高(cm)：", out lbl_Height, ref row, 1);
            CreateInfoItem(tlp_BaseInfo, "体重(kg)：", out lbl_Weight, ref row, 2);
            CreateInfoItem(tlp_BaseInfo, "糖尿病类型：", out lbl_deibetetype, ref row, 3);
            row += 3;
            // 第五行：BMI指数
            CreateInfoItem(tlp_BaseInfo, "BMI指数：", out lbl_BMI, ref row, 0);
            #endregion
        }

        // 1.2 血糖监测记录卡片（可折叠）
        private void InitBloodGlucoseCard(FlowLayoutPanel parent)
        {
            // 创建可折叠卡片，通用方法封装了尺寸/位置逻辑
            grp_BloodGlucoseRecord = CreateCollapsibleCard("血糖监测记录", parent, out btn_ToggleGlucose);

            #region 顶部操作栏
            Panel pnl_Operate = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(10) };
            // Dock=Top：靠卡片顶部停靠，高度40px刚好容纳操作控件
            grp_BloodGlucoseRecord.Controls.Add(pnl_Operate);

            // 时间维度单选按钮，固定位置，左对齐
            rbtn_3Month = new RadioButton { Text = "近3个月", Checked = true, AutoSize = true, Location = new Point(0, 10) };
            rbtn_6Month = new RadioButton { Text = "近6个月", AutoSize = true, Location = new Point(100, 10) };
            // 筛选按钮，固定在操作栏右上角
            // 原固定定位代码删掉，替换为：
            btn_FilterGlucose = CreateBtn("筛选", Color.FromArgb(0, 122, 204));
            // 👇 改用Dock靠右，自动适配，永远显示
            btn_FilterGlucose.Dock = DockStyle.Right;
            btn_FilterGlucose.Margin = new Padding(0, 0, 10, 0);

            // 添加到操作栏
            pnl_Operate.Controls.AddRange(new Control[] { rbtn_3Month, rbtn_6Month, btn_FilterGlucose });
            #endregion

            #region 趋势图预留区域
            Panel pnl_Chart = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(248, 248, 248), Margin = new Padding(10) };
            // Dock=Top：在操作栏下方，高度100px适配折线图显示
            Label lbl_ChartTip = new Label { Text = "血糖趋势折线图区域", ForeColor = Color.Gray, AutoSize = true };
            // 文字在面板中水平垂直居中
            lbl_ChartTip.Location = new Point((pnl_Chart.Width - lbl_ChartTip.Width) / 2, (pnl_Chart.Height - lbl_ChartTip.Height) / 2);
            pnl_Chart.Controls.Add(lbl_ChartTip);
            grp_BloodGlucoseRecord.Controls.Add(pnl_Chart);
            #endregion

            #region 明细列表
            dgv_GlucoseDetail = new DataGridView();
            // Dock=Fill：填满卡片剩余空间，自动适配宽高
            dgv_GlucoseDetail.Dock = DockStyle.Fill;
            dgv_GlucoseDetail.BackgroundColor = Color.White;
            // 列宽自动填充，避免列宽过窄导致内容显示不全
            dgv_GlucoseDetail.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            // 禁止用户手动添加行、编辑单元格，保证数据只读
            dgv_GlucoseDetail.AllowUserToAddRows = false;
            dgv_GlucoseDetail.ReadOnly = true;
            grp_BloodGlucoseRecord.Controls.Add(dgv_GlucoseDetail);
            #endregion
        }

        // 1.3 饮食行为记录卡片（可折叠）
        private void InitDietRecordCard(FlowLayoutPanel parent)
        {
            // 创建可折叠卡片
            grp_DietRecord = CreateCollapsibleCard("饮食行为记录", parent, out btn_ToggleDiet);

            #region 核心汇总区域
            Panel pnl_Summary = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(10) };
            // Dock=Top：靠卡片顶部停靠，高度50px容纳汇总信息
            grp_DietRecord.Controls.Add(pnl_Summary);
            Label lbl_SummaryTip = new Label { Text = "饮食核心情况汇总区域", ForeColor = Color.Gray, AutoSize = true, Location = new Point(0, 10) };
            pnl_Summary.Controls.Add(lbl_SummaryTip);
            #endregion

            #region 明细列表
            dgv_DietDetail = new DataGridView();
            dgv_DietDetail.Dock = DockStyle.Fill;
            dgv_DietDetail.BackgroundColor = Color.White;
            dgv_DietDetail.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv_DietDetail.AllowUserToAddRows = false;
            dgv_DietDetail.ReadOnly = true;
            grp_DietRecord.Controls.Add(dgv_DietDetail);
            #endregion
        }

        // 1.4 运动行为记录卡片（可折叠）
        private void InitExerciseRecordCard(FlowLayoutPanel parent)
        {
            // 创建可折叠卡片
            grp_ExerciseRecord = CreateCollapsibleCard("运动行为记录", parent, out btn_ToggleExercise);

            #region 顶部操作栏
            Panel pnl_Operate = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(10) };
            grp_ExerciseRecord.Controls.Add(pnl_Operate);
            // 导出按钮，固定在右上角
            btn_ExportExercise = CreateBtn("导出Excel", Color.FromArgb(0, 150, 136));
            btn_ExportExercise.Dock = DockStyle.Right;
            btn_ExportExercise.Location = new Point(grp_ExerciseRecord.Width - btn_ExportExercise.Width - 20, 2);
            btn_ExportExercise.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pnl_Operate.Controls.Add(btn_ExportExercise);
            #endregion

            #region 核心汇总区域
            Panel pnl_Summary = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(10) };
            grp_ExerciseRecord.Controls.Add(pnl_Summary);
            Label lbl_SummaryTip = new Label { Text = "运动核心情况汇总区域", ForeColor = Color.Gray, AutoSize = true, Location = new Point(0, 10) };
            pnl_Summary.Controls.Add(lbl_SummaryTip);
            #endregion

            #region 明细列表
            dgv_ExerciseDetail = new DataGridView();
            dgv_ExerciseDetail.Dock = DockStyle.Fill;
            dgv_ExerciseDetail.BackgroundColor = Color.White;
            dgv_ExerciseDetail.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv_ExerciseDetail.AllowUserToAddRows = false;
            dgv_ExerciseDetail.ReadOnly = true;
            grp_ExerciseRecord.Controls.Add(dgv_ExerciseDetail);
            #endregion
        }

        // 1.5 用药治疗记录卡片（可折叠）
        private void InitMedicationRecordCard(FlowLayoutPanel parent)
        {
            // 创建可折叠卡片
            grp_MedicationRecord = CreateCollapsibleCard("用药治疗记录", parent, out btn_ToggleMedication);

            #region 顶部操作栏
            Panel pnl_Operate = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(10) };
            grp_MedicationRecord.Controls.Add(pnl_Operate);
            // 查看完整历史链接按钮，固定在右上角
            btn_ViewFullMedication = new Button();
            btn_ViewFullMedication.Text = "查看完整用药历史 →";
            btn_ViewFullMedication.FlatStyle = FlatStyle.Flat;
            btn_ViewFullMedication.FlatAppearance.BorderSize = 0;
            btn_ViewFullMedication.ForeColor = Color.FromArgb(0, 122, 204);
            btn_ViewFullMedication.BackColor = Color.Transparent;
            btn_ViewFullMedication.AutoSize = true;
            btn_ViewFullMedication.Dock = DockStyle.Right;
            btn_ViewFullMedication.Location = new Point(grp_MedicationRecord.Width - 200, 5);
            btn_ViewFullMedication.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pnl_Operate.Controls.Add(btn_ViewFullMedication);
            #endregion

            #region 当前方案汇总区域
            Panel pnl_Summary = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(10) };
            grp_MedicationRecord.Controls.Add(pnl_Summary);
            Label lbl_SummaryTip = new Label { Text = "当前用药方案汇总区域", ForeColor = Color.Gray, AutoSize = true, Location = new Point(0, 10) };
            pnl_Summary.Controls.Add(lbl_SummaryTip);
            #endregion

            #region 明细列表
            dgv_MedicationDetail = new DataGridView();
            dgv_MedicationDetail.Dock = DockStyle.Fill;
            dgv_MedicationDetail.BackgroundColor = Color.White;
            dgv_MedicationDetail.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv_MedicationDetail.AllowUserToAddRows = false;
            dgv_MedicationDetail.ReadOnly = true;
            grp_MedicationRecord.Controls.Add(dgv_MedicationDetail);
            #endregion
        }

        // 1.6 既往健康评估与干预效果记录卡片（可折叠）
        private void InitPastAssessmentCard(FlowLayoutPanel parent)
        {
            // 创建可折叠卡片
            grp_PastAssessment = CreateCollapsibleCard("既往健康评估与干预效果记录", parent, out btn_TogglePastAssessment);

            #region 明细列表
            dgv_PastAssessment = new DataGridView();
            dgv_PastAssessment.Dock = DockStyle.Fill;
            dgv_PastAssessment.BackgroundColor = Color.White;
            dgv_PastAssessment.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv_PastAssessment.AllowUserToAddRows = false;
            dgv_PastAssessment.ReadOnly = true;
            grp_PastAssessment.Controls.Add(dgv_PastAssessment);
            #endregion
        }
        #endregion

        #region 标签页2：糖尿病风险评估（优化原有）
        private void InitDiabetesRiskPage()
        {
            #region 基础信息分组框
            grp_DiabetesBase = new GroupBox { Text = "风险评估信息", Dock = DockStyle.Top, Height = 300, Padding = _globalGroupBoxPadding };
            // Dock=Top：靠标签页顶部停靠，高度300px适配7个输入项
            tab_DiabetesRisk.Controls.Add(grp_DiabetesBase);

            // 表格布局，4行2列，均分宽度
            TableLayoutPanel tlp_Diabetes = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Diabetes.RowCount = 4;
            tlp_Diabetes.ColumnCount = 2;
            tlp_Diabetes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_Diabetes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            // 每行固定高度，和全局统一
            for (int i = 0; i < 4; i++) tlp_Diabetes.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_DiabetesBase.Controls.Add(tlp_Diabetes);
            #endregion

            #region 输入项创建
            int row = 0;
            // 自动带入不可修改字段（readOnly=true）
            CreateEditItem<TextBox>(tlp_Diabetes, out _, out txt_DiabetesAge, "年龄：", ref row, true);
            CreateEditItem<ComboBox>(tlp_Diabetes, out _, out cbo_DiabetesFamily, "家族病史：", ref row, false);
            CreateEditItem<TextBox>(tlp_Diabetes, out _, out txt_DiabetesHeight, "身高(cm)：", ref row, true);
            CreateEditItem<TextBox>(tlp_Diabetes, out _, out txt_DiabetesWeight, "体重(kg)：", ref row, true);
            CreateEditItem<TextBox>(tlp_Diabetes, out _, out txt_DiabetesBMI, "BMI指数：", ref row, true);
            CreateEditItem<ComboBox>(tlp_Diabetes, out _, out cbo_DiabetesDiet, "饮食结构：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Diabetes, out _, out cbo_DiabetesExercise, "运动情况：", ref row, false);
            #endregion

            #region 按钮区
            Panel pnl_DiabetesBtn = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            // Dock=Top：在基础信息分组框下方，高度60px容纳按钮
            tab_DiabetesRisk.Controls.Add(pnl_DiabetesBtn);
            // 流布局，按钮从左到右排列
            FlowLayoutPanel flp_DiabetesBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_CalcDiabetes = CreateBtn("开始评估", Color.FromArgb(0, 122, 204));
            btn_ResetDiabetes = CreateBtn("重置", Color.Gray);
            flp_DiabetesBtn.Controls.AddRange(new Control[] { btn_CalcDiabetes, btn_ResetDiabetes });
            pnl_DiabetesBtn.Controls.Add(flp_DiabetesBtn);
            #endregion

            #region 评估结果分组框
            grp_DiabetesResult = new GroupBox { Text = "评估结果", Dock = DockStyle.Fill, Padding = _globalGroupBoxPadding };
            // Dock=Fill：填满标签页剩余空间，窗体缩放时自动适配
            tab_DiabetesRisk.Controls.Add(grp_DiabetesResult);

            // 表格布局，3行2列
            TableLayoutPanel tlp_Result = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Result.RowCount = 3;
            tlp_Result.ColumnCount = 2;
            tlp_Result.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F)); // 标题列30%宽度
            tlp_Result.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F)); // 内容列70%宽度
            tlp_Result.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            tlp_Result.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            tlp_Result.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // 结论行填满剩余空间
            grp_DiabetesResult.Controls.Add(tlp_Result);

            // 结果控件创建
            Label lbl_ScoreTitle = new Label { Text = "风险评分：", Font = new Font("微软雅黑", 11, FontStyle.Bold), AutoSize = true };
            lbl_DiabetesScore = new Label { Text = "0", Font = new Font("微软雅黑", 12, FontStyle.Bold), ForeColor = Color.Red, AutoSize = true };
            Label lbl_LevelTitle = new Label { Text = "风险等级：", Font = new Font("微软雅黑", 11, FontStyle.Bold), AutoSize = true };
            lbl_DiabetesLevel = new Label { Text = "未评估", Font = new Font("微软雅黑", 12, FontStyle.Bold), ForeColor = Color.Blue, AutoSize = true };
            Label lbl_ConclusionTitle = new Label { Text = "评估结论：", Font = new Font("微软雅黑", 11, FontStyle.Bold), AutoSize = true };
            txt_DiabetesConclusion = new TextBox { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical, Font = new Font("微软雅黑", 9.5F) };

            // 添加到表格
            tlp_Result.Controls.Add(lbl_ScoreTitle, 0, 0);
            tlp_Result.Controls.Add(lbl_DiabetesScore, 1, 0);
            tlp_Result.Controls.Add(lbl_LevelTitle, 0, 1);
            tlp_Result.Controls.Add(lbl_DiabetesLevel, 1, 1);
            tlp_Result.Controls.Add(lbl_ConclusionTitle, 0, 2);
            tlp_Result.Controls.Add(txt_DiabetesConclusion, 1, 2);
            #endregion
        }
        #endregion

        #region 标签页3：并发症风险评估（优化原有）
        private void InitComplicationRiskPage()
        {
            #region 基础信息分组框
            grp_ComplicationBase = new GroupBox { Text = "并发症评估信息", Dock = DockStyle.Top, Height = 260, Padding = _globalGroupBoxPadding };
            // Dock=Top：靠标签页顶部停靠，高度260px适配4个输入项
            tab_ComplicationRisk.Controls.Add(grp_ComplicationBase);

            // 表格布局，4行2列，均分宽度
            TableLayoutPanel tlp_Complication = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Complication.RowCount = 4;
            tlp_Complication.ColumnCount = 2;
            tlp_Complication.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_Complication.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (int i = 0; i < 4; i++) tlp_Complication.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_ComplicationBase.Controls.Add(tlp_Complication);
            #endregion

            #region 输入项创建
            int row = 0;
            // 病程年限不可修改（readOnly=true）
            CreateEditItem<ComboBox>(tlp_Complication, out _, out cbo_Duration, "病程年限：", ref row, true);
            CreateEditItem<ComboBox>(tlp_Complication, out _, out cbo_HbA1cLevel, "糖化控制：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Complication, out _, out cbo_BloodPressure, "血压情况：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Complication, out _, out cbo_Lipid, "血脂情况：", ref row, false);
            #endregion

            #region 按钮区
            Panel pnl_ComplicationBtn = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            tab_ComplicationRisk.Controls.Add(pnl_ComplicationBtn);
            FlowLayoutPanel flp_ComplicationBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_CalcComplication = CreateBtn("开始评估", Color.FromArgb(0, 122, 204));
            btn_ResetComplication = CreateBtn("重置", Color.Gray);
            flp_ComplicationBtn.Controls.AddRange(new Control[] { btn_CalcComplication, btn_ResetComplication });
            pnl_ComplicationBtn.Controls.Add(flp_ComplicationBtn);
            #endregion

            #region 结果分组框
            grp_ComplicationResult = new GroupBox { Text = "并发症风险结果", Dock = DockStyle.Fill, Padding = _globalGroupBoxPadding };
            tab_ComplicationRisk.Controls.Add(grp_ComplicationResult);

            TableLayoutPanel tlp_Result = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Result.RowCount = 3;
            tlp_Result.ColumnCount = 2;
            tlp_Result.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlp_Result.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            tlp_Result.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            tlp_Result.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            tlp_Result.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            grp_ComplicationResult.Controls.Add(tlp_Result);

            Label lbl_ScoreTitle = new Label { Text = "风险评分：", Font = new Font("微软雅黑", 11, FontStyle.Bold), AutoSize = true };
            lbl_ComplicationScore = new Label { Text = "0", Font = new Font("微软雅黑", 12, FontStyle.Bold), ForeColor = Color.Red, AutoSize = true };
            Label lbl_LevelTitle = new Label { Text = "风险等级：", Font = new Font("微软雅黑", 11, FontStyle.Bold), AutoSize = true };
            lbl_ComplicationLevel = new Label { Text = "未评估", Font = new Font("微软雅黑", 12, FontStyle.Bold), ForeColor = Color.Blue, AutoSize = true };
            Label lbl_SuggestionTitle = new Label { Text = "筛查建议：", Font = new Font("微软雅黑", 11, FontStyle.Bold), AutoSize = true };
            txt_ComplicationSuggestion = new TextBox { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical, Font = new Font("微软雅黑", 9.5F) };

            tlp_Result.Controls.Add(lbl_ScoreTitle, 0, 0);
            tlp_Result.Controls.Add(lbl_ComplicationScore, 1, 0);
            tlp_Result.Controls.Add(lbl_LevelTitle, 0, 1);
            tlp_Result.Controls.Add(lbl_ComplicationLevel, 1, 1);
            tlp_Result.Controls.Add(lbl_SuggestionTitle, 0, 2);
            tlp_Result.Controls.Add(txt_ComplicationSuggestion, 1, 2);
            #endregion
        }
        #endregion

        #region 标签页4：健康数据录入与评分（优化原有）
        private void InitHealthDataPage()
        {
            #region 数据录入分组框
            grp_HealthInput = new GroupBox { Text = "健康数据录入", Dock = DockStyle.Top, Height = 320, Padding = _globalGroupBoxPadding };
            // Dock=Top：靠标签页顶部停靠，高度320px适配7个输入项
            tab_HealthData.Controls.Add(grp_HealthInput);

            // 表格布局，7行2列，均分宽度
            TableLayoutPanel tlp_Health = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Health.RowCount = 7;
            tlp_Health.ColumnCount = 2;
            tlp_Health.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_Health.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (int i = 0; i < 7; i++) tlp_Health.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_HealthInput.Controls.Add(tlp_Health);
            #endregion

            #region 输入项创建
            int row = 0;
            CreateEditItem<DateTimePicker>(tlp_Health, out _, out dtp_AssessDate, "评估日期：", ref row, false);
            CreateEditItem<TextBox>(tlp_Health, out _, out txt_FastingGlucose, "空腹血糖：", ref row, false);
            CreateEditItem<TextBox>(tlp_Health, out _, out txt_PostGlucose, "餐后血糖：", ref row, false);
            CreateEditItem<TextBox>(tlp_Health, out _, out txt_HbA1c, "糖化血红蛋白：", ref row, false);
            CreateEditItem<TextBox>(tlp_Health, out _, out txt_Systolic, "收缩压：", ref row, false);
            CreateEditItem<TextBox>(tlp_Health, out _, out txt_Diastolic, "舒张压：", ref row, false);
            CreateEditItem<TextBox>(tlp_Health, out _, out txt_BMI, "BMI指数：", ref row, false);
            #endregion

            #region 按钮区
            Panel pnl_HealthBtn = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            tab_HealthData.Controls.Add(pnl_HealthBtn);
            FlowLayoutPanel flp_HealthBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            // 按钮按操作顺序创建，从左到右排列
            btn_CalcHealth = CreateBtn("计算评分", Color.FromArgb(0, 150, 136));
            btn_SaveHealth = CreateBtn("保存数据", Color.FromArgb(255, 152, 0));
            btn_ResetHealth = CreateBtn("重置", Color.Gray);
            btn_GenerateReport = CreateBtn("生成评估报告", Color.FromArgb(102, 126, 234));
            btn_SyncToIntervention = CreateBtn("同步至干预方案", Color.FromArgb(67, 160, 71));
            flp_HealthBtn.Controls.AddRange(new Control[] { btn_CalcHealth, btn_SaveHealth, btn_ResetHealth, btn_GenerateReport, btn_SyncToIntervention });
            pnl_HealthBtn.Controls.Add(flp_HealthBtn);
            #endregion

            #region 评分结果分组框
            grp_HealthScore = new GroupBox { Text = "健康评分结果", Dock = DockStyle.Fill, Padding = _globalGroupBoxPadding };
            tab_HealthData.Controls.Add(grp_HealthScore);

            TableLayoutPanel tlp_Result = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Result.RowCount = 2;
            tlp_Result.ColumnCount = 2;
            tlp_Result.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_Result.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_Result.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            tlp_Result.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            grp_HealthScore.Controls.Add(tlp_Result);

            Label lbl_TotalTitle = new Label { Text = "综合评分：", Font = new Font("微软雅黑", 11, FontStyle.Bold), AutoSize = true };
            lbl_HealthTotalScore = new Label { Text = "0", Font = new Font("微软雅黑", 12, FontStyle.Bold), ForeColor = Color.Green, AutoSize = true };
            Label lbl_HealthLevelTitle = new Label { Text = "健康等级：", Font = new Font("微软雅黑", 11, FontStyle.Bold), AutoSize = true };
            lbl_HealthLevel = new Label { Text = "未评分", Font = new Font("微软雅黑", 12, FontStyle.Bold), ForeColor = Color.Orange, AutoSize = true };

            tlp_Result.Controls.Add(lbl_TotalTitle, 0, 0);
            tlp_Result.Controls.Add(lbl_HealthTotalScore, 1, 0);
            tlp_Result.Controls.Add(lbl_HealthLevelTitle, 0, 1);
            tlp_Result.Controls.Add(lbl_HealthLevel, 1, 1);
            #endregion
        }
        #endregion
        #endregion

        #region 通用控件创建方法（封装所有控件的尺寸/位置逻辑，全局统一）
        /// <summary>
        /// 【按钮统一创建方法】全页面所有按钮都通过该方法创建，保证尺寸/样式完全统一
        /// </summary>
        /// <param name="text">按钮显示文字</param>
        /// <param name="backColor">按钮背景色</param>
        /// <returns>创建好的按钮控件</returns>
        private Button CreateBtn(string text, Color backColor)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.BackColor = backColor;
            btn.ForeColor = Color.White;
            // 扁平样式，去掉系统默认边框
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            // 尺寸使用全局统一参数，保证所有按钮大小一致
            btn.Size = new Size(_globalButtonWidth, _globalButtonHeight);
            btn.Font = this.Font;
            // 外边距使用全局统一参数，保证按钮之间间距一致
            btn.Margin = _globalControlMargin;
            return btn;
        }

        /// <summary>
        /// 【输入项统一创建方法】所有标签+输入框的组合都通过该方法创建，保证布局/尺寸完全统一
        /// </summary>
        /// <typeparam name="T">输入控件类型（TextBox/ComboBox/DateTimePicker）</typeparam>
        /// <param name="tlp">父表格容器</param>
        /// <param name="lbl">输出创建好的标签控件</param>
        /// <param name="ctrl">输出创建好的输入控件</param>
        /// <param name="text">标签显示文字</param>
        /// <param name="row">引用传递的行号，自动递增</param>
        /// <param name="readOnly">是否只读</param>
        private void CreateEditItem<T>(TableLayoutPanel tlp, out Label lbl, out T ctrl, string text, ref int row, bool readOnly) where T : Control, new()
        {
            #region 标签控件
            lbl = new Label
            {
                Text = text,
                // 宽度使用全局统一标签宽度，保证所有标签左对齐
                Size = new Size(_globalLabelWidth, _globalControlHeight),
                TextAlign = ContentAlignment.MiddleLeft,
                // 外边距全局统一
                Margin = _globalControlMargin,
                Font = this.Font
            };
            #endregion

            #region 输入控件
            ctrl = new T();
            // 宽度固定260px，适配输入内容，避免过窄显示不全
            ctrl.Size = new Size(260, _globalControlHeight);
            ctrl.Margin = _globalControlMargin;
            ctrl.Font = this.Font;

            // 不同控件的只读样式适配
            if (ctrl is TextBox t)
            {
                t.ReadOnly = readOnly;
                // 只读控件背景色变灰，视觉区分可编辑/不可编辑
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
            #endregion

            #region 组合面板（包裹标签+输入框，保证表格内布局稳定）
            Panel pairPanel = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            // 标签固定在面板最左侧
            lbl.Location = new Point(0, 0);
            // 输入框固定在标签右侧，无缝衔接
            ctrl.Location = new Point(lbl.Width, 0);
            pairPanel.Controls.AddRange(new Control[] { lbl, ctrl });
            // 按行号奇偶，分别放到左右两列，实现双列布局
            tlp.Controls.Add(pairPanel, row % 2 == 0 ? 0 : 1, row / 2);
            // 行号自动递增
            row++;
            #endregion
        }

        /// <summary>
        /// 【信息展示项统一创建方法】用于只读的信息展示，保证布局/尺寸统一
        /// </summary>
        /// <param name="tlp">父表格容器</param>
        /// <param name="labelText">标签文字</param>
        /// <param name="valueLabel">输出创建好的值标签</param>
        /// <param name="row">引用传递的行号</param>
        /// <param name="column">要放入的列号</param>
        private void CreateInfoItem(TableLayoutPanel tlp, string labelText, out Label valueLabel, ref int row, int column)
        {
            Panel panel = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            // 标签控件，加粗显示标题
            Label lbl = new Label
            {
                Text = labelText,
                AutoSize = true, // 自动适配文字宽度，避免截断
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("微软雅黑", 9.5F, FontStyle.Bold),
                Location = new Point(0, 5) // 距离顶部5px，垂直居中
            };
            // 值标签，普通字体显示内容
            valueLabel = new Label
            {
                Text = "--",
                AutoSize = true, // 自动适配内容宽度，避免长内容截断
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("微软雅黑", 9.5F),
                ForeColor = Color.FromArgb(64, 64, 64),
                Location = new Point(lbl.Width + 10, 5) // 距离标签10px，不贴边
            };
            panel.Controls.AddRange(new Control[] { lbl, valueLabel });
            // 添加到表格指定列和行
            tlp.Controls.Add(panel, column, row / 3);
        }

        /// <summary>
        /// 【可折叠卡片统一创建方法】封装卡片的尺寸、折叠按钮、布局逻辑
        /// </summary>
        /// <param name="title">卡片标题</param>
        /// <param name="parent">父流容器</param>
        /// <param name="toggleBtn">输出创建好的折叠按钮</param>
        /// <returns>创建好的卡片分组框</returns>
        private GroupBox CreateCollapsibleCard(string title, FlowLayoutPanel parent, out Button toggleBtn)
        {
            #region 卡片分组框（修复宽度，自动适配，不溢出）
            GroupBox groupBox = new GroupBox();
            groupBox.Text = title;
            // 👇 关键修复：宽度=父容器宽度-40，预留边距，按钮全显
            groupBox.Width = parent.Width - 40;
            groupBox.Height = _cardExpandedHeight;
            groupBox.Margin = new Padding(10);
            groupBox.Padding = new Padding(15);
            groupBox.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            parent.Controls.Add(groupBox);

            // 👇 关键修复：宽度跟随父容器正确变化
            parent.Resize += (s, e) => { groupBox.Width = parent.Width - 40; };
            #endregion

            #region 折叠/展开按钮（改用Dock靠右，放弃固定坐标，永不消失）
            toggleBtn = new Button();
            toggleBtn.Text = "收起 ↑";
            toggleBtn.FlatStyle = FlatStyle.Flat;
            toggleBtn.FlatAppearance.BorderSize = 0;
            toggleBtn.ForeColor = Color.FromArgb(0, 122, 204);
            toggleBtn.BackColor = Color.Transparent;
            toggleBtn.AutoSize = true;
            // 👇 关键修复：Dock靠右，自动适配卡片宽度，永远显示在右上角
            toggleBtn.Dock = DockStyle.Top;
            toggleBtn.TextAlign = ContentAlignment.MiddleRight;
            toggleBtn.Padding = new Padding(0, 0, 10, 0);
            toggleBtn.Tag = "expanded";
            groupBox.Controls.Add(toggleBtn);
            #endregion

            #region 折叠/展开事件（不变）
            toggleBtn.Click += (s, e) =>
            {
                var btn = s as Button;
                if (btn == null || btn.Tag == null) return;
                if (btn.Tag.ToString() == "expanded")
                {
                    groupBox.Height = _cardCollapsedHeight;
                    btn.Text = "展开 ↓";
                    btn.Tag = "collapsed";
                }
                else
                {
                    groupBox.Height = _cardExpandedHeight;
                    btn.Text = "收起 ↑";
                    btn.Tag = "expanded";
                }
            };
            #endregion
            return groupBox;
        }
        #endregion

        #region 下拉数据初始化（糖尿病专业评估项，兼容原有）
        private void InitControlData()
        {
            // 糖尿病风险
            cbo_DiabetesFamily.Items.AddRange(new string[] { "无", "父母一方", "父母双方" });
            cbo_DiabetesDiet.Items.AddRange(new string[] { "均衡", "高糖高脂", "高碳水" });
            cbo_DiabetesExercise.Items.AddRange(new string[] { "规律", "偶尔", "几乎不" });
            // 并发症风险
            cbo_Duration.Items.AddRange(new string[] { "小于5年", "5-10年", "10-20年", "大于20年" });
            cbo_HbA1cLevel.Items.AddRange(new string[] { "≤7.0%", "7.0%-9.0%", ">9.0%" });
            cbo_BloodPressure.Items.AddRange(new string[] { "正常", "临界高压", "高血压" });
            cbo_Lipid.Items.AddRange(new string[] { "正常", "异常" });
            // 默认选中第一项
            cbo_DiabetesFamily.SelectedIndex = 0;
            cbo_Duration.SelectedIndex = 0;
            cbo_DiabetesDiet.SelectedIndex = 0;
            cbo_DiabetesExercise.SelectedIndex = 0;
            cbo_HbA1cLevel.SelectedIndex = 0;
            cbo_BloodPressure.SelectedIndex = 0;
            cbo_Lipid.SelectedIndex = 0;
        }
        #endregion

        #region 事件绑定（评估/计算/保存/重置，预留业务逻辑入口）
        private void BindAllEvents()
        {
            // 患者选择核心事件
            cbo_PatientSearch.SelectedIndexChanged += (s, e) =>
            {
                // 预留：选中患者后自动加载全量基线数据
                MessageBox.Show("患者已选中，将自动加载基线数据！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            btn_RefreshPatient.Click += (s, e) =>
            {
                // 预留：刷新患者列表
                cbo_PatientSearch.Items.Clear();
                MessageBox.Show("患者列表已刷新！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            // 糖尿病风险评估
            btn_CalcDiabetes.Click += (s, e) => CalcDiabetesRisk();
            btn_ResetDiabetes.Click += (s, e) => ResetDiabetes();
            // 并发症风险评估
            btn_CalcComplication.Click += (s, e) => CalcComplicationRisk();
            btn_ResetComplication.Click += (s, e) => ResetComplication();
            // 健康数据评分
            btn_CalcHealth.Click += (s, e) => CalcHealthScore();
            btn_SaveHealth.Click += (s, e) => MessageBox.Show("健康数据保存完成！已同步至基线数据总览", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            btn_ResetHealth.Click += (s, e) => ResetHealth();
            // 新增按钮事件预留
            btn_GenerateReport.Click += (s, e) => MessageBox.Show("健康评估报告已生成，支持预览/打印/导出PDF", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            btn_SyncToIntervention.Click += (s, e) => MessageBox.Show("本次评估全量数据已同步至「干预方案制定」模块！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            btn_EditArchive.Click += (s, e) => MessageBox.Show("已跳转至患者档案管理模块", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // 原有糖尿病风险计算逻辑（保留兼容）
        private void CalcDiabetesRisk()
        {
            int score = 0;
            if (int.TryParse(txt_DiabetesAge.Text, out int age)) score += age > 40 ? 20 : 10;
            if (cbo_DiabetesFamily.SelectedIndex == 1) score += 25;
            if (cbo_DiabetesFamily.SelectedIndex == 2) score += 40;
            if (cbo_DiabetesDiet.SelectedIndex == 2) score += 15;
            if (cbo_DiabetesExercise.SelectedIndex == 2) score += 15;
            lbl_DiabetesScore.Text = score.ToString();
            lbl_DiabetesLevel.Text = score < 30 ? "低风险" : score < 60 ? "中风险" : "高风险";
            lbl_DiabetesLevel.ForeColor = score < 30 ? Color.Green : score < 60 ? Color.Orange : Color.Red;
            txt_DiabetesConclusion.Text = $"经评估，患者糖尿病风险为{lbl_DiabetesLevel.Text}，风险评分为{score}分。";
        }

        // 原有并发症风险计算逻辑（保留兼容）
        private void CalcComplicationRisk()
        {
            int score = 0;
            score += cbo_Duration.SelectedIndex * 15;
            score += cbo_HbA1cLevel.SelectedIndex * 20;
            score += cbo_BloodPressure.SelectedIndex * 15;
            score += cbo_Lipid.SelectedIndex * 10;
            lbl_ComplicationScore.Text = score.ToString();
            lbl_ComplicationLevel.Text = score < 20 ? "低风险" : score < 40 ? "中风险" : "高风险";
            lbl_ComplicationLevel.ForeColor = score < 20 ? Color.Green : score < 40 ? Color.Orange : Color.Red;
            txt_ComplicationSuggestion.Text = $"经评估，患者糖尿病并发症风险为{lbl_ComplicationLevel.Text}，建议定期完成并发症相关筛查，严控血糖、血压、血脂等危险因素。";
        }

        // 原有健康数据评分逻辑（保留兼容）
        private void CalcHealthScore()
        {
            int total = 100;
            if (double.TryParse(txt_FastingGlucose.Text, out double fg) && fg > 7.0) total -= 15;
            if (double.TryParse(txt_PostGlucose.Text, out double pg) && pg > 11.1) total -= 15;
            if (double.TryParse(txt_HbA1c.Text, out double hb) && hb > 7.0) total -= 20;
            if (double.TryParse(txt_BMI.Text, out double bmi) && (bmi < 18.5 || bmi > 24)) total -= 10;
            lbl_HealthTotalScore.Text = total.ToString();
            lbl_HealthLevel.Text = total >= 80 ? "优秀" : total >= 60 ? "合格" : "不合格";
            lbl_HealthLevel.ForeColor = total >= 80 ? Color.Green : total >= 60 ? Color.Orange : Color.Red;
        }

        // 原有重置方法（优化适配新增控件）
        private void ResetDiabetes()
        {
            txt_DiabetesAge.Clear();
            txt_DiabetesHeight.Clear();
            txt_DiabetesWeight.Clear();
            txt_DiabetesBMI.Clear();
            cbo_DiabetesFamily.SelectedIndex = 0;
            cbo_DiabetesDiet.SelectedIndex = 0;
            cbo_DiabetesExercise.SelectedIndex = 0;
            lbl_DiabetesScore.Text = "0";
            lbl_DiabetesLevel.Text = "未评估";
            txt_DiabetesConclusion.Clear();
        }
        private void ResetComplication()
        {
            cbo_Duration.SelectedIndex = 0;
            cbo_HbA1cLevel.SelectedIndex = 0;
            cbo_BloodPressure.SelectedIndex = 0;
            cbo_Lipid.SelectedIndex = 0;
            lbl_ComplicationScore.Text = "0";
            lbl_ComplicationLevel.Text = "未评估";
            txt_ComplicationSuggestion.Clear();
        }
        private void ResetHealth()
        {
            txt_FastingGlucose.Clear();
            txt_PostGlucose.Clear();
            txt_HbA1c.Clear();
            txt_Systolic.Clear();
            txt_Diastolic.Clear();
            txt_BMI.Clear();
            dtp_AssessDate.Value = DateTime.Now;
            lbl_HealthTotalScore.Text = "0";
            lbl_HealthLevel.Text = "未评分";
        }
        #endregion
    }
}