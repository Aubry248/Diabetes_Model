using System;
using System.Drawing;
using System.Windows.Forms;
using Model;
using BLL;

namespace DoctorUI
{
    public partial class FrmDoctorMyProfile : Form
    {
        // ==============================
        // 页面可调节参数（注释清晰，方便修改）
        // ==============================
        private readonly int _pageTopMargin = 100;      // 页面顶部留白
        private readonly int _pageLeftMargin = 150;     // 页面左侧留白
        private readonly int _pageRightMargin = 20;     // 页面右侧留白
        private readonly int _pageBottomMargin = 20;    // 页面底部留白
        //private readonly int _infoAreaHeight = 380;      // 信息区域高度
        private readonly Padding _controlMargin = new Padding(5); // 控件间距
        private readonly Color _themeColor = Color.FromArgb(0, 122, 204); // 主题色

        // 业务层
        private readonly B_User _bllUser = new B_User();

        public FrmDoctorMyProfile()
        {
            // 子窗体标准配置（适配主窗体嵌入）
            this.TopLevel = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.Font = new Font("微软雅黑", 9F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.DoubleBuffered = true;

            // 页面显示时加载布局
            this.VisibleChanged += (s, e) =>
            {
                if (Visible) BuildLayout();
            };
        }

        // 构建页面布局
        private void BuildLayout()
        {
            Controls.Clear();
            SuspendLayout();

            // 根容器
            var rootPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 1,
                BackColor = Color.White,
                Padding = new Padding(_pageLeftMargin, _pageTopMargin, _pageRightMargin, _pageBottomMargin)
            };
            rootPanel.Controls.Add(CreateInfoPanel(), 0, 0);

            Controls.Add(rootPanel);
            ResumeLayout(true);
            PerformLayout();
        }

        // 创建个人信息面板
        private Panel CreateInfoPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(20)
            };

            // 标题
            var lblTitle = new Label
            {
                Text = "医生个人中心",
                Font = new Font("微软雅黑", 14F, FontStyle.Bold),
                ForeColor = _themeColor,
                Dock = DockStyle.Top,
                Height = 35,
                Margin = new Padding(0, 0, 0, 10)
            };

            // 信息布局容器
            var infoLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 10,
                BackColor = Color.FromArgb(248, 250, 252)
            };
            infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
            infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
            for (int i = 0; i < 10; i++)
                infoLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // 获取当前登录医生信息
            Users user = Program.LoginUser;

            // 添加信息行（基于数据库t_user字段）
            AddInfoRow(infoLayout, "医生姓名：", user?.user_name ?? "");
            AddInfoRow(infoLayout, "登录账号：", user?.login_account ?? "");
            AddInfoRow(infoLayout, "手机号码：", user?.phone ?? "");
            AddInfoRow(infoLayout, "身份证号：", user?.id_card ?? "");
            AddInfoRow(infoLayout, "性别：", user?.gender == 1 ? "男" : "女");
            AddInfoRow(infoLayout, "年龄：", user?.age.ToString() ?? "");
            AddInfoRow(infoLayout, "账号类型：", "公卫医生");
            AddInfoRow(infoLayout, "账号状态：", user?.status == 1 ? "正常启用" : "已禁用");
            AddInfoRow(infoLayout, "创建时间：", user?.create_time.ToString("yyyy-MM-dd HH:mm") ?? "");
            AddInfoRow(infoLayout, "最后登录：", user?.last_login_time.HasValue == true ?
                user.last_login_time.Value.ToString("yyyy-MM-dd HH:mm") : "无");

            panel.Controls.Add(infoLayout);
            panel.Controls.Add(lblTitle);
            return panel;
        }

        // 添加一行信息（标签+只读文本框）
        private void AddInfoRow(TableLayoutPanel layout, string labelText, string value)
        {
            var lbl = new Label
            {
                Text = labelText,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("微软雅黑", 9F, FontStyle.Bold),
                Margin = _controlMargin
            };

            var txt = new TextBox
            {
                Text = value,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                Margin = _controlMargin,
                BackColor = Color.White,
                Font = new Font("微软雅黑", 9F)
            };

            layout.Controls.Add(lbl);
            layout.Controls.Add(txt);
        }
    }
}