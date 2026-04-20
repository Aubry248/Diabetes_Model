using System;
using System.Collections.Generic;

namespace Model
{
    /// <summary>
    /// 患者健康数据概览聚合实体（无侵入式新增）
    /// </summary>
    public class PatientHealthOverview
    {
        #region 患者基本信息
        public int user_id { get; set; }
        public string user_name { get; set; }
        public int gender { get; set; }
        public int age { get; set; }
        public string diabetes_type { get; set; }
        public DateTime? diagnose_date { get; set; }
        public decimal? disease_duration_years { get; set; }
        public decimal? latest_height { get; set; }
        public decimal? latest_weight { get; set; }
        public decimal? latest_bmi { get; set; }
        #endregion

        #region 核心业务数据列表
        public List<BloodSugar> BloodSugarList { get; set; } = new List<BloodSugar>();
        public List<Diet> DietList { get; set; } = new List<Diet>();
        public List<Exercise> ExerciseList { get; set; } = new List<Exercise>();
        public List<Medicine> MedicineList { get; set; } = new List<Medicine>();
        public HealthAssessment LatestAssessment { get; set; }
        #endregion

        #region 统计指标
        public decimal AvgFastingGlucose { get; set; }
        public decimal AvgPostprandialGlucose { get; set; }
        public decimal GlucoseAbnormalRate { get; set; }
        public decimal AvgDailyCalorie { get; set; }
        public int AvgDailyExerciseMinutes { get; set; }
        #endregion
    }
}