namespace Framework.Configuration.Hocon
{
    public enum TokenKind : byte
    {
        Comment,
        Key,
        LiteralValue,
        Assign,
        ObjectStart,
        ObjectEnd,
        Dot,
        EoF,
        ArrayStart,
        ArrayEnd,
        Comma,
        Substitute,
        Include,
    }
}
