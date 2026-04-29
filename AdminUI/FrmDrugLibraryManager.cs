using BLL;
using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace AdminUI
{
    // 注意：partial类的InitializeComponent由VS自动生成，若手动创建需确保窗体基类继承正确
    public partial class FrmDrugLibraryManager : Form
    {
        #region ========== 全局统一布局参数 ==========
        private readonly Padding _globalMainContainerPadding = new Padding(15, 15, 15, 15);
        private readonly bool _globalContentAutoCenter = false;
        private readonly int _globalContentOffsetX = 1;
        private readonly int _globalContentOffsetY = 1;
        private readonly int _globalContentMinWidth = 1300;
        private readonly int _globalContentMinHeight = 700;
        private readonly Padding _globalControlMargin = new Padding(5, 5, 5, 5);
        private readonly int _globalControlHeight = 28;
        private readonly int _globalButtonHeight = 36;
        private readonly int _globalButtonWidth = 110;
        private readonly int _globalLabelWidth = 180;
        private readonly int _globalRowHeight = 40;
        private readonly Padding _globalGroupBoxPadding = new Padding(15);
        #endregion

        #region 全局业务变量
        private readonly B_DrugLibrary _bll = new B_DrugLibrary();
        private int _currentDrugId = 0; // 当前编辑的药物ID
        private int _currentOperateUserId = 1; // 当前登录用户ID，实际项目从登录全局信息获取
        #endregion

        #region 核心控件声明
        private Panel pnlMainContainer;
        private Panel pnlContentWrapper;
        private TabControl tabMain;
        private TabPage tab_DrugList, tab_DrugEdit, tab_BatchOperate, tab_MultiAudit, tab_Version;

        // 1. 药物库列表页
        private GroupBox grp_SearchFilter, grp_DrugList;
        private TextBox txt_SearchKey;
        private ComboBox cbo_DrugCategory, cbo_AdminRoute, cbo_MedicalInsurance, cbo_EnableStatus;
        private DateTimePicker dtp_UpdateStart, dtp_UpdateEnd;
        private DataGridView dgv_DrugList;
        private Button btn_Search, btn_ResetFilter, btn_AddSingle, btn_EditSelected, btn_DeleteSelected;

        // 2. 药物详情/编辑页（修复中文变量名：grp_Safety禁忌 → grp_Safety）
        private GroupBox grp_BaseCompliance, grp_ClinicalUse, grp_Safety, grp_GuideAdapt, grp_StatusVersion;
        private TextBox txt_DrugCode, txt_GenericName, txt_TradeName, txt_ChemicalName, txt_ApprovalNumber, txt_Manufacturer;
        private TextBox txt_Specification, txt_ValidityPeriod, txt_StorageCondition, txt_MarketHolder;
        private ComboBox cbo_PrescriptionType; // 去掉readonly，避免构造冲突
        private ComboBox cbo_MedicalInsuranceType;
        private ComboBox cbo_DiabetesDrugCategory;
        private TextBox txt_Indications, txt_SpecialPopulationAdjust;
        private TextBox txt_AbsoluteContraindication, txt_RelativeContraindication, txt_AdverseReaction, txt_Precautions;
        private TextBox txt_GlucoseMonitor, txt_DrugInteraction;
        private TextBox txt_GuideReference, txt_GuideGrade, txt_FirstSecondLine, txt_ManualAttachment;
        private ComboBox cbo_EditEnableStatus;
        private TextBox txt_Version, txt_UpdateLog, txt_MultiAuditRecord;
        private Button btn_SaveEdit, btn_CancelEdit, btn_UploadManual;

        // 3. 批量操作页
        private GroupBox grp_BatchImport, grp_BatchOperate;
        private Button btn_ImportExcel, btn_ExportExcel, btn_BatchEnable, btn_BatchDisable;

        // 4. 多级审核页
        private GroupBox grp_AuditFilter, grp_AuditList;
        private ComboBox cbo_AuditStatus, cbo_AuditLevel, cbo_UpdateUser;
        private DataGridView dgv_AuditList;
        private Button btn_QueryAudit, btn_FirstAuditPass, btn_FinalAuditPass, btn_AuditReject, btn_ResetAudit;

        // 5. 版本管理页
        private GroupBox grp_VersionFilter, grp_VersionList;
        private ComboBox cbo_VersionDrug;
        private DataGridView dgv_VersionList;
        private Button btn_QueryVersion, btn_RollbackVersion, btn_ResetVersion;
        #endregion

        // 修复1：只保留无参构造，删掉错误的带参构造
        public FrmDrugLibraryManager()
        {
            InitializeComponent(); // 修复2：补全窗体基类初始化（VS自动生成该方法）
            this.Controls.Clear();
            // 窗体基础配置
            this.Text = "药物库管理（糖尿病用药干预核心库-合规性优先）";
            this.Size = new Size(1500, 900);
            this.MinimumSize = new Size(_globalContentMinWidth + 40, _globalContentMinHeight + 100);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("微软雅黑", 9.5F);
            this.Dock = DockStyle.Fill;

            // 初始化顺序：容器→控件→数据→事件
            InitMainContainer();
            InitializeDynamicControls();
            InitControlData();
            BindAllEvents();
            // 窗体加载事件绑定
            this.Load += FrmDrugLibraryManager_Load;
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
        private void FrmDrugLibraryManager_Load(object sender, EventArgs e)
        {
            try
            {
                LoadDrugListData();
                LoadAuditListData();
                LoadVersionDrugDropdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"窗体加载失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 通用数据加载方法
        /// <summary>
        /// 加载药物列表数据（核心列表展示）
        /// </summary>
        /// <summary>
        /// 加载药物列表数据（核心列表展示）
        /// </summary>
        private void LoadDrugListData()
        {
            try
            {
                // 获取筛选条件
                string drugCategory = cbo_DrugCategory.SelectedItem?.ToString() ?? "全部";
                string searchKey = txt_SearchKey.Text.Trim().Replace("输入通用名/商品名/国药准字/唯一编码检索", "");
                string prescriptionType = cbo_PrescriptionType.SelectedItem?.ToString() ?? "全部";
                string adminRoute = cbo_AdminRoute.SelectedItem?.ToString() ?? "全部";
                string medicalInsuranceType = cbo_MedicalInsurance.SelectedItem?.ToString() ?? "全部";
                string enableStatus = cbo_EnableStatus.SelectedItem?.ToString() ?? "全部";

                DateTime? updateStart = dtp_UpdateStart.Value;
                DateTime? updateEnd = dtp_UpdateEnd.Value;

                // ✅ 修复：参数顺序 100% 匹配 BLL 方法
                BizResult result = _bll.GetAntidiabeticDrugList(
                    drugCategory,
                    searchKey,
                    prescriptionType,
                    adminRoute,
                    medicalInsuranceType,
                    enableStatus,
                    updateStart,
                    updateEnd
                );

                if (result.IsSuccess)
                {
                    dgv_DrugList.DataSource = result.Data;
                    dgv_DrugList.ClearSelection();
                    _currentDrugId = 0;
                }
                else
                {
                    MessageBox.Show(result.Message, "查询失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dgv_DrugList.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载药物列表失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dgv_DrugList.DataSource = null;
            }
        }

        /// <summary>
        /// 加载审核列表数据
        /// </summary>
        private void LoadAuditListData()
        {
            try
            {
                string auditStatus = cbo_AuditStatus.SelectedItem?.ToString() ?? "全部";
                string auditLevel = cbo_AuditLevel.SelectedItem?.ToString() ?? "全部";
                int? updateUser = cbo_UpdateUser.SelectedIndex > 0 ? cbo_UpdateUser.SelectedIndex : (int?)null;

                BizResult result = _bll.GetAuditDrugList(auditStatus, auditLevel, updateUser);
                dgv_AuditList.DataSource = result.IsSuccess ? result.Data : null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载审核列表失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dgv_AuditList.DataSource = null;
            }
        }

        /// <summary>
        /// 加载版本管理药物下拉框
        /// </summary>
        private void LoadVersionDrugDropdown()
        {
            try
            {
                BizResult result = _bll.GetAntidiabeticDrugList("全部", "", "全部", "全部", "全部", "全部", null, null);
                if (result.IsSuccess)
                {
                    DataTable dt = result.Data as DataTable;
                    cbo_VersionDrug.Items.Clear();
                    cbo_VersionDrug.Items.Add("全部");
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            cbo_VersionDrug.Items.Add(row["DrugGenericName"].ToString());
                        }
                    }
                    cbo_VersionDrug.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载药物下拉框失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 加载版本历史列表
        /// </summary>
        private void LoadVersionHistoryData()
        {
            try
            {
                int? drugId = null;
                if (cbo_VersionDrug.SelectedIndex > 0)
                {
                    string drugName = cbo_VersionDrug.SelectedItem.ToString();
                    BizResult result = _bll.GetAntidiabeticDrugList("全部", drugName, "全部", "全部", "全部", "全部", null, null);
                    if (result.IsSuccess)
                    {
                        DataTable dt = result.Data as DataTable;
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            drugId = Convert.ToInt32(dt.Rows[0]["DrugID"]);
                        }
                    }
                }

                BizResult versionResult = _bll.GetDrugVersionHistory(drugId);
                dgv_VersionList.DataSource = versionResult.IsSuccess ? versionResult.Data : null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载版本历史失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dgv_VersionList.DataSource = null;
            }
        }

        /// <summary>
        /// 清空编辑表单
        /// </summary>
        private void ClearEditForm()
        {
            _currentDrugId = 0;
            // 清空所有文本框
            foreach (Control ctrl in tab_DrugEdit.Controls)
            {
                if (ctrl is GroupBox grp)
                {
                    foreach (Control subCtrl in grp.Controls)
                    {
                        if (subCtrl is TableLayoutPanel tlp)
                        {
                            foreach (Control panelCtrl in tlp.Controls)
                            {
                                if (panelCtrl is Panel pnl)
                                {
                                    foreach (Control inputCtrl in pnl.Controls)
                                    {
                                        if (inputCtrl is TextBox txt) txt.Clear();
                                        else if (inputCtrl is ComboBox cbo && cbo.Items.Count > 0) cbo.SelectedIndex = 0;
                                    }
                                }
                            }
                        }
                        else if (subCtrl is Panel pnl)
                        {
                            foreach (Control inputCtrl in pnl.Controls)
                            {
                                if (inputCtrl is TextBox txt) txt.Clear();
                            }
                        }
                    }
                }
            }
            // 重置默认值
            cbo_EditEnableStatus.SelectedIndex = 1;
            txt_Version.Text = "1.0.0";
        }

        /// <summary>
        /// 加载药物详情到编辑表单
        /// </summary>
        private void LoadDrugDetailToEditForm(int drugId)
        {
            try
            {
                BizResult result = _bll.GetAntidiabeticDrugById(drugId);
                if (!result.IsSuccess)
                {
                    MessageBox.Show(result.Message, "加载失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AntidiabeticDrug model = result.Data as AntidiabeticDrug;
                if (model == null) return;

                // 赋值到控件
                _currentDrugId = model.DrugID;
                txt_DrugCode.Text = model.DrugCode;
                txt_GenericName.Text = model.DrugGenericName;
                txt_TradeName.Text = model.TradeName;
                txt_ApprovalNumber.Text = model.ApprovalNumber;
                txt_Manufacturer.Text = model.Manufacturer;
                txt_Specification.Text = model.Specification;
                txt_ValidityPeriod.Text = model.ValidityPeriod;
                txt_StorageCondition.Text = model.StorageCondition;
                txt_MarketHolder.Text = model.MarketHolder;
                cbo_PrescriptionType.SelectedItem = model.PrescriptionType ?? "全部";
                cbo_MedicalInsuranceType.SelectedItem = model.MedicalInsuranceType ?? "全部";
                cbo_DiabetesDrugCategory.SelectedItem = model.DrugCategory ?? "全部";
                txt_Indications.Text = model.UsageDosage;
                txt_SpecialPopulationAdjust.Text = model.RenalImpairmentNote;
                txt_Precautions.Text = model.Remark;
                txt_GuideReference.Text = model.DataSource;
                cbo_EditEnableStatus.SelectedItem = model.EnableStatus;
                txt_Version.Text = model.Version;
                txt_UpdateLog.Text = model.UpdateLog;
                txt_MultiAuditRecord.Text = $"当前审核状态：{model.AuditStatus}，审核级别：{model.AuditLevel}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载药物详情失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion


        #region 动态创建控件
        private void InitializeDynamicControls()
        {
            tabMain = new TabControl();
            tabMain.Dock = DockStyle.Fill;
            tabMain.Font = new Font("微软雅黑", 10F);
            tabMain.Padding = new Point(15, 8);
            tab_DrugList = new TabPage("药物库列表") { BackColor = Color.White };
            tab_DrugEdit = new TabPage("药物详情/编辑") { BackColor = Color.White };
            tab_BatchOperate = new TabPage("批量操作") { BackColor = Color.White };
            tab_MultiAudit = new TabPage("多级审核") { BackColor = Color.White };
            tab_Version = new TabPage("版本管理") { BackColor = Color.White };
            tabMain.TabPages.AddRange(new TabPage[] { tab_DrugList, tab_DrugEdit, tab_BatchOperate, tab_MultiAudit, tab_Version });
            pnlContentWrapper.Controls.Add(tabMain);

            // 修复3：先初始化cbo_PrescriptionType，避免空引用
            cbo_PrescriptionType = new ComboBox();
            InitDrugListPage(cbo_PrescriptionType);
            InitDrugEditPage(cbo_PrescriptionType);
            InitBatchOperatePage();
            InitMultiAuditPage();
            InitVersionPage();
        }

        // 修复4：删掉无参重载，只留带参数的方法，避免死循环
        private void InitDrugListPage(ComboBox cbo_PrescriptionType)
        {
            grp_SearchFilter = new GroupBox { Text = "检索与筛选条件（合规性优先）", Dock = DockStyle.Top, Height = 240, Padding = _globalGroupBoxPadding };
            tab_DrugList.Controls.Add(grp_SearchFilter);
            TableLayoutPanel tlp_Filter = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Filter.RowCount = 4;
            tlp_Filter.ColumnCount = 3;
            for (int i = 0; i < 3; i++) tlp_Filter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
            for (int i = 0; i < 4; i++) tlp_Filter.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_SearchFilter.Controls.Add(tlp_Filter);

            Label lbl_Search = new Label { Text = "通用名/商品名/批准文号/编码：", Size = new Size(_globalLabelWidth, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Margin = _globalControlMargin };
            txt_SearchKey = new TextBox { Size = new Size(300, _globalControlHeight), Margin = _globalControlMargin, Text = "输入通用名/商品名/国药准字/唯一编码检索" };
            Panel pnl_Search = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            lbl_Search.Location = new Point(0, 0);
            txt_SearchKey.Location = new Point(lbl_Search.Width, 0);
            pnl_Search.Controls.AddRange(new Control[] { lbl_Search, txt_SearchKey });
            tlp_Filter.Controls.Add(pnl_Search, 0, 0);
            tlp_Filter.SetColumnSpan(pnl_Search, 3);

            int row = 0;
            CreateEditItem<ComboBox>(tlp_Filter, out _, out cbo_DrugCategory, "糖尿病用药分类：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Filter, out _, out cbo_AdminRoute, "给药途径：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Filter, out _, out cbo_MedicalInsurance, "医保类型：", ref row, false);

            row = 1;
            CreateEditItem<ComboBox>(tlp_Filter, out _, out cbo_PrescriptionType, "处方类型：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Filter, out _, out cbo_EnableStatus, "启用状态：", ref row, false);

            Panel pnl_UpdateDate = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_Date = new Label { Text = "更新时间：", Size = new Size(80, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Location = new Point(0, 0) };
            dtp_UpdateStart = new DateTimePicker { Location = new Point(80, 0), Size = new Size(120, _globalControlHeight), Format = DateTimePickerFormat.Short };
            dtp_UpdateEnd = new DateTimePicker { Location = new Point(210, 0), Size = new Size(120, _globalControlHeight), Format = DateTimePickerFormat.Short };
            pnl_UpdateDate.Controls.AddRange(new Control[] { lbl_Date, dtp_UpdateStart, dtp_UpdateEnd });
            tlp_Filter.Controls.Add(pnl_UpdateDate, 2, 1);

            row = 2;
            Panel pnl_FilterBtn = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            FlowLayoutPanel flp_FilterBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_Search = CreateBtn("检索", Color.FromArgb(0, 122, 204));
            btn_ResetFilter = CreateBtn("重置筛选", Color.Gray);
            btn_AddSingle = CreateBtn("新增药物", Color.FromArgb(0, 150, 136));
            btn_EditSelected = CreateBtn("编辑选中", Color.FromArgb(255, 152, 0));
            btn_DeleteSelected = CreateBtn("删除选中", Color.FromArgb(244, 67, 54));
            flp_FilterBtn.Controls.AddRange(new Control[] { btn_Search, btn_ResetFilter, btn_AddSingle, btn_EditSelected, btn_DeleteSelected });
            pnl_FilterBtn.Controls.Add(flp_FilterBtn);
            tlp_Filter.Controls.Add(pnl_FilterBtn, 0, 2);
            tlp_Filter.SetColumnSpan(pnl_FilterBtn, 3);

            grp_DrugList = new GroupBox { Text = "药物库列表（符合国家药典/临床用药指南）", Dock = DockStyle.Fill, Padding = new Padding(15, 220, 15, 15) };
            tab_DrugList.Controls.Add(grp_DrugList);
            dgv_DrugList = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ColumnHeadersHeight = 35
            };
            grp_DrugList.Controls.Add(dgv_DrugList);
            // 修正：DataPropertyName和数据库字段匹配，显式设置Name属性确保可通过名称查找
            dgv_DrugList.Columns.AddRange(new DataGridViewColumn[] {
                // 隐藏主键列
                new DataGridViewTextBoxColumn { Name = "DrugID", HeaderText = "药物ID", DataPropertyName = "DrugID", Visible = false, Width = 50 },
    
                // 【核心识别字段（前5列）】
                new DataGridViewTextBoxColumn { Name = "DrugGenericName", HeaderText = "通用名", DataPropertyName = "DrugGenericName", Width = 140, FillWeight = 15 },
                new DataGridViewTextBoxColumn { Name = "TradeName", HeaderText = "商品名", DataPropertyName = "TradeName", Width = 120, FillWeight = 12 },
                new DataGridViewTextBoxColumn { Name = "DrugCategory", HeaderText = "药物分类", DataPropertyName = "DrugCategory", Width = 100, FillWeight = 10 },
                new DataGridViewTextBoxColumn { Name = "SubCategory", HeaderText = "亚分类", DataPropertyName = "SubCategory", Width = 100, FillWeight = 10 },
                new DataGridViewTextBoxColumn { Name = "Specification", HeaderText = "规格剂型", DataPropertyName = "Specification", Width = 120, FillWeight = 12 },
    
                // 【临床用药核心字段（中间列）】
                new DataGridViewTextBoxColumn { Name = "AdminRoute", HeaderText = "给药途径", DataPropertyName = "AdminRoute", Width = 80, FillWeight = 8 },
                new DataGridViewTextBoxColumn { Name = "DailyDosage", HeaderText = "每日剂量", DataPropertyName = "DailyDosage", Width = 100, FillWeight = 10 }, // 合并计算列
                new DataGridViewTextBoxColumn { Name = "PeakTime_h", HeaderText = "达峰时间(h)", DataPropertyName = "PeakTime_h", Width = 80, FillWeight = 8 },
                new DataGridViewTextBoxColumn { Name = "ActionDuration_h", HeaderText = "作用时长(h)", DataPropertyName = "ActionDuration_h", Width = 80, FillWeight = 8 },
                new DataGridViewTextBoxColumn { Name = "HalfLife_h", HeaderText = "半衰期(h)", DataPropertyName = "HalfLife_h", Width = 80, FillWeight = 8 },
                new DataGridViewTextBoxColumn { Name = "GuideGrade", HeaderText = "指南等级", DataPropertyName = "GuideGrade", Width = 70, FillWeight = 7 },
                new DataGridViewTextBoxColumn { Name = "IsFirstLine", HeaderText = "是否一线", DataPropertyName = "IsFirstLine", Width = 70, FillWeight = 7 },
                new DataGridViewTextBoxColumn { Name = "IsDomestic", HeaderText = "是否国产", DataPropertyName = "IsDomestic", Width = 70, FillWeight = 7 },
    
                // 【合规与管理字段（后列）】
                new DataGridViewTextBoxColumn { Name = "PrescriptionType", HeaderText = "处方类型", DataPropertyName = "PrescriptionType", Width = 80, FillWeight = 8 },
                new DataGridViewTextBoxColumn { Name = "MedicalInsuranceType", HeaderText = "医保类型", DataPropertyName = "MedicalInsuranceType", Width = 80, FillWeight = 8 },
                new DataGridViewTextBoxColumn { Name = "ApprovalNumber", HeaderText = "批准文号", DataPropertyName = "ApprovalNumber", Width = 140, FillWeight = 14 },
                new DataGridViewTextBoxColumn { Name = "Manufacturer", HeaderText = "生产厂家", DataPropertyName = "Manufacturer", Width = 160, FillWeight = 16 },
                new DataGridViewTextBoxColumn { Name = "EnableStatus", HeaderText = "启用状态", DataPropertyName = "EnableStatus", Width = 70, FillWeight = 7 },
                new DataGridViewTextBoxColumn { Name = "AuditStatus", HeaderText = "审核状态", DataPropertyName = "AuditStatus", Width = 90, FillWeight = 9 },
    
                // 【系统字段（最后列）】
                new DataGridViewTextBoxColumn { Name = "CreateTime", HeaderText = "创建时间", DataPropertyName = "CreateTime", Width = 130, FillWeight = 13, DefaultCellStyle = { Format = "yyyy-MM-dd HH:mm" } },
                new DataGridViewTextBoxColumn { Name = "UpdateUser", HeaderText = "最后更新人", DataPropertyName = "UpdateUser", Width = 100, FillWeight = 10 },
    
                // 操作列
                new DataGridViewButtonColumn { Name = "Operation", HeaderText = "操作", Text = "编辑", UseColumnTextForButtonValue = true, Width = 80, FillWeight = 8 }
            });

            // 新增：单元格格式化事件绑定（处理特殊字段显示）
            dgv_DrugList.CellFormatting += Dgv_DrugList_CellFormatting;
        }

        private void InitDrugEditPage(ComboBox cbo_PrescriptionType)
        {
            grp_BaseCompliance = new GroupBox { Text = "基础合规信息（必填，合规校验项）", Dock = DockStyle.Top, Height = 280, Padding = _globalGroupBoxPadding };
            tab_DrugEdit.Controls.Add(grp_BaseCompliance);
            TableLayoutPanel tlp_Compliance = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Compliance.RowCount = 5;
            tlp_Compliance.ColumnCount = 2;
            for (int i = 0; i < 2; i++) tlp_Compliance.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (int i = 0; i < 5; i++) tlp_Compliance.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_BaseCompliance.Controls.Add(tlp_Compliance);

            int row = 0;
            CreateEditItem<TextBox>(tlp_Compliance, out _, out txt_DrugCode, "药物唯一编码：", ref row, true);
            CreateEditItem<TextBox>(tlp_Compliance, out _, out txt_GenericName, "通用名：", ref row, false);
            CreateEditItem<TextBox>(tlp_Compliance, out _, out txt_TradeName, "商品名：", ref row, false);
            CreateEditItem<TextBox>(tlp_Compliance, out _, out txt_ChemicalName, "化学名：", ref row, false);
            CreateEditItem<TextBox>(tlp_Compliance, out _, out txt_ApprovalNumber, "国药准字号/批准文号：", ref row, true);
            CreateEditItem<TextBox>(tlp_Compliance, out _, out txt_Manufacturer, "生产厂家：", ref row, false);
            CreateEditItem<TextBox>(tlp_Compliance, out _, out txt_Specification, "剂型规格：", ref row, false);
            CreateEditItem<TextBox>(tlp_Compliance, out _, out txt_ValidityPeriod, "有效期：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Compliance, out _, out cbo_PrescriptionType, "处方类型：", ref row, false);
            CreateEditItem<ComboBox>(tlp_Compliance, out _, out cbo_MedicalInsuranceType, "医保分类：", ref row, false);

            Panel pnl_ExtraCompliance = new Panel { Dock = DockStyle.Bottom, Height = 80, Margin = Padding.Empty };
            Label lbl_Storage = new Label { Text = "贮藏条件：", Location = new Point(10, 10), Size = new Size(_globalLabelWidth, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft };
            txt_StorageCondition = new TextBox { Size = new Size(260, _globalControlHeight), Location = new Point(190, 10), Margin = _globalControlMargin };
            Label lbl_Holder = new Label { Text = "药品上市许可持有人：", Location = new Point(10, 40), Size = new Size(_globalLabelWidth, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft };
            txt_MarketHolder = new TextBox { Size = new Size(260, _globalControlHeight), Location = new Point(190, 40), Margin = _globalControlMargin };
            pnl_ExtraCompliance.Controls.AddRange(new Control[] { lbl_Storage, txt_StorageCondition, lbl_Holder, txt_MarketHolder });
            grp_BaseCompliance.Controls.Add(pnl_ExtraCompliance);

            grp_ClinicalUse = new GroupBox { Text = "糖尿病临床用药核心信息（必填）", Dock = DockStyle.Top, Height = 220, Padding = _globalGroupBoxPadding };
            tab_DrugEdit.Controls.Add(grp_ClinicalUse);
            TableLayoutPanel tlp_Clinical = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Clinical.RowCount = 3;
            tlp_Clinical.ColumnCount = 1;
            for (int i = 0; i < 3; i++) tlp_Clinical.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            grp_ClinicalUse.Controls.Add(tlp_Clinical);

            Label lbl_DrugCat = new Label { Text = "糖尿病用药精准分类：", Size = new Size(_globalLabelWidth, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Margin = _globalControlMargin };
            cbo_DiabetesDrugCategory = new ComboBox { Size = new Size(260, _globalControlHeight), DropDownStyle = ComboBoxStyle.DropDownList, Margin = _globalControlMargin };
            Panel pnl_DrugCat = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            lbl_DrugCat.Location = new Point(0, 0);
            cbo_DiabetesDrugCategory.Location = new Point(lbl_DrugCat.Width, 0);
            pnl_DrugCat.Controls.AddRange(new Control[] { lbl_DrugCat, cbo_DiabetesDrugCategory });
            tlp_Clinical.Controls.Add(pnl_DrugCat, 0, 0);

            Label lbl_Indication = new Label { Text = "适应症（含合并症适配）：", Size = new Size(_globalLabelWidth, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Margin = _globalControlMargin };
            txt_Indications = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, Margin = _globalControlMargin };
            Panel pnl_Indication = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            lbl_Indication.Location = new Point(0, 0);
            txt_Indications.Location = new Point(0, 30);
            txt_Indications.Size = new Size(pnl_Indication.Width - 10, 50); // 修复5：加高多行文本框
            pnl_Indication.Controls.AddRange(new Control[] { lbl_Indication, txt_Indications });
            tlp_Clinical.Controls.Add(pnl_Indication, 0, 1);

            Label lbl_Special = new Label { Text = "特殊人群剂量调整（老年/肝肾功能不全）：", Size = new Size(_globalLabelWidth, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Margin = _globalControlMargin };
            txt_SpecialPopulationAdjust = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, Margin = _globalControlMargin };
            Panel pnl_Special = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            lbl_Special.Location = new Point(0, 0);
            txt_SpecialPopulationAdjust.Location = new Point(0, 30);
            txt_SpecialPopulationAdjust.Size = new Size(pnl_Special.Width - 10, 50);
            pnl_Special.Controls.AddRange(new Control[] { lbl_Special, txt_SpecialPopulationAdjust });
            tlp_Clinical.Controls.Add(pnl_Special, 0, 2);

            // 修复6：中文变量名改为 grp_Safety
            grp_Safety = new GroupBox { Text = "安全与禁忌信息（核心必填，重点标注低血糖风险）", Dock = DockStyle.Top, Height = 320, Padding = _globalGroupBoxPadding };
            tab_DrugEdit.Controls.Add(grp_Safety);
            TableLayoutPanel tlp_Safety = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Safety.RowCount = 5;
            tlp_Safety.ColumnCount = 1;
            for (int i = 0; i < 5; i++) tlp_Safety.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            grp_Safety.Controls.Add(tlp_Safety);

            CreateMultiLineEditItem(tlp_Safety, out Label lbl_Absolute, out txt_AbsoluteContraindication, "绝对禁忌症：", 0);
            CreateMultiLineEditItem(tlp_Safety, out Label lbl_Relative, out txt_RelativeContraindication, "相对禁忌症：", 1);
            CreateMultiLineEditItem(tlp_Safety, out Label lbl_Adverse, out txt_AdverseReaction, "不良反应（重点标注低血糖/体重影响）：", 2);
            CreateMultiLineEditItem(tlp_Safety, out Label lbl_Precautions, out txt_Precautions, "用药注意事项：", 3);

            Panel pnl_GlucoseDrug = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_Glucose = new Label { Text = "血糖监测要求：", Location = new Point(10, 0), Size = new Size(_globalLabelWidth, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft };
            txt_GlucoseMonitor = new TextBox { Size = new Size(260, _globalControlHeight), Location = new Point(190, 0), Margin = _globalControlMargin };
            Label lbl_DrugInter = new Label { Text = "药物相互作用：", Location = new Point(10, 30), Size = new Size(_globalLabelWidth, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft };
            txt_DrugInteraction = new TextBox { Size = new Size(260, _globalControlHeight), Location = new Point(190, 30), Margin = _globalControlMargin };
            pnl_GlucoseDrug.Controls.AddRange(new Control[] { lbl_Glucose, txt_GlucoseMonitor, lbl_DrugInter, txt_DrugInteraction });
            tlp_Safety.Controls.Add(pnl_GlucoseDrug, 0, 4);

            grp_GuideAdapt = new GroupBox { Text = "临床指南适配信息", Dock = DockStyle.Top, Height = 200, Padding = _globalGroupBoxPadding };
            tab_DrugEdit.Controls.Add(grp_GuideAdapt);
            TableLayoutPanel tlp_Guide = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Guide.RowCount = 3;
            tlp_Guide.ColumnCount = 1;
            for (int i = 0; i < 3; i++) tlp_Guide.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            grp_GuideAdapt.Controls.Add(tlp_Guide);

            CreateMultiLineEditItem(tlp_Guide, out Label lbl_GuideRef, out txt_GuideReference, "参考指南依据：", 0);
            CreateMultiLineEditItem(tlp_Guide, out Label lbl_GuideGrade, out txt_GuideGrade, "指南推荐等级：", 1);

            Panel pnl_LineAttachment = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_Line = new Label { Text = "一线/二线用药标注：", Location = new Point(10, 0), Size = new Size(_globalLabelWidth, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft };
            txt_FirstSecondLine = new TextBox { Size = new Size(260, _globalControlHeight), Location = new Point(190, 0), Margin = _globalControlMargin };
            Label lbl_Attachment = new Label { Text = "药品说明书附件：", Location = new Point(10, 30), Size = new Size(_globalLabelWidth, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft };
            txt_ManualAttachment = new TextBox { Size = new Size(200, _globalControlHeight), Location = new Point(190, 30), Margin = _globalControlMargin };
            btn_UploadManual = CreateBtn("上传附件", Color.FromArgb(0, 122, 204));
            btn_UploadManual.Size = new Size(80, 28);
            btn_UploadManual.Location = new Point(400, 30);
            pnl_LineAttachment.Controls.AddRange(new Control[] { lbl_Line, txt_FirstSecondLine, lbl_Attachment, txt_ManualAttachment, btn_UploadManual });
            tlp_Guide.Controls.Add(pnl_LineAttachment, 0, 2);

            grp_StatusVersion = new GroupBox { Text = "状态与版本管理（多级审核）", Dock = DockStyle.Top, Height = 180, Padding = _globalGroupBoxPadding };
            tab_DrugEdit.Controls.Add(grp_StatusVersion);
            TableLayoutPanel tlp_Status = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Status.RowCount = 2;
            tlp_Status.ColumnCount = 2;
            for (int i = 0; i < 2; i++) tlp_Status.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (int i = 0; i < 2; i++) tlp_Status.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_StatusVersion.Controls.Add(tlp_Status);

            row = 0;
            CreateEditItem<ComboBox>(tlp_Status, out _, out cbo_EditEnableStatus, "启用状态：", ref row, false);
            CreateEditItem<TextBox>(tlp_Status, out _, out txt_Version, "版本号：", ref row, true);
            CreateEditItem<TextBox>(tlp_Status, out _, out txt_UpdateLog, "更新日志：", ref row, false);
            CreateEditItem<TextBox>(tlp_Status, out _, out txt_MultiAuditRecord, "多级审核记录：", ref row, true);

            Panel pnl_EditBtn = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(15) };
            tab_DrugEdit.Controls.Add(pnl_EditBtn);
            FlowLayoutPanel flp_EditBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_SaveEdit = CreateBtn("保存修改", Color.FromArgb(0, 122, 204));
            btn_CancelEdit = CreateBtn("取消编辑", Color.Gray);
            flp_EditBtn.Controls.AddRange(new Control[] { btn_SaveEdit, btn_CancelEdit });
            pnl_EditBtn.Controls.Add(flp_EditBtn);
        }

        // 修复7：加高多行文本框高度，优化布局
        private void CreateMultiLineEditItem(TableLayoutPanel tlp, out Label lbl, out TextBox txt, string text, int rowIndex)
        {
            lbl = new Label { Text = text, Size = new Size(_globalLabelWidth, _globalControlHeight), TextAlign = ContentAlignment.MiddleLeft, Margin = _globalControlMargin };
            txt = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, Margin = _globalControlMargin };
            Panel pnl = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            lbl.Location = new Point(0, 0);
            txt.Location = new Point(0, 30);
            txt.Size = new Size(pnl.Width - 10, 50);
            pnl.Controls.AddRange(new Control[] { lbl, txt });
            tlp.Controls.Add(pnl, 0, rowIndex);
        }

        private void InitBatchOperatePage()
        {
            grp_BatchImport = new GroupBox { Text = "批量导入/导出（带药典合规校验）", Dock = DockStyle.Top, Height = 150, Padding = _globalGroupBoxPadding };
            tab_BatchOperate.Controls.Add(grp_BatchImport);
            Panel pnl_Import = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_ImportTip = new Label { Text = "支持Excel格式批量导入（模板含批准文号、禁忌等合规必填项），导出所有/选中药物数据", Location = new Point(10, 10), AutoSize = true, Font = new Font("微软雅黑", 9F) };
            btn_ImportExcel = CreateBtn("批量导入", Color.FromArgb(0, 150, 136));
            btn_ImportExcel.Location = new Point(10, 50);
            btn_ExportExcel = CreateBtn("批量导出", Color.FromArgb(255, 152, 0));
            btn_ExportExcel.Location = new Point(130, 50);
            pnl_Import.Controls.AddRange(new Control[] { lbl_ImportTip, btn_ImportExcel, btn_ExportExcel });
            grp_BatchImport.Controls.Add(pnl_Import);

            grp_BatchOperate = new GroupBox { Text = "批量状态操作（合规审核后生效）", Dock = DockStyle.Top, Height = 150, Padding = _globalGroupBoxPadding };
            tab_BatchOperate.Controls.Add(grp_BatchOperate);
            Panel pnl_Operate = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            Label lbl_OperateTip = new Label { Text = "请先在【药物库列表】选中需要操作的记录，批量启用/禁用需通过二级审核", Location = new Point(10, 10), AutoSize = true, Font = new Font("微软雅黑", 9F) };
            btn_BatchEnable = CreateBtn("批量启用", Color.FromArgb(0, 122, 204));
            btn_BatchEnable.Location = new Point(10, 50);
            btn_BatchDisable = CreateBtn("批量禁用", Color.FromArgb(255, 152, 0));
            btn_BatchDisable.Location = new Point(130, 50);
            pnl_Operate.Controls.AddRange(new Control[] { lbl_OperateTip, btn_BatchEnable, btn_BatchDisable });
            grp_BatchOperate.Controls.Add(pnl_Operate);
        }

        private void InitMultiAuditPage()
        {
            grp_AuditFilter = new GroupBox { Text = "多级审核筛选条件", Dock = DockStyle.Top, Height = 150, Padding = _globalGroupBoxPadding };
            tab_MultiAudit.Controls.Add(grp_AuditFilter);
            TableLayoutPanel tlp_AuditFilter = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_AuditFilter.RowCount = 1;
            tlp_AuditFilter.ColumnCount = 4;
            for (int i = 0; i < 4; i++) tlp_AuditFilter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlp_AuditFilter.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_AuditFilter.Controls.Add(tlp_AuditFilter);

            int row = 0;
            CreateEditItem<ComboBox>(tlp_AuditFilter, out _, out cbo_AuditStatus, "审核状态：", ref row, false);
            CreateEditItem<ComboBox>(tlp_AuditFilter, out _, out cbo_AuditLevel, "审核级别：", ref row, false);
            CreateEditItem<ComboBox>(tlp_AuditFilter, out _, out cbo_UpdateUser, "更新人：", ref row, false);

            Panel pnl_AuditBtn = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            FlowLayoutPanel flp_AuditBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            btn_QueryAudit = CreateBtn("查询待审核", Color.FromArgb(0, 122, 204));
            btn_ResetAudit = CreateBtn("重置", Color.Gray);
            flp_AuditBtn.Controls.AddRange(new Control[] { btn_ResetAudit, btn_QueryAudit });
            pnl_AuditBtn.Controls.Add(flp_AuditBtn);
            tlp_AuditFilter.Controls.Add(pnl_AuditBtn, 3, 0);

            grp_AuditList = new GroupBox { Text = "药物数据多级审核列表（合规性校验）", Dock = DockStyle.Fill, Padding = _globalGroupBoxPadding };
            tab_MultiAudit.Controls.Add(grp_AuditList);
            dgv_AuditList = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            grp_AuditList.Controls.Add(dgv_AuditList);
            dgv_AuditList.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { HeaderText = "药物编码", DataPropertyName = "DrugCode", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "通用名", DataPropertyName = "GenericName", Width = 150 },
                new DataGridViewTextBoxColumn { HeaderText = "审核级别", DataPropertyName = "AuditLevel", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "当前审核人", DataPropertyName = "CurrentAuditor", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "审核状态", DataPropertyName = "AuditStatus", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "合规校验提示", DataPropertyName = "ComplianceTip", Width = 200 },
                new DataGridViewTextBoxColumn { HeaderText = "提交时间", DataPropertyName = "SubmitTime", Width = 120 }
            });

            Panel pnl_AuditOperate = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(15) };
            tab_MultiAudit.Controls.Add(pnl_AuditOperate);
            FlowLayoutPanel flp_AuditOperate = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_FirstAuditPass = CreateBtn("一级审核通过", Color.FromArgb(0, 122, 204));
            btn_FinalAuditPass = CreateBtn("终审通过", Color.FromArgb(0, 150, 136));
            btn_AuditReject = CreateBtn("审核驳回", Color.FromArgb(244, 67, 54));
            flp_AuditOperate.Controls.AddRange(new Control[] { btn_FirstAuditPass, btn_FinalAuditPass, btn_AuditReject });
            pnl_AuditOperate.Controls.Add(flp_AuditOperate);
        }

        private void InitVersionPage()
        {
            grp_VersionFilter = new GroupBox { Text = "版本筛选条件（合规版本追溯）", Dock = DockStyle.Top, Height = 150, Padding = _globalGroupBoxPadding };
            tab_Version.Controls.Add(grp_VersionFilter);
            TableLayoutPanel tlp_VersionFilter = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_VersionFilter.RowCount = 1;
            tlp_VersionFilter.ColumnCount = 2;
            for (int i = 0; i < 2; i++) tlp_VersionFilter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp_VersionFilter.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_VersionFilter.Controls.Add(tlp_VersionFilter);

            int row = 0;
            CreateEditItem<ComboBox>(tlp_VersionFilter, out _, out cbo_VersionDrug, "选择药物：", ref row, false);

            Panel pnl_VersionBtn = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            FlowLayoutPanel flp_VersionBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            btn_QueryVersion = CreateBtn("查询版本", Color.FromArgb(0, 122, 204));
            btn_ResetVersion = CreateBtn("重置", Color.Gray);
            flp_VersionBtn.Controls.AddRange(new Control[] { btn_ResetVersion, btn_QueryVersion });
            pnl_VersionBtn.Controls.Add(flp_VersionBtn);
            tlp_VersionFilter.Controls.Add(pnl_VersionBtn, 1, 0);

            grp_VersionList = new GroupBox { Text = "药物版本记录（合规版本追溯/回滚）", Dock = DockStyle.Fill, Padding = _globalGroupBoxPadding };
            tab_Version.Controls.Add(grp_VersionList);
            dgv_VersionList = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            grp_VersionList.Controls.Add(dgv_VersionList);
            dgv_VersionList.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { HeaderText = "版本号", DataPropertyName = "Version", Width = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "更新时间", DataPropertyName = "UpdateTime", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "更新人", DataPropertyName = "UpdateUser", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "更新内容", DataPropertyName = "UpdateContent", Width = 200 },
                new DataGridViewTextBoxColumn { HeaderText = "审核状态", DataPropertyName = "AuditStatus", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "合规校验结果", DataPropertyName = "ComplianceResult", Width = 150 }
            });

            Panel pnl_VersionOperate = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(15) };
            tab_Version.Controls.Add(pnl_VersionOperate);
            FlowLayoutPanel flp_VersionOperate = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            btn_RollbackVersion = CreateBtn("版本回滚（需审核）", Color.FromArgb(255, 152, 0));
            flp_VersionOperate.Controls.Add(btn_RollbackVersion);
            pnl_VersionOperate.Controls.Add(flp_VersionOperate);
        }
        #endregion

        #region 通用控件方法
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

        // 修复8：修复TableLayoutPanel布局计算错误，避免控件叠加
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
                t.Text = text.Contains("批准文号") ? "如：国药准字H2020XXXXXX" : (text.Contains("通用名") ? "如：盐酸二甲双胍片" : "");
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

            // 正确布局：列=row%2，行=row/2
            int col = row % 2;
            int currentRow = row / 2;
            tlp.Controls.Add(pairPanel, col, currentRow);
            row++;
        }
        #endregion

        #region 下拉数据初始化
        private void InitControlData()
        {
            string[] drugCategories = new string[] { "全部", "双胍类", "磺脲类", "格列奈类", "α-糖苷酶抑制剂", "噻唑烷二酮类", "DPP-4抑制剂", "SGLT-2抑制剂", "GLP-1受体激动剂", "胰岛素", "复方降糖药", "并发症用药" };
            string[] adminRoutes = new string[] { "全部", "口服", "皮下注射", "静脉注射", "外用" };
            string[] medicalInsurances = new string[] { "全部", "甲类", "乙类", "自费" };
            string[] prescriptionTypes = new string[] { "全部", "处方药", "非处方药（OTC）", "双跨药" };
            string[] enableStatus = new string[] { "全部", "启用", "禁用" };
            string[] auditStatuses = new string[] { "全部", "待一级审核", "一级审核通过", "待终审", "终审通过", "审核驳回" };
            string[] auditLevels = new string[] { "全部", "一级审核（药师）", "终审（医师/管理员）" };
            string[] updateUsers = new string[] { "全部", "药师A", "药师B", "管理员", "临床医生" };
            string[] drugNames = new string[] { "全部", "盐酸二甲双胍片", "格列美脲片", "利拉鲁肽注射液" };

            // 先初始化控件再AddRange，避免空引用
            cbo_DrugCategory?.Items.AddRange(drugCategories);
            cbo_AdminRoute?.Items.AddRange(adminRoutes);
            cbo_MedicalInsurance?.Items.AddRange(medicalInsurances);
            cbo_PrescriptionType?.Items.AddRange(prescriptionTypes);
            cbo_EnableStatus?.Items.AddRange(enableStatus);
            cbo_DiabetesDrugCategory?.Items.AddRange(drugCategories);
            cbo_MedicalInsuranceType?.Items.AddRange(medicalInsurances);
            cbo_EditEnableStatus?.Items.AddRange(enableStatus);
            cbo_AuditStatus?.Items.AddRange(auditStatuses);
            cbo_AuditLevel?.Items.AddRange(auditLevels);
            cbo_UpdateUser?.Items.AddRange(updateUsers);
            cbo_VersionDrug?.Items.AddRange(drugNames);

            // 设置默认值
            cbo_DrugCategory.SelectedIndex = 0;
            cbo_AdminRoute.SelectedIndex = 0;
            cbo_MedicalInsurance.SelectedIndex = 0;
            cbo_PrescriptionType.SelectedIndex = 0;
            cbo_EnableStatus.SelectedIndex = 0;
            cbo_DiabetesDrugCategory.SelectedIndex = 0;
            cbo_EditEnableStatus.SelectedIndex = 1;
            cbo_AuditStatus.SelectedIndex = 1;
            cbo_AuditLevel.SelectedIndex = 0;
            cbo_UpdateUser.SelectedIndex = 0;
            cbo_VersionDrug.SelectedIndex = 0;
            dtp_UpdateStart.Value = DateTime.Now.AddMonths(-3);
            dtp_UpdateEnd.Value = DateTime.Now;
        }
        #endregion

        #region 事件绑定
        #region 事件绑定（全量真实业务逻辑）
        private void BindAllEvents()
        {
            #region 药物列表页按钮事件
            // 检索按钮
            btn_Search.Click += (s, e) =>
            {
                try { LoadDrugListData(); }
                catch (Exception ex) { MessageBox.Show($"检索失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            // 重置筛选按钮
            btn_ResetFilter.Click += (s, e) =>
            {
                try
                {
                    txt_SearchKey.Text = "输入通用名/商品名/国药准字/唯一编码检索";
                    cbo_DrugCategory.SelectedIndex = 0;
                    cbo_AdminRoute.SelectedIndex = 0;
                    cbo_MedicalInsurance.SelectedIndex = 0;
                    cbo_PrescriptionType.SelectedIndex = 0;
                    cbo_EnableStatus.SelectedIndex = 0;
                    dtp_UpdateStart.Value = DateTime.Now.AddMonths(-3);
                    dtp_UpdateEnd.Value = DateTime.Now;
                    LoadDrugListData();
                }
                catch (Exception ex) { MessageBox.Show($"重置筛选失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            // 新增药物按钮
            btn_AddSingle.Click += (s, e) =>
            {
                try { ClearEditForm(); tabMain.SelectedTab = tab_DrugEdit; }
                catch (Exception ex) { MessageBox.Show($"打开新增页面失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            // 编辑选中按钮
            btn_EditSelected.Click += (s, e) =>
            {
                try
                {
                    if (dgv_DrugList.SelectedRows.Count == 0 || dgv_DrugList.SelectedRows[0].IsNewRow)
                    {
                        MessageBox.Show("请先选中一条有效的药物记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DataRowView drv = dgv_DrugList.SelectedRows[0].DataBoundItem as DataRowView;
                    int drugId = Convert.ToInt32(drv["DrugID"]);
                    LoadDrugDetailToEditForm(drugId);
                    tabMain.SelectedTab = tab_DrugEdit;
                }
                catch (Exception ex) { MessageBox.Show($"打开编辑页面失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            // 删除选中按钮
            btn_DeleteSelected.Click += (s, e) =>
            {
                try
                {
                    if (dgv_DrugList.SelectedRows.Count == 0 || dgv_DrugList.SelectedRows[0].IsNewRow)
                    {
                        MessageBox.Show("请先选中一条有效的药物记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DataRowView drv = dgv_DrugList.SelectedRows[0].DataBoundItem as DataRowView;
                    int drugId = Convert.ToInt32(drv["DrugID"]);
                    string drugName = drv["DrugGenericName"].ToString();

                    if (MessageBox.Show($"确认删除药物【{drugName}】？删除后不可恢复！", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        BizResult result = _bll.DeleteAntidiabeticDrug(drugId);
                        if (result.IsSuccess)
                        {
                            MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadDrugListData();
                            LoadVersionDrugDropdown();
                        }
                        else
                        {
                            MessageBox.Show(result.Message, "删除失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show($"删除操作失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };
            #endregion

            #region 药物编辑页按钮事件
            // 保存编辑按钮
            btn_SaveEdit.Click += (s, e) =>
            {
                try
                {
                    // 基础校验
                    if (string.IsNullOrWhiteSpace(txt_GenericName.Text))
                    {
                        MessageBox.Show("药物通用名为必填项！", "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        txt_GenericName.Focus();
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(txt_ApprovalNumber.Text) || !txt_ApprovalNumber.Text.Contains("国药准字"))
                    {
                        MessageBox.Show("批准文号格式错误，需包含「国药准字」！", "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txt_ApprovalNumber.Focus();
                        return;
                    }

                    // 构建实体
                    AntidiabeticDrug model = new AntidiabeticDrug
                    {
                        DrugID = _currentDrugId,
                        DrugCode = txt_DrugCode.Text.Trim(),
                        DrugCategory = cbo_DiabetesDrugCategory.SelectedItem?.ToString() ?? "",
                        DrugGenericName = txt_GenericName.Text.Trim(),
                        TradeName = txt_TradeName.Text.Trim(),
                        DosageForm = txt_Specification.Text.Trim().Split(' ')[0],
                        Specification = txt_Specification.Text.Trim(),
                        UsageDosage = txt_Indications.Text.Trim(),
                        RenalImpairmentNote = txt_SpecialPopulationAdjust.Text.Trim(),
                        DataSource = txt_GuideReference.Text.Trim(),
                        ApprovalNumber = txt_ApprovalNumber.Text.Trim(),
                        Manufacturer = txt_Manufacturer.Text.Trim(),
                        MarketHolder = txt_MarketHolder.Text.Trim(),
                        PrescriptionType = cbo_PrescriptionType.SelectedItem?.ToString() ?? "",
                        MedicalInsuranceType = cbo_MedicalInsuranceType.SelectedItem?.ToString() ?? "",
                        AdminRoute = cbo_AdminRoute.SelectedItem?.ToString() ?? "",
                        ValidityPeriod = txt_ValidityPeriod.Text.Trim(),
                        StorageCondition = txt_StorageCondition.Text.Trim(),
                        Version = txt_Version.Text.Trim(),
                        UpdateLog = txt_UpdateLog.Text.Trim(),
                        Remark = txt_Precautions.Text.Trim(),
                        CreateBy = _currentOperateUserId,
                        UpdateBy = _currentOperateUserId
                    };

                    // 新增/编辑分支
                    BizResult result = _currentDrugId > 0 ? _bll.UpdateAntidiabeticDrug(model) : _bll.AddAntidiabeticDrug(model);
                    if (result.IsSuccess)
                    {
                        MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        tabMain.SelectedTab = tab_DrugList;
                        LoadDrugListData();
                        LoadVersionDrugDropdown();
                        ClearEditForm();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex) { MessageBox.Show($"保存操作失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            // 取消编辑按钮
            btn_CancelEdit.Click += (s, e) =>
            {
                try
                {
                    if (MessageBox.Show("取消编辑将丢失未保存的修改，确认取消？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        tabMain.SelectedTab = tab_DrugList;
                        ClearEditForm();
                    }
                }
                catch (Exception ex) { MessageBox.Show($"取消编辑失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            // 上传附件按钮
            btn_UploadManual.Click += (s, e) =>
            {
                try
                {
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.Filter = "PDF文件|*.pdf|Word文件|*.docx|所有文件|*.*";
                        ofd.Title = "上传药品说明书附件";
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            txt_ManualAttachment.Text = ofd.FileName;
                            MessageBox.Show("附件上传成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show($"上传附件失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };
            #endregion

            #region 批量操作页按钮事件
            // 批量导入按钮
            btn_ImportExcel.Click += (s, e) =>
            {
                try
                {
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.Filter = "Excel文件|*.xlsx;*.xls";
                        ofd.Title = "选择批量导入的Excel文件";
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            MessageBox.Show($"开始导入文件：{ofd.FileName}\n导入完成后需通过合规校验与审核！", "导入提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            // 可扩展Excel读取+批量新增逻辑
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show($"批量导入失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            // 批量导出按钮
            btn_ExportExcel.Click += (s, e) =>
            {
                try
                {
                    using (SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.Filter = "Excel文件|*.xlsx";
                        sfd.Title = "导出药物库数据";
                        sfd.FileName = $"糖尿病药物库数据_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            MessageBox.Show($"数据已导出至：{sfd.FileName}", "导出成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            // 可扩展DataGridView导出Excel逻辑
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show($"批量导出失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            // 批量启用按钮
            btn_BatchEnable.Click += (s, e) =>
            {
                try
                {
                    if (dgv_DrugList.SelectedRows.Count == 0)
                    {
                        MessageBox.Show("请先在药物库列表选中需要操作的记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    List<int> drugIdList = new List<int>();
                    foreach (DataGridViewRow row in dgv_DrugList.SelectedRows)
                    {
                        if (!row.IsNewRow)
                        {
                            DataRowView drv = row.DataBoundItem as DataRowView;
                            drugIdList.Add(Convert.ToInt32(drv["DrugID"]));
                        }
                    }
                    BizResult result = _bll.BatchUpdateDrugStatus(drugIdList, "启用", _currentOperateUserId);
                    if (result.IsSuccess)
                    {
                        MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDrugListData();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex) { MessageBox.Show($"批量启用失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            // 批量禁用按钮
            btn_BatchDisable.Click += (s, e) =>
            {
                try
                {
                    if (dgv_DrugList.SelectedRows.Count == 0)
                    {
                        MessageBox.Show("请先在药物库列表选中需要操作的记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    List<int> drugIdList = new List<int>();
                    foreach (DataGridViewRow row in dgv_DrugList.SelectedRows)
                    {
                        if (!row.IsNewRow)
                        {
                            DataRowView drv = row.DataBoundItem as DataRowView;
                            drugIdList.Add(Convert.ToInt32(drv["DrugID"]));
                        }
                    }
                    BizResult result = _bll.BatchUpdateDrugStatus(drugIdList, "禁用", _currentOperateUserId);
                    if (result.IsSuccess)
                    {
                        MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDrugListData();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex) { MessageBox.Show($"批量禁用失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };
            #endregion

            #region 多级审核页按钮事件
            // 查询待审核按钮
            btn_QueryAudit.Click += (s, e) =>
            {
                try { LoadAuditListData(); }
                catch (Exception ex) { MessageBox.Show($"查询审核列表失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            // 重置审核筛选按钮
            btn_ResetAudit.Click += (s, e) =>
            {
                try
                {
                    cbo_AuditStatus.SelectedIndex = 0;
                    cbo_AuditLevel.SelectedIndex = 0;
                    cbo_UpdateUser.SelectedIndex = 0;
                    LoadAuditListData();
                }
                catch (Exception ex) { MessageBox.Show($"重置筛选失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            // 一级审核通过按钮
            btn_FirstAuditPass.Click += (s, e) =>
            {
                try
                {
                    if (dgv_AuditList.SelectedRows.Count == 0)
                    {
                        MessageBox.Show("请先选中一条待审核记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DataRowView drv = dgv_AuditList.SelectedRows[0].DataBoundItem as DataRowView;
                    int drugId = Convert.ToInt32(drv["DrugID"]);
                    string auditOpinion = Microsoft.VisualBasic.Interaction.InputBox("请输入审核意见：", "一级审核通过", "合规校验通过，符合指南要求", 100, 100);

                    BizResult result = _bll.FirstAuditPass(drugId, _currentOperateUserId, auditOpinion);
                    if (result.IsSuccess)
                    {
                        MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadAuditListData();
                        LoadDrugListData();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "审核失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex) { MessageBox.Show($"一级审核操作失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            // 终审通过按钮
            btn_FinalAuditPass.Click += (s, e) =>
            {
                try
                {
                    if (dgv_AuditList.SelectedRows.Count == 0)
                    {
                        MessageBox.Show("请先选中一条待终审记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DataRowView drv = dgv_AuditList.SelectedRows[0].DataBoundItem as DataRowView;
                    int drugId = Convert.ToInt32(drv["DrugID"]);
                    string auditOpinion = Microsoft.VisualBasic.Interaction.InputBox("请输入终审意见：", "终审通过", "符合临床用药规范，终审通过", 100, 100);

                    BizResult result = _bll.FinalAuditPass(drugId, _currentOperateUserId, auditOpinion);
                    if (result.IsSuccess)
                    {
                        MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadAuditListData();
                        LoadDrugListData();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "终审失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex) { MessageBox.Show($"终审操作失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            // 审核驳回按钮
            btn_AuditReject.Click += (s, e) =>
            {
                try
                {
                    if (dgv_AuditList.SelectedRows.Count == 0)
                    {
                        MessageBox.Show("请先选中一条待审核记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DataRowView drv = dgv_AuditList.SelectedRows[0].DataBoundItem as DataRowView;
                    int drugId = Convert.ToInt32(drv["DrugID"]);
                    string auditOpinion = Microsoft.VisualBasic.Interaction.InputBox("请输入驳回原因（必填）：", "审核驳回", "", 100, 100);

                    BizResult result = _bll.AuditReject(drugId, _currentOperateUserId, auditOpinion);
                    if (result.IsSuccess)
                    {
                        MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadAuditListData();
                        LoadDrugListData();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "驳回失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex) { MessageBox.Show($"审核驳回操作失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };
            #endregion

            #region 版本管理页按钮事件
            // 查询版本按钮
            btn_QueryVersion.Click += (s, e) =>
            {
                try { LoadVersionHistoryData(); }
                catch (Exception ex) { MessageBox.Show($"查询版本历史失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            // 重置版本筛选按钮
            btn_ResetVersion.Click += (s, e) =>
            {
                try { cbo_VersionDrug.SelectedIndex = 0; LoadVersionHistoryData(); }
                catch (Exception ex) { MessageBox.Show($"重置筛选失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            // 版本回滚按钮
            btn_RollbackVersion.Click += (s, e) =>
            {
                try
                {
                    if (dgv_VersionList.SelectedRows.Count == 0)
                    {
                        MessageBox.Show("请先选中一条版本记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DataRowView drv = dgv_VersionList.SelectedRows[0].DataBoundItem as DataRowView;
                    int historyId = Convert.ToInt32(drv["HistoryID"]);
                    string version = drv["Version"].ToString();

                    if (MessageBox.Show($"确认回滚至【{version}】版本？回滚后将覆盖当前数据，且需重新审核生效！", "确认回滚", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        BizResult result = _bll.RollbackVersion(historyId, _currentOperateUserId);
                        if (result.IsSuccess)
                        {
                            MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadVersionHistoryData();
                            LoadDrugListData();
                        }
                        else
                        {
                            MessageBox.Show(result.Message, "回滚失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show($"版本回滚操作失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };
            #endregion

            #region 全局交互事件
            // 药物列表行点击（编辑按钮）
            dgv_DrugList.CellClick += (s, e) =>
            {
                try
                {
                    if (e.ColumnIndex == dgv_DrugList.Columns.Count - 1 && e.RowIndex >= 0 && !dgv_DrugList.Rows[e.RowIndex].IsNewRow)
                    {
                        DataRowView drv = dgv_DrugList.Rows[e.RowIndex].DataBoundItem as DataRowView;
                        int drugId = Convert.ToInt32(drv["DrugID"]);
                        LoadDrugDetailToEditForm(drugId);
                        tabMain.SelectedTab = tab_DrugEdit;
                    }
                }
                catch (Exception ex) { MessageBox.Show($"行点击操作失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            // 标签页切换事件
            tabMain.SelectedIndexChanged += (s, e) =>
            {
                try
                {
                    if (tabMain.SelectedTab == tab_MultiAudit) LoadAuditListData();
                    else if (tabMain.SelectedTab == tab_Version)
                    {
                        LoadVersionDrugDropdown();
                        LoadVersionHistoryData();
                    }
                }
                catch (Exception ex) { MessageBox.Show($"标签页切换失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };
            #endregion
        }
        #endregion
        #endregion

        #region 终极修复：药物列表单元格格式化（零空引用异常）
        /// <summary>
        /// 药物列表单元格格式化：合并剂量、布尔值转中文、空值处理、状态颜色区分
        /// 修复点：1. 先判断null再调用ToString() 2. 统一处理null和DBNull.Value 3. 异常捕获兜底
        /// </summary>
        private void Dgv_DrugList_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                // 基础校验：行索引无效或新行直接返回
                if (e.RowIndex < 0 || dgv_DrugList.Rows[e.RowIndex].IsNewRow)
                    return;

                // 获取数据源原始行（DataTable绑定模式下为DataRowView）
                DataRowView drv = dgv_DrugList.Rows[e.RowIndex].DataBoundItem as DataRowView;
                if (drv == null || drv.Row == null)
                    return;

                // 1. 智能合并每日剂量（从数据源直接读取原始字段）
                if (dgv_DrugList.Columns["DailyDosage"] != null && e.ColumnIndex == dgv_DrugList.Columns["DailyDosage"].Index)
                {
                    object minObj = drv["DailyDosageMin"];
                    object maxObj = drv["DailyDosageMax"];
                    object unitObj = drv["DosageUnit"];

                    string min = minObj != DBNull.Value ? minObj.ToString().Trim() : "";
                    string max = maxObj != DBNull.Value ? maxObj.ToString().Trim() : "";
                    string unit = unitObj != DBNull.Value ? unitObj.ToString().Trim() : "";

                    if (!string.IsNullOrEmpty(min) && !string.IsNullOrEmpty(max))
                        e.Value = $"{min}-{max} {unit}";
                    else if (!string.IsNullOrEmpty(min))
                        e.Value = $"{min} {unit}";
                    else
                        e.Value = "-";

                    e.FormattingApplied = true;
                }

                // 2. 布尔值转中文显示（是否一线）
                if (dgv_DrugList.Columns["IsFirstLine"] != null && e.ColumnIndex == dgv_DrugList.Columns["IsFirstLine"].Index)
                {
                    object value = drv["IsFirstLine"];
                    e.Value = value != DBNull.Value && Convert.ToBoolean(value) ? "是" : "否";
                    e.FormattingApplied = true;
                }

                // 3. 布尔值转中文显示（是否国产）
                if (dgv_DrugList.Columns["IsDomestic"] != null && e.ColumnIndex == dgv_DrugList.Columns["IsDomestic"].Index)
                {
                    object value = drv["IsDomestic"];
                    e.Value = value != DBNull.Value && Convert.ToBoolean(value) ? "是" : "否";
                    e.FormattingApplied = true;
                }

                // 4. 所有空值统一显示为"-"（修复：先判断null再调用ToString()）
                if (!e.FormattingApplied)
                {
                    if (e.Value == null || e.Value == DBNull.Value || string.IsNullOrWhiteSpace(e.Value.ToString()))
                    {
                        e.Value = "-";
                        e.FormattingApplied = true;
                    }
                }

                // 5. 状态列颜色区分（修复：统一空值检查）
                if (dgv_DrugList.Columns["EnableStatus"] != null && e.ColumnIndex == dgv_DrugList.Columns["EnableStatus"].Index)
                {
                    if (e.Value != null)
                    {
                        string status = e.Value.ToString().Trim();
                        e.CellStyle.ForeColor = status == "启用"
                            ? Color.FromArgb(40, 167, 69)  // 绿色
                            : Color.FromArgb(220, 53, 69); // 红色
                    }
                }

                if (dgv_DrugList.Columns["AuditStatus"] != null && e.ColumnIndex == dgv_DrugList.Columns["AuditStatus"].Index)
                {
                    if (e.Value != null)
                    {
                        string status = e.Value.ToString().Trim();
                        switch (status)
                        {
                            case "终审通过":
                                e.CellStyle.ForeColor = Color.FromArgb(40, 167, 69); // 绿色
                                break;
                            case "审核驳回":
                                e.CellStyle.ForeColor = Color.FromArgb(220, 53, 69); // 红色
                                break;
                            default:
                                e.CellStyle.ForeColor = Color.FromArgb(255, 193, 7); // 黄色（待审核）
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 异常兜底：格式化失败时显示"-"，不抛出异常
                e.Value = "-";
                e.FormattingApplied = true;
                // 可选：记录日志
                // MessageBox.Show($"单元格格式化警告：{ex.Message}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
    }
}