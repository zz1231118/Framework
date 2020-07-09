using SimpleProtoGenerater.Generater.Statements;
using SimpleProtoGenerater.Generater.Tokens;

namespace SimpleProtoGenerater.Generater.Expressions
{
    public class AuthorityDefinition : NodeLeaf
    {
        public AuthorityDefinition(Token token)
            : base(token)
        { }

        public string Authority => Token.Image;
    }
}
