using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Framework.Data
{
    public static class CacheManager
    {
        private static readonly IDataFactory _global = new DataFactory();
        private static readonly Func<Type, IDataGroup> _valueFactory = key =>
        {
            var dataGroupType = typeof(DataGroup<>).MakeGenericType(key);
            return (IDataGroup)Activator.CreateInstance(dataGroupType);
        };

        public static IDataFactory Global => _global;

        public static IDataGroup Gain(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return _global.GetOrAdd(type, _valueFactory);
        }

        public static IDataGroup<T> Gain<T>()
        {
            return (IDataGroup<T>)_global.GetOrAdd(typeof(T), _valueFactory);
        }
    }

    public interface IDataGroup : IEnumerable
    {
        object this[long key] { get; set; }
        ICollection<long> Keys { get; }
        ICollection<object> Values { get; }
        int Count { get; }

        bool ContainsKey(long key);
        bool TryAdd(long key, object value);
        bool TryGetValue(long key, out object value);
        bool TryRemove(long key, out object value);
        bool TryUpdate(long key, object newValue, object comparisonValue);
        object GetOrAdd(long key, object newValue);
        object GetOrAdd(long key, Func<long, object> valueFactory);
        void Clear();
    }

    public interface IDataGroup<T> : IEnumerable<KeyValuePair<long, T>>
    {
        T this[long key] { get; set; }
        ICollection<long> Keys { get; }
        ICollection<T> Values { get; }
        int Count { get; }

        bool ContainsKey(long key);
        bool TryAdd(long key, T value);
        bool TryGetValue(long key, out T value);
        bool TryRemove(long key, out T value);
        bool TryUpdate(long key, T newValue, T comparisonValue);
        T GetOrAdd(long key, T newValue);
        T GetOrAdd(long key, Func<long, T> valueFactory);
        void Clear();
    }
    public interface IDataFactory : IEnumerable<KeyValuePair<Type, IDataGroup>>
    {
        IDataGroup this[Type key] { get; set; }
        ICollection<Type> Keys { get; }
        ICollection<IDataGroup> Values { get; }
        int Count { get; }

        bool ContainsKey(Type key);
        bool TryAdd(Type key, IDataGroup value);
        bool TryGetValue(Type key, out IDataGroup value);
        bool TryRemove(Type key, out IDataGroup value);
        bool TryUpdate(Type key, IDataGroup newValue, IDataGroup comparisonValue);
        IDataGroup GetOrAdd(Type key, Func<Type, IDataGroup> valueFactory);
        void Clear();
    }
    class DataGroup<T> : IDataGroup<T>, IDataGroup
    {
        private readonly ConcurrentDictionary<long, T> dictionary = new ConcurrentDictionary<long, T>();

        public T this[long key]
        {
            get { return dictionary[key]; }
            set { dictionary[key] = value; }
        }
        public ICollection<long> Keys => dictionary.Keys;
        public ICollection<T> Values => dictionary.Values;
        public int Count => dictionary.Count;

        object IDataGroup.this[long key]
        {
            get { return this[key]; }
            set { this[key] = (T)value; }
        }
        ICollection<object> IDataGroup.Values => new Collection(dictionary.Values);

        public bool ContainsKey(long key)
        {
            return dictionary.ContainsKey(key);
        }
        public bool TryAdd(long key, T value)
        {
            return dictionary.TryAdd(key, value);
        }
        public T GetOrAdd(long key, T newValue)
        {
            return dictionary.GetOrAdd(key, newValue);
        }
        public T GetOrAdd(long key, Func<long, T> valueFactory)
        {
            return dictionary.GetOrAdd(key, valueFactory);
        }
        public bool TryGetValue(long key, out T value)
        {
            return dictionary.TryGetValue(key, out value);
        }
        public bool TryRemove(long key, out T value)
        {
            return dictionary.TryRemove(key, out value);
        }
        public bool TryUpdate(long key, T newValue, T comparisonValue)
        {
            return dictionary.TryUpdate(key, newValue, comparisonValue);
        }
        public IEnumerator<KeyValuePair<long, T>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }
        public void Clear()
        {
            dictionary.Clear();
        }

        bool IDataGroup.TryAdd(long key, object value)
        {
            return dictionary.TryAdd(key, (T)value);
        }
        object IDataGroup.GetOrAdd(long key, object newValue)
        {
            return dictionary.GetOrAdd(key, (T)newValue);
        }
        object IDataGroup.GetOrAdd(long key, Func<long, object> valueFactory)
        {
            return dictionary.GetOrAdd(key, new Func<long, T>((ke) => (T)valueFactory(ke)));
        }
        bool IDataGroup.TryGetValue(long key, out object value)
        {
            var result = dictionary.TryGetValue(key, out T tvalue);
            value = tvalue;
            return result;
        }
        bool IDataGroup.TryRemove(long key, out object value)
        {
            var result = dictionary.TryRemove(key, out T tvalue);
            value = tvalue;
            return result;
        }
        bool IDataGroup.TryUpdate(long key, object newValue, object comparisonValue)
        {
            return dictionary.TryUpdate(key, (T)newValue, (T)comparisonValue);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        class Collection : ICollection<object>
        {
            private readonly List<object> _items;

            public Collection(ICollection<T> items)
            {
                _items = new List<object>();

                foreach (var item in items)
                    _items.Add(item);
            }

            public int Count => _items.Count;
            bool ICollection<object>.IsReadOnly => true;

            void ICollection<object>.Add(object item)
            {
                throw new NotSupportedException();
            }
            void ICollection<object>.Clear()
            {
                throw new NotSupportedException();
            }
            public bool Contains(object item)
            {
                return _items.Contains(item);
            }
            public void CopyTo(object[] array, int arrayIndex)
            {
                _items.CopyTo(array, arrayIndex);
            }
            bool ICollection<object>.Remove(object item)
            {
                throw new NotSupportedException();
            }
            public IEnumerator<object> GetEnumerator()
            {
                return _items.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return _items.GetEnumerator();
            }
        }
    }
    class DataFactory : IDataFactory
    {
        private readonly ConcurrentDictionary<Type, IDataGroup> dictionary = new ConcurrentDictionary<Type, IDataGroup>();

        public IDataGroup this[Type key]
        {
            get { return dictionary[key]; }
            set { dictionary[key] = value; }
        }
        public ICollection<Type> Keys
        {
            get { return dictionary.Keys; }
        }
        public ICollection<IDataGroup> Values
        {
            get { return dictionary.Values; }
        }
        public int Count
        {
            get { return dictionary.Count; }
        }

        public bool ContainsKey(Type key)
        {
            return dictionary.ContainsKey(key);
        }
        public bool TryAdd(Type key, IDataGroup value)
        {
            return dictionary.TryAdd(key, value);
        }
        public bool TryGetValue(Type key, out IDataGroup value)
        {
            return dictionary.TryGetValue(key, out value);
        }
        public bool TryRemove(Type key, out IDataGroup value)
        {
            return dictionary.TryRemove(key, out value);
        }
        public bool TryUpdate(Type key, IDataGroup newValue, IDataGroup comparisonValue)
        {
            return dictionary.TryUpdate(key, newValue, comparisonValue);
        }
        public IDataGroup GetOrAdd(Type key, Func<Type, IDataGroup> valueFactory)
        {
            return dictionary.GetOrAdd(key, valueFactory);
        }
        public void Clear()
        {
            dictionary.Clear();
        }
        public IEnumerator<KeyValuePair<Type, IDataGroup>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }
    }
}