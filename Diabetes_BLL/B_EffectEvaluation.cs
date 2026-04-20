using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using DAL;

namespace BLL
{
    /// <summary>
    /// 干预效果评估业务逻辑层
    /// </summary>
    public class B_EffectEvaluation
    {
        private readonly D_EffectEvaluation _dalEvaluation = new D_EffectEvaluation();

        #region 公共方法：获取有效患者列表
        public List<PatientSimpleInfo> GetAllValidPatient()
        {
            return _dalEvaluation.GetAllValidPatient();
        }
        #endregion

        #region 1. 血糖趋势分析业务逻辑
        /// <summary>
        /// 获取患者血糖趋势明细与统计数据
        /// </summary>
        public (bool Success, string Msg, List<BloodSugarTrendViewModel> TrendList, BloodSugarStatisticsModel Statistics) GetBloodSugarTrendData(int userId, DateTime startDate, DateTime endDate)
        {
            // 基础校验
            if (userId <= 0)
                return (false, "请选择有效患者", null, null);
            if (endDate < startDate)
                return (false, "结束日期不能早于开始日期", null, null);

            try
            {
                var trendList = _dalEvaluation.GetBloodSugarTrendList(userId, startDate, endDate);
                var statistics = _dalEvaluation.GetBloodSugarStatistics(userId, startDate, endDate);

                if (trendList == null || !trendList.Any())
                    return (true, "该时间段内无患者血糖数据", trendList, statistics);

                return (true, "查询成功", trendList, statistics);
            }
            catch (Exception ex)
            {
                return (false, $"系统异常：{ex.Message}", null, null);
            }
        }
        #endregion

        #region 2. 并发症进展跟踪业务逻辑
        /// <summary>
        /// 获取患者并发症进展记录
        /// </summary>
        public (bool Success, string Msg, List<ComplicationProgressViewModel> Data) GetComplicationProgressList(int userId, string complicationType, DateTime startDate, DateTime endDate)
        {
            if (userId <= 0)
                return (false, "请选择有效患者", null);
            if (endDate < startDate)
                return (false, "结束日期不能早于开始日期", null);

            try
            {
                var data = _dalEvaluation.GetComplicationProgressList(userId, complicationType, startDate, endDate);
                if (data == null || !data.Any())
                    return (true, "该时间段内无患者并发症随访记录", data);

                return (true, "查询成功", data);
            }
            catch (Exception ex)
            {
                return (false, $"系统异常：{ex.Message}", null);
            }
        }
        #endregion

        #region 3. 干预效果评分业务逻辑
        /// <summary>
        /// 计算并保存干预效果评分
        /// </summary>
        public (bool Success, string Msg, int TotalScore, string EffectLevel) CalcAndSaveEffectScore(InterventionEffectScore model)
        {
            // 输入校验
            if (model.UserId <= 0)
                return (false, "请选择有效患者", 0, "未评分");
            if (model.AssessmentBy <= 0)
                return (false, "当前登录医生信息异常，请重新登录", 0, "未评分");
            if (model.DietScore < 0 || model.DietScore > 25)
                return (false, "饮食干预评分范围为0-25分", 0, "未评分");
            if (model.ExerciseScore < 0 || model.ExerciseScore > 25)
                return (false, "运动干预评分范围为0-25分", 0, "未评分");
            if (model.MedicationScore < 0 || model.MedicationScore > 25)
                return (false, "用药干预评分范围为0-25分", 0, "未评分");
            if (model.ComplianceScore < 0 || model.ComplianceScore > 25)
                return (false, "依从性评分范围为0-25分", 0, "未评分");

            // 计算总分与效果等级
            model.TotalScore = model.DietScore + model.ExerciseScore + model.MedicationScore + model.ComplianceScore;
            model.EffectLevel = model.TotalScore >= 80 ? "优秀" : model.TotalScore >= 60 ? "良好" : model.TotalScore >= 40 ? "一般" : "差";
            model.AssessmentDate = DateTime.Now.Date;

            try
            {
                // 保存至数据库
                int assessmentId = _dalEvaluation.SaveInterventionEffectScore(model);
                if (assessmentId <= 0)
                    return (false, "评分保存失败，请重试", model.TotalScore, model.EffectLevel);

                return (true, "干预效果评分计算并保存成功", model.TotalScore, model.EffectLevel);
            }
            catch (Exception ex)
            {
                return (false, $"系统异常：{ex.Message}", model.TotalScore, model.EffectLevel);
            }
        }

        /// <summary>
        /// 获取患者最新评分
        /// </summary>
        public InterventionEffectScore GetLatestEffectScore(int userId)
        {
            if (userId <= 0) return null;
            return _dalEvaluation.GetLatestEffectScore(userId);
        }
        #endregion

        #region 4. 评估报告生成业务逻辑
        /// <summary>
        /// 生成干预效果评估报告聚合数据
        /// </summary>
        public (bool Success, string Msg, EffectEvaluationReportModel ReportData) GenerateEvaluationReport(int userId, DateTime startDate, DateTime endDate, string doctorName)
        {
            if (userId <= 0)
                return (false, "请选择有效患者", null);
            if (endDate < startDate)
                return (false, "结束日期不能早于开始日期", null);

            try
            {
                // 1. 获取患者基础信息
                var patientInfo = _dalEvaluation.GetPatientBaseInfo(userId);
                if (patientInfo == null)
                    return (false, "患者信息不存在", null);

                // 2. 获取血糖统计数据
                var glucoseStatistics = _dalEvaluation.GetBloodSugarStatistics(userId, startDate, endDate);

                // 3. 获取并发症进展记录
                var complicationRecords = _dalEvaluation.GetComplicationProgressList(userId, "全部", startDate, endDate);

                // 4. 获取最新干预评分
                var latestScore = _dalEvaluation.GetLatestEffectScore(userId);

                // 5. 获取周期内干预方案
                var interventionPlans = _dalEvaluation.GetPatientInterventionPlans(userId, startDate, endDate);

                // 6. 聚合报告数据
                var reportData = new EffectEvaluationReportModel
                {
                    PatientName = patientInfo.user_name,
                    PatientAge = patientInfo.age,
                    DiabetesType = patientInfo.diabetes_type,
                    DiagnoseDate = patientInfo.diagnose_date ?? DateTime.Now,
                    ReportStartDate = startDate,
                    ReportEndDate = endDate,
                    GlucoseStatistics = glucoseStatistics,
                    ComplicationRecords = complicationRecords,
                    LatestScore = latestScore,
                    InterventionPlans = interventionPlans,
                    ReportDoctor = doctorName,
                    ReportGenerateTime = DateTime.Now
                };

                return (true, "报告生成成功", reportData);
            }
            catch (Exception ex)
            {
                return (false, $"报告生成异常：{ex.Message}", null);
            }
        }

        /// <summary>
        /// 生成报告文本内容
        /// </summary>
        public string BuildReportText(EffectEvaluationReportModel report)
        {
            if (report == null) return "";

            string reportText = $@"
====================================================================
                      糖尿病干预效果评估报告
====================================================================
一、患者基本信息
    患者姓名：{report.PatientName}
    年    龄：{report.PatientAge}岁
    糖尿病类型：{report.DiabetesType}
    确诊日期：{report.DiagnoseDate:yyyy年MM月dd日}
    评估周期：{report.ReportStartDate:yyyy年MM月dd日} 至 {report.ReportEndDate:yyyy年MM月dd日}
    生成医生：{report.ReportDoctor}
    生成时间：{report.ReportGenerateTime:yyyy年MM月dd日 HH:mm:ss}

--------------------------------------------------------------------
二、血糖控制情况分析
    1. 数据统计：
       - 周期内血糖记录总数：{report.GlucoseStatistics.TotalRecordCount}条
       - 异常记录数：{report.GlucoseStatistics.AbnormalCount}条
       - 平均空腹血糖：{report.GlucoseStatistics.AvgFastingGlucose:F1} mmol/L
       - 平均餐后2小时血糖：{report.GlucoseStatistics.AvgPostprandialGlucose:F1} mmol/L
       - 血糖波动幅度：{report.GlucoseStatistics.GlucoseFluctuation:F1} mmol/L
       - 血糖达标率：{report.GlucoseStatistics.StandardReachRate:F2}%
    2. 控制情况评估：
       {(report.GlucoseStatistics.StandardReachRate >= 80 ? "血糖控制优秀，达标率超过80%，继续保持当前干预方案。" :
         report.GlucoseStatistics.StandardReachRate >= 60 ? "血糖控制良好，达标率超过60%，可微调干预方案进一步提升达标率。" :
         "血糖控制较差，达标率不足60%，需尽快调整干预方案，加强血糖监测。")}

--------------------------------------------------------------------
三、并发症进展跟踪
    周期内共随访评估{report.ComplicationRecords?.Count ?? 0}次，主要并发症情况：
    {(report.ComplicationRecords == null || !report.ComplicationRecords.Any() ? "无相关并发症随访记录" :
      string.Join("\r\n    ", report.ComplicationRecords.Select(c => $"{c.RecordDate:yyyy-MM-dd} | {c.ComplicationType} | 进展：{c.ProgressDegree}")))}

--------------------------------------------------------------------
四、干预效果评分
    {(report.LatestScore == null ? "暂无干预效果评分记录" :
    $@"最新评分时间：{report.LatestScore.AssessmentDate:yyyy年MM月dd日}
    干预类型：{report.LatestScore.InterventionType}
    综合评分：{report.LatestScore.TotalScore}分
    效果等级：{report.LatestScore.EffectLevel}")}

--------------------------------------------------------------------
五、周期内干预方案执行情况
    周期内共制定干预方案{report.InterventionPlans?.Count ?? 0}个：
    {(report.InterventionPlans == null || !report.InterventionPlans.Any() ? "无相关干预方案记录" :
      string.Join("\r\n    ", report.InterventionPlans.Select(p => $"{p.create_time:yyyy-MM-dd} | {p.plan_type} | 执行状态：{(p.execute_status == 0 ? "待执行" : p.execute_status == 1 ? "执行中" : "已完成")}")))}

--------------------------------------------------------------------
六、综合评估与建议
    1. 综合评估：
       {(report.LatestScore?.TotalScore >= 80 ? "患者整体干预效果优秀，血糖控制达标，并发症稳定，依从性良好。" :
         report.LatestScore?.TotalScore >= 60 ? "患者整体干预效果良好，血糖控制基本达标，并发症无明显进展，需继续保持。" :
         "患者整体干预效果较差，血糖控制不达标，需立即调整干预方案，加强患者健康教育与随访管理。")}
    2. 后续建议：
       - 继续保持规律的血糖监测，重点关注空腹及餐后2小时血糖；
       - 严格执行饮食、运动干预方案，提升用药与生活方式依从性；
       - 定期复查并发症相关指标，每3个月进行一次全面健康评估；
       - 如有血糖剧烈波动、身体不适等情况，及时就诊。

====================================================================
";
            return reportText;
        }
        #endregion
    }
}