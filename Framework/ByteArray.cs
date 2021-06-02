using System;

namespace Framework
{
    internal static class ByteArray
    {
        private const int CopyThreshold = 12;

        public static void Copy(byte[] src, int srcOffset, byte[] dst, int dstOffset, int count)
        {
            if (count > CopyThreshold) 
            {
                Buffer.BlockCopy(src, srcOffset, dst, dstOffset, count);
                return;
            }

            int stop = srcOffset + count;
            for (int i = srcOffset; i < stop; i++)
            {
                dst[dstOffset++] = src[i];
            }
        }

        public static void Reverse(byte[] bytes)
        {
            int first = 0;
            int last = bytes.Length - 1;
            while (first < last)
            {
                var temp = bytes[first];
                bytes[first] = bytes[last];
                bytes[last] = temp;
                first++;
                last--;
            }
        }
    }
}
