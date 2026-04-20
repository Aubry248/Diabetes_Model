using System;
using System.Collections.Generic;
using System.Data;
using DAL;
using Model;

namespace BLL
{
    /// <summary>
    /// 运动库管理业务逻辑层
    /// </summary>
    public class B_ExerciseLibrary
    {
        private readonly D_ExerciseLibrary _dal = new D_ExerciseLibrary();
        //private readonly string _systemUser = "系统管理员"; // 可从登录信息动态获取

        #region 核心：查询运动库列表
        /// <summary>
        /// 多条件分页查询运动库列表
        /// </summary>
        public BizResult GetExerciseList(string searchKey, string exerciseCategory, string intensityCategory, string suitablePeople, string enableStatus, DateTime updateStart, DateTime updateEnd)
        {
            try
            {
                int totalCount = 0;
                DataTable dt = _dal.GetExerciseListByPage(searchKey, exerciseCategory, intensityCategory, suitablePeople, enableStatus, updateStart, updateEnd, out totalCount);
                return BizResult.Success($"查询到{totalCount}条运动数据", dt, totalCount);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"查询运动库失败：{ex.Message}");
            }
        }
        #endregion

        #region 基础CRUD业务
        /// <summary>
        /// 根据ID获取运动详情
        /// </summary>
        public BizResult GetExerciseDetail(int exerciseId)
        {
            try
            {
                if (exerciseId <= 0) return BizResult.Fail("无效的运动ID");
                var model = _dal.GetExerciseById(exerciseId);
                if (model == null) return BizResult.Fail("未找到该运动数据");
                return BizResult.Success("加载成功", model);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"加载运动详情失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 新增运动数据
        /// </summary>
        public BizResult AddExercise(ExerciseEnergyExpenditure model, string currentUser)
        {
            try
            {
                // 业务校验
                if (string.IsNullOrWhiteSpace(model.ExerciseName))
                    return BizResult.Fail("运动名称不能为空");
                if (string.IsNullOrWhiteSpace(model.ExerciseCategory))
                    return BizResult.Fail("请选择运动分类");
                if (model.MET_Value <= 0)
                    return BizResult.Fail("MET值必须大于0");
                if (string.IsNullOrWhiteSpace(model.IntensityCategory))
                    return BizResult.Fail("请选择运动强度");
                if (string.IsNullOrWhiteSpace(model.StandardSource))
                    model.StandardSource = "中国糖尿病患者运动指南";

                // 唯一性校验
                if (_dal.CheckExerciseNameExists(model.ExerciseName))
                    return BizResult.Fail("该运动名称已存在，请勿重复添加");

                // 填充系统字段
                model.ExerciseCode = _dal.GenerateExerciseCode();
                model.CreateTime = DateTime.Now;
                model.CreateUser = currentUser;
                model.UpdateTime = DateTime.Now;
                model.UpdateUser = currentUser;
                model.EnableStatus = "启用";
                model.AuditStatus = "待审核";
                model.Version = "V1.0.0";

                int newId = _dal.AddExercise(model);
                if (newId > 0)
                    return BizResult.Success("新增成功，已提交待审核", newId);
                else
                    return BizResult.Fail("新增失败，请重试");
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"新增运动失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 修改运动数据
        /// </summary>
        public BizResult UpdateExercise(ExerciseEnergyExpenditure model, string currentUser, string updateLog)
        {
            try
            {
                if (model.ExerciseID <= 0) return BizResult.Fail("无效的运动ID");
                if (string.IsNullOrWhiteSpace(updateLog)) return BizResult.Fail("请填写更新日志，用于版本追溯");

                // 唯一性校验
                if (_dal.CheckExerciseNameExists(model.ExerciseName, model.ExerciseID))
                    return BizResult.Fail("该运动名称已存在");

                // 填充系统字段
                model.UpdateTime = DateTime.Now;
                model.UpdateUser = currentUser;
                model.UpdateLog = updateLog;
                model.AuditStatus = "待审核";

                // 版本号自增
                string[] versionArr = model.Version.Split('.');
                if (versionArr.Length == 3 && int.TryParse(versionArr[2], out int minor))
                {
                    model.Version = $"{versionArr[0]}.{versionArr[1]}.{minor + 1}";
                }

                int row = _dal.UpdateExercise(model);
                if (row > 0)
                    return BizResult.Success("修改成功，已提交待审核");
                else
                    return BizResult.Fail("修改失败，未更新任何数据");
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"修改运动失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 删除运动数据
        /// </summary>
        public BizResult DeleteExercise(int exerciseId)
        {
            try
            {
                if (exerciseId <= 0) return BizResult.Fail("无效的运动ID");
                int row = _dal.DeleteExercise(exerciseId);
                if (row > 0)
                    return BizResult.Success("删除成功");
                else
                    return BizResult.Fail("删除失败，未找到该运动数据");
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"删除运动失败：{ex.Message}");
            }
        }
        #endregion

        #region 批量操作业务
        /// <summary>
        /// 批量更新启用状态
        /// </summary>
        public BizResult BatchUpdateEnableStatus(List<int> exerciseIdList, string enableStatus, string currentUser)
        {
            try
            {
                if (exerciseIdList == null || exerciseIdList.Count == 0)
                    return BizResult.Fail("请先选中要操作的运动记录");
                if (string.IsNullOrWhiteSpace(enableStatus))
                    return BizResult.Fail("请选择要设置的启用状态");

                int row = _dal.BatchUpdateEnableStatus(exerciseIdList, enableStatus, currentUser);
                return BizResult.Success($"成功操作{row}条运动数据", row);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"批量操作失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 批量审核操作
        /// </summary>
        public BizResult BatchAuditExercise(List<int> exerciseIdList, string auditStatus, string auditRecord, string currentUser)
        {
            try
            {
                if (exerciseIdList == null || exerciseIdList.Count == 0)
                    return BizResult.Fail("请先选中要审核的运动记录");
                if (string.IsNullOrWhiteSpace(auditStatus))
                    return BizResult.Fail("请选择审核结果");
                if (auditStatus == "审核驳回" && string.IsNullOrWhiteSpace(auditRecord))
                    return BizResult.Fail("驳回操作必须填写驳回原因");

                int row = _dal.BatchAuditExercise(exerciseIdList, auditStatus, auditRecord, currentUser);
                return BizResult.Success($"审核操作完成，共处理{row}条数据", row);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"审核操作失败：{ex.Message}");
            }
        }
        #endregion

        #region 辅助方法业务
        /// <summary>
        /// 获取所有运动分类列表
        /// </summary>
        public BizResult GetAllExerciseCategory()
        {
            try
            {
                DataTable dt = _dal.GetAllExerciseCategory();
                return BizResult.Success("加载成功", dt);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"加载运动分类失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 获取所有运动名称列表
        /// </summary>
        public BizResult GetAllExerciseNameList()
        {
            try
            {
                DataTable dt = _dal.GetAllExerciseNameList();
                return BizResult.Success("加载成功", dt);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"加载运动列表失败：{ex.Message}");
            }
        }
        #endregion

        #region 审核/版本管理业务
        /// <summary>
        /// 查询待审核数据列表
        /// </summary>
        public BizResult GetAuditList(string auditStatus, string uploader)
        {
            try
            {
                DataTable dt = _dal.GetAuditList(auditStatus, uploader);
                return BizResult.Success($"查询到{dt.Rows.Count}条待审核数据", dt, dt.Rows.Count);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"查询审核列表失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 查询运动版本历史记录
        /// </summary>
        public BizResult GetVersionHistory(int exerciseId)
        {
            try
            {
                if (exerciseId <= 0) return BizResult.Fail("无效的运动ID");
                DataTable dt = _dal.GetVersionHistory(exerciseId);
                return BizResult.Success($"查询到{dt.Rows.Count}条版本记录", dt, dt.Rows.Count);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"查询版本历史失败：{ex.Message}");
            }
        }
        #endregion
    }
}