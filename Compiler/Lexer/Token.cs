using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public enum TokenType
    {
        String,
        Identifier,
        Integer,
        Real,
        Key_word,
        Operation_sign,
        Separator,
        Eof
    }
    public enum KeyWord
    {
        AND,
        ARRAY,
        AS,
        ASM,
        BEGIN,
        CASE,
        CONST,
        CONSTRUCTOR,
        DESTRUCTOR,
        DIV,
        DO,
        DOWNTO,
        ELSE,
        END,
        FILE,
        FOR,
        FOREACH,
        FUNCTION,
        GOTO,
        IMPLEMENTATION,
        IF,
        IN,
        INHERITED,
        INLINE,
        INTERFACE,
        LABEL,
        MOD,
        NIL,
        NOT,
        OBJECT,
        OF,
        OPERATOR,
        OR,
        PACKED,
        PROCEDURE,
        PROGRAM,
        RECORD,
        REPEAT,
        SELF,
        SET,
        SHL,
        SHR,
        STRING,
        THEN,
        TO,
        TYPE,
        UNIT,
        UNTIL,
        USES,
        VAR,
        WHILE,
        WITH,
        XOR,
        DISPOSE,
        EXIT,
        FALSE,
        NEW,
        TRUE,
        CLASS,
        DISPINTERFACE,
        EXCEPT,
        EXPORTS,
        FINALIZATION,
        FINALLY,
        INITIALIZATION,
        IS,
        LIBRARY,
        ON,
        OUT,
        PROPERTY,
        RAISE,
        RESOURCESTRING,
        THREADVAR,
        TRY
    }
    public enum OperationSign
    {
        Unidentified,
        Equal, // =
        Colon, // :
        Plus, // +
        Minus, // -
        Multiply, // *
        Divide, // /
        Greater, //>
        Less, //<
        At, // @
        BitwiseShiftToTheLeft, // <<
        BitwiseShiftToTheRight, //>>
        NotEqual, //<>
        SymmetricalDifference, // ><
        LessOrEqual, // <=
        GreaterOrEqual, // >=
        Assignment, // :=
        Addition, // +=
        Subtraction, // -=
        Multiplication, // *=
        Division, // /=
        PointRecord, // .
    }
    public enum Separator
    {
        Unidentified,
        Comma, // ,
        Semiсolon, // ;
        OpenParenthesis, // (
        CloseParenthesis, // )
        OpenBracket, // [
        CloseBracket, // ]
        Point, // .
        DoublePoint // ..
    }
    public struct Token
    {
        public int NumberLine;
        public int NumberSymbol;
        public TokenType Type;
        public object Value;
        public string Source;
        public Token(int numberLine, int numberSymbol, TokenType type, object value, string lexeme)
        {
            this.NumberLine = numberLine;
            this.NumberSymbol = numberSymbol;
            this.Type = type;
            this.Value = value;
            this.Source = lexeme;
        }
        public override string ToString()
        {
            return $"{NumberLine} {NumberSymbol} {Type} {Value} {Source}";
        }
    }
}
