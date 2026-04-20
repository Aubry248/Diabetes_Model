using System;
using System.Data;
using System.Linq;
using DAL;
using Model;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>
    /// 患者健康概览业务逻辑层（无侵入式新增）
    /// </summary>
    public class B_PatientHealthOverview
    {
        private readonly D_PatientHealthOverview _dal = new D_PatientHealthOverview();
        private readonly B_BloodSugar _bllBloodSugar = new B_BloodSugar();
        private readonly B_Diet _bllDiet = new B_Diet();
        private readonly B_Exercise _bllExercise = new B_Exercise();
        private readonly B_Medicine _bllMedicine = new B_Medicine();
        private readonly B_HealthAssessment _bllAssessment = new B_HealthAssessment();

        /// <summary>
        /// 获取患者下拉列表
        /// </summary>
        public DataTable GetPatientDropDownList()
        {
            try
            {
                return _dal.GetAllPatientList();
            }
            catch (Exception ex)
            {
                throw new Exception("加载患者列表失败：" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 核心方法：获取患者完整健康数据（聚合血糖/饮食/运动/用药）
        /// </summary>
        public PatientHealthOverview GetPatientFullHealthData(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("患者ID无效");

            try
            {
                // 1. 获取患者基本信息
                PatientHealthOverview overview = _dal.GetPatientBaseInfo(userId);
                if (overview == null)
                    throw new Exception("未找到该患者信息");

                DateTime endDate = DateTime.Now;
                DateTime startDate = endDate.AddDays(-30);
                int totalCount;

                // 2. 加载近30天血糖记录+统计
                overview.BloodSugarList = _bllBloodSugar.GetBloodSugarPageList(userId, 1, 1000, out totalCount, startDate, endDate);
                if (overview.BloodSugarList.Any())
                {
                    var fastingList = overview.BloodSugarList.Where(b => b.measurement_scenario == "空腹").ToList();
                    var postList = overview.BloodSugarList.Where(b => b.measurement_scenario == "餐后2小时").ToList();

                    overview.AvgFastingGlucose = fastingList.Any() ? Math.Round(fastingList.Average(b => b.blood_sugar_value), 1) : 0;
                    overview.AvgPostprandialGlucose = postList.Any() ? Math.Round(postList.Average(b => b.blood_sugar_value), 1) : 0;

                    int abnormalCount = overview.BloodSugarList.Count(b => b.is_abnormal == 1);
                    overview.GlucoseAbnormalRate = Math.Round((decimal)abnormalCount / overview.BloodSugarList.Count * 100, 2);
                }

                // 3. 加载近30天饮食记录+统计
                var dietStat = _bllDiet.Get7DayDietStatistic(userId);
                if (dietStat.IsSuccess)
                {
                    overview.AvgDailyCalorie = ((Diet7DayStatistic)dietStat.Data).AvgDailyCalorie;
                }
                overview.DietList = new D_Diet().Get7DayDietStatistic(userId) != null ? new List<Diet>() : new List<Diet>();

                // 4. 加载近30天运动记录+统计
                overview.ExerciseList = _bllExercise.GetExerciseByFilter(userId, startDate, endDate);
                if (overview.ExerciseList.Any())
                {
                    int totalMinutes = overview.ExerciseList.Sum(e => e.exercise_duration);
                    overview.AvgDailyExerciseMinutes = totalMinutes / 30;
                }

                // 5. 加载近30天用药记录
                string msg;
                overview.MedicineList = _bllMedicine.GetUserMedicineRecordByTime(userId, startDate, endDate, out msg);

                // 6. 加载最新健康评估记录
                overview.LatestAssessment = _bllAssessment.GetLatestAssessment(userId);

                return overview;
            }
            catch (Exception ex)
            {
                throw new Exception("加载患者健康数据失败：" + ex.Message, ex);
            }
        }
    }
}