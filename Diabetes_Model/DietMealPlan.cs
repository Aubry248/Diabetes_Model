using System.Collections.Generic;
namespace Model
{
    /// <summary>
    /// 一日三餐饮食方案实体
    /// </summary>
    public class DietMealPlan
    {
        /// <summary>
        /// 早餐食物列表
        /// </summary>
        public List<FoodNutrition> BreakfastFoods { get; set; } = new List<FoodNutrition>();
        /// <summary>
        /// 早餐总热量(kcal)
        /// </summary>
        public decimal BreakfastTotalCalorie { get; set; }
        /// <summary>
        /// 午餐食物列表
        /// </summary>
        public List<FoodNutrition> LunchFoods { get; set; } = new List<FoodNutrition>();
        /// <summary>
        /// 午餐总热量(kcal)
        /// </summary>
        public decimal LunchTotalCalorie { get; set; }
        /// <summary>
        /// 晚餐食物列表
        /// </summary>
        public List<FoodNutrition> DinnerFoods { get; set; } = new List<FoodNutrition>();
        /// <summary>
        /// 晚餐总热量(kcal)
        /// </summary>
        public decimal DinnerTotalCalorie { get; set; }
        /// <summary>
        /// 全天总热量(kcal)
        /// </summary>
        public decimal TotalDayCalorie { get; set; }
    }
}