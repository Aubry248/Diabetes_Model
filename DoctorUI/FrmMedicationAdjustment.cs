using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using BLL;
using Model;

namespace DoctorUI
{
    public partial class FrmMedicationAdjustment : Form
    {
        #region ========== 原有全局布局参数完全保留，无任何改动 ==========
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
        #endregion

        #region ========== 原有控件声明完全保留，无任何改动 ==========
        private Panel pnlMainContainer;
        private Panel pnlContentWrapper;
        private TabControl tabMain;
        private TabPage tab_MedicationRecord, tab_MedicationAdjust, tab_PlanSave;
        // 用药记录查看
        private GroupBox grp_RecordQuery;
        private ComboBox cbo_RecordPatient;
        private DateTimePicker dtp_RecordStart, dtp_RecordEnd;
        private DataGridView dgv_MedicationRecord;
        private Button btn_QueryRecord, btn_ResetRecord;
        // 血糖联动用药调整
        private GroupBox grp_PatientSelect, grp_BloodGlucose, grp_AdjustContent;
        private ComboBox cbo_AdjustPatient;
        private DataGridView dgv_BloodGlucose;
        private TextBox txt_MedicationName, txt_Dosage, txt_AdjustReason, txt_AdjustContent;
        private ComboBox cbo_MedicationType;
        private Button btn_LoadGlucose, btn_CalcAdjust, btn_SaveAdjust;
        // 用药方案保存
        private GroupBox grp_PlanList;
        private DataGridView dgv_MedicationPlan;
        private Button btn_ViewPlan, btn_SavePlan;
        #endregion

        #region 新增：业务层对象与全局业务变量
        private readonly B_Medicine _medicineBll = new B_Medicine();
        // 当前登录医生ID（与项目全局登录逻辑保持一致，登录后赋值）
        public int CurrentDoctorId { get; set; } = 1;
        // 当前登录医生姓名
        public string CurrentDoctorName { get; set; } = "系统管理员";
        #endregion

        public FrmMedicationAdjustment()
        {
            // 窗体基础配置完全保留
            this.Text = "用药调整管理";
            this.Size = new Size(1300, 800);
            this.MinimumSize = new Size(_globalContentMinWidth + 40, _globalContentMinHeight + 100);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("微软雅黑", 9.5F);
            this.Dock = DockStyle.Fill;

            // 原有初始化流程完全保留
            InitMainContainer();
            InitializeDynamicControls();
            InitControlData();
            BindAllEvents();

            // 新增：窗体加载时初始化数据
            this.Load += FrmMedicationAdjustment_Load;
        }

        #region ========== 原有容器/控件创建方法完全保留，无任何改动 ==========
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

        private void InitializeDynamicControls()
        {
            tabMain = new TabControl();
            tabMain.Dock = DockStyle.Fill;
            tabMain.Font = new Font("微软雅黑", 10F);
            tabMain.Padding = new Point(15, 8);
            tab_MedicationRecord = new TabPage("用药记录查看") { BackColor = Color.White };
            tab_MedicationAdjust = new TabPage("血糖联动用药调整") { BackColor = Color.White };
            tab_PlanSave = new TabPage("用药方案保存") { BackColor = Color.White };
            tabMain.TabPages.AddRange(new TabPage[] { tab_MedicationRecord, tab_MedicationAdjust, tab_PlanSave });
            pnlContentWrapper.Controls.Add(tabMain);

            InitMedicationRecordPage();
            InitMedicationAdjustPage();
            InitPlanSavePage();
        }

        private void InitMedicationRecordPage()
        {
            grp_RecordQuery = new GroupBox { Text = "查询条件", Dock = DockStyle.Top, Height = 120, Padding = _globalGroupBoxPadding };
            tab_MedicationRecord.Controls.Add(grp_RecordQuery);
            TableLayoutPanel tlp_Query = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Query.RowCount = 1;
            tlp_Query.ColumnCount = 3;
            tlp_Query.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlp_Query.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlp_Query.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            tlp_Query.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_RecordQuery.Controls.Add(tlp_Query);
            int row = 0;
            CreateEditItem<ComboBox>(tlp_Query, out _, out cbo_RecordPatient, "选择患者：", ref row, false);

            Panel pnl_DateRange = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_Date = new Label { Text = "记录时间：", Size = new Size(80, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(0, 0) };
            dtp_RecordStart = new DateTimePicker { Location = new Point(80, 0), Size = new Size(140, _globalControlHeight), Format = DateTimePickerFormat.Short };
            dtp_RecordEnd = new DateTimePicker { Location = new Point(230, 0), Size = new Size(140, _globalControlHeight), Format = DateTimePickerFormat.Short };
            pnl_DateRange.Controls.AddRange(new Control[] { lbl_Date, dtp_RecordStart, dtp_RecordEnd });
            tlp_Query.Controls.Add(pnl_DateRange, 1, 0);

            Panel pnl_QueryBtn = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            FlowLayoutPanel flp_QueryBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            btn_QueryRecord = CreateBtn("查询", Color.FromArgb(0, 122, 204));
            btn_ResetRecord = CreateBtn("重置", Color.Gray);
            flp_QueryBtn.Controls.AddRange(new Control[] { btn_ResetRecord, btn_QueryRecord });
            pnl_QueryBtn.Controls.Add(flp_QueryBtn);
            tlp_Query.Controls.Add(pnl_QueryBtn, 2, 0);

            grp_RecordQuery = new GroupBox { Text = "患者用药记录列表", Dock = DockStyle.Fill, Padding = _globalGroupBoxPadding };
            tab_MedicationRecord.Controls.Add(grp_RecordQuery);
            dgv_MedicationRecord = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };
            grp_RecordQuery.Controls.Add(dgv_MedicationRecord);

            // 原有列保留，新增隐藏主键列
            dgv_MedicationRecord.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { HeaderText = "用药日期", DataPropertyName = "MedicationDate", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "药物名称", DataPropertyName = "MedicationName", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "药物类型", DataPropertyName = "MedicationType", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "服用剂量", DataPropertyName = "Dosage", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "服用时间", DataPropertyName = "TakeTime", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "用药方式", DataPropertyName = "TakeWay", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { Name = "medicine_id", DataPropertyName = "medicine_id", Visible = false } // 隐藏主键列
            });
        }

        private void InitMedicationAdjustPage()
        {
            grp_PatientSelect = new GroupBox { Text = "选择患者", Dock = DockStyle.Top, Height = 80, Padding = _globalGroupBoxPadding };
            tab_MedicationAdjust.Controls.Add(grp_PatientSelect);
            TableLayoutPanel tlp_Patient = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Patient.RowCount = 1;
            tlp_Patient.ColumnCount = 2;
            tlp_Patient.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_Patient.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_Patient.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_PatientSelect.Controls.Add(tlp_Patient);
            int row = 0;
            CreateEditItem<ComboBox>(tlp_Patient, out _, out cbo_AdjustPatient, "选择患者：", ref row, false);
            btn_LoadGlucose = CreateBtn("加载血糖数据", Color.FromArgb(0, 150, 136));
            tlp_Patient.Controls.Add(btn_LoadGlucose, 1, 0);

            grp_BloodGlucose = new GroupBox { Text = "患者近期血糖数据", Dock = DockStyle.Top, Height = 200, Padding = _globalGroupBoxPadding };
            tab_MedicationAdjust.Controls.Add(grp_BloodGlucose);
            dgv_BloodGlucose = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };
            grp_BloodGlucose.Controls.Add(dgv_BloodGlucose);

            // 原有列保留，新增隐藏主键列
            dgv_BloodGlucose.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { HeaderText = "测量日期", DataPropertyName = "MeasureDate", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "空腹血糖", DataPropertyName = "FastingGlucose", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "餐后2h血糖", DataPropertyName = "PostGlucose", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "血糖趋势", DataPropertyName = "GlucoseTrend", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { Name = "blood_sugar_id", DataPropertyName = "blood_sugar_id", Visible = false }
            });

            grp_AdjustContent = new GroupBox { Text = "用药调整内容", Dock = DockStyle.Top, Height = 260, Padding = _globalGroupBoxPadding };
            tab_MedicationAdjust.Controls.Add(grp_AdjustContent);
            TableLayoutPanel tlp_Adjust = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Adjust.RowCount = 5;
            tlp_Adjust.ColumnCount = 2;
            tlp_Adjust.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_Adjust.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (int i = 0; i < 5; i++) tlp_Adjust.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_AdjustContent.Controls.Add(tlp_Adjust);
            row = 0;
            CreateEditItem<TextBox>(tlp_Adjust, out _, out txt_MedicationName, "药物名称：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Adjust, out _, out cbo_MedicationType, "药物类型：", ref row, false);
            CreateEditItem<TextBox>(tlp_Adjust, out _, out txt_Dosage, "调整后剂量：", ref row, false);
            CreateEditItem<TextBox>(tlp_Adjust, out _, out txt_AdjustReason, "调整原因：", ref row, false);
            CreateEditItem<TextBox>(tlp_Adjust, out _, out txt_AdjustContent, "调整说明：", ref row, false);
            // 调整说明允许多行
            txt_AdjustContent.Multiline = true;
            txt_AdjustContent.Height = 80;

            Panel pnl_AdjustBtn = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(15) };
            tab_MedicationAdjust.Controls.Add(pnl_AdjustBtn);
            FlowLayoutPanel flp_AdjustBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_CalcAdjust = CreateBtn("智能调整建议", Color.FromArgb(255, 152, 0));
            btn_SaveAdjust = CreateBtn("保存调整方案", Color.FromArgb(0, 122, 204));
            flp_AdjustBtn.Controls.AddRange(new Control[] { btn_CalcAdjust, btn_SaveAdjust });
            pnl_AdjustBtn.Controls.Add(flp_AdjustBtn);
        }

        private void InitPlanSavePage()
        {
            grp_PlanList = new GroupBox { Text = "患者用药方案列表", Dock = DockStyle.Fill, Padding = _globalGroupBoxPadding };
            tab_PlanSave.Controls.Add(grp_PlanList);
            dgv_MedicationPlan = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };
            grp_PlanList.Controls.Add(dgv_MedicationPlan);

            // 原有列保留，新增隐藏主键列
            dgv_MedicationPlan.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { HeaderText = "患者姓名", DataPropertyName = "PatientName", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "药物名称", DataPropertyName = "MedicationName", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "服用剂量", DataPropertyName = "Dosage", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "创建时间", DataPropertyName = "CreateTime", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { HeaderText = "方案状态", DataPropertyName = "PlanStatus", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { Name = "plan_id", DataPropertyName = "plan_id", Visible = false }
            });

            Panel pnl_PlanBtn = new Panel { Dock = DockStyle.Bottom, Height = 80, Padding = new Padding(15) };
            tab_PlanSave.Controls.Add(pnl_PlanBtn);
            FlowLayoutPanel flp_PlanBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            btn_ViewPlan = CreateBtn("查看详情", Color.FromArgb(0, 150, 136));
            btn_SavePlan = CreateBtn("保存新方案", Color.FromArgb(0, 122, 204));
            flp_PlanBtn.Controls.AddRange(new Control[] { btn_SavePlan, btn_ViewPlan });
            pnl_PlanBtn.Controls.Add(flp_PlanBtn);
        }

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
            btn.Cursor = Cursors.Hand;
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

        #region 新增：窗体加载初始化数据
        private void FrmMedicationAdjustment_Load(object sender, EventArgs e)
        {
            // 绑定患者下拉框
            BindPatientComboBox();
            // 绑定药物类型下拉框
            BindDrugTypeComboBox();
            // 加载用药方案列表
            LoadMedicationPlanList();
            // 时间范围默认值
            dtp_RecordStart.Value = DateTime.Now.AddMonths(-1);
            dtp_RecordEnd.Value = DateTime.Now;
        }

        // 绑定患者下拉框
        private void BindPatientComboBox()
        {
            try
            {
                DataTable patientDt = _medicineBll.GetPatientList();
                // 绑定用药记录页患者下拉
                cbo_RecordPatient.DataSource = patientDt.Copy();
                cbo_RecordPatient.DisplayMember = "user_name";
                cbo_RecordPatient.ValueMember = "user_id";
                // 绑定用药调整页患者下拉
                cbo_AdjustPatient.DataSource = patientDt.Copy();
                cbo_AdjustPatient.DisplayMember = "user_name";
                cbo_AdjustPatient.ValueMember = "user_id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载患者列表失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 绑定药物类型下拉框
        private void BindDrugTypeComboBox()
        {
            cbo_MedicationType.Items.AddRange(new string[] {
                "胰岛素", "双胍类", "磺脲类", "DPP-4抑制剂",
                "GLP-1受体激动剂", "α-糖苷酶抑制剂", "SGLT-2抑制剂"
            });
            cbo_MedicationType.SelectedIndex = 0;
        }
        #endregion

        #region 原有InitControlData方法重构，替换模拟数据
        private void InitControlData()
        {
            // 时间范围默认值
            dtp_RecordStart.Value = DateTime.Now.AddMonths(-1);
            dtp_RecordEnd.Value = DateTime.Now;
        }
        #endregion

        #region 核心：所有按钮事件绑定与业务逻辑实现
        private void BindAllEvents()
        {
            // ========== 用药记录查看页面事件 ==========
            // 查询按钮
            btn_QueryRecord.Click += Btn_QueryRecord_Click;
            // 重置按钮
            btn_ResetRecord.Click += Btn_ResetRecord_Click;

            // ========== 血糖联动用药调整页面事件 ==========
            // 加载血糖数据按钮
            btn_LoadGlucose.Click += Btn_LoadGlucose_Click;
            // 智能调整建议按钮
            btn_CalcAdjust.Click += Btn_CalcAdjust_Click;
            // 保存调整方案按钮
            btn_SaveAdjust.Click += Btn_SaveAdjust_Click;

            // ========== 用药方案保存页面事件 ==========
            // 查看详情按钮
            btn_ViewPlan.Click += Btn_ViewPlan_Click;
            // 保存新方案按钮
            btn_SavePlan.Click += Btn_SavePlan_Click;

            // 标签页切换事件
            tabMain.SelectedIndexChanged += TabMain_SelectedIndexChanged;
        }

        #region 用药记录查看-查询按钮事件
        private void Btn_QueryRecord_Click(object sender, EventArgs e)
        {
            try
            {
                // 获取选中患者ID
                if (cbo_RecordPatient.SelectedValue == null || !int.TryParse(cbo_RecordPatient.SelectedValue.ToString(), out int userId))
                {
                    MessageBox.Show("请选择有效患者", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DateTime startTime = dtp_RecordStart.Value;
                DateTime endTime = dtp_RecordEnd.Value;

                // 调用业务层查询
                var recordList = _medicineBll.GetUserMedicineRecordByTime(userId, startTime, endTime, out string msg);
                if (!string.IsNullOrEmpty(msg))
                {
                    MessageBox.Show(msg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dgv_MedicationRecord.DataSource = null;
                    return;
                }

                // 构建DataTable绑定到DataGridView
                DataTable dt = new DataTable();
                dt.Columns.Add("medicine_id", typeof(int));
                dt.Columns.Add("MedicationDate", typeof(DateTime));
                dt.Columns.Add("MedicationName", typeof(string));
                dt.Columns.Add("MedicationType", typeof(string));
                dt.Columns.Add("Dosage", typeof(string));
                dt.Columns.Add("TakeTime", typeof(string));
                dt.Columns.Add("TakeWay", typeof(string));

                foreach (var item in recordList)
                {
                    DataRow dr = dt.NewRow();
                    dr["medicine_id"] = item.medicine_id;
                    dr["MedicationDate"] = item.take_medicine_time.Date;
                    dr["MedicationName"] = item.drug_name;
                    dr["MedicationType"] = "-";
                    dr["Dosage"] = $"{item.drug_dosage}";
                    dr["TakeTime"] = item.take_medicine_time.ToString("HH:mm");
                    dr["TakeWay"] = item.take_way;
                    dt.Rows.Add(dr);
                }

                dgv_MedicationRecord.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"查询用药记录失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 用药记录查看-重置按钮事件
        private void Btn_ResetRecord_Click(object sender, EventArgs e)
        {
            if (cbo_RecordPatient.Items.Count > 0) cbo_RecordPatient.SelectedIndex = 0;
            dtp_RecordStart.Value = DateTime.Now.AddMonths(-1);
            dtp_RecordEnd.Value = DateTime.Now;
            dgv_MedicationRecord.DataSource = null;
        }
        #endregion

        #region 血糖联动调整-加载血糖数据按钮事件
        private void Btn_LoadGlucose_Click(object sender, EventArgs e)
        {
            try
            {
                // 获取选中患者ID
                if (cbo_AdjustPatient.SelectedValue == null || !int.TryParse(cbo_AdjustPatient.SelectedValue.ToString(), out int userId))
                {
                    MessageBox.Show("请选择有效患者", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 调用业务层获取血糖数据
                var sugarDt = _medicineBll.GetPatientBloodSugarData(userId, out string msg);
                if (!string.IsNullOrEmpty(msg))
                {
                    MessageBox.Show(msg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dgv_BloodGlucose.DataSource = null;
                    return;
                }

                dgv_BloodGlucose.DataSource = sugarDt;
                MessageBox.Show("患者血糖数据加载完成！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载血糖数据失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 血糖联动调整-智能调整建议按钮事件
        private void Btn_CalcAdjust_Click(object sender, EventArgs e)
        {
            try
            {
                // 获取选中患者ID
                if (cbo_AdjustPatient.SelectedValue == null || !int.TryParse(cbo_AdjustPatient.SelectedValue.ToString(), out int userId))
                {
                    MessageBox.Show("请先选择患者并加载血糖数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (dgv_BloodGlucose.DataSource == null || dgv_BloodGlucose.RowCount == 0)
                {
                    MessageBox.Show("请先加载患者血糖数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 调用业务层生成智能建议
                (string drugName, string drugType, string dosage, string adjustReason, string adjustContent) = _medicineBll.GenerateMedicationSuggestion(userId, out string msg);
                if (!string.IsNullOrEmpty(msg))
                {
                    MessageBox.Show(msg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 填充到窗体控件
                txt_MedicationName.Text = drugName;
                // 匹配药物类型下拉框
                for (int i = 0; i < cbo_MedicationType.Items.Count; i++)
                {
                    if (cbo_MedicationType.Items[i].ToString() == drugType)
                    {
                        cbo_MedicationType.SelectedIndex = i;
                        break;
                    }
                }
                txt_Dosage.Text = dosage;
                txt_AdjustReason.Text = adjustReason;
                txt_AdjustContent.Text = adjustContent;

                MessageBox.Show("智能用药调整建议生成完成！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"生成调整建议失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 血糖联动调整-保存调整方案按钮事件
        private void Btn_SaveAdjust_Click(object sender, EventArgs e)
        {
            try
            {
                // 获取选中患者ID
                if (cbo_AdjustPatient.SelectedValue == null || !int.TryParse(cbo_AdjustPatient.SelectedValue.ToString(), out int userId))
                {
                    MessageBox.Show("请选择有效患者", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 构建方案实体
                MedicationPlan plan = new MedicationPlan
                {
                    user_id = userId,
                    drug_name = txt_MedicationName.Text.Trim(),
                    drug_type = cbo_MedicationType.SelectedItem?.ToString() ?? "",
                    drug_dosage = decimal.TryParse(txt_Dosage.Text.Trim().Split('/')[0], out decimal dosage) ? dosage : 0,
                    adjust_reason = txt_AdjustReason.Text.Trim(),
                    adjust_content = txt_AdjustContent.Text.Trim(),
                    create_by = CurrentDoctorId
                };

                // 调用业务层保存
                (bool success, string msg, int planId) = _medicineBll.SaveMedicationPlan(plan);
                if (!success)
                {
                    MessageBox.Show(msg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show($"{msg} 方案编号：{planId}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // 重置表单
                ResetAdjustForm();
                // 刷新方案列表
                LoadMedicationPlanList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存调整方案失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 重置调整表单
        private void ResetAdjustForm()
        {
            txt_MedicationName.Clear();
            txt_Dosage.Clear();
            txt_AdjustReason.Clear();
            txt_AdjustContent.Clear();
            cbo_MedicationType.SelectedIndex = 0;
            dgv_BloodGlucose.DataSource = null;
            if (cbo_AdjustPatient.Items.Count > 0) cbo_AdjustPatient.SelectedIndex = 0;
        }
        #endregion

        #region 用药方案保存-查看详情按钮事件
        private void Btn_ViewPlan_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgv_MedicationPlan.SelectedRows.Count == 0 || dgv_MedicationPlan.SelectedRows[0].Cells["plan_id"].Value == null)
                {
                    MessageBox.Show("请先选择一条用药方案", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 获取方案ID
                int planId = Convert.ToInt32(dgv_MedicationPlan.SelectedRows[0].Cells["plan_id"].Value);
                // 调用业务层获取详情
                var plan = _medicineBll.GetMedicationPlanById(planId, out string msg);
                if (!string.IsNullOrEmpty(msg))
                {
                    MessageBox.Show(msg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 构建详情内容
                string detailContent = $"===== 用药方案详情 =====\r\n" +
                                       $"方案编号：{plan.plan_id}\r\n" +
                                       $"患者ID：{plan.user_id}\r\n" +
                                       $"药物名称：{plan.drug_name}\r\n" +
                                       $"药物类型：{plan.drug_type}\r\n" +
                                       $"用药剂量：{plan.drug_dosage}\r\n" +
                                       $"调整原因：{plan.adjust_reason}\r\n" +
                                       $"方案内容：\r\n{plan.adjust_content}\r\n\r\n" +
                                       $"方案开始时间：{plan.start_time:yyyy-MM-dd}\r\n" +
                                       $"创建医生ID：{plan.create_by}\r\n" +
                                       $"创建时间：{plan.create_time:yyyy-MM-dd HH:mm}\r\n" +
                                       $"方案状态：{(plan.status == 0 ? "待执行" : plan.status == 1 ? "执行中" : plan.status == 2 ? "已结束" : "已停用")}";

                MessageBox.Show(detailContent, "用药方案详情", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"查看方案详情失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 用药方案保存-保存新方案按钮事件
        private void Btn_SavePlan_Click(object sender, EventArgs e)
        {
            // 切换到用药调整标签页，引导创建新方案
            tabMain.SelectedTab = tab_MedicationAdjust;
            MessageBox.Show("请在【血糖联动用药调整】页面创建并保存新的用药方案", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region 标签页切换事件
        private void TabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 切换到用药方案保存页时，刷新列表
            if (tabMain.SelectedTab == tab_PlanSave)
            {
                LoadMedicationPlanList();
            }
        }

        // 加载用药方案列表
        private void LoadMedicationPlanList()
        {
            try
            {
                DataTable planDt = _medicineBll.GetMedicationPlanList();
                dgv_MedicationPlan.DataSource = planDt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载用药方案列表失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
        #endregion
    }
}