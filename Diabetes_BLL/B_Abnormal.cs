using System;
using System.Collections.Generic;
using Model;
using DAL;

namespace BLL
{
    /// <summary>
    /// 异常数据处理业务逻辑层
    /// </summary>
    public class B_Abnormal
    {
        private readonly D_Abnormal _dalAbnormal = new D_Abnormal();

        public (bool Success, string Msg, int AbnormalId) AddSystemAbnormal(Abnormal model)
        {
            if (model == null)
                return (false, "异常数据不能为空", 0);
            if (model.user_id <= 0)
                return (false, "患者信息无效", 0);
            if (model.original_data_id <= 0)
                return (false, "原始业务数据ID无效", 0);
            if (string.IsNullOrWhiteSpace(model.data_type))
                return (false, "异常数据类型不能为空", 0);
            if (string.IsNullOrWhiteSpace(model.abnormal_type))
                return (false, "异常类型不能为空", 0);
            if (string.IsNullOrWhiteSpace(model.abnormal_reason))
                return (false, "异常原因不能为空", 0);
            if (string.IsNullOrWhiteSpace(model.suggestion))
                return (false, "干预建议不能为空", 0);
            if (model.mark_by <= 0)
                return (false, "未获取到负责医生，无法生成异常预警", 0);

            if (model.data_version <= 0)
                model.data_version = 1;
            if (model.handle_status < 0)
                model.handle_status = 0;

            if (model.mark_time == default(DateTime))
                model.mark_time = DateTime.Now;
            if (model.create_time == default(DateTime))
                model.create_time = DateTime.Now;
            if (model.update_time == default(DateTime))
                model.update_time = DateTime.Now;

            try
            {
                int abnormalId = _dalAbnormal.AddAbnormal(model);
                return abnormalId > 0
                    ? (true, "异常预警写入成功", abnormalId)
                    : (false, "异常预警写入失败", 0);
            }
            catch (Exception ex)
            {
                return (false, $"系统异常：{ex.Message}", 0);
            }
        }

        #region 公共方法：获取患者列表
        public List<PatientSimpleInfo> GetAllValidPatient()
        {
            return _dalAbnormal.GetAllValidPatient();
        }
        #endregion
        #region 追加：带医生ID的患者查询方法（传递权限参数）
        public List<PatientSimpleInfo> GetAllValidPatient(int doctorId)
        {
            if (doctorId <= 0)
                return GetAllValidPatient(); // 异常情况下返回所有患者（兼容原有逻辑）
            return _dalAbnormal.GetAllValidPatient(doctorId);
        }
        #endregion
        #region 1. 异常数据预警业务
        public (bool Success, string Msg, List<AbnormalWarnViewModel> Data) GetAbnormalWarnList(int? userId, string abnormalType, DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                return (false, "结束日期不能早于开始日期", null);

            try
            {
                var data = _dalAbnormal.GetAbnormalWarnList(userId, abnormalType, startDate, endDate);
                return (true, "查询成功", data);
            }
            catch (Exception ex)
            {
                return (false, $"系统异常：{ex.Message}", null);
            }
        }
        #endregion

        #region 2. 异常原因标注业务
        public List<Abnormal> GetWaitMarkAbnormalByUserId(int userId)
        {
            if (userId <= 0) return new List<Abnormal>();
            return _dalAbnormal.GetWaitMarkAbnormalByUserId(userId);
        }

        public (bool Success, string Msg) SaveAbnormalMark(Abnormal model)
        {
            if (model.abnormal_id <= 0)
                return (false, "请选择有效异常数据");
            if (string.IsNullOrEmpty(model.abnormal_reason))
                return (false, "异常原因不能为空");
            if (model.mark_by <= 0)
                return (false, "当前登录医生信息异常，请重新登录");

            // 赋值系统默认值
            model.mark_time = DateTime.Now;
            model.update_time = DateTime.Now;

            try
            {
                int rows = _dalAbnormal.UpdateAbnormalMark(model);
                return rows > 0 ? (true, "异常原因标注保存成功") : (false, "保存失败，请重试");
            }
            catch (Exception ex)
            {
                return (false, $"系统异常：{ex.Message}");
            }
        }
        #endregion

        #region 3. 异常干预处理业务
        public (bool Success, string Msg) SaveInterventionPlan(InterventionPlan plan, int abnormalId)
        {
            if (plan.user_id <= 0)
                return (false, "请选择有效患者");
            if (string.IsNullOrEmpty(plan.plan_content))
                return (false, "干预处理措施不能为空");
            if (string.IsNullOrEmpty(plan.expected_effect))
                return (false, "干预效果预期不能为空");
            if (plan.create_by <= 0)
                return (false, "当前登录医生信息异常，请重新登录");
            if (plan.end_time < plan.start_time)
                return (false, "结束日期不能早于开始日期");
            if (plan.review_time < plan.start_time)
                return (false, "复查日期不能早于开始日期");

            // 赋值系统默认值
            plan.execute_status = 0;
            plan.data_version = 1;
            plan.create_time = DateTime.Now;
            plan.update_time = DateTime.Now;
            plan.status = 1;

            try
            {
                int result = _dalAbnormal.AddInterventionPlan(plan, abnormalId);
                return result > 0 ? (true, "干预方案保存成功，异常状态已更新为已处理") : (false, "保存失败，请重试");
            }
            catch (Exception ex)
            {
                return (false, $"系统异常：{ex.Message}");
            }
        }
        #endregion

        #region 4. 复查提醒管理业务
        public (bool Success, string Msg, int RemindId) AddReviewRemind(FollowUp model)
        {
            if (model.user_id <= 0)
                return (false, "请选择有效患者", 0);
            if (model.follow_up_time < DateTime.Now.Date)
                return (false, "复查日期不能早于当前日期", 0);
            if (string.IsNullOrEmpty(model.follow_up_content))
                return (false, "复查提醒内容不能为空", 0);
            if (model.follow_up_by <= 0)
                return (false, "当前登录医生信息异常，请重新登录", 0);

            try
            {
                int remindId = _dalAbnormal.AddReviewRemind(model);
                return remindId > 0 ? (true, "复查提醒设置成功", remindId) : (false, "设置失败，请重试", 0);
            }
            catch (Exception ex)
            {
                return (false, $"系统异常：{ex.Message}", 0);
            }
        }

        public (bool Success, string Msg, List<ReviewRemindViewModel> Data) GetReviewRemindList(int? userId, DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                return (false, "结束日期不能早于开始日期", null);

            try
            {
                var data = _dalAbnormal.GetReviewRemindList(userId, startDate, endDate);
                return (true, "查询成功", data);
            }
            catch (Exception ex)
            {
                return (false, $"系统异常：{ex.Message}", null);
            }
        }
        #endregion

        #region 追加：带筛选+分页的患者查询业务方法（弹窗专用）
        public (bool Success, string Msg, List<PatientSimpleInfo> Data, int TotalCount) GetPatientListByPage(int doctorId, string userName, string phone, string diabetesType, int pageIndex, int pageSize)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 20;

            try
            {
                int totalCount;
                var data = _dalAbnormal.GetPatientListByPage(doctorId, userName, phone, diabetesType, pageIndex, pageSize, out totalCount);
                return (true, "查询成功", data, totalCount);
            }
            catch (Exception ex)
            {
                return (false, $"系统异常：{ex.Message}", null, 0);
            }
        }
        #endregion
        #region 【追加新方法】分页查询有异常数据的患者列表
        public (bool Success, string Msg, List<PatientSimpleInfo> Data, int TotalCount) GetPatientWithAbnormalByPage(int doctorId, string userName, string phone, string diabetesType, int pageIndex, int pageSize)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 20;
            try
            {
                int totalCount;
                var data = _dalAbnormal.GetPatientWithAbnormalByPage(doctorId, userName, phone, diabetesType, pageIndex, pageSize, out totalCount);
                return (true, "查询成功", data, totalCount);
            }
            catch (Exception ex)
            {
                return (false, $"系统异常：{ex.Message}", null, 0);
            }
        }
        #endregion

        #region 【追加新方法】按患者ID获取异常预警数据
        public (bool Success, string Msg, List<AbnormalWarnViewModel> Data) GetAbnormalWarnByPatientId(int patientId, DateTime startDate, DateTime endDate)
        {
            if (patientId <= 0)
                return (false, "请选择有效患者", null);
            if (endDate < startDate)
                return (false, "结束日期不能早于开始日期", null);
            try
            {
                var data = _dalAbnormal.GetAbnormalWarnByPatientId(patientId, startDate, endDate);
                return (true, "查询成功", data);
            }
            catch (Exception ex)
            {
                return (false, $"系统异常：{ex.Message}", null);
            }
        }
        #endregion


    }
}