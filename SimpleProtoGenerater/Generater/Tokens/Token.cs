using System;

namespace SimpleProtoGenerater.Generater.Tokens
{
    public class Token : IEquatable<Token>
    {
        public const string EolSymbol = "<eol>";
        public const string EofSymbol = "<eof>";
        public static readonly Token EOF = new Token(TokenKind.Symbol, EofSymbol, EofSymbol, SourceLocation.Empty);

        private readonly TokenKind _kind;
        private readonly string _image;
        private readonly object _value;
        private readonly SourceLocation _location;

        public Token(TokenKind kind, string image, object value, SourceLocation location)
        {
            _kind = kind;
            _image = image;
            _value = value;
            _location = location;
        }

        public TokenKind Kind => _kind;
        public object Value => _value;
        public string Image => _image;
        public SourceLocation Location => _location;

        public bool Equals(Token other)
        {
            return other != null && other._kind == _kind && other._image == _image && other._value == _value;
        }

        public override int GetHashCode()
        {
            return _value == null
                ? _kind.GetHashCode() ^ _image.GetHashCode()
                : _kind.GetHashCode() ^ _image.GetHashCode() ^ _value.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as Token);
        }
        public override string ToString()
        {
            return string.Format("{{Kind:{0} Image:{1} Value:{2}}}", _kind, _image, _value);
        }
    }
}
