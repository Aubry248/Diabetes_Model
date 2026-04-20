// Model/MedicineReminder.cs
using System;

namespace Model
{
    /// <summary>
    /// 用药提醒实体
    /// </summary>
    public class MedicineReminder
    {
        public int reminder_id { get; set; }
        public int user_id { get; set; }
        public string drug_name { get; set; }
        public string drug_dosage { get; set; }
        public string take_way { get; set; }
        public string reminder_time { get; set; } // 格式：HH:mm
        public bool is_enabled { get; set; }
        public string remark { get; set; }
        public int data_version { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }
    }
}