// 补充所有必需的using引用，彻底解决类型找不到问题
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Timers;
using System.Windows.Forms;
using BLL;
using Model;

namespace AdminUI
{
    public partial class FrmBackupRestore : Form
    {
        // 业务层对象（全局唯一实例）
        private readonly B_BackupRestore _bllBackup = new B_BackupRestore();

        // 分页参数
        private int _currentPageIndex = 1;
        private const int PageSize = 15;
        private int _totalCount = 0;

        #region 全局控件定义（彻底解决作用域问题，无局部重名）
        // Tab1 手动备份
        private TextBox txtDbName, txtLastBackupTime, txtBackupPath, txtBackupFileName;
        private ProgressBar pbBackupProgress;
        private Label lblBackupStatus;
        private Button btnStartBackup, btnSelectPath;

        // Tab2 备份记录
        private DateTimePicker dtpRecordStart, dtpRecordEnd;
        private ComboBox cboBackupType;
        private DataGridView dgvBackupList;
        private Label lblRecordPageInfo;
        private Button btnRecordPrev, btnRecordNext, btnRecordGo, btnQuery, btnRefresh, btnClearExpire;
        private NumericUpDown nudRecordPageNum;

        // Tab3 自动备份配置
        private CheckBox chkAutoBackupEnable;
        private ComboBox cboBackupCycle;
        private DateTimePicker dtpBackupTime;
        private TextBox txtAutoBackupPath, txtRetainDays;
        private Label lblAutoBackupStatus;
        private Button btnSelectAutoPath, btnSaveConfig, btnResetConfig;

        // 全局定时器（自动备份）
        private System.Timers.Timer _autoBackupTimer;
        #endregion

        #region 窗体初始化（彻底移除空的InitializeComponent，无设计器冲突）
        public FrmBackupRestore()
        {
            // 窗体基础配置
            this.Text = "数据备份与还原";
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1200, 700);
            this.Font = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

            // 动态创建所有控件
            CreateAllControls();
            // 绑定所有事件
            BindAllEvents();
        }
        #endregion

        #region 页面布局（三Tab页结构，稳定Anchor布局，无局部变量重名）
        private void CreateAllControls()
        {
            this.SuspendLayout();
            int margin = 12;
            int formWidth = this.ClientSize.Width;
            int formHeight = this.ClientSize.Height;

            // 主TabControl
            TabControl tabMain = new TabControl();
            tabMain.Name = "tabMain";
            tabMain.Location = new Point(margin, margin);
            tabMain.Size = new Size(formWidth - 2 * margin, formHeight - 2 * margin);
            tabMain.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            tabMain.Font = new Font("微软雅黑", 9F);
            tabMain.Appearance = TabAppearance.Normal;
            tabMain.ItemSize = new Size(120, 30);
            tabMain.SizeMode = TabSizeMode.Fixed;

            // ===================== Tab1：手动备份 =====================
            TabPage tabManualBackup = new TabPage("手动备份");
            tabManualBackup.Name = "tabManualBackup";
            tabManualBackup.BackColor = Color.White;
            tabManualBackup.Font = new Font("微软雅黑", 9F);
            CreateManualBackupTab(tabManualBackup);
            tabMain.TabPages.Add(tabManualBackup);

            // ===================== Tab2：备份记录与还原 =====================
            TabPage tabBackupRecord = new TabPage("备份记录与还原");
            tabBackupRecord.Name = "tabBackupRecord";
            tabBackupRecord.BackColor = Color.White;
            tabBackupRecord.Font = new Font("微软雅黑", 9F);
            CreateBackupRecordTab(tabBackupRecord);
            tabMain.TabPages.Add(tabBackupRecord);

            // ===================== Tab3：自动备份配置 =====================
            TabPage tabAutoBackup = new TabPage("自动备份配置");
            tabAutoBackup.Name = "tabAutoBackup";
            tabAutoBackup.BackColor = Color.White;
            tabAutoBackup.Font = new Font("微软雅黑", 9F);
            CreateAutoBackupTab(tabAutoBackup);
            tabMain.TabPages.Add(tabAutoBackup);

            this.Controls.Add(tabMain);
            this.ResumeLayout(true);

            // Tab切换事件
            tabMain.SelectedIndexChanged += (s, e) =>
            {
                if (tabMain.SelectedIndex == 1)
                {
                    LoadBackupRecordList();
                }
                else if (tabMain.SelectedIndex == 2)
                {
                    LoadAutoBackupConfig();
                }
            };
        }

        #region Tab1 手动备份布局
        private void CreateManualBackupTab(TabPage tab)
        {
            int margin = 20;
            int tabWidth = tab.ClientSize.Width;

            // 1. 核心信息区
            GroupBox gbBaseInfo = new GroupBox();
            gbBaseInfo.Name = "gbBaseInfo";
            gbBaseInfo.Text = "数据库信息";
            gbBaseInfo.Location = new Point(margin, margin);
            gbBaseInfo.Size = new Size(tabWidth - 2 * margin, 120);
            gbBaseInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbBaseInfo.Font = new Font("微软雅黑", 9F, FontStyle.Bold);

            // 数据库名称
            Label lblDbName = new Label { Text = "当前数据库：", Location = new Point(20, 30), Size = new Size(100, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            txtDbName = new TextBox { Name = "txtDbName", Location = new Point(120, 27), Size = new Size(300, 25), ReadOnly = true, BackColor = Color.WhiteSmoke, Font = new Font("微软雅黑", 9F) };
            txtDbName.Text = "DB_DiabetesHealthManagement";

            // 上次备份时间
            Label lblLastBackup = new Label { Text = "上次备份时间：", Location = new Point(120, 65), Size = new Size(100, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            txtLastBackupTime = new TextBox { Name = "txtLastBackupTime", Location = new Point(225, 62), Size = new Size(200, 25), ReadOnly = true, BackColor = Color.WhiteSmoke, Font = new Font("微软雅黑", 9F) };

            gbBaseInfo.Controls.AddRange(new Control[] { lblDbName, txtDbName, lblLastBackup, txtLastBackupTime });
            tab.Controls.Add(gbBaseInfo);

            // 2. 备份配置区
            GroupBox gbBackupConfig = new GroupBox();
            gbBackupConfig.Name = "gbBackupConfig";
            gbBackupConfig.Text = "备份配置";
            gbBackupConfig.Location = new Point(margin, 150);
            gbBackupConfig.Size = new Size(tabWidth - 2 * margin, 120);
            gbBackupConfig.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbBackupConfig.Font = new Font("微软雅黑", 9F, FontStyle.Bold);

            // 备份路径
            Label lblBackupPath = new Label { Text = "备份路径：", Location = new Point(20, 30), Size = new Size(80, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            txtBackupPath = new TextBox { Name = "txtBackupPath", Location = new Point(100, 27), Size = new Size(500, 25), ReadOnly = true, BackColor = Color.WhiteSmoke, Font = new Font("微软雅黑", 9F) };
            btnSelectPath = new Button
            {
                Name = "btnSelectPath",
                Text = "选择路径",
                Location = new Point(610, 25),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 9F)
            };

            // 备份文件名
            Label lblFileName = new Label { Text = "备份文件名：", Location = new Point(20, 65), Size = new Size(80, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            txtBackupFileName = new TextBox { Name = "txtBackupFileName", Location = new Point(100, 62), Size = new Size(500, 25), Font = new Font("微软雅黑", 9F) };

            gbBackupConfig.Controls.AddRange(new Control[] { lblBackupPath, txtBackupPath, btnSelectPath, lblFileName, txtBackupFileName });
            tab.Controls.Add(gbBackupConfig);

            // 3. 进度与状态区
            GroupBox gbProgress = new GroupBox();
            gbProgress.Name = "gbProgress";
            gbProgress.Text = "备份进度";
            gbProgress.Location = new Point(margin, 290);
            gbProgress.Size = new Size(tabWidth - 2 * margin, 100);
            gbProgress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbProgress.Font = new Font("微软雅黑", 9F, FontStyle.Bold);

            pbBackupProgress = new ProgressBar { Name = "pbBackupProgress", Location = new Point(20, 30), Size = new Size(tabWidth - 4 * margin, 30), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            lblBackupStatus = new Label { Name = "lblBackupStatus", Text = "就绪", Location = new Point(20, 70), AutoSize = true, Font = new Font("微软雅黑", 9F, FontStyle.Regular), ForeColor = Color.Green };

            gbProgress.Controls.AddRange(new Control[] { pbBackupProgress, lblBackupStatus });
            tab.Controls.Add(gbProgress);

            // 4. 操作按钮 🔴 全局变量赋值，无局部重名
            btnStartBackup = new Button
            {
                Name = "btnStartBackup",
                Text = "立即备份",
                Location = new Point(margin, 410),
                Size = new Size(150, 45),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 12F, FontStyle.Bold)
            };
            tab.Controls.Add(btnStartBackup);
        }
        #endregion

        #region Tab2 备份记录与还原布局
        private void CreateBackupRecordTab(TabPage tab)
        {
            int margin = 20;
            int tabWidth = tab.ClientSize.Width;
            int tabHeight = tab.ClientSize.Height;

            // 1. 查询区
            GroupBox gbQuery = new GroupBox();
            gbQuery.Name = "gbQuery";
            gbQuery.Text = "查询条件";
            gbQuery.Location = new Point(margin, margin);
            gbQuery.Size = new Size(tabWidth - 2 * margin, 70);
            gbQuery.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbQuery.Font = new Font("微软雅黑", 9F, FontStyle.Bold);

            // 时间范围
            Label lblTime = new Label { Text = "时间范围：", Location = new Point(15, 25), Size = new Size(70, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            dtpRecordStart = new DateTimePicker { Name = "dtpRecordStart", Location = new Point(85, 22), Size = new Size(120, 25), Font = new Font("微软雅黑", 9F), Format = DateTimePickerFormat.Short };
            Label lblLine = new Label { Text = "至", Location = new Point(210, 25), Size = new Size(20, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            dtpRecordEnd = new DateTimePicker { Name = "dtpRecordEnd", Location = new Point(235, 22), Size = new Size(120, 25), Font = new Font("微软雅黑", 9F), Format = DateTimePickerFormat.Short };

            // 备份类型
            Label lblType = new Label { Text = "备份类型：", Location = new Point(370, 25), Size = new Size(70, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            cboBackupType = new ComboBox { Name = "cboBackupType", Location = new Point(445, 22), Size = new Size(120, 25), Font = new Font("微软雅黑", 9F), DropDownStyle = ComboBoxStyle.DropDownList };
            cboBackupType.Items.AddRange(new object[] { "全部", "手动备份", "自动备份", "还原前自动备份" });
            cboBackupType.SelectedIndex = 0;

            // 操作按钮
            btnQuery = new Button
            {
                Name = "btnQuery",
                Text = "查询",
                Location = new Point(580, 20),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 9F)
            };
            btnRefresh = new Button
            {
                Name = "btnRefresh",
                Text = "刷新",
                Location = new Point(670, 20),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(40, 160, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 9F)
            };
            btnClearExpire = new Button
            {
                Name = "btnClearExpire",
                Text = "清理过期备份",
                Location = new Point(760, 20),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 9F)
            };

            gbQuery.Controls.AddRange(new Control[] { lblTime, dtpRecordStart, lblLine, dtpRecordEnd, lblType, cboBackupType, btnQuery, btnRefresh, btnClearExpire });
            tab.Controls.Add(gbQuery);

            // 2. 列表区
            dgvBackupList = new DataGridView();
            dgvBackupList.Name = "dgvBackupList";
            dgvBackupList.Location = new Point(margin, 100);
            dgvBackupList.Size = new Size(tabWidth - 2 * margin, tabHeight - 200);
            dgvBackupList.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;

            // 表格样式
            dgvBackupList.BackgroundColor = Color.White;
            dgvBackupList.BorderStyle = BorderStyle.FixedSingle;
            dgvBackupList.GridColor = Color.LightGray;
            dgvBackupList.AllowUserToAddRows = false;
            dgvBackupList.AllowUserToDeleteRows = false;
            dgvBackupList.ReadOnly = true;
            dgvBackupList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBackupList.MultiSelect = false;
            dgvBackupList.RowHeadersVisible = false;
            dgvBackupList.ColumnHeadersHeight = 35;
            dgvBackupList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvBackupList.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvBackupList.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvBackupList.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
            dgvBackupList.ScrollBars = ScrollBars.Both;
            dgvBackupList.AutoGenerateColumns = false;
            dgvBackupList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBackupList.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

            // 初始化列
            InitBackupRecordColumns();
            tab.Controls.Add(dgvBackupList);

            // 3. 分页控件
            Panel pnlPage = new Panel();
            pnlPage.Name = "pnlPage";
            pnlPage.Location = new Point(margin, tabHeight - 90);
            pnlPage.Size = new Size(tabWidth - 2 * margin, 50);
            pnlPage.Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            pnlPage.BackColor = Color.White;

            lblRecordPageInfo = new Label { Name = "lblRecordPageInfo", Text = "总条数：0  当前页：1/0", Location = new Point(20, 15), AutoSize = true, Font = new Font("微软雅黑", 9F) };
            btnRecordPrev = new Button { Name = "btnRecordPrev", Text = "上一页", Location = new Point(300, 10), Size = new Size(80, 30), Enabled = false, Font = new Font("微软雅黑", 9F) };
            btnRecordNext = new Button { Name = "btnRecordNext", Text = "下一页", Location = new Point(390, 10), Size = new Size(80, 30), Enabled = false, Font = new Font("微软雅黑", 9F) };
            Label lblGo = new Label { Text = "跳转到：", Location = new Point(490, 15), AutoSize = true, Font = new Font("微软雅黑", 9F) };
            nudRecordPageNum = new NumericUpDown { Name = "nudRecordPageNum", Location = new Point(560, 12), Size = new Size(80, 25), Minimum = 1, Maximum = 1, Font = new Font("微软雅黑", 9F) };
            btnRecordGo = new Button { Name = "btnRecordGo", Text = "跳转", Location = new Point(650, 10), Size = new Size(60, 30), Font = new Font("微软雅黑", 9F) };

            pnlPage.Controls.AddRange(new Control[] { lblRecordPageInfo, btnRecordPrev, btnRecordNext, lblGo, nudRecordPageNum, btnRecordGo });
            tab.Controls.Add(pnlPage);
        }

        // 初始化备份记录列表列
        private void InitBackupRecordColumns()
        {
            dgvBackupList.Columns.Clear();
            dgvBackupList.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn() { Name = "index", HeaderText = "序号", FillWeight = 5 },
                new DataGridViewTextBoxColumn() { Name = "backup_id", HeaderText = "备份ID", Visible = false },
                new DataGridViewTextBoxColumn() { Name = "backup_time", HeaderText = "备份时间", FillWeight = 15 },
                new DataGridViewTextBoxColumn() { Name = "backup_file_name", HeaderText = "备份文件名", FillWeight = 20 },
                new DataGridViewTextBoxColumn() { Name = "backup_type", HeaderText = "备份类型", FillWeight = 10 },
                new DataGridViewTextBoxColumn() { Name = "backup_size", HeaderText = "文件大小", FillWeight = 10 },
                new DataGridViewTextBoxColumn() { Name = "backup_status", HeaderText = "备份状态", FillWeight = 8 },
                new DataGridViewTextBoxColumn() { Name = "backup_user", HeaderText = "备份人", FillWeight = 8 },
                new DataGridViewTextBoxColumn() { Name = "restore_status", HeaderText = "还原状态", FillWeight = 8 },
                new DataGridViewLinkColumn() { Name = "restore", HeaderText = "操作", Text = "还原", UseColumnTextForLinkValue = true, FillWeight = 6 },
                new DataGridViewLinkColumn() { Name = "delete", HeaderText = "", Text = "删除", UseColumnTextForLinkValue = true, FillWeight = 5, LinkColor = Color.Red }
            });
        }
        #endregion

        #region Tab3 自动备份配置布局
        private void CreateAutoBackupTab(TabPage tab)
        {
            int margin = 20;
            int tabWidth = tab.ClientSize.Width;

            // 1. 配置区
            GroupBox gbConfig = new GroupBox();
            gbConfig.Name = "gbConfig";
            gbConfig.Text = "自动备份配置";
            gbConfig.Location = new Point(margin, margin);
            gbConfig.Size = new Size(tabWidth - 2 * margin, 250);
            gbConfig.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbConfig.Font = new Font("微软雅黑", 9F, FontStyle.Bold);

            // 启用自动备份
            chkAutoBackupEnable = new CheckBox { Name = "chkAutoBackupEnable", Text = "启用自动备份", Location = new Point(20, 30), AutoSize = true, Font = new Font("微软雅黑", 10F, FontStyle.Bold) };

            // 备份周期
            Label lblCycle = new Label { Text = "备份周期：", Location = new Point(20, 70), Size = new Size(80, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            cboBackupCycle = new ComboBox { Name = "cboBackupCycle", Location = new Point(100, 67), Size = new Size(150, 25), Font = new Font("微软雅黑", 9F), DropDownStyle = ComboBoxStyle.DropDownList };
            cboBackupCycle.Items.AddRange(new object[] { "每天", "每周", "每月" });
            cboBackupCycle.SelectedIndex = 0;

            // 备份时间
            Label lblTime = new Label { Text = "备份时间：", Location = new Point(280, 70), Size = new Size(80, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            dtpBackupTime = new DateTimePicker { Name = "dtpBackupTime", Location = new Point(360, 67), Size = new Size(120, 25), Font = new Font("微软雅黑", 9F), Format = DateTimePickerFormat.Time, ShowUpDown = true };

            // 备份路径
            Label lblPath = new Label { Text = "备份路径：", Location = new Point(20, 110), Size = new Size(80, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            txtAutoBackupPath = new TextBox { Name = "txtAutoBackupPath", Location = new Point(100, 107), Size = new Size(500, 25), ReadOnly = true, BackColor = Color.WhiteSmoke, Font = new Font("微软雅黑", 9F) };
            btnSelectAutoPath = new Button
            {
                Name = "btnSelectAutoPath",
                Text = "选择路径",
                Location = new Point(610, 105),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 9F)
            };

            // 保留天数
            Label lblRetainDays = new Label { Text = "文件保留天数：", Location = new Point(20, 150), Size = new Size(100, 25), Font = new Font("微软雅黑", 9F, FontStyle.Regular) };
            txtRetainDays = new TextBox { Name = "txtRetainDays", Location = new Point(120, 147), Size = new Size(150, 25), Font = new Font("微软雅黑", 9F) };
            Label lblDayTip = new Label { Text = "天（超过天数的备份文件将自动清理）", Location = new Point(280, 150), AutoSize = true, Font = new Font("微软雅黑", 9F, FontStyle.Regular), ForeColor = Color.Gray };

            // 状态提示
            lblAutoBackupStatus = new Label { Name = "lblAutoBackupStatus", Text = "自动备份状态：未启用", Location = new Point(20, 190), AutoSize = true, Font = new Font("微软雅黑", 10F, FontStyle.Bold), ForeColor = Color.Gray };

            gbConfig.Controls.AddRange(new Control[] { chkAutoBackupEnable, lblCycle, cboBackupCycle, lblTime, dtpBackupTime, lblPath, txtAutoBackupPath, btnSelectAutoPath, lblRetainDays, txtRetainDays, lblDayTip, lblAutoBackupStatus });
            tab.Controls.Add(gbConfig);

            // 2. 操作按钮
            btnSaveConfig = new Button
            {
                Name = "btnSaveConfig",
                Text = "保存配置",
                Location = new Point(margin, 280),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 10F, FontStyle.Bold)
            };
            btnResetConfig = new Button
            {
                Name = "btnResetConfig",
                Text = "重置默认配置",
                Location = new Point(150, 280),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 10F)
            };

            tab.Controls.AddRange(new Control[] { btnSaveConfig, btnResetConfig });
        }
        #endregion
        #endregion

        #region 事件绑定（统一管理，无重复绑定）
        private void BindAllEvents()
        {
            // 窗体生命周期事件
            this.Load += FrmBackupRestore_Load;
            this.FormClosing += FrmBackupRestore_FormClosing;

            // Tab1 手动备份事件
            btnSelectPath.Click += BtnSelectPath_Click;
            btnStartBackup.Click += BtnStartBackup_Click;

            // Tab2 备份记录事件
            btnQuery.Click += (s, e) => { _currentPageIndex = 1; LoadBackupRecordList(); };
            btnRefresh.Click += (s, e) => LoadBackupRecordList();
            btnClearExpire.Click += BtnClearExpire_Click;
            btnRecordPrev.Click += (s, e) => { _currentPageIndex--; LoadBackupRecordList(); };
            btnRecordNext.Click += (s, e) => { _currentPageIndex++; LoadBackupRecordList(); };
            btnRecordGo.Click += (s, e) => { _currentPageIndex = (int)nudRecordPageNum.Value; LoadBackupRecordList(); };
            dgvBackupList.CellContentClick += DgvBackupList_CellContentClick;

            // Tab3 自动备份事件
            btnSelectAutoPath.Click += BtnSelectAutoPath_Click;
            btnSaveConfig.Click += BtnSaveConfig_Click;
            btnResetConfig.Click += BtnResetConfig_Click;
            chkAutoBackupEnable.CheckedChanged += (s, e) =>
            {
                lblAutoBackupStatus.Text = chkAutoBackupEnable.Checked ? "自动备份状态：已启用" : "自动备份状态：未启用";
                lblAutoBackupStatus.ForeColor = chkAutoBackupEnable.Checked ? Color.Green : Color.Gray;
            };
        }
        #endregion

        #region 页面加载与初始化
        private void FrmBackupRestore_Load(object sender, EventArgs e)
        {
            // 初始化默认路径
            string defaultPath = Path.Combine(Application.StartupPath, "Backup");
            if (!Directory.Exists(defaultPath))
            {
                Directory.CreateDirectory(defaultPath);
            }

            // 初始化手动备份默认值
            txtBackupPath.Text = defaultPath;
            txtBackupFileName.Text = $"DiabetesDB_{DateTime.Now:yyyyMMddHHmmss}.bak";
            txtAutoBackupPath.Text = defaultPath;

            // 初始化时间范围
            dtpRecordStart.Value = DateTime.Now.AddMonths(-1);
            dtpRecordEnd.Value = DateTime.Now;

            // 加载基础数据
            LoadLastBackupTime();
            LoadAutoBackupConfig();
            InitAutoBackupTimer();

            // 更新状态栏
            if (this.MdiParent is FrmAdminMain mainForm)
            {
                mainForm.SetStatusTip("操作提示：已打开【数据备份与还原】");
            }
        }

        // 窗体关闭时释放定时器
        private void FrmBackupRestore_FormClosing(object sender, FormClosingEventArgs e)
        {
            _autoBackupTimer?.Stop();
            _autoBackupTimer?.Dispose();
        }

        // 加载上次备份时间
        private void LoadLastBackupTime()
        {
            try
            {
                string sql = "SELECT TOP 1 backup_time FROM t_backup_log WHERE backup_status = 1 ORDER BY backup_time DESC";
                object obj = Tools.SqlHelper.ExecuteScalar(sql);
                if (obj != null && obj != DBNull.Value)
                {
                    txtLastBackupTime.Text = Convert.ToDateTime(obj).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    txtLastBackupTime.Text = "暂无备份记录";
                }
            }
            catch
            {
                txtLastBackupTime.Text = "加载失败";
            }
        }
        #endregion

        #region Tab1 手动备份事件处理
        // 选择备份路径
        private void BtnSelectPath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "选择备份文件保存路径";
                fbd.SelectedPath = txtBackupPath.Text;
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtBackupPath.Text = fbd.SelectedPath;
                }
            }
        }

        // 立即备份 🔴 彻底解决参数匹配、空引用、控件访问问题
        private void BtnStartBackup_Click(object sender, EventArgs e)
        {
            // 空引用校验
            if (Program.LoginUser == null)
            {
                MessageBox.Show("登录信息失效，请重新登录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 输入校验
            if (string.IsNullOrEmpty(txtBackupPath.Text))
            {
                MessageBox.Show("请选择备份路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtBackupFileName.Text))
            {
                MessageBox.Show("请输入备份文件名", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!txtBackupFileName.Text.Trim().EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
            {
                txtBackupFileName.Text = txtBackupFileName.Text.Trim() + ".bak";
            }

            string fullPath = Path.Combine(txtBackupPath.Text, txtBackupFileName.Text.Trim());

            // 二次确认
            if (MessageBox.Show($"确定要执行数据库备份吗？\n备份路径：{fullPath}", "确认备份", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            // 执行备份
            this.Cursor = Cursors.WaitCursor;
            pbBackupProgress.Value = 20;
            lblBackupStatus.Text = "正在准备备份...";
            btnStartBackup.Enabled = false;

            try
            {
                // 🔴 严格对齐BLL层方法签名，参数100%匹配
                bool result = _bllBackup.ExecuteBackup(
                    backupPath: fullPath,
                    backupType: "手动备份",
                    operatorId: Program.LoginUser.user_id,
                    operatorRoleId: Program.LoginUser.user_type,
                    clientIP: Program.ClientIP,
                    errorMsg: out string errorMsg);

                pbBackupProgress.Value = 100;

                if (result)
                {
                    lblBackupStatus.Text = "备份成功！";
                    lblBackupStatus.ForeColor = Color.Green;
                    MessageBox.Show("数据库备份成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadLastBackupTime();
                    txtBackupFileName.Text = $"DiabetesDB_{DateTime.Now:yyyyMMddHHmmss}.bak";
                }
                else
                {
                    lblBackupStatus.Text = $"备份失败：{errorMsg}";
                    lblBackupStatus.ForeColor = Color.Red;
                    MessageBox.Show(errorMsg, "备份失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                pbBackupProgress.Value = 0;
                lblBackupStatus.Text = $"备份异常：{ex.Message}";
                lblBackupStatus.ForeColor = Color.Red;
                MessageBox.Show($"备份异常：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnStartBackup.Enabled = true;
            }
        }
        #endregion

        #region Tab2 备份记录事件处理
        // 加载备份记录列表 🔴 解决Model属性缺失问题，直接计算显示值
        private void LoadBackupRecordList()
        {
            this.Cursor = Cursors.WaitCursor;
            dgvBackupList.Rows.Clear();

            try
            {
                DateTime startTime = dtpRecordStart.Value.Date;
                DateTime endTime = dtpRecordEnd.Value.Date.AddDays(1).AddSeconds(-1);
                string backupType = cboBackupType.SelectedItem?.ToString() ?? "全部";

                // 调用BLL方法
                var list = _bllBackup.GetBackupLogByPage(startTime, endTime, backupType, _currentPageIndex, PageSize, out _totalCount);

                int index = (_currentPageIndex - 1) * PageSize + 1;
                foreach (var log in list)
                {
                    // 🔴 直接计算显示值，不依赖Model的扩展属性
                    string sizeDisplay = log.backup_size > 0 ? $"{Math.Round(log.backup_size / 1024.0 / 1024.0, 2)} MB" : "0 KB";
                    string backupStatusDisplay = log.backup_status == 1 ? "成功" : "失败";
                    string restoreStatusDisplay = log.restore_status == 1 ? "已还原" : "未还原";

                    dgvBackupList.Rows.Add(
                        index++,
                        log.backup_id,
                        log.backup_time.ToString("yyyy-MM-dd HH:mm:ss"),
                        Path.GetFileName(log.backup_path),
                        log.backup_type,
                        sizeDisplay,
                        backupStatusDisplay,
                        log.backup_user_name,
                        restoreStatusDisplay
                    );
                }

                // 更新分页控件
                UpdateRecordPageControl();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载备份记录失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        // 更新分页控件
        private void UpdateRecordPageControl()
        {
            int totalPage = (int)Math.Ceiling(_totalCount * 1.0 / PageSize);
            lblRecordPageInfo.Text = $"总条数：{_totalCount}  当前页：{_currentPageIndex}/{Math.Max(totalPage, 1)}";
            nudRecordPageNum.Maximum = Math.Max(totalPage, 1);
            nudRecordPageNum.Value = _currentPageIndex;

            btnRecordPrev.Enabled = _currentPageIndex > 1;
            btnRecordNext.Enabled = _currentPageIndex < totalPage;
            btnRecordGo.Enabled = totalPage > 1;
        }

        // 还原/删除按钮点击 🔴 解决参数匹配、空引用问题
        private void DgvBackupList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || Program.LoginUser == null) return;

            int backupId = Convert.ToInt32(dgvBackupList.Rows[e.RowIndex].Cells["backup_id"].Value);
            string backupStatus = dgvBackupList.Rows[e.RowIndex].Cells["backup_status"].Value?.ToString() ?? "";

            // 还原操作
            if (dgvBackupList.Columns[e.ColumnIndex].Name == "restore")
            {
                if (backupStatus != "成功")
                {
                    MessageBox.Show("该备份文件备份失败，无法还原", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show("警告：数据还原将覆盖当前数据库所有数据！\n系统将自动备份当前数据库，确认要执行还原吗？", "风险确认",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                {
                    return;
                }

                this.Cursor = Cursors.WaitCursor;
                try
                {
                    // 🔴 严格对齐BLL方法签名
                    bool result = _bllBackup.ExecuteRestore(
                        backupId: backupId,
                        operatorId: Program.LoginUser.user_id,
                        operatorRoleId: Program.LoginUser.user_type,
                        clientIP: Program.ClientIP,
                        errorMsg: out string errorMsg);

                    if (result)
                    {
                        MessageBox.Show("数据还原成功！请重启系统以加载新数据。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadBackupRecordList();
                    }
                    else
                    {
                        MessageBox.Show(errorMsg, "还原失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"还原异常：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                }
            }

            // 删除操作
            if (dgvBackupList.Columns[e.ColumnIndex].Name == "delete")
            {
                if (MessageBox.Show("确定要删除该备份记录和对应的备份文件吗？删除后无法恢复！", "确认删除",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                // 🔴 严格对齐BLL方法签名
                bool result = _bllBackup.DeleteBackupLog(
                    backupId: backupId,
                    operatorId: Program.LoginUser.user_id,
                    operatorRoleId: Program.LoginUser.user_type,
                    clientIP: Program.ClientIP,
                    errorMsg: out string errorMsg);

                if (result)
                {
                    MessageBox.Show("删除成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadBackupRecordList();
                }
                else
                {
                    MessageBox.Show(errorMsg, "删除失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 清理过期备份
        private void BtnClearExpire_Click(object sender, EventArgs e)
        {
            var config = _bllBackup.GetAutoBackupConfig();
            if (!int.TryParse(config["AutoBackup_RetainDays"], out int retainDays))
            {
                retainDays = 30;
            }

            if (MessageBox.Show($"确定要清理超过{retainDays}天的备份文件吗？", "确认清理",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            int deleteCount = _bllBackup.ClearExpiredBackup(retainDays, out string errorMsg);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                MessageBox.Show(errorMsg, "清理失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show($"清理完成，共删除{deleteCount}条过期备份记录", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadBackupRecordList();
            }
        }
        #endregion

        #region Tab3 自动备份配置事件处理
        // 加载自动备份配置
        private void LoadAutoBackupConfig()
        {
            var config = _bllBackup.GetAutoBackupConfig();
            chkAutoBackupEnable.Checked = config["AutoBackup_Enable"] == "1";
            cboBackupCycle.SelectedItem = config["AutoBackup_Cycle"];

            if (TimeSpan.TryParse(config["AutoBackup_Time"], out TimeSpan time))
            {
                dtpBackupTime.Value = DateTime.Today.Add(time);
            }
            else
            {
                dtpBackupTime.Value = DateTime.Today.AddHours(2);
            }

            txtAutoBackupPath.Text = config["AutoBackup_Path"];
            txtRetainDays.Text = config["AutoBackup_RetainDays"];
        }

        // 选择自动备份路径
        private void BtnSelectAutoPath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "选择自动备份文件保存路径";
                fbd.SelectedPath = txtAutoBackupPath.Text;
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtAutoBackupPath.Text = fbd.SelectedPath;
                }
            }
        }

        // 保存配置 🔴 解决参数匹配、空引用问题
        private void BtnSaveConfig_Click(object sender, EventArgs e)
        {
            if (Program.LoginUser == null)
            {
                MessageBox.Show("登录信息失效，请重新登录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtAutoBackupPath.Text))
            {
                MessageBox.Show("请选择自动备份路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txtRetainDays.Text.Trim(), out int retainDays) || retainDays < 1)
            {
                MessageBox.Show("请输入有效的文件保留天数（正整数）", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool enable = chkAutoBackupEnable.Checked;
            string cycle = cboBackupCycle.SelectedItem?.ToString() ?? "每天";
            string backupTime = dtpBackupTime.Value.ToString("HH:mm:ss");
            string path = txtAutoBackupPath.Text;

            // 🔴 严格对齐BLL方法签名
            bool result = _bllBackup.SaveAutoBackupConfig(
                enable: enable,
                cycle: cycle,
                backupTime: backupTime,
                backupPath: path,
                retainDays: retainDays,
                operatorId: Program.LoginUser.user_id,
                errorMsg: out string errorMsg);

            if (result)
            {
                MessageBox.Show("配置保存成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                InitAutoBackupTimer();
            }
            else
            {
                MessageBox.Show(errorMsg, "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 重置默认配置
        private void BtnResetConfig_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要重置为默认配置吗？", "确认重置", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            string defaultPath = Path.Combine(Application.StartupPath, "Backup");
            chkAutoBackupEnable.Checked = false;
            cboBackupCycle.SelectedIndex = 0;
            dtpBackupTime.Value = DateTime.Today.AddHours(2);
            txtAutoBackupPath.Text = defaultPath;
            txtRetainDays.Text = "30";
        }

        // 初始化自动备份定时器 🔴 解决跨线程、GC回收问题
        private void InitAutoBackupTimer()
        {
            // 先释放旧定时器
            _autoBackupTimer?.Stop();
            _autoBackupTimer?.Dispose();

            var config = _bllBackup.GetAutoBackupConfig();
            if (config["AutoBackup_Enable"] != "1") return;

            // 每分钟检查一次是否到备份时间
            _autoBackupTimer = new System.Timers.Timer(60000);
            _autoBackupTimer.Elapsed += AutoBackupTimer_Elapsed;
            _autoBackupTimer.AutoReset = true;
            _autoBackupTimer.Start();

            // 存入全局变量，防止GC回收
            Program.GlobalAutoBackupTimer = _autoBackupTimer;
        }

        // 自动备份定时器事件 🔴 彻底解决跨线程UI访问问题
        private void AutoBackupTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var config = _bllBackup.GetAutoBackupConfig();
            if (config["AutoBackup_Enable"] != "1") return;

            DateTime now = DateTime.Now;
            string cycle = config["AutoBackup_Cycle"];
            if (!TimeSpan.TryParse(config["AutoBackup_Time"], out TimeSpan backupTime))
            {
                backupTime = TimeSpan.FromHours(2);
            }

            // 判断是否到备份时间
            bool isBackupTime = false;
            switch (cycle)
            {
                case "每天":
                    isBackupTime = now.Hour == backupTime.Hours && now.Minute == backupTime.Minutes;
                    break;
                case "每周":
                    isBackupTime = now.DayOfWeek == DayOfWeek.Sunday && now.Hour == backupTime.Hours && now.Minute == backupTime.Minutes;
                    break;
                case "每月":
                    isBackupTime = now.Day == 1 && now.Hour == backupTime.Hours && now.Minute == backupTime.Minutes;
                    break;
            }

            if (isBackupTime)
            {
                string fileName = $"AutoBackup_{now:yyyyMMddHHmmss}.bak";
                string fullPath = Path.Combine(config["AutoBackup_Path"], fileName);

                // 执行自动备份，用系统默认参数
                _bllBackup.ExecuteBackup(
                    backupPath: fullPath,
                    backupType: "自动备份",
                    operatorId: 1,
                    operatorRoleId: 1,
                    clientIP: "127.0.0.1",
                    errorMsg: out _);

                // 清理过期备份
                if (int.TryParse(config["AutoBackup_RetainDays"], out int retainDays))
                {
                    _bllBackup.ClearExpiredBackup(retainDays, out _);
                }

                // 🔴 跨线程UI更新，通过Invoke封送
                this.Invoke(new Action(() =>
                {
                    LoadLastBackupTime();
                    LoadBackupRecordList();
                }));
            }
        }
        #endregion
    }
}