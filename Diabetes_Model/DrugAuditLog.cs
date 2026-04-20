using System;
namespace Model
{
    /// <summary>
    /// 药物审核日志实体，对应 Diabetes_Drug_Audit_Log 表
    /// </summary>
    public class DrugAuditLog
    {
        public int AuditLogID { get; set; }
        public int DrugID { get; set; }
        public string DrugType { get; set; }
        public int AuditLevel { get; set; }
        public int AuditStatus { get; set; }
        public string AuditOpinion { get; set; }
        public DateTime AuditTime { get; set; }
        public int AuditBy { get; set; }
    }
}