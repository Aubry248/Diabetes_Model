using System;
using System.Drawing;
using System.Windows.Forms;
using BLL;
using Model;

namespace AdminUI
{
    public partial class FrmLogDetail : Form
    {
        private readonly int _logType; // 0=登录日志 1=操作日志
        private readonly int _logId;
        private readonly B_AccessLog bllAccess = new B_AccessLog();
        private readonly B_AuditLog bllAudit = new B_AuditLog();

        public FrmLogDetail(int logType, int logId)
        {
            _logType = logType;
            _logId = logId;
            InitializeComponent();
            this.Controls.Clear();

            // 窗体配置
            this.Text = "日志详情";
            this.Size = new Size(800, 600);
            this.MinimumSize = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("微软雅黑", 9F);
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            CreateDetailControls();
            this.Load += FrmLogDetail_Load;
        }

        private void CreateDetailControls()
        {
            this.SuspendLayout();
            int margin = 20;

            // 标题
            Label lblTitle = new Label
            {
                Text = _logType == 0 ? "登录日志详情" : "操作日志详情",
                Font = new Font("微软雅黑", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(margin, margin),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            // 详情文本框（只读）
            TextBox txtDetail = new TextBox();
            txtDetail.Name = "txtDetail";
            txtDetail.Location = new Point(margin, 60);
            txtDetail.Size = new Size(this.ClientSize.Width - 2 * margin, this.ClientSize.Height - 130);
            txtDetail.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            txtDetail.Multiline = true;
            txtDetail.ReadOnly = true;
            txtDetail.ScrollBars = ScrollBars.Vertical;
            txtDetail.Font = new Font("微软雅黑", 9F);
            txtDetail.WordWrap = true;
            this.Controls.Add(txtDetail);

            // 关闭按钮
            Button btnClose = new Button
            {
                Text = "关闭",
                Location = new Point(this.ClientSize.Width - 120 - margin, this.ClientSize.Height - 50),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 10F)
            };
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);

            this.ResumeLayout(true);
        }

        private void FrmLogDetail_Load(object sender, EventArgs e)
        {
            TextBox txtDetail = this.Controls["txtDetail"] as TextBox;
            if (_logType == 0)
            {
                // 登录日志详情
                AuditLog log = bllAudit.GetAuditLogById(_logId);
                if (log == null)
                {
                    txtDetail.Text = "日志不存在！";
                    return;
                }
                txtDetail.Text = $"日志ID：{log.audit_id}\r\n" +
                                 $"用户名：{log.user_name}\r\n" +
                                 $"用户角色：{log.role_name}\r\n" +
                                 $"操作类型：{log.operate_type}\r\n" +
                                 $"操作内容：{log.operate_content}\r\n" +
                                 $"操作IP：{log.operate_ip}\r\n" +
                                 $"设备信息：{log.operate_device}\r\n" +
                                 $"操作时间：{log.operate_time:yyyy-MM-dd HH:mm:ss}\r\n" +
                                 $"备注：{log.remark}";
            }
            else
            {
                // 操作日志详情
                AccessLog log = bllAccess.GetAccessLogById(_logId);
                if (log == null)
                {
                    txtDetail.Text = "日志不存在！";
                    return;
                }
                txtDetail.Text = $"日志ID：{log.access_id}\r\n" +
                                 $"用户名：{log.user_name}\r\n" +
                                 $"用户角色：{log.role_name}\r\n" +
                                 $"操作模块：{log.interface_module}\r\n" +
                                 $"操作类型：{log.action}\r\n" +
                                 $"操作表名：{log.table_name}\r\n" +
                                 $"操作记录ID：{log.record_id}\r\n" +
                                 $"操作状态：{(log.access_status == 1 ? "成功" : "失败")}\r\n" +
                                 $"响应耗时：{log.response_time}ms\r\n" +
                                 $"是否敏感操作：{(log.is_sensitive_operation ? "是" : "否")}\r\n" +
                                 $"数据敏感等级：{log.data_sensitivity_level}\r\n" +
                                 $"操作IP：{log.ip_address}\r\n" +
                                 $"设备类型：{log.device_type}\r\n" +
                                 $"操作时间：{log.access_time:yyyy-MM-dd HH:mm:ss}\r\n" +
                                 $"错误信息：{log.error_msg}\r\n" +
                                 $"==================== 原始值 ====================\r\n{log.old_value}\r\n" +
                                 $"==================== 新值 ====================\r\n{log.new_value}";
            }
            // 光标定位到开头
            txtDetail.SelectionStart = 0;
            txtDetail.ScrollToCaret();
        }
    }
}