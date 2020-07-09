using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Framework.Configuration.Hocon
{
    public class Tokenizer
    {
        private readonly string _text;
        private readonly Stack<int> _indexStack = new Stack<int>();
        private int _index;

        public Tokenizer(string text)
        {
            _text = text;
        }

        public bool EoF
        {
            get { return _index >= _text.Length; }
        }

        public void Push()
        {
            _indexStack.Push(_index);
        }
        public void Pop()
        {
            _index = _indexStack.Pop();
        }
        public bool Matches(string pattern)
        {
            if (pattern.Length + _index > _text.Length)
                return false;
            string selected = _text.Substring(_index, pattern.Length);
            if (selected == pattern)
                return true;

            return false;
        }
        public string Take(int length)
        {
            if (_index + length > _text.Length)
                return null;

            string s = _text.Substring(_index, length);
            _index += length;
            return s;
        }
        public bool Matches(params string[] patterns)
        {
            foreach (string pattern in patterns)
            {
                if (pattern.Length + _index >= _text.Length)
                    continue;

                if (_text.Substring(_index, pattern.Length) == pattern)
                    return true;
            }
            return false;
        }
        public char Peek()
        {
            if (EoF)
                return (char)0;

            return _text[_index];
        }
        public char Take()
        {
            if (EoF)
                return (char)0;

            return _text[_index++];
        }
        public void PullWhitespace()
        {
            while (!EoF && char.IsWhiteSpace(Peek()))
            {
                Take();
            }
        }
    }

    public class HoconTokenizer : Tokenizer
    {
        private const string NotInUnquotedKey = "$\"{}[]:=,#`^?!@*&\\.";
        private const string NotInUnquotedText = "$\"{}[]:=,#`^?!@*&\\";

        public HoconTokenizer(string text)
            : base(text)
        { }

        public void PullWhitespaceAndComments()
        {
            do
            {
                PullWhitespace();
                while (IsStartOfComment())
                {
                    PullComment();
                }
            } while (IsWhitespace());
        }
        public string PullRestOfLine()
        {
            var sb = new StringBuilder();
            while (!EoF)
            {
                char c = Take();
                if (c == '\n')
                    break;

                //ignore
                if (c == '\r')
                    continue;

                sb.Append(c);
            }
            return sb.ToString().Trim();
        }
        public Token PullNext()
        {
            PullWhitespaceAndComments();
            if (IsDot())
            {
                return PullDot();
            }
            if (IsObjectStart())
            {
                return PullStartOfObject();
            }
            if (IsEndOfObject())
            {
                return PullEndOfObject();
            }
            if (IsAssignment())
            {
                return PullAssignment();
            }
            if (IsInclude())
            {
                return PullInclude();
            }
            if (IsStartOfQuotedKey())
            {
                return PullQuotedKey();
            }
            if (IsUnquotedKeyStart())
            {
                return PullUnquotedKey();
            }
            if (IsArrayStart())
            {
                return PullArrayStart();
            }
            if (IsArrayEnd())
            {
                return PullArrayEnd();
            }
            if (EoF)
            {
                return new Token(TokenKind.EoF);
            }
            throw new FormatException("unknown token");
        }

        private bool IsStartOfQuotedKey()
        {
            return Matches("\"");
        }
        public Token PullArrayEnd()
        {
            Take();
            return new Token(TokenKind.ArrayEnd);
        }
        public bool IsArrayEnd()
        {
            return Matches("]");
        }
        public bool IsArrayStart()
        {
            return Matches("[");
        }
        public Token PullArrayStart()
        {
            Take();
            return new Token(TokenKind.ArrayStart);
        }
        public Token PullDot()
        {
            Take();
            return new Token(TokenKind.Dot);
        }
        public Token PullComma()
        {
            Take();
            return new Token(TokenKind.Comma);
        }
        public Token PullStartOfObject()
        {
            Take();
            return new Token(TokenKind.ObjectStart);
        }
        public Token PullEndOfObject()
        {
            Take();
            return new Token(TokenKind.ObjectEnd);
        }
        public Token PullAssignment()
        {
            Take();
            return new Token(TokenKind.Assign);
        }
        public bool IsComma()
        {
            return Matches(",");
        }
        public bool IsDot()
        {
            return Matches(".");
        }
        public bool IsObjectStart()
        {
            return Matches("{");
        }
        public bool IsEndOfObject()
        {
            return Matches("}");
        }
        public bool IsAssignment()
        {
            return Matches("=", ":");
        }
        public bool IsStartOfQuotedText()
        {
            return Matches("\"");
        }
        public bool IsStartOfTripleQuotedText()
        {
            return Matches("\"\"\"");
        }
        public Token PullComment()
        {
            PullRestOfLine();
            return new Token(TokenKind.Comment);
        }
        public Token PullUnquotedKey()
        {
            var sb = new StringBuilder();
            while (!EoF && IsUnquotedKey())
            {
                sb.Append(Take());
            }

            return Token.Key((sb.ToString().Trim()));
        }
        public bool IsUnquotedKey()
        {
            return (!EoF && !IsStartOfComment() && !NotInUnquotedKey.Contains(Peek()));
        }
        public bool IsUnquotedKeyStart()
        {
            return (!EoF && !IsWhitespace() && !IsStartOfComment() && !NotInUnquotedKey.Contains(Peek()));
        }

        public bool IsWhitespace()
        {
            return char.IsWhiteSpace(Peek());
        }

        public bool IsWhitespaceOrComment()
        {
            return IsWhitespace() || IsStartOfComment();
        }
        public Token PullTripleQuotedText()
        {
            var sb = new StringBuilder();
            Take(3);
            while (!EoF && !Matches("\"\"\""))
            {
                sb.Append(Peek());
                Take();
            }
            Take(3);
            return Token.LiteralValue(sb.ToString());
        }
        public Token PullQuotedText()
        {
            var sb = new StringBuilder();
            Take();
            while (!EoF && !Matches("\""))
            {
                if (Matches("\\"))
                {
                    sb.Append(PullEscapeSequence());
                }
                else
                {
                    sb.Append(Peek());
                    Take();
                }
            }
            Take();
            return Token.LiteralValue(sb.ToString());
        }
        public Token PullQuotedKey()
        {
            var sb = new StringBuilder();
            Take();
            while (!EoF && !Matches("\""))
            {
                if (Matches("\\"))
                {
                    sb.Append(PullEscapeSequence());
                }
                else
                {
                    sb.Append(Peek());
                    Take();
                }
            }
            Take();
            return Token.Key(sb.ToString());
        }

        public Token PullInclude()
        {
            Take("include".Length);
            PullWhitespaceAndComments();
            var rest = PullQuotedText();
            var unQuote = rest.Value;
            return Token.Include(unQuote);
        }

        private string PullEscapeSequence()
        {
            Take();
            char escaped = Take();
            switch (escaped)
            {
                case '"':
                    return ("\"");
                case '\\':
                    return ("\\");
                case '/':
                    return ("/");
                case 'b':
                    return ("\b");
                case 'f':
                    return ("\f");
                case 'n':
                    return ("\n");
                case 'r':
                    return ("\r");
                case 't':
                    return ("\t");
                case 'u':
                    string hex = "0x" + Take(4);
                    int j = Convert.ToInt32(hex, 16);
                    return ((char)j).ToString(CultureInfo.InvariantCulture);
                default:
                    throw new NotSupportedException(string.Format("Unknown escape code: {0}", escaped));
            }
        }

        public bool IsStartOfComment()
        {
            return (Matches("#", "//"));
        }
        public Token PullValue()
        {
            if (IsObjectStart())
            {
                return PullStartOfObject();
            }

            if (IsStartOfTripleQuotedText())
            {
                return PullTripleQuotedText();
            }

            if (IsStartOfQuotedText())
            {
                return PullQuotedText();
            }

            if (IsUnquotedText())
            {
                return PullUnquotedText();
            }
            if (IsArrayStart())
            {
                return PullArrayStart();
            }
            if (IsArrayEnd())
            {
                return PullArrayEnd();
            }
            if (IsSubstitutionStart())
            {
                return PullSubstitution();
            }

            throw new FormatException(
                "Expected value: Null literal, Array, Quoted Text, Unquoted Text, Triple quoted Text, Object or End of array");
        }
        public bool IsSubstitutionStart()
        {
            return Matches("${");
        }

        public bool IsInclude()
        {
            Push();
            try
            {
                if (Matches("include"))
                {
                    Take("include".Length);

                    if (IsWhitespaceOrComment())
                    {
                        PullWhitespaceAndComments();

                        if (IsStartOfQuotedText())
                        {
                            PullQuotedText();
                            return true;
                        }
                    }
                }
                return false;
            }
            finally
            {
                Pop();
            }
        }
        public Token PullSubstitution()
        {
            var sb = new StringBuilder();
            Take(2);
            while (!EoF && IsUnquotedText())
            {
                sb.Append(Take());
            }
            Take();
            return Token.Substitution(sb.ToString());
        }
        public bool IsSpaceOrTab()
        {
            return Matches(" ", "\t");
        }
        public bool IsStartSimpleValue()
        {
            if (IsSpaceOrTab())
                return true;

            if (IsUnquotedText())
                return true;

            return false;
        }
        public Token PullSpaceOrTab()
        {
            var sb = new StringBuilder();
            while (IsSpaceOrTab())
            {
                sb.Append(Take());
            }
            return Token.LiteralValue(sb.ToString());
        }

        private Token PullUnquotedText()
        {
            var sb = new StringBuilder();
            while (!EoF && IsUnquotedText())
            {
                sb.Append(Take());
            }

            return Token.LiteralValue(sb.ToString());
        }

        private bool IsUnquotedText()
        {
            return (!EoF && !IsWhitespace() && !IsStartOfComment() && !NotInUnquotedText.Contains(Peek()));
        }
        public Token PullSimpleValue()
        {
            if (IsSpaceOrTab())
                return PullSpaceOrTab();
            if (IsUnquotedText())
                return PullUnquotedText();

            throw new FormatException("No simple value found");
        }
        internal bool IsValue()
        {
            if (IsArrayStart())
                return true;
            if (IsObjectStart())
                return true;
            if (IsStartOfTripleQuotedText())
                return true;
            if (IsSubstitutionStart())
                return true;
            if (IsStartOfQuotedText())
                return true;
            if (IsUnquotedText())
                return true;

            return false;
        }
    }
}