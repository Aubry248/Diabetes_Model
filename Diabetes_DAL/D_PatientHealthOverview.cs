using System;
using System.Data;
using System.Data.SqlClient;
using Model;
using Tools;

namespace DAL
{
    /// <summary>
    /// 患者健康概览数据访问层（无侵入式新增）
    /// </summary>
    public class D_PatientHealthOverview
    {
        private readonly string _connStr = SqlHelper.connStr;

        /// <summary>
        /// 获取患者基本信息（脱敏视图+用户表）
        /// </summary>
        public PatientHealthOverview GetPatientBaseInfo(int userId)
        {
            try
            {
                string sql = @"
                SELECT 
                    user_id, user_name, gender, age, diabetes_type, diagnose_date,
                    DATEDIFF(YEAR, diagnose_date, GETDATE()) as disease_duration_years
                FROM t_user 
                WHERE user_id = @user_id AND user_type = 1 AND status = 1";

                SqlParameter[] param = { new SqlParameter("@user_id", userId) };
                DataTable dt = SqlHelper.ExecuteDataTable(sql, param);

                if (dt == null || dt.Rows.Count == 0)
                    return null;

                DataRow dr = dt.Rows[0];
                PatientHealthOverview model = new PatientHealthOverview
                {
                    user_id = Convert.ToInt32(dr["user_id"]),
                    user_name = dr["user_name"].ToString(),
                    gender = Convert.ToInt32(dr["gender"]),
                    age = Convert.ToInt32(dr["age"]),
                    diabetes_type = dr["diabetes_type"]?.ToString(),
                    diagnose_date = dr["diagnose_date"] != DBNull.Value ? Convert.ToDateTime(dr["diagnose_date"]) : (DateTime?)null,
                    disease_duration_years = dr["disease_duration_years"] != DBNull.Value ? Convert.ToDecimal(dr["disease_duration_years"]) : (decimal?)null
                };

                // 获取最新健康评估的身高/体重/BMI
                string assessSql = @"
                SELECT TOP 1 height, weight, bmi 
                FROM t_health_assessment 
                WHERE user_id = @user_id AND status = 1 
                ORDER BY assessment_date DESC";
                DataTable assessDt = SqlHelper.ExecuteDataTable(assessSql, param);
                if (assessDt != null && assessDt.Rows.Count > 0)
                {
                    DataRow assessDr = assessDt.Rows[0];
                    model.latest_height = assessDr["height"] != DBNull.Value ? Convert.ToDecimal(assessDr["height"]) : (decimal?)null;
                    model.latest_weight = assessDr["weight"] != DBNull.Value ? Convert.ToDecimal(assessDr["weight"]) : (decimal?)null;
                    model.latest_bmi = assessDr["bmi"] != DBNull.Value ? Convert.ToDecimal(assessDr["bmi"]) : (decimal?)null;
                }

                return model;
            }
            catch (Exception ex)
            {
                throw new Exception("获取患者基本信息失败：" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 获取所有启用患者列表（下拉选择用，脱敏处理）
        /// </summary>
        public DataTable GetAllPatientList()
        {
            try
            {
                string sql = @"
                SELECT user_id, user_name + '(' + id_card_desensitized + ')' as patient_display
                FROM v_patient_data_desensitized 
                WHERE status = 1 
                ORDER BY user_name ASC";
                return SqlHelper.ExecuteDataTable(sql);
            }
            catch (Exception ex)
            {
                throw new Exception("获取患者列表失败：" + ex.Message, ex);
            }
        }
    }
}