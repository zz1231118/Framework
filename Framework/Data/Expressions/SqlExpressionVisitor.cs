using System.Text;

namespace Framework.Data.Expressions
{
    public abstract class SqlExpressionVisitor
    {
        private readonly StringBuilder sqlBuilder = new StringBuilder();

        protected StringBuilder SqlBuilder => sqlBuilder;

        public abstract void Constant(SqlConstantExpression node);
        public abstract void Parameter(SqlParameterExpression node);
        public abstract void Member(SqlMemberExpression node);
        public abstract void Binary(SqlBinaryExpression node);
        public abstract void Unary(SqlUnaryExpression node);
        public abstract void NewArray(SqlNewArrayExpression node);
        public abstract void Symbol(SqlSymbolExpression node);
        public abstract void Function(SqlFunctionExpression node);

        public abstract void Select(SqlSelectLambdaExpression node);
        public abstract void Insert(SqlInsertLambdaExpression node);
        public abstract void Update(SqlUpdateLambdaExpression node);
        public abstract void Delete(SqlDeleteLamdbaExpression node);

        public string Complier()
        {
            return sqlBuilder.ToString();
        }
        public void Reset()
        {
            sqlBuilder.Clear();
        }
    }
}
