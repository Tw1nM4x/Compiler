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
                if (args[1] == "-spar")
                {
                    SimpleParser sParser = new SimpleParser(lexer);
                    Node firstNode = sParser.ParseExpression();
                    if (lexer.GetLastToken().Type != TokenType.Eof)
                    {
                        throw new ExceptionWithPosition(lexer.CurrentLine, lexer.CurrentSymbol - 1, "expected operation sign");
                    }
                    Console.Write(firstNode.ToString(new List<bool>()) + "\r\n");
                }
                if (args[1] == "-par")
                {
                    try
                    {
                        //Parser parser = new Parser(lexer);
                        //Node firstNode = parser.ParseProgram();
                        //Console.Write(firstNode.ToString(new List<bool>()) + "\r\n");
                    }
                    catch (ExceptionWithPosition ex)
                    {
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        throw new ExceptionWithPosition(lexer.CurrentLine, lexer.CurrentSymbol - 1, ex.Message);
                    }
                }
            }
            catch (ExceptionWithPosition e)
            {
                Console.Write($"{e}\r\n");
            }
        }
    }
}
