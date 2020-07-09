using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Framework.Log
{
    public sealed class FileLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string name)
        {
            return new FileLogger(name);
        }

        class FileLogger : BaseLogger
        {
            private readonly object root = new object();
            private readonly string baseDirectory;
            private readonly ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();
            private FileStream fileStream;
            private StreamWriter streamWriter;
            private DateTime lastAccessTime;
            private int fileIndex;

            public FileLogger(string name)
                : base(name)
            {
                baseDirectory = Path.Combine(Logger.Setting.BaseDirectory, name);
                var directoryInfo = new DirectoryInfo(baseDirectory);
                if (directoryInfo.Exists)
                {
                    var now = DateTime.Now;
                    var files = directoryInfo.GetFiles(now.ToString("yyyy-MM-dd") + @"-*.log");
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

            private void CheckSubstituteFile()
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
                    else if (fileStream.Length > Logger.Setting.MaximumFileLength)
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

                    var filePath = Path.Combine(baseDirectory, DateTime.Now.ToString("yyyy-MM-dd-") + fileIndex.ToString() + ".log");
                    fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                    streamWriter = new StreamWriter(fileStream, Logger.Setting.Encoding);
                    streamWriter.AutoFlush = true;
                    lastAccessTime = DateTime.Now;
                }
            }

            private void CloseFileStream()
            {
                lock (root)
                {
                    if (fileStream == null)
                        return;

                    streamWriter.Dispose();
                    fileStream.Dispose();
                    streamWriter = null;
                    fileStream = null;
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

            protected override void WriteMessage(string message)
            {
                messageQueue.Enqueue(message);
            }

            public override void Flush()
            {
                base.Flush();
                if (messageQueue.Count == 0)
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

                    CheckSubstituteFile();
                    while (messageQueue.TryDequeue(out string message))
                    {
                        streamWriter.WriteLine(message);
                    }
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
