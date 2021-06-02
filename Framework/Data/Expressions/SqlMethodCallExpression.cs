using System.Collections.Generic;

namespace Framework.Data.Expressions
{
    /// <inheritdoc />
    public class SqlMethodCallExpression : SqlExpression
    {
        internal SqlMethodCallExpression(string name, IReadOnlyCollection<SqlExpression> arguments)
        {
            Name = name;
            Arguments = arguments;
        }

        /// <inheritdoc />
        public override SqlExpressionType NodeType => SqlExpressionType.MethodCall;

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<SqlExpression> Arguments { get; }

        /// <inheritdoc />
        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            throw new System.NotImplementedException();
        }
    }
}
