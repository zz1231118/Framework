using System;
using System.IO;
using System.Text;

namespace Framework.Runtime.Serialization.Protobuf
{
    public sealed class ProtoStreamWriter : BaseDisposed
    {
        private Encoding _encoding = Encoding.UTF8;
        private Stream _ioStream;
        private long _position;
        private byte[] _buffer;
        private int _index;

        public ProtoStreamWriter(Stream ioStream)
            : this(ioStream, Encoding.UTF8)
        { }

        public ProtoStreamWriter(Stream ioStream, Encoding encoding)
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
            Ensure(5);
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
            Ensure(10);
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
            uint value = (field << 3) | (((uint)wireType) & 7);
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
                    _encoding = null;
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

        public void Write(byte value)
        {
            WriteUInt32Variant(value);
        }

        public void Write(sbyte value)
        {
            Write((int)value);
        }

        public void Write(short value)
        {
            Write((int)value);
        }

        public void Write(ushort value)
        {
            WriteUInt32Variant(value);
        }

        public void Write(int value)
        {
            WriteUInt32Variant((uint)value);
        }

        public void Write(uint value)
        {
            WriteUInt32Variant(value);
        }

        public void Write(long value)
        {
            WriteUInt64Variant((ulong)value);
        }

        public void Write(ulong value)
        {
            WriteUInt64Variant(value);
        }

        public void Write(float value)
        {
            Write(BitConverter.ToInt32(BitConverter.GetBytes(value), 0));
        }

        public void Write(double value)
        {
            Write(BitConverter.ToInt64(BitConverter.GetBytes(value), 0));
        }

        public void Write(bool value)
        {
            WriteUInt32Variant(value ? 1U : 0U);
        }

        public void Write(char value)
        {
            Write((short)value);
        }

        public void Write(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Length == 0)
            {
                WriteUInt32Variant(0);
                return;
            }

            var len = _encoding.GetByteCount(value);
            WriteUInt32Variant((uint)len);
            Ensure(len);
            len = _encoding.GetBytes(value, 0, value.Length, _buffer, _index);
            _index += len;
            _position += len;
        }

        public void Write(byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Length == 0)
            {
                WriteUInt32Variant(0);
                return;
            }

            WriteUInt32Variant((uint)value.Length);
            Ensure(value.Length);
            Array.Copy(value, 0, _buffer, _index, value.Length);
            _index += value.Length;
            _position += value.Length;
        }
    }
}
