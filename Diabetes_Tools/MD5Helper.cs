using System;
using System.Security.Cryptography;
using System.Text;

public static class MD5Helper
{
    /// <summary>
    /// 32位大写MD5加密
    /// </summary>
    public static string Encrypt32(string str)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("X2")); // X2：大写两位十六进制
            }
            return sb.ToString();
        }
    }
}