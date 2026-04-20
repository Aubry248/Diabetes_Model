using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Model
{
    /// <summary>
    /// 对应数据库 t_permission 表，系统权限
    /// </summary>
    public class Permission
    {
        public int permission_id { get; set; }
        public string permission_type { get; set; }
        public string permission_name { get; set; }
        public string permission_code { get; set; }
        public string permission_url { get; set; }
        public string permission_desc { get; set; }
        public int create_by { get; set; }
        public DateTime create_time { get; set; }
        public int update_by { get; set; }
        public DateTime update_time { get; set; }
        public int status { get; set; }
    }
}
