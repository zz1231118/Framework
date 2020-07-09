using System;
using System.Collections.Generic;
using SimpleProtoGenerater.Generater.Statements;
using SimpleProtoGenerater.Generater.Tokens;

namespace SimpleProtoGenerater.Generater
{
    public class Parser
    {
        private readonly List<Element> _elements = new List<Element>();
        private Factory _factory;

        public static Parser Rule()
        {
            return new Parser();
        }
        public static Parser Rule<T>()
            where T : INode
        {
            return new Parser() { _factory = Factory.Create<T>() };
        }
        public INode Parse(Tokenizer lexer)
        {
            var context = new ParserContext();
            foreach (var element in _elements)
            {
                element.Parse(lexer, context);
            }

            return _factory.Make(context.Nodes);
        }
        public T Parse<T>(Tokenizer lexer)
            where T : INode
        {
            return (T)Parse(lexer);
        }
        public Parser Number<T>()
            where T : INode
        {
            _elements.Add(new NumberToken(typeof(T)));
            return this;
        }
        public Parser Access<T>()
            where T : INode
        {
            _elements.Add(new IdentityToken(typeof(T)));
            return this;
        }
        public Parser Sep(params string[] pats)
        {
            _elements.Add(new Skip(pats));
            return this;
        }
        public Parser Ast(Parser parser)
        {
            _elements.Add(new Tree(parser));
            return this;
        }
        public Parser Or(params Parser[] parsers)
        {
            _elements.Add(new OrTree(parsers));
            return this;
        }
        /// <summary>
        /// 零次至一次
        /// </summary>
        public Parser Option(Parser parser)
        {
            _elements.Add(new RepeatElement(parser, true));
            return this;
        }
        /// <summary>
        /// 零次至多次
        /// </summary>
        public Parser Repeat(Parser parser)
        {
            _elements.Add(new RepeatElement(parser, false));
            return this;
        }
        public Parser Identity(params string[] symbols)
        {
            _elements.Add(new Leaf(symbols));
            return this;
        }
        public bool Match(Tokenizer lexer)
        {
            if (_elements.Count >= 0)
            {
                return _elements[0].Match(lexer);
            }
            return true;
        }

        private void Parse(Tokenizer lexer, ParserContext context)
        {
            var internalContext = _factory == null ? context : new ParserContext();
            foreach (var element in _elements)
            {
                element.Parse(lexer, internalContext);
            }
            if (_factory != null)
            {
                var node = _factory.Make(internalContext.Nodes);
                context.Add(node);
            }
        }

        class ParserContext
        {
            private readonly List<INode> _nodes = new List<INode>();

            public IReadOnlyList<INode> Nodes => _nodes;

            public void Add(INode node)
            {
                _nodes.Add(node);
            }
        }
        abstract class Factory
        {
            public static readonly Factory Default = new MergeFactory();

            public static Factory Create(Type ft)
            {
                if (ft == null)
                    throw new ArgumentNullException(nameof(ft));

                return typeof(NodeLeaf).IsAssignableFrom(ft)
                    ? (Factory)new LeafFactory(ft)
                    : (Factory)new TreeFactory(ft);
            }
            public static Factory Create<T>()
                where T : INode
            {
                return Create(typeof(T));
            }

            public abstract INode Make(IReadOnlyList<INode> nodes);

            class LeafFactory : Factory
            {
                private readonly Type _nt;

                public LeafFactory(Type nt)
                {
                    _nt = nt;
                }

                public override INode Make(IReadOnlyList<INode> nodes)
                {
                    if (nodes.Count != 1)
                        throw new CompileException();

                    var leaf = nodes[0] as NodeLeaf;
                    var objParams = new object[] { leaf.Token };
                    return (INode)Activator.CreateInstance(_nt, objParams);
                }
            }
            class TreeFactory : Factory
            {
                private readonly Type _nt;

                public TreeFactory(Type nt)
                {
                    _nt = nt;
                }

                public override INode Make(IReadOnlyList<INode> nodes)
                {
                    var objParams = new object[] { nodes };
                    return (INode)Activator.CreateInstance(_nt, objParams);
                }
            }
            class MergeFactory : Factory
            {
                private IEnumerable<INode> Resolve(INode node)
                {
                    if (node is NodeLeaf)
                    {
                        yield return node;
                    }
                    else
                    {
                        foreach (var nd in ((NodeTree)node).Nodes)
                        {
                            foreach (var n in Resolve(nd))
                                yield return n;
                        }
                    }
                }
                public override INode Make(IReadOnlyList<INode> nodes)
                {
                    var list = new List<INode>();
                    foreach (var node in nodes)
                    {
                        foreach (var nd in Resolve(node))
                            list.Add(nd);
                    }

                    return new NodeTree(list);
                }
            }
        }
        abstract class Element
        {
            public abstract bool Match(Tokenizer lexer);
            public abstract void Parse(Tokenizer lexer, ParserContext context);
        }
        class Tree : Element
        {
            private readonly Parser _parser;

            public Tree(Parser parser)
            {
                _parser = parser;
            }

            public override bool Match(Tokenizer lexer)
            {
                return _parser.Match(lexer);
            }
            public override void Parse(Tokenizer lexer, ParserContext context)
            {
                _parser.Parse(lexer, context);
            }
        }
        class OrTree : Element
        {
            private readonly Parser[] _parsers;

            public OrTree(Parser[] parsers)
            {
                _parsers = parsers;
            }

            protected Parser Find(Tokenizer lexer)
            {
                foreach (var p in _parsers)
                {
                    if (p.Match(lexer))
                        return p;
                }

                return null;
            }
            public override bool Match(Tokenizer lexer)
            {
                return Find(lexer) != null;
            }
            public override void Parse(Tokenizer lexer, ParserContext context)
            {
                Parser parser = Find(lexer);
                if (parser == null)
                    throw new CompileException();

                parser.Parse(lexer, context);
            }
        }
        class RepeatElement : Element
        {
            private readonly Parser _parser;
            private readonly bool _onlyOnce;

            public RepeatElement(Parser parser, bool onlyOnce)
            {
                _parser = parser;
                _onlyOnce = onlyOnce;
            }

            public override bool Match(Tokenizer lexer)
            {
                return _parser.Match(lexer);
            }
            public override void Parse(Tokenizer lexer, ParserContext context)
            {
                while (_parser.Match(lexer))
                {
                    _parser.Parse(lexer, context);
                    if (_onlyOnce)
                        break;
                }
            }
        }
        abstract class TokenElement : Element
        {
            private readonly Factory _factory;

            public TokenElement(Type ft)
            {
                _factory = Factory.Create(ft);
            }

            protected abstract bool Test(Token token);

            public override bool Match(Tokenizer lexer)
            {
                var token = lexer.Peek();
                return Test(token);
            }
            public override void Parse(Tokenizer lexer, ParserContext context)
            {
                Token token = lexer.Read();
                if (!Test(token))
                    throw new CompileException(token);

                var node = _factory.Make(new List<INode>() { new NodeLeaf(token) });
                context.Add(node);
            }
        }
        class IdentityToken : TokenElement
        {
            public IdentityToken(Type ft)
                : base(ft)
            { }

            protected override bool Test(Token token)
            {
                return token.Kind == TokenKind.Identity;
            }
        }
        class NumberToken : TokenElement
        {
            public NumberToken(Type ft)
                : base(ft)
            { }

            protected override bool Test(Token token)
            {
                return token.Kind == TokenKind.Number;
            }
        }
        class Leaf : Element
        {
            private readonly string[] _tokens;

            public Leaf(string[] pat)
            {
                _tokens = pat;
            }

            protected virtual void AddToContext(ParserContext context, Token token)
            {
                context.Add(new NodeLeaf(token));
            }
            public override bool Match(Tokenizer lexer)
            {
                var token = lexer.Peek();
                if (token.Kind == TokenKind.Identity ||
                    token.Kind == TokenKind.Symbol)
                {
                    foreach (var symbol in _tokens)
                    {
                        if (token.Image.Equals(symbol))
                            return true;
                    }
                }

                return false;
            }
            public override void Parse(Tokenizer lexer, ParserContext context)
            {
                var token = lexer.Read();
                if (token.Kind == TokenKind.Identity ||
                    token.Kind == TokenKind.Symbol)
                {
                    foreach (var symbol in _tokens)
                    {
                        if (token.Image.Equals(symbol))
                        {
                            AddToContext(context, token);
                            return;
                        }
                    }
                }

                throw new CompileException(token);
            }
        }
        class Skip : Leaf
        {
            public Skip(string[] pat)
                : base(pat)
            { }

            protected override void AddToContext(ParserContext context, Token token)
            { }
        }
    }
}
