/// <summary>
/// 患者档案绑定实体（对接现有患者档案管理模块）
/// </summary>
public class PatientBindInfo
{
    /// <summary>
    /// 患者唯一ID（核心绑定标识）
    /// </summary>
    public string PatientId { get; set; }
    /// <summary>
    /// 患者姓名
    /// </summary>
    public string PatientName { get; set; }
    /// <summary>
    /// 性别
    /// </summary>
    public string Gender { get; set; }
    /// <summary>
    /// 年龄（绑定糖尿病风险评估-年龄）
    /// </summary>
    public int Age { get; set; }
    /// <summary>
    /// 身高cm（绑定糖尿病风险评估-身高）
    /// </summary>
    public decimal Height { get; set; }
    /// <summary>
    /// 体重kg（绑定糖尿病风险评估-体重）
    /// </summary>
    public decimal Weight { get; set; }
    /// <summary>
    /// 家族病史（绑定糖尿病风险评估-家族病史）
    /// </summary>
    public string FamilyHistory { get; set; }
    /// <summary>
    /// 饮食结构（绑定糖尿病风险评估-饮食结构）
    /// </summary>
    public string DietStructure { get; set; }
    /// <summary>
    /// 运动情况（绑定糖尿病风险评估-运动情况）
    /// </summary>
    public string ExerciseSituation { get; set; }
    /// <summary>
    /// 病程年限（绑定并发症风险评估-病程年限）
    /// </summary>
    public string DiseaseDuration { get; set; }
    /// <summary>
    /// 用药情况
    /// </summary>
    public string MedicationInfo { get; set; }
    /// <summary>
    /// 最新空腹血糖mmol/L（绑定健康数据-空腹血糖）
    /// </summary>
    public decimal FastingGlucose { get; set; }
    /// <summary>
    /// 最新餐后血糖mmol/L（绑定健康数据-餐后血糖）
    /// </summary>
    public decimal PostprandialGlucose { get; set; }
    /// <summary>
    /// 最新糖化血红蛋白%（绑定健康数据+并发症评估-糖化）
    /// </summary>
    public decimal HbA1c { get; set; }
    /// <summary>
    /// 收缩压mmHg（绑定健康数据-收缩压）
    /// </summary>
    public decimal SystolicPressure { get; set; }
    /// <summary>
    /// 舒张压mmHg（绑定健康数据-舒张压）
    /// </summary>
    public decimal DiastolicPressure { get; set; }
    /// <summary>
    /// 血压情况（绑定并发症风险评估-血压情况）
    /// </summary>
    public string BloodPressureLevel { get; set; }
    /// <summary>
    /// 血脂情况（绑定并发症风险评估-血脂情况）
    /// </summary>
    public string LipidLevel { get; set; }
    /// <summary>
    /// BMI指数（绑定健康数据-BMI）
    /// </summary>
    public decimal BMI { get; set; }
}