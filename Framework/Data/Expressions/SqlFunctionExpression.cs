using System.Collections.Generic;

namespace Framework.Data.Expressions
{
    /// <inheritdoc />
    public class SqlFunctionExpression : SqlExpression
    {
        private readonly string name;
        private readonly IReadOnlyCollection<SqlExpression> arguments;

        internal SqlFunctionExpression(string name, IReadOnlyCollection<SqlExpression> arguments)
        {
            this.name = name;
            this.arguments = arguments;
        }

        /// <inheritdoc />
        public sealed override SqlExpressionType NodeType => SqlExpressionType.Function;

        /// <inheritdoc />
        public string Name => name;

        /// <inheritdoc />
        public IReadOnlyCollection<SqlExpression> Arguments => arguments;

        /// <inheritdoc />
        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Function(this);
        }
    }
}
