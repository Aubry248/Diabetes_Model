using System;
using System.Collections.Generic;
using System.Data;
using Model;
using Tools;

namespace BLL
{
    /// <summary>
    /// 审计日志业务层（登录/退出日志）
    /// </summary>
    public class B_AuditLog
    {
        /// <summary>
        /// 分页查询登录日志
        /// </summary>
        public List<AuditLog> GetLoginLogByPage(DateTime startTime, DateTime endTime, string userName, int? roleId, int? status, int pageIndex, int pageSize, out int totalCount)
        {
            // 基础SQL
            string sqlCount = @"SELECT COUNT(1) FROM t_audit_log a
                        LEFT JOIN t_user u ON a.operate_user_id = u.user_id
                        LEFT JOIN t_user_role ur ON u.user_id = ur.user_id
                        WHERE a.operate_type IN ('登录','退出') 
                        AND a.operate_time BETWEEN @startTime AND @endTime";

            string sqlData = @"SELECT a.*, u.user_name, r.role_name, r.role_id FROM t_audit_log a
                        LEFT JOIN t_user u ON a.operate_user_id = u.user_id
                        LEFT JOIN t_user_role ur ON u.user_id = ur.user_id
                        LEFT JOIN t_role r ON ur.role_id = r.role_id
                        WHERE a.operate_type IN ('登录','退出') 
                        AND a.operate_time BETWEEN @startTime AND @endTime";

            // 动态拼接查询条件（参数化防注入）
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
                sqlCount += " AND a.operate_content LIKE @status";
                sqlData += " AND a.operate_content LIKE @status";
                parameters.Add(new System.Data.SqlClient.SqlParameter("@status", status == 1 ? "%成功%" : "%失败%"));
            }

            // 🔴 修复点：先创建基础参数数组，用于统计总条数
            var baseParams = parameters.ToArray();
            totalCount = Convert.ToInt32(Tools.SqlHelper.ExecuteScalar(sqlCount, baseParams));

            // 分页SQL（SQL Server 2012+ 支持OFFSET FETCH）
            sqlData += " ORDER BY a.operate_time DESC OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            // 🔴 修复点：创建新的参数集合，包含基础参数 + 分页参数，避免共享
            var dataParams = new List<System.Data.SqlClient.SqlParameter>(baseParams);
            dataParams.Add(new System.Data.SqlClient.SqlParameter("@offset", (pageIndex - 1) * pageSize));
            dataParams.Add(new System.Data.SqlClient.SqlParameter("@pageSize", pageSize));

            // 执行查询
            DataTable dt = Tools.SqlHelper.ExecuteDataTable(sqlData, dataParams.ToArray());
            List<AuditLog> list = new List<AuditLog>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new AuditLog
                {
                    audit_id = Convert.ToInt32(dr["audit_id"]),
                    operate_user_id = Convert.ToInt32(dr["operate_user_id"]),
                    operate_type = dr["operate_type"].ToString(),
                    operate_content = dr["operate_content"].ToString(),
                    operate_ip = dr["operate_ip"].ToString(),
                    operate_device = dr["operate_device"].ToString(),
                    operate_time = Convert.ToDateTime(dr["operate_time"]),
                    user_name = dr["user_name"]?.ToString(),
                    role_name = dr["role_name"]?.ToString() ?? "未知",
                    role_id = dr["role_id"] != DBNull.Value ? Convert.ToInt32(dr["role_id"]) : 0
                });
            }
            return list;
        }

        /// <summary>
        /// 根据ID获取单条日志详情（修复ExecuteDataRow报错）
        /// </summary>
        public AuditLog GetAuditLogById(int auditId)
        {
            string sql = @"SELECT a.*, u.user_name, r.role_name FROM t_audit_log a
                            LEFT JOIN t_user u ON a.operate_user_id = u.user_id
                            LEFT JOIN t_user_role ur ON u.user_id = ur.user_id
                            LEFT JOIN t_role r ON ur.role_id = r.role_id
                            WHERE a.audit_id = @auditId";
            // 修复：用ExecuteDataTable替代不存在的ExecuteDataRow
            DataTable dt = Tools.SqlHelper.ExecuteDataTable(sql, new System.Data.SqlClient.SqlParameter("@auditId", auditId));
            if (dt == null || dt.Rows.Count == 0) return null;

            DataRow dr = dt.Rows[0];
            return new AuditLog
            {
                audit_id = Convert.ToInt32(dr["audit_id"]),
                operate_user_id = Convert.ToInt32(dr["operate_user_id"]),
                operate_type = dr["operate_type"].ToString(),
                operate_content = dr["operate_content"].ToString(),
                operate_ip = dr["operate_ip"].ToString(),
                operate_device = dr["operate_device"].ToString(),
                operate_time = Convert.ToDateTime(dr["operate_time"]),
                remark = dr["remark"]?.ToString(),
                user_name = dr["user_name"]?.ToString(),
                role_name = dr["role_name"]?.ToString() ?? "未知"
            };
        }

        /// <summary>
        /// 写入登录/退出日志
        /// </summary>
        public int WriteLoginLog(int userId, string operateType, string content, string ip, string device)
        {
            string sql = @"INSERT INTO t_audit_log(operate_user_id, operate_type, operate_content, operate_ip, operate_device, operate_time, remark, data_version, create_time, update_time)
                            VALUES(@userId, @operateType, @content, @ip, @device, GETDATE(), '', 1, GETDATE(), GETDATE())";
            return Tools.SqlHelper.ExecuteNonQuery(sql,
                new System.Data.SqlClient.SqlParameter("@userId", userId),
                new System.Data.SqlClient.SqlParameter("@operateType", operateType),
                new System.Data.SqlClient.SqlParameter("@content", content),
                new System.Data.SqlClient.SqlParameter("@ip", ip),
                new System.Data.SqlClient.SqlParameter("@device", device));
        }
    }
}