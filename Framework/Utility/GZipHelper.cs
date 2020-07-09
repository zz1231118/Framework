using System;
using System.IO;
using System.IO.Compression;

namespace Framework.Utility
{
    /// <summary>
    /// 数据压缩类,数据长度大于50000，压缩才有意义
    /// </summary>
    internal static class GZipHelper
    {
        #region 压缩

        /// <summary>
        /// 压缩流数据
        /// </summary>
        /// <param name="aSourceStream"></param>
        /// <returns></returns>
        public static byte[] EnCompress(Stream aSourceStream)
        {
            MemoryStream vMemory = new MemoryStream();
            aSourceStream.Seek(0, SeekOrigin.Begin);
            vMemory.Seek(0, SeekOrigin.Begin);
            try
            {
                using (GZipStream vZipStream = new GZipStream(vMemory, CompressionMode.Compress))
                {
                    byte[] vFileByte = new byte[1024];
                    int vRedLen = 0;
                    do
                    {
                        vRedLen = aSourceStream.Read(vFileByte, 0, vFileByte.Length);
                        vZipStream.Write(vFileByte, 0, vRedLen);
                    }
                    while (vRedLen > 0);
                }
            }
            finally
            {
                vMemory.Dispose();
            }
            return vMemory.ToArray();
        }
        /// <summary>
        /// 压缩数据
        /// </summary>
        /// <param name="aSourceStream"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static byte[] EnCompress(byte[] aSourceStream, int index, int count)
        {
            using (MemoryStream vMemory = new MemoryStream(aSourceStream, index, count))
            {
                return EnCompress(vMemory);
            }
        }
        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="oldStream">源流</param>
        /// <param name="newStream">压缩流</param>
        public static void EnCompress(Stream oldStream, Stream newStream)
        {
            if (oldStream == null)
                throw new ArgumentNullException(nameof(oldStream));
            if (newStream == null)
                throw new ArgumentNullException(nameof(newStream));

            oldStream.Seek(0, SeekOrigin.Begin);
            newStream.Seek(0, SeekOrigin.Begin);
            using (GZipStream vZipStream = new GZipStream(newStream, CompressionMode.Compress, true))
            {
                byte[] vFileByte = new byte[1024];
                int vRedLen = 0;
                do
                {
                    vRedLen = oldStream.Read(vFileByte, 0, vFileByte.Length);
                    vZipStream.Write(vFileByte, 0, vRedLen);
                }
                while (vRedLen > 0);
            }
            oldStream.Seek(0, SeekOrigin.Begin);
            newStream.Seek(0, SeekOrigin.Begin);
        }
        #endregion

        #region 解压

        /// <summary>
        /// 解压数据
        /// </summary>
        /// <param name="aSourceStream"></param>
        /// <returns></returns>
        public static byte[] DeCompress(Stream aSourceStream)
        {
            byte[] vUnZipByte = null;
            GZipStream vUnZipStream;

            using (MemoryStream vMemory = new MemoryStream())
            {
                vUnZipStream = new GZipStream(aSourceStream, CompressionMode.Decompress);
                try
                {
                    byte[] vTempByte = new byte[1024];
                    int vRedLen = 0;
                    do
                    {
                        vRedLen = vUnZipStream.Read(vTempByte, 0, vTempByte.Length);
                        vMemory.Write(vTempByte, 0, vRedLen);
                    }
                    while (vRedLen > 0);
                    vUnZipStream.Close();
                }
                finally
                {
                    vUnZipStream.Dispose();
                }
                vUnZipByte = vMemory.ToArray();
            }
            return vUnZipByte;
        }
        /// <summary>
        /// 解压数据
        /// </summary>
        /// <param name="aSourceByte"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static byte[] DeCompress(byte[] aSourceByte, int index, int count)
        {
            using (MemoryStream vMemory = new MemoryStream(aSourceByte, index, count))
            {
                return DeCompress(vMemory);
            }
        }
        /// <summary>
        /// 解压
        /// </summary>
        /// <param name="oldStream">压缩流</param>
        /// <param name="newStream">解压流</param>
        public static void DeCompress(Stream oldStream, Stream newStream)
        {
            if (oldStream == null)
                throw new ArgumentNullException(nameof(oldStream));
            if (newStream == null)
                throw new ArgumentNullException(nameof(newStream));

            oldStream.Seek(0, SeekOrigin.Begin);
            newStream.Seek(0, SeekOrigin.Begin);
            using (var vUnZipStream = new GZipStream(oldStream, CompressionMode.Decompress, true))
            {
                byte[] vTempByte = new byte[1024];
                int vRedLen = 0;
                do
                {
                    vRedLen = vUnZipStream.Read(vTempByte, 0, vTempByte.Length);
                    newStream.Write(vTempByte, 0, vRedLen);
                }
                while (vRedLen > 0);
            }
            oldStream.Seek(0, SeekOrigin.Begin);
            newStream.Seek(0, SeekOrigin.Begin);
        }
        #endregion
    }
}
