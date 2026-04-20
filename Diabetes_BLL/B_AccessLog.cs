using System;
using System.Collections.Generic;
using System.Data;
using Model;
using Tools;

namespace BLL
{
    /// <summary>
    /// 操作访问日志业务层
    /// </summary>
    public class B_AccessLog
    {
        /// <summary>
        /// 分页查询操作日志
        /// </summary>
        public List<AccessLog> GetOperateLogByPage(DateTime startTime, DateTime endTime, string userName, int? roleId, int? status, int pageIndex, int pageSize, out int totalCount)
        {
            string sqlCount = @"SELECT COUNT(1) FROM t_access_log a
                                LEFT JOIN t_user u ON a.user_id = u.user_id
                                LEFT JOIN t_role r ON a.access_role_id = r.role_id
                                WHERE a.access_time BETWEEN @startTime AND @endTime";

            string sqlData = @"SELECT a.*, u.user_name, r.role_name FROM t_access_log a
                                LEFT JOIN t_user u ON a.user_id = u.user_id
                                LEFT JOIN t_role r ON a.access_role_id = r.role_id
                                WHERE a.access_time BETWEEN @startTime AND @endTime";

            List<System.Data.SqlClient.SqlParameter> parameters = new List<System.Data.SqlClient.SqlParameter>
            {
                new System.Data.SqlClient.SqlParameter("@startTime", startTime),
                new System.Data.SqlClient.SqlParameter("@endTime", endTime)
            };

            if (!string.IsNullOrEmpty(userName))
            {
                sqlCount += " AND u.user_name LIKE @userName";
                sqlData += " AND u.user_name LIKE @userName";
                parameters.Add(new System.Data.SqlClient.SqlParameter("@userName", $"%{userName}%"));
            }
            if (roleId.HasValue && roleId > 0)
            {
                sqlCount += " AND r.role_id = @roleId";
                sqlData += " AND r.role_id = @roleId";
                parameters.Add(new System.Data.SqlClient.SqlParameter("@roleId", roleId));
            }
            if (status.HasValue)
            {
                sqlCount += " AND a.access_status = @status";
                sqlData += " AND a.access_status = @status";
                parameters.Add(new System.Data.SqlClient.SqlParameter("@status", status));
            }

            // 获取总条数
            totalCount = Convert.ToInt32(Tools.SqlHelper.ExecuteScalar(sqlCount, parameters.ToArray()));

            // 分页SQL（SQL Server 2012+ 支持OFFSET FETCH）
            sqlData += " ORDER BY a.access_time DESC OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";
            parameters.Add(new System.Data.SqlClient.SqlParameter("@offset", (pageIndex - 1) * pageSize));
            parameters.Add(new System.Data.SqlClient.SqlParameter("@pageSize", pageSize));

            // 执行查询
            DataTable dt = Tools.SqlHelper.ExecuteDataTable(sqlData, parameters.ToArray());
            List<AccessLog> list = new List<AccessLog>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new AccessLog
                {
                    access_id = Convert.ToInt32(dr["access_id"]),
                    user_id = dr["user_id"] != DBNull.Value ? Convert.ToInt32(dr["user_id"]) : 0,
                    access_role_id = dr["access_role_id"] != DBNull.Value ? Convert.ToInt32(dr["access_role_id"]) : 0,
                    interface_module = dr["interface_module"].ToString(),
                    action = dr["action"].ToString(),
                    request_param = dr["request_param"]?.ToString(),
                    response_result = dr["response_result"]?.ToString(),
                    access_status = Convert.ToInt32(dr["access_status"]),
                    error_msg = dr["error_msg"]?.ToString(),
                    response_time = dr["response_time"] != DBNull.Value ? Convert.ToInt32(dr["response_time"]) : 0,
                    ip_address = dr["ip_address"].ToString(),
                    device_type = dr["device_type"].ToString(),
                    access_time = Convert.ToDateTime(dr["access_time"]),
                    is_sensitive_operation = dr["is_sensitive_operation"] != DBNull.Value && Convert.ToBoolean(dr["is_sensitive_operation"]),
                    data_sensitivity_level = dr["data_sensitivity_level"] != DBNull.Value ? Convert.ToInt32(dr["data_sensitivity_level"]) : 0,
                    table_name = dr["table_name"]?.ToString(),
                    record_id = dr["record_id"] != DBNull.Value ? Convert.ToInt32(dr["record_id"]) : 0,
                    old_value = dr["old_value"]?.ToString(),
                    new_value = dr["new_value"]?.ToString(),
                    user_name = dr["user_name"]?.ToString(),
                    role_name = dr["role_name"]?.ToString() ?? "未知"
                });
            }
            return list;
        }

        /// <summary>
        /// 根据ID获取单条操作日志详情（修复ExecuteDataRow报错）
        /// </summary>
        public AccessLog GetAccessLogById(int accessId)
        {
            string sql = @"SELECT a.*, u.user_name, r.role_name FROM t_access_log a
                            LEFT JOIN t_user u ON a.user_id = u.user_id
                            LEFT JOIN t_role r ON a.access_role_id = r.role_id
                            WHERE a.access_id = @accessId";
            // 修复：用ExecuteDataTable替代不存在的ExecuteDataRow
            DataTable dt = Tools.SqlHelper.ExecuteDataTable(sql, new System.Data.SqlClient.SqlParameter("@accessId", accessId));
            if (dt == null || dt.Rows.Count == 0) return null;

            DataRow dr = dt.Rows[0];
            return new AccessLog
            {
                access_id = Convert.ToInt32(dr["access_id"]),
                user_id = dr["user_id"] != DBNull.Value ? Convert.ToInt32(dr["user_id"]) : 0,
                access_role_id = dr["access_role_id"] != DBNull.Value ? Convert.ToInt32(dr["access_role_id"]) : 0,
                interface_module = dr["interface_module"].ToString(),
                action = dr["action"].ToString(),
                table_name = dr["table_name"]?.ToString(),
                record_id = dr["record_id"] != DBNull.Value ? Convert.ToInt32(dr["record_id"]) : 0,
                old_value = dr["old_value"]?.ToString(),
                new_value = dr["new_value"]?.ToString(),
                request_param = dr["request_param"]?.ToString(),
                response_result = dr["response_result"]?.ToString(),
                access_status = Convert.ToInt32(dr["access_status"]),
                error_msg = dr["error_msg"]?.ToString(),
                response_time = dr["response_time"] != DBNull.Value ? Convert.ToInt32(dr["response_time"]) : 0,
                ip_address = dr["ip_address"].ToString(),
                device_type = dr["device_type"].ToString(),
                access_time = Convert.ToDateTime(dr["access_time"]),
                is_sensitive_operation = dr["is_sensitive_operation"] != DBNull.Value && Convert.ToBoolean(dr["is_sensitive_operation"]),
                data_sensitivity_level = dr["data_sensitivity_level"] != DBNull.Value ? Convert.ToInt32(dr["data_sensitivity_level"]) : 0,
                user_name = dr["user_name"]?.ToString(),
                role_name = dr["role_name"]?.ToString() ?? "未知"
            };
        }

        /// <summary>
        /// 写入操作日志
        /// </summary>
        public int WriteOperateLog(int userId, int roleId, string module, string action, string tableName, int recordId, string oldValue, string newValue, int status, string ip, string device)
        {
            string sql = @"INSERT INTO t_access_log(user_id, access_role_id, interface_module, action, table_name, record_id, old_value, new_value, 
                            access_status, ip_address, device_type, access_time, data_version, create_time, update_time, is_sensitive_operation)
                            VALUES(@userId, @roleId, @module, @action, @tableName, @recordId, @oldValue, @newValue, @status, @ip, @device, GETDATE(), 1, GETDATE(), GETDATE(), 0)";
            return Tools.SqlHelper.ExecuteNonQuery(sql,
                new System.Data.SqlClient.SqlParameter("@userId", userId),
                new System.Data.SqlClient.SqlParameter("@roleId", roleId),
                new System.Data.SqlClient.SqlParameter("@module", module),
                new System.Data.SqlClient.SqlParameter("@action", action),
                new System.Data.SqlClient.SqlParameter("@tableName", tableName ?? ""),
                new System.Data.SqlClient.SqlParameter("@recordId", recordId),
                new System.Data.SqlClient.SqlParameter("@oldValue", oldValue ?? ""),
                new System.Data.SqlClient.SqlParameter("@newValue", newValue ?? ""),
                new System.Data.SqlClient.SqlParameter("@status", status),
                new System.Data.SqlClient.SqlParameter("@ip", ip),
                new System.Data.SqlClient.SqlParameter("@device", device));
        }
    }
}