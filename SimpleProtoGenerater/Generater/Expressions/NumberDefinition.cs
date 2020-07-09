using System;
using SimpleProtoGenerater.Generater.Statements;
using SimpleProtoGenerater.Generater.Tokens;

namespace SimpleProtoGenerater.Generater.Expressions
{
    class NumberDefinition : NodeLeaf
    {
        public NumberDefinition(Token token)
            : base(token)
        { }

        public decimal Value => Convert.ToDecimal(Token.Value);
    }
}
