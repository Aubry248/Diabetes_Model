using System;
namespace Model
{
    /// <summary>
    /// 对应数据库 t_medication_plan 表，患者用药方案实体
    /// </summary>
    public class MedicationPlan
    {
        /// <summary>
        /// 方案ID，主键，自增
        /// </summary>
        public int plan_id { get; set; }
        /// <summary>
        /// 对应用户ID（患者ID）
        /// </summary>
        public int user_id { get; set; }
        /// <summary>
        /// 药物编码
        /// </summary>
        public string drug_code { get; set; }
        /// <summary>
        /// 药物名称
        /// </summary>
        public string drug_name { get; set; }
        /// <summary>
        /// 药物类型
        /// </summary>
        public string drug_type { get; set; }
        /// <summary>
        /// 用药剂量
        /// </summary>
        public decimal drug_dosage { get; set; }
        /// <summary>
        /// 调整原因
        /// </summary>
        public string adjust_reason { get; set; }
        /// <summary>
        /// 调整说明/方案内容
        /// </summary>
        public string adjust_content { get; set; }
        /// <summary>
        /// 方案开始时间
        /// </summary>
        public DateTime start_time { get; set; }
        /// <summary>
        /// 方案结束时间
        /// </summary>
        public DateTime? end_time { get; set; }
        /// <summary>
        /// 创建人ID（医生ID）
        /// </summary>
        public int create_by { get; set; }
        /// <summary>
        /// 方案状态：0=待执行 1=执行中 2=已结束 3=已停用
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 数据版本号
        /// </summary>
        public int data_version { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime create_time { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime update_time { get; set; }
    }
}