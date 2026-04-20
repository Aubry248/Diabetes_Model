using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Model
{
    /// <summary>
    /// 对应数据库 t_role_permission 表，角色-权限关联
    /// </summary>
    public class RolePermission
    {
        public int role_permission_id { get; set; }
        public int role_id { get; set; }
        public int permission_id { get; set; }
        public int create_by { get; set; }
        public string remark { get; set; }
        public int status { get; set; }
        public int data_version { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }
    }
}
