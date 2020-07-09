using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Framework.Data
{
    public static class DbConnectionManager
    {
        private static bool isDisposed;
        private static ConcurrentDictionary<string, DbConnectionProvider> connectionProviders = new ConcurrentDictionary<string, DbConnectionProvider>();

        public static ICollection<string> Names => connectionProviders.Keys;

        public static ICollection<DbConnectionProvider> ConnectionProviders => connectionProviders.Values;

        public static void Register(string name, DbConnectionProvider connectionProvider)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));

            connectionProviders.TryAdd(name, connectionProvider);
        }

        public static bool Unregister(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return connectionProviders.TryRemove(name, out _);
        }

        public static bool Contains(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return connectionProviders.ContainsKey(name);
        }

        public static DbConnectionProvider Gain(string name, bool throwOnError = false)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (!connectionProviders.TryGetValue(name, out DbConnectionProvider value) && throwOnError)
            {
                throw new KeyNotFoundException("name:" + name);
            }
            return value;
        }

        public static bool TryGet(string name, out DbConnectionProvider connectionProvider)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return connectionProviders.TryGetValue(name, out connectionProvider);
        }

        public static void Dispose()
        {
            if (!isDisposed)
            {
                try
                {
                    foreach (var dbProvider in connectionProviders.Values)
                    {
                        dbProvider.Dispose();
                    }

                    connectionProviders.Clear();
                    connectionProviders = null;
                }
                finally
                {
                    isDisposed = true;
                }
            }
        }
    }
}