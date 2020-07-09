namespace Framework.Data.Expressions
{
    public class SqlParameterExpression : SqlExpression
    {
        private readonly string _name;

        internal SqlParameterExpression(string name)
        {
            _name = name;
        }

        public sealed override SqlExpressionType NodeType => SqlExpressionType.Parameter;
        public string Name => _name;

        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Parameter(this);
        }
    }
}
