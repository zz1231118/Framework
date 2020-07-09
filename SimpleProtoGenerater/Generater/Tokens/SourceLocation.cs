using System;

namespace SimpleProtoGenerater.Generater.Tokens
{
    public struct SourceLocation : IEquatable<SourceLocation>
    {
        public static readonly SourceLocation Empty = new SourceLocation(-1, -1, -1);
        private readonly int _index;
        private readonly int _row;
        private readonly int _column;

        public SourceLocation(int index, int row, int column)
        {
            _index = index;
            _row = row;
            _column = column;
        }

        public int Index => _index;
        public int Row => _row;
        public int Column => _column;

        public static bool operator ==(SourceLocation lhs, SourceLocation rhs)
        {
            return lhs._index == rhs._index && lhs._row == rhs._row && lhs._column == rhs._column;
        }
        public static bool operator !=(SourceLocation lhs, SourceLocation rhs)
        {
            return lhs._index != rhs._index || lhs._row != rhs._row || lhs._column != rhs._column;
        }

        public bool Equals(SourceLocation other)
        {
            return other._index == _index && other._row == _row && other._column == _column;
        }

        public override bool Equals(object obj)
        {
            return Equals((SourceLocation)obj); ;
        }
        public override int GetHashCode()
        {
            return _index.GetHashCode() ^ _row.GetHashCode() ^ _column.GetHashCode();
        }
    }
}
