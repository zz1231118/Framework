using System;
using System.Collections.Generic;
using System.Data;
using Framework.Data.Expressions;

namespace Framework.Data.Command
{
    internal abstract class DbCommandStruct : IDbCommandStruct
    {
        private readonly string _name;
        private readonly List<SqlExpression> _columns = new List<SqlExpression>();
        private readonly Dictionary<string, IDataParameter> _parameters = new Dictionary<string, IDataParameter>();
        private readonly DbCommandMode _mode;

        public DbCommandStruct(string name, DbCommandMode mode, IEnumerable<SqlExpression>? columns = null)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            _name = name;
            _mode = mode;
            if (columns != null)
            {
                _columns.AddRange(columns);
            }
        }

        public string Name => _name;

        public int? Top { get; set; }

        public DbCommandMode Mode => _mode;

        public IList<SqlExpression> Columns => _columns;

        public ICollection<IDataParameter> Parameters => _parameters.Values;

        public SqlExpression? Condition { get; set; }

        public IEnumerable<IDbSortClause>? SortOrders { get; set; }

        public IEnumerable<SqlMemberExpression>? Groups { get; set; }

        public IDbRowOffset? RowOffset { get; set; }

        public string CommandText
        {
            get
            {
                switch (_mode)
                {
                    case DbCommandMode.Select:
                        return OnParseSelect();
                    case DbCommandMode.Update:
                        return OnParseUpdate();
                    case DbCommandMode.Insert:
                        return OnParseInsert();
                    case DbCommandMode.Delete:
                        return OnParseDelete();
                    default:
                        throw new InvalidOperationException(string.Format("Unknown {0}:{1}", nameof(DbCommandMode), _mode.ToString()));
                }
            }
        }
        public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(30);

        protected abstract string OnParseSelect();
        protected abstract string OnParseUpdate();
        protected abstract string OnParseInsert();
        protected abstract string OnParseDelete();

        public abstract void AddParameter(string name, object value);
        public void AddParameter(IDataParameter parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            var name = parameter.ParameterName.Substring(1);
            _parameters[name] = parameter;
        }
        public void ClearParameter()
        {
            _parameters.Clear();
        }
        public void SetRowOffset(int offset, int count)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            RowOffset = new RowOffsetStruct() { Offset = offset, Count = count };
        }

        class RowOffsetStruct : IDbRowOffset
        {
            public int Offset { get; set; }

            public int Count { get; set; }
        }
    }
}