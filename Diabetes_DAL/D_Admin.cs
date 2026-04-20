using Model;
using System;
using System.Data;
using System.Data.SqlClient;
using Tools;

namespace DAL
{
    public class D_Admin
    {
        /// <summary>
        /// 根据ID查询管理员信息
        /// </summary>
        public static Admin GetAdminById(int adminId)
        {
            string sql = @"
                SELECT u.*,a.* 
                FROM t_user u
                INNER JOIN t_admin a ON u.user_id = a.admin_id
                WHERE u.user_id=@AdminId AND u.user_type=3";

            SqlParameter[] paras = {
                new SqlParameter("@AdminId",adminId)
            };

            DataTable dt = SqlHelper.ExecuteDataTable(sql, paras);
            if (dt.Rows.Count == 0) return null;

            DataRow row = dt.Rows[0];
            Admin admin = new Admin();
            admin.admin_id = Convert.ToInt32(row["admin_id"]);
            admin.permission_level = Convert.ToByte(row["permission_level"]);
            admin.department = row["department"]?.ToString();
            admin.create_time = Convert.ToDateTime(row["create_time"]);
            admin.update_time = Convert.ToDateTime(row["update_time"]);
            admin.data_version = Convert.ToInt32(row["data_version"]);

            return admin;
        }

        /// <summary>
        /// 更新管理员信息
        /// </summary>
        public static int UpdateAdmin(Admin admin)
        {
            string sql = @"
                UPDATE t_admin SET
                permission_level=@Level,
                department=@Department,
                update_time=GETDATE(),
                data_version=data_version+1
                WHERE admin_id=@AdminId";

            SqlParameter[] paras = {
                new SqlParameter("@Level",admin.permission_level),
                new SqlParameter("@Department",(object)admin.department??DBNull.Value),
                new SqlParameter("@AdminId",admin.admin_id)
            };

            return SqlHelper.ExecuteNonQuery(sql, paras);
        }
    }
}