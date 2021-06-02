using System.Collections.Generic;

namespace Framework.Data.Expressions
{
    /// <inheritdoc />
    public class SqlInsertLambdaExpression : SqlExpression
    {
        internal SqlInsertLambdaExpression(IReadOnlyCollection<SqlExpression> columns, SqlExpression from, IReadOnlyCollection<IReadOnlyCollection<SqlExpression>> values)
        {
            Columns = columns;
            From = from;
            Values = values;
        }

        /// <inheritdoc />
        public override SqlExpressionType NodeType => SqlExpressionType.Insert;

        /// <inheritdoc />
        public IReadOnlyCollection<SqlExpression> Columns { get; }

        /// <inheritdoc />
        public SqlExpression From { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<IReadOnlyCollection<SqlExpression>> Values { get; }

        /// <inheritdoc />
        protected internal override void Accept(SqlExpressionVisitor visitor)
        {
            visitor.Insert(this);
        }
    }
}
