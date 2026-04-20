using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// 用药趋势统计实体
    /// </summary>
    public class MedicineTrend
    {
        public DateTime Date { get; set; }
        public decimal TotalDosage { get; set; }
        public int MedicineCount { get; set; }
    }
}
