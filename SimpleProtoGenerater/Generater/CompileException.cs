using System;
using SimpleProtoGenerater.Generater.Tokens;

namespace SimpleProtoGenerater.Generater
{
    public class CompileException : Exception
    {
        private readonly Token _token;

        public CompileException()
        { }
        public CompileException(Token token)
            : this(token, string.Format("token:{0} row:{1} column:{2}", token.Image, token.Location.Row, token.Location.Column))
        { }
        public CompileException(Token token, string message)
            : base(message)
        {
            _token = token;
        }
        public CompileException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public Token Token => _token;
        public SourceLocation Location => _token?.Location ?? SourceLocation.Empty;
    }
}
