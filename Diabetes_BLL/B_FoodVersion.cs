using BLL;
using DAL;
using Model;
using Newtonsoft.Json;
using System;
using System.Data;

/// <summary>
/// 食物版本业务逻辑层
/// </summary>
public class B_FoodVersion
{
    private readonly D_FoodVersion _dFoodVersion = new D_FoodVersion();
    private readonly D_FoodNutrition _dFoodNutrition = new D_FoodNutrition();

    #region 版本列表查询
    /// <summary>
    /// 查询食物版本历史列表
    /// </summary>
    public BizResult GetVersionList(int foodId)
    {
        try
        {
            if (foodId <= 0)
                return BizResult.Fail("食物ID参数非法");

            DataTable dt = _dFoodVersion.GetVersionListByFoodId(foodId);
            return BizResult.Success(data: dt);
        }
        catch (Exception ex)
        {
            return BizResult.Fail($"查询版本列表失败：{ex.Message}");
        }
    }
    #endregion

    #region 版本回滚
    /// <summary>
    /// 版本回滚
    /// </summary>
    public BizResult RollbackVersion(int versionId, string currentUser)
    {
        try
        {
            if (versionId <= 0)
                return BizResult.Fail("版本ID参数非法");
            if (string.IsNullOrWhiteSpace(currentUser))
                return BizResult.Fail("当前登录用户信息异常");

            // 获取版本快照
            DataTable dt = _dFoodVersion.GetVersionById(versionId);
            if (dt == null || dt.Rows.Count == 0)
                return BizResult.Fail("未找到对应版本记录");
            DataRow dr = dt.Rows[0];
            int foodId = Convert.ToInt32(dr["FoodID"]);
            string snapshot = dr["FoodDataSnapshot"].ToString();
            string oldVersion = dr["Version"].ToString();
            string updateContent = dr["UpdateContent"].ToString();

            if (string.IsNullOrWhiteSpace(snapshot))
                return BizResult.Fail("该版本无数据快照，无法回滚");

            // 反序列化快照数据
            FoodNutrition rollbackFood = JsonConvert.DeserializeObject<FoodNutrition>(snapshot);
            if (rollbackFood == null)
                return BizResult.Fail("版本数据解析失败");

            // 获取当前最新版本
            var currentResult = new B_FoodNutrition().GetFoodDetailById(foodId);
            if (!string.IsNullOrWhiteSpace(snapshot))
                return BizResult.Fail($"当前食物数据异常：{currentResult.Message}");
            FoodNutrition currentFood = currentResult.Data as FoodNutrition;

            // 生成新版本号
            Version currentVersion = new Version(currentFood.Version);
            string newVersion = $"{currentVersion.Major}.{currentVersion.Minor}.{currentVersion.Build + 1}";

            // 更新回滚后的系统字段
            rollbackFood.FoodID = foodId;
            rollbackFood.Version = newVersion;
            rollbackFood.UpdateLog = $"版本回滚：从{currentFood.Version}回滚至{oldVersion}，原更新内容：{updateContent}";
            rollbackFood.AuditStatus = "待审核";
            rollbackFood.UpdateTime = DateTime.Now;
            rollbackFood.UpdateUser = currentUser;

            // 更新食物主表
            int row = _dFoodNutrition.UpdateFood(rollbackFood);
            if (row <= 0)
                return BizResult.Fail("版本回滚失败，更新数据异常");

            // 生成新版本记录
            FoodVersion newVersionRecord = new FoodVersion
            {
                FoodID = foodId,
                Version = newVersion,
                UpdateTime = DateTime.Now,
                UpdateUser = currentUser,
                UpdateContent = rollbackFood.UpdateLog,
                AuditStatus = "待审核",
                FoodDataSnapshot = JsonConvert.SerializeObject(rollbackFood)
            };
            _dFoodVersion.AddVersionRecord(newVersionRecord);

            // 生成审核记录
            var auditResult = new B_FoodAudit().AddAuditRecord(new FoodAudit
            {
                FoodID = foodId,
                FoodCode = rollbackFood.FoodCode,
                FoodName = rollbackFood.FoodName,
                Uploader = currentUser,
                UploadTime = DateTime.Now,
                AuditStatus = "待审核",
                Version = newVersion,
                Remark = rollbackFood.UpdateLog
            });

            return BizResult.Success($"版本回滚成功，已生成新版本{newVersion}，请提交审核", newVersion);
        }
        catch (Exception ex)
        {
            return BizResult.Fail($"版本回滚失败：{ex.Message}");
        }
    }
    #endregion
}