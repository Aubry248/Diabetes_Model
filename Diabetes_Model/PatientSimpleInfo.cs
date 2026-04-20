using System;
namespace Model
{
    /// <summary>
    /// 患者简易信息实体（扩展版，兼容原有逻辑）
    /// </summary>
    public class PatientSimpleInfo
    {

        /// <summary>
        /// 患者ID（主键）
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 患者姓名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 手机号（脱敏）
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 糖尿病类型
        /// </summary>
        public string DiabetesType { get; set; }
        /// <summary>
        /// 待处理异常数
        /// </summary>
        public int UnhandledAbnormalCount { get; set; }
        /// <summary>
        /// 评估状态：已评估/未评估
        /// </summary>
        public string AssessmentStatus { get; set; }
        /// <summary>
        /// 最新评估时间
        /// </summary>
        public DateTime? LastAssessmentTime { get; set; }
        public DateTime? DiagnoseDate { get; set; }
        public string IdCardLast4 { get; set; }
        public int? DoctorId { get; set; }
        #region 【核心修改】DisplayText：从只读改为可写，保留默认计算逻辑
        private string _displayText;

        /// <summary>
        /// 下拉框显示文本（默认：姓名+脱敏手机号+糖尿病类型，支持手动赋值特殊选项）
        /// </summary>
        public string DisplayText
        {
            get
            {
                // 若手动赋值了自定义文本，优先返回（如「全部患者」）
                if (!string.IsNullOrEmpty(_displayText))
                    return _displayText;
                // 否则返回默认复合文本（原有逻辑完全保留）
                return $"{UserName}（{DesensitizePhone(Phone)} | {DiabetesType}）";
            }
            set
            {
                // 允许手动赋值，解决CS0200错误
                _displayText = value;
            }
        }
        #endregion

        #region 原有脱敏方法（完全保留，不修改）
        private string DesensitizePhone(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length != 11)
                return phone;
            return $"{phone.Substring(0, 3)}****{phone.Substring(7)}";
        }
        #endregion
    }
}