using System;
using System.Data;
using System.Data.SqlClient;
using Tools;
using Model;

namespace DAL
{
    /// <summary>
    /// 饮食记录数据访问层
    /// </summary>
    public class D_Diet
    {
        /// <summary>
        /// 获取用户近7天饮食统计数据
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>统计数据表</returns>
        public DataTable Get7DayDietStatistic(int userId)
        {
            string sql = @"
        SELECT 
            ISNULL(SUM(actual_calorie),0) AS TotalCalorie,
            ISNULL(SUM(actual_carb),0) AS TotalCarb,
            ISNULL(AVG(actual_calorie),0) AS AvgDailyCalorie
        FROM t_diet 
        WHERE user_id = @userId 
        AND meal_time >= DATEADD(DAY,-7,GETDATE())
        AND data_status = 1"; // 统一有效状态为1
            SqlParameter[] param = { new SqlParameter("@userId", userId) };
            return SqlHelper.ExecuteDataTable(sql, param);
        }

        /// <summary>
        /// 获取用户今日热量
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>今日热量</returns>
        public decimal GetTodayTotalCalorie(int userId)
        {
            string sql = @"
        SELECT ISNULL(SUM(actual_calorie), 0)
        FROM t_diet
        WHERE user_id = @UserId
        AND CONVERT(date, meal_time) = CONVERT(date, GETDATE())
        AND data_status = 1";
            SqlParameter[] param = { new SqlParameter("@UserId", userId) };
            object result = SqlHelper.ExecuteScalar(sql, param);
            return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
        }

        /// <summary>
        /// 获取用户近7天饮食热量占比分布
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>热量占比分布表</returns>
        public DataTable Get7DayMealCalorieDistribution(int userId)
        {
            string sql = @"
        SELECT 
            meal_type,
            ISNULL(SUM(actual_calorie), 0) AS total_calorie
        FROM t_diet
        WHERE user_id = @UserId
        AND meal_time >= DATEADD(DAY, -7, CONVERT(date, GETDATE()))
        AND data_status = 1
        GROUP BY meal_type
        ORDER BY CASE meal_type
            WHEN '早餐' THEN 1
            WHEN '午餐' THEN 2
            WHEN '晚餐' THEN 3
            WHEN '加餐' THEN 4
            ELSE 5
        END";
            SqlParameter[] param = { new SqlParameter("@UserId", userId) };
            return SqlHelper.ExecuteDataTable(sql, param);
        }

        /// <summary>
        /// 新增饮食记录到数据库
        /// </summary>
        /// <param name="diet">饮食记录实体</param>
        /// <returns>受影响行数</returns>
        public int AddDietRecord(Diet diet)
        {
            string sql = @"
        INSERT INTO t_diet (user_id, food_name, food_gi, food_calorie, food_carb, 
            food_amount, actual_calorie, actual_carb, meal_time, meal_type, 
            data_source, operator_id, is_custom, data_status, data_version, 
            create_time, update_time, calorie_unit, carb_unit)
        VALUES (@user_id, @food_name, @food_gi, @food_calorie, @food_carb,
            @food_amount, @actual_calorie, @actual_carb, @meal_time, @meal_type,
            @data_source, @operator_id, @is_custom, 1, @data_version,
            GETDATE(), GETDATE(), @calorie_unit, @carb_unit)";
            SqlParameter[] param = {
        new SqlParameter("@user_id", diet.user_id),
        new SqlParameter("@food_name", diet.food_name),
        new SqlParameter("@food_gi", diet.food_gi),
        new SqlParameter("@food_calorie", diet.food_calorie),
        new SqlParameter("@food_carb", diet.food_carb),
        new SqlParameter("@food_amount", diet.food_amount),
        new SqlParameter("@actual_calorie", diet.actual_calorie),
        new SqlParameter("@actual_carb", diet.actual_carb),
        new SqlParameter("@meal_time", diet.meal_time),
        new SqlParameter("@meal_type", diet.meal_type),
        new SqlParameter("@data_source", diet.data_source),
        new SqlParameter("@operator_id", diet.operator_id),
        new SqlParameter("@is_custom", diet.is_custom),
        new SqlParameter("@data_version", diet.data_version),
        new SqlParameter("@calorie_unit", diet.calorie_unit),
        new SqlParameter("@carb_unit", diet.carb_unit)
    };
            return SqlHelper.ExecuteNonQuery(sql, param);
        }

        /// <summary>
        /// 获取用户今日饮食记录数（修复：用meal_time筛选，统一data_status过滤）
        /// </summary>
        /// <summary>
        /// 获取用户今日饮食记录数（修复：参数+时间+状态）
        /// </summary>
        /// <summary>
        /// 获取用户今日饮食记录数（修复：参数+时间+状态）
        /// </summary>
        public int GetTodayDietCount(int userId)
        {
            try
            {
                string sql = @"
SELECT COUNT(*) 
FROM t_diet 
WHERE user_id = @UserId 
AND CONVERT(date, meal_time) = CONVERT(date, GETDATE()) -- 用用餐时间，不是create_time
AND data_status = 1";
                SqlParameter[] param = {
            new SqlParameter("@UserId", userId)
        };
                object res = SqlHelper.ExecuteScalar(sql, param);
                return res != null ? Convert.ToInt32(res) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"饮食数据查询异常：{ex.Message}");
            }
        }
    }
}