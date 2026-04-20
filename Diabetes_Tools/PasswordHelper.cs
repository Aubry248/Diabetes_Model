using System;
using System.Security.Cryptography;
using System.Text;

namespace Tools
{
    /// <summary>
    /// 安全密码加密工具类（带随机盐值的SHA-256）
    /// 替换原有MD5Helper，完全兼容原有调用方式
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// 生成16位随机盐值（每个用户唯一）
        /// </summary>
        public static string GenerateSalt()
        {
            byte[] saltBytes = new byte[8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return BitConverter.ToString(saltBytes).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 加密密码：SHA256(明文密码 + 盐值)
        /// </summary>
        public static string HashPassword(string plainPassword, string salt)
        {
            if (string.IsNullOrEmpty(plainPassword))
                throw new ArgumentNullException(nameof(plainPassword));
            if (string.IsNullOrEmpty(salt))
                throw new ArgumentNullException(nameof(salt));

            string combined = plainPassword.Trim() + salt;
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// 验证密码
        /// </summary>
        public static bool VerifyPassword(string plainPassword, string storedHash, string storedSalt)
        {
            if (string.IsNullOrEmpty(plainPassword) || string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(storedSalt))
                return false;

            string computedHash = HashPassword(plainPassword, storedSalt);
            return computedHash.Equals(storedHash, StringComparison.OrdinalIgnoreCase);
        }

        // 【兼容原有MD5Helper调用】保留原方法名，方便批量替换
        [Obsolete("请使用HashPassword方法替代", false)]
        public static string Encrypt32(string plainText)
        {
            // 仅用于过渡，实际新增用户必须使用带盐的HashPassword
            return HashPassword(plainText, "default_salt_2026");
        }
    }
}