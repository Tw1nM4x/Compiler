using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Reflection.PortableExecutable;

namespace Compiler
{
    class Compiler
    {
        public static void CompileFile(string pathIn = $"../../../tests/001_input.txt", string pathOut = $"../../../tests/001_out.txt")
        {
            int lexemeLenght = 0;
            int сurrentSymbol = 0;
            int currentLine = 1;
            bool nowCommentLine = false;
            bool typeCommentIsFigureScope = true;
            List<string> ans = new List<string>();

            LexicalAnalyzer.CreateTableDFA();

            using (StreamReader sr = new StreamReader(pathIn, Encoding.Default))
            {
                LexicalAnalyzer.allFileForCheckComments = sr.ReadToEnd();
                sr.DiscardBufferedData();
                sr.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line != null)
                    {
                        while (line.Length > 0)
                        {
                            string typeLexeme = LexicalAnalyzer.GetFirstLexeme(line, ref lexemeLenght, ref nowCommentLine, ref typeCommentIsFigureScope);
                            //if lexeme invalid
                            if (typeLexeme == "ERROR")
                            {
                                ans.Add($"{currentLine} {сurrentSymbol + 1} ERROR: Такой лексемы не существует");
                                if (pathOut == "console")
                                {
                                    Console.WriteLine($"{currentLine} {сurrentSymbol + 1} ERROR: Такой лексемы не существует");
                                }
                                return;
                            }
                            if (typeLexeme != "Space" && typeLexeme != "Comment")
                            {
                                string lexeme = line.Substring(0, lexemeLenght);
                                string value = LexicalAnalyzer.GetValueLexeme(typeLexeme, lexeme);
                                //if value invalid
                                if (value.Length >= 5 && value.Substring(0, 5) == "ERROR")
                                {
                                    ans.Add($"{currentLine} {сurrentSymbol + 1} {value}");
                                    if (pathOut == "console")
                                    {
                                        Console.WriteLine($"{currentLine} {сurrentSymbol + 1} {value}");
                                    }
                                    return;
                                }
                                ans.Add($"{currentLine} {сurrentSymbol + 1} {typeLexeme} {value} {lexeme}");
                                if (pathOut == "console")
                                {
                                    Console.WriteLine($"{currentLine} {сurrentSymbol + 1} {typeLexeme} {value} {lexeme}");
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
                using (StreamWriter sw = new StreamWriter(pathOut))
                {
                    foreach (var lineOut in ans)
                    {
                        sw.WriteLine(lineOut);
                    }
                }
            }
        }
    }
}