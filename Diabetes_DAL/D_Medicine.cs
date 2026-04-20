using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Model;
using Tools;

namespace DAL
{
    /// <summary>
    /// 用药记录管理数据访问层
    /// </summary>
    public class D_Medicine
    {
        private string NormalizeMedicineDataSource(string dataSource)
        {
            // t_medicine 的数据库约束仅允许这两个值
            if (string.Equals(dataSource, "Excel批量导入", StringComparison.Ordinal))
            {
                return "Excel批量导入";
            }
            return "手动录入";
        }

        #region 原有核心方法（完全保留，未做任何修改）
        #region 1. 获取降糖药物字典列表
        /// <summary>
        /// 获取所有降糖药物字典数据
        /// </summary>
        public List<AntidiabeticDrug> GetDrugDictionaryList()
        {
            string sql = "SELECT * FROM Diabetes_Antidiabetic_Drugs WHERE 1=1 ORDER BY DrugGenericName ASC";
            return SqlHelper.GetModelList<AntidiabeticDrug>(sql);
        }
        #endregion

        #region 2. 新增用药记录
        /// <summary>
        /// 新增单条用药记录，返回新增的主键ID
        /// </summary>
        // 原代码（已处理，但强化冗余校验）
        public int AddMedicineRecord(Medicine model)
        {
            string sql = @"
INSERT INTO t_medicine (
    user_id, drug_code, drug_name, drug_dosage, take_medicine_time, take_way, 
    prescribe_doctor_id, related_bs_id, data_source, operator_id, abnormal_note, 
    data_status, data_version, create_time, update_time
) VALUES (
    @user_id, @drug_code, @drug_name, @drug_dosage, @take_medicine_time, @take_way, 
    @prescribe_doctor_id, @related_bs_id, @data_source, @operator_id, @abnormal_note, 
    @data_status, @data_version, GETDATE(), GETDATE()
);
SELECT SCOPE_IDENTITY();";
            SqlParameter[] param = {
        new SqlParameter("@user_id", model.user_id),
        // 强化非空处理：即使model.drug_code为null，也默认赋值为"DEFAULT"，避免违反非空约束
        new SqlParameter("@drug_code", string.IsNullOrEmpty(model.drug_code) ? "DEFAULT" : model.drug_code),
        new SqlParameter("@drug_name", model.drug_name),
        new SqlParameter("@drug_dosage", model.drug_dosage),
        new SqlParameter("@take_medicine_time", model.take_medicine_time),
        new SqlParameter("@take_way", model.take_way),
        new SqlParameter("@prescribe_doctor_id", model.prescribe_doctor_id ?? (object)DBNull.Value),
        new SqlParameter("@related_bs_id", model.related_bs_id ?? (object)DBNull.Value),
        new SqlParameter("@data_source", NormalizeMedicineDataSource(model.data_source)),
        new SqlParameter("@operator_id", model.operator_id),
        new SqlParameter("@abnormal_note", model.abnormal_note ?? (object)DBNull.Value),
        new SqlParameter("@data_status", model.data_status),
        new SqlParameter("@data_version", model.data_version)
    };
            object result = SqlHelper.GetSingle(sql, param);
            return result != null ? Convert.ToInt32(result) : 0;
        }
        #endregion

        #region 3. 批量新增用药记录
        /// <summary>
        /// 批量新增用药记录，返回成功条数
        /// </summary>
        public int BatchAddMedicineRecord(List<Medicine> list)
        {
            int successCount = 0;
            using (SqlConnection conn = new SqlConnection(Tools.SqlHelper.connStr))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        string sql = @"
INSERT INTO t_medicine (
    user_id, drug_code, drug_name, drug_dosage, take_medicine_time, take_way, 
    related_bs_id, data_source, operator_id, data_status, data_version, create_time, update_time
) VALUES (
    @user_id, @drug_code, @drug_name, @drug_dosage, @take_medicine_time, @take_way, 
    @related_bs_id, @data_source, @operator_id, @data_status, @data_version, GETDATE(), GETDATE()
);";
                        using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
                        {
                            cmd.Parameters.Add("@user_id", SqlDbType.Int);
                            cmd.Parameters.Add("@drug_code", SqlDbType.NVarChar, 30);
                            cmd.Parameters.Add("@drug_name", SqlDbType.NVarChar, 100);
                            cmd.Parameters.Add("@drug_dosage", SqlDbType.Decimal);
                            cmd.Parameters.Add("@take_medicine_time", SqlDbType.DateTime);
                            cmd.Parameters.Add("@take_way", SqlDbType.NVarChar, 20);
                            cmd.Parameters.Add("@related_bs_id", SqlDbType.Int);
                            cmd.Parameters.Add("@data_source", SqlDbType.NVarChar, 30);
                            cmd.Parameters.Add("@operator_id", SqlDbType.Int);
                            cmd.Parameters.Add("@data_status", SqlDbType.Int);
                            cmd.Parameters.Add("@data_version", SqlDbType.Int);
                            foreach (var item in list)
                            {
                                cmd.Parameters["@user_id"].Value = item.user_id;
                                cmd.Parameters["@drug_code"].Value = string.IsNullOrWhiteSpace(item.drug_code) ? "DEFAULT" : item.drug_code.Trim();
                                cmd.Parameters["@drug_name"].Value = item.drug_name;
                                cmd.Parameters["@drug_dosage"].Value = item.drug_dosage;
                                cmd.Parameters["@take_medicine_time"].Value = item.take_medicine_time;
                                cmd.Parameters["@take_way"].Value = item.take_way;
                                cmd.Parameters["@related_bs_id"].Value = item.related_bs_id ?? (object)DBNull.Value;
                                cmd.Parameters["@data_source"].Value = NormalizeMedicineDataSource(item.data_source);
                                cmd.Parameters["@operator_id"].Value = item.operator_id;
                                cmd.Parameters["@data_status"].Value = item.data_status > 0 ? item.data_status : 1;
                                cmd.Parameters["@data_version"].Value = item.data_version > 0 ? item.data_version : 1;
                                successCount += cmd.ExecuteNonQuery();
                            }
                        }
                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        successCount = 0;
                        throw;
                    }
                }
            }
            return successCount;
        }
        #endregion

        #region 4. 获取用户用药记录列表
        /// <summary>
        /// 获取指定用户的所有用药记录
        /// </summary>
        public List<Medicine> GetUserMedicineList(int userId)
        {
            string sql = @"
SELECT * FROM t_medicine 
WHERE user_id = @UserId AND data_status != 2 
ORDER BY take_medicine_time DESC";
            SqlParameter[] param = { new SqlParameter("@UserId", userId) };
            return SqlHelper.GetModelList<Medicine>(sql, param);
        }
        #endregion

        #region 5. 批量更新用药记录（数据校准用）
        /// <summary>
        /// 批量更新用药记录数据，返回更新成功条数
        /// </summary>
        public int BatchUpdateMedicineRecord(List<Medicine> list)
        {
            int updateCount = 0;
            using (SqlConnection conn = new SqlConnection(Tools.SqlHelper.connStr))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        string sql = @"
UPDATE t_medicine SET 
    drug_dosage = @drug_dosage,
    take_medicine_time = @take_medicine_time,
    related_bs_id = @related_bs_id,
    data_source = @data_source,
    data_version = data_version + 1,
    update_time = GETDATE()
WHERE medicine_id = @medicine_id";
                        using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
                        {
                            cmd.Parameters.Add("@drug_dosage", SqlDbType.Decimal);
                            cmd.Parameters.Add("@take_medicine_time", SqlDbType.DateTime);
                            cmd.Parameters.Add("@related_bs_id", SqlDbType.Int);
                            cmd.Parameters.Add("@data_source", SqlDbType.NVarChar, 30);
                            cmd.Parameters.Add("@medicine_id", SqlDbType.Int);
                            foreach (var item in list)
                            {
                                cmd.Parameters["@drug_dosage"].Value = item.drug_dosage;
                                cmd.Parameters["@take_medicine_time"].Value = item.take_medicine_time;
                                cmd.Parameters["@related_bs_id"].Value = item.related_bs_id ?? (object)DBNull.Value;
                                cmd.Parameters["@data_source"].Value = NormalizeMedicineDataSource(item.data_source);
                                cmd.Parameters["@medicine_id"].Value = item.medicine_id;
                                updateCount += cmd.ExecuteNonQuery();
                            }
                        }
                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        updateCount = 0;
                        throw;
                    }
                }
            }
            return updateCount;
        }
        #endregion
        #endregion

        #region 新增：医生端专用方法（追加，不影响原有逻辑）
        #region 新增：获取患者下拉列表（仅启用的普通患者）
        /// <summary>
        /// 获取患者下拉列表数据
        /// </summary>
        public DataTable GetPatientList()
        {
            string sql = "SELECT user_id, user_name FROM t_user WHERE user_type = 1 AND status = 1 ORDER BY user_name ASC";
            return SqlHelper.ExecuteDataTable(sql);
        }
        #endregion

        #region 新增：按时间范围获取患者用药记录
        /// <summary>
        /// 按时间范围获取患者用药记录
        /// </summary>
        public List<Medicine> GetUserMedicineRecordByTime(int userId, DateTime startTime, DateTime endTime)
        {
            string sql = @"
            SELECT * FROM t_medicine 
            WHERE user_id = @UserId 
            AND take_medicine_time BETWEEN @StartTime AND @EndTime
            AND data_status != 2 
            ORDER BY take_medicine_time DESC";
            SqlParameter[] param = {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@StartTime", startTime),
                new SqlParameter("@EndTime", endTime.AddDays(1).AddSeconds(-1))
            };
            return SqlHelper.GetModelList<Medicine>(sql, param);
        }
        #endregion

        #region 新增：获取患者近期血糖数据（近30天）
        /// <summary>
        /// 获取患者近30天血糖数据
        /// </summary>
        public List<BloodSugar> GetPatientLatestBloodSugar(int userId)
        {
            string sql = @"
            SELECT * FROM t_blood_sugar 
            WHERE user_id = @UserId 
            AND measurement_time >= DATEADD(DAY,-30,GETDATE())
            AND data_status != 2 
            ORDER BY measurement_time DESC";
            SqlParameter[] param = { new SqlParameter("@UserId", userId) };
            return SqlHelper.GetModelList<BloodSugar>(sql, param);
        }
        #endregion

        #region 新增：新增用药调整方案
        /// <summary>
        /// 新增用药方案，返回主键ID
        /// </summary>
        public int AddMedicationPlan(MedicationPlan model)
        {
            string sql = @"
            INSERT INTO t_medication_plan (
                user_id, drug_code, drug_name, drug_type, drug_dosage, 
                adjust_reason, adjust_content, start_time, create_by, 
                status, data_version, create_time, update_time
            ) VALUES (
                @user_id, @drug_code, @drug_name, @drug_type, @drug_dosage, 
                @adjust_reason, @adjust_content, @start_time, @create_by, 
                @status, @data_version, GETDATE(), GETDATE()
            );
            SELECT SCOPE_IDENTITY();";
            SqlParameter[] param = {
                new SqlParameter("@user_id", model.user_id),
                new SqlParameter("@drug_code", model.drug_code ?? "DEFAULT"),
                new SqlParameter("@drug_name", model.drug_name),
                new SqlParameter("@drug_type", model.drug_type),
                new SqlParameter("@drug_dosage", model.drug_dosage),
                new SqlParameter("@adjust_reason", model.adjust_reason ?? (object)DBNull.Value),
                new SqlParameter("@adjust_content", model.adjust_content ?? (object)DBNull.Value),
                new SqlParameter("@start_time", model.start_time),
                new SqlParameter("@create_by", model.create_by),
                new SqlParameter("@status", model.status),
                new SqlParameter("@data_version", model.data_version)
            };
            object result = SqlHelper.ExecuteScalar(sql, param);
            return result != null ? Convert.ToInt32(result) : 0;
        }
        #endregion

        #region 新增：获取用药方案列表
        /// <summary>
        /// 获取所有用药方案列表（含患者姓名）
        /// </summary>
        public DataTable GetMedicationPlanList()
        {
            string sql = @"
            SELECT 
                p.plan_id, u.user_name as PatientName, p.drug_name as MedicationName, 
                p.drug_dosage as Dosage, p.create_time as CreateTime,
                CASE p.status 
                    WHEN 0 THEN '待执行' 
                    WHEN 1 THEN '执行中' 
                    WHEN 2 THEN '已结束' 
                    WHEN 3 THEN '已停用' 
                END as PlanStatus
            FROM t_medication_plan p
            LEFT JOIN t_user u ON p.user_id = u.user_id
            ORDER BY p.create_time DESC";
            return SqlHelper.ExecuteDataTable(sql);
        }
        #endregion

        #region 新增：根据ID获取用药方案详情
        /// <summary>
        /// 根据ID获取用药方案详情
        /// </summary>
        public MedicationPlan GetMedicationPlanById(int planId)
        {
            string sql = "SELECT * FROM t_medication_plan WHERE plan_id = @plan_id";
            SqlParameter[] param = { new SqlParameter("@plan_id", planId) };
            return SqlHelper.GetModel<MedicationPlan>(sql, param);
        }
        #endregion

        #region 新增：更新用药方案状态
        /// <summary>
        /// 更新用药方案状态
        /// </summary>
        public bool UpdateMedicationPlanStatus(int planId, int status, int operateUserId)
        {
            string sql = @"
            UPDATE t_medication_plan SET 
                status = @status, 
                update_time = GETDATE(),
                data_version = data_version + 1
            WHERE plan_id = @plan_id";
            SqlParameter[] param = {
                new SqlParameter("@plan_id", planId),
                new SqlParameter("@status", status)
            };
            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }
        #endregion
        #endregion

        #region 新增：更新用药记录
        /// <summary>
        /// 更新用药记录到数据库
        /// </summary>
        /// <param name="medicine">用药实体</param>
        /// <returns>是否更新成功</returns>
        public bool UpdateMedicine(Medicine medicine)
        {
            string sql = @"
    UPDATE t_medicine 
    SET 
        drug_code = @drug_code,
        drug_name = @drug_name,
        drug_dosage = @drug_dosage,
        take_medicine_time = @take_medicine_time,
        take_way = @take_way,
        related_bs_id = @related_bs_id,
        data_source = @data_source,
        operator_id = @operator_id,
        update_time = GETDATE()
    WHERE medicine_id = @medicine_id AND user_id = @user_id";

            SqlParameter[] param = {
        new SqlParameter("@medicine_id", medicine.medicine_id),
        new SqlParameter("@user_id", medicine.user_id),
        new SqlParameter("@drug_code", medicine.drug_code ?? (object)DBNull.Value),
        new SqlParameter("@drug_name", medicine.drug_name),
        new SqlParameter("@drug_dosage", medicine.drug_dosage),
        new SqlParameter("@take_medicine_time", medicine.take_medicine_time),
        new SqlParameter("@take_way", medicine.take_way),
        new SqlParameter("@related_bs_id", medicine.related_bs_id ?? (object)DBNull.Value),
        new SqlParameter("@data_source", NormalizeMedicineDataSource(medicine.data_source)),
        new SqlParameter("@operator_id", medicine.operator_id)
    };

            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }
        #endregion
        #region 新增：删除用药记录
        /// <summary>
        /// 逻辑删除用药记录（修改data_status=2）
        /// </summary>
        public bool DeleteMedicineRecord(int medicineId, int userId)
        {
            string sql = @"
    UPDATE t_medicine 
    SET 
        data_status = 2,
        update_time = GETDATE(),
        data_version = data_version + 1
    WHERE medicine_id = @medicine_id AND user_id = @user_id";

            SqlParameter[] param = {
        new SqlParameter("@medicine_id", medicineId),
        new SqlParameter("@user_id", userId)
    };

            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }
        #endregion

        #region 新增：获取30天用药趋势数据
        /// <summary>
        /// 获取用户近30天用药趋势统计
        /// </summary>
        public List<MedicineTrend> Get30DayMedicineTrend(int userId)
        {
            string sql = @"
    SELECT 
        CAST(take_medicine_time AS DATE) AS Date,
        SUM(drug_dosage) AS TotalDosage,
        COUNT(*) AS MedicineCount
    FROM t_medicine 
    WHERE 
        user_id = @UserId 
        AND take_medicine_time >= DATEADD(DAY, -30, GETDATE())
        AND data_status != 2
    GROUP BY CAST(take_medicine_time AS DATE)
    ORDER BY Date ASC";

            SqlParameter[] param = { new SqlParameter("@UserId", userId) };
            return SqlHelper.GetModelList<MedicineTrend>(sql, param);
        }
        #endregion

        #region 新增：批量删除用药记录
        /// <summary>
        /// 批量逻辑删除用药记录
        /// </summary>
        public int BatchDeleteMedicineRecords(List<int> medicineIds, int userId)
        {
            if (medicineIds == null || medicineIds.Count == 0)
                return 0;

            // 构建IN条件
            string ids = string.Join(",", medicineIds);
            string sql = $@"
    UPDATE t_medicine 
    SET 
        data_status = 2,
        update_time = GETDATE(),
        data_version = data_version + 1
    WHERE medicine_id IN ({ids}) AND user_id = @user_id";

            SqlParameter param = new SqlParameter("@user_id", userId);
            return SqlHelper.ExecuteNonQuery(sql, param);
        }
        #endregion
    }
}