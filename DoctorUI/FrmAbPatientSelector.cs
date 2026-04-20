using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BLL;
using Model;

namespace DoctorUI
{
    public partial class FrmAbPatientSelector : Form
    {
        #region 原有全局参数100%完全保留
        private readonly Padding _globalControlMargin = new Padding(5, 5, 5, 5);
        private readonly int _globalControlHeight = 28;
        private readonly int _globalButtonHeight = 36;
        private readonly int _globalButtonWidth = 110;
        private readonly int _globalLabelWidth = 80;
        private readonly int _globalRowHeight = 40;
        private readonly Padding _globalGroupBoxPadding = new Padding(15);
        private readonly Color _themeColor = Color.FromArgb(0, 122, 204);
        private readonly B_Abnormal _bllAbnormal = new B_Abnormal();
        // 新增BLL实例
        private readonly B_User _bllUser = new B_User();
        #endregion

        #region 原有窗体配置参数100%完全保留
        public bool AllowSelectAll { get; set; } = false;
        public PatientSimpleInfo SelectedPatient { get; private set; }
        public bool IsSelectAll { get; private set; } = false;
        #endregion

        #region 原有分页参数100%完全保留
        private int _pageIndex = 1;
        private readonly int _pageSize = 20;
        private int _totalCount = 0;
        private int _totalPage => _totalCount == 0 ? 1 : (int)Math.Ceiling(_totalCount * 1.0 / _pageSize);
        #endregion

        #region 原有核心控件声明100%完全保留
        private GroupBox grp_Query;
        private TextBox txt_QueryName, txt_QueryPhone;
        private ComboBox cbo_QueryDiabetesType;
        private Button btn_Query, btn_Reset;
        private DataGridView dgv_PatientList;
        private Button btn_SelectAll, btn_Confirm, btn_Cancel;
        private Label lbl_PageInfo;
        private Button btn_PrevPage, btn_NextPage;
        #endregion

        #region 【新增配置】是否仅加载有异常数据的患者（默认true，匹配业务需求）
        public bool OnlyLoadPatientWithAbnormal { get; set; } = true;
        #endregion

        public FrmAbPatientSelector()
        {
            this.Text = "选择患者";
            this.Size = new Size(1000, 700);
            this.MinimumSize = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("微软雅黑", 9.5F);
            this.BackColor = Color.White;
            InitializeControls();
            BindDiabetesType();
            LoadPatientData();
            BindAllEvents();
        }

        #region 【修正+完善】控件初始化，新增评估状态列
        private void InitializeControls()
        {
            // 1. 查询条件区域（原有逻辑保留，仅微调列宽，保证控件完整）
            grp_Query = new GroupBox { Text = "筛选条件", Dock = DockStyle.Top, Height = 100, Padding = _globalGroupBoxPadding };
            this.Controls.Add(grp_Query);
            TableLayoutPanel tlp_Query = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            tlp_Query.RowCount = 1;
            tlp_Query.ColumnCount = 5;
            tlp_Query.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24F));
            tlp_Query.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24F));
            tlp_Query.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24F));
            tlp_Query.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14F));
            tlp_Query.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14F));
            tlp_Query.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_Query.Controls.Add(tlp_Query);
            int row = 0;
            // 姓名筛选
            CreateEditItem<TextBox>(tlp_Query, out _, out txt_QueryName, "患者姓名：", ref row);
            // 手机号筛选
            CreateEditItem<TextBox>(tlp_Query, out _, out txt_QueryPhone, "手机号：", ref row);
            // 糖尿病类型筛选
            CreateEditItem<ComboBox>(tlp_Query, out _, out cbo_QueryDiabetesType, "糖尿病类型：", ref row);
            // 查询按钮
            btn_Query = CreateBtn("查询", _themeColor);
            tlp_Query.Controls.Add(btn_Query, 3, 0);
            // 重置按钮
            btn_Reset = CreateBtn("重置", Color.Gray);
            tlp_Query.Controls.Add(btn_Reset, 4, 0);

            // 2. 患者列表表格（新增评估状态列，完整匹配实体）
            dgv_PatientList = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                RowHeadersVisible = false
            };
            // 表格列：严格对应PatientSimpleInfo实体类的属性
            dgv_PatientList.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn {
                    HeaderText = "患者ID",
                    DataPropertyName = "UserId",
                    Visible = false
                },
                new DataGridViewTextBoxColumn {
                    HeaderText = "患者姓名",
                    DataPropertyName = "UserName",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                },
                new DataGridViewTextBoxColumn {
                    HeaderText = "手机号",
                    DataPropertyName = "Phone",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                },
                new DataGridViewTextBoxColumn {
                    HeaderText = "糖尿病类型",
                    DataPropertyName = "DiabetesType",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                },
                new DataGridViewTextBoxColumn {
                    HeaderText = "待处理异常数",
                    DataPropertyName = "UnhandledAbnormalCount",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                },
                new DataGridViewTextBoxColumn {
                    HeaderText = "评估状态",
                    DataPropertyName = "AssessmentStatus",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                },
                new DataGridViewTextBoxColumn {
                    HeaderText = "最新评估时间",
                    DataPropertyName = "LastAssessmentTime",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd" }
                },
                new DataGridViewTextBoxColumn {
                    HeaderText = "显示文本",
                    DataPropertyName = "DisplayText",
                    Visible = false
                }
            });
            this.Controls.Add(dgv_PatientList);

            // 3. 底部按钮与分页区域（修正位置，解决按钮显示不全问题）
            Panel pnl_Footer = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(15) };
            this.Controls.Add(pnl_Footer);
            // 分页控件（修正位置，保证按钮完整呈现）
            Panel pnl_Page = new Panel { Dock = DockStyle.Left, Width = 400 };
            lbl_PageInfo = new Label { Text = "第 1 页 / 共 1 页  总计 0 条", AutoSize = true, Location = new Point(0, 10) };
            btn_PrevPage = new Button { Text = "上一页", Size = new Size(80, 30), Location = new Point(220, 5), Enabled = false };
            btn_NextPage = new Button { Text = "下一页", Size = new Size(80, 30), Location = new Point(310, 5), Enabled = false };
            pnl_Page.Controls.AddRange(new Control[] { lbl_PageInfo, btn_PrevPage, btn_NextPage });
            pnl_Footer.Controls.Add(pnl_Page);
            // 操作按钮（修正间距，保证完整显示）
            FlowLayoutPanel flp_Btn = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, Width = 400 };
            btn_SelectAll = CreateBtn("选择全部", Color.FromArgb(255, 152, 0));
            btn_Confirm = CreateBtn("确认选择", _themeColor);
            btn_Cancel = CreateBtn("取消", Color.Gray);
            flp_Btn.Controls.AddRange(new Control[] { btn_SelectAll, btn_Confirm, btn_Cancel });
            pnl_Footer.Controls.Add(flp_Btn);
            // 控制「选择全部」按钮显示
            btn_SelectAll.Visible = AllowSelectAll;
        }

        // 原有通用控件创建方法100%完全保留
        private void CreateEditItem<T>(TableLayoutPanel tlp, out Label lbl, out T ctrl, string text, ref int row) where T : Control, new()
        {
            lbl = new Label
            {
                Text = text,
                Size = new Size(_globalLabelWidth, _globalControlHeight),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = _globalControlMargin
            };
            ctrl = new T();
            ctrl.Size = new Size(120, _globalControlHeight);
            ctrl.Margin = _globalControlMargin;
            if (ctrl is ComboBox c) c.DropDownStyle = ComboBoxStyle.DropDownList;
            Panel pairPanel = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            lbl.Location = new Point(0, 0);
            ctrl.Location = new Point(lbl.Width, 0);
            pairPanel.Controls.AddRange(new Control[] { lbl, ctrl });
            tlp.Controls.Add(pairPanel, row, 0);
            row++;
        }

        // 原有按钮创建方法100%完全保留
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
        #endregion

        #region 原有方法完全保留，仅修改数据加载逻辑，匹配业务需求
        private void BindDiabetesType()
        {
            cbo_QueryDiabetesType.Items.Clear();
            cbo_QueryDiabetesType.Items.AddRange(new string[] { "全部", "1型", "2型", "妊娠", "其他" });
            cbo_QueryDiabetesType.SelectedIndex = 0;
        }

        // 完善数据加载方法，带异常处理
        private void LoadPatientData()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                (bool Success, string Msg, List<PatientSimpleInfo> Data, int TotalCount) result;

                // 调用BLL层：获取带评估状态的患者列表
                result = _bllUser.GetPatientWithAssessmentByPage(
                    GlobalConfig.CurrentDoctorID,
                    txt_QueryName.Text.Trim(),
                    txt_QueryPhone.Text.Trim(),
                    cbo_QueryDiabetesType.SelectedItem?.ToString(),
                    _pageIndex,
                    _pageSize
                );

                if (!result.Success)
                {
                    MessageBox.Show($"查询失败：{result.Msg}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    dgv_PatientList.DataSource = null;
                    _totalCount = 0;
                    return;
                }

                _totalCount = result.TotalCount;
                dgv_PatientList.DataSource = result.Data;
                // 更新分页信息
                lbl_PageInfo.Text = $"第 {_pageIndex} 页 / 共 {_totalPage} 页  总计 {_totalCount} 条";
                btn_PrevPage.Enabled = _pageIndex > 1;
                btn_NextPage.Enabled = _pageIndex < _totalPage;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载患者列表异常：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"【FrmPatientSelector加载异常】{ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        #endregion

        #region 原有事件绑定100%完全保留，无任何修改
        private void BindAllEvents()
        {
            btn_Query.Click += (s, e) =>
            {
                _pageIndex = 1;
                LoadPatientData();
            };
            btn_Reset.Click += (s, e) =>
            {
                txt_QueryName.Clear();
                txt_QueryPhone.Clear();
                cbo_QueryDiabetesType.SelectedIndex = 0;
                _pageIndex = 1;
                LoadPatientData();
            };
            btn_PrevPage.Click += (s, e) =>
            {
                _pageIndex--;
                LoadPatientData();
            };
            btn_NextPage.Click += (s, e) =>
            {
                _pageIndex++;
                LoadPatientData();
            };
            dgv_PatientList.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0) ConfirmSelectSingle();
            };
            btn_SelectAll.Click += (s, e) =>
            {
                IsSelectAll = true;
                SelectedPatient = null;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            btn_Confirm.Click += (s, e) => ConfirmSelectSingle();
            btn_Cancel.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
        }

        private void ConfirmSelectSingle()
        {
            if (dgv_PatientList.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选择一位患者", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (dgv_PatientList.SelectedRows[0].DataBoundItem is PatientSimpleInfo patient)
            {
                IsSelectAll = false;
                SelectedPatient = patient;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
        #endregion
    }
}