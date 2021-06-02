using System;
using System.IO;
using System.Text;

namespace Framework.Runtime.Serialization.Protobuf
{
    public sealed class ProtoWriter : IDisposable
    {
        private readonly Encoding encoding;
        private Stream inputStream;
        private bool isDisposed;
        private byte[] buffer;
        private int index;
        private uint depth;
        private uint field;
        private WireType wireType;

        public ProtoWriter(Stream inputStream)
            : this(inputStream, Encoding.UTF8)
        { }

        public ProtoWriter(Stream inputStream, Encoding encoding)
        {
            if (inputStream == null)
                throw new ArgumentNullException(nameof(inputStream));
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));

            this.inputStream = inputStream;
            this.encoding = encoding;

            buffer = BufferPool.GetBuffer();
            wireType = WireType.None;
        }

        public long Position => inputStream.Position + index;

        public long Length => inputStream.Length + index;

        private void EnsureBuffer(int count)
        {
            if (buffer.Length < count)
            {
                BufferPool.Resize(ref buffer, count, 0, index);
            }
            if (buffer.Length - index < count)
            {
                Flush();
            }
        }

        private void WriteUInt32Variant(uint value)
        {
            EnsureBuffer(5);
            int count = 0;

            do
            {
                buffer[index++] = (byte)((value & 0x7F) | 0x80);
                count++;
            } while ((value >>= 7) != 0);
            buffer[index - 1] &= 0x7F;
        }

        private void WriteUInt64Variant(ulong value)
        {
            EnsureBuffer(10);
            int count = 0;

            do
            {
                buffer[index++] = (byte)((value & 0x7F) | 0x80);
                count++;
            } while ((value >>= 7) != 0);
            buffer[index - 1] &= 0x7F;
        }

        private void WriteHeaderCore(uint field, WireType wireType)
        {
            uint value = (field << 3) | (((uint)wireType) & 0x07);
            WriteUInt32Variant(value);
        }

        private void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                try
                {
                    Flush();

                    BufferPool.Release(buffer);

                    buffer = null;
                    inputStream = null;
                }
                finally
                {
                    isDisposed = true;
                }
            }
        }

        public void Flush()
        {
            if (index > 0)
            {
                inputStream.Write(buffer, 0, index);
                inputStream.Flush();
                index = 0;
            }
        }

        public void WriteField(uint field, WireType wireType)
        {
            if (this.wireType != WireType.None)
                throw new InvalidOperationException();

            switch (wireType)
            {
                case WireType.Variant:
                case WireType.Fixed16:
                case WireType.Fixed32:
                case WireType.Fixed64:
                case WireType.String:
                case WireType.Binary:
                case WireType.StartGroup:
                    break;
                default:
                    throw new ArgumentException(nameof(wireType));
            }

            this.field = field;
            this.wireType = wireType;
            WriteHeaderCore(field, wireType);
        }

        public SubItemToken StartSubItem()
        {
            switch (wireType)
            {
                case WireType.StartGroup:
                    depth++;
                    wireType = WireType.None;
                    return new SubItemToken(depth, field, WireType.StartGroup);
                default:
                    throw new InvalidOperationException();
            }
        }

        public void EndSubItem(SubItemToken token)
        {
            if (wireType != WireType.None)
                throw new InvalidOperationException();
            if (depth <= 0 || token.depth != depth)
                throw new InvalidOperationException();

            switch (token.wireType)
            {
                case WireType.StartGroup:
                    WriteHeaderCore(token.value, WireType.EndGroup);
                    break;
                default:
                    throw new ArgumentException(nameof(token));
            }

            wireType = WireType.None;
            depth--;
        }

        public void WriteByte(byte value)
        {
            switch (wireType)
            {
                case WireType.Variant:
                    WriteUInt32Variant(value);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            wireType = WireType.None;
        }

        public void WriteSByte(sbyte value)
        {
            switch (wireType)
            {
                case WireType.Variant:
                    WriteUInt32Variant((uint)value);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            wireType = WireType.None;
        }

        public void WriteInt16(short value)
        {
            switch (wireType)
            {
                case WireType.Variant:
                    WriteUInt32Variant((uint)value);
                    break;
                case WireType.Fixed16:
                    EnsureBuffer(2);
                    buffer[index++] = (byte)((value >> 0) & 0xFF);
                    buffer[index++] = (byte)((value >> 8) & 0xFF);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            wireType = WireType.None;
        }

        public void WriteUInt16(ushort value)
        {
            switch (wireType)
            {
                case WireType.Variant:
                    WriteUInt32Variant(value);
                    break;
                case WireType.Fixed16:
                    EnsureBuffer(2);
                    buffer[index++] = (byte)((value >> 0) & 0xFF);
                    buffer[index++] = (byte)((value >> 8) & 0xFF);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            wireType = WireType.None;
        }

        public void WriteInt32(int value)
        {
            switch (wireType)
            {
                case WireType.Variant:
                    WriteUInt32Variant((uint)value);
                    break;
                case WireType.Fixed32:
                    EnsureBuffer(4);
                    buffer[index++] = (byte)((value >> 0) & 0xFF);
                    buffer[index++] = (byte)((value >> 8) & 0xFF);
                    buffer[index++] = (byte)((value >> 16) & 0xFF);
                    buffer[index++] = (byte)((value >> 24) & 0xFF);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            wireType = WireType.None;
        }

        public void WriteUInt32(uint value)
        {
            switch (wireType)
            {
                case WireType.Variant:
                    WriteUInt32Variant(value);
                    break;
                case WireType.Fixed32:
                    EnsureBuffer(4);
                    buffer[index++] = (byte)((value >> 0) & 0xFF);
                    buffer[index++] = (byte)((value >> 8) & 0xFF);
                    buffer[index++] = (byte)((value >> 16) & 0xFF);
                    buffer[index++] = (byte)((value >> 24) & 0xFF);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            wireType = WireType.None;
        }

        public void WriteInt64(long value)
        {
            switch (wireType)
            {
                case WireType.Variant:
                    WriteUInt64Variant((ulong)value);
                    break;
                case WireType.Fixed64:
                    EnsureBuffer(8);
                    buffer[index++] = (byte)((value >> 0) & 0xFF);
                    buffer[index++] = (byte)((value >> 8) & 0xFF);
                    buffer[index++] = (byte)((value >> 16) & 0xFF);
                    buffer[index++] = (byte)((value >> 24) & 0xFF);
                    buffer[index++] = (byte)((value >> 32) & 0xFF);
                    buffer[index++] = (byte)((value >> 40) & 0xFF);
                    buffer[index++] = (byte)((value >> 48) & 0xFF);
                    buffer[index++] = (byte)((value >> 56) & 0xFF);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            wireType = WireType.None;
        }

        public void WriteUInt64(ulong value)
        {
            switch (wireType)
            {
                case WireType.Variant:
                    WriteUInt64Variant(value);
                    break;
                case WireType.Fixed64:
                    EnsureBuffer(8);
                    buffer[index++] = (byte)((value >> 0) & 0xFF);
                    buffer[index++] = (byte)((value >> 8) & 0xFF);
                    buffer[index++] = (byte)((value >> 16) & 0xFF);
                    buffer[index++] = (byte)((value >> 24) & 0xFF);
                    buffer[index++] = (byte)((value >> 32) & 0xFF);
                    buffer[index++] = (byte)((value >> 40) & 0xFF);
                    buffer[index++] = (byte)((value >> 48) & 0xFF);
                    buffer[index++] = (byte)((value >> 56) & 0xFF);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            wireType = WireType.None;
        }
#if UNSAFE
        public unsafe void WriteSingle(float value)
        {
            WriteInt32(*(int*)&value);
        }

        public unsafe void WriteDouble(double value)
        {
            WriteInt64(*(long*)&value);
        }
#else
        public void WriteSingle(float value)
        {
            WriteInt32(BitConverter.ToInt32(BitConverter.GetBytes(value), 0));
        }
        public void WriteDouble(double value)
        {
            WriteInt64(BitConverter.ToInt64(BitConverter.GetBytes(value), 0));
        }
#endif
        public void WriteBoolean(bool value)
        {
            switch (wireType)
            {
                case WireType.Variant:
                    WriteUInt32Variant(value ? 1U : 0U);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            wireType = WireType.None;
        }

        public void WriteChar(char value)
        {
            WriteInt16((short)value);
        }

        public void WriteString(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (wireType != WireType.String)
                throw new InvalidOperationException();

            if (value.Length == 0)
            {
                WriteUInt32Variant(0);
                wireType = WireType.None;
                return;
            }

            var len = encoding.GetByteCount(value);
            WriteUInt32Variant((uint)len);
            EnsureBuffer(len);
            len = encoding.GetBytes(value, 0, value.Length, buffer, index);
            index += len;
            wireType = WireType.None;
        }

        public void WriteBytes(byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (wireType != WireType.Binary)
                throw new InvalidOperationException();

            WriteUInt32Variant((uint)value.Length);
            if (value.Length > 0)
            {
                EnsureBuffer(value.Length);
                Array.Copy(value, 0, buffer, index, value.Length);
                index += value.Length;
            }
            wireType = WireType.None;
        }

        public void WriteEnum(Enum value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            switch (Convert.GetTypeCode(value))
            {
                case TypeCode.Byte:
                    WriteByte(Convert.ToByte(value));
                    break;
                case TypeCode.SByte:
                    WriteSByte(Convert.ToSByte(value));
                    break;
                case TypeCode.Int16:
                    WriteInt16(Convert.ToInt16(value));
                    break;
                case TypeCode.UInt16:
                    WriteUInt16(Convert.ToUInt16(value));
                    break;
                case TypeCode.Int32:
                    WriteInt32(Convert.ToInt32(value));
                    break;
                case TypeCode.UInt32:
                    WriteUInt32(Convert.ToUInt32(value));
                    break;
                case TypeCode.Int64:
                    WriteInt64(Convert.ToInt64(value));
                    break;
                case TypeCode.UInt64:
                    WriteUInt64(Convert.ToUInt64(value));
                    break;
                default:
                    throw new InvalidOperationException($"unknown type: {value.GetType()}");
            }
        }

        public void WriteEnum<T>(T value)
            where T : Enum
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            switch (Convert.GetTypeCode(value))
            {
                case TypeCode.Byte:
                    WriteByte(Convert.ToByte(value));
                    break;
                case TypeCode.SByte:
                    WriteSByte(Convert.ToSByte(value));
                    break;
                case TypeCode.Int16:
                    WriteInt16(Convert.ToInt16(value));
                    break;
                case TypeCode.UInt16:
                    WriteUInt16(Convert.ToUInt16(value));
                    break;
                case TypeCode.Int32:
                    WriteInt32(Convert.ToInt32(value));
                    break;
                case TypeCode.UInt32:
                    WriteUInt32(Convert.ToUInt32(value));
                    break;
                case TypeCode.Int64:
                    WriteInt64(Convert.ToInt64(value));
                    break;
                case TypeCode.UInt64:
                    WriteUInt64(Convert.ToUInt64(value));
                    break;
                default:
                    throw new InvalidOperationException($"unknown type: {value.GetType()}");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
