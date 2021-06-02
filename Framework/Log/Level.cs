using System;

namespace Framework.Log
{
    public sealed class Level : IComparable<Level>, IEquatable<Level>, IComparable
    {
        public static readonly Level Off = new Level(int.MaxValue, "Off");
        public static readonly Level Fatal = new Level(60000, "Fatal");
        public static readonly Level Error = new Level(50000, "Error");
        public static readonly Level Warn = new Level(40000, "Warn");
        public static readonly Level Info = new Level(30000, "Info");
        public static readonly Level Debug = new Level(20000, "Debug");
        public static readonly Level Trace = new Level(10000, "Trace");
        public static readonly Level All = new Level(int.MinValue, "ALL");

        private readonly int value;
        private readonly string name;

        public Level(int value, string name)
        {
            this.name = name;
            this.value = value;
        }

        public int Value => value;

        public string Name => name;

        public static bool operator >(Level left, Level right)
        {
            if (left is null || right is null) return false;
            return left.value > right.value;
        }

        public static bool operator >=(Level left, Level right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.value >= right.value;
        }

        public static bool operator ==(Level left, Level right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.value == right.value;
        }

        public static bool operator !=(Level left, Level right)
        {
            if (left is null && right is null) return false;
            if (left is null || right is null) return true;
            return left.value != right.value;
        }

        public static bool operator <=(Level left, Level right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.value <= right.value;
        }

        public static bool operator <(Level left, Level right)
        {
            if (left is null || right is null) return false;
            return left.value < right.value;
        }

        public int CompareTo(Level other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return value - other.value;
        }

        public int CompareTo(object obj)
        {
            if (obj is Level other) return CompareTo(other);
            else throw new InvalidCastException();
        }

        public bool Equals(Level other)
        {
            return other?.value == value;
        }

        public override bool Equals(object obj)
        {
            return obj is Level other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return $"{{name:{name},value:{value}}}";
        }
    }
}
