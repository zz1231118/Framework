namespace Framework.Configuration.Hocon
{
    internal class Token
    {
        private readonly TokenKind kind;
        private readonly string value;

        public Token(TokenKind kind, string value)
        {
            this.kind = kind;
            this.value = value;
        }

        public Token(string value)
        {
            this.kind = TokenKind.LiteralValue;
            this.value = value;
        }

        public TokenKind Kind => kind;

        public string Value => value;

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
