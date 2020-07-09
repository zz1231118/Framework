using System;
using System.Threading;

namespace Framework.Statistics
{
    public class Int32CounterStatistic : ICounter<int>
    {
        private readonly string name;
        private int value;

        public Int32CounterStatistic(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            this.name = name;
        }

        public string Name => name;

        public int Value => value;

        public void IncrementBy(int value)
        {
            Interlocked.Add(ref this.value, value);
        }

        public void DecrementBy(int value)
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
    }
}
