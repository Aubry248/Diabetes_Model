using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Tools;

namespace DAL
{
    /// <summary>
    /// 血糖记录数据访问层
    /// </summary>
    public class D_BloodSugar
    {
        #region 1. 新增血糖记录
        /// <summary>
        /// 新增血糖记录，返回新记录ID
        /// </summary>
        public int AddBloodSugar(BloodSugar bs)
        {
            string sql = @"
            INSERT INTO t_blood_sugar (
                user_id, device_id, blood_sugar_value, measurement_scenario, measurement_time, 
                data_source, is_abnormal, operator_id, related_diet_id, remark,
                data_status, data_version, create_time, update_time
            ) VALUES (
                @user_id, @device_id, @blood_sugar_value, @measurement_scenario, @measurement_time,
                @data_source, @is_abnormal, @operator_id, @related_diet_id, @remark,
                @data_status, @data_version, @create_time, @update_time
            );
            SELECT SCOPE_IDENTITY();";

            SqlParameter[] param = {
                new SqlParameter("@user_id", bs.user_id),
                new SqlParameter("@device_id", bs.device_id),
                new SqlParameter("@blood_sugar_value", bs.blood_sugar_value),
                new SqlParameter("@measurement_scenario", bs.measurement_scenario),
                new SqlParameter("@measurement_time", bs.measurement_time),
                new SqlParameter("@data_source", bs.data_source),
                new SqlParameter("@is_abnormal", bs.is_abnormal),
                new SqlParameter("@operator_id", bs.operator_id),
                new SqlParameter("@related_diet_id", bs.related_diet_id ?? (object)DBNull.Value),
                new SqlParameter("@abnormal_note", bs.abnormal_note ?? (object)DBNull.Value),
                new SqlParameter("@data_status", bs.data_status),
                new SqlParameter("@data_version", bs.data_version),
                new SqlParameter("@create_time", bs.create_time),
                new SqlParameter("@update_time", bs.update_time)
            };

            object result = SqlHelper.GetSingle(sql, param);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }
        #endregion

        #region 2. 修改血糖记录
        /// <summary>
        /// 修改血糖记录，返回是否成功
        /// </summary>
        public bool UpdateBloodSugar(BloodSugar bs)
        {
            // 移除了SET子句中的 is_abnormal = @is_abnormal
            string sql = @"
    UPDATE t_blood_sugar SET 
    blood_sugar_value = @blood_sugar_value,
    measurement_scenario = @measurement_scenario,
    measurement_time = @measurement_time,
    remark = @remark,
    update_time = GETDATE(),
    data_version = data_version + 1
    WHERE blood_sugar_id = @blood_sugar_id AND user_id = @user_id AND data_status=1";

            // 移除了@is_abnormal对应的SqlParameter参数行
            SqlParameter[] param = {
        new SqlParameter("@blood_sugar_value", bs.blood_sugar_value),
        new SqlParameter("@measurement_scenario", bs.measurement_scenario),
        new SqlParameter("@measurement_time", bs.measurement_time),
        new SqlParameter("@abnormal_note", bs.abnormal_note ?? (object)DBNull.Value),
        new SqlParameter("@blood_sugar_id", bs.blood_sugar_id),
        new SqlParameter("@user_id", bs.user_id)
    };
            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }
        #endregion

        #region 3. 软删除血糖记录
        /// <summary>
        /// 软删除血糖记录
        /// </summary>
        public bool DeleteBloodSugar(int bloodSugarId, int userId)
        {
            string sql = "UPDATE t_blood_sugar SET data_status=2, update_time=GETDATE() WHERE blood_sugar_id=@Id AND user_id=@UserId";
            SqlParameter[] param = {
                new SqlParameter("@Id", bloodSugarId),
                new SqlParameter("@UserId", userId)
            };
            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }
        #endregion

        #region 4. 单条记录详情查询
        /// <summary>
        /// 根据ID和用户ID查询单条血糖记录详情
        /// </summary>
        public BloodSugar GetBloodSugarDetail(int bloodSugarId, int userId)
        {
            string sql = "SELECT * FROM t_blood_sugar WHERE blood_sugar_id=@Id AND user_id=@UserId AND data_status=1";
            SqlParameter[] param = {
                new SqlParameter("@Id", bloodSugarId),
                new SqlParameter("@UserId", userId)
            };

            DataTable dt = SqlHelper.ExecuteDataTable(sql, param);
            if (dt.Rows.Count == 0) return null;

            return DataRowToModel(dt.Rows[0]);
        }
        #endregion

        #region 5. 分页查询血糖记录
        /// <summary>
        /// 分页查询患者血糖记录
        /// </summary>
        public List<BloodSugar> GetBloodSugarPageList(int userId, int pageIndex, int pageSize, out int totalCount,
            DateTime? startTime = null, DateTime? endTime = null, string scenario = "")
        {
            totalCount = 0;
            // 统计总条数
            string countSql = "SELECT COUNT(1) FROM t_blood_sugar WHERE user_id=@UserId AND data_status=1";
            List<SqlParameter> countParam = new List<SqlParameter> { new SqlParameter("@UserId", userId) };

            // 拼接筛选条件
            string whereSql = "";
            if (startTime.HasValue)
            {
                whereSql += " AND measurement_time >= @StartTime";
                countParam.Add(new SqlParameter("@StartTime", startTime.Value));
            }
            if (endTime.HasValue)
            {
                whereSql += " AND measurement_time <= @EndTime";
                countParam.Add(new SqlParameter("@EndTime", endTime.Value.AddDays(1).AddSeconds(-1)));
            }
            if (!string.IsNullOrEmpty(scenario) && scenario != "全部")
            {
                whereSql += " AND measurement_scenario = @Scenario";
                countParam.Add(new SqlParameter("@Scenario", scenario));
            }

            countSql += whereSql;
            object countResult = SqlHelper.GetSingle(countSql, countParam.ToArray());
            totalCount = countResult != null && countResult != DBNull.Value ? Convert.ToInt32(countResult) : 0;
            if (totalCount == 0) return new List<BloodSugar>();

            // 分页查询数据
            string dataSql = @"
            SELECT * FROM (
                SELECT *, ROW_NUMBER() OVER (ORDER BY measurement_time DESC) AS row_num
                FROM t_blood_sugar WHERE user_id=@UserId AND data_status=1" + whereSql + @"
            ) AS temp WHERE row_num BETWEEN @StartRow AND @EndRow ORDER BY row_num ASC";

            List<SqlParameter> dataParam = new List<SqlParameter>(countParam);
            int startRow = (pageIndex - 1) * pageSize + 1;
            int endRow = pageIndex * pageSize;
            dataParam.Add(new SqlParameter("@StartRow", startRow));
            dataParam.Add(new SqlParameter("@EndRow", endRow));

            DataTable dt = SqlHelper.ExecuteDataTable(dataSql, dataParam.ToArray());
            List<BloodSugar> list = new List<BloodSugar>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(DataRowToModel(dr));
            }
            return list;
        }
        #endregion

        #region 6. 今日统计数据查询
        /// <summary>
        /// 获取用户当日血糖统计数据
        /// </summary>
        public DataTable GetTodayStats(int userId)
        {
            string sql = @"
            SELECT 
                AVG(blood_sugar_value) AS avg_value,
                MAX(blood_sugar_value) AS max_value,
                MIN(blood_sugar_value) AS min_value,
                SUM(CASE WHEN is_abnormal=1 THEN 1 ELSE 0 END) AS abnormal_count
            FROM t_blood_sugar 
            WHERE user_id=@UserId AND data_status=1 
            AND CONVERT(DATE, measurement_time) = CONVERT(DATE, GETDATE())";

            SqlParameter[] param = { new SqlParameter("@UserId", userId) };
            return SqlHelper.ExecuteDataTable(sql, param);
        }
        #endregion

        #region 7. 30天趋势数据查询
        /// <summary>
        /// 获取30天血糖趋势数据
        /// </summary>
        public DataTable Get30DayTrendData(int userId)
        {
            string sql = @"
            SELECT 
                CONVERT(DATE, measurement_time) AS record_date,
                measurement_scenario,
                AVG(blood_sugar_value) AS avg_value
            FROM t_blood_sugar 
            WHERE user_id=@UserId AND data_status=1 
            AND measurement_time >= DATEADD(DAY, -30, GETDATE())
            GROUP BY CONVERT(DATE, measurement_time), measurement_scenario
            ORDER BY record_date ASC";

            SqlParameter[] param = { new SqlParameter("@UserId", userId) };
            return SqlHelper.ExecuteDataTable(sql, param);
        }
        #endregion

        #region 私有方法：DataRow转实体
        /// <summary>
        /// DataRow转换为BloodSugar实体
        /// </summary>
        private BloodSugar DataRowToModel(DataRow dr)
        {
            return new BloodSugar
            {
                blood_sugar_id = dr["blood_sugar_id"] != DBNull.Value ? Convert.ToInt32(dr["blood_sugar_id"]) : 0,
                user_id = dr["user_id"] != DBNull.Value ? Convert.ToInt32(dr["user_id"]) : 0,
                blood_sugar_value = dr["blood_sugar_value"] != DBNull.Value ? Convert.ToDecimal(dr["blood_sugar_value"]) : 0m,
                measurement_scenario = dr["measurement_scenario"]?.ToString() ?? string.Empty,
                measurement_time = dr["measurement_time"] != DBNull.Value ? Convert.ToDateTime(dr["measurement_time"]) : DateTime.Now,
                data_source = dr["data_source"]?.ToString() ?? "手动录入",
                is_abnormal = dr["is_abnormal"] != DBNull.Value ? Convert.ToInt32(dr["is_abnormal"]) : 0,
                operator_id = dr["operator_id"] != DBNull.Value ? Convert.ToInt32(dr["operator_id"]) : 0,
                related_diet_id = dr["related_diet_id"] != DBNull.Value ? Convert.ToInt32(dr["related_diet_id"]) : (int?)null,
                abnormal_note = dr["abnormal_note"]?.ToString() ?? string.Empty,
                data_status = dr["data_status"] != DBNull.Value ? Convert.ToInt32(dr["data_status"]) : 1,
                data_version = dr["data_version"] != DBNull.Value ? Convert.ToInt32(dr["data_version"]) : 1,
                create_time = dr["create_time"] != DBNull.Value ? Convert.ToDateTime(dr["create_time"]) : DateTime.Now,
                update_time = dr["update_time"] != DBNull.Value ? Convert.ToDateTime(dr["update_time"]) : DateTime.Now
            };
        }
        #endregion

        /// <summary>
        /// 获取用户今日最新血糖值（修复：时间筛选+参数+状态+排序）
        /// </summary>
        public decimal GetTodayLatestBloodSugar(int userId)
        {
            try
            {
                string sql = @"
                SELECT TOP 1 blood_sugar_value 
                FROM t_blood_sugar 
                WHERE user_id = @UserId 
                AND CONVERT(date, measurement_time) = CONVERT(date, GETDATE()) -- 【修复1：用测量时间，不是create_time】
                AND data_status = 1 -- 【修复2：统一有效状态为1】
                ORDER BY measurement_time DESC"; // 【修复3：强制取最新一条】
                SqlParameter[] param = {
            new SqlParameter("@UserId", userId) // 【修复4：参数名统一】
        };
                object res = SqlHelper.ExecuteScalar(sql, param);
                return res != null ? Convert.ToDecimal(res) : 0m;
            }
            catch (Exception ex)
            {
                throw new Exception($"血糖数据查询异常：{ex.Message}");
            }
        }

        public List<BloodSugar> GetUserBloodSugarList(int userId)
        {
            string sql = @"
            SELECT blood_sugar_id, blood_sugar_value, measurement_time, measurement_scenario
            FROM t_blood_sugar 
            WHERE user_id = @UserId AND data_status != 2
            ORDER BY measurement_time DESC";

            SqlParameter[] param = { new SqlParameter("@UserId", userId) };
            return SqlHelper.GetModelList<BloodSugar>(sql, param);
        }
    }
}