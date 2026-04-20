using BLL;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace PatientUI
{
    public partial class FrmMedicineManage : Form
    {
        // ==============================================
        // 【原有页面参数 完全保留 不做任何改动】
        // ==============================================
        private readonly int _pageTopMargin = 100;
        private readonly int _pageLeftMargin = 250;
        private readonly int _pageRightMargin = 20;
        private readonly int _pageBottomMargin = 20;
        private readonly int _inputAreaHeight = 200;
        private readonly int _gridAreaHeight = 380;
        private readonly int _tableTopOffset = 100;
        private readonly int _tableLeftOffset = 250;
        private readonly float _columnWidthScale = 0.7f;
        private readonly int _colWidth_DrugName = 120;
        private readonly int _colWidth_Dosage = 80;
        private readonly int _colWidth_TakeTime = 150;
        private readonly int _colWidth_TakeWay = 100;
        private readonly int _colWidth_Source = 80;
        private readonly int _colWidth_RelatedBS = 80;
        private readonly int _colWidth_CreateTime = 150;
        private readonly int _colWidth_Operation = 80;
        private readonly Padding _controlMargin = new Padding(5);
        private readonly Padding _btnMargin = new Padding(3);
        private readonly int _btnBatchDeleteWidth = 120;

        private ComboBox _cboDrug;
        private TextBox _txtDosage;
        private DateTimePicker _dtpTakeTime;
        private ComboBox _cboTakeWay;
        private TextBox _txtRelatedBS;
        private DataGridView _dgvMedicine;
        private Label _lblModeTip;
        private Label _lblStandardTip;
        private bool _isEditMode = false;
        private int _currentEditMedicineId = 0;
        private readonly B_Medicine _bllMedicine = new B_Medicine();
        private List<Medicine> _medicineList = new List<Medicine>();
        private readonly B_MedicineReminder _bllReminder = new B_MedicineReminder();
        private bool _isInitialized = false;
        private readonly int _pageSize = 10;
        private int _currentPageIndex = 1;
        private Label _lblPageInfo;

        // 适配新拆分架构：获取登录用户ID
        private int _currentUserId => Program.LoginUser?.user_id ?? 0;

        public FrmMedicineManage()
        {
            InitializeComponent();
            this.TopLevel = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.Font = new Font("微软雅黑", 9F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.DoubleBuffered = true;
            this.HandleCreated += (s, e) =>
            {
                if (!_isInitialized)
                {
                    InitializeControls();
                    BindDrugDictionary();
                    BindMedicationData();
                    LoadMedicineTrendChart();
                    _isInitialized = true;
                }
            };
        }

        private void InitializeControls()
        {
            try
            {
                _cboDrug = null;
                _txtDosage = null;
                _dtpTakeTime = null;
                _cboTakeWay = null;
                _txtRelatedBS = null;
                _dgvMedicine = null;
                _lblModeTip = null;
                _lblStandardTip = null;
                this.Controls.Clear();
                this.SuspendLayout();
                this.MinimumSize = new Size(800, 600);
                var inputPanel = CreateInputPanel();
                var gridPanel = CreateGridPanel();
                var chartPanel = CreateChartPanel();
                var scrollContainer = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    AutoScroll = true
                };
                var rootPanel = new TableLayoutPanel
                {
                    Name = "rootPanel",
                    Dock = DockStyle.Top,
                    RowCount = 3,
                    ColumnCount = 1,
                    BackColor = Color.White,
                    Padding = new Padding(_pageLeftMargin, _pageTopMargin, _pageRightMargin, _pageBottomMargin),
                    GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink
                };
                rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, _inputAreaHeight + 10));
                rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, _gridAreaHeight + 20));
                rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 450));
                rootPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                rootPanel.Controls.Add(inputPanel, 0, 0);
                rootPanel.Controls.Add(gridPanel, 0, 1);
                rootPanel.Controls.Add(chartPanel, 0, 2);
                scrollContainer.Controls.Add(rootPanel);
                this.Controls.Add(scrollContainer);
                this.ResumeLayout(true);
                this.PerformLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化界面控件失败：{ex.Message}\n{ex.StackTrace}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BindDrugDictionary()
        {
            try
            {
                var drugList = _bllMedicine.GetDrugDictionary();
                _cboDrug.DataSource = null;
                _cboDrug.Items.Clear();
                if (drugList != null && drugList.Count > 0)
                {
                    _cboDrug.DisplayMember = "DrugGenericName";
                    _cboDrug.ValueMember = "DrugCode";
                    _cboDrug.DataSource = drugList;
                }
                else
                {
                    _cboDrug.Items.AddRange(new[] { "二甲双胍", "格列美脲", "胰岛素", "甲钴胺" });
                }
                if (_cboDrug.Items.Count > 0)
                {
                    _cboDrug.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载药物字典失败：{ex.Message}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _cboDrug.DataSource = null;
                _cboDrug.Items.Clear();
                _cboDrug.Items.AddRange(new[] { "二甲双胍", "格列美脲", "胰岛素", "甲钴胺" });
                if (_cboDrug.Items.Count > 0)
                {
                    _cboDrug.SelectedIndex = 0;
                }
            }
        }

        private void BindMedicationData()
        {
            try
            {
                // 适配新架构：规范用户ID获取
                int currentUserId = _currentUserId;
                if (currentUserId <= 0)
                {
                    MessageBox.Show("用户登录信息失效，请重新登录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                _medicineList = _bllMedicine.GetUserMedicineList(currentUserId);
                _currentPageIndex = 1;
                BindCurrentPageData();
                LoadMedicineTrendChart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载用药记录失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetTotalPage()
        {
            if (_medicineList == null || _medicineList.Count == 0)
            {
                return 1;
            }
            return (int)Math.Ceiling(_medicineList.Count * 1.0 / _pageSize);
        }

        private void BindCurrentPageData()
        {
            if (_dgvMedicine == null)
            {
                return;
            }

            int totalPage = GetTotalPage();
            if (_currentPageIndex < 1)
            {
                _currentPageIndex = 1;
            }
            if (_currentPageIndex > totalPage)
            {
                _currentPageIndex = totalPage;
            }

            var pageData = (_medicineList ?? new List<Medicine>())
                .Skip((_currentPageIndex - 1) * _pageSize)
                .Take(_pageSize)
                .Select(m => new
                {
                    m.medicine_id,
                    m.drug_name,
                    m.drug_dosage,
                    take_medicine_time = m.take_medicine_time.ToString("yyyy-MM-dd HH:mm"),
                    m.take_way,
                    m.data_source,
                    related_bs_id = m.related_bs_id.HasValue ? m.related_bs_id.Value : 0,
                    create_time = m.create_time.ToString("yyyy-MM-dd HH:mm")
                }).ToList();

            _dgvMedicine.DataSource = null;
            _dgvMedicine.ColumnHeadersVisible = true;
            _dgvMedicine.DataSource = pageData;
            _dgvMedicine.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dgvMedicine.Invalidate();

            if (_lblPageInfo != null)
            {
                _lblPageInfo.Text = $"第{_currentPageIndex}页/共{totalPage}页 总计{(_medicineList?.Count ?? 0)}条";
            }
        }

        private void SaveMedication()
        {
            #region 表单校验
            if (_cboDrug.SelectedIndex < 0 || string.IsNullOrWhiteSpace(_cboDrug.Text))
            {
                MessageBox.Show("【填写规范提醒】请选择药物名称，必须为医嘱内的正规降糖药物", "输入校验", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _cboDrug.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(_txtDosage.Text) || _txtDosage.Text == "例：0.5g")
            {
                MessageBox.Show("【填写规范提醒】请输入用药剂量，必须填写准确数值+单位（如0.5g、10mg），严格遵循医嘱", "输入校验", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtDosage.Focus();
                return;
            }
            string dosageText = _txtDosage.Text.Replace("mg", "").Replace("g", "").Replace("IU", "").Replace("ml", "").Replace("mL", "").Trim();
            if (!decimal.TryParse(dosageText, out decimal dosage) || dosage <= 0)
            {
                MessageBox.Show("【填写规范提醒】用药剂量格式错误！请输入有效的正数数值+单位，如0.5g、20IU", "输入校验", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtDosage.Focus();
                return;
            }
            RefreshTakeTimePickerMaxDate();
            if (_dtpTakeTime.Value > DateTime.Now)
            {
                MessageBox.Show("【填写规范提醒】用药时间不能晚于当前时间，请填写实际的用药时间", "输入校验", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _dtpTakeTime.Focus();
                return;
            }
            int? relatedBsId = null;
            if (!string.IsNullOrWhiteSpace(_txtRelatedBS.Text) && _txtRelatedBS.Text != "选填")
            {
                if (!int.TryParse(_txtRelatedBS.Text, out int bsId) || bsId <= 0)
                {
                    MessageBox.Show("【填写规范提醒】关联血糖ID格式错误！请输入对应血糖记录的正整数ID", "输入校验", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtRelatedBS.Focus();
                    return;
                }
                relatedBsId = bsId;
            }
            int currentUserId = _currentUserId;
            if (currentUserId <= 0)
            {
                MessageBox.Show("用户登录信息失效，请重新登录", "权限错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            #endregion

            // 适配新架构：统一data_status=1
            var medicine = new Medicine
            {
                user_id = currentUserId,
                drug_code = _cboDrug.SelectedValue?.ToString() ?? GetDefaultDrugCode(_cboDrug.Text),
                drug_name = _cboDrug.Text,
                drug_dosage = dosage,
                take_medicine_time = _dtpTakeTime.Value,
                take_way = _cboTakeWay.Text,
                related_bs_id = relatedBsId,
                data_source = "手动录入",
                operator_id = currentUserId,
                data_status = 1
            };
            if (_isEditMode)
            {
                medicine.medicine_id = _currentEditMedicineId;
            }

            ResultModel result = _isEditMode
                ? _bllMedicine.UpdateMedicineRecord(medicine)
                : _bllMedicine.SaveMedicineRecord(medicine);

            if (result.Success)
            {
                MessageBox.Show(result.Msg, _isEditMode ? "编辑成功" : "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearInput();
                BindMedicationData();
            }
            else
            {
                MessageBox.Show(result.Msg, _isEditMode ? "编辑失败" : "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetDefaultDrugCode(string drugName)
        {
            switch (drugName)
            {
                case "二甲双胍":
                    return "MET";
                case "阿格列汀":
                    return "ANA";
                case "沙格列汀":
                    return "SAX";
                case "胰岛素":
                    return "INS";
                default:
                    return "DEFAULT";
            }
        }

        private void ClearInput()
        {
            try
            {
                SetWatermark(_txtDosage, "例：0.5g");
                SetWatermark(_txtRelatedBS, "选填");
                _cboDrug.SelectedIndex = 0;
                _cboTakeWay.SelectedIndex = 0;
                ResetTakeTimePickerToNow();
                _dgvMedicine.ClearSelection();
                _isEditMode = false;
                _currentEditMedicineId = 0;
                _lblModeTip.Text = "📝 当前模式：新增用药记录";
                _lblModeTip.ForeColor = Color.FromArgb(0, 122, 204);
                var btnCancelEdit = this.Controls.Find("btnCancelEdit", true).FirstOrDefault() as Button;
                if (btnCancelEdit != null) btnCancelEdit.Visible = false;
                var btnSave = this.Controls.Find("btnSave", true).FirstOrDefault() as Button;
                if (btnSave != null) btnSave.Text = "保存录入";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"清空输入框失败：{ex.Message}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SimulateDeviceImport()
        {
            if (MessageBox.Show("确定导入模拟设备采集的用药数据？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            int currentUserId = _currentUserId;
            if (currentUserId <= 0)
            {
                MessageBox.Show("用户登录信息失效，请重新登录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                var random = new Random();
                var wayList = new[] { "口服", "注射" };
                var importList = new List<Medicine>();
                var drugDict = _bllMedicine.GetDrugDictionary() ?? new List<AntidiabeticDrug>();
                for (int i = 0; i < 10; i++)
                {
                    string drugName;
                    string drugCode;

                    if (drugDict.Count > 0)
                    {
                        var randomDrug = drugDict[random.Next(drugDict.Count)];
                        drugName = string.IsNullOrWhiteSpace(randomDrug.DrugGenericName) ? "二甲双胍" : randomDrug.DrugGenericName.Trim();
                        drugCode = string.IsNullOrWhiteSpace(randomDrug.DrugCode) ? GetDefaultDrugCode(drugName) : randomDrug.DrugCode.Trim();
                    }
                    else
                    {
                        // 字典表不可用时兜底，避免导入流程中断
                        drugName = "二甲双胍";
                        drugCode = GetDefaultDrugCode(drugName);
                    }

                    importList.Add(new Medicine
                    {
                        user_id = currentUserId,
                        drug_code = drugCode,
                        drug_name = drugName,
                        drug_dosage = (decimal)random.NextDouble() * 2 + 0.5m,
                        take_medicine_time = DateTime.Now.AddDays(-i),
                        take_way = wayList[random.Next(wayList.Length)],
                        related_bs_id = random.Next(100, 200),
                        data_source = "Excel批量导入",
                        operator_id = currentUserId,
                        data_status = 1
                    });
                }
                var result = _bllMedicine.BatchImportSimulateData(importList);
                if (result.Success)
                {
                    MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    BindMedicationData();
                }
                else
                {
                    MessageBox.Show(result.Message, "导入失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"模拟导入失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalibrateMedicationData()
        {
            int currentUserId = _currentUserId;
            if (currentUserId <= 0)
            {
                MessageBox.Show("用户登录信息失效，请重新登录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                var result = _bllMedicine.CalibrateMedicineData(currentUserId);
                if (result.Success)
                {
                    MessageBox.Show(result.Message, "校准完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    BindMedicationData();
                }
                else
                {
                    MessageBox.Show(result.Message, "校准失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"数据校准失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Panel CreateInputPanel()
        {
            var inputPanel = new Panel
            {
                Name = "inputPanel",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10),
                MinimumSize = new Size(0, _inputAreaHeight)
            };
            var tipPanel = new Panel
            {
                Name = "tipPanel",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.Transparent
            };
            _lblModeTip = new Label
            {
                Name = "lblModeTip",
                Text = "📝 当前模式：新增用药记录",
                Font = new Font("微软雅黑", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Dock = DockStyle.Left,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            _lblStandardTip = new Label
            {
                Name = "lblStandardTip",
                Text = "💡 填写规范：所有信息请严格遵循医嘱填写，确保用药时间、剂量准确，用于血糖关联分析",
                Font = new Font("微软雅黑", 8F),
                ForeColor = Color.FromArgb(255, 136, 0),
                Dock = DockStyle.Right,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight
            };
            tipPanel.Controls.Add(_lblStandardTip);
            tipPanel.Controls.Add(_lblModeTip);
            inputPanel.Controls.Add(tipPanel);

            var inputLayout = new TableLayoutPanel
            {
                Name = "inputLayout",
                Dock = DockStyle.Top,
                Height = 96,
                RowCount = 2,
                ColumnCount = 5
            };
            for (int i = 0; i < 5; i++)
                inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));
            inputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
            inputLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _cboDrug = new ComboBox
            {
                Name = "cboDrugName",
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = _controlMargin
            };
            var lblDrug = new Label { Name = "lblDrugName", Text = "药物名称", Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft };
            inputLayout.Controls.Add(lblDrug, 0, 0);
            inputLayout.Controls.Add(_cboDrug, 0, 1);

            _txtDosage = new TextBox { Name = "txtDosage", Dock = DockStyle.Fill, Margin = _controlMargin };
            SetWatermark(_txtDosage, "例：0.5g");
            var lblDosage = new Label { Name = "lblDosage", Text = "用药剂量", Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft };
            inputLayout.Controls.Add(lblDosage, 1, 0);
            inputLayout.Controls.Add(_txtDosage, 1, 1);

            _dtpTakeTime = new DateTimePicker
            {
                Name = "dtpTakeTime",
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm",
                Margin = _controlMargin,
                MaxDate = DateTime.Now
            };
            var lblTakeTime = new Label { Name = "lblTakeTime", Text = "用药时间", Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft };
            inputLayout.Controls.Add(lblTakeTime, 2, 0);
            var timePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Margin = new Padding(0)
            };
            timePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            timePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
            timePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            timePanel.Controls.Add(_dtpTakeTime, 0, 0);
            timePanel.Controls.Add(CreateTakeTimeShortcutPanel(), 0, 1);
            inputLayout.Controls.Add(timePanel, 2, 1);

            _cboTakeWay = new ComboBox
            {
                Name = "cboTakeWay",
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = _controlMargin
            };
            _cboTakeWay.Items.AddRange(new[] { "口服", "皮下注射", "静脉注射", "外用", "雾化" });
            _cboTakeWay.SelectedIndex = 0;
            var lblTakeWay = new Label { Name = "lblTakeWay", Text = "用药方式", Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft };
            inputLayout.Controls.Add(lblTakeWay, 3, 0);
            inputLayout.Controls.Add(_cboTakeWay, 3, 1);

            _txtRelatedBS = new TextBox { Name = "txtRelatedBS", Dock = DockStyle.Fill, Margin = _controlMargin };
            SetWatermark(_txtRelatedBS, "选填");
            var lblRelatedBS = new Label { Name = "lblRelatedBS", Text = "关联血糖ID", Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft };
            inputLayout.Controls.Add(lblRelatedBS, 4, 0);
            inputLayout.Controls.Add(_txtRelatedBS, 4, 1);
            inputPanel.Controls.Add(inputLayout);

            var btnSave = CreateButton("保存录入", Color.FromArgb(0, 122, 204), "btnSave");
            var btnCancelEdit = CreateButton("取消编辑", Color.FromArgb(108, 117, 125), "btnCancelEdit");
            var btnClear = CreateButton("清空", Color.FromArgb(108, 117, 125), "btnClear");
            var btnImport = CreateButton("模拟设备导入", Color.FromArgb(40, 167, 69), "btnImport");
            var btnCalibrate = CreateButton("多源数据校准", Color.FromArgb(255, 136, 0), "btnCalibrate");
            var btnExport = CreateButton("导出Excel", Color.FromArgb(102, 16, 242), "btnExport");
            var btnSelectBS = CreateButton("选择血糖", Color.FromArgb(23, 162, 184), "btnSelectBS");
            var btnReminder = CreateButton("用药提醒", Color.FromArgb(111, 66, 193), "btnReminder");

            btnSave.Click += (s, e) =>
            {
                try
                {
                    SaveMedication();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存操作失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            btnCancelEdit.Click += (s, e) => { ClearInput(); };
            btnClear.Click += (s, e) => { try { ClearInput(); } catch (Exception ex) { MessageBox.Show($"清空操作失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); } };
            btnImport.Click += (s, e) => { try { SimulateDeviceImport(); } catch (Exception ex) { MessageBox.Show($"导入操作失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); } };
            btnCalibrate.Click += (s, e) => { try { CalibrateMedicationData(); } catch (Exception ex) { MessageBox.Show($"校准操作失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); } };
            btnExport.Click += (s, e) => { try { ExportMedicineToExcel(); } catch (Exception ex) { MessageBox.Show($"导出操作失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); } };
            btnSelectBS.Click += (s, e) => { try { SelectBloodSugarRecord(); } catch (Exception ex) { MessageBox.Show($"选择血糖记录失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); } };
            btnReminder.Click += (s, e) => { try { OpenReminderForm(); } catch (Exception ex) { MessageBox.Show($"打开用药提醒失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); } };

            btnCancelEdit.Visible = _isEditMode;
            var btnPanel = new FlowLayoutPanel
            {
                Name = "btnPanel",
                Dock = DockStyle.Bottom,
                Height = 45,
                Margin = new Padding(5),
                FlowDirection = FlowDirection.LeftToRight
            };
            btnPanel.Controls.Add(btnSave);
            btnPanel.Controls.Add(btnCancelEdit);
            btnPanel.Controls.Add(btnClear);
            btnPanel.Controls.Add(btnImport);
            btnPanel.Controls.Add(btnCalibrate);
            btnPanel.Controls.Add(btnExport);
            btnPanel.Controls.Add(btnSelectBS);
            btnPanel.Controls.Add(btnReminder);
            inputPanel.Controls.Add(btnPanel);
            return inputPanel;
        }

        private Panel CreateGridPanel()
        {
            var listPanel = new Panel
            {
                Name = "gridPanel",
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0, 10, 0, 18)
            };
            var lblTitle = new Label
            {
                Name = "lblGridTitle",
                Text = "用药历史记录（双击行可编辑）",
                Font = new Font("微软雅黑", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Dock = DockStyle.Top,
                Height = 36
            };
            var pagePanel = CreatePagerPanel();
            _dgvMedicine = new DataGridView
            {
                Name = "dgvMedicineHistory",
                Dock = DockStyle.Fill,
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
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            _dgvMedicine.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "medicine_id", HeaderText = "记录ID", DataPropertyName = "medicine_id", Visible = false },
                new DataGridViewTextBoxColumn { Name = "colDrugName", HeaderText = "药物名称", DataPropertyName = "drug_name", Width = (int)(_colWidth_DrugName * _columnWidthScale) },
                new DataGridViewTextBoxColumn { Name = "colDosage", HeaderText = "用药剂量", DataPropertyName = "drug_dosage", Width = (int)(_colWidth_Dosage * _columnWidthScale) },
                new DataGridViewTextBoxColumn { Name = "colTakeTime", HeaderText = "用药时间", DataPropertyName = "take_medicine_time", Width = (int)(_colWidth_TakeTime * _columnWidthScale) },
                new DataGridViewTextBoxColumn { Name = "colTakeWay", HeaderText = "用药方式", DataPropertyName = "take_way", Width = (int)(_colWidth_TakeWay * _columnWidthScale) },
                new DataGridViewTextBoxColumn { Name = "colSource", HeaderText = "数据来源", DataPropertyName = "data_source", Width = (int)(_colWidth_Source * _columnWidthScale) },
                new DataGridViewTextBoxColumn { Name = "colRelatedBS", HeaderText = "关联血糖ID", DataPropertyName = "related_bs_id", Width = (int)(_colWidth_RelatedBS * _columnWidthScale) },
                new DataGridViewTextBoxColumn { Name = "colCreateTime", HeaderText = "录入时间", DataPropertyName = "create_time", Width = (int)(_colWidth_CreateTime * _columnWidthScale) }
            });
            _dgvMedicine.DefaultCellStyle.Font = new Font("微软雅黑", 9F);
            _dgvMedicine.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", 9F, FontStyle.Bold);
            _dgvMedicine.RowTemplate.Height = 34;
            _dgvMedicine.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dgvMedicine.CellDoubleClick += (s, e) => { if (e.RowIndex < 0) return; LoadEditData(e.RowIndex); };
            listPanel.Controls.Add(_dgvMedicine);
            listPanel.Controls.Add(pagePanel);
            listPanel.Controls.Add(lblTitle);
            return listPanel;
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

            btnPanel.Controls.Add(CreatePageBtn("首页", () => { _currentPageIndex = 1; BindCurrentPageData(); }));
            btnPanel.Controls.Add(CreatePageBtn("上一页", () => { _currentPageIndex--; BindCurrentPageData(); }));
            btnPanel.Controls.Add(CreatePageBtn("下一页", () => { _currentPageIndex++; BindCurrentPageData(); }));
            btnPanel.Controls.Add(CreatePageBtn("末页", () => { _currentPageIndex = GetTotalPage(); BindCurrentPageData(); }));

            layout.Controls.Add(_lblPageInfo, 0, 0);
            layout.Controls.Add(btnPanel, 1, 0);
            pagePanel.Controls.Add(layout);
            return pagePanel;
        }

        private void LoadEditData(int rowIndex)
        {
            try
            {
                var selectedRow = _dgvMedicine.Rows[rowIndex];
                if (selectedRow.Cells["medicine_id"].Value == null || !int.TryParse(selectedRow.Cells["medicine_id"].Value.ToString(), out int medicineId))
                {
                    MessageBox.Show("选中的记录ID无效，无法编辑", "操作提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                _isEditMode = true;
                _currentEditMedicineId = medicineId;
                _lblModeTip.Text = "✏️ 当前模式：编辑用药记录（ID：" + medicineId + "）";
                _lblModeTip.ForeColor = Color.FromArgb(255, 136, 0);
                RefreshTakeTimePickerMaxDate();
                _cboDrug.Text = selectedRow.Cells["colDrugName"].Value?.ToString() ?? "";
                _txtDosage.Text = selectedRow.Cells["colDosage"].Value?.ToString() ?? "";
                _txtDosage.ForeColor = Color.Black;
                if (DateTime.TryParse(selectedRow.Cells["colTakeTime"].Value?.ToString(), out DateTime takeTime))
                {
                    SetSafeTakeTimeValue(takeTime);
                }
                _cboTakeWay.Text = selectedRow.Cells["colTakeWay"].Value?.ToString() ?? "口服";
                var relatedBsId = selectedRow.Cells["colRelatedBS"].Value?.ToString() ?? "0";
                _txtRelatedBS.Text = relatedBsId == "0" ? "选填" : relatedBsId;
                _txtRelatedBS.ForeColor = relatedBsId == "0" ? Color.Gray : Color.Black;
                var btnCancelEdit = this.Controls.Find("btnCancelEdit", true).FirstOrDefault() as Button;
                if (btnCancelEdit != null) btnCancelEdit.Visible = true;
                var btnSave = this.Controls.Find("btnSave", true).FirstOrDefault() as Button;
                if (btnSave != null) btnSave.Text = "确认修改";
                this.ScrollControlIntoView(this.Controls.Find("inputPanel", true).FirstOrDefault());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载编辑数据失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ClearInput();
            }
        }

        private Panel CreateChartPanel()
        {
            var chartPanel = new Panel
            {
                Name = "chartPanel",
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0, 0, 0, 16),
                MinimumSize = new Size(0, 360)
            };
            var lblTitle = new Label
            {
                Name = "lblChartTitle",
                Text = "30天用药趋势图",
                Font = new Font("微软雅黑", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Dock = DockStyle.Top,
                Height = 36
            };
            var chart = new Chart
            {
                Name = "chartMedicineTrend",
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10, 6, 10, 20),
                MinimumSize = new Size(0, 210)
            };
            var chartArea = new ChartArea("MainArea");
            chartArea.AxisX.Title = "日期";
            chartArea.AxisY.Title = "用药剂量(g)";
            chartArea.AxisX.LabelStyle.Format = "MM-dd";
            chartArea.AxisX.LabelStyle.Angle = -30;
            chartArea.AxisX.Interval = 2;
            chartArea.AxisX.IsLabelAutoFit = true;
            chart.ChartAreas.Add(chartArea);
            var series = new Series("每日用药剂量")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.FromArgb(0, 122, 204),
                BorderWidth = 2,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 6
            };
            chart.Series.Add(series);
            var legend = new Legend();
            legend.Docking = Docking.Top;
            chart.Legends.Add(legend);
            chartPanel.Controls.Add(chart);
            chartPanel.Controls.Add(lblTitle);
            return chartPanel;
        }

        private Button CreateButton(string text, Color bgColor, string controlName)
        {
            return new Button
            {
                Name = controlName,
                Text = text,
                Width = 120,
                Height = 35,
                Margin = _btnMargin,
                BackColor = bgColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
        }

        private FlowLayoutPanel CreateTakeTimeShortcutPanel()
        {
            var shortcutPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                WrapContents = true,
                Margin = new Padding(5, 0, 5, 0),
                Padding = new Padding(0)
            };
            shortcutPanel.Controls.Add(CreateTakeTimeShortcutButton("晨起", 7, 0));
            shortcutPanel.Controls.Add(CreateTakeTimeShortcutButton("早餐后", 8, 0));
            shortcutPanel.Controls.Add(CreateTakeTimeShortcutButton("午餐后", 13, 0));
            shortcutPanel.Controls.Add(CreateTakeTimeShortcutButton("晚餐后", 19, 0));
            shortcutPanel.Controls.Add(CreateTakeTimeShortcutButton("睡前", 21, 30));
            return shortcutPanel;
        }

        private Button CreateTakeTimeShortcutButton(string text, int hour, int minute)
        {
            var button = new Button
            {
                Text = text,
                Width = 62,
                Height = 24,
                Margin = new Padding(0, 0, 6, 4),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(237, 246, 255),
                ForeColor = Color.FromArgb(0, 122, 204),
                Font = new Font("微软雅黑", 8F, FontStyle.Bold)
            };
            button.FlatAppearance.BorderColor = Color.FromArgb(0, 122, 204);
            button.Click += (s, e) => ApplyTakeTimeShortcut(hour, minute);
            return button;
        }

        private void ApplyTakeTimeShortcut(int hour, int minute)
        {
            DateTime baseDate = _dtpTakeTime.Value.Date;
            DateTime targetTime = baseDate.AddHours(hour).AddMinutes(minute);
            if (targetTime > DateTime.Now)
            {
                targetTime = DateTime.Now;
            }
            SetSafeTakeTimeValue(targetTime);
        }

        private void RefreshTakeTimePickerMaxDate()
        {
            if (_dtpTakeTime == null)
            {
                return;
            }

            DateTime now = DateTime.Now;
            // 避免MaxDate比MinDate小导致再次异常
            _dtpTakeTime.MaxDate = now < _dtpTakeTime.MinDate ? _dtpTakeTime.MinDate : now;
        }

        private void SetSafeTakeTimeValue(DateTime targetTime)
        {
            if (_dtpTakeTime == null)
            {
                return;
            }

            RefreshTakeTimePickerMaxDate();
            DateTime value = targetTime;
            if (value < _dtpTakeTime.MinDate)
            {
                value = _dtpTakeTime.MinDate;
            }
            if (value > _dtpTakeTime.MaxDate)
            {
                value = _dtpTakeTime.MaxDate;
            }
            _dtpTakeTime.Value = value;
        }

        private void ResetTakeTimePickerToNow()
        {
            SetSafeTakeTimeValue(DateTime.Now);
        }

        private void SetWatermark(TextBox txt, string watermark)
        {
            txt.Text = watermark;
            txt.ForeColor = Color.Gray;
            txt.GotFocus += (s, e) => { if (txt.Text == watermark) { txt.Text = ""; txt.ForeColor = Color.Black; } };
            txt.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txt.Text)) { txt.Text = watermark; txt.ForeColor = Color.Gray; } };
        }

        private void DeleteMedicineRecord(int rowIndex)
        {
            try
            {
                var selectedRow = _dgvMedicine.Rows[rowIndex];
                if (selectedRow.Cells["medicine_id"].Value == null || !int.TryParse(selectedRow.Cells["medicine_id"].Value.ToString(), out int medicineId))
                {
                    MessageBox.Show("选中的记录ID无效，无法删除", "操作提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (MessageBox.Show($"确定要删除这条用药记录吗？\n药物：{selectedRow.Cells["colDrugName"].Value}\n时间：{selectedRow.Cells["colTakeTime"].Value}", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
                int currentUserId = _currentUserId;
                if (currentUserId <= 0)
                {
                    MessageBox.Show("用户登录信息失效，请重新登录", "权限错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var result = _bllMedicine.DeleteMedicineRecord(medicineId, currentUserId);
                if (result.Success)
                {
                    MessageBox.Show(result.Msg, "删除成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    BindMedicationData();
                    LoadMedicineTrendChart();
                }
                else
                {
                    MessageBox.Show(result.Msg, "删除失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除用药记录失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMedicineTrendChart()
        {
            try
            {
                int currentUserId = _currentUserId;
                if (currentUserId <= 0) return;
                var trendList = _bllMedicine.Get30DayMedicineTrend(currentUserId);
                var chart = this.Controls.Find("chartMedicineTrend", true).FirstOrDefault() as Chart;
                if (chart == null) return;
                chart.Series["每日用药剂量"].Points.Clear();
                foreach (var item in trendList)
                {
                    chart.Series["每日用药剂量"].Points.AddXY(item.Date, item.TotalDosage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"【加载趋势图异常】{ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ExportMedicineToExcel()
        {
            try
            {
                int currentUserId = _currentUserId;
                if (currentUserId <= 0)
                {
                    MessageBox.Show("用户登录信息失效，请重新登录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Excel文件(*.xlsx)|*.xlsx|所有文件(*.*)|*.*";
                    saveDialog.FileName = $"用药记录_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        var result = _bllMedicine.ExportMedicineToExcel(currentUserId, saveDialog.FileName);
                        if (result.Success)
                        {
                            MessageBox.Show($"{result.Msg}\n文件已保存至：{saveDialog.FileName}", "导出成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show(result.Msg, "导出失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出Excel失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectBloodSugarRecord()
        {
            try
            {
                int currentUserId = _currentUserId;
                if (currentUserId <= 0)
                {
                    MessageBox.Show("用户登录信息失效，请重新登录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                using (var frm = new FrmSelectBloodSugar(currentUserId))
                {
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        _txtRelatedBS.Text = frm.SelectedBloodSugarId.ToString();
                        _txtRelatedBS.ForeColor = Color.Black;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"选择血糖记录失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenReminderForm()
        {
            try
            {
                int currentUserId = _currentUserId;
                if (currentUserId <= 0)
                {
                    MessageBox.Show("用户登录信息失效，请重新登录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                using (var frm = new FrmMedicineReminder(currentUserId))
                {
                    frm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开用药提醒失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}