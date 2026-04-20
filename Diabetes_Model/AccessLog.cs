using System;

namespace Model
{
    public class AccessLog
    {
        public int access_id { get; set; }
        public int user_id { get; set; }
        public int access_role_id { get; set; }
        public string interface_url { get; set; }
        public string interface_module { get; set; }
        public string request_method { get; set; }
        public string request_param { get; set; }
        public string response_result { get; set; }
        public int access_status { get; set; }
        public string error_msg { get; set; }
        public int response_time { get; set; }
        public string ip_address { get; set; }
        public string device_type { get; set; }
        public DateTime access_time { get; set; }
        public int data_version { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }
        public bool is_sensitive_operation { get; set; }
        public int data_sensitivity_level { get; set; }
        public string table_name { get; set; }
        public int record_id { get; set; }
        public string action { get; set; }
        public string old_value { get; set; }
        public string new_value { get; set; }
        public string changed_by { get; set; }
        public DateTime change_time { get; set; }

        // 关联字段
        public string user_name { get; set; }
        public string role_name { get; set; }
    }
}