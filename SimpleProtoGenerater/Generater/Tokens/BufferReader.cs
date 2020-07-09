using System;
using System.IO;
using Framework;

namespace SimpleProtoGenerater.Generater.Tokens
{
    class BufferReader : BaseDisposed
    {
        private TextReader _reader;
        private char[] _buffer = new char[1024];
        private int _readPos;
        private int _readEnd;

        private bool _endOfStream;
        private long _position;
        private int _row = 1;
        private int _column = 0;

        public BufferReader(TextReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            _reader = reader;
            _endOfStream = reader.Peek() == -1;
        }

        public bool EndOfStream => _endOfStream;
        public long Position => _position;
        public int Row => _row;
        public int Column => _column;

        private void Ensure(int count)
        {
            int available = _readEnd - _readPos;
            if (available < count)
            {
                if (count > _buffer.Length)
                {
                    int newLength = _buffer.Length * 2;
                    if (newLength < count) newLength = count;
                    Array.Resize(ref _buffer, newLength);
                }
                if (available + (_buffer.Length - _readEnd) < count)
                {
                    Array.Copy(_buffer, _readPos, _buffer, 0, available);
                    _readEnd -= _readPos;
                    _readPos = 0;
                }

                int length;
                do
                {
                    length = _reader.Read(_buffer, _readEnd, _buffer.Length - _readEnd);
                    if (length <= 0)
                        return;

                    _readEnd += length;
                    available += length;
                } while (_readEnd < _buffer.Length);
            }
        }
        public int Peek(int offset = 0)
        {
            Ensure(offset + 1);
            var position = _readPos + offset;
            return position < _readEnd ? _buffer[position] : -1;
        }
        public int Read()
        {
            Ensure(1);
            if (_readPos >= _readEnd)
            {
                _endOfStream = true;
                return -1;
            }

            var value = _buffer[_readPos++];
            _position++;
            _column++;

            if (value == '\n')
            {
                _row++;
                _column = 0;
            }

            return value;
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                try
                {
                    _reader.Dispose();

                    _reader = null;
                    _buffer = null;
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }
    }
}
