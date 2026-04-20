using Model;
using System;
using System.Data;
using System.Data.SqlClient;
using Tools;

namespace DAL
{
    /// <summary>
    /// 患者扩展表数据访问层（仅操作 t_patient）
    /// </summary>
    public class D_Patient
    {
        /// <summary>
        /// 新增患者信息
        /// </summary>
        public bool AddPatient(Patient patient)
        {
            string sql = @"
INSERT INTO t_patient (
    patient_id, diabetes_type, diagnose_date, fasting_glucose_baseline, 
    create_time, update_time
)
VALUES (
    @patient_id, @diabetes_type, @diagnose_date, @fasting_glucose_baseline,
    GETDATE(), GETDATE()
)";
            // ✅ 修复CS0019：DBNull.Value 不能直接用??，改用三元运算符
            SqlParameter[] param = {
                new SqlParameter("@patient_id", patient.patient_id),
                new SqlParameter("@diabetes_type", (object)patient.diabetes_type ?? DBNull.Value),
                new SqlParameter("@diagnose_date", patient.diagnose_date.HasValue ? (object)patient.diagnose_date.Value : DBNull.Value),
                new SqlParameter("@fasting_glucose_baseline", patient.fasting_glucose_baseline.HasValue ? (object)patient.fasting_glucose_baseline.Value : DBNull.Value)
            };
            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }

        /// <summary>
        /// 修改患者信息
        /// </summary>
        public bool UpdatePatient(Patient patient)
        {
            string sql = @"
UPDATE t_patient SET 
    diabetes_type = @diabetes_type,
    diagnose_date = @diagnose_date,
    fasting_glucose_baseline = @fasting_glucose_baseline,
    update_time = GETDATE()
WHERE patient_id = @patient_id";
            // ✅ 修复CS0019：同上
            SqlParameter[] param = {
                new SqlParameter("@patient_id", patient.patient_id),
                new SqlParameter("@diabetes_type", (object)patient.diabetes_type ?? DBNull.Value),
                new SqlParameter("@diagnose_date", patient.diagnose_date.HasValue ? (object)patient.diagnose_date.Value : DBNull.Value),
                new SqlParameter("@fasting_glucose_baseline", patient.fasting_glucose_baseline.HasValue ? (object)patient.fasting_glucose_baseline.Value : DBNull.Value)
            };
            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }

        /// <summary>
        /// 根据用户ID获取患者信息
        /// </summary>
        public Patient GetPatientById(int patientId)
        {
            string sql = @"
SELECT * FROM t_patient 
WHERE patient_id = @patient_id";
            SqlParameter[] param = { new SqlParameter("@patient_id", patientId) };
            var dt = SqlHelper.ExecuteDataTable(sql, param);
            if (dt.Rows.Count == 0) return null;

            DataRow dr = dt.Rows[0];
            return new Patient
            {
                patient_id = Convert.ToInt32(dr["patient_id"]),
                diabetes_type = dr["diabetes_type"]?.ToString(),
                // ✅ 修复CS0266：显式转换为可空类型
                diagnose_date = dr["diagnose_date"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(dr["diagnose_date"]) : null,
                fasting_glucose_baseline = dr["fasting_glucose_baseline"] != DBNull.Value ? (decimal?)Convert.ToDecimal(dr["fasting_glucose_baseline"]) : null
            };
        }
    }
}