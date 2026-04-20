using BLL;
using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace AdminUI
{
    public partial class FrmDictionaryManager : Form
    {
        #region 全局统一布局参数
        private readonly Padding _globalMainContainerPadding = new Padding(15, 15, 15, 15);
        private readonly bool _globalContentAutoCenter = false;
        private readonly int _globalContentOffsetX = 20;
        private readonly int _globalContentOffsetY = 20;
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

        #region 按钮事件
        private void btn_TypeAdd_Click(object sender, EventArgs e) { }
        private void btn_TypeEdit_Click(object sender, EventArgs e) { }
        private void btn_TypeDelete_Click(object sender, EventArgs e) { }
        private void btn_ItemAdd_Click(object sender, EventArgs e) { }
        private void btn_ItemEdit_Click(object sender, EventArgs e) { }
        private void btn_ItemDelete_Click(object sender, EventArgs e) { }
        private void btn_BatchEnable_Click(object sender, EventArgs e) { }
        private void btn_BatchDisable_Click(object sender, EventArgs e) { }
        private void btn_ImportExcel_Click(object sender, EventArgs e) { }
        private void btn_ExportExcel_Click(object sender, EventArgs e) { }
        #endregion

        #region 全局业务变量
        private readonly B_DictLibrary _bll = new B_DictLibrary();
        #endregion

        #region 核心控件声明
        private Panel pnlMainContainer;
        private Panel pnlContentWrapper;
        private TabControl tabMain;
        private TabPage tab_DictType, tab_DictItem, tab_BatchOperate, tab_ReferenceQuery;
        private GroupBox grp_TypeFilter, grp_TypeList;
        // 已废弃：private ComboBox cbo_TypeEnableStatus;
        private TextBox txt_TypeSearchKey;
        private DateTimePicker dtp_TypeCreateStart, dtp_TypeCreateEnd;
        private DataGridView dgv_TypeList;
        private Button btn_TypeSearch, btn_TypeReset, btn_TypeAdd, btn_TypeEdit, btn_TypeDelete;

        private GroupBox grp_ItemFilter, grp_ItemList;
        private ComboBox cbo_ItemDictType, cbo_ItemEnableStatus;
        private TextBox txt_ItemSearchKey;
        private DataGridView dgv_ItemList;
        private Button btn_ItemSearch, btn_ItemReset, btn_ItemAdd, btn_ItemEdit, btn_ItemDelete;

        private GroupBox grp_BatchImport, grp_BatchStatus;
        private Button btn_ImportExcel, btn_ExportExcel, btn_BatchEnable, btn_BatchDisable;

        private GroupBox grp_QueryFilter, grp_QueryList;
        private ComboBox cbo_QueryDictType, cbo_QueryDictCode;
        private DataGridView dgv_ReferenceList;
        private Button btn_QuerySearch, btn_QueryReset;
        #endregion

        public FrmDictionaryManager()
        {
            InitializeComponent();
            this.Text = "字典库管理（糖尿病健康管理系统）";
            this.Size = new Size(1500, 900);
            this.MinimumSize = new Size(_globalContentMinWidth + 40, _globalContentMinHeight + 100);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("微软雅黑", 9.5F);
            this.Dock = DockStyle.Fill;

            // ✅ 修复执行顺序：先容器 → 再动态创建控件 → 再初始化数据 → 绑定事件
            InitMainContainer();
            InitializeDynamicControls();
            InitControlData();
            BindAllEvents();
            this.Load += FrmDictLibraryManager_Load;
            this.FormClosed += (s, e) => this.Dispose();
        }

        #region 主容器初始化
        private void InitMainContainer()
        {
            pnlMainContainer = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = _globalMainContainerPadding, AutoScroll = true };
            this.Controls.Add(pnlMainContainer);
            pnlContentWrapper = new Panel { MinimumSize = new Size(_globalContentMinWidth, _globalContentMinHeight), BackColor = Color.White };
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
                    pnlContentWrapper.Left = _globalContentOffsetX;
                    pnlContentWrapper.Top = _globalContentOffsetY;
                }
            };
            pnlMainContainer.Resize += (s, e) => updateLocation();
            this.Resize += (s, e) => updateLocation();
            this.Load += (s, e) => updateLocation();
        }

        private void FrmDictLibraryManager_Load(object sender, EventArgs e)
        {
            try
            {
                LoadDictTypeListData();
                LoadDictItemListData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"页面加载失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 数据加载
        private void LoadDictTypeListData()
        {
            try
            {
                string searchKey = txt_TypeSearchKey.Text.Trim();
                BizResult result = _bll.GetDictTypeList(searchKey);
                if (result.IsSuccess)
                {
                    dgv_TypeList.DataSource = result.Data;
                    dgv_TypeList.ClearSelection();
                }
                else
                {
                    MessageBox.Show(result.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dgv_TypeList.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载分类列表失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDictItemListData()
        {
            try
            {
                string dictType = cbo_ItemDictType.SelectedItem?.ToString() ?? "全部";
                string enableStatus = cbo_ItemEnableStatus.SelectedItem?.ToString() ?? "全部";
                string searchKey = txt_ItemSearchKey.Text.Trim();
                BizResult result = _bll.GetDictList(dictType, enableStatus, searchKey);
                if (result.IsSuccess)
                {
                    dgv_ItemList.DataSource = result.Data;
                    dgv_ItemList.ClearSelection();
                }
                else
                {
                    MessageBox.Show(result.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dgv_ItemList.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载字典项失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDictTypeDropdown()
        {
            try
            {
                cbo_ItemDictType.Items.Clear();
                cbo_QueryDictType.Items.Clear();
                cbo_ItemDictType.Items.Add("全部");
                cbo_QueryDictType.Items.Add("全部");
                BizResult result = _bll.GetDictTypeList("");
                if (result.IsSuccess && result.Data is DataTable dt)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        cbo_ItemDictType.Items.Add(row["type_code"].ToString());
                        cbo_QueryDictType.Items.Add(row["type_code"].ToString());
                    }
                }
                cbo_ItemDictType.SelectedIndex = 0;
                cbo_QueryDictType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载分类下拉框失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadReferenceListData()
        {
            try
            {
                string dictType = cbo_QueryDictType.SelectedItem?.ToString() ?? "";
                string dictCode = cbo_QueryDictCode.SelectedItem?.ToString() ?? "";
                BizResult result = _bll.GetDictReferenceRelation(dictType, dictCode);
                if (result.IsSuccess)
                {
                    dgv_ReferenceList.DataSource = result.Data;
                    dgv_ReferenceList.ClearSelection();
                }
                else
                {
                    MessageBox.Show(result.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dgv_ReferenceList.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载引用关系失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 控件初始化
        private void InitializeDynamicControls()
        {
            tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 10F),
                Padding = new Point(15, 8)
            };
            tab_DictType = new TabPage("字典分类管理") { BackColor = Color.White };
            tab_DictItem = new TabPage("字典项管理") { BackColor = Color.White };
            tab_BatchOperate = new TabPage("批量操作") { BackColor = Color.White };
            tab_ReferenceQuery = new TabPage("引用关系查询") { BackColor = Color.White };
            tabMain.TabPages.AddRange(new[] { tab_DictType, tab_DictItem, tab_BatchOperate, tab_ReferenceQuery });
            pnlContentWrapper.Controls.Add(tabMain);

            InitDictTypePage();
            InitDictItemPage();
            InitBatchOperatePage();
            InitReferenceQueryPage();
        }

        private void InitDictTypePage()
        {
            grp_TypeFilter = new GroupBox
            {
                Text = "分类筛选",
                Dock = DockStyle.Top,
                Height = 180,
                Padding = _globalGroupBoxPadding
            };
            tab_DictType.Controls.Add(grp_TypeFilter);
            TableLayoutPanel tlp_Filter = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 3
            };
            for (int i = 0; i < 3; i++)
                tlp_Filter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
            for (int i = 0; i < 3; i++)
                tlp_Filter.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_TypeFilter.Controls.Add(tlp_Filter);

            int row = 0;
            CreateEditItem<TextBox>(tlp_Filter, out _, out txt_TypeSearchKey, "关键词检索：", ref row);

            Label lblTime = new Label
            {
                Text = "创建时间：",
                AutoSize = true,
                Location = new Point(0, 5),
                Font = this.Font
            };
            dtp_TypeCreateStart = new DateTimePicker
            {
                Width = 120,
                Location = new Point(lblTime.Width + 10, 2),
                Font = this.Font
            };
            Label lblSplit = new Label
            {
                Text = "至",
                AutoSize = true,
                Location = new Point(lblTime.Width + 10 + dtp_TypeCreateStart.Width + 5, 5),
                Font = this.Font
            };
            dtp_TypeCreateEnd = new DateTimePicker
            {
                Width = 120,
                Location = new Point(lblTime.Width + 5 + dtp_TypeCreateStart.Width + 5 + lblSplit.Width + 5, 2),
                Font = this.Font
            };
            Panel pnlTime = new Panel { Dock = DockStyle.Fill };
            pnlTime.Controls.Add(lblTime);
            pnlTime.Controls.Add(dtp_TypeCreateStart);
            pnlTime.Controls.Add(lblSplit);
            pnlTime.Controls.Add(dtp_TypeCreateEnd);
            tlp_Filter.Controls.Add(pnlTime, 1, 0);

            btn_TypeSearch = CreateBtn("检索", Color.FromArgb(0, 122, 204));
            btn_TypeReset = CreateBtn("重置", Color.Gray);
            btn_TypeAdd = CreateBtn("新增分类", Color.FromArgb(0, 150, 136));
            btn_TypeEdit = CreateBtn("编辑选中", Color.Orange);
            btn_TypeDelete = CreateBtn("删除选中", Color.Red);
            FlowLayoutPanel flpBtn = new FlowLayoutPanel { Dock = DockStyle.Fill };
            flpBtn.Controls.Add(btn_TypeSearch);
            flpBtn.Controls.Add(btn_TypeReset);
            flpBtn.Controls.Add(btn_TypeAdd);
            flpBtn.Controls.Add(btn_TypeEdit);
            flpBtn.Controls.Add(btn_TypeDelete);
            tlp_Filter.Controls.Add(flpBtn, 0, 2);
            tlp_Filter.SetColumnSpan(flpBtn, 3);

            // 修改后示例（核心：把顶部内边距从15改成30，按需调整数值）
            grp_TypeList = new GroupBox
            {
                Text = "分类列表",
                Dock = DockStyle.Fill,
                Padding = new Padding(15, 160, 15, 15) // 左、上、右、下，只加大顶部内边距
            };
            tab_DictType.Controls.Add(grp_TypeList);

            dgv_TypeList = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                GridColor = Color.LightGray,
                RowHeadersVisible = true,
                EnableHeadersVisualStyles = false
            };
            dgv_TypeList.RowHeadersDefaultCellStyle.BackColor = Color.White;
            dgv_TypeList.RowHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
            dgv_TypeList.DefaultCellStyle.BackColor = Color.White;
            dgv_TypeList.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
            dgv_TypeList.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv_TypeList.AlternatingRowsDefaultCellStyle.BackColor = Color.White;

            dgv_TypeList.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { HeaderText = "分类ID", DataPropertyName = "type_id", Width = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "分类编码", DataPropertyName = "type_code", Width = 200 },
                new DataGridViewTextBoxColumn { HeaderText = "分类名称", DataPropertyName = "type_name", Width = 250 },
                new DataGridViewTextBoxColumn { HeaderText = "描述", DataPropertyName = "description", Width = 500 }
            });
            grp_TypeList.Controls.Add(dgv_TypeList);
        }

        private void InitDictItemPage()
        {
            grp_ItemFilter = new GroupBox
            {
                Text = "字典项筛选",
                Dock = DockStyle.Top,
                Height = 180,
                Padding = _globalGroupBoxPadding
            };
            tab_DictItem.Controls.Add(grp_ItemFilter);
            TableLayoutPanel tlp_Filter = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 3
            };
            for (int i = 0; i < 3; i++)
                tlp_Filter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
            for (int i = 0; i < 3; i++)
                tlp_Filter.RowStyles.Add(new RowStyle(SizeType.Absolute, _globalRowHeight));
            grp_ItemFilter.Controls.Add(tlp_Filter);

            int row = 0;
            CreateEditItem<TextBox>(tlp_Filter, out _, out txt_ItemSearchKey, "关键词检索：", ref row);
            CreateEditItem<ComboBox>(tlp_Filter, out _, out cbo_ItemDictType, "所属分类：", ref row);
            CreateEditItem<ComboBox>(tlp_Filter, out _, out cbo_ItemEnableStatus, "启用状态：", ref row);

            btn_ItemSearch = CreateBtn("检索", Color.FromArgb(0, 122, 204));
            btn_ItemReset = CreateBtn("重置", Color.Gray);
            btn_ItemAdd = CreateBtn("新增字典项", Color.FromArgb(0, 150, 136));
            btn_ItemEdit = CreateBtn("编辑选中", Color.Orange);
            btn_ItemDelete = CreateBtn("删除选中", Color.Red);
            FlowLayoutPanel flpBtn = new FlowLayoutPanel { Dock = DockStyle.Fill };
            flpBtn.Controls.Add(btn_ItemSearch);
            flpBtn.Controls.Add(btn_ItemReset);
            flpBtn.Controls.Add(btn_ItemAdd);
            flpBtn.Controls.Add(btn_ItemEdit);
            flpBtn.Controls.Add(btn_ItemDelete);
            tlp_Filter.Controls.Add(flpBtn, 0, 2);
            tlp_Filter.SetColumnSpan(flpBtn, 3);

            grp_ItemList = new GroupBox
            {
                Text = "字典项列表",
                Dock = DockStyle.Fill,
                Padding = new Padding(15, 165, 15, 15) // 加大顶部内边距
            };
            tab_DictItem.Controls.Add(grp_ItemList);

            dgv_ItemList = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false
            };
            dgv_ItemList.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { HeaderText = "字典ID", DataPropertyName = "dict_id", Width = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "所属分类", DataPropertyName = "dict_type_name", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "字典编码", DataPropertyName = "dict_code", Width = 150 },
                new DataGridViewTextBoxColumn { HeaderText = "字典名称", DataPropertyName = "dict_name", Width = 200 },
                new DataGridViewTextBoxColumn { HeaderText = "状态", DataPropertyName = "status", Width = 80 }
            });
            grp_ItemList.Controls.Add(dgv_ItemList);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "字典库管理";
            this.ResumeLayout(false);
        }

        private void InitBatchOperatePage()
        {
            grp_BatchImport = new GroupBox
            {
                Text = "导入/导出",
                Dock = DockStyle.Top,
                Height = 150,
                Padding = _globalGroupBoxPadding
            };
            tab_BatchOperate.Controls.Add(grp_BatchImport);
            btn_ImportExcel = CreateBtn("批量导入", Color.FromArgb(0, 150, 136));
            btn_ExportExcel = CreateBtn("批量导出", Color.Orange);
            grp_BatchImport.Controls.Add(btn_ImportExcel);
            grp_BatchImport.Controls.Add(btn_ExportExcel);

            grp_BatchStatus = new GroupBox
            {
                Text = "批量状态操作",
                Dock = DockStyle.Top,
                Height = 150,
                Padding = _globalGroupBoxPadding
            };
            tab_BatchOperate.Controls.Add(grp_BatchStatus);
            btn_BatchEnable = CreateBtn("批量启用", Color.Green);
            btn_BatchDisable = CreateBtn("批量禁用", Color.Red);
            grp_BatchStatus.Controls.Add(btn_BatchEnable);
            grp_BatchStatus.Controls.Add(btn_BatchDisable);
        }

        private void InitReferenceQueryPage()
        {
            grp_QueryFilter = new GroupBox
            {
                Text = "引用查询条件",
                Dock = DockStyle.Top,
                Height = 150,
                Padding = _globalGroupBoxPadding
            };
            tab_ReferenceQuery.Controls.Add(grp_QueryFilter);
            cbo_QueryDictType = new ComboBox { Width = 200, Location = new Point(20, 50) };
            cbo_QueryDictCode = new ComboBox { Width = 200, Location = new Point(240, 50) };
            btn_QuerySearch = CreateBtn("查询引用", Color.FromArgb(0, 122, 204));
            btn_QueryReset = CreateBtn("重置条件", Color.Gray);
            btn_QuerySearch.Location = new Point(460, 50);
            btn_QueryReset.Location = new Point(580, 50);
            grp_QueryFilter.Controls.Add(cbo_QueryDictType);
            grp_QueryFilter.Controls.Add(cbo_QueryDictCode);
            grp_QueryFilter.Controls.Add(btn_QuerySearch);
            grp_QueryFilter.Controls.Add(btn_QueryReset);

            grp_QueryList = new GroupBox { Text = "引用关系列表", Dock = DockStyle.Fill, Padding = _globalGroupBoxPadding };
            tab_ReferenceQuery.Controls.Add(grp_QueryList);

            dgv_ReferenceList = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false
            };
            dgv_ReferenceList.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { HeaderText = "引用表名", DataPropertyName = "reference_table", Width = 180 },
                new DataGridViewTextBoxColumn { HeaderText = "引用字段", DataPropertyName = "reference_field", Width = 150 },
                new DataGridViewTextBoxColumn { HeaderText = "引用内容", DataPropertyName = "reference_name", Width = 200 }
            });
            grp_QueryList.Controls.Add(dgv_ReferenceList);
        }

        private Button CreateBtn(string text, Color backColor)
        {
            return new Button
            {
                Text = text,
                BackColor = backColor,
                ForeColor = Color.White,
                Size = new Size(_globalButtonWidth, _globalButtonHeight),
                FlatStyle = FlatStyle.Flat,
                Margin = _globalControlMargin
            };
        }

        private void CreateEditItem<T>(TableLayoutPanel tlp, out Label lbl, out T ctrl, string text, ref int row, bool readOnly = false) where T : Control, new()
        {
            lbl = new Label
            {
                Text = text,
                Size = new Size(_globalLabelWidth, _globalControlHeight),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = _globalControlMargin
            };
            ctrl = new T
            {
                Size = new Size(260, _globalControlHeight),
                Margin = _globalControlMargin
            };
            if (ctrl is ComboBox cbo)
                cbo.DropDownStyle = ComboBoxStyle.DropDownList;
            Panel panel = new Panel { Dock = DockStyle.Fill };
            lbl.Location = new Point(0, 0);
            ctrl.Location = new Point(lbl.Width, 0);
            panel.Controls.Add(lbl);
            panel.Controls.Add(ctrl);
            tlp.Controls.Add(panel, row % 2, row / 2);
            row++;
        }
        #endregion

        #region 初始化数据 & 事件绑定
        // ✅ 修复：移除废弃控件访问，只初始化存在的控件
        private void InitControlData()
        {
            try
            {
                string[] statusOptions = new[] { "全部", "启用", "禁用" };
                // 只初始化存在的 cbo_ItemEnableStatus
                cbo_ItemEnableStatus.Items.Clear();
                cbo_ItemEnableStatus.Items.AddRange(statusOptions);
                cbo_ItemEnableStatus.SelectedIndex = 0;

                LoadDictTypeDropdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"字典库控件初始化失败：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ✅ 修复：移除对废弃控件的调用
        private void BindAllEvents()
        {
            // 字典分类按钮
            btn_TypeSearch.Click += (s, e) => LoadDictTypeListData();
            btn_TypeReset.Click += (s, e) => { txt_TypeSearchKey.Clear(); LoadDictTypeListData(); };
            btn_TypeAdd.Click += btn_TypeAdd_Click;
            btn_TypeEdit.Click += btn_TypeEdit_Click;
            btn_TypeDelete.Click += btn_TypeDelete_Click;

            // 字典项按钮
            btn_ItemSearch.Click += (s, e) => LoadDictItemListData();
            btn_ItemReset.Click += (s, e) => { txt_ItemSearchKey.Clear(); cbo_ItemDictType.SelectedIndex = 0; cbo_ItemEnableStatus.SelectedIndex = 0; LoadDictItemListData(); };
            btn_ItemAdd.Click += btn_ItemAdd_Click;
            btn_ItemEdit.Click += btn_ItemEdit_Click;
            btn_ItemDelete.Click += btn_ItemDelete_Click;

            // 批量操作
            btn_BatchEnable.Click += btn_BatchEnable_Click;
            btn_BatchDisable.Click += btn_BatchDisable_Click;
            btn_ImportExcel.Click += btn_ImportExcel_Click;
            btn_ExportExcel.Click += btn_ExportExcel_Click;

            // 引用查询
            btn_QuerySearch.Click += (s, e) => LoadReferenceListData();
            btn_QueryReset.Click += (s, e) => { cbo_QueryDictType.SelectedIndex = 0; cbo_QueryDictCode.Items.Clear(); dgv_ReferenceList.DataSource = null; };

            // 下拉联动
            cbo_QueryDictType.SelectedIndexChanged += (s, e) =>
            {
                cbo_QueryDictCode.Items.Clear();
                if (cbo_QueryDictType.SelectedIndex > 0)
                {
                    string typeCode = cbo_QueryDictType.SelectedItem.ToString();
                    BizResult result = _bll.GetDictByTypeCode(typeCode);
                    if (result.IsSuccess && result.Data is DataTable dt)
                    {
                        foreach (DataRow row in dt.Rows)
                            cbo_QueryDictCode.Items.Add(row["dict_code"].ToString());
                    }
                }
            };
        }
        #endregion
    }
}