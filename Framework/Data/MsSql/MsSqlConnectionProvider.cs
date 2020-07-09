using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Framework.Data.Command;
using Framework.Data.Expressions;

namespace Framework.Data.MsSql
{
    public sealed class MsSqlConnectionProvider : DbConnectionProvider
    {
        private readonly string _connectionString;

        public MsSqlConnectionProvider(int capacity, string connectionString, AccessLevel accessLevel = AccessLevel.ReadWrite)
            : base(capacity, accessLevel)
        {
            if (connectionString == null)
                throw new ArgumentNullException(nameof(connectionString));

            _connectionString = connectionString;
        }

        public sealed override DbConnectionCategory ConnectionCategory => DbConnectionCategory.MsSql;

        public sealed override string ConnectionString => _connectionString;

        protected override IDbConnection CreateConnection(bool isTemporary)
        {
            return new MsSqlConnection(_connectionString, isTemporary);
        }

        public override string NormalizeSymbol(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return MsSqlHelper.FormatSymbolName(name);
        }

        public override string NormalizeParameter(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return MsSqlHelper.FormatParamName(name);
        }

        public override IDataParameter CreateParameter(string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            name = MsSqlHelper.FormatParamName(name);
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        public override IDbCommandStruct CreateCommand(string name, DbCommandMode mode, IEnumerable<SqlExpression> columns = null)
        {
            return new MsSqlCommandStruct(name, mode, columns);
        }

        public override IDbCommandStruct<T> CreateCommand<T>(string name, DbCommandMode mode, IEnumerable<SqlExpression> columns = null)
        {
            return new MsSqlDbCommandStruct<T>(name, mode, columns);
        }
    }
}