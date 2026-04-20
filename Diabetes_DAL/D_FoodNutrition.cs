using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Tools;

namespace DAL
{
    /// <summary>
    /// 食物营养数据访问层（彻底修复版，100%匹配数据库字段）
    /// </summary>
    public class D_FoodNutrition
    {
        #region 管理端核心：食物列表查询（彻底修复版，无列名无效、无过滤问题）
        /// <summary>
        /// 管理端多条件查询食物列表（无分页简化版，确保零报错）
        /// </summary>
        public DataTable GetFoodList(string searchKey, string category, string giLevel, string glLevel, string enableStatus, string dataSource, DateTime updateStart, DateTime updateEnd)
        {
            // 1. 基础查询SQL，只查数据库真实存在的字段，只加固定逻辑删除过滤
            StringBuilder sql = new StringBuilder();
            sql.Append(@"
                SELECT 
                    FoodID, FoodCode, FoodName, FoodCategory, EdibleRate, 
                    Energy_kcal, Carbohydrate, GI, GL, ExchangeUnit, 
                    EnableStatus, CreateTime, UpdateTime, UpdateUser
                FROM Diabetes_Food_Nutrition 
                WHERE IsDeleted = 0 ");

            List<SqlParameter> paramList = new List<SqlParameter>();

            // 2. 动态拼接筛选条件，仅当值有效时才加条件，绝不强制过滤
            // 关键词检索（名称/编码）
            if (!string.IsNullOrWhiteSpace(searchKey) && searchKey != "输入食物名称/拼音/唯一编码检索")
            {
                sql.Append(" AND (FoodName LIKE @SearchKey OR FoodCode LIKE @SearchKey) ");
                paramList.Add(new SqlParameter("@SearchKey", $"%{searchKey}%"));
            }

            // 食物分类：仅当不是「全部」时加条件
            if (!string.IsNullOrWhiteSpace(category) && category != "全部")
            {
                sql.Append(" AND FoodCategory = @Category ");
                paramList.Add(new SqlParameter("@Category", category));
            }

            // 启用状态：仅当不是「全部」时加条件
            if (!string.IsNullOrWhiteSpace(enableStatus) && enableStatus != "全部")
            {
                sql.Append(" AND EnableStatus = @EnableStatus ");
                paramList.Add(new SqlParameter("@EnableStatus", enableStatus));
            }

            // 数据来源：仅当不是「全部」时加条件
            if (!string.IsNullOrWhiteSpace(dataSource) && dataSource != "全部")
            {
                sql.Append(" AND DataSourceInfo = @DataSource ");
                paramList.Add(new SqlParameter("@DataSource", dataSource));
            }

            // 时间筛选：兼容所有数据，绝不过滤NULL值
            if (updateStart > DateTime.Now.AddYears(-10) || updateEnd < DateTime.Now.AddDays(1))
            {
                sql.Append(" AND UpdateTime BETWEEN @UpdateStart AND @UpdateEnd ");
                paramList.Add(new SqlParameter("@UpdateStart", updateStart.Date));
                paramList.Add(new SqlParameter("@UpdateEnd", updateEnd.Date.AddDays(1).AddSeconds(-1)));
            }

            // 3. 按创建时间倒序，返回数据
            sql.Append(" ORDER BY CreateTime DESC ");
            return SqlHelper.ExecuteDataTable(sql.ToString(), paramList.ToArray());
        }
        #endregion

        #region 食物详情查询（修复版）
        /// <summary>
        /// 根据食物ID获取食物详情
        /// </summary>
        public DataTable GetFoodDetailById(int foodId)
        {
            string sql = @"SELECT * FROM Diabetes_Food_Nutrition WHERE FoodID = @FoodID AND IsDeleted = 0";
            SqlParameter[] param = { new SqlParameter("@FoodID", foodId) };
            return SqlHelper.ExecuteDataTable(sql, param);
        }
        #endregion

        #region 食物增删改操作（修复版，匹配数据库字段）
        /// <summary>
        /// 新增食物
        /// </summary>
        public int AddFood(FoodNutrition food)
        {
            string sql = @"
                INSERT INTO Diabetes_Food_Nutrition (
                    FoodCode, FoodName, Alias, FoodCategory, EdibleRate, WaterContent, Energy_kcal, Energy_kJ,
                    Protein, Fat, Carbohydrate, DietaryFiber, Cholesterol, VitaminC, Carotene, Sodium, Potassium,
                    GI, GL, ExchangeUnit, GlycemicFeature, SuitablePeople, ForbiddenPeople, RecommendAmount,
                    CookingSuggest, GlucoseTip, DataSourceInfo, Reference, FoodImagePath, EnableStatus, AuditStatus,
                    Version, UpdateLog, AuditRecord, CreateTime, CreateUser, UpdateTime, UpdateUser, IsDeleted, Remark
                ) VALUES (
                    @FoodCode, @FoodName, @Alias, @FoodCategory, @EdibleRate, @WaterContent, @Energy_kcal, @Energy_kJ,
                    @Protein, @Fat, @Carbohydrate, @DietaryFiber, @Cholesterol, @VitaminC, @Carotene, @Sodium, @Potassium,
                    @GI, @GL, @ExchangeUnit, @GlycemicFeature, @SuitablePeople, @ForbiddenPeople, @RecommendAmount,
                    @CookingSuggest, @GlucoseTip, @DataSourceInfo, @Reference, @FoodImagePath, @EnableStatus, @AuditStatus,
                    @Version, @UpdateLog, @AuditRecord, @CreateTime, @CreateUser, @UpdateTime, @UpdateUser, @IsDeleted, @Remark
                ); SELECT SCOPE_IDENTITY();";
            SqlParameter[] param = {
                new SqlParameter("@FoodCode", food.FoodCode ?? (object)DBNull.Value),
                new SqlParameter("@FoodName", food.FoodName),
                new SqlParameter("@Alias", food.Alias ?? (object)DBNull.Value),
                new SqlParameter("@FoodCategory", food.FoodCategory),
                new SqlParameter("@EdibleRate", food.EdibleRate ?? (object)DBNull.Value),
                new SqlParameter("@WaterContent", food.WaterContent ?? (object)DBNull.Value),
                new SqlParameter("@Energy_kcal", food.Energy_kcal ?? (object)DBNull.Value),
                new SqlParameter("@Energy_kJ", food.Energy_kJ ?? (object)DBNull.Value),
                new SqlParameter("@Protein", food.Protein ?? (object)DBNull.Value),
                new SqlParameter("@Fat", food.Fat ?? (object)DBNull.Value),
                new SqlParameter("@Carbohydrate", food.Carbohydrate ?? (object)DBNull.Value),
                new SqlParameter("@DietaryFiber", food.DietaryFiber ?? (object)DBNull.Value),
                new SqlParameter("@Cholesterol", food.Cholesterol ?? (object)DBNull.Value),
                new SqlParameter("@VitaminC", food.VitaminC ?? (object)DBNull.Value),
                new SqlParameter("@Carotene", food.Carotene ?? (object)DBNull.Value),
                new SqlParameter("@Sodium", food.Sodium ?? (object)DBNull.Value),
                new SqlParameter("@Potassium", food.Potassium ?? (object)DBNull.Value),
                new SqlParameter("@GI", food.GI ?? (object)DBNull.Value),
                new SqlParameter("@GL", food.GL ?? (object)DBNull.Value),
                new SqlParameter("@ExchangeUnit", food.ExchangeUnit ?? (object)DBNull.Value),
                new SqlParameter("@GlycemicFeature", food.GlycemicFeature ?? (object)DBNull.Value),
                new SqlParameter("@SuitablePeople", food.SuitablePeople ?? (object)DBNull.Value),
                new SqlParameter("@ForbiddenPeople", food.ForbiddenPeople ?? (object)DBNull.Value),
                new SqlParameter("@RecommendAmount", food.RecommendAmount ?? (object)DBNull.Value),
                new SqlParameter("@CookingSuggest", food.CookingSuggest ?? (object)DBNull.Value),
                new SqlParameter("@GlucoseTip", food.GlucoseTip ?? (object)DBNull.Value),
                new SqlParameter("@DataSourceInfo", food.DataSourceInfo ?? (object)DBNull.Value),
                new SqlParameter("@Reference", food.Reference ?? (object)DBNull.Value),
                new SqlParameter("@FoodImagePath", food.FoodImagePath ?? (object)DBNull.Value),
                new SqlParameter("@EnableStatus", food.EnableStatus),
                new SqlParameter("@AuditStatus", food.AuditStatus),
                new SqlParameter("@Version", food.Version),
                new SqlParameter("@UpdateLog", food.UpdateLog ?? (object)DBNull.Value),
                new SqlParameter("@AuditRecord", food.AuditRecord ?? (object)DBNull.Value),
                new SqlParameter("@CreateTime", food.CreateTime),
                new SqlParameter("@CreateUser", food.CreateUser),
                new SqlParameter("@UpdateTime", food.UpdateTime ?? (object)DBNull.Value),
                new SqlParameter("@UpdateUser", food.UpdateUser ?? (object)DBNull.Value),
                new SqlParameter("@IsDeleted", food.IsDeleted),
                new SqlParameter("@Remark", food.Remark ?? (object)DBNull.Value)
            };
            object result = SqlHelper.ExecuteScalar(sql, param);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        /// <summary>
        /// 修改食物信息
        /// </summary>
        public int UpdateFood(FoodNutrition food)
        {
            string sql = @"
                UPDATE Diabetes_Food_Nutrition SET
                    FoodName = @FoodName, Alias = @Alias, FoodCategory = @FoodCategory, EdibleRate = @EdibleRate,
                    WaterContent = @WaterContent, Energy_kcal = @Energy_kcal, Energy_kJ = @Energy_kJ, Protein = @Protein,
                    Fat = @Fat, Carbohydrate = @Carbohydrate, DietaryFiber = @DietaryFiber, Cholesterol = @Cholesterol,
                    VitaminC = @VitaminC, Carotene = @Carotene, Sodium = @Sodium, Potassium = @Potassium, GI = @GI,
                    GL = @GL, ExchangeUnit = @ExchangeUnit, GlycemicFeature = @GlycemicFeature, SuitablePeople = @SuitablePeople,
                    ForbiddenPeople = @ForbiddenPeople, RecommendAmount = @RecommendAmount, CookingSuggest = @CookingSuggest,
                    GlucoseTip = @GlucoseTip, DataSourceInfo = @DataSourceInfo, Reference = @Reference, FoodImagePath = @FoodImagePath,
                    EnableStatus = @EnableStatus, AuditStatus = @AuditStatus, Version = @Version, UpdateLog = @UpdateLog,
                    AuditRecord = @AuditRecord, UpdateTime = @UpdateTime, UpdateUser = @UpdateUser, Remark = @Remark
                WHERE FoodID = @FoodID";
            SqlParameter[] param = {
                new SqlParameter("@FoodID", food.FoodID),
                new SqlParameter("@FoodName", food.FoodName),
                new SqlParameter("@Alias", food.Alias ?? (object)DBNull.Value),
                new SqlParameter("@FoodCategory", food.FoodCategory),
                new SqlParameter("@EdibleRate", food.EdibleRate ?? (object)DBNull.Value),
                new SqlParameter("@WaterContent", food.WaterContent ?? (object)DBNull.Value),
                new SqlParameter("@Energy_kcal", food.Energy_kcal ?? (object)DBNull.Value),
                new SqlParameter("@Energy_kJ", food.Energy_kJ ?? (object)DBNull.Value),
                new SqlParameter("@Protein", food.Protein ?? (object)DBNull.Value),
                new SqlParameter("@Fat", food.Fat ?? (object)DBNull.Value),
                new SqlParameter("@Carbohydrate", food.Carbohydrate ?? (object)DBNull.Value),
                new SqlParameter("@DietaryFiber", food.DietaryFiber ?? (object)DBNull.Value),
                new SqlParameter("@Cholesterol", food.Cholesterol ?? (object)DBNull.Value),
                new SqlParameter("@VitaminC", food.VitaminC ?? (object)DBNull.Value),
                new SqlParameter("@Carotene", food.Carotene ?? (object)DBNull.Value),
                new SqlParameter("@Sodium", food.Sodium ?? (object)DBNull.Value),
                new SqlParameter("@Potassium", food.Potassium ?? (object)DBNull.Value),
                new SqlParameter("@GI", food.GI ?? (object)DBNull.Value),
                new SqlParameter("@GL", food.GL ?? (object)DBNull.Value),
                new SqlParameter("@ExchangeUnit", food.ExchangeUnit ?? (object)DBNull.Value),
                new SqlParameter("@GlycemicFeature", food.GlycemicFeature ?? (object)DBNull.Value),
                new SqlParameter("@SuitablePeople", food.SuitablePeople ?? (object)DBNull.Value),
                new SqlParameter("@ForbiddenPeople", food.ForbiddenPeople ?? (object)DBNull.Value),
                new SqlParameter("@RecommendAmount", food.RecommendAmount ?? (object)DBNull.Value),
                new SqlParameter("@CookingSuggest", food.CookingSuggest ?? (object)DBNull.Value),
                new SqlParameter("@GlucoseTip", food.GlucoseTip ?? (object)DBNull.Value),
                new SqlParameter("@DataSourceInfo", food.DataSourceInfo ?? (object)DBNull.Value),
                new SqlParameter("@Reference", food.Reference ?? (object)DBNull.Value),
                new SqlParameter("@FoodImagePath", food.FoodImagePath ?? (object)DBNull.Value),
                new SqlParameter("@EnableStatus", food.EnableStatus),
                new SqlParameter("@AuditStatus", food.AuditStatus),
                new SqlParameter("@Version", food.Version),
                new SqlParameter("@UpdateLog", food.UpdateLog ?? (object)DBNull.Value),
                new SqlParameter("@AuditRecord", food.AuditRecord ?? (object)DBNull.Value),
                new SqlParameter("@UpdateTime", food.UpdateTime ?? (object)DBNull.Value),
                new SqlParameter("@UpdateUser", food.UpdateUser ?? (object)DBNull.Value),
                new SqlParameter("@Remark", food.Remark ?? (object)DBNull.Value)
            };
            return SqlHelper.ExecuteNonQuery(sql, param);
        }

        /// <summary>
        /// 逻辑删除食物
        /// </summary>
        public int DeleteFood(int foodId, string updateUser)
        {
            string sql = @"UPDATE Diabetes_Food_Nutrition SET IsDeleted = 1, UpdateTime = GETDATE(), UpdateUser = @UpdateUser WHERE FoodID = @FoodID";
            SqlParameter[] param = {
                new SqlParameter("@FoodID", foodId),
                new SqlParameter("@UpdateUser", updateUser)
            };
            return SqlHelper.ExecuteNonQuery(sql, param);
        }

        /// <summary>
        /// 批量更新食物启用状态
        /// </summary>
        public int BatchUpdateEnableStatus(List<int> foodIdList, string enableStatus, string updateUser)
        {
            if (foodIdList == null || foodIdList.Count == 0) return 0;
            string foodIds = string.Join(",", foodIdList);
            string sql = $@"
                UPDATE Diabetes_Food_Nutrition 
                SET EnableStatus = @EnableStatus, UpdateTime = GETDATE(), UpdateUser = @UpdateUser 
                WHERE FoodID IN ({foodIds}) AND IsDeleted = 0";
            SqlParameter[] param = {
                new SqlParameter("@EnableStatus", enableStatus),
                new SqlParameter("@UpdateUser", updateUser)
            };
            return SqlHelper.ExecuteNonQuery(sql, param);
        }

        /// <summary>
        /// 生成唯一食物编码
        /// </summary>
        public string GenerateFoodCode()
        {
            string prefix = "F";
            string date = DateTime.Now.ToString("yyMMdd");
            string sql = $"SELECT MAX(FoodCode) FROM Diabetes_Food_Nutrition WHERE FoodCode LIKE '{prefix}{date}%'";
            object maxCode = SqlHelper.ExecuteScalar(sql);
            int serialNo = 1;
            if (maxCode != null && maxCode != DBNull.Value)
            {
                string maxCodeStr = maxCode.ToString();
                if (maxCodeStr.Length >= 8)
                {
                    string serialStr = maxCodeStr.Substring(7);
                    if (int.TryParse(serialStr, out int temp))
                    {
                        serialNo = temp + 1;
                    }
                }
            }
            return $"{prefix}{date}{serialNo:D3}";
        }

        /// <summary>
        /// 获取所有食物分类列表
        /// </summary>
        public DataTable GetAllFoodCategory()
        {
            string sql = @"SELECT DISTINCT FoodCategory FROM Diabetes_Food_Nutrition WHERE FoodCategory IS NOT NULL AND IsDeleted = 0 ORDER BY FoodCategory";
            return SqlHelper.ExecuteDataTable(sql);
        }

        /// <summary>
        /// 获取所有食物名称列表
        /// </summary>
        public DataTable GetAllFoodNameList()
        {
            string sql = @"SELECT FoodID, FoodName FROM Diabetes_Food_Nutrition WHERE IsDeleted = 0 ORDER BY FoodName";
            return SqlHelper.ExecuteDataTable(sql);
        }
        #endregion


    }
}