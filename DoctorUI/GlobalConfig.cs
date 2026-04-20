// 全局配置类：存储登录医生信息，所有窗体都能用
using Model;

public static class GlobalConfig
{
    /// <summary>
    /// 登录医生ID（从登录页获取）
    /// </summary>
    public static int CurrentDoctorID { get; set; }
    /// <summary>
    /// 登录医生姓名
    /// </summary>
    public static string CurrentDoctorName { get; set; }
    /// <summary>
    /// 当前选中的患者ID
    /// </summary>
    public static int CurrentPatientID { get; set; }

    /// <summary>
    /// 当前登录医生的完整用户实体（替代原SysGlobal.CurrentLoginUser）
    /// </summary>
    public static Users CurrentLoginUser { get; set; }

    #region 全局会话管理方法
    /// <summary>
    /// 清空全局会话（退出登录时调用，彻底清除登录信息，避免会话残留）
    /// </summary>
    public static void ClearSession()
    {
        CurrentLoginUser = null;
    }
    #endregion
}