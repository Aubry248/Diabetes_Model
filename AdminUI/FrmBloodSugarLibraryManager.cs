// AdminUI/FrmBloodSugarLibraryManager.cs
using BLL;
using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace AdminUI
{
    public partial class FrmBloodSugarLibraryManager : Form
    {
        #region 全局统一布局参数（与运动库完全一致）
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
        private readonly int _globalLabelWidth = 160;
        private readonly int _globalRowHeight = 40;
        private readonly Padding _globalGroupBoxPadding = new Padding(15);
        #endregion

        #region 业务逻辑全局变量
        private readonly B_BloodSugarLibrary _bBloodSugar = new B_BloodSugarLibrary();
        private readonly string _currentUser = "系统管理员1";
        private int _currentEditBloodSugarId = 0; // 当前编辑的血糖记录ID
        #endregion

        #region 核心控件声明
        private Panel pnlMainContainer;
        private Panel pnlContentWrapper;
        private TabControl tabMain;
        private TabPage tab_BloodSugarList, tab_BloodSugarEdit, tab_BatchOperate;

        // 血糖数据列表页
        private GroupBox grp_SearchFilter, grp_BloodSugarList;
        private ComboBox cbo_Patient, cbo_MeasurementScenario, cbo_AbnormalStatus, cbo_DataSource;
        private DateTimePicker dtp_MeasureStart, dtp_MeasureEnd;
        private DataGridView dgv_BloodSugarList;
        private Button btn_Search, btn_ResetFilter, btn_AddSingle, btn_EditSelected, btn_DeleteSelected, btn_ExportExcel;

        // 血糖数据编辑页
        private GroupBox grp_BaseInfo, grp_RelatedInfo, grp_StatusInfo;
        private ComboBox cbo_EditPatient, cbo_EditScenario, cbo_EditDataSource;
        private TextBox txt_BloodSugarValue, txt_AbnormalNote;
        private DateTimePicker dtp_EditMeasurementTime;
        private TextBox txt_RelatedDietId, txt_RelatedExerciseId;
        private Button btn_SaveEdit, btn_CancelEdit;

        // 批量操作页
        private GroupBox grp_BatchOperate;
        private Button btn_BatchDelete, btn_BatchExportAll;
        #endregion

        public FrmBloodSugarLibraryManager()
        {
            this.Text = "血糖数据管理（糖尿病患者核心监测数据）";
            this.Size = new Size(1400, 850);
            this.MinimumSize = new Size(_globalContentMinWidth + 40, _globalContentMinHeight + 100);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("微软雅黑", 9.5F);
            this.Dock = DockStyle.Fill;

            InitMainContainer();
            InitializeDynamicControls();
            InitControlData();
            BindAllEvents();

            this.Load += (s, e) =>
            {
                BindPatientComboBox();
                BindBloodSugarListData();
            };

            this.FormClosed += (s, e) => { this.Dispose(); };
        }

        #region 全局容器初始化（与运动库完全一致）
        private void InitMainContainer()
        {
            InitializeComponent();
            pnlMainContainer = new Panel();
            pnlMainContainer.Dock = DockStyle.Fill;
            pnlMainContainer.BackColor = Color.White;
            pnlMainContainer.Padding = _globalMainContainerPadding;
            pnlMainContainer.AutoScroll = true;
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

            tab_BloodSugarList = new TabPage("血糖数据列表") { BackColor = Color.White };
            tab_BloodSugarEdit = new TabPage("血糖数据编辑") { BackColor = Color.White };
            tab_BatchOperate = new TabPage("批量操作") { BackColor = Color.White };

            tabMain.TabPages.AddRange(new[] { tab_BloodSugarList, tab_BloodSugarEdit, tab_BatchOperate });
            pnlContentWrapper.Controls.Add(tabMain);

            InitBloodSugarListPage();
            InitBloodSugarEditPage();
            InitBatchOperatePage();
        }

        // 1. 血糖数据列表页
        private void InitBloodSugarListPage()
        {
            grp_SearchFilter = new GroupBox { Text = "检索与筛选条件", Dock = DockStyle.Top, Height = 220, Padding = _globalGroupBoxPadding };
            tab_BloodSugarList.Controls.Add(grp_SearchFilter);

            TableLayoutPanel tlp_Filter = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Filter.RowCount = 4;
            tlp_Filter.ColumnCount = 3;
            for (int i = 0; i < 3; i++) tlp_Filter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
            for (int i = 0; i < 4; i++) tlp_Filter.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_SearchFilter.Controls.Add(tlp_Filter);

            int row = 0;
            CreateEditItem<ComboBox>(tlp_Filter, out _, out cbo_Patient, "选择患者：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Filter, out _, out cbo_MeasurementScenario, "测量场景：", ref row, false);

            row = 1;
            CreateEditItem<ComboBox>(tlp_Filter, out _, out cbo_AbnormalStatus, "异常状态：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Filter, out _, out cbo_DataSource, "数据来源：", ref row, false);

            Panel pnl_MeasureDate = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_Date = new Label { Text = "测量时间：", Size = new Size(80, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(0, 0) };
            dtp_MeasureStart = new DateTimePicker { Location = new Point(80, 0), Size = new Size(120, _globalControlHeight), Format = DateTimePickerFormat.Short };
            dtp_MeasureEnd = new DateTimePicker { Location = new Point(210, 0), Size = new Size(120, _globalControlHeight), Format = DateTimePickerFormat.Short };
            pnl_MeasureDate.Controls.AddRange(new Control[] { lbl_Date, dtp_MeasureStart, dtp_MeasureEnd });
            tlp_Filter.Controls.Add(pnl_MeasureDate, 2, 1);

            row = 2;
            Panel pnl_FilterBtn = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            FlowLayoutPanel flp_FilterBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_Search = CreateBtn("检索", Color.FromArgb(0, 122, 204));
            btn_ResetFilter = CreateBtn("重置筛选", Color.Gray);
            btn_AddSingle = CreateBtn("新增记录", Color.FromArgb(0, 150, 136));
            btn_EditSelected = CreateBtn("编辑选中", Color.FromArgb(255, 152, 0));
            btn_DeleteSelected = CreateBtn("删除选中", Color.FromArgb(244, 67, 54));
            btn_ExportExcel = CreateBtn("导出Excel", Color.FromArgb(103, 58, 183));
            flp_FilterBtn.Controls.AddRange(new Control[] { btn_Search, btn_ResetFilter, btn_AddSingle, btn_EditSelected, btn_DeleteSelected, btn_ExportExcel });
            pnl_FilterBtn.Controls.Add(flp_FilterBtn);
            tlp_Filter.Controls.Add(pnl_FilterBtn, 0, 2);
            tlp_Filter.SetColumnSpan(pnl_FilterBtn, 3);

            grp_BloodSugarList = new GroupBox { Text = "血糖数据列表（糖尿病核心监测指标）", Dock = DockStyle.Fill, Padding = new Padding(15, 220, 15, 15) };
            tab_BloodSugarList.Controls.Add(grp_BloodSugarList);

            dgv_BloodSugarList = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ColumnHeadersHeight = 35,
                AllowUserToResizeColumns = true
            };

            grp_BloodSugarList.Controls.Add(dgv_BloodSugarList);

            dgv_BloodSugarList.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { HeaderText = "记录ID", DataPropertyName = "blood_sugar_id", Visible = false },
                new DataGridViewTextBoxColumn { HeaderText = "患者姓名", DataPropertyName = "user_name", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "血糖值(mmol/L)", DataPropertyName = "blood_sugar_value", Width = 120, DefaultCellStyle = { Format = "0.0" } },
                new DataGridViewTextBoxColumn { HeaderText = "测量时间", DataPropertyName = "measurement_time", Width = 150 },
                new DataGridViewTextBoxColumn { HeaderText = "测量场景", DataPropertyName = "measurement_scenario", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "数据来源", DataPropertyName = "data_source", Width = 100 },
                new DataGridViewTextBoxColumn
                {
                    Name = "is_abnormal",  // 关键：显式设置列名，与DataPropertyName一致
                    HeaderText = "异常状态",
                    DataPropertyName = "is_abnormal",
                    Width = 80
                },
                new DataGridViewTextBoxColumn { HeaderText = "异常备注", DataPropertyName = "abnormal_note", Width = 150 },
                new DataGridViewTextBoxColumn { HeaderText = "操作人", DataPropertyName = "operator_name", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "创建时间", DataPropertyName = "create_time", Width = 150 },
                new DataGridViewButtonColumn { HeaderText = "操作", Text = "编辑", UseColumnTextForButtonValue = true, Width = 70 }
            });

            // 异常数据高亮事件
            dgv_BloodSugarList.CellFormatting += Dgv_BloodSugarList_CellFormatting;
        }

        // 2. 血糖数据编辑页
        private void InitBloodSugarEditPage()
        {
            grp_BaseInfo = new GroupBox { Text = "基础信息（必填）", Dock = DockStyle.Top, Height = 200, Padding = _globalGroupBoxPadding };
            tab_BloodSugarEdit.Controls.Add(grp_BaseInfo);

            TableLayoutPanel tlp_Base = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Base.RowCount = 3;
            tlp_Base.ColumnCount = 2;
            for (int i = 0; i < 2; i++) tlp_Base.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (int i = 0; i < 3; i++) tlp_Base.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_BaseInfo.Controls.Add(tlp_Base);

            int row = 0;
            CreateEditItem<ComboBox>(tlp_Base, out _, out cbo_EditPatient, "选择患者：", ref row, false);
            CreateEditItem<TextBox>(tlp_Base, out _, out txt_BloodSugarValue, "血糖值(mmol/L)：", ref row, false);
            CreateEditItem<DateTimePicker>(tlp_Base, out _, out dtp_EditMeasurementTime, "测量时间：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Base, out _, out cbo_EditScenario, "测量场景：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Base, out _, out cbo_EditDataSource, "数据来源：", ref row, false);
            CreateEditItem<TextBox>(tlp_Base, out _, out txt_AbnormalNote, "异常备注：", ref row, false);

            grp_RelatedInfo = new GroupBox { Text = "关联信息（可选）", Dock = DockStyle.Top, Height = 150, Padding = _globalGroupBoxPadding };
            tab_BloodSugarEdit.Controls.Add(grp_RelatedInfo);

            TableLayoutPanel tlp_Related = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Related.RowCount = 2;
            tlp_Related.ColumnCount = 2;
            for (int i = 0; i < 2; i++) tlp_Related.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (int i = 0; i < 2; i++) tlp_Related.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_RelatedInfo.Controls.Add(tlp_Related);

            row = 0;
            CreateEditItem<TextBox>(tlp_Related, out _, out txt_RelatedDietId, "关联饮食记录ID：", ref row, false);
            CreateEditItem<TextBox>(tlp_Related, out _, out txt_RelatedExerciseId, "关联运动记录ID：", ref row, false);

            Panel pnl_EditBtn = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            tab_BloodSugarEdit.Controls.Add(pnl_EditBtn);

            FlowLayoutPanel flp_EditBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_SaveEdit = CreateBtn("保存修改", Color.FromArgb(0, 122, 204));
            btn_CancelEdit = CreateBtn("取消编辑", Color.Gray);
            flp_EditBtn.Controls.AddRange(new Control[] { btn_SaveEdit, btn_CancelEdit });
            pnl_EditBtn.Controls.Add(flp_EditBtn);
        }

        // 3. 批量操作页
        private void InitBatchOperatePage()
        {
            grp_BatchOperate = new GroupBox { Text = "批量操作", Dock = DockStyle.Top, Height = 200, Padding = _globalGroupBoxPadding };
            tab_BatchOperate.Controls.Add(grp_BatchOperate);

            Panel pnl_Operate = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_OperateTip = new Label { Text = "请先在【血糖数据列表】选中需要操作的记录，再执行以下批量操作", Location = new Point(10, 10), AutoSize = true, Font = new Font("微软雅黑", 9F) };

            btn_BatchDelete = CreateBtn("批量删除", Color.FromArgb(244, 67, 54));
            btn_BatchDelete.Location = new Point(10, 50);

            btn_BatchExportAll = CreateBtn("导出全部数据", Color.FromArgb(255, 152, 0));
            btn_BatchExportAll.Location = new Point(130, 50);

            pnl_Operate.Controls.AddRange(new Control[] { lbl_OperateTip, btn_BatchDelete, btn_BatchExportAll });
            grp_BatchOperate.Controls.Add(pnl_Operate);
        }
        #endregion

        #region 通用控件创建方法（与运动库完全一致）
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

            int currentColumn = row % 2;
            int currentRow = row / 2;
            tlp.Controls.Add(pairPanel, currentColumn, currentRow);
            row++;
        }
        #endregion

        #region 下拉数据初始化
        private void InitControlData()
        {
            string[] scenarios = { "全部", "空腹", "餐后2小时", "睡前", "随机", "早餐后", "午餐后", "晚餐后" };
            string[] abnormalStatus = { "全部", "正常", "异常" };
            string[] dataSources = { "全部", "手动录入", "设备上传" };

            cbo_MeasurementScenario.Items.AddRange(scenarios);
            cbo_AbnormalStatus.Items.AddRange(abnormalStatus);
            cbo_DataSource.Items.AddRange(dataSources);
            cbo_EditScenario.Items.AddRange(scenarios);
            cbo_EditDataSource.Items.AddRange(dataSources);

            cbo_MeasurementScenario.SelectedIndex = 0;
            cbo_AbnormalStatus.SelectedIndex = 0;
            cbo_DataSource.SelectedIndex = 0;
            cbo_EditScenario.SelectedIndex = 0;
            cbo_EditDataSource.SelectedIndex = 0;

            dtp_MeasureStart.Value = DateTime.Now.AddMonths(-1);
            dtp_MeasureEnd.Value = DateTime.Now;
            dtp_EditMeasurementTime.Value = DateTime.Now;
        }

        // 绑定患者下拉框
        // AdminUI/FrmBloodSugarLibraryManager.cs 第350行
        private void BindPatientComboBox()
        {
            try
            {
                var result = _bBloodSugar.GetAllPatients();
                if (result.IsSuccess)
                {
                    DataTable dt = result.Data as DataTable;
                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("系统中没有患者数据，请先添加患者", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DataRow dr = dt.NewRow();
                    dr["user_id"] = 0;
                    dr["user_name"] = "全部";
                    dt.Rows.InsertAt(dr, 0);

                    cbo_Patient.DataSource = dt;
                    cbo_Patient.DisplayMember = "user_name";
                    cbo_Patient.ValueMember = "user_id";
                    cbo_Patient.SelectedIndex = 0;

                    // 编辑页患者下拉框
                    DataTable dtEdit = dt.Copy();
                    dtEdit.Rows.RemoveAt(0);
                    cbo_EditPatient.DataSource = dtEdit;
                    cbo_EditPatient.DisplayMember = "user_name";
                    cbo_EditPatient.ValueMember = "user_id";

                    // 确保编辑页有默认选中项
                    if (dtEdit.Rows.Count > 0)
                        cbo_EditPatient.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载患者列表失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 事件绑定
        private void BindAllEvents()
        {
            #region 列表页事件
            btn_Search.Click += (s, e) => BindBloodSugarListData();
            btn_ResetFilter.Click += (s, e) =>
            {
                cbo_Patient.SelectedIndex = 0;
                cbo_MeasurementScenario.SelectedIndex = 0;
                cbo_AbnormalStatus.SelectedIndex = 0;
                cbo_DataSource.SelectedIndex = 0;
                dtp_MeasureStart.Value = DateTime.Now.AddMonths(-1);
                dtp_MeasureEnd.Value = DateTime.Now;
                BindBloodSugarListData();
            };

            btn_AddSingle.Click += (s, e) =>
            {
                ClearEditForm();
                tabMain.SelectedTab = tab_BloodSugarEdit;
            };

            btn_EditSelected.Click += (s, e) =>
            {
                if (dgv_BloodSugarList.SelectedRows.Count == 0)
                {
                    MessageBox.Show("请先选中一条要编辑的血糖记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int id = Convert.ToInt32(dgv_BloodSugarList.SelectedRows[0].Cells["blood_sugar_id"].Value);
                LoadBloodSugarDetailToEdit(id);
                tabMain.SelectedTab = tab_BloodSugarEdit;
            };

            btn_DeleteSelected.Click += (s, e) =>
            {
                if (dgv_BloodSugarList.SelectedRows.Count == 0)
                {
                    MessageBox.Show("请先选中要删除的血糖记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int id = Convert.ToInt32(dgv_BloodSugarList.SelectedRows[0].Cells["blood_sugar_id"].Value);
                string patientName = dgv_BloodSugarList.SelectedRows[0].Cells["user_name"].Value.ToString();

                if (MessageBox.Show($"确认删除患者【{patientName}】的这条血糖记录？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var result = _bBloodSugar.DeleteBloodSugar(id);
                    if (result.IsSuccess)
                    {
                        MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        BindBloodSugarListData();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            dgv_BloodSugarList.CellContentClick += (s, e) =>
            {
                if (e.ColumnIndex == dgv_BloodSugarList.Columns["操作"].Index && e.RowIndex >= 0)
                {
                    int id = Convert.ToInt32(dgv_BloodSugarList.Rows[e.RowIndex].Cells["blood_sugar_id"].Value);
                    LoadBloodSugarDetailToEdit(id);
                    tabMain.SelectedTab = tab_BloodSugarEdit;
                }
            };

            btn_ExportExcel.Click += (s, e) =>
            {
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "Excel文件|*.xlsx", Title = "保存血糖数据", FileName = $"血糖数据_{DateTime.Now:yyyyMMddHHmmss}.xlsx" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        MessageBox.Show("导出功能已对接，可扩展Excel生成逻辑，保存路径：" + sfd.FileName, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };
            #endregion

            #region 编辑页事件
            btn_SaveEdit.Click += (s, e) =>
            {
                try
                {
                    // 基础校验
                    if (cbo_EditPatient.SelectedIndex == -1)
                    {
                        MessageBox.Show("请选择患者", "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!decimal.TryParse(txt_BloodSugarValue.Text.Trim(), out decimal bsValue))
                    {
                        MessageBox.Show("请输入有效的血糖值", "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (cbo_EditScenario.SelectedIndex == 0)
                    {
                        MessageBox.Show("请选择测量场景", "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 构建实体
                    BloodSugar model = new BloodSugar
                    {
                        blood_sugar_id = _currentEditBloodSugarId,
                        user_id = Convert.ToInt32(cbo_EditPatient.SelectedValue),
                        blood_sugar_value = bsValue,
                        measurement_time = dtp_EditMeasurementTime.Value,
                        measurement_scenario = cbo_EditScenario.SelectedItem.ToString(),
                        data_source = cbo_EditDataSource.SelectedItem.ToString(),
                        abnormal_note = txt_AbnormalNote.Text.Trim()
                    };

                    // 关联信息
                    if (int.TryParse(txt_RelatedDietId.Text.Trim(), out int dietId))
                        model.related_diet_id = dietId;
                    if (int.TryParse(txt_RelatedExerciseId.Text.Trim(), out int exerciseId))
                        model.related_exercise_id = exerciseId;

                    BizResult result;
                    if (_currentEditBloodSugarId == 0)
                    {
                        result = _bBloodSugar.AddBloodSugar(model);
                    }
                    else
                    {
                        result = _bBloodSugar.UpdateBloodSugar(model);
                    }

                    if (result.IsSuccess)
                    {
                        MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        tabMain.SelectedTab = tab_BloodSugarList;
                        BindBloodSugarListData();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btn_CancelEdit.Click += (s, e) =>
            {
                ClearEditForm();
                tabMain.SelectedTab = tab_BloodSugarList;
            };
            #endregion

            #region 批量操作页事件
            btn_BatchDelete.Click += (s, e) =>
            {
                if (dgv_BloodSugarList.SelectedRows.Count == 0)
                {
                    MessageBox.Show("请先在血糖数据列表选中要删除的记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                List<int> idList = new List<int>();
                foreach (DataGridViewRow row in dgv_BloodSugarList.SelectedRows)
                {
                    idList.Add(Convert.ToInt32(row.Cells["blood_sugar_id"].Value));
                }

                if (MessageBox.Show($"确认删除选中的{idList.Count}条血糖记录？", "确认批量删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var result = _bBloodSugar.BatchDeleteBloodSugar(idList);
                    if (result.IsSuccess)
                    {
                        MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        BindBloodSugarListData();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            btn_BatchExportAll.Click += (s, e) =>
            {
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "Excel文件|*.xlsx", Title = "保存全部血糖数据", FileName = $"全部血糖数据_{DateTime.Now:yyyyMMddHHmmss}.xlsx" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        MessageBox.Show("导出功能已对接，可扩展Excel生成逻辑，保存路径：" + sfd.FileName, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };
            #endregion
        }
        #endregion

        #region 核心业务数据绑定方法
        /// <summary>
        /// 绑定血糖数据列表
        /// </summary>
        private void BindBloodSugarListData()
        {
            try
            {
                int userId = cbo_Patient.SelectedValue != null ? Convert.ToInt32(cbo_Patient.SelectedValue) : 0;
                string scenario = cbo_MeasurementScenario.SelectedItem?.ToString() ?? "全部";
                int isAbnormal = cbo_AbnormalStatus.SelectedIndex == 0 ? -1 : (cbo_AbnormalStatus.SelectedIndex == 1 ? 0 : 1);
                string dataSource = cbo_DataSource.SelectedItem?.ToString() ?? "全部";
                DateTime startTime = dtp_MeasureStart.Value;
                DateTime endTime = dtp_MeasureEnd.Value;

                var result = _bBloodSugar.GetBloodSugarList(userId, scenario, isAbnormal, dataSource, startTime, endTime);
                if (!result.IsSuccess)
                {
                    MessageBox.Show(result.Message, "数据加载失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dgv_BloodSugarList.DataSource = null;
                    return;
                }

                DataTable dt = result.Data as DataTable;
                dgv_BloodSugarList.DataSource = dt;
                dgv_BloodSugarList.ClearSelection();

                this.Text = $"糖尿病患者综合健康管理系统 - 管理员后台 - 血糖数据管理 | 共{result.TotalCount}条数据";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载血糖数据失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dgv_BloodSugarList.DataSource = null;
            }
        }

        /// <summary>
        /// 加载血糖详情到编辑页
        /// </summary>
        private void LoadBloodSugarDetailToEdit(int bloodSugarId)
        {
            try
            {
                var result = _bBloodSugar.GetBloodSugarDetail(bloodSugarId);
                if (!result.IsSuccess)
                {
                    MessageBox.Show(result.Message, "加载失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                BloodSugar model = result.Data as BloodSugar;
                _currentEditBloodSugarId = bloodSugarId;

                // 填充基础信息
                cbo_EditPatient.SelectedValue = model.user_id;
                txt_BloodSugarValue.Text = model.blood_sugar_value.ToString("0.0");
                dtp_EditMeasurementTime.Value = model.measurement_time ?? DateTime.Now;
                cbo_EditScenario.SelectedItem = model.measurement_scenario;
                cbo_EditDataSource.SelectedItem = model.data_source;
                txt_AbnormalNote.Text = model.abnormal_note;

                // 填充关联信息
                txt_RelatedDietId.Text = model.related_diet_id?.ToString() ?? "";
                txt_RelatedExerciseId.Text = model.related_exercise_id?.ToString() ?? "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载血糖详情失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 清空编辑表单
        /// </summary>
        private void ClearEditForm()
        {
            _currentEditBloodSugarId = 0;
            cbo_EditPatient.SelectedIndex = 0;
            txt_BloodSugarValue.Clear();
            dtp_EditMeasurementTime.Value = DateTime.Now;
            cbo_EditScenario.SelectedIndex = 0;
            cbo_EditDataSource.SelectedIndex = 0;
            txt_AbnormalNote.Clear();
            txt_RelatedDietId.Clear();
            txt_RelatedExerciseId.Clear();
        }
        #endregion

        #region 异常数据高亮显示
        private void Dgv_BloodSugarList_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || dgv_BloodSugarList.Rows[e.RowIndex].IsNewRow) return;

            // 先检查列是否存在
            if (!dgv_BloodSugarList.Columns.Contains("is_abnormal")) return;

            // 异常状态列显示文字
            if (e.ColumnIndex == dgv_BloodSugarList.Columns["is_abnormal"].Index)
            {
                if (e.Value != null && e.Value != DBNull.Value)
                {
                    int isAbnormal = Convert.ToInt32(e.Value);
                    e.Value = isAbnormal == 1 ? "异常" : "正常";
                    e.FormattingApplied = true;
                }
            }

            // 异常数据整行高亮
            DataGridViewRow row = dgv_BloodSugarList.Rows[e.RowIndex];
            DataGridViewCell cell = row.Cells["is_abnormal"];

            if (cell.Value != null && cell.Value != DBNull.Value)
            {
                int isAbnormal = Convert.ToInt32(cell.Value);
                if (isAbnormal == 1)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 238);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(183, 28, 28);
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
            }
        }
        #endregion


    }
}