using System;

namespace Model
{
    /// <summary>
    /// 对应数据库 t_dict 表，系统数据字典
    /// </summary>
    public class Dict
    {
        // 基础字段（大驼峰，与DAL层完全一致）
        public int DictId { get; set; }
        public string DictType { get; set; }
        public string DictCode { get; set; }
        public string DictName { get; set; }
        public string DictValue { get; set; }
        public string DictDesc { get; set; }

        // 公共字段
        public string Source { get; set; }
        public int Sort { get; set; }
        public byte Status { get; set; }
        public int? ParentDictId { get; set; }
        public int? TypeId { get; set; }
        public int CreateBy { get; set; }
        public int UpdateBy { get; set; }
        public int DataVersion { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}