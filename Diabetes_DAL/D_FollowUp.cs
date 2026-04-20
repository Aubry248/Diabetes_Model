using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Model;
using Tools;
namespace DAL
{
    /// <summary>
    /// 随访管理数据访问层
    /// </summary>
    public class D_FollowUp
    {
        #region 患者数据查询
        /// <summary>
        /// 获取所有有效患者列表
        /// </summary>
        public List<PatientSimpleInfo> GetAllValidPatient()
        {
            string sql = @"SELECT user_id AS UserId, user_name AS UserName 
                           FROM t_user 
                           WHERE user_type = 1 AND status = 1 
                           ORDER BY user_name";
            DataTable dt = SqlHelper.ExecuteDataTable(sql);
            return dt.AsEnumerable().Select(row => new PatientSimpleInfo
            {
                UserId = row.Field<int>("UserId"),
                UserName = row.Field<string>("UserName")
            }).ToList();
        }
        #endregion

        #region 随访计划操作
        /// <summary>
        /// 新增随访计划
        /// </summary>
        public int AddFollowUpPlan(FollowUp model)
        {
            string sql = @"INSERT INTO t_follow_up 
                           (user_id, plan_id, follow_up_time, follow_up_way, follow_up_content, follow_up_result, 
                            next_follow_up_time, follow_up_by, follow_up_status, data_version, create_time, update_time)
                           VALUES 
                           (@user_id, @plan_id, @follow_up_time, @follow_up_way, @follow_up_content, @follow_up_result, 
                            @next_follow_up_time, @follow_up_by, @follow_up_status, @data_version, @create_time, @update_time);
                           SELECT SCOPE_IDENTITY();";
            SqlParameter[] parameters = {
                new SqlParameter("@user_id", model.user_id),
                new SqlParameter("@plan_id", model.plan_id ?? (object)DBNull.Value),
                new SqlParameter("@follow_up_time", model.follow_up_time),
                new SqlParameter("@follow_up_way", model.follow_up_way),
                new SqlParameter("@follow_up_content", model.follow_up_content),
                new SqlParameter("@follow_up_result", model.follow_up_result ?? (object)DBNull.Value),
                new SqlParameter("@next_follow_up_time", model.next_follow_up_time ?? (object)DBNull.Value),
                new SqlParameter("@follow_up_by", model.follow_up_by),
                new SqlParameter("@follow_up_status", model.follow_up_status),
                new SqlParameter("@data_version", model.data_version),
                new SqlParameter("@create_time", model.create_time),
                new SqlParameter("@update_time", model.update_time)
            };
            object result = SqlHelper.ExecuteScalar(sql, parameters);
            return result == null ? 0 : Convert.ToInt32(result);
        }

        /// <summary>
        /// 根据患者ID获取待随访的计划列表
        /// </summary>
        public List<FollowUp> GetWaitFollowUpPlanByUserId(int userId)
        {
            string sql = @"SELECT follow_up_id, follow_up_time, follow_up_way, follow_up_content 
                           FROM t_follow_up 
                           WHERE user_id = @user_id AND follow_up_status = 0 
                           ORDER BY follow_up_time DESC";
            SqlParameter[] parameters = { new SqlParameter("@user_id", userId) };
            DataTable dt = SqlHelper.ExecuteDataTable(sql, parameters);
            return dt.AsEnumerable().Select(row => new FollowUp
            {
                follow_up_id = row.Field<int>("follow_up_id"),
                follow_up_time = row.Field<DateTime>("follow_up_time"),
                follow_up_way = row.Field<string>("follow_up_way"),
                follow_up_content = row.Field<string>("follow_up_content")
            }).ToList();
        }
        #endregion

        #region 随访记录操作
        /// <summary>
        /// 更新随访记录（提交随访结果）
        /// </summary>
        public int UpdateFollowUpRecord(FollowUp model)
        {
            string sql = @"UPDATE t_follow_up 
                           SET follow_up_result = @follow_up_result, 
                               next_follow_up_time = @next_follow_up_time, 
                               follow_up_status = @follow_up_status,
                               data_version = data_version + 1,
                               update_time = @update_time
                           WHERE follow_up_id = @follow_up_id";
            SqlParameter[] parameters = {
                new SqlParameter("@follow_up_result", model.follow_up_result),
                new SqlParameter("@next_follow_up_time", model.next_follow_up_time ?? (object)DBNull.Value),
                new SqlParameter("@follow_up_status", model.follow_up_status),
                new SqlParameter("@update_time", model.update_time),
                new SqlParameter("@follow_up_id", model.follow_up_id)
            };
            return SqlHelper.ExecuteNonQuery(sql, parameters);
        }
        #endregion

        #region 随访历史查询
        /// <summary>
        /// 多条件查询随访历史列表
        /// </summary>
        public List<FollowUpHistoryViewModel> GetFollowUpHistoryList(int? userId, string followUpType, DateTime startDate, DateTime endDate)
        {
            string sql = @"SELECT 
                            f.follow_up_id AS FollowUpId,
                            f.follow_up_time AS FollowUpDate,
                            u.user_name AS PatientName,
                            CASE f.follow_up_status 
                                WHEN 0 THEN '待随访' 
                                WHEN 1 THEN '随访中' 
                                WHEN 2 THEN '已完成' 
                            END AS StatusDesc,
                            f.follow_up_way AS FollowUpWay,
                            f.follow_up_result AS FollowUpResult,
                            d.user_name AS FollowUpDoctor
                           FROM t_follow_up f
                           LEFT JOIN t_user u ON f.user_id = u.user_id
                           LEFT JOIN t_user d ON f.follow_up_by = d.user_id
                           WHERE f.follow_up_time BETWEEN @startDate AND @endDate";
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter("@startDate", startDate.Date),
                new SqlParameter("@endDate", endDate.Date.AddDays(1).AddSeconds(-1))
            };

            // 拼接条件
            if (userId.HasValue && userId > 0)
            {
                sql += " AND f.user_id = @userId";
                parameters.Add(new SqlParameter("@userId", userId.Value));
            }
            if (!string.IsNullOrEmpty(followUpType) && followUpType != "全部")
            {
                sql += " AND f.follow_up_content LIKE @followUpType";
                parameters.Add(new SqlParameter("@followUpType", $"%{followUpType}%"));
            }
            sql += " ORDER BY f.follow_up_time DESC";

            DataTable dt = SqlHelper.ExecuteDataTable(sql, parameters.ToArray());
            return dt.AsEnumerable().Select(row => new FollowUpHistoryViewModel
            {
                FollowUpId = row.Field<int>("FollowUpId"),
                FollowUpDate = row.Field<DateTime>("FollowUpDate"),
                PatientName = row.Field<string>("PatientName"),
                FollowUpWay = row.Field<string>("FollowUpWay"),
                FollowUpResult = row.Field<string>("FollowUpResult"),
                StatusDesc = row.Field<string>("StatusDesc"),
                FollowUpDoctor = row.Field<string>("FollowUpDoctor")
            }).ToList();
        }
        #endregion

        /// <summary>
        /// 分页查询随访历史数据
        /// </summary>
        public List<FollowUpHistoryViewModel> GetFollowUpHistoryByPage(
            int? userId, string followUpType, DateTime startDate, DateTime endDate,
            int pageIndex, int pageSize, out int totalCount)
        {
            List<FollowUpHistoryViewModel> list = new List<FollowUpHistoryViewModel>();
            // 先查询总记录数
            string countSql = @"
SELECT COUNT(1) FROM t_follow_up f
LEFT JOIN t_user u ON f.user_id=u.user_id
WHERE CONVERT(date, f.follow_up_time) BETWEEN CONVERT(date, @StartDate) AND CONVERT(date, @EndDate)
AND (@UserId IS NULL OR f.user_id=@UserId);";

            // 分页查询数据
            string dataSql = @"
SELECT 
    f.follow_up_id AS FollowUpId,
    f.follow_up_time AS FollowUpDate,
    u.user_name AS PatientName,
    f.follow_up_content AS FollowUpType,
    f.follow_up_way AS FollowUpWay,
    f.follow_up_result AS FollowUpResult,
    CASE f.follow_up_status WHEN 0 THEN '待随访' WHEN 1 THEN '随访中' ELSE '已完成' END AS StatusDesc,
    u2.user_name AS FollowUpDoctor
FROM t_follow_up f
LEFT JOIN t_user u ON f.user_id=u.user_id
LEFT JOIN t_user u2 ON f.follow_up_by=u2.user_id
WHERE CONVERT(date, f.follow_up_time) BETWEEN CONVERT(date, @StartDate) AND CONVERT(date, @EndDate)
AND (@UserId IS NULL OR f.user_id=@UserId)
ORDER BY f.follow_up_time DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            using (SqlConnection conn = new SqlConnection(SqlHelper.connStr))
            {
                conn.Open();
                // 先获取总记录数
                using (SqlCommand cmd = new SqlCommand(countSql, conn))
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    cmd.Parameters.AddWithValue("@UserId", userId.HasValue ? (object)userId.Value : DBNull.Value);
                    totalCount = (int)cmd.ExecuteScalar();
                }

                // 再查询分页数据
                using (SqlCommand cmd = new SqlCommand(dataSql, conn))
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    cmd.Parameters.AddWithValue("@UserId", userId.HasValue ? (object)userId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Offset", (pageIndex - 1) * pageSize);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            FollowUpHistoryViewModel model = new FollowUpHistoryViewModel
                            {
                                FollowUpId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                FollowUpDate = reader.IsDBNull(1) ? DateTime.Now : reader.GetDateTime(1),
                                PatientName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                FollowUpType = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                FollowUpWay = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                FollowUpResult = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                StatusDesc = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                FollowUpDoctor = reader.IsDBNull(7) ? "" : reader.GetString(7)
                            };
                            list.Add(model);
                        }
                    }
                }
            }
            return list;
        }
    }
}