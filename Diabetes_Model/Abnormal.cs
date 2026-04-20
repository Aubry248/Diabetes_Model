using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Model
{
    /// <summary>
    /// 对应数据库 t_abnormal 表，异常数据预警实体
    /// </summary>
    public class Abnormal
    {
        /// <summary>
        /// 异常记录ID，主键，自增
        /// </summary>
        public int abnormal_id { get; set; }

        /// <summary>
        /// 数据类型：血糖/饮食/运动/用药
        /// </summary>
        public string data_type { get; set; }

        /// <summary>
        /// 原始数据ID
        /// </summary>
        public int original_data_id { get; set; }

        /// <summary>
        /// 对应用户ID（患者ID）
        /// </summary>
        public int user_id { get; set; }

        /// <summary>
        /// 异常类型
        /// </summary>
        public string abnormal_type { get; set; }

        /// <summary>
        /// 异常原因
        /// </summary>
        public string abnormal_reason { get; set; }

        /// <summary>
        /// 干预建议
        /// </summary>
        public string suggestion { get; set; }

        /// <summary>
        /// 标记时间
        /// </summary>
        public DateTime mark_time { get; set; }

        /// <summary>
        /// 标记人ID
        /// </summary>
        public int mark_by { get; set; }

        /// <summary>
        /// 处理状态：0=待处理 1=处理中 2=已处理
        /// </summary>
        public int handle_status { get; set; }

        /// <summary>
        /// 处理备注
        /// </summary>
        public string handle_note { get; set; }

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
    }
}
