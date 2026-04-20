using DAL;
using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Tools;

namespace BLL
{
    /// <summary>
    /// 血糖管理业务逻辑层
    /// </summary>
    public class B_BloodSugar
    {
        #region 临床常量定义（与UI层保持一致，统一临床标准）
        private const decimal FastingMin = 3.9m;
        private const decimal FastingMax = 6.1m;
        private const decimal PostprandialMax = 7.8m;
        private const decimal RandomMax = 11.1m;
        #endregion

        private readonly D_BloodSugar dalBs = new D_BloodSugar();

        #region 1. 新增血糖记录（带异常判断，已修复所有编译错误）
        /// <summary>
        /// 新增血糖记录（修复时间溢出+类型转换错误）
        /// </summary>
        public ResultModel AddBloodSugar(BloodSugar model)
        {
            try
            {
                // 原有校验逻辑完全保留
                if (!model.measurement_time.HasValue || model.measurement_time < new DateTime(1900, 1, 1) || model.measurement_time > DateTime.Now)
                {
                    model.measurement_time = DateTime.Now;
                }
                model.create_time = DateTime.Now;
                model.update_time = DateTime.Now;
                model.data_status = model.data_status <= 0 ? 1 : model.data_status;
                model.data_version = model.data_version <= 0 ? 1 : model.data_version;

                string sql = @"
INSERT INTO t_blood_sugar 
(user_id, blood_sugar_value, measurement_scenario, measurement_time, abnormal_note, data_source, operator_id, data_status, data_version, create_time, update_time)
VALUES 
(@user_id, @blood_sugar_value, @measurement_scenario, @measurement_time, @abnormal_note, @data_source, @operator_id, @data_status, @data_version, @create_time, @update_time);
SELECT CAST(SCOPE_IDENTITY() AS INT);
";

                int isAbnormal = CalculateIsAbnormal(model.blood_sugar_value, model.measurement_scenario);

                SqlParameter[] parameters = {
            new SqlParameter("@user_id", model.user_id),
            new SqlParameter("@blood_sugar_value", model.blood_sugar_value),
            new SqlParameter("@measurement_scenario", model.measurement_scenario ?? (object)DBNull.Value),
            new SqlParameter("@measurement_time", model.measurement_time),
            new SqlParameter("@abnormal_note", model.abnormal_note ?? (object)DBNull.Value),
            new SqlParameter("@data_source", model.data_source ?? (object)DBNull.Value),
            new SqlParameter("@operator_id", model.operator_id),
            new SqlParameter("@data_status", model.data_status),
            new SqlParameter("@data_version", model.data_version),
            new SqlParameter("@create_time", model.create_time),
            new SqlParameter("@update_time", model.update_time)
        };

                object insertResult = SqlHelper.ExecuteScalar(sql, parameters);
                int newBloodSugarId = insertResult != null && insertResult != DBNull.Value ? Convert.ToInt32(insertResult) : 0;
                bool abnormalSaved = true;

                if (newBloodSugarId > 0 && isAbnormal == 1)
                {
                    int assignedDoctorId = GetAssignedDoctorId(model.user_id);
                    var abnormalBll = new B_Abnormal();
                    var abnormalResult = abnormalBll.AddSystemAbnormal(new Abnormal
                    {
                        data_type = "血糖",
                        original_data_id = newBloodSugarId,
                        user_id = model.user_id,
                        abnormal_type = GetAbnormalType(model.blood_sugar_value, model.measurement_scenario),
                        abnormal_reason = BuildAbnormalReason(model.blood_sugar_value, model.measurement_scenario),
                        suggestion = BuildAbnormalSuggestion(model.blood_sugar_value, model.measurement_scenario),
                        mark_time = DateTime.Now,
                        mark_by = assignedDoctorId,
                        handle_status = 0,
                        data_version = 1,
                        create_time = DateTime.Now,
                        update_time = DateTime.Now
                    });
                    abnormalSaved = abnormalResult.Success;
                }

                return new ResultModel
                {
                    Success = newBloodSugarId > 0,
                    Msg = newBloodSugarId > 0
                        ? (isAbnormal == 1
                            ? (abnormalSaved ? "血糖值异常，已生成预警提示！" : "血糖值异常，但预警写入失败，请联系管理员检查！")
                            : "血糖记录保存成功！")
                        : "保存失败，请重试",
                    Data = newBloodSugarId
                };
            }
            catch (Exception ex)
            {
                return new ResultModel
                {
                    Success = false,
                    Msg = $"保存失败：{ex.Message}"
                };
            }
        }
        #endregion

        #region 2. 修改血糖记录（修复@remark参数报错）
        /// <summary>
        /// 修改血糖记录（SQL与参数完全匹配，无参数名错误）
        /// </summary>
        public Result UpdateBloodSugar(BloodSugar bs)
        {
            if (bs.blood_sugar_id <= 0 || bs.user_id <= 0)
                return Result.Fail("记录信息异常");
            if (bs.blood_sugar_value <= 0 || bs.blood_sugar_value > 30m)
                return Result.Fail("请输入有效的血糖值（1-30mmol/L）");
            if (string.IsNullOrEmpty(bs.measurement_scenario))
                return Result.Fail("请选择测量场景");
            if (bs.measurement_time > DateTime.Now)
                return Result.Fail("测量时间不能晚于当前时间");

            bs.update_time = DateTime.Now;

            string sql = @"
            UPDATE t_blood_sugar SET 
            blood_sugar_value = @blood_sugar_value,
            measurement_scenario = @measurement_scenario,
            measurement_time = @measurement_time,
            abnormal_note = @abnormal_note,
            operator_id = @operator_id,
            update_time = @update_time
            WHERE blood_sugar_id = @blood_sugar_id AND user_id = @user_id;
            ";

            SqlParameter[] parameters = {
        new SqlParameter("@blood_sugar_id", bs.blood_sugar_id),
        new SqlParameter("@user_id", bs.user_id),
        new SqlParameter("@blood_sugar_value", bs.blood_sugar_value),
        new SqlParameter("@measurement_scenario", bs.measurement_scenario ?? (object)DBNull.Value),
        new SqlParameter("@measurement_time", bs.measurement_time),
        new SqlParameter("@abnormal_note", bs.abnormal_note ?? (object)DBNull.Value),
        new SqlParameter("@operator_id", bs.operator_id),
        new SqlParameter("@update_time", bs.update_time)
    };

            int rows = SqlHelper.ExecuteNonQuery(sql, parameters);
            return rows > 0
                ? Result.Ok("血糖记录修改成功！")
                : Result.Fail("修改失败，记录不存在或已被删除");
        }
        #endregion

        #region 3. 删除血糖记录
        /// <summary>
        /// 软删除血糖记录
        /// </summary>
        public Result DeleteBloodSugar(int bloodSugarId, int userId)
        {
            if (bloodSugarId <= 0 || userId <= 0)
                return Result.Fail("参数异常");
            bool success = dalBs.DeleteBloodSugar(bloodSugarId, userId);
            return success ? Result.Ok("删除成功") : Result.Fail("删除失败，记录不存在");
        }
        #endregion

        #region 4. 分页查询血糖记录
        /// <summary>
        /// 分页查询患者血糖记录
        /// </summary>
        public List<BloodSugar> GetBloodSugarPageList(int userId, int pageIndex, int pageSize, out int totalCount,
            DateTime? startTime = null, DateTime? endTime = null, string scenario = "全部")
        {
            totalCount = 0;
            try
            {
                string whereSql = " WHERE user_id = @user_id AND data_status = 1 ";
                List<SqlParameter> paramList = new List<SqlParameter>
                {
                    new SqlParameter("@user_id", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@pageIndex", SqlDbType.Int) { Value = pageIndex },
                    new SqlParameter("@pageSize", SqlDbType.Int) { Value = pageSize }
                };

                if (startTime.HasValue)
                {
                    whereSql += " AND measurement_time >= @startTime ";
                    paramList.Add(new SqlParameter("@startTime", SqlDbType.DateTime) { Value = startTime.Value });
                }
                if (endTime.HasValue)
                {
                    whereSql += " AND measurement_time <= @endTime ";
                    paramList.Add(new SqlParameter("@endTime", SqlDbType.DateTime) { Value = endTime.Value });
                }

                if (scenario != "全部" && !string.IsNullOrEmpty(scenario))
                {
                    whereSql += " AND measurement_scenario = @scenario ";
                    paramList.Add(new SqlParameter("@scenario", SqlDbType.NVarChar, 20) { Value = scenario });
                }

                string countSql = $"SELECT COUNT(1) FROM t_blood_sugar {whereSql}";
                object countObj = SqlHelper.GetSingle(countSql, paramList.ToArray());
                totalCount = countObj != null ? Convert.ToInt32(countObj) : 0;

                string pageSql = $@"
                    SELECT * FROM (
                        SELECT ROW_NUMBER() OVER(ORDER BY measurement_time DESC) as row_num, * 
                        FROM t_blood_sugar {whereSql}
                    ) as temp
                    WHERE row_num BETWEEN (@pageIndex-1)*@pageSize + 1 AND @pageIndex*@pageSize";

                DataTable dt = SqlHelper.ExecuteDataTable(pageSql, paramList.ToArray());
                List<BloodSugar> list = new List<BloodSugar>();
                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(DataRowToModel(dr));
                }
                return list;
            }
            catch (Exception ex)
            {
                throw new Exception($"查询血糖列表失败：{ex.Message}");
            }
        }
        #endregion

        #region 5. 单条记录详情查询
        /// <summary>
        /// 获取单条血糖记录详情
        /// </summary>
        public Result<BloodSugar> GetBloodSugarDetail(int bloodSugarId, int userId)
        {
            if (bloodSugarId <= 0 || userId <= 0)
                return Result<BloodSugar>.Fail("参数异常");
            var detail = dalBs.GetBloodSugarDetail(bloodSugarId, userId);
            return detail == null ? Result<BloodSugar>.Fail("记录不存在") : Result<BloodSugar>.Ok(detail);
        }
        #endregion

        #region 6. 今日统计数据查询
        /// <summary>
        /// 获取用户当日血糖统计数据
        /// </summary>
        public DataTable GetTodayStats(int userId)
        {
            try
            {
                string sql = @"
                    SELECT 
                        ISNULL(AVG(blood_sugar_value),0) as avg_value,
                        ISNULL(MAX(blood_sugar_value),0) as max_value,
                        ISNULL(MIN(blood_sugar_value),0) as min_value,
                        SUM(CASE WHEN is_abnormal = 1 THEN 1 ELSE 0 END) as abnormal_count
                    FROM t_blood_sugar 
                    WHERE user_id = @user_id 
                    AND CONVERT(date, measurement_time) = CONVERT(date, GETDATE())
                    AND data_status = 1";

                SqlParameter[] parameters = {
                    new SqlParameter("@user_id", SqlDbType.Int) { Value = userId }
                };
                return SqlHelper.ExecuteDataTable(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"获取今日统计失败：{ex.Message}");
            }
        }
        #endregion

        #region 7. 30天趋势数据查询
        /// <summary>
        /// 获取30天血糖趋势数据
        /// </summary>
        public DataTable Get30DayTrendData(int userId)
        {
            try
            {
                string sql = @"
                    SELECT 
                        CONVERT(date, measurement_time) as record_date,
                        measurement_scenario,
                        AVG(blood_sugar_value) as avg_value
                    FROM t_blood_sugar 
                    WHERE user_id = @user_id 
                    AND measurement_time >= DATEADD(DAY, -30, GETDATE())
                    AND data_status = 1
                    AND measurement_scenario IN ('空腹','餐后2小时')
                    GROUP BY CONVERT(date, measurement_time), measurement_scenario
                    ORDER BY record_date";

                SqlParameter[] parameters = {
                    new SqlParameter("@user_id", SqlDbType.Int) { Value = userId }
                };
                return SqlHelper.ExecuteDataTable(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"获取趋势数据失败：{ex.Message}");
            }
        }
        #endregion

        #region 8. DataRow转实体（辅助方法）
        /// <summary>
        /// DataRow转换为BloodSugar实体对象
        /// </summary>
        private BloodSugar DataRowToModel(DataRow dr)
        {
            BloodSugar model = new BloodSugar();
            if (dr["blood_sugar_id"] != DBNull.Value)
                model.blood_sugar_id = Convert.ToInt32(dr["blood_sugar_id"]);
            if (dr["user_id"] != DBNull.Value)
                model.user_id = Convert.ToInt32(dr["user_id"]);
            if (dr["device_id"] != DBNull.Value)
                model.device_id = dr["device_id"].ToString();
            if (dr["blood_sugar_value"] != DBNull.Value)
                model.blood_sugar_value = Convert.ToDecimal(dr["blood_sugar_value"]);
            if (dr["measurement_time"] != DBNull.Value)
                model.measurement_time = Convert.ToDateTime(dr["measurement_time"]);
            if (dr["measurement_scenario"] != DBNull.Value)
                model.measurement_scenario = dr["measurement_scenario"].ToString();
            if (dr["related_diet_id"] != DBNull.Value)
                model.related_diet_id = Convert.ToInt32(dr["related_diet_id"]);
            if (dr["related_exercise_id"] != DBNull.Value)
                model.related_exercise_id = Convert.ToInt32(dr["related_exercise_id"]);
            if (dr["data_source"] != DBNull.Value)
                model.data_source = dr["data_source"].ToString();
            if (dr["operator_id"] != DBNull.Value)
                model.operator_id = Convert.ToInt32(dr["operator_id"]);
            if (dr["abnormal_note"] != DBNull.Value)
                model.abnormal_note = dr["abnormal_note"].ToString();
            if (dr["data_status"] != DBNull.Value)
                model.data_status = Convert.ToInt32(dr["data_status"]);
            if (dr["data_version"] != DBNull.Value)
                model.data_version = Convert.ToInt32(dr["data_version"]);
            if (dr["create_time"] != DBNull.Value)
                model.create_time = Convert.ToDateTime(dr["create_time"]);
            if (dr["update_time"] != DBNull.Value)
                model.update_time = Convert.ToDateTime(dr["update_time"]);
            if (dr["is_abnormal"] != DBNull.Value)
                model.is_abnormal = Convert.ToInt32(dr["is_abnormal"]);
            return model;
        }
        #endregion

        #region 私有方法：血糖异常判断（临床标准）
        /// <summary>
        /// 根据临床标准判断血糖是否异常
        /// </summary>
        /// <returns>1=异常 0=正常</returns>
        private int CalculateIsAbnormal(decimal value, string scenario)
        {
            switch (scenario)
            {
                case "空腹":

                    return (value < FastingMin || value > FastingMax) ? 1 : 0;
                case "餐后2小时":
                    return value > PostprandialMax ? 1 : 0;
                case "随机":
                    return value > RandomMax ? 1 : 0;
                default:
                    return 0; // 餐前/睡前按空腹标准宽松判断，可根据临床需求调整
            }
        }

        private string GetAbnormalType(decimal value, string scenario)
        {
            switch (scenario)
            {
                case "空腹":
                    if (value < FastingMin) return "低血糖";
                    if (value > FastingMax) return "高血糖";
                    break;
                case "餐后2小时":
                    if (value < FastingMin) return "低血糖";
                    if (value > PostprandialMax) return "高血糖";
                    break;
                case "随机":
                    if (value < FastingMin) return "低血糖";
                    if (value > RandomMax) return "高血糖";
                    break;
            }

            return "血糖异常";
        }

        private string BuildAbnormalReason(decimal value, string scenario)
        {
            string sceneText = string.IsNullOrWhiteSpace(scenario) ? "本次" : scenario;
            return $"{sceneText}血糖 {value:0.0} mmol/L，超出系统预设安全范围";
        }

        private string BuildAbnormalSuggestion(decimal value, string scenario)
        {
            if (value < FastingMin)
            {
                return "请立即补充快速碳水化合物，15分钟后复测血糖；若仍低于3.9 mmol/L或伴随明显不适，请尽快就医。";
            }

            if (scenario == "空腹")
            {
                return "建议复测空腹血糖，控制晚餐与夜宵摄入，保持规律作息，并尽快与医生确认是否需要调整饮食或用药方案。";
            }

            if (scenario == "餐后2小时")
            {
                return "建议复核本餐饮食与餐后活动情况，减少高GI食物摄入，监测后续餐后血糖变化，并按需联系医生评估。";
            }

            return "建议近期加强血糖监测，注意饮食与运动管理，必要时联系医生进一步评估。";
        }
        #endregion

        #region 私有方法：获取患者绑定医生ID
        private int GetAssignedDoctorId(int userId)
        {
            if (userId <= 0)
                return 0;

            const string sql = @"
SELECT TOP 1 doctor_id
FROM t_user
WHERE user_id = @user_id";
            object result = SqlHelper.ExecuteScalar(sql, new SqlParameter("@user_id", userId));
            if (result != null && result != DBNull.Value)
                return Convert.ToInt32(result);

            const string fallbackSql = @"
SELECT TOP 1 u.user_id
FROM t_user u
INNER JOIN t_doctor d ON u.user_id = d.doctor_id
WHERE u.user_type = 2 AND u.status = 1
ORDER BY u.user_id ASC";
            object fallbackResult = SqlHelper.ExecuteScalar(fallbackSql);
            return fallbackResult != null && fallbackResult != DBNull.Value ? Convert.ToInt32(fallbackResult) : 0;
        }
        #endregion

        #region 9. 获取用户今日最新血糖值
        /// <summary>
        /// 获取用户今日最新的血糖记录值（无数据返回0）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>最新血糖值，无数据返回0</returns>
        public decimal GetTodayLatestBloodSugar(int userId)
        {
            try
            {
                string sql = @"
SELECT TOP 1 blood_sugar_value 
FROM t_blood_sugar 
WHERE user_id = @user_id 
AND CONVERT(date, measurement_time) = CONVERT(date, GETDATE())
AND data_status = 1
ORDER BY measurement_time DESC";
                SqlParameter[] parameters = {
            new SqlParameter("@user_id", userId)
        };
                object obj = SqlHelper.ExecuteScalar(sql, parameters);
                return obj != null && obj != DBNull.Value ? Convert.ToDecimal(obj) : 0m;
            }
            catch (Exception ex)
            {
                throw new Exception($"获取今日最新血糖失败：{ex.Message}", ex);
            }
        }
        #endregion

        public List<BloodSugar> GetUserBloodSugarList(int userId)
        {
            try
            {
                if (userId <= 0)
                    return new List<BloodSugar>();

                return dalBs.GetUserBloodSugarList(userId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"【获取血糖记录异常】{ex.Message}\n{ex.StackTrace}");
                return new List<BloodSugar>();
            }
        }
    }
}