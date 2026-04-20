using System;

namespace Tools
{
    /// <summary>
    /// 通用业务返回结果类
    /// </summary>
    public class Result
    {
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 返回提示信息
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 成功结果
        /// </summary>
        public static Result Ok(string msg = "操作成功")
        {
            return new Result { Success = true, Msg = msg };
        }

        /// <summary>
        /// 失败结果
        /// </summary>
        public static Result Fail(string msg = "操作失败")
        {
            return new Result { Success = false, Msg = msg };
        }
    }

    /// <summary>
    /// 带数据的通用返回结果类
    /// </summary>k
    /// <typeparam ..j  name="T">返回数据类型</typeparam>
    public class Result<T> : Result      
    {
        /// <summary>
        /// 返回的业务数据
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 带数据的成功结果
        /// </summary>
        public static Result<T> Ok(T data, string msg = "操作成功")
        {
            return new Result<T> { Success = true, Msg = msg, Data = data };
        }

        /// <summary>
        /// 带数据的失败结果
        /// </summary>
        public static new Result<T> Fail(string msg = "操作失败")
        {
            return new Result<T> { Success = false, Msg = msg, Data = default };
        }
    }
}