using System;

namespace Framework
{
    internal static class ByteConverter
    {
        private const char HexCharPrefix1 = '0';
        private const char HexCharPerfix2 = 'x';
        private const string HexStringPrefix = "0x";
        //private const string HexStringFormat = "X2";
        //private const string HexStringTemplate = "0123456789ABCDEF";

        public static string GetString(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            var array = new char[2 + bytes.Length * 2];
            array[0] = HexCharPrefix1;
            array[1] = HexCharPerfix2;
            for (int i = 0; i < bytes.Length; i++)
            {
                var offset = i * 2 + 2;
                var value = bytes[i];
                var high = (value >> 4) & 0xF;
                var low = value & 0xF;
                array[offset] = high <= 9 ? (char)(high + 48) : (char)(high + 55);
                array[offset + 1] = low <= 9 ? (char)(low + 48) : (char)(low + 55);
            }

            return new string(array);
        }

        public static byte[] GetBytes(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (!text.StartsWith(HexStringPrefix))
                throw new FormatException(nameof(text));

            var bytes = new byte[text.Length / 2 - 1];
            for (int i = 1; i <= bytes.Length; i++)
            {
                var offset = i * 2;
                var value = (short)text[offset];
                var high = 48 <= value && value <= 57 ? value - 48 : 65 <= value && value <= 70 ? value - 55 : throw new FormatException(text);
                value = (short)text[offset + 1];
                var low = 48 <= value && value <= 57 ? value - 48 : 65 <= value && value <= 70 ? value - 55 : throw new FormatException(text);
                bytes[i - 1] = checked((byte)(high * 16 + low));
            }

            return bytes;
        }

        public static void SetBytes(byte[] array, int offset, short value)
        {
            array[offset + 0] = (byte)((value >> 0) & 0xFF);
            array[offset + 1] = (byte)((value >> 8) & 0xFF);
        }

        public static void SetBytes(byte[] array, int offset, ushort value)
        {
            array[offset + 0] = (byte)((value >> 0) & 0xFF);
            array[offset + 1] = (byte)((value >> 8) & 0xFF);
        }

        public static void SetBytes(byte[] array, int offset, int value)
        {
            array[offset + 0] = (byte)((value >> 0) & 0xFF);
            array[offset + 1] = (byte)((value >> 8) & 0xFF);
            array[offset + 2] = (byte)((value >> 16) & 0xFF);
            array[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        public static void SetBytes(byte[] array, int offset, uint value)
        {
            array[offset + 0] = (byte)((value >> 0) & 0xFF);
            array[offset + 1] = (byte)((value >> 8) & 0xFF);
            array[offset + 2] = (byte)((value >> 16) & 0xFF);
            array[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        public static void SetBytes(byte[] array, int offset, long value)
        {
            array[offset + 0] = (byte)((value >> 0) & 0xFF);
            array[offset + 1] = (byte)((value >> 8) & 0xFF);
            array[offset + 2] = (byte)((value >> 16) & 0xFF);
            array[offset + 3] = (byte)((value >> 24) & 0xFF);
            array[offset + 4] = (byte)((value >> 32) & 0xFF);
            array[offset + 5] = (byte)((value >> 40) & 0xFF);
            array[offset + 6] = (byte)((value >> 48) & 0xFF);
            array[offset + 7] = (byte)((value >> 56) & 0xFF);
        }

        public static void SetBytes(byte[] array, int offset, ulong value)
        {
            array[offset + 0] = (byte)((value >> 0) & 0xFF);
            array[offset + 1] = (byte)((value >> 8) & 0xFF);
            array[offset + 2] = (byte)((value >> 16) & 0xFF);
            array[offset + 3] = (byte)((value >> 24) & 0xFF);
            array[offset + 4] = (byte)((value >> 32) & 0xFF);
            array[offset + 5] = (byte)((value >> 40) & 0xFF);
            array[offset + 6] = (byte)((value >> 48) & 0xFF);
            array[offset + 7] = (byte)((value >> 56) & 0xFF);
        }

        public static void SetBytes(byte[] array, int offset, float value)
        {
            var bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, array, offset, sizeof(float));
        }

        public static void SetBytes(byte[] array, int offset, double value)
        {
            var bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, array, offset, sizeof(double));
        }
    }
}
