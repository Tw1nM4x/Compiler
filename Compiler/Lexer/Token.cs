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
        Indifier,
        Integer,
        Real,
        Key_word,
        Operation_sign,
        Separator,
        Eof
    }
    public struct Token
    {
        public int NumberLine;
        public int NumberSymbol;
        public TokenType Type;
        public string Value;
        public string Source;
        public Token(int numberLine, int numberSymbol, TokenType type, string value, string lexeme)
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
