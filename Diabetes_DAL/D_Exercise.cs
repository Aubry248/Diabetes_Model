using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Model;
using Tools;

namespace DAL
{
    public class D_Exercise
    {
        #region 新增运动记录
        /// <summary>
        /// 新增单条运动记录
        /// </summary>
        public int AddExercise(Exercise model)
        {
            string sql = @"
INSERT INTO t_exercise (user_id, device_id, exercise_type, met_value, exercise_duration, actual_calorie, 
exercise_intensity, exercise_time, related_bs_id, data_source, operator_id, data_status)
VALUES (@user_id, @device_id, @exercise_type, @met_value, @exercise_duration, @actual_calorie, 
@exercise_intensity, @exercise_time, @related_bs_id, @data_source, @operator_id, @data_status);
SELECT SCOPE_IDENTITY();";

            SqlParameter[] param = {
                new SqlParameter("@user_id", model.user_id),
                new SqlParameter("@device_id", model.device_id ?? (object)DBNull.Value),
                new SqlParameter("@exercise_type", model.exercise_type),
                new SqlParameter("@met_value", model.met_value),
                new SqlParameter("@exercise_duration", model.exercise_duration),
                new SqlParameter("@actual_calorie", model.actual_calorie),
                new SqlParameter("@exercise_intensity", model.exercise_intensity),
                new SqlParameter("@exercise_time", model.exercise_time),
                new SqlParameter("@related_bs_id", model.related_bs_id ?? (object)DBNull.Value),
                new SqlParameter("@data_source", model.data_source),
                new SqlParameter("@operator_id", model.operator_id),
                new SqlParameter("@data_status", model.data_status)
            };

            object res = SqlHelper.ExecuteScalar(sql, param);
            return res != null ? Convert.ToInt32(res) : 0;
        }
        #endregion

        #region 批量新增运动记录（模拟导入）
        /// <summary>
        /// 批量插入运动记录
        /// </summary>
        public bool BatchAddExercise(List<Exercise> list)
        {
            using (SqlConnection conn = new SqlConnection(SqlHelper.connStr))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var model in list)
                        {
                            string sql = @"
INSERT INTO t_exercise (user_id, device_id, exercise_type, met_value, exercise_duration, actual_calorie, 
exercise_intensity, exercise_time, data_source, operator_id, data_status)
VALUES (@user_id, @device_id, @exercise_type, @met_value, @exercise_duration, @actual_calorie, 
@exercise_intensity, @exercise_time, @data_source, @operator_id, @data_status);";

                            SqlParameter[] param = {
                                new SqlParameter("@user_id", model.user_id),
                                new SqlParameter("@device_id", model.device_id ?? (object)DBNull.Value),
                                new SqlParameter("@exercise_type", model.exercise_type),
                                new SqlParameter("@met_value", model.met_value),
                                new SqlParameter("@exercise_duration", model.exercise_duration),
                                new SqlParameter("@actual_calorie", model.actual_calorie),
                                new SqlParameter("@exercise_intensity", model.exercise_intensity),
                                new SqlParameter("@exercise_time", model.exercise_time),
                                new SqlParameter("@data_source", model.data_source),
                                new SqlParameter("@operator_id", model.operator_id),
                                new SqlParameter("@data_status", model.data_status)
                            };
                            SqlHelper.ExecuteNonQuery(tran, sql, param);
                        }
                        tran.Commit();
                        return true;
                    }
                    catch
                    {
                        tran.Rollback();
                        return false;
                    }
                }
            }
        }
        #endregion

        #region 更新运动记录
        /// <summary>
        /// 更新运动记录
        /// </summary>
        public bool UpdateExercise(Exercise model)
        {
            string sql = @"
UPDATE t_exercise SET 
exercise_type = @exercise_type,
met_value = @met_value,
exercise_duration = @exercise_duration,
actual_calorie = @actual_calorie,
exercise_intensity = @exercise_intensity,
exercise_time = @exercise_time,
update_time = GETDATE()
WHERE exercise_id = @exercise_id AND user_id = @user_id";

            SqlParameter[] param = {
                new SqlParameter("@exercise_id", model.exercise_id),
                new SqlParameter("@user_id", model.user_id),
                new SqlParameter("@exercise_type", model.exercise_type),
                new SqlParameter("@met_value", model.met_value),
                new SqlParameter("@exercise_duration", model.exercise_duration),
                new SqlParameter("@actual_calorie", model.actual_calorie),
                new SqlParameter("@exercise_intensity", model.exercise_intensity),
                new SqlParameter("@exercise_time", model.exercise_time)
            };

            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }
        #endregion

        #region 删除运动记录
        /// <summary>
        /// 删除运动记录（硬删除，可改为软删除）
        /// </summary>
        public bool DeleteExercise(int exerciseId, int userId)
        {
            string sql = "DELETE FROM t_exercise WHERE exercise_id = @exercise_id AND user_id = @user_id";
            SqlParameter[] param = {
                new SqlParameter("@exercise_id", exerciseId),
                new SqlParameter("@user_id", userId)
            };
            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }
        #endregion

        #region 查询用户运动记录
        /// <summary>
        /// 获取用户所有运动记录（按运动时间降序）
        /// </summary>
        public List<Exercise> GetUserAllExercise(int userId)
        {
            string sql = "SELECT * FROM t_exercise WHERE user_id = @user_id AND data_status = 1 ORDER BY exercise_time DESC";
            SqlParameter[] param = { new SqlParameter("@userId", userId) };
            DataTable dt = SqlHelper.ExecuteDataTable(sql, param);
            return DataTableToList(dt);
        }

        /// <summary>
        /// 条件筛选运动记录
        /// </summary>
        public List<Exercise> GetExerciseByFilter(int userId, DateTime startDate, DateTime endDate, string exerciseType, string intensity)
        {
            DateTime startDateTime = startDate.Date;
            DateTime endDateTime = endDate.Date.AddDays(1).AddSeconds(-1);

            string sql = @"
SELECT * FROM t_exercise 
WHERE user_id = @user_id 
AND exercise_time BETWEEN @startDate AND @endDate
AND (@exerciseType = '' OR exercise_type = @exerciseType)
AND (@intensity = '' OR exercise_intensity = @intensity)
ORDER BY exercise_time DESC";

            SqlParameter[] param = {
                new SqlParameter("@user_id", userId),
                new SqlParameter("@startDate", startDateTime),
                new SqlParameter("@endDate", endDateTime),
                new SqlParameter("@exerciseType", exerciseType ?? ""),
                new SqlParameter("@intensity", intensity ?? "")
            };

            DataTable dt = SqlHelper.ExecuteDataTable(sql, param);
            return DataTableToList(dt);
        }
        #endregion

        #region 血糖与统计查询
        /// <summary>
        /// 获取用户近7天空腹血糖平均值
        /// </summary>
        public decimal GetUser7DayFastingAvgGlucose(int userId)
        {
            string sql = @"
SELECT ISNULL(AVG(blood_sugar_value), 0) 
FROM t_blood_sugar 
WHERE user_id = @userId 
AND measurement_scenario = '空腹'
AND measurement_time >= DATEADD(DAY, -7, GETDATE())";
            SqlParameter[] param = { new SqlParameter("@userId", userId) };
            object res = SqlHelper.ExecuteScalar(sql, param);
            return res != null ? Convert.ToDecimal(res) : 0;
        }

        /// <summary>
        /// 获取用户近30天每日运动总时长
        /// </summary>
        public DataTable Get30DayExerciseDailyTotal(int userId)
        {
            string sql = @"
SELECT CONVERT(date, exercise_time) AS exercise_date, ISNULL(SUM(exercise_duration), 0) AS total_minutes
FROM t_exercise 
WHERE user_id = @userId 
AND exercise_time >= DATEADD(DAY, -30, GETDATE())
GROUP BY CONVERT(date, exercise_time)
ORDER BY exercise_date ASC";
            SqlParameter[] param = { new SqlParameter("@userId", userId) };
            return SqlHelper.ExecuteDataTable(sql, param);
        }

        /// <summary>
        /// 获取指定日期用户运动明细
        /// </summary>
        public List<Exercise> GetExerciseByDate(int userId, DateTime date)
        {
            string sql = @"
SELECT * FROM t_exercise 
WHERE user_id = @userId 
AND CONVERT(date, exercise_time) = @date
ORDER BY exercise_time DESC";
            SqlParameter[] param = {
                new SqlParameter("@userId", userId),
                new SqlParameter("@date", date.Date)
            };
            DataTable dt = SqlHelper.ExecuteDataTable(sql, param);
            return DataTableToList(dt);
        }
        #endregion

        #region 运动MET值查询（匹配运动能量消耗字典）
        /// <summary>
        /// 根据运动名称获取MET值
        /// </summary>
        public decimal GetExerciseMetValue(string exerciseName)
        {
            string sql = "SELECT ISNULL(MET_Value, 3.5) FROM Diabetes_Exercise_Energy_Expenditure WHERE ExerciseName = @ExerciseName";
            SqlParameter[] param = { new SqlParameter("@ExerciseName", exerciseName) };
            object res = SqlHelper.ExecuteScalar(sql, param);
            return res != null ? Convert.ToDecimal(res) : 3.5m;
        }
        #endregion

        #region 私有方法：DataTable转List
        private List<Exercise> DataTableToList(DataTable dt)
        {
            List<Exercise> list = new List<Exercise>();
            if (dt == null || dt.Rows.Count == 0) return list;

            foreach (DataRow dr in dt.Rows)
            {
                Exercise model = new Exercise
                {
                    exercise_id = Convert.ToInt32(dr["exercise_id"]),
                    user_id = Convert.ToInt32(dr["user_id"]),
                    device_id = dr["device_id"]?.ToString(),
                    exercise_type = dr["exercise_type"].ToString(),
                    met_value = Convert.ToDecimal(dr["met_value"]),
                    exercise_duration = Convert.ToInt32(dr["exercise_duration"]),
                    actual_calorie = Convert.ToDecimal(dr["actual_calorie"]),
                    exercise_intensity = dr["exercise_intensity"].ToString(),
                    exercise_time = Convert.ToDateTime(dr["exercise_time"]),
                    related_bs_id = dr["related_bs_id"] == DBNull.Value ? null : (int?)Convert.ToInt32(dr["related_bs_id"]),
                    data_source = dr["data_source"].ToString(),
                    operator_id = Convert.ToInt32(dr["operator_id"]),
                    data_status = Convert.ToInt32(dr["data_status"]),
                    data_version = Convert.ToInt32(dr["data_version"]),
                    create_time = Convert.ToDateTime(dr["create_time"]),
                    update_time = Convert.ToDateTime(dr["update_time"])
                };
                list.Add(model);
            }
            return list;
        }
        #endregion
        #region 根据ID获取单条运动记录
        /// <summary>
        /// 根据运动ID和用户ID获取单条运动记录（用于编辑功能）
        /// </summary>
        public Exercise GetExerciseById(int exerciseId, int userId)
        {
            string sql = "SELECT * FROM t_exercise WHERE exercise_id = @exerciseId AND user_id = @userId";
            SqlParameter[] param = {
        new SqlParameter("@exerciseId", exerciseId),
        new SqlParameter("@userId", userId)
    };
            DataTable dt = SqlHelper.ExecuteDataTable(sql, param);
            if (dt == null || dt.Rows.Count == 0) return null;
            return DataTableToList(dt)[0];
        }
        #endregion

        /// <summary>
        /// 获取用户今日运动总时长（修复：参数名统一+SQL逻辑+异常捕获）
        /// </summary>
        public int GetTodayExerciseMinutes(int userId)
        {
            try
            {
                // 【修复1：参数名统一为@UserId，SQL和C#完全一致】
                string sql = @"
                SELECT ISNULL(SUM(exercise_duration), 0) 
                FROM t_exercise 
                WHERE user_id = @UserId 
                AND CONVERT(date, exercise_time) = CONVERT(date, GETDATE())
                AND data_status = 1"; // 统一有效状态为1
                SqlParameter[] param = {
            new SqlParameter("@UserId", userId) // 【修复2：参数名和SQL严格匹配】
        };
                object res = SqlHelper.ExecuteScalar(sql, param);
                return res != null ? Convert.ToInt32(res) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"运动数据查询异常：{ex.Message}");
            }
        }
    } 
}