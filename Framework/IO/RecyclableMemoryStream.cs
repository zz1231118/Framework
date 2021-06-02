using System;
using System.Collections.Generic;
using System.IO;

namespace Framework.IO
{
    public sealed class RecyclableMemoryStream : MemoryStream
    {
        private const long MaxStreamLength = int.MaxValue;

        private readonly List<byte[]> blocks = new List<byte[]>(1);
        private readonly string tag;
        private readonly RecyclableMemoryStreamManager memoryManager;
        private readonly byte[] byteBuffer = new byte[1];
        private byte[] largeBuffer;
        private List<byte[]> dirtyBuffers;
        private bool isDisposed;
        private int length;
        private int position;

        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager)
            : this(memoryManager, null)
        { }

        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag)
            : this(memoryManager, tag, 0)
        { }

        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag, int requestedSize)
            : this(memoryManager, tag, requestedSize, null)
        { }

        internal RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag, int requestedSize, byte[] initialLargeBuffer)
        {
            if (memoryManager == null)
                throw new ArgumentNullException(nameof(memoryManager));

            this.tag = tag;
            this.memoryManager = memoryManager;
            if (requestedSize < memoryManager.BlockSize)
            {
                requestedSize = memoryManager.BlockSize;
            }
            if (initialLargeBuffer == null)
            {
                EnsureCapacity(requestedSize);
            }
            else
            {
                largeBuffer = initialLargeBuffer;
            }

            this.isDisposed = false;
            this.memoryManager.ReportStreamCreated();
        }

        ~RecyclableMemoryStream()
        {
            Dispose(false);
        }

        internal string Tag
        {
            get
            {
                CheckDisposed();
                return tag;
            }
        }

        internal RecyclableMemoryStreamManager MemoryManager
        {
            get
            {
                CheckDisposed();
                return memoryManager;
            }
        }

        public override int Capacity
        {
            get
            {
                CheckDisposed();
                if (largeBuffer != null)
                {
                    return largeBuffer.Length;
                }
                if (blocks.Count > 0)
                {
                    return blocks.Count * memoryManager.BlockSize;
                }
                return 0;
            }
            set
            {
                EnsureCapacity(value);
            }
        }

        public override long Length
        {
            get
            {
                CheckDisposed();
                return length;
            }
        }

        public override long Position
        {
            get
            {
                CheckDisposed();
                return position;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "value must be non-negative");
                if (value > MaxStreamLength)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "value cannot be more than " + 2147483647L);

                position = (int)value;
            }
        }

        public override bool CanRead => !isDisposed;

        public override bool CanSeek => !isDisposed;

        public override bool CanTimeout => false;

        public override bool CanWrite => !isDisposed;

        private void CheckDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException($"The stream with Id {GetHashCode()} and Tag {tag} is disposed.");
            }
        }

        private int InternalRead(byte[] buffer, int offset, int count, int fromPosition)
        {
            if (length - fromPosition <= 0)
            {
                return 0;
            }
            if (largeBuffer == null)
            {
                int num = OffsetToBlockIndex(fromPosition);
                int num2 = 0;
                int num3 = Math.Min(count, length - fromPosition);
                int num4 = OffsetToBlockOffset(fromPosition);
                while (num3 > 0)
                {
                    int num5 = Math.Min(blocks[num].Length - num4, num3);
                    Buffer.BlockCopy(blocks[num], num4, buffer, num2 + offset, num5);
                    num2 += num5;
                    num3 -= num5;
                    num++;
                    num4 = 0;
                }
                return num2;
            }

            int num6 = Math.Min(count, length - fromPosition);
            Buffer.BlockCopy(largeBuffer, fromPosition, buffer, offset, num6);
            return num6;
        }

        private int OffsetToBlockIndex(int offset)
        {
            return offset / memoryManager.BlockSize;
        }

        private int OffsetToBlockOffset(int offset)
        {
            return offset % memoryManager.BlockSize;
        }

        private void EnsureCapacity(int newCapacity)
        {
            if (newCapacity > memoryManager.MaximumStreamCapacity && memoryManager.MaximumStreamCapacity > 0)
            {
                throw new InvalidOperationException("Requested capacity is too large: " + newCapacity + ". Limit is " + memoryManager.MaximumStreamCapacity);
            }
            if (largeBuffer != null)
            {
                if (newCapacity > largeBuffer.Length)
                {
                    byte[] buffer = memoryManager.GetLargeBuffer(newCapacity);
                    InternalRead(buffer, 0, length, 0);
                    ReleaseLargeBuffer();
                    largeBuffer = buffer;
                }
            }
            else
            {
                while (Capacity < newCapacity)
                {
                    blocks.Add(MemoryManager.GetBlock());
                }
            }
        }

        private void ReleaseLargeBuffer()
        {
            if (memoryManager.AggressiveBufferReturn)
            {
                memoryManager.ReturnLargeBuffer(largeBuffer);
            }
            else
            {
                if (dirtyBuffers == null)
                {
                    dirtyBuffers = new List<byte[]>(1);
                }
                dirtyBuffers.Add(largeBuffer);
            }

            largeBuffer = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                //throw new InvalidOperationException("Cannot dispose of RecyclableMemoryStream twice");
                isDisposed = true;
                if (disposing)
                {
                    memoryManager.ReportStreamDisposed();
                    GC.SuppressFinalize(this);
                }
                else
                {
                    if (AppDomain.CurrentDomain.IsFinalizingForUnload())
                    {
                        base.Dispose(disposing);
                        return;
                    }

                    memoryManager.ReportStreamFinalized();
                }

                memoryManager.ReportStreamLength(length);
                if (largeBuffer != null)
                {
                    memoryManager.ReturnLargeBuffer(largeBuffer);
                }
                if (dirtyBuffers != null)
                {
                    foreach (byte[] dirtyBuffer in dirtyBuffers)
                    {
                        memoryManager.ReturnLargeBuffer(dirtyBuffer);
                    }
                }

                memoryManager.ReturnBlocks(blocks);
                base.Dispose(disposing);
            }
        }

        public override void Close()
        {
            Dispose(true);
        }

        public override byte[] GetBuffer()
        {
            CheckDisposed();
            if (largeBuffer != null)
            {
                return largeBuffer;
            }
            if (blocks.Count == 1)
            {
                return blocks[0];
            }

            byte[] buffer = MemoryManager.GetLargeBuffer(Capacity);
            InternalRead(buffer, 0, length, 0);
            largeBuffer = buffer;
            if (blocks.Count > 0 && memoryManager.AggressiveBufferReturn)
            {
                memoryManager.ReturnBlocks(blocks);
                blocks.Clear();
            }

            return largeBuffer;
        }

        public override byte[] ToArray()
        {
            CheckDisposed();
            byte[] array = new byte[Length];
            InternalRead(array, 0, length, 0);
            memoryManager.ReportStreamToArray();
            return array;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "offset cannot be negative");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "count cannot be negative");
            }
            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("buffer length must be at least offset + count");
            }

            int num = InternalRead(buffer, offset, count, position);
            position += num;
            return num;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset must be in the range of 0 - buffer.Length-1");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "count must be non-negative");
            }
            if (count + offset > buffer.Length)
            {
                throw new ArgumentException("count must be greater than buffer.Length - offset");
            }
            if (Position + count > MaxStreamLength)
            {
                throw new IOException("Maximum capacity exceeded");
            }

            int num = (int)Position + count;
            int blockSize = memoryManager.BlockSize;
            int num2 = (num + blockSize - 1) / blockSize;
            if (((long)num2 * blockSize) > 2147483647L)
            {
                throw new IOException("Maximum capacity exceeded");
            }

            EnsureCapacity(num);
            if (largeBuffer == null)
            {
                int num3 = count;
                int num4 = 0;
                int num5 = OffsetToBlockIndex(position);
                int num6 = OffsetToBlockOffset(position);
                while (num3 > 0)
                {
                    byte[] dst = blocks[num5];
                    int val = blockSize - num6;
                    int num7 = Math.Min(val, num3);
                    Buffer.BlockCopy(buffer, offset + num4, dst, num6, num7);
                    num3 -= num7;
                    num4 += num7;
                    num5++;
                    num6 = 0;
                }
            }
            else
            {
                Buffer.BlockCopy(buffer, offset, largeBuffer, position, count);
            }

            Position = num;
            length = Math.Max(position, length);
        }

        public override string ToString()
        {
            return $"Id = {GetHashCode()} Tag = {Tag}, Length = {Length:N0} bytes";
        }

        public override void WriteByte(byte value)
        {
            CheckDisposed();
            byteBuffer[0] = value;
            Write(byteBuffer, 0, 1);
        }

        public override int ReadByte()
        {
            CheckDisposed();
            if (position == length)
            {
                return -1;
            }

            byte b;
            if (largeBuffer == null)
            {
                int index = OffsetToBlockIndex(position);
                int num = OffsetToBlockOffset(position);
                b = blocks[index][num];
            }
            else
            {
                b = largeBuffer[position];
            }

            position++;
            return b;
        }

        public override void SetLength(long value)
        {
            CheckDisposed();
            if (value < 0 || value > MaxStreamLength)
            {
                throw new ArgumentOutOfRangeException("value", "value must be non-negative and at most " + 2147483647L);
            }

            EnsureCapacity((int)value);
            length = (int)value;
            if (position > value)
            {
                position = (int)value;
            }
        }

        public override long Seek(long offset, SeekOrigin loc)
        {
            CheckDisposed();
            if (offset > MaxStreamLength)
            {
                throw new ArgumentOutOfRangeException("offset", "offset cannot be larger than " + 2147483647L);
            }

            int num;
            switch (loc)
            {
                case SeekOrigin.Begin:
                    num = (int)offset;
                    break;
                case SeekOrigin.Current:
                    num = (int)offset + position;
                    break;
                case SeekOrigin.End:
                    num = (int)offset + length;
                    break;
                default:
                    throw new ArgumentException("Invalid seek origin", "loc");
            }
            if (num < 0)
            {
                throw new IOException("Seek before beginning");
            }

            position = num;
            return position;
        }

        public override void WriteTo(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (largeBuffer == null)
            {
                int num = 0;
                int num2 = length;
                while (num2 > 0)
                {
                    int num3 = Math.Min(blocks[num].Length, num2);
                    stream.Write(blocks[num], 0, num3);
                    num2 -= num3;
                    num++;
                }
            }
            else
            {
                stream.Write(largeBuffer, 0, length);
            }
        }
    }
}
