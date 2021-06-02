using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

namespace Framework.Net.Sockets
{
    internal class BufferManager
    {
        private readonly int capacity;
        private readonly int saeaSize;
        private readonly byte[] bufferBlock;
        private readonly ConcurrentStack<int> freeIndexPool;
        private int currentIndex;

        public BufferManager(int capacity, int saeaSize)
        {
            this.capacity = capacity;
            this.saeaSize = saeaSize;

            freeIndexPool = new ConcurrentStack<int>();
            bufferBlock = new byte[this.capacity];
        }

        public bool SetBuffer(SocketAsyncEventArgs ioEventArgs)
        {
            if (freeIndexPool.TryPop(out int index))
            {
                ioEventArgs.SetBuffer(bufferBlock, index, saeaSize);
            }
            else
            {
                int oldIndex;
                while (true)
                {
                    oldIndex = currentIndex;
                    var newIndex = oldIndex + saeaSize;
                    if (newIndex > capacity)
                        return false;

                    if (Interlocked.CompareExchange(ref currentIndex, newIndex, oldIndex) == oldIndex)
                        break;
                }
                ioEventArgs.SetBuffer(bufferBlock, oldIndex, saeaSize);
            }
            return true;
        }

        public void FreeBuffer(SocketAsyncEventArgs ioEventArgs)
        {
            freeIndexPool.Push(ioEventArgs.Offset);
            ioEventArgs.SetBuffer(null, 0, 0);
        }
    }
}