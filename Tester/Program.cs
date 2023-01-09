using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler;

namespace Tester
{
    static class Menu
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Options:");
                Console.WriteLine("  -help     Display help");
                return;
            }
            if (args.Contains("-help"))
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  dotnet run [option] [addition]");
                Console.WriteLine("Options:");
                Console.WriteLine("  -l       Lexical parser");
                Console.WriteLine("  -sp      Simple expression parser");
                Console.WriteLine("  -p       Parser (syntax analyzer)");
                Console.WriteLine("  -sa      Semantic analysis");
                Console.WriteLine("Additions:");
                Console.WriteLine("  -detail    view detail tests");
                return;
            }
            Tester.StartTest(args[0], args.Length == 2? args[1] : "-null");
        }
    }
}
