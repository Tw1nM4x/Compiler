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

        public static void StartTest(string key) 
        {
            switch (key)
            {
                case "-l":
                    folders = new Folder[] { new("string", 14), new("indifier", 9), new("integer", 19), new("real", 28), new("space", 2), new("comment", 9), new("key word", 4), new("operation sign", 3), new("separator", 3), new("errors", 3) };
                    break;
                case "-spar":
                    folders = new Folder[] { new("simple expressions", 14) };
                    break;
                case "-par":
                    folders = new Folder[] { new("syntax", 14) };
                    break;
            }
            int countOK = 0;
            int countERROR = 0;
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
                                if (lexer.LastToken.Type != TokenType.Eof)
                                {
                                    throw new ExceptionWithPosition(lexer.CurrentLine, lexer.CurrentSymbol - 1, "expected operation sign");
                                }
                            }
                            if (key == "-par")
                            {
                                Parser parser = new Parser(lexer);
                                firstNode = parser.ParseProgram(isMain: true);
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
            Console.WriteLine("\nWrite 'go' to see details");
            string? input = Console.ReadLine();
            if (input == "go")
            {
                StartDetailTest(key);
            }
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
                    if (input != "")
                    {
                        return;
                    }
                }
            }
        }
    }
}
