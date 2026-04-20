using System;
using System.Collections.Generic;
using System.Data;
using Model;
using DAL;
using Newtonsoft.Json;

namespace BLL
{
    /// <summary>
    /// 药物库管理业务逻辑层（最终修复版，零类型报错）
    /// </summary>
    public class B_DrugLibrary
    {
        private readonly D_DrugLibrary _dal = new D_DrugLibrary();

        #region 降糖药物业务操作
        /// <summary>
        /// 条件查询药物列表
        /// </summary>
        public BizResult GetAntidiabeticDrugList(string drugCategory, string searchKey, string prescriptionType,
            string adminRoute, string medicalInsuranceType, string enableStatus, DateTime? updateStart, DateTime? updateEnd)
        {
            try
            {
                DataTable dt = _dal.GetAntidiabeticDrugList(drugCategory, searchKey, prescriptionType, adminRoute,
                    medicalInsuranceType, enableStatus, updateStart, updateEnd);
                return BizResult.Success("查询成功", dt, dt.Rows.Count);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"查询药物列表失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 根据ID获取药物详情
        /// </summary>
        public BizResult GetAntidiabeticDrugById(int drugId)
        {
            try
            {
                if (drugId <= 0) return BizResult.Fail("药物ID无效");
                AntidiabeticDrug model = _dal.GetAntidiabeticDrugById(drugId);
                if (model == null) return BizResult.Fail("未找到该药物信息");
                return BizResult.Success("获取成功", model);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"获取药物详情失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 新增降糖药物
        /// </summary>
        public BizResult AddAntidiabeticDrug(AntidiabeticDrug model)
        {
            try
            {
                // 基础必填校验
                if (string.IsNullOrEmpty(model.DrugGenericName)) return BizResult.Fail("药物通用名为必填项");
                if (string.IsNullOrEmpty(model.DrugCategory)) return BizResult.Fail("药物分类为必填项");
                if (string.IsNullOrEmpty(model.DosageForm)) return BizResult.Fail("药物剂型为必填项");
                if (string.IsNullOrEmpty(model.Specification)) return BizResult.Fail("药物规格为必填项");
                // 唯一性校验
                if (_dal.CheckDrugNameExists(model.DrugGenericName))
                    return BizResult.Fail("该药物通用名已存在，请勿重复添加");
                // 合规性校验
                if (string.IsNullOrEmpty(model.ApprovalNumber) || !model.ApprovalNumber.Contains("国药准字"))
                    return BizResult.Fail("批准文号格式错误，需包含「国药准字」");
                // 初始化默认值
                model.AuditStatus = "待一级审核";
                model.AuditLevel = 1;
                model.EnableStatus = "启用";
                model.Version = string.IsNullOrEmpty(model.Version) ? "1.0.0" : model.Version;
                // 执行新增
                int drugId = _dal.AddAntidiabeticDrug(model);
                if (drugId <= 0) return BizResult.Fail("新增药物失败，数据库无返回");
                // 写入版本历史
                DrugVersionHistory versionModel = new DrugVersionHistory
                {
                    DrugID = drugId,
                    DrugType = "Antidiabetic",
                    DrugCode = model.DrugCode,
                    Version = model.Version,
                    DrugContent = JsonConvert.SerializeObject(model),
                    UpdateContent = "新增药物",
                    UpdateBy = model.CreateBy,
                    AuditStatus = model.AuditStatus,
                    ComplianceResult = "合规校验通过，待审核"
                };
                _dal.AddDrugVersionHistory(versionModel);
                return BizResult.Success("新增药物成功，已提交至一级审核", drugId);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"新增药物失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 更新降糖药物
        /// </summary>
        public BizResult UpdateAntidiabeticDrug(AntidiabeticDrug model)
        {
            try
            {
                if (model.DrugID <= 0) return BizResult.Fail("药物ID无效");
                if (string.IsNullOrEmpty(model.DrugGenericName)) return BizResult.Fail("药物通用名为必填项");
                // 唯一性校验
                if (_dal.CheckDrugNameExists(model.DrugGenericName, model.DrugID))
                    return BizResult.Fail("该药物通用名已存在，无法修改");
                // 合规性校验
                if (!string.IsNullOrEmpty(model.ApprovalNumber) && !model.ApprovalNumber.Contains("国药准字"))
                    return BizResult.Fail("批准文号格式错误，需包含「国药准字」");
                // 重置审核状态
                model.AuditStatus = "待一级审核";
                model.AuditLevel = 1;
                // 执行更新
                bool result = _dal.UpdateAntidiabeticDrug(model);
                if (!result) return BizResult.Fail("更新药物失败，数据库无影响");
                // 写入版本历史
                DrugVersionHistory versionModel = new DrugVersionHistory
                {
                    DrugID = model.DrugID,
                    DrugType = "Antidiabetic",
                    DrugCode = model.DrugCode,
                    Version = model.Version,
                    DrugContent = JsonConvert.SerializeObject(model),
                    UpdateContent = model.UpdateLog ?? "更新药物信息",
                    UpdateBy = model.UpdateBy,
                    AuditStatus = model.AuditStatus,
                    ComplianceResult = "合规校验通过，待审核"
                };
                _dal.AddDrugVersionHistory(versionModel);
                return BizResult.Success("更新药物成功，已重新提交审核");
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"更新药物失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 删除降糖药物
        /// </summary>
        public BizResult DeleteAntidiabeticDrug(int drugId)
        {
            try
            {
                if (drugId <= 0) return BizResult.Fail("药物ID无效");
                AntidiabeticDrug model = _dal.GetAntidiabeticDrugById(drugId);
                if (model == null) return BizResult.Fail("未找到该药物信息");
                // 修复：和数据库字符串类型匹配
                if (model.AuditStatus == "终审通过") return BizResult.Fail("终审通过的药物不允许直接删除，如需删除请先禁用");
                bool result = _dal.DeleteAntidiabeticDrug(drugId);
                return result ? BizResult.Success("删除药物成功") : BizResult.Fail("删除药物失败");
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"删除药物失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 批量更新药物启用状态
        /// </summary>
        public BizResult BatchUpdateDrugStatus(List<int> drugIdList, string enableStatus, int updateBy)
        {
            try
            {
                if (drugIdList == null || drugIdList.Count == 0) return BizResult.Fail("请先选择需要操作的药物记录");
                if (updateBy <= 0) return BizResult.Fail("操作人信息无效");
                bool result = _dal.BatchUpdateDrugStatus(drugIdList, enableStatus, updateBy);
                return result ? BizResult.Success($"批量{(enableStatus == "启用" ? "启用" : "禁用")}成功，已提交审核") : BizResult.Fail("批量操作失败");
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"批量操作失败：{ex.Message}");
            }
        }
        #endregion

        #region 审核业务操作
        /// <summary>
        /// 查询待审核药物列表
        /// </summary>
        public BizResult GetAuditDrugList(string auditStatus, string auditLevel, int? updateUser)
        {
            try
            {
                DataTable dt = _dal.GetAuditDrugList(auditStatus, auditLevel, updateUser);
                return BizResult.Success("查询成功", dt, dt.Rows.Count);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"查询审核列表失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 一级审核通过
        /// </summary>
        public BizResult FirstAuditPass(int drugId, int auditBy, string auditOpinion)
        {
            try
            {
                if (drugId <= 0) return BizResult.Fail("药物ID无效");
                if (auditBy <= 0) return BizResult.Fail("审核人信息无效");
                bool result = _dal.UpdateDrugAuditStatus(drugId, "一级审核通过", 2, auditBy, auditOpinion);
                return result ? BizResult.Success("一级审核通过，已提交至终审") : BizResult.Fail("一级审核操作失败");
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"一级审核操作失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 终审通过
        /// </summary>
        public BizResult FinalAuditPass(int drugId, int auditBy, string auditOpinion)
        {
            try
            {
                if (drugId <= 0) return BizResult.Fail("药物ID无效");
                if (auditBy <= 0) return BizResult.Fail("审核人信息无效");
                bool result = _dal.UpdateDrugAuditStatus(drugId, "终审通过", 2, auditBy, auditOpinion);
                return result ? BizResult.Success("终审通过，药物数据已生效") : BizResult.Fail("终审操作失败");
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"终审操作失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 审核驳回
        /// </summary>
        public BizResult AuditReject(int drugId, int auditBy, string auditOpinion)
        {
            try
            {
                if (drugId <= 0) return BizResult.Fail("药物ID无效");
                if (auditBy <= 0) return BizResult.Fail("审核人信息无效");
                if (string.IsNullOrEmpty(auditOpinion)) return BizResult.Fail("请输入驳回原因");
                bool result = _dal.UpdateDrugAuditStatus(drugId, "审核驳回", 1, auditBy, auditOpinion);
                return result ? BizResult.Success("审核驳回成功") : BizResult.Fail("审核驳回操作失败");
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"审核驳回操作失败：{ex.Message}");
            }
        }
        #endregion

        #region 版本管理业务操作
        /// <summary>
        /// 查询药物版本历史
        /// </summary>
        public BizResult GetDrugVersionHistory(int? drugId)
        {
            try
            {
                DataTable dt = _dal.GetDrugVersionHistory(drugId);
                return BizResult.Success("查询成功", dt, dt.Rows.Count);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"查询版本历史失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 版本回滚
        /// </summary>
        public BizResult RollbackVersion(int historyId, int operateBy)
        {
            try
            {
                if (historyId <= 0) return BizResult.Fail("版本记录ID无效");
                if (operateBy <= 0) return BizResult.Fail("操作人信息无效");
                DrugVersionHistory historyModel = _dal.GetVersionHistoryById(historyId);
                if (historyModel == null) return BizResult.Fail("未找到该版本记录");
                AntidiabeticDrug drugModel = JsonConvert.DeserializeObject<AntidiabeticDrug>(historyModel.DrugContent);
                if (drugModel == null) return BizResult.Fail("版本内容解析失败");
                // 版本号自增+更新人
                drugModel.UpdateBy = operateBy;
                drugModel.UpdateLog = $"版本回滚至{historyModel.Version}版本";
                // 版本号自增逻辑修复
                if (decimal.TryParse(drugModel.Version, out decimal versionNum))
                {
                    drugModel.Version = (versionNum + 0.01m).ToString("0.00.0");
                }
                else
                {
                    drugModel.Version = "1.0.0";
                }
                BizResult updateResult = UpdateAntidiabeticDrug(drugModel);
                return updateResult.IsSuccess ? BizResult.Success("版本回滚成功，已重新提交审核") : BizResult.Fail(updateResult.Message);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"版本回滚失败：{ex.Message}");
            }
        }
        #endregion

    }
}