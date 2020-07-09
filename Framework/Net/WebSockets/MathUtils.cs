namespace Framework.Net.WebSockets
{
    internal static class MathUtils
    {
        public static int IndexOf(byte[] bytes, int offset, int length, byte[] pattern)
        {
            int index = -1;
            int pos = offset;
            if (length > bytes.Length)
            {
                length = bytes.Length;
            }
            while (pos < length)
            {
                if (bytes[pos] == pattern[0])
                {
                    index = pos;
                    for (int i = 1; i < pattern.Length; i++)
                    {
                        if (pos + i >= length || pattern[i] != bytes[pos + i])
                        {
                            index = -1;
                            break;
                        }
                    }
                    if (index > -1)
                    {
                        break;
                    }
                }
                pos++;
            }
            return index;
        }

        public static int IndexOf(byte[] bytes, byte[] pattern)
        {
            return IndexOf(bytes, 0, bytes.Length, pattern);
        }
    }
}
