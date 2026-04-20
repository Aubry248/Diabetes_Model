using System;
namespace Model
{
    /// <summary>
    /// 对应数据库 Diabetes_Food_Nutrition 表，食物营养成分字典（完整扩展版）
    /// </summary>
    public class FoodNutrition
    {
        /// <summary>
        /// 食物ID，主键，自增
        /// </summary>
        public int FoodID { get; set; }
        /// <summary>
        /// 食物唯一编码（系统自动生成）
        /// </summary>
        public string FoodCode { get; set; }
        /// <summary>
        /// 食物名称
        /// </summary>
        public string FoodName { get; set; }
        /// <summary>
        /// 食物别名
        /// </summary>
        public string Alias { get; set; }
        /// <summary>
        /// 食物分类
        /// </summary>
        public string FoodCategory { get; set; }
        /// <summary>
        /// 可食部比例 %
        /// </summary>
        public decimal? EdibleRate { get; set; }
        /// <summary>
        /// 水分含量 g/100g
        /// </summary>
        public decimal? WaterContent { get; set; }
        /// <summary>
        /// 热量 kcal/100g
        /// </summary>
        public decimal? Energy_kcal { get; set; }
        /// <summary>
        /// 热量 kJ/100g
        /// </summary>
        public decimal? Energy_kJ { get; set; }
        /// <summary>
        /// 蛋白质 g/100g
        /// </summary>
        public decimal? Protein { get; set; }
        /// <summary>
        /// 脂肪 g/100g
        /// </summary>
        public decimal? Fat { get; set; }
        /// <summary>
        /// 碳水化合物 g/100g
        /// </summary>
        public decimal? Carbohydrate { get; set; }
        /// <summary>
        /// 膳食纤维 g/100g
        /// </summary>
        public decimal? DietaryFiber { get; set; }
        /// <summary>
        /// 胆固醇 mg/100g
        /// </summary>
        public decimal? Cholesterol { get; set; }
        /// <summary>
        /// 维生素C mg/100g
        /// </summary>
        public decimal? VitaminC { get; set; }
        /// <summary>
        /// 胡萝卜素 μg/100g
        /// </summary>
        public decimal? Carotene { get; set; }
        /// <summary>
        /// 钠 mg/100g
        /// </summary>
        public decimal? Sodium { get; set; }
        /// <summary>
        /// 钾 mg/100g
        /// </summary>
        public decimal? Potassium { get; set; }
        /// <summary>
        /// 血糖生成指数GI
        /// </summary>
        public decimal? GI { get; set; }
        /// <summary>
        /// 升糖负荷GL
        /// </summary>
        public decimal? GL { get; set; }
        /// <summary>
        /// 糖尿病交换份
        /// </summary>
        public decimal? ExchangeUnit { get; set; }
        /// <summary>
        /// 升糖特点标注
        /// </summary>
        public string GlycemicFeature { get; set; }
        /// <summary>
        /// 适用人群
        /// </summary>
        public string SuitablePeople { get; set; }
        /// <summary>
        /// 禁忌人群
        /// </summary>
        public string ForbiddenPeople { get; set; }
        /// <summary>
        /// 推荐食用量
        /// </summary>
        public string RecommendAmount { get; set; }
        /// <summary>
        /// 烹饪方式建议
        /// </summary>
        public string CookingSuggest { get; set; }
        /// <summary>
        /// 血糖影响提示
        /// </summary>
        public string GlucoseTip { get; set; }
        /// <summary>
        /// 数据来源
        /// </summary>
        public string DataSourceInfo { get; set; }
        /// <summary>
        /// 参考依据
        /// </summary>
        public string Reference { get; set; }
        /// <summary>
        /// 食物图片存储路径
        /// </summary>
        public string FoodImagePath { get; set; }
        /// <summary>
        /// 启用状态：启用/禁用
        /// </summary>
        public string EnableStatus { get; set; }
        /// <summary>
        /// 审核状态：待审核/审核通过/审核驳回
        /// </summary>
        public string AuditStatus { get; set; }
        /// <summary>
        /// 当前版本号
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 更新日志
        /// </summary>
        public string UpdateLog { get; set; }
        /// <summary>
        /// 审核记录
        /// </summary>
        public string AuditRecord { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 最后更新人
        /// </summary>
        public string UpdateUser { get; set; }
        /// <summary>
        /// 逻辑删除标记：0未删除 1已删除
        /// </summary>
        public int IsDeleted { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}