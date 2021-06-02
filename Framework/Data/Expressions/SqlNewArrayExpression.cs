using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Data.Expressions
{
    /// <inheritdoc />
    public class SqlNewArrayExpression : SqlExpression
    {
        private readonly SqlExpression[] _array;

        internal SqlNewArrayExpression(IEnumerable<SqlExpression> expressions)
        {
            if (expressions == null)
                throw new ArgumentNullException(nameof(expressions));

            _array = expressions.ToArray();
        }

        /// <inheritdoc />
        public override SqlExpressionType NodeType => SqlExpressionType.Array;

        /// <inheritdoc />
        public IReadOnlyCollection<SqlExpression> Items => _array;

        /// <inheritdoc />
        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.NewArray(this);
        }
    }
}
