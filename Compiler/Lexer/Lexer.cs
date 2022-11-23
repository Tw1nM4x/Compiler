using System.Text;

namespace Compiler
{
    public class Lexer
    {
        public int CurrentLine = 1;
        public int CurrentSymbol = 1;
        static string[] keyWords = new string[] { "and", "array", "as", "asm", "begin", "case", "const", "constructor", 
            "destructor", "div", "do", "downto", "else", "end", "file", "for", "foreach", "function", "goto", 
            "implementation", "if", "in", "inherited", "inline", "interface", "label", "mod", "nil", "not", "object", 
            "of", "operator", "or", "packed", "procedure", "program", "record", "repeat", "self", "set", "shl", "shr", 
            "string", "then", "to", "type", "unit", "until", "uses", "var", "while", "with", "xor", "dispose", "exit", 
            "false", "new", "true", "as", "class", "dispinterface", "except", "exports", "finalization", "finally", 
            "initialization", "inline", "is", "library", "on", "out", "packed", "property", "raise", "resourcestring", 
            "threadvar", "try" };
        const int COUNT_STATUS = 10;
        const int COUNT_SYMBOLS = 256;
        static int[,] Table = new int[COUNT_STATUS, COUNT_SYMBOLS];
        byte[] Input;
        public Lexer(string path)
        {
            CreateTableDFA();
            CurrentLine = 1;
            CurrentSymbol = 1;
            using (FileStream fstream = File.OpenRead(path))
            {
                this.Input = new byte[fstream.Length];
                fstream.Read(Input, 0, Input.Length);
            }
        }
        static TokenType GetTypeLexeme(int index)
        {
            switch (index)
            {
                case 2:
                    return TokenType.String;
                case 3:
                    return TokenType.Indifier;
                case 4:
                    return TokenType.Integer;
                case 5:
                    return TokenType.Integer;
                case 6:
                    return TokenType.Integer;
                case 7:
                    return TokenType.Integer;
                case 8:
                    return TokenType.Real;
                case 9:
                    return TokenType.String;
                case 10:
                    return TokenType.Key_word;
                case 11:
                    return TokenType.Eof;
                case 12:
                    return TokenType.Operation_sign;
                case 13:
                    return TokenType.Separator;
                case 14:
                    return TokenType.Eof;

            }
            throw new Exception("Incorrect lexeme");
        }
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
         11 - состояние Eof
         12 - состояние Operation_sign
         13 - состояние Separator
        */
        public void CreateTableDFA()
        {
            //string
            Table[1, (int)'\''] = 2;
            Table[9, (int)'\''] = 2;
            //string whith char
            Table[1, (int)'#'] = 9;
            Table[9, (int)'#'] = 9;
            //indifier
            Table[1, (int)'_'] = 3;
            Table[3, (int)'_'] = 3;
            //integer
            Table[1, (int)'%'] = 5;
            Table[1, (int)'&'] = 6;
            Table[1, (int)'$'] = 7;
            //real
            Table[4, (int)'.'] = 8;
            Table[4, (int)'e'] = 8;
            Table[5, (int)'.'] = 8;
            Table[5, (int)'e'] = 8;
            Table[6, (int)'.'] = 8;
            Table[6, (int)'e'] = 8;
            Table[7, (int)'.'] = 8;
            Table[7, (int)'e'] = 8;
            Table[8, (int)'e'] = 8;
            Table[8, (int)'-'] = 8;
            Table[8, (int)'+'] = 8;
            //operation sign
            Table[1, (int)'='] = 12;
            Table[1, (int)':'] = 12;
            Table[1, (int)'+'] = 12;
            Table[1, (int)'-'] = 12;
            Table[1, (int)'*'] = 12;
            Table[1, (int)'/'] = 12;
            Table[1, (int)'>'] = 12;
            Table[1, (int)'<'] = 12;
            //separator
            Table[1, (int)','] = 13;
            Table[1, (int)';'] = 13;
            Table[1, (int)'('] = 13;
            Table[1, (int)')'] = 13;
            Table[1, (int)'['] = 13;
            Table[1, (int)']'] = 13;
            Table[1, (int)'.'] = 13;

            for (int i = 0; i < COUNT_SYMBOLS; i++)
            {
                //string
                Table[2, i] = 2;
            }
            for (int i = (int)'a'; i <= (int)'z'; i++)
            {
                //indifier
                Table[1, i] = 3;
                Table[3, i] = 3;
            }
            for (int i = (int)'0'; i <= (int)'9'; i++)
            {
                //indifier
                Table[3, i] = 3;
                //integer x10
                Table[1, i] = 4;
                Table[4, i] = 4;
                //integer x16
                Table[7, i] = 7;
                //real
                Table[8, i] = 8;
                //string whith char
                Table[9, i] = 9;
            }
            for (int i = (int)'0'; i <= (int)'1'; i++)
            {
                //integer x2
                Table[5, i] = 5;
            }
            for (int i = (int)'0'; i <= (int)'7'; i++)
            {
                //integer x8
                Table[6, i] = 6;
            }
            for (int i = (int)'a'; i <= (int)'f'; i++)
            {
                //integer x16
                Table[7, i] = 7;
            }
        }
        public Token GetNextToken()
        {
            int statusDFA = 1;
            int lexemeLenght = 0;
            int countQuotesInString = 0;

            if (Input.Length == 0)
            {
                statusDFA = 11;
                return Out(ref Input);
            }

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

            Token Out(ref byte[] inputBytes)
            {
                Token outLex = new Token(CurrentLine, CurrentSymbol, GetTypeLexeme(statusDFA), GetValueLexeme(GetTypeLexeme(statusDFA), GetString(inputBytes,0,lexemeLenght)), GetString(inputBytes, 0, lexemeLenght));
                CutFirstElementsFromArray(ref inputBytes, lexemeLenght);
                CurrentSymbol += lexemeLenght;
                return outLex;
            }

            while (Input[0] == '\n' || Input[0] == '\r' || Input[0] == ' ' || Input[0] == '\t' || Input[0] == '{' || (Input.Length > 1 && (Input[0] == '/' && Input[1] == '/')))
            {
                switch ((char)Input[0])
                {
                    case '\n':
                        CurrentLine += 1;
                        CutFirstElementsFromArray(ref Input, 1);
                        break;
                    case '\r':
                        CurrentSymbol = 1;
                        CutFirstElementsFromArray(ref Input, 1);
                        break;
                    case '\t':
                        CurrentSymbol += 4;
                        CutFirstElementsFromArray(ref Input, 1);
                        break;
                    case ' ':
                        CurrentSymbol += 1;
                        CutFirstElementsFromArray(ref Input, 1);
                        break;
                    case '{':
                        if (GetString(Input, 0, Input.Length).IndexOf('}') != -1)
                        {
                            while (Input[0] != '}')
                            {
                                CurrentSymbol += 1;
                                if(Input[0] == 13)
                                {
                                    CurrentLine += 1;
                                    CurrentSymbol = 1;
                                }
                                if (Input[0] == 10)
                                {
                                    CurrentSymbol = 1;
                                }
                                CutFirstElementsFromArray(ref Input, 1);
                            }
                            CurrentSymbol += 1;
                            CutFirstElementsFromArray(ref Input, 1);
                        }
                        else
                        {
                            throw new Exception("Incorrect lexeme");
                        }
                        break;
                }
                if (Input.Length > 1)
                {
                    if(Input[0] == '/' && Input[1] == '/')
                    {
                        while (Input.Length > 0)
                        {
                            if (Input[0] != 13 && Input[0] != 10)
                            {
                                CurrentSymbol += 1;
                                CutFirstElementsFromArray(ref Input, 1);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                if(Input.Length == 0)
                {
                    statusDFA = 14;
                    return Out(ref Input);
                }
            }

            for (int i = lexemeLenght; i < Input.Length; i++)
            {
                if (Input[i] == 13 || Input[i] == 10)
                {
                    break;
                }
                if (statusDFA < COUNT_STATUS)
                {
                    if (Table[statusDFA, Char.ToLower((char)Input[i])] != 0)
                    {
                        lexemeLenght += 1;
                        statusDFA = Table[statusDFA, Char.ToLower((char)Input[i])];
                        //string
                        if (statusDFA == 2 && Input[i] == '\'')
                        {
                            countQuotesInString += 1;
                            if (countQuotesInString % 2 == 0)
                            {
                                if (i + 1 < Input.Length && Input[i + 1] == '\'')
                                {
                                    statusDFA = 1;
                                }
                                else
                                {
                                    if (i + 1 < Input.Length && Input[i + 1] == '#')
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
                        if (statusDFA == 8 && Input[i] == '.')
                        {
                            if (GetString(Input, 0, Input.Length).Substring(0, lexemeLenght).ToLower().IndexOf('e') != -1)
                            {
                                throw new Exception("Incorrect lexeme");
                            }
                            if ((Input[0] == '%' || Input[0] == '&' || Input[0] == '$') && i + 1 < Input.Length && Input[i + 1] >= '0' && Input[i + 1] <= '9')
                            {
                                throw new Exception("Incorrect lexeme");
                            }
                        }
                        //if real have 'e'
                        if (statusDFA == 8 && Char.ToLower((char)Input[i]) == 'e')
                        {
                            if (GetString(Input, 0, lexemeLenght - 1).ToLower().IndexOf('e') != -1)
                            {
                                throw new Exception("Incorrect lexeme");
                            }
                        }
                        //if real have '-' or '+'
                        if (statusDFA == 8 && (Input[i] == '-' || Input[i] == '+'))
                        {
                            if (Char.ToLower((char)Input[i - 1]) != 'e')
                            {
                                throw new Exception("Incorrect lexeme");
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
                throw new Exception("Incorrect lexeme");
            }
            //if key word or end file
            if (statusDFA == 3)
            {
                foreach (string keyWord in keyWords)
                {
                    if (GetString(Input, 0, lexemeLenght).ToLower() == keyWord)
                    {
                        statusDFA = 10;
                        return Out(ref Input);
                    }
                }
            }
            //if only % or & or $
            if (statusDFA >= 5 && statusDFA <= 7 && lexemeLenght == 1)
            {
                throw new Exception("Incorrect lexeme");
            }
            //if out system integer
            if (statusDFA >= 4 && statusDFA <= 7 && lexemeLenght < Input.Length)
            {
                if ((Input[lexemeLenght] >= '0' && Input[lexemeLenght] <= '9') || (Char.ToLower((char)Input[lexemeLenght]) >= 'a' && Char.ToLower((char)Input[lexemeLenght]) <= 'z'))
                {
                    throw new Exception("Incorrect lexeme");
                }
            }
            //check real with last 'e' or '-' or '+'
            if (statusDFA == 8 && (Char.ToLower((char)Input[lexemeLenght - 1]) == 'e' || Input[lexemeLenght - 1] == '-' || Input[lexemeLenght - 1] == '+'))
            {
                int offset = 1;
                if(Input[lexemeLenght - 1] == '-' || Input[lexemeLenght - 1] == '+')
                {
                    offset = 2;
                }
                if(GetString(Input, 0, lexemeLenght - offset).ToLower().IndexOf('.') != -1)
                {
                    if(Input[lexemeLenght - offset - 1] == '.')
                    {
                        throw new Exception("Incorrect lexeme");
                    }
                    else
                    {
                        lexemeLenght -= offset;
                    }
                }
                else
                {
                    throw new Exception("Incorrect lexeme");
                }
            }
            if (statusDFA == 12)
            {
                if (lexemeLenght < Input.Length &&
                    ((Input[lexemeLenght - 1] == '<' && Input[lexemeLenght] == '<') ||
                    (Input[lexemeLenght - 1] == '>' && Input[lexemeLenght] == '>') ||
                    (Input[lexemeLenght - 1] == '*' && Input[lexemeLenght] == '*') ||
                    (Input[lexemeLenght - 1] == '<' && Input[lexemeLenght] == '>') ||
                    (Input[lexemeLenght - 1] == '<' && Input[lexemeLenght] == '=') ||
                    (Input[lexemeLenght - 1] == '>' && Input[lexemeLenght] == '=') ||
                    (Input[lexemeLenght - 1] == ':' && Input[lexemeLenght] == '=') ||
                    (Input[lexemeLenght - 1] == '+' && Input[lexemeLenght] == '=') ||
                    (Input[lexemeLenght - 1] == '-' && Input[lexemeLenght] == '=') ||
                    (Input[lexemeLenght - 1] == '*' && Input[lexemeLenght] == '=') ||
                    (Input[lexemeLenght - 1] == '/' && Input[lexemeLenght] == '=')))
                {
                    lexemeLenght += 1;
                }
            }
            if (statusDFA == 13)
            {
                if (lexemeLenght < Input.Length && (Input[lexemeLenght - 1] == '.' && Input[lexemeLenght] == '.'))
                { 
                    lexemeLenght += 1;
                }
            }
            if (statusDFA == 8 && lexemeLenght < Input.Length && (Input[lexemeLenght - 1] == '.' && Input[lexemeLenght] == '.'))
            {
                statusDFA = 4;
                lexemeLenght -= 1;
            }
            return Out(ref Input);
        }
        public static string GetValueLexeme(TokenType typeLexeme, string lexeme)
        {
            string valueLexeme = "";
            switch (typeLexeme)
            {
                case TokenType.String:
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
                                        throw new Exception("Overflow symbol in string");
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
                                            throw new Exception("Overflow symbol in string");
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
                                throw new Exception("Overflow symbol in string");
                            }
                            valueLexeme += (char)symbol;
                            symbolStr = "";
                        }
                        if (valueLexeme.Length > 255)
                        {
                            throw new Exception("Overflow string");
                        }
                        break;
                    }
                case TokenType.Indifier:
                    {
                        valueLexeme = lexeme;
                        if (valueLexeme.Length > 127)
                        {
                            throw new Exception("Overflow indifier");
                        }
                        break;
                    }
                case TokenType.Integer:
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
                                throw new Exception("Overflow integer");
                            }
                        }
                        valueLexeme = value.ToString();
                        break;
                    }
                case TokenType.Real:
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
                case TokenType.Key_word:
                    {
                        valueLexeme = lexeme.ToLower();
                        break;
                    }
                case TokenType.Operation_sign:
                    {
                        valueLexeme = lexeme;
                        break;
                    }
                case TokenType.Separator:
                    {
                        valueLexeme = lexeme;
                        break;
                    }
            }
            return valueLexeme;
        }
    }
}