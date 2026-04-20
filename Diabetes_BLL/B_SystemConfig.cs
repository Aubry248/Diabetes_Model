using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Tools;

namespace BLL
{
    /// <summary>
    /// 系统配置业务逻辑层
    /// </summary>
    public class B_SystemConfig
    {
        #region 核心查询方法
        /// <summary>
        /// 获取所有配置的键值对字典
        /// </summary>
        public Dictionary<string, string> GetAllConfigDict()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            string sql = "SELECT config_key, config_value FROM t_system_config WHERE status = 1";
            DataTable dt = SqlHelper.ExecuteDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                string key = dr["config_key"].ToString().Trim();
                string value = dr["config_value"].ToString().Trim();
                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, value);
                }
            }
            return dict;
        }

        /// <summary>
        /// 按配置类型获取配置列表
        /// </summary>
        public DataTable GetConfigByType(string configType)
        {
            string sql = "SELECT * FROM t_system_config WHERE config_type = @configType AND status = 1";
            SqlParameter[] param = {
                new SqlParameter("@configType", configType)
            };
            return SqlHelper.ExecuteDataTable(sql, param);
        }

        /// <summary>
        /// 获取单个配置值
        /// </summary>
        public string GetConfigValue(string configKey)
        {
            string sql = "SELECT TOP 1 config_value FROM t_system_config WHERE config_key = @configKey AND status = 1";
            SqlParameter[] param = {
                new SqlParameter("@configKey", configKey)
            };
            object obj = SqlHelper.ExecuteScalar(sql, param);
            return obj?.ToString() ?? "";
        }
        #endregion

        #region 保存配置方法
        /// <summary>
        /// 批量保存配置（不存在则新增，存在则更新）
        /// </summary>
        public bool SaveConfigBatch(Dictionary<string, string> configDict, string configType, int operatorId, out string errorMsg)
        {
            errorMsg = "";
            try
            {
                List<string> sqlList = new List<string>();
                List<SqlParameter[]> paramList = new List<SqlParameter[]>();

                foreach (var item in configDict)
                {
                    // 先判断是否存在
                    string checkSql = "SELECT COUNT(1) FROM t_system_config WHERE config_key = @configKey";
                    SqlParameter[] checkParam = {
                        new SqlParameter("@configKey", item.Key)
                    };
                    int count = Convert.ToInt32(SqlHelper.ExecuteScalar(checkSql, checkParam));

                    if (count > 0)
                    {
                        // 更新
                        string updateSql = @"UPDATE t_system_config 
                                            SET config_value = @configValue, update_by = @updateBy, update_time = GETDATE(), data_version = data_version + 1
                                            WHERE config_key = @configKey";
                        SqlParameter[] updateParam = {
                            new SqlParameter("@configKey", item.Key),
                            new SqlParameter("@configValue", item.Value),
                            new SqlParameter("@updateBy", operatorId)
                        };
                        sqlList.Add(updateSql);
                        paramList.Add(updateParam);
                    }
                    else
                    {
                        // 新增
                        string insertSql = @"INSERT INTO t_system_config 
                                            (config_type, config_key, config_value, config_desc, create_by, update_by, data_version, create_time, update_time, status)
                                            VALUES (@configType, @configKey, @configValue, '', @createBy, @updateBy, 1, GETDATE(), GETDATE(), 1)";
                        SqlParameter[] insertParam = {
                            new SqlParameter("@configType", configType),
                            new SqlParameter("@configKey", item.Key),
                            new SqlParameter("@configValue", item.Value),
                            new SqlParameter("@createBy", operatorId),
                            new SqlParameter("@updateBy", operatorId)
                        };
                        sqlList.Add(insertSql);
                        paramList.Add(insertParam);
                    }
                }

                // 事务执行
                return SqlHelper.ExecuteTransaction(sqlList.ToArray(), paramList.ToArray());
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return false;
            }
        }
        #endregion

        #region 重置默认配置方法
        /// <summary>
        /// 重置指定类型的配置为默认值
        /// </summary>
        public bool ResetConfigByType(string configType, int operatorId, out string errorMsg)
        {
            errorMsg = "";
            try
            {
                // 先删除该类型的配置，再插入默认值
                string deleteSql = "DELETE FROM t_system_config WHERE config_type = @configType";
                SqlParameter[] deleteParam = {
                    new SqlParameter("@configType", configType)
                };
                SqlHelper.ExecuteNonQuery(deleteSql, deleteParam);

                // 插入对应类型的默认配置
                Dictionary<string, string> defaultConfig = GetDefaultConfigByType(configType);
                return SaveConfigBatch(defaultConfig, configType, operatorId, out errorMsg);
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 获取指定类型的默认配置
        /// </summary>
        private Dictionary<string, string> GetDefaultConfigByType(string configType)
        {
            Dictionary<string, string> defaultConfig = new Dictionary<string, string>();

            switch (configType)
            {
                case "基础配置":
                    defaultConfig.Add("System_Name", "糖尿病患者综合健康管理系统");
                    defaultConfig.Add("System_Version", "V1.0.0");
                    defaultConfig.Add("System_Copyright", "© 2026 糖尿病健康管理平台 版权所有");
                    defaultConfig.Add("System_LogoPath", "");
                    defaultConfig.Add("Page_DefaultSize", "15");
                    defaultConfig.Add("Session_TimeoutMinutes", "30");
                    defaultConfig.Add("Log_RetainDays", "90");
                    defaultConfig.Add("Backup_DefaultRetainDays", "30");
                    break;

                case "业务配置":
                    defaultConfig.Add("Glucose_FastingNormalMin", "3.9");
                    defaultConfig.Add("Glucose_FastingNormalMax", "6.1");
                    defaultConfig.Add("Glucose_PostprandialNormalMax", "7.8");
                    defaultConfig.Add("Glucose_HypoglycemiaThreshold", "3.9");
                    defaultConfig.Add("Glucose_HyperglycemiaThreshold", "16.7");
                    defaultConfig.Add("FollowUp_NormalPatientCycle", "30");
                    defaultConfig.Add("FollowUp_HighRiskPatientCycle", "7");
                    defaultConfig.Add("MedicalReport_ValidDays", "180");
                    defaultConfig.Add("Remind_MedicineAdvanceMinutes", "30");
                    defaultConfig.Add("Remind_GlucoseAdvanceMinutes", "15");
                    defaultConfig.Add("Remind_FollowUpAdvanceDays", "3");
                    defaultConfig.Add("Template_EnableDefaultDiet", "1");
                    defaultConfig.Add("Template_EnableDefaultExercise", "1");
                    break;

                case "安全配置":
                    defaultConfig.Add("Security_FirstLoginForceChangePwd", "1");
                    defaultConfig.Add("Security_AllowMultiPlaceLogin", "0");
                    defaultConfig.Add("Security_LoginFailMaxTimes", "5");
                    defaultConfig.Add("Security_AccountLockMinutes", "30");
                    defaultConfig.Add("Security_NoOperationAutoLogoutMinutes", "15");
                    defaultConfig.Add("Security_EnableFullOperationLog", "1");
                    break;

                case "密码策略":
                    defaultConfig.Add("PwdPolicy_MinLength", "8");
                    defaultConfig.Add("PwdPolicy_RequireUpperLower", "1");
                    defaultConfig.Add("PwdPolicy_RequireNumber", "1");
                    defaultConfig.Add("PwdPolicy_RequireSpecialChar", "0");
                    defaultConfig.Add("PwdPolicy_ForceChangeCycleDays", "90");
                    defaultConfig.Add("PwdPolicy_HistoryForbidRepeatCount", "3");
                    break;
            }

            return defaultConfig;
        }
        #endregion

        #region 审计日志写入方法
        /// <summary>
        /// 写入配置操作审计日志
        /// </summary>
        public void WriteAuditLog(int operatorId, string operateType, string operateContent, string operateIP)
        {
            try
            {
                string sql = @"INSERT INTO t_audit_log 
                                (operate_user_id, operate_type, operate_content, operate_ip, operate_device, operate_time, remark, data_version, create_time, update_time)
                                VALUES (@operateUserId, @operateType, @operateContent, @operateIP, 'PC端', GETDATE(), '', 1, GETDATE(), GETDATE())";
                SqlParameter[] param = {
                    new SqlParameter("@operateUserId", operatorId),
                    new SqlParameter("@operateType", operateType),
                    new SqlParameter("@operateContent", operateContent),
                    new SqlParameter("@operateIP", operateIP)
                };
                SqlHelper.ExecuteNonQuery(sql, param);
            }
            catch { /* 日志写入失败不影响主业务 */ }
        }
        #endregion
    }
}