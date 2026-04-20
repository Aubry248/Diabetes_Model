using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Tools;
using Model;

namespace DAL
{
    /// <summary>
    /// 食物版本数据访问层
    /// </summary>
    public class D_FoodVersion
    {
        #region 版本列表查询
        /// <summary>
        /// 根据食物ID查询版本历史列表
        /// </summary>
        public DataTable GetVersionListByFoodId(int foodId)
        {
            string sql = @"
                SELECT * FROM Diabetes_Food_Version 
                WHERE FoodID = @FoodID
                ORDER BY UpdateTime DESC";
            SqlParameter[] param = { new SqlParameter("@FoodID", foodId) };
            return SqlHelper.ExecuteDataTable(sql, param);
        }
        #endregion

        #region 版本操作
        /// <summary>
        /// 新增版本记录
        /// </summary>
        public int AddVersionRecord(FoodVersion version)
        {
            string sql = @"
                INSERT INTO Diabetes_Food_Version (
                    FoodID, Version, UpdateTime, UpdateUser, UpdateContent, AuditStatus, FoodDataSnapshot
                ) VALUES (
                    @FoodID, @Version, @UpdateTime, @UpdateUser, @UpdateContent, @AuditStatus, @FoodDataSnapshot
                ); SELECT SCOPE_IDENTITY();";

            SqlParameter[] param = {
                new SqlParameter("@FoodID", version.FoodID),
                new SqlParameter("@Version", version.Version),
                new SqlParameter("@UpdateTime", version.UpdateTime),
                new SqlParameter("@UpdateUser", version.UpdateUser),
                new SqlParameter("@UpdateContent", version.UpdateContent ?? (object)DBNull.Value),
                new SqlParameter("@AuditStatus", version.AuditStatus),
                new SqlParameter("@FoodDataSnapshot", version.FoodDataSnapshot ?? (object)DBNull.Value)
            };
            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql, param));
        }

        /// <summary>
        /// 根据版本ID获取版本详情
        /// </summary>
        public DataTable GetVersionById(int versionId)
        {
            string sql = @"SELECT * FROM Diabetes_Food_Version WHERE VersionID = @VersionID";
            SqlParameter[] param = { new SqlParameter("@VersionID", versionId) };
            return SqlHelper.ExecuteDataTable(sql, param);
        }

        /// <summary>
        /// 更新版本审核状态
        /// </summary>
        public int UpdateVersionAuditStatus(int versionId, string auditStatus)
        {
            string sql = @"UPDATE Diabetes_Food_Version SET AuditStatus = @AuditStatus WHERE VersionID = @VersionID";
            SqlParameter[] param = {
                new SqlParameter("@VersionID", versionId),
                new SqlParameter("@AuditStatus", auditStatus)
            };
            return SqlHelper.ExecuteNonQuery(sql, param);
        }
        #endregion
    }
}