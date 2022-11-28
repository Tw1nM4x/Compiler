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
                Console.WriteLine("  dotnet run [file] [option]");
                Console.WriteLine("Options:");
                Console.WriteLine("  -l       Lexical parser");
                Console.WriteLine("  -spar    Simple expression parser");
                Console.WriteLine("  -par     Parser (syntax analyzer)");
                return;
            }
            try
            {
                Lexer lexer = new Lexer(args[0]);
                if (args[1] == "-l")
                {
                    Token token = lexer.GetNextToken();
                    Console.Write($"{token}\r\n");
                    while (token.Type != TokenType.Eof)
                    {
                        token = lexer.GetNextToken();
                        Console.Write($"{token}\r\n");
                    }
                }
                if (args[1] == "-par" || args[1] == "-spar")
                {
                    Parser parser = new Parser(lexer);
                    Node firstNode;
                    if (args[1] == "-par")
                    {
                        firstNode = parser.ParseProgram(isMain: true);
                    }
                    else
                    {
                        firstNode = parser.ParseSimpleExpression();
                        if(lexer.LastToken.Type != TokenType.Eof)
                        {
                            throw new ExceptionWithPosition(lexer.CurrentLine, lexer.CurrentSymbol, "expected operation sign");
                        }
                    }
                    Console.Write(firstNode.ToString(new List<bool>()) + "\r\n");
                }
            }
            catch(ExceptionWithPosition e)
            {
                Console.Write($"{e}\r\n");
            }
        }
    }
}
