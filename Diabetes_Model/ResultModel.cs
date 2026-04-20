namespace Model
{
    /// <summary>
    /// 通用返回结果模型（适配血糖/用药全业务）
    /// </summary>
    public class ResultModel
    {
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 操作返回消息
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 通用返回数据（兼容所有业务实体，替代原有固定BloodSugar类型）
        /// </summary>
        public object Data { get; set; }

        #region 新增构造函数（解决CS1729核心报错）
        /// <summary>
        /// 无参构造函数（兼容原有代码）
        /// </summary>
        public ResultModel() { }

        /// <summary>
        /// 2个参数的构造函数（适配新增/编辑业务代码）
        /// </summary>
        /// <param name="success">操作是否成功</param>
        /// <param name="msg">返回消息</param>
        public ResultModel(bool success, string msg)
        {
            Success = success;
            Msg = msg;
        }

        /// <summary>
        /// 全参数构造函数（兼容带数据返回的场景）
        /// </summary>
        /// <param name="success">操作是否成功</param>
        /// <param name="msg">返回消息</param>
        /// <param name="data">返回的业务数据</param>
        public ResultModel(bool success, string msg, object data)
        {
            Success = success;
            Msg = msg;
            Data = data;
        }
        #endregion
    }
}