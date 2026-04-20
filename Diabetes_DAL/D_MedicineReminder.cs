// DAL/D_MedicineReminder.cs
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Model;
using Tools;

namespace DAL
{
    /// <summary>
    /// 用药提醒数据访问层
    /// </summary>
    public class D_MedicineReminder
    {
        #region 1. 获取用户所有用药提醒
        public List<MedicineReminder> GetUserReminders(int userId)
        {
            string sql = @"
            SELECT * FROM t_medicine_reminder 
            WHERE user_id = @UserId 
            ORDER BY reminder_time ASC";

            SqlParameter[] param = { new SqlParameter("@UserId", userId) };
            return SqlHelper.GetModelList<MedicineReminder>(sql, param);
        }
        #endregion

        #region 2. 新增用药提醒
        public int AddReminder(MedicineReminder reminder)
        {
            string sql = @"
            INSERT INTO t_medicine_reminder (
                user_id, drug_name, drug_dosage, take_way, reminder_time, 
                is_enabled, remark, data_version, create_time, update_time
            ) VALUES (
                @user_id, @drug_name, @drug_dosage, @take_way, @reminder_time,
                @is_enabled, @remark, 1, GETDATE(), GETDATE()
            );
            SELECT SCOPE_IDENTITY();";

            SqlParameter[] param = {
                new SqlParameter("@user_id", reminder.user_id),
                new SqlParameter("@drug_name", reminder.drug_name),
                new SqlParameter("@drug_dosage", reminder.drug_dosage),
                new SqlParameter("@take_way", reminder.take_way),
                new SqlParameter("@reminder_time", reminder.reminder_time),
                new SqlParameter("@is_enabled", reminder.is_enabled),
                new SqlParameter("@remark", reminder.remark ?? (object)DBNull.Value)
            };

            object result = SqlHelper.ExecuteScalar(sql, param);
            return result != null ? Convert.ToInt32(result) : 0;
        }
        #endregion

        #region 3. 更新用药提醒
        public bool UpdateReminder(MedicineReminder reminder)
        {
            string sql = @"
            UPDATE t_medicine_reminder SET
                drug_name = @drug_name,
                drug_dosage = @drug_dosage,
                take_way = @take_way,
                reminder_time = @reminder_time,
                is_enabled = @is_enabled,
                remark = @remark,
                update_time = GETDATE(),
                data_version = data_version + 1
            WHERE reminder_id = @reminder_id AND user_id = @user_id";

            SqlParameter[] param = {
                new SqlParameter("@reminder_id", reminder.reminder_id),
                new SqlParameter("@user_id", reminder.user_id),
                new SqlParameter("@drug_name", reminder.drug_name),
                new SqlParameter("@drug_dosage", reminder.drug_dosage),
                new SqlParameter("@take_way", reminder.take_way),
                new SqlParameter("@reminder_time", reminder.reminder_time),
                new SqlParameter("@is_enabled", reminder.is_enabled),
                new SqlParameter("@remark", reminder.remark ?? (object)DBNull.Value)
            };

            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }
        #endregion

        #region 4. 删除用药提醒
        public bool DeleteReminder(int reminderId, int userId)
        {
            string sql = "DELETE FROM t_medicine_reminder WHERE reminder_id = @reminder_id AND user_id = @user_id";

            SqlParameter[] param = {
                new SqlParameter("@reminder_id", reminderId),
                new SqlParameter("@user_id", userId)
            };

            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }
        #endregion
    }
}