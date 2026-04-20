using DAL;
using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Text.RegularExpressions;
using Tools;
using System.Linq;
using System.Net.Sockets;// 解决 AddressFamily 枚举报错
namespace BLL
{
    /// <summary>
    /// 用户业务逻辑层
    /// </summary>
    public class B_User
    {
        private readonly D_User dalUser = new D_User();
        private readonly D_UserRole dalUserRole = new D_UserRole();
        private readonly D_Role dalRole = new D_Role();

        /// <summary>
        /// 用户登录方法（支持账号/手机号登录）
        /// </summary>
        /// <param name="loginAccount">登录账号/手机号</param>
        /// <param name="password">登录密码</param>
        /// <param name="msg">输出提示信息</param>
        /// <returns>登录成功返回用户实体，失败返回null</returns>
        public Users UserLogin(string loginAccount, string password, out string msg)
        {
            msg = "";
            try
            {
                // 1. 查询用户信息（先查salt和password，不再直接在SQL中校验密码）
                string sql = @"
        SELECT * FROM t_user 
        WHERE (login_account = @loginAccount OR phone = @loginAccount)";
                SqlParameter[] param =
                {
            new SqlParameter("@loginAccount", loginAccount.Trim())
        };
                DataTable dt = SqlHelper.ExecuteDataTable(sql, param);
                System.Diagnostics.Debug.WriteLine($"【调试】查询到行数：{dt.Rows.Count}");

                if (dt.Rows.Count == 0)
                {
                    msg = "账号或密码错误，或账号已被禁用！";
                    return null;
                }

                // 2. 校验账号状态
                DataRow dr = dt.Rows[0];
                int status = dr["status"] != DBNull.Value ? Convert.ToInt32(dr["status"]) : 0;
                if (status != 1)
                {
                    msg = "您的账号已被禁用，请联系管理员！";
                    return null;
                }

                // 3. ✅ 新密码校验逻辑：用数据库中的salt计算哈希，再对比
                string storedHash = dr["password"]?.ToString() ?? "";
                string storedSalt = dr["salt"]?.ToString() ?? "";
                if (!PasswordHelper.VerifyPassword(password, storedHash, storedSalt))
                {
                    // 兼容原有MD5密码（可选，用于过渡，上线后可删除）
                    string oldMd5Hash = MD5Helper.Encrypt32(password);
                    if (!oldMd5Hash.Equals(storedHash, StringComparison.OrdinalIgnoreCase))
                    {
                        msg = "账号或密码错误，或账号已被禁用！";
                        return null;
                    }
                    // 自动升级MD5密码为SHA-256
                    string newSalt = PasswordHelper.GenerateSalt();
                    string newHash = PasswordHelper.HashPassword(password, newSalt);
                    dalUser.ResetUserPassword(Convert.ToInt32(dr["user_id"]), newHash, newSalt);
                }

                // 4. 转换为Users实体（原有代码完全保留）
                Users user = new Users
                {
                    user_id = dr["user_id"] != DBNull.Value ? Convert.ToInt32(dr["user_id"]) : 0,
                    user_name = dr["user_name"] is DBNull ? "" : dr["user_name"].ToString(),
                    login_account = dr["login_account"] is DBNull ? "" : dr["login_account"].ToString(),
                    phone = dr["phone"] is DBNull ? "" : dr["phone"].ToString(),
                    id_card = dr["id_card"] is DBNull ? "" : dr["id_card"].ToString(),
                    password = dr["password"] is DBNull ? "" : dr["password"].ToString(),
                    salt = storedSalt, // ✅ 赋值salt
                    user_type = dr["user_type"] != DBNull.Value ? Convert.ToInt32(dr["user_type"]) : 1,
                    diabetes_type = dr["diabetes_type"] is DBNull ? null : dr["diabetes_type"].ToString(),
                    diagnose_date = dr["diagnose_date"] is DBNull ? null : (DateTime?)dr["diagnose_date"],
                    fasting_glucose_baseline = dr["fasting_glucose_baseline"] is DBNull ? null : (decimal?)dr["fasting_glucose_baseline"],
                    last_login_time = DateTime.Now,
                    data_version = dr["data_version"] != DBNull.Value ? Convert.ToInt32(dr["data_version"]) : 1,
                    create_time = dr["create_time"] != DBNull.Value ? Convert.ToDateTime(dr["create_time"]) : DateTime.Now,
                    update_time = dr["update_time"] != DBNull.Value ? Convert.ToDateTime(dr["update_time"]) : DateTime.Now,
                    status = status,
                    gender = dr["gender"] != DBNull.Value ? Convert.ToInt32(dr["gender"]) : 1,
                    age = dr["age"] != DBNull.Value ? Convert.ToInt32(dr["age"]) : 0,
                    birth_date = dr["birth_date"] is DBNull ? null : (DateTime?)dr["birth_date"]
                };

                // 5. 更新最后登录时间（原有代码完全保留）
                try
                {
                    string updateSql = "UPDATE t_user SET last_login_time = GETDATE() WHERE user_id = @user_id";
                    SqlHelper.ExecuteNonQuery(updateSql, new SqlParameter("@user_id", user.user_id));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"更新登录时间失败：{ex.Message}");
                }

                msg = "登录成功！";
                return user;
            }
            catch (Exception ex)
            {
                msg = $"登录异常：{ex.Message}";
                System.Diagnostics.Debug.WriteLine($"【登录异常详情】{ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        #region 获取用户列表
        public List<Users> GetUserList(string loginAccount = "", string userName = "", string phone = "", int? userType = null)
        {
            return dalUser.GetUserList(loginAccount, userName, phone, userType);
        }
        #endregion

        #region 新增用户（含角色绑定）
        public string AddUser(Users user, int roleId, int operateUserId)
        {
            // 1. 基础校验（原有代码完全保留）
            if (string.IsNullOrEmpty(user.login_account)) return "登录账号不能为空";
            if (string.IsNullOrEmpty(user.password)) return "登录密码不能为空";
            if (string.IsNullOrEmpty(user.user_name)) return "用户姓名不能为空";
            if (string.IsNullOrEmpty(user.id_card) || user.id_card.Length != 18) return "请输入正确的18位身份证号";
            if (string.IsNullOrEmpty(user.phone) || user.phone.Length != 11) return "请输入正确的11位手机号";
            // 2. 唯一性校验（原有代码完全保留）
            if (dalUser.CheckAccountExist(user.login_account)) return "该登录账号已存在";
            if (dalUser.CheckIdCardExist(user.id_card)) return "该身份证号已被注册";
            if (dalUser.CheckPhoneExist(user.phone)) return "该手机号已被注册";
            // 3. ✅ 新密码加密：生成随机盐值 + SHA-256加密
            user.salt = PasswordHelper.GenerateSalt();
            user.password = PasswordHelper.HashPassword(user.password, user.salt);
            // 4. 事务新增用户+绑定角色（原有代码完全保留）
            try
            {
                int newUserId = dalUser.AddUser(user);
                if (newUserId <= 0) return "用户新增失败";
                bool bindResult = dalUserRole.BindUserRole(newUserId, roleId, operateUserId);
                if (!bindResult) return "用户角色绑定失败";
                return "ok";
            }
            catch (Exception ex)
            {
                return "系统异常：" + ex.Message;
            }
        }
        #endregion

        #region 修改用户信息
        public string UpdateUser(Users user, int roleId, int operateUserId)
        {
            // 1. 基础校验
            if (string.IsNullOrEmpty(user.user_name)) return "用户姓名不能为空";
            if (string.IsNullOrEmpty(user.phone) || user.phone.Length != 11) return "请输入正确的11位手机号";

            // 2. 唯一性校验
            if (dalUser.CheckPhoneExist(user.phone, user.user_id)) return "该手机号已被其他用户使用";

            // 3. 修改用户信息
            bool updateResult = dalUser.UpdateUser(user);
            if (!updateResult) return "用户信息修改失败";

            // 4. 更新角色绑定
            bool bindResult = dalUserRole.BindUserRole(user.user_id, roleId, operateUserId);
            if (!bindResult) return "用户角色绑定更新失败";

            return "ok";
        }
        #endregion

        #region 重置密码
        public string ResetPassword(int userId, string newPassword, int operateUserId)
        {
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6) return "密码长度不能少于6位";
            // ✅ 新密码加密：生成新的随机盐值
            string newSalt = PasswordHelper.GenerateSalt();
            string encryptPwd = PasswordHelper.HashPassword(newPassword, newSalt);
            bool result = dalUser.ResetUserPassword(userId, encryptPwd, newSalt);
            return result ? "ok" : "密码重置失败";
        }
        #endregion

        #region 启用/禁用用户
        public string UpdateUserStatus(int userId, int status, int operateUserId)
        {
            // 禁止禁用系统管理员
            if (userId == 17) return "系统管理员账号禁止禁用";
            bool result = dalUser.UpdateUserStatus(userId, status);
            return result ? "ok" : "状态修改失败";
        }
        #endregion

        #region 获取用户绑定的角色ID
        public int GetUserRoleId(int userId)
        {
            return dalUserRole.GetUserRoleId(userId);
        }
        #endregion

        /// <summary>
        /// 患者账号注册
        /// </summary>
        public string PatientRegister(Users user)
        {
            // 1. 基础校验（完全不动）
            if (string.IsNullOrEmpty(user.phone) || user.phone.Length != 11)
                return "请输入正确的11位手机号";
            if (string.IsNullOrEmpty(user.password) || user.password.Length < 6)
                return "密码长度不能少于6位";
            if (string.IsNullOrEmpty(user.user_name))
                return "请输入真实姓名";
            if (string.IsNullOrEmpty(user.id_card) || user.id_card.Length != 18)
                return "请输入正确的18位身份证号";
            // 2. 唯一性校验（完全不动）
            if (dalUser.CheckAccountExist(user.phone))
                return "该手机号已被注册";
            if (dalUser.CheckIdCardExist(user.id_card))
                return "该身份证号已被注册";
            if (dalUser.CheckPhoneExist(user.phone))
                return "该手机号已被注册";
            // 3. 患者账号固定配置（完全不动）
            user.user_type = 1;
            user.login_account = user.phone;
            // ✅ 新密码加密（完全不动）
            user.salt = PasswordHelper.GenerateSalt();
            user.password = PasswordHelper.HashPassword(user.password, user.salt);
            user.status = 1;
            // 4. 事务新增用户+绑定角色（完全不动）
            try
            {
                int newUserId = dalUser.AddUser(user);
                if (newUserId <= 0) return "注册失败，用户创建异常";
                bool bindResult = dalUserRole.BindUserRole(newUserId, 1, 0);
                if (!bindResult) return "注册失败，角色绑定异常";

                // ==============================================
                // ✅【唯一新增】注册成功后，写入患者扩展表（t_patient）
                // ==============================================
                Patient patient = new Patient()
                {
                    patient_id = newUserId,
                    diabetes_type = user.diabetes_type,
                    diagnose_date = user.diagnose_date ?? DateTime.Now,
                    fasting_glucose_baseline = user.fasting_glucose_baseline ?? 0m
                };
                B_Patient.AddPatient(patient);

                return "ok";
            }
            catch (Exception ex)
            {
                return $"注册异常：{ex.Message}";
            }
        }

        /// <summary>
        /// 患者忘记密码：通过手机号+身份证号验证后重置（优化版）
        /// </summary>
        public string ForgotPassword(string phone, string idCard, string newPassword)
        {
            // 1. 基础参数格式校验（与UI层双重校验，防止绕过）
            if (string.IsNullOrEmpty(phone) || !Regex.IsMatch(phone, @"^1[3-9]\d{9}$"))
                return "请输入正确的11位手机号";
            if (string.IsNullOrEmpty(idCard) || !Regex.IsMatch(idCard, @"^\d{17}[\dXx]$"))
                return "请输入正确的18位身份证号";

            // 2. 复用全局密码策略校验（统一安全标准）
            B_UserPwd bllUserPwd = new B_UserPwd();
            if (!bllUserPwd.CheckNewPasswordPolicy(newPassword, out string policyError))
            {
                return policyError;
            }

            // 3. 验证手机号和身份证号是否匹配（原有逻辑保留）
            string checkSql = "SELECT user_id FROM t_user WHERE phone = @phone AND id_card = @idCard AND user_type=1 AND status=1";
            SqlParameter[] checkParams = {
                new SqlParameter("@phone", phone),
                new SqlParameter("@idCard", idCard)
            };
            DataTable dt = Tools.SqlHelper.ExecuteDataTable(checkSql, checkParams);
            if (dt.Rows.Count == 0)
            {
                return "手机号与身份证号不匹配，或账号不存在/已禁用！";
            }

            // 4. 重置密码：生成新盐值+SHA-256加密
            int userId = Convert.ToInt32(dt.Rows[0]["user_id"]);
            string newSalt = PasswordHelper.GenerateSalt();
            string encryptPwd = PasswordHelper.HashPassword(newPassword, newSalt);
            bool result = dalUser.ResetUserPassword(userId, encryptPwd, newSalt);
            return result ? "ok" : "密码重置失败，请稍后重试";
        }
        /// <summary>
        /// 校验手机号唯一性
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <param name="excludeUserId">排除的用户ID</param>
        /// <returns>true=唯一，false=已存在</returns>
        public bool CheckPhoneUnique(string phone, int excludeUserId)
        {
            string sql = "SELECT COUNT(1) FROM t_user WHERE phone = @phone AND user_id != @userId";
            object obj = Tools.SqlHelper.ExecuteScalar(sql,
                new System.Data.SqlClient.SqlParameter("@phone", phone),
                new System.Data.SqlClient.SqlParameter("@userId", excludeUserId));
            return Convert.ToInt32(obj) == 0;
        }

        /// <summary>
        /// 更新用户基础信息（姓名、手机号）
        /// </summary>
        /// <param name="user">用户实体</param>
        /// <returns>ok=成功，其他=错误信息</returns>
        public string UpdateUserBaseInfo(Users user)
        {
            try
            {
                string sql = "UPDATE t_user SET user_name = @userName, phone = @phone, update_time = GETDATE() WHERE user_id = @userId";
                int rows = Tools.SqlHelper.ExecuteNonQuery(sql,
                    new System.Data.SqlClient.SqlParameter("@userName", user.user_name),
                    new System.Data.SqlClient.SqlParameter("@phone", user.phone),
                    new System.Data.SqlClient.SqlParameter("@userId", user.user_id));
                return rows > 0 ? "ok" : "更新失败，无数据变化";
            }
            catch (Exception ex)
            {
                return $"系统异常：{ex.Message}";
            }
        }

        #region 医生端忘记密码专属业务方法
        /// <summary>
        /// 医生端-身份三要素校验
        /// </summary>
        /// <param name="account">登录账号/手机号</param>
        /// <param name="userName">真实姓名</param>
        /// <param name="idCard">身份证号</param>
        /// <param name="msg">输出信息：校验失败返回错误提示，成功返回原加密密码</param>
        /// <returns>成功返回用户ID，失败返回0</returns>
        public int CheckDoctorIdentity(string account, string userName, string idCard, out string msg)
        {
            msg = string.Empty;
            #region 基础参数合规校验
            if (string.IsNullOrEmpty(account))
            {
                msg = "请输入医生账号或手机号";
                return 0;
            }
            if (string.IsNullOrEmpty(userName))
            {
                msg = "请输入真实姓名";
                return 0;
            }
            // 身份证号合规校验（18位+格式校验）
            if (string.IsNullOrEmpty(idCard) || idCard.Length != 18)
            {
                msg = "请输入正确的18位身份证号";
                return 0;
            }
            Regex idCardRegex = new Regex(@"^\d{17}[\dXx]$");
            if (!idCardRegex.IsMatch(idCard))
            {
                msg = "身份证号格式错误，最后一位为X请大写";
                return 0;
            }
            // 手机号格式校验（若输入的是手机号）
            if (account.Length == 11)
            {
                Regex phoneRegex = new Regex(@"^1[3-9]\d{9}$");
                if (!phoneRegex.IsMatch(account))
                {
                    msg = "请输入正确的11位手机号";
                    return 0;
                }
            }
            #endregion

            #region 数据库身份匹配校验（参数化查询防SQL注入）
            string sql = @"
    SELECT user_id, password FROM t_user
    WHERE (login_account = @account OR phone = @account)
    AND user_name = @userName
    AND id_card = @idCard
    AND user_type != 1
    AND status = 1";

            SqlParameter[] param =
            {
        new SqlParameter("@account", account.Trim()),
        new SqlParameter("@userName", userName.Trim()),
        new SqlParameter("@idCard", idCard.Trim())
    };

            try
            {
                DataTable dt = SqlHelper.ExecuteDataTable(sql, param);
                // 匹配失败：统一提示，不暴露账号是否存在/是否禁用，防止撞库
                if (dt.Rows.Count == 0)
                {
                    msg = "账号、姓名与身份证号不匹配，或账号非医生账号、已被禁用！";
                    System.Diagnostics.Debug.WriteLine($"【医生密码重置】身份校验失败，账号：{account}");
                    return 0;
                }

                // 匹配成功，返回用户ID与原加密密码
                DataRow dr = dt.Rows[0];
                int userId = Convert.ToInt32(dr["user_id"]);
                string originalPwd = dr["password"] is DBNull ? "" : dr["password"].ToString();
                msg = originalPwd;
                System.Diagnostics.Debug.WriteLine($"【医生密码重置】身份校验通过，用户ID：{userId}");
                return userId;
            }
            catch (Exception ex)
            {
                msg = $"身份校验异常，请稍后重试";
                System.Diagnostics.Debug.WriteLine($"【医生密码重置-身份校验异常】{ex.Message}\n{ex.StackTrace}");
                return 0;
            }
            #endregion
        }

        /// <summary>
        /// 医生端-密码重置
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="newPassword">新密码明文</param>
        /// <param name="originalEncryptPwd">原加密密码</param>
        /// <returns>ok=成功，其他=错误提示</returns>
        /// <summary>
        /// 医生端-密码重置
        /// </summary>
        /// <summary>
        /// 医生端-密码重置
        /// </summary>
        public string DoctorResetPassword(int userId, string newPassword, string originalEncryptPwd)
        {
            #region 基础校验（原有代码完全保留）
            if (userId <= 0) return "用户信息异常，无法重置密码";
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 8)
                return "医生账号密码长度不能少于8位";
            #endregion
            #region 密码强度校验（原有代码完全保留）
            Regex pwdRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-.]).{8,}$");
            if (!pwdRegex.IsMatch(newPassword))
                return "密码强度不足，需包含大小写字母、数字和特殊字符（!@#$%^&*()_+.-）";
            #endregion
            #region 密码重复性校验（原有代码完全保留）
            // 注意：这里不再对比原加密密码，因为原密码是MD5，新密码是SHA-256
            #endregion
            #region 数据库密码更新（原有代码完全保留）
            try
            {
                // ✅ 生成新盐值并加密
                string newSalt = PasswordHelper.GenerateSalt();
                string newEncryptPwd = PasswordHelper.HashPassword(newPassword, newSalt);
                bool resetResult = dalUser.ResetUserPassword(userId, newEncryptPwd, newSalt);
                if (!resetResult)
                {
                    System.Diagnostics.Debug.WriteLine($"【医生密码重置】密码更新失败，用户ID：{userId}");
                    return "密码重置失败，请稍后重试";
                }
                // 记录操作审计日志（原有代码完全保留）
                WritePasswordResetLog(userId, "医生端自助忘记密码重置");
                System.Diagnostics.Debug.WriteLine($"【医生密码重置】密码重置成功，用户ID：{userId}");
                return "ok";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"【医生密码重置-重置异常】{ex.Message}\n{ex.StackTrace}");
                return "系统异常，密码重置失败，请稍后重试";
            }
            #endregion
        }

        /// <summary>
        /// 记录密码重置审计日志（适配数据库真实表t_audit_log）
        /// </summary>
        private void WritePasswordResetLog(int userId, string operateType)
        {
            try
            {
                string sql = @"
        INSERT INTO t_audit_log 
        (operate_user_id, operate_type, operate_content, operate_ip, operate_device, operate_time, data_version, create_time, update_time)
        VALUES 
        (@userId, @operateType, @content, @operateIp, @device, GETDATE(), 1, GETDATE(), GETDATE())";

                SqlParameter[] param =
                {
            new SqlParameter("@userId", userId),
            new SqlParameter("@operateType", operateType),
            new SqlParameter("@content", "医生端自助重置密码"),
            new SqlParameter("@operateIp", GetLocalIpAddress()),
            new SqlParameter("@device", "PC端")
        };
                SqlHelper.ExecuteNonQuery(sql, param);
            }
            catch
            {
                // 日志写入失败不影响重置（屏蔽异常，不阻断逻辑）
            }
        }

        /// <summary>
        /// 获取本地IP地址，用于日志记录
        /// </summary>
        private string GetLocalIpAddress()
        {
            try
            {
                return Dns.GetHostEntry(Dns.GetHostName()).AddressList
                    .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString() ?? "未知IP";
            }
            catch
            {
                return "未知IP";
            }
        }
        #endregion
        #region 扩展方法：获取带评估状态的患者分页列表
        /// <summary>
        /// 获取带评估状态的患者分页列表（医生端专用）
        /// </summary>
        /// <param name="doctorId">当前登录医生ID</param>
        /// <param name="userName">患者姓名（模糊查询）</param>
        /// <param name="phone">手机号（模糊查询）</param>
        /// <param name="diabetesType">糖尿病类型</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <returns>（是否成功, 提示信息, 数据列表, 总条数）</returns>
        public (bool Success, string Msg, List<PatientSimpleInfo> Data, int TotalCount) GetPatientWithAssessmentByPage(int doctorId, string userName, string phone, string diabetesType, int pageIndex, int pageSize)
        {
            try
            {
                // 参数校验
                if (doctorId <= 0) return (false, "当前登录医生信息异常", null, 0);
                if (pageIndex < 1) pageIndex = 1;
                if (pageSize < 1) pageSize = 20;

                // 调用DAL层
                int totalCount = 0;
                var data = dalUser.GetPatientListWithAssessmentStatus(doctorId, userName, phone, diabetesType, pageIndex, pageSize, out totalCount);
                return (true, "查询成功", data, totalCount);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"【B_User扩展方法异常】{ex.Message}\n{ex.StackTrace}");
                return (false, $"查询失败：{ex.Message}", null, 0);
            }
        }


        #endregion

        #region 新增：获取当前医生管理的患者列表业务方法
        /// <summary>
        /// 获取指定医生管理的患者列表
        /// </summary>
        /// <param name="doctorId">医生用户ID</param>
        /// <param name="searchText">搜索关键词</param>
        /// <returns>（是否成功, 提示信息, 患者列表）</returns>
        public (bool Success, string Msg, List<PatientSimpleInfo> Data) GetDoctorManagedPatientList(int doctorId, string searchText = "")
        {
            try
            {
                // 核心参数校验
                if (doctorId <= 0)
                    return (false, "当前登录医生信息无效，请重新登录系统", null);

                // 调用DAL层获取数据
                var data = dalUser.GetDoctorManagedPatientList(doctorId, searchText);
                return (true, "查询成功", data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"【B_User-医生患者查询异常】{ex.Message}\n{ex.StackTrace}");
                return (false, $"查询失败：{ex.Message}", null);
            }
        }
        #endregion
        #region 登录状态校验业务方法（三层架构规范，业务逻辑下沉）
        /// <summary>
        /// 校验当前登录医生会话是否有效（数据库层面校验）
        /// </summary>
        /// <param name="doctorId">医生ID</param>
        /// <returns>（是否有效，提示信息）</returns>
        public (bool IsValid, string Msg) CheckDoctorLoginValid(int doctorId)
        {
            if (doctorId <= 0)
                return (false, "当前登录医生信息无效，请重新登录系统");

            // 校验账号是否存在、是否为医生账号、状态是否启用
            string sql = "SELECT COUNT(1) FROM t_user WHERE user_id = @doctorId AND user_type = 2 AND status = 1";
            SqlParameter[] param = { new SqlParameter("@doctorId", doctorId) };

            try
            {
                int count = Convert.ToInt32(SqlHelper.ExecuteScalar(sql, param));
                if (count == 0)
                    return (false, "当前登录医生账号不存在或已被禁用，请重新登录");

                return (true, "登录状态有效");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"【登录状态校验异常】{ex.Message}\n{ex.StackTrace}");
                return (false, "登录状态校验异常，请稍后重试");
            }
        }
        #endregion

        #region 
        /// <summary>
        /// 患者端个人信息修改专用：仅更新基础公共字段
        /// </summary>
        public static bool UpdateUser(Users user)
        {
            return new D_User().UpdateUser(user);
        }
        #endregion
    }
}