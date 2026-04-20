using System;
using System.Data;
using DAL;
using Model;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>
    /// 饮食记录业务逻辑层
    /// </summary>
    public class B_Diet
    {
        private readonly D_Diet _dDiet = new D_Diet();

        /// <summary>
        /// 获取用户近7天饮食统计数据
        /// </summary>
        public BizResult Get7DayDietStatistic(int userId)
        {
            try
            {
                if (userId <= 0) return BizResult.Fail("用户信息异常");

                Diet7DayStatistic statistic = new Diet7DayStatistic();
                DataTable dt = _dDiet.Get7DayDietStatistic(userId);
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    statistic.TotalCalorie = Convert.ToDecimal(dr["TotalCalorie"]);
                    statistic.TotalCarb = Convert.ToDecimal(dr["TotalCarb"]);
                    statistic.AvgDailyCalorie = Convert.ToDecimal(dr["AvgDailyCalorie"]);

                    if (statistic.AvgDailyCalorie > 0)
                    {
                        decimal avgDailyCarb = statistic.TotalCarb / 7;
                        statistic.CarbEnergyRatio = Math.Round((avgDailyCarb * 4 / statistic.AvgDailyCalorie) * 100, 1);
                    }

                    B_UserHealth bUserHealth = new B_UserHealth();
                    var userResult = bUserHealth.GetUserHealthInfo(userId);
                    if (userResult.IsSuccess)
                    {
                        UserHealthInfo userInfo = userResult.Data as UserHealthInfo;
                        statistic.RecommendDailyCalorie = userInfo.Weight > 0 ? Math.Round(userInfo.Weight * 25, 0) : 1800;
                    }
                    else
                    {
                        statistic.RecommendDailyCalorie = 1800;
                    }

                    // 推荐碳水比例
                    statistic.RecommendCarbRatioMin = 45;
                    statistic.RecommendCarbRatioMax = 60;
                }
                return BizResult.Success(data: statistic);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"获取饮食统计数据失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 新增饮食记录
        /// </summary>
        public BizResult AddDietRecord(Diet diet)
        {
            try
            {
                if (diet.user_id <= 0) return BizResult.Fail("用户信息异常");
                if (string.IsNullOrWhiteSpace(diet.food_name)) return BizResult.Fail("食物名称不能为空");
                if (diet.food_amount <= 0) return BizResult.Fail("食用重量必须大于0");

                int rows = _dDiet.AddDietRecord(diet);
                return rows > 0 ? BizResult.Success("添加成功") : BizResult.Fail("添加失败");
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"添加失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 获取用户今日饮食记录数
        /// </summary>
        public int GetTodayDietCount(int userId)
        {
            if (userId <= 0) return 0;
            try
            {
                return _dDiet.GetTodayDietCount(userId);
            }
            catch
            {
                return 0;
            }
        }

        public decimal GetTodayTotalCalorie(int userId)
        {
            if (userId <= 0) return 0m;
            try
            {
                return _dDiet.GetTodayTotalCalorie(userId);
            }
            catch
            {
                return 0m;
            }
        }

        public Dictionary<string, int> Get7DayMealCalorieDistribution(int userId)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            if (userId <= 0) return result;

            try
            {
                DataTable dt = _dDiet.Get7DayMealCalorieDistribution(userId);
                if (dt == null || dt.Rows.Count == 0)
                    return result;

                foreach (DataRow dr in dt.Rows)
                {
                    string mealType = dr["meal_type"]?.ToString();
                    if (string.IsNullOrWhiteSpace(mealType))
                        continue;

                    int totalCalorie = dr["total_calorie"] != DBNull.Value
                        ? Convert.ToInt32(Math.Round(Convert.ToDecimal(dr["total_calorie"]), 0))
                        : 0;

                    result[mealType] = totalCalorie;
                }
            }
            catch
            {
                return new Dictionary<string, int>();
            }

            return result;
        }
    }
}