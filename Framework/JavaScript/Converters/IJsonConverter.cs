using System;

namespace Framework.JavaScript.Converters
{
    /// <summary>
    /// Json 转换器 接口
    /// </summary>
    public interface IJsonConverter
    {
        /// <summary>
        /// 把指定类型转换成 Json
        /// </summary>
        /// <param name="value">欲转换的对象</param>
        /// <param name="conversionType">欲转换的类型</param>
        /// <returns></returns>
        Json ConvertFrom(object value, Type conversionType);

        /// <summary>
        /// 把 Json 转换成指定 类型对象
        /// </summary>
        /// <param name="value">欲转换的 Json</param>
        /// <param name="conversionType">欲转换到的对象类型</param>
        /// <returns></returns>
        object ConvertTo(Json value, Type conversionType);
    }
}