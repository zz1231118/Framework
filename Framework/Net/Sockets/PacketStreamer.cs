using System.Collections.Generic;

namespace Framework.Net.Sockets
{
    /// <inheritdoc />
    public class PacketStreamer
    {
        private readonly Queue<byte[]> queue = new Queue<byte[]>();

        /// <inheritdoc />
        public int Count => queue.Count;

        /// <inheritdoc />
        public void Enqueue(byte[] packet)
        {
            queue.Enqueue(packet);
        }

        /// <inheritdoc />
        public byte[] Dequeue()
        {
            return queue.Dequeue();
        }

        /// <inheritdoc />
        public bool TryDequeue(out byte[] packet)
        {
            if (queue.Count == 0)
            {
                packet = null;
                return false;
            }

            packet = queue.Dequeue();
            return true;
        }

        /// <inheritdoc />
        public void Clear()
        {
            queue.Clear();
        }
    }
}
