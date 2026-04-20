// BLL/B_MedicineReminder.cs
using System;
using System.Collections.Generic;
using DAL;
using Model;

namespace BLL
{
    /// <summary>
    /// 用药提醒业务逻辑层
    /// </summary>
    public class B_MedicineReminder
    {
        private readonly D_MedicineReminder _dalReminder = new D_MedicineReminder();

        #region 1. 获取用户所有用药提醒
        public List<MedicineReminder> GetUserReminders(int userId)
        {
            try
            {
                if (userId <= 0)
                    return new List<MedicineReminder>();

                return _dalReminder.GetUserReminders(userId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"【获取用药提醒异常】{ex.Message}\n{ex.StackTrace}");
                return new List<MedicineReminder>();
            }
        }
        #endregion

        #region 2. 新增用药提醒
        public ResultModel AddReminder(MedicineReminder reminder)
        {
            try
            {
                if (reminder == null || reminder.user_id <= 0)
                    return new ResultModel(false, "用户信息无效");
                if (string.IsNullOrWhiteSpace(reminder.drug_name))
                    return new ResultModel(false, "药物名称不能为空");
                if (string.IsNullOrWhiteSpace(reminder.reminder_time))
                    return new ResultModel(false, "提醒时间不能为空");

                int reminderId = _dalReminder.AddReminder(reminder);
                if (reminderId > 0)
                    return new ResultModel(true, "用药提醒添加成功", reminderId);
                else
                    return new ResultModel(false, "添加失败");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"【添加用药提醒异常】{ex.Message}\n{ex.StackTrace}");
                return new ResultModel(false, $"系统异常：{ex.Message}");
            }
        }
        #endregion

        #region 3. 更新用药提醒
        public ResultModel UpdateReminder(MedicineReminder reminder)
        {
            try
            {
                if (reminder == null || reminder.reminder_id <= 0)
                    return new ResultModel(false, "提醒ID无效");
                if (reminder.user_id <= 0)
                    return new ResultModel(false, "用户信息无效");

                bool result = _dalReminder.UpdateReminder(reminder);
                if (result)
                    return new ResultModel(true, "用药提醒更新成功");
                else
                    return new ResultModel(false, "更新失败，记录不存在");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"【更新用药提醒异常】{ex.Message}\n{ex.StackTrace}");
                return new ResultModel(false, $"系统异常：{ex.Message}");
            }
        }
        #endregion

        #region 4. 删除用药提醒
        public ResultModel DeleteReminder(int reminderId, int userId)
        {
            try
            {
                if (reminderId <= 0)
                    return new ResultModel(false, "提醒ID无效");
                if (userId <= 0)
                    return new ResultModel(false, "用户信息无效");

                bool result = _dalReminder.DeleteReminder(reminderId, userId);
                if (result)
                    return new ResultModel(true, "用药提醒删除成功");
                else
                    return new ResultModel(false, "删除失败，记录不存在");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"【删除用药提醒异常】{ex.Message}\n{ex.StackTrace}");
                return new ResultModel(false, $"系统异常：{ex.Message}");
            }
        }
        #endregion
    }
}