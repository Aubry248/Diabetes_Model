using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// 血糖趋势明细视图模型
    /// </summary>
    public class BloodSugarTrendViewModel
    {
        public int BloodSugarId { get; set; }
        public DateTime MeasureTime { get; set; }
        public string MeasureScenario { get; set; }
        public decimal BloodSugarValue { get; set; }
        public string ControlStatus { get; set; }
        public bool IsAbnormal { get; set; }
    }
}
