using System;
using System.IO;
using Framework;

namespace SimpleProtoGenerater.Generater.Tokens
{
    public class TokenReader : BaseDisposed
    {
        private BufferReader _reader;

        public TokenReader(TextReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            _reader = new BufferReader(reader);
        }

        public bool EndOfStream => _reader.EndOfStream;
        public SourceLocation Location => new SourceLocation(checked((int)_reader.Position), _reader.Row, _reader.Column);

        private void PullWhitespace()
        {
            var val = _reader.Peek();
            while (val == 32)
            {
                _reader.Read();
                val = _reader.Peek();
            }
        }
        private void PullSpaceOrTab()
        {
            var val = _reader.Peek();
            while (val == 32 || val == 9)
            {
                _reader.Read();
                val = _reader.Peek();
            }
        }

        private Token ReadIdentity()
        {
            string str = string.Empty;
            SourceLocation location = Location;
            var val = _reader.Peek();
            var ch = (char)val;

            do
            {
                _reader.Read();
                str += ch;
                val = _reader.Peek();
                if (val == -1) break;
                ch = (char)val;
            } while (char.IsLetter(ch) || char.IsDigit(ch) || ch == '_');

            return new Token(TokenKind.Identity, str, str, location);
        }
        private Token ReadNumber()
        {
            string str = string.Empty;
            SourceLocation location = Location;
            var val = _reader.Peek();
            var ch = (char)val;
            bool isDot = false;

            do
            {
                _reader.Read();
                if (ch == '.') isDot = true;

                str += ch;
                val = _reader.Peek();
                if (val == -1) break;
                ch = (char)val;
            } while (char.IsDigit(ch) || (!isDot && ch == '.'));

            return new Token(TokenKind.Number, str, decimal.Parse(str), location);
        }
        private Token ReadString()
        {
            if ((char)_reader.Read() != '"')
                throw new InvalidOperationException();

            SourceLocation location = Location;
            string str = string.Empty;
            var val = _reader.Read();
            var ch = (char)val;
            while (ch != '"')
            {
                if (ch == '\\')
                {
                    ch = (char)_reader.Read();
                }

                str += ch;
                val = _reader.Read();
                if (val == -1) break;

                ch = (char)val;
            }

            return new Token(TokenKind.String, str, str, location);
        }
        private Token ReadComment()
        {
            if ((char)_reader.Read() != '/')
                throw new InvalidOperationException();

            var nextChar = (char)_reader.Read();
            if (nextChar != '/' && nextChar != '*')
                throw new InvalidOperationException();

            var str = string.Empty;
            SourceLocation location = Location;
            int val;
            while (true)
            {
                val = _reader.Read();
                if (nextChar == '*')
                {
                    if (val == 42 && _reader.Peek() == 47)
                    {
                        _reader.Read();
                        break;
                    }
                }
                else if (val == 13 && _reader.Peek() == 10)
                {
                    _reader.Read();
                    break;
                }

                str += (char)val;
            }

            return new Token(TokenKind.Comment, str, str, location);
        }

        public Token ReadToken()
        {
            PullSpaceOrTab();
            var val = _reader.Peek();
            if (val == -1)
            {
                return Token.EOF;
            }

            var ch = (char)val;
            if (char.IsLetter(ch) || ch == '_')
            {
                return ReadIdentity();
            }
            else if (char.IsDigit(ch))
            {
                return ReadNumber();
            }
            else if (ch == '.')
            {
                var location = Location;
                val = _reader.Peek(1);
                if (val == -1)
                {
                    _reader.Read();
                    return new Token(TokenKind.Symbol, ".", ".", location);
                }

                ch = (char)val;
                if (char.IsDigit(ch))
                {
                    return ReadNumber();
                }

                _reader.Read();
                return new Token(TokenKind.Symbol, ".", ".", location);
            }
            else if (ch == '"')
            {
                return ReadString();
            }
            else if (ch == '/')
            {
                val = _reader.Peek(1);
                if (val == -1)
                    throw new InvalidOperationException();

                var necr = (char)val;
                if (necr == '/' || necr == '*')
                {
                    return ReadComment();
                }
            }
            else if (ch == ';')
            {
                var location = Location;
                _reader.Read();
                return new Token(TokenKind.Symbol, ";", ";", location);
            }
            else if (ch == '{' || ch == '}' || ch == '[' || ch == ']' || ch == '(' || ch == ')' || ch == ':' || ch == ',')
            {
                var location = Location;
                _reader.Read();
                return new Token(TokenKind.Symbol, new string(ch, 1), new string(ch, 1), location);
            }
            else if (ch == '=')
            {
                var location = Location;
                _reader.Read();
                val = _reader.Peek();
                if (val == -1)
                    return new Token(TokenKind.Symbol, "=", "=", location);

                ch = (char)val;
                if (ch == '=')
                {
                    _reader.Read();
                    return new Token(TokenKind.Symbol, "==", "==", location);
                }

                return new Token(TokenKind.Symbol, "=", "=", location);
            }
            else if (ch == '\r')
            {
                val = _reader.Peek(1);
                ch = (char)val;
                if (ch == '\n')
                {
                    var location = Location;
                    _reader.Read();
                    _reader.Read();
                    return new Token(TokenKind.Symbol, Token.EolSymbol, Token.EolSymbol, location);
                }
            }

            throw new InvalidOperationException("Unknown char:" + ch);
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                try
                {
                    _reader.Dispose();

                    _reader = null;
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }
    }
}
