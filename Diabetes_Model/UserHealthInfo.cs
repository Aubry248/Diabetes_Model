using System;
namespace Model
{
    /// <summary>
    /// 用户健康信息实体，用于个性化建议生成
    /// </summary>
    public class UserHealthInfo
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 近7天空腹血糖平均值
        /// </summary>
        public decimal AvgFastingBloodSugar { get; set; }
        /// <summary>
        /// 近7天餐后2小时血糖平均值
        /// </summary>
        public decimal AvgPostprandialBloodSugar { get; set; }
        /// <summary>
        /// 患病年限（年）
        /// </summary>
        public decimal DiseaseDurationYears { get; set; }
        /// <summary>
        /// 是否有肾病并发症
        /// </summary>
        public bool HasNephropathy { get; set; }
        /// <summary>
        /// 是否有心血管并发症
        /// </summary>
        public bool HasCardiovascularDisease { get; set; }
        /// <summary>
        /// 是否有用药记录
        /// </summary>
        public bool HasMedicineRecord { get; set; }
        /// <summary>
        /// 患者体重(kg)
        /// </summary>
        public decimal Weight { get; set; }
        /// <summary>
        /// 患者身高(cm)
        /// </summary>
        public decimal Height { get; set; }
    }
}