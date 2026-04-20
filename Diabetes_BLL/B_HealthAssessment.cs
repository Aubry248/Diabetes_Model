using System;
using System.Collections.Generic;
using Model;
using DAL;

namespace BLL
{
    public class B_HealthAssessment
    {
        private readonly D_HealthAssessment _dal = new D_HealthAssessment();
        // 👇 新增：计算健康综合评分与等级的核心方法
        public (decimal totalScore, HealthLevel healthLevel, string levelDesc) CalcHealthComprehensiveScore(
            decimal? avgFastingGlucose,    // 空腹血糖
            decimal? avgPostprandialGlucose, // 餐后2小时血糖
            decimal? hba1c,                // 糖化血红蛋白
            short? systolicBp,             // 收缩压
            short? diastolicBp,            // 舒张压
            decimal? bmi                    // 身体质量指数
        )

        {
            decimal totalScore = 0;

            // 1. 空腹血糖评分（正常：3.9-6.1mmol/L，糖尿病控制目标：<7.0mmol/L）
            if (avgFastingGlucose.HasValue)
            {
                if (avgFastingGlucose >= 3.9m && avgFastingGlucose <= 6.1m)
                    totalScore += 20;
                else if (avgFastingGlucose <= 7.0m)
                    totalScore += 10;
                else
                    totalScore += 0;
            }

            // 2. 餐后2小时血糖评分（正常：<7.8mmol/L，糖尿病控制目标：<10.0mmol/L）
            if (avgPostprandialGlucose.HasValue)
            {
                if (avgPostprandialGlucose < 7.8m)
                    totalScore += 20;
                else if (avgPostprandialGlucose <= 10.0m)
                    totalScore += 10;
                else
                    totalScore += 0;
            }

            // 3. 糖化血红蛋白HbA1c评分（正常：<5.7%，糖尿病控制目标：<7.0%）
            if (hba1c.HasValue)
            {
                if (hba1c < 5.7m)
                    totalScore += 20;
                else if (hba1c < 7.0m)
                    totalScore += 10;
                else
                    totalScore += 0;
            }

            // 4. 血压评分（糖尿病患者控制目标：<130/80mmHg）
            if (systolicBp.HasValue && diastolicBp.HasValue)
            {
                if (systolicBp < 130 && diastolicBp < 80)
                    totalScore += 20;
                else if (systolicBp < 140 && diastolicBp < 90)
                    totalScore += 10;
                else
                    totalScore += 0;
            }

            // 5. BMI评分（正常：18.5-23.9，超重：24.0-27.9，肥胖：≥28.0）
            if (bmi.HasValue)
            {
                if (bmi >= 18.5m && bmi <= 23.9m)
                    totalScore += 20;
                else if (bmi <= 27.9m)
                    totalScore += 10;
                else
                    totalScore += 0;
            }

            // 计算健康等级（满分100分）
            HealthLevel healthLevel;
            string levelDesc;

            if (totalScore >= 90)
            {
                healthLevel = HealthLevel.优秀;
                levelDesc = "优秀";
            }
            else if (totalScore >= 60)
            {
                healthLevel = HealthLevel.合格;
                levelDesc = "合格";
            }
            else
            {
                healthLevel = HealthLevel.不合格;
                levelDesc = "不合格";
            }

            // 返回元组结果
            return (totalScore, healthLevel, levelDesc);
        }

        /// <summary>
        /// 计算健康综合评分与等级（原有方法名保留，补充完整实现，适配糖尿病指南标准）
        /// </summary>
        /// <returns>元组（综合评分，健康等级，等级描述）</returns>
        public (decimal Score, HealthLevel Level, string Description) CalcHealthComprehensiveScore(
            decimal? fastingGlucose, decimal? postGlucose, decimal? hba1c,
            int? systolicBp, int? diastolicBp, decimal? bmi)
        {
            decimal totalScore = 100;
            string description = "健康指标综合评估";

            // 1. 空腹血糖评分（权重30%）
            if (fastingGlucose.HasValue)
            {
                if (fastingGlucose < 3.9m) totalScore -= 30; // 低血糖，严重异常
                else if (fastingGlucose > 7.0m) totalScore -= Math.Min(30, (fastingGlucose.Value - 7.0m) * 5); // 高血糖扣分
                else if (fastingGlucose >= 3.9m && fastingGlucose <= 6.1m) totalScore += 5; // 达标加分
            }
            else
            {
                totalScore -= 10; // 无数据扣分
            }

            // 2. 餐后血糖评分（权重25%）
            if (postGlucose.HasValue)
            {
                if (postGlucose < 3.9m) totalScore -= 25;
                else if (postGlucose > 10.0m) totalScore -= Math.Min(25, (postGlucose.Value - 10.0m) * 3);
                else if (postGlucose >= 3.9m && postGlucose <= 7.8m) totalScore += 5;
            }
            else
            {
                totalScore -= 8;
            }

            // 3. 糖化血红蛋白评分（权重20%）
            if (hba1c.HasValue)
            {
                if (hba1c > 7.0m) totalScore -= Math.Min(20, (hba1c.Value - 7.0m) * 4);
                else if (hba1c >= 4.0m && hba1c <= 6.5m) totalScore += 5;
            }
            else
            {
                totalScore -= 7;
            }

            // 4. 血压评分（权重15%）
            if (systolicBp.HasValue && diastolicBp.HasValue)
            {
                if (systolicBp > 140 || diastolicBp > 90) totalScore -= Math.Min(15, ((systolicBp.Value - 140) / 10) * 2);
                else if (systolicBp >= 90 && systolicBp <= 130 && diastolicBp >= 60 && diastolicBp <= 80) totalScore += 3;
            }
            else
            {
                totalScore -= 5;
            }

            // 5. BMI评分（权重10%）
            if (bmi.HasValue)
            {
                if (bmi < 18.5m || bmi > 28.0m) totalScore -= 10;
                else if (bmi >= 18.5m && bmi <= 23.9m) totalScore += 2;
            }
            else
            {
                totalScore -= 3;
            }

            // 分数兜底（0-100）
            totalScore = Math.Max(0, Math.Min(100, totalScore));

            // 健康等级判定
            HealthLevel level;
            if (totalScore >= 90)
            {
                level = HealthLevel.优秀;
                description = $"综合评分{totalScore:F1}分，血糖、血压、BMI等核心指标整体控制优秀，糖尿病并发症风险低";
            }
            else if (totalScore >= 70)
            {
                level = HealthLevel.合格;
                description = $"综合评分{totalScore:F1}分，核心指标基本达标，存在轻微异常，需针对性调整生活方式";
            }
            else
            {
                level = HealthLevel.不合格;
                description = $"综合评分{totalScore:F1}分，核心指标控制不佳，糖尿病并发症风险较高，需尽快调整干预方案";
            }

            return (Math.Round(totalScore, 1), level, description);
        }

        /// <summary>
        /// 保存评估（带校验）
        /// </summary>
        public int Save(HealthAssessment model)
        {
            if (model.user_id <= 0) throw new ArgumentException("请选择患者");
            if (model.assessment_by <= 0) throw new ArgumentException("医生信息无效");

            model.data_version = 1;
            model.status = 1;
            model.create_time = DateTime.Now;
            model.update_time = DateTime.Now;

            return _dal.AddHealthAssessment(model);
        }

        /// <summary>
        /// 自动BMI
        /// </summary>
        public decimal? CalcBMI(decimal? height, decimal? weight)
        {
            if (!height.HasValue || !weight.HasValue || height <= 0) return null;
            return Math.Round(weight.Value / (height.Value / 100m) / (height.Value / 100m), 1);
        }

        /// <summary>
        /// 获取患者最新的健康评估记录（原有方法名保留，补充完整实现）
        /// </summary>
        /// <param name="userId">患者ID</param>
        /// <returns>最新健康评估实体，无数据返回null</returns>
        public HealthAssessment GetLatestAssessment(int userId)
        {
            if (userId <= 0) return null;
            try
            {
                return _dal.GetLatestHealthAssessment(userId);
            }
            catch (Exception ex)
            {
                throw new Exception("获取患者健康评估失败：" + ex.Message, ex);
            }
        }

    }
}