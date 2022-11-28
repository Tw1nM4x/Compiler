using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class SimpleParser
    {
        Lexer lexer;
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
        public ExpressionNode ParseExpression()
        {
            ExpressionNode left = ParseTerm();
            while ((currentLex.Type == TokenType.Operation_sign && (currentLex.Value == "+" || currentLex.Value == "-")))
            {
                string operation = currentLex.Value;
                NextToken();
                ExpressionNode right = ParseTerm();
                left = new NodeBinOp(operation, left, right);
            }
            return left;
        }
        public ExpressionNode ParseTerm()
        {
            ExpressionNode left = ParseFactor(withUnOp: true);
            while ((currentLex.Type == TokenType.Operation_sign && (currentLex.Value == "*" || currentLex.Value == "/")))
            {
                string operation = currentLex.Value;
                NextToken();
                ExpressionNode right = ParseFactor(withUnOp: true);
                left = new NodeBinOp(operation, left, right);
            }
            return left;
        }
        public ExpressionNode ParseFactor(bool withUnOp = false)
        {
            if (currentLex.Type == TokenType.Separator && currentLex.Value == "(")
            {
                ExpressionNode e;
                NextToken();
                if (currentLex.Type != TokenType.Eof)
                {
                    e = ParseExpression();
                }
                else
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected ')'");
                }
                if (!(currentLex.Type == TokenType.Separator && currentLex.Value == ")"))
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
                    int result = int.Parse(factor.Value);
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
                return new NodeString(factor.Value);
            }
            if (currentLex.Type == TokenType.Real)
            {
                Token factor = currentLex;
                try
                {
                    float result = float.Parse(factor.Value.Replace(".", ","));
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
                ExpressionNode ans;
                Token factor = currentLex;
                NextToken();
                ans = new NodeVar(factor.Value);
                return ans;
            }

            throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected factor");
        }
    }
}
