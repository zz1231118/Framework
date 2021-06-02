using System;
using System.IO;
using System.Text;

namespace Framework.Runtime.Serialization.Protobuf
{
    public sealed class ProtoReader : IDisposable
    {
        private readonly Encoding encoding;
        private Stream outputStream;
        private bool isDisposed;
        private byte[] buffer;
        private int index;
        private int length;
        private uint depth;
        private uint field;
        private WireType wireType;

        public ProtoReader(Stream outputStream)
            : this(outputStream, Encoding.UTF8)
        { }

        public ProtoReader(Stream outputStream, Encoding encoding)
        {
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));

            this.outputStream = outputStream;
            this.encoding = encoding;

            buffer = BufferPool.GetBuffer();
            wireType = WireType.None;
        }

        public long Position => outputStream.Position - (length - index);

        public long Length => outputStream.Length;

        private void EnsureBuffer(int count)
        {
            if (length >= count)
            {
                return;
            }
            if (count > buffer.Length)
            {
                BufferPool.Resize(ref buffer, count, index, length);
                index = 0;
            }
            if (buffer.Length - index < count)
            {
                Buffer.BlockCopy(buffer, index, buffer, 0, length);
                index = 0;
            }

            int writePos = index + length, bytesRead;
            while (writePos < buffer.Length)
            {
                bytesRead = outputStream.Read(buffer, writePos, buffer.Length - writePos);
                if (bytesRead <= 0)
                    break;

                length += bytesRead;
                writePos += bytesRead;
            }
        }

        private int ReadUInt32VariantWithoutMoving(bool negative, out uint value)
        {
            EnsureBuffer(10);
            if (length == 0)
            {
                value = 0;
                return 0;
            }

            int readPos = index;
            value = buffer[readPos++];
            if ((value & 0x80) == 0) return 1;

            if (length == 1) throw new EndOfStreamException();
            value &= 0x7F;
            uint chunk = buffer[readPos++];
            value |= (chunk & 0x7F) << 7;
            if ((chunk & 0x80) == 0) return 2;

            if (length == 2) throw new EndOfStreamException();
            chunk = buffer[readPos++];
            value |= (chunk & 0x7F) << 14;
            if ((chunk & 0x80) == 0) return 3;

            if (length == 3) throw new EndOfStreamException();
            chunk = buffer[readPos++];
            value |= (chunk & 0x7F) << 21;
            if ((chunk & 0x80) == 0) return 4;

            if (length == 4) throw new EndOfStreamException();
            chunk = buffer[readPos++];
            value |= chunk << 28;
            if ((chunk & 0xF0) == 0) return 5;

            if (negative &&
                (chunk & 0xF0) == 0xF0 &&
                length >= 10 &&
                buffer[readPos++] == 0xFF &&
                buffer[readPos++] == 0xFF &&
                buffer[readPos++] == 0xFF &&
                buffer[readPos++] == 0xFF &&
                buffer[readPos++] == 0x01)
            {
                return 10;
            }

            throw new EndOfStreamException();
        }

        private int ReadUInt64VariantWithoutMoving(out ulong value)
        {
            EnsureBuffer(10);
            if (length == 0)
            {
                value = 0;
                return 0;
            }

            int readPos = index;
            value = buffer[readPos++];
            if ((value & 0x80) == 0) return 1;

            if (length == 1) throw new EndOfStreamException();
            value &= 0x7F;
            ulong chunk = buffer[readPos++];
            value |= (chunk & 0x7F) << 7;
            if ((chunk & 0x80) == 0) return 2;

            if (length == 2) throw new EndOfStreamException();
            chunk = buffer[readPos++];
            value |= (chunk & 0x7F) << 14;
            if ((chunk & 0x80) == 0) return 3;

            if (length == 3) throw new EndOfStreamException();
            chunk = buffer[readPos++];
            value |= (chunk & 0x7F) << 21;
            if ((chunk & 0x80) == 0) return 4;

            if (length == 4) throw new EndOfStreamException();
            chunk = buffer[readPos++];
            value |= (chunk & 0x7F) << 28;
            if ((chunk & 0x80) == 0) return 5;

            if (length == 5) throw new EndOfStreamException();
            chunk = buffer[readPos++];
            value |= (chunk & 0x7F) << 35;
            if ((chunk & 0x80) == 0) return 6;

            if (length == 6) throw new EndOfStreamException();
            chunk = buffer[readPos++];
            value |= (chunk & 0x7F) << 42;
            if ((chunk & 0x80) == 0) return 7;

            if (length == 7) throw new EndOfStreamException();
            chunk = buffer[readPos++];
            value |= (chunk & 0x7F) << 49;
            if ((chunk & 0x80) == 0) return 8;

            if (length == 8) throw new EndOfStreamException();
            chunk = buffer[readPos++];
            value |= (chunk & 0x7F) << 56;
            if ((chunk & 0x80) == 0) return 9;

            if (length == 9) throw new EndOfStreamException();
            chunk = buffer[readPos++];
            value |= chunk << 63;
            if ((chunk & 0x01) == 0) return 10;

            throw new EndOfStreamException();
        }

        private bool TryReadUInt32Variant(out uint value)
        {
            int len = ReadUInt32VariantWithoutMoving(false, out value);
            if (len > 0)
            {
                index += len;
                length -= len;
                return true;
            }

            return false;
        }

        private uint ReadUInt32Variant(bool negative)
        {
            int len = ReadUInt32VariantWithoutMoving(negative, out uint value);
            if (len == 0) throw new EndOfStreamException();

            index += len;
            length -= len;
            return value;
        }

        private ulong ReadUInt64Varint()
        {
            int len = ReadUInt64VariantWithoutMoving(out ulong value);
            if (len == 0) throw new EndOfStreamException();

            index += len;
            length -= len;
            return value;
        }

        private void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                try
                {
                    BufferPool.Release(buffer);

                    buffer = null;
                    outputStream = null;
                }
                finally
                {
                    isDisposed = true;
                }
            }
        }

        public uint ReadField()
        {
            if (wireType != WireType.None)
                throw new InvalidOperationException();
            if (!TryReadField(out uint field))
                throw new EndOfStreamException();

            return field;
        }

        public bool TryReadField(out uint field)
        {
            if (wireType != WireType.None)
                throw new InvalidOperationException();

            if (TryReadUInt32Variant(out uint value))
            {
                wireType = (WireType)(value & 7);
                this.field = field = value >> 3;
                if (wireType == WireType.EndGroup && depth == 0)
                    throw new InvalidOperationException();

                return wireType != WireType.EndGroup;
            }

            field = 0;
            return false;
        }

        public void SkipField()
        {
            switch (wireType)
            {
                case WireType.Variant:
                    ReadUInt64Varint();
                    break;
                case WireType.Fixed16:
                    EnsureBuffer(2);
                    index += 2;
                    length -= 2;
                    break;
                case WireType.Fixed32:
                    EnsureBuffer(4);
                    index += 4;
                    length -= 4;
                    break;
                case WireType.Fixed64:
                    EnsureBuffer(8);
                    index += 8;
                    length -= 8;
                    break;
                case WireType.String:
                    int len = checked((int)ReadUInt32Variant(false));
                    EnsureBuffer(len);
                    if (length < len) throw new EndOfStreamException();

                    index += len;
                    length -= len;
                    break;
                case WireType.Binary:
                    int aryLen = checked((int)ReadUInt32Variant(false));
                    EnsureBuffer(aryLen);
                    if (length < aryLen) throw new EndOfStreamException();

                    index += aryLen;
                    length -= aryLen;
                    break;
                case WireType.StartGroup:
                    depth++;
                    uint originalField = field;
                    while (TryReadField(out _))
                    {
                        SkipField();
                    }

                    depth--;
                    if (wireType != WireType.EndGroup || originalField != field)
                        throw new InvalidOperationException();

                    break;
            }

            wireType = WireType.None;
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
            if (depth <= 0 || depth != token.depth)
                throw new InvalidOperationException();
            if (token.wireType != WireType.StartGroup)
                throw new InvalidOperationException();

            switch (wireType)
            {
                case WireType.EndGroup:
                    depth--;
                    wireType = WireType.None;
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        public byte ReadByte()
        {
            switch (wireType)
            {
                case WireType.Variant:
                    wireType = WireType.None;
                    return checked((byte)ReadUInt32Variant(false));
                default:
                    throw new InvalidOperationException();
            }
        }

        public sbyte ReadSByte()
        {
            switch (wireType)
            {
                case WireType.Variant:
                    return unchecked((sbyte)ReadInt32());
                default:
                    throw new InvalidOperationException();
            }
        }

        public short ReadInt16()
        {
            switch (wireType)
            {
                case WireType.Variant:
                    return checked((short)ReadInt32());
                case WireType.Fixed16:
                    EnsureBuffer(2);
                    length -= 2;
                    wireType = WireType.None;
                    return checked((short)(buffer[index++] | (buffer[index++] << 8)));
                default:
                    throw new InvalidOperationException();
            }
        }

        public ushort ReadUInt16()
        {
            switch (wireType)
            {
                case WireType.Variant:
                    wireType = WireType.None;
                    return checked((ushort)ReadUInt32Variant(false));
                case WireType.Fixed16:
                    EnsureBuffer(2);
                    length -= 2;
                    wireType = WireType.None;
                    return unchecked((ushort)(buffer[index++] | (buffer[index++] << 8)));
                default:
                    throw new InvalidOperationException();
            }
        }

        public int ReadInt32()
        {
            switch (wireType)
            {
                case WireType.Variant:
                    wireType = WireType.None;
                    return unchecked((int)ReadUInt32Variant(false));
                case WireType.Fixed32:
                    EnsureBuffer(4);
                    length -= 4;
                    wireType = WireType.None;
                    return buffer[index++] | (buffer[index++] << 8) | (buffer[index++] << 16) | (buffer[index++] << 24);
                default:
                    throw new InvalidOperationException();
            }
        }

        public uint ReadUInt32()
        {
            switch (wireType)
            {
                case WireType.Variant:
                    wireType = WireType.None;
                    return ReadUInt32Variant(false);
                case WireType.Fixed32:
                    EnsureBuffer(4);
                    length -= 4;
                    wireType = WireType.None;
                    return unchecked((uint)(buffer[index++] | (buffer[index++] << 8) | (buffer[index++] << 16) | (buffer[index++] << 24)));
                default:
                    throw new InvalidOperationException();
            }
        }

        public long ReadInt64()
        {
            switch (wireType)
            {
                case WireType.Variant:
                    wireType = WireType.None;
                    return unchecked((long)ReadUInt64Varint());
                case WireType.Fixed64:
                    EnsureBuffer(8);
                    length -= 8;
                    wireType = WireType.None;
                    return ((long)buffer[index++] << 0) |
                        ((long)buffer[index++] << 8) |
                        ((long)buffer[index++] << 16) |
                        ((long)buffer[index++] << 24) |
                        ((long)buffer[index++] << 32) |
                        ((long)buffer[index++] << 40) |
                        ((long)buffer[index++] << 48) |
                        ((long)buffer[index++] << 56);
                default:
                    throw new InvalidOperationException();
            }
        }

        public ulong ReadUInt64()
        {
            switch (wireType)
            {
                case WireType.Variant:
                    wireType = WireType.None;
                    return ReadUInt64Varint();
                case WireType.Fixed64:
                    EnsureBuffer(8);
                    length -= 8;
                    wireType = WireType.None;
                    return ((ulong)buffer[index++] << 0) |
                        ((ulong)buffer[index++] << 8) |
                        ((ulong)buffer[index++] << 16) |
                        ((ulong)buffer[index++] << 24) |
                        ((ulong)buffer[index++] << 32) |
                        ((ulong)buffer[index++] << 40) |
                        ((ulong)buffer[index++] << 48) |
                        ((ulong)buffer[index++] << 56);
                default:
                    throw new InvalidOperationException();
            }
        }
#if UNSAFE
        public unsafe float ReadSingle()
        {
            int value = ReadInt32();
            return *(float*)&value;
        }

        public unsafe double ReadDouble()
        {
            long value = ReadInt64();
            return *(double*)&value;
        }
#else
        public float ReadSingle()
        {
            int value = ReadInt32();
            return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
        }
        public double ReadDouble()
        {
            long value = ReadInt64();
            return BitConverter.ToDouble(BitConverter.GetBytes(value), 0);
        }
#endif
        public bool ReadBoolean()
        {
            switch (wireType)
            {
                case WireType.Variant:
                    wireType = WireType.None;
                    return ReadUInt32Variant(false) == 1;
                default:
                    throw new InvalidOperationException();
            }
        }

        public char ReadChar()
        {
            return (char)ReadInt16();
        }

        public string ReadString()
        {
            if (wireType != WireType.String)
                throw new InvalidOperationException();

            var length = checked((int)ReadUInt32Variant(false));
            if (length == 0)
            {
                wireType = WireType.None;
                return string.Empty;
            }

            EnsureBuffer(length);
            if (this.length < length)
                throw new EndOfStreamException();

            var str = encoding.GetString(buffer, index, length);
            index += length;
            this.length -= length;
            wireType = WireType.None;
            return str;
        }

        public byte[] ReadBytes()
        {
            if (wireType != WireType.Binary)
                throw new InvalidOperationException();

            var len = checked((int)ReadUInt32Variant(false));
            if (len == 0)
            {
                wireType = WireType.None;
                return new byte[0];
            }

            EnsureBuffer(len);
            if (length < len)
                throw new EndOfStreamException();

            var array = new byte[len];
            Array.Copy(buffer, index, array, 0, len);
            index += len;
            length -= len;
            wireType = WireType.None;
            return array;
        }

        public Enum ReadEnum(Type enumType)
        {
            if (enumType == null)
                throw new ArgumentNullException(nameof(enumType));

            switch (Type.GetTypeCode(enumType))
            {
                case TypeCode.Byte:
                    return (Enum)Enum.ToObject(enumType, ReadByte());
                case TypeCode.SByte:
                    return (Enum)Enum.ToObject(enumType, ReadSByte());
                case TypeCode.Int16:
                    return (Enum)Enum.ToObject(enumType, ReadInt16());
                case TypeCode.UInt16:
                    return (Enum)Enum.ToObject(enumType, ReadUInt16());
                case TypeCode.Int32:
                    return (Enum)Enum.ToObject(enumType, ReadInt32());
                case TypeCode.UInt32:
                    return (Enum)Enum.ToObject(enumType, ReadUInt32());
                case TypeCode.Int64:
                    return (Enum)Enum.ToObject(enumType, ReadInt64());
                case TypeCode.UInt64:
                    return (Enum)Enum.ToObject(enumType, ReadUInt64());
                default:
                    throw new InvalidOperationException("unknown type: " + enumType);
            }
        }

        public T ReadEnum<T>()
            where T : Enum
        {
            var enumType = typeof(T);
            switch (Type.GetTypeCode(enumType))
            {
                case TypeCode.Byte:
                    return (T)Enum.ToObject(enumType, ReadByte());
                case TypeCode.SByte:
                    return (T)Enum.ToObject(enumType, ReadSByte());
                case TypeCode.Int16:
                    return (T)Enum.ToObject(enumType, ReadInt16());
                case TypeCode.UInt16:
                    return (T)Enum.ToObject(enumType, ReadUInt16());
                case TypeCode.Int32:
                    return (T)Enum.ToObject(enumType, ReadInt32());
                case TypeCode.UInt32:
                    return (T)Enum.ToObject(enumType, ReadUInt32());
                case TypeCode.Int64:
                    return (T)Enum.ToObject(enumType, ReadInt64());
                case TypeCode.UInt64:
                    return (T)Enum.ToObject(enumType, ReadUInt64());
                default:
                    throw new InvalidOperationException("unknown type: " + enumType);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
