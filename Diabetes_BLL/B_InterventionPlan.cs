using System;
using System.Collections.Generic;
using System.Data;
using Model;
using DAL;

namespace BLL
{
    /// <summary>
    /// 干预方案业务逻辑层
    /// </summary>
    public class B_InterventionPlan
    {
        #region 依赖注入DAL层对象
        private readonly D_InterventionPlan _dal = new D_InterventionPlan();
        private readonly B_HealthAssessment _healthBll = new B_HealthAssessment();
        #endregion

        #region 1. 干预方案保存业务
        /// <summary>
        /// 保存干预方案（业务校验+入库）
        /// </summary>
        /// <returns>成功返回主键ID，失败返回-1</returns>
        public int SaveInterventionPlan(InterventionPlan model)
        {
            // 核心业务校验（新增 plan_type 合法性校验）
            if (string.IsNullOrEmpty(model.plan_type))
                throw new ArgumentException("请选择方案类型");
            // 严格匹配数据库 CHECK 约束的3个值，避免拼写错误
            var validPlanTypes = new List<string> { "用药调整", "运动干预", "饮食干预" };
            if (!validPlanTypes.Contains(model.plan_type))
                throw new ArgumentException($"方案类型仅支持：{string.Join("、", validPlanTypes)}");

            // 原有其他校验（保留）
            if (model.user_id <= 0) throw new ArgumentException("请选择有效患者");
            if (model.create_by <= 0) throw new ArgumentException("当前登录医生信息异常，请重新登录");
            if (string.IsNullOrEmpty(model.plan_content)) throw new ArgumentException("请填写干预方案内容");
            if (model.start_time > model.end_time) throw new ArgumentException("方案结束时间不能早于开始时间");
            if (model.review_time < model.start_time) throw new ArgumentException("复查时间不能早于方案开始时间");

            // 系统字段赋值（保留）
            model.data_version = 1;
            model.create_time = DateTime.Now;
            model.update_time = DateTime.Now;
            model.execute_status = 0; // 未执行，byte类型
            model.status = 0; // 待下发，byte类型
            model.execute_note = string.Empty;

            // 调用DAL入库
            return _dal.AddInterventionPlan(model);
        }
        #endregion

        #region 2. 方案下发业务
        /// <summary>
        /// 下发干预方案
        /// </summary>
        /// <param name="planId">方案ID</param>
        /// <returns>是否下发成功</returns>
        public bool IssueInterventionPlan(int planId)
        {
            if (planId <= 0) throw new ArgumentException("请选择有效方案");

            // 获取方案详情校验
            var plan = _dal.GetPlanById(planId);
            if (plan == null) throw new Exception("方案不存在或已被删除");
            if (plan.status == 1) throw new Exception("该方案已下发，无需重复操作");
            if (plan.status == 2) throw new Exception("该方案已结束，无法下发");

            // 更新下发状态
            plan.status = 1; // 启用
            plan.execute_status = 1; // 执行中
            plan.update_time = DateTime.Now;

            return _dal.UpdateInterventionPlan(plan);
        }
        #endregion

        #region 3. 公共查询业务
        /// <summary>
        /// 获取患者下拉列表
        /// </summary>
        public DataTable GetPatientList()
        {
            return _dal.GetPatientList();
        }

        /// <summary>
        /// 获取待下发方案列表
        /// </summary>
        public DataTable GetWaitIssuePlanList()
        {
            return _dal.GetWaitIssuePlanList();
        }

        /// <summary>
        /// 获取方案详情
        /// </summary>
        public InterventionPlan GetPlanById(int planId)
        {
            if (planId <= 0) throw new ArgumentException("方案ID无效");
            return _dal.GetPlanById(planId);
        }

        /// <summary>
        /// 获取患者方案调整记录
        /// </summary>
        public DataTable GetPatientPlanAdjustRecords(int userId, DateTime startTime, DateTime endTime)
        {
            if (userId <= 0) throw new ArgumentException("请选择有效患者");
            if (startTime > endTime) throw new ArgumentException("开始时间不能晚于结束时间");
            return _dal.GetPatientPlanAdjustRecords(userId, startTime, endTime);
        }

        /// <summary>
        /// 获取患者最新健康评估信息（用于方案制定参考）
        /// </summary>
        public HealthAssessment GetPatientLatestHealthInfo(int userId)
        {
            if (userId <= 0) return null;
            return _healthBll.GetLatestAssessment(userId);
        }
        #endregion
    }
}