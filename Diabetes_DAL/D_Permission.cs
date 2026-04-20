using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace Diabetes_DAL
{
    public class D_Permission
    {
        /// <summary>
        /// 获取所有启用的权限列表（返回实体列表，更符合分层设计）
        /// </summary>
        public List<Permission> GetAllPermissionList() // 修正返回类型
        {
            string sql = "SELECT * FROM t_permission WHERE status=1 ORDER BY permission_type, permission_id";
            DataTable dt = SqlHelper.GetDataTable(sql);
            List<Permission> list = new List<Permission>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(DataRowToModel(dr));
            }
            return list; // 现在返回类型与声明一致
        }

        /// <summary>
        /// DataRow转实体
        /// </summary>
        private Permission DataRowToModel(DataRow dr)
        {
            return new Permission
            {
                permission_id = Convert.ToInt32(dr["permission_id"]),
                permission_type = dr["permission_type"].ToString(),
                permission_name = dr["permission_name"].ToString(),
                permission_code = dr["permission_code"].ToString(),
                permission_url = dr["permission_url"]?.ToString(),
                permission_desc = dr["permission_desc"]?.ToString(),
                create_by = Convert.ToInt32(dr["create_by"]),
                create_time = Convert.ToDateTime(dr["create_time"]),
                update_by = Convert.ToInt32(dr["update_by"]),
                update_time = Convert.ToDateTime(dr["update_time"]),
                status = Convert.ToInt32(dr["status"])
            };
        }
    }
}