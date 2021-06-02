using System;

namespace Framework.Data.Entry
{
    public class DbContextOptions
    {
        public static readonly DbContextOptions Default = new DbContextOptions();

        /// <summary>
        /// 执行超时时长
        /// </summary>
        public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 异常处理
        /// <para>默认：<see cref="ExceptionHandling.Interrupt"/></para>
        /// </summary>
        public ExceptionHandling ExceptionHandling { get; set; } = ExceptionHandling.Interrupt;
    }

    public enum ExceptionHandling : byte
    {
        /// <summary>
        /// 中断
        /// </summary>
        Interrupt,
        /// <summary>
        /// 跳过
        /// </summary>
        Skip,
    }
}
