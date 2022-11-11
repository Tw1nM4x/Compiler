using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class Node
    {
        public string type;
        public string value;
        public Node? left;
        public Node? right;
        public Node(string type, string value, Node? left, Node? right)
        {
            this.type = type;
            this.value = value;
            this.left = left;
            this.right = right;
        }

    }
    internal class Parser
    {
        public static Lexeme currentLex;
        public static Node Parse(ref byte[] inputBytes)
        {
            currentLex = Lexer.GetFirstLexeme(ref inputBytes);
            Node res;
            if (currentLex.type == Lexer.status[10])
            {
                switch (currentLex.value)
                {
                    case "program":
                        res = new Node("StartProgram", "program", ParseProgram(ref inputBytes), null);
                        break;
                    case "var":
                        res = new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: program has not started", null, null);
                        break;
                    case "begin":
                        res = new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: program has not started", null, null);
                        break;
                    default:
                        res = new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: program has not started", null, null);
                        break;
                }
            }
            else
            {
                res = new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: program has not started", null, null);
            }
            return res;
        }
        public static Node ParseProgram(ref byte[] inputBytes)
        {
            currentLex = Lexer.GetFirstLexeme(ref inputBytes);
            Node res;
            if (currentLex.type == Lexer.status[3])
            {
                res = new Node("NameProgram", currentLex.value, null, null);
            }
            else
            {
                res = new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected Indifier", null, null);
            }
            currentLex = Lexer.GetFirstLexeme(ref inputBytes);
            if (!(currentLex.type == Lexer.status[13] && currentLex.value == ";"))
            {
                res = new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected ';'", null, null);
            }
            return res;
        }
        public static Node ParseSimpleExpression(ref byte[] inputBytes)
        {
            Node left = ParseTerm(ref inputBytes);
            while (currentLex.type == Lexer.status[12] && (currentLex.value == "+" || currentLex.value == "-"))
            {
                Lexeme operation = currentLex;
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                }
                Node right = ParseTerm(ref inputBytes);
                left = new Node("BinOperation", operation.value, left, right);
                if (right.type == "ERROR")
                {
                    return new Node("ERROR", right.value, null, null);
                }
            }
            return left;
        }
        public static Node ParseTerm(ref byte[] inputBytes)
        {
            Node left = ParseFactor(ref inputBytes);
            if(left.type != "ERROR")
            {
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                    Node check = ParseFactor(ref inputBytes);
                    if (check.type != "ERROR")
                    {
                        return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - 1}) ERROR: don't have operation sign", null, null);
                    }
                }
                while (currentLex.type == Lexer.status[12] && (currentLex.value == "*" || currentLex.value == "/"))
                {
                    Lexeme operation = currentLex;
                    if (inputBytes.Length > 0)
                    {
                        currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                    }
                    Node right = ParseFactor(ref inputBytes);
                    if (inputBytes.Length > 0)
                    {
                        currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                    }
                    left = new Node("BinOperation", operation.value, left, right);
                    if (right.type == "ERROR")
                    {
                        return new Node("ERROR", right.value, null, null);
                    }
                }
            }
            return left;
        }
        public static Node ParseFactor(ref byte[] inputBytes)
        {
            if (currentLex.type == Lexer.status[13] && currentLex.value == "(")
            {
                Node e;
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                    e = ParseSimpleExpression(ref inputBytes);
                }
                else
                {
                    e = new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - 1}) ERROR: don't have ')'", null, null);
                }
                if(!(currentLex.type == Lexer.status[13] && currentLex.value == ")"))
                {
                    return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - 1}) ERROR: don't have ')'", null, null);
                }
                return e;
            }
            if (currentLex.type == Lexer.status[8] || currentLex.type == Lexer.status[4] || currentLex.type == Lexer.status[3])
            {
                Lexeme factor = currentLex;
                return new Node(factor.type, factor.value, null, null);
            }

            return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: don't have factor", null, null);
        }
        public static Node ParseExpression(ref byte[] inputBytes)
        {
            Node left = ParseSimpleExpression(ref inputBytes);
            if (currentLex.type == Lexer.status[12] && (currentLex.value == "<" || currentLex.value == "<=" || currentLex.value == ">" || currentLex.value == ">=" || currentLex.value == "=" || currentLex.value == "<>"))
            {
                Lexeme operation = currentLex;
                currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                Node right = ParseSimpleExpression(ref inputBytes);
                left = new Node("Comparison", operation.value, left, right);
                if (right.type == "ERROR")
                {
                    return new Node("ERROR", right.value, null, null);
                }
            }
            else
            {
                return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: don't have operation sign of comparison", null, null);
            }
            return left;
        }
        public static Node ParseСondition(ref byte[] inputBytes)
        {
            currentLex = Lexer.GetFirstLexeme(ref inputBytes);
            Node left = new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: incorrect condition", null, null);
            if (currentLex.type == Lexer.status[13] && currentLex.value == "(")
            {
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                    left = ParseExpression(ref inputBytes);
                }
                else
                {
                    return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: don't have operation sign of condition", null, null);
                }
                if (currentLex.type == Lexer.status[13] && currentLex.value == ")")
                {
                    if (inputBytes.Length > 0)
                    {
                        currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                    }
                }
                else
                {
                    return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: don't have ')'", null, null);
                }
                if (currentLex.type == Lexer.status[10] && (currentLex.value == "or" || currentLex.value == "and"))
                {
                    Lexeme keyWord = currentLex;
                    Node right = ParseСondition(ref inputBytes);
                    left = new Node("BinOperation", keyWord.value, left, right);
                }
            }
            return left;
        }
    }
}
