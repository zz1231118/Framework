using System;
using System.Threading;

namespace Framework.Threading
{
    /// <summary>
    /// 完成来源
    /// </summary>
    /// <typeparam name="T">结果模板</typeparam>
    public class CompletionSource<T> : IDisposable
    {
        private const int NoneSentinel = 0;
        private const int CompletedSentinel = 1;

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEventSlim manualResetEventSlim = new ManualResetEventSlim();
        private bool isDisposed;
        private volatile int isInCompleted;
        private volatile Exception? exception;
        private volatile object? result;

        /// <summary>
        /// 是否已完成
        /// </summary>
        public bool IsCompleted => isInCompleted == CompletedSentinel;

        /// <summary>
        /// 是否已异常
        /// </summary>
        public bool IsFaulted => exception != null;

        /// <summary>
        /// 是否已被取消
        /// </summary>
        public bool IsCancelled => cancellationTokenSource.IsCancellationRequested;

        protected void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                isDisposed = true;
                manualResetEventSlim.Dispose();
            }
        }

        /// <summary>
        /// 获取结果
        /// </summary>
        /// <param name="timeout">超时时长</param>
        /// <returns></returns>
        /// <exception cref="System.TimeoutException" />
        /// <exception cref="System.OperationCanceledException" />
        public T GetResult(TimeSpan timeout)
        {
            if (!manualResetEventSlim.Wait(timeout, cancellationTokenSource.Token))
            {
                if (cancellationTokenSource.IsCancellationRequested) throw new OperationCanceledException();
                else throw new TimeoutException();
            }
            if (exception != null)
            {
                throw exception;
            }

            return (T)result;
        }

        /// <summary>
        /// 获取结果
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.TimeoutException" />
        /// <exception cref="System.OperationCanceledException" />
        public T GetResult()
        {
            return GetResult(Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// 尝试设置结果
        /// </summary>
        public bool TrySetResult(T result)
        {
            if (Interlocked.CompareExchange(ref isInCompleted, CompletedSentinel, NoneSentinel) == NoneSentinel)
            {
                this.result = result;
                manualResetEventSlim.Set();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 尝试设置异常
        /// </summary>
        public bool TrySetException(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            if (Interlocked.CompareExchange(ref isInCompleted, CompletedSentinel, NoneSentinel) == NoneSentinel)
            {
                this.exception = exception;
                manualResetEventSlim.Set();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 尝试设置取消
        /// </summary>
        public bool TrySetCanceled()
        {
            if (Interlocked.CompareExchange(ref isInCompleted, CompletedSentinel, NoneSentinel) == NoneSentinel)
            {
                cancellationTokenSource.Cancel();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 设置结果
        /// </summary>
        /// <param name="result">结果值</param>
        public void SetResult(T result)
        {
            if (!TrySetResult(result))
            {
                throw new InvalidOperationException("already completed");
            }
        }

        /// <summary>
        /// 设置异常
        /// </summary>
        /// <param name="exception">异常</param>
        public void SetException(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            if (!TrySetException(exception))
            {
                throw new InvalidOperationException("already completed");
            }
        }

        /// <summary>
        /// 设置取消
        /// </summary>
        public void SetCanceled()
        {
            if (!TrySetCanceled())
            {
                throw new InvalidOperationException("already completed");
            }
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
