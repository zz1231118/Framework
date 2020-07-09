using System;
using System.IO;
using System.Text;

namespace Framework.Runtime.Serialization.Protobuf
{
    public sealed class ProtoStreamReader : BaseDisposed
    {
        private Encoding _encoding = Encoding.UTF8;
        private Stream _ioStream;
        private long _position;
        private byte[] _buffer;
        private int _index;
        private int _length;

        public ProtoStreamReader(Stream ioStream)
            : this(ioStream, Encoding.UTF8)
        { }
        public ProtoStreamReader(Stream ioStream, Encoding encoding)
        {
            if (ioStream == null)
                throw new ArgumentNullException(nameof(ioStream));
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));

            _ioStream = ioStream;
            _encoding = encoding;

            _buffer = BufferPool.GetBuffer();
        }

        public long Position => _position;

        public long Length => _ioStream.Length;

        private void Ensure(int count)
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
            Ensure(10);

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
            Ensure(10);

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
                    _encoding = null;
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }

        public byte ReadByte()
        {
            return checked((byte)ReadUInt32Variant(false));
        }

        public sbyte ReadSByte()
        {
            checked { return (sbyte)ReadInt32(); }
        }

        public short ReadInt16()
        {
            checked { return (short)ReadInt32(); };
        }

        public ushort ReadUInt16()
        {
            return checked((ushort)ReadUInt32Variant(false));
        }

        public int ReadInt32()
        {
            return checked((int)ReadUInt32Variant(true));
        }

        public uint ReadUInt32()
        {
            return ReadUInt32Variant(false);
        }

        public long ReadInt64()
        {
            return (long)ReadUInt64Varint();
        }

        public ulong ReadUInt64()
        {
            return ReadUInt64Varint();
        }

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

        public bool ReadBoolean()
        {
            return ReadUInt32Variant(false) == 1;
        }

        public char ReadChar()
        {
            return (char)ReadInt16();
        }

        public string ReadString()
        {
            var len = checked((int)ReadUInt32Variant(false));
            if (len == 0) return string.Empty;

            Ensure(len);
            if (_length < len) throw new EndOfStreamException();
            var str = _encoding.GetString(_buffer, _index, len);
            _index += len;
            _length -= len;
            _position += len;
            return str;
        }

        public byte[] ReadBytes()
        {
            var len = checked((int)ReadUInt32Variant(false));
            if (len == 0) return EmptyArray<byte>.Empty;

            Ensure(len);
            if (_length < len) throw new EndOfStreamException();

            var array = new byte[len];
            Array.Copy(_buffer, _index, array, 0, len);
            _index += len;
            _length -= len;
            _position += len;
            return array;
        }
    }
}
