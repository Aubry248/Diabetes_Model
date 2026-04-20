using System;

namespace Model
{
    /// <summary>
    /// 字典分类实体（对应t_dict_type表）
    /// </summary>
    public class DictType
    {
        public int TypeId { get; set; }
        public string TypeCode { get; set; }
        public string TypeName { get; set; }
        public string Description { get; set; }
        public int CreateBy { get; set; }
        public int UpdateBy { get; set; }
        public int DataVersion { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public byte Status { get; set; } // 1-启用 0-禁用
    }
}