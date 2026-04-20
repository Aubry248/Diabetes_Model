using System;

namespace Model
{
    /// <summary>
    /// 糖尿病神经病变药物字典实体，对应 Diabetes_Neuropathy_Drugs 表（完整匹配版）
    /// </summary>
    public class NeuropathyDrug
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
        /// 用法用量
        /// </summary>
        public string UsageDosage { get; set; }
        /// <summary>
        /// 不良反应说明
        /// </summary>
        public string AdverseReactionNote { get; set; }
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
        /// <summary>
        /// 适应症
        /// </summary>
        public string Indications { get; set; }
        /// <summary>
        /// 特殊人群剂量调整
        /// </summary>
        public string SpecialPopulationAdjust { get; set; }
        /// <summary>
        /// 绝对禁忌
        /// </summary>
        public string AbsoluteContraindication { get; set; }
        /// <summary>
        /// 相对禁忌
        /// </summary>
        public string RelativeContraindication { get; set; }
        /// <summary>
        /// 注意事项
        /// </summary>
        public string Precautions { get; set; }
        /// <summary>
        /// 药物相互作用
        /// </summary>
        public string DrugInteraction { get; set; }
        /// <summary>
        /// 指南引用
        /// </summary>
        public string GuideReference { get; set; }
        /// <summary>
        /// 指南推荐级别
        /// </summary>
        public string GuideGrade { get; set; }
        /// <summary>
        /// 一线/二线用药
        /// </summary>
        public string FirstSecondLine { get; set; }
        /// <summary>
        /// 说明书附件
        /// </summary>
        public string ManualAttachment { get; set; }
        /// <summary>
        /// 数据来源
        /// </summary>
        public string DataSource { get; set; }
        /// <summary>
        /// 批准文号
        /// </summary>
        public string ApprovalNumber { get; set; }

        /// <summary>
        /// 启用状态：1=启用，0=禁用
        /// </summary>
        public int EnableStatus { get; set; }
        /// <summary>
        /// 审核状态：0=待一级审核，1=一级审核通过，2=待终审，3=终审通过，4=审核驳回
        /// </summary>
        public int AuditStatus { get; set; }
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