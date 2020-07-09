using System;

namespace Framework.JavaScript
{
    /// <summary>
    /// JsonTypeCode
    /// </summary>
    [Serializable]
    internal enum JsonTypeCode : byte
    {
        /// <summary>
        /// Null
        /// </summary>
        Null,
        /// <summary>
        /// Boolean
        /// </summary>
        Boolean,
        /// <summary>
        /// Byte
        /// </summary>
        Byte,
        /// <summary>
        /// SByte
        /// </summary>
        SByte,
        /// <summary>
        /// Char
        /// </summary>
        Char,
        /// <summary>
        /// Short
        /// </summary>
        Int16,
        /// <summary>
        /// UShort
        /// </summary>
        UInt16,
        /// <summary>
        /// Int
        /// </summary>
        Int32,
        /// <summary>
        /// UInt
        /// </summary>
        UInt32,
        /// <summary>
        /// Long
        /// </summary>
        Int64,
        /// <summary>
        /// ULong
        /// </summary>
        UInt64,
        /// <summary>
        /// Float
        /// </summary>
        Single,
        /// <summary>
        /// Double
        /// </summary>
        Double,
        /// <summary>
        /// Decimal
        /// </summary>
        Decimal,
        /// <summary>
        /// String
        /// </summary>
        String,
        /// <summary>
        /// JsonObject
        /// </summary>
        Object,
        /// <summary>
        /// JsonArray
        /// </summary>
        Array,
        /// <summary>
        /// JsonBinary
        /// </summary>
        Binary,
    }
}