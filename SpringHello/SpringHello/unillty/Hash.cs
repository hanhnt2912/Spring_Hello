using System;
using System.Security.Cryptography;

namespace SpringHello.utillty
{
    public class Hash
    {
        public static string EncryptedString(string content, string salt)
        {
            String str_md5 = "";
            Byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content + salt);
            MD5CryptoServiceProvider md5CryptoServiceProvider = new MD5CryptoServiceProvider();
            buffer = md5CryptoServiceProvider.ComputeHash(buffer);
            foreach (Byte b in buffer)
            {
                str_md5 += b.ToString("X2");
            }

            return str_md5;
        }
    }
}