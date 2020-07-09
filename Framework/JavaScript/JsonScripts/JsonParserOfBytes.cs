using System;
using System.IO;
using System.Text;

namespace Framework.JavaScript
{
    /// <summary>
    /// Bson 解析类
    /// </summary>
    internal static partial class JsonParser
    {
        /// <summary>
        /// 把 Bson 形式的 byte[] 转换到 Bson
        /// </summary>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="Framework.Jsons.JsoncriptException"></exception>
        public static Json Parse(byte[] data, int offset, int count)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException(nameof(data));

            using (var reader = new BinaryReader(new MemoryStream(data, offset, count), Encoding.Unicode))
            {
                try
                {
                    return ReadJson(reader);
                }
                catch (EndOfStreamException ex)
                {
                    throw new JsonException("不是正确的Json 流", ex);
                }
            }
        }

        private static Json ReadJson(BinaryReader reader)
        {
            byte byteForBsonType = reader.ReadByte();
            var bsonType = (JsonTypeCode)byteForBsonType;
            switch (bsonType)
            {
                case JsonTypeCode.Null:
                    return Json.Null;
                case JsonTypeCode.Boolean:
                    return new JsonValue(reader.ReadByte() == 1);
                case JsonTypeCode.Int16:
                    return new JsonValue(reader.ReadInt16());
                case JsonTypeCode.UInt16:
                    return new JsonValue(reader.ReadUInt16());
                case JsonTypeCode.Int32:
                    return new JsonValue(reader.ReadInt32());
                case JsonTypeCode.UInt32:
                    return new JsonValue(reader.ReadUInt32());
                case JsonTypeCode.Int64:
                    return new JsonValue(reader.ReadInt64());
                case JsonTypeCode.UInt64:
                    return new JsonValue(reader.ReadUInt64());
                case JsonTypeCode.Single:
                    return new JsonValue(reader.ReadSingle());
                case JsonTypeCode.Double:
                    return new JsonValue(reader.ReadDouble());
                case JsonTypeCode.Byte:
                    return new JsonValue(reader.ReadByte());
                case JsonTypeCode.SByte:
                    return new JsonValue(unchecked((sbyte)reader.ReadByte()));
                case JsonTypeCode.Char:
                    return new JsonValue(reader.ReadChar());
                case JsonTypeCode.Decimal:
                    return new JsonValue((decimal)reader.ReadDouble());
                case JsonTypeCode.String:
                    return new JsonValue(ReadString(reader));
                case JsonTypeCode.Binary:
                    return ReadJsonBinary(reader);
                case JsonTypeCode.Array:
                    return ReadJsonArray(reader);
                case JsonTypeCode.Object:
                    return ReadJsonObject(reader);
                default:
                    throw new JsonException("未知类型");
            }
        }

        private static string ReadString(BinaryReader reader)
        {
            var length = reader.ReadInt32();
            var byteForAry = reader.ReadBytes(length);
            return Jsonetting.Encoding.GetString(byteForAry);
        }

        private static JsonBinary ReadJsonBinary(BinaryReader reader)
        {
            var length = reader.ReadInt32();
            return new JsonBinary(reader.ReadBytes(length));
        }

        private static JsonArray ReadJsonArray(BinaryReader reader)
        {
            var jsonArray = new JsonArray();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                jsonArray.Add(ReadJson(reader));

            return jsonArray;
        }

        private static JsonObject ReadJsonObject(BinaryReader reader)
        {
            var jsonObject = new JsonObject();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                jsonObject.Add(ReadString(reader), ReadJson(reader));

            return jsonObject;
        }
    }
}