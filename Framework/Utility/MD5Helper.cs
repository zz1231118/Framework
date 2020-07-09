using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Framework.Utility
{
    public static class MD5Helper
    {
        private static string InternalMD5(byte[] data, int offset, int count)
        {
            byte[] bytes;
            using (var md5 = new MD5CryptoServiceProvider())
                bytes = md5.ComputeHash(data, offset, count);

            var sb = new StringBuilder(32);
            foreach (var by in bytes)
                sb.Append(by.ToString("x2"));

            return sb.ToString();
        }

        public static string MD5(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            byte[] bytes;
            using (var md5 = new MD5CryptoServiceProvider())
                bytes = md5.ComputeHash(stream);

            var sb = new StringBuilder(32);
            foreach (var by in bytes)
                sb.Append(by.ToString("x2"));

            return sb.ToString();
        }

        public static string MD5(byte[] data, int offset, int count)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (offset + count > data.Length)
                throw new ArgumentException();

            return InternalMD5(data, offset, count);
        }

        public static string MD5(string text)
        {
            var ary = Encoding.UTF8.GetBytes(text);
            return InternalMD5(ary, 0, ary.Length);
        }
    }
}