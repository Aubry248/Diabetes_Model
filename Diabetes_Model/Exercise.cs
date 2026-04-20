using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// 对应数据库 t_exercise 表，患者运动记录实体
    /// </summary>
    public class Exercise
    {
        /// <summary>
        /// 运动记录ID，主键，自增
        /// </summary>
        public int exercise_id { get; set; }

        /// <summary>
        /// 对应用户ID（患者ID）
        /// </summary>
        public int user_id { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string device_id { get; set; }

        /// <summary>
        /// 运动类型
        /// </summary>
        public string exercise_type { get; set; }

        /// <summary>
        /// 运动MET值
        /// </summary>
        public decimal met_value { get; set; }

        /// <summary>
        /// 运动时长 分钟
        /// </summary>
        public int exercise_duration { get; set; }

        /// <summary>
        /// 实际消耗热量 kcal
        /// </summary>
        public decimal actual_calorie { get; set; }

        /// <summary>
        /// 运动强度：低/中/高
        /// </summary>
        public string exercise_intensity { get; set; }

        /// <summary>
        /// 运动时间
        /// </summary>
        public DateTime exercise_time { get; set; }

        /// <summary>
        /// 关联血糖记录ID
        /// </summary>
        public int? related_bs_id { get; set; }

        /// <summary>
        /// 数据来源：手动录入/模拟手环
        /// </summary>
        public string data_source { get; set; }

        /// <summary>
        /// 操作人ID
        /// </summary>
        public int operator_id { get; set; }

        /// <summary>
        /// 数据状态：0=正常 1=待审核 2=异常
        /// </summary>
        public int data_status { get; set; }

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
