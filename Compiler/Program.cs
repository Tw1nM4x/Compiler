using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                Console.WriteLine("  -sa      Semantic analysis");
                Console.WriteLine("  -gen     Code generator");
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
                        Parser parser = new Parser(lexer);
                        Node firstNode = parser.ParseMainProgram();
                        Console.Write(firstNode.ToString(new List<bool>()) + "\r\n");
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
                if (args[1] == "-sa")
                {
                    try
                    {
                        Parser parser = new Parser(lexer);
                        Node firstNode = parser.ParseMainProgram();
                        Console.Write(firstNode.ToString(new List<bool>()) + "\r\n");
                        Console.Write(parser.PrintSymTable());
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
                if (args[1] == "-gen")
                {
                    try
                    {
                        Parser parser = new Parser(lexer);
                        Node firstNode = parser.ParseMainProgram();
                        string pathOut = @"D:/GitProjects/Compiler/Tester/tests/1.asm";
                        Generator generator = new Generator(pathOut);
                        using (StreamWriter sw = new StreamWriter(pathOut, false, Encoding.Default))
                        {
                            sw.Write("");
                        }
                        firstNode.Generate(generator);
                    }
                    catch (ExceptionWithPosition ex)
                    {
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        throw new ExceptionWithPosition(lexer.CurrentLine, lexer.CurrentSymbol - 1, ex.Message);
                    }

                    Process nasmProcess = new Process();
                    nasmProcess.StartInfo.FileName = "nasm";
                    nasmProcess.StartInfo.Arguments = "-f win32 D:/GitProjects/Compiler/Tester/tests/1.asm -o D:/GitProjects/Compiler/Tester/tests/1.obj";
                    nasmProcess.Start();
                    nasmProcess.WaitForExit();

                    Process golinkProcess = new Process();
                    golinkProcess.StartInfo.FileName = "gcc";
                    golinkProcess.StartInfo.Arguments = "-m32 -mconsole D:/GitProjects/Compiler/Tester/tests/1.obj -o D:/GitProjects/Compiler/Tester/tests/1.exe";
                    golinkProcess.Start();
                    golinkProcess.WaitForExit();
                    new FileInfo("D:/GitProjects/Compiler/Tester/tests/1.obj").Delete();

                    Process exeProcess = new Process();
                    exeProcess.StartInfo.FileName = "D:/GitProjects/Compiler/Tester/tests/1.exe";
                    //exeProcess.StartInfo.UseShellExecute = true;
                    exeProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    exeProcess.Start();
                }
            }
            catch (ExceptionWithPosition e)
            {
                Console.Write($"{e}\r\n");
            }
        }
    }
}
