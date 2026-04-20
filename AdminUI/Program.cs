using System;
using System.Timers;
using System.Windows.Forms;
using Model;

namespace AdminUI
{
    static class Program
    {
        /// <summary>
        /// 全局登录用户信息（和Model层Users类完全对齐）
        /// </summary>
        public static Users LoginUser { get; set; }

        /// <summary>
        /// 客户端IP地址
        /// </summary>
        public static string ClientIP = "127.0.0.1";

        /// <summary>
        /// 全局自动备份定时器（防止GC回收）
        /// </summary>
        public static System.Timers.Timer GlobalAutoBackupTimer;

        /// <summary>
        /// 应用程序的主入口点
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // 启动登录窗体
            Application.Run(new FrmAdminLogin());
            // 🔴 新增：系统启动时加载所有全局配置
            SystemGlobalConfig.LoadAllConfig();
        }

    }
}