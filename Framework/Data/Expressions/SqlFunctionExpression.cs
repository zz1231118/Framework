using System.Collections.Generic;

namespace Framework.Data.Expressions
{
    public class SqlFunctionExpression : SqlExpression
    {
        private readonly string name;
        private readonly IReadOnlyCollection<SqlExpression> arguments;

        internal SqlFunctionExpression(string name, IReadOnlyCollection<SqlExpression> arguments)
        {
            this.name = name;
            this.arguments = arguments;
        }

        public sealed override SqlExpressionType NodeType => SqlExpressionType.Function;

        public string Name => name;

        public IReadOnlyCollection<SqlExpression> Arguments => arguments;

        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Function(this);
        }
    }
}
