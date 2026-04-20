namespace DoctorModel
{
    /// <summary>
    /// 并发症风险评估实体
    /// </summary>
    public class ComplicationRiskModel
    {
        public int PatientID { get; set; }
        public string DiseaseDuration { get; set; }
        public string HbA1cLevel { get; set; }
        public string BloodPressureLevel { get; set; }
        public string LipidStatus { get; set; }
        public string Proteinuria { get; set; }
        public string SmokingHistory { get; set; }
        public int TotalScore { get; set; }
        public string RiskLevel { get; set; }
        public string Suggestion { get; set; }
        public string CreateUser { get; set; }
    }
}