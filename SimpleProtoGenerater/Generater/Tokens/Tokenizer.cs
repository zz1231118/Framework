using System;

namespace SimpleProtoGenerater.Generater.Tokens
{
    public class Tokenizer
    {
        private readonly TokenReader _reader;
        private readonly Token[] _buffer = new Token[256];
        private int _readPos;
        private int _readEnd;

        public Tokenizer(TokenReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            _reader = reader;
        }

        private void Ensure(int count)
        {
            if (_readEnd - _readPos < count)
            {
                if (_buffer.Length - _readEnd < count)
                {
                    Array.Copy(_buffer, _readPos, _buffer, 0, _readEnd - _readPos);
                    _readEnd -= _readPos;
                    _readPos = 0;
                }
                while (_readEnd - _readPos < count && !_reader.EndOfStream)
                {
                    var val = _reader.ReadToken();
                    if (val == Token.EOF)
                        break;
                    if (val.Kind == TokenKind.Comment)
                        continue;

                    _buffer[_readEnd++] = val;
                }
            }
        }

        public Token Peek(int offset = 0)
        {
            if (offset >= _buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            Ensure(offset + 1);
            var pos = _readPos + offset;
            return pos < _readEnd ? _buffer[_readPos + offset] : Token.EOF;
        }
        public Token Read()
        {
            Ensure(1);
            return _readPos < _readEnd
                ? _buffer[_readPos++]
                : Token.EOF;
        }
    }
}
