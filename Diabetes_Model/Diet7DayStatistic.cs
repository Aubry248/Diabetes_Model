namespace Model
{
    /// <summary>
    /// 近7天饮食统计实体
    /// </summary>
    public class Diet7DayStatistic
    {
        /// <summary>
        /// 近7天总摄入热量(kcal)
        /// </summary>
        public decimal TotalCalorie { get; set; }
        /// <summary>
        /// 日均摄入热量(kcal)
        /// </summary>
        public decimal AvgDailyCalorie { get; set; }
        /// <summary>
        /// 推荐日均热量(kcal)
        /// </summary>
        public decimal RecommendDailyCalorie { get; set; }
        /// <summary>
        /// 近7天总摄入碳水(g)
        /// </summary>
        public decimal TotalCarb { get; set; }
        /// <summary>
        /// 碳水化合物供能占比(%)
        /// </summary>
        public decimal CarbEnergyRatio { get; set; }
        /// <summary>
        /// 推荐碳水占比下限(%)
        /// </summary>
        public decimal RecommendCarbRatioMin { get; set; } = 50;
        /// <summary>
        /// 推荐碳水占比上限(%)
        /// </summary>
        public decimal RecommendCarbRatioMax { get; set; } = 60;

    }
}