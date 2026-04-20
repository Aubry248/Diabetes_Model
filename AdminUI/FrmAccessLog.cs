using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BLL;
using Model;
// 修复NPOI引用
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace AdminUI
{
    public partial class FrmAccessLog : Form
    {
        // 业务层对象
        private readonly B_AccessLog bllAccess = new B_AccessLog();
        private readonly B_AuditLog bllAudit = new B_AuditLog();
        private readonly B_Role bllRole = new B_Role();

        // 分页参数
        private int _currentPageIndex = 1;
        private const int PageSize = 20;
        private int _totalCount = 0;

        // 日志类型枚举 0=登录日志 1=操作日志
        private int _logType = 0;

        // 控件定义
        private DateTimePicker dtpStart, dtpEnd;
        private ComboBox cboRole, cboStatus;
        private TextBox txtUserName;
        private TabControl tabLogType;
        private DataGridView dgvLogList;
        private Label lblPageInfo;
        private Button btnPrev, btnNext, btnGo;
        private NumericUpDown nudPageNum;

        public FrmAccessLog()
        {
            InitializeComponent();
            this.Controls.Clear();

            // 窗体基础配置（和现有系统完全兼容）
            this.Text = "日志查看";
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1200, 700);
            this.Font = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // 创建页面控件
            CreateAllControls();
            // 绑定事件
            this.Load += FrmAccessLog_Load;
        }

        #region 页面布局（稳定Anchor布局，无控件丢失）
        private void CreateAllControls()
        {
            this.SuspendLayout();
            int margin = 12;
            int formWidth = this.ClientSize.Width;
            int formHeight = this.ClientSize.Height;

            // ==============================================
            // 顶部查询区（GroupBox分组）
            // ==============================================
            GroupBox gbQuery = new GroupBox();
            gbQuery.Text = "查询条件";
            gbQuery.Location = new Point(margin, margin);
            gbQuery.Size = new Size(formWidth - 2 * margin, 100);
            gbQuery.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbQuery.Font = new Font("微软雅黑", 9F, FontStyle.Bold);
            gbQuery.BackColor = Color.White;

            // 1. 时间范围
            Label lblTime = new Label { Text = "时间范围：", Location = new Point(15, 25), Size = new Size(70, 25), Font = new Font("微软雅黑", 9F) };
            dtpStart = new DateTimePicker { Location = new Point(85, 22), Size = new Size(120, 25), Font = new Font("微软雅黑", 9F), Format = DateTimePickerFormat.Short };
            Label lblLine = new Label { Text = "至", Location = new Point(210, 25), Size = new Size(20, 25), Font = new Font("微软雅黑", 9F) };
            dtpEnd = new DateTimePicker { Location = new Point(235, 22), Size = new Size(120, 25), Font = new Font("微软雅黑", 9F), Format = DateTimePickerFormat.Short };

            // 2. 角色筛选
            Label lblRole = new Label { Text = "用户角色：", Location = new Point(370, 25), Size = new Size(70, 25), Font = new Font("微软雅黑", 9F) };
            cboRole = new ComboBox { Location = new Point(445, 22), Size = new Size(120, 25), Font = new Font("微软雅黑", 9F), DropDownStyle = ComboBoxStyle.DropDownList };

            // 3. 用户名筛选
            Label lblName = new Label { Text = "用户名：", Location = new Point(580, 25), Size = new Size(60, 25), Font = new Font("微软雅黑", 9F) };
            txtUserName = new TextBox { Location = new Point(645, 22), Size = new Size(120, 25), Font = new Font("微软雅黑", 9F) };

            // 4. 操作状态
            Label lblStatus = new Label { Text = "操作状态：", Location = new Point(780, 25), Size = new Size(70, 25), Font = new Font("微软雅黑", 9F) };
            cboStatus = new ComboBox { Location = new Point(855, 22), Size = new Size(100, 25), Font = new Font("微软雅黑", 9F), DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new object[] { "全部", "成功", "失败" });
            cboStatus.SelectedIndex = 0;

            // 第二行：日志类型选项卡 + 操作按钮
            tabLogType = new TabControl { Location = new Point(15, 55), Size = new Size(200, 35), Font = new Font("微软雅黑", 9F) };
            TabPage tabLogin = new TabPage("登录日志");
            TabPage tabOperate = new TabPage("操作日志");
            tabLogin.BackColor = Color.White;
            tabOperate.BackColor = Color.White;
            tabLogType.TabPages.Add(tabLogin);
            tabLogType.TabPages.Add(tabOperate);

            // 操作按钮
            Button btnQuery = new Button
            {
                Text = "查询",
                Location = new Point(230, 55),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 9F)
            };
            Button btnRefresh = new Button
            {
                Text = "刷新",
                Location = new Point(320, 55),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(40, 160, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 9F)
            };
            Button btnExport = new Button
            {
                Text = "导出Excel",
                Location = new Point(410, 55),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(23, 162, 184),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 9F)
            };
            Button btnReset = new Button
            {
                Text = "重置",
                Location = new Point(520, 55),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 9F)
            };

            // 加入查询分组
            gbQuery.Controls.AddRange(new Control[] { lblTime, dtpStart, lblLine, dtpEnd, lblRole, cboRole, lblName, txtUserName, lblStatus, cboStatus, tabLogType, btnQuery, btnRefresh, btnExport, btnReset });
            this.Controls.Add(gbQuery);

            // ==============================================
            // 中部日志列表DataGridView（只读、分页）
            // ==============================================
            dgvLogList = new DataGridView();
            dgvLogList.Location = new Point(margin, 120);
            dgvLogList.Size = new Size(formWidth - 2 * margin, formHeight - 200);
            dgvLogList.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;

            // 表格样式（只读、不可编辑、奇偶行异色）
            dgvLogList.BackgroundColor = Color.White;
            dgvLogList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            dgvLogList.GridColor = Color.LightGray;
            dgvLogList.AllowUserToAddRows = false;
            dgvLogList.AllowUserToDeleteRows = false;
            dgvLogList.ReadOnly = true;
            dgvLogList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLogList.MultiSelect = false;
            dgvLogList.RowHeadersVisible = false;
            dgvLogList.ColumnHeadersHeight = 35;
            dgvLogList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvLogList.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvLogList.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvLogList.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
            dgvLogList.ScrollBars = ScrollBars.Both;
            dgvLogList.AutoGenerateColumns = false;
            dgvLogList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // 奇偶行样式
            dgvLogList.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

            this.Controls.Add(dgvLogList);

            // ==============================================
            // 底部分页控件
            // ==============================================
            Panel pnlPage = new Panel();
            pnlPage.Location = new Point(margin, formHeight - 70);
            pnlPage.Size = new Size(formWidth - 2 * margin, 50);
            pnlPage.Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            pnlPage.BackColor = Color.White;

            lblPageInfo = new Label { Text = "总条数：0  当前页：1/0", Location = new Point(20, 15), AutoSize = true, Font = new Font("微软雅黑", 9F) };
            btnPrev = new Button { Text = "上一页", Location = new Point(300, 10), Size = new Size(80, 30), Enabled = false, Font = new Font("微软雅黑", 9F) };
            btnNext = new Button { Text = "下一页", Location = new Point(390, 10), Size = new Size(80, 30), Enabled = false, Font = new Font("微软雅黑", 9F) };
            Label lblGo = new Label { Text = "跳转到：", Location = new Point(490, 15), AutoSize = true, Font = new Font("微软雅黑", 9F) };
            nudPageNum = new NumericUpDown { Location = new Point(560, 12), Size = new Size(80, 25), Minimum = 1, Maximum = 1, Font = new Font("微软雅黑", 9F) };
            btnGo = new Button { Text = "跳转", Location = new Point(650, 10), Size = new Size(60, 30), Font = new Font("微软雅黑", 9F) };

            pnlPage.Controls.AddRange(new Control[] { lblPageInfo, btnPrev, btnNext, lblGo, nudPageNum, btnGo });
            this.Controls.Add(pnlPage);

            // 事件绑定
            tabLogType.SelectedIndexChanged += (s, e) =>
            {
                _logType = tabLogType.SelectedIndex;
                _currentPageIndex = 1;
                LoadLogList();
            };
            btnQuery.Click += (s, e) => { _currentPageIndex = 1; LoadLogList(); };
            btnRefresh.Click += (s, e) => LoadLogList();
            btnReset.Click += BtnReset_Click;
            btnExport.Click += BtnExport_Click;
            btnPrev.Click += (s, e) => { _currentPageIndex--; LoadLogList(); };
            btnNext.Click += (s, e) => { _currentPageIndex++; LoadLogList(); };
            btnGo.Click += (s, e) => { _currentPageIndex = (int)nudPageNum.Value; LoadLogList(); };
            dgvLogList.CellDoubleClick += DgvLogList_CellDoubleClick;

            this.ResumeLayout(true);
        }
        #endregion

        #region 页面加载&初始化
        private void FrmAccessLog_Load(object sender, EventArgs e)
        {
            // 初始化角色下拉框
            InitRoleComboBox();
            // 初始化时间范围（默认近7天）
            dtpStart.Value = DateTime.Now.AddDays(-7);
            dtpEnd.Value = DateTime.Now;
            // 加载日志列表
            LoadLogList();
            // 更新状态栏
            if (this.MdiParent is FrmAdminMain mainForm)
            {
                mainForm.SetStatusTip("操作提示：已打开【日志查看】");
            }
        }

        private void InitRoleComboBox()
        {
            var roleList = bllRole.GetAllRoleList();
            roleList.Insert(0, new Role { role_id = 0, role_name = "全部" });
            cboRole.DataSource = roleList;
            cboRole.DisplayMember = "role_name";
            cboRole.ValueMember = "role_id";
        }
        #endregion

        #region 核心方法：加载日志列表（双类型切换）
        private void LoadLogList()
        {
            this.Cursor = Cursors.WaitCursor;
            dgvLogList.Rows.Clear();
            dgvLogList.Columns.Clear();

            // 获取查询条件
            DateTime startTime = dtpStart.Value.Date;
            DateTime endTime = dtpEnd.Value.Date.AddDays(1).AddSeconds(-1); // 结束时间到当天23:59:59
            string userName = txtUserName.Text.Trim();
            int? roleId = cboRole.SelectedValue as int?;
            int? status = null;
            if (cboStatus.SelectedIndex == 1) status = 1;
            if (cboStatus.SelectedIndex == 2) status = 0;

            try
            {
                // 登录日志
                if (_logType == 0)
                {
                    // 初始化登录日志列
                    InitLoginLogColumns();
                    // 查询数据
                    var list = bllAudit.GetLoginLogByPage(startTime, endTime, userName, roleId, status, _currentPageIndex, PageSize, out _totalCount);
                    // 绑定数据
                    int index = 1;
                    foreach (var log in list)
                    {
                        dgvLogList.Rows.Add(
                            index++,
                            log.audit_id,
                            log.user_name,
                            log.role_name,
                            log.operate_time.ToString("yyyy-MM-dd HH:mm:ss"),
                            log.operate_type,
                            log.operate_content,
                            log.operate_ip,
                            log.operate_device
                        );
                    }
                }
                // 操作日志
                else
                {
                    // 初始化操作日志列
                    InitOperateLogColumns();
                    // 查询数据
                    var list = bllAccess.GetOperateLogByPage(startTime, endTime, userName, roleId, status, _currentPageIndex, PageSize, out _totalCount);
                    // 绑定数据
                    int index = 1;
                    foreach (var log in list)
                    {
                        dgvLogList.Rows.Add(
                            index++,
                            log.access_id,
                            log.user_name,
                            log.role_name,
                            log.access_time.ToString("yyyy-MM-dd HH:mm:ss"),
                            log.interface_module,
                            log.action,
                            log.access_status == 1 ? "成功" : "失败",
                            log.ip_address,
                            log.is_sensitive_operation ? "是" : "否"
                        );
                    }
                }

                // 更新分页控件
                UpdatePageControl();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载日志失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        // 初始化登录日志列
        private void InitLoginLogColumns()
        {
            dgvLogList.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn() { Name = "index", HeaderText = "序号", FillWeight = 8 },
                new DataGridViewTextBoxColumn() { Name = "audit_id", HeaderText = "日志ID", Visible = false },
                new DataGridViewTextBoxColumn() { Name = "user_name", HeaderText = "用户名", FillWeight = 12 },
                new DataGridViewTextBoxColumn() { Name = "role_name", HeaderText = "用户角色", FillWeight = 12 },
                new DataGridViewTextBoxColumn() { Name = "operate_time", HeaderText = "操作时间", FillWeight = 18 },
                new DataGridViewTextBoxColumn() { Name = "operate_type", HeaderText = "操作类型", FillWeight = 10 },
                new DataGridViewTextBoxColumn() { Name = "operate_content", HeaderText = "操作内容", FillWeight = 20 },
                new DataGridViewTextBoxColumn() { Name = "operate_ip", HeaderText = "登录IP", FillWeight = 12 },
                new DataGridViewTextBoxColumn() { Name = "operate_device", HeaderText = "设备信息", FillWeight = 8 }
            });
        }

        // 初始化操作日志列
        private void InitOperateLogColumns()
        {
            dgvLogList.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn() { Name = "index", HeaderText = "序号", FillWeight = 5 },
                new DataGridViewTextBoxColumn() { Name = "access_id", HeaderText = "日志ID", Visible = false },
                new DataGridViewTextBoxColumn() { Name = "user_name", HeaderText = "用户名", FillWeight = 10 },
                new DataGridViewTextBoxColumn() { Name = "role_name", HeaderText = "用户角色", FillWeight = 10 },
                new DataGridViewTextBoxColumn() { Name = "access_time", HeaderText = "操作时间", FillWeight = 15 },
                new DataGridViewTextBoxColumn() { Name = "interface_module", HeaderText = "操作模块", FillWeight = 15 },
                new DataGridViewTextBoxColumn() { Name = "action", HeaderText = "操作类型", FillWeight = 10 },
                new DataGridViewTextBoxColumn() { Name = "access_status", HeaderText = "操作结果", FillWeight = 8 },
                new DataGridViewTextBoxColumn() { Name = "ip_address", HeaderText = "操作IP", FillWeight = 12 },
                new DataGridViewTextBoxColumn() { Name = "is_sensitive", HeaderText = "敏感操作", FillWeight = 8 }
            });
        }

        // 更新分页控件状态
        private void UpdatePageControl()
        {
            int totalPage = (int)Math.Ceiling(_totalCount * 1.0 / PageSize);
            lblPageInfo.Text = $"总条数：{_totalCount}  当前页：{_currentPageIndex}/{totalPage}";
            nudPageNum.Maximum = totalPage > 0 ? totalPage : 1;
            nudPageNum.Value = _currentPageIndex;

            // 按钮启用状态
            btnPrev.Enabled = _currentPageIndex > 1;
            btnNext.Enabled = _currentPageIndex < totalPage;
            btnGo.Enabled = totalPage > 1;
        }
        #endregion

        #region 按钮事件
        // 重置查询条件
        private void BtnReset_Click(object sender, EventArgs e)
        {
            dtpStart.Value = DateTime.Now.AddDays(-7);
            dtpEnd.Value = DateTime.Now;
            cboRole.SelectedIndex = 0;
            txtUserName.Clear();
            cboStatus.SelectedIndex = 0;
            tabLogType.SelectedIndex = 0;
            _currentPageIndex = 1;
            LoadLogList();
        }

        // 双击查看详情
        private void DgvLogList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = dgvLogList.Rows[e.RowIndex];
            int logId = Convert.ToInt32(row.Cells[_logType == 0 ? "audit_id" : "access_id"].Value);

            // 打开详情弹窗
            FrmLogDetail frmDetail = new FrmLogDetail(_logType, logId);
            frmDetail.ShowDialog();
        }

        // 导出Excel
        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (dgvLogList.Rows.Count == 0)
            {
                MessageBox.Show("暂无数据可导出！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 保存文件对话框
            SaveFileDialog sfd = new SaveFileDialog();
            string logTypeName = _logType == 0 ? "登录日志" : "操作日志";
            sfd.FileName = $"{logTypeName}_{DateTime.Now:yyyyMMddHHmmss}.xls";
            sfd.Filter = "Excel文件(*.xls)|*.xls|所有文件(*.*)|*.*";
            sfd.Title = "保存日志文件";

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                // 创建Excel工作簿
                IWorkbook workbook = new HSSFWorkbook();
                ISheet sheet = workbook.CreateSheet(logTypeName);

                // 1. 创建表头
                IRow headerRow = sheet.CreateRow(0);
                for (int i = 0; i < dgvLogList.Columns.Count; i++)
                {
                    if (dgvLogList.Columns[i].Visible)
                    {
                        headerRow.CreateCell(i).SetCellValue(dgvLogList.Columns[i].HeaderText);
                    }
                }

                // 2. 写入数据
                for (int i = 0; i < dgvLogList.Rows.Count; i++)
                {
                    IRow dataRow = sheet.CreateRow(i + 1);
                    for (int j = 0; j < dgvLogList.Columns.Count; j++)
                    {
                        if (dgvLogList.Columns[j].Visible)
                        {
                            dataRow.CreateCell(j).SetCellValue(dgvLogList.Rows[i].Cells[j].Value?.ToString() ?? "");
                        }
                    }
                }

                // 3. 自动调整列宽
                for (int i = 0; i < dgvLogList.Columns.Count; i++)
                {
                    if (dgvLogList.Columns[i].Visible)
                    {
                        sheet.AutoSizeColumn(i);
                    }
                }

                // 4. 保存文件
                using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write))
                {
                    workbook.Write(fs);
                }

                MessageBox.Show($"导出成功！文件已保存至：{sfd.FileName}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        #endregion
    }
}