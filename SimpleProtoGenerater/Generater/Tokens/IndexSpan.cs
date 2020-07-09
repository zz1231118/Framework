using System;

namespace SimpleProtoGenerater.Generater.Tokens
{
    public struct IndexSpan : IEquatable<IndexSpan>
    {
        private readonly int _start;
        private readonly int _end;

        public IndexSpan(int start, int end)
        {
            _start = start;
            _end = end;
        }

        public int Start => _start;
        public int End => _end;
        public bool IsEmpty => _start == _end;
        public int Length => _end - _start;

        public static bool operator ==(IndexSpan lhs, IndexSpan rhs)
        {
            return lhs._start == rhs._start && lhs._end == rhs._end;
        }
        public static bool operator !=(IndexSpan lhs, IndexSpan rhs)
        {
            return lhs._start != rhs._start || lhs._end != rhs._end;
        }

        public bool Equals(IndexSpan other)
        {
            return other._start == _start && other._end == _end;
        }
        public override bool Equals(object obj)
        {
            if (obj is IndexSpan)
            {
                return Equals((IndexSpan)obj);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return _start.GetHashCode() ^ _end.GetHashCode();
        }
    }
}
