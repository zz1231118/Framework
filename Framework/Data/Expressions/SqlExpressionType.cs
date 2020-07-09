namespace Framework.Data.Expressions
{
    public enum SqlExpressionType
    {
        And,
        Or,
        In,
        Is,
        As,

        GreaterThan,
        GreaterThanOrEqual,
        Equal,
        LessThanOrEqual,
        LessThan,
        NotEqual,

        Negate,
        Not,

        Symbol,
        Constant,
        Parameter,
        Array,
        MemberAccess,
        MethodCall,
        Function,

        Select,
        Insert,
        Update,
        Delete,
    }
}
