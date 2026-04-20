using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DAL;
using Model;

namespace BLL
{
    public class B_Exercise
    {
        private readonly D_Exercise _dal = new D_Exercise();
        #region 基础业务方法
        public int GetTodayExerciseMinutes(int userId)
        {
            if (userId <= 0) return 0;
            try
            {
                return _dal.GetTodayExerciseMinutes(userId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"运动BLL异常：{ex.Message}");
                return -1;
            }
        }

        public (bool success, string msg) SaveExercise(Exercise model)
        {
            model.exercise_type = NormalizeExerciseType(model.exercise_type);
            var checkResult = ValidateExerciseForm(model);
            if (!checkResult.success) return checkResult;
            decimal weight = 60m;
            model.actual_calorie = model.met_value * weight * (model.exercise_duration / 60m);
            model.operator_id = model.user_id;
            // 适配新架构：统一有效状态为1
            model.data_status = 1;
            int newId = _dal.AddExercise(model);
            return newId > 0 ? (true, "保存成功") : (false, "保存失败，请重试");
        }

        public (bool success, string msg) UpdateExercise(Exercise model)
        {
            model.exercise_type = NormalizeExerciseType(model.exercise_type);
            var checkResult = ValidateExerciseForm(model);
            if (!checkResult.success) return checkResult;
            decimal weight = 60m;
            model.actual_calorie = model.met_value * weight * (model.exercise_duration / 60m);
            bool res = _dal.UpdateExercise(model);
            return res ? (true, "更新成功") : (false, "更新失败，请重试");
        }

        public (bool success, string msg) DeleteExercise(int exerciseId, int userId)
        {
            bool res = _dal.DeleteExercise(exerciseId, userId);
            return res ? (true, "删除成功") : (false, "删除失败，请重试");
        }

        public (bool success, string msg) BatchImportMockData(int userId, int dayCount = 7)
        {
            if (userId <= 0) return (false, "用户信息异常");
            List<Exercise> mockList = new List<Exercise>();
            Random rand = new Random();
            string[] types = { "快走", "慢跑", "太极拳" };
            string[] intensities = { "低强度", "中强度" };
            decimal[] metValues = { 3.5m, 6.0m, 3.3m };
            decimal weight = 60m;
            for (int i = 1; i <= dayCount; i++)
            {
                DateTime baseDate = DateTime.Now.AddDays(-i);
                int[] postMealHours = { 8, 13, 19 };
                DateTime exerciseTime = baseDate.Date.AddHours(postMealHours[rand.Next(postMealHours.Length)]);
                int duration = rand.Next(20, 45);
                int typeIndex = rand.Next(types.Length);
                mockList.Add(new Exercise
                {
                    user_id = userId,
                    device_id = "MOCK_BAND_001",
                    exercise_type = types[typeIndex],
                    met_value = metValues[typeIndex],
                    exercise_duration = duration,
                    actual_calorie = metValues[typeIndex] * weight * (duration / 60m),
                    exercise_intensity = intensities[rand.Next(intensities.Length)],
                    exercise_time = exerciseTime,
                    data_source = "模拟手环",
                    operator_id = userId,
                    // 适配新架构：统一有效状态为1
                    data_status = 1
                });
            }
            bool res = _dal.BatchAddExercise(mockList);
            return res ? (true, $"模拟数据导入完成！共{dayCount}条") : (false, "导入失败，请重试");
        }
        #endregion

        #region 数据查询业务
        public List<Exercise> GetUserExerciseList(int userId, DateTime? startDate = null, DateTime? endDate = null, string exerciseType = "", string intensity = "")
        {
            if (userId <= 0) return new List<Exercise>();
            startDate = DateTime.Now.AddMonths(-3);
            endDate = DateTime.Now;
            return _dal.GetExerciseByFilter(userId, startDate.Value, endDate.Value, exerciseType, intensity);
        }

        public (string htmlContent, bool isAbnormal) GetPersonalizedExercisePlan(int userId)
        {
            decimal avgGlucose = _dal.GetUser7DayFastingAvgGlucose(userId);
            bool isAbnormal = avgGlucose > 10.0m;
            string planContent = "";

            if (isAbnormal)
            {
                planContent = $"【糖尿病运动干预警示】\n⚠️ 您近7天空腹血糖平均值：{avgGlucose:F1}mmol/L\n❌ 当前血糖＞10.0mmol/L，暂停所有运动，先遵医嘱控制血糖\n📌 待空腹血糖稳定至10.0mmol/L以下后，再逐步恢复运动\n📞 如有不适请立即联系您的主治医生";
            }
            else if (avgGlucose >= 7.0m && avgGlucose <= 10.0m)
            {
                planContent = $"【糖尿病专属运动干预方案】\n📊 您近7天空腹血糖平均值：{avgGlucose:F1}mmol/L\n1. 推荐运动：20-40分钟低强度运动（散步、太极拳）\n2. 运动频率：每周≥5次，餐后1小时运动最佳\n3. 禁忌：空腹运动、剧烈运动、血糖波动时运动\n4. 注意：运动时随身携带糖果，预防低血糖";
            }
            else if (avgGlucose < 7.0m && avgGlucose >= 3.9m)
            {
                planContent = $"【糖尿病专属运动干预方案】\n📊 您近7天空腹血糖平均值：{avgGlucose:F1}mmol/L，血糖控制良好\n1. 推荐运动：30-60分钟中强度运动（慢跑、瑜伽、游泳、快走）\n2. 运动频率：每周≥5次，每次30分钟以上，餐后1小时运动最佳\n3. 进阶推荐：每周2次力量训练，提升肌肉量与胰岛素敏感性\n4. 禁忌：空腹运动、高强度极限运动、身体不适时运动";
            }
            else
            {
                planContent = $"【糖尿病运动干预警示】\n⚠️ 您近7天空腹血糖平均值：{avgGlucose:F1}mmol/L\n❌ 当前血糖低于3.9mmol/L，严禁运动，立即补充碳水化合物纠正低血糖\n📌 待血糖恢复至正常范围后，再评估运动方案";
                isAbnormal = true;
            }

            if (!isAbnormal)
            {
                planContent += "\n\n【通用运动安全规则】\n1. 运动前监测血糖，血糖＜3.9mmol/L或＞16.7mmol/L严禁运动\n2. 运动中出现心慌、出汗、头晕等症状，立即停止并监测血糖\n3. 合并严重心肾并发症、足部溃疡者，需遵医嘱制定个性化方案";
            }
            return (planContent, isAbnormal);
        }

        public DataTable Get30DayTrendData(int userId)
        {
            return _dal.Get30DayExerciseDailyTotal(userId);
        }

        public List<Exercise> GetDateExerciseDetail(int userId, DateTime date)
        {
            return _dal.GetExerciseByDate(userId, date);
        }

        public decimal GetExerciseMetValue(string exerciseName)
        {
            return _dal.GetExerciseMetValue(NormalizeExerciseType(exerciseName));
        }
        #endregion

        #region 私有方法：表单校验
        private (bool success, string msg) ValidateExerciseForm(Exercise model)
        {
            if (model.user_id <= 0) return (false, "用户登录状态异常，请重新登录");
            if (string.IsNullOrWhiteSpace(model.exercise_type)) return (false, "请选择运动类型");
            if (model.exercise_duration <= 0) return (false, "运动时长必须为大于0的整数（单位：分钟）");

            if (string.IsNullOrWhiteSpace(model.exercise_intensity)) return (false, "请选择运动强度");
            if (model.exercise_time > DateTime.Now) return (false, "运动时间不能晚于当前系统时间");
            if (model.met_value <= 0) return (false, "运动MET值异常，请重新选择运动类型");
            return (true, "校验通过");
        }

        private string NormalizeExerciseType(string exerciseType)
        {
            if (string.IsNullOrWhiteSpace(exerciseType))
                return exerciseType;

            switch (exerciseType.Trim())
            {
                case "散步":
                    return "快走";
                case "瑜伽":
                    return "太极拳";
                case "力量训练":
                    return "其他";
                default:
                    return exerciseType.Trim();
            }
        }
        #endregion

        public Exercise GetExerciseById(int exerciseId, int userId)
        {
            if (exerciseId <= 0 || userId <= 0) return null;
            return _dal.GetExerciseById(exerciseId, userId);
        }

        public List<Exercise> GetExerciseByFilter(int userId, DateTime? startDate = null, DateTime? endDate = null, string exerciseType = "", string intensity = "")
        {
            if (userId <= 0) return new List<Exercise>();
            startDate = startDate ?? DateTime.Now.AddYears(-10);
            endDate = endDate ?? DateTime.Now;
            if (!string.IsNullOrWhiteSpace(exerciseType))
                exerciseType = NormalizeExerciseType(exerciseType);
            return _dal.GetExerciseByFilter(userId, startDate.Value, endDate.Value, exerciseType, intensity);
        }
    }
}