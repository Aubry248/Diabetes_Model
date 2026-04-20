using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Model
{
    /// <summary>
    /// 对应数据库 t_follow_up 表，医生随访记录实体
    /// </summary>
    public class FollowUp
    {
        /// <summary>
        /// 随访记录ID，主键，自增
        /// </summary>
        public int follow_up_id { get; set; }

        /// <summary>
        /// 对应用户ID（患者ID）
        /// </summary>
        public int user_id { get; set; }

        /// <summary>
        /// 关联干预方案ID
        /// </summary>
        public int? plan_id { get; set; }

        /// <summary>
        /// 随访时间
        /// </summary>
        public DateTime follow_up_time { get; set; }

        /// <summary>
        /// 随访方式：电话/上门/门诊/线上
        /// </summary>
        public string follow_up_way { get; set; }

        /// <summary>
        /// 随访内容
        /// </summary>
        public string follow_up_content { get; set; }

        /// <summary>
        /// 随访结果
        /// </summary>
        public string follow_up_result { get; set; }

        /// <summary>
        /// 下次随访时间
        /// </summary>
        public DateTime? next_follow_up_time { get; set; }

        /// <summary>
        /// 随访人ID（医生ID）
        /// </summary>
        public int follow_up_by { get; set; }

        /// <summary>
        /// 随访状态：0=待随访 1=随访中 2=已完成
        /// </summary>
        public int follow_up_status { get; set; }

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
