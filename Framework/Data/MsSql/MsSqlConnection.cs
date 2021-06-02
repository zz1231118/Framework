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
        private readonly SqlConnection dbConnection;

        public MsSqlConnection(string sqlConnectionString, bool isTemporary)
        {
            if (sqlConnectionString == null)
                throw new ArgumentNullException(nameof(sqlConnectionString));
            if (sqlConnectionString == string.Empty)
                throw new ArgumentException(nameof(sqlConnectionString));

            dbConnection = new SqlConnection(sqlConnectionString);
            IsTemporary = isTemporary;
        }

        public DbConnectionCategory Category => DbConnectionCategory.MsSql;

        public ConnectionState State => dbConnection.State;

        public DbConnection DbConnection => dbConnection;

        public bool IsTemporary { get; }

        public void Open()
        {
            dbConnection.Open();
        }

        public void Close()
        {
            dbConnection.Close();
        }

        public void EnsureConnection()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
        }

        public int ExecuteNonQuery(string commandText, CommandType commandType = CommandType.Text, TimeSpan? commandTimeout = null, IEnumerable<IDataParameter>? parameters = null)
        {
            using (var dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = commandText;
                dbCommand.CommandType = commandType;
                if (commandTimeout != null)
                {
                    dbCommand.CommandTimeout = checked((int)commandTimeout.Value.TotalSeconds);
                }
                if (parameters != null)
                {
                    dbCommand.Parameters.AddRange(parameters.Cast<SqlParameter>().ToArray());
                }
                return dbCommand.ExecuteNonQuery();
            }
        }

        public IDataReader ExecuteReader(string commandText, CommandType commandType = CommandType.Text, TimeSpan? commandTimeout = null, IEnumerable<IDataParameter>? parameters = null)
        {
            using (var dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = commandText;
                dbCommand.CommandType = commandType;
                if (commandTimeout != null)
                {
                    dbCommand.CommandTimeout = checked((int)commandTimeout.Value.TotalSeconds);
                }
                if (parameters != null)
                {
                    dbCommand.Parameters.AddRange(parameters.Cast<SqlParameter>().ToArray());
                }
                return dbCommand.ExecuteReader();
            }
        }

        public object ExecuteScalar(string commandText, CommandType commandType = CommandType.Text, TimeSpan? commandTimeout = null, IEnumerable<IDataParameter>? parameters = null)
        {
            using (var dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = commandText;
                dbCommand.CommandType = commandType;
                if (commandTimeout != null)
                {
                    dbCommand.CommandTimeout = checked((int)commandTimeout.Value.TotalSeconds);
                }
                if (parameters != null)
                {
                    dbCommand.Parameters.AddRange(parameters.Cast<SqlParameter>().ToArray());
                }
                return dbCommand.ExecuteScalar();
            }
        }

        public T? ExecuteScalar<T>(string commandText, CommandType commandType = CommandType.Text, TimeSpan? commandTimeout = null, IEnumerable<IDataParameter>? parameters = null)
        {
            using (var dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = commandText;
                dbCommand.CommandType = commandType;
                if (commandTimeout != null)
                {
                    dbCommand.CommandTimeout = checked((int)commandTimeout.Value.TotalSeconds);
                }
                if (parameters != null)
                {
                    dbCommand.Parameters.AddRange(parameters.Cast<SqlParameter>().ToArray());
                }
                var value = dbCommand.ExecuteScalar();
                if (value == DBNull.Value)
                {
                    if (typeof(T).IsValueType)
                    {
                        throw new InvalidCastException();
                    }

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
                    dbConnection.Dispose();
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }
    }
}
