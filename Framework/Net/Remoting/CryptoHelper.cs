using System;
using System.Security.Cryptography;
using System.Text;

namespace Framework.Net.Remoting
{
    static class CryptoHelper
    {
        public static string Md5(string str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            var byteForAry = Encoding.ASCII.GetBytes(str);
            var md5 = new MD5CryptoServiceProvider();
            byteForAry = md5.ComputeHash(byteForAry);
            md5.Dispose();
            var builder = new StringBuilder();
            foreach (var by in byteForAry)
                builder.Append(by.ToString("x2"));

            return builder.ToString();
        }
    }
}