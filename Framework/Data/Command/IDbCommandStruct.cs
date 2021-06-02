using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using Framework.Data.Expressions;

namespace Framework.Data.Command
{
    /// <inheritdoc />
    public interface IDbCommandStruct
    {
        /// <inheritdoc />
        string Name { get; }

        /// <inheritdoc />
        int? Top { get; set; }

        /// <inheritdoc />
        DbCommandMode Mode { get; }

        /// <inheritdoc />
        IList<SqlExpression> Columns { get; }

        /// <inheritdoc />
        ICollection<IDataParameter> Parameters { get; }

        /// <inheritdoc />
        SqlExpression? Condition { get; set; }

        /// <inheritdoc />
        IEnumerable<IDbSortClause>? SortOrders { get; set; }

        /// <inheritdoc />
        IEnumerable<SqlMemberExpression>? Groups { get; set; }

        /// <inheritdoc />
        IDbRowOffset? RowOffset { get; set; }

        /// <inheritdoc />
        string CommandText { get; }

        /// <inheritdoc />
        TimeSpan CommandTimeout { get; set; }

        /// <inheritdoc />
        void AddParameter(IDataParameter parameter);

        /// <inheritdoc />
        void AddParameter(string name, object value);

        /// <inheritdoc />
        void ClearParameter();

        /// <inheritdoc />
        void SetRowOffset(int offset, int count);
    }

    /// <inheritdoc />
    public interface IDbCommandStruct<T> : IDbCommandStruct
    {
        /// <inheritdoc />
        void Where(Expression<Func<T, bool>> expression);

        /// <inheritdoc />
        void OrderBy(Expression<Func<T, object>> expression, bool ascending = true);

        /// <inheritdoc />
        void GroupBy(Expression<Func<T, object>> expression);
    }

    /// <inheritdoc />
    public interface IDbSortClause
    {
        /// <inheritdoc />
        SqlMemberExpression Member { get; }

        /// <inheritdoc />
        bool Ascending { get; }
    }
}
