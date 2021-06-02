using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Framework.Data.Converters;

namespace Framework.Data
{
    internal static class EntityConverterManager
    {
        private static readonly Dictionary<Type, DbType> typeToDbTypeMappings = new Dictionary<Type, DbType>()
        {
            [typeof(bool)] = DbType.Boolean,
            [typeof(byte)] = DbType.Byte,
            [typeof(sbyte)] = DbType.SByte,
            [typeof(short)] = DbType.Int16,
            [typeof(ushort)] = DbType.UInt16,
            [typeof(int)] = DbType.Int32,
            [typeof(uint)] = DbType.UInt32,
            [typeof(long)] = DbType.Int64,
            [typeof(ulong)] = DbType.UInt64,
            [typeof(float)] = DbType.Single,
            [typeof(double)] = DbType.Double,
            [typeof(decimal)] = DbType.Decimal,
            [typeof(string)] = DbType.String,
            [typeof(byte[])] = DbType.Binary,
            [typeof(DateTime)] = DbType.DateTime,
            [typeof(Guid)] = DbType.Guid,
        };
        private static readonly ConcurrentDictionary<Type, Type> defaultEntityConverterType = new ConcurrentDictionary<Type, Type>()
        {
            [typeof(Enum)] = typeof(EnumConverter),
            [typeof(TimeSpan)] = typeof(TimeSpanConverter),
        };
        private static readonly ConcurrentDictionary<Type, IEntityConverter> globalEntityConverter = new ConcurrentDictionary<Type, IEntityConverter>();
        private static readonly Func<Type, IEntityConverter> converterFactory = new Func<Type, IEntityConverter>(key => (IEntityConverter)Activator.CreateInstance(key, true));

        /// <summary>
        /// Try get default entity converter type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="entityConverterType"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException" />
        public static bool TryGetDefaultEntityConverterType(Type type, out Type entityConverterType)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return defaultEntityConverterType.TryGetValue(type, out entityConverterType);
        }

        /// <summary>
        /// Find default entity converter type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="throwOnError"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException" />
        /// <exception cref="Framework.Data.ConverterTypeNotFoundException" />
        public static Type GetDefaultEntityConverterType(Type type, bool throwOnError = false)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (!defaultEntityConverterType.TryGetValue(type, out Type entityConverterType) && throwOnError)
                throw new ConverterTypeNotFoundException(type.FullName);

            return entityConverterType;
        }

        public static IEntityConverter Gain(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (!typeof(IEntityConverter).IsAssignableFrom(type))
                throw new ArgumentException("type not is assignable IEntityConverter!");

            return globalEntityConverter.GetOrAdd(type, converterFactory);
        }

        public static DbType GetDefaultDbType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!typeToDbTypeMappings.TryGetValue(type, out DbType dbType))
            {
                throw new Exception(string.Format("[{0}] can't change to DbType", type.FullName));
            }

            return dbType;
        }
    }

    public class ConverterTypeNotFoundException : Exception
    {
        public ConverterTypeNotFoundException(string message)
            : base(message)
        { }
    }
}