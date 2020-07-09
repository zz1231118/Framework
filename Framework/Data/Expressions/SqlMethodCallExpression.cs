using System.Collections.Generic;

namespace Framework.Data.Expressions
{
    public class SqlMethodCallExpression : SqlExpression
    {
        internal SqlMethodCallExpression(string name, IReadOnlyCollection<SqlExpression> arguments)
        {
            Name = name;
            Arguments = arguments;
        }

        public override SqlExpressionType NodeType => SqlExpressionType.MethodCall;

        public string Name { get; }

        public IReadOnlyCollection<SqlExpression> Arguments { get; }

        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            throw new System.NotImplementedException();
        }
    }
}
