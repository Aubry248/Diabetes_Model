using System;
using System.Collections.Generic;

namespace Model
{
    /// <summary>
    /// 患者档案聚合实体，全维度患者档案信息
    /// </summary>
    public class PatientArchive
    {
        /// <summary>
        /// 患者基础信息
        /// </summary>
        public Users BaseInfo { get; set; }

        /// <summary>
        /// 最新健康评估信息
        /// </summary>
        public HealthAssessment LatestAssessment { get; set; }

        /// <summary>
        /// 近30天血糖统计-平均空腹血糖
        /// </summary>
        public decimal? AvgFastingGlucose { get; set; }

        /// <summary>
        /// 近30天血糖统计-平均餐后2小时血糖
        /// </summary>
        public decimal? AvgPostprandialGlucose { get; set; }

        /// <summary>
        /// 近30天血糖异常次数
        /// </summary>
        public int AbnormalBloodSugarCount { get; set; }

        /// <summary>
        /// 最新糖化血红蛋白HbA1c
        /// </summary>
        public decimal? LatestHbA1c { get; set; }

        /// <summary>
        /// 血糖控制状态
        /// </summary>
        public string GlycemicControlStatus { get; set; }

        /// <summary>
        /// 并发症列表（逗号分隔转数组）
        /// </summary>
        public string[] DiabetesComplications { get; set; }

        /// <summary>
        /// 合并症列表（逗号分隔转数组）
        /// </summary>
        public string[] Comorbidities { get; set; }
    }

    /// <summary>
    /// 患者档案历史记录实体，用于历史追溯
    /// </summary>
    public class PatientArchiveHistory
    {
        /// <summary>
        /// 记录ID
        /// </summary>
        public int record_id { get; set; }

        /// <summary>
        /// 患者ID
        /// </summary>
        public int user_id { get; set; }

        /// <summary>
        /// 记录类型：健康评估/血糖检测/干预方案/用药记录/饮食记录/运动记录
        /// </summary>
        public string record_type { get; set; }

        /// <summary>
        /// 关联业务表ID
        /// </summary>
        public int related_id { get; set; }

        /// <summary>
        /// 记录时间
        /// </summary>
        public DateTime record_time { get; set; }

        /// <summary>
        /// 记录核心内容摘要
        /// </summary>
        public string record_summary { get; set; }

        /// <summary>
        /// 操作人ID
        /// </summary>
        public int operator_id { get; set; }

        /// <summary>
        /// 操作人姓名
        /// </summary>
        public string operator_name { get; set; }
    }
}