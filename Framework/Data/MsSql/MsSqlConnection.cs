using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace Framework.Data.MsSql
{
    internal class MsSqlConnection : BaseDisposed, IDbConnection
    {
        private SqlConnection sqlConnection;

        public MsSqlConnection(string sqlConnectionString, bool isTemporary)
        {
            if (sqlConnectionString == null)
                throw new ArgumentNullException(nameof(sqlConnectionString));
            if (sqlConnectionString == string.Empty)
                throw new ArgumentException(nameof(sqlConnectionString));

            sqlConnection = new SqlConnection(sqlConnectionString);
            IsTemporary = isTemporary;
        }

        public DbConnectionCategory Category => DbConnectionCategory.MsSql;

        public ConnectionState State => sqlConnection.State;

        public DbConnection DbConnection => sqlConnection;

        public bool IsTemporary { get; }

        public void Open()
        {
            sqlConnection.Open();
        }

        public void Close()
        {
            sqlConnection.Close();
        }

        public void CheckConnect()
        {
            if (sqlConnection.State != ConnectionState.Open)
                sqlConnection.Open();
        }

        public int ExecuteNonQuery(string commandText, CommandType commandType = CommandType.Text, IEnumerable<IDataParameter> parameters = null)
        {
            using (var sqlCommand = sqlConnection.CreateCommand())
            {
                sqlCommand.CommandText = commandText;
                sqlCommand.CommandType = commandType;
                if (parameters != null)
                {
                    sqlCommand.Parameters.AddRange(parameters.Cast<SqlParameter>().ToArray());
                }
                return sqlCommand.ExecuteNonQuery();
            }
        }

        public IDataReader ExecuteReader(string commandText, CommandType commandType = CommandType.Text, IEnumerable<IDataParameter> parameters = null)
        {
            using (var sqlCommand = sqlConnection.CreateCommand())
            {
                sqlCommand.CommandText = commandText;
                sqlCommand.CommandType = commandType;
                if (parameters != null)
                {
                    sqlCommand.Parameters.AddRange(parameters.Cast<SqlParameter>().ToArray());
                }
                return sqlCommand.ExecuteReader();
            }
        }

        public object ExecuteScalar(string commandText, CommandType commandType = CommandType.Text, IEnumerable<IDataParameter> parameters = null)
        {
            using (var sqlCommand = sqlConnection.CreateCommand())
            {
                sqlCommand.CommandText = commandText;
                sqlCommand.CommandType = commandType;
                if (parameters != null)
                {
                    sqlCommand.Parameters.AddRange(parameters.Cast<SqlParameter>().ToArray());
                }
                return sqlCommand.ExecuteScalar();
            }
        }

        public T ExecuteScalar<T>(string commandText, CommandType commandType = CommandType.Text, IEnumerable<IDataParameter> parameters = null)
        {
            using (var sqlCommand = sqlConnection.CreateCommand())
            {
                sqlCommand.CommandText = commandText;
                sqlCommand.CommandType = commandType;
                if (parameters != null)
                {
                    sqlCommand.Parameters.AddRange(parameters.Cast<SqlParameter>().ToArray());
                }
                var value = sqlCommand.ExecuteScalar();
                if (value == DBNull.Value)
                {
                    if (typeof(T).IsValueType)
                        throw new InvalidCastException();

                    return default(T);
                }
                return (T)Convert.ChangeType(value, typeof(T));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                try
                {
                    sqlConnection.Dispose();

                    sqlConnection = null;
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }
    }
}
