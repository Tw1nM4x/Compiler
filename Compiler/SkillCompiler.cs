using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Reflection.PortableExecutable;

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
                        if(lex.type == "ERROR")
                        {
                            sw.WriteLine($"{lex.numberLine} {lex.numberSymbol} {lex.type}");
                        }
                        else
                        {
                            if(lex.value.Length > 5 && lex.value.Substring(0, 5) == "ERROR")
                            {
                                sw.WriteLine($"{lex.numberLine} {lex.numberSymbol} {lex.value}");
                            }
                            else
                            {
                                sw.WriteLine($"{lex.numberLine} {lex.numberSymbol} {lex.type} {lex.value} {lex.lexeme}");
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
                        Console.WriteLine($"{lex.numberLine} {lex.numberSymbol} {lex.type}");
                    }
                    else
                    {
                        if (lex.value.Length > 5 && lex.value.Substring(0, 5) == "ERROR")
                        {
                            Console.WriteLine($"{lex.numberLine} {lex.numberSymbol} {lex.value}");
                        }
                        else
                        {
                            Console.WriteLine($"{lex.numberLine} {lex.numberSymbol} {lex.type} {lex.value} {lex.lexeme}");
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

            Lexer.currentSymbol = 1;
            Lexer.currentLine = 1;
            Node? firstNode = Parser.ParseExpression(ref input);

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
                node = now.left;
                if (node != null)
                {
                    ToPrint(node, deep, true, isLeftParents);
                    List<bool> isLeftParentsForLeft = new List<bool>();
                    for (int i = 0; i < isLeftParents.Count; i++)
                    {
                        isLeftParentsForLeft.Add(isLeftParents[i]);
                    }
                    isLeftParentsForLeft.Add(true);
                    CheckChildren(node, deep + 1, isLeftParentsForLeft);
                }
                node = now.right;
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

            int deep = 0;
            List<bool> isLeftParents = new List<bool>();
            ToPrint(firstNode, deep, true, isLeftParents);
            CheckChildren(firstNode, deep + 1, isLeftParents);

            if (error != "")
            {
                if(pathOut == "console")
                {
                    Console.WriteLine(error);
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter(pathOut, false, Encoding.Default))
                    {
                        sw.WriteLine(error);
                    }
                }
            }
            else
            {
                if (pathOut == "console")
                {
                    foreach (string lineOut in ans)
                    {
                        Console.WriteLine(lineOut);
                    }
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter(pathOut, false, Encoding.Default))
                    {
                        foreach (string lineOut in ans)
                        {
                            sw.WriteLine(lineOut);
                        }
                    }
                }
            }
        }
    }
}