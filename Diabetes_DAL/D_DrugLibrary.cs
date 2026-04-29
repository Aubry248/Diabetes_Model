using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Model;
using Tools;

namespace DAL
{
    /// <summary>
    /// 药物库管理数据访问层（最终修复版，100%匹配数据库表）
    /// </summary>
    public class D_DrugLibrary
    {
        #region 降糖药物核心CRUD
        /// <summary>
        /// 多条件查询降糖药物列表（优化版：包含糖尿病用药核心管理字段）
        /// </summary>
        public DataTable GetAntidiabeticDrugList(string drugCategory, string searchKey, string prescriptionType,
            string adminRoute, string medicalInsuranceType, string enableStatus, DateTime? updateStart, DateTime? updateEnd)
        {
            StringBuilder sql = new StringBuilder(@"
            SELECT 
                DrugID, DrugCode, DrugCategory, SubCategory, DrugGenericName, TradeName, 
                DosageForm, Specification, AdminRoute, DailyDosageMin, DailyDosageMax, DosageUnit,
                ApprovalNumber, Manufacturer, MarketHolder, PrescriptionType, MedicalInsuranceType,
                GuideGrade, FirstSecondLine, IsFirstLine, IsDomestic, EnableStatus, AuditStatus,
                CreateTime, UpdateTime, Version, AuditLevel, CurrentAuditor, Remark
            FROM Diabetes_Medicine_Master
            WHERE IsDeleted = 0  ");

            List<SqlParameter> paramList = new List<SqlParameter>();

            // 扩展搜索范围：支持通用名、商品名、批准文号、药物编码、适应症、生产厂家搜索
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                sql.Append(@" AND (DrugGenericName LIKE @SearchKey OR TradeName LIKE @SearchKey 
                OR ApprovalNumber LIKE @SearchKey OR DrugCode LIKE @SearchKey
                OR Manufacturer LIKE @SearchKey OR Indications LIKE @SearchKey) ");
                paramList.Add(new SqlParameter("@SearchKey", $"%{searchKey}%"));
            }

            if (!string.IsNullOrWhiteSpace(drugCategory) && drugCategory != "全部")
            {
                sql.Append(" AND DrugCategory = @DrugCategory ");
                paramList.Add(new SqlParameter("@DrugCategory", drugCategory));
            }

            if (!string.IsNullOrWhiteSpace(prescriptionType) && prescriptionType != "全部")
            {
                sql.Append(" AND PrescriptionType = @PrescriptionType ");
                paramList.Add(new SqlParameter("@PrescriptionType", prescriptionType));
            }

            if (!string.IsNullOrWhiteSpace(adminRoute) && adminRoute != "全部")
            {
                sql.Append(" AND AdminRoute = @AdminRoute ");
                paramList.Add(new SqlParameter("@AdminRoute", adminRoute));
            }

            if (!string.IsNullOrWhiteSpace(medicalInsuranceType) && medicalInsuranceType != "全部")
            {
                sql.Append(" AND MedicalInsuranceType = @MedicalInsuranceType ");
                paramList.Add(new SqlParameter("@MedicalInsuranceType", medicalInsuranceType));
            }

            if (!string.IsNullOrWhiteSpace(enableStatus) && enableStatus != "全部")
            {
                sql.Append(" AND EnableStatus = @EnableStatus ");
                paramList.Add(new SqlParameter("@EnableStatus", enableStatus));
            }

            // 时间范围条件
            if (updateStart.HasValue)
            {
                sql.Append(" AND UpdateTime >= @UpdateStart ");
                paramList.Add(new SqlParameter("@UpdateStart", updateStart.Value));
            }

            if (updateEnd.HasValue)
            {
                sql.Append(" AND UpdateTime <= @UpdateEnd ");
                paramList.Add(new SqlParameter("@UpdateEnd", updateEnd.Value.AddDays(1).AddSeconds(-1)));
            }

            sql.Append(" ORDER BY IsFirstLine DESC, GuideGrade ASC, DrugGenericName ASC ");

            return SqlHelper.ExecuteDataTable(sql.ToString(), paramList.ToArray());
        }

        /// <summary>
        /// 根据ID获取药物详情
        /// </summary>
        public AntidiabeticDrug GetAntidiabeticDrugById(int drugId)
        {
            string sql = "SELECT * FROM Diabetes_Medicine_Master WHERE DrugID = @DrugID AND IsDeleted = 0";
            SqlParameter[] param = { new SqlParameter("@DrugID", drugId) };
            return SqlHelper.GetModel<AntidiabeticDrug>(sql, param);
        }

        /// <summary>
        /// 新增降糖药物，返回主键ID（列名和表完全匹配）
        /// </summary>
        public int AddAntidiabeticDrug(AntidiabeticDrug model)
        {
            string sql = @"
INSERT INTO Diabetes_Medicine_Master(
    DrugCode, DrugCategory, DrugGenericName, TradeName, DosageForm, Specification, 
    DailyDosageRange, PeakTime_h, ActionDuration_h, HalfLife_h, UsageDosage, 
    RenalImpairmentNote, DataSource, ApprovalNumber, Manufacturer, MarketHolder, 
    PrescriptionType, MedicalInsuranceType, AdminRoute, ValidityPeriod, StorageCondition, 
    EnableStatus, AuditStatus, AuditLevel, CurrentAuditor, Version, CreateTime, CreateBy, UpdateTime, UpdateBy, UpdateLog, AuditRecord, IsDeleted, Remark
) VALUES (
    @DrugCode, @DrugCategory, @DrugGenericName, @TradeName, @DosageForm, @Specification, 
    @DailyDosageRange, @PeakTime_h, @ActionDuration_h, @HalfLife_h, @UsageDosage, 
    @RenalImpairmentNote, @DataSource, @ApprovalNumber, @Manufacturer, @MarketHolder, 
    @PrescriptionType, @MedicalInsuranceType, @AdminRoute, @ValidityPeriod, @StorageCondition, 
    @EnableStatus, @AuditStatus, @AuditLevel, @CurrentAuditor, @Version, GETDATE(), @CreateBy, GETDATE(), @UpdateBy, @UpdateLog, @AuditRecord, 0, @Remark
);
SELECT SCOPE_IDENTITY();";
            SqlParameter[] param = {
                new SqlParameter("@DrugCode", model.DrugCode ?? (object)DBNull.Value),
                new SqlParameter("@DrugCategory", model.DrugCategory ?? (object)DBNull.Value),
                new SqlParameter("@DrugGenericName", model.DrugGenericName ?? (object)DBNull.Value),
                new SqlParameter("@TradeName", model.TradeName ?? (object)DBNull.Value),
                new SqlParameter("@DosageForm", model.DosageForm ?? (object)DBNull.Value),
                new SqlParameter("@Specification", model.Specification ?? (object)DBNull.Value),
                new SqlParameter("@DailyDosageRange", model.DailyDosageRange ?? (object)DBNull.Value),
                new SqlParameter("@PeakTime_h", model.PeakTime_h ?? (object)DBNull.Value),
                new SqlParameter("@ActionDuration_h", model.ActionDuration_h ?? (object)DBNull.Value),
                new SqlParameter("@HalfLife_h", model.HalfLife_h ?? (object)DBNull.Value),
                new SqlParameter("@UsageDosage", model.UsageDosage ?? (object)DBNull.Value),
                new SqlParameter("@RenalImpairmentNote", model.RenalImpairmentNote ?? (object)DBNull.Value),
                new SqlParameter("@DataSource", model.DataSource ?? "中国糖尿病防治指南(2024版)"),
                new SqlParameter("@ApprovalNumber", model.ApprovalNumber ?? (object)DBNull.Value),
                new SqlParameter("@Manufacturer", model.Manufacturer ?? (object)DBNull.Value),
                new SqlParameter("@MarketHolder", model.MarketHolder ?? (object)DBNull.Value),
                new SqlParameter("@PrescriptionType", model.PrescriptionType ?? (object)DBNull.Value),
                new SqlParameter("@MedicalInsuranceType", model.MedicalInsuranceType ?? (object)DBNull.Value),
                new SqlParameter("@AdminRoute", model.AdminRoute ?? (object)DBNull.Value),
                new SqlParameter("@ValidityPeriod", model.ValidityPeriod ?? (object)DBNull.Value),
                new SqlParameter("@StorageCondition", model.StorageCondition ?? (object)DBNull.Value),
                new SqlParameter("@EnableStatus", model.EnableStatus ?? "启用"),
                new SqlParameter("@AuditStatus", model.AuditStatus ?? "待一级审核"),
                new SqlParameter("@AuditLevel", model.AuditLevel),
                new SqlParameter("@CurrentAuditor", model.CurrentAuditor ?? (object)DBNull.Value),
                new SqlParameter("@Version", model.Version ?? "1.0.0"),
            
                new SqlParameter("@CreateBy", model.CreateBy),
         
                new SqlParameter("@UpdateBy", model.UpdateBy),
                new SqlParameter("@UpdateLog", model.UpdateLog ?? (object)DBNull.Value),
                new SqlParameter("@AuditRecord", model.AuditRecord ?? (object)DBNull.Value),
                new SqlParameter("@Remark", model.Remark ?? (object)DBNull.Value)
            };
            object result = SqlHelper.ExecuteScalar(sql, param);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        /// <summary>
        /// 更新降糖药物信息（列名和表完全匹配）
        /// </summary>
        public bool UpdateAntidiabeticDrug(AntidiabeticDrug model)
        {
            string sql = @"
UPDATE Diabetes_Medicine_Master SET 
    DrugCode = @DrugCode,
    DrugCategory = @DrugCategory,
    DrugGenericName = @DrugGenericName,
    TradeName = @TradeName,
    DosageForm = @DosageForm,
    Specification = @Specification,
    DailyDosageRange = @DailyDosageRange,
    PeakTime_h = @PeakTime_h,
    ActionDuration_h = @ActionDuration_h,
    HalfLife_h = @HalfLife_h,
    UsageDosage = @UsageDosage,
    RenalImpairmentNote = @RenalImpairmentNote,
    DataSource = @DataSource,
    ApprovalNumber = @ApprovalNumber,
    Manufacturer = @Manufacturer,
    MarketHolder = @MarketHolder,
    PrescriptionType = @PrescriptionType,
    MedicalInsuranceType = @MedicalInsuranceType,
    AdminRoute = @AdminRoute,
    ValidityPeriod = @ValidityPeriod,
    StorageCondition = @StorageCondition,
    EnableStatus = @EnableStatus,
    AuditStatus = @AuditStatus,
    AuditLevel = @AuditLevel,
    CurrentAuditor = @CurrentAuditor,
    Version = @Version,
    UpdateTime = GETDATE(),
    UpdateUser = @UpdateUser,
    UpdateBy = @UpdateBy,
    UpdateLog = @UpdateLog,
    AuditRecord = @AuditRecord,
    Remark = @Remark
WHERE DrugID = @DrugID AND IsDeleted = 0";
            SqlParameter[] param = {
                new SqlParameter("@DrugID", model.DrugID),
                new SqlParameter("@DrugCode", model.DrugCode ?? (object)DBNull.Value),
                new SqlParameter("@DrugCategory", model.DrugCategory ?? (object)DBNull.Value),
                new SqlParameter("@DrugGenericName", model.DrugGenericName ?? (object)DBNull.Value),
                new SqlParameter("@TradeName", model.TradeName ?? (object)DBNull.Value),
                new SqlParameter("@DosageForm", model.DosageForm ?? (object)DBNull.Value),
                new SqlParameter("@Specification", model.Specification ?? (object)DBNull.Value),
                new SqlParameter("@DailyDosageRange", model.DailyDosageRange ?? (object)DBNull.Value),
                new SqlParameter("@PeakTime_h", model.PeakTime_h ?? (object)DBNull.Value),
                new SqlParameter("@ActionDuration_h", model.ActionDuration_h ?? (object)DBNull.Value),
                new SqlParameter("@HalfLife_h", model.HalfLife_h ?? (object)DBNull.Value),
                new SqlParameter("@UsageDosage", model.UsageDosage ?? (object)DBNull.Value),
                new SqlParameter("@RenalImpairmentNote", model.RenalImpairmentNote ?? (object)DBNull.Value),
                new SqlParameter("@DataSource", model.DataSource ?? "中国糖尿病防治指南(2024版)"),
                new SqlParameter("@ApprovalNumber", model.ApprovalNumber ?? (object)DBNull.Value),
                new SqlParameter("@Manufacturer", model.Manufacturer ?? (object)DBNull.Value),
                new SqlParameter("@MarketHolder", model.MarketHolder ?? (object)DBNull.Value),
                new SqlParameter("@PrescriptionType", model.PrescriptionType ?? (object)DBNull.Value),
                new SqlParameter("@MedicalInsuranceType", model.MedicalInsuranceType ?? (object)DBNull.Value),
                new SqlParameter("@AdminRoute", model.AdminRoute ?? (object)DBNull.Value),
                new SqlParameter("@ValidityPeriod", model.ValidityPeriod ?? (object)DBNull.Value),
                new SqlParameter("@StorageCondition", model.StorageCondition ?? (object)DBNull.Value),
                new SqlParameter("@EnableStatus", model.EnableStatus ?? "启用"),
                new SqlParameter("@AuditStatus", model.AuditStatus ?? "待一级审核"),
                new SqlParameter("@AuditLevel", model.AuditLevel),
                new SqlParameter("@CurrentAuditor", model.CurrentAuditor ?? (object)DBNull.Value),
                new SqlParameter("@Version", model.Version ?? "1.0.0"),
                new SqlParameter("@UpdateUser", model.UpdateUser ?? "系统"),
                new SqlParameter("@UpdateBy", model.UpdateBy),
                new SqlParameter("@UpdateLog", model.UpdateLog ?? (object)DBNull.Value),
                new SqlParameter("@AuditRecord", model.AuditRecord ?? (object)DBNull.Value),
                new SqlParameter("@Remark", model.Remark ?? (object)DBNull.Value)
            };
            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }

        /// <summary>
        /// 逻辑删除降糖药物
        /// </summary>
        public bool DeleteAntidiabeticDrug(int drugId)
        {
            string sql = "UPDATE  Diabetes_Medicine_Master SET IsDeleted = 1, UpdateTime = GETDATE() WHERE DrugID = @DrugID";
            SqlParameter[] param = { new SqlParameter("@DrugID", drugId) };
            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }

        /// <summary>
        /// 批量更新药物启用状态
        /// </summary>
        public bool BatchUpdateDrugStatus(List<int> drugIdList, string enableStatus, int updateBy)
        {
            if (drugIdList == null || drugIdList.Count == 0) return false;
            string drugIds = string.Join(",", drugIdList);
            string sql = $@"
UPDATE  Diabetes_Medicine_Master SET 
    EnableStatus = @EnableStatus,
    UpdateTime = GETDATE(),
    UpdateBy = @UpdateBy,
    AuditStatus = '待一级审核',
    AuditLevel = 1
WHERE DrugID IN ({drugIds}) AND IsDeleted = 0";
            SqlParameter[] param = {
                new SqlParameter("@EnableStatus", enableStatus),
                new SqlParameter("@UpdateBy", updateBy)
            };
            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }

        /// <summary>
        /// 校验药物通用名唯一性
        /// </summary>
        public bool CheckDrugNameExists(string drugGenericName, int excludeDrugId = 0)
        {
            string sql = "SELECT COUNT(1) FROM Diabetes_Medicine_Master WHERE DrugGenericName = @DrugGenericName AND IsDeleted = 0";
            List<SqlParameter> paramList = new List<SqlParameter>
            { new SqlParameter("@DrugGenericName", drugGenericName) };
            if (excludeDrugId > 0)
            {
                sql += " AND DrugID != @ExcludeDrugId";
                paramList.Add(new SqlParameter("@ExcludeDrugId", excludeDrugId));
            }
            object result = SqlHelper.ExecuteScalar(sql, paramList.ToArray());
            return result != null && Convert.ToInt32(result) > 0;
        }
        #endregion

        #region 审核相关数据操作
        /// <summary>
        /// 查询待审核药物列表
        /// </summary>
        public DataTable GetAuditDrugList(string auditStatus, string auditLevel, int? updateUser)
        {
            StringBuilder sql = new StringBuilder(@"
SELECT 
    d.DrugID, d.DrugCode, d.DrugGenericName, d.AuditLevel, d.AuditStatus,
    CASE d.AuditStatus 
        WHEN '待一级审核' THEN '待一级审核' WHEN '一级审核通过' THEN '一级审核通过' 
        WHEN '待终审' THEN '待终审' WHEN '终审通过' THEN '终审通过' WHEN '审核驳回' THEN '审核驳回' 
    END AS AuditStatusName,
    CASE d.AuditLevel WHEN 1 THEN '一级审核（药师）' ELSE '终审（医师/管理员）' END AS AuditLevelName,
    u.user_name AS CurrentAuditor,
    d.Remark AS ComplianceTip,
    d.CreateTime AS SubmitTime
FROM  Diabetes_Medicine_Master d
LEFT JOIN t_user u ON d.CurrentAuditor = u.user_id
WHERE d.IsDeleted = 0");
            List<SqlParameter> paramList = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(auditStatus) && auditStatus != "全部")
            {
                sql.Append(" AND d.AuditStatus = @AuditStatus");
                paramList.Add(new SqlParameter("@AuditStatus", auditStatus));
            }
            if (!string.IsNullOrEmpty(auditLevel) && auditLevel != "全部")
            {
                sql.Append(" AND d.AuditLevel = @AuditLevel");
                paramList.Add(new SqlParameter("@AuditLevel", auditLevel));
            }
            if (updateUser.HasValue && updateUser > 0)
            {
                sql.Append(" AND d.UpdateBy = @UpdateUser");
                paramList.Add(new SqlParameter("@UpdateUser", updateUser.Value));
            }
            sql.Append(" ORDER BY d.UpdateTime DESC");
            return SqlHelper.ExecuteDataTable(sql.ToString(), paramList.ToArray());
        }

        /// <summary>
        /// 更新药物审核状态（事务+日志）
        /// </summary>
        public bool UpdateDrugAuditStatus(int drugId, string auditStatus, int auditLevel, int auditBy, string auditOpinion)
        {
            string updateSql = @"
UPDATE  Diabetes_Medicine_Master SET 
    AuditStatus = @AuditStatus,
    AuditLevel = @AuditLevel,
    CurrentAuditor = @AuditBy,
    UpdateTime = GETDATE()
WHERE DrugID = @DrugID AND IsDeleted = 0";
            SqlParameter[] updateParam = {
                new SqlParameter("@DrugID", drugId),
                new SqlParameter("@AuditStatus", auditStatus),
                new SqlParameter("@AuditLevel", auditLevel),
                new SqlParameter("@AuditBy", auditBy)
            };
            string logSql = @"
INSERT INTO Diabetes_Drug_Audit_Log (DrugID, DrugType, AuditLevel, AuditStatus, AuditOpinion, AuditBy)
VALUES (@DrugID, 'Antidiabetic', @AuditLevel, @AuditStatus, @AuditOpinion, @AuditBy)";
            SqlParameter[] logParam = {
                new SqlParameter("@DrugID", drugId),
                new SqlParameter("@AuditLevel", auditLevel),
                new SqlParameter("@AuditStatus", auditStatus),
                new SqlParameter("@AuditOpinion", auditOpinion ?? (object)DBNull.Value),
                new SqlParameter("@AuditBy", auditBy)
            };
            // 事务执行
            List<string> sqlList = new List<string> { updateSql, logSql };
            List<SqlParameter[]> paramList = new List<SqlParameter[]> { updateParam, logParam };
            return SqlHelper.ExecuteSqlTran(sqlList, paramList);
        }
        #endregion

        #region 版本管理数据操作
        /// <summary>
        /// 查询药物版本历史
        /// </summary>
        public DataTable GetDrugVersionHistory(int? drugId)
        {
            StringBuilder sql = new StringBuilder(@"
SELECT 
    h.HistoryID, h.Version, h.UpdateTime, u.user_name AS UpdateUser,
    h.UpdateContent,
    CASE h.AuditStatus 
        WHEN '待一级审核' THEN '待审核' WHEN '终审通过' THEN '已通过' WHEN '审核驳回' THEN '已驳回' 
    END AS AuditStatus,
    h.ComplianceResult
FROM Diabetes_Drug_Version_History h
LEFT JOIN t_user u ON h.UpdateBy = u.user_id
WHERE 1=1");
            List<SqlParameter> paramList = new List<SqlParameter>();
            if (drugId.HasValue && drugId > 0)
            {
                sql.Append(" AND h.DrugID = @DrugID");
                paramList.Add(new SqlParameter("@DrugID", drugId.Value));
            }
            sql.Append(" ORDER BY h.UpdateTime DESC");
            return SqlHelper.ExecuteDataTable(sql.ToString(), paramList.ToArray());
        }

        /// <summary>
        /// 新增版本历史记录
        /// </summary>
        public bool AddDrugVersionHistory(DrugVersionHistory model)
        {
            string sql = @"
INSERT INTO Diabetes_Drug_Version_History (
    DrugID, DrugType, DrugCode, Version, DrugContent, UpdateContent, UpdateBy, AuditStatus, ComplianceResult
) VALUES (
    @DrugID, @DrugType, @DrugCode, @Version, @DrugContent, @UpdateContent, @UpdateBy, @AuditStatus, @ComplianceResult
)";
            SqlParameter[] param = {
                new SqlParameter("@DrugID", model.DrugID),
                new SqlParameter("@DrugType", model.DrugType),
                new SqlParameter("@DrugCode", model.DrugCode ?? (object)DBNull.Value),
                new SqlParameter("@Version", model.Version),
                new SqlParameter("@DrugContent", model.DrugContent),
                new SqlParameter("@UpdateContent", model.UpdateContent ?? (object)DBNull.Value),
                new SqlParameter("@UpdateBy", model.UpdateBy),
                new SqlParameter("@AuditStatus", model.AuditStatus),
                new SqlParameter("@ComplianceResult", model.ComplianceResult ?? (object)DBNull.Value)
            };
            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }

        /// <summary>
        /// 根据ID获取版本详情
        /// </summary>
        public DrugVersionHistory GetVersionHistoryById(int historyId)
        {
            string sql = "SELECT * FROM Diabetes_Drug_Version_History WHERE HistoryID = @HistoryID";
            SqlParameter[] param = { new SqlParameter("@HistoryID", historyId) };
            return SqlHelper.GetModel<DrugVersionHistory>(sql, param);
        }
        #endregion
    }
}