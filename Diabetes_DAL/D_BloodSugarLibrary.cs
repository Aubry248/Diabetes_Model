// DAL/D_BloodSugarLibrary.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Model;
using Tools;

namespace DAL
{
    /// <summary>
    /// 血糖数据管理数据访问层
    /// </summary>
    public class D_BloodSugarLibrary
    {
        #region 核心：多条件分页查询血糖数据
        /// <summary>
        /// 多条件分页查询血糖数据列表
        /// </summary>
        public DataTable GetBloodSugarListByPage(int userId, string scenario, int isAbnormal, string dataSource,
            DateTime startTime, DateTime endTime, out int totalCount)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(@"
                SELECT 
                    bs.blood_sugar_id, bs.user_id, u.user_name, bs.blood_sugar_value, 
                    bs.measurement_time, bs.measurement_scenario, bs.data_source, 
                    bs.is_abnormal, bs.abnormal_note, bs.create_time, 
                    op.user_name AS operator_name
                FROM t_blood_sugar bs
                LEFT JOIN t_user u ON bs.user_id = u.user_id
                LEFT JOIN t_user op ON bs.operator_id = op.user_id
               WHERE bs.data_status = 1 ");

            List<SqlParameter> paramList = new List<SqlParameter>();

            // 动态拼接筛选条件
            if (userId > 0)
            {
                sql.Append(" AND bs.user_id = @UserId ");
                paramList.Add(new SqlParameter("@UserId", userId));
            }
            if (!string.IsNullOrWhiteSpace(scenario) && scenario != "全部")
            {
                sql.Append(" AND bs.measurement_scenario = @Scenario ");
                paramList.Add(new SqlParameter("@Scenario", scenario));
            }
            if (isAbnormal != -1)
            {
                sql.Append(" AND bs.is_abnormal = @IsAbnormal ");
                paramList.Add(new SqlParameter("@IsAbnormal", isAbnormal));
            }
            if (!string.IsNullOrWhiteSpace(dataSource) && dataSource != "全部")
            {
                sql.Append(" AND bs.data_source = @DataSource ");
                paramList.Add(new SqlParameter("@DataSource", dataSource));
            }
            // 时间范围筛选
            if (startTime > DateTime.Now.AddYears(-10) || endTime < DateTime.Now.AddDays(1))
            {
                sql.Append(" AND bs.measurement_time BETWEEN @StartTime AND @EndTime ");
                paramList.Add(new SqlParameter("@StartTime", startTime.Date));
                paramList.Add(new SqlParameter("@EndTime", endTime.Date.AddDays(1).AddSeconds(-1)));
            }

            // 查询总条数
            string countSql = $"SELECT COUNT(1) FROM ({sql.ToString()}) AS t";
            totalCount = Convert.ToInt32(SqlHelper.ExecuteScalar(countSql, paramList.ToArray()));

            // 排序返回数据（按测量时间倒序）
            sql.Append(" ORDER BY bs.measurement_time DESC ");
            return SqlHelper.ExecuteDataTable(sql.ToString(), paramList.ToArray());
        }
        #endregion

        #region 基础CRUD操作
        /// <summary>
        /// 根据ID获取血糖详情
        /// </summary>
        public BloodSugar GetBloodSugarById(int bloodSugarId)
        {
            string sql = @"
                SELECT 
                    bs.*, u.user_name, op.user_name AS operator_name
                FROM t_blood_sugar bs
                LEFT JOIN t_user u ON bs.user_id = u.user_id
                LEFT JOIN t_user op ON bs.operator_id = op.user_id
                WHERE bs.blood_sugar_id = @BloodSugarId AND bs.data_status = 0";

            SqlParameter[] param = { new SqlParameter("@BloodSugarId", bloodSugarId) };
            DataTable dt = SqlHelper.ExecuteDataTable(sql, param);

            if (dt == null || dt.Rows.Count == 0) return null;

            DataRow dr = dt.Rows[0];
            return new BloodSugar
            {
                blood_sugar_id = Convert.ToInt32(dr["blood_sugar_id"]),
                user_id = Convert.ToInt32(dr["user_id"]),
                user_name = dr["user_name"].ToString(),
                device_id = dr["device_id"]?.ToString(),
                blood_sugar_value = Convert.ToDecimal(dr["blood_sugar_value"]),
                measurement_time = dr["measurement_time"] != DBNull.Value ? Convert.ToDateTime(dr["measurement_time"]) : (DateTime?)null,
                measurement_scenario = dr["measurement_scenario"].ToString(),
                related_diet_id = dr["related_diet_id"] != DBNull.Value ? Convert.ToInt32(dr["related_diet_id"]) : (int?)null,
                related_exercise_id = dr["related_exercise_id"] != DBNull.Value ? Convert.ToInt32(dr["related_exercise_id"]) : (int?)null,
                data_source = dr["data_source"].ToString(),
                operator_id = Convert.ToInt32(dr["operator_id"]),
                operator_name = dr["operator_name"].ToString(),
                abnormal_note = dr["abnormal_note"]?.ToString(),
                data_status = Convert.ToInt32(dr["data_status"]),
                data_version = Convert.ToInt32(dr["data_version"]),
                create_time = dr["create_time"] != DBNull.Value ? Convert.ToDateTime(dr["create_time"]) : (DateTime?)null,
                update_time = dr["update_time"] != DBNull.Value ? Convert.ToDateTime(dr["update_time"]) : (DateTime?)null,
                is_abnormal = Convert.ToInt32(dr["is_abnormal"])
            };
        }

        /// <summary>
        /// 新增血糖记录
        /// </summary>
        public int AddBloodSugar(BloodSugar model)
        {
            string sql = @"
                INSERT INTO t_blood_sugar (
                    user_id, blood_sugar_value, measurement_time, measurement_scenario,
                    related_diet_id, related_exercise_id, data_source, operator_id,
                    abnormal_note, data_status, data_version, create_time, update_time
                ) VALUES (
                    @UserId, @BloodSugarValue, @MeasurementTime, @MeasurementScenario,
                    @RelatedDietId, @RelatedExerciseId, @DataSource, @OperatorId,
                    @AbnormalNote, 0, 1, GETDATE(), GETDATE()
                ); SELECT SCOPE_IDENTITY();";

            SqlParameter[] param = {
                new SqlParameter("@UserId", model.user_id),
                new SqlParameter("@BloodSugarValue", model.blood_sugar_value),
                new SqlParameter("@MeasurementTime", model.measurement_time ?? (object)DBNull.Value),
                new SqlParameter("@MeasurementScenario", model.measurement_scenario),
                new SqlParameter("@RelatedDietId", model.related_diet_id ?? (object)DBNull.Value),
                new SqlParameter("@RelatedExerciseId", model.related_exercise_id ?? (object)DBNull.Value),
                new SqlParameter("@DataSource", model.data_source),
                new SqlParameter("@OperatorId", model.operator_id),
                new SqlParameter("@AbnormalNote", model.abnormal_note ?? (object)DBNull.Value)
            };

            object res = SqlHelper.ExecuteScalar(sql, param);
            return res != null && res != DBNull.Value ? Convert.ToInt32(res) : 0;
        }

        /// <summary>
        /// 修改血糖记录
        /// </summary>
        public int UpdateBloodSugar(BloodSugar model)
        {
            string sql = @"
                UPDATE t_blood_sugar SET
                    user_id = @UserId,
                    blood_sugar_value = @BloodSugarValue,
                    measurement_time = @MeasurementTime,
                    measurement_scenario = @MeasurementScenario,
                    related_diet_id = @RelatedDietId,
                    related_exercise_id = @RelatedExerciseId,
                    data_source = @DataSource,
                    operator_id = @OperatorId,
                    abnormal_note = @AbnormalNote,
                    data_version = data_version + 1,
                    update_time = GETDATE()
                WHERE blood_sugar_id = @BloodSugarId AND data_status = 0";

            SqlParameter[] param = {
                new SqlParameter("@BloodSugarId", model.blood_sugar_id),
                new SqlParameter("@UserId", model.user_id),
                new SqlParameter("@BloodSugarValue", model.blood_sugar_value),
                new SqlParameter("@MeasurementTime", model.measurement_time ?? (object)DBNull.Value),
                new SqlParameter("@MeasurementScenario", model.measurement_scenario),
                new SqlParameter("@RelatedDietId", model.related_diet_id ?? (object)DBNull.Value),
                new SqlParameter("@RelatedExerciseId", model.related_exercise_id ?? (object)DBNull.Value),
                new SqlParameter("@DataSource", model.data_source),
                new SqlParameter("@OperatorId", model.operator_id),
                new SqlParameter("@AbnormalNote", model.abnormal_note ?? (object)DBNull.Value)
            };

            return SqlHelper.ExecuteNonQuery(sql, param);
        }

        /// <summary>
        /// 逻辑删除血糖记录
        /// </summary>
        public int DeleteBloodSugar(int bloodSugarId)
        {
            string sql = "UPDATE t_blood_sugar SET data_status = 0, update_time = GETDATE() WHERE blood_sugar_id = @BloodSugarId";
            SqlParameter[] param = { new SqlParameter("@BloodSugarId", bloodSugarId) };
            return SqlHelper.ExecuteNonQuery(sql, param);
        }
        #endregion

        #region 批量操作
        /// <summary>
        /// 批量逻辑删除血糖记录
        /// </summary>
        public int BatchDeleteBloodSugar(List<int> idList)
        {
            if (idList == null || idList.Count == 0) return 0;

            string ids = string.Join(",", idList);
            string sql = $@"
                UPDATE t_blood_sugar 
                SET data_status = 1, update_time = GETDATE() 
                WHERE blood_sugar_id IN ({ids})";

            return SqlHelper.ExecuteNonQuery(sql);
        }
        #endregion

        #region 辅助方法
        /// <summary>
        /// 获取所有患者列表（用于下拉筛选）
        /// </summary>
        public DataTable GetAllPatients()
        {
            string sql = "SELECT user_id, user_name FROM t_user WHERE user_role = '患者' ORDER BY user_name";
            return SqlHelper.ExecuteDataTable(sql);
        }
        #endregion
    }
}