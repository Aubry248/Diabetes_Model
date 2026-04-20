using System;
using System.Collections.Generic;
using System.Data;
using Model;
using DAL;

namespace BLL
{
    /// <summary>
    /// 字典库管理业务逻辑层
    /// </summary>
    public class B_DictLibrary
    {
        private readonly D_DictLibrary _dal = new D_DictLibrary();
        private readonly int _currentOperateUserId = 1; // 实际项目从登录全局信息获取

        #region 字典分类业务操作
        /// <summary>
        /// 条件查询字典分类列表
        /// </summary>
        public BizResult GetDictTypeList(string searchKey)
        {
            try
            {
                DataTable dt = _dal.GetDictTypeList(searchKey);
                return BizResult.Success("查询成功", dt, dt.Rows.Count);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"查询字典分类列表失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 根据ID获取字典分类详情
        /// </summary>
        public BizResult GetDictTypeById(int typeId)
        {
            try
            {
                if (typeId <= 0) return BizResult.Fail("分类ID无效");
                DictType model = _dal.GetDictTypeById(typeId);
                if (model == null) return BizResult.Fail("未找到该字典分类信息");
                return BizResult.Success("获取成功", model);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"获取字典分类详情失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 新增字典分类
        /// </summary>
        public BizResult AddDictType(DictType model)
        {
            try
            {
                // 基础必填校验
                if (string.IsNullOrEmpty(model.TypeCode)) return BizResult.Fail("分类编码为必填项");
                if (string.IsNullOrEmpty(model.TypeName)) return BizResult.Fail("分类名称为必填项");
                // 唯一性校验
                if (_dal.CheckDictTypeCodeExists(model.TypeCode))
                    return BizResult.Fail("该分类编码已存在，请勿重复添加");
                // 执行新增
                int typeId = _dal.AddDictType(model);
                if (typeId <= 0) return BizResult.Fail("新增字典分类失败，数据库无返回");
                return BizResult.Success("新增字典分类成功", typeId);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"新增字典分类失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 更新字典分类
        /// </summary>
        public BizResult UpdateDictType(DictType model)
        {
            try
            {
                if (model.TypeId <= 0) return BizResult.Fail("分类ID无效");
                if (string.IsNullOrEmpty(model.TypeCode)) return BizResult.Fail("分类编码为必填项");
                if (string.IsNullOrEmpty(model.TypeName)) return BizResult.Fail("分类名称为必填项");
                // 唯一性校验
                if (_dal.CheckDictTypeCodeExists(model.TypeCode, model.TypeId))
                    return BizResult.Fail("该分类编码已存在，无法修改");
                // 执行更新
                bool result = _dal.UpdateDictType(model);
                return result ? BizResult.Success("更新字典分类成功") : BizResult.Fail("更新字典分类失败，数据库无影响");
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"更新字典分类失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 删除字典分类
        /// </summary>
        public BizResult DeleteDictType(int typeId)
        {
            try
            {
                if (typeId <= 0) return BizResult.Fail("分类ID无效");
                DictType model = _dal.GetDictTypeById(typeId);
                if (model == null) return BizResult.Fail("未找到该字典分类信息");
                // 校验是否有字典项引用
                DataTable dt = _dal.GetDictList(model.TypeCode, "全部", "");
                if (dt != null && dt.Rows.Count > 0)
                    return BizResult.Fail("该分类下存在字典项，无法删除，请先删除对应字典项");
                bool result = _dal.DeleteDictType(typeId);
                return result ? BizResult.Success("删除字典分类成功") : BizResult.Fail("删除字典分类失败");
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"删除字典分类失败：{ex.Message}");
            }
        }
        #endregion

        #region 字典项业务操作
        /// <summary>
        /// 条件查询字典项列表
        /// </summary>
        public BizResult GetDictList(string dictType, string enableStatus, string searchKey)
        {
            try
            {
                DataTable dt = _dal.GetDictList(dictType, enableStatus, searchKey);
                return BizResult.Success("查询成功", dt, dt.Rows.Count);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"查询字典项列表失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 根据ID获取字典项详情
        /// </summary>
        public BizResult GetDictById(int dictId)
        {
            try
            {
                if (dictId <= 0) return BizResult.Fail("字典项ID无效");
                Dict model = _dal.GetDictById(dictId);
                if (model == null) return BizResult.Fail("未找到该字典项信息");
                return BizResult.Success("获取成功", model);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"获取字典项详情失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 新增字典项
        /// </summary>
        public BizResult AddDict(Dict model)
        {
            try
            {
                // 基础必填校验
                if (string.IsNullOrEmpty(model.DictType)) return BizResult.Fail("所属分类为必填项");
                if (string.IsNullOrEmpty(model.DictCode)) return BizResult.Fail("字典编码为必填项");
                if (string.IsNullOrEmpty(model.DictName)) return BizResult.Fail("字典名称为必填项");
                if (string.IsNullOrEmpty(model.DictValue)) return BizResult.Fail("字典值为必填项");

                // 唯一性校验
                if (_dal.CheckDictCodeExists(model.DictType, model.DictCode))
                    return BizResult.Fail("该分类下字典编码已存在，请勿重复添加");

                // 初始化默认值
                model.Status = 1;
                model.CreateBy = _currentOperateUserId;
                model.UpdateBy = _currentOperateUserId;

                // 执行新增
                int dictId = _dal.AddDict(model);
                if (dictId <= 0) return BizResult.Fail("新增字典项失败，数据库无返回");

                return BizResult.Success("新增字典项成功", dictId);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"新增字典项失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 更新字典项
        /// </summary>
        public BizResult UpdateDict(Dict model)
        {
            try
            {
                if (model.DictId <= 0) return BizResult.Fail("字典项ID无效");
                if (string.IsNullOrEmpty(model.DictType)) return BizResult.Fail("所属分类为必填项");
                if (string.IsNullOrEmpty(model.DictCode)) return BizResult.Fail("字典编码为必填项");
                if (string.IsNullOrEmpty(model.DictName)) return BizResult.Fail("字典名称为必填项");
                if (string.IsNullOrEmpty(model.DictValue)) return BizResult.Fail("字典值为必填项");

                // 唯一性校验
                if (_dal.CheckDictCodeExists(model.DictType, model.DictCode, model.DictId))
                    return BizResult.Fail("该分类下字典编码已存在，无法修改");

                // 赋值操作人
                model.UpdateBy = _currentOperateUserId;

                // 执行更新
                bool result = _dal.UpdateDict(model);
                return result ? BizResult.Success("更新字典项成功") : BizResult.Fail("更新字典项失败，数据库无影响");
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"更新字典项失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 批量更新字典项状态
        /// </summary>
        public BizResult BatchUpdateDictStatus(List<int> dictIdList, byte status, int updateBy)
        {
            try
            {
                if (dictIdList == null || dictIdList.Count == 0) return BizResult.Fail("请先选择需要操作的字典项记录");
                if (updateBy <= 0) return BizResult.Fail("操作人信息无效");

                bool result = _dal.BatchUpdateDictStatus(dictIdList, status, updateBy);
                return result ? BizResult.Success($"批量{(status == 1 ? "启用" : "禁用")}成功") : BizResult.Fail("批量操作失败");
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"批量操作失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 删除字典项
        /// </summary>
        public BizResult DeleteDict(int dictId)
        {
            try
            {
                if (dictId <= 0) return BizResult.Fail("字典项ID无效");
                Dict model = _dal.GetDictById(dictId);
                if (model == null) return BizResult.Fail("未找到该字典项信息");

                // 业务校验：启用状态不允许删除
                if (model.Status == 1) return BizResult.Fail("启用状态的字典项不允许直接删除，如需删除请先禁用");

                // 校验引用关系
                DataTable dt = _dal.GetDictReferenceRelation(model.DictType, model.DictCode);
                if (dt != null && dt.Rows.Count > 0)
                    return BizResult.Fail("该字典项已被业务数据引用，无法删除，请先清除引用数据");

                bool result = _dal.DeleteDict(dictId);
                return result ? BizResult.Success("删除字典项成功") : BizResult.Fail("删除字典项失败");
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"删除字典项失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 根据分类编码获取启用的字典项（供全系统下拉框绑定）
        /// </summary>
        public BizResult GetDictByTypeCode(string typeCode)
        {
            try
            {
                if (string.IsNullOrEmpty(typeCode)) return BizResult.Fail("分类编码不能为空");
                DataTable dt = _dal.GetDictByTypeCode(typeCode);
                return BizResult.Success("查询成功", dt, dt.Rows.Count);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"获取字典项失败：{ex.Message}");
            }
        }
        #endregion

        #region 引用关系查询业务
        /// <summary>
        /// 查询字典项引用关系
        /// </summary>
        public BizResult GetDictReferenceRelation(string dictType, string dictCode)
        {
            try
            {
                if (string.IsNullOrEmpty(dictType) || string.IsNullOrEmpty(dictCode))
                    return BizResult.Fail("分类编码和字典编码不能为空");

                DataTable dt = _dal.GetDictReferenceRelation(dictType, dictCode);
                return BizResult.Success("查询成功", dt, dt.Rows.Count);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"查询引用关系失败：{ex.Message}");
            }
        }
        #endregion
    }
}