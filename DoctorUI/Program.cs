using System;
using System.Windows.Forms;
using Model; // 必须引用Users实体

namespace DoctorUI
{
    internal static class Program
    {
     
        public static Users LoginUser { get; set; }

       
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // 启动医生端登录窗体
            Application.Run(new FrmDoctorLogin());
        }
    }
}