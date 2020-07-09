using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Configuration.Hocon
{
    public class Parser
    {
        private readonly List<HoconSubstitution> _substitutions = new List<HoconSubstitution>();
        private HoconTokenizer _reader;
        private HoconValue _root;
        private Func<string, HoconRoot> _includeCallback;

        public static HoconRoot Parse(string text, Func<string, HoconRoot> includeCallback)
        {
            return new Parser().ParseText(text, includeCallback);
        }

        private HoconRoot ParseText(string text, Func<string, HoconRoot> includeCallback)
        {
            _includeCallback = includeCallback;
            _root = new HoconValue();
            _reader = new HoconTokenizer(text);
            _reader.PullWhitespaceAndComments();
            ParseObject(_root, true, "");

            var c = new Config(new HoconRoot(_root, Enumerable.Empty<HoconSubstitution>()));
            foreach (HoconSubstitution sub in _substitutions)
            {
                HoconValue res = c.GetValue(sub.Path);
                if (res == null)
                    throw new FormatException("Unresolved substitution:" + sub.Path);
                sub.ResolvedValue = res;
            }
            return new HoconRoot(_root, _substitutions);
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
                        var included = _includeCallback(t.Value);
                        var substitutions = included.Substitutions;
                        foreach (var substitution in substitutions)
                        {
                            substitution.Path = currentPath + "." + substitution.Path;
                        }
                        _substitutions.AddRange(substitutions);
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
                        var lit = new HoconLiteral
                        {
                            Value = t.Value
                        };
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
                        _substitutions.Add(sub);
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
                var wsLit = new HoconLiteral
                {
                    Value = ws.Value,
                };
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