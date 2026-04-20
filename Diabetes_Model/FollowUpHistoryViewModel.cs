using System;
namespace Model
{
    /// <summary>
    /// 随访历史视图模型，用于DataGridView数据绑定
    /// </summary>
    public class FollowUpHistoryViewModel
    {
        /// <summary>
        /// 随访记录ID
        /// </summary>
        public int FollowUpId { get; set; }
        /// <summary>
        /// 随访日期
        /// </summary>
        public DateTime FollowUpDate { get; set; }
        /// <summary>
        /// 患者姓名
        /// </summary>
        public string PatientName { get; set; }
        /// <summary>
        /// 随访类型
        /// </summary>
        public string FollowUpType { get; set; }
        /// <summary>
        /// 随访方式
        /// </summary>
        public string FollowUpWay { get; set; }
        /// <summary>
        /// 随访结果
        /// </summary>
        public string FollowUpResult { get; set; }
        /// <summary>
        /// 随访状态
        /// </summary>
        public string StatusDesc { get; set; }
        /// <summary>
        /// 随访医生
        /// </summary>
        public string FollowUpDoctor { get; set; }
    }
}