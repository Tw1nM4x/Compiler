using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Reflection.PortableExecutable;

namespace Compiler
{
    class Compiler
    {
        public static void CompileFile(string pathIn = $"../../../tests/1.txt", string pathOut = $"console")
        {
            int lexemeLenght = 0;
            int сurrentSymbol = 0;
            int currentLine = 1;
            bool nowCommentLine = false;
            List<string> ans = new List<string>();

            LexicalAnalyzer.CreateTableDFA();

            using (StreamReader sr = new StreamReader(pathIn, Encoding.UTF8))
            {
                LexicalAnalyzer.allFileForCheckComments = sr.ReadToEnd();
                sr.DiscardBufferedData();
                sr.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                string? line;
                bool end = false;
                while ((line = sr.ReadLine()) != null && !end)
                {
                    if (line != null)
                    {
                        while (line.Length > 0)
                        {
                            string typeLexeme = LexicalAnalyzer.GetFirstLexeme(line, ref lexemeLenght, ref nowCommentLine);
                            string lexeme = line.Substring(0, lexemeLenght);
                            string value = LexicalAnalyzer.GetValueLexeme(typeLexeme, lexeme);
                            if (typeLexeme == "ERROR")
                            {
                                ans.Add($"{currentLine} {сurrentSymbol + 1} {typeLexeme}");
                                if (pathOut == "console")
                                {
                                    Console.WriteLine($"{currentLine} {сurrentSymbol + 1} {typeLexeme}");
                                }
                                end = true;
                                break;
                            }
                            if (typeLexeme != "Space" && typeLexeme != "Comment")
                            {
                                if (value.Length >= 5 && value.Substring(0, 5) == "ERROR")
                                {
                                    ans.Add($"{currentLine} {сurrentSymbol + 1} {value}");
                                    if (pathOut == "console")
                                    {
                                        Console.WriteLine($"{currentLine} {сurrentSymbol + 1} {value}");
                                    }
                                    end = true;
                                    break;
                                }
                                ans.Add($"{currentLine} {сurrentSymbol + 1} {typeLexeme} {value} {lexeme}");
                                if (pathOut == "console")
                                {
                                    Console.WriteLine($"{currentLine} {сurrentSymbol + 1} {typeLexeme} {value} {lexeme}");
                                }
                                if(value == "finish")
                                {
                                    end = true;
                                    break;
                                }
                            }
                            сurrentSymbol += lexemeLenght;
                            if (lexemeLenght <= line.Length)
                            {
                                line = line.Substring(lexemeLenght);
                            }
                        }
                    }
                    currentLine += 1;
                    сurrentSymbol = 0;
                }
            }

            if(pathOut != "console")
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