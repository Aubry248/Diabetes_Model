using System;
namespace Model
{
    /// <summary>
    /// 复查提醒列表视图模型
    /// </summary>
    public class ReviewRemindViewModel
    {
        public int RemindId { get; set; }
        public string PatientName { get; set; }
        public DateTime ReviewDate { get; set; }
        public string RemindContent { get; set; }
        public string CreateDoctor { get; set; }
        public DateTime CreateTime { get; set; }
        public string StatusDesc { get; set; }
    }
}