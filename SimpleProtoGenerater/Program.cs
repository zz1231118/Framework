using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SimpleProtoGenerater.Emit;
using SimpleProtoGenerater.Generater;
using SimpleProtoGenerater.Generater.Tokens;
using SimpleProtoGenerater.Utility;

namespace SimpleProtoGenerater
{
    class Program
    {
        static void Main(string[] args)
        {
            var wapper = new ParamWrapper(args);
            var ins = wapper.GetCommand("in").Select(p => p.IndexOf(":") >= 0 ? p : Path.Combine(Environment.CurrentDirectory, p)).ToList();
            var outs = wapper.GetCommand("out").Select(p => p.IndexOf(":") >= 0 ? p : Path.Combine(Environment.CurrentDirectory, p)).ToList();
            if (ins.Count == 0)
                ErrorAndExit("error: in empty");
            if (outs.Count == 0)
                ErrorAndExit("error: out empty");

            try
            {
                var outPath = outs.First();
                BasicParser.Initialize();
                var assemblys = new List<AssemblyBuilder>();
                foreach (var @in in ins)
                {
                    Console.WriteLine("正在编译：{0}", @in);
                    using (var reader = new TokenReader(new StreamReader(File.OpenRead(@in), Encoding.UTF8)))
                    {
                        var lexer = new Tokenizer(reader);
                        var assembly = BasicParser.Parse(lexer);
                        assembly.AddReference(typeof(Framework.Runtime.Serialization.Protobuf.ISerializable).Namespace);

                        var sb = new StringBuilder();
                        assembly.BuildCode(sb);
                        var script = sb.ToString();
                        var name = Path.GetFileNameWithoutExtension(@in);
                        var path = Path.Combine(outPath, name + ".cs");
                        File.WriteAllText(path, script, Encoding.UTF8);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorAndExit(ex.ToString());
            }
        }

        private static void ErrorAndExit(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("error: " + message);
            Environment.Exit(1);
        }
    }
}
