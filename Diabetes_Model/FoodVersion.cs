using System;
namespace Model
{
    /// <summary>
    /// 对应数据库 Diabetes_Food_Version 表，食物版本历史记录
    /// </summary>
    public class FoodVersion
    {
        /// <summary>
        /// 版本记录ID，主键，自增
        /// </summary>
        public int VersionID { get; set; }
        /// <summary>
        /// 关联食物ID
        /// </summary>
        public int FoodID { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 版本更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// 更新人
        /// </summary>
        public string UpdateUser { get; set; }
        /// <summary>
        /// 更新内容说明
        /// </summary>
        public string UpdateContent { get; set; }
        /// <summary>
        /// 审核状态：待审核/审核通过/审核驳回
        /// </summary>
        public string AuditStatus { get; set; }
        /// <summary>
        /// 该版本完整数据快照（JSON格式）
        /// </summary>
        public string FoodDataSnapshot { get; set; }
    }
}