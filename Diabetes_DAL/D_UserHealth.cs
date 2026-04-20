using System;
using System.Data;
using System.Data.SqlClient;
using Tools;

namespace DAL
{
    /// <summary>
    /// 用户健康数据访问层
    /// </summary>
    public class D_UserHealth
    {
        /// <summary>
        /// 获取用户健康信息（用于个性化建议）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户健康数据表</returns>
        public DataTable GetUserHealthInfo(int userId)
        {
            string sql = @"
                -- 近7天血糖平均值
                DECLARE @AvgFastingBS decimal(5,1), @AvgPostprandialBS decimal(5,1)
                SELECT 
                    @AvgFastingBS = ISNULL(AVG(blood_sugar_value),0)
                FROM t_blood_sugar 
                WHERE user_id = @userId AND measurement_scenario = '空腹' 
                AND measurement_time >= DATEADD(DAY,-7,GETDATE()) AND data_status = 0

                SELECT 
                    @AvgPostprandialBS = ISNULL(AVG(blood_sugar_value),0)
                FROM t_blood_sugar 
                WHERE user_id = @userId AND measurement_scenario = '餐后2小时' 
                AND measurement_time >= DATEADD(DAY,-7,GETDATE()) AND data_status = 0

                -- 用户基础信息
                SELECT 
                    @AvgFastingBS AS AvgFastingBloodSugar,
                    @AvgPostprandialBS AS AvgPostprandialBloodSugar,
                    ISNULL(disease_duration_years,0) AS DiseaseDurationYears,
                    ISNULL(diabetes_complications,'') AS DiabetesComplications,
                    ISNULL(weight,0) AS Weight,
                    ISNULL(height,0) AS Height,
                    -- 是否有用药记录
                    CASE WHEN EXISTS(SELECT 1 FROM t_medicine WHERE user_id = @userId) THEN 1 ELSE 0 END AS HasMedicineRecord
                FROM t_health_assessment 
                WHERE user_id = @userId 
                ORDER BY assessment_date DESC 
                OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY";
            SqlParameter[] param = { new SqlParameter("@userId", userId) };
            return SqlHelper.ExecuteDataTable(sql, param);
        }
    }
}