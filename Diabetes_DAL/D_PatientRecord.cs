using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Model;
using Tools;

namespace DAL
{
    /// <summary>
    /// 患者档案管理数据访问层
    /// </summary>
    public class D_PatientRecord
    {
        #region 1. 获取患者档案分页列表（全维度查询）
        /// <summary>
        /// 获取患者档案分页列表，支持多条件查询
        /// </summary>
        /// <param name="pageIndex">页码（从1开始）</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="totalCount">输出总条数</param>
        /// <param name="userName">患者姓名（模糊查询）</param>
        /// <param name="phone">手机号（模糊查询）</param>
        /// <param name="diabetesType">糖尿病类型（精准匹配）</param>
        /// <param name="controlStatus">血糖控制状态（精准匹配）</param>
        /// <param name="startDiagnoseDate">确诊开始日期</param>
        /// <param name="endDiagnoseDate">确诊结束日期</param>
        /// <param name="onlyValid">仅查询启用状态患者</param>
        /// <returns>患者档案列表</returns>
        public List<PatientArchive> GetPatientArchivePageList(int pageIndex, int pageSize, out int totalCount,
            string userName = "", string phone = "", string diabetesType = "", string controlStatus = "",
            DateTime? startDiagnoseDate = null, DateTime? endDiagnoseDate = null, bool onlyValid = true)
        {
            // 先查询总条数
            string countSql = @"
SELECT COUNT(1) 
FROM t_user u
LEFT JOIN (
    SELECT user_id, MAX(assessment_date) AS latest_assess_date 
    FROM t_health_assessment 
    WHERE status=1 
    GROUP BY user_id
) ha_latest ON u.user_id = ha_latest.user_id
LEFT JOIN t_health_assessment ha ON ha_latest.user_id = ha.user_id AND ha_latest.latest_assess_date = ha.assessment_date AND ha.status=1
WHERE u.user_type = 1"; // 仅查询患者类型用户

            List<SqlParameter> countParam = new List<SqlParameter>();
            // 拼接查询条件
            string whereSql = BuildQueryWhereSql(countParam, userName, phone, diabetesType, controlStatus, startDiagnoseDate, endDiagnoseDate, onlyValid);
            countSql += whereSql;

            totalCount = Convert.ToInt32(SqlHelper.GetSingle(countSql, countParam.ToArray()));
            if (totalCount == 0) return new List<PatientArchive>();

            // 分页查询主数据
            string dataSql = @"
SELECT * FROM (
    SELECT 
        u.*,
        ha.assessment_id, ha.assessment_date, ha.hba1c, ha.glycemic_control_status, ha.diabetes_complications, ha.comorbidities, ha.disease_duration_years,
        -- 近30天血糖统计
        (SELECT AVG(blood_sugar_value) FROM t_blood_sugar bs WHERE bs.user_id=u.user_id AND bs.measurement_scenario='空腹' AND bs.measurement_time >= DATEADD(DAY,-30,GETDATE()) AND bs.data_status=1) AS avg_fasting_glucose,
        (SELECT AVG(blood_sugar_value) FROM t_blood_sugar bs WHERE bs.user_id=u.user_id AND bs.measurement_scenario='餐后2小时' AND bs.measurement_time >= DATEADD(DAY,-30,GETDATE()) AND bs.data_status=1) AS avg_postprandial_glucose,
        (SELECT COUNT(1) FROM t_blood_sugar bs WHERE bs.user_id=u.user_id AND bs.is_abnormal=1 AND bs.measurement_time >= DATEADD(DAY,-30,GETDATE()) AND bs.data_status=1) AS abnormal_bs_count,
        ROW_NUMBER() OVER (ORDER BY u.create_time DESC) AS row_num
    FROM t_user u
    LEFT JOIN (
        SELECT user_id, MAX(assessment_date) AS latest_assess_date 
        FROM t_health_assessment 
        WHERE status=1 
        GROUP BY user_id
    ) ha_latest ON u.user_id = ha_latest.user_id
    LEFT JOIN t_health_assessment ha ON ha_latest.user_id = ha.user_id AND ha_latest.latest_assess_date = ha.assessment_date AND ha.status=1
    WHERE u.user_type = 1" + whereSql + @"
) AS temp_table
WHERE row_num BETWEEN @StartRow AND @EndRow
ORDER BY row_num ASC";

            List<SqlParameter> dataParam = new List<SqlParameter>(countParam);
            int startRow = (pageIndex - 1) * pageSize + 1;
            int endRow = pageIndex * pageSize;
            dataParam.Add(new SqlParameter("@StartRow", startRow));
            dataParam.Add(new SqlParameter("@EndRow", endRow));

            DataTable dt = SqlHelper.ExecuteDataTable(dataSql, dataParam.ToArray());
            List<PatientArchive> list = new List<PatientArchive>();

            foreach (DataRow dr in dt.Rows)
            {
                PatientArchive archive = new PatientArchive();
                // 填充基础信息
                archive.BaseInfo = new Users
                {
                    user_id = Convert.ToInt32(dr["user_id"]),
                    user_name = dr["user_name"].ToString(),
                    id_card = dr["id_card"].ToString(),
                    phone = dr["phone"].ToString(),
                    emergency_contact = dr["emergency_contact"]?.ToString(),
                    emergency_phone = dr["emergency_phone"]?.ToString(),
                    password = dr["password"].ToString(),
                    user_type = Convert.ToInt32(dr["user_type"]),
                    diabetes_type = dr["diabetes_type"]?.ToString(),
                    diagnose_date = dr["diagnose_date"] as DateTime?,
                    fasting_glucose_baseline = dr["fasting_glucose_baseline"] as decimal?,
                    last_login_time = dr["last_login_time"] as DateTime?,
                    data_version = Convert.ToInt32(dr["data_version"]),
                    create_time = Convert.ToDateTime(dr["create_time"]),
                    update_time = Convert.ToDateTime(dr["update_time"]),
                    status = Convert.ToInt32(dr["status"]),
                    gender = Convert.ToInt32(dr["gender"]),
                    age = Convert.ToInt32(dr["age"]),
                    birth_date = dr["birth_date"] as DateTime?,
                    login_account = dr["login_account"].ToString()
                };

                // 填充最新健康评估
                if (dr["assessment_id"] != DBNull.Value)
                {
                    archive.LatestAssessment = new HealthAssessment
                    {
                        assessment_id = Convert.ToInt32(dr["assessment_id"]),
                        user_id = archive.BaseInfo.user_id,
                        assessment_date = Convert.ToDateTime(dr["assessment_date"]),
                        hba1c = dr["hba1c"] as decimal?,
                        glycemic_control_status = dr["glycemic_control_status"]?.ToString(),
                        diabetes_complications = dr["diabetes_complications"]?.ToString(),
                        comorbidities = dr["comorbidities"]?.ToString(),
                        disease_duration_years = dr["disease_duration_years"] as decimal?
                    };
                    // 拆分并发症/合并症数组
                    archive.DiabetesComplications = archive.LatestAssessment.diabetes_complications?.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                    archive.Comorbidities = archive.LatestAssessment.comorbidities?.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                    archive.LatestHbA1c = archive.LatestAssessment.hba1c;
                    archive.GlycemicControlStatus = archive.LatestAssessment.glycemic_control_status;
                }

                // 填充血糖统计数据
                archive.AvgFastingGlucose = dr["avg_fasting_glucose"] as decimal?;
                archive.AvgPostprandialGlucose = dr["avg_postprandial_glucose"] as decimal?;
                archive.AbnormalBloodSugarCount = dr["abnormal_bs_count"] != DBNull.Value ? Convert.ToInt32(dr["abnormal_bs_count"]) : 0;

                list.Add(archive);
            }
            return list;
        }

        /// <summary>
        /// 构建查询条件SQL（内部私有方法）
        /// </summary>
        private string BuildQueryWhereSql(List<SqlParameter> param, string userName, string phone, string diabetesType,
            string controlStatus, DateTime? startDiagnoseDate, DateTime? endDiagnoseDate, bool onlyValid)
        {
            string whereSql = "";
            if (onlyValid)
            {
                whereSql += " AND u.status = 1";
            }
            if (!string.IsNullOrEmpty(userName))
            {
                whereSql += " AND u.user_name LIKE @UserName";
                param.Add(new SqlParameter("@UserName", "%" + userName + "%"));
            }
            if (!string.IsNullOrEmpty(phone))
            {
                whereSql += " AND u.phone LIKE @Phone";
                param.Add(new SqlParameter("@Phone", "%" + phone + "%"));
            }
            if (!string.IsNullOrEmpty(diabetesType))
            {
                whereSql += " AND u.diabetes_type = @DiabetesType";
                param.Add(new SqlParameter("@DiabetesType", diabetesType));
            }
            if (!string.IsNullOrEmpty(controlStatus))
            {
                whereSql += " AND ha.glycemic_control_status = @ControlStatus";
                param.Add(new SqlParameter("@ControlStatus", controlStatus));
            }
            if (startDiagnoseDate.HasValue)
            {
                whereSql += " AND u.diagnose_date >= @StartDiagnoseDate";
                param.Add(new SqlParameter("@StartDiagnoseDate", startDiagnoseDate.Value));
            }
            if (endDiagnoseDate.HasValue)
            {
                whereSql += " AND u.diagnose_date <= @EndDiagnoseDate";
                param.Add(new SqlParameter("@EndDiagnoseDate", endDiagnoseDate.Value.AddDays(1).AddSeconds(-1)));
            }
            return whereSql;
        }
        #endregion

        #region 2. 获取患者档案详情
        /// <summary>
        /// 获取单个患者全维度档案详情
        /// </summary>
        /// <param name="userId">患者ID</param>
        /// <returns>患者档案详情</returns>
        public PatientArchive GetPatientArchiveDetail(int userId)
        {
            string sql = @"
SELECT 
    u.*,
    ha.assessment_id, ha.assessment_date, ha.assessment_type, ha.assessment_by,
    ha.height, ha.weight, ha.waist_circumference, ha.hip_circumference,
    ha.systolic_bp, ha.diastolic_bp, ha.heart_rate, ha.glycemic_control_status,
    ha.hba1c, ha.avg_fasting_glucose, ha.avg_postprandial_glucose,
    ha.disease_duration_years, ha.diabetes_complications, ha.comorbidities,
    ha.data_completeness, ha.assessment_score, ha.bmi, ha.waist_hip_ratio,
    -- 近30天血糖统计
    (SELECT AVG(blood_sugar_value) FROM t_blood_sugar bs WHERE bs.user_id=u.user_id AND bs.measurement_scenario='空腹' AND bs.measurement_time >= DATEADD(DAY,-30,GETDATE()) AND bs.data_status=1) AS avg_fasting_glucose,
    (SELECT AVG(blood_sugar_value) FROM t_blood_sugar bs WHERE bs.user_id=u.user_id AND bs.measurement_scenario='餐后2小时' AND bs.measurement_time >= DATEADD(DAY,-30,GETDATE()) AND bs.data_status=1) AS avg_postprandial_glucose,
    (SELECT COUNT(1) FROM t_blood_sugar bs WHERE bs.user_id=u.user_id AND bs.is_abnormal=1 AND bs.measurement_time >= DATEADD(DAY,-30,GETDATE()) AND bs.data_status=1) AS abnormal_bs_count
FROM t_user u
LEFT JOIN (
    SELECT user_id, MAX(assessment_date) AS latest_assess_date 
    FROM t_health_assessment 
    WHERE status=1 AND user_id=@UserId
    GROUP BY user_id
) ha_latest ON u.user_id = ha_latest.user_id
LEFT JOIN t_health_assessment ha ON ha_latest.user_id = ha.user_id AND ha_latest.latest_assess_date = ha.assessment_date AND ha.status=1
WHERE u.user_id = @UserId AND u.user_type=1";

            SqlParameter[] param = { new SqlParameter("@UserId", userId) };
            DataTable dt = SqlHelper.ExecuteDataTable(sql, param);
            if (dt.Rows.Count == 0) return null;

            DataRow dr = dt.Rows[0];
            PatientArchive archive = new PatientArchive();

            // 填充基础信息
            archive.BaseInfo = new Users
            {
                user_id = Convert.ToInt32(dr["user_id"]),
                user_name = dr["user_name"].ToString(),
                id_card = dr["id_card"].ToString(),
                phone = dr["phone"].ToString(),
                emergency_contact = dr["emergency_contact"]?.ToString(),
                emergency_phone = dr["emergency_phone"]?.ToString(),
                password = dr["password"].ToString(),
                user_type = Convert.ToInt32(dr["user_type"]),
                diabetes_type = dr["diabetes_type"]?.ToString(),
                diagnose_date = dr["diagnose_date"] as DateTime?,
                fasting_glucose_baseline = dr["fasting_glucose_baseline"] as decimal?,
                last_login_time = dr["last_login_time"] as DateTime?,
                data_version = Convert.ToInt32(dr["data_version"]),
                create_time = Convert.ToDateTime(dr["create_time"]),
                update_time = Convert.ToDateTime(dr["update_time"]),
                status = Convert.ToInt32(dr["status"]),
                gender = Convert.ToInt32(dr["gender"]),
                age = Convert.ToInt32(dr["age"]),
                birth_date = dr["birth_date"] as DateTime?,
                login_account = dr["login_account"].ToString()
            };

            // 填充健康评估信息
            if (dr["assessment_id"] != DBNull.Value)
            {
                archive.LatestAssessment = new HealthAssessment
                {
                    assessment_id = Convert.ToInt32(dr["assessment_id"]),
                    user_id = userId,
                    assessment_date = Convert.ToDateTime(dr["assessment_date"]),
                    assessment_type = dr["assessment_type"].ToString(),
                    assessment_by = Convert.ToInt32(dr["assessment_by"]),
                    height = dr["height"] as decimal?,
                    weight = dr["weight"] as decimal?,
                    waist_circumference = dr["waist_circumference"] as decimal?,
                    hip_circumference = dr["hip_circumference"] as decimal?,
                    systolic_bp = dr["systolic_bp"] == DBNull.Value ? null : (short?)Convert.ToInt16(dr["systolic_bp"]),
                    diastolic_bp = dr["diastolic_bp"] == DBNull.Value ? null : (short?)Convert.ToInt16(dr["diastolic_bp"]),
                    heart_rate = dr["heart_rate"] == DBNull.Value ? null : (short?)Convert.ToInt16(dr["heart_rate"]),
                    glycemic_control_status = dr["glycemic_control_status"]?.ToString(),
                    hba1c = dr["hba1c"] as decimal?,
                    avg_fasting_glucose = dr["avg_fasting_glucose"] as decimal?,
                    avg_postprandial_glucose = dr["avg_postprandial_glucose"] as decimal?,
                    disease_duration_years = dr["disease_duration_years"] as decimal?,
                    diabetes_complications = dr["diabetes_complications"]?.ToString(),
                    comorbidities = dr["comorbidities"]?.ToString(),
                    data_completeness = Convert.ToInt32(dr["data_completeness"]),
                    assessment_score = dr["assessment_score"] as decimal?,
                    bmi = dr["bmi"] as decimal?,
                    waist_hip_ratio = dr["waist_hip_ratio"] as decimal?,
                    status = Convert.ToInt32(dr["status"]),
                    data_version = Convert.ToInt32(dr["data_version"]),
                    create_time = Convert.ToDateTime(dr["create_time"]),
                    update_time = Convert.ToDateTime(dr["update_time"])
                };
                archive.DiabetesComplications = archive.LatestAssessment.diabetes_complications?.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                archive.Comorbidities = archive.LatestAssessment.comorbidities?.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                archive.LatestHbA1c = archive.LatestAssessment.hba1c;
                archive.GlycemicControlStatus = archive.LatestAssessment.glycemic_control_status;
            }

            // 填充血糖统计
            archive.AvgFastingGlucose = dr["avg_fasting_glucose"] as decimal?;
            archive.AvgPostprandialGlucose = dr["avg_postprandial_glucose"] as decimal?;
            archive.AbnormalBloodSugarCount = dr["abnormal_bs_count"] != DBNull.Value ? Convert.ToInt32(dr["abnormal_bs_count"]) : 0;

            return archive;
        }
        #endregion

        #region 3. 维护患者病程/并发症/合并症信息
        /// <summary>
        /// 新增/更新患者病程与并发症信息（新增健康评估版本，实现历史追溯）
        /// </summary>
        /// <param name="assessment">健康评估实体</param>
        /// <returns>新增的评估ID，失败返回0</returns>
        public int SavePatientDiseaseInfo(HealthAssessment assessment)
        {
            // 新增评估记录，不修改历史数据，实现版本化管理
            string sql = @"
INSERT INTO t_health_assessment (
    user_id, assessment_date, assessment_type, assessment_by, height, weight, waist_circumference, hip_circumference,
    systolic_bp, diastolic_bp, heart_rate, glycemic_control_status, hba1c, avg_fasting_glucose, avg_postprandial_glucose,
    disease_duration_years, diabetes_complications, comorbidities, data_completeness, assessment_score, bmi, waist_hip_ratio,
    status, data_version, create_time, update_time
) VALUES (
    @user_id, @assessment_date, @assessment_type, @assessment_by, @height, @weight, @waist_circumference, @hip_circumference,
    @systolic_bp, @diastolic_bp, @heart_rate, @glycemic_control_status, @hba1c, @avg_fasting_glucose, @avg_postprandial_glucose,
    @disease_duration_years, @diabetes_complications, @comorbidities, @data_completeness, @assessment_score, @bmi, @waist_hip_ratio,
    1, 1, GETDATE(), GETDATE()
);
SELECT SCOPE_IDENTITY();";

            SqlParameter[] param = {
                new SqlParameter("@user_id", assessment.user_id),
                new SqlParameter("@assessment_date", assessment.assessment_date),
                new SqlParameter("@assessment_type", assessment.assessment_type),
                new SqlParameter("@assessment_by", assessment.assessment_by),
                new SqlParameter("@height", assessment.height ?? (object)DBNull.Value),
                new SqlParameter("@weight", assessment.weight ?? (object)DBNull.Value),
                new SqlParameter("@waist_circumference", assessment.waist_circumference ?? (object)DBNull.Value),
                new SqlParameter("@hip_circumference", assessment.hip_circumference ?? (object)DBNull.Value),
                new SqlParameter("@systolic_bp", assessment.systolic_bp ?? (object)DBNull.Value),
                new SqlParameter("@diastolic_bp", assessment.diastolic_bp ?? (object)DBNull.Value),
                new SqlParameter("@heart_rate", assessment.heart_rate ?? (object)DBNull.Value),
                new SqlParameter("@glycemic_control_status", assessment.glycemic_control_status ?? (object)DBNull.Value),
                new SqlParameter("@hba1c", assessment.hba1c ?? (object)DBNull.Value),
                new SqlParameter("@avg_fasting_glucose", assessment.avg_fasting_glucose ?? (object)DBNull.Value),
                new SqlParameter("@avg_postprandial_glucose", assessment.avg_postprandial_glucose ?? (object)DBNull.Value),
                new SqlParameter("@disease_duration_years", assessment.disease_duration_years ?? (object)DBNull.Value),
                new SqlParameter("@diabetes_complications", assessment.diabetes_complications ?? (object)DBNull.Value),
                new SqlParameter("@comorbidities", assessment.comorbidities ?? (object)DBNull.Value),
                new SqlParameter("@data_completeness", assessment.data_completeness),
                new SqlParameter("@assessment_score", assessment.assessment_score ?? (object)DBNull.Value),
                new SqlParameter("@bmi", assessment.bmi ?? (object)DBNull.Value),
                new SqlParameter("@waist_hip_ratio", assessment.waist_hip_ratio ?? (object)DBNull.Value)
            };

            object result = SqlHelper.GetSingle(sql, param);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        /// <summary>
        /// 更新患者基础患病信息（糖尿病类型、确诊日期等）
        /// </summary>
        /// <param name="user">用户实体</param>
        /// <returns>是否更新成功</returns>
        public bool UpdatePatientBaseDiseaseInfo(Users user)
        {
            string sql = @"
UPDATE t_user SET 
diabetes_type = @diabetes_type,
diagnose_date = @diagnose_date,
fasting_glucose_baseline = @fasting_glucose_baseline,
update_time = GETDATE(),
data_version = data_version + 1
WHERE user_id = @user_id AND user_type=1";

            SqlParameter[] param = {
                new SqlParameter("@diabetes_type", user.diabetes_type ?? (object)DBNull.Value),
                new SqlParameter("@diagnose_date", user.diagnose_date ?? (object)DBNull.Value),
                new SqlParameter("@fasting_glucose_baseline", user.fasting_glucose_baseline ?? (object)DBNull.Value),
                new SqlParameter("@user_id", user.user_id)
            };

            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }
        #endregion

        #region 4. 患者健康档案历史追溯
        /// <summary>
        /// 获取患者健康档案历史记录全量列表
        /// </summary>
        /// <param name="userId">患者ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="recordType">记录类型（空则查询全部）</param>
        /// <returns>历史记录列表</returns>
        public List<PatientArchiveHistory> GetPatientArchiveHistory(int userId, DateTime? startTime = null, DateTime? endTime = null, string recordType = "")
        {
            // 多表联合查询，整合全业务线历史记录
            string sql = @"
            -- 评估记录
            SELECT 
                assessment_id AS record_id,
                user_id,
                '评估记录' AS record_type,
                assessment_id AS related_id,
                assessment_date AS record_time,
                '评估类型：'+assessment_type+'，血糖控制状态：'+ISNULL(glycemic_control_status,'未填写')+'，HbA1c：'+ISNULL(CAST(hba1c AS VARCHAR),'未检测') AS record_summary,
                assessment_by AS operator_id,
                (SELECT user_name FROM t_user WHERE user_id=ha.assessment_by) AS operator_name
            FROM t_health_assessment ha
            WHERE user_id = @UserId
            UNION ALL
            -- 血糖监测
            SELECT 
                blood_sugar_id AS record_id,
                user_id,
                '血糖监测' AS record_type,
                blood_sugar_id AS related_id,
                measurement_time AS record_time,
                '测量场景：'+measurement_scenario+'，血糖值：'+CAST(blood_sugar_value AS VARCHAR)+'mmol/L，异常状态：'+CASE WHEN is_abnormal=1 THEN '异常' ELSE '正常' END AS record_summary,
                operator_id AS operator_id,
                (SELECT user_name FROM t_user WHERE user_id=bs.operator_id) AS operator_name
            FROM t_blood_sugar bs
            WHERE user_id = @UserId
            UNION ALL
            -- 干预方案
            SELECT 
                plan_id AS record_id,
                user_id,
                '干预方案' AS record_type,
                plan_id AS related_id,
                create_time AS record_time,
                '方案类型：'+plan_type+'，执行状态：'+CASE execute_status WHEN 0 THEN '未执行' WHEN 1 THEN '执行中' WHEN 2 THEN '已完成' END AS record_summary,
                create_by AS operator_id,
                (SELECT user_name FROM t_user WHERE user_id=ip.create_by) AS operator_name
            FROM t_intervention_plan ip
            WHERE user_id = @UserId
            UNION ALL
            -- 用药调整
            SELECT 
                medicine_id AS record_id,
                user_id,
                '用药调整' AS record_type,
                medicine_id AS related_id,
                take_medicine_time AS record_time,
                '药品名称：'+drug_name+'，用药剂量：'+CAST(drug_dosage AS VARCHAR) AS record_summary,
                operator_id AS operator_id,
                (SELECT user_name FROM t_user WHERE user_id=med.operator_id) AS operator_name
            FROM t_medicine med
            WHERE user_id = @UserId
            UNION ALL
            -- 随访记录（新增）
            SELECT 
                follow_up_id AS record_id,
                user_id,
                '随访记录' AS record_type,
                follow_up_id AS related_id,
                follow_up_time AS record_time,
                '随访方式：'+follow_up_way+'，随访结果：'+follow_up_result AS record_summary,
                follow_up_by AS operator_id,
                (SELECT user_name FROM t_user WHERE user_id=fu.follow_up_by) AS operator_name
            FROM t_follow_up fu
            WHERE user_id = @UserId
            UNION ALL
            -- 体检记录（映射健康评估，可根据业务调整）
            SELECT 
                assessment_id AS record_id,
                user_id,
                '体检记录' AS record_type,
                assessment_id AS related_id,
                assessment_date AS record_time,
                '身高：'+ISNULL(CAST(height AS VARCHAR),'未填')+'cm，体重：'+ISNULL(CAST(weight AS VARCHAR),'未填')+'kg，BMI：'+ISNULL(CAST(bmi AS VARCHAR),'未填') AS record_summary,
                assessment_by AS operator_id,
                (SELECT user_name FROM t_user WHERE user_id=ha.assessment_by) AS operator_name
            FROM t_health_assessment ha
            WHERE user_id = @UserId
            UNION ALL
            -- 饮食记录
            SELECT 
                diet_id AS record_id,
                user_id,
                '饮食记录' AS record_type,
                diet_id AS related_id,
                meal_time AS record_time,
                '用餐类型：'+meal_type+'，食物名称：'+food_name+'，摄入热量：'+CAST(actual_calorie AS VARCHAR)+'kcal' AS record_summary,
                operator_id AS operator_id,
                (SELECT user_name FROM t_user WHERE user_id=diet.operator_id) AS operator_name
            FROM t_diet diet
            WHERE user_id = @UserId
            UNION ALL
            -- 运动记录
            SELECT 
                exercise_id AS record_id,
                user_id,
                '运动记录' AS record_type,
                exercise_id AS related_id,
                exercise_time AS record_time,
                '运动类型：'+exercise_type+'，运动时长：'+CAST(exercise_duration AS VARCHAR)+'分钟，消耗热量：'+CAST(actual_calorie AS VARCHAR)+'kcal' AS record_summary,
                operator_id AS operator_id,
                (SELECT user_name FROM t_user WHERE user_id=ex.operator_id) AS operator_name
            FROM t_exercise ex
            WHERE user_id = @UserId
            ";

            // 拼接时间和类型筛选
            List<SqlParameter> param = new List<SqlParameter>();
            param.Add(new SqlParameter("@UserId", userId));
            string whereFilter = "";
            if (startTime.HasValue)
            {
                whereFilter += " AND record_time >= @StartTime";
                param.Add(new SqlParameter("@StartTime", startTime.Value));
            }
            if (endTime.HasValue)
            {
                whereFilter += " AND record_time <= @EndTime";
                param.Add(new SqlParameter("@EndTime", endTime.Value.AddDays(1).AddSeconds(-1)));
            }
            if (!string.IsNullOrEmpty(recordType))
            {
                whereFilter += " AND record_type = @RecordType";
                param.Add(new SqlParameter("@RecordType", recordType));
            }

            if (!string.IsNullOrEmpty(whereFilter))
            {
                sql = $"SELECT * FROM ({sql}) AS all_history WHERE 1=1 {whereFilter} ORDER BY record_time DESC";
            }
            else
            {
                sql += " ORDER BY record_time DESC";
            }

            DataTable dt = SqlHelper.ExecuteDataTable(sql, param.ToArray());
            List<PatientArchiveHistory> list = new List<PatientArchiveHistory>();

            foreach (DataRow dr in dt.Rows)
            {
                PatientArchiveHistory history = new PatientArchiveHistory
                {
                    record_id = Convert.ToInt32(dr["record_id"]),
                    user_id = Convert.ToInt32(dr["user_id"]),
                    record_type = dr["record_type"].ToString(),
                    related_id = Convert.ToInt32(dr["related_id"]),
                    record_time = Convert.ToDateTime(dr["record_time"]),
                    record_summary = dr["record_summary"].ToString(),
                    operator_id = Convert.ToInt32(dr["operator_id"]),
                    operator_name = dr["operator_name"]?.ToString()
                };
                list.Add(history);
            }
            return list;
        }

        /// <summary>
        /// 获取患者历史健康评估列表（用于版本对比）
        /// </summary>
        /// <param name="userId">患者ID</param>
        /// <returns>历史评估列表</returns>
        public List<HealthAssessment> GetPatientHistoryAssessmentList(int userId)
        {
            string sql = @"
SELECT * FROM t_health_assessment 
WHERE user_id = @UserId AND status=1
ORDER BY assessment_date DESC";
            SqlParameter[] param = { new SqlParameter("@UserId", userId) };
            return SqlHelper.GetModelList<HealthAssessment>(sql, param);
        }
        #endregion

        #region 5. 新增：患者简易列表查询（用于追溯页下拉选择）
        /// <summary>
        /// 获取患者简易列表，用于下拉选择，支持姓名/电话模糊搜索
        /// </summary>
        /// <param name="searchText">搜索关键词</param>
        /// <returns>患者简易信息列表</returns>
        public List<Users> GetPatientSimpleList(string searchText = "")
        {
            string sql = @"
SELECT user_id, user_name, phone 
FROM t_user 
WHERE user_type = 1 AND status = 1";
            List<SqlParameter> param = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(searchText))
            {
                sql += " AND (user_name LIKE @SearchText OR phone LIKE @SearchText)";
                param.Add(new SqlParameter("@SearchText", "%" + searchText + "%"));
            }
            sql += " ORDER BY create_time DESC";

            DataTable dt = SqlHelper.ExecuteDataTable(sql, param.ToArray());
            List<Users> list = new List<Users>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new Users
                {
                    user_id = Convert.ToInt32(dr["user_id"]),
                    user_name = dr["user_name"].ToString(),
                    phone = dr["phone"].ToString()
                });
            }
            return list;
        }
        #endregion
    }
}