using System.Text;

namespace Compiler
{
    public class Lexer
    {
        public int CurrentLine = 1;
        public int CurrentSymbol = 1;
        Token lastToken;
        private const int COUNT_STATUS = 10;
        private const int COUNT_SYMBOLS = 256;
        private static string[] keyWords = new string[] { "and", "array", "as", "asm", "begin", "case", "const", "constructor", 
            "destructor", "div", "do", "downto", "else", "end", "file", "for", "foreach", "function", "goto", 
            "implementation", "if", "in", "inherited", "inline", "interface", "label", "mod", "nil", "not", "object", 
            "of", "operator", "or", "packed", "procedure", "program", "record", "repeat", "self", "set", "shl", "shr", 
            "string", "then", "to", "type", "unit", "until", "uses", "var", "while", "with", "xor", "dispose", "exit", 
            "false", "new", "true", "as", "class", "dispinterface", "except", "exports", "finalization", "finally", 
            "initialization", "inline", "is", "library", "on", "out", "packed", "property", "raise", "resourcestring", 
            "threadvar", "try" };
        private static int[,] tableDFA = new int[COUNT_STATUS, COUNT_SYMBOLS];
        private byte[] input;
        enum StatusDFA
        {
            Error = 0,
            StartStatus = 1,
            String = 2,
            Identifier = 3,
            Integer10 = 4,
            Integer2 = 5,
            Integer8 = 6,
            Integer16 = 7,
            Real = 8,
            String_with_Char = 9,
            Key_word = 10,
            Eof = 11,
            Operation_sign = 12,
            Separator = 13,
            Eof_ = 14
        }
        public Lexer(string path)
        {
            CreateTableDFA();
            CurrentLine = 1;
            CurrentSymbol = 1;
            using (FileStream fstream = File.OpenRead(path))
            {
                this.input = new byte[fstream.Length];
                fstream.Read(input, 0, input.Length);
            }
        }
        public Token GetLastToken()
        {
            return lastToken;
        }
        static OperationSign GetEnumOperationSign(string operationSignStr)
        {
            switch (operationSignStr)
            {
                case "=":
                    return OperationSign.Equal;
                case ":":
                    return  OperationSign.Colon;
                case "+":
                    return  OperationSign.Plus;
                case "-":
                    return  OperationSign.Minus;
                case "*":
                    return  OperationSign.Multiply;
                case "/":
                    return  OperationSign.Divide;
                case ">":
                    return  OperationSign.Greater;
                case "<":
                    return  OperationSign.Less;
                case "@":
                    return  OperationSign.At;
                case "<<":
                    return  OperationSign.BitwiseShiftToTheLeft;
                case ">>":
                    return  OperationSign.BitwiseShiftToTheRight;
                case "<>":
                    return  OperationSign.NotEqual;
                case "><":
                    return  OperationSign.SymmetricalDifference;
                case "<=":
                    return  OperationSign.LessOrEqual;
                case ">=":
                    return  OperationSign.GreaterOrEqual;
                case ":=":
                    return  OperationSign.Assignment;
                case "+=":
                    return  OperationSign.Addition;
                case "-=":
                    return  OperationSign.Subtraction;
                case "*=":
                    return  OperationSign.Multiplication;
                case "/=":
                    return  OperationSign.Division;
                default:
                    return OperationSign.Unidentified;
            }
        }
        public static string GetStrOperationSign(OperationSign operationSignOs)
        {
            switch (operationSignOs)
            {
                case OperationSign.Equal:
                    return "=";
                case OperationSign.Colon:
                    return ":";
                case OperationSign.Plus:
                    return "+";
                case OperationSign.Minus:
                    return "-";
                case OperationSign.Multiply:
                    return "*";
                case OperationSign.Divide:
                    return "/";
                case OperationSign.Greater:
                    return ">";
                case OperationSign.Less:
                    return "<";
                case OperationSign.At:
                    return "@";
                case OperationSign.BitwiseShiftToTheLeft:
                    return "<<";
                case OperationSign.BitwiseShiftToTheRight:
                    return ">>";
                case OperationSign.NotEqual:
                    return "<>";
                case OperationSign.SymmetricalDifference:
                    return "><";
                case OperationSign.LessOrEqual:
                    return "<=";
                case OperationSign.GreaterOrEqual:
                    return ">=";
                case OperationSign.Assignment:
                    return ":=";
                case OperationSign.Addition:
                    return "+=";
                case OperationSign.Subtraction:
                    return "-=";
                case OperationSign.Multiplication:
                    return "*=";
                case OperationSign.Division:
                    return "/=";
                case OperationSign.PointRecord:
                    return ".";
                default:
                    return "";
            }
        }
        static Separator GetEnumSeparator(string operationSignStr)
        {
            switch (operationSignStr)
            {
                case ",":
                    return Separator.Comma;
                case ";":
                    return Separator.Semiсolon;
                case "(":
                    return Separator.OpenParenthesis;
                case ")":
                    return Separator.CloseParenthesis;
                case "[":
                    return Separator.OpenBracket;
                case "]":
                    return Separator.CloseBracket;
                case ".":
                    return Separator.Point;
                case "..":
                    return Separator.DoublePoint;
                default:
                    return Separator.Unidentified;
            }
        }
        public static string GetStrSeparator(Separator operationSignOs)
        {
            switch (operationSignOs)
            {
                case Separator.Comma:
                    return ",";
                case Separator.Semiсolon:
                    return ";";
                case Separator.OpenParenthesis:
                    return "(";
                case Separator.CloseParenthesis:
                    return ")";
                case Separator.OpenBracket:
                    return "[";
                case Separator.CloseBracket:
                    return "]";
                case Separator.Point:
                    return ".";
                case Separator.DoublePoint:
                    return "..";
                default:
                    return "";
            }
        }
        TokenType GetTokenType(int index)
        {
            switch (index)
            {
                case 2:
                    return TokenType.String;
                case 3:
                    return TokenType.Identifier;
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
            throw new ExceptionWithPosition(CurrentLine, CurrentSymbol,"Incorrect lexeme");
        }
        public void CreateTableDFA()
        {
            //string
            tableDFA[(int)StatusDFA.StartStatus,(int)'\''] = (int)StatusDFA.String;
            tableDFA[9, (int)'\''] = (int)StatusDFA.String;
            //string whith char
            tableDFA[(int)StatusDFA.StartStatus,(int)'#'] = (int)StatusDFA.String_with_Char;
            tableDFA[(int)StatusDFA.String_with_Char, (int)'#'] = (int)StatusDFA.String_with_Char;
            //Identifier
            tableDFA[(int)StatusDFA.StartStatus,(int)'_'] = (int)StatusDFA.Identifier;
            tableDFA[(int)StatusDFA.Identifier,(int)'_'] = (int)StatusDFA.Identifier;
            //integer
            tableDFA[(int)StatusDFA.StartStatus,(int)'%'] = (int)StatusDFA.Integer2;
            tableDFA[(int)StatusDFA.StartStatus,(int)'&'] = (int)StatusDFA.Integer8;
            tableDFA[(int)StatusDFA.StartStatus,(int)'$'] = (int)StatusDFA.Integer16;
            //real
            tableDFA[(int)StatusDFA.Integer10, (int)'.'] = (int)StatusDFA.Real;
            tableDFA[(int)StatusDFA.Integer10, (int)'e'] = (int)StatusDFA.Real;
            tableDFA[(int)StatusDFA.Integer2, (int)'.'] = (int)StatusDFA.Real;
            tableDFA[(int)StatusDFA.Integer2, (int)'e'] = (int)StatusDFA.Real;
            tableDFA[(int)StatusDFA.Integer8, (int)'.'] = (int)StatusDFA.Real;
            tableDFA[(int)StatusDFA.Integer8, (int)'e'] = (int)StatusDFA.Real;
            tableDFA[(int)StatusDFA.Integer16, (int)'.'] = (int)StatusDFA.Real;
            tableDFA[(int)StatusDFA.Integer16, (int)'e'] = (int)StatusDFA.Real;
            tableDFA[(int)StatusDFA.Real, (int)'e'] = (int)StatusDFA.Real;
            tableDFA[(int)StatusDFA.Real, (int)'-'] = (int)StatusDFA.Real;
            tableDFA[(int)StatusDFA.Real, (int)'+'] = (int)StatusDFA.Real;
            //operation sign
            tableDFA[(int)StatusDFA.StartStatus,(int)'='] = (int)StatusDFA.Operation_sign;
            tableDFA[(int)StatusDFA.StartStatus,(int)':'] = (int)StatusDFA.Operation_sign;
            tableDFA[(int)StatusDFA.StartStatus,(int)'+'] = (int)StatusDFA.Operation_sign;
            tableDFA[(int)StatusDFA.StartStatus,(int)'-'] = (int)StatusDFA.Operation_sign;
            tableDFA[(int)StatusDFA.StartStatus,(int)'*'] = (int)StatusDFA.Operation_sign;
            tableDFA[(int)StatusDFA.StartStatus,(int)'/'] = (int)StatusDFA.Operation_sign;
            tableDFA[(int)StatusDFA.StartStatus,(int)'>'] = (int)StatusDFA.Operation_sign;
            tableDFA[(int)StatusDFA.StartStatus,(int)'<'] = (int)StatusDFA.Operation_sign;
            tableDFA[(int)StatusDFA.StartStatus,(int)'@'] = (int)StatusDFA.Operation_sign;
            //separator
            tableDFA[(int)StatusDFA.StartStatus,(int)','] = (int)StatusDFA.Separator;
            tableDFA[(int)StatusDFA.StartStatus,(int)';'] = (int)StatusDFA.Separator;
            tableDFA[(int)StatusDFA.StartStatus,(int)'('] = (int)StatusDFA.Separator;
            tableDFA[(int)StatusDFA.StartStatus,(int)')'] = (int)StatusDFA.Separator;
            tableDFA[(int)StatusDFA.StartStatus,(int)'['] = (int)StatusDFA.Separator;
            tableDFA[(int)StatusDFA.StartStatus,(int)']'] = (int)StatusDFA.Separator;
            tableDFA[(int)StatusDFA.StartStatus,(int)'.'] = (int)StatusDFA.Separator;

            for (int i = 0; i < COUNT_SYMBOLS; i++)
            {
                //string
                tableDFA[(int)StatusDFA.String,i] = (int)StatusDFA.String;
            }
            for (int i = (int)'a'; i <= (int)'z'; i++)
            {
                //Identifier
                tableDFA[(int)StatusDFA.StartStatus,i] = (int)StatusDFA.Identifier;
                tableDFA[(int)StatusDFA.Identifier,i] = (int)StatusDFA.Identifier;
            }
            for (int i = (int)'0'; i <= (int)'9'; i++)
            {
                //Identifier
                tableDFA[(int)StatusDFA.Identifier,i] = (int)StatusDFA.Identifier;
                //integer x10
                tableDFA[(int)StatusDFA.StartStatus,i] = (int)StatusDFA.Integer10;
                tableDFA[(int)StatusDFA.Integer10, i] = (int)StatusDFA.Integer10;
                //integer x16
                tableDFA[(int)StatusDFA.Integer16, i] = (int)StatusDFA.Integer16;
                //real
                tableDFA[(int)StatusDFA.Real, i] = (int)StatusDFA.Real;
                //string whith char
                tableDFA[(int)StatusDFA.String_with_Char, i] = (int)StatusDFA.String_with_Char;
            }
            for (int i = (int)'0'; i <= (int)'1'; i++)
            {
                //integer x2
                tableDFA[(int)StatusDFA.Integer2, i] = (int)StatusDFA.Integer2;
            }
            for (int i = (int)'0'; i <= (int)'7'; i++)
            {
                //integer x8
                tableDFA[(int)StatusDFA.Integer8, i] = (int)StatusDFA.Integer8;
            }
            for (int i = (int)'a'; i <= (int)'f'; i++)
            {
                //integer x16
                tableDFA[(int)StatusDFA.Integer16, i] = (int)StatusDFA.Integer16;
            }
        }
        public Token GetNextToken()
        {
            int currentStatusDFA = (int)StatusDFA.StartStatus;
            int lexemeLenght = 0;
            int countQuotesInString = 0;

            if (input.Length == 0)
            {
                currentStatusDFA = (int)StatusDFA.Eof;
                return Out(ref input);
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
                Token outLex = new Token(CurrentLine, CurrentSymbol, GetTokenType(currentStatusDFA), GetValueLexeme(GetTokenType(currentStatusDFA), GetString(inputBytes,0,lexemeLenght)), GetString(inputBytes, 0, lexemeLenght));
                CutFirstElementsFromArray(ref inputBytes, lexemeLenght);
                CurrentSymbol += lexemeLenght;
                lastToken = outLex;
                return outLex;
            }

            while (input[0] == '\n' || input[0] == '\r' || input[0] == ' ' || input[0] == '\t' || input[0] == '{' || (input.Length > 1 && (input[0] == '/' && input[1] == '/')))
            {
                switch ((char)input[0])
                {
                    case '\n':
                        CurrentLine += 1;
                        CutFirstElementsFromArray(ref input, 1);
                        break;
                    case '\r':
                        CurrentSymbol = 1;
                        CutFirstElementsFromArray(ref input, 1);
                        break;
                    case '\t':
                        CurrentSymbol += 4;
                        CutFirstElementsFromArray(ref input, 1);
                        break;
                    case ' ':
                        CurrentSymbol += 1;
                        CutFirstElementsFromArray(ref input, 1);
                        break;
                    case '{':
                        if (GetString(input, 0, input.Length).IndexOf('}') != -1)
                        {
                            while (input[0] != '}')
                            {
                                CurrentSymbol += 1;
                                if(input[0] == 13)
                                {
                                    CurrentLine += 1;
                                    CurrentSymbol = 1;
                                }
                                if (input[0] == 10)
                                {
                                    CurrentSymbol = 1;
                                }
                                CutFirstElementsFromArray(ref input, 1);
                            }
                            CurrentSymbol += 1;
                            CutFirstElementsFromArray(ref input, 1);
                        }
                        else
                        {
                            throw new ExceptionWithPosition(CurrentLine, CurrentSymbol,"Incorrect lexeme");
                        }
                        break;
                }
                if (input.Length > 1)
                {
                    if(input[0] == '/' && input[1] == '/')
                    {
                        while (input.Length > 0)
                        {
                            if (input[0] != 13 && input[0] != 10)
                            {
                                CurrentSymbol += 1;
                                CutFirstElementsFromArray(ref input, 1);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                if(input.Length == 0)
                {
                    currentStatusDFA = 14;
                    return Out(ref input);
                }
            }

            for (int i = lexemeLenght; i < input.Length; i++)
            {
                if (input[i] == '\n' || input[i] == '\r')
                {
                    break;
                }
                if (currentStatusDFA < COUNT_STATUS)
                {
                    if (tableDFA[currentStatusDFA, Char.ToLower((char)input[i])] != (int)StatusDFA.Error)
                    {
                        lexemeLenght += 1;
                        currentStatusDFA = tableDFA[currentStatusDFA, Char.ToLower((char)input[i])];
                        //string
                        if (currentStatusDFA == (int)StatusDFA.String && input[i] == '\'')
                        {
                            countQuotesInString += 1;
                            if (countQuotesInString % 2 == 0)
                            {
                                if (i + 1 < input.Length && input[i + 1] == '\'')
                                {
                                    currentStatusDFA = (int)StatusDFA.StartStatus;
                                }
                                else
                                {
                                    if (i + 1 < input.Length && input[i + 1] == '#')
                                    {
                                        currentStatusDFA = (int)StatusDFA.StartStatus;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        //if real have '.'
                        if (currentStatusDFA == (int)StatusDFA.Real && input[i] == '.')
                        {
                            if (GetString(input, 0, input.Length).Substring(0, lexemeLenght).ToLower().IndexOf('e') != -1)
                            {
                                throw new ExceptionWithPosition(CurrentLine, CurrentSymbol,"Incorrect lexeme");
                            }
                            if ((input[0] == '%' || input[0] == '&' || input[0] == '$') && i + 1 < input.Length && input[i + 1] >= '0' && input[i + 1] <= '9')
                            {
                                throw new ExceptionWithPosition(CurrentLine, CurrentSymbol,"Incorrect lexeme");
                            }
                        }
                        //if real have 'e'
                        if (currentStatusDFA == (int)StatusDFA.Real && Char.ToLower((char)input[i]) == 'e')
                        {
                            if (GetString(input, 0, lexemeLenght - 1).ToLower().IndexOf('e') != -1)
                            {
                                throw new ExceptionWithPosition(CurrentLine, CurrentSymbol,"Incorrect lexeme");
                            }
                        }
                        //if real have '-' or '+'
                        if (currentStatusDFA == (int)StatusDFA.Real && (input[i] == '-' || input[i] == '+'))
                        {
                            if (Char.ToLower((char)input[i - 1]) != 'e')
                            {
                                throw new ExceptionWithPosition(CurrentLine, CurrentSymbol,"Incorrect lexeme");
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            //string
            if (currentStatusDFA == (int)StatusDFA.String && countQuotesInString % 2 == 1)
            {
                throw new ExceptionWithPosition(CurrentLine, CurrentSymbol,"Incorrect lexeme");
            }
            //if key word
            if (currentStatusDFA == (int)StatusDFA.Identifier)
            {
                if(Enum.TryParse((GetString(input, 0, lexemeLenght).ToUpper()), out KeyWord res))
                {
                    currentStatusDFA = (int)StatusDFA.Key_word;
                    return Out(ref input);
                }
            }
            //if only % or & or $
            if (currentStatusDFA >= (int)StatusDFA.Integer2 && currentStatusDFA <= (int)StatusDFA.Integer16 && lexemeLenght == 1)
            {
                throw new ExceptionWithPosition(CurrentLine, CurrentSymbol,"Incorrect lexeme");
            }
            //if out system integer
            if (currentStatusDFA >= (int)StatusDFA.Integer10 && currentStatusDFA <= (int)StatusDFA.Integer16 && lexemeLenght < input.Length)
            {
                if ((input[lexemeLenght] >= '0' && input[lexemeLenght] <= '9') || (Char.ToLower((char)input[lexemeLenght]) >= 'a' && Char.ToLower((char)input[lexemeLenght]) <= 'z'))
                {
                    throw new ExceptionWithPosition(CurrentLine, CurrentSymbol,"Incorrect lexeme");
                }
            }
            //check real with last 'e' or '-' or '+'
            if (currentStatusDFA == (int)StatusDFA.Real && (Char.ToLower((char)input[lexemeLenght - 1]) == 'e' || input[lexemeLenght - 1] == '-' || input[lexemeLenght - 1] == '+'))
            {
                int offset = 1;
                if(input[lexemeLenght - 1] == '-' || input[lexemeLenght - 1] == '+')
                {
                    offset = 2;
                }
                if(GetString(input, 0, lexemeLenght - offset).ToLower().IndexOf('.') != -1)
                {
                    if(input[lexemeLenght - offset - 1] == '.')
                    {
                        throw new ExceptionWithPosition(CurrentLine, CurrentSymbol,"Incorrect lexeme");
                    }
                    else
                    {
                        lexemeLenght -= offset;
                    }
                }
                else
                {
                    throw new ExceptionWithPosition(CurrentLine, CurrentSymbol,"Incorrect lexeme");
                }
            }
            if (currentStatusDFA == (int)StatusDFA.Operation_sign)
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
            if (currentStatusDFA == (int)StatusDFA.Separator)
            {
                if (lexemeLenght < input.Length && (input[lexemeLenght - 1] == '.' && input[lexemeLenght] == '.'))
                { 
                    lexemeLenght += 1;
                }
            }
            if (currentStatusDFA == (int)StatusDFA.Real && lexemeLenght < input.Length && (input[lexemeLenght - 1] == '.' && input[lexemeLenght] == '.'))
            {
                currentStatusDFA = (int)StatusDFA.Integer10;
                lexemeLenght -= 1;
            }
            return Out(ref input);
        }
        public object GetValueLexeme(TokenType typeLexeme, string lexeme)
        {
            object valueLexeme = "";
            switch (typeLexeme)
            {
                case TokenType.String:
                    {
                        string valueLexemeString = "";
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
                                        throw new ExceptionWithPosition(CurrentLine, CurrentSymbol,"Overflow symbol in string");
                                    }
                                    valueLexemeString += (char)symbol;
                                    symbolStr = "";
                                }
                                nowSymbol = false;
                                if (i + 1 < lexeme.Length && lexeme[i + 1] == '\'')
                                {
                                    valueLexemeString += lexeme[i];
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
                                            throw new ExceptionWithPosition(CurrentLine, CurrentSymbol,"Overflow symbol in string");
                                        }
                                        valueLexemeString += (char)symbol;
                                        symbolStr = "";
                                    }
                                    nowSymbol = true;
                                }
                                else
                                {
                                    if (!nowSymbol)
                                    {
                                        valueLexemeString += lexeme[i];
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
                                throw new ExceptionWithPosition(CurrentLine, CurrentSymbol,"Overflow symbol in string");
                            }
                            valueLexemeString += (char)symbol;
                            symbolStr = "";
                        }
                        if (valueLexemeString.Length > 255)
                        {
                            throw new ExceptionWithPosition(CurrentLine, CurrentSymbol,"Overflow string");
                        }
                        valueLexeme = valueLexemeString;
                        break;
                    }
                case TokenType.Identifier:
                    {
                        string valueLexemeString = "";
                        valueLexemeString = lexeme.ToLower();
                        if (valueLexemeString.Length > 127)
                        {
                            throw new ExceptionWithPosition(CurrentLine, CurrentSymbol,"Overflow Identifier");
                        }
                        valueLexeme = valueLexemeString;
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
                                throw new ExceptionWithPosition(CurrentLine, CurrentSymbol,"Overflow integer");
                            }
                        }
                        int valueInt = (int)value;
                        valueLexeme = valueInt;
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
                        valueLexeme = value;
                        break;
                    }
                case TokenType.Key_word:
                    {
                        Enum.TryParse(lexeme.ToUpper(), out KeyWord res);
                        valueLexeme = res;
                        break;
                    }
                case TokenType.Operation_sign:
                    {
                        OperationSign res = GetEnumOperationSign(lexeme);
                        valueLexeme = res;
                        break;
                    }
                case TokenType.Separator:
                    {
                        Separator res = GetEnumSeparator(lexeme);
                        valueLexeme = res;
                        break;
                    }
            }
            return valueLexeme;
        }
    }
}