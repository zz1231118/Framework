using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Framework.IO
{
    public sealed class RecyclableMemoryStreamManager
    {
        public const int DefaultBlockSize = 131072;
        public const int DefaultLargeBufferMultiple = 1048576;
        public const int DefaultMaximumBufferSize = 134217728;

        private readonly int blockSize;
        private readonly long[] largeBufferFreeSize;
        private readonly long[] largeBufferInUseSize;
        private readonly int largeBufferMultiple;
        private readonly ConcurrentStack<byte[]>[] largePools;
        private readonly int maximumBufferSize;
        private readonly ConcurrentStack<byte[]> smallPool;
        private long smallPoolFreeSize;
        private long smallPoolInUseSize;

        public RecyclableMemoryStreamManager()
            : this(DefaultBlockSize, DefaultLargeBufferMultiple, DefaultMaximumBufferSize)
        { }

        public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize)
        {
            if (blockSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(blockSize), blockSize, "blockSize must be a positive number");
            if (largeBufferMultiple <= 0)
                throw new ArgumentOutOfRangeException(nameof(largeBufferMultiple), largeBufferMultiple, "largeBufferMultiple must be a positive number");
            if (maximumBufferSize < blockSize)
                throw new ArgumentOutOfRangeException(nameof(maximumBufferSize), maximumBufferSize, "maximumBufferSize must be at least blockSize");

            this.blockSize = blockSize;
            this.largeBufferMultiple = largeBufferMultiple;
            this.maximumBufferSize = maximumBufferSize;
            if (!IsLargeBufferMultiple(maximumBufferSize))
            {
                throw new ArgumentException("maximumBufferSize is not a multiple of largeBufferMultiple", "maximumBufferSize");
            }

            smallPool = new ConcurrentStack<byte[]>();
            int num = maximumBufferSize / largeBufferMultiple;
            largeBufferInUseSize = new long[num + 1];
            largeBufferFreeSize = new long[num];
            largePools = new ConcurrentStack<byte[]>[num];
            for (int i = 0; i < largePools.Length; i++)
            {
                largePools[i] = new ConcurrentStack<byte[]>();
            }
        }

        public int BlockSize => blockSize;

        public int LargeBufferMultiple => largeBufferMultiple;

        public int MaximumBufferSize => maximumBufferSize;

        public long SmallPoolFreeSize => smallPoolFreeSize;

        public long SmallPoolInUseSize => smallPoolInUseSize;

        public long LargePoolFreeSize => largeBufferFreeSize.Sum();

        public long LargePoolInUseSize => largeBufferInUseSize.Sum();

        public long SmallBlocksFree => smallPool.Count;

        public long LargeBuffersFree => largePools.Sum(p => (long)p.Count);

        public long MaximumFreeSmallPoolBytes { get; set; }

        public long MaximumFreeLargePoolBytes { get; set; }

        public long MaximumStreamCapacity { get; set; }

        public bool AggressiveBufferReturn { get; set; }

        public event EventHandler? BlockCreated;

        public event EventHandler? BlockDiscarded;

        public event EventHandler? LargeBufferCreated;

        public event EventHandler? StreamCreated;

        public event EventHandler? StreamDisposed;

        public event EventHandler? StreamFinalized;

        public event StreamLengthReportHandler? StreamLength;

        public event EventHandler? StreamConvertedToArray;

        public event LargeBufferDiscardedEventHandler? LargeBufferDiscarded;

        public event UsageReportEventHandler? UsageReport;

        private int RoundToLargeBufferMultiple(int requiredSize)
        {
            return (requiredSize + LargeBufferMultiple - 1) / LargeBufferMultiple * LargeBufferMultiple;
        }

        private bool IsLargeBufferMultiple(int value)
        {
            return value != 0 && value % LargeBufferMultiple == 0;
        }

        internal byte[] GetBlock()
        {
            if (!smallPool.TryPop(out byte[] result))
            {
                result = new byte[BlockSize];
                BlockCreated?.Invoke();
            }
            else
            {
                Interlocked.Add(ref smallPoolFreeSize, -BlockSize);
            }

            Interlocked.Add(ref smallPoolInUseSize, BlockSize);
            return result;
        }

        internal byte[] GetLargeBuffer(int requiredSize)
        {
            byte[] array;
            requiredSize = RoundToLargeBufferMultiple(requiredSize);
            var num = requiredSize / largeBufferMultiple - 1;
            if (num < largePools.Length)
            {
                if (!largePools[num].TryPop(out array))
                {
                    array = new byte[requiredSize];
                    LargeBufferCreated?.Invoke();
                }
                else
                {
                    Interlocked.Add(ref largeBufferFreeSize[num], -array.Length);
                }
            }
            else
            {
                num = largeBufferInUseSize.Length - 1;
                array = new byte[requiredSize];
                LargeBufferCreated?.Invoke();
            }

            Interlocked.Add(ref largeBufferInUseSize[num], array.Length);
            return array;
        }

        internal void ReturnLargeBuffer(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (!IsLargeBufferMultiple(buffer.Length))
                throw new ArgumentException("buffer did not originate from this memory manager. The size is not a multiple of " + LargeBufferMultiple);

            var num = buffer.Length / largeBufferMultiple - 1;
            if (num < largePools.Length)
            {
                if ((largePools[num].Count + 1) * buffer.Length <= MaximumFreeLargePoolBytes || MaximumFreeLargePoolBytes == 0)
                {
                    largePools[num].Push(buffer);
                    Interlocked.Add(ref largeBufferFreeSize[num], buffer.Length);
                }
                else
                {
                    LargeBufferDiscarded?.Invoke(MemoryStreamDiscardReason.EnoughFree);
                }
            }
            else
            {
                num = largeBufferInUseSize.Length - 1;
                LargeBufferDiscarded?.Invoke(MemoryStreamDiscardReason.TooLarge);
            }

            Interlocked.Add(ref largeBufferInUseSize[num], -buffer.Length);
            UsageReport?.Invoke(smallPoolInUseSize, smallPoolFreeSize, LargePoolInUseSize, LargePoolFreeSize);
        }

        internal void ReturnBlocks(ICollection<byte[]> blocks)
        {
            if (blocks == null)
                throw new ArgumentNullException(nameof(blocks));

            var num = blocks.Count * BlockSize;
            Interlocked.Add(ref smallPoolInUseSize, -num);
            foreach (var block in blocks)
            {
                if (block == null || block.Length != BlockSize)
                {
                    throw new ArgumentException("blocks contains buffers that are not BlockSize in length");
                }
            }
            foreach (byte[] block2 in blocks)
            {
                if (MaximumFreeSmallPoolBytes != 0 && SmallPoolFreeSize >= MaximumFreeSmallPoolBytes)
                {
                    BlockDiscarded?.Invoke();
                    break;
                }

                Interlocked.Add(ref smallPoolFreeSize, BlockSize);
                smallPool.Push(block2);
            }

            UsageReport?.Invoke(smallPoolInUseSize, smallPoolFreeSize, LargePoolInUseSize, LargePoolFreeSize);
        }

        internal void ReportStreamCreated()
        {
            StreamCreated?.Invoke();
        }

        internal void ReportStreamDisposed()
        {
            StreamDisposed?.Invoke();
        }

        internal void ReportStreamFinalized()
        {
            StreamFinalized?.Invoke();
        }

        internal void ReportStreamLength(long bytes)
        {
            StreamLength?.Invoke(bytes);
        }

        internal void ReportStreamToArray()
        {
            StreamConvertedToArray?.Invoke();
        }

        public MemoryStream GetStream()
        {
            return new RecyclableMemoryStream(this);
        }

        public MemoryStream GetStream(string tag)
        {
            return new RecyclableMemoryStream(this, tag);
        }

        public MemoryStream GetStream(string tag, int requiredSize)
        {
            return new RecyclableMemoryStream(this, tag, requiredSize);
        }

        public MemoryStream GetStream(string tag, int requiredSize, bool asContiguousBuffer)
        {
            if (!asContiguousBuffer || requiredSize <= BlockSize)
            {
                return GetStream(tag, requiredSize);
            }

            return new RecyclableMemoryStream(this, tag, requiredSize, GetLargeBuffer(requiredSize));
        }

        public MemoryStream GetStream(string tag, byte[] buffer)
        {
            var recyclableMemoryStream = new RecyclableMemoryStream(this, tag, buffer.Length);
            recyclableMemoryStream.Write(buffer, 0, buffer.Length);
            recyclableMemoryStream.Position = 0L;
            return recyclableMemoryStream;
        }

        public MemoryStream GetStream(string tag, byte[] buffer, int offset, int count)
        {
            var recyclableMemoryStream = new RecyclableMemoryStream(this, tag, count);
            recyclableMemoryStream.Write(buffer, offset, count);
            recyclableMemoryStream.Position = 0L;
            return recyclableMemoryStream;
        }

        public enum MemoryStreamDiscardReason
        {
            TooLarge,
            EnoughFree
        }

        public delegate void EventHandler();

        public delegate void LargeBufferDiscardedEventHandler(MemoryStreamDiscardReason reason);

        public delegate void StreamLengthReportHandler(long bytes);

        public delegate void UsageReportEventHandler(long smallPoolInUseBytes, long smallPoolFreeBytes, long largePoolInUseBytes, long largePoolFreeBytes);
    }
}
