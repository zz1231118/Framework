using System;
using System.IO;

namespace Framework.JavaScript
{
    /// <summary>
    /// Json 文本解释 类
    /// </summary>
    internal static partial class JsonParser
    {
        private static void EnsureCapacity(ref char[] array, int offset)
        {
            if (offset == array.Length)
            {
                var newarray = new char[array.Length * 2];
                Array.Copy(array, newarray, offset);
                array = newarray;
            }
        }

        /// <summary>
        /// 解析 Json 文本
        /// </summary>
        /// <param name="text">欲解析的 Json 文本</param>
        /// <returns>返回解析的 Json</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="JsonException"></exception>
        public static Json Parse(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException(nameof(text));

            using (var reader = new StringReader(text.Trim()))
                return ReadJson(reader);
        }

        private static Json ReadJson(TextReader reader)
        {
            if (reader.Peek() < 0)
                throw new JsonException("不是正确的Json 字符串");

            int value;
            while ((value = reader.Peek()) >= 0)
            {
                switch (value)
                {
                    case 9:     // \t
                    case 10:    // \n
                    case 13:    // \r
                    case 32:    // ' '
                        reader.Read();
                        break;

                    case 34:    //"
                        return new JsonValue(ReadString(reader));

                    case 39:    //'
                        reader.Read();
                        char ch = (char)reader.Read();
                        if (reader.Read() != 39)
                            throw new JsonException("转换失败 [Char 匹配错误]");

                        return new JsonValue(ch);
                    case 0x66://f
                    case 70://F
                        reader.Read();

                        if (!IsMatch(reader, "alse"))
                            throw new JsonException("转换失败 [false]");

                        return new JsonValue(false);
                    case 0x6E://n
                        reader.Read();
                        if (!IsMatch(reader, "ull"))
                            throw new JsonException("转换失败 [null]");

                        return Json.Null;
                    case 78://N
                        reader.Read();
                        switch (reader.Peek())
                        {
                            case 97://a
                                reader.Read();
                                if (reader.Read() != 78)
                                    throw new JsonException("invalid cast NaN");

                                return double.NaN;
                            case 117://u
                                reader.Read();
                                if (!IsMatch(reader, "ll"))
                                    throw new JsonException("转换失败 [null]");

                                return Json.Null;
                            default:
                                throw new JsonException("unknown:" + reader.ReadToEnd());
                        }
                    case 84://T
                    case 0x74://t
                        reader.Read();

                        if (!IsMatch(reader, "rue"))
                            throw new JsonException("转换失败 [true]");

                        return new JsonValue(true);
                    case 91://[
                        return ReadJsonArray(reader);
                    case 123://{
                        return ReadJsonObject(reader);

                    default:
                        /* 45 => -
                         * 46 => .
                         */
                        if ((value >= 48 && value <= 57) || value == 46 || value == 45)
                            return new JsonValue(ReadNumber(reader));
                        else
                            throw new JsonException("转换失败 [可能原因:未知类型]");
                }
            }

            throw new JsonException("转换失败 [可能原因:解析的不是Json 文本或Json 文本不完整]");
        }

        private static string ReadString(TextReader reader)
        {
            if (reader.Read() != 34)//"
                throw new JsonException("转换失败 [字符串 始 匹配失败]");

            int value;
            int offset = 0;
            var array = new char[16];
            while ((value = reader.Read()) >= 0)
            {
                switch (value)
                {
                    case 92:// => \
                        EnsureCapacity(ref array, offset);
                        value = reader.Read();
                        switch (value)
                        {
                            case 34://" 双引号
                                array[offset++] = '"';
                                break;
                            case 39://' 单引号
                                array[offset++] = '\'';
                                break;
                            case 48://0 空
                                array[offset++] = '\0';
                                break;
                            case 92://\ 反斜杠
                                array[offset++] = '\\';
                                break;
                            case 97://a 警告（产生峰鸣）
                                array[offset++] = '\a';
                                break;
                            case 98://b 退格
                                array[offset++] = '\b';
                                break;
                            case 102://f 换页
                                array[offset++] = '\f';
                                break;
                            case 110://n 换行
                                array[offset++] = '\n';
                                break;
                            case 114://r 回车
                                array[offset++] = '\r';
                                break;
                            case 116://t 水平制表符
                                array[offset++] = '\t';
                                break;
                            case 117://u 十六进制字符
                                var num = 0;
                                for (int i = 0; i < 4; i++)
                                {
                                    value = reader.Read();
                                    if (48 <= value && value <= 57)
                                    {
                                        //0-9
                                        num = (num << 4) + value - 48;
                                    }
                                    else if (97 <= value && value <= 102)
                                    {
                                        //a-f
                                        num = (num << 4) + value - 87;
                                    }
                                    else if (65 <= value && value <= 70)
                                    {
                                        //A-F
                                        num = (num << 4) + value - 55;
                                    }
                                    else
                                    {
                                        throw new JsonException("无效的十六进制字符：\\" + (char)num);
                                    }
                                }
                                array[offset++] = (char)num;
                                break;
                            case 118://v 垂直制表符
                                array[offset++] = '\v';
                                break;
                            default:
                                throw new JsonException("无效的转义字符：\\" + (char)value);
                        }
                        break;
                    case 34: // "
                        return new string(array, 0, offset);
                    default:
                        EnsureCapacity(ref array, offset);
                        array[offset++] = (char)value;
                        break;
                }
            }

            throw new JsonException("转换失败 [字符串 终 匹配失败]");
        }

        private static void ReadBufferNaiveNumber(TextReader reader, ref char[] array, ref int offset, bool married)
        {
            int value;
            if (married)
            {
                value = reader.Read();
                if (value < 48 || value > 57)
                    throw new JsonFormatException();

                EnsureCapacity(ref array, offset);
                array[offset++] = (char)value;
            }
            while ((value = reader.Peek()) >= 0)
            {
                if (value < 48 || value > 57)
                    break;

                EnsureCapacity(ref array, offset);
                array[offset++] = (char)reader.Read();
            }
        }

        private static decimal ReadNumber(TextReader reader)
        {
            int value = reader.Peek();
            if (value == -1)
                throw new EndOfStreamException();

            int offset = 0;
            var array = new char[4];
            if (value == 43 || value == 45)
            {
                /* 43 => +
                 * 45 => -
                 */
                array[offset++] = (char)reader.Read();
                value = reader.Peek();
                if (value < 48 || value > 57)
                    throw new JsonFormatException(new string(array, 0, offset));
            }
            if (value == 48)
            {
                array[offset++] = (char)reader.Read();
                value = reader.Peek();
                if (value == 120 || value == 88)
                {
                    //0x, 0X
                    array[offset++] = (char)reader.Read();
                    while ((value = reader.Peek()) >= 0)
                    {
                        if ((value >= 48 && value <= 57) || (value >= 97 && value <= 102) || (value >= 65 && value <= 70))
                        {
                            EnsureCapacity(ref array, offset);
                            array[offset++] = (char)reader.Read();
                        }
                        else
                        {
                            break;
                        }
                    }

                    var istr = new string(array, 0, offset);
                    try
                    {
                        return Convert.ToInt64(istr, 16);
                    }
                    catch (FormatException)
                    {
                        throw new JsonFormatException(istr);
                    }
                }
            }

            ReadBufferNaiveNumber(reader, ref array, ref offset, false);
            value = reader.Peek();
            if (value == 46)
            {
                //.
                EnsureCapacity(ref array, offset);
                array[offset++] = (char)reader.Read();
                ReadBufferNaiveNumber(reader, ref array, ref offset, true);
                value = reader.Peek();
            }
            if (value == 101 || value == 69)
            {
                //e, E
                EnsureCapacity(ref array, offset);
                array[offset++] = (char)reader.Read();
                value = reader.Read();
                if (value != 43 && value != 45)
                    throw new JsonFormatException();

                EnsureCapacity(ref array, offset);
                array[offset++] = (char)value;
                ReadBufferNaiveNumber(reader, ref array, ref offset, true);
            }

            var dstr = new string(array, 0, offset);
            if (!decimal.TryParse(dstr, out decimal dval))
                throw new JsonFormatException(dstr);

            return dval;
        }

        private static JsonArray ReadJsonArray(TextReader reader)
        {
            if (reader.Read() != 91)// 91 => [
                throw new JsonException("转换 JsonArray 失败 [JsonArray 始 匹配失败]");

            JsonArray jsonAry = new JsonArray();
            if (reader.Peek() != 93)//93 => ]
                jsonAry.Add(ReadJson(reader));

            while (reader.Peek() >= 0)
            {
                switch (reader.Read())
                {
                    case 9:     // \t
                    case 10:    // \n
                    case 13:    // \r
                    case 32:    // ' '
                        break;
                    case 44:// => ,
                        jsonAry.Add(ReadJson(reader));
                        break;
                    case 93://]
                        return jsonAry;
                    default:
                        throw new JsonException(string.Format("转换 JsonArray 失败 [JsonArray 乱码字符] [{0}]", reader.ReadToEnd()));
                }
            }

            throw new JsonException("转换 JsonArray 失败 [JsonArray 终 匹配失败]");
        }

        private static JsonObject ReadJsonObject(TextReader reader)
        {
            if (reader.Read() != 123)// 123 => {
                throw new JsonException("转换 JsonObject 失败 [JsonObject 始 匹配失败]");

            JsonObject jsonObject = new JsonObject();
            int temp;
            while ((temp = reader.Peek()) >= 0)
            {
                switch (temp)
                {
                    case 9:     // \t
                    case 10:    // \n
                    case 13:    // \r
                    case 32:    // ' '
                        reader.Read();
                        break;

                    case 34:// "
                        string keyText = ReadString(reader);

                        if (!IsMatch(reader, ":"))
                        {
                            throw new JsonException("转换 JsonObject 失败 [间隔符:丢失]");
                        }

                        jsonObject.Add(keyText, ReadJson(reader));
                        break;

                    case 44:// ,
                        reader.Read();
                        break;

                    case 125: // }
                        reader.Read();
                        return jsonObject;
                    default:
                        throw new JsonException(string.Format("转换 JsonObject 失败 [JsonObject 乱码字符] [{0}]", reader.ReadToEnd()));
                }
            }

            throw new JsonException("转换 JsonObject 失败 [JsonObject 终 匹配失败]");
        }

        private static bool IsMatch(TextReader reader, string str)
        {
            foreach (char c in str)
            {
                if (reader.Read() != (int)c)
                    return false;
            }
            return true;
        }
    }
}