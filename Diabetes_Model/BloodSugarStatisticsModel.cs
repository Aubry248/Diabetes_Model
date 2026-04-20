using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// 血糖统计汇总模型
    /// </summary>
    public class BloodSugarStatisticsModel
    {
        public decimal AvgFastingGlucose { get; set; }
        public decimal AvgPostprandialGlucose { get; set; }
        public decimal MaxGlucose { get; set; }
        public decimal MinGlucose { get; set; }
        public decimal GlucoseFluctuation { get; set; }
        public decimal StandardReachRate { get; set; }
        public int TotalRecordCount { get; set; }
        public int AbnormalCount { get; set; }
    }
}
