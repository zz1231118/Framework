namespace Framework.Configuration.Hocon
{
    public class Token
    {
        internal Token(TokenKind kind, string value)
        {
            Kind = kind;
            Value = value;
        }

        public Token(TokenKind type)
        {
            Kind = type;
        }

        public Token(string value)
        {
            Kind = TokenKind.LiteralValue;
            Value = value;
        }

        public TokenKind Kind { get; private set; }

        public string Value { get; set; }

        public static Token Key(string key)
        {
            return new Token(TokenKind.Key, key);
        }

        public static Token Substitution(string path)
        {
            return new Token(TokenKind.Substitute, path);
        }

        public static Token LiteralValue(string value)
        {
            return new Token(TokenKind.LiteralValue, value);
        }

        internal static Token Include(string path)
        {
            return new Token(TokenKind.Include, path);
        }
    }
}
