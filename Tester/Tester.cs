using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler;

namespace Tester
{
    class Tester
    {
        struct Folder
        {
            public string name;
            public int countTest;
            public Folder(string name = "string", int countTest = 0)
            {
                this.name = name;
                this.countTest = countTest;
            }
        }
        static Folder[] folders = new Folder[] { };

        public static void StartTest(string key, string addition = "-null") 
        {
            switch (key)
            {
                case "-l":
                    folders = new Folder[] { new("lexer/string", 14), new("lexer/identifier", 9), new("lexer/integer", 19), new("lexer/real", 28), new("lexer/space", 2),
                        new("lexer/comment", 9), new("lexer/key word", 4), new("lexer/operation sign", 3), new("lexer/separator", 3), new("lexer/errors", 3) };
                    break;
                case "-spar":
                    folders = new Folder[] { new("simple expressions", 14) };
                    break;
                case "-par":
                    folders = new Folder[] { new("parser/defs", 8), new("parser/composite data types", 5), new("parser/scalar data types", 6),
                        new("parser/control structures", 7), new("parser/procedures", 3) };
                    break;
                case "-sa":
                    folders = new Folder[] { new("semantic analysis", 24) };
                    break;
            }
            int countOK = 0;
            int countERROR = 0;
            if(addition == "-detail")
            {
                StartDetailTest(key);
                return;
            }
            for (int numberFolder = 0; numberFolder < folders.Length; numberFolder++)
            {
                Console.WriteLine($"----------{folders[numberFolder].name}----------");
                for (int numberTest = 1; numberTest <= folders[numberFolder].countTest; numberTest++)
                {
                    string numberTestStr = numberTest.ToString();
                    if (numberTestStr.Length < 2)
                    {
                        numberTestStr = "0" + numberTestStr;
                    }
                    string pathIn = Environment.CurrentDirectory + $"/tests/{folders[numberFolder].name}/" + $"{numberTestStr}_input.txt";
                    string pathOut = Environment.CurrentDirectory + $"/tests/{folders[numberFolder].name}/" + $"{numberTestStr}_out.txt";
                    string pathCheck = Environment.CurrentDirectory + $"/tests/{folders[numberFolder].name}/" + $"{numberTestStr}_correct.txt";
                    Lexer lexer = new Lexer(pathIn);
                    if(key == "-l")
                    {
                        try
                        {
                            using (StreamWriter sw = new StreamWriter(pathOut, false, Encoding.Default))
                            {
                                Token token = lexer.GetNextToken();
                                sw.Write($"{token}\r\n");
                                while (token.Type != TokenType.Eof)
                                {
                                    token = lexer.GetNextToken();
                                    sw.Write($"{token}\r\n");
                                }
                            }
                        }
                        catch (ExceptionWithPosition ex)
                        {
                            using (StreamWriter sw = new StreamWriter(pathOut, false, Encoding.Default))
                            {
                                sw.Write($"{ex}\r\n");
                            }
                        }
                    }
                    if(key == "-spar" || key == "-par")
                    {
                        try
                        {
                            Node firstNode = new Node();
                            if (key == "-spar")
                            {
                                SimpleParser sParser = new SimpleParser(lexer);
                                firstNode = sParser.ParseExpression();
                                if (lexer.GetLastToken().Type != TokenType.Eof)
                                {
                                    throw new ExceptionWithPosition(lexer.CurrentLine, lexer.CurrentSymbol - 1, "expected operation sign");
                                }
                            }
                            if (key == "-par")
                            {
                                try
                                {
                                    Parser parser = new Parser(lexer);
                                    firstNode = parser.ParseMainProgram();
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
                            using (StreamWriter sw = new StreamWriter(pathOut, false, Encoding.Default))
                            {
                                sw.Write(firstNode.ToString(new List<bool>()) + "\r\n");
                            }
                        }
                        catch (ExceptionWithPosition ex)
                        {
                            using (StreamWriter sw = new StreamWriter(pathOut, false, Encoding.Default))
                            {
                                sw.Write($"{ex}\r\n");
                            }
                        }
                    }
                    if (key == "-sa")
                    {
                        using (StreamWriter sw = new StreamWriter(pathOut, false, Encoding.Default))
                        {
                            try
                            {
                                Parser parser = new Parser(lexer);
                                Node firstNode = parser.ParseMainProgram();
                                sw.Write(firstNode.ToString(new List<bool>()) + "\r\n");
                                sw.Write(parser.PrintSymTable());
                            }
                            catch (ExceptionWithPosition ex)
                            {
                                sw.Write($"{ex}\r\n");
                            }
                            catch (Exception ex)
                            {
                                sw.Write(new ExceptionWithPosition(lexer.CurrentLine, lexer.CurrentSymbol - 1, ex.Message).ToString());
                            }
                        }
                    }
                    string checkFile;
                    string outFile;
                    using (StreamReader sr = new StreamReader(pathCheck, Encoding.UTF8))
                    {
                        checkFile = sr.ReadToEnd();
                    }
                    using (StreamReader sr = new StreamReader(pathOut, Encoding.UTF8))
                    {
                        outFile = sr.ReadToEnd();
                    }
                    if (checkFile == outFile)
                    {
                        Console.WriteLine($"{numberTest}-OK");
                        countOK += 1;
                    }
                    else
                    {
                        Console.WriteLine($"{numberTest}-ERROR");
                        countERROR += 1;
                    }
                }
            }
            Console.WriteLine($"OK: {countOK}  ERRORS: {countERROR}");
        }
        static void StartDetailTest(string key)
        {
            for (int numberFolder = 0; numberFolder < folders.Length; numberFolder++)
            {
                Console.WriteLine($"----------{folders[numberFolder].name}----------");
                for (int numberTest = 1; numberTest <= folders[numberFolder].countTest; numberTest++)
                {
                    Console.WriteLine($"{numberTest}-----------------------------------\n");
                    string numberTestStr = numberTest.ToString();
                    if (numberTestStr.Length < 2)
                    {
                        numberTestStr = "0" + numberTestStr;
                    }
                    string pathIn = Environment.CurrentDirectory + $"/tests/{folders[numberFolder].name}/" + $"{numberTestStr}_input.txt";
                    string pathOut = Environment.CurrentDirectory + $"/tests/{folders[numberFolder].name}/" + $"{numberTestStr}_out.txt";
                    using (StreamReader sr = new StreamReader(pathIn, Encoding.Default))
                    {
                        Console.WriteLine(sr.ReadToEnd() + "\n");
                    }
                    using (StreamReader sr = new StreamReader(pathOut, Encoding.Default))
                    {
                        Console.WriteLine(sr.ReadToEnd());
                    }
                    Console.WriteLine("Press Enter to continue");
                    string? input = Console.ReadLine();
                }
            }
        }
    }
}
