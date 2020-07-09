namespace Framework.Data.Expressions
{
    public class SqlMemberExpression : SqlExpression
    {
        private readonly string _name;

        internal SqlMemberExpression(string name)
        {
            _name = name;
        }

        public sealed override SqlExpressionType NodeType => SqlExpressionType.MemberAccess;

        public string Name => _name;

        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Member(this);
        }
    }
}
