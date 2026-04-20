using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// 干预效果评估报告聚合模型
    /// </summary>
    public class EffectEvaluationReportModel
    {
        public string PatientName { get; set; }
        public int PatientAge { get; set; }
        public string DiabetesType { get; set; }
        public DateTime DiagnoseDate { get; set; }
        public DateTime ReportStartDate { get; set; }
        public DateTime ReportEndDate { get; set; }
        public BloodSugarStatisticsModel GlucoseStatistics { get; set; }
        public List<ComplicationProgressViewModel> ComplicationRecords { get; set; }
        public InterventionEffectScore LatestScore { get; set; }
        public List<InterventionPlan> InterventionPlans { get; set; }
        public string ReportDoctor { get; set; }
        public DateTime ReportGenerateTime { get; set; }
    }
}
