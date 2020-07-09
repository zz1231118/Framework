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
        string Name { get; }
        int? Top { get; set; }
        DbCommandMode Mode { get; }
        IList<SqlExpression> Columns { get; }
        ICollection<IDataParameter> Parameters { get; }
        SqlExpression Condition { get; set; }
        IEnumerable<IDbSortClause> SortOrders { get; set; }
        IEnumerable<SqlMemberExpression> Groups { get; set; }
        IDbRowOffset RowOffset { get; set; }
        string CommandText { get; }

        void AddParameter(IDataParameter parameter);
        void AddParameter(string name, object value);
        void ClearParameter();
        void SetRowOffset(int offset, int count);
    }

    /// <inheritdoc />
    public interface IDbCommandStruct<T> : IDbCommandStruct
    {
        void Where(Expression<Func<T, bool>> expression);
        void OrderBy(Expression<Func<T, object>> expression, bool ascending = true);
        void GroupBy(Expression<Func<T, object>> expression);
    }

    /// <inheritdoc />
    public interface IDbSortClause
    {
        SqlMemberExpression Member { get; }

        bool Ascending { get; }
    }
}
