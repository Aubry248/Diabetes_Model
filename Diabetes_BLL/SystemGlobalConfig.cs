using System;

namespace BLL
{
    /// <summary>
    /// 系统全局配置类（统一管理密码策略、账号安全规则）
    /// </summary>
    public static class SystemGlobalConfig
    {
        #region 账号安全配置
        /// <summary>
        /// 登录失败最大次数，超过锁定账号
        /// </summary>
        public static int LoginFailMaxTimes { get; set; } = 5;

        /// <summary>
        /// 账号锁定时长（分钟）
        /// </summary>
        public static int AccountLockMinutes { get; set; } = 30;
        #endregion

        #region 密码策略配置
        /// <summary>
        /// 密码最小长度
        /// </summary>
        public static int PwdMinLength { get; set; } = 8;

        /// <summary>
        /// 密码是否必须包含大小写字母
        /// </summary>
        public static bool PwdRequireUpperLower { get; set; } = true;

        /// <summary>
        /// 密码是否必须包含数字
        /// </summary>
        public static bool PwdRequireNumber { get; set; } = true;

        /// <summary>
        /// 密码是否必须包含特殊字符
        /// </summary>
        public static bool PwdRequireSpecialChar { get; set; } = true;

        /// <summary>
        /// 密码强制修改周期（天），0=不强制过期
        /// </summary>
        public static int PwdForceChangeCycleDays { get; set; } = 90;
        #endregion
    }
}