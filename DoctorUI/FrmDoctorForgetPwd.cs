using BLL;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows.Forms;

// 你的项目现有命名空间，直接替换即可
namespace DoctorUI
{
    public partial class FrmDoctorForgetPwd : Form
    {
        #region 窗体控件与全局变量定义
        // 分步面板
        private Panel pnlStep1_Identity;  // 步骤1：身份校验
        private Panel pnlStep2_ResetPwd;  // 步骤2：密码重置
        private Panel pnlStep3_Success;   // 步骤3：重置完成

        // 步骤1控件
        private TextBox txtAccount;
        private TextBox txtUserName;
        private TextBox txtIdCard;
        private Label lblMsgStep1;

        // 步骤2控件
        private TextBox txtNewPwd;
        private TextBox txtConfirmPwd;
        private Label lblMsgStep2;

        // 全局状态变量
        private int _currentUserId = 0;          // 校验通过的用户ID
        private string _originalEncryptPwd = ""; // 原加密密码（用于重复校验）
        private readonly B_User bllUser = new B_User(); // 复用现有用户业务类
        private readonly Color _themePrimary = Color.FromArgb(0, 122, 204);
        private readonly Color _themeMuted = Color.FromArgb(100, 116, 139);
        private readonly Color _themeText = Color.FromArgb(51, 65, 85);
        private readonly Color _themeDanger = Color.FromArgb(220, 38, 38);
        #endregion
        #region 修复2：补充缺失的InitializeComponent()
        public FrmDoctorForgetPwd()
        {
            InitializeComponent();
            InitFormBaseStyle();
            InitStepPanels();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(620, 430);
            this.Name = "FrmDoctorForgetPwd";
            this.Text = "公卫医生端 - 密码重置";
            this.ResumeLayout(false);
        }
        #endregion

        /// <summary>
        /// 窗体基础样式初始化，与登录页完全统一
        /// </summary>
        private void InitFormBaseStyle()
        {
            this.Text = "公卫医生端 - 密码重置";
            this.ClientSize = new Size(620, 430);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Font = new Font("微软雅黑", 9.5F);
            this.BackColor = Color.FromArgb(241, 245, 249);

            Label lblTitle = new Label();
            lblTitle.Text = "找回密码";
            lblTitle.Font = new Font("微软雅黑", 17F, FontStyle.Bold);
            lblTitle.ForeColor = _themePrimary;
            lblTitle.Location = new Point((this.ClientSize.Width - 500) / 2 + 32, 22);
            lblTitle.Size = new Size(160, 34);
            Label lblSubTitle = new Label();
            lblSubTitle.Text = "验证医生身份后即可重置登录密码";
            lblSubTitle.Font = new Font("微软雅黑", 9.5F);
            lblSubTitle.ForeColor = _themeMuted;
            lblSubTitle.Location = new Point((this.ClientSize.Width - 500) / 2 + 32, 58);
            lblSubTitle.Size = new Size(300, 24);
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblSubTitle);
        }

        /// <summary>
        /// 初始化分步面板，默认只显示身份校验面板
        /// </summary>
        private void InitStepPanels()
        {
            int panelWidth = 500;
            int panelHeight = 360;
            int panelLeft = (this.ClientSize.Width - panelWidth) / 2;
            int panelTop = 56;

            // 步骤1：身份校验面板
            pnlStep1_Identity = new Panel();
            pnlStep1_Identity.Size = new Size(panelWidth, panelHeight);
            pnlStep1_Identity.Location = new Point(panelLeft, panelTop);
            pnlStep1_Identity.BackColor = Color.White;
            pnlStep1_Identity.BorderStyle = BorderStyle.FixedSingle;
            InitStep1Controls();
            this.Controls.Add(pnlStep1_Identity);

            // 步骤2：密码重置面板
            pnlStep2_ResetPwd = new Panel();
            pnlStep2_ResetPwd.Size = new Size(panelWidth, panelHeight);
            pnlStep2_ResetPwd.Location = new Point(panelLeft, panelTop);
            pnlStep2_ResetPwd.BackColor = Color.White;
            pnlStep2_ResetPwd.BorderStyle = BorderStyle.FixedSingle;
            pnlStep2_ResetPwd.Visible = false;
            InitStep2Controls();
            this.Controls.Add(pnlStep2_ResetPwd);

            // 步骤3：重置完成面板
            pnlStep3_Success = new Panel();
            pnlStep3_Success.Size = new Size(panelWidth, panelHeight);
            pnlStep3_Success.Location = new Point(panelLeft, panelTop);
            pnlStep3_Success.BackColor = Color.White;
            pnlStep3_Success.BorderStyle = BorderStyle.FixedSingle;
            pnlStep3_Success.Visible = false;
            InitStep3Controls();
            this.Controls.Add(pnlStep3_Success);
        }
        #region 分步控件初始化
        /// <summary>
        /// 步骤1：身份校验控件初始化
        /// </summary>
        private void InitStep1Controls()
        {
            int labelWidth = 88;
            int txtWidth = 300;
            int startY = 38;
            int gap = 52;

            // 1. 医生账号/手机号输入框
            Label lblAccount = new Label();
            lblAccount.Text = "医生账号";
            lblAccount.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            lblAccount.ForeColor = _themeText;
            lblAccount.AutoSize = false;
            lblAccount.Size = new Size(labelWidth, 25);
            lblAccount.Location = new Point(36, startY + 4);
            lblAccount.TextAlign = ContentAlignment.MiddleLeft;
            pnlStep1_Identity.Controls.Add(lblAccount);

            txtAccount = new TextBox();
            txtAccount.Font = new Font("微软雅黑", 10.5F);
            txtAccount.Size = new Size(txtWidth, 32);
            txtAccount.Location = new Point(138, startY);
            txtAccount.BorderStyle = BorderStyle.FixedSingle;
            pnlStep1_Identity.Controls.Add(txtAccount);
            startY += gap;

            // 2. 真实姓名输入框
            Label lblUserName = new Label();
            lblUserName.Text = "真实姓名：";
            lblUserName.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            lblUserName.ForeColor = _themeText;
            lblUserName.AutoSize = false;
            lblUserName.Size = new Size(labelWidth, 25);
            lblUserName.Location = new Point(36, startY + 4);
            lblUserName.TextAlign = ContentAlignment.MiddleLeft;
            pnlStep1_Identity.Controls.Add(lblUserName);

            txtUserName = new TextBox();
            txtUserName.Font = new Font("微软雅黑", 10.5F);
            txtUserName.Size = new Size(txtWidth, 32);
            txtUserName.Location = new Point(138, startY);
            txtUserName.BorderStyle = BorderStyle.FixedSingle;
            pnlStep1_Identity.Controls.Add(txtUserName);
            startY += gap;

            // 3. 身份证号输入框
            Label lblIdCard = new Label();
            lblIdCard.Text = "身份证号：";
            lblIdCard.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            lblIdCard.ForeColor = _themeText;
            lblIdCard.AutoSize = false;
            lblIdCard.Size = new Size(labelWidth, 25);
            lblIdCard.Location = new Point(36, startY + 4);
            lblIdCard.TextAlign = ContentAlignment.MiddleLeft;
            pnlStep1_Identity.Controls.Add(lblIdCard);

            txtIdCard = new TextBox();
            txtIdCard.Font = new Font("微软雅黑", 10.5F);
            txtIdCard.Size = new Size(txtWidth, 32);
            txtIdCard.Location = new Point(138, startY);
            txtIdCard.BorderStyle = BorderStyle.FixedSingle;
            pnlStep1_Identity.Controls.Add(txtIdCard);
            startY += gap;

            // 错误提示标签
            lblMsgStep1 = new Label();
            lblMsgStep1.Font = new Font("微软雅黑", 9F);
            lblMsgStep1.ForeColor = _themeDanger;
            lblMsgStep1.AutoSize = false;
            lblMsgStep1.Size = new Size(300, 42);
            lblMsgStep1.Location = new Point(138, startY - 2);
            pnlStep1_Identity.Controls.Add(lblMsgStep1);
            startY += 52;

            // 操作按钮
            Button btnNext = new Button();
            btnNext.Text = "下一步验证";
            StylePrimaryButton(btnNext, _themePrimary, 230, startY, 100, 40);
            btnNext.Click += BtnNextStep_Click;
            pnlStep1_Identity.Controls.Add(btnNext);

            Button btnCancel = new Button();
            btnCancel.Text = "取消";
            StylePrimaryButton(btnCancel, Color.FromArgb(100, 116, 139), 342, startY, 96, 40);
            btnCancel.Click += BtnCancel_Click;
            pnlStep1_Identity.Controls.Add(btnCancel);
        }

        /// <summary>
        /// 步骤2：密码重置控件初始化
        /// </summary>
        private void InitStep2Controls()
        {
            int labelWidth = 88;
            int txtWidth = 300;
            int startY = 62;
            int gap = 52;

            // 1. 新密码输入框
            Label lblNewPwd = new Label();
            lblNewPwd.Text = "新密码：";
            lblNewPwd.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            lblNewPwd.ForeColor = _themeText;
            lblNewPwd.AutoSize = false;
            lblNewPwd.Size = new Size(labelWidth, 25);
            lblNewPwd.Location = new Point(36, startY + 4);
            lblNewPwd.TextAlign = ContentAlignment.MiddleLeft;
            pnlStep2_ResetPwd.Controls.Add(lblNewPwd);

            txtNewPwd = new TextBox();
            txtNewPwd.Font = new Font("微软雅黑", 10.5F);
            txtNewPwd.Size = new Size(txtWidth, 32);
            txtNewPwd.Location = new Point(138, startY);
            txtNewPwd.PasswordChar = '*';
            txtNewPwd.BorderStyle = BorderStyle.FixedSingle;
            pnlStep2_ResetPwd.Controls.Add(txtNewPwd);
            startY += gap;

            // 2. 确认新密码输入框
            Label lblConfirmPwd = new Label();
            lblConfirmPwd.Text = "确认新密码：";
            lblConfirmPwd.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            lblConfirmPwd.ForeColor = _themeText;
            lblConfirmPwd.AutoSize = false;
            lblConfirmPwd.Size = new Size(labelWidth, 25);
            lblConfirmPwd.Location = new Point(36, startY + 4);
            lblConfirmPwd.TextAlign = ContentAlignment.MiddleLeft;
            pnlStep2_ResetPwd.Controls.Add(lblConfirmPwd);

            txtConfirmPwd = new TextBox();
            txtConfirmPwd.Font = new Font("微软雅黑", 10.5F);
            txtConfirmPwd.Size = new Size(txtWidth, 32);
            txtConfirmPwd.Location = new Point(138, startY);
            txtConfirmPwd.PasswordChar = '*';
            txtConfirmPwd.BorderStyle = BorderStyle.FixedSingle;
            pnlStep2_ResetPwd.Controls.Add(txtConfirmPwd);
            startY += gap;

            // 密码规则提示
            Label lblTip = new Label();
            lblTip.Text = "密码规则：长度不少于8位，需包含大小写字母、数字和特殊字符(!@#$%^&*()_+.-)";
            lblTip.Font = new Font("微软雅黑", 9F);
            lblTip.ForeColor = _themeMuted;
            lblTip.AutoSize = false;
            lblTip.Size = new Size(300, 40);
            lblTip.Location = new Point(138, startY - 2);
            pnlStep2_ResetPwd.Controls.Add(lblTip);
            startY += 46;

            // 错误提示标签
            lblMsgStep2 = new Label();
            lblMsgStep2.Font = new Font("微软雅黑", 9F);
            lblMsgStep2.ForeColor = _themeDanger;
            lblMsgStep2.AutoSize = false;
            lblMsgStep2.Size = new Size(300, 42);
            lblMsgStep2.Location = new Point(138, startY - 2);
            pnlStep2_ResetPwd.Controls.Add(lblMsgStep2);
            startY += 54;

            // 操作按钮
            Button btnPrev = new Button();
            btnPrev.Text = "上一步";
            StylePrimaryButton(btnPrev, Color.FromArgb(100, 116, 139), 230, startY, 100, 40);
            btnPrev.Click += BtnPrevStep_Click;
            pnlStep2_ResetPwd.Controls.Add(btnPrev);

            Button btnReset = new Button();
            btnReset.Text = "确认重置";
            StylePrimaryButton(btnReset, _themePrimary, 342, startY, 96, 40);
            btnReset.Click += BtnConfirmReset_Click;
            pnlStep2_ResetPwd.Controls.Add(btnReset);
        }

        /// <summary>
        /// 步骤3：重置完成控件初始化
        /// </summary>
        private void InitStep3Controls()
        {
            // 成功提示
            Label lblSuccess = new Label();
            lblSuccess.Text = "密码重置成功！";
            lblSuccess.Font = new Font("微软雅黑", 18F, FontStyle.Bold);
            lblSuccess.ForeColor = Color.FromArgb(40, 167, 69);
            lblSuccess.AutoSize = false;
            lblSuccess.Size = new Size(320, 40);
            lblSuccess.Location = new Point(90, 72);
            lblSuccess.TextAlign = ContentAlignment.MiddleCenter;
            pnlStep3_Success.Controls.Add(lblSuccess);

            Label lblTip = new Label();
            lblTip.Text = "请返回登录页面，使用新密码进行登录";
            lblTip.Font = new Font("微软雅黑", 12F);
            lblTip.ForeColor = _themeMuted;
            lblTip.AutoSize = false;
            lblTip.Size = new Size(320, 32);
            lblTip.Location = new Point(90, 126);
            lblTip.TextAlign = ContentAlignment.MiddleCenter;
            pnlStep3_Success.Controls.Add(lblTip);

            // 返回登录按钮
            Button btnBackLogin = new Button();
            btnBackLogin.Text = "返回登录页";
            StylePrimaryButton(btnBackLogin, _themePrimary, 160, 218, 180, 42);
            btnBackLogin.Click += BtnBackLogin_Click;
            pnlStep3_Success.Controls.Add(btnBackLogin);
        }

        private void StylePrimaryButton(Button button, Color backColor, int x, int y, int width, int height)
        {
            button.Location = new Point(x, y);
            button.Size = new Size(width, height);
            button.BackColor = backColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
        }
        #endregion

        #region 按钮点击事件处理
        /// <summary>
        /// 步骤1：下一步，身份校验
        /// </summary>
        private void BtnNextStep_Click(object sender, EventArgs e)
        {
            lblMsgStep1.Text = string.Empty;
            string account = txtAccount.Text.Trim();
            string userName = txtUserName.Text.Trim();
            string idCard = txtIdCard.Text.Trim();

            // 调用BLL层身份校验方法
            string checkMsg;
            int userId = bllUser.CheckDoctorIdentity(account, userName, idCard, out checkMsg);

            // 校验失败
            if (userId <= 0)
            {
                lblMsgStep1.Text = checkMsg;
                return;
            }

            // 校验通过，保存用户信息，切换到密码重置步骤
            _currentUserId = userId;
            _originalEncryptPwd = checkMsg;
            pnlStep1_Identity.Visible = false;
            pnlStep2_ResetPwd.Visible = true;
            txtNewPwd.Focus();
        }

        /// <summary>
        /// 步骤2：返回上一步
        /// </summary>
        private void BtnPrevStep_Click(object sender, EventArgs e)
        {
            // 清空输入内容与错误提示
            lblMsgStep2.Text = string.Empty;
            txtNewPwd.Clear();
            txtConfirmPwd.Clear();
            // 切换回身份校验步骤
            pnlStep2_ResetPwd.Visible = false;
            pnlStep1_Identity.Visible = true;
        }

        /// <summary>
        /// 步骤2：确认重置密码
        /// </summary>
        private void BtnConfirmReset_Click(object sender, EventArgs e)
        {
            lblMsgStep2.Text = string.Empty;
            string newPwd = txtNewPwd.Text.Trim();
            string confirmPwd = txtConfirmPwd.Text.Trim();

            // 基础校验
            if (string.IsNullOrEmpty(newPwd))
            {
                lblMsgStep2.Text = "请输入新密码";
                return;
            }
            if (string.IsNullOrEmpty(confirmPwd))
            {
                lblMsgStep2.Text = "请确认新密码";
                return;
            }
            if (!newPwd.Equals(confirmPwd))
            {
                lblMsgStep2.Text = "两次输入的密码不一致，请重新输入";
                return;
            }

            // 调用BLL层密码重置方法
            string resetResult = bllUser.DoctorResetPassword(_currentUserId, newPwd, _originalEncryptPwd);
            if (resetResult != "ok")
            {
                lblMsgStep2.Text = resetResult;
                return;
            }

            // 重置成功，切换到完成页面
            pnlStep2_ResetPwd.Visible = false;
            pnlStep3_Success.Visible = true;
        }

        /// <summary>
        /// 取消按钮，关闭窗体
        /// </summary>
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 返回登录页，关闭窗体
        /// </summary>
        private void BtnBackLogin_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}