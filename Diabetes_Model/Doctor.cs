using Model;
using System;

namespace Model
{
    public class Doctor
    {
        public int doctor_id { get; set; }
        public string title { get; set; }
        public string department { get; set; }
        public string hospital { get; set; }
        public string qualification_no { get; set; }
        public string specialty { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }
        public int data_version { get; set; }

        // 导航属性：关联用户基础信息
        public Users User { get; set; }
    }
}