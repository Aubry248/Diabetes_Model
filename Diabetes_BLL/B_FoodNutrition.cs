using DAL;
using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Tools;

namespace BLL
{
    /// <summary>
    /// 食物营养业务逻辑层（彻底修复版，和DAL完全匹配）
    /// </summary>
    public class B_FoodNutrition
    {
        private readonly D_FoodNutrition _dFood = new D_FoodNutrition();

        #region 核心：获取食物列表（前端直接调用）
        /// <summary>
        /// 多条件查询食物列表，返回BizResult统一结果
        /// </summary>
        public BizResult GetFoodList(string searchKey, string category, string giLevel, string glLevel, string enableStatus, string dataSource, DateTime updateStart, DateTime updateEnd)
        {
            try
            {
                DataTable dt = _dFood.GetFoodList(searchKey, category, giLevel, glLevel, enableStatus, dataSource, updateStart, updateEnd);
                return BizResult.Success($"查询到{dt.Rows.Count}条数据", dt, dt.Rows.Count);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"查询食物列表失败：{ex.Message}");
            }
        }
        #endregion

        #region 食物详情查询
        /// <summary>
        /// 根据ID获取食物详情
        /// </summary>
        public BizResult GetFoodDetailById(int foodId)
        {
            try
            {
                DataTable dt = _dFood.GetFoodDetailById(foodId);
                if (dt.Rows.Count == 0)
                {
                    return BizResult.Fail("未找到该食物数据");
                }

                // 转换为FoodNutrition实体
                DataRow dr = dt.Rows[0];
                FoodNutrition food = new FoodNutrition
                {
                    FoodID = Convert.ToInt32(dr["FoodID"]),
                    FoodCode = dr["FoodCode"].ToString(),
                    FoodName = dr["FoodName"].ToString(),
                    Alias = dr["Alias"].ToString(),
                    FoodCategory = dr["FoodCategory"].ToString(),
                    EdibleRate = dr["EdibleRate"] != DBNull.Value ? Convert.ToDecimal(dr["EdibleRate"]) : (decimal?)null,
                    WaterContent = dr["WaterContent"] != DBNull.Value ? Convert.ToDecimal(dr["WaterContent"]) : (decimal?)null,
                    Energy_kcal = dr["Energy_kcal"] != DBNull.Value ? Convert.ToDecimal(dr["Energy_kcal"]) : (decimal?)null,
                    Energy_kJ = dr["Energy_kJ"] != DBNull.Value ? Convert.ToDecimal(dr["Energy_kJ"]) : (decimal?)null,
                    Protein = dr["Protein"] != DBNull.Value ? Convert.ToDecimal(dr["Protein"]) : (decimal?)null,
                    Fat = dr["Fat"] != DBNull.Value ? Convert.ToDecimal(dr["Fat"]) : (decimal?)null,
                    Carbohydrate = dr["Carbohydrate"] != DBNull.Value ? Convert.ToDecimal(dr["Carbohydrate"]) : (decimal?)null,
                    DietaryFiber = dr["DietaryFiber"] != DBNull.Value ? Convert.ToDecimal(dr["DietaryFiber"]) : (decimal?)null,
                    Cholesterol = dr["Cholesterol"] != DBNull.Value ? Convert.ToDecimal(dr["Cholesterol"]) : (decimal?)null,
                    VitaminC = dr["VitaminC"] != DBNull.Value ? Convert.ToDecimal(dr["VitaminC"]) : (decimal?)null,
                    Carotene = dr["Carotene"] != DBNull.Value ? Convert.ToDecimal(dr["Carotene"]) : (decimal?)null,
                    Sodium = dr["Sodium"] != DBNull.Value ? Convert.ToDecimal(dr["Sodium"]) : (decimal?)null,
                    Potassium = dr["Potassium"] != DBNull.Value ? Convert.ToDecimal(dr["Potassium"]) : (decimal?)null,
                    GI = dr["GI"] != DBNull.Value ? Convert.ToDecimal(dr["GI"]) : (decimal?)null,
                    GL = dr["GL"] != DBNull.Value ? Convert.ToDecimal(dr["GL"]) : (decimal?)null,
                    ExchangeUnit = dr["ExchangeUnit"] != DBNull.Value ? Convert.ToDecimal(dr["ExchangeUnit"]) : (decimal?)null,
                    GlycemicFeature = dr["GlycemicFeature"].ToString(),
                    SuitablePeople = dr["SuitablePeople"].ToString(),
                    ForbiddenPeople = dr["ForbiddenPeople"].ToString(),
                    RecommendAmount = dr["RecommendAmount"].ToString(),
                    CookingSuggest = dr["CookingSuggest"].ToString(),
                    GlucoseTip = dr["GlucoseTip"].ToString(),
                    DataSourceInfo = dr["DataSourceInfo"].ToString(),
                    Reference = dr["Reference"].ToString(),
                    FoodImagePath = dr["FoodImagePath"].ToString(),
                    EnableStatus = dr["EnableStatus"].ToString(),
                    AuditStatus = dr["AuditStatus"].ToString(),
                    Version = dr["Version"].ToString(),
                    UpdateLog = dr["UpdateLog"].ToString(),
                    AuditRecord = dr["AuditRecord"].ToString(),
                    CreateTime = Convert.ToDateTime(dr["CreateTime"]),
                    CreateUser = dr["CreateUser"].ToString(),
                    UpdateTime = dr["UpdateTime"] != DBNull.Value ? Convert.ToDateTime(dr["UpdateTime"]) : (DateTime?)null,
                    UpdateUser = dr["UpdateUser"].ToString(),
                    IsDeleted = Convert.ToInt32(dr["IsDeleted"]),
                    Remark = dr["Remark"].ToString()
                };

                return BizResult.Success("加载成功", food);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"加载食物详情失败：{ex.Message}");
            }
        }
        #endregion

        #region 食物增删改操作
        /// <summary>
        /// 新增食物
        /// </summary>
        public BizResult AddFood(FoodNutrition food, string currentUser)
        {
            try
            {
                // 基础校验
                if (string.IsNullOrWhiteSpace(food.FoodName))
                {
                    return BizResult.Fail("食物名称不能为空");
                }
                if (string.IsNullOrWhiteSpace(food.FoodCategory))
                {
                    return BizResult.Fail("请选择食物分类");
                }

                // 生成唯一编码
                food.FoodCode = _dFood.GenerateFoodCode();
                food.CreateTime = DateTime.Now;
                food.CreateUser = currentUser;
                food.UpdateTime = DateTime.Now;
                food.UpdateUser = currentUser;
                food.IsDeleted = 0;
                food.AuditStatus = "待审核";
                food.Version = "V1.0.0";

                int foodId = _dFood.AddFood(food);
                if (foodId > 0)
                {
                    return BizResult.Success("新增成功，已提交待审核", foodId);
                }
                else
                {
                    return BizResult.Fail("新增失败，请重试");
                }
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"新增食物失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 修改食物
        /// </summary>
        public BizResult UpdateFood(FoodNutrition food, string currentUser, string updateLog)
        {
            try
            {
                if (food.FoodID <= 0)
                {
                    return BizResult.Fail("无效的食物ID");
                }
                if (string.IsNullOrWhiteSpace(updateLog))
                {
                    return BizResult.Fail("请填写更新日志");
                }

                food.UpdateTime = DateTime.Now;
                food.UpdateUser = currentUser;
                food.UpdateLog = updateLog;
                food.AuditStatus = "待审核";
                // 版本号自增，示例：V1.0.0 → V1.0.1
                string[] versionArr = food.Version.Split('.');
                if (versionArr.Length == 3 && int.TryParse(versionArr[2], out int minor))
                {
                    food.Version = $"{versionArr[0]}.{versionArr[1]}.{minor + 1}";
                }

                int row = _dFood.UpdateFood(food);
                if (row > 0)
                {
                    return BizResult.Success("修改成功，已提交待审核");
                }
                else
                {
                    return BizResult.Fail("修改失败，未更新任何数据");
                }
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"修改食物失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 逻辑删除食物
        /// </summary>
        public BizResult DeleteFood(int foodId, string currentUser)
        {
            try
            {
                if (foodId <= 0)
                {
                    return BizResult.Fail("无效的食物ID");
                }

                int row = _dFood.DeleteFood(foodId, currentUser);
                if (row > 0)
                {
                    return BizResult.Success("删除成功");
                }
                else
                {
                    return BizResult.Fail("删除失败，未找到该食物");
                }
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"删除食物失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 批量更新启用状态
        /// </summary>
        public BizResult BatchUpdateEnableStatus(List<int> foodIdList, string enableStatus, string currentUser)
        {
            try
            {
                if (foodIdList == null || foodIdList.Count == 0)
                {
                    return BizResult.Fail("请先选中要操作的食物");
                }

                int row = _dFood.BatchUpdateEnableStatus(foodIdList, enableStatus, currentUser);
                return BizResult.Success($"成功操作{row}条数据", row);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"批量操作失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 重复数据去重
        /// </summary>
        public BizResult DeduplicateFoods()
        {
            try
            {
                DataTable dt = _dFood.GetAllFoodNameList();
                if (dt.Rows.Count == 0)
                {
                    return BizResult.Success("未检测到重复数据");
                }

                // 简单去重逻辑，按名称+分类去重
                int deleteCount = 0;
                Dictionary<string, int> foodDict = new Dictionary<string, int>();
                foreach (DataRow dr in dt.Rows)
                {
                    string key = $"{dr["FoodName"].ToString()}_{dr["FoodCategory"].ToString()}";
                    if (foodDict.ContainsKey(key))
                    {
                        // 重复数据，删除
                        _dFood.DeleteFood(Convert.ToInt32(dr["FoodID"]), "系统");
                        deleteCount++;
                    }
                    else
                    {
                        foodDict.Add(key, Convert.ToInt32(dr["FoodID"]));
                    }
                }

                return BizResult.Success($"去重完成，共清理{deleteCount}条重复数据", deleteCount);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"去重失败：{ex.Message}");
            }
        }
        #endregion

        #region 患者端专用方法（追加补充，不改动原有任何业务方法）
        /// <summary>
        /// 获取低GI推荐食物（GI<55，适合糖尿病患者）
        /// </summary>
        /// <param name="topCount">返回条数，默认10条</param>
        /// <returns>统一BizResult结果，Data为食物列表DataTable</returns>
        public BizResult GetLowGIFoods(int topCount = 0)
        {
            try
            {
                string topClause = topCount > 0 ? $"TOP {topCount}" : string.Empty;
                string sql = $@"
        SELECT {topClause} 
            FoodID, FoodCode, FoodName, FoodCategory, GI, Energy_kcal, Carbohydrate
        FROM Diabetes_Food_Nutrition 
        WHERE GI < 55 AND GI > 0 
            AND EnableStatus = '启用' 
            AND IsDeleted = 0
        ORDER BY GI ASC";

                DataTable dt = SqlHelper.ExecuteDataTable(sql);
                return BizResult.Success($"查询到{dt.Rows.Count}条低GI推荐食物", dt, dt.Rows.Count);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"获取低GI食物失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 获取所有食物分类列表
        /// </summary>
        /// <returns>统一BizResult结果，Data为分类列表DataTable</returns>
        public BizResult GetAllFoodCategory()
        {
            try
            {
                string sql = @"
        SELECT DISTINCT FoodCategory 
        FROM Diabetes_Food_Nutrition 
        WHERE IsDeleted = 0 
        ORDER BY FoodCategory ASC";

                DataTable dt = SqlHelper.ExecuteDataTable(sql);
                return BizResult.Success($"加载到{dt.Rows.Count}个食物分类", dt, dt.Rows.Count);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"加载食物分类失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 多条件筛选食物列表（患者端查询用，完全匹配前端调用参数）
        /// </summary>
        /// <param name="category">食物分类，传“全部”则不筛选分类</param>
        /// <param name="minGi">GI最小值</param>
        /// <param name="maxGi">GI最大值</param>
        /// <param name="maxEnergy">热量上限，传0则不筛选热量</param>
        /// <returns>统一BizResult结果，Data为筛选后的食物列表DataTable</returns>
        public BizResult GetFoodsByFilter(string category, decimal minGi, decimal maxGi, decimal maxEnergy)
        {
            try
            {
                // 基础固定筛选条件
                string sql = @"
        SELECT 
            FoodID, FoodCode, FoodName, FoodCategory, GI, Energy_kcal, Carbohydrate
        FROM Diabetes_Food_Nutrition 
        WHERE IsDeleted = 0 AND EnableStatus = '启用' ";

                // 动态拼接筛选条件
                if (!string.IsNullOrWhiteSpace(category) && category != "全部")
                {
                    sql += " AND FoodCategory = @Category ";
                }
                sql += " AND GI >= @MinGi AND GI <= @MaxGi ";
                if (maxEnergy > 0)
                {
                    sql += " AND Energy_kcal <= @MaxEnergy ";
                }
                sql += " ORDER BY GI ASC ";

                // 构建SQL参数，防注入
                List<SqlParameter> paramList = new List<SqlParameter>
        {
            new SqlParameter("@MinGi", minGi),
            new SqlParameter("@MaxGi", maxGi)
        };
                if (!string.IsNullOrWhiteSpace(category) && category != "全部")
                {
                    paramList.Add(new SqlParameter("@Category", category));
                }
                if (maxEnergy > 0)
                {
                    paramList.Add(new SqlParameter("@MaxEnergy", maxEnergy));
                }

                DataTable dt = SqlHelper.ExecuteDataTable(sql, paramList.ToArray());
                return BizResult.Success($"查询到{dt.Rows.Count}条符合条件的食物", dt, dt.Rows.Count);
            }
            catch (Exception ex)
            {
                return BizResult.Fail($"筛选食物失败：{ex.Message}");
            }
        }
        #endregion
    }
}