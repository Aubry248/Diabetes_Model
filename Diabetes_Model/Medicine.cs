using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// 对应数据库 t_medicine 表，患者用药记录实体
    /// </summary>
    public class Medicine
    {
        /// <summary>
        /// 用药记录ID，主键，自增
        /// </summary>
        public int medicine_id { get; set; }

        /// <summary>
        /// 对应用户ID（患者ID）
        /// </summary>
        public int user_id { get; set; }

        /// <summary>
        /// 药物编码
        /// </summary>
        public string drug_code { get; set; }

        /// <summary>
        /// 药物名称
        /// </summary>
        public string drug_name { get; set; }

        /// <summary>
        /// 用药剂量
        /// </summary>
        public decimal drug_dosage { get; set; }

        /// <summary>
        /// 用药时间
        /// </summary>
        public DateTime take_medicine_time { get; set; }

        /// <summary>
        /// 用药方式
        /// </summary>
        public string take_way { get; set; }

        /// <summary>
        /// 开方医生ID
        /// </summary>
        public int? prescribe_doctor_id { get; set; }

        /// <summary>
        /// 关联血糖记录ID
        /// </summary>
        public int? related_bs_id { get; set; }

        /// <summary>
        /// 数据来源：手动录入/Excel批量导入
        /// </summary>
        public string data_source { get; set; }

        /// <summary>
        /// 操作人ID
        /// </summary>
        public int operator_id { get; set; }

        /// <summary>
        /// 异常备注
        /// </summary>
        public string abnormal_note { get; set; }

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
