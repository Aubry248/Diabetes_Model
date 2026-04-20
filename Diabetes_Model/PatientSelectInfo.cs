using System;

namespace Model
{
    /// <summary>
    /// 患者选择列表视图模型
    /// </summary>
    public class PatientSelectInfo
    {
        /// <summary>
        /// 患者ID（主键，隐藏列，用于回传）
        /// </summary>
        public int user_id { get; set; }

        /// <summary>
        /// 患者姓名
        /// </summary>
        public string user_name { get; set; }

        /// <summary>
        /// 性别（1=男 2=女）
        /// </summary>
        public string gender_text { get; set; }

        /// <summary>
        /// 年龄
        /// </summary>
        public int age { get; set; }

        /// <summary>
        /// 脱敏手机号
        /// </summary>
        public string phone_desensitized { get; set; }

        /// <summary>
        /// 脱敏身份证号
        /// </summary>
        public string id_card_desensitized { get; set; }

        /// <summary>
        /// 糖尿病类型
        /// </summary>
        public string diabetes_type { get; set; }

        /// <summary>
        /// 确诊时间
        /// </summary>
        public DateTime? diagnose_date { get; set; }

        /// <summary>
        /// 评估状态（已评估/未评估）
        /// </summary>
        public string assessment_status { get; set; }

        /// <summary>
        /// 最新评估时间
        /// </summary>
        public DateTime? last_assessment_time { get; set; }
    }
}