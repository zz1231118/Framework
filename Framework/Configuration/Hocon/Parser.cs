using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Configuration.Hocon
{
    internal class Parser
    {
        private readonly List<HoconSubstitution> substitutions = new List<HoconSubstitution>();
        private readonly HoconValue _root = new HoconValue();
        private readonly HoconTokenizer _reader;
        private readonly Func<string, HoconRoot>? _includeCallback;

        public Parser(HoconTokenizer reader, Func<string, HoconRoot>? includeCallback)
        {
            this._reader = reader;
            this._includeCallback = includeCallback;
        }

        public static HoconRoot Parse(string text, Func<string, HoconRoot>? includeCallback)
        {
            var reader = new HoconTokenizer(text);
            return new Parser(reader, includeCallback).Parse();
        }

        private HoconRoot Parse()
        {
            _reader.PullWhitespaceAndComments();
            ParseObject(_root, true, "");

            var config = new Config(new HoconRoot(_root, Enumerable.Empty<HoconSubstitution>()));
            foreach (HoconSubstitution sub in substitutions)
            {
                var res = config.GetValue(sub.Path);
                if (res == null)
                    throw new FormatException("Unresolved substitution:" + sub.Path);

                sub.ResolvedValue = res;
            }
            return new HoconRoot(_root, substitutions);
        }

        private void ParseObject(HoconValue owner, bool root, string currentPath)
        {
            if (!owner.IsObject())
                owner.NewValue(new HoconObject());

            HoconObject currentObject = owner.GetObject();
            while (!_reader.EoF)
            {
                Token t = _reader.PullNext();
                switch (t.Kind)
                {
                    case TokenKind.Include:
                        if (_includeCallback == null)
                        {
                            throw new InvalidOperationException("include callback is null");
                        }

                        var included = _includeCallback(t.Value);
                        var substitutions = included.Substitutions;
                        foreach (var substitution in substitutions)
                        {
                            substitution.Path = currentPath + "." + substitution.Path;
                        }
                        this.substitutions.AddRange(substitutions);
                        var otherObj = included.Value.GetObject();
                        owner.GetObject().Merge(otherObj);

                        break;
                    case TokenKind.EoF:
                        break;
                    case TokenKind.Key:
                        HoconValue value = currentObject.GetOrCreateKey(t.Value);
                        var nextPath = currentPath == "" ? t.Value : currentPath + "." + t.Value;
                        ParseKeyContent(value, nextPath);
                        if (!root)
                            return;
                        break;

                    case TokenKind.ObjectEnd:
                        return;
                }
            }
        }

        private void ParseKeyContent(HoconValue value, string currentPath)
        {
            while (!_reader.EoF)
            {
                Token t = _reader.PullNext();
                switch (t.Kind)
                {
                    case TokenKind.Dot:
                        ParseObject(value, false, currentPath);
                        return;
                    case TokenKind.Assign:
                        if (!value.IsObject())
                        {
                            value.Clear();
                        }
                        ParseValue(value, currentPath);
                        return;
                    case TokenKind.ObjectStart:
                        ParseObject(value, true, currentPath);
                        return;
                }
            }
        }

        public void ParseValue(HoconValue owner, string currentPath)
        {
            if (_reader.EoF)
                throw new Exception("End of file reached while trying to read a value");

            _reader.PullWhitespaceAndComments();
            while (_reader.IsValue())
            {
                Token t = _reader.PullValue();
                switch (t.Kind)
                {
                    case TokenKind.EoF:
                        break;
                    case TokenKind.LiteralValue:
                        if (owner.IsObject())
                        {
                            owner.Clear();
                        }

                        var lit = new HoconLiteral(t.Value);
                        owner.AppendValue(lit);
                        break;
                    case TokenKind.ObjectStart:
                        ParseObject(owner, true, currentPath);
                        break;
                    case TokenKind.ArrayStart:
                        HoconArray arr = ParseArray(currentPath);
                        owner.AppendValue(arr);
                        break;
                    case TokenKind.Substitute:
                        HoconSubstitution sub = ParseSubstitution(t.Value);
                        substitutions.Add(sub);
                        owner.AppendValue(sub);
                        break;
                }
                if (_reader.IsSpaceOrTab())
                {
                    ParseTrailingWhitespace(owner);
                }
            }

            IgnoreComma();
        }

        private void ParseTrailingWhitespace(HoconValue owner)
        {
            Token ws = _reader.PullSpaceOrTab();
            if (ws.Value.Length > 0)
            {
                var wsLit = new HoconLiteral(ws.Value);
                owner.AppendValue(wsLit);
            }
        }

        private static HoconSubstitution ParseSubstitution(string value)
        {
            return new HoconSubstitution(value);
        }

        public HoconArray ParseArray(string currentPath)
        {
            var arr = new HoconArray();
            while (!_reader.EoF && !_reader.IsArrayEnd())
            {
                var v = new HoconValue();
                ParseValue(v, currentPath);
                arr.Add(v);
                _reader.PullWhitespaceAndComments();
            }
            _reader.PullArrayEnd();
            return arr;
        }

        private void IgnoreComma()
        {
            if (_reader.IsComma())
            {
                _reader.PullComma();
            }
        }
    }
}