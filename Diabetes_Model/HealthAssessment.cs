using System;

namespace Model
{
    /// <summary>
    /// 健康评估实体（原有字段完全保留，仅追加业务所需字段）
    /// </summary>
    public class HealthAssessment
    {
        #region 原有字段完全保留，无任何修改
        public int assessment_id { get; set; }
        public int user_id { get; set; }
        public DateTime assessment_date { get; set; }
        public string assessment_type { get; set; }
        public int assessment_by { get; set; }
        public decimal? height { get; set; }
        public decimal? weight { get; set; }
        public decimal? waist_circumference { get; set; }
        public decimal? hip_circumference { get; set; }
        public short? systolic_bp { get; set; }
        public short? diastolic_bp { get; set; }
        public short? heart_rate { get; set; }
        public string glycemic_control_status { get; set; }
        public decimal? hba1c { get; set; }
        public decimal? avg_fasting_glucose { get; set; }
        public decimal? avg_postprandial_glucose { get; set; }
        public decimal? disease_duration_years { get; set; }
        public string diabetes_complications { get; set; }
        public string comorbidities { get; set; }
        public int data_completeness { get; set; }
        public decimal? assessment_score { get; set; }
        public string health_level { get; set; }
        public string health_suggestion { get; set; }
        public decimal? bmi { get; set; }
        public decimal? waist_hip_ratio { get; set; }
        public int status { get; set; }
        public int data_version { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }
        #endregion

        #region 追加：糖尿病/并发症风险评估业务字段（无侵入式新增）
        /// <summary>
        /// 糖尿病风险评分
        /// </summary>
        public int? diabetes_risk_score { get; set; }
        /// <summary>
        /// 糖尿病风险等级（低风险/中风险/高风险）
        /// </summary>
        public string diabetes_risk_level { get; set; }
        /// <summary>
        /// 糖尿病风险干预建议
        /// </summary>
        public string diabetes_risk_suggestion { get; set; }
        /// <summary>
        /// 并发症风险评分
        /// </summary>
        public int? complication_risk_score { get; set; }
        /// <summary>
        /// 并发症风险等级（低风险/中风险/高风险/极高风险）
        /// </summary>
        public string complication_risk_level { get; set; }
        /// <summary>
        /// 并发症风险干预建议
        /// </summary>
        public string complication_risk_suggestion { get; set; }
        /// <summary>
        /// 评估医生ID（当前登录医生）
        /// </summary>
        public int assess_doctor_id { get; set; }
        #endregion
    }

    /// <summary>
    /// 健康等级枚举（与原有系统完全兼容）
    /// </summary>
    public enum HealthLevel
    {
        优秀,
        合格,
        不合格
    }
}