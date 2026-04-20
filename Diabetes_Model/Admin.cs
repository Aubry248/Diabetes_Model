using Model;
using System;

namespace Model
{
    public class Admin
    {
        public int admin_id { get; set; }
        public byte permission_level { get; set; }
        public string department { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }
        public int data_version { get; set; }

        // 导航属性：关联用户基础信息
        public Users User { get; set; }
    }
}