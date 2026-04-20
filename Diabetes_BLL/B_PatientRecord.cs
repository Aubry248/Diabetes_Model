using DAL;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    /// <summary>
    /// 患者档案管理业务逻辑层
    /// </summary>
    public class B_PatientRecord
    {
        private readonly D_PatientRecord _dalPatient = new D_PatientRecord();
        private readonly D_User _dalUser = new D_User();

        #region 1. 患者档案分页查询业务
        /// <summary>
        /// 分页查询患者档案列表
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="totalCount">输出总条数</param>
        /// <param name="userName">患者姓名</param>
        /// <param name="phone">手机号</param>
        /// <param name="diabetesType">糖尿病类型</param>
        /// <param name="controlStatus">血糖控制状态</param>
        /// <param name="startDiagnoseDate">确诊开始日期</param>
        /// <param name="endDiagnoseDate">确诊结束日期</param>
        /// <param name="onlyValid">仅启用患者</param>
        /// <returns>患者档案列表</returns>
        public List<PatientArchive> GetPatientPageList(int pageIndex, int pageSize, out int totalCount,
            string userName = "", string phone = "", string diabetesType = "", string controlStatus = "",
            DateTime? startDiagnoseDate = null, DateTime? endDiagnoseDate = null, bool onlyValid = true)
        {
            totalCount = 0;
            // 入参校验
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 1000) pageSize = 1000; // 防止超大查询

            try
            {
                return _dalPatient.GetPatientArchivePageList(pageIndex, pageSize, out totalCount,
                    userName, phone, diabetesType, controlStatus, startDiagnoseDate, endDiagnoseDate, onlyValid);
            }
            catch (Exception ex)
            {
                throw new Exception("查询患者档案列表失败：" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 获取患者档案详情
        /// </summary>
        /// <param name="userId">患者ID</param>
        /// <returns>患者档案详情</returns>
        public PatientArchive GetPatientDetail(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("患者ID无效");

            try
            {
                return _dalPatient.GetPatientArchiveDetail(userId);
            }
            catch (Exception ex)
            {
                throw new Exception("获取患者档案详情失败：" + ex.Message, ex);
            }
        }
        #endregion

        #region 2. 患者病程/并发症/合并症维护业务
        /// <summary>
        /// 保存患者患病信息（基础信息+健康评估）
        /// </summary>
        /// <param name="user">患者基础信息</param>
        /// <param name="assessment">健康评估信息</param>
        /// <returns>元组（是否成功，消息，评估ID）</returns>
        public (bool Success, string Message, int AssessmentId) SavePatientDiseaseInfo(Users user, HealthAssessment assessment)
        {
            // 基础校验
            if (user == null || user.user_id <= 0)
                return (false, "患者基础信息无效", 0);
            if (assessment == null)
                return (false, "健康评估信息不能为空", 0);
            if (assessment.assessment_by <= 0)
                return (false, "操作人信息无效", 0);

            // 业务规则校验
            if (!string.IsNullOrEmpty(user.diabetes_type) && !new[] { "1型", "2型", "妊娠", "其他" }.Contains(user.diabetes_type))
                return (false, "糖尿病类型不符合规范", 0);
            if (user.diagnose_date.HasValue && user.diagnose_date.Value > DateTime.Now)
                return (false, "确诊日期不能晚于当前时间", 0);
            if (assessment.assessment_date > DateTime.Now)
                return (false, "评估日期不能晚于当前时间", 0);
            if (assessment.hba1c.HasValue && (assessment.hba1c.Value < 2 || assessment.hba1c.Value > 20))
                return (false, "糖化血红蛋白值超出合理范围", 0);

            try
            {
                // 1. 更新患者基础患病信息
                bool updateBaseResult = _dalPatient.UpdatePatientBaseDiseaseInfo(user);
                if (!updateBaseResult)
                    return (false, "更新患者基础患病信息失败", 0);

                // 2. 新增健康评估版本
                int assessmentId = _dalPatient.SavePatientDiseaseInfo(assessment);
                if (assessmentId <= 0)
                    return (false, "保存患者健康评估信息失败", 0);

                return (true, "保存成功", assessmentId);
            }
            catch (Exception ex)
            {
                return (false, "保存患者患病信息异常：" + ex.Message, 0);
            }
        }
        #endregion

        #region 3. 健康档案历史追溯业务
        /// <summary>
        /// 获取患者档案历史记录
        /// </summary>
        /// <param name="userId">患者ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="recordType">记录类型</param>
        /// <returns>历史记录列表</returns>
        public List<PatientArchiveHistory> GetPatientHistoryList(int userId, DateTime? startTime = null, DateTime? endTime = null, string recordType = "")
        {
            if (userId <= 0)
                throw new ArgumentException("患者ID无效");

            try
            {
                return _dalPatient.GetPatientArchiveHistory(userId, startTime, endTime, recordType);
            }
            catch (Exception ex)
            {
                throw new Exception("获取患者历史档案失败：" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 获取患者历史健康评估列表
        /// </summary>
        /// <param name="userId">患者ID</param>
        /// <returns>历史评估列表</returns>
        public List<HealthAssessment> GetPatientHistoryAssessment(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("患者ID无效");

            try
            {
                return _dalPatient.GetPatientHistoryAssessmentList(userId);
            }
            catch (Exception ex)
            {
                throw new Exception("获取患者历史评估记录失败：" + ex.Message, ex);
            }
        }
        #endregion

        #region 4. 辅助业务方法
        /// <summary>
        /// 校验患者是否存在且有效
        /// </summary>
        /// <param name="userId">患者ID</param>
        /// <returns>是否有效</returns>
        public bool CheckPatientValid(int userId)
        {
            if (userId <= 0) return false;
            try
            {
                var detail = GetPatientDetail(userId);
                return detail != null && detail.BaseInfo.status == 1;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region 5. 新增：患者简易列表查询（用于追溯页下拉选择）
        /// <summary>
        /// 获取患者简易列表，用于下拉选择，支持姓名/电话模糊搜索
        /// </summary>
        /// <param name="searchText">搜索关键词</param>
        /// <returns>患者简易信息列表</returns>
        public List<Users> GetPatientSimpleList(string searchText = "")
        {
            if (searchText == null) searchText = "";
            try
            {
                return _dalPatient.GetPatientSimpleList(searchText);
            }
            catch (Exception ex)
            {
                throw new Exception("查询患者简易列表失败：" + ex.Message, ex);
            }
        }
        #endregion
    }
}