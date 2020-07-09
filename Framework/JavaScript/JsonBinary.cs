using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.JavaScript
{
    /// <summary>
    /// JsonBinary
    /// </summary>
    [Serializable]
    public sealed class JsonBinary : Json, IList<byte>
    {
        private readonly List<byte> list;

        /// <summary>
        /// JsonBinary 构造函数
        /// </summary>
        public JsonBinary()
        {
            list = new List<byte>();
        }

        /// <summary>
        /// JsonBinary 构造函数
        /// </summary>
        /// <param name="capacity"></param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public JsonBinary(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            list = new List<byte>(capacity);
        }

        /// <summary>
        /// JsonBinary 构造函数
        /// </summary>
        /// <param name="collection"></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public JsonBinary(IEnumerable<byte> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            list = new List<byte>(collection);
        }

        /// <summary>
        /// 要获得或设置从零开始的索引
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte this[int index]
        {
            get { return list[index]; }
            set { list[index] = value; }
        }

        /// <summary>
        /// 返回列表中的元素数
        /// </summary>
        public int Count => list.Count;

        /// <summary>
        /// 是否为只读
        /// </summary>
        bool ICollection<byte>.IsReadOnly => false;

        /// <inheritdoc />
        public static implicit operator JsonBinary(byte[] value)
        {
            return new JsonBinary(value);
        }

        /// <inheritdoc />
        public static explicit operator byte[](JsonBinary value)
        {
            return value.list.ToArray();
        }

        /// <summary>
        /// 把指定集合添加到列表
        /// </summary>
        /// <param name="array"></param>
        public void AddRange(byte[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            AddRange(array, 0, array.Length);
        }

        /// <summary>
        /// 把指定集合添加到列表
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayOffset"></param>
        /// <param name="count"></param>
        public void AddRange(byte[] array, int arrayOffset, int count)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayOffset < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayOffset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (arrayOffset + count > array.Length)
                throw new ArgumentException();

            var end = arrayOffset + count;
            for (int i = arrayOffset; i < end; i++)
                list.Add(array[i]);
        }

        /// <summary>
        /// 返回第一个匹配项从零开始的索引
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(byte item)
        {
            return list.IndexOf(item);
        }

        /// <summary>
        /// 将元素插入到索引位置
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, byte item)
        {
            list.Insert(index, item);
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
        /// 添加元素到结尾
        /// </summary>
        /// <param name="item"></param>
        public void Add(byte item)
        {
            list.Add(item);
        }

        /// <summary>
        /// 清除列表中所有的元素
        /// </summary>
        public void Clear()
        {
            list.Clear();
        }

        /// <summary>
        /// 确定某元素是否在列表中
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(byte item)
        {
            return list.Contains(item);
        }

        /// <summary>
        /// 将整个列表复制到兼容的一维数组中
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public void CopyTo(byte[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex >= array.Length)
                throw new IndexOutOfRangeException();

            list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// 移除特定对象的第一个匹配项
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(byte item)
        {
            return list.Remove(item);
        }
        /// <summary>
        /// 返回循环访问列表的枚举数
        /// </summary>
        /// <returns></returns>
        public IEnumerator<byte> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        /// <summary>
        /// 深度克隆
        /// </summary>
        public override object Clone()
        {
            return new JsonBinary(list);
        }

        /// <summary>
        /// 返回表示当前对象的 字节集
        /// </summary>
        public override byte[] ToBinary()
        {
            var bytes = new byte[1 + sizeof(int) + list.Count];
            bytes[0] = (byte)JsonTypeCode.Binary;
            BitHelper.SetBytes(bytes, 1, list.Count);
            list.CopyTo(0, bytes, 5, list.Count);
            return bytes;
        }

        /// <summary>
        /// 返回表示当前对象的 字符串
        /// </summary>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("[");
            if (list.Count > 0)
                builder.Append(list[0].ToString());

            for (int i = 1; i < list.Count; i++)
            {
                var by = list[i];
                builder.Append(",");
                builder.Append(by.ToString());
            }
            builder.Append("]");
            return builder.ToString();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}