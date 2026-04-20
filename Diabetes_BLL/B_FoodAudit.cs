using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Model;
namespace BLL
{
    using DAL;
    using Model;
    using System;
    using System.Data;
    using Tools;

    /// <summary>
    /// 食物审核业务逻辑层
    /// </summary>
    public class B_FoodAudit
    {
        private readonly D_FoodAudit _dFoodAudit = new D_FoodAudit();
        private readonly D_FoodNutrition _dFoodNutrition = new D_FoodNutrition();
        private readonly D_FoodVersion _dFoodVersion = new D_FoodVersion();

        #region 审核列表查询
        /// <summary>
        /// 查询审核列表
        /// </summary>
        public BizResult GetAuditList(string auditStatus, string uploader)
        {
            try
            {
                int totalCount;
                DataTable dt = _dFoodAudit.GetAuditList(auditStatus, uploader, out totalCount);
                return BizResult.Success(data: dt, totalCount: totalCount, message: "查询成功");
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"查询审核列表失败：{ex.Message}");
            }
        }
        #endregion

        #region 审核操作
        /// <summary>
        /// 审核通过
        /// </summary>
        public BizResult AuditPass(int auditId, string currentUser)
        {
            try
            {
                if (auditId <= 0)
                    return BizResult.Fail("审核ID参数非法");
                if (string.IsNullOrWhiteSpace(currentUser))
                    return BizResult.Fail("当前登录用户信息异常");

                // 获取审核记录
                DataTable dt = _dFoodAudit.GetAuditById(auditId);
                if (dt == null || dt.Rows.Count == 0)
                    return BizResult.Fail("未找到对应审核记录");
                DataRow dr = dt.Rows[0];
                int foodId = Convert.ToInt32(dr["FoodID"]);
                string auditStatus = dr["AuditStatus"].ToString();
                string version = dr["Version"].ToString();

                if (auditStatus != "待审核")
                    return BizResult.Fail("该记录已审核，无需重复操作");

                // 更新审核状态
                int row = _dFoodAudit.AuditOperate(auditId, "审核通过", currentUser, "审核通过");
                if (row <= 0)
                    return BizResult.Fail("审核操作失败");

                // 更新食物主表审核状态与启用状态
                string sql = @"
                UPDATE Diabetes_Food_Nutrition 
                SET AuditStatus = '审核通过', EnableStatus = '启用', AuditRecord = AuditRecord + CHAR(13) + '审核通过：' + @CurrentUser + ' ' + CONVERT(VARCHAR, GETDATE(), 120)
                WHERE FoodID = @FoodID";
                SqlHelper.ExecuteNonQuery(sql,
                    new System.Data.SqlClient.SqlParameter("@FoodID", foodId),
                    new System.Data.SqlClient.SqlParameter("@CurrentUser", currentUser));

                // 更新版本记录审核状态
                string versionSql = @"UPDATE Diabetes_Food_Version SET AuditStatus = '审核通过' WHERE FoodID = @FoodID AND Version = @Version";
                SqlHelper.ExecuteNonQuery(versionSql,
                    new System.Data.SqlClient.SqlParameter("@FoodID", foodId),
                    new System.Data.SqlClient.SqlParameter("@Version", version));

                return BizResult.Success("审核通过，食物已自动启用");
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"审核通过操作失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 审核驳回
        /// </summary>
        public BizResult AuditReject(int auditId, string rejectReason, string currentUser)
        {
            try
            {
                if (auditId <= 0)
                    return BizResult.Fail("审核ID参数非法");
                if (string.IsNullOrWhiteSpace(rejectReason))
                    return BizResult.Fail("请输入驳回原因");
                if (string.IsNullOrWhiteSpace(currentUser))
                    return BizResult.Fail("当前登录用户信息异常");

                // 获取审核记录
                DataTable dt = _dFoodAudit.GetAuditById(auditId);
                if (dt == null || dt.Rows.Count == 0)
                    return BizResult.Fail("未找到对应审核记录");
                DataRow dr = dt.Rows[0];
                int foodId = Convert.ToInt32(dr["FoodID"]);
                string auditStatus = dr["AuditStatus"].ToString();
                string version = dr["Version"].ToString();

                if (auditStatus != "待审核")
                    return BizResult.Fail("该记录已审核，无需重复操作");

                // 更新审核状态
                int row = _dFoodAudit.AuditOperate(auditId, "审核驳回", currentUser, rejectReason);
                if (row <= 0)
                    return BizResult.Fail("审核驳回操作失败");

                // 更新食物主表审核状态
                string sql = @"
                UPDATE Diabetes_Food_Nutrition 
                SET AuditStatus = '审核驳回', AuditRecord = AuditRecord + CHAR(13) + '审核驳回：' + @CurrentUser + ' ' + CONVERT(VARCHAR, GETDATE(), 120) + ' 原因：' + @RejectReason
                WHERE FoodID = @FoodID";
                SqlHelper.ExecuteNonQuery(sql,
                    new System.Data.SqlClient.SqlParameter("@FoodID", foodId),
                    new System.Data.SqlClient.SqlParameter("@CurrentUser", currentUser),
                    new System.Data.SqlClient.SqlParameter("@RejectReason", rejectReason));

                // 更新版本记录审核状态
                string versionSql = @"UPDATE Diabetes_Food_Version SET AuditStatus = '审核驳回' WHERE FoodID = @FoodID AND Version = @Version";
                SqlHelper.ExecuteNonQuery(versionSql,
                    new System.Data.SqlClient.SqlParameter("@FoodID", foodId),
                    new System.Data.SqlClient.SqlParameter("@Version", version));

                return BizResult.Success("审核驳回成功");
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"审核驳回操作失败：{ex.Message}");
            }
        }
        #endregion

        /// <summary>
        /// 新增审核记录
        /// </summary>
        public BizResult AddAuditRecord(FoodAudit audit)
        {
            try
            {
                if (audit == null)
                    return BizResult.Fail("审核记录对象不能为空");
                if (audit.FoodID <= 0)
                    return BizResult.Fail("关联食物ID非法");

                int auditId = _dFoodAudit.AddAuditRecord(audit);
                if (auditId <= 0)
                    return BizResult.Fail("新增审核记录失败");

                return BizResult.Success("新增成功", auditId);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"新增审核记录失败：{ex.Message}");
            }
        }
    }
}
