namespace DoctorModel
{
    /// <summary>
    /// 糖尿病风险评估实体
    /// </summary>
    public class DiabetesRiskModel
    {
        public int PatientID { get; set; }
        public int Age { get; set; }
        public string FamilyHistory { get; set; }
        public double BMI { get; set; }
        public string DietHabits { get; set; }
        public string ExerciseHabits { get; set; }
        public int WaistCircumference { get; set; }
        public string HypertensionHistory { get; set; }
        public string GestationalDiabetesHistory { get; set; }
        public int TotalScore { get; set; }
        public string RiskLevel { get; set; }
        public string Suggestion { get; set; }
        public string CreateUser { get; set; }
    }
}