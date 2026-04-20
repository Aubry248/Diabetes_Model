using System;

namespace Model
{
    /// <summary>
    /// 降糖药物字典实体，100%匹配 Diabetes_Antidiabetic_Drugs 数据库表
    /// </summary>
    public class AntidiabeticDrug
    {
        /// <summary>
        /// 药物ID，主键自增
        /// </summary>
        public int DrugID { get; set; }
        /// <summary>
        /// 药物唯一编码
        /// </summary>
        public string DrugCode { get; set; }
        /// <summary>
        /// 药物分类
        /// </summary>
        public string DrugCategory { get; set; }
        /// <summary>
        /// 通用名
        /// </summary>
        public string DrugGenericName { get; set; }
        /// <summary>
        /// 商品名
        /// </summary>
        public string TradeName { get; set; }
        /// <summary>
        /// 剂型
        /// </summary>
        public string DosageForm { get; set; }
        /// <summary>
        /// 规格
        /// </summary>
        public string Specification { get; set; }
        /// <summary>
        /// 每日剂量范围
        /// </summary>
        public string DailyDosageRange { get; set; }
        /// <summary>
        /// 达峰时间(h)
        /// </summary>
        public string PeakTime_h { get; set; }
        /// <summary>
        /// 作用持续时间(h)
        /// </summary>
        public string ActionDuration_h { get; set; }
        /// <summary>
        /// 半衰期(h)
        /// </summary>
        public string HalfLife_h { get; set; }
        /// <summary>
        /// 用法用量
        /// </summary>
        public string UsageDosage { get; set; }
        /// <summary>
        /// 肾功能不全用药说明
        /// </summary>
        public string RenalImpairmentNote { get; set; }
        /// <summary>
        /// 数据来源
        /// </summary>
        public string DataSource { get; set; }
        /// <summary>
        /// 批准文号
        /// </summary>
        public string ApprovalNumber { get; set; }
        /// <summary>
        /// 生产厂家
        /// </summary>
        public string Manufacturer { get; set; }
        /// <summary>
        /// 上市许可持有人
        /// </summary>
        public string MarketHolder { get; set; }
        /// <summary>
        /// 处方类型
        /// </summary>
        public string PrescriptionType { get; set; }
        /// <summary>
        /// 医保类型
        /// </summary>
        public string MedicalInsuranceType { get; set; }
        /// <summary>
        /// 给药途径
        /// </summary>
        public string AdminRoute { get; set; }
        /// <summary>
        /// 有效期
        /// </summary>
        public string ValidityPeriod { get; set; }
        /// <summary>
        /// 储存条件
        /// </summary>
        public string StorageCondition { get; set; }

        // ==================== 修复：和数据库表字段完全匹配，类型一致 ====================
        /// <summary>
        /// 启用状态：启用/禁用（和数据库表nvarchar类型匹配）
        /// </summary>
        public string EnableStatus { get; set; }
        /// <summary>
        /// 审核状态：待一级审核/一级审核通过/待终审/终审通过/审核驳回（和数据库表nvarchar类型匹配）
        /// </summary>
        public string AuditStatus { get; set; }
        /// <summary>
        /// 审核级别：1=一级审核（药师），2=终审（医师/管理员）
        /// </summary>
        public int AuditLevel { get; set; }
        /// <summary>
        /// 当前审核人ID
        /// </summary>
        public int? CurrentAuditor { get; set; }
        /// <summary>
        /// 创建人ID
        /// </summary>
        public int CreateBy { get; set; }
        /// <summary>
        /// 更新人ID
        /// </summary>
        public int UpdateBy { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 创建人名称
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 更新人名称
        /// </summary>
        public string UpdateUser { get; set; }
        /// <summary>
        /// 更新日志
        /// </summary>
        public string UpdateLog { get; set; }
        /// <summary>
        /// 审核记录
        /// </summary>
        public string AuditRecord { get; set; }
        /// <summary>
        /// 逻辑删除标记
        /// </summary>
        public int IsDeleted { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}