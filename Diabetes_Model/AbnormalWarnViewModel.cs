using System;
namespace Model
{
    /// <summary>
    /// 异常预警列表视图模型
    /// </summary>
    public class AbnormalWarnViewModel
    {
        public int AbnormalId { get; set; }
        public string PatientName { get; set; }
        public string DataType { get; set; }
        public string AbnormalType { get; set; }
        public string AbnormalDesc { get; set; }
        public DateTime AbnormalTime { get; set; }
        public string HandleStatusDesc { get; set; }
        public string MarkDoctor { get; set; }
        public DateTime? MarkTime { get; set; }
            /// <summary>
            /// 异常记录主键ID（对应数据库t_abnormal表的abnormal_id）
            /// </summary>
            public int abnormal_id { get; set; }

            /// <summary>
            /// 患者ID
            /// </summary>
            public int user_id { get; set; }

            /// <summary>
            /// 数据类型
            /// </summary>
            public string data_type { get; set; }

            /// <summary>
            /// 异常类型
            /// </summary>
            public string abnormal_type { get; set; }

            /// <summary>
            /// 异常原因
            /// </summary>
            public string abnormal_reason { get; set; }

            /// <summary>
            /// 处理状态描述
            /// </summary>

            /// <summary>
            /// 处理状态编码
            /// </summary>
            public int handle_status { get; set; }

            /// <summary>
            /// 异常创建时间
            /// </summary>
            public DateTime create_time { get; set; }
        
    }
}