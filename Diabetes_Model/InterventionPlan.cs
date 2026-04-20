using System;

namespace Model
{
    /// <summary>
    /// 对应数据库 t_intervention_plan 表，医生干预方案实体
    /// 【修复版】全字段匹配，完全对齐数据库与业务代码调用
    /// </summary>
    public class InterventionPlan
    {
        /// <summary>
        /// 方案ID，主键，自增
        /// </summary>
        public int plan_id { get; set; }

        /// <summary>
        /// 对应用户ID（患者ID）
        /// </summary>
        public int user_id { get; set; }

        /// <summary>
        /// 关联异常记录ID
        /// </summary>
        public int? related_abnormal_id { get; set; }

        /// <summary>
        /// 方案类型：饮食干预/运动干预/用药调整
        /// </summary>
        public string plan_type { get; set; }

        /// <summary>
        /// 方案内容
        /// </summary>
        public string plan_content { get; set; }

        /// <summary>
        /// 预期效果
        /// </summary>
        public string expected_effect { get; set; }

        /// <summary>
        /// 方案开始时间
        /// </summary>
        public DateTime start_time { get; set; }

        /// <summary>
        /// 方案结束时间
        /// </summary>
        public DateTime end_time { get; set; }

        /// <summary>
        /// 复查时间
        /// </summary>
        public DateTime review_time { get; set; }

        /// <summary>
        /// 创建人ID（医生ID）
        /// </summary>
        public int create_by { get; set; }

        /// <summary>
        /// 执行状态：0=未执行 1=执行中 2=已完成
        /// 数据库类型：tinyint，对应C# byte
        /// </summary>
        public byte execute_status { get; set; }

        /// <summary>
        /// 执行备注
        /// </summary>
        public string execute_note { get; set; }

        /// <summary>
        /// 数据版本号
        /// </summary>
        public int data_version { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime create_time { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime update_time { get; set; }

        /// <summary>
        /// 方案状态：0=禁用 1=启用 2=已结束
        /// 数据库类型：tinyint，对应C# byte
        /// </summary>
        public byte status { get; set; }
    }
}