using System.Text;

namespace Framework.Data.Expressions
{
    /// <inheritdoc />
    public abstract class SqlExpressionVisitor
    {
        private readonly StringBuilder sqlBuilder = new StringBuilder();

        /// <inheritdoc />
        protected StringBuilder SqlBuilder => sqlBuilder;

        /// <inheritdoc />
        public abstract void Constant(SqlConstantExpression node);
        /// <inheritdoc />
        public abstract void Parameter(SqlParameterExpression node);
        /// <inheritdoc />
        public abstract void Member(SqlMemberExpression node);
        /// <inheritdoc />
        public abstract void Binary(SqlBinaryExpression node);
        /// <inheritdoc />
        public abstract void Unary(SqlUnaryExpression node);
        /// <inheritdoc />
        public abstract void NewArray(SqlNewArrayExpression node);
        /// <inheritdoc />
        public abstract void Symbol(SqlSymbolExpression node);
        /// <inheritdoc />
        public abstract void Function(SqlFunctionExpression node);

        /// <inheritdoc />
        public abstract void Select(SqlSelectLambdaExpression node);
        /// <inheritdoc />
        public abstract void Insert(SqlInsertLambdaExpression node);
        /// <inheritdoc />
        public abstract void Update(SqlUpdateLambdaExpression node);
        /// <inheritdoc />
        public abstract void Delete(SqlDeleteLamdbaExpression node);

        /// <inheritdoc />
        public string Complier()
        {
            return sqlBuilder.ToString();
        }
        /// <inheritdoc />
        public void Reset()
        {
            sqlBuilder.Clear();
        }
    }
}
