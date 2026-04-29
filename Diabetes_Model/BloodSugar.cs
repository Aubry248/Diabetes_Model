using System;

namespace Model
{
    /// <summary>
    /// 血糖数据实体（对应数据库t_blood_sugar表）
    /// </summary>
    public class BloodSugar
    {
        /// <summary>
        /// 血糖记录ID，主键
        /// </summary>
        public int blood_sugar_id { get; set; }

        /// <summary>
        /// 对应用户ID（患者ID）
        /// </summary>
        public int user_id { get; set; }

        /// <summary>
        /// 患者姓名（显示用，关联用户表）
        /// </summary>
        public string user_name { get; set; }

        /// <summary>
        /// 设备ID
        /// </summary>
        public string device_id { get; set; }

        /// <summary>
        /// 血糖值 mmol/L
        /// </summary>
        public decimal blood_sugar_value { get; set; }

        /// <summary>
        /// 测量时间
        /// </summary>
        public DateTime? measurement_time { get; set; }

        /// <summary>
        /// 测量场景：空腹/餐后2小时/睡前/随机等
        /// </summary>
        public string measurement_scenario { get; set; }

        /// <summary>
        /// 关联饮食记录ID
        /// </summary>
        public int? related_diet_id { get; set; }

        /// <summary>
        /// 关联运动记录ID
        /// </summary>
        public int? related_exercise_id { get; set; }

        /// <summary>
        /// 数据来源：手动录入/设备上传
        /// </summary>
        public string data_source { get; set; }

        /// <summary>
        /// 操作人ID
        /// </summary>
        public int operator_id { get; set; }

        /// <summary>
        /// 操作人姓名（显示用）
        /// </summary>
        public string operator_name { get; set; }

        /// <summary>
        /// 异常备注
        /// </summary>
        public string abnormal_note { get; set; }

        /// <summary>
        /// 数据状态：1=正常 0=已删除
        /// </summary>
        public int data_status { get; set; }

        /// <summary>
        /// 数据版本号
        /// </summary>
        public int data_version { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? create_time { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? update_time { get; set; }

        /// <summary>
        /// 数据库计算列：是否异常 1=异常 0=正常
        /// </summary>
        public int is_abnormal { get; set; }
    }
}