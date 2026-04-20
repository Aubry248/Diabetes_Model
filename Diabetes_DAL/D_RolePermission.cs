using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Tools;
using Model;

namespace DAL
{
    public class D_RolePermission
    {
        /// <summary>
        /// 获取角色已分配的权限ID列表
        /// </summary>
        public List<int> GetPermissionIdListByRoleId(int roleId)
        {
            string sql = "SELECT permission_id FROM t_role_permission WHERE role_id=@role_id AND status=1";
            SqlParameter[] param = {
                new SqlParameter("@role_id", SqlDbType.Int) { Value = roleId }
            };
            DataTable dt = SqlHelper.GetDataTable(sql, param);
            List<int> list = new List<int>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(Convert.ToInt32(dr["permission_id"]));
            }
            return list;
        }

        /// <summary>
        /// 事务保存角色权限（先删后增，保证原子性）
        /// </summary>
        public bool SaveRolePermissionByTrans(int roleId, List<int> permissionIdList, int operateUserId)
        {
            using (SqlConnection conn = new SqlConnection(SqlHelper.connStr))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    // 第一步：删除该角色原有所有权限
                    string delSql = "DELETE FROM t_role_permission WHERE role_id=@role_id";
                    SqlCommand delCmd = new SqlCommand(delSql, conn, trans);
                    delCmd.Parameters.AddWithValue("@role_id", roleId);
                    delCmd.ExecuteNonQuery();

                    // 第二步：批量新增新的权限
                    foreach (int permissionId in permissionIdList)
                    {
                        string addSql = @"INSERT INTO t_role_permission (role_id, permission_id, create_by, create_time, update_by, update_time, status, data_version)
                                   VALUES (@role_id, @permission_id, @create_by, GETDATE(), @update_by, GETDATE(), 1, 1);";
                        SqlCommand addCmd = new SqlCommand(addSql, conn, trans);
                        addCmd.Parameters.AddWithValue("@role_id", roleId);
                        addCmd.Parameters.AddWithValue("@permission_id", permissionId);
                        addCmd.Parameters.AddWithValue("@create_by", operateUserId);
                        addCmd.Parameters.AddWithValue("@update_by", operateUserId);
                        addCmd.ExecuteNonQuery();
                    }

                    // 提交事务
                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    // 出错回滚
                    trans.Rollback();
                    return false;
                }
                finally
                {
                    conn.Close();
                }
            }
        
    }
    }
}