using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using Framework.Data.Command;
using Framework.Data.Expressions;

namespace Framework.Data
{
    public abstract class DbConnectionProvider : BaseDisposed
    {
        private readonly int _capacity;
        private readonly AccessLevel _accessLevel;
        private readonly ConcurrentQueue<IDbConnection> _connectionPool = new ConcurrentQueue<IDbConnection>();
        private int _size;

        public DbConnectionProvider(int capacity, AccessLevel accessLevel)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _capacity = capacity;
            _accessLevel = accessLevel;
        }

        public abstract DbConnectionCategory ConnectionCategory { get; }

        public AccessLevel AccessLevel => _accessLevel;

        public abstract string ConnectionString { get; }

        public int Capacity => _capacity;

        public int Available => _connectionPool.Count;

        protected abstract IDbConnection CreateConnection(bool isTemporary);

        public abstract string NormalizeSymbol(string name);

        public abstract string NormalizeParameter(string name);

        public abstract IDataParameter CreateParameter(string name, object value);

        public abstract IDbCommandStruct CreateCommand(string name, DbCommandMode mode, IEnumerable<SqlExpression> columns = null);

        public abstract IDbCommandStruct<T> CreateCommand<T>(string name, DbCommandMode mode, IEnumerable<SqlExpression> columns = null);

        public IDbConnection Allocate()
        {
            if (_connectionPool.TryDequeue(out IDbConnection connection))
            {
                return connection;
            }
            if (_size < _capacity)
            {
                if (Interlocked.Increment(ref _size) <= _capacity)
                {
                    return CreateConnection(false);
                }
                else
                {
                    Interlocked.Decrement(ref _size);
                }
            }

            return CreateConnection(true);
        }

        public void Release(IDbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (connection.IsTemporary)
            {
                connection.Dispose();
                return;
            }

            connection.Close();
            _connectionPool.Enqueue(connection);
        }

        public void ExecuteReader(string commandText, CommandType commandType, IEnumerable<IDataParameter> parameters, Action<IDataReader> readProcessor)
        {
            if (commandText == null)
                throw new ArgumentNullException(nameof(commandText));
            if (readProcessor == null)
                throw new ArgumentNullException(nameof(readProcessor));

            var dbConnection = Allocate();

            try
            {
                dbConnection.CheckConnect();
                using (var reader = dbConnection.ExecuteReader(commandText, commandType, parameters))
                {
                    readProcessor(reader);
                }
            }
            finally
            {
                Release(dbConnection);
            }
        }

        public object ExecuteScalar(string commandText, CommandType commandType = CommandType.Text, IEnumerable<IDataParameter> parameters = null)
        {
            if (commandText == null)
                throw new ArgumentNullException(nameof(commandText));

            var dbConnection = Allocate();

            try
            {
                dbConnection.CheckConnect();
                return dbConnection.ExecuteScalar(commandText, commandType, parameters);
            }
            finally
            {
                Release(dbConnection);
            }
        }

        public T ExecuteScalar<T>(string commandText, CommandType commandType = CommandType.Text, IEnumerable<IDataParameter> parameters = null)
        {
            if (commandText == null)
                throw new ArgumentNullException(nameof(commandText));

            var dbConnection = Allocate();

            try
            {
                dbConnection.CheckConnect();
                return dbConnection.ExecuteScalar<T>(commandText, commandType, parameters);
            }
            finally
            {
                Release(dbConnection);
            }
        }

        public int ExecuteNonQuery(string commandText, CommandType commandType = CommandType.Text, IEnumerable<IDataParameter> parameters = null)
        {
            if (commandText == null)
                throw new ArgumentNullException(nameof(commandText));

            var dbConnection = Allocate();

            try
            {
                dbConnection.CheckConnect();
                return dbConnection.ExecuteNonQuery(commandText, commandType, parameters);
            }
            finally
            {
                Release(dbConnection);
            }
        }

        public void ExecuteReader(IDbCommandStruct commandStruct, Action<IDataReader> readProcessor)
        {
            if (commandStruct == null)
                throw new ArgumentNullException(nameof(commandStruct));
            if (readProcessor == null)
                throw new ArgumentNullException(nameof(readProcessor));

            var dbConnection = Allocate();

            try
            {
                dbConnection.CheckConnect();
                using (var reader = dbConnection.ExecuteReader(commandStruct.CommandText, parameters: commandStruct.Parameters))
                {
                    readProcessor(reader);
                }
            }
            finally
            {
                Release(dbConnection);
            }
        }

        public object ExecuteScalar(IDbCommandStruct commandStruct)
        {
            if (commandStruct == null)
                throw new ArgumentNullException(nameof(commandStruct));

            var dbConnection = Allocate();

            try
            {
                dbConnection.CheckConnect();
                return dbConnection.ExecuteScalar(commandStruct.CommandText, parameters: commandStruct.Parameters);
            }
            finally
            {
                Release(dbConnection);
            }
        }

        public T ExecuteScalar<T>(IDbCommandStruct commandStruct)
        {
            if (commandStruct == null)
                throw new ArgumentNullException(nameof(commandStruct));

            var dbConnection = Allocate();

            try
            {
                dbConnection.CheckConnect();
                return dbConnection.ExecuteScalar<T>(commandStruct.CommandText, parameters: commandStruct.Parameters);
            }
            finally
            {
                Release(dbConnection);
            }
        }

        public int ExecuteNonQuery(IDbCommandStruct commandStruct)
        {
            if (commandStruct == null)
                throw new ArgumentNullException(nameof(commandStruct));

            var dbConnection = Allocate();

            try
            {
                dbConnection.CheckConnect();
                return dbConnection.ExecuteNonQuery(commandStruct.CommandText, parameters: commandStruct.Parameters);
            }
            finally
            {
                Release(dbConnection);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                try
                {
                    while (_connectionPool.TryDequeue(out IDbConnection connection))
                        connection.Dispose();

                    _size = 0;
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }
    }
}