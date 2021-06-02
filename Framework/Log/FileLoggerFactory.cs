using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Framework.Log
{
    public sealed class FileLoggerFactory : ILoggerFactory
    {
        /// <summary>
        /// 保存的基础目录
        /// </summary>
        public string BaseDirectory { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");

        /// <summary>
        /// 编码
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// 最大文件大小
        /// </summary>
        public int MaxFileLength { get; set; } = 1024 * 1024 * 10;

        /// <summary>
        /// 最大缓存数
        /// </summary>
        public int MaxCacheCount { get; set; } = 10 * 10000;

        public MessageLogger CreateLogger(Level level)
        {
            return new FileLogger(this, level);
        }

        class FileLogger : MessageLogger
        {
            private readonly object root = new object();
            private readonly FileLoggerFactory loggerFactory;
            private readonly string baseDirectory;
            private readonly ConcurrentQueue<string> messages = new ConcurrentQueue<string>();
            private FileStream fileStream;
            private StreamWriter streamWriter;
            private DateTime lastAccessTime;
            private int fileIndex;

            public FileLogger(FileLoggerFactory loggerFactory, Level level)
                : base(level)
            {
                this.loggerFactory = loggerFactory;
                baseDirectory = Path.Combine(loggerFactory.BaseDirectory, level.Name);
                if (Directory.Exists(baseDirectory))
                {
                    var now = DateTime.Now;
                    var path = Path.Combine(baseDirectory, now.ToString("yyyy-MM"));
                    var directory = new DirectoryInfo(path);
                    if (directory.Exists)
                    {
                        var files = directory.GetFiles(now.ToString("yyyy-MM-dd") + @"-*.log");
                        if (files.Length > 0)
                        {
                            var pattern = "^" + now.ToString("yyyy-MM-dd") + @"-(?<index>\d+)\.log$";
                            fileIndex = files.Select(p =>
                            {
                                var match = Regex.Match(p.Name, pattern);
                                if (!match.Success)
                                    return 0;

                                return int.Parse(match.Groups["index"].Value);
                            }).Max(p => p) + 1;
                        }
                    }
                }
            }

            private void CheckSwitchFile()
            {
                lock (root)
                {
                    OpenFileStream();
                    if (lastAccessTime.Date != DateTime.Now.Date)
                    {
                        CloseFileStream();
                        fileIndex = 0;
                        OpenFileStream();
                    }
                    else if (fileStream.Length > loggerFactory.MaxFileLength)
                    {
                        CloseFileStream();
                        fileIndex++;
                        OpenFileStream();
                    }
                }
            }

            private void OpenFileStream()
            {
                lock (root)
                {
                    if (fileStream != null)
                    {
                        //文件没关闭
                        return;
                    }
                    if (!Directory.Exists(baseDirectory))
                    {
                        //如果目录不存在则创建
                        Directory.CreateDirectory(baseDirectory);
                    }

                    var now = DateTime.Now;
                    var path = Path.Combine(baseDirectory, now.ToString("yyyy-MM"));
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                    path = Path.Combine(path, DateTime.Now.ToString("yyyy-MM-dd-") + fileIndex.ToString() + ".log");
                    fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
                    streamWriter = new StreamWriter(fileStream, loggerFactory.Encoding);
                    streamWriter.AutoFlush = true;
                    lastAccessTime = DateTime.Now;
                }
            }

            private void CloseFileStream()
            {
                lock (root)
                {
                    if (fileStream != null)
                    {
                        streamWriter.Dispose();
                        fileStream.Dispose();
                        streamWriter = null;
                        fileStream = null;
                    }
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (!IsDisposed)
                {
                    try
                    {
                        Flush();
                        CloseFileStream();
                    }
                    finally
                    {
                        base.Dispose(disposing);
                    }
                }
            }

            public override void Log(string message)
            {
                messages.Enqueue(message);
                if (messages.Count > loggerFactory.MaxCacheCount)
                {
                    messages.TryDequeue(out _);
                }
            }

            public override void Flush()
            {
                base.Flush();
                if (messages.Count == 0)
                {
                    //没有需要落地的消息
                    return;
                }

                var lockToken = false;
                try
                {
                    Monitor.TryEnter(root, ref lockToken);
                    if (!lockToken)
                    {
                        //进入失败
                        return;
                    }

                    CheckSwitchFile();
                    while (messages.TryDequeue(out string message))
                    {
                        streamWriter.WriteLine(message);
                    }
                }
                catch (Exception ex)
                {
                    var e = new LoggerUnhandledExceptionEventArgs(ex);
                    Logger.NotifyUnhandledExceptionEvent(e);
                }
                finally
                {
                    if (lockToken)
                    {
                        Monitor.Exit(root);
                    }
                }
            }
        }
    }
}
