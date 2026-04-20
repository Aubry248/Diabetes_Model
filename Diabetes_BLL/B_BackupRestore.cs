using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Model;
using Tools;

namespace BLL
{
    /// <summary>
    /// 数据备份还原业务层
    /// </summary>
    public class B_BackupRestore
    {
        // 数据库名称（和你的数据库名完全一致）
        private readonly string _dbName = "DB_DiabetesHealthManagement";
        private readonly B_AccessLog _bllAccess = new B_AccessLog();

        #region 核心备份方法
        /// <summary>
        /// 执行数据库备份
        /// </summary>
        /// <param name="backupPath">备份文件完整路径</param>
        /// <param name="backupType">备份类型：手动备份/自动备份</param>
        /// <param name="operatorId">操作人ID</param>
        /// <param name="operatorRoleId">操作人角色ID</param>
        /// <param name="clientIP">操作人IP</param>
        /// <param name="errorMsg">输出错误信息</param>
        /// <returns>是否备份成功</returns>
        public bool ExecuteBackup(string backupPath, string backupType, int operatorId, int operatorRoleId, string clientIP, out string errorMsg)
        {
            errorMsg = string.Empty;
            string backupDir = Path.GetDirectoryName(backupPath);

            try
            {
                // 1. 校验路径，不存在则创建
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                // 2. 执行SQL Server备份命令（带压缩+校验和）
                string backupSql = $@"BACKUP DATABASE {_dbName} TO DISK = @backupPath WITH INIT, CHECKSUM, COMPRESSION;";
                SqlHelper.ExecuteNonQuery(backupSql,
                    new System.Data.SqlClient.SqlParameter("@backupPath", backupPath));

                // 3. 校验备份文件是否生成
                if (!File.Exists(backupPath))
                {
                    errorMsg = "备份失败，未生成备份文件";
                    WriteBackupLog(backupPath, backupType, 0, operatorId, errorMsg);
                    return false;
                }

                FileInfo fileInfo = new FileInfo(backupPath);
                long fileSize = fileInfo.Length;
                if (fileSize < 1024)
                {
                    errorMsg = "备份失败，备份文件大小异常";
                    WriteBackupLog(backupPath, backupType, 0, operatorId, errorMsg);
                    return false;
                }

                // 4. 计算文件MD5校验和，防篡改
                string checksum = CalculateFileMD5(backupPath);

                // 5. 写入备份记录到数据库
                WriteBackupLog(backupPath, backupType, 1, operatorId, "备份成功", fileSize, checksum);

                // 6. 写入操作审计日志（参数传递，无Program依赖）
                _bllAccess.WriteOperateLog(
                    userId: operatorId,
                    roleId: operatorRoleId,
                    module: "系统运维管理",
                    action: "数据备份",
                    tableName: "t_backup_log",
                    recordId: 0,
                    oldValue: "",
                    newValue: $"执行{backupType}，备份路径：{backupPath}",
                    status: 1,
                    ip: clientIP,
                    device: "PC端");

                return true;
            }
            catch (Exception ex)
            {
                errorMsg = $"备份失败：{ex.Message}";
                WriteBackupLog(backupPath, backupType, 0, operatorId, errorMsg);
                return false;
            }
        }

        /// <summary>
        /// 执行数据库还原
        /// </summary>
        /// <param name="backupId">备份记录ID</param>
        /// <param name="operatorId">操作人ID</param>
        /// <param name="operatorRoleId">操作人角色ID</param>
        /// <param name="clientIP">操作人IP</param>
        /// <param name="errorMsg">输出错误信息</param>
        /// <returns>是否还原成功</returns>
        public bool ExecuteRestore(int backupId, int operatorId, int operatorRoleId, string clientIP, out string errorMsg)
        {
            errorMsg = string.Empty;
            // 1. 获取备份记录
            BackupLog log = GetBackupLogById(backupId);
            if (log == null)
            {
                errorMsg = "备份记录不存在";
                return false;
            }
            if (log.backup_status != 1)
            {
                errorMsg = "该备份文件备份失败，无法还原";
                return false;
            }
            if (!File.Exists(log.backup_path))
            {
                errorMsg = "备份文件不存在，无法还原";
                return false;
            }

            try
            {
                // 2. 还原前强制备份当前数据库，防止误操作
                string tempBackupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup", $"Restore_Before_{DateTime.Now:yyyyMMddHHmmss}.bak");
                ExecuteBackup(tempBackupPath, "还原前自动备份", operatorId, operatorRoleId, clientIP, out string tempError);

                // 3. 执行还原命令（切换master库，单用户模式防连接占用）
                string restoreSql = $@"
                USE master;
                ALTER DATABASE {_dbName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                RESTORE DATABASE {_dbName} FROM DISK = @backupPath WITH REPLACE, RECOVERY;
                ALTER DATABASE {_dbName} SET MULTI_USER;";

                SqlHelper.ExecuteNonQuery(restoreSql,
                    new System.Data.SqlClient.SqlParameter("@backupPath", log.backup_path));

                // 4. 更新还原记录
                UpdateRestoreLog(backupId, 1, operatorId, "还原成功");

                // 5. 写入操作审计日志
                _bllAccess.WriteOperateLog(
                    userId: operatorId,
                    roleId: operatorRoleId,
                    module: "系统运维管理",
                    action: "数据还原",
                    tableName: "t_backup_log",
                    recordId: backupId,
                    oldValue: "",
                    newValue: $"从备份文件还原：{log.backup_path}",
                    status: 1,
                    ip: clientIP,
                    device: "PC端");

                return true;
            }
            catch (Exception ex)
            {
                errorMsg = $"还原失败：{ex.Message}";
                UpdateRestoreLog(backupId, 0, operatorId, errorMsg);
                // 还原失败时强制恢复多用户模式
                try
                {
                    SqlHelper.ExecuteNonQuery($"USE master; ALTER DATABASE {_dbName} SET MULTI_USER;");
                }
                catch { }
                return false;
            }
        }
        #endregion

        #region 备份记录管理
        /// <summary>
        /// 分页查询备份记录
        /// </summary>
        public List<BackupLog> GetBackupLogByPage(DateTime startTime, DateTime endTime, string backupType, int pageIndex, int pageSize, out int totalCount)
        {
            string sqlCount = @"SELECT COUNT(1) FROM t_backup_log WHERE backup_time BETWEEN @startTime AND @endTime";
            string sqlData = @"SELECT b.*, u1.user_name as backup_user_name, u2.user_name as restore_user_name 
                                FROM t_backup_log b
                                LEFT JOIN t_user u1 ON b.backup_by = u1.user_id
                                LEFT JOIN t_user u2 ON b.restore_by = u2.user_id
                                WHERE b.backup_time BETWEEN @startTime AND @endTime";

            List<System.Data.SqlClient.SqlParameter> parameters = new List<System.Data.SqlClient.SqlParameter>
            {
                new System.Data.SqlClient.SqlParameter("@startTime", startTime),
                new System.Data.SqlClient.SqlParameter("@endTime", endTime)
            };

            if (!string.IsNullOrEmpty(backupType) && backupType != "全部")
            {
                sqlCount += " AND backup_type = @backupType";
                sqlData += " AND backup_type = @backupType";
                parameters.Add(new System.Data.SqlClient.SqlParameter("@backupType", backupType));
            }

            // 统计总条数（参数分离，避免重复使用）
            var baseParams = parameters.ToArray();
            totalCount = Convert.ToInt32(SqlHelper.ExecuteScalar(sqlCount, baseParams));

            // 分页查询
            sqlData += " ORDER BY b.backup_time DESC OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";
            var dataParams = new List<System.Data.SqlClient.SqlParameter>(baseParams);
            dataParams.Add(new System.Data.SqlClient.SqlParameter("@offset", (pageIndex - 1) * pageSize));
            dataParams.Add(new System.Data.SqlClient.SqlParameter("@pageSize", pageSize));

            DataTable dt = SqlHelper.ExecuteDataTable(sqlData, dataParams.ToArray());
            List<BackupLog> list = new List<BackupLog>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new BackupLog
                {
                    backup_id = Convert.ToInt32(dr["backup_id"]),
                    backup_type = dr["backup_type"].ToString(),
                    backup_path = dr["backup_path"].ToString(),
                    backup_size = dr["backup_size"] != DBNull.Value ? Convert.ToInt64(dr["backup_size"]) : 0,
                    backup_checksum = dr["backup_checksum"]?.ToString(),
                    backup_status = Convert.ToInt32(dr["backup_status"]),
                    backup_remark = dr["backup_remark"]?.ToString(),
                    backup_time = Convert.ToDateTime(dr["backup_time"]),
                    backup_by = Convert.ToInt32(dr["backup_by"]),
                    restore_status = Convert.ToInt32(dr["restore_status"]),
                    // 🔴 修复CS8957：显式强转为可空类型
                    restore_by = dr["restore_by"] != DBNull.Value ? (int?)Convert.ToInt32(dr["restore_by"]) : null,
                    restore_time = dr["restore_time"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(dr["restore_time"]) : null,
                    restore_remark = dr["restore_remark"]?.ToString(),
                    backup_user_name = dr["backup_user_name"]?.ToString(),
                    restore_user_name = dr["restore_user_name"]?.ToString()
                });
            }
            return list;
        }

        /// <summary>
        /// 根据ID获取备份记录
        /// </summary>
        public BackupLog GetBackupLogById(int backupId)
        {
            string sql = @"SELECT b.*, u1.user_name as backup_user_name, u2.user_name as restore_user_name 
                            FROM t_backup_log b
                            LEFT JOIN t_user u1 ON b.backup_by = u1.user_id
                            LEFT JOIN t_user u2 ON b.restore_by = u2.user_id
                            WHERE b.backup_id = @backupId";
            DataTable dt = SqlHelper.ExecuteDataTable(sql, new System.Data.SqlClient.SqlParameter("@backupId", backupId));
            if (dt == null || dt.Rows.Count == 0) return null;

            DataRow dr = dt.Rows[0];
            return new BackupLog
            {
                backup_id = Convert.ToInt32(dr["backup_id"]),
                backup_type = dr["backup_type"].ToString(),
                backup_path = dr["backup_path"].ToString(),
                backup_size = dr["backup_size"] != DBNull.Value ? Convert.ToInt64(dr["backup_size"]) : 0,
                backup_checksum = dr["backup_checksum"]?.ToString(),
                backup_status = Convert.ToInt32(dr["backup_status"]),
                backup_remark = dr["backup_remark"]?.ToString(),
                backup_time = Convert.ToDateTime(dr["backup_time"]),
                backup_by = Convert.ToInt32(dr["backup_by"]),
                restore_status = Convert.ToInt32(dr["restore_status"]),
                // 🔴 修复CS8957：显式强转为可空类型
                restore_by = dr["restore_by"] != DBNull.Value ? (int?)Convert.ToInt32(dr["restore_by"]) : null,
                restore_time = dr["restore_time"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(dr["restore_time"]) : null,
                restore_remark = dr["restore_remark"]?.ToString(),
                backup_user_name = dr["backup_user_name"]?.ToString(),
                restore_user_name = dr["restore_user_name"]?.ToString()
            };
        }

        /// <summary>
        /// 删除备份记录（同时删除本地文件）
        /// </summary>
        public bool DeleteBackupLog(int backupId, int operatorId, int operatorRoleId, string clientIP, out string errorMsg)
        {
            errorMsg = string.Empty;
            try
            {
                BackupLog log = GetBackupLogById(backupId);
                if (log == null)
                {
                    errorMsg = "备份记录不存在";
                    return false;
                }

                // 删除本地备份文件
                if (File.Exists(log.backup_path))
                {
                    File.Delete(log.backup_path);
                }

                // 删除数据库记录
                string sql = "DELETE FROM t_backup_log WHERE backup_id = @backupId";
                SqlHelper.ExecuteNonQuery(sql, new System.Data.SqlClient.SqlParameter("@backupId", backupId));

                // 写入审计日志
                _bllAccess.WriteOperateLog(
                    userId: operatorId,
                    roleId: operatorRoleId,
                    module: "系统运维管理",
                    action: "删除备份",
                    tableName: "t_backup_log",
                    recordId: backupId,
                    oldValue: $"备份文件：{log.backup_path}",
                    newValue: "",
                    status: 1,
                    ip: clientIP,
                    device: "PC端");

                return true;
            }
            catch (Exception ex)
            {
                errorMsg = $"删除失败：{ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// 清理过期备份
        /// </summary>
        public int ClearExpiredBackup(int retainDays, out string errorMsg)
        {
            errorMsg = string.Empty;
            int deleteCount = 0;
            try
            {
                DateTime expireTime = DateTime.Now.AddDays(-retainDays);
                string sql = "SELECT backup_id, backup_path FROM t_backup_log WHERE backup_time < @expireTime AND backup_status = 1";
                DataTable dt = SqlHelper.ExecuteDataTable(sql, new System.Data.SqlClient.SqlParameter("@expireTime", expireTime));

                foreach (DataRow dr in dt.Rows)
                {
                    int backupId = Convert.ToInt32(dr["backup_id"]);
                    string path = dr["backup_path"].ToString();

                    // 删除文件
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    // 删除记录
                    SqlHelper.ExecuteNonQuery("DELETE FROM t_backup_log WHERE backup_id = @backupId",
                        new System.Data.SqlClient.SqlParameter("@backupId", backupId));
                    deleteCount++;
                }
                return deleteCount;
            }
            catch (Exception ex)
            {
                errorMsg = $"清理过期备份失败：{ex.Message}";
                return deleteCount;
            }
        }
        #endregion

        #region 自动备份配置管理
        /// <summary>
        /// 保存自动备份配置
        /// </summary>
        /// <param name="enable">是否启用自动备份</param>
        /// <param name="cycle">备份周期</param>
        /// <param name="backupTime">备份时间</param>
        /// <param name="backupPath">备份路径</param>
        /// <param name="retainDays">保留天数</param>
        /// <param name="operatorId">操作人ID</param>
        /// <param name="errorMsg">输出错误信息</param>
        /// <returns>是否保存成功</returns>
        public bool SaveAutoBackupConfig(bool enable, string cycle, string backupTime, string backupPath, int retainDays, int operatorId, out string errorMsg)
        {
            errorMsg = string.Empty;
            try
            {
                // 配置项字典
                Dictionary<string, string> configs = new Dictionary<string, string>
                {
                    { "AutoBackup_Enable", enable ? "1" : "0" },
                    { "AutoBackup_Cycle", cycle },
                    { "AutoBackup_Time", backupTime },
                    { "AutoBackup_Path", backupPath },
                    { "AutoBackup_RetainDays", retainDays.ToString() }
                };

                foreach (var item in configs)
                {
                    // 先判断是否存在，存在则更新，不存在则插入
                    string checkSql = "SELECT COUNT(1) FROM t_system_config WHERE config_key = @configKey";
                    int count = Convert.ToInt32(SqlHelper.ExecuteScalar(checkSql,
                        new System.Data.SqlClient.SqlParameter("@configKey", item.Key)));

                    if (count > 0)
                    {
                        string updateSql = @"UPDATE t_system_config SET config_value = @configValue, update_time = GETDATE(), update_by = @userId 
                                            WHERE config_key = @configKey";
                        SqlHelper.ExecuteNonQuery(updateSql,
                            new System.Data.SqlClient.SqlParameter("@configKey", item.Key),
                            new System.Data.SqlClient.SqlParameter("@configValue", item.Value),
                            new System.Data.SqlClient.SqlParameter("@userId", operatorId));
                    }
                    else
                    {
                        string insertSql = @"INSERT INTO t_system_config(config_type, config_key, config_value, config_desc, create_by, update_by, status)
                                            VALUES('备份配置', @configKey, @configValue, '自动备份配置', @userId, @userId, 1)";
                        SqlHelper.ExecuteNonQuery(insertSql,
                            new System.Data.SqlClient.SqlParameter("@configKey", item.Key),
                            new System.Data.SqlClient.SqlParameter("@configValue", item.Value),
                            new System.Data.SqlClient.SqlParameter("@userId", operatorId));
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                errorMsg = $"保存配置失败：{ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// 获取自动备份配置
        /// </summary>
        public Dictionary<string, string> GetAutoBackupConfig()
        {
            // 默认配置
            Dictionary<string, string> configs = new Dictionary<string, string>
            {
                { "AutoBackup_Enable", "0" },
                { "AutoBackup_Cycle", "每天" },
                { "AutoBackup_Time", "02:00:00" },
                { "AutoBackup_Path", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup") },
                { "AutoBackup_RetainDays", "30" }
            };

            try
            {
                string sql = "SELECT config_key, config_value FROM t_system_config WHERE config_type = '备份配置'";
                DataTable dt = SqlHelper.ExecuteDataTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    string key = dr["config_key"].ToString();
                    if (configs.ContainsKey(key))
                    {
                        configs[key] = dr["config_value"].ToString();
                    }
                }
            }
            catch { }
            return configs;
        }
        #endregion

        #region 私有辅助方法
        /// <summary>
        /// 写入备份记录
        /// </summary>
        private void WriteBackupLog(string backupPath, string backupType, int status, int operatorId, string remark, long fileSize = 0, string checksum = "")
        {
            string sql = @"INSERT INTO t_backup_log(backup_type, backup_path, backup_size, backup_checksum, backup_status, backup_remark, backup_time, backup_by, restore_status, data_version, create_time, update_time)
                            VALUES(@backup_type, @backup_path, @backup_size, @backup_checksum, @backup_status, @backup_remark, GETDATE(), @backup_by, 0, 1, GETDATE(), GETDATE())";
            SqlHelper.ExecuteNonQuery(sql,
                new System.Data.SqlClient.SqlParameter("@backup_type", backupType),
                new System.Data.SqlClient.SqlParameter("@backup_path", backupPath),
                new System.Data.SqlClient.SqlParameter("@backup_size", fileSize),
                new System.Data.SqlClient.SqlParameter("@backup_checksum", checksum),
                new System.Data.SqlClient.SqlParameter("@backup_status", status),
                new System.Data.SqlClient.SqlParameter("@backup_remark", remark),
                new System.Data.SqlClient.SqlParameter("@backup_by", operatorId));
        }

        /// <summary>
        /// 更新还原记录
        /// </summary>
        private void UpdateRestoreLog(int backupId, int status, int operatorId, string remark)
        {
            string sql = @"UPDATE t_backup_log SET restore_status = @restore_status, restore_by = @restore_by, restore_time = GETDATE(), restore_remark = @restore_remark, update_time = GETDATE()
                            WHERE backup_id = @backupId";
            SqlHelper.ExecuteNonQuery(sql,
                new System.Data.SqlClient.SqlParameter("@backupId", backupId),
                new System.Data.SqlClient.SqlParameter("@restore_status", status),
                new System.Data.SqlClient.SqlParameter("@restore_by", operatorId),
                new System.Data.SqlClient.SqlParameter("@restore_remark", remark));
        }

        /// <summary>
        /// 计算文件MD5校验和
        /// </summary>
        private string CalculateFileMD5(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    MD5 md5 = MD5.Create();
                    byte[] hash = md5.ComputeHash(fs);
                    StringBuilder sb = new StringBuilder();
                    foreach (byte b in hash)
                    {
                        sb.Append(b.ToString("x2"));
                    }
                    return sb.ToString();
                }
            }
            catch
            {
                return "";
            }
        }
        #endregion
    }
}