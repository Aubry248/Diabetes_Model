using System;
using System.Data.SqlClient;
using Tools;

namespace BLL
{
    /// <summary>
    /// 用户密码管理业务逻辑层
    /// </summary>
    public class B_UserPwd
    {
        #region 核心方法：原密码合法性校验
        /// <summary>
        /// 校验原密码是否正确，同时处理失败次数与账号锁定
        /// </summary>
        /// <param name="userId">当前登录用户ID</param>
        /// <param name="originalPwd">用户输入的原密码（明文）</param>
        /// <param name="errorMsg">输出错误信息</param>
        /// <returns>校验是否通过</returns>
        public bool CheckOriginalPassword(int userId, string originalPwd, out string errorMsg)
        {
            errorMsg = "";
            try
            {
                // 1. 先获取用户的 salt 和 password
                string userSql = "SELECT salt, password, lock_end_time, login_fail_count, status FROM t_user WHERE user_id = @userId";
                SqlParameter[] userParam = { new SqlParameter("@userId", userId) };
                var dt = SqlHelper.ExecuteDataTable(userSql, userParam);
                if (dt.Rows.Count == 0)
                {
                    errorMsg = "账号不存在或已被禁用！";
                    return false;
                }

                string dbSalt = dt.Rows[0]["salt"].ToString();
                string dbPassword = dt.Rows[0]["password"].ToString();
                object lockEndTimeObj = dt.Rows[0]["lock_end_time"];
                int failCount = Convert.ToInt32(dt.Rows[0]["login_fail_count"]);

                // 2. 判断账号是否锁定
                if (lockEndTimeObj != DBNull.Value && Convert.ToDateTime(lockEndTimeObj) > DateTime.Now)
                {
                    TimeSpan remainTime = Convert.ToDateTime(lockEndTimeObj) - DateTime.Now;
                    errorMsg = $"账号已被锁定，请{remainTime.Minutes}分钟后再试！";
                    return false;
                }

                // 3. 【关键】按数据库规则计算 SHA256(明文 + salt) 小写无0x
                string pwdWithSalt = originalPwd + dbSalt;
                string inputPwdHash = BitConverter.ToString(System.Security.Cryptography.SHA256.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(pwdWithSalt))).Replace("-", "").ToLower();

                // 4. 和数据库密码比对
                if (inputPwdHash != dbPassword)
                {
                    int maxFailTimes = SystemGlobalConfig.LoginFailMaxTimes;
                    int lockMinutes = SystemGlobalConfig.AccountLockMinutes;
                    int newFailCount = failCount + 1;

                    if (newFailCount >= maxFailTimes)
                    {
                        string updateLockSql = @"UPDATE t_user 
                                        SET login_fail_count = @newFailCount, lock_end_time = @lockEndTime, update_time = GETDATE()
                                        WHERE user_id = @userId";
                        SqlParameter[] updateLockParam = {
                    new SqlParameter("@newFailCount", newFailCount),
                    new SqlParameter("@lockEndTime", DateTime.Now.AddMinutes(lockMinutes)),
                    new SqlParameter("@userId", userId)
                };
                        SqlHelper.ExecuteNonQuery(updateLockSql, updateLockParam);
                        errorMsg = $"原密码错误，已连续失败{newFailCount}次，账号已被锁定{lockMinutes}分钟！";
                    }
                    else
                    {
                        string updateFailSql = "UPDATE t_user SET login_fail_count = @newFailCount, update_time = GETDATE() WHERE user_id = @userId";
                        SqlParameter[] updateFailParam = {
                    new SqlParameter("@newFailCount", newFailCount),
                    new SqlParameter("@userId", userId)
                };
                        SqlHelper.ExecuteNonQuery(updateFailSql, updateFailParam);
                        errorMsg = $"原密码错误，还可尝试{maxFailTimes - newFailCount}次！";
                    }
                    WritePwdAuditLog(userId, "密码修改失败", $"原密码校验失败，失败原因：{errorMsg}", "");
                    return false;
                }

                // 5. 校验成功，清空失败次数
                string resetFailSql = "UPDATE t_user SET login_fail_count = 0, lock_end_time = NULL, update_time = GETDATE() WHERE user_id = @userId";
                SqlParameter[] resetFailParam = { new SqlParameter("@userId", userId) };
                SqlHelper.ExecuteNonQuery(resetFailSql, resetFailParam);
                return true;
            }
            catch (Exception ex)
            {
                errorMsg = $"原密码校验异常：{ex.Message}";
                WritePwdAuditLog(userId, "密码修改失败", $"原密码校验异常：{ex.Message}", "");
                return false;
            }
        }
        #endregion

        #region 核心方法：新密码策略合规性校验
        /// <summary>
        /// 校验新密码是否符合系统配置的密码策略
        /// </summary>
        /// <param name="newPwd">新密码明文</param>
        /// <param name="errorMsg">输出不合规原因</param>
        /// <returns>是否合规</returns>
        public bool CheckNewPasswordPolicy(string newPwd, out string errorMsg)
        {
            errorMsg = "";
            if (string.IsNullOrWhiteSpace(newPwd))
            {
                errorMsg = "新密码不能为空！";
                return false;
            }

            int minLength = SystemGlobalConfig.PwdMinLength;
            bool requireUpperLower = SystemGlobalConfig.PwdRequireUpperLower;
            bool requireNumber = SystemGlobalConfig.PwdRequireNumber;
            bool requireSpecialChar = SystemGlobalConfig.PwdRequireSpecialChar;

            // 1. 最小长度校验
            if (newPwd.Length < minLength)
            {
                errorMsg = $"密码长度不能少于{minLength}位！";
                return false;
            }

            // 2. 大小写字母校验
            if (requireUpperLower)
            {
                bool hasUpper = false;
                bool hasLower = false;
                foreach (char c in newPwd)
                {
                    if (char.IsUpper(c)) hasUpper = true;
                    if (char.IsLower(c)) hasLower = true;
                }
                if (!hasUpper || !hasLower)
                {
                    errorMsg = "密码必须同时包含大写和小写字母！";
                    return false;
                }
            }

            // 3. 数字校验
            if (requireNumber)
            {
                bool hasNumber = false;
                foreach (char c in newPwd)
                {
                    if (char.IsDigit(c))
                    {
                        hasNumber = true;
                        break;
                    }
                }
                if (!hasNumber)
                {
                    errorMsg = "密码必须包含数字！";
                    return false;
                }
            }

            // 4. 特殊字符校验
            if (requireSpecialChar)
            {
                string specialChars = "~!@#$%^&*()_+-=[]{}|;':\",./<>?";
                bool hasSpecial = false;
                foreach (char c in newPwd)
                {
                    if (specialChars.Contains(c.ToString()))
                    {
                        hasSpecial = true;
                        break;
                    }
                }
                if (!hasSpecial)
                {
                    errorMsg = "密码必须包含特殊字符（如!@#$%^&*等）！";
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region 核心方法：执行密码修改
        /// <summary>
        /// 执行密码修改，更新数据库
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="newPwd">新密码明文</param>
        /// <param name="isFirstLogin">是否首次登录改密</param>
        /// <param name="clientIP">客户端IP（从UI层传入）</param>
        /// <param name="errorMsg">输出错误信息</param>
        /// <returns>修改是否成功</returns>
        public bool ChangePassword(int userId, string newPwd, bool isFirstLogin, string clientIP, out string errorMsg)
        {
            errorMsg = "";
            try
            {
                // 1. 先获取用户的 salt
                string getSaltSql = "SELECT salt FROM t_user WHERE user_id = @userId AND status = 1";
                SqlParameter[] saltParam = { new SqlParameter("@userId", userId) };
                var saltDt = SqlHelper.ExecuteDataTable(getSaltSql, saltParam);
                if (saltDt.Rows.Count == 0)
                {
                    errorMsg = "账号不存在或已被禁用！";
                    WritePwdAuditLog(userId, "密码修改失败", errorMsg, clientIP);
                    return false;
                }
                string dbSalt = saltDt.Rows[0]["salt"].ToString();

                // 2. 【关键】按数据库规则加密新密码
                string pwdWithSalt = newPwd + dbSalt;
                string encryptNewPwd = BitConverter.ToString(System.Security.Cryptography.SHA256.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(pwdWithSalt))).Replace("-", "").ToLower();

                // 3. 更新数据库
                string updateSql = @"UPDATE t_user 
                            SET password = @newPwd, 
                                is_first_login = 0, 
                                last_pwd_change_time = GETDATE(), 
                                data_version = data_version + 1, 
                                update_time = GETDATE()
                            WHERE user_id = @userId AND status = 1";
                SqlParameter[] updateParam = {
            new SqlParameter("@newPwd", encryptNewPwd),
            new SqlParameter("@userId", userId)
        };
                int affectRows = SqlHelper.ExecuteNonQuery(updateSql, updateParam);

                if (affectRows > 0)
                {
                    string operateContent = isFirstLogin ? "首次登录强制修改密码成功" : "主动修改密码成功";
                    WritePwdAuditLog(userId, "密码修改成功", operateContent, clientIP);
                    return true;
                }
                else
                {
                    errorMsg = "密码修改失败，账号不存在或已被禁用！";
                    WritePwdAuditLog(userId, "密码修改失败", errorMsg, clientIP);
                    return false;
                }
            }
            catch (Exception ex)
            {
                errorMsg = $"密码修改异常：{ex.Message}";
                WritePwdAuditLog(userId, "密码修改失败", errorMsg, clientIP);
                return false;
            }
        }
        #endregion

        #region 辅助方法：判断是否需要强制改密
        /// <summary>
        /// 判断用户是否需要强制改密（首次登录/密码过期）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="isForceChange">输出是否需要强制改密</param>
        /// <param name="forceReason">输出强制改密原因</param>
        /// <param name="isFirstLogin">输出是否为首次登录</param>
        public void CheckNeedForceChangePwd(int userId, out bool isForceChange, out string forceReason, out bool isFirstLogin)
        {
            isForceChange = false;
            forceReason = "";
            isFirstLogin = false;
            try
            {
                string sql = "SELECT is_first_login, last_pwd_change_time FROM t_user WHERE user_id = @userId AND status = 1";
                SqlParameter[] param = { new SqlParameter("@userId", userId) };
                var dt = SqlHelper.ExecuteDataTable(sql, param);
                if (dt.Rows.Count == 0) return;

                // 1. 首次登录强制改密
                int firstLoginFlag = Convert.ToInt32(dt.Rows[0]["is_first_login"]);
                isFirstLogin = firstLoginFlag == 1;
                if (isFirstLogin)
                {
                    isForceChange = true;
                    forceReason = "您是首次登录系统，为保障账号安全，请立即修改初始密码！";
                    return;
                }

                // 2. 密码过期强制改密
                int pwdCycleDays = SystemGlobalConfig.PwdForceChangeCycleDays;
                if (pwdCycleDays > 0 && dt.Rows[0]["last_pwd_change_time"] != DBNull.Value)
                {
                    DateTime lastChangeTime = Convert.ToDateTime(dt.Rows[0]["last_pwd_change_time"]);
                    if ((DateTime.Now - lastChangeTime).TotalDays >= pwdCycleDays)
                    {
                        isForceChange = true;
                        forceReason = $"您的密码已超过{pwdCycleDays}天有效期，为保障账号安全，请立即修改密码！";
                    }
                }
            }
            catch
            {
                isForceChange = false;
                forceReason = "";
                isFirstLogin = false;
            }
        }
        #endregion

        #region 私有方法：写入密码操作审计日志
        private void WritePwdAuditLog(int userId, string operateType, string operateContent, string clientIP)
        {
            try
            {
                string sql = @"INSERT INTO t_audit_log 
                                (operate_user_id, operate_type, operate_content, operate_ip, operate_device, operate_time, remark, data_version, create_time, update_time)
                                VALUES (@operateUserId, @operateType, @operateContent, @operateIP, 'PC端', GETDATE(), '', 1, GETDATE(), GETDATE())";
                SqlParameter[] param = {
                    new SqlParameter("@operateUserId", userId),
                    new SqlParameter("@operateType", operateType),
                    new SqlParameter("@operateContent", operateContent),
                    new SqlParameter("@operateIP", string.IsNullOrEmpty(clientIP) ? "127.0.0.1" : clientIP)
                };
                SqlHelper.ExecuteNonQuery(sql, param);
            }
            catch { }
        }
        #endregion
    }
}