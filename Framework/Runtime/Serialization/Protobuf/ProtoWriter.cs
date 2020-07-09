using System;
using System.IO;
using System.Text;

namespace Framework.Runtime.Serialization.Protobuf
{
    public sealed class ProtoWriter : BaseDisposed
    {
        private readonly Encoding _encoding;
        private Stream _ioStream;
        private long _position;
        private byte[] _buffer;
        private int _index;
        private uint _depth;
        private uint _field;
        private WireType _wireType;

        public ProtoWriter(Stream ioStream)
            : this(ioStream, Encoding.UTF8)
        { }

        public ProtoWriter(Stream ioStream, Encoding encoding)
        {
            if (ioStream == null)
                throw new ArgumentNullException(nameof(ioStream));
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));

            _ioStream = ioStream;
            _encoding = encoding;

            _buffer = BufferPool.GetBuffer();
            _wireType = WireType.None;
        }

        public long Position => _position;

        public long Length => _ioStream.Length;

        private void EnsureBuffer(int count)
        {
            if (_buffer.Length < count)
            {
                BufferPool.Resize(ref _buffer, count, 0, _index);
            }
            if (_buffer.Length - _index < count)
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
                _buffer[_index++] = (byte)((value & 0x7F) | 0x80);
                count++;
            } while ((value >>= 7) != 0);
            _buffer[_index - 1] &= 0x7F;
            _position += count;
        }

        private void WriteUInt64Variant(ulong value)
        {
            EnsureBuffer(10);
            int count = 0;

            do
            {
                _buffer[_index++] = (byte)((value & 0x7F) | 0x80);
                count++;
            } while ((value >>= 7) != 0);
            _buffer[_index - 1] &= 0x7F;
            _position += count;
        }

        private void WriteHeaderCore(uint field, WireType wireType)
        {
            uint value = (field << 3) | (((uint)wireType) & 0x07);
            WriteUInt32Variant(value);
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                try
                {
                    Flush();

                    BufferPool.Release(_buffer);

                    _buffer = null;
                    _ioStream = null;
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }

        public void Flush()
        {
            if (_index > 0)
            {
                _ioStream.Write(_buffer, 0, _index);
                _ioStream.Flush();
                _index = 0;
            }
        }

        public void WriteFieldHeader(uint field, WireType wireType)
        {
            if (_wireType != WireType.None)
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

            _field = field;
            _wireType = wireType;
            WriteHeaderCore(field, wireType);
        }

        public SubItemToken StartSubItem(object obj)
        {
            switch (_wireType)
            {
                case WireType.StartGroup:
                    _depth++;
                    _wireType = WireType.None;
                    return new SubItemToken(_depth, _field, WireType.StartGroup);
                default:
                    throw new InvalidOperationException();
            }
        }

        public void EndSubItem(SubItemToken token)
        {
            if (_wireType != WireType.None)
                throw new InvalidOperationException();
            if (_depth <= 0 || token.depth != _depth)
                throw new InvalidOperationException();

            switch (token.wireType)
            {
                case WireType.StartGroup:
                    WriteHeaderCore(token.value, WireType.EndGroup);
                    break;
                default:
                    throw new ArgumentException(nameof(token));
            }

            _wireType = WireType.None;
            _depth--;
        }

        public void WriteByte(byte value)
        {
            switch (_wireType)
            {
                case WireType.Variant:
                    WriteUInt32Variant(value);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            _wireType = WireType.None;
        }

        public void WriteSByte(sbyte value)
        {
            switch (_wireType)
            {
                case WireType.Variant:
                    WriteUInt32Variant((uint)value);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            _wireType = WireType.None;
        }

        public void WriteInt16(short value)
        {
            switch (_wireType)
            {
                case WireType.Variant:
                    WriteUInt32Variant((uint)value);
                    break;
                case WireType.Fixed16:
                    EnsureBuffer(2);
                    _buffer[_index++] = (byte)((value >> 0) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 8) & 0xFF);
                    _position += 2;
                    break;
                default:
                    throw new InvalidOperationException();
            }
            _wireType = WireType.None;
        }

        public void WriteUInt16(ushort value)
        {
            switch (_wireType)
            {
                case WireType.Variant:
                    WriteUInt32Variant(value);
                    break;
                case WireType.Fixed16:
                    EnsureBuffer(2);
                    _buffer[_index++] = (byte)((value >> 0) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 8) & 0xFF);
                    _position += 2;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            _wireType = WireType.None;
        }

        public void WriteInt32(int value)
        {
            switch (_wireType)
            {
                case WireType.Variant:
                    WriteUInt32Variant((uint)value);
                    break;
                case WireType.Fixed32:
                    EnsureBuffer(4);
                    _buffer[_index++] = (byte)((value >> 0) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 8) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 16) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 24) & 0xFF);
                    _position += 4;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            _wireType = WireType.None;
        }

        public void WriteUInt32(uint value)
        {
            switch (_wireType)
            {
                case WireType.Variant:
                    WriteUInt32Variant(value);
                    break;
                case WireType.Fixed32:
                    EnsureBuffer(4);
                    _buffer[_index++] = (byte)((value >> 0) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 8) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 16) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 24) & 0xFF);
                    _position += 4;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            _wireType = WireType.None;
        }

        public void WriteInt64(long value)
        {
            switch (_wireType)
            {
                case WireType.Variant:
                    WriteUInt64Variant((ulong)value);
                    break;
                case WireType.Fixed64:
                    EnsureBuffer(8);
                    _buffer[_index++] = (byte)((value >> 0) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 8) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 16) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 24) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 32) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 40) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 48) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 56) & 0xFF);
                    _position += 8;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            _wireType = WireType.None;
        }

        public void WriteUInt64(ulong value)
        {
            switch (_wireType)
            {
                case WireType.Variant:
                    WriteUInt64Variant(value);
                    break;
                case WireType.Fixed64:
                    EnsureBuffer(8);
                    _buffer[_index++] = (byte)((value >> 0) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 8) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 16) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 24) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 32) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 40) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 48) & 0xFF);
                    _buffer[_index++] = (byte)((value >> 56) & 0xFF);
                    _position += 8;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            _wireType = WireType.None;
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
            switch (_wireType)
            {
                case WireType.Variant:
                    WriteUInt32Variant(value ? 1U : 0U);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            _wireType = WireType.None;
        }

        public void WriteChar(char value)
        {
            WriteInt16((short)value);
        }

        public void WriteString(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (_wireType != WireType.String)
                throw new InvalidOperationException();

            if (value.Length == 0)
            {
                WriteUInt32Variant(0);
                _wireType = WireType.None;
                return;
            }

            var len = _encoding.GetByteCount(value);
            WriteUInt32Variant((uint)len);
            EnsureBuffer(len);
            len = _encoding.GetBytes(value, 0, value.Length, _buffer, _index);
            _index += len;
            _position += len;
            _wireType = WireType.None;
        }

        public void WriteBytes(byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (_wireType != WireType.Binary)
                throw new InvalidOperationException();

            WriteUInt32Variant((uint)value.Length);
            if (value.Length > 0)
            {
                EnsureBuffer(value.Length);
                Array.Copy(value, 0, _buffer, _index, value.Length);
                _index += value.Length;
                _position += value.Length;
            }
            _wireType = WireType.None;
        }

        public void WriteEnum(Enum value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var enumType = value.GetType();
            var underlyingType = Enum.GetUnderlyingType(enumType);
            switch (Type.GetTypeCode(underlyingType))
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
                    throw new InvalidOperationException("unknown type: " + enumType);
            }
        }
    }
}
