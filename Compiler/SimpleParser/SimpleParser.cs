using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class SimpleParser
    {
        readonly Lexer lexer;
        Token currentLex;
        void NextToken()
        {
            currentLex = lexer.GetNextToken();
        }
        public SimpleParser(Lexer lexer)
        {
            this.lexer = lexer;
            NextToken();
        }
        public NodeExpression ParseExpression()
        {
            NodeExpression left = ParseTerm();
            while (currentLex.Type == TokenType.Operation_sign && ((OperationSign)currentLex.Value == OperationSign.Plus || (OperationSign)currentLex.Value == OperationSign.Minus))
            {
                string operation = Lexer.GetStrOperationSign((OperationSign)currentLex.Value);
                NextToken();
                NodeExpression right = ParseTerm();
                left = new NodeBinOp(operation, left, right);
            }
            return left;
        }
        public NodeExpression ParseTerm()
        {
            NodeExpression left = ParseFactor();
            while (currentLex.Type == TokenType.Operation_sign && ((OperationSign)currentLex.Value == OperationSign.Multiply || (OperationSign)currentLex.Value == OperationSign.Divide))
            {
                string operation = Lexer.GetStrOperationSign((OperationSign)currentLex.Value);
                NextToken();
                NodeExpression right = ParseFactor();
                left = new NodeBinOp(operation, left, right);
            }
            return left;
        }
        public NodeExpression ParseFactor()
        {
            if (currentLex.Type == TokenType.Separator && (Separator)currentLex.Value == Separator.OpenParenthesis)
            {
                NodeExpression e;
                NextToken();
                if (currentLex.Type != TokenType.Eof)
                {
                    e = ParseExpression();
                }
                else
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected ')'");
                }
                if (!(currentLex.Type == TokenType.Separator && (Separator)currentLex.Value == Separator.CloseParenthesis))
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected ')'");
                }
                NextToken();
                return e;
            }
            if (currentLex.Type == TokenType.Integer)
            {
                Token factor = currentLex;
                try
                {
                    int result = (int)factor.Value;
                    NextToken();
                    return new NodeInt(result);
                }
                catch (Exception ex)
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, ex.Message);
                }
            }
            if (currentLex.Type == TokenType.String)
            {
                Token factor = currentLex;
                NextToken();
                return new NodeString((string)factor.Value);
            }
            if (currentLex.Type == TokenType.Real)
            {
                Token factor = currentLex;
                try
                {
                    string factorStr = (string)factor.Value;
                    float result = float.Parse(factorStr.Replace(".", ","));
                    NextToken();
                    return new NodeReal(result);
                }
                catch (Exception ex)
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, ex.Message);
                }
            }
            if (currentLex.Type == TokenType.Indifier)
            {
                NodeExpression ans;
                Token factor = currentLex;
                NextToken();
                ans = new NodeVar(new SymVar((string)factor.Value, new SymType("var")));
                return ans;
            }

            throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected factor");
        }
    }
}
