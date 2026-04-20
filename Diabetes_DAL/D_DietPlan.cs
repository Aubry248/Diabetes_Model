using System.Data;
using System.Data.SqlClient;
using Tools;

namespace DAL
{
    /// <summary>
    /// 饮食方案数据访问层
    /// </summary>
    public class D_DietPlan
    {
        /// <summary>
        /// 随机获取指定分类、指定数量的低GI食物
        /// </summary>
        /// <param name="category">食物分类</param>
        /// <param name="count">获取数量</param>
        /// <param name="maxCalorie">最大热量限制</param>
        /// <returns>食物数据表</returns>
        public DataTable GetRandomLowGIFoods(string category, int count, decimal maxCalorie)
        {
            string sql = $@"
                SELECT TOP {count} * 
                FROM Diabetes_Food_Nutrition 
                WHERE GI < 55 AND GI > 0 AND FoodCategory = @Category AND Energy_kcal <= @MaxCalorie
                ORDER BY NEWID()";
            SqlParameter[] param = {
                new SqlParameter("@Category", category),
                new SqlParameter("@MaxCalorie", maxCalorie)
            };
            return SqlHelper.ExecuteDataTable(sql, param);
        }
    }
}