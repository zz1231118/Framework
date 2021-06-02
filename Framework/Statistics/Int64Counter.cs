using System;
using System.Threading;

namespace Framework.Statistics
{
    public class Int64Counter : ICounter<long>
    {
        private readonly string name;
        private long value;

        public Int64Counter(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            this.name = name;
        }

        public string Name => name;

        public long Value => value;

        public void IncrementBy(long value)
        {
            Interlocked.Add(ref this.value, value);
        }

        public void DecrementBy(long value)
        {
            Interlocked.Add(ref this.value, -value);
        }

        public void Increment()
        {
            Interlocked.Increment(ref value);
        }

        public void Decrement()
        {
            Interlocked.Decrement(ref value);
        }

        public long Reset()
        {
            return Interlocked.Exchange(ref value, 0);
        }
    }
}
