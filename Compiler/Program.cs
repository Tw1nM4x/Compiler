using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Options:");
                Console.WriteLine("  -help    Display help");
                return;
            }
            if (args.Contains("-help"))
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  dotnet run [file] [options]");
                Console.WriteLine("Options:");
                Console.WriteLine("  -l       lexical parser");
                Console.WriteLine("  -sp      simple expression parser");
                Console.WriteLine("  -p       parser (syntax analyzer)");
                return;
            }
            try
            {
                Lexer lexer = new Lexer(args[0]);
                if (args.Contains("-l"))
                {
                    Token token = lexer.GetNextToken();
                    Console.Write($"{token}\r\n");
                    while (token.Type != TokenType.Eof)
                    {
                        token = lexer.GetNextToken();
                        Console.Write($"{token}\r\n");
                    }
                }
            }
            catch(Exception e)
            {
                Console.Write($"ERROR: {e.Message}\r\n");
            }
            /*string? input = Console.ReadLine();
            if (input == "1" || input == "2" || input == "3")
            {
                Tester.StartTest(input);
            }
            if (input == "")
            {
                Console.WriteLine($"Введите имя файла (файл в формате .txt должен храниться в папке tests)");
                string? fileName = Console.ReadLine();
                string path = $"../../../tests/{fileName}.txt";
                if (!File.Exists(path))
                {
                    Console.WriteLine($"Такого файла не сущестует");
                    return;
                }
                Console.WriteLine($"Введите ключ");
                string? key = Console.ReadLine();
                switch (key)
                {
                    case "1":
                        SkillCompiler.OutputLexemeParsing(path, "console");
                        break;
                    case "2":
                        SkillCompiler.OutputSimpleExpressionsParsing(path, "console");
                        break;
                    case "3":
                        SkillCompiler.OutputSyntaxParsing(path, "console");
                        break;
                    default:
                        Console.WriteLine($"Такого ключа не существует");
                        break;
                }
                Console.WriteLine("\nЧтобы завершить программу нажмите Enter");
                Console.ReadLine();
            }*/
        }
    }
}
