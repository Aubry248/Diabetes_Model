
using DAL;
using Model;
using System;
using System.Collections.Generic;
using System.Data;

/// <summary>
/// 饮食方案业务逻辑层
/// </summary>
public class B_DietPlan
{
    private readonly D_DietPlan _dDietPlan = new D_DietPlan();

    /// <summary>
    /// 生成今日一日三餐饮食搭配方案
    /// </summary>
    public BizResult GenerateTodayMealPlan(int userId)
    {
        try
        {
            DietMealPlan plan = new DietMealPlan();
            B_UserHealth bUserHealth = new B_UserHealth();
            var userResult = bUserHealth.GetUserHealthInfo(userId);
            if (!userResult.IsSuccess)
                return BizResult.Fail(userResult.Message);

            UserHealthInfo userInfo = userResult.Data as UserHealthInfo;
            decimal dailyTotalCalorie = userInfo.Weight > 0 ? userInfo.Weight * 25 : 1800;
            decimal breakfastCalorie = dailyTotalCalorie * 0.3m;
            decimal lunchCalorie = dailyTotalCalorie * 0.4m;
            decimal dinnerCalorie = dailyTotalCalorie * 0.3m;

            // 生成三餐食物
            DataTable dtBreakfastGrain = _dDietPlan.GetRandomLowGIFoods("谷薯类", 1, breakfastCalorie * 0.6m);
            DataTable dtBreakfastEgg = _dDietPlan.GetRandomLowGIFoods("蛋类", 1, breakfastCalorie * 0.3m);
            DataTable dtBreakfastMilk = _dDietPlan.GetRandomLowGIFoods("乳类", 1, breakfastCalorie * 0.1m);
            plan.BreakfastFoods.AddRange(ConvertDataTableToFoodList(dtBreakfastGrain));
            plan.BreakfastFoods.AddRange(ConvertDataTableToFoodList(dtBreakfastEgg));
            plan.BreakfastFoods.AddRange(ConvertDataTableToFoodList(dtBreakfastMilk));
            plan.BreakfastTotalCalorie = CalculateFoodListTotalCalorie(plan.BreakfastFoods);

            DataTable dtLunchGrain = _dDietPlan.GetRandomLowGIFoods("谷薯类", 1, lunchCalorie * 0.4m);
            DataTable dtLunchVegetable = _dDietPlan.GetRandomLowGIFoods("蔬菜类", 2, lunchCalorie * 0.2m);
            DataTable dtLunchMeat = _dDietPlan.GetRandomLowGIFoods("肉类", 1, lunchCalorie * 0.4m);
            plan.LunchFoods.AddRange(ConvertDataTableToFoodList(dtLunchGrain));
            plan.LunchFoods.AddRange(ConvertDataTableToFoodList(dtLunchVegetable));
            plan.LunchFoods.AddRange(ConvertDataTableToFoodList(dtLunchMeat));
            plan.LunchTotalCalorie = CalculateFoodListTotalCalorie(plan.LunchFoods);

            DataTable dtDinnerGrain = _dDietPlan.GetRandomLowGIFoods("谷薯类", 1, dinnerCalorie * 0.3m);
            DataTable dtDinnerVegetable = _dDietPlan.GetRandomLowGIFoods("蔬菜类", 2, dinnerCalorie * 0.4m);
            DataTable dtDinnerBean = _dDietPlan.GetRandomLowGIFoods("豆类及制品", 1, dinnerCalorie * 0.3m);
            plan.DinnerFoods.AddRange(ConvertDataTableToFoodList(dtDinnerGrain));
            plan.DinnerFoods.AddRange(ConvertDataTableToFoodList(dtDinnerVegetable));
            plan.DinnerFoods.AddRange(ConvertDataTableToFoodList(dtDinnerBean));
            plan.DinnerTotalCalorie = CalculateFoodListTotalCalorie(plan.DinnerFoods);

            plan.TotalDayCalorie = plan.BreakfastTotalCalorie + plan.LunchTotalCalorie + plan.DinnerTotalCalorie;
            return BizResult.Success(data: plan);
        }
        catch (Exception ex)
        {
            return BizResult.Fail($"生成饮食方案失败：{ex.Message}");
        }
    }

    #region 私有辅助方法
    private List<FoodNutrition> ConvertDataTableToFoodList(DataTable dt)
    {
        List<FoodNutrition> list = new List<FoodNutrition>();
        if (dt == null || dt.Rows.Count == 0) return list;

        foreach (DataRow dr in dt.Rows)
        {
            list.Add(new FoodNutrition
            {
                FoodID = Convert.ToInt32(dr["FoodID"]),
                FoodName = dr["FoodName"].ToString(),
                FoodCategory = dr["FoodCategory"].ToString(),
                Energy_kcal = dr["Energy_kcal"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["Energy_kcal"]),
                GI = dr["GI"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["GI"]),
                Carbohydrate = dr["Carbohydrate"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["Carbohydrate"])
            });
        }
        return list;
    }

    private decimal CalculateFoodListTotalCalorie(List<FoodNutrition> list)
    {
        decimal total = 0;
        foreach (var food in list)
        {
            total += food.Energy_kcal ?? 0;
        }
        return total;
    }
}
        #endregion