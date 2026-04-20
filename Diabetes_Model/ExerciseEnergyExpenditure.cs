using System;

namespace Model
{
    /// <summary>
    /// 对应数据库 Diabetes_Exercise_Energy_Expenditure 表，运动能量消耗字典（完整字段版）
    /// </summary>
    public class ExerciseEnergyExpenditure
    {
        /// <summary>
        /// 运动ID，主键，自增
        /// </summary>
        public int ExerciseID { get; set; }
        /// <summary>
        /// 运动编码
        /// </summary>
        public string ExerciseCode { get; set; }
        /// <summary>
        /// 运动分类
        /// </summary>
        public string ExerciseCategory { get; set; }
        /// <summary>
        /// 运动名称
        /// </summary>
        public string ExerciseName { get; set; }
        /// <summary>
        /// 运动别名
        /// </summary>
        public string Alias { get; set; }
        /// <summary>
        /// MET值
        /// </summary>
        public decimal MET_Value { get; set; }
        /// <summary>
        /// 强度分类：低/中/高
        /// </summary>
        public string IntensityCategory { get; set; }
        /// <summary>
        /// 是否糖尿病友好：是/否
        /// </summary>
        public string IsDiabetesFriendly { get; set; }
        /// <summary>
        /// 推荐单次时长
        /// </summary>
        public string RecommendDuration { get; set; }
        /// <summary>
        /// 推荐运动频次
        /// </summary>
        public string RecommendFrequency { get; set; }
        /// <summary>
        /// 适用人群
        /// </summary>
        public string SuitablePeople { get; set; }
        /// <summary>
        /// 禁忌/禁用人群
        /// </summary>
        public string ForbiddenPeople { get; set; }
        /// <summary>
        /// 糖尿病安全提示
        /// </summary>
        public string SafetyTip { get; set; }
        /// <summary>
        /// 运动描述/动作步骤
        /// </summary>
        public string ExerciseDesc { get; set; }
        /// <summary>
        /// 标准来源
        /// </summary>
        public string StandardSource { get; set; }
        /// <summary>
        /// 启用状态
        /// </summary>
        public string EnableStatus { get; set; }
        /// <summary>
        /// 审核状态
        /// </summary>
        public string AuditStatus { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 更新人
        /// </summary>
        public string UpdateUser { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 更新日志
        /// </summary>
        public string UpdateLog { get; set; }
        /// <summary>
        /// 审核记录
        /// </summary>
        public string AuditRecord { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}