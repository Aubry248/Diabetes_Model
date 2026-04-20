using System;

namespace Model
{
    /// <summary>
    /// 用户-角色关联实体类，对应t_user_role表
    /// </summary>
    public class UserRole
    {
        public int user_role_id { get; set; }
        public int user_id { get; set; }
        public int role_id { get; set; }
        public int create_by { get; set; }
        public string remark { get; set; }
        public int status { get; set; }
        public int data_version { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }
    }
}