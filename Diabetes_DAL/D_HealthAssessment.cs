using System;
using System.Data;
using System.Data.SqlClient;
using Model;
using Tools;

namespace DAL
{
    public class D_HealthAssessment
    {
        private readonly string _connStr = SqlHelper.connStr;

        /// <summary>
        /// 保存健康评估
        /// </summary>
        public int AddHealthAssessment(HealthAssessment model)
        {
            string sql = @"
INSERT INTO t_health_assessment
(user_id, assessment_date, assessment_type, assessment_by,
height, weight, waist_circumference, hip_circumference,
systolic_bp, diastolic_bp, heart_rate,
glycemic_control_status, hba1c, avg_fasting_glucose, avg_postprandial_glucose,
disease_duration_years, diabetes_complications, comorbidities,
data_completeness, assessment_score, data_version,
create_time, update_time, status, bmi, waist_hip_ratio)
VALUES
(@user_id, @assessment_date, @assessment_type, @assessment_by,
@height, @weight, @waist_circumference, @hip_circumference,
@systolic_bp, @diastolic_bp, @heart_rate,
@glycemic_control_status, @hba1c, @avg_fasting_glucose, @avg_postprandial_glucose,
@disease_duration_years, @diabetes_complications, @comorbidities,
@data_completeness, @assessment_score, @data_version,
@create_time, @update_time, @status, @bmi, @waist_hip_ratio);
SELECT SCOPE_IDENTITY();
";
            SqlParameter[] pms = new SqlParameter[]
            {
                new SqlParameter("@user_id", SqlDbType.Int) { Value = model.user_id },
                new SqlParameter("@assessment_date", SqlDbType.Date) { Value = model.assessment_date },
                new SqlParameter("@assessment_type", SqlDbType.VarChar,20) { Value = model.assessment_type },
                new SqlParameter("@assessment_by", SqlDbType.Int) { Value = model.assessment_by },

                new SqlParameter("@height", SqlDbType.Decimal) { Value = model.height ?? (object)DBNull.Value },
                new SqlParameter("@weight", SqlDbType.Decimal) { Value = model.weight ?? (object)DBNull.Value },
                new SqlParameter("@waist_circumference", SqlDbType.Decimal) { Value = model.waist_circumference ?? (object)DBNull.Value },
                new SqlParameter("@hip_circumference", SqlDbType.Decimal) { Value = model.hip_circumference ?? (object)DBNull.Value },

                new SqlParameter("@systolic_bp", SqlDbType.SmallInt) { Value = model.systolic_bp ?? (object)DBNull.Value },
                new SqlParameter("@diastolic_bp", SqlDbType.SmallInt) { Value = model.diastolic_bp ?? (object)DBNull.Value },
                new SqlParameter("@heart_rate", SqlDbType.SmallInt) { Value = model.heart_rate ?? (object)DBNull.Value },

                new SqlParameter("@glycemic_control_status", SqlDbType.VarChar,20) { Value = model.glycemic_control_status ?? (object)DBNull.Value },
                new SqlParameter("@hba1c", SqlDbType.Decimal) { Value = model.hba1c ?? (object)DBNull.Value },
                new SqlParameter("@avg_fasting_glucose", SqlDbType.Decimal) { Value = model.avg_fasting_glucose ?? (object)DBNull.Value },
                new SqlParameter("@avg_postprandial_glucose", SqlDbType.Decimal) { Value = model.avg_postprandial_glucose ?? (object)DBNull.Value },

                new SqlParameter("@disease_duration_years", SqlDbType.Decimal) { Value = model.disease_duration_years ?? (object)DBNull.Value },
                new SqlParameter("@diabetes_complications", SqlDbType.VarChar,500) { Value = model.diabetes_complications ?? (object)DBNull.Value },
                new SqlParameter("@comorbidities", SqlDbType.VarChar,500) { Value = model.comorbidities ?? (object)DBNull.Value },

                new SqlParameter("@data_completeness", SqlDbType.Int) { Value = model.data_completeness },
                new SqlParameter("@assessment_score", SqlDbType.Decimal) { Value = model.assessment_score ?? (object)DBNull.Value },
                new SqlParameter("@data_version", SqlDbType.Int) { Value = model.data_version },

                new SqlParameter("@create_time", SqlDbType.DateTime) { Value = model.create_time },
                new SqlParameter("@update_time", SqlDbType.DateTime) { Value = model.update_time },
                new SqlParameter("@status", SqlDbType.Int) { Value = model.status },
                new SqlParameter("@bmi", SqlDbType.Decimal) { Value = model.bmi ?? (object)DBNull.Value },
                new SqlParameter("@waist_hip_ratio", SqlDbType.Decimal) { Value = model.waist_hip_ratio ?? (object)DBNull.Value }
            };

            object obj = SqlHelper.ExecuteScalar(sql, pms);
            return obj == null ? -1 : Convert.ToInt32(obj);
        }

        /// <summary>
        /// 获取患者最新评估
        /// </summary>
        public HealthAssessment GetLatestByUserId(int userId)
        {
            string sql = @"
SELECT TOP 1 * FROM t_health_assessment 
WHERE user_id=@user_id AND status=1 
ORDER BY assessment_date DESC";
            return SqlHelper.GetModel<HealthAssessment>(sql,
                new SqlParameter("@user_id", SqlDbType.Int) { Value = userId });
        }

        #region 追加：获取患者最新健康评估记录
        /// <summary>
        /// 获取患者最新的健康评估记录
        /// </summary>
        public HealthAssessment GetLatestHealthAssessment(int userId)
        {
            string sql = @"
            SELECT TOP 1 * FROM t_health_assessment 
            WHERE user_id = @UserId 
              AND status = 1 
            ORDER BY assessment_date DESC";
            SqlParameter[] param = { new SqlParameter("@UserId", userId) };
            try
            {
                return SqlHelper.GetModel<HealthAssessment>(sql, param);
            }
            catch (Exception ex)
            {
                throw new Exception("查询健康评估数据失败：" + ex.Message, ex);
            }
        }
        #endregion
    }
}