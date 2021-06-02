using System;
using System.Threading;

namespace Framework.Runtime.Serialization.Protobuf
{
    internal static class BufferPool
    {
        private const int BufferLength = 1024;
        private const int PoolSize = 20;

        private static readonly byte[]?[] _pool = new byte[PoolSize][];

        private static void InternalRelease(byte[] buffer)
        {
            for (int i = 0; i < PoolSize; i++)
            {
                if (Interlocked.CompareExchange(ref _pool[i], buffer, null) == null)
                {
                    break;
                }
            }
        }

        public static byte[] GetBuffer()
        {
            byte[]? array;
            for (int i = 0; i < PoolSize; i++)
            {
                array = Interlocked.Exchange(ref _pool[i], null);
                if (array != null) return array;
            }

            return new byte[BufferLength];
        }

        public static void Release(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (buffer.Length == BufferLength)
            {
                InternalRelease(buffer);
            }
        }

        public static void Resize(ref byte[] buffer, int count, int offset, int copyedBytes)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (offset < 0 || offset >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (copyedBytes < 0 || offset + copyedBytes > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(copyedBytes));

            int newLength = buffer.Length * 2;
            if (newLength < count)
                newLength = count;

            byte[] newBuffer = new byte[newLength];
            if (copyedBytes > 0)
                Buffer.BlockCopy(buffer, offset, newBuffer, 0, copyedBytes);

            if (buffer.Length == BufferLength)
                Release(buffer);

            buffer = newBuffer;
        }
    }
}
