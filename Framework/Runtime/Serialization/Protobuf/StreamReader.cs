using System;
using System.IO;
using System.Text;

namespace Framework.Runtime.Serialization.Protobuf
{
    public sealed class StreamReader : BaseDisposed
    {
        private const int DefaultBufferSize = 1024;

        private readonly Stream outputStream;
        private readonly Encoding encoding;
        private readonly bool leaveOpen;
        private readonly byte[] buffer;
        private readonly int bufferLimit;
        private int bufferPos;
        private int bufferSize;

        public StreamReader(Stream outputStream)
            : this(outputStream, Encoding.UTF8, DefaultBufferSize, false)
        { }

        public StreamReader(Stream outputStream, bool leaveOpen)
            : this(outputStream, Encoding.UTF8, DefaultBufferSize, leaveOpen)
        { }

        public StreamReader(Stream outputStream, Encoding encoding)
            : this(outputStream, encoding, DefaultBufferSize, false)
        { }

        public StreamReader(Stream outputStream, Encoding encoding, int bufferSize, bool leaveOpen)
        {
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            this.outputStream = outputStream;
            this.encoding = encoding;
            this.leaveOpen = leaveOpen;
            this.buffer = new byte[bufferSize];
            this.bufferPos = 0;
            this.bufferSize = 0;
            this.bufferLimit = bufferSize;
        }

        public long Position => outputStream.Position - (bufferSize - bufferPos);

        public long Length => outputStream.Length;

        private void RefillBuffer(bool mustSucceed)
        {
            if (bufferPos < bufferSize)
                throw new InvalidOperationException("RefillBuffer() called when buffer wasn't empty.");

            bufferPos = 0;
            bufferSize = outputStream.Read(buffer, 0, buffer.Length);
            if (bufferSize < 0)
            {
                throw new InvalidOperationException("Stream.Read returned a negative count");
            }
            if (bufferSize == 0 && mustSucceed)
            {
                throw new EndOfStreamException();
            }
        }

        private byte ReadRawByte()
        {
            if (bufferPos == bufferSize)
            {
                RefillBuffer(true);
            }

            return buffer[bufferPos++];
        }

        private byte[] ReadRawBytes(int size)
        {
            var bytes = new byte[size];
            if (size <= bufferSize - bufferPos)
            {
                ByteArray.Copy(buffer, bufferPos, bytes, 0, size);
                bufferPos += size;
                return bytes;
            }

            int bytesRead;
            int writePos = 0;

            do
            {
                if (bufferPos == bufferSize)
                {
                    RefillBuffer(true);
                }

                bytesRead = Math.Min(bufferSize - bufferPos, size - writePos);
                ByteArray.Copy(buffer, bufferPos, bytes, writePos, bytesRead);
                bufferPos += bytesRead;
                writePos += bytesRead;
            } while (writePos < size);
            return bytes;
        }

        private uint SlowReadRawVarint32()
        {
            int value = ReadRawByte();
            if (value < 0x80) return (uint)value;
            int result = value & 0x7F;
            if ((value = ReadRawByte()) < 0x80)
            {
                result |= value << 7;
                return (uint)result;
            }
            result |= (value & 0x7F) << 7;
            if ((value = ReadRawByte()) < 0x80)
            {
                result |= value << 14;
                return (uint)result;
            }
            result |= (value & 0x7F) << 14;
            if ((value = ReadRawByte()) < 0x80)
            {
                result |= value << 21;
                return (uint)result;
            }
            result |= (value & 0x7F) << 21;
            if ((value = ReadRawByte()) < 0x80)
            {
                result |= value << 28;
                return (uint)result;
            }

            throw new InvalidOperationException("malformed varint");
        }

        private ulong SlowReadRawVarint64()
        {
            long value = ReadRawByte();
            if (value < 0x80) return (ulong)value;
            long result = value & 0x7F;
            if ((value = ReadRawByte()) < 0x80)
            {
                result |= value << 7;
                return (ulong)result;
            }
            result |= (value & 0x7F) << 7;
            if ((value = ReadRawByte()) < 0x80)
            {
                result |= value << 14;
                return (ulong)result;
            }
            result |= (value & 0x7F) << 14;
            if ((value = ReadRawByte()) < 0x80)
            {
                result |= value << 21;
                return (ulong)result;
            }
            result |= (value & 0x7F) << 21;
            if ((value = ReadRawByte()) < 0x80)
            {
                result |= value << 28;
                return (ulong)result;
            }
            result |= (value & 0x7F) << 28;
            if ((value = ReadRawByte()) < 0x80)
            {
                result |= value << 35;
                return (ulong)result;
            }
            result |= (value & 0x7F) << 35;
            if ((value = ReadRawByte()) < 0x80)
            {
                result |= value << 42;
                return (ulong)result;
            }
            result |= (value & 0x7F) << 42;
            if ((value = ReadRawByte()) < 0x80)
            {
                result |= value << 49;
                return (ulong)result;
            }
            result |= (value & 0x7F) << 49;
            if ((value = ReadRawByte()) < 0x80)
            {
                result |= value << 56;
                return (ulong)result;
            }
            result |= (value & 0x7F) << 56;
            if ((value = ReadRawByte()) < 0x80)
            {
                result |= value << 63;
                return (ulong)result;
            }

            throw new InvalidOperationException("malformed varint");
        }

        private uint ReadRawVarint32()
        {
            if (bufferPos + 5 > bufferSize)
            {
                return SlowReadRawVarint32();
            }

            int value = buffer[bufferPos++];
            if (value < 0x80) return (uint)value;
            int result = value & 0x7F;
            if ((value = buffer[bufferPos++]) < 0x80)
            {
                result |= value << 7;
                return (uint)result;
            }
            result |= (value & 0x7F) << 7;
            if ((value = buffer[bufferPos++]) < 0x80)
            {
                result |= value << 14;
                return (uint)result;
            }
            result |= (value & 0x7F) << 14;
            if ((value = buffer[bufferPos++]) < 0x80)
            {
                result |= value << 21;
                return (uint)result;
            }
            result |= (value & 0x7F) << 21;
            if ((value = buffer[bufferPos++]) < 0x80)
            {
                result |= value << 28;
                return (uint)result;
            }

            throw new InvalidOperationException("malformed varint");
        }

        private ulong ReadRawVarint64()
        {
            if (bufferPos + 10 > bufferSize)
            {
                return SlowReadRawVarint64();
            }

            long value = buffer[bufferPos++];
            if (value < 0x80) return (ulong)value;
            long result = value & 0x7F;
            if ((value = buffer[bufferPos++]) < 0x80)
            {
                result |= value << 7;
                return (ulong)result;
            }
            result |= (value & 0x7F) << 7;
            if ((value = buffer[bufferPos++]) < 0x80)
            {
                result |= value << 14;
                return (ulong)result;
            }
            result |= (value & 0x7F) << 14;
            if ((value = buffer[bufferPos++]) < 0x80)
            {
                result |= value << 21;
                return (ulong)result;
            }
            result |= (value & 0x7F) << 21;
            if ((value = buffer[bufferPos++]) < 0x80)
            {
                result |= value << 28;
                return (ulong)result;
            }
            result |= (value & 0x7F) << 28;
            if ((value = buffer[bufferPos++]) < 0x80)
            {
                result |= value << 35;
                return (ulong)result;
            }
            result |= (value & 0x7F) << 35;
            if ((value = buffer[bufferPos++]) < 0x80)
            {
                result |= value << 42;
                return (ulong)result;
            }
            result |= (value & 0x7F) << 42;
            if ((value = buffer[bufferPos++]) < 0x80)
            {
                result |= value << 49;
                return (ulong)result;
            }
            result |= (value & 0x7F) << 49;
            if ((value = buffer[bufferPos++]) < 0x80)
            {
                result |= value << 56;
                return (ulong)result;
            }
            result |= (value & 0x7F) << 56;
            if ((value = buffer[bufferPos++]) < 0x80)
            {
                result |= value << 63;
                return (ulong)result;
            }

            throw new InvalidOperationException("malformed varint");
        }

        private uint ReadRawLittleEndian32()
        {
            if (bufferPos + 4 <= bufferSize)
            {
                byte n1 = buffer[bufferPos++];
                uint n2 = buffer[bufferPos++];
                uint n3 = buffer[bufferPos++];
                uint n4 = buffer[bufferPos++];
                return n1 | (n2 << 8) | (n3 << 16) | (n4 << 24);
            }
            else
            {
                byte n1 = ReadRawByte();
                uint n2 = ReadRawByte();
                uint n3 = ReadRawByte();
                uint n4 = ReadRawByte();
                return n1 | (n2 << 8) | (n3 << 16) | (n4 << 24);
            }
        }

        private ulong ReadRawLittleEndian64()
        {
            if (bufferPos + 8 <= bufferSize)
            {
                ulong n1 = buffer[bufferPos++];
                ulong n2 = buffer[bufferPos++];
                ulong n3 = buffer[bufferPos++];
                ulong n4 = buffer[bufferPos++];
                ulong n5 = buffer[bufferPos++];
                ulong n6 = buffer[bufferPos++];
                ulong n7 = buffer[bufferPos++];
                ulong n8 = buffer[bufferPos++];
                return n1 | (n2 << 8) | (n3 << 16) | (n4 << 24) | (n5 << 32) | (n6 << 40) | (n7 << 48) | (n8 << 56);
            }
            else
            {
                ulong n1 = ReadRawByte();
                ulong n2 = ReadRawByte();
                ulong n3 = ReadRawByte();
                ulong n4 = ReadRawByte();
                ulong n5 = ReadRawByte();
                ulong n6 = ReadRawByte();
                ulong n7 = ReadRawByte();
                ulong n8 = ReadRawByte();
                return n1 | (n2 << 8) | (n3 << 16) | (n4 << 24) | (n5 << 32) | (n6 << 40) | (n7 << 48) | (n8 << 56);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                try
                {
                    if (!leaveOpen)
                    {
                        outputStream.Dispose();
                    }
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }

        public bool ReadBoolean()
        {
            return ReadRawByte() == 1;
        }

        public byte ReadByte()
        {
            return ReadRawByte();
        }

        public sbyte ReadSByte()
        {
            return (sbyte)ReadRawByte();
        }

        public short ReadInt16()
        {
            return checked((short)ReadRawVarint32());
        }

        public ushort ReadUInt16()
        {
            return checked((ushort)ReadRawVarint32());
        }

        public int ReadInt32()
        {
            return (int)ReadRawVarint32();
        }

        public uint ReadUInt32()
        {
            return ReadRawVarint32();
        }

        public long ReadInt64()
        {
            return (long)ReadRawVarint64();
        }

        public ulong ReadUInt64()
        {
            return ReadRawVarint64();
        }

        public float ReadSingle()
        {
            var value = ReadInt32();
            var bytes = BitConverter.GetBytes(value);
            return BitConverter.ToSingle(bytes, 0);
        }

        public double ReadDouble()
        {
            var value = ReadInt64();
            var bytes = BitConverter.GetBytes(value);
            return BitConverter.ToDouble(bytes, 0);
        }

        public char ReadChar()
        {
            return (char)ReadInt16();
        }

        public string ReadString()
        {
            var length = ReadInt32();
            if (length == 0) return string.Empty;

            var bytes = ReadRawBytes(length);
            return encoding.GetString(bytes);
        }

        public byte[] ReadBytes()
        {
            var length = ReadInt32();
            return length == 0 ? EmptyArray<byte>.Empty : ReadRawBytes(length);
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
    }
}
