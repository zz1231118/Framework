using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework.JavaScript
{
    /// <summary>
    /// JsonObject
    /// </summary>
    [Serializable]
    public sealed class JsonObject : Json, IDictionary<string, Json>
    {
        private readonly Dictionary<string, Json> dictionary;

        /// <summary>
        /// JsonObject 构造函数
        /// </summary>
        public JsonObject()
        {
            dictionary = new Dictionary<string, Json>();
        }

        /// <summary>
        /// JsonObject 构造函数
        /// </summary>
        /// <param name="capacity">可包含的初始元素数</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public JsonObject(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            dictionary = new Dictionary<string, Json>(capacity);
        }

        /// <summary>
        /// JsonObject 构造函数
        /// </summary>
        /// <param name="dictionary">初始元素</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public JsonObject(IDictionary<string, Json> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            this.dictionary = new Dictionary<string, Json>(dictionary);
        }

        /// <summary>
        /// JsonObject 构造函数
        /// </summary>
        /// <param name="collection"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public JsonObject(params KeyValuePair<string, Json>[] collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            dictionary = new Dictionary<string, Json>();
            foreach (var entry in collection)
            {
                dictionary.Add(entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// JsonObject 构造函数
        /// </summary>
        /// <param name="collection"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public JsonObject(IEnumerable<KeyValuePair<string, Json>> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            dictionary = new Dictionary<string, Json>();
            foreach (var entry in collection)
            {
                dictionary.Add(entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// JsonObject this
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public Json this[string key]
        {
            get => dictionary[key];
            set
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                dictionary[key] = value;
            }
        }

        /// <summary>
        /// 键集合
        /// </summary>
        public ICollection<string> Keys => dictionary.Keys;

        /// <summary>
        /// 值的集合
        /// </summary>
        public ICollection<Json> Values => dictionary.Values;

        /// <summary>
        /// 包含的键值对数目
        /// </summary>
        public int Count => dictionary.Count;

        /// <summary>
        /// 是否为只读
        /// </summary>
        bool ICollection<KeyValuePair<string, Json>>.IsReadOnly => false;

        /// <summary>
        /// 把指定集合添加到集合
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddRange(IEnumerable<KeyValuePair<string, Json>> value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            foreach (KeyValuePair<string, Json> kv in value)
            {
                dictionary.Add(kv.Key, kv.Value);
            }
        }

        /// <summary>
        /// 将指定的键值添加到字典
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public void Add(string key, Json value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            dictionary.Add(key, value);
        }

        /// <summary>
        /// 确定是否包含指定键
        /// </summary>
        public bool ContainsKey(string key)
        {
            return dictionary.ContainsKey(key);
        }

        /// <summary>
        /// 移除指定键的值
        /// </summary>
        public bool Remove(string key)
        {
            return dictionary.Remove(key);
        }

        /// <summary>
        /// 获取与指定键相关联的值
        /// </summary>
        public bool TryGetValue(string key, out Json value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// 移除所有的键和值
        /// </summary>
        public void Clear()
        {
            dictionary.Clear();
        }

        /// <summary>
        /// 把指定范围内的数据 Copy 到 array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void CopyTo(KeyValuePair<string, Json>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            var count = dictionary.Count;
            if (arrayIndex >= count)
            {
                throw new IndexOutOfRangeException();
            }

            Json value;
            string key;
            array = new KeyValuePair<string, Json>[count - arrayIndex];
            for (int i = arrayIndex; i < count; i++)
            {
                key = dictionary.ElementAt(i).Key;
                value = dictionary.ElementAt(i).Value;
                array[i - arrayIndex] = new KeyValuePair<string, Json>(key, value);
            }
        }

        /// <summary>
        /// 返回循环访问的枚举数
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, Json>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        /// <summary>
        /// 深度克隆
        /// </summary>
        public override object Clone()
        {
            return new JsonObject(dictionary);
        }

        /// <summary>
        /// 返回表示当前对象的 字符串
        /// </summary>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append('{');
            foreach (var key in dictionary.Keys)
            {
                //原本这边是用(new MyJsonValue(Key)).ToString() 更方便一点，但是，
                //考虑 类和类之间关联的会太紧密，导致以后不好修改
                builder.Append("\"");
                builder.Append(JsonUtility.EscapeString(key));
                builder.Append("\"");
                builder.Append(":");
                builder.Append(dictionary[key].ToString());
                builder.Append(",");
            }
            if (builder.ToString().EndsWith(","))
                builder.Remove(builder.Length - 1, 1);

            builder.Append('}');
            return builder.ToString();
        }

        void ICollection<KeyValuePair<string, Json>>.Add(KeyValuePair<string, Json> value)
        {
            if (value.Key == null || value.Value == null)
                throw new ArgumentException(nameof(value));

            dictionary.Add(value.Key, value.Value);
        }

        bool ICollection<KeyValuePair<string, Json>>.Contains(KeyValuePair<string, Json> item)
        {
            return dictionary.Contains(item);
        }

        bool ICollection<KeyValuePair<string, Json>>.Remove(KeyValuePair<string, Json> item)
        {
            return dictionary.Remove(item.Key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }
    }
}