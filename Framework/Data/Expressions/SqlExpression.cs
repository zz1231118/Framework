using System;
using System.Collections.Generic;

namespace Framework.Data.Expressions
{
    /// <inheritdoc />
    public abstract class SqlExpression
    {
        /// <inheritdoc />
        public abstract SqlExpressionType NodeType { get; }

        /// <inheritdoc />
        public static SqlConstantExpression Constant(object? value)
        {
            //System.Linq.Expressions.Expression ex;
            //System.Linq.Expressions.BinaryExpression a;
            //System.Linq.Expressions.Expression<Func<object, bool>> p;
            //System.Linq.Expressions.MemberExpression b;
            //System.Linq.Expressions.Expression.MakeMemberAccess(,);
            //protected internal override Expression Accept(ExpressionVisitor visitor);
            return new SqlConstantExpression(value);
        }

        /// <inheritdoc />
        public static SqlMemberExpression Member(string name)
        {
            return new SqlMemberExpression(name);
        }

        /// <inheritdoc />
        public static SqlMemberExpression Member<T>(System.Linq.Expressions.Expression<Func<T, object>> expression)
            where T : class
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var member = expression.Body as System.Linq.Expressions.MemberExpression;
            if (member == null)
            {
                throw new InvalidOperationException($"Expression body type should be {typeof(System.Linq.Expressions.MemberExpression).FullName}");
            }
            return Member(member.Member.Name);
        }

        /// <inheritdoc />
        public static SqlParameterExpression Paramter(string name)
        {
            return new SqlParameterExpression(name);
        }

        /// <inheritdoc />
        public static SqlUnaryExpression Negate(SqlExpression node)
        {
            return new SqlUnaryExpression(SqlExpressionType.Negate, node);
        }

        /// <inheritdoc />
        public static SqlUnaryExpression Not(SqlExpression node)
        {
            return new SqlUnaryExpression(SqlExpressionType.Not, node);
        }

        /// <inheritdoc />
        public static SqlBinaryExpression And(SqlExpression left, SqlExpression right)
        {
            return new SqlBinaryExpression(SqlExpressionType.And, left, right);
        }

        /// <inheritdoc />
        public static SqlBinaryExpression Or(SqlExpression left, SqlExpression right)
        {
            return new SqlBinaryExpression(SqlExpressionType.Or, left, right);
        }

        /// <inheritdoc />
        public static SqlBinaryExpression In(SqlExpression left, SqlExpression right)
        {
            return new SqlBinaryExpression(SqlExpressionType.In, left, right);
        }

        /// <inheritdoc />
        public static SqlBinaryExpression Is(SqlExpression left, SqlExpression right)
        {
            return new SqlBinaryExpression(SqlExpressionType.Is, left, right);
        }

        /// <inheritdoc />
        public static SqlBinaryExpression As(SqlExpression expression, string name)
        {
            return new SqlBinaryExpression(SqlExpressionType.As, expression, new SqlMemberExpression(name));
        }

        /// <inheritdoc />
        public static SqlBinaryExpression GreaterThan(SqlExpression left, SqlExpression right)
        {
            return new SqlBinaryExpression(SqlExpressionType.GreaterThan, left, right);
        }

        /// <inheritdoc />
        public static SqlBinaryExpression GreaterThanOrEqual(SqlExpression left, SqlExpression right)
        {
            return new SqlBinaryExpression(SqlExpressionType.GreaterThanOrEqual, left, right);
        }

        /// <inheritdoc />
        public static SqlBinaryExpression Equal(SqlExpression left, SqlExpression right)
        {
            return new SqlBinaryExpression(SqlExpressionType.Equal, left, right);
        }

        /// <inheritdoc />
        public static SqlBinaryExpression LessThanOrEqual(SqlExpression left, SqlExpression right)
        {
            return new SqlBinaryExpression(SqlExpressionType.LessThanOrEqual, left, right);
        }

        /// <inheritdoc />
        public static SqlBinaryExpression LessThan(SqlExpression left, SqlExpression right)
        {
            return new SqlBinaryExpression(SqlExpressionType.LessThan, left, right);
        }

        /// <inheritdoc />
        public static SqlBinaryExpression NotEqual(SqlExpression left, SqlExpression right)
        {
            return new SqlBinaryExpression(SqlExpressionType.NotEqual, left, right);
        }

        /// <inheritdoc />
        public static SqlNewArrayExpression NewArray(IEnumerable<SqlExpression> expressions)
        {
            if (expressions == null)
                throw new ArgumentNullException(nameof(expressions));

            return new SqlNewArrayExpression(expressions);
        }

        /// <inheritdoc />
        public static SqlNewArrayExpression NewArray(params SqlExpression[] expressions)
        {
            if (expressions == null)
                throw new ArgumentNullException(nameof(expressions));

            return new SqlNewArrayExpression(expressions);
        }

        /// <inheritdoc />
        public static SqlSymbolExpression Symbol(string token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            return new SqlSymbolExpression(token);
        }

        /// <inheritdoc />
        public static SqlFunctionExpression Function(string name, params SqlExpression[] arguments)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return new SqlFunctionExpression(name, arguments);
        }

        /// <inheritdoc />
        public static SqlSelectLambdaExpression Select(SqlExpression[] columns, SqlExpression from, SqlExpression? condition = null, SqlExpression[]? groups = null, SqlExpression[]? orders = null, SqlExpression? having = null, int? top = null)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));
            if (from == null)
                throw new ArgumentNullException(nameof(from));

            return new SqlSelectLambdaExpression(top, columns, from, condition, groups, orders, having);
        }

        /// <inheritdoc />
        public static SqlInsertLambdaExpression Insert(SqlExpression[] columns, SqlExpression from, params SqlConstantExpression[][] values)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));
            if (from == null)
                throw new ArgumentNullException(nameof(from));
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            return new SqlInsertLambdaExpression(columns, from, values);
        }

        /// <inheritdoc />
        public static SqlUpdateLambdaExpression Update(SqlBinaryExpression[] columns, SqlExpression from, SqlExpression? condition = null)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));
            if (from == null)
                throw new ArgumentNullException(nameof(from));

            return new SqlUpdateLambdaExpression(columns, from, condition);
        }

        /// <inheritdoc />
        public static SqlDeleteLamdbaExpression Delete(SqlExpression from, SqlExpression? condition = null)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));

            return new SqlDeleteLamdbaExpression(from, condition);
        }

        /// <inheritdoc />
        protected internal abstract void Accept(SqlExpressionVisitor visitor);
    }
}