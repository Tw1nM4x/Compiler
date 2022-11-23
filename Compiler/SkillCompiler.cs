using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Reflection.PortableExecutable;
using System.Threading;

namespace Compiler
{
    /*class SkillCompiler
    {
        public static void OutputLexemeParsing(string pathIn = "../../../tests/1.txt", string pathOut = "console")
        {
            List<Token> lexemes = new List<Token>();

            using (FileStream fstream = File.OpenRead(pathIn))
            {
                byte[] input = new byte[fstream.Length];
                fstream.Read(input, 0, input.Length);
                Lexer.currentSymbol = 1;
                Lexer.currentLine = 1;
                List<Token> ans = new List<Token> { };
                while (true)
                {
                    try
                    {
                        Token nextLex = Lexer.GetFirstLexeme(ref input);
                        ans.Add(nextLex);
                        if (nextLex.type == TokenType.Eof)
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (pathOut == "console")
                        {
                            Console.Write($"({Lexer.currentLine},{Lexer.currentSymbol}) ERROR: {ex.Message}\r\n");
                        }
                        else
                        {
                            using (StreamWriter sw = new StreamWriter(pathOut, false, Encoding.Default))
                            {
                                sw.Write($"({Lexer.currentLine},{Lexer.currentSymbol}) ERROR: {ex.Message}\r\n");
                            }
                        }
                        return;
                    }
                }
                if (pathOut == "console")
                {
                    foreach(Token lex in ans)
                    {
                        Console.Write($"{lex.numberLine} {lex.numberSymbol} {lex.type} {lex.value} {lex.lexeme}\r\n");
                    }
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter(pathOut, false, Encoding.Default))
                    {
                        foreach (Token lex in ans)
                        {
                            sw.Write($"{lex.numberLine} {lex.numberSymbol} {lex.type} {lex.value} {lex.lexeme}\r\n");
                        }
                    }
                }
            }
        }
        public static void OutputSimpleExpressionsParsing(string pathIn = "../../../tests/1.txt", string pathOut = "console")
        {
            List<string> ans = new List<string>();
            List<Token> lexemes = new List<Token>();
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
            try
            {
                Parser.currentLex = Lexer.GetFirstLexeme(ref input);
                Node? firstNode = Parser.ParseSimpleExpression(ref input);

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
                        ans.Add(print);
                    }
                }
                void CheckChildren(Node? node, int deep, List<bool> isLeftParents)
                {
                    Node? now = node;
                    if (now != null && now.children != null)
                    {
                        for (int i = 0; i < now.children.Count - 1; i++)
                        {
                            node = now.children[i];
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
                        if (now.children.Count > 0)
                        {
                            node = now.children[^1];
                        }
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
            catch (Exception ex)
            {
                if (pathOut == "console")
                {
                    Console.Write($"({Lexer.currentLine},{Lexer.currentSymbol - Parser.currentLex.value.Length}) ERROR: {ex.Message}\r\n");
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter(pathOut, false, Encoding.Default))
                    {
                        sw.Write($"({Lexer.currentLine},{Lexer.currentSymbol - Parser.currentLex.value.Length}) ERROR: {ex.Message}\r\n");
                    }
                }
            }
        }
        public static void OutputSyntaxParsing(string pathIn = "../../../tests/1.txt", string pathOut = "console")
        {
            List<string> ans = new List<string>();
            List<Token> lexemes = new List<Token>();
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
            try
            {
                Node? firstNode = Parser.Parse(ref input);

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
                        ans.Add(print);
                    }
                }
                void CheckChildren(Node? node, int deep, List<bool> isLeftParents)
                {
                    Node? now = node;
                    if (now != null && now.children != null)
                    {
                        for (int i = 0; i < now.children.Count - 1; i++)
                        {
                            node = now.children[i];
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
                        node = now.children[^1];
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
            catch (Exception ex)
            {
                if (pathOut == "console")
                {
                    Console.Write($"({Lexer.currentLine},{Lexer.currentSymbol - Parser.currentLex.value.Length}) ERROR: {ex.Message}\r\n");
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter(pathOut, false, Encoding.Default))
                    {
                        sw.Write($"({Lexer.currentLine},{Lexer.currentSymbol - Parser.currentLex.value.Length}) ERROR: {ex.Message}\r\n");
                    }
                }
            }
        }
    }*/
}