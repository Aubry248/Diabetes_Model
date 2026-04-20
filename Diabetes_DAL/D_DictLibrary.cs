using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Tools;

namespace DAL
{
    /// <summary>
    /// 字典库管理数据访问层
    /// </summary>
    public class D_DictLibrary
    {
        #region 字典分类相关操作
        /// <summary>
        /// 条件查询字典分类列表（适配实际表结构）
        /// </summary>
        public DataTable GetDictTypeList(string searchKey)
        {
            string sql = @"
SELECT type_id, type_code, type_name, description
FROM t_dict_type WHERE 1=1";
            List<SqlParameter> paramList = new List<SqlParameter>();

            // 关键词筛选（分类名称/编码）
            if (!string.IsNullOrEmpty(searchKey))
            {
                sql += " AND (type_name LIKE @SearchKey OR type_code LIKE @SearchKey)";
                paramList.Add(new SqlParameter("@SearchKey", $"%{searchKey}%"));
            }

            sql += " ORDER BY type_id ASC";
            return SqlHelper.ExecuteDataTable(sql, paramList.ToArray());
        }

        /// <summary>
        /// 根据ID获取字典分类详情（适配实际表结构）
        /// </summary>
        public DictType GetDictTypeById(int typeId)
        {
            string sql = "SELECT type_id, type_code, type_name, description FROM t_dict_type WHERE type_id = @TypeId";
            SqlParameter[] param = { new SqlParameter("@TypeId", typeId) };
            DataTable dt = SqlHelper.ExecuteDataTable(sql, param);
            if (dt == null || dt.Rows.Count == 0) return null;
            DataRow dr = dt.Rows[0];
            return new DictType
            {
                TypeId = Convert.ToInt32(dr["type_id"]),
                TypeCode = dr["type_code"].ToString(),
                TypeName = dr["type_name"].ToString(),
                Description = dr["description"].ToString()
            };
        }

        /// <summary>
        /// 校验分类编码是否唯一
        /// </summary>
        public bool CheckDictTypeCodeExists(string typeCode, int excludeTypeId = 0)
        {
            string sql = "SELECT COUNT(1) FROM t_dict_type WHERE type_code = @TypeCode";
            List<SqlParameter> paramList = new List<SqlParameter> { new SqlParameter("@TypeCode", typeCode) };
            if (excludeTypeId > 0)
            {
                sql += " AND type_id != @ExcludeTypeId";
                paramList.Add(new SqlParameter("@ExcludeTypeId", excludeTypeId));
            }
            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql, paramList.ToArray())) > 0;
        }

        /// <summary>
        /// 新增字典分类（适配实际表结构）
        /// </summary>
        public int AddDictType(DictType model)
        {
            string sql = @"
INSERT INTO t_dict_type (type_code, type_name, description)
VALUES (@TypeCode, @TypeName, @Description);
SELECT SCOPE_IDENTITY();";
            SqlParameter[] param = {
        new SqlParameter("@TypeCode", model.TypeCode),
        new SqlParameter("@TypeName", model.TypeName),
        new SqlParameter("@Description", model.Description)
    };
            object obj = SqlHelper.ExecuteScalar(sql, param);
            return obj == null ? 0 : Convert.ToInt32(obj);
        }

        /// <summary>
        /// 更新字典分类（适配实际表结构）
        /// </summary>
        public bool UpdateDictType(DictType model)
        {
            string sql = @"
UPDATE t_dict_type SET 
type_code = @TypeCode, type_name = @TypeName, description = @Description
WHERE type_id = @TypeId";
            SqlParameter[] param = {
        new SqlParameter("@TypeId", model.TypeId),
        new SqlParameter("@TypeCode", model.TypeCode),
        new SqlParameter("@TypeName", model.TypeName),
        new SqlParameter("@Description", model.Description)
    };
            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }

        /// <summary>
        /// 删除字典分类（适配实际表结构）
        /// </summary>
        public bool DeleteDictType(int typeId)
        {
            string sql = "DELETE FROM t_dict_type WHERE type_id = @TypeId";
            SqlParameter[] param = { new SqlParameter("@TypeId", typeId) };
            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }
        #endregion

        #region 字典项相关操作
        /// <summary>
        /// 条件查询字典项列表
        /// </summary>
        public DataTable GetDictList(string dictType, string enableStatus, string searchKey)
        {
            string sql = @"
SELECT d.dict_id, d.dict_type, d.dict_code, d.dict_name, d.dict_value, d.dict_desc, d.source, d.sort, 
d.status, d.create_time, d.create_by, t.type_name AS dict_type_name
FROM t_dict d
LEFT JOIN t_dict_type t ON d.dict_type = t.type_code
WHERE 1=1";
            List<SqlParameter> paramList = new List<SqlParameter>();

            // 分类筛选
            if (!string.IsNullOrEmpty(dictType) && dictType != "全部")
            {
                sql += " AND d.dict_type = @DictType";
                paramList.Add(new SqlParameter("@DictType", dictType));
            }

            // 启用状态筛选
            if (enableStatus != "全部")
            {
                sql += " AND d.status = @Status";
                paramList.Add(new SqlParameter("@Status", enableStatus == "启用" ? 1 : 0));
            }

            // 关键词筛选
            if (!string.IsNullOrEmpty(searchKey))
            {
                sql += " AND (d.dict_name LIKE @SearchKey OR d.dict_code LIKE @SearchKey OR d.dict_value LIKE @SearchKey)";
                paramList.Add(new SqlParameter("@SearchKey", $"%{searchKey}%"));
            }

            sql += " ORDER BY d.dict_type ASC, d.sort ASC, d.create_time DESC";
            return SqlHelper.ExecuteDataTable(sql, paramList.ToArray());
        }

        /// <summary>
        /// 根据ID获取字典项详情
        /// </summary>
        public Dict GetDictById(int dictId)
        {
            string sql = "SELECT * FROM t_dict WHERE dict_id = @DictId";
            SqlParameter[] param = { new SqlParameter("@DictId", dictId) };
            DataTable dt = SqlHelper.ExecuteDataTable(sql, param);
            if (dt == null || dt.Rows.Count == 0) return null;

            DataRow dr = dt.Rows[0];
            return new Dict
            {
                DictId = Convert.ToInt32(dr["dict_id"]),
                DictType = dr["dict_type"].ToString(),
                DictCode = dr["dict_code"].ToString(),
                DictName = dr["dict_name"].ToString(),
                DictValue = dr["dict_value"].ToString(),
                DictDesc = dr["dict_desc"].ToString(),
                Source = dr["source"].ToString(),
                Sort = Convert.ToInt32(dr["sort"]),
                Status = Convert.ToByte(dr["status"]),
                ParentDictId = dr["parent_dict_id"] == DBNull.Value ? null : (int?)Convert.ToInt32(dr["parent_dict_id"]),
                TypeId = dr["type_id"] == DBNull.Value ? null : (int?)Convert.ToInt32(dr["type_id"]),
                CreateBy = Convert.ToInt32(dr["create_by"]),
                UpdateBy = Convert.ToInt32(dr["update_by"]),
                DataVersion = Convert.ToInt32(dr["data_version"]),
                CreateTime = Convert.ToDateTime(dr["create_time"]),
                UpdateTime = Convert.ToDateTime(dr["update_time"])
            };
        }

        /// <summary>
        /// 校验字典项编码是否唯一（同分类下）
        /// </summary>
        public bool CheckDictCodeExists(string dictType, string dictCode, int excludeDictId = 0)
        {
            string sql = "SELECT COUNT(1) FROM t_dict WHERE dict_type = @DictType AND dict_code = @DictCode";
            List<SqlParameter> paramList = new List<SqlParameter> {
                new SqlParameter("@DictType", dictType),
                new SqlParameter("@DictCode", dictCode)
            };
            if (excludeDictId > 0)
            {
                sql += " AND dict_id != @ExcludeDictId";
                paramList.Add(new SqlParameter("@ExcludeDictId", excludeDictId));
            }
            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql, paramList.ToArray())) > 0;
        }

        /// <summary>
        /// 新增字典项
        /// </summary>
        public int AddDict(Dict model)
        {
            string sql = @"
INSERT INTO t_dict (dict_type, dict_code, dict_name, dict_value, dict_desc, source, sort, status, parent_dict_id, type_id, create_by, update_by, data_version, create_time, update_time)
VALUES (@DictType, @DictCode, @DictName, @DictValue, @DictDesc, @Source, @Sort, @Status, @ParentDictId, @TypeId, @CreateBy, @UpdateBy, 1, GETDATE(), GETDATE());
SELECT SCOPE_IDENTITY();";
            SqlParameter[] param = {
                new SqlParameter("@DictType", model.DictType),
                new SqlParameter("@DictCode", model.DictCode),
                new SqlParameter("@DictName", model.DictName),
                new SqlParameter("@DictValue", model.DictValue),
                new SqlParameter("@DictDesc", model.DictDesc ?? (object)DBNull.Value),
                new SqlParameter("@Source", model.Source ?? (object)DBNull.Value),
                new SqlParameter("@Sort", model.Sort),
                new SqlParameter("@Status", model.Status),
                new SqlParameter("@ParentDictId", model.ParentDictId ?? (object)DBNull.Value),
                new SqlParameter("@TypeId", model.TypeId ?? (object)DBNull.Value),
                new SqlParameter("@CreateBy", model.CreateBy),
                new SqlParameter("@UpdateBy", model.UpdateBy)
            };
            object obj = SqlHelper.ExecuteScalar(sql, param);
            return obj == null ? 0 : Convert.ToInt32(obj);
        }

        /// <summary>
        /// 更新字典项
        /// </summary>
        public bool UpdateDict(Dict model)
        {
            string sql = @"
UPDATE t_dict SET 
dict_type = @DictType, dict_code = @DictCode, dict_name = @DictName, dict_value = @DictValue,
dict_desc = @DictDesc, source = @Source, sort = @Sort, status = @Status, parent_dict_id = @ParentDictId, type_id = @TypeId,
update_by = @UpdateBy, update_time = GETDATE(), data_version = data_version + 1
WHERE dict_id = @DictId";
            SqlParameter[] param = {
                new SqlParameter("@DictId", model.DictId),
                new SqlParameter("@DictType", model.DictType),
                new SqlParameter("@DictCode", model.DictCode),
                new SqlParameter("@DictName", model.DictName),
                new SqlParameter("@DictValue", model.DictValue),
                new SqlParameter("@DictDesc", model.DictDesc ?? (object)DBNull.Value),
                new SqlParameter("@Source", model.Source ?? (object)DBNull.Value),
                new SqlParameter("@Sort", model.Sort),
                new SqlParameter("@Status", model.Status),
                new SqlParameter("@ParentDictId", model.ParentDictId ?? (object)DBNull.Value),
                new SqlParameter("@TypeId", model.TypeId ?? (object)DBNull.Value),
                new SqlParameter("@UpdateBy", model.UpdateBy)
            };
            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }

        /// <summary>
        /// 批量更新字典项状态
        /// </summary>
        public bool BatchUpdateDictStatus(List<int> dictIdList, byte status, int updateBy)
        {
            if (dictIdList == null || dictIdList.Count == 0) return false;
            string dictIds = string.Join(",", dictIdList);
            string sql = $@"
UPDATE t_dict SET status = @Status, update_by = @UpdateBy, update_time = GETDATE(), data_version = data_version + 1
WHERE dict_id IN ({dictIds})";
            SqlParameter[] param = {
                new SqlParameter("@Status", status),
                new SqlParameter("@UpdateBy", updateBy)
            };
            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }

        /// <summary>
        /// 删除字典项
        /// </summary>
        public bool DeleteDict(int dictId)
        {
            string sql = "DELETE FROM t_dict WHERE dict_id = @DictId AND status = 0";
            SqlParameter[] param = { new SqlParameter("@DictId", dictId) };
            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }

        /// <summary>
        /// 根据分类编码获取启用的字典项（供全系统下拉框绑定）
        /// </summary>
        public DataTable GetDictByTypeCode(string typeCode)
        {
            string sql = @"
SELECT dict_code, dict_name, dict_value FROM t_dict 
WHERE dict_type = @TypeCode AND status = 1 ORDER BY sort ASC";
            SqlParameter[] param = { new SqlParameter("@TypeCode", typeCode) };
            return SqlHelper.ExecuteDataTable(sql, param);
        }
        #endregion

        #region 引用关系查询
        /// <summary>
        /// 查询字典项引用关系
        /// </summary>
        public DataTable GetDictReferenceRelation(string dictType, string dictCode)
        {
            // 适配系统现有表的外键引用关系，可根据实际表扩展
            string sql = @"
-- 字典分类引用
SELECT '字典项' AS reference_table, 'dict_type' AS reference_field, dict_name AS reference_name, dict_id AS reference_id
FROM t_dict WHERE dict_type = @DictType
UNION ALL
-- 权限表引用
SELECT 't_permission' AS reference_table, 'permission_type' AS reference_field, permission_name AS reference_name, permission_id AS reference_id
FROM t_permission WHERE permission_type = @DictCode
UNION ALL
-- 异常表引用
SELECT 't_abnormal' AS reference_table, 'abnormal_type' AS reference_field, abnormal_reason AS reference_name, abnormal_id AS reference_id
FROM t_abnormal WHERE abnormal_type = @DictCode
UNION ALL
-- 饮食表引用
SELECT 't_diet' AS reference_table, 'meal_type' AS reference_field, food_name AS reference_name, diet_id AS reference_id
FROM t_diet WHERE meal_type = @DictCode
UNION ALL
-- 运动表引用
SELECT 't_exercise' AS reference_table, 'exercise_intensity' AS reference_field, exercise_type AS reference_name, exercise_id AS reference_id
FROM t_exercise WHERE exercise_intensity = @DictCode
UNION ALL
-- 血糖表引用
SELECT 't_blood_sugar' AS reference_table, 'measurement_scenario' AS reference_field, blood_sugar_value AS reference_name, blood_sugar_id AS reference_id
FROM t_blood_sugar WHERE measurement_scenario = @DictCode";
            SqlParameter[] param = {
                new SqlParameter("@DictType", dictType),
                new SqlParameter("@DictCode", dictCode)
            };
            return SqlHelper.ExecuteDataTable(sql, param);
        }
        #endregion
    }
}