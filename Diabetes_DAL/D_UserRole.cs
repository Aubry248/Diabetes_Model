using System;
using System.Data;
using System.Data.SqlClient;
using Tools;

namespace DAL
{
    /// <summary>
    /// 用户-角色关联数据访问层
    /// </summary>
    public class D_UserRole
    {
        /// <summary>
        /// 绑定用户和角色（先删后增，保证唯一）
        /// </summary>
        public bool BindUserRole(int userId, int roleId, int operateUserId)
        {
            // 修复字段名：connectionString → connStr，和你的SqlHelper完全匹配
            using (SqlConnection conn = new SqlConnection(SqlHelper.connStr))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    // 删除原有绑定
                    string delSql = "DELETE FROM t_user_role WHERE user_id = @user_id";
                    SqlCommand delCmd = new SqlCommand(delSql, conn, trans);
                    delCmd.Parameters.AddWithValue("@user_id", userId);
                    delCmd.ExecuteNonQuery();

                    // 新增新绑定
                    string insertSql = @"
INSERT INTO t_user_role (user_id, role_id, create_by, status, data_version, create_time, update_time)
VALUES (@user_id, @role_id, @create_by, 1, 1, GETDATE(), GETDATE())";
                    SqlCommand insertCmd = new SqlCommand(insertSql, conn, trans);
                    insertCmd.Parameters.AddWithValue("@user_id", userId);
                    insertCmd.Parameters.AddWithValue("@role_id", roleId);
                    insertCmd.Parameters.AddWithValue("@create_by", operateUserId);
                    insertCmd.ExecuteNonQuery();

                    trans.Commit();
                    return true;
                }
                catch
                {
                    trans.Rollback();
                    return false;
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// 获取用户绑定的角色ID
        /// </summary>
        public int GetUserRoleId(int userId)
        {
            string sql = "SELECT TOP 1 role_id FROM t_user_role WHERE user_id = @user_id AND status=1";
            SqlParameter[] param = { new SqlParameter("@user_id", userId) };
            object obj = SqlHelper.ExecuteScalar(sql, param);
            return obj == null ? 0 : Convert.ToInt32(obj);
        }
    }
}