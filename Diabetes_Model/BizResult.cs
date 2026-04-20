namespace Model
{
    public class BizResult
    {
        /// <summary>
        /// 业务操作结果对象（全局统一版，修复属性不匹配问题）
        /// </summary>
            /// <summary>
            /// 操作是否成功
            /// </summary>
            public bool IsSuccess { get; set; }
            /// <summary>
            /// 操作返回消息
            /// </summary>
            public string Message { get; set; }
            /// <summary>
            /// 返回数据
            /// </summary>
            public object Data { get; set; }
            /// <summary>
            /// 总条数（列表/分页用）
            /// </summary>
            public int TotalCount { get; set; }

            /// <summary>
            /// 成功返回（全参数兼容）
            /// </summary>
            public static BizResult Success(string message = "操作成功", object data = null, int totalCount = 0)
            {
                return new BizResult
                {
                    IsSuccess = true,
                    Message = message,
                    Data = data,
                    TotalCount = totalCount
                };
            }

            /// <summary>
            /// 失败返回
            /// </summary>
            public static BizResult Fail(string message)
            {
                return new BizResult
                {
                    IsSuccess = false,
                    Message = message
                };
            }
        }
    }