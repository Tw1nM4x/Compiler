using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Reflection.PortableExecutable;
using System.Threading;

namespace Compiler
{
    class SkillCompiler
    {
        public static void OutputLexemeParsing(string pathIn = "../../../tests/1.txt", string pathOut = "console")
        {
            List<Lexeme> lexemes = new List<Lexeme>();

            using (FileStream fstream = File.OpenRead(pathIn))
            {
                byte[] input = new byte[fstream.Length];
                fstream.Read(input, 0, input.Length);
                Lexer.currentSymbol = 1;
                Lexer.currentLine = 1;
                while (input.Length > 0)
                {
                    Lexeme nextLex = Lexer.GetFirstLexeme(ref input);
                    if (nextLex.type != "Not_Lexeme")
                    {
                        lexemes.Add(nextLex);
                    }
                    if (nextLex.type == "ERROR" || nextLex.type == "End_file")
                    {
                        break;
                    }
                }
            }

            if (pathOut != "console")
            {
                using (StreamWriter sw = new StreamWriter(pathOut, false, Encoding.Default))
                {
                    foreach (Lexeme lex in lexemes)
                    {
                        if (lex.type == "ERROR")
                        {
                            sw.Write($"{lex.numberLine} {lex.numberSymbol} {lex.type}\r\n");
                        }
                        else
                        {
                            if (lex.value.Length > 5 && lex.value.Substring(0, 5) == "ERROR")
                            {
                                sw.Write($"{lex.numberLine} {lex.numberSymbol} {lex.value}\r\n");
                            }
                            else
                            {
                                sw.Write($"{lex.numberLine} {lex.numberSymbol} {lex.type} {lex.value} {lex.lexeme}\r\n");
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (Lexeme lex in lexemes)
                {
                    if (lex.type == "ERROR")
                    {
                        Console.Write($"{lex.numberLine} {lex.numberSymbol} {lex.type}\r\n");
                    }
                    else
                    {
                        if (lex.value.Length > 5 && lex.value.Substring(0, 5) == "ERROR")
                        {
                            Console.Write($"{lex.numberLine} {lex.numberSymbol} {lex.value}\r\n");
                        }
                        else
                        {
                            Console.Write($"{lex.numberLine} {lex.numberSymbol} {lex.type} {lex.value} {lex.lexeme}\r\n");
                        }
                    }
                }
            }
        }
        public static void OutputSimpleExpressionsParsing(string pathIn = "../../../tests/1.txt", string pathOut = "console")
        {
            List<string> ans = new List<string>();
            List<Lexeme> lexemes = new List<Lexeme>();
            byte[] input = new byte[0];

            using (FileStream fstream = File.OpenRead(pathIn))
            {
                input = new byte[fstream.Length];
                fstream.Read(input, 0, input.Length);
            }

            if(input.Length == 0)
            {
                return;
            }

            Lexer.currentSymbol = 1;
            Lexer.currentLine = 1;
            Parser.currentLex = Lexer.GetFirstLexeme(ref input);
            Node? firstNode = Parser.ParseSimpleExpression(ref input);

            string error = "";

            void ToPrint(Node? last, int deep, bool isLeft, List<bool> isLeftParents)
            {
                if (last != null)
                {
                    string print = "";
                    if (deep > 0)
                    {
                        for (int i = 1; i < deep; i++)
                        {
                            if (isLeftParents[i - 1])
                            {
                                print = print + "│    ";
                            }
                            else
                            {
                                print = print + "     ";
                            }
                        }
                        if (isLeft)
                        {
                            print = print + "├─── ";
                        }
                        else
                        {
                            print = print + "└─── ";
                        }
                    }
                    print = print + last.value;
                    if (last.type == "ERROR")
                    {
                        error = last.value;
                    }
                    else
                    {
                        ans.Add(print);
                    }
                }
            }

            void CheckChildren(Node? node, int deep, List<bool> isLeftParents)
            {
                Node? now = node;
                if (now != null && now.childrens != null)
                {
                    for (int i = 0; i < now.childrens.Count - 1; i++)
                    {
                        node = now.childrens[i];
                        if (node != null)
                        {
                            ToPrint(node, deep, true, isLeftParents);
                            List<bool> isLeftParentsForLeft = new List<bool>();
                            for (int j = 0; j < isLeftParents.Count; j++)
                            {
                                isLeftParentsForLeft.Add(isLeftParents[j]);
                            }
                            isLeftParentsForLeft.Add(true);
                            CheckChildren(node, deep + 1, isLeftParentsForLeft);
                        }
                    }
                    node = now.childrens[^1];
                    if (node != null)
                    {
                        ToPrint(node, deep, false, isLeftParents);
                        List<bool> isLeftParentsForRight = new List<bool>();
                        for (int i = 0; i < isLeftParents.Count; i++)
                        {
                            isLeftParentsForRight.Add(isLeftParents[i]);
                        }
                        isLeftParentsForRight.Add(false);
                        CheckChildren(node, deep + 1, isLeftParentsForRight);
                    }
                }
            }

            int deep = 0;
            List<bool> isLeftParents = new List<bool>();
            ToPrint(firstNode, deep, true, isLeftParents);
            CheckChildren(firstNode, deep + 1, isLeftParents);

            if (error != "")
            {
                if (pathOut == "console")
                {
                    Console.Write($"{error}\r\n");
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter(pathOut, false, Encoding.Default))
                    {
                        sw.Write($"{error}\r\n");
                    }
                }
            }
            else
            {
                if (pathOut == "console")
                {
                    foreach (string lineOut in ans)
                    {
                        Console.Write($"{lineOut}\r\n");
                    }
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter(pathOut, false, Encoding.Default))
                    {
                        foreach (string lineOut in ans)
                        {
                            sw.Write($"{lineOut}\r\n");
                        }
                    }
                }
            }
        }
        public static void OutputSyntaxParsing(string pathIn = "../../../tests/1.txt", string pathOut = "console")
        {
            List<string> ans = new List<string>();
            List<Lexeme> lexemes = new List<Lexeme>();
            byte[] input = new byte[0];

            using (FileStream fstream = File.OpenRead(pathIn))
            {
                input = new byte[fstream.Length];
                fstream.Read(input, 0, input.Length);
            }

            if (input.Length == 0)
            {
                return;
            }

            Lexer.currentSymbol = 1;
            Lexer.currentLine = 1;
            Node? firstNode = Parser.Parse(ref input);

            string error = "";
            
            void ToPrint(Node? last, int deep, bool isLeft, List<bool> isLeftParents)
            {
                if (last != null)
                {
                    string print = "";
                    if (deep > 0)
                    {
                        for (int i = 1; i < deep; i++)
                        {
                            if (isLeftParents[i - 1])
                            {
                                print = print + "│    ";
                            }
                            else
                            {
                                print = print + "     ";
                            }
                        }
                        if (isLeft)
                        {
                            print = print + "├─── ";
                        }
                        else
                        {
                            print = print + "└─── ";
                        }
                    }
                    print = print + last.value;
                    if (last.type == "ERROR")
                    {
                        error = last.value;
                    }
                    else
                    {
                        ans.Add(print);
                    }
                }
            }

            void CheckChildren(Node? node, int deep, List<bool> isLeftParents)
            {
                Node? now = node;
                if (now != null && now.childrens != null)
                {
                    for (int i = 0; i < now.childrens.Count - 1; i++)
                    {
                        node = now.childrens[i];
                        if (node != null)
                        {
                            ToPrint(node, deep, true, isLeftParents);
                            List<bool> isLeftParentsForLeft = new List<bool>();
                            for (int j = 0; j < isLeftParents.Count; j++)
                            {
                                isLeftParentsForLeft.Add(isLeftParents[j]);
                            }
                            isLeftParentsForLeft.Add(true);
                            CheckChildren(node, deep + 1, isLeftParentsForLeft);
                        }
                    }
                    node = now.childrens[^1];
                    if (node != null)
                    {
                        ToPrint(node, deep, false, isLeftParents);
                        List<bool> isLeftParentsForRight = new List<bool>();
                        for (int i = 0; i < isLeftParents.Count; i++)
                        {
                            isLeftParentsForRight.Add(isLeftParents[i]);
                        }
                        isLeftParentsForRight.Add(false);
                        CheckChildren(node, deep + 1, isLeftParentsForRight);
                    }
                }
            }

            int deep = 0;
            List<bool> isLeftParents = new List<bool>();
            ToPrint(firstNode, deep, true, isLeftParents);
            CheckChildren(firstNode, deep + 1, isLeftParents);

            if (error != "")
            {
                if (pathOut == "console")
                {
                    Console.Write($"{error}\r\n");
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter(pathOut, false, Encoding.Default))
                    {
                        sw.Write($"{error}\r\n");
                    }
                }
            }
            else
            {
                if (pathOut == "console")
                {
                    foreach (string lineOut in ans)
                    {
                        Console.Write($"{lineOut}\r\n");
                    }
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter(pathOut, false, Encoding.Default))
                    {
                        foreach (string lineOut in ans)
                        {
                            sw.Write($"{lineOut}\r\n");
                        }
                    }
                }
            }
        }
    }
}