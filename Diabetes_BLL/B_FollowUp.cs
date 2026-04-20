using System;
using System.Collections.Generic;
using Model;
using DAL;

namespace BLL
{
    /// <summary>
    /// 随访管理业务逻辑层
    /// </summary>
    public class B_FollowUp
    {
        private readonly D_FollowUp _dalFollowUp = new D_FollowUp();

        #region 患者数据业务
        /// <summary>
        /// 获取所有有效患者列表
        /// </summary>
        public List<PatientSimpleInfo> GetAllValidPatient()
        {
            return _dalFollowUp.GetAllValidPatient();
        }
        #endregion

        #region 随访计划业务
        /// <summary>
        /// 新增随访计划（带业务校验）
        /// </summary>
        public (bool Success, string Msg, int PlanId) AddFollowUpPlan(FollowUp model)
        {
            // 业务校验
            if (model.user_id <= 0)
                return (false, "请选择有效患者", 0);
            if (string.IsNullOrEmpty(model.follow_up_content))
                return (false, "随访计划内容不能为空", 0);
            if (model.follow_up_time < DateTime.Now.Date)
                return (false, "随访日期不能早于当前日期", 0);
            if (model.follow_up_by <= 0)
                return (false, "当前登录医生信息异常，请重新登录", 0);

            // 赋值默认值
            model.follow_up_status = 0; // 待随访
            model.data_version = 1;
            model.create_time = DateTime.Now;
            model.update_time = DateTime.Now;

            try
            {
                int planId = _dalFollowUp.AddFollowUpPlan(model);
                return planId > 0 ? (true, "随访计划保存成功", planId) : (false, "随访计划保存失败，请重试", 0);
            }
            catch (Exception ex)
            {
                return (false, $"系统异常：{ex.Message}", 0);
            }
        }

        /// <summary>
        /// 根据患者ID获取待随访计划列表
        /// </summary>
        public List<FollowUp> GetWaitFollowUpPlanByUserId(int userId)
        {
            if (userId <= 0) return new List<FollowUp>();
            return _dalFollowUp.GetWaitFollowUpPlanByUserId(userId);
        }
        #endregion

        #region 随访记录业务
        /// <summary>
        /// 提交随访记录（带业务校验）
        /// </summary>
        public (bool Success, string Msg) SubmitFollowUpRecord(FollowUp model)
        {
            // 业务校验
            if (model.follow_up_id <= 0)
                return (false, "请选择有效随访计划");
            if (string.IsNullOrEmpty(model.follow_up_result))
                return (false, "随访结果不能为空");
            if (model.next_follow_up_time.HasValue && model.next_follow_up_time.Value < DateTime.Now.Date)
                return (false, "下次随访日期不能早于当前日期");

            // 赋值默认值
            model.follow_up_status = 2; // 已完成
            model.update_time = DateTime.Now;

            try
            {
                int rows = _dalFollowUp.UpdateFollowUpRecord(model);
                return rows > 0 ? (true, "随访记录提交成功，下次随访提醒已设置") : (false, "随访记录提交失败，请重试");
            }
            catch (Exception ex)
            {
                return (false, $"系统异常：{ex.Message}");
            }
        }
        #endregion

        #region 随访历史业务
        /// <summary>
        /// 多条件查询随访历史
        /// </summary>
        public (bool Success, string Msg, List<FollowUpHistoryViewModel> Data) GetFollowUpHistoryList(int? userId, string followUpType, DateTime startDate, DateTime endDate)
        {
            // 业务校验
            if (endDate < startDate)
                return (false, "结束日期不能早于开始日期", null);

            try
            {
                var data = _dalFollowUp.GetFollowUpHistoryList(userId, followUpType, startDate, endDate);
                return (true, "查询成功", data);
            }
            catch (Exception ex)
            {
                return (false, $"系统异常：{ex.Message}", null);
            }
        }
        #endregion

        /// <summary>
        /// 分页查询随访历史
        /// </summary>
        public (bool Success, string Msg, List<FollowUpHistoryViewModel> Data) GetFollowUpHistoryByPage(
            int? userId, string followUpType, DateTime startDate, DateTime endDate,
            int pageIndex, int pageSize, out int totalCount)
        {
            totalCount = 0;
            // 业务校验
            if (endDate < startDate)
                return (false, "结束日期不能早于开始日期", null);
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 20;

            try
            {
                var data = _dalFollowUp.GetFollowUpHistoryByPage(userId, followUpType, startDate, endDate, pageIndex, pageSize, out totalCount);
                return (true, "查询成功", data);
            }
            catch (Exception ex)
            {
                return (false, $"系统异常：{ex.Message}", null);
            }
        }
    }
}