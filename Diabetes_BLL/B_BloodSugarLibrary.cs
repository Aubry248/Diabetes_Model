// BLL/B_BloodSugarLibrary.cs
using System;
using System.Collections.Generic;
using System.Data;
using DAL;
using Model;

namespace BLL
{
    /// <summary>
    /// 血糖数据管理业务逻辑层
    /// </summary>
    public class B_BloodSugarLibrary
    {
        private readonly D_BloodSugarLibrary _dal = new D_BloodSugarLibrary();
        private readonly string _systemUser = "系统管理员1"; // 可从登录全局变量获取

        #region 核心：查询血糖数据列表
        /// <summary>
        /// 多条件分页查询血糖数据
        /// </summary>
        public BizResult GetBloodSugarList(int userId, string scenario, int isAbnormal, string dataSource,
            DateTime startTime, DateTime endTime)
        {
            try
            {
                int totalCount = 0;
                DataTable dt = _dal.GetBloodSugarListByPage(userId, scenario, isAbnormal, dataSource, startTime, endTime, out totalCount);
                return BizResult.Success($"查询到{totalCount}条血糖数据", dt, totalCount);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"查询血糖数据失败：{ex.Message}");
            }
        }
        #endregion

        #region 基础CRUD业务
        /// <summary>
        /// 根据ID获取血糖详情
        /// </summary>
        public BizResult GetBloodSugarDetail(int bloodSugarId)
        {
            try
            {
                if (bloodSugarId <= 0) return BizResult.Fail("无效的血糖记录ID");

                var model = _dal.GetBloodSugarById(bloodSugarId);
                if (model == null) return BizResult.Fail("未找到该血糖记录");

                return BizResult.Success("加载成功", model);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"加载血糖详情失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 新增血糖记录
        /// </summary>
        public BizResult AddBloodSugar(BloodSugar model)
        {
            try
            {
                // 业务校验
                if (model.user_id <= 0) return BizResult.Fail("请选择患者");
                if (model.blood_sugar_value < 0 || model.blood_sugar_value > 30)
                    return BizResult.Fail("血糖值必须在0-30mmol/L之间");
                if (model.measurement_time == null || model.measurement_time > DateTime.Now)
                    return BizResult.Fail("测量时间不能大于当前时间");
                if (string.IsNullOrWhiteSpace(model.measurement_scenario))
                    return BizResult.Fail("请选择测量场景");

                // 填充系统字段
                model.operator_id = 1; // 系统管理员ID，可从登录信息获取
                model.data_status = 0;
                model.data_version = 1;

                // 自动判断异常（补充数据库计算列逻辑）
                model.is_abnormal = IsBloodSugarAbnormal(model.blood_sugar_value, model.measurement_scenario);

                int newId = _dal.AddBloodSugar(model);
                if (newId > 0)
                    return BizResult.Success("血糖记录新增成功", newId);
                else
                    return BizResult.Fail("新增失败，请重试");
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"新增血糖记录失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 修改血糖记录
        /// </summary>
        public BizResult UpdateBloodSugar(BloodSugar model)
        {
            try
            {
                if (model.blood_sugar_id <= 0) return BizResult.Fail("无效的血糖记录ID");
                if (model.blood_sugar_value < 0 || model.blood_sugar_value > 30)
                    return BizResult.Fail("血糖值必须在0-30mmol/L之间");
                if (model.measurement_time == null || model.measurement_time > DateTime.Now)
                    return BizResult.Fail("测量时间不能大于当前时间");

                // 重新判断异常
                model.is_abnormal = IsBloodSugarAbnormal(model.blood_sugar_value, model.measurement_scenario);
                model.operator_id = 1; // 系统管理员ID

                int row = _dal.UpdateBloodSugar(model);
                if (row > 0)
                    return BizResult.Success("血糖记录修改成功");
                else
                    return BizResult.Fail("修改失败，未更新任何数据");
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"修改血糖记录失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 删除血糖记录
        /// </summary>
        public BizResult DeleteBloodSugar(int bloodSugarId)
        {
            try
            {
                if (bloodSugarId <= 0) return BizResult.Fail("无效的血糖记录ID");

                int row = _dal.DeleteBloodSugar(bloodSugarId);
                if (row > 0)
                    return BizResult.Success("血糖记录删除成功");
                else
                    return BizResult.Fail("删除失败，未找到该记录");
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"删除血糖记录失败：{ex.Message}");
            }
        }
        #endregion

        #region 批量操作业务
        /// <summary>
        /// 批量删除血糖记录
        /// </summary>
        public BizResult BatchDeleteBloodSugar(List<int> idList)
        {
            try
            {
                if (idList == null || idList.Count == 0)
                    return BizResult.Fail("请先选中要删除的记录");

                int row = _dal.BatchDeleteBloodSugar(idList);
                return BizResult.Success($"成功删除{row}条血糖记录", row);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"批量删除失败：{ex.Message}");
            }
        }
        #endregion

        #region 辅助方法业务
        /// <summary>
        /// 获取所有患者列表
        /// </summary>
        public BizResult GetAllPatients()
        {
            try
            {
                DataTable dt = _dal.GetAllPatients();
                return BizResult.Success("加载成功", dt);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"加载患者列表失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 判断血糖是否异常（遵循《中国2型糖尿病防治指南》）
        /// </summary>
        private int IsBloodSugarAbnormal(decimal value, string scenario)
        {
            switch (scenario)
            {
                case "空腹":
                    return (value < 3.9m || value > 7.0m) ? 1 : 0;
                case "餐后2小时":
                case "早餐后":
                case "午餐后":
                case "晚餐后":
                    return value > 11.1m ? 1 : 0;
                case "睡前":
                case "随机":
                    return value > 11.1m ? 1 : 0;
                default:
                    return 0;
            }
        }
        #endregion
    }
}