using System;
using System.Collections.Generic;
using DAL;
using Model;

namespace BLL
{
    /// <summary>
    /// 首页统计业务逻辑层
    /// </summary>
    public class B_HomeStatistics
    {
        private readonly D_HomeStatistics _dalHome = new D_HomeStatistics();

        #region 首页统计数据业务
        /// <summary>
        /// 获取首页统计数据（带业务校验与异常处理）
        /// </summary>
        /// <param name="doctorId">当前登录医生ID</param>
        /// <returns>统一业务返回格式</returns>
        public (bool Success, string Msg, HomeStatisticViewModel Data) GetHomeStatisticData(int doctorId)
        {
            // 业务前置校验
            if (doctorId <= 0)
                return (false, "当前登录医生信息异常，请重新登录", null);

            try
            {
                var data = _dalHome.GetHomeStatisticData(doctorId);
                return (true, "查询成功", data);
            }
            catch (Exception ex)
            {
                return (false, $"系统异常：{ex.Message}", null);
            }
        }
        #endregion

        #region 首页待办事项业务
        /// <summary>
        /// 获取首页待办事项列表（带业务校验与异常处理）
        /// </summary>
        /// <param name="doctorId">当前登录医生ID</param>
        /// <returns>统一业务返回格式</returns>
        public (bool Success, string Msg, List<HomeTodoViewModel> Data) GetHomeTodoList(int doctorId)
        {
            // 业务前置校验
            if (doctorId <= 0)
                return (false, "当前登录医生信息异常，请重新登录", null);

            try
            {
                var data = _dalHome.GetHomeTodoList(doctorId);
                return (true, "查询成功", data);
            }
            catch (Exception ex)
            {
                return (false, $"系统异常：{ex.Message}", null);
            }
        }
        #endregion
    }
}