using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Reflection.PortableExecutable;

namespace Compiler
{
    class Compiler
    {
        const string lexemeEndFile = "finish";
        static string[] keyWords = new string[] { "and", "array", "as", "asm", "begin", "case", "const", "constructor", "destructor", "div", "do", "downto", "else", "end", "file", "for", "foreach", "function", "goto", "implementation", "if", "in", "inherited", "inline", "interface", "label", "mod", "nil", "not", "object", "of", "operator", "or", "packed", "procedure", "program", "record", "repeat", "self", "set", "shl", "shr", "string", "then", "to", "type", "unit", "until", "uses", "var", "while", "with", "xor", "dispose", "exit", "false", "new", "true", "as", "class", "dispinterface", "except", "exports", "finalization", "finally", "initialization", "inline", "is", "library", "on", "out", "packed", "property", "raise", "resourcestring", "threadvar", "try" };
        const int numberOfStatusAutomaton = 12;
        const int numberOfSymbols = 256;
        static string allFileForCheckComments = "";
        static int[,] table = new int[numberOfStatusAutomaton, numberOfSymbols];
        static string[] status = { "ERROR", "ERROR", "String", "Indifier", "Integer", "Integer", "Integer", "Integer", "Real", "Space", "Comment", "Char", "Key_word", "End_file", "Operation_sign", "Separator" };
        /*
         0 - состояние ошибки (ERROR)
         1 - состояние начальное
         2 - состояние String
         3 - состояние Indifier
         4 - состояние Integer x10
         5 - состояние Integer x2
         6 - состояние Integer x8
         7 - состояние Integer x16
         8 - состояние Real
         9 - состояние Space
         10 - состояние Comment
         11 - состояние Char
         -------------------------
         12 - состояние Key_word
         13 - состояние End_file
         14 - состояние Operation_sign
         15 - состояние Separator
        */
        static void CreateTableDFA()
        {
            //string
            table[1, (int)'\''] = 2;
            //indifier
            table[1, (int)'_'] = 3;
            table[3, (int)'_'] = 3;
            //integer
            table[1, (int)'%'] = 5;
            table[1, (int)'&'] = 6;
            table[1, (int)'$'] = 7;
            //real
            table[4, (int)'.'] = 8;
            //space
            table[1, (int)' '] = 9;
            table[9, (int)' '] = 9;
            //operation sign
            table[1, (int)'='] = 14;
            table[1, (int)':'] = 14;
            table[1, (int)'+'] = 14;
            table[1, (int)'-'] = 14;
            table[1, (int)'*'] = 14;
            table[1, (int)'/'] = 14;
            table[1, (int)'>'] = 14;
            table[1, (int)'<'] = 14;
            //separator
            table[1, (int)','] = 15;
            table[1, (int)';'] = 15;
            table[1, (int)'('] = 15;
            table[1, (int)')'] = 15;
            table[1, (int)'['] = 15;
            table[1, (int)']'] = 15;
            table[1, (int)'.'] = 15;
            //char
            table[1, (int)'#'] = 11;

            for (int i = 0; i < numberOfSymbols; i++)
            {
                //string
                table[2, i] = 2;
                //comment
                table[10, i] = 10;
            }
            for (int i = (int)'a'; i <= (int)'z'; i++)
            {
                //indifier
                table[1, i] = 3;
                table[3, i] = 3;
            }
            for (int i = (int)'0'; i <= (int)'9'; i++)
            {
                //indifier
                table[3, i] = 3;
                //integer x10
                table[1, i] = 4;
                table[4, i] = 4;
                //integer x16
                table[7, i] = 7;
                //real
                table[8, i] = 8;
                //char
                table[11, i] = 11;
            }
            for (int i = (int)'0'; i <= (int)'1'; i++)
            {
                //integer x2
                table[5, i] = 5;
            }
            for (int i = (int)'0'; i <= (int)'7'; i++)
            {
                //integer x8
                table[6, i] = 6;
            }
            for (int i = (int)'a'; i <= (int)'f'; i++)
            {
                //integer x16
                table[7, i] = 7;
            }
        }

        static string LexicalAnalyzer(string input, ref int lexemeLenght, ref bool nowCommentLine, ref bool typeCommentIsFigureScope)
        {
            int statusDFA = 1;
            lexemeLenght = 0;

            //if Comment
            if (input[0] == '/' && input[1] == '/' && input.Length >= 1 && !nowCommentLine)
            {
                statusDFA = 10;
                lexemeLenght = input.Length;
                return status[statusDFA];
            }
            //if comment long (* *)
            if (input[0] == '(' && input[1] == '*' && input.Length >= 2 && !nowCommentLine)
            {
                int indexStart = allFileForCheckComments.IndexOf("(*") + 2;
                if (allFileForCheckComments.Substring(indexStart, allFileForCheckComments.Length - indexStart).IndexOf("*)") != -1)
                {
                    allFileForCheckComments = allFileForCheckComments.Substring(indexStart, allFileForCheckComments.Length - indexStart);
                    typeCommentIsFigureScope = false;
                    statusDFA = 10;
                    lexemeLenght = 2;
                }
                else
                {
                    statusDFA = 15;
                    lexemeLenght = 1;
                    return status[statusDFA];
                }
            }
            //if comment long { }
            if (input[0] == '{' && input.Length >= 1 && !nowCommentLine)
            {
                if (allFileForCheckComments.Substring(allFileForCheckComments.IndexOf("{") + 1, allFileForCheckComments.Length - allFileForCheckComments.IndexOf("{") - 1).IndexOf("}") != -1)
                {
                    allFileForCheckComments = allFileForCheckComments.Substring(allFileForCheckComments.IndexOf("}") + 1, allFileForCheckComments.Length - allFileForCheckComments.IndexOf("}") - 1);
                    typeCommentIsFigureScope = true;
                    statusDFA = 10;
                    lexemeLenght = 1;
                }
                else
                {
                    statusDFA = 0;
                    lexemeLenght = 1;
                    return status[statusDFA];
                }
            }
            //if comment now
            if (nowCommentLine)
            {
                statusDFA = 10;
            }

            for (int i = lexemeLenght; i < input.Length; i++)
            {
                if (statusDFA < numberOfStatusAutomaton)
                {
                    if (table[statusDFA, input[i] < numberOfSymbols ? Char.ToLower(input[i]) : 255] != 0)
                    {
                        lexemeLenght += 1;
                        statusDFA = table[statusDFA, input[i] < numberOfSymbols ? Char.ToLower(input[i]) : 255];
                        //if end string
                        if (statusDFA == 2 && input[i] == '\'' && lexemeLenght > 1)
                        {
                            break;
                        }
                        //if end comment {}
                        if (statusDFA == 10 && input[i] == '}' && typeCommentIsFigureScope)
                        {
                            typeCommentIsFigureScope = true;
                            nowCommentLine = false;
                            return status[statusDFA];
                        }
                        //if end comment (* *)
                        if (statusDFA == 10 && (input[i] == '*' && input[i + 1] == ')' && i + 1 < input.Length) && !typeCommentIsFigureScope)
                        {
                            typeCommentIsFigureScope = true;
                            nowCommentLine = false;
                            lexemeLenght += 1;
                            return status[statusDFA];
                        }
                        //if key word or end file
                        if (statusDFA == 3)
                        {
                            if (input.Substring(0, lexemeLenght).ToLower() == lexemeEndFile)
                            {
                                //end file
                                statusDFA = 13;
                                return status[statusDFA];
                            }

                            foreach (string keyWord in keyWords)
                            {
                                if (input.Substring(0, lexemeLenght).ToLower() == keyWord)
                                {
                                    //key word
                                    statusDFA = 12;
                                    return status[statusDFA];
                                }
                            }
                        }
                    }
                    else
                    {
                        //if long error
                        if (table[1, input[0] < numberOfSymbols ? Char.ToLower(input[0]) : 255] == 0)
                        {
                            lexemeLenght += 1;
                            statusDFA = 1;
                            if (i + 1 < input.Length && table[statusDFA, input[i + 1] < numberOfSymbols ? input[i + 1] : 255] != 0)
                            {
                                break;
                            }
                        }
                        else
                        {
                            //if end lexem
                            if (lexemeLenght == 0)
                            {
                                lexemeLenght = 1;
                            }
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            //check string
            if (statusDFA == 2)
            {
                //if dont string
                if (input[lexemeLenght - 1] != '\'' || lexemeLenght == 1)
                {
                    lexemeLenght = 1;
                    statusDFA = 0;
                }
                else
                {
                    //if char
                    if (lexemeLenght == 3)
                    {
                        statusDFA = 11;
                    }
                }
            }
            //if only % or & or $
            if (statusDFA >= 5 && statusDFA <= 7 && lexemeLenght == 1)
            {
                statusDFA = 0;
            }
            //check real
            if (input[lexemeLenght - 1] == '.' && statusDFA == 8)
            {
                statusDFA = 4;
                lexemeLenght -= 1;
            }
            //check couple operation signs
            if (statusDFA == 14)
            {
                if (lexemeLenght < input.Length &&
                    ((input[lexemeLenght - 1] == '<' && input[lexemeLenght] == '<') ||
                    (input[lexemeLenght - 1] == '>' && input[lexemeLenght] == '>') ||
                    (input[lexemeLenght - 1] == '*' && input[lexemeLenght] == '*') ||
                    (input[lexemeLenght - 1] == '<' && input[lexemeLenght] == '>') ||
                    (input[lexemeLenght - 1] == '<' && input[lexemeLenght] == '=') ||
                    (input[lexemeLenght - 1] == '>' && input[lexemeLenght] == '=') ||
                    (input[lexemeLenght - 1] == ':' && input[lexemeLenght] == '=') ||
                    (input[lexemeLenght - 1] == '+' && input[lexemeLenght] == '=') ||
                    (input[lexemeLenght - 1] == '-' && input[lexemeLenght] == '=') ||
                    (input[lexemeLenght - 1] == '*' && input[lexemeLenght] == '=') ||
                    (input[lexemeLenght - 1] == '/' && input[lexemeLenght] == '=')))
                {
                    lexemeLenght += 1;
                }
            }
            //check couple separator
            if (statusDFA == 15)
            {
                if (lexemeLenght < input.Length &&
                    ((input[lexemeLenght - 1] == '(' && input[lexemeLenght] == '.') ||
                    (input[lexemeLenght - 1] == '.' && input[lexemeLenght] == ')')))
                {
                    lexemeLenght += 1;
                }
                else
                {
                    if (input[lexemeLenght - 1] == '.')
                    {
                        statusDFA = 0;
                    }
                }
            }
            //if comment
            if (statusDFA == 10)
            {
                nowCommentLine = true;
            }
            return status[statusDFA];
        }

        public static void Main(string pathIn, string pathOut)
        {
            int lexemeLenght = 0;
            int сurrentSymbol = 0;
            int currentLine = 1;
            bool nowCommentLine = false;
            bool typeCommentIsFigureScope = true;
            List<string> ans = new List<string>();

            CreateTableDFA();

            using (StreamReader sr = new StreamReader(pathIn, Encoding.Default))
            {
                allFileForCheckComments = sr.ReadToEnd();
                sr.DiscardBufferedData();
                sr.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line != null)
                    {
                        while (line.Length > 0)
                        {
                            string typeLexeme = LexicalAnalyzer(line, ref lexemeLenght, ref nowCommentLine, ref typeCommentIsFigureScope);
                            if (typeLexeme != "Space" && typeLexeme != "Comment")
                            {
                                ans.Add($"{currentLine} {сurrentSymbol + 1} {typeLexeme} {line.Substring(0, lexemeLenght)}");
                                Console.WriteLine($"{currentLine} {сurrentSymbol + 1} {typeLexeme} {line.Substring(0, lexemeLenght)}");
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

            using (StreamWriter sw = new StreamWriter(pathOut))
            {
                foreach(var lineOut in ans)
                {
                    sw.WriteLine(lineOut);
                }
            }
        }
    }
}
//е в real
//значение у всех
//проверять границы
//