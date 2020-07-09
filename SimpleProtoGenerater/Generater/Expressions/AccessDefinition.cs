using SimpleProtoGenerater.Generater.Statements;
using SimpleProtoGenerater.Generater.Tokens;

namespace SimpleProtoGenerater.Generater.Expressions
{
    class AccessDefinition : NodeLeaf
    {
        public AccessDefinition(Token token)
            : base(token)
        { }

        public string Name => Token.Image;
    }
}
