using System;
namespace Model
{
    /// <summary>
    /// 药物版本历史实体，对应 Diabetes_Drug_Version_History 表
    /// </summary>
    public class DrugVersionHistory
    {
        public int HistoryID { get; set; }
        public int DrugID { get; set; }
        public string DrugType { get; set; }
        public string DrugCode { get; set; }
        public string Version { get; set; }
        public string DrugContent { get; set; }
        public string UpdateContent { get; set; }
        public DateTime UpdateTime { get; set; }
        public int UpdateBy { get; set; }
        public string AuditStatus { get; set; }
        public string ComplianceResult { get; set; }
    }
}