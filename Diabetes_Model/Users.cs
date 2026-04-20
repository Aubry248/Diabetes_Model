using System;

namespace Model
{
    /// <summary>
    /// 用户实体类，对应t_user表
    /// </summary>
    public class Users
    {
        public int user_id { get; set; }
        public string user_name { get; set; }
        public string id_card { get; set; }
        public string phone { get; set; }
        public string emergency_contact { get; set; }
        public string emergency_phone { get; set; }
        public string password { get; set; }
        /// <summary>
        /// 用户类型：1-普通患者 2-公卫医生 3-系统管理员
        /// </summary>
        public int user_type { get; set; }
        public string diabetes_type { get; set; }
        public DateTime? diagnose_date { get; set; }
        public decimal? fasting_glucose_baseline { get; set; }
        public DateTime? last_login_time { get; set; }
        public int data_version { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }
        /// <summary>
        /// 账号状态：1-启用 0-禁用
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 性别：1-男 2-女
        /// </summary>
        public int gender { get; set; }
        public byte[] phone_encrypted { get; set; }
        public byte[] id_card_encrypted { get; set; }
        public DateTime? birth_date { get; set; }
        public int age { get; set; }
        public string login_account { get; set; }

        /// <summary>
        /// 是否首次登录 1=是 0=否
        /// </summary>
        public int is_first_login { get; set; }

        /// <summary>
        /// 最后一次密码修改时间
        /// </summary>
        public DateTime? last_pwd_change_time { get; set; }

        /// <summary>
        /// 登录失败累计次数
        /// </summary>
        public int login_fail_count { get; set; }

        /// <summary>
        /// 账号锁定结束时间
        /// </summary>
        public DateTime? lock_end_time { get; set; }

        public string salt { get; set; }

        // 导航属性：一对一关联角色扩展表
        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
        public Admin Admin { get; set; }
    }
}