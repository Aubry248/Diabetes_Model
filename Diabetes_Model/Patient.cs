using System;

namespace Model
{
    /// <summary>
    /// 患者扩展表实体（和数据库t_patient完全对应）
    /// </summary>
    public class Patient
    {
        /// <summary>
        /// 患者ID（和t_user.user_id一一对应）
        /// </summary>
        public int patient_id { get; set; }

        /// <summary>
        /// 糖尿病类型
        /// </summary>
        public string diabetes_type { get; set; }

        /// <summary>
        /// 确诊日期（可空）
        /// </summary>
        public DateTime? diagnose_date { get; set; }

        /// <summary>
        /// 空腹血糖基线（可空）
        /// </summary>
        public decimal? fasting_glucose_baseline { get; set; }

        /// <summary>
        /// 家族史（可空）
        /// </summary>
        public string family_history { get; set; }

        /// <summary>
        /// 既往病史（可空）
        /// </summary>
        public string past_medical_history { get; set; }

        /// <summary>
        /// 过敏史（可空）
        /// </summary>
        public string allergy_history { get; set; }

        /// <summary>
        /// 并发症（可空）
        /// </summary>
        public string complications { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime create_time { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime update_time { get; set; }

        /// <summary>
        /// 数据版本
        /// </summary>
        public int data_version { get; set; }
    }
}