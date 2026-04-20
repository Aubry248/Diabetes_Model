using DAL;
using Model;
using System;
using System.Data;

/// <summary>
/// 用户健康业务逻辑层
/// </summary>
public class B_UserHealth
{
    private readonly D_UserHealth _dUserHealth = new D_UserHealth();

    /// <summary>
    /// 获取用户健康信息
    /// </summary>
    public BizResult GetUserHealthInfo(int userId)
    {
        try
        {
            DataTable dt = _dUserHealth.GetUserHealthInfo(userId);
            UserHealthInfo userInfo = new UserHealthInfo { UserId = userId };

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                userInfo.AvgFastingBloodSugar = Convert.ToDecimal(dr["AvgFastingBloodSugar"]);
                userInfo.AvgPostprandialBloodSugar = Convert.ToDecimal(dr["AvgPostprandialBloodSugar"]);
                userInfo.DiseaseDurationYears = Convert.ToDecimal(dr["DiseaseDurationYears"]);
                userInfo.HasMedicineRecord = Convert.ToBoolean(dr["HasMedicineRecord"]);
                userInfo.Weight = Convert.ToDecimal(dr["Weight"]);
                userInfo.Height = Convert.ToDecimal(dr["Height"]);

                string complications = dr["DiabetesComplications"].ToString();
                userInfo.HasNephropathy = complications.Contains("肾病");
                userInfo.HasCardiovascularDisease = complications.Contains("心血管");
            }
            return BizResult.Success(data: userInfo);
        }
        catch (Exception ex)
        {
            return BizResult.Fail($"获取用户健康信息失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 生成个性化饮食建议
    /// </summary>
    public BizResult GeneratePersonalizedDietSuggestion(int userId)
    {
        try
        {
            var userResult = GetUserHealthInfo(userId);
            if (!userResult.IsSuccess)
                return BizResult.Fail(userResult.Message);

            UserHealthInfo userInfo = userResult.Data as UserHealthInfo;
            // 基础通用建议（与截图完全一致）
            string baseSuggestion = "1. 优先选择低GI食物，控制每日碳水化合物摄入量；\r\n" +
                                    "2. 三餐定时定量，避免暴饮暴食；\r\n" +
                                    "3. 多吃绿叶蔬菜，减少精制米面、含糖饮料摄入；\r\n" +
                                    "4. 烹饪方式优先选择蒸、煮、清炒，避免油炸、红烧。\r\n\r\n";

            // 个性化建议拼接
            string personalizedSuggestion = "【个性化饮食建议】\r\n";
            if (userInfo.AvgFastingBloodSugar > 6.1m || userInfo.AvgPostprandialBloodSugar > 7.8m)
            {
                personalizedSuggestion += "• 您近期血糖水平偏高，**建议增加每日膳食纤维摄入量≥25g**，多吃芹菜、菠菜、西兰花等绿叶蔬菜，延缓血糖上升；\r\n";
                personalizedSuggestion += "• 严格控制精制碳水摄入，**主食替换为全谷物、杂豆类**，占比不低于主食总量的1/3；\r\n";
            }
            if (userInfo.HasNephropathy)
            {
                personalizedSuggestion += "• 您合并糖尿病肾病，**需严格限制蛋白质摄入，每日0.6-0.8g/kg体重**，优先选择优质动物蛋白，避免植物蛋白过量摄入；\r\n";
                personalizedSuggestion += "• 严格限制钠盐摄入，每日食盐量≤3g，避免腌制食品、加工肉类；\r\n";
            }
            if (userInfo.HasCardiovascularDisease)
            {
                personalizedSuggestion += "• 您合并心血管疾病，**严格限制脂肪摄入，每日脂肪供能比≤25%**，避免动物内脏、肥肉、油炸食品；\r\n";
                personalizedSuggestion += "• 增加不饱和脂肪酸摄入，适量食用深海鱼、坚果，每日坚果摄入量≤25g；\r\n";
            }
            if (userInfo.DiseaseDurationYears >= 10)
            {
                personalizedSuggestion += "• 您患病年限较长，**建议定期监测餐后血糖波动**，少食多餐，避免单次大量进食导致血糖骤升；\r\n";
            }
            if (userInfo.HasMedicineRecord)
            {
                personalizedSuggestion += "• 您正在使用降糖药物，**需保证三餐规律，避免漏餐导致低血糖**，随身携带糖果等应急食品；\r\n";
            }

            if (personalizedSuggestion == "【个性化饮食建议】\r\n")
                return BizResult.Success(data: baseSuggestion);

            return BizResult.Success(data: baseSuggestion + personalizedSuggestion);
        }
        catch (Exception )
        {
            // 异常时返回通用建议
            string defaultSuggestion = "1. 优先选择低GI食物，控制每日碳水化合物摄入量；\r\n" +
                                       "2. 三餐定时定量，避免暴饮暴食；\r\n" +
                                       "3. 多吃绿叶蔬菜，减少精制米面、含糖饮料摄入；\r\n" +
                                       "4. 烹饪方式优先选择蒸、煮、清炒，避免油炸、红烧。";
            return BizResult.Success(data: defaultSuggestion);
        }
    }
}