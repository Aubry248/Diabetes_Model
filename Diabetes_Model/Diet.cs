using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// 对应数据库 t_diet 表，患者饮食记录实体
    /// </summary>
    public class Diet
    {
        /// <summary>
        /// 饮食记录ID，主键，自增
        /// </summary>
        public int diet_id { get; set; }

        /// <summary>
        /// 对应用户ID（患者ID）
        /// </summary>
        public int user_id { get; set; }

        /// <summary>
        /// 自定义食物名称
        /// </summary>
        public string local_food_name { get; set; }

        /// <summary>
        /// 标准食物名称
        /// </summary>
        public string food_name { get; set; }

        /// <summary>
        /// 食物GI值
        /// </summary>
        public decimal food_gi { get; set; }

        /// <summary>
        /// 食物每100g热量
        /// </summary>
        public int food_calorie { get; set; }

        /// <summary>
        /// 食物每100g碳水含量
        /// </summary>
        public decimal food_carb { get; set; }

        /// <summary>
        /// 食用重量 g
        /// </summary>
        public decimal food_amount { get; set; }

        /// <summary>
        /// 实际摄入热量
        /// </summary>
        public decimal actual_calorie { get; set; }

        /// <summary>
        /// 实际摄入碳水
        /// </summary>
        public decimal actual_carb { get; set; }

        /// <summary>
        /// 用餐时间
        /// </summary>
        public DateTime meal_time { get; set; }

        /// <summary>
        /// 用餐类型：早餐/午餐/晚餐/加餐
        /// </summary>
        public string meal_type { get; set; }

        /// <summary>
        /// 数据来源：手动录入/Excel批量导入
        /// </summary>
        public string data_source { get; set; }

        /// <summary>
        /// 操作人ID
        /// </summary>
        public int operator_id { get; set; }

        /// <summary>
        /// 是否自定义食物：1=是 0=否
        /// </summary>
        public int is_custom { get; set; }

        /// <summary>
        /// 自定义原因
        /// </summary>
        public string custom_reason { get; set; }

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

        /// <summary>
        /// 热量单位
        /// </summary>
        public string calorie_unit { get; set; }

        /// <summary>
        /// 碳水单位
        /// </summary>
        public string carb_unit { get; set; }

        /// <summary>
        /// 计算列：升糖负荷GL（和数据库计算逻辑完全一致）
        /// </summary>
        public decimal? glycemic_load
        {
            get
            {
                if (food_gi <= 0 || actual_carb <= 0) return null;
                return (food_gi * actual_carb) / 100.0m;
            }
        }
    }
}
