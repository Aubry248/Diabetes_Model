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
    /// 干预效果评估数据访问层
    /// </summary>
    public class D_EffectEvaluation
    {
        #region 1. 血糖趋势分析相关数据访问
        /// <summary>
        /// 获取患者指定时间范围的血糖明细数据
        /// </summary>
        public List<BloodSugarTrendViewModel> GetBloodSugarTrendList(int userId, DateTime startDate, DateTime endDate)
        {
            string sql = @"SELECT 
                            blood_sugar_id AS BloodSugarId,
                            measurement_time AS MeasureTime,
                            measurement_scenario AS MeasureScenario,
                            blood_sugar_value AS BloodSugarValue,
                            is_abnormal AS IsAbnormal,
                            CASE 
                                WHEN measurement_scenario='空腹' AND blood_sugar_value BETWEEN 3.9 AND 6.1 THEN '达标'
                                WHEN measurement_scenario='餐后2小时' AND blood_sugar_value BETWEEN 3.9 AND 7.8 THEN '达标'
                                ELSE '不达标' 
                            END AS ControlStatus
                           FROM t_blood_sugar
                           WHERE user_id = @userId 
                           AND measurement_time BETWEEN @startDate AND @endDate
                           ORDER BY measurement_time ASC";
            SqlParameter[] parameters = {
                new SqlParameter("@userId", userId),
                new SqlParameter("@startDate", startDate.Date),
                new SqlParameter("@endDate", endDate.Date.AddDays(1).AddSeconds(-1))
            };

            DataTable dt = SqlHelper.ExecuteDataTable(sql, parameters);
            return dt.AsEnumerable().Select(row => new BloodSugarTrendViewModel
            {
                BloodSugarId = row.Field<int>("BloodSugarId"),
                MeasureTime = row.Field<DateTime>("MeasureTime"),
                MeasureScenario = row.Field<string>("MeasureScenario"),
                BloodSugarValue = row.Field<decimal>("BloodSugarValue"),
                IsAbnormal = row.Field<bool>("IsAbnormal"),
                ControlStatus = row.Field<string>("ControlStatus")
            }).ToList();
        }

        /// <summary>
        /// 获取患者血糖统计汇总数据
        /// </summary>
        public BloodSugarStatisticsModel GetBloodSugarStatistics(int userId, DateTime startDate, DateTime endDate)
        {
            string sql = @"SELECT 
                            COUNT(1) AS TotalRecordCount,
                            SUM(CASE WHEN is_abnormal=1 THEN 1 ELSE 0 END) AS AbnormalCount,
                            AVG(CASE WHEN measurement_scenario='空腹' THEN blood_sugar_value ELSE NULL END) AS AvgFastingGlucose,
                            AVG(CASE WHEN measurement_scenario='餐后2小时' THEN blood_sugar_value ELSE NULL END) AS AvgPostprandialGlucose,
                            MAX(blood_sugar_value) AS MaxGlucose,
                            MIN(blood_sugar_value) AS MinGlucose,
                            MAX(blood_sugar_value) - MIN(blood_sugar_value) AS GlucoseFluctuation,
                            CAST(SUM(CASE WHEN 
                                (measurement_scenario='空腹' AND blood_sugar_value BETWEEN 3.9 AND 6.1) 
                                OR (measurement_scenario='餐后2小时' AND blood_sugar_value BETWEEN 3.9 AND 7.8) 
                            THEN 1 ELSE 0 END) * 100.0 / COUNT(1) AS DECIMAL(5,2)) AS StandardReachRate
                           FROM t_blood_sugar
                           WHERE user_id = @userId 
                           AND measurement_time BETWEEN @startDate AND @endDate";
            SqlParameter[] parameters = {
                new SqlParameter("@userId", userId),
                new SqlParameter("@startDate", startDate.Date),
                new SqlParameter("@endDate", endDate.Date.AddDays(1).AddSeconds(-1))
            };

            DataTable dt = SqlHelper.ExecuteDataTable(sql, parameters);
            if (dt.Rows.Count == 0) return new BloodSugarStatisticsModel();

            DataRow row = dt.Rows[0];
            return new BloodSugarStatisticsModel
            {
                TotalRecordCount = row.Field<int>("TotalRecordCount"),
                AbnormalCount = row.Field<int>("AbnormalCount"),
                AvgFastingGlucose = row["AvgFastingGlucose"] == DBNull.Value ? 0 : row.Field<decimal>("AvgFastingGlucose"),
                AvgPostprandialGlucose = row["AvgPostprandialGlucose"] == DBNull.Value ? 0 : row.Field<decimal>("AvgPostprandialGlucose"),
                MaxGlucose = row["MaxGlucose"] == DBNull.Value ? 0 : row.Field<decimal>("MaxGlucose"),
                MinGlucose = row["MinGlucose"] == DBNull.Value ? 0 : row.Field<decimal>("MinGlucose"),
                GlucoseFluctuation = row["GlucoseFluctuation"] == DBNull.Value ? 0 : row.Field<decimal>("GlucoseFluctuation"),
                StandardReachRate = row["StandardReachRate"] == DBNull.Value ? 0 : row.Field<decimal>("StandardReachRate")
            };
        }
        #endregion

        #region 2. 并发症进展跟踪相关数据访问
        /// <summary>
        /// 获取患者并发症进展记录
        /// </summary>
        public List<ComplicationProgressViewModel> GetComplicationProgressList(int userId, string complicationType, DateTime startDate, DateTime endDate)
        {
            string sql = @"SELECT 
                            a.assessment_id AS RecordId,
                            a.assessment_date AS RecordDate,
                            a.diabetes_complications AS ComplicationType,
                            '随访评估指标更新' AS IndexChangeDesc,
                            CASE a.glycemic_control_status
                                WHEN '良好' THEN '稳定'
                                WHEN '一般' THEN '进展'
                                WHEN '差' THEN '恶化'
                                ELSE '待评估'
                            END AS ProgressDegree,
                            f.follow_up_content AS InterventionMeasures,
                            u.user_name AS FollowUpDoctor
                           FROM t_health_assessment a
                           LEFT JOIN t_follow_up f ON a.user_id = f.user_id AND f.follow_up_time BETWEEN a.assessment_date AND DATEADD(DAY,7,a.assessment_date)
                           LEFT JOIN t_user u ON a.assessment_by = u.user_id
                           WHERE a.user_id = @userId 
                           AND a.assessment_date BETWEEN @startDate AND @endDate";
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter("@userId", userId),
                new SqlParameter("@startDate", startDate.Date),
                new SqlParameter("@endDate", endDate.Date.AddDays(1).AddSeconds(-1))
            };

            // 并发症类型过滤
            if (!string.IsNullOrEmpty(complicationType) && complicationType != "全部")
            {
                sql += " AND a.diabetes_complications LIKE @complicationType";
                parameters.Add(new SqlParameter("@complicationType", $"%{complicationType}%"));
            }
            sql += " ORDER BY a.assessment_date DESC";

            DataTable dt = SqlHelper.ExecuteDataTable(sql, parameters.ToArray());
            return dt.AsEnumerable().Select(row => new ComplicationProgressViewModel
            {
                RecordId = row.Field<int>("RecordId"),
                RecordDate = row.Field<DateTime>("RecordDate"),
                ComplicationType = row.Field<string>("ComplicationType"),
                IndexChangeDesc = row.Field<string>("IndexChangeDesc"),
                ProgressDegree = row.Field<string>("ProgressDegree"),
                InterventionMeasures = row.Field<string>("InterventionMeasures"),
                FollowUpDoctor = row.Field<string>("FollowUpDoctor")
            }).ToList();
        }
        #endregion

        #region 3. 干预效果评分相关数据访问
        /// <summary>
        /// 保存干预效果评分至健康评估表
        /// </summary>
        public int SaveInterventionEffectScore(InterventionEffectScore model)
        {
            string sql = @"INSERT INTO t_health_assessment
                            (user_id, assessment_date, assessment_type, assessment_by, assessment_score, diabetes_complications, data_completeness, data_version, create_time, update_time, status)
                           VALUES
                            (@userId, @assessmentDate, '干预效果评分', @assessmentBy, @totalScore, @interventionType, 100, 1, GETDATE(), GETDATE(), 1);
                           SELECT SCOPE_IDENTITY();";
            SqlParameter[] parameters = {
                new SqlParameter("@userId", model.UserId),
                new SqlParameter("@assessmentDate", model.AssessmentDate),
                new SqlParameter("@assessmentBy", model.AssessmentBy),
                new SqlParameter("@totalScore", model.TotalScore),
                new SqlParameter("@interventionType", model.InterventionType)
            };

            object result = SqlHelper.ExecuteScalar(sql, parameters);
            return result == null ? 0 : Convert.ToInt32(result);
        }

        /// <summary>
        /// 获取患者最新干预效果评分
        /// </summary>
        public InterventionEffectScore GetLatestEffectScore(int userId)
        {
            string sql = @"SELECT TOP 1
                            assessment_id AS AssessmentId,
                            user_id AS UserId,
                            diabetes_complications AS InterventionType,
                            assessment_score AS TotalScore,
                            assessment_date AS AssessmentDate,
                            assessment_by AS AssessmentBy
                           FROM t_health_assessment
                           WHERE user_id = @userId AND assessment_type = '干预效果评分'
                           ORDER BY assessment_date DESC";
            SqlParameter[] parameters = { new SqlParameter("@userId", userId) };

            DataTable dt = SqlHelper.ExecuteDataTable(sql, parameters);
            if (dt.Rows.Count == 0) return null;

            DataRow row = dt.Rows[0];
            return new InterventionEffectScore
            {
                AssessmentId = row.Field<int>("AssessmentId"),
                UserId = row.Field<int>("UserId"),
                InterventionType = row.Field<string>("InterventionType"),
                TotalScore = row.Field<int>("TotalScore"),
                AssessmentDate = row.Field<DateTime>("AssessmentDate"),
                AssessmentBy = row.Field<int>("AssessmentBy")
            };
        }
        #endregion

        #region 4. 评估报告相关数据访问
        /// <summary>
        /// 获取患者基础信息（用于报告生成）
        /// </summary>
        public Users GetPatientBaseInfo(int userId)
        {
            string sql = @"SELECT user_id, user_name, gender, age, diabetes_type, diagnose_date, status
                           FROM t_user WHERE user_id = @userId AND user_type = 1";
            SqlParameter[] parameters = { new SqlParameter("@userId", userId) };

            DataTable dt = SqlHelper.ExecuteDataTable(sql, parameters);
            if (dt.Rows.Count == 0) return null;

            DataRow row = dt.Rows[0];
            return new Users
            {
                user_id = row.Field<int>("user_id"),
                user_name = row.Field<string>("user_name"),
                gender = row.Field<byte>("gender"),
                age = row.Field<int>("age"),
                diabetes_type = row.Field<string>("diabetes_type"),
                diagnose_date = row.Field<DateTime?>("diagnose_date"),
                status = row.Field<byte>("status")
            };
        }

        /// <summary>
        /// 获取患者周期内干预方案列表
        /// </summary>
        public List<InterventionPlan> GetPatientInterventionPlans(int userId, DateTime startDate, DateTime endDate)
        {
            string sql = @"SELECT * FROM t_intervention_plan
                           WHERE user_id = @userId 
                           AND create_time BETWEEN @startDate AND @endDate
                           ORDER BY create_time DESC";
            SqlParameter[] parameters = {
                new SqlParameter("@userId", userId),
                new SqlParameter("@startDate", startDate.Date),
                new SqlParameter("@endDate", endDate.Date.AddDays(1).AddSeconds(-1))
            };

            DataTable dt = SqlHelper.ExecuteDataTable(sql, parameters);
            return dt.AsEnumerable().Select(row => new InterventionPlan
            {
                plan_id = row.Field<int>("plan_id"),
                user_id = row.Field<int>("user_id"),
                plan_type = row.Field<string>("plan_type"),
                plan_content = row.Field<string>("plan_content"),
                expected_effect = row.Field<string>("expected_effect"),
                start_time = row.Field<DateTime>("start_time"),
                end_time = row.Field<DateTime>("end_time"),
                create_time = row.Field<DateTime>("create_time"),
                execute_status = row.Field<byte>("execute_status")
            }).ToList();
        }
        #endregion

        #region 公共方法：获取有效患者列表（复用系统逻辑）
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
    }
}