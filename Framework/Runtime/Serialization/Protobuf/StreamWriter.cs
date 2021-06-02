using System;
using System.IO;
using System.Text;

namespace Framework.Runtime.Serialization.Protobuf
{
    public sealed class StreamWriter : BaseDisposed
    {
        private const int DefaultBufferSize = 1024;

        private readonly Stream inputStream;
        private readonly Encoding encoding;
        private readonly bool leaveOpen;
        private readonly byte[] buffer;
        private readonly int limit;
        private int position;

        public StreamWriter(Stream inputStream)
            : this(inputStream, Encoding.UTF8, DefaultBufferSize, false)
        { }

        public StreamWriter(Stream inputStream, bool leaveOpen)
            : this(inputStream, Encoding.UTF8, DefaultBufferSize, leaveOpen)
        { }

        public StreamWriter(Stream inputStream, Encoding encoding)
            : this(inputStream, encoding, DefaultBufferSize, false)
        { }

        public StreamWriter(Stream inputStream, Encoding encoding, int bufferSize, bool leaveOpen)
        {
            if (inputStream == null)
                throw new ArgumentNullException(nameof(inputStream));
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            this.inputStream = inputStream;
            this.encoding = encoding;
            this.buffer = new byte[bufferSize];
            this.leaveOpen = leaveOpen;
            this.position = 0;
            this.limit = bufferSize;
        }

        public long Position => inputStream.Position + position;

        public long Length => inputStream.Length + position;

        private void RefreshBuffer()
        {
            inputStream.Write(buffer, 0, position);
            position = 0;
        }

        private void WriteRawByte(byte value)
        {
            if (position == limit)
            {
                RefreshBuffer();
            }

            buffer[position++] = value;
        }

        private void WriteRawBytes(byte[] value, int offset, int length)
        {
            if (limit - position >= length)
            {
                ByteArray.Copy(value, offset, buffer, position, length);
                position += length;
                return;
            }

            var bytesWritten = limit - position;
            ByteArray.Copy(value, offset, buffer, position, bytesWritten);
            offset += bytesWritten;
            length -= bytesWritten;
            position = limit;
            RefreshBuffer();
            if (length <= limit)
            {
                ByteArray.Copy(value, offset, buffer, 0, length);
                position = length;
            }
            else
            {
                inputStream.Write(value, offset, length);
            }
        }

        private void WriteRawVarint32(uint value)
        {
            if (value < 0x80 && position < limit)
            {
                buffer[position++] = (byte)value;
                return;
            }
            while (value > 0x7F && position < limit)
            {
                buffer[position++] = (byte)((value & 0x7F) | 0x80);
                value >>= 7;
            }
            while (value > 0x7F)
            {
                WriteRawByte((byte)((value & 0x7F) | 0x80));
                value >>= 7;
            }
            if (position < limit)
            {
                buffer[position++] = (byte)value;
            }
            else
            {
                WriteRawByte((byte)value);
            }
        }

        private void WriteRawVarint64(ulong value)
        {
            while (value > 0x7F && position < limit)
            {
                buffer[position++] = (byte)((value & 0x7F) | 0x80);
                value >>= 7;
            }
            while (value > 0x7F)
            {
                WriteRawByte((byte)((value & 0x7F) | 0x80));
                value >>= 7;
            }
            if (position < limit)
            {
                buffer[position++] = (byte)value;
            }
            else
            {
                WriteRawByte((byte)value);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                try
                {
                    Flush();
                    if (!leaveOpen)
                    {
                        inputStream.Dispose();
                    }
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }

        public void Flush()
        {
            if (position > 0)
            {
                RefreshBuffer();
            }
        }

        public void WriteBoolean(bool value)
        {
            WriteRawByte((byte)(value ? 1 : 0));
        }

        public void WriteByte(byte value)
        {
            WriteRawByte(value);
        }

        public void WriteSByte(sbyte value)
        {
            WriteRawByte((byte)value);
        }

        public void WriteInt16(short value)
        {
            WriteRawVarint32((uint)value);
        }

        public void WriteUInt16(ushort value)
        {
            WriteRawVarint32(value);
        }

        public void WriteInt32(int value)
        {
            WriteRawVarint32((uint)value);
        }

        public void WriteUInt32(uint value)
        {
            WriteRawVarint32(value);
        }

        public void WriteInt64(long value)
        {
            WriteRawVarint64((ulong)value);
        }

        public void WriteUInt64(ulong value)
        {
            WriteRawVarint64(value);
        }

        public void WriteSingle(float value)
        {
            WriteInt32(BitConverter.ToInt32(BitConverter.GetBytes(value), 0));
        }

        public void WriteDouble(double value)
        {
            WriteInt64(BitConverter.ToInt64(BitConverter.GetBytes(value), 0));
        }

        public void WriteChar(char value)
        {
            WriteInt16((short)value);
        }

        public void WriteString(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Length == 0)
            {
                WriteRawVarint32(0);
                return;
            }

            var bytes = encoding.GetBytes(value);
            WriteRawVarint32((uint)bytes.Length);
            WriteRawBytes(bytes, 0, bytes.Length);
        }

        public void WriteBytes(byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Length == 0)
            {
                WriteRawVarint32(0);
                return;
            }

            WriteRawVarint32((uint)value.Length);
            WriteRawBytes(value, 0, value.Length);
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
    }
}
