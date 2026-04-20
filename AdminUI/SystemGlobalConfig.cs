using System;
using System.Collections.Generic;
using BLL;

namespace AdminUI
{
    /// <summary>
    /// 系统全局配置静态类（全系统可直接调用，配置修改后即时更新）
    /// </summary>
    public static class SystemGlobalConfig
    {
        #region 配置字典存储
        private static readonly Dictionary<string, string> _configDict = new Dictionary<string, string>();
        private static readonly object _lockObj = new object();
        #endregion

        #region 基础系统配置
        /// <summary> 系统名称 </summary>
        public static string SystemName => GetConfigValue("System_Name", "糖尿病患者综合健康管理系统");
        /// <summary> 系统版本号 </summary>
        public static string SystemVersion => GetConfigValue("System_Version", "V1.0.0");
        /// <summary> 版权信息 </summary>
        public static string SystemCopyright => GetConfigValue("System_Copyright", "© 2026 糖尿病健康管理平台 版权所有");
        /// <summary> 系统Logo路径 </summary>
        public static string SystemLogoPath => GetConfigValue("System_LogoPath", "");
        /// <summary> 分页默认条数 </summary>
        public static int PageDefaultSize => GetConfigIntValue("Page_DefaultSize", 15);
        /// <summary> 会话超时时间(分钟) </summary>
        public static int SessionTimeoutMinutes => GetConfigIntValue("Session_TimeoutMinutes", 30);
        /// <summary> 操作日志保留天数 </summary>
        public static int LogRetainDays => GetConfigIntValue("Log_RetainDays", 90);
        /// <summary> 备份文件默认保留天数 </summary>
        public static int BackupDefaultRetainDays => GetConfigIntValue("Backup_DefaultRetainDays", 30);
        #endregion

        #region 糖尿病业务参数配置（符合2024版糖尿病防治指南）
        /// <summary> 空腹血糖正常下限(mmol/L) </summary>
        public static decimal FastingGlucoseNormalMin => GetConfigDecimalValue("Glucose_FastingNormalMin", 3.9m);
        /// <summary> 空腹血糖正常上限(mmol/L) </summary>
        public static decimal FastingGlucoseNormalMax => GetConfigDecimalValue("Glucose_FastingNormalMax", 6.1m);
        /// <summary> 餐后2小时血糖正常上限(mmol/L) </summary>
        public static decimal PostprandialGlucoseNormalMax => GetConfigDecimalValue("Glucose_PostprandialNormalMax", 7.8m);
        /// <summary> 低血糖紧急阈值(mmol/L) </summary>
        public static decimal HypoglycemiaEmergencyThreshold => GetConfigDecimalValue("Glucose_HypoglycemiaThreshold", 3.9m);
        /// <summary> 高血糖紧急阈值(mmol/L) </summary>
        public static decimal HyperglycemiaEmergencyThreshold => GetConfigDecimalValue("Glucose_HyperglycemiaThreshold", 16.7m);
        /// <summary> 普通患者默认随访周期(天) </summary>
        public static int NormalPatientFollowUpCycle => GetConfigIntValue("FollowUp_NormalPatientCycle", 30);
        /// <summary> 高危患者默认随访周期(天) </summary>
        public static int HighRiskPatientFollowUpCycle => GetConfigIntValue("FollowUp_HighRiskPatientCycle", 7);
        /// <summary> 体检报告有效期(天) </summary>
        public static int MedicalReportValidDays => GetConfigIntValue("MedicalReport_ValidDays", 180);
        /// <summary> 用药提醒提前时长(分钟) </summary>
        public static int MedicineRemindAdvanceMinutes => GetConfigIntValue("Remind_MedicineAdvanceMinutes", 30);
        /// <summary> 血糖监测提醒提前时长(分钟) </summary>
        public static int GlucoseRemindAdvanceMinutes => GetConfigIntValue("Remind_GlucoseAdvanceMinutes", 15);
        /// <summary> 随访提醒提前天数 </summary>
        public static int FollowUpRemindAdvanceDays => GetConfigIntValue("Remind_FollowUpAdvanceDays", 3);
        /// <summary> 是否启用系统默认饮食模板 </summary>
        public static bool EnableDefaultDietTemplate => GetConfigBoolValue("Template_EnableDefaultDiet", true);
        /// <summary> 是否启用系统默认运动模板 </summary>
        public static bool EnableDefaultExerciseTemplate => GetConfigBoolValue("Template_EnableDefaultExercise", true);
        #endregion

        #region 安全权限配置
        /// <summary> 首次登录是否强制改密 </summary>
        public static bool FirstLoginForceChangePwd => GetConfigBoolValue("Security_FirstLoginForceChangePwd", true);
        /// <summary> 是否允许账号多地登录 </summary>
        public static bool AllowMultiPlaceLogin => GetConfigBoolValue("Security_AllowMultiPlaceLogin", false);
        /// <summary> 登录失败最大次数 </summary>
        public static int LoginFailMaxTimes => GetConfigIntValue("Security_LoginFailMaxTimes", 5);
        /// <summary> 账号锁定时长(分钟) </summary>
        public static int AccountLockMinutes => GetConfigIntValue("Security_AccountLockMinutes", 30);
        /// <summary> 无操作自动退出时长(分钟) </summary>
        public static int NoOperationAutoLogoutMinutes => GetConfigIntValue("Security_NoOperationAutoLogoutMinutes", 15);
        /// <summary> 是否开启操作日志全记录 </summary>
        public static bool EnableFullOperationLog => GetConfigBoolValue("Security_EnableFullOperationLog", true);
        #endregion

        #region 密码策略配置
        /// <summary> 密码最小长度 </summary>
        public static int PwdMinLength => GetConfigIntValue("PwdPolicy_MinLength", 8);
        /// <summary> 是否必须包含大小写字母 </summary>
        public static bool PwdRequireUpperLower => GetConfigBoolValue("PwdPolicy_RequireUpperLower", true);
        /// <summary> 是否必须包含数字 </summary>
        public static bool PwdRequireNumber => GetConfigBoolValue("PwdPolicy_RequireNumber", true);
        /// <summary> 是否必须包含特殊字符 </summary>
        public static bool PwdRequireSpecialChar => GetConfigBoolValue("PwdPolicy_RequireSpecialChar", false);
        /// <summary> 密码强制更换周期(天) </summary>
        public static int PwdForceChangeCycleDays => GetConfigIntValue("PwdPolicy_ForceChangeCycleDays", 90);
        /// <summary> 历史密码禁止重复次数 </summary>
        public static int PwdHistoryForbidRepeatCount => GetConfigIntValue("PwdPolicy_HistoryForbidRepeatCount", 3);
        #endregion

        #region 核心读写方法
        /// <summary>
        /// 从数据库加载所有配置到内存（系统启动时调用）
        /// </summary>
        public static void LoadAllConfig()
        {
            lock (_lockObj)
            {
                _configDict.Clear();
                var bll = new B_SystemConfig();
                var allConfig = bll.GetAllConfigDict();
                foreach (var item in allConfig)
                {
                    _configDict[item.Key] = item.Value;
                }
            }
        }

        /// <summary>
        /// 刷新内存中的配置（保存配置后调用）
        /// </summary>
        public static void RefreshConfig()
        {
            LoadAllConfig();
        }

        /// <summary>
        /// 获取字符串类型配置值
        /// </summary>
        private static string GetConfigValue(string key, string defaultValue)
        {
            lock (_lockObj)
            {
                return _configDict.TryGetValue(key, out string value) ? value : defaultValue;
            }
        }

        /// <summary>
        /// 获取整型配置值
        /// </summary>
        private static int GetConfigIntValue(string key, int defaultValue)
        {
            string value = GetConfigValue(key, defaultValue.ToString());
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        /// <summary>
        /// 获取小数型配置值
        /// </summary>
        private static decimal GetConfigDecimalValue(string key, decimal defaultValue)
        {
            string value = GetConfigValue(key, defaultValue.ToString());
            return decimal.TryParse(value, out decimal result) ? result : defaultValue;
        }

        /// <summary>
        /// 获取布尔型配置值
        /// </summary>
        private static bool GetConfigBoolValue(string key, bool defaultValue)
        {
            string value = GetConfigValue(key, defaultValue ? "1" : "0");
            return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        #endregion
    }
}