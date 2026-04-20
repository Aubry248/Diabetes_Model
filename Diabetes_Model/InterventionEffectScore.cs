using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// 干预效果评分实体（映射t_health_assessment表评分字段）
    /// </summary>
    public class InterventionEffectScore
    {
        public int AssessmentId { get; set; }
        public int UserId { get; set; }
        public string InterventionType { get; set; }
        public int DietScore { get; set; }
        public int ExerciseScore { get; set; }
        public int MedicationScore { get; set; }
        public int ComplianceScore { get; set; }
        public int TotalScore { get; set; }
        public string EffectLevel { get; set; }
        public DateTime AssessmentDate { get; set; }
        public int AssessmentBy { get; set; }
    }
}
