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
        public NodeExpression ParseExpression()
        {
            NodeExpression left = ParseTerm();
            while ((currentLex.Type == TokenType.Operation_sign && (currentLex.Value == "+" || currentLex.Value == "-")))
            {
                string operation = currentLex.Value;
                NextToken();
                NodeExpression right = ParseTerm();
                left = new NodeBinOp(operation, left, right);
            }
            return left;
        }
        public NodeExpression ParseTerm()
        {
            NodeExpression left = ParseFactor(withUnOp: true);
            while ((currentLex.Type == TokenType.Operation_sign && (currentLex.Value == "*" || currentLex.Value == "/")))
            {
                string operation = currentLex.Value;
                NextToken();
                NodeExpression right = ParseFactor(withUnOp: true);
                left = new NodeBinOp(operation, left, right);
            }
            return left;
        }
        public NodeExpression ParseFactor(bool withUnOp = false)
        {
            if (currentLex.Type == TokenType.Separator && currentLex.Value == "(")
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
                NodeExpression ans;
                Token factor = currentLex;
                NextToken();
                ans = new NodeVar(new SymVar(factor.Value, new SymType("var")));
                return ans;
            }

            throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected factor");
        }
    }
}
