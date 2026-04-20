using System;
namespace Tools
{
    /// <summary>
    /// 全局登录用户信息静态类
    /// </summary>
    public static class LoginUser
    {
        /// <summary>
        /// 当前登录用户ID
        /// </summary>
        public static int CurrentUserId { get; set; }

        /// <summary>
        /// 当前登录用户名
        /// </summary>
        public static string CurrentUserName { get; set; }

        /// <summary>
        /// 当前登录用户类型（1=患者，2=医生，3=管理员）
        /// </summary>
        public static int UserType { get; set; }
    }
}