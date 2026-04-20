using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace Tools
{
    /// <summary>
    /// SQL Server数据库操作帮助类（修复版：彻底解决SqlParameter复用报错）
    /// 封装增删改查全操作，参数化查询防SQL注入
    /// </summary>
    public static class SqlHelper
    {
        // 保留你原有的连接字符串，无需修改
        public static readonly string connStr = "Server=172.16.1.140,1433;Database=DB_DiabetesHealthManagement;User ID=sa;Password=18318482923;TrustServerCertificate=True;";
        #region 【核心私有方法】SqlParameter克隆（解决报错的核心！）
        /// <summary>
        /// 克隆SqlParameter对象，彻底避免同一个参数被多个Command复用的报错
        /// </summary>
        private static SqlParameter CloneParameter(SqlParameter sourceParam)
        {
            if (sourceParam == null) return null;
            return new SqlParameter(sourceParam.ParameterName, sourceParam.SqlDbType, sourceParam.Size)
            {
                Value = sourceParam.Value,
                Direction = sourceParam.Direction,
                IsNullable = sourceParam.IsNullable,
                SourceColumn = sourceParam.SourceColumn,
                SourceVersion = sourceParam.SourceVersion,
                Precision = sourceParam.Precision,
                Scale = sourceParam.Scale
            };
        }

        /// <summary>
        /// 给SqlCommand安全添加参数（克隆后添加，避免复用报错）
        /// </summary>
        private static void SafeAddParameters(SqlCommand cmd, SqlParameter[] parameters)
        {
            if (cmd == null) return;
            // 清空原有参数，避免污染
            cmd.Parameters.Clear();
            // 空参数直接返回
            if (parameters == null || parameters.Length == 0) return;
            // 逐个克隆后添加，彻底解决复用问题
            foreach (var param in parameters)
            {
                if (param != null)
                {
                    cmd.Parameters.Add(CloneParameter(param));
                }
            }
        }
        #endregion

        #region 通用方法：执行增删改（INSERT/UPDATE/DELETE）
        /// <summary>
        /// 执行增删改语句，返回受影响的行数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数（可选）</param>
        /// <returns>受影响的行数</returns>
        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    // 【修复】安全添加克隆后的参数
                    SafeAddParameters(cmd, parameters);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 兼容原有ExecuteSql方法（和ExecuteNonQuery完全一致，保留兼容）
        /// </summary>
        public static int ExecuteSql(string sql, params SqlParameter[] parameters)
        {
            return ExecuteNonQuery(sql, parameters);
        }
        #endregion

        #region 通用方法：查询单个值（比如COUNT、ID、密码校验）
        /// <summary>
        /// 执行查询，返回第一行第一列的结果
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数（可选）</param>
        /// <returns>查询结果</returns>
        public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    // 【修复】安全添加克隆后的参数
                    SafeAddParameters(cmd, parameters);
                    return cmd.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// 兼容原有GetSingle方法（和ExecuteScalar完全一致，保留兼容）
        /// </summary>
        public static object GetSingle(string sql, params SqlParameter[] parameters)
        {
            return ExecuteScalar(sql, parameters);
        }
        #endregion

        #region 通用方法：查询数据表（返回DataTable）
        /// <summary>
        /// 执行查询，返回DataTable结果集
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数（可选）</param>
        /// <returns>DataTable结果集</returns>
        public static DataTable ExecuteDataTable(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    // 【修复】安全添加克隆后的参数
                    SafeAddParameters(cmd, parameters);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        /// <summary>
        /// 兼容原有GetDataTable方法（和ExecuteDataTable完全一致，保留兼容）
        /// </summary>
        public static DataTable GetDataTable(string sql, params SqlParameter[] parameters)
        {
            return ExecuteDataTable(sql, parameters);
        }
        #endregion

        #region 通用方法：查询实体列表（泛型，直接转Model实体）
        /// <summary>
        /// 执行查询，返回对应Model的实体列表
        /// </summary>
        /// <typeparam name="T">Model实体类型</typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数（可选）</param>
        /// <returns>实体列表</returns>
        public static List<T> GetModelList<T>(string sql, params SqlParameter[] parameters) where T : class, new()
        {
            DataTable dt = ExecuteDataTable(sql, parameters);
            List<T> list = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T model = new T();
                Type type = typeof(T);
                foreach (PropertyInfo prop in type.GetProperties())
                {
                    if (dt.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                    {
                        prop.SetValue(model, row[prop.Name]);
                    }
                }
                list.Add(model);
            }
            return list;
        }
        #endregion

        #region 通用方法：查询单个实体
        /// <summary>
        /// 执行查询，返回单个Model实体
        /// </summary>
        /// <typeparam name="T">Model实体类型</typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数（可选）</param>
        /// <returns>单个实体，无数据返回null</returns>
        public static T GetModel<T>(string sql, params SqlParameter[] parameters) where T : class, new()
        {
            var list = GetModelList<T>(sql, parameters);
            return list.Count > 0 ? list[0] : null;
        }
        #endregion

        #region 事务执行方法（修复参数复用问题）
        /// <summary>
        /// 执行多条SQL语句的事务（数组参数，兼容原有调用）
        /// </summary>
        public static bool ExecuteTransaction(string[] sqlArray, SqlParameter[][] paramArray)
        {
            List<string> sqlList = new List<string>(sqlArray);
            List<SqlParameter[]> paramList = new List<SqlParameter[]>(paramArray);
            return ExecuteSqlTran(sqlList, paramList);
        }

        /// <summary>
        /// 执行多条SQL语句的事务（List参数，和原有方法完全兼容）
        /// </summary>
        public static bool ExecuteTransaction(List<string> sqlList, List<SqlParameter[]> paramList)
        {
            return ExecuteSqlTran(sqlList, paramList);
        }

        /// <summary>
        /// 执行多条SQL语句的事务（保证原子性，修复参数复用问题）
        /// </summary>
        public static bool ExecuteSqlTran(List<string> sqlList, List<SqlParameter[]> paramList)
        {
            // 校验参数：SQL列表和参数列表长度必须一致
            if (sqlList == null || paramList == null || sqlList.Count != paramList.Count)
            {
                throw new ArgumentException("SQL语句列表和参数列表长度必须一致！");
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        // 循环执行每一条SQL
                        for (int i = 0; i < sqlList.Count; i++)
                        {
                            if (string.IsNullOrWhiteSpace(sqlList[i])) continue;
                            using (SqlCommand cmd = new SqlCommand(sqlList[i], conn, tran))
                            {
                                // 【修复】安全添加克隆后的参数，彻底避免事务内参数复用报错
                                SafeAddParameters(cmd, paramList[i]);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tran.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw new Exception($"事务执行失败：{ex.Message}", ex);
                    }
                }
            }
        }
        #endregion

        #region 新增：带现有事务的增删改执行方法（修复BatchAddExercise调用报错）
        /// <summary>
        /// 在现有事务中执行增删改语句，返回受影响的行数
        /// </summary>
        /// <param name="tran">现有开启的数据库事务</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <returns>受影响的行数</returns>
        public static int ExecuteNonQuery(SqlTransaction tran, string sql, params SqlParameter[] parameters)
        {
            if (tran == null)
                throw new ArgumentNullException(nameof(tran), "事务对象不能为空");
            if (tran.Connection == null)
                throw new ArgumentException("事务关联的连接已关闭", nameof(tran));

            using (SqlCommand cmd = new SqlCommand(sql, tran.Connection, tran))
            {
                // 复用你现有的安全参数添加方法，避免参数复用报错
                SafeAddParameters(cmd, parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 兼容原有命名：ExecuteSql 带事务重载
        /// </summary>
        public static int ExecuteSql(SqlTransaction tran, string sql, params SqlParameter[] parameters)
        {
            return ExecuteNonQuery(tran, sql, parameters);
        }
        #endregion
    }
}