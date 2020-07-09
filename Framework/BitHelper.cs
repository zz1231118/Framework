using System;

namespace Framework
{
    internal static class BitHelper
    {
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
