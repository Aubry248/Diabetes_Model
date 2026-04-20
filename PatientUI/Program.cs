using System;
using System.Drawing;
using System.Windows.Forms;
using Model;

namespace PatientUI
{
    static class Program
    {
        /// <summary>
        /// 患者端全局登录用户信息
        /// </summary>
        public static Users LoginUser { get; set; }

        public static float PatientFontScale { get; set; } = 1f;

        public static bool PatientHighContrastMode { get; set; } = false;

        public static float ScaleFont(float baseSize)
        {
            return Math.Max(8f, baseSize * PatientFontScale);
        }
        // 打开 Program.cs，在 Program 类里加上这一行
        public static bool IsHighContrastMode { get; set; }
        public static Color GetMainBackColor()
        {
            return PatientHighContrastMode ? Color.FromArgb(24, 24, 27) : Color.White;
        }

        public static Color GetPanelBackColor()
        {
            return PatientHighContrastMode ? Color.FromArgb(39, 39, 42) : Color.White;
        }

        public static Color GetPrimaryColor()
        {
            return PatientHighContrastMode ? Color.FromArgb(245, 158, 11) : Color.FromArgb(0, 122, 204);
        }

        public static Color GetMenuBackColor()
        {
            return PatientHighContrastMode ? Color.FromArgb(24, 24, 27) : Color.FromArgb(248, 250, 252);
        }

        public static Color GetMenuHoverColor()
        {
            return PatientHighContrastMode ? Color.FromArgb(63, 63, 70) : Color.FromArgb(230, 240, 255);
        }

        public static Color GetTextColor()
        {
            return PatientHighContrastMode ? Color.White : Color.Black;
        }

        /// <summary>
        /// 应用程序的主入口点
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmPatientLogin());
        }
    }
}