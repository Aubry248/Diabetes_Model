using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Tools;
using Model;

namespace DAL
{
    /// <summary>
    /// 食物审核数据访问层
    /// </summary>
    public class D_FoodAudit
    {
        #region 审核列表查询
        /// <summary>
        /// 按条件查询审核列表
        /// </summary>
        public DataTable GetAuditList(string auditStatus, string uploader, out int totalCount)
        {
            string sqlWhere = @" WHERE 1=1 ";
            var paramList = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(auditStatus) && auditStatus != "全部")
            {
                sqlWhere += " AND AuditStatus = @AuditStatus";
                paramList.Add(new SqlParameter("@AuditStatus", auditStatus));
            }
            if (!string.IsNullOrEmpty(uploader) && uploader != "全部")
            {
                sqlWhere += " AND Uploader = @Uploader";
                paramList.Add(new SqlParameter("@Uploader", uploader));
            }

            // 获取总条数
            string countSql = $"SELECT COUNT(1) FROM Diabetes_Food_Audit {sqlWhere}";
            totalCount = Convert.ToInt32(SqlHelper.ExecuteScalar(countSql, paramList.ToArray()));

            // 查询数据
            string sql = $@"
                SELECT * FROM Diabetes_Food_Audit {sqlWhere}
                ORDER BY UploadTime DESC";
            return SqlHelper.ExecuteDataTable(sql, paramList.ToArray());
        }
        #endregion

        #region 审核操作
        /// <summary>
        /// 新增审核记录
        /// </summary>
        public int AddAuditRecord(FoodAudit audit)
        {
            string sql = @"
        INSERT INTO Diabetes_Food_Audit (
            FoodID, FoodCode, FoodName, Uploader, UploadTime, AuditStatus, Version, Remark
        ) VALUES (
            @FoodID, @FoodCode, @FoodName, @Uploader, @UploadTime, @AuditStatus, @Version, @Remark
        ); SELECT SCOPE_IDENTITY();";

            SqlParameter[] param = {
        new SqlParameter("@FoodID", audit.FoodID),
        new SqlParameter("@FoodCode", audit.FoodCode),
        new SqlParameter("@FoodName", audit.FoodName),
        new SqlParameter("@Uploader", audit.Uploader),
        new SqlParameter("@UploadTime", audit.UploadTime),
        new SqlParameter("@AuditStatus", audit.AuditStatus),
        new SqlParameter("@Version", audit.Version),
        new SqlParameter("@Remark", audit.Remark ?? (object)DBNull.Value)
    };
            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql, param));
        }

        /// <summary>
        /// 审核操作（通过/驳回）
        /// </summary>
        public int AuditOperate(int auditId, string auditStatus, string auditUser, string remark)
        {
            string sql = @"
                UPDATE Diabetes_Food_Audit 
                SET AuditStatus = @AuditStatus, AuditUser = @AuditUser, AuditTime = GETDATE(), Remark = @Remark
                WHERE AuditID = @AuditID";

            SqlParameter[] param = {
                new SqlParameter("@AuditID", auditId),
                new SqlParameter("@AuditStatus", auditStatus),
                new SqlParameter("@AuditUser", auditUser),
                new SqlParameter("@Remark", remark ?? (object)DBNull.Value)
            };
            return SqlHelper.ExecuteNonQuery(sql, param);
        }

        /// <summary>
        /// 根据审核ID获取审核详情
        /// </summary>
        public DataTable GetAuditById(int auditId)
        {
            string sql = @"SELECT * FROM Diabetes_Food_Audit WHERE AuditID = @AuditID";
            SqlParameter[] param = { new SqlParameter("@AuditID", auditId) };
            return SqlHelper.ExecuteDataTable(sql, param);
        }
        #endregion
    }
}