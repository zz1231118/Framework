using SimpleProtoGenerater.Generater.Statements;
using SimpleProtoGenerater.Generater.Tokens;

namespace SimpleProtoGenerater.Generater.Expressions
{
    class TypeDefinition : NodeLeaf
    {
        public TypeDefinition(Token token)
            : base(token)
        { }

        public string Type => Token.Image;
    }
}
