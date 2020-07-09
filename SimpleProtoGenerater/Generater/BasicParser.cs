using System.Collections.Generic;
using System.Linq;
using SimpleProtoGenerater.Emit;
using SimpleProtoGenerater.Generater.Expressions;
using SimpleProtoGenerater.Generater.Tokens;

namespace SimpleProtoGenerater.Generater
{
    public static class BasicParser
    {
        private static Parser _namespace;
        private static Parser _imports;
        private static Parser _assembly;

        public static void Initialize()
        {
            var newline = Parser.Rule().Sep(Token.EolSymbol);
            var types = new List<string> { "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong" };
            types.ToList().ForEach(p => types.Add(SystemAssembly.GetType(p).Struct));
            var field = Parser.Rule<FieldDefinition>().Access<AccessDefinition>().Sep("=").Number<NumberDefinition>().Sep(";");
            var enumBase = Parser.Rule().Sep(":").Identity(types.ToArray());
            var enumParser = Parser.Rule<EnumDefinition>().Sep("enum").Access<AccessDefinition>().Option(enumBase).Repeat(newline).Sep("{").Repeat(newline).Repeat(field.Repeat(newline)).Sep("}");

            var array = Parser.Rule<ArrayDefinition>().Sep("[").Sep("]");
            var property = Parser.Rule<PropertyDefinition>().Identity("optional", "required", "repeated").Access<TypeDefinition>().Option(array).Access<AccessDefinition>().Sep("=").Number<NumberDefinition>().Sep(";");
            var messageParser = Parser.Rule<MessageDefinition>();
            var messageMemberParser = Parser.Rule().Or(property, enumParser, messageParser);
            var messageParent = Parser.Rule<TypeDefinition>().Sep(":").Access<AccessDefinition>();
            messageParser.Sep("message").Access<AccessDefinition>().Option(messageParent).Repeat(newline).Sep("{").Repeat(newline).Repeat(messageMemberParser.Repeat(newline)).Sep("}");

            //rpc Login (LoginRequest)	returns (LoginResponse);
            var serviceParent = Parser.Rule().Sep(":").Access<TypeDefinition>().Repeat(Parser.Rule().Sep(",").Access<TypeDefinition>());
            var parameters = Parser.Rule<ParametersDefinition>().Option(Parser.Rule().Access<TypeDefinition>().Repeat(Parser.Rule().Sep(",").Access<TypeDefinition>()));
            var returns = Parser.Rule<ReturnsDefinition>().Option(Parser.Rule().Access<TypeDefinition>().Repeat(Parser.Rule().Sep(",").Access<TypeDefinition>()));
            var method = Parser.Rule<MethodDefinition>().Sep("rpc").Access<AccessDefinition>().Sep("(").Ast(parameters).Sep(")").Sep("returns").Sep("(").Ast(returns).Sep(")").Sep(";");
            var serviceParser = Parser.Rule<ServiceDefinition>().Sep("service").Access<AccessDefinition>().Option(serviceParent).Repeat(newline).Sep("{").Repeat(newline).Repeat(method.Repeat(newline)).Sep("}");

            var import = Parser.Rule<ImportDefinition>().Sep("import").Access<AccessDefinition>().Repeat(Parser.Rule().Sep(".").Access<AccessDefinition>()).Sep(";");
            _imports = Parser.Rule<ImportsDefinition>().Repeat(Parser.Rule().Or(newline, import));
            _namespace = Parser.Rule<NamespaceDefinition>().Sep("package").Access<AccessDefinition>().Repeat(Parser.Rule().Sep(".").Access<AccessDefinition>()).Sep(";");
            _assembly = Parser.Rule<AssemblyDefinition>().Repeat(newline).Repeat(Parser.Rule().Or(enumParser, messageParser, serviceParser).Repeat(newline));
        }

        public static AssemblyBuilder Parse(Tokenizer lexer)
        {
            NamespaceDefinition nd = null;
            ImportsDefinition isd = null;
            if (_namespace.Match(lexer))
            {
                nd = _namespace.Parse<NamespaceDefinition>(lexer);
            }
            if (_imports.Match(lexer))
            {
                isd = _imports.Parse<ImportsDefinition>(lexer);
            }

            AssemblyDefinition ad = _assembly.Parse(lexer) as AssemblyDefinition;
            return ad.Build(isd?.Imports.Select(p => p.Import), nd?.Namespace);
        }
    }
}
