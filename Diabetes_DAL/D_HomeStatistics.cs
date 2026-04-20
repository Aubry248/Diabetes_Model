using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Model;
using Tools;
namespace DAL
{
    /// <summary>
    /// 首页统计数据访问层
    /// </summary>
    public class D_HomeStatistics
    {
        #region 核心查询方法
        /// <summary>
        /// 获取医生首页统计数据
        /// </summary>
        /// <param name="doctorId">当前登录医生ID</param>
        /// <returns>统计数据实体</returns>
        public HomeStatisticViewModel GetHomeStatisticData(int doctorId)
        {
            HomeStatisticViewModel result = new HomeStatisticViewModel();
            string sql = @"
-- 1. 总管理有效患者数
SELECT COUNT(1) FROM t_user WHERE user_type=1 AND status=1;

-- 2. 今日待随访数（当前医生负责）
SELECT COUNT(1) FROM t_follow_up 
WHERE follow_up_by=@DoctorId 
AND follow_up_status=0 
AND CONVERT(date, follow_up_time)=CONVERT(date, GETDATE());

-- 3. 近7天有血糖异常的患者数（去重）
SELECT COUNT(DISTINCT user_id) FROM t_blood_sugar 
WHERE is_abnormal=1 
AND measurement_time >= DATEADD(day, -7, GETDATE());

-- 4. 当前医生待处理的异常数
SELECT COUNT(1) FROM t_abnormal 
WHERE handle_status=0;";

            using (SqlConnection conn = new SqlConnection(SqlHelper.connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // 读取总患者数
                        if (reader.Read())
                        {
                            result.TotalPatientCount = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        }
                        // 读取今日待随访
                        if (reader.NextResult() && reader.Read())
                        {
                            result.TodayWaitFollowUpCount = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        }
                        // 读取血糖异常患者数
                        if (reader.NextResult() && reader.Read())
                        {
                            result.BloodSugarAbnormalPatientCount = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        }
                        // 读取待处理异常数
                        if (reader.NextResult() && reader.Read())
                        {
                            result.WaitHandleAbnormalCount = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取医生首页待办事项列表
        /// </summary>
        /// <param name="doctorId">当前登录医生ID</param>
        /// <returns>待办列表</returns>
        public List<HomeTodoViewModel> GetHomeTodoList(int doctorId)
        {
            List<HomeTodoViewModel> list = new List<HomeTodoViewModel>();
            string sql = @"
-- 合并待随访、待处理异常、待复查待办数据
SELECT 
    u.user_name AS PatientName,
    '定期随访' AS TodoContent,
    f.follow_up_time AS DeadlineTime,
    CASE f.follow_up_status WHEN 0 THEN '待随访' WHEN 1 THEN '随访中' ELSE '已完成' END AS HandleStatus,
    f.follow_up_id AS RelationId,
    '随访' AS BusinessType
FROM t_follow_up f
LEFT JOIN t_user u ON f.user_id=u.user_id
WHERE f.follow_up_by=@DoctorId AND f.follow_up_status IN (0,1)
UNION ALL
SELECT 
    u.user_name AS PatientName,
    '异常数据处理' AS TodoContent,
    a.create_time AS DeadlineTime,
    CASE a.handle_status WHEN 0 THEN '待处理' WHEN 1 THEN '处理中' ELSE '已处理' END AS HandleStatus,
    a.abnormal_id AS RelationId,
    '异常处理' AS BusinessType
FROM t_abnormal a
LEFT JOIN t_user u ON a.user_id=u.user_id
WHERE a.handle_status IN (0,1)
UNION ALL
SELECT 
    u.user_name AS PatientName,
    '复查提醒' AS TodoContent,
    f.next_follow_up_time AS DeadlineTime,
    '待复查' AS HandleStatus,
    f.follow_up_id AS RelationId,
    '复查' AS BusinessType
FROM t_follow_up f
LEFT JOIN t_user u ON f.user_id=u.user_id
WHERE f.follow_up_by=@DoctorId 
AND f.follow_up_status=2 
AND f.next_follow_up_time IS NOT NULL 
AND CONVERT(date, f.next_follow_up_time) >= CONVERT(date, GETDATE())
ORDER BY DeadlineTime ASC;";

            using (SqlConnection conn = new SqlConnection(SqlHelper.connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            HomeTodoViewModel model = new HomeTodoViewModel
                            {
                                PatientName = reader.IsDBNull(0) ? "" : reader.GetString(0),
                                TodoContent = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                DeadlineTime = reader.IsDBNull(2) ? DateTime.Now : reader.GetDateTime(2),
                                HandleStatus = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                RelationId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                BusinessType = reader.IsDBNull(5) ? "" : reader.GetString(5)
                            };
                            list.Add(model);
                        }
                    }
                }
            }
            return list;
        }
        #endregion
    }
}