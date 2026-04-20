using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Model
{
    /// <summary>
    /// 对应数据库 t_role 表，系统角色
    /// </summary>
    public class Role
    {
        public int role_id { get; set; }
        public string role_code { get; set; }
        public string role_name { get; set; }
        public string role_desc { get; set; }
        public int status { get; set; }
        public int create_by { get; set; }
        public DateTime create_time { get; set; }
        public int update_by { get; set; }
        public DateTime update_time { get; set; }
        public int data_version { get; set; }
    }
}
