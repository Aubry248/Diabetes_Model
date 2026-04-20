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
    /// 异常数据处理数据访问层
    /// </summary>
    public class D_Abnormal
    {
        public int AddAbnormal(Abnormal model)
        {
            string sql = @"INSERT INTO t_abnormal 
                          (data_type, original_data_id, user_id, abnormal_type, abnormal_reason, suggestion, mark_time, mark_by, handle_status, handle_note, data_version, create_time, update_time)
                          VALUES 
                          (@data_type, @original_data_id, @user_id, @abnormal_type, @abnormal_reason, @suggestion, @mark_time, @mark_by, @handle_status, @handle_note, @data_version, @create_time, @update_time);
                          SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = {
                new SqlParameter("@data_type", model.data_type),
                new SqlParameter("@original_data_id", model.original_data_id),
                new SqlParameter("@user_id", model.user_id),
                new SqlParameter("@abnormal_type", model.abnormal_type),
                new SqlParameter("@abnormal_reason", model.abnormal_reason),
                new SqlParameter("@suggestion", model.suggestion),
                new SqlParameter("@mark_time", model.mark_time),
                new SqlParameter("@mark_by", model.mark_by),
                new SqlParameter("@handle_status", model.handle_status),
                new SqlParameter("@handle_note", string.IsNullOrWhiteSpace(model.handle_note) ? (object)DBNull.Value : model.handle_note),
                new SqlParameter("@data_version", model.data_version),
                new SqlParameter("@create_time", model.create_time),
                new SqlParameter("@update_time", model.update_time)
            };

            object result = SqlHelper.ExecuteScalar(sql, parameters);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        #region 1. 异常数据预警查询
        /// <summary>
        /// 多条件查询异常预警数据
        /// </summary>
        public List<AbnormalWarnViewModel> GetAbnormalWarnList(int? userId, string abnormalType, DateTime startDate, DateTime endDate)
        {
            string sql = @"SELECT 
                            a.abnormal_id AS AbnormalId,
                            u.user_name AS PatientName,
                            a.data_type AS DataType,
                            a.abnormal_type AS AbnormalType,
                            CASE a.data_type 
                                WHEN '血糖' THEN '血糖值异常'
                                WHEN '饮食' THEN '饮食摄入超标'
                                WHEN '运动' THEN '运动时长不足'
                                WHEN '用药' THEN '用药不规范'
                            END AS AbnormalDesc,
                            a.create_time AS AbnormalTime,
                            CASE a.handle_status 
                                WHEN 0 THEN '待处理'
                                WHEN 1 THEN '处理中'
                                WHEN 2 THEN '已处理'
                            END AS HandleStatusDesc,
                            d.user_name AS MarkDoctor,
                            a.mark_time AS MarkTime
                           FROM t_abnormal a
                           LEFT JOIN t_user u ON a.user_id = u.user_id
                           LEFT JOIN t_user d ON a.mark_by = d.user_id
                           WHERE a.create_time BETWEEN @startDate AND @endDate";
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter("@startDate", startDate.Date),
                new SqlParameter("@endDate", endDate.Date.AddDays(1).AddSeconds(-1))
            };

            // 动态拼接查询条件
            if (userId.HasValue && userId > 0)
            {
                sql += " AND a.user_id = @userId";
                parameters.Add(new SqlParameter("@userId", userId.Value));
            }
            if (!string.IsNullOrEmpty(abnormalType) && abnormalType != "全部")
            {
                sql += " AND a.abnormal_type = @abnormalType";
                parameters.Add(new SqlParameter("@abnormalType", abnormalType));
            }
            sql += " ORDER BY a.create_time DESC";

            DataTable dt = SqlHelper.ExecuteDataTable(sql, parameters.ToArray());
            return dt.AsEnumerable().Select(row => new AbnormalWarnViewModel
            {
                AbnormalId = row.Field<int>("AbnormalId"),
                PatientName = row.Field<string>("PatientName"),
                DataType = row.Field<string>("DataType"),
                AbnormalType = row.Field<string>("AbnormalType"),
                AbnormalDesc = row.Field<string>("AbnormalDesc"),
                AbnormalTime = row.Field<DateTime>("AbnormalTime"),
                HandleStatusDesc = row.Field<string>("HandleStatusDesc"),
                MarkDoctor = row.Field<string>("MarkDoctor"),
                MarkTime = row.Field<DateTime?>("MarkTime")
            }).ToList();
        }
        #endregion

        #region 2. 异常原因标注
        /// <summary>
        /// 根据患者ID获取待标注的异常数据列表
        /// </summary>
        public List<Abnormal> GetWaitMarkAbnormalByUserId(int userId)
        {
            string sql = @"SELECT abnormal_id, abnormal_type, data_type, create_time 
                           FROM t_abnormal 
                           WHERE user_id = @userId AND handle_status = 0 
                           ORDER BY create_time DESC";
            SqlParameter[] parameters = { new SqlParameter("@userId", userId) };
            DataTable dt = SqlHelper.ExecuteDataTable(sql, parameters);
            return dt.AsEnumerable().Select(row => new Abnormal
            {
                abnormal_id = row.Field<int>("abnormal_id"),
                abnormal_type = row.Field<string>("abnormal_type"),
                data_type = row.Field<string>("data_type"),
                create_time = row.Field<DateTime>("create_time")
            }).ToList();
        }

        /// <summary>
        /// 更新异常原因标注
        /// </summary>
        public int UpdateAbnormalMark(Abnormal model)
        {
            string sql = @"UPDATE t_abnormal 
                           SET abnormal_reason = @abnormal_reason,
                               suggestion = @suggestion,
                               mark_time = @mark_time,
                               mark_by = @mark_by,
                               handle_status = 1,
                               data_version = data_version + 1,
                               update_time = @update_time
                           WHERE abnormal_id = @abnormal_id";
            SqlParameter[] parameters = {
                new SqlParameter("@abnormal_reason", model.abnormal_reason),
                new SqlParameter("@suggestion", model.suggestion),
                new SqlParameter("@mark_time", model.mark_time),
                new SqlParameter("@mark_by", model.mark_by),
                new SqlParameter("@update_time", model.update_time),
                new SqlParameter("@abnormal_id", model.abnormal_id)
            };
            return SqlHelper.ExecuteNonQuery(sql, parameters);
        }
        #endregion

        #region 3. 异常干预处理
        /// <summary>
        /// 新增干预方案（事务：新增方案+更新异常处理状态）
        /// </summary>
        public int AddInterventionPlan(InterventionPlan plan, int abnormalId)
        {
            string sql = @"BEGIN TRANSACTION;
                           BEGIN TRY
                               -- 新增干预方案
                               INSERT INTO t_intervention_plan 
                               (user_id, related_abnormal_id, plan_type, plan_content, expected_effect, start_time, end_time, review_time, create_by, execute_status, data_version, create_time, update_time, status)
                               VALUES
                               (@user_id, @related_abnormal_id, @plan_type, @plan_content, @expected_effect, @start_time, @end_time, @review_time, @create_by, @execute_status, @data_version, @create_time, @update_time, @status);
                               -- 更新异常处理状态为已处理
                               UPDATE t_abnormal SET handle_status = 2, update_time = GETDATE() WHERE abnormal_id = @abnormalId;
                               COMMIT TRANSACTION;
                               SELECT 1;
                           END TRY
                           BEGIN CATCH
                               ROLLBACK TRANSACTION;
                               SELECT 0;
                           END CATCH";
            SqlParameter[] parameters = {
                new SqlParameter("@user_id", plan.user_id),
                new SqlParameter("@related_abnormal_id", plan.related_abnormal_id),
                new SqlParameter("@plan_type", plan.plan_type),
                new SqlParameter("@plan_content", plan.plan_content),
                new SqlParameter("@expected_effect", plan.expected_effect),
                new SqlParameter("@start_time", plan.start_time),
                new SqlParameter("@end_time", plan.end_time),
                new SqlParameter("@review_time", plan.review_time),
                new SqlParameter("@create_by", plan.create_by),
                new SqlParameter("@execute_status", plan.execute_status),
                new SqlParameter("@data_version", plan.data_version),
                new SqlParameter("@create_time", plan.create_time),
                new SqlParameter("@update_time", plan.update_time),
                new SqlParameter("@status", plan.status),
                new SqlParameter("@abnormalId", abnormalId)
            };
            object result = SqlHelper.ExecuteScalar(sql, parameters);
            return result == null ? 0 : Convert.ToInt32(result);
        }
        #endregion

        #region 4. 复查提醒管理
        /// <summary>
        /// 设置复查提醒（写入随访表）
        /// </summary>
        public int AddReviewRemind(FollowUp model)
        {
            string sql = @"INSERT INTO t_follow_up 
                           (user_id, follow_up_time, follow_up_way, follow_up_content, follow_up_by, follow_up_status, data_version, create_time, update_time)
                           VALUES 
                           (@user_id, @follow_up_time, '复查提醒', @follow_up_content, @follow_up_by, 0, 1, GETDATE(), GETDATE());
                           SELECT SCOPE_IDENTITY();";
            SqlParameter[] parameters = {
                new SqlParameter("@user_id", model.user_id),
                new SqlParameter("@follow_up_time", model.follow_up_time),
                new SqlParameter("@follow_up_content", model.follow_up_content),
                new SqlParameter("@follow_up_by", model.follow_up_by)
            };
            object result = SqlHelper.ExecuteScalar(sql, parameters);
            return result == null ? 0 : Convert.ToInt32(result);
        }

        /// <summary>
        /// 查询复查提醒列表
        /// </summary>
        public List<ReviewRemindViewModel> GetReviewRemindList(int? userId, DateTime startDate, DateTime endDate)
        {
            string sql = @"SELECT 
                            f.follow_up_id AS RemindId,
                            u.user_name AS PatientName,
                            f.follow_up_time AS ReviewDate,
                            f.follow_up_content AS RemindContent,
                            d.user_name AS CreateDoctor,
                            f.create_time AS CreateTime,
                            CASE f.follow_up_status 
                                WHEN 0 THEN '待复查'
                                WHEN 1 THEN '复查中'
                                WHEN 2 THEN '已复查'
                            END AS StatusDesc
                           FROM t_follow_up f
                           LEFT JOIN t_user u ON f.user_id = u.user_id
                           LEFT JOIN t_user d ON f.follow_up_by = d.user_id
                           WHERE f.follow_up_way = '复查提醒' AND f.create_time BETWEEN @startDate AND @endDate";
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter("@startDate", startDate.Date),
                new SqlParameter("@endDate", endDate.Date.AddDays(1).AddSeconds(-1))
            };

            if (userId.HasValue && userId > 0)
            {
                sql += " AND f.user_id = @userId";
                parameters.Add(new SqlParameter("@userId", userId.Value));
            }
            sql += " ORDER BY f.follow_up_time DESC";

            DataTable dt = SqlHelper.ExecuteDataTable(sql, parameters.ToArray());
            return dt.AsEnumerable().Select(row => new ReviewRemindViewModel
            {
                RemindId = row.Field<int>("RemindId"),
                PatientName = row.Field<string>("PatientName"),
                ReviewDate = row.Field<DateTime>("ReviewDate"),
                RemindContent = row.Field<string>("RemindContent"),
                CreateDoctor = row.Field<string>("CreateDoctor"),
                CreateTime = row.Field<DateTime>("CreateTime"),
                StatusDesc = row.Field<string>("StatusDesc")
            }).ToList();
        }
        #endregion

        #region 公共方法：获取有效患者列表
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
        #region 追加：带权限过滤的患者查询方法（支持医生仅看自己的患者）
        public List<PatientSimpleInfo> GetAllValidPatient(int doctorId)
        {
            string sql = @"SELECT 
                    user_id AS UserId,
                    user_name AS UserName,
                    phone AS Phone,
                    RIGHT(id_card,4) AS IdCardLast4,
                    diabetes_type AS DiabetesType,
                    doctor_id AS DoctorId
                   FROM t_user 
                   WHERE user_type = 1 AND status = 1";
            List<SqlParameter> parameters = new List<SqlParameter>();
            if (doctorId > 0)
            {
                sql += " AND doctor_id = @doctorId";
                parameters.Add(new SqlParameter("@doctorId", doctorId));
            }
            sql += " ORDER BY user_name ASC";
            DataTable dt = SqlHelper.ExecuteDataTable(sql, parameters.ToArray());
            return dt.AsEnumerable().Select(row => new PatientSimpleInfo
            {
                UserId = row.Field<int>("UserId"),
                UserName = row.Field<string>("UserName"),
                Phone = row.Field<string>("Phone"),
                IdCardLast4 = row.Field<string>("IdCardLast4"),
                DiabetesType = row.Field<string>("DiabetesType"),
                DoctorId = row.Field<int?>("DoctorId") // 【修改后：自动处理DBNull，删除冗余判断】
            }).ToList();
        }
        #endregion
        #region 【追加新方法】获取有异常数据的患者列表（弹窗专用，仅显示有异常的患者）
        /// <summary>
        /// 分页查询有异常数据的患者列表（支持多条件筛选+医生权限控制）
        /// </summary>
        public List<PatientSimpleInfo> GetPatientWithAbnormalByPage(int doctorId, string userName, string phone, string diabetesType, int pageIndex, int pageSize, out int totalCount)
        {
            // 核心逻辑：仅查询存在待处理/处理中异常数据的患者
            string whereSql = @" WHERE u.user_type = 1 AND u.status = 1 
                                AND EXISTS (SELECT 1 FROM t_abnormal a WHERE a.user_id = u.user_id AND a.handle_status IN (0,1)) ";
            List<SqlParameter> parameters = new List<SqlParameter>();

            // 医生权限控制
            if (doctorId > 0)
            {
                whereSql += " AND u.doctor_id = @doctorId ";
                parameters.Add(new SqlParameter("@doctorId", doctorId));
            }
            // 姓名模糊筛选
            if (!string.IsNullOrEmpty(userName))
            {
                whereSql += " AND u.user_name LIKE @userName ";
                parameters.Add(new SqlParameter("@userName", $"%{userName}%"));
            }
            // 手机号模糊筛选
            if (!string.IsNullOrEmpty(phone))
            {
                whereSql += " AND u.phone LIKE @phone ";
                parameters.Add(new SqlParameter("@phone", $"%{phone}%"));
            }
            // 糖尿病类型精准筛选
            if (!string.IsNullOrEmpty(diabetesType) && diabetesType != "全部")
            {
                whereSql += " AND u.diabetes_type = @diabetesType ";
                parameters.Add(new SqlParameter("@diabetesType", diabetesType));
            }

            // 查询总条数
            string countSql = $"SELECT COUNT(DISTINCT u.user_id) FROM t_user u {whereSql}";
            totalCount = Convert.ToInt32(SqlHelper.ExecuteScalar(countSql, parameters.ToArray()));

            // 分页查询数据
            string pageSql = $@"
            SELECT * FROM (
                SELECT 
                    ROW_NUMBER() OVER(ORDER BY u.user_name ASC) AS RowId,
                    u.user_id AS UserId,
                    u.user_name AS UserName,
                    u.phone AS Phone,
                    RIGHT(u.id_card,4) AS IdCardLast4,
                    u.diabetes_type AS DiabetesType,
                    u.doctor_id AS DoctorId,
                    -- 追加异常数据统计，便于前端展示
                    (SELECT COUNT(1) FROM t_abnormal a WHERE a.user_id = u.user_id AND a.handle_status = 0) AS UnhandledAbnormalCount
                FROM t_user u 
                {whereSql}
                GROUP BY u.user_id, u.user_name, u.phone, u.id_card, u.diabetes_type, u.doctor_id
            ) AS TempTable
            WHERE RowId BETWEEN @startRow AND @endRow
            ORDER BY RowId ASC";

            int startRow = (pageIndex - 1) * pageSize + 1;
            int endRow = pageIndex * pageSize;
            parameters.Add(new SqlParameter("@startRow", startRow));
            parameters.Add(new SqlParameter("@endRow", endRow));

            DataTable dt = SqlHelper.ExecuteDataTable(pageSql, parameters.ToArray());
            return dt.AsEnumerable().Select(row => new PatientSimpleInfo
            {
                UserId = row.Field<int>("UserId"),
                UserName = row.Field<string>("UserName"),
                Phone = row.Field<string>("Phone"),
                IdCardLast4 = row.Field<string>("IdCardLast4"),
                DiabetesType = row.Field<string>("DiabetesType"),
                DoctorId = row.Field<int?>("DoctorId"),
                // 扩展字段：待处理异常数量，前端可展示
                UnhandledAbnormalCount = row.Field<int>("UnhandledAbnormalCount")
            }).ToList();
        }
        #endregion

        #region 【追加新方法】获取患者最新异常数据（确认选择后自动加载）
        /// <summary>
        /// 按患者ID获取异常预警数据（复用原有查询逻辑，单独封装）
        /// </summary>
        public List<AbnormalWarnViewModel> GetAbnormalWarnByPatientId(int patientId, DateTime startDate, DateTime endDate)
        {
            string sql = @"SELECT 
                            a.abnormal_id AS AbnormalId,
                            u.user_name AS PatientName,
                            a.data_type AS DataType,
                            a.abnormal_type AS AbnormalType,
                            CASE a.data_type 
                                WHEN '血糖' THEN '血糖值异常'
                                WHEN '饮食' THEN '饮食摄入超标'
                                WHEN '运动' THEN '运动时长不足'
                                WHEN '用药' THEN '用药不规范'
                            END AS AbnormalDesc,
                            a.create_time AS AbnormalTime,
                            CASE a.handle_status 
                                WHEN 0 THEN '待处理'
                                WHEN 1 THEN '处理中'
                                WHEN 2 THEN '已处理'
                            END AS HandleStatusDesc,
                            d.user_name AS MarkDoctor,
                            a.mark_time AS MarkTime
                           FROM t_abnormal a
                           LEFT JOIN t_user u ON a.user_id = u.user_id
                           LEFT JOIN t_user d ON a.mark_by = d.user_id
                           WHERE a.user_id = @patientId AND a.create_time BETWEEN @startDate AND @endDate
                           ORDER BY a.create_time DESC";

            SqlParameter[] parameters = {
                new SqlParameter("@patientId", patientId),
                new SqlParameter("@startDate", startDate.Date),
                new SqlParameter("@endDate", endDate.Date.AddDays(1).AddSeconds(-1))
            };

            DataTable dt = SqlHelper.ExecuteDataTable(sql, parameters);
            return dt.AsEnumerable().Select(row => new AbnormalWarnViewModel
            {
                AbnormalId = row.Field<int>("AbnormalId"),
                PatientName = row.Field<string>("PatientName"),
                DataType = row.Field<string>("DataType"),
                AbnormalType = row.Field<string>("AbnormalType"),
                AbnormalDesc = row.Field<string>("AbnormalDesc"),
                AbnormalTime = row.Field<DateTime>("AbnormalTime"),
                HandleStatusDesc = row.Field<string>("HandleStatusDesc"),
                MarkDoctor = row.Field<string>("MarkDoctor"),
                MarkTime = row.Field<DateTime?>("MarkTime")
            }).ToList();
        }
        #endregion
        #region 追加：带筛选+分页的患者查询方法（弹窗专用，不影响原有逻辑）
        /// <summary>
        /// 分页查询患者列表（支持多条件筛选）
        /// </summary>
        /// <param name="doctorId">当前登录医生ID（权限控制）</param>
        /// <param name="userName">患者姓名（模糊筛选）</param>
        /// <param name="phone">手机号（模糊筛选）</param>
        /// <param name="diabetesType">糖尿病类型（精准筛选）</param>
        /// <param name="pageIndex">页码（从1开始）</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="totalCount">输出总条数（分页用）</param>
        public List<PatientSimpleInfo> GetPatientListByPage(int doctorId, string userName, string phone, string diabetesType, int pageIndex, int pageSize, out int totalCount)
        {
            // 1. 构建查询条件
            string whereSql = " WHERE user_type = 1 AND status = 1 ";
            List<SqlParameter> parameters = new List<SqlParameter>();

            // 权限控制：仅看当前医生负责的患者
            if (doctorId > 0)
            {
                whereSql += " AND doctor_id = @doctorId ";
                parameters.Add(new SqlParameter("@doctorId", doctorId));
            }

            // 姓名模糊筛选
            if (!string.IsNullOrEmpty(userName))
            {
                whereSql += " AND user_name LIKE @userName ";
                parameters.Add(new SqlParameter("@userName", $"%{userName}%"));
            }

            // 手机号模糊筛选
            if (!string.IsNullOrEmpty(phone))
            {
                whereSql += " AND phone LIKE @phone ";
                parameters.Add(new SqlParameter("@phone", $"%{phone}%"));
            }

            // 糖尿病类型精准筛选
            if (!string.IsNullOrEmpty(diabetesType) && diabetesType != "全部")
            {
                whereSql += " AND diabetes_type = @diabetesType ";
                parameters.Add(new SqlParameter("@diabetesType", diabetesType));
            }

            // 2. 查询总条数
            string countSql = $"SELECT COUNT(1) FROM t_user {whereSql}";
            totalCount = Convert.ToInt32(SqlHelper.ExecuteScalar(countSql, parameters.ToArray()));

            // 3. 分页查询数据（SQL Server分页语法）
            string pageSql = $@"
        SELECT * FROM (
            SELECT 
                ROW_NUMBER() OVER(ORDER BY user_name ASC) AS RowId,
                user_id AS UserId,
                user_name AS UserName,
                phone AS Phone,
                RIGHT(id_card,4) AS IdCardLast4,
                diabetes_type AS DiabetesType,
                doctor_id AS DoctorId
            FROM t_user 
            {whereSql}
        ) AS TempTable
        WHERE RowId BETWEEN @startRow AND @endRow
        ORDER BY RowId ASC";

            int startRow = (pageIndex - 1) * pageSize + 1;
            int endRow = pageIndex * pageSize;
            parameters.Add(new SqlParameter("@startRow", startRow));
            parameters.Add(new SqlParameter("@endRow", endRow));

            DataTable dt = SqlHelper.ExecuteDataTable(pageSql, parameters.ToArray());

            return dt.AsEnumerable().Select(row => new PatientSimpleInfo
            {
                UserId = row.Field<int>("UserId"),
                UserName = row.Field<string>("UserName"),
                Phone = row.Field<string>("Phone"),
                IdCardLast4 = row.Field<string>("IdCardLast4"),
                DiabetesType = row.Field<string>("DiabetesType"),
                DoctorId = row.Field<int?>("DoctorId")
            }).ToList();
        }
        #endregion
    }
}