using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Model;
using Tools; // 你项目里的SqlHelper所在命名空间

namespace DAL
{
    /// <summary>
    /// 用户数据访问层
    /// </summary>
    public class D_User
    {
        #region 获取用户列表（支持多条件查询）
        /// <summary>
        /// 获取用户列表，支持多条件模糊查询
        /// </summary>
        public List<Users> GetUserList(string loginAccount = "", string userName = "", string phone = "", int? userType = null)
        {
            string sql = @"
SELECT u.*, r.role_name, r.role_id 
FROM t_user u
LEFT JOIN t_user_role ur ON u.user_id = ur.user_id AND ur.status=1
LEFT JOIN t_role r ON ur.role_id = r.role_id AND r.status=1
WHERE 1=1";
            List<SqlParameter> param = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(loginAccount))
            {
                sql += " AND u.login_account LIKE @LoginAccount";
                param.Add(new SqlParameter("@LoginAccount", "%" + loginAccount + "%"));
            }
            if (!string.IsNullOrEmpty(userName))
            {
                sql += " AND u.user_name LIKE @UserName";
                param.Add(new SqlParameter("@UserName", "%" + userName + "%"));
            }
            if (!string.IsNullOrEmpty(phone))
            {
                sql += " AND u.phone LIKE @Phone";
                param.Add(new SqlParameter("@Phone", "%" + phone + "%"));
            }
            if (userType.HasValue)
            {
                sql += " AND u.user_type = @UserType";
                param.Add(new SqlParameter("@UserType", userType.Value));
            }

            sql += " ORDER BY u.create_time DESC";
            DataTable dt = SqlHelper.ExecuteDataTable(sql, param.ToArray());

            List<Users> list = new List<Users>();
            foreach (DataRow dr in dt.Rows)
            {
                Users user = new Users
                {
                    user_id = Convert.ToInt32(dr["user_id"]),
                    user_name = dr["user_name"].ToString(),
                    id_card = dr["id_card"].ToString(),
                    phone = dr["phone"].ToString(),
                    emergency_contact = dr["emergency_contact"]?.ToString(),
                    emergency_phone = dr["emergency_phone"]?.ToString(),
                    password = dr["password"].ToString(),
                    user_type = Convert.ToInt32(dr["user_type"]),
                    diabetes_type = dr["diabetes_type"]?.ToString(),
                    diagnose_date = dr["diagnose_date"] as DateTime?,
                    fasting_glucose_baseline = dr["fasting_glucose_baseline"] as decimal?,
                    last_login_time = dr["last_login_time"] as DateTime?,
                    data_version = Convert.ToInt32(dr["data_version"]),
                    create_time = Convert.ToDateTime(dr["create_time"]),
                    update_time = Convert.ToDateTime(dr["update_time"]),
                    status = Convert.ToInt32(dr["status"]),
                    gender = Convert.ToInt32(dr["gender"]),
                    age = Convert.ToInt32(dr["age"]),
                    birth_date = dr["birth_date"] as DateTime?,
                    login_account = dr["login_account"].ToString()
                };
                // 扩展字段：绑定的角色名称和ID
                if (dr["role_id"] != DBNull.Value)
                {
                    // 这里用Tag存储角色ID，方便界面绑定
                }
                list.Add(user);
            }
            return list;
        }
        #endregion

        /// <summary>
        /// 新增用户，返回新用户ID
        /// </summary>
        public int AddUser(Users user)
        {
            string sql = @"
            INSERT INTO t_user (
                user_name, id_card, phone, emergency_contact, emergency_phone, 
                password, salt, user_type, gender, birth_date, login_account, 
                status, data_version, create_time, update_time
            )
            VALUES (
                @user_name, @id_card, @phone, @emergency_contact, @emergency_phone, 
                @password, @salt, @user_type, @gender, @birth_date, @login_account, 
                1, 1, GETDATE(), GETDATE()
            );
            SELECT SCOPE_IDENTITY();";

            SqlParameter[] param = {
        new SqlParameter("@user_name", user.user_name),
        new SqlParameter("@id_card", user.id_card),
        new SqlParameter("@phone", user.phone),
        new SqlParameter("@emergency_contact", user.emergency_contact ?? (object)DBNull.Value),
        new SqlParameter("@emergency_phone", user.emergency_phone ?? (object)DBNull.Value),
        new SqlParameter("@password", user.password),
        new SqlParameter("@salt", user.salt),
        new SqlParameter("@user_type", user.user_type),
        new SqlParameter("@gender", user.gender),
        new SqlParameter("@birth_date", user.birth_date ?? (object)DBNull.Value),
        new SqlParameter("@login_account", user.login_account)
    };
            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql, param));
        }

        #region 修改用户
        /// <summary>
        /// 修改用户信息
        /// </summary>
        public bool UpdateUser(Users user)
        {
            string sql = @"
UPDATE t_user SET 
    user_name = @user_name,
    phone = @phone,
    emergency_contact = @emergency_contact,
    emergency_phone = @emergency_phone,
    user_type = @user_type,
    gender = @gender,
    birth_date = @birth_date,
    update_time = GETDATE()
WHERE user_id = @user_id";

            SqlParameter[] param = {
        new SqlParameter("@user_name", user.user_name),
        new SqlParameter("@phone", user.phone),
        new SqlParameter("@emergency_contact", user.emergency_contact ?? (object)DBNull.Value),
        new SqlParameter("@emergency_phone", user.emergency_phone ?? (object)DBNull.Value),
        new SqlParameter("@user_type", user.user_type),
        new SqlParameter("@gender", user.gender),
        new SqlParameter("@birth_date", user.birth_date ?? (object)DBNull.Value),
        new SqlParameter("@user_id", user.user_id)
    };
            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }
        #endregion

        /// <summary>
        /// 重置用户密码
        /// </summary>
        public bool ResetUserPassword(int userId, string newPassword, string newSalt)
        {
            string sql = "UPDATE t_user SET password = @password, salt = @salt, update_time = GETDATE() WHERE user_id = @user_id";
            SqlParameter[] param = {
        new SqlParameter("@password", newPassword),
        new SqlParameter("@salt", newSalt), // ✅ 新增salt参数
        new SqlParameter("@user_id", userId)
    };
            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }

        #region 修改用户状态（启用/禁用）
        /// <summary>
        /// 修改用户启用/禁用状态
        /// </summary>
        public bool UpdateUserStatus(int userId, int status)
        {
            string sql = "UPDATE t_user SET status = @status, update_time = GETDATE() WHERE user_id = @user_id";
            SqlParameter[] param = {
                new SqlParameter("@status", status),
                new SqlParameter("@user_id", userId)
            };
            return SqlHelper.ExecuteNonQuery(sql, param) > 0;
        }
        #endregion

        #region 校验账号/身份证/手机号唯一性
        public bool CheckAccountExist(string loginAccount, int excludeUserId = 0)
        {
            string sql = "SELECT COUNT(1) FROM t_user WHERE login_account = @login_account";
            if (excludeUserId > 0) sql += " AND user_id != @excludeUserId";
            List<SqlParameter> param = new List<SqlParameter>();
            param.Add(new SqlParameter("@login_account", loginAccount));
            if (excludeUserId > 0) param.Add(new SqlParameter("@excludeUserId", excludeUserId));
            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql, param.ToArray())) > 0;
        }

        public bool CheckIdCardExist(string idCard, int excludeUserId = 0)
        {
            string sql = "SELECT COUNT(1) FROM t_user WHERE id_card = @id_card";
            if (excludeUserId > 0) sql += " AND user_id != @excludeUserId";
            List<SqlParameter> param = new List<SqlParameter>();
            param.Add(new SqlParameter("@id_card", idCard));
            if (excludeUserId > 0) param.Add(new SqlParameter("@excludeUserId", excludeUserId));
            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql, param.ToArray())) > 0;
        }

        public bool CheckPhoneExist(string phone, int excludeUserId = 0)
        {
            string sql = "SELECT COUNT(1) FROM t_user WHERE phone = @phone";
            if (excludeUserId > 0) sql += " AND user_id != @excludeUserId";
            List<SqlParameter> param = new List<SqlParameter>();
            param.Add(new SqlParameter("@phone", phone));
            if (excludeUserId > 0) param.Add(new SqlParameter("@excludeUserId", excludeUserId));
            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql, param.ToArray())) > 0;
        }
        #endregion

        #region 扩展方法：获取带评估状态的患者分页列表
        /// <summary>
        /// 获取带评估状态的患者分页列表（医生端专用）
        /// </summary>
        /// <param name="doctorId">当前医生ID</param>
        /// <param name="userName">患者姓名（模糊查询）</param>
        /// <param name="phone">手机号（模糊查询）</param>
        /// <param name="diabetesType">糖尿病类型</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="totalCount">输出总条数</param>
        /// <returns>患者列表</returns>
        public List<PatientSimpleInfo> GetPatientListWithAssessmentStatus(int doctorId, string userName, string phone, string diabetesType, int pageIndex, int pageSize, out int totalCount)
        {
            totalCount = 0;
            List<PatientSimpleInfo> list = new List<PatientSimpleInfo>();
            try
            {
                // 总条数查询SQL
                string countSql = @"
        SELECT COUNT(1) 
        FROM t_user u
        LEFT JOIN (
            SELECT user_id, MAX(assessment_date) AS last_assess_time 
            FROM t_health_assessment 
            WHERE status=1 
            GROUP BY user_id
        ) ha ON u.user_id = ha.user_id
        WHERE u.user_type=1 AND u.status=1";

                // 分页数据查询SQL
                string dataSql = @"
        SELECT * FROM (
            SELECT 
                u.user_id AS UserId,
                u.user_name AS UserName,
                STUFF(u.phone,4,4,'****') AS Phone,
                u.diabetes_type AS DiabetesType,
                ISNULL(abnormal.unhandled_count,0) AS UnhandledAbnormalCount,
                CASE WHEN ha.last_assess_time IS NOT NULL THEN '已评估' ELSE '未评估' END AS AssessmentStatus,
                ha.last_assess_time AS LastAssessmentTime,
                ROW_NUMBER() OVER (ORDER BY u.create_time DESC) AS RowNum
            FROM t_user u
            LEFT JOIN (
                SELECT user_id, COUNT(1) AS unhandled_count 
                FROM t_abnormal 
                WHERE handle_status=0 
                GROUP BY user_id
            ) abnormal ON u.user_id = abnormal.user_id
            LEFT JOIN (
                SELECT user_id, MAX(assessment_date) AS last_assess_time 
                FROM t_health_assessment 
                WHERE status=1 
                GROUP BY user_id
            ) ha ON u.user_id = ha.user_id
            WHERE u.user_type=1 AND u.status=1
        ) AS temp
        WHERE temp.RowNum BETWEEN @StartRow AND @EndRow";

                // 筛选条件拼接
                List<SqlParameter> paramList = new List<SqlParameter>();
                if (!string.IsNullOrEmpty(userName))
                {
                    countSql += " AND u.user_name LIKE @UserName";
                    dataSql += " AND temp.UserName LIKE @UserName";
                    paramList.Add(new SqlParameter("@UserName", "%" + userName.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(phone))
                {
                    countSql += " AND u.phone LIKE @Phone";
                    dataSql += " AND temp.Phone LIKE @Phone";
                    paramList.Add(new SqlParameter("@Phone", "%" + phone.Trim() + "%"));
                }
                if (!string.IsNullOrEmpty(diabetesType) && diabetesType != "全部")
                {
                    countSql += " AND u.diabetes_type = @DiabetesType";
                    dataSql += " AND temp.DiabetesType = @DiabetesType";
                    paramList.Add(new SqlParameter("@DiabetesType", diabetesType));
                }

                // 分页参数
                int startRow = (pageIndex - 1) * pageSize + 1;
                int endRow = pageIndex * pageSize;
                paramList.Add(new SqlParameter("@StartRow", startRow));
                paramList.Add(new SqlParameter("@EndRow", endRow));

                // 执行查询
                object countObj = SqlHelper.ExecuteScalar(countSql, paramList.ToArray());
                totalCount = countObj != DBNull.Value ? Convert.ToInt32(countObj) : 0;
                if (totalCount == 0) return list;

                DataTable dt = SqlHelper.ExecuteDataTable(dataSql, paramList.ToArray());
                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(new PatientSimpleInfo
                    {
                        UserId = dr["UserId"] != DBNull.Value ? Convert.ToInt32(dr["UserId"]) : 0,
                        UserName = dr["UserName"]?.ToString() ?? "",
                        Phone = dr["Phone"]?.ToString() ?? "",
                        DiabetesType = dr["DiabetesType"]?.ToString() ?? "未分类",
                        UnhandledAbnormalCount = dr["UnhandledAbnormalCount"] != DBNull.Value ? Convert.ToInt32(dr["UnhandledAbnormalCount"]) : 0,
                        AssessmentStatus = dr["AssessmentStatus"]?.ToString() ?? "未评估",
                        LastAssessmentTime = dr["LastAssessmentTime"] as DateTime?
                    });
                }
                return list;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"【D_User扩展方法异常】{ex.Message}\n{ex.StackTrace}");
                throw new Exception("查询患者列表数据异常", ex);
            }
        }
        #endregion

        #region 新增：获取当前医生管理的患者列表（去重，仅该医生做过健康评估的患者）
        /// <summary>
        /// 获取指定医生管理的患者列表（去重）
        /// </summary>
        /// <param name="doctorId">医生用户ID</param>
        /// <param name="searchText">搜索关键词（姓名/手机号）</param>
        /// <returns>患者简易信息列表</returns>
        public List<PatientSimpleInfo> GetDoctorManagedPatientList(int doctorId, string searchText = "")
        {
            string sql = @"
            SELECT DISTINCT
                u.user_id AS UserId,
                u.user_name AS UserName,
                STUFF(u.phone,4,4,'*****') AS Phone,
                u.diabetes_type AS DiabetesType,
                u.diagnose_date AS DiagnoseDate,
                ISNULL(abnormal.unhandled_count,0) AS UnhandledAbnormalCount,
                CASE WHEN ha.last_assess_time IS NOT NULL THEN '已评估' ELSE '未评估' END AS AssessmentStatus,
                ha.last_assess_time AS LastAssessmentTime
            FROM t_user u
            LEFT JOIN (
                SELECT user_id, MAX(assessment_date) AS last_assess_time 
                FROM t_health_assessment 
                WHERE status=1 AND assessment_by = @DoctorId
                GROUP BY user_id
            ) ha ON u.user_id = ha.user_id
            LEFT JOIN (
                SELECT user_id, COUNT(1) AS unhandled_count 
                FROM t_abnormal 
                WHERE handle_status=0 
                GROUP BY user_id
            ) abnormal ON u.user_id = abnormal.user_id
            WHERE u.user_type=1 AND u.status=1
            -- ✅ 移除u.doctor_id = @DoctorId（当前doctor_id全为NULL，过滤后无数据）
            -- 后续给患者分配doctor_id后，可加回此条件，实现「仅显示分配给自己的患者」
            ORDER BY 
                CASE WHEN ha.last_assess_time IS NOT NULL THEN 0 ELSE 1 END, -- 已评估患者排前面
                ha.last_assess_time DESC
            ";
            List<SqlParameter> paramList = new List<SqlParameter>();
            paramList.Add(new SqlParameter("@DoctorId", doctorId));

            // 模糊搜索条件
            if (!string.IsNullOrEmpty(searchText))
            {
                sql += " AND (u.user_name LIKE @SearchText OR u.phone LIKE @SearchText)";
                paramList.Add(new SqlParameter("@SearchText", "%" + searchText.Trim() + "%"));
            }

            sql += " ORDER BY ha.last_assess_time DESC";

            try
            {
                return SqlHelper.GetModelList<PatientSimpleInfo>(sql, paramList.ToArray());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"【D_User-医生患者查询异常】{ex.Message}\n{ex.StackTrace}");
                throw new Exception("查询医生管理的患者列表异常", ex);
            }
        }
        #endregion


    }
}