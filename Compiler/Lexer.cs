using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Compiler
{
    public struct Lexeme
    {
        public int numberLine;
        public int numberSymbol;
        public string type;
        public string value;
        public string lexeme;
        public Lexeme(int numberLine, int numberSymbol, string type, string value, string lexeme)
        {
            this.numberLine = numberLine;
            this.numberSymbol = numberSymbol;
            this.type = type;
            this.value = value;
            this.lexeme = lexeme;
        }
    }
    internal class Lexer
    {
        public static int currentLine = 1;
        public static int currentSymbol = 1;
        const string END_FILE = "end.";
        static string[] keyWords = new string[] { "and", "array", "as", "asm", "begin", "case", "const", "constructor", "destructor", "div", "do", "downto", "else", "end", "file", "for", "foreach", "function", "goto", "implementation", "if", "in", "inherited", "inline", "interface", "label", "mod", "nil", "not", "object", "of", "operator", "or", "packed", "procedure", "program", "record", "repeat", "self", "set", "shl", "shr", "string", "then", "to", "type", "unit", "until", "uses", "var", "while", "with", "xor", "dispose", "exit", "false", "new", "true", "as", "class", "dispinterface", "except", "exports", "finalization", "finally", "initialization", "inline", "is", "library", "on", "out", "packed", "property", "raise", "resourcestring", "threadvar", "try" };
        const int COUNT_STATUS = 10;
        const int COUNT_SYMBOLS = 256;
        public static string allFileForCheckComments = "";
        static int[,] table = new int[COUNT_STATUS, COUNT_SYMBOLS];
        public static string[] status = { "ERROR", "ERROR", "String", "Indifier", "Integer", "Integer", "Integer", "Integer", "Real", "String", "Key_word", "End_file", "Operation_sign", "Separator", "Not_Lexeme" };
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
         9 - состояние String whith Char
        -------------------------
         10 - состояние Key_word
         11 - состояние End_file
         12 - состояние Operation_sign
         13 - состояние Separator
         14 - состояние Not_Lexeme
        */
        public static void CreateTableDFA()
        {
            //string
            table[1, (int)'\''] = 2;
            table[9, (int)'\''] = 2;
            //string whith char
            table[1, (int)'#'] = 9;
            table[9, (int)'#'] = 9;
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
            //operation sign
            table[1, (int)'='] = 12;
            table[1, (int)':'] = 12;
            table[1, (int)'+'] = 12;
            table[1, (int)'-'] = 12;
            table[1, (int)'*'] = 12;
            table[1, (int)'/'] = 12;
            table[1, (int)'>'] = 12;
            table[1, (int)'<'] = 12;
            //separator
            table[1, (int)','] = 13;
            table[1, (int)';'] = 13;
            table[1, (int)'('] = 13;
            table[1, (int)')'] = 13;
            table[1, (int)'['] = 13;
            table[1, (int)']'] = 13;
            table[1, (int)'.'] = 13;

            for (int i = 0; i < COUNT_SYMBOLS; i++)
            {
                //string
                table[2, i] = 2;
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
                table[9, i] = 9;
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
        public static Lexeme GetFirstLexeme(ref byte[] inputBytes)
        {
            int statusDFA = 1;
            int lexemeLenght = 0;
            int countQuotesInString = 0;

            //string input = Encoding.Default.GetString(inputBytes);

            void CutFirstElementsFromArray(ref byte[] array, int countFirstElements)
            {
                for (int i = 0; i < array.Length - countFirstElements; i++)
                {
                    array[i] = array[i + countFirstElements];
                }
                Array.Resize(ref array, array.Length - countFirstElements);
            }

            string GetString(byte[] input, int start, int end)
            {
                return Encoding.Default.GetString(input[start..end]);
            }

            Lexeme Out(ref byte[] inputBytes)
            {
                Lexeme outLex = new Lexeme(currentLine, currentSymbol, status[statusDFA], GetValueLexeme(status[statusDFA], GetString(inputBytes,0,lexemeLenght)), GetString(inputBytes, 0, lexemeLenght));
                CutFirstElementsFromArray(ref inputBytes, lexemeLenght);
                currentSymbol += lexemeLenght;
                return outLex;
            }

            while (inputBytes[0] == '\n' || inputBytes[0] == '\r' || inputBytes[0] == ' ' || inputBytes[0] == '{' || (inputBytes.Length > 1 && (inputBytes[0] == '/' && inputBytes[1] == '/')))
            {
                switch ((char)inputBytes[0])
                {
                    case '\n':
                        currentLine += 1;
                        CutFirstElementsFromArray(ref inputBytes, 1);
                        break;
                    case '\r':
                        currentSymbol = 1;
                        CutFirstElementsFromArray(ref inputBytes, 1);
                        break;
                    case ' ':
                        currentSymbol += 1;
                        CutFirstElementsFromArray(ref inputBytes, 1);
                        break;
                    case '{':
                        if (GetString(inputBytes, 0, inputBytes.Length).IndexOf('}') != -1)
                        {
                            while (inputBytes[0] != '}')
                            {
                                currentSymbol += 1;
                                if(inputBytes[0] == 13)
                                {
                                    currentLine += 1;
                                    currentSymbol = 1;
                                }
                                if (inputBytes[0] == 10)
                                {
                                    currentSymbol = 1;
                                }
                                CutFirstElementsFromArray(ref inputBytes, 1);
                            }
                            currentSymbol += 1;
                            CutFirstElementsFromArray(ref inputBytes, 1);
                        }
                        else
                        {
                            statusDFA = 0;
                            lexemeLenght = 1;
                            return Out(ref inputBytes);
                        }
                        break;
                }
                if (inputBytes.Length > 1)
                {
                    if(inputBytes[0] == '/' && inputBytes[1] == '/')
                    {
                        while (inputBytes.Length > 0)
                        {
                            if (inputBytes[0] != 13 && inputBytes[0] != 10)
                            {
                                currentSymbol += 1;
                                CutFirstElementsFromArray(ref inputBytes, 1);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                if(inputBytes.Length == 0)
                {
                    statusDFA = 14;
                    return Out(ref inputBytes);
                }
            }

            for (int i = lexemeLenght; i < inputBytes.Length; i++)
            {
                if (inputBytes[i] == 13 || inputBytes[i] == 10)
                {
                    break;
                }
                if (statusDFA < COUNT_STATUS)
                {
                    if (table[statusDFA, Char.ToLower((char)inputBytes[i])] != 0)
                    {
                        lexemeLenght += 1;
                        statusDFA = table[statusDFA, Char.ToLower((char)inputBytes[i])];
                        //string
                        if (statusDFA == 2 && inputBytes[i] == '\'')
                        {
                            countQuotesInString += 1;
                            if (countQuotesInString % 2 == 0)
                            {
                                if (i + 1 < inputBytes.Length && inputBytes[i + 1] == '\'')
                                {
                                    statusDFA = 1;
                                }
                                else
                                {
                                    if (i + 1 < inputBytes.Length && inputBytes[i + 1] == '#')
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
                        //if real have '.'
                        if (statusDFA == 8 && inputBytes[i] == '.')
                        {
                            if (GetString(inputBytes, 0, inputBytes.Length).Substring(0, lexemeLenght).ToLower().IndexOf('e') != -1)
                            {
                                statusDFA = 0;
                                lexemeLenght -= 1;
                                break;
                            }
                            if ((inputBytes[0] == '%' || inputBytes[0] == '&' || inputBytes[0] == '$') && i + 1 < inputBytes.Length && inputBytes[i + 1] >= '0' && inputBytes[i + 1] <= '9')
                            {
                                statusDFA = 0;
                                lexemeLenght -= 1;
                                break;
                            }
                        }
                        //if real have 'e'
                        if (statusDFA == 8 && Char.ToLower((char)inputBytes[i]) == 'e')
                        {
                            if (GetString(inputBytes, 0, lexemeLenght - 1).ToLower().IndexOf('e') != -1)
                            {
                                statusDFA = 0;
                                lexemeLenght -= 1;
                                break;
                            }
                        }
                        //if real have '-' or '+'
                        if (statusDFA == 8 && (inputBytes[i] == '-' || inputBytes[i] == '+'))
                        {
                            if (Char.ToLower((char)inputBytes[i - 1]) != 'e')
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
                return Out(ref inputBytes);
            }
            //if key word or end file
            if (statusDFA == 3)
            {
                foreach (string keyWord in keyWords)
                {
                    if (GetString(inputBytes, 0, lexemeLenght).ToLower() == keyWord)
                    {
                        if(keyWord == "end" && lexemeLenght < inputBytes.Length  && inputBytes[lexemeLenght] == '.')
                        {
                            statusDFA = 11;
                            lexemeLenght += 1;
                            return Out(ref inputBytes);
                        }
                        statusDFA = 10;
                        return Out(ref inputBytes);
                    }
                }
            }
            //if only % or & or $
            if (statusDFA >= 5 && statusDFA <= 7 && lexemeLenght == 1)
            {
                statusDFA = 0;
            }
            //if out system integer
            if (statusDFA >= 4 && statusDFA <= 7 && lexemeLenght < inputBytes.Length)
            {
                if ((inputBytes[lexemeLenght] >= '0' && inputBytes[lexemeLenght] <= '9') || (Char.ToLower((char)inputBytes[lexemeLenght]) >= 'a' && Char.ToLower((char)inputBytes[lexemeLenght]) <= 'z'))
                {
                    lexemeLenght += 1;
                    statusDFA = 0;
                }
            }
            //check real with last 'e' or '-' or '+'
            if (statusDFA == 8 && (Char.ToLower((char)inputBytes[lexemeLenght - 1]) == 'e' || inputBytes[lexemeLenght - 1] == '-' || inputBytes[lexemeLenght - 1] == '+'))
            {
                int offset = 1;
                if(inputBytes[lexemeLenght - 1] == '-' || inputBytes[lexemeLenght - 1] == '+')
                {
                    offset = 2;
                }
                if(GetString(inputBytes, 0, lexemeLenght - offset).ToLower().IndexOf('.') != -1)
                {
                    if(inputBytes[lexemeLenght - offset - 1] == '.')
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
            if (statusDFA == 12)
            {
                if (lexemeLenght < inputBytes.Length &&
                    ((inputBytes[lexemeLenght - 1] == '<' && inputBytes[lexemeLenght] == '<') ||
                    (inputBytes[lexemeLenght - 1] == '>' && inputBytes[lexemeLenght] == '>') ||
                    (inputBytes[lexemeLenght - 1] == '*' && inputBytes[lexemeLenght] == '*') ||
                    (inputBytes[lexemeLenght - 1] == '<' && inputBytes[lexemeLenght] == '>') ||
                    (inputBytes[lexemeLenght - 1] == '<' && inputBytes[lexemeLenght] == '=') ||
                    (inputBytes[lexemeLenght - 1] == '>' && inputBytes[lexemeLenght] == '=') ||
                    (inputBytes[lexemeLenght - 1] == ':' && inputBytes[lexemeLenght] == '=') ||
                    (inputBytes[lexemeLenght - 1] == '+' && inputBytes[lexemeLenght] == '=') ||
                    (inputBytes[lexemeLenght - 1] == '-' && inputBytes[lexemeLenght] == '=') ||
                    (inputBytes[lexemeLenght - 1] == '*' && inputBytes[lexemeLenght] == '=') ||
                    (inputBytes[lexemeLenght - 1] == '/' && inputBytes[lexemeLenght] == '=')))
                {
                    lexemeLenght += 1;
                }
            }
            if (statusDFA == 13)
            {
                if (lexemeLenght < inputBytes.Length && (inputBytes[lexemeLenght - 1] == '.' && inputBytes[lexemeLenght] == '.'))
                { 
                    lexemeLenght += 1;
                }
            }
            if (statusDFA == 8 && lexemeLenght < inputBytes.Length && (inputBytes[lexemeLenght - 1] == '.' && inputBytes[lexemeLenght] == '.'))
            {
                statusDFA = 4;
                lexemeLenght -= 1;
            }
            return Out(ref inputBytes);
        }
        public static string GetValueLexeme(string typeLexeme, string lexeme)
        {
            string valueLexeme = "";
            switch (typeLexeme)
            {
                case "String":
                    {
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
                        if (valueLexeme.Length > 255)
                        {
                            valueLexeme = "ERROR: Overflow string";
                            break;
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
        public static List<Lexeme> GetAllLexeme(string pathIn = "../../../tests/1.txt")
        {
            List<Lexeme> ans = new List<Lexeme>();

            using (FileStream fstream = File.OpenRead(pathIn))
            {
                byte[] input = new byte[fstream.Length];
                fstream.Read(input, 0, input.Length);
                while (input.Length > 0)
                {
                    Lexeme nextLex = GetFirstLexeme(ref input);
                    if(nextLex.type != "Not_Lexeme")
                    {
                        ans.Add(nextLex);
                    }
                    if (nextLex.type == "ERROR" || nextLex.type == "End_file")
                    {
                        break;
                    }
                }
            }
            return ans;
        }
    }
}