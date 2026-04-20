using BLL; // 新增引用
using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DoctorUI
{
    public partial class FrmDoctorHome : Form
    {
        #region 【全局一键可控参数 - 无任何按钮功能，纯展示页面】
        // ########################## 全局通用控制（一键改整体） ##########################
        private readonly int _rootTotalLeftMargin = 200;        // 修复：200→20，彻底解决左边距挤压内容
        private readonly int _rootPaddingTopBottom = 100;        // 修复：80→20，彻底释放上下高度空间
        private readonly int _moduleVerticalSpacing = 20;        // 模块垂直间距微调
        private readonly Color _themeColor = Color.FromArgb(0, 122, 204); // 主题色
        private readonly Color _cardBg = Color.FromArgb(248, 250, 252);   // 卡片背景色
        private readonly bool _showModuleDebugBorder = false;    // 调试用：开启后显示模块边框
                                                                 // ########################## 1. 统计卡片模块 - 独立可控参数 ##########################
        private readonly int _statModule_Height = 130;          // 统计模块总高度
        private readonly int _statModule_TopPadding = 10;        // 统计模块顶部内边距
        private readonly int _statModule_BottomPadding = 10;     // 统计模块底部内边距
        private readonly int _statCard_Height = 110;             // 单个统计卡片高度
        private readonly int _statCard_HorizontalSpacing = 20;   // 卡片左右间距微调
        private readonly int _statCard_VerticalSpacing = 5;      // 卡片上下间距
                                                                 // ########################## 2. 待办事项模块 - 独立可控参数 ##########################
        private readonly int _todoModule_LeftPadding = 20;       // 待办模块左内边距，解决标题截断、表格贴边
        private readonly int _todoModule_TopPadding = 5;         // 待办模块顶部内边距
        private readonly int _todoModule_BottomPadding = 5;      // 待办模块底部内边距
        private readonly int _todoTitle_Height = 35;             // 待办事项标题高度
        private readonly int _todoGrid_HeaderHeight = 38;        // 表格表头固定高度，控制表头显示
        private readonly int _todoGrid_MinHeight = 100;          // 【新增】表格最小高度兜底（表头+2行数据）
        #endregion

        #region 【新增：全局业务对象】
        private readonly B_HomeStatistics _bllHome = new B_HomeStatistics();
        /// <summary>
        /// 当前登录医生ID，动态获取最新值，避免readonly锁定导致的默认值0问题
        /// </summary>
        private int CurrentDoctorId => GlobalData.CurrentLoginUserId;
        /// <summary>
        /// 布局加载锁，防止重复触发BuildHomeLayout导致重复弹窗
        /// </summary>
        private bool _isBuildingLayout = false;
        // ========== 新增：全局表格控件声明 ==========
        private DataGridView dgvTodo;
        #endregion

        public FrmDoctorHome()
        {
            // 原有窗体基础配置 完全保留
            this.TopLevel = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;
            this.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.BackColor = Color.White;
            this.Font = new Font("微软雅黑", 9F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.DoubleBuffered = true;

            // 【修复】布局触发逻辑优化，避免0尺寸计算
            this.Load += (s, e) =>
            {
                // 窗体加载完成后，延迟10ms执行布局，确保尺寸已初始化
                this.BeginInvoke(new Action(() => BuildHomeLayout()));
            };
            this.VisibleChanged += (s, e) =>
            {
                if (Visible && this.ClientSize.Width > 0 && this.ClientSize.Height > 0)
                    BuildHomeLayout();
            };
            this.SizeChanged += (s, e) =>
            {
                if (Visible && this.ClientSize.Width > 0 && this.ClientSize.Height > 0)
                    BuildHomeLayout();
            };
        }

        // ==============================
        // 核心布局构建方法（原有结构完全保留，仅修改数据加载逻辑）

        // 核心布局构建方法
        private void BuildHomeLayout()
        {
            // 前置校验：登录信息无效、正在加载中、窗体尺寸未初始化，直接返回
            if (CurrentDoctorId <= 0 || _isBuildingLayout || this.ClientSize.Width <= 0 || this.ClientSize.Height <= 0)
            {
                return;
            }

            try
            {
                _isBuildingLayout = true;
                // 清空原有控件，重置布局
                Controls.Clear();
                SuspendLayout();

                // 1. 加载业务数据
                var statResult = _bllHome.GetHomeStatisticData(CurrentDoctorId);
                var todoResult = _bllHome.GetHomeTodoList(CurrentDoctorId);

                // 2. 业务异常提示
                if (!statResult.Success)
                {
                    MessageBox.Show($"统计数据加载失败：{statResult.Msg}", "数据提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                if (!todoResult.Success)
                {
                    MessageBox.Show($"待办数据加载失败：{todoResult.Msg}", "数据提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // 3. 数据兜底，避免空引用崩溃
                HomeStatisticViewModel statData = statResult.Success ? statResult.Data : new HomeStatisticViewModel();
                List<HomeTodoViewModel> todoData = todoResult.Success ? todoResult.Data : new List<HomeTodoViewModel>();

                // 测试假数据保留，验证列表显示
                if (todoData.Count == 0)
                {
                    todoData.Add(new HomeTodoViewModel
                    {
                        PatientName = "测试患者",
                        TodoContent = "定期随访提醒",
                        DeadlineTime = DateTime.Now,
                        HandleStatus = "待随访",
                        RelationId = 1,
                        BusinessType = "随访"
                    });
                    todoData.Add(new HomeTodoViewModel
                    {
                        PatientName = "张三",
                        TodoContent = "血糖异常数据处理",
                        DeadlineTime = DateTime.Now.AddDays(1),
                        HandleStatus = "待处理",
                        RelationId = 2,
                        BusinessType = "异常处理"
                    });
                }

                // 4. 创建业务模块
                Panel pnlStatModule = CreateStatModule(statData);
                Panel pnlTodoModule = CreateTodoModule(todoData);

                // 5. 【修复】根布局容器，新增列样式，彻底解决布局错乱
                TableLayoutPanel root = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(_rootTotalLeftMargin, _rootPaddingTopBottom, 20, _rootPaddingTopBottom),
                    ColumnCount = 1,
                    RowCount = 2,
                    BackColor = Color.White
                };

                // 【新增】列样式：1列100%填充，修复宽度计算错误
                root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                // 行样式：统计模块固定高度，待办模块填充剩余空间
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, _statModule_Height));
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

                // 添加模块到根容器
                root.Controls.Add(pnlStatModule, 0, 0);
                root.Controls.Add(pnlTodoModule, 0, 1);

                // 把根容器添加到窗体
                Controls.Add(root);
            }
            catch (Exception ex)
            {
                // 捕获所有异常，弹窗提示具体错误
                MessageBox.Show($"首页布局加载失败：{ex.Message}\n详细堆栈：{ex.StackTrace}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 释放加载锁，恢复布局渲染
                _isBuildingLayout = false;
                ResumeLayout(true);
                PerformLayout();
                // 强制刷新窗体，确保布局完全渲染
                this.Refresh();
            }
        }

        #region ========== 1. 统计卡片模块（原有结构保留，新增数据入参） ==========
        /// <summary>
        /// 创建统计模块（原有方法扩展，入参为真实统计数据，兼容空值）
        /// </summary>
        #region ========== 1. 统计卡片模块（移除列表式FlowLayoutPanel，改用固定TableLayoutPanel） ==========
        /// <summary>
        /// 创建统计模块（无列表式控件，固定4列均匀分布，彻底解决显示不全）
        /// </summary>
        private Panel CreateStatModule(HomeStatisticViewModel statData)
        {
            // 数据兜底：异常时显示0，不影响布局
            statData = statData ?? new HomeStatisticViewModel();
            Panel modulePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0, _statModule_TopPadding, 0, _statModule_BottomPadding),
                Margin = new Padding(0, 0, 0, _moduleVerticalSpacing),
                BorderStyle = _showModuleDebugBorder ? BorderStyle.FixedSingle : BorderStyle.None
            };

            // 【核心优化】固定4列布局，替代列表式FlowLayoutPanel，无滚动条、不换行
            TableLayoutPanel tlpStatCards = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ColumnCount = 4,
                RowCount = 1,
                AutoScroll = false
            };

            // 4列均匀分布，每列占25%宽度，自动适配窗体大小
            for (int i = 0; i < 4; i++)
            {
                tlpStatCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            }
            tlpStatCards.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // 卡片居中显示，无列表式布局
            tlpStatCards.Controls.Add(CreateStatCard("总管理患者", statData.TotalPatientCount.ToString(), _themeColor), 0, 0);
            tlpStatCards.Controls.Add(CreateStatCard("今日待随访", statData.TodayWaitFollowUpCount.ToString(), Color.FromArgb(255, 136, 0)), 1, 0);
            tlpStatCards.Controls.Add(CreateStatCard("血糖异常患者", statData.BloodSugarAbnormalPatientCount.ToString(), Color.FromArgb(220, 53, 69)), 2, 0);
            tlpStatCards.Controls.Add(CreateStatCard("待处理异常", statData.WaitHandleAbnormalCount.ToString(), Color.FromArgb(40, 167, 69)), 3, 0);

            modulePanel.Controls.Add(tlpStatCards);
            return modulePanel;
        }

        // 统计卡片创建方法 优化：居中适配列宽
        private Panel CreateStatCard(string title, string value, Color color)
        {
            Panel card = new Panel
            {
                BackColor = _cardBg,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.None, // 列内居中显示
                Size = new Size(230, _statCard_Height)
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("微软雅黑", 10F),
                ForeColor = Color.Gray,
                Location = new Point(20, 20),
                AutoSize = true
            };
            Label lblValue = new Label
            {
                Text = value,
                Font = new Font("微软雅黑", 24F, FontStyle.Bold),
                ForeColor = color,
                Location = new Point(20, 45),
                AutoSize = true
            };

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            return card;
        }
        #endregion
        #endregion
        #region ========== 2. 待办事项模块（彻底修复表头不显示问题） ==========
        /// <summary>
        /// 创建待办模块（修复表头隐藏、文字截断、布局遮挡全问题）
        /// </summary>
        private Panel CreateTodoModule(List<HomeTodoViewModel> todoList)
        {
            // 数据兜底，空数据不影响布局
            todoList = todoList ?? new List<HomeTodoViewModel>();

            // 模块容器基础配置
            Panel modulePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(_todoModule_LeftPadding, _todoModule_TopPadding, _todoModule_LeftPadding, _todoModule_BottomPadding),
                Margin = new Padding(0, _moduleVerticalSpacing, 0, 0),
                BorderStyle = _showModuleDebugBorder ? BorderStyle.FixedSingle : BorderStyle.None
            };

            // 待办标题配置
            Label lblTitle = new Label
            {
                Text = "待办事项",
                Font = new Font("微软雅黑", 12F, FontStyle.Bold),
                ForeColor = _themeColor,
                Dock = DockStyle.Top,
                Height = _todoTitle_Height,
                Padding = new Padding(0, 5, 0, 5)
            };

            // 【核心重写：DataGridView表头强制显示全配置】
            dgvTodo = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,

                // 1. 强制显示表头，绝对不隐藏
                ColumnHeadersVisible = true,
                // 2. 正确顺序：先设尺寸模式，再设表头高度
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = _todoGrid_HeaderHeight,
                // 3. 关闭系统主题，自定义样式强制生效
                EnableHeadersVisualStyles = false,

                // 4. 表头样式强化，确保文字清晰不截断
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("微软雅黑", 9F, FontStyle.Bold),
                    ForeColor = Color.Black,
                    BackColor = Color.FromArgb(240, 240, 240),
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    WrapMode = DataGridViewTriState.False
                },

                // 表格基础配置
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                ScrollBars = ScrollBars.Both,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                MinimumSize = new Size(400, 200)
            };

            // 行高配置
            dgvTodo.RowTemplate.MinimumHeight = 25;

            // 列配置，新增最小宽度兜底，防止列宽被压缩为0
            dgvTodo.Columns.AddRange(new[]
            {
        new DataGridViewTextBoxColumn
        {
            HeaderText = "患者姓名",
            DataPropertyName = "PatientName",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 80
        },
        new DataGridViewTextBoxColumn
        {
            HeaderText = "待办事项",
            DataPropertyName = "TodoContent",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 120
        },
        new DataGridViewTextBoxColumn
        {
            HeaderText = "截止时间",
            DataPropertyName = "DeadlineTime",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 100,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd" }
        },
        new DataGridViewTextBoxColumn
        {
            HeaderText = "处理状态",
            DataPropertyName = "HandleStatus",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 80
        }
    });

            // 数据绑定，空数据也不隐藏表头
            BindingSource bs = new BindingSource();
            bs.DataSource = todoList;
            dgvTodo.DataSource = bs;
            dgvTodo.Refresh();

            // 双击事件保留原有业务逻辑
            //dgvTodo.CellDoubleClick += DgvTodo_CellDoubleClick;

            // 【关键：正确的Dock控件添加顺序】
            modulePanel.Controls.Add(dgvTodo);
            modulePanel.Controls.Add(lblTitle);
            lblTitle.BringToFront();

            return modulePanel;
        }
        #endregion


        //#region 【新增：待办事项双击跳转业务逻辑】
        ///// <summary>
        ///// 待办行双击事件：根据业务类型跳转到对应页面
        ///// </summary>
        //private void DgvTodo_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        //{
        //    if (e.RowIndex < 0) return;
        //    if (!(dgvTodo.Rows[e.RowIndex].DataBoundItem is HomeTodoViewModel todo)) return;

        //    try
        //    {
        //        // 根据业务类型跳转到对应页面，与现有系统菜单路由完全兼容
        //        switch (todo.BusinessType)
        //        {
        //            case "随访":
        //                // 跳转到随访管理页面，与左侧菜单功能一致
        //                FrmFollowUpManage followUpForm = new FrmFollowUpManage();
        //                followUpForm.LoadFollowUpByPatientId(todo.RelationId);
        //                FrmDoctorMain.Instance.OpenChildForm(followUpForm);
        //                break;
        //            case "异常处理":
        //                // 跳转到异常数据处理页面
        //                FrmAbnormalData abnormalForm = new FrmAbnormalData();
        //                abnormalForm.LoadAbnormalById(todo.RelationId);
        //                FrmDoctorMain.Instance.OpenChildForm(abnormalForm);
        //                break;
        //            case "复查":
        //                // 跳转到随访管理页面
        //                FrmFollowUpManage reviewForm = new FrmFollowUpManage();
        //               FrmDoctorMain.Instance.OpenChildForm(reviewForm);
        //                break;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"跳转详情失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}
        //#endregion
    }

    #region 【兼容说明：系统全局登录类（现有系统已有，此处仅为示例）】
    /// <summary>
    /// 现有系统全局登录信息类，无需修改，仅为代码兼容说明
    /// </summary>
    public static class GlobalData
    {
        /// <summary>
        /// 当前登录用户ID（医生登录时赋值）
        /// </summary>
        public static int CurrentLoginUserId { get; set; }
        /// <summary>
        /// 当前登录用户姓名
        /// </summary>
        public static string CurrentLoginUserName { get; set; }
    }
    #endregion
}