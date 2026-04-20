using System;

namespace Model
{
    public class AuditLog
    {
        public int audit_id { get; set; }
        public int operate_user_id { get; set; }
        public string operate_type { get; set; }
        public string operate_content { get; set; }
        public string operate_ip { get; set; }
        public string operate_device { get; set; }
        public DateTime operate_time { get; set; }
        public string remark { get; set; }
        public int data_version { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }

        // 关联字段
        public string user_name { get; set; }
        public string role_name { get; set; }
        public int role_id { get; set; }
    }
}