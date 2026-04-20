using System;

namespace DoctorModel
{
    /// <summary>
    /// 患者健康数据实体
    /// </summary>
    public class HealthDataModel
    {
        public int PatientID { get; set; }
        public DateTime AssessDate { get; set; }
        public double? FastingGlucose { get; set; }
        public double? PostprandialGlucose { get; set; }
        public double? HbA1c { get; set; }
        public int? SystolicBP { get; set; }
        public int? DiastolicBP { get; set; }
        public double? BMI { get; set; }
        public int TotalScore { get; set; }
        public string HealthLevel { get; set; }
        public string CreateUser { get; set; }
    }
}