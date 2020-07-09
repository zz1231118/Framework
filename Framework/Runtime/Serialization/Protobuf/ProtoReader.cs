using System;
using System.IO;
using System.Text;

namespace Framework.Runtime.Serialization.Protobuf
{
    public sealed class ProtoReader : BaseDisposed
    {
        private readonly Encoding _encoding;
        private Stream _ioStream;
        private long _position;
        private byte[] _buffer;
        private int _index;
        private int _length;
        private uint _depth;
        private uint _field;
        private WireType _wireType;

        public ProtoReader(Stream ioStream)
            : this(ioStream, Encoding.UTF8)
        { }

        public ProtoReader(Stream ioStream, Encoding encoding)
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
            if (_length >= count)
            {
                return;
            }
            if (count > _buffer.Length)
            {
                BufferPool.Resize(ref _buffer, count, _index, _length);
                _index = 0;
            }
            if (_buffer.Length - _index < count)
            {
                Buffer.BlockCopy(_buffer, _index, _buffer, 0, _length);
                _index = 0;
            }

            int writePos = _index + _length, bytesRead;
            while (writePos < _buffer.Length)
            {
                bytesRead = _ioStream.Read(_buffer, writePos, _buffer.Length - writePos);
                if (bytesRead <= 0)
                    break;

                _length += bytesRead;
                writePos += bytesRead;
            }
        }

        private int TryReadUInt32VariantWithoutMoving(bool negative, out uint value)
        {
            EnsureBuffer(10);

            if (_length == 0)
            {
                value = 0;
                return 0;
            }

            int readPos = _index;
            value = _buffer[readPos++];
            if ((value & 0x80) == 0) return 1;

            if (_length == 1) throw new EndOfStreamException();
            value &= 0x7F;
            uint chunk = _buffer[readPos++];
            value |= (chunk & 0x7F) << 7;
            if ((chunk & 0x80) == 0) return 2;

            if (_length == 2) throw new EndOfStreamException();
            chunk = _buffer[readPos++];
            value |= (chunk & 0x7F) << 14;
            if ((chunk & 0x80) == 0) return 3;

            if (_length == 3) throw new EndOfStreamException();
            chunk = _buffer[readPos++];
            value |= (chunk & 0x7F) << 21;
            if ((chunk & 0x80) == 0) return 4;

            if (_length == 4) throw new EndOfStreamException();
            chunk = _buffer[readPos++];
            value |= chunk << 28;
            if ((chunk & 0xF0) == 0) return 5;

            if (negative &&
                (chunk & 0xF0) == 0xF0 &&
                _length >= 10 &&
                _buffer[readPos++] == 0xFF &&
                _buffer[readPos++] == 0xFF &&
                _buffer[readPos++] == 0xFF &&
                _buffer[readPos++] == 0xFF &&
                _buffer[readPos++] == 0x01)
            {
                return 10;
            }

            throw new EndOfStreamException();
        }

        private int TryReadUInt64VariantWithoutMoving(out ulong value)
        {
            EnsureBuffer(10);

            if (_length == 0)
            {
                value = 0;
                return 0;
            }

            int readPos = _index;
            value = _buffer[readPos++];
            if ((value & 0x80) == 0) return 1;

            if (_length == 1) throw new EndOfStreamException();
            value &= 0x7F;
            ulong chunk = _buffer[readPos++];
            value |= (chunk & 0x7F) << 7;
            if ((chunk & 0x80) == 0) return 2;

            if (_length == 2) throw new EndOfStreamException();
            chunk = _buffer[readPos++];
            value |= (chunk & 0x7F) << 14;
            if ((chunk & 0x80) == 0) return 3;

            if (_length == 3) throw new EndOfStreamException();
            chunk = _buffer[readPos++];
            value |= (chunk & 0x7F) << 21;
            if ((chunk & 0x80) == 0) return 4;

            if (_length == 4) throw new EndOfStreamException();
            chunk = _buffer[readPos++];
            value |= (chunk & 0x7F) << 28;
            if ((chunk & 0x80) == 0) return 5;

            if (_length == 5) throw new EndOfStreamException();
            chunk = _buffer[readPos++];
            value |= (chunk & 0x7F) << 35;
            if ((chunk & 0x80) == 0) return 6;

            if (_length == 6) throw new EndOfStreamException();
            chunk = _buffer[readPos++];
            value |= (chunk & 0x7F) << 42;
            if ((chunk & 0x80) == 0) return 7;

            if (_length == 7) throw new EndOfStreamException();
            chunk = _buffer[readPos++];
            value |= (chunk & 0x7F) << 49;
            if ((chunk & 0x80) == 0) return 8;

            if (_length == 8) throw new EndOfStreamException();
            chunk = _buffer[readPos++];
            value |= (chunk & 0x7F) << 56;
            if ((chunk & 0x80) == 0) return 9;

            if (_length == 9) throw new EndOfStreamException();
            chunk = _buffer[readPos++];
            value |= chunk << 63;
            if ((chunk & 0x01) == 0) return 10;

            throw new EndOfStreamException();
        }

        private bool TryReadUInt32Variant(out uint value)
        {
            int len = TryReadUInt32VariantWithoutMoving(false, out value);
            if (len > 0)
            {
                _index += len;
                _length -= len;
                _position += len;
                return true;
            }

            return false;
        }

        private uint ReadUInt32Variant(bool negative)
        {
            int len = TryReadUInt32VariantWithoutMoving(negative, out uint value);
            if (len == -1) throw new EndOfStreamException();

            _index += len;
            _length -= len;
            _position += len;
            return value;
        }

        private ulong ReadUInt64Varint()
        {
            int len = TryReadUInt64VariantWithoutMoving(out ulong value);
            if (len == -1) throw new EndOfStreamException();

            _index += len;
            _length -= len;
            _position += len;
            return value;
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                try
                {
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

        public bool TryReadFieldHeader(out uint field)
        {
            if (_wireType != WireType.None)
                throw new InvalidOperationException();

            if (TryReadUInt32Variant(out uint value))
            {
                _wireType = (WireType)(value & 7);
                _field = field = value >> 3;
                if (_wireType == WireType.EndGroup && _depth == 0)
                    throw new InvalidOperationException();

                return _wireType != WireType.EndGroup;
            }

            field = 0;
            return false;
        }

        public void SkipField()
        {
            switch (_wireType)
            {
                case WireType.Variant:
                    ReadUInt64Varint();
                    break;
                case WireType.Fixed16:
                    EnsureBuffer(2);
                    _index += 2;
                    _length -= 2;
                    _position += 2;
                    break;
                case WireType.Fixed32:
                    EnsureBuffer(4);
                    _index += 4;
                    _length -= 4;
                    _position += 4;
                    break;
                case WireType.Fixed64:
                    EnsureBuffer(8);
                    _index += 8;
                    _length -= 8;
                    _position += 8;
                    break;
                case WireType.String:
                    int len = checked((int)ReadUInt32Variant(false));
                    EnsureBuffer(len);
                    if (_length < len) throw new EndOfStreamException();

                    _index += len;
                    _length -= len;
                    _position += len;
                    break;
                case WireType.Binary:
                    int aryLen = checked((int)ReadUInt32Variant(false));
                    EnsureBuffer(aryLen);
                    if (_length < aryLen) throw new EndOfStreamException();

                    _index += aryLen;
                    _length -= aryLen;
                    _position += aryLen;
                    break;
                case WireType.StartGroup:
                    _depth++;
                    uint originalField = _field;
                    while (TryReadFieldHeader(out _))
                    {
                        SkipField();
                    }

                    _depth--;
                    if (_wireType != WireType.EndGroup || originalField != _field)
                        throw new InvalidOperationException();

                    break;
            }

            _wireType = WireType.None;
        }

        public SubItemToken StartSubItem()
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
            if (_depth <= 0 || _depth != token.depth)
                throw new InvalidOperationException();
            if (token.wireType != WireType.StartGroup)
                throw new InvalidOperationException();

            switch (_wireType)
            {
                case WireType.EndGroup:
                    _depth--;
                    _wireType = WireType.None;
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        public byte ReadByte()
        {
            switch (_wireType)
            {
                case WireType.Variant:
                    _wireType = WireType.None;
                    return checked((byte)ReadUInt32Variant(false));
                default:
                    throw new InvalidOperationException();
            }
        }

        public sbyte ReadSByte()
        {
            switch (_wireType)
            {
                case WireType.Variant:
                    return checked((sbyte)ReadInt32());
                default:
                    throw new InvalidOperationException();
            }
        }

        public short ReadInt16()
        {
            switch (_wireType)
            {
                case WireType.Variant:
                    return checked((short)ReadInt32());
                case WireType.Fixed16:
                    EnsureBuffer(2);
                    _position += 2;
                    _length -= 2;
                    _wireType = WireType.None;
                    return checked((short)(_buffer[_index++] | (_buffer[_index++] << 8)));
                default:
                    throw new InvalidOperationException();
            }
        }

        public ushort ReadUInt16()
        {
            switch (_wireType)
            {
                case WireType.Variant:
                    _wireType = WireType.None;
                    return checked((ushort)ReadUInt32Variant(false));
                case WireType.Fixed16:
                    EnsureBuffer(2);
                    _position += 2;
                    _length -= 2;
                    _wireType = WireType.None;
                    return checked((ushort)(_buffer[_index++] | (_buffer[_index++] << 8)));
                default:
                    throw new InvalidOperationException();
            }
        }

        public int ReadInt32()
        {
            switch (_wireType)
            {
                case WireType.Variant:
                    _wireType = WireType.None;
                    return checked((int)ReadUInt32Variant(false));
                case WireType.Fixed32:
                    EnsureBuffer(4);
                    _position += 4;
                    _length -= 4;
                    _wireType = WireType.None;
                    return checked(_buffer[_index++] | (_buffer[_index++] << 8) | (_buffer[_index++] << 16) | (_buffer[_index++] << 24));
                default:
                    throw new InvalidOperationException();
            }
        }

        public uint ReadUInt32()
        {
            switch (_wireType)
            {
                case WireType.Variant:
                    _wireType = WireType.None;
                    return ReadUInt32Variant(false);
                case WireType.Fixed32:
                    EnsureBuffer(4);
                    _position += 4;
                    _length -= 4;
                    _wireType = WireType.None;
                    return checked((uint)(_buffer[_index++] | (_buffer[_index++] << 8) | (_buffer[_index++] << 16) | (_buffer[_index++] << 24)));
                default:
                    throw new InvalidOperationException();
            }
        }

        public long ReadInt64()
        {
            switch (_wireType)
            {
                case WireType.Variant:
                    _wireType = WireType.None;
                    return checked((long)ReadUInt64Varint());
                case WireType.Fixed64:
                    EnsureBuffer(8);
                    _position += 8;
                    _length -= 8;
                    _wireType = WireType.None;
                    return ((long)_buffer[_index++] << 0) |
                        ((long)_buffer[_index++] << 8) |
                        ((long)_buffer[_index++] << 16) |
                        ((long)_buffer[_index++] << 24) |
                        ((long)_buffer[_index++] << 32) |
                        ((long)_buffer[_index++] << 40) |
                        ((long)_buffer[_index++] << 48) |
                        ((long)_buffer[_index++] << 56);
                default:
                    throw new InvalidOperationException();
            }
        }

        public ulong ReadUInt64()
        {
            switch (_wireType)
            {
                case WireType.Variant:
                    _wireType = WireType.None;
                    return ReadUInt64Varint();
                case WireType.Fixed64:
                    EnsureBuffer(8);
                    _position += 8;
                    _length -= 8;
                    _wireType = WireType.None;
                    return ((ulong)_buffer[_index++] << 0) |
                        ((ulong)_buffer[_index++] << 8) |
                        ((ulong)_buffer[_index++] << 16) |
                        ((ulong)_buffer[_index++] << 24) |
                        ((ulong)_buffer[_index++] << 32) |
                        ((ulong)_buffer[_index++] << 40) |
                        ((ulong)_buffer[_index++] << 48) |
                        ((ulong)_buffer[_index++] << 56);
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
            switch (_wireType)
            {
                case WireType.Variant:
                    _wireType = WireType.None;
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
            if (_wireType != WireType.String)
                throw new InvalidOperationException();

            var length = checked((int)ReadUInt32Variant(false));
            if (length == 0)
            {
                _wireType = WireType.None;
                return string.Empty;
            }

            EnsureBuffer(length);
            if (_length < length)
                throw new EndOfStreamException();

            var str = _encoding.GetString(_buffer, _index, length);
            _index += length;
            _length -= length;
            _position += length;
            _wireType = WireType.None;
            return str;
        }

        public byte[] ReadBytes()
        {
            if (_wireType != WireType.Binary)
                throw new InvalidOperationException();

            var len = checked((int)ReadUInt32Variant(false));
            if (len == 0)
            {
                _wireType = WireType.None;
                return EmptyArray<byte>.Empty;
            }

            EnsureBuffer(len);
            if (_length < len)
                throw new EndOfStreamException();

            var array = new byte[len];
            Array.Copy(_buffer, _index, array, 0, len);
            _index += len;
            _length -= len;
            _position += len;
            _wireType = WireType.None;
            return array;
        }

        public Enum ReadEnum(Type enumType)
        {
            if (enumType == null)
                throw new ArgumentNullException(nameof(enumType));

            var underlyingType = Enum.GetUnderlyingType(enumType);
            switch (Type.GetTypeCode(underlyingType))
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
            return (T)ReadEnum(typeof(T));
        }
    }
}
