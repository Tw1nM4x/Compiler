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
                Console.WriteLine("  dotnet run [options]");
                Console.WriteLine("Options:");
                Console.WriteLine("  -l       lexical parser");
                Console.WriteLine("  -sp      simple expression parser");
                Console.WriteLine("  -p       parser (syntax analyzer)");
                return;
            }
            Tester.StartTest(args[0]);
        }
    }
}
