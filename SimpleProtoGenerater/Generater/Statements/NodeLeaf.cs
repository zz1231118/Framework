using SimpleProtoGenerater.Generater.Tokens;

namespace SimpleProtoGenerater.Generater.Statements
{
    public class NodeLeaf : INode
    {
        private readonly Token _token;

        public NodeLeaf(Token token)
        {
            _token = token;
        }

        public Token Token => _token;
    }
}
