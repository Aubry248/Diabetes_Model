using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DAL;
using Model;

namespace BLL
{
    public class B_Medicine
    {
        private readonly D_Medicine _dalMedicine = new D_Medicine();
        private static bool _epplusLicenseConfigured = false;

        private static void EnsureEpplusLicenseConfigured()
        {
            if (_epplusLicenseConfigured)
            {
                return;
            }

            // EPPlus 8 必须显式设置许可证，否则会抛 LicenseNotSetException
            OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("DiabetesHealthManagement");
            _epplusLicenseConfigured = true;
        }

        #region 原有核心方法（完全保留，仅统一data_status=1）
        public List<AntidiabeticDrug> GetDrugDictionary()
        {
            try
            {
                var list = _dalMedicine.GetDrugDictionaryList();
                // 确保返回非空列表，避免后续绑定异常
                return list ?? new List<AntidiabeticDrug>();
            }
            catch (Exception ex)
            {
                // 记录详细错误日志，方便排查
                System.Diagnostics.Debug.WriteLine($"【药物字典加载异常】{ex.Message}\n{ex.StackTrace}");
                // 兜底数据：确保页面不会崩溃
                return new List<AntidiabeticDrug>
        {
            new AntidiabeticDrug { DrugCode = "MET", DrugGenericName = "盐酸二甲双胍片" },
            new AntidiabeticDrug { DrugCode = "GLM", DrugGenericName = "格列美脲片" },
            new AntidiabeticDrug { DrugCode = "INS", DrugGenericName = "胰岛素注射液" },
            new AntidiabeticDrug { DrugCode = "ACA", DrugGenericName = "阿卡波糖片" }
        };
            }
        }

        public ResultModel SaveMedicineRecord(Medicine medicine)
        {
            try
            {
                if (medicine == null || medicine.user_id <= 0)
                    return new ResultModel(false, "用户信息无效，无法保存");
                if (string.IsNullOrWhiteSpace(medicine.drug_name))
                    return new ResultModel(false, "药物名称不能为空");
                if (medicine.drug_dosage <= 0)
                    return new ResultModel(false, "用药剂量必须大于0");

                // 适配新架构：统一有效状态=1
                medicine.data_status = 1;
                medicine.data_version = 1;

                int recordId = _dalMedicine.AddMedicineRecord(medicine);
                return recordId > 0 ? new ResultModel(true, "用药记录保存成功", recordId) : new ResultModel(false, "保存失败，数据未写入数据库");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"【保存用药记录异常】{ex.Message}\n{ex.StackTrace}");
                return new ResultModel(false, $"系统异常：{ex.Message}");
            }
        }

        public (bool Success, string Message, int SuccessCount) BatchImportSimulateData(List<Medicine> list)
        {
            if (list == null || list.Count == 0) return (false, "导入数据不能为空", 0);
            try
            {
                // 适配新架构：统一有效状态=1
                list.ForEach(item => { item.data_status = 1; item.data_version = 1; });
                int count = _dalMedicine.BatchAddMedicineRecord(list);
                return count > 0 ? (true, $"批量导入完成！共成功导入{count}条数据", count) : (false, "导入失败，无数据写入数据库", 0);
            }
            catch (Exception ex) { return (false, "批量导入数据异常：" + ex.Message, 0); }
        }

        public List<Medicine> GetUserMedicineList(int userId)
        {
            if (userId <= 0) throw new ArgumentException("用户ID无效");
            try { return _dalMedicine.GetUserMedicineList(userId); }
            catch (Exception ex) { throw new Exception("获取用药记录列表失败：" + ex.Message, ex); }
        }

        public (bool Success, string Message, int CalibrateCount) CalibrateMedicineData(int userId)
        {
            if (userId <= 0) return (false, "用户信息无效", 0);
            try
            {
                var list = GetUserMedicineList(userId);
                if (list.Count == 0) return (false, "暂无用药数据可校准", 0);
                int calibrateCount = 0;
                List<Medicine> needUpdateList = new List<Medicine>();
                foreach (var item in list)
                {
                    bool isModified = false;
                    if (item.drug_dosage > 5 || item.drug_dosage < 0.1m) { item.drug_dosage = 0.5m; isModified = true; }
                    if (item.take_medicine_time > DateTime.Now) { item.take_medicine_time = DateTime.Now; isModified = true; }
                    if (item.related_bs_id < 0) { item.related_bs_id = null; isModified = true; }
                    if (string.IsNullOrEmpty(item.data_source)) { item.data_source = "未知来源"; isModified = true; }
                    if (isModified) { needUpdateList.Add(item); calibrateCount++; }
                }
                if (needUpdateList.Count > 0)
                {
                    int updateCount = _dalMedicine.BatchUpdateMedicineRecord(needUpdateList);
                    if (updateCount != calibrateCount) return (false, "校准异常，部分数据更新失败", 0);
                }
                return (true, $"多源数据校准完成！共修正异常数据{calibrateCount}条", calibrateCount);
            }
            catch (Exception ex) { return (false, "数据校准异常：" + ex.Message, 0); }
        }
        #endregion

        // 其余方法完全保留，无修改
        #region 医生端专用方法（完全保留）
        public DataTable GetPatientList()
        {
            try { return _dalMedicine.GetPatientList(); }
            catch (Exception ex) { throw new Exception("获取患者列表失败：" + ex.Message, ex); }
        }

        public List<Medicine> GetUserMedicineRecordByTime(int userId, DateTime startTime, DateTime endTime, out string msg)
        {
            msg = "";
            try
            {
                if (userId <= 0) { msg = "请选择有效患者"; return null; }
                if (startTime > endTime) { msg = "开始时间不能晚于结束时间"; return null; }
                var list = _dalMedicine.GetUserMedicineRecordByTime(userId, startTime, endTime);
                if (list == null || list.Count == 0) msg = "该时间段内暂无用药记录";
                return list;
            }
            catch (Exception ex) { msg = $"查询用药记录异常：{ex.Message}"; throw new Exception(msg, ex); }
        }

        public DataTable GetPatientBloodSugarData(int userId, out string msg)
        {
            msg = "";
            try
            {
                if (userId <= 0) { msg = "请选择有效患者"; return null; }
                var list = _dalMedicine.GetPatientLatestBloodSugar(userId);
                if (list == null || list.Count == 0) { msg = "该患者暂无近30天血糖数据"; return null; }
                DataTable dt = new DataTable();
                dt.Columns.Add("MeasureDate", typeof(DateTime));
                dt.Columns.Add("FastingGlucose", typeof(string));
                dt.Columns.Add("PostGlucose", typeof(string));
                dt.Columns.Add("GlucoseTrend", typeof(string));
                dt.Columns.Add("blood_sugar_id", typeof(int));
                foreach (var item in list)
                {
                    DataRow dr = dt.NewRow();
                    dr["MeasureDate"] = item.measurement_time.Value;
                    dr["blood_sugar_id"] = item.blood_sugar_id;
                    if (item.measurement_scenario == "空腹")
                    {
                        dr["FastingGlucose"] = item.blood_sugar_value.ToString("F1");
                        dr["PostGlucose"] = "-";
                        dr["GlucoseTrend"] = item.blood_sugar_value > 7.0m ? "偏高" : item.blood_sugar_value < 3.9m ? "偏低" : "正常";
                    }
                    else if (item.measurement_scenario == "餐后2小时")
                    {
                        dr["FastingGlucose"] = "-";
                        dr["PostGlucose"] = item.blood_sugar_value.ToString("F1");
                        dr["GlucoseTrend"] = item.blood_sugar_value > 10.0m ? "偏高" : item.blood_sugar_value < 3.9m ? "偏低" : "正常";
                    }
                    else
                    {
                        dr["FastingGlucose"] = "-";
                        dr["PostGlucose"] = item.blood_sugar_value.ToString("F1");
                        dr["GlucoseTrend"] = item.blood_sugar_value > 11.1m ? "偏高" : item.blood_sugar_value < 3.9m ? "偏低" : "正常";
                    }
                    dt.Rows.Add(dr);
                }
                return dt;
            }
            catch (Exception ex) { msg = $"加载血糖数据异常：{ex.Message}"; throw new Exception(msg, ex); }
        }

        // 适配新架构：个性化用药建议兼容患者扩展表
        public (string drugName, string drugType, string dosage, string adjustReason, string adjustContent) GenerateMedicationSuggestion(int userId, out string msg)
        {
            msg = "";
            try
            {
                // 适配新拆分架构：获取患者扩展信息
                Patient patient = B_Patient.GetPatientById(userId);
                var sugarList = _dalMedicine.GetPatientLatestBloodSugar(userId);
                if (sugarList == null || sugarList.Count == 0) { msg = "该患者暂无血糖数据，无法生成调整建议"; return ("", "", "", "", ""); }

                int highFastingCount = sugarList.Count(s => s.measurement_scenario == "空腹" && s.blood_sugar_value > 7.0m);
                int highPostCount = sugarList.Count(s => s.measurement_scenario == "餐后2小时" && s.blood_sugar_value > 10.0m);
                int lowCount = sugarList.Count(s => s.blood_sugar_value < 3.9m);
                int totalCount = sugarList.Count;

                string drugName = "", drugType = "", dosage = "", adjustReason = "", adjustContent = "";
                if (highFastingCount >= totalCount * 0.6m)
                {
                    drugType = "双胍类"; drugName = "盐酸二甲双胍片"; dosage = "0.5g/次，每日2次，早晚餐后服用";
                    adjustReason = "患者近30天空腹血糖持续偏高，达标率不足40%，需加强基础降糖治疗";
                    adjustContent = "1. 启用/调整二甲双胍基础治疗，起始剂量0.5g/次，每日2次，根据血糖耐受情况可逐步加量至每日2000mg；\n2. 每周监测3-4次空腹血糖，2周后复诊评估血糖控制情况；\n3. 配合低GI饮食，控制每日碳水化合物摄入量。";
                }
                else if (highPostCount >= totalCount * 0.6m)
                {
                    drugType = "α-糖苷酶抑制剂"; drugName = "阿卡波糖片"; dosage = "50mg/次，每日3次，随第一口饭服用";
                    adjustReason = "患者近30天餐后2小时血糖持续偏高，达标率不足40%，需针对性控制餐后血糖";
                    adjustContent = "1. 加用阿卡波糖控制餐后血糖，50mg/次，每日3次，随餐服用；\n2. 每餐监测餐后2小时血糖，观察药物降糖效果；\n3. 调整饮食结构，减少精制碳水摄入，增加膳食纤维比例。";
                }
                else if (highFastingCount > 0 && lowCount > 0)
                {
                    drugType = "DPP-4抑制剂"; drugName = "西格列汀片"; dosage = "100mg/次，每日1次，晨起服用";
                    adjustReason = "患者血糖波动较大，存在高低血糖交替情况，需选择血糖依赖性降糖药物，降低低血糖风险";
                    adjustContent = "1. 调整为DPP-4抑制剂治疗，西格列汀100mg每日1次，平稳控糖，低血糖风险低；\n2. 每日监测空腹+餐后血糖，记录血糖波动情况；\n3. 规律饮食运动，避免空腹剧烈运动，减少血糖波动诱因。";
                }
                else if ((highFastingCount + highPostCount) >= totalCount * 0.8m)
                {
                    drugType = "胰岛素"; drugName = "门冬胰岛素30注射液"; dosage = "起始0.2-0.3U/kg/日，分早晚2次皮下注射";
                    adjustReason = "患者近30天血糖整体严重不达标，口服药降糖效果有限，需启动胰岛素强化治疗";
                    adjustContent = "1. 启动门冬胰岛素30注射液强化治疗，起始剂量按0.2-0.3U/kg/日计算，分早晚餐前皮下注射；\n2. 每日监测7点血糖（三餐前后+睡前），根据血糖值调整胰岛素剂量；\n3. 严格规律饮食运动，随身携带糖果，预防低血糖发生；\n4. 每周复诊，由医生调整胰岛素剂量。";
                }
                else
                {
                    drugType = "双胍类"; drugName = "盐酸二甲双胍片"; dosage = "0.5g/次，每日2次";
                    adjustReason = "患者血糖整体控制达标，维持当前基础治疗方案，定期监测即可";
                    adjustContent = "1. 维持当前降糖方案，继续规律服药；\n2. 每周监测2-3次空腹+餐后血糖；\n3. 每3个月复查糖化血红蛋白，评估长期血糖控制情况。";
                }
                return (drugName, drugType, dosage, adjustReason, adjustContent);
            }
            catch (Exception ex) { msg = $"生成用药建议异常：{ex.Message}"; throw new Exception(msg, ex); }
        }

        public (bool Success, string Message, int PlanId) SaveMedicationPlan(MedicationPlan model)
        {
            if (model == null) return (false, "用药方案数据不能为空", 0);
            if (model.user_id <= 0) return (false, "患者信息无效，请重新选择", 0);
            if (model.create_by <= 0) return (false, "当前登录医生信息异常，请重新登录", 0);
            if (string.IsNullOrWhiteSpace(model.drug_name)) return (false, "请填写药物名称", 0);
            if (string.IsNullOrWhiteSpace(model.drug_type)) return (false, "请选择药物类型", 0);
            if (model.drug_dosage <= 0) return (false, "用药剂量必须大于0", 0);
            model.data_version = 1; model.status = 1; model.start_time = DateTime.Now.Date;
            try
            {
                int planId = _dalMedicine.AddMedicationPlan(model);
                return planId > 0 ? (true, "用药调整方案保存成功！", planId) : (false, "保存失败，数据库未返回有效方案ID", 0);
            }
            catch (Exception ex) { return (false, "保存用药方案异常：" + ex.Message, 0); }
        }

        public DataTable GetMedicationPlanList()
        {
            try { return _dalMedicine.GetMedicationPlanList(); }
            catch (Exception ex) { throw new Exception("获取用药方案列表失败：" + ex.Message, ex); }
        }

        public MedicationPlan GetMedicationPlanById(int planId, out string msg)
        {
            msg = "";
            try
            {
                if (planId <= 0) { msg = "请选择有效方案"; return null; }
                var model = _dalMedicine.GetMedicationPlanById(planId);
                if (model == null) msg = "方案不存在或已被删除";
                return model;
            }
            catch (Exception ex) { msg = $"获取方案详情异常：{ex.Message}"; throw new Exception(msg, ex); }
        }
        #endregion

        // 新增方法完全保留，无修改
        #region 新增：更新/删除/趋势/导出/批量删除方法（完全保留）
        public ResultModel UpdateMedicineRecord(Medicine medicine)
        {
            try
            {
                if (medicine == null || medicine.medicine_id <= 0) return new ResultModel(false, "编辑的记录ID无效，无法更新");
                if (medicine.user_id <= 0) return new ResultModel(false, "用户信息无效，无法更新");
                if (string.IsNullOrWhiteSpace(medicine.drug_name)) return new ResultModel(false, "药物名称不能为空");
                if (medicine.drug_dosage <= 0) return new ResultModel(false, "用药剂量必须大于0");
                bool updateResult = _dalMedicine.UpdateMedicine(medicine);
                return updateResult ? new ResultModel(true, "用药记录编辑成功") : new ResultModel(false, "编辑失败，无数据被修改");
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"【编辑用药记录异常】{ex.Message}\n{ex.StackTrace}"); return new ResultModel(false, $"系统异常：{ex.Message}"); }
        }

        public ResultModel DeleteMedicineRecord(int medicineId, int userId)
        {
            try
            {
                if (medicineId <= 0) return new ResultModel(false, "记录ID无效，无法删除");
                if (userId <= 0) return new ResultModel(false, "用户信息无效，无法删除");
                bool result = _dalMedicine.DeleteMedicineRecord(medicineId, userId);
                return result ? new ResultModel(true, "用药记录删除成功") : new ResultModel(false, "删除失败，记录不存在或无权限");
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"【删除用药记录异常】{ex.Message}\n{ex.StackTrace}"); return new ResultModel(false, $"系统异常：{ex.Message}"); }
        }

        public List<MedicineTrend> Get30DayMedicineTrend(int userId)
        {
            try { if (userId <= 0) return new List<MedicineTrend>(); return _dalMedicine.Get30DayMedicineTrend(userId); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"【获取用药趋势异常】{ex.Message}\n{ex.StackTrace}"); return new List<MedicineTrend>(); }
        }

        public ResultModel ExportMedicineToExcel(int userId, string filePath)
        {
            try
            {
                if (userId <= 0) return new ResultModel(false, "用户信息无效，无法导出");
                if (string.IsNullOrWhiteSpace(filePath)) return new ResultModel(false, "保存路径不能为空");
                var medicineList = GetUserMedicineList(userId);
                if (medicineList == null || medicineList.Count == 0) return new ResultModel(false, "暂无用药记录可导出");
                EnsureEpplusLicenseConfigured();
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("用药记录");
                    worksheet.Cells[1, 1].Value = "药物名称";
                    worksheet.Cells[1, 2].Value = "用药剂量";
                    worksheet.Cells[1, 3].Value = "用药时间";
                    worksheet.Cells[1, 4].Value = "用药方式";
                    worksheet.Cells[1, 5].Value = "数据来源";
                    worksheet.Cells[1, 6].Value = "关联血糖ID";
                    worksheet.Cells[1, 7].Value = "录入时间";
                    for (int i = 0; i < medicineList.Count; i++)
                    {
                        var item = medicineList[i];
                        worksheet.Cells[i + 2, 1].Value = item.drug_name;
                        worksheet.Cells[i + 2, 2].Value = item.drug_dosage;
                        worksheet.Cells[i + 2, 3].Value = item.take_medicine_time.ToString("yyyy-MM-dd HH:mm");
                        worksheet.Cells[i + 2, 4].Value = item.take_way;
                        worksheet.Cells[i + 2, 5].Value = item.data_source;
                        worksheet.Cells[i + 2, 6].Value = item.related_bs_id ?? 0;
                        worksheet.Cells[i + 2, 7].Value = item.create_time.ToString("yyyy-MM-dd HH:mm");
                    }
                    worksheet.Cells.AutoFitColumns();
                    package.SaveAs(new System.IO.FileInfo(filePath));
                }
                return new ResultModel(true, "导出成功");
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"【导出Excel异常】{ex.Message}\n{ex.StackTrace}"); return new ResultModel(false, $"导出失败：{ex.Message}"); }
        }

        public ResultModel BatchDeleteMedicineRecords(List<int> medicineIds, int userId)
        {
            try
            {
                if (medicineIds == null || medicineIds.Count == 0) return new ResultModel(false, "没有选择要删除的记录");
                if (userId <= 0) return new ResultModel(false, "用户信息无效，无法删除");
                int successCount = _dalMedicine.BatchDeleteMedicineRecords(medicineIds, userId);
                return new ResultModel(true, "批量删除完成", successCount);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"【批量删除用药记录异常】{ex.Message}\n{ex.StackTrace}"); return new ResultModel(false, $"系统异常：{ex.Message}"); }
        }
        #endregion
    }
}