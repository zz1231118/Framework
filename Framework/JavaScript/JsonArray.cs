using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework.JavaScript
{
    /// <summary>
    /// JsonArray
    /// </summary>
    [Serializable]
    public sealed class JsonArray : Json, IList<Json>
    {
        private readonly List<Json> list;

        /// <summary>
        /// JsonArray 构造函数
        /// </summary>
        public JsonArray()
        {
            list = new List<Json>();
        }

        /// <summary>
        /// JsonArray 构造函数
        /// </summary>
        /// <param name="capacity"></param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public JsonArray(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            list = new List<Json>(capacity);
        }

        /// <summary>
        /// JsonArray 构造函数
        /// </summary>
        /// <param name="collection"></param>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public JsonArray(IEnumerable<Json> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (collection.Contains(null))
                throw new ArgumentException(nameof(collection));

            list = new List<Json>(collection);
        }

        /// <summary>
        /// MyJsonList this
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public Json this[int index]
        {
            get
            {
                if (index < 0 || index >= list.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return list[index];
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                if (index < 0 || index >= list.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                list[index] = value;
            }
        }

        /// <summary>
        /// 实际包含的元素数
        /// </summary>
        public int Count => list.Count;

        /// <summary>
        /// 是否为只读
        /// </summary>
        bool ICollection<Json>.IsReadOnly => false;

        /// <summary>
        /// 把指定集合添加到列表
        /// </summary>
        /// <param name="collection"></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void AddRange(IEnumerable<Json> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            list.AddRange(collection);
        }

        /// <summary>
        /// 对 JsonArray 的每个元素执行指定操作
        /// </summary>
        /// <param name="action"></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void ForEach(Action<Json> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (var item in list)
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
            return list.IndexOf(item);
        }

        /// <summary>
        /// 把元素插入到指定索引处
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void Insert(int index, Json value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            list.Insert(index, value);
        }

        /// <summary>
        /// 移除指定索引处的元素
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        /// <summary>
        /// 把对象添加到列表的结尾
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void Add(Json value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            list.Add(value);
        }

        /// <summary>
        /// 移除所有元素
        /// </summary>
        public void Clear()
        {
            list.Clear();
        }

        /// <summary>
        /// 确定指定项是否存在
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(Json item)
        {
            return list.Contains(item);
        }

        /// <summary>
        /// 复制从指定索引开始的元素到数组
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public void CopyTo(Json[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex >= array.Length)
                throw new IndexOutOfRangeException();

            list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// 移除指定项
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(Json item)
        {
            return list.Remove(item);
        }

        /// <summary>
        /// 返回循环访问的枚举数
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Json> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        /// <summary>
        /// 深度克隆
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new JsonArray(list);
        }

        /// <summary>
        /// 返回表示当前对象的 字节集
        /// </summary>
        public override byte[] ToBinary()
        {
            var list = new List<byte>();
            list.Add((byte)JsonTypeCode.Array);
            list.AddRange(BitConverter.GetBytes(this.list.Count));
            foreach (var item in this.list)
                list.AddRange(item.ToBinary());

            return list.ToArray();
        }

        /// <summary>
        /// 返回表示当前对象的 字符串
        /// </summary>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            if (list.Count > 0)
                builder.Append(list[0].ToString());

            for (int i = 1; i < list.Count; i++)
            {
                var json = list[i];
                builder.Append(',');
                builder.Append(json.ToString());
            }
            builder.Append(']');
            return builder.ToString();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}