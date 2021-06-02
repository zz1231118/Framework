using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.JavaScript
{
    /// <summary>
    /// JsonArray
    /// </summary>
    [Serializable]
    public sealed class JsonArray : Json, IList<Json>
    {
        private readonly List<Json> items;

        /// <summary>
        /// JsonArray 构造函数
        /// </summary>
        public JsonArray()
        {
            items = new List<Json>();
        }

        /// <summary>
        /// JsonArray 构造函数
        /// </summary>
        /// <param name="capacity"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public JsonArray(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            items = new List<Json>(capacity);
        }

        /// <summary>
        /// JsonArray 构造函数
        /// </summary>
        /// <param name="collection"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public JsonArray(IEnumerable<Json> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            items = new List<Json>(collection);
        }

        /// <summary>
        /// MyJsonList this
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Json this[int index]
        {
            get
            {
                if (index < 0 || index >= items.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return items[index];
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                if (index < 0 || index >= items.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                items[index] = value;
            }
        }

        /// <summary>
        /// 实际包含的元素数
        /// </summary>
        public int Count => items.Count;

        /// <summary>
        /// 是否为只读
        /// </summary>
        bool ICollection<Json>.IsReadOnly => false;

        /// <summary>
        /// 把指定集合添加到列表
        /// </summary>
        /// <param name="collection"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddRange(IEnumerable<Json> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            items.AddRange(collection);
        }

        /// <summary>
        /// 对 JsonArray 的每个元素执行指定操作
        /// </summary>
        /// <param name="action"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void ForEach(Action<Json> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (var item in items)
            {
                action(item);
            }
        }

        /// <summary>
        /// 搜索指定对象，返回第一个从零开始的匹配的索引
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(Json item)
        {
            return items.IndexOf(item);
        }

        /// <summary>
        /// 把元素插入到指定索引处
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Insert(int index, Json value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            items.Insert(index, value);
        }

        /// <summary>
        /// 移除指定索引处的元素
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        /// <summary>
        /// 把对象添加到列表的结尾
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Add(Json value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            items.Add(value);
        }

        /// <summary>
        /// 移除所有元素
        /// </summary>
        public void Clear()
        {
            items.Clear();
        }

        /// <summary>
        /// 确定指定项是否存在
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(Json value)
        {
            return items.Contains(value);
        }

        /// <summary>
        /// 复制从指定索引开始的元素到数组
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void CopyTo(Json[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex >= array.Length)
                throw new IndexOutOfRangeException();

            items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// 移除指定项
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Remove(Json value)
        {
            return items.Remove(value);
        }

        /// <summary>
        /// 返回循环访问的枚举数
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Json> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        /// <summary>
        /// 深度克隆
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new JsonArray(items);
        }

        /// <summary>
        /// 返回表示当前对象的 字符串
        /// </summary>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append('[');
            if (items.Count > 0)
                builder.Append(items[0].ToString());

            for (int i = 1; i < items.Count; i++)
            {
                var json = items[i];
                builder.Append(',');
                builder.Append(json.ToString());
            }
            builder.Append(']');
            return builder.ToString();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
    }
}