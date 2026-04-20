using System;
namespace Model
{
    /// <summary>
    /// 首页统计数据视图模型
    /// </summary>
    public class HomeStatisticViewModel
    {
        /// <summary>
        /// 总管理患者数
        /// </summary>
        public int TotalPatientCount { get; set; }
        /// <summary>
        /// 今日待随访数
        /// </summary>
        public int TodayWaitFollowUpCount { get; set; }
        /// <summary>
        /// 血糖异常患者数（近7天）
        /// </summary>
        public int BloodSugarAbnormalPatientCount { get; set; }
        /// <summary>
        /// 待处理异常数
        /// </summary>
        public int WaitHandleAbnormalCount { get; set; }
    }

    /// <summary>
    /// 首页待办事项视图模型（与表格列完全对应）
    /// </summary>
    public class HomeTodoViewModel
    {
        /// <summary>
        /// 患者姓名
        /// </summary>
        public string PatientName { get; set; }
        /// <summary>
        /// 待办事项
        /// </summary>
        public string TodoContent { get; set; }
        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime DeadlineTime { get; set; }
        /// <summary>
        /// 处理状态
        /// </summary>
        public string HandleStatus { get; set; }
        /// <summary>
        /// 关联业务ID（用于跳转详情）
        /// </summary>
        public int RelationId { get; set; }
        /// <summary>
        /// 业务类型（随访/异常处理/复查）
        /// </summary>
        public string BusinessType { get; set; }
    }
}