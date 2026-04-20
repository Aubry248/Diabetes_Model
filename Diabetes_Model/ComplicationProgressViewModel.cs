using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// 并发症进展记录视图模型
    /// </summary>
    public class ComplicationProgressViewModel
    {
        public int RecordId { get; set; }
        public DateTime RecordDate { get; set; }
        public string ComplicationType { get; set; }
        public string IndexChangeDesc { get; set; }
        public string ProgressDegree { get; set; }
        public string InterventionMeasures { get; set; }
        public string FollowUpDoctor { get; set; }
    }
}
