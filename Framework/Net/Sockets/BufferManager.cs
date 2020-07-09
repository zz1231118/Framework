using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

namespace Framework.Net.Sockets
{
    internal class BufferManager
    {
        private readonly int _capacity;
        private readonly int _saeaSize;
        private readonly byte[] _bufferBlock;
        private readonly ConcurrentStack<int> _freeIndexPool;
        private int _currentIndex;

        public BufferManager(int capacity, int saeaSize)
        {
            _capacity = capacity;
            _saeaSize = saeaSize;

            _freeIndexPool = new ConcurrentStack<int>();
            _bufferBlock = new byte[_capacity];
        }

        public bool SetBuffer(SocketAsyncEventArgs args)
        {
            if (_freeIndexPool.TryPop(out int index))
            {
                args.SetBuffer(_bufferBlock, index, _saeaSize);
            }
            else
            {
                int oldIndex;
                while (true)
                {
                    oldIndex = _currentIndex;
                    var newIndex = oldIndex + _saeaSize;
                    if (newIndex > _capacity)
                        return false;

                    if (Interlocked.CompareExchange(ref _currentIndex, newIndex, oldIndex) == oldIndex)
                        break;
                }
                args.SetBuffer(_bufferBlock, oldIndex, _saeaSize);
            }
            return true;
        }

        public void FreeBuffer(SocketAsyncEventArgs args)
        {
            _freeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }
    }
}