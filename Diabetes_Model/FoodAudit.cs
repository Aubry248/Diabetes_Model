using System;
namespace Model
{
    /// <summary>
    /// 对应数据库 Diabetes_Food_Audit 表，食物审核记录
    /// </summary>
    public class FoodAudit
    {
        /// <summary>
        /// 审核ID，主键，自增
        /// </summary>
        public int AuditID { get; set; }
        /// <summary>
        /// 关联食物ID
        /// </summary>
        public int FoodID { get; set; }
        /// <summary>
        /// 食物编码
        /// </summary>
        public string FoodCode { get; set; }
        /// <summary>
        /// 食物名称
        /// </summary>
        public string FoodName { get; set; }
        /// <summary>
        /// 上传人/提交人
        /// </summary>
        public string Uploader { get; set; }
        /// <summary>
        /// 上传/提交时间
        /// </summary>
        public DateTime UploadTime { get; set; }
        /// <summary>
        /// 审核状态：待审核/审核通过/审核驳回
        /// </summary>
        public string AuditStatus { get; set; }
        /// <summary>
        /// 审核人
        /// </summary>
        public string AuditUser { get; set; }
        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? AuditTime { get; set; }
        /// <summary>
        /// 审核备注/驳回原因
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 提交的版本号
        /// </summary>
        public string Version { get; set; }
    }
}