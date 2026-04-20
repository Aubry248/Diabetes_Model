using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Model;
using Tools;

namespace DAL
{
    public class D_Role
    {
        /// <summary>
        /// 获取所有启用的角色列表
        /// </summary>
        public List<Role> GetAllRoleList()
        {
            string sql = "SELECT * FROM t_role WHERE status=1 ORDER BY role_id ASC";
            DataTable dt = SqlHelper.GetDataTable(sql);
            List<Role> list = new List<Role>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(DataRowToModel(dr));
            }
            return list;
        }

        /// <summary>
        /// 根据ID获取角色详情
        /// </summary>
        public Role GetRoleById(int roleId)
        {
            string sql = "SELECT * FROM t_role WHERE role_id=@role_id";
            SqlParameter[] param = {
                new SqlParameter("@role_id", SqlDbType.Int) { Value = roleId }
            };
            DataTable dt = SqlHelper.GetDataTable(sql, param);
            if (dt.Rows.Count == 0) return null;
            return DataRowToModel(dt.Rows[0]);
        }

        /// <summary>
        /// 新增角色
        /// </summary>
        public int AddRole(Role role)
        {
            string sql = @"INSERT INTO t_role (role_code, role_name, role_desc, status, create_by, create_time, update_by, update_time, data_version)
                           VALUES (@role_code, @role_name, @role_desc, @status, @create_by, GETDATE(), @update_by, GETDATE(), 1);
                           SELECT SCOPE_IDENTITY();";
            SqlParameter[] param = {
                new SqlParameter("@role_code", SqlDbType.VarChar, 10) { Value = role.role_code },
                new SqlParameter("@role_name", SqlDbType.VarChar, 20) { Value = role.role_name },
                new SqlParameter("@role_desc", SqlDbType.VarChar, 100) { Value = role.role_desc ?? (object)DBNull.Value },
                new SqlParameter("@status", SqlDbType.TinyInt) { Value = role.status },
                new SqlParameter("@create_by", SqlDbType.Int) { Value = role.create_by },
                new SqlParameter("@update_by", SqlDbType.Int) { Value = role.update_by }
            };
            object obj = SqlHelper.GetSingle(sql, param);
            return obj == null ? 0 : Convert.ToInt32(obj);
        }

        /// <summary>
        /// 修改角色
        /// </summary>
        public bool UpdateRole(Role role)
        {
            string sql = @"UPDATE t_role SET role_name=@role_name, role_desc=@role_desc, update_by=@update_by, update_time=GETDATE(), data_version=data_version+1
                           WHERE role_id=@role_id";
            SqlParameter[] param = {
                new SqlParameter("@role_name", SqlDbType.VarChar, 20) { Value = role.role_name },
                new SqlParameter("@role_desc", SqlDbType.VarChar, 100) { Value = role.role_desc ?? (object)DBNull.Value },
                new SqlParameter("@update_by", SqlDbType.Int) { Value = role.update_by },
                new SqlParameter("@role_id", SqlDbType.Int) { Value = role.role_id }
            };
            return SqlHelper.ExecuteSql(sql, param) > 0;
        }

        /// <summary>
        /// 逻辑删除角色
        /// </summary>
        public bool DeleteRole(int roleId, int updateBy)
        {
            string sql = "UPDATE t_role SET status=0, update_by=@update_by, update_time=GETDATE(), data_version=data_version+1 WHERE role_id=@role_id";
            SqlParameter[] param = {
                new SqlParameter("@update_by", SqlDbType.Int) { Value = updateBy },
                new SqlParameter("@role_id", SqlDbType.Int) { Value = roleId }
            };
            return SqlHelper.ExecuteSql(sql, param) > 0;
        }

        /// <summary>
        /// 校验角色是否被用户关联
        /// </summary>
        public bool CheckRoleHasUser(int roleId)
        {
            string sql = "SELECT COUNT(1) FROM t_user_role WHERE role_id=@role_id AND status=1";
            SqlParameter[] param = {
                new SqlParameter("@role_id", SqlDbType.Int) { Value = roleId }
            };
            return Convert.ToInt32(SqlHelper.GetSingle(sql, param)) > 0;
        }

        /// <summary>
        /// 校验角色编码是否重复
        /// </summary>
        public bool CheckRoleCodeExist(string roleCode, int excludeRoleId = 0)
        {
            string sql = "SELECT COUNT(1) FROM t_role WHERE role_code=@role_code AND role_id<>@exclude_role_id";
            SqlParameter[] param = {
                new SqlParameter("@role_code", SqlDbType.VarChar, 10) { Value = roleCode },
                new SqlParameter("@exclude_role_id", SqlDbType.Int) { Value = excludeRoleId }
            };
            return Convert.ToInt32(SqlHelper.GetSingle(sql, param)) > 0;
        }

        /// <summary>
        /// 校验角色名称是否重复
        /// </summary>
        public bool CheckRoleNameExist(string roleName, int excludeRoleId = 0)
        {
            string sql = "SELECT COUNT(1) FROM t_role WHERE role_name=@role_name AND role_id<>@exclude_role_id";
            SqlParameter[] param = {
                new SqlParameter("@role_name", SqlDbType.VarChar, 20) { Value = roleName },
                new SqlParameter("@exclude_role_id", SqlDbType.Int) { Value = excludeRoleId }
            };
            return Convert.ToInt32(SqlHelper.GetSingle(sql, param)) > 0;
        }

        /// <summary>
        /// DataRow转实体
        /// </summary>
        private Role DataRowToModel(DataRow dr)
        {
            return new Role
            {
                role_id = Convert.ToInt32(dr["role_id"]),
                role_code = dr["role_code"].ToString(),
                role_name = dr["role_name"].ToString(),
                role_desc = dr["role_desc"]?.ToString(),
                status = Convert.ToInt32(dr["status"]),
                create_by = Convert.ToInt32(dr["create_by"]),
                create_time = Convert.ToDateTime(dr["create_time"]),
                update_by = Convert.ToInt32(dr["update_by"]),
                update_time = Convert.ToDateTime(dr["update_time"]),
                data_version = Convert.ToInt32(dr["data_version"])
            };
        }
    }
}