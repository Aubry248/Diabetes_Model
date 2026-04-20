using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Model;
using Tools;

namespace DAL
{
    /// <summary>
    /// 干预方案数据操作层
    /// </summary>
    public class D_InterventionPlan
    {
        #region 数据库连接字符串（与项目全局统一）
        private readonly string _connString = SqlHelper.connStr;
        #endregion
     
        /// <summary>
        /// 安全版患者列表查询（原有方法保留，优化空值过滤）
        /// </summary>
        public DataTable GetPatientListSafe()
        {
            string sql = @"
            SELECT 
                user_id = ISNULL(user_id, 0),
                user_name = ISNULL(user_name, '未知患者')
            FROM t_user 
            WHERE user_type = 1 
              AND status = 1 
              AND user_id IS NOT NULL 
              AND user_name IS NOT NULL
            ORDER BY user_name ASC";
            DataTable dt = SqlHelper.ExecuteDataTable(sql);

            // 过滤无效数据
            DataRow[] invalidRows = dt.Select("user_id = 0");
            foreach (var row in invalidRows)
            {
                dt.Rows.Remove(row);
            }
            return dt;
        }

    #region 核心增删改操作
    /// <summary>
    /// 新增干预方案记录
    /// </summary>
    /// <param name="model">干预方案实体</param>
    /// <returns>新增成功返回主键ID，失败返回-1</returns>
    public int AddInterventionPlan(InterventionPlan model)
        {
            string sql = @"
INSERT INTO [dbo].[t_intervention_plan]
([user_id],[related_abnormal_id],[plan_type],[plan_content],[expected_effect],
[start_time],[end_time],[review_time],[create_by],[execute_status],[execute_note],
[data_version],[create_time],[update_time],[status])
VALUES
(@user_id,@related_abnormal_id,@plan_type,@plan_content,@expected_effect,
@start_time,@end_time,@review_time,@create_by,@execute_status,@execute_note,
@data_version,@create_time,@update_time,@status);
SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@user_id", SqlDbType.Int) { Value = model.user_id },
                new SqlParameter("@related_abnormal_id", SqlDbType.Int) { Value = model.related_abnormal_id.HasValue ? (object)model.related_abnormal_id : DBNull.Value },
                new SqlParameter("@plan_type", SqlDbType.VarChar, 20) { Value = model.plan_type },
                new SqlParameter("@plan_content", SqlDbType.VarChar, -1) { Value = model.plan_content },
                new SqlParameter("@expected_effect", SqlDbType.VarChar, -1) { Value = model.expected_effect },
                new SqlParameter("@start_time", SqlDbType.Date) { Value = model.start_time },
                new SqlParameter("@end_time", SqlDbType.Date) { Value = model.end_time },
                new SqlParameter("@review_time", SqlDbType.Date) { Value = model.review_time },
                new SqlParameter("@create_by", SqlDbType.Int) { Value = model.create_by },
                new SqlParameter("@execute_status", SqlDbType.TinyInt) { Value = model.execute_status },
                new SqlParameter("@execute_note", SqlDbType.VarChar, 200) { Value = model.execute_note ?? (object)DBNull.Value },
                new SqlParameter("@data_version", SqlDbType.Int) { Value = model.data_version },
                new SqlParameter("@create_time", SqlDbType.DateTime) { Value = model.create_time },
                new SqlParameter("@update_time", SqlDbType.DateTime) { Value = model.update_time },
                new SqlParameter("@status", SqlDbType.TinyInt) { Value = model.status }
            };

            try
            {
                object obj = SqlHelper.ExecuteScalar(sql, parameters);
                return obj != null && int.TryParse(obj.ToString(), out int id) ? id : -1;
            }
            catch (Exception ex)
            {
                throw new Exception("新增干预方案失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 更新干预方案（用于下发、调整、状态修改）
        /// </summary>
        public bool UpdateInterventionPlan(InterventionPlan model)
        {
            string sql = @"
UPDATE [dbo].[t_intervention_plan] SET
[user_id] = @user_id,
[related_abnormal_id] = @related_abnormal_id,
[plan_type] = @plan_type,
[plan_content] = @plan_content,
[expected_effect] = @expected_effect,
[start_time] = @start_time,
[end_time] = @end_time,
[review_time] = @review_time,
[execute_status] = @execute_status,
[execute_note] = @execute_note,
[data_version] = @data_version + 1,
[update_time] = @update_time,
[status] = @status
WHERE plan_id = @plan_id";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@plan_id", SqlDbType.Int) { Value = model.plan_id },
                new SqlParameter("@user_id", SqlDbType.Int) { Value = model.user_id },
                new SqlParameter("@related_abnormal_id", SqlDbType.Int) { Value = model.related_abnormal_id.HasValue ? (object)model.related_abnormal_id : DBNull.Value },
                new SqlParameter("@plan_type", SqlDbType.VarChar, 20) { Value = model.plan_type },
                new SqlParameter("@plan_content", SqlDbType.VarChar, -1) { Value = model.plan_content },
                new SqlParameter("@expected_effect", SqlDbType.VarChar, -1) { Value = model.expected_effect },
                new SqlParameter("@start_time", SqlDbType.Date) { Value = model.start_time },
                new SqlParameter("@end_time", SqlDbType.Date) { Value = model.end_time },
                new SqlParameter("@review_time", SqlDbType.Date) { Value = model.review_time },
                new SqlParameter("@execute_status", SqlDbType.TinyInt) { Value = model.execute_status },
                new SqlParameter("@execute_note", SqlDbType.VarChar, 200) { Value = model.execute_note ?? (object)DBNull.Value },
                new SqlParameter("@data_version", SqlDbType.Int) { Value = model.data_version },
                new SqlParameter("@update_time", SqlDbType.DateTime) { Value = DateTime.Now },
                new SqlParameter("@status", SqlDbType.TinyInt) { Value = model.status }
            };

            try
            {
                return SqlHelper.ExecuteNonQuery(sql, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("更新干预方案失败：" + ex.Message);
            }
        }
        #endregion

        #region 查询操作
        /// <summary>
        /// 获取患者下拉列表（原有方法保留，追加视图失败兜底查t_user表）
        /// </summary>
        public DataTable GetPatientList()
        {
            try
            {
                // 优先查脱敏视图（原有逻辑保留）
                string sql = "SELECT user_id, user_name FROM [dbo].[v_patient_data_desensitized] WHERE status = 1 ORDER BY user_name";
                DataTable dt = SqlHelper.ExecuteDataTable(sql);

                // 视图无数据/执行失败，兜底查t_user表（新增容错，不破坏原有逻辑）
                if (dt == null || dt.Rows.Count == 0)
                {
                    sql = "SELECT user_id, user_name FROM t_user WHERE user_type = 1 AND status = 1 ORDER BY user_name ASC";
                    dt = SqlHelper.ExecuteDataTable(sql);
                }
                return dt;
            }
            catch (Exception ex)
            {
                // 视图执行报错，兜底查t_user表
                string sql = "SELECT user_id, user_name FROM t_user WHERE user_type = 1 AND status = 1 ORDER BY user_name ASC";
                DataTable dt = SqlHelper.ExecuteDataTable(sql);
                if (dt == null || dt.Rows.Count == 0)
                {
                    throw new Exception("获取患者列表失败：视图与基础表均无有效患者数据，" + ex.Message);
                }
                return dt;
            }
        }

        /// <summary>
        /// 获取待下发干预方案列表
        /// </summary>
        public DataTable GetWaitIssuePlanList()
        {
            string sql = @"
SELECT p.plan_id, u.user_name as PatientName, p.plan_type as PlanType, 
p.create_time as CreateTime, 
CASE p.status WHEN 0 THEN '待下发' WHEN 1 THEN '已下发' WHEN 2 THEN '已结束' END as IssueStatus
FROM [dbo].[t_intervention_plan] p
LEFT JOIN [dbo].[t_user] u ON p.user_id = u.user_id
WHERE p.status IN (0,1) AND p.execute_status != 2
ORDER BY p.create_time DESC";
            try
            {
                return SqlHelper.ExecuteDataTable(sql);
            }
            catch (Exception ex)
            {
                throw new Exception("获取待下发方案列表失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 根据ID获取方案详情
        /// </summary>
        public InterventionPlan GetPlanById(int planId)
        {
            string sql = "SELECT * FROM [dbo].[t_intervention_plan] WHERE plan_id = @plan_id AND status != 0";
            SqlParameter[] parameters = { new SqlParameter("@plan_id", SqlDbType.Int) { Value = planId } };
            try
            {
                return SqlHelper.GetModel<InterventionPlan>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("获取方案详情失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 获取患者方案调整记录（历史版本）
        /// </summary>
        public DataTable GetPatientPlanAdjustRecords(int userId, DateTime startTime, DateTime endTime)
        {
            string sql = @"
SELECT u.user_name as PatientName, p.plan_type as PlanType, 
p.update_time as AdjustTime, p.plan_content as AdjustContent, 
u2.user_name as AdjustDoctor, p.data_version as VersionNo
FROM [dbo].[t_intervention_plan] p
LEFT JOIN [dbo].[t_user] u ON p.user_id = u.user_id
LEFT JOIN [dbo].[t_user] u2 ON p.create_by = u2.user_id
WHERE p.user_id = @user_id 
AND p.update_time BETWEEN @startTime AND @endTime
AND p.data_version >= 1
ORDER BY p.update_time DESC, p.data_version DESC";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@user_id", SqlDbType.Int) { Value = userId },
                new SqlParameter("@startTime", SqlDbType.DateTime) { Value = startTime },
                new SqlParameter("@endTime", SqlDbType.DateTime) { Value = endTime.AddDays(1).AddSeconds(-1) }
            };

            try
            {
                return SqlHelper.ExecuteDataTable(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("获取方案调整记录失败：" + ex.Message);
            }
        }
        #endregion
    }
}