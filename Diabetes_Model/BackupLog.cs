using System;

namespace Model
{
    /// <summary>
    /// 备份还原日志实体（对应t_backup_log表）
    /// </summary>
    public class BackupLog
    {
        public int backup_id { get; set; }
        public string backup_type { get; set; }
        public string backup_path { get; set; }
        public long backup_size { get; set; }
        public string backup_checksum { get; set; }
        public int backup_status { get; set; }
        public string backup_remark { get; set; }
        public DateTime backup_time { get; set; }
        public int backup_by { get; set; }
        public int restore_status { get; set; }
        public int? restore_by { get; set; }
        public DateTime? restore_time { get; set; }
        public string restore_remark { get; set; }
        public int data_version { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }

        // 关联字段
        public string backup_user_name { get; set; }
        public string restore_user_name { get; set; }
        // 格式化显示
        public string backup_size_display => backup_size > 0 ? $"{Math.Round(backup_size * 1.0 / 1024 / 1024, 2)} MB" : "0 MB";
        public string backup_status_display => backup_status == 1 ? "成功" : "失败";
        public string restore_status_display => restore_status == 1 ? "已还原" : "未还原";
    }
}