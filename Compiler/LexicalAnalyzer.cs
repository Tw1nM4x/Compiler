using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Compiler
{
    internal class LexicalAnalyzer
    {
        const string lexemeEndFile = "finish";
        static string[] keyWords = new string[] { "and", "array", "as", "asm", "begin", "case", "const", "constructor", "destructor", "div", "do", "downto", "else", "end", "file", "for", "foreach", "function", "goto", "implementation", "if", "in", "inherited", "inline", "interface", "label", "mod", "nil", "not", "object", "of", "operator", "or", "packed", "procedure", "program", "record", "repeat", "self", "set", "shl", "shr", "string", "then", "to", "type", "unit", "until", "uses", "var", "while", "with", "xor", "dispose", "exit", "false", "new", "true", "as", "class", "dispinterface", "except", "exports", "finalization", "finally", "initialization", "inline", "is", "library", "on", "out", "packed", "property", "raise", "resourcestring", "threadvar", "try" };
        const int numberOfStatusAutomaton = 12;
        const int numberOfSymbols = 256;
        public static string allFileForCheckComments = "";
        static int[,] table = new int[numberOfStatusAutomaton, numberOfSymbols];
        static string[] status = { "ERROR", "ERROR", "String", "Indifier", "Integer", "Integer", "Integer", "Integer", "Real", "Space", "Comment", "String", "Key_word", "End_file", "Operation_sign", "Separator" };
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
         11 - состояние String_whith_Char
        -------------------------
         12 - состояние Key_word
         13 - состояние End_file
         14 - состояние Operation_sign
         15 - состояние Separator
        */
        public static void CreateTableDFA()
        {
            //string
            table[1, (int)'\''] = 2;
            table[11, (int)'\''] = 2;
            //string whith char
            table[1, (int)'#'] = 11;
            table[11, (int)'#'] = 11;
            //indifier
            table[1, (int)'_'] = 3;
            table[3, (int)'_'] = 3;
            //integer
            table[1, (int)'%'] = 5;
            table[1, (int)'&'] = 6;
            table[1, (int)'$'] = 7;
            //real
            table[4, (int)'.'] = 8;
            table[4, (int)'e'] = 8;
            table[5, (int)'.'] = 8;
            table[5, (int)'e'] = 8;
            table[6, (int)'.'] = 8;
            table[6, (int)'e'] = 8;
            table[7, (int)'.'] = 8;
            table[7, (int)'e'] = 8;
            table[8, (int)'e'] = 8;
            table[8, (int)'-'] = 8;
            table[8, (int)'+'] = 8;
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
                //string whith char
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
        public static string GetFirstLexeme(string input, ref int lexemeLenght, ref bool nowCommentLine)
        {
            int statusDFA = 1;
            lexemeLenght = 0;
            int countQuotesInString = 0;

            if (input[0] == '/' && input[1] == '/' && input.Length >= 1 && !nowCommentLine)
            {
                statusDFA = 10;
                lexemeLenght = input.Length;
                return status[statusDFA];
            }
            if (input[0] == '{' && input.Length >= 1 && !nowCommentLine)
            {
                if (allFileForCheckComments.Substring(allFileForCheckComments.IndexOf("{") + 1, allFileForCheckComments.Length - allFileForCheckComments.IndexOf("{") - 1).IndexOf("}") != -1)
                {
                    allFileForCheckComments = allFileForCheckComments.Substring(allFileForCheckComments.IndexOf("}") + 1, allFileForCheckComments.Length - allFileForCheckComments.IndexOf("}") - 1);
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
                        //string
                        if (statusDFA == 2 && input[i] == '\'')
                        {
                            countQuotesInString += 1;
                            if (countQuotesInString % 2 == 0)
                            {
                                if (i + 1 < input.Length && input[i + 1] == '\'')
                                {
                                    statusDFA = 1;
                                }
                                else
                                {
                                    if (i + 1 < input.Length && input[i + 1] == '#')
                                    {
                                        statusDFA = 1;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        //if end comment {}
                        if (statusDFA == 10 && input[i] == '}')
                        {
                            nowCommentLine = false;
                            return status[statusDFA];
                        }
                        //if real have '.'
                        if (statusDFA == 8 && input[i] == '.')
                        {
                            if (input.Substring(0, lexemeLenght).ToLower().IndexOf('e') != -1)
                            {
                                statusDFA = 0;
                                lexemeLenght -= 1;
                                break;
                            }
                            if ((input[0] == '%' || input[0] == '&' || input[0] == '$') && i + 1 < input.Length && input[i + 1] >= '0' && input[i + 1] <= '9')
                            {
                                statusDFA = 0;
                                lexemeLenght -= 1;
                                break;
                            }
                        }
                        //if real have 'e'
                        if (statusDFA == 8 && Char.ToLower(input[i]) == 'e')
                        {
                            if (input.Substring(0, lexemeLenght - 1).ToLower().IndexOf('e') != -1)
                            {
                                statusDFA = 0;
                                lexemeLenght -= 1;
                                break;
                            }
                        }
                        //if real have '-' or '+'
                        if (statusDFA == 8 && (input[i] == '-' || input[i] == '+'))
                        {
                            if (Char.ToLower(input[i - 1]) != 'e')
                            {
                                statusDFA = 0;
                                lexemeLenght -= 1;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (lexemeLenght == 0)
                        {
                            lexemeLenght = 1;
                        }
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            //string
            if (statusDFA == 2 && countQuotesInString % 2 == 1)
            {
                statusDFA = 0;
                return status[statusDFA];
            }
            //if key word or end file
            if (statusDFA == 3)
            {
                if (input.Substring(0, lexemeLenght).ToLower() == lexemeEndFile)
                {
                    statusDFA = 13;
                    return status[statusDFA];
                }

                foreach (string keyWord in keyWords)
                {
                    if (input.Substring(0, lexemeLenght).ToLower() == keyWord)
                    {
                        statusDFA = 12;
                        return status[statusDFA];
                    }
                }
            }
            //if only % or & or $
            if (statusDFA >= 5 && statusDFA <= 7 && lexemeLenght == 1)
            {
                statusDFA = 0;
            }
            //if out system integer
            if (statusDFA >= 4 && statusDFA <= 7 && lexemeLenght < input.Length)
            {
                if ((input[lexemeLenght] >= '0' && input[lexemeLenght] <= '9') || (Char.ToLower(input[lexemeLenght]) >= 'a' && Char.ToLower(input[lexemeLenght]) <= 'z'))
                {
                    lexemeLenght += 1;
                    statusDFA = 0;
                }
            }
            //check real with last 'e' or '-' or '+'
            if (statusDFA == 8 && (Char.ToLower(input[lexemeLenght - 1]) == 'e' || input[lexemeLenght - 1] == '-' || input[lexemeLenght - 1] == '+'))
            {
                int offset = 1;
                if(input[lexemeLenght - 1] == '-' || input[lexemeLenght - 1] == '+')
                {
                    offset = 2;
                }
                if(input.Substring(0, lexemeLenght - offset).ToLower().IndexOf('.') != -1)
                {
                    if(input[lexemeLenght - offset - 1] == '.')
                    {
                        statusDFA = 0;
                        lexemeLenght -= (offset + 1);
                    }
                    else
                    {
                        lexemeLenght -= offset;
                    }
                }
                else
                {
                    statusDFA = 0;
                    lexemeLenght -= offset;
                }
            }
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
            if (statusDFA == 10)
            {
                nowCommentLine = true;
            }
            return status[statusDFA];
        }
        public static string GetValueLexeme(string typeLexeme, string lexeme)
        {
            string valueLexeme = "";
            switch (typeLexeme)
            {
                case "String":
                    {
                        if (lexeme.Length > 255)
                        {
                            valueLexeme = "ERROR: Overflow string";
                            break;
                        }
                        bool nowSymbol = false;
                        int countQuotes = 0;
                        string symbolStr = "";
                        for (int i = 0; i < lexeme.Length; i++)
                        {
                            if (lexeme[i] == '\'')
                            {
                                countQuotes += 1;
                                if (nowSymbol)
                                {
                                    int symbol = Int32.Parse(symbolStr);
                                    if (symbol > 65535)
                                    {
                                        valueLexeme = "ERROR: Overflow symbol in string";
                                        break;
                                    }
                                    valueLexeme += (char)symbol;
                                    symbolStr = "";
                                }
                                nowSymbol = false;
                                if (i + 1 < lexeme.Length && lexeme[i + 1] == '\'')
                                {
                                    valueLexeme += lexeme[i];
                                    i++;
                                }
                            }
                            else
                            {
                                if (lexeme[i] == '#' && countQuotes % 2 == 0)
                                {
                                    if (nowSymbol)
                                    {
                                        int symbol = Int32.Parse(symbolStr);
                                        if (symbol > 65535)
                                        {
                                            valueLexeme = "ERROR: Overflow symbol in string";
                                            break;
                                        }
                                        valueLexeme += (char)symbol;
                                        symbolStr = "";
                                    }
                                    nowSymbol = true;
                                }
                                else
                                {
                                    if (!nowSymbol)
                                    {
                                        valueLexeme += lexeme[i];
                                    }
                                    else
                                    {
                                        symbolStr += lexeme[i];
                                    }
                                }
                            }
                        }
                        if (nowSymbol)
                        {
                            int symbol = Int32.Parse(symbolStr);
                            if (symbol > 65535)
                            {
                                valueLexeme = "ERROR: Overflow symbol in string";
                                break;
                            }
                            valueLexeme += (char)symbol;
                            symbolStr = "";
                        }
                        break;
                    }
                case "Indifier":
                    {
                        valueLexeme = lexeme;
                        if (valueLexeme.Length > 127)
                        {
                            valueLexeme = "ERROR: Overflow indifier";
                        }
                        break;
                    }
                case "Integer":
                    {
                        uint typeInt = 10;
                        switch (lexeme[0])
                        {
                            case '%':
                                typeInt = 2;
                                break;
                            case '&':
                                typeInt = 8;
                                break;
                            case '$':
                                typeInt = 16;
                                break;
                            default:
                                typeInt = 10;
                                break;
                        }
                        uint value = 0;
                        for (int i = typeInt == 10? 0: 1; i < lexeme.Length; i++)
                        {
                            uint convertChar;
                            if (lexeme[i] >= '0' && lexeme[i] <= '9')
                            {
                                convertChar = ((uint)lexeme[i] - (uint)'0');
                            }
                            else
                            {
                                convertChar = ((uint)Char.ToLower(lexeme[i]) - (uint)'a') + 10;
                            }
                            value = (value * typeInt) + convertChar;
                            if (value > 2147483648)
                            {
                                return "ERROR: Overflow integer";
                            }
                        }
                        valueLexeme = value.ToString();
                        break;
                    }
                case "Real":
                    {
                        double value = 0;
                        lexeme = lexeme.Replace(".", ",");
                        lexeme = lexeme.ToUpper();
                        int mantis;
                        switch (lexeme[0])
                        {
                            case '%':
                                lexeme = lexeme.Remove(0, 1);
                                if (lexeme.IndexOf(',') != -1)
                                {
                                    mantis = Convert.ToInt32(lexeme.Substring(0, lexeme.IndexOf(',')), 2);
                                    lexeme = lexeme.Remove(0, lexeme.IndexOf(','));
                                }
                                else
                                {
                                    mantis = Convert.ToInt32(lexeme.Substring(0, lexeme.IndexOf('E')), 2);
                                    lexeme = lexeme.Remove(0, lexeme.IndexOf('E'));
                                }
                                lexeme = mantis.ToString() + lexeme;
                                value = Convert.ToDouble(lexeme);
                                break;
                            case '&':
                                lexeme = lexeme.Remove(0, 1);
                                if (lexeme.IndexOf(',') != -1)
                                {
                                    mantis = Convert.ToInt32(lexeme.Substring(0, lexeme.IndexOf(',')), 8);
                                    lexeme = lexeme.Remove(0, lexeme.IndexOf(','));
                                }
                                else
                                {
                                    mantis = Convert.ToInt32(lexeme.Substring(0, lexeme.IndexOf('E')), 8);
                                    lexeme = lexeme.Remove(0, lexeme.IndexOf('E'));
                                }
                                lexeme = mantis.ToString() + lexeme;
                                value = Convert.ToDouble(lexeme);
                                break;
                            case '$':
                                lexeme = lexeme.Remove(0, 1);
                                if (lexeme.IndexOf(',') != -1)
                                {
                                    mantis = Convert.ToInt32(lexeme.Substring(0, lexeme.IndexOf(',')), 16);
                                    lexeme = lexeme.Remove(0, lexeme.IndexOf(','));
                                }
                                else
                                {
                                    mantis = Convert.ToInt32(lexeme.Substring(0, lexeme.IndexOf('E')), 16);
                                    lexeme = lexeme.Remove(0, lexeme.IndexOf('E'));
                                }
                                lexeme = mantis.ToString() + lexeme;
                                value = Convert.ToDouble(lexeme);
                                break;
                            default:
                                value = Convert.ToDouble(lexeme);
                                break;
                        }
                        valueLexeme = value.ToString("E10").Replace(",", ".");
                        if (valueLexeme == "∞")
                        {
                            valueLexeme = "+Inf";
                        }
                        break;
                    }
                case "Key_word":
                    {
                        valueLexeme = lexeme.ToLower();
                        break;
                    }
                case "End_file":
                    {
                        valueLexeme = lexeme.ToLower();
                        break;
                    }
                case "Operation_sign":
                    {
                        valueLexeme = lexeme;
                        break;
                    }
                case "Separator":
                    {
                        valueLexeme = lexeme;
                        break;
                    }
            }
            return valueLexeme;
        }
    }
}
