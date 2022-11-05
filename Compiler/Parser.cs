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
        static Lexeme currentLex;
        static bool addCurrentLex = false;
        public static Node ParseExpression(ref byte[] inputBytes)
        {
            bool firstNode = false;
            if(Lexer.currentLine == 1 && Lexer.currentSymbol == 1)
            {
                currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                addCurrentLex = false;
                firstNode = true;
            }
            Node left = ParseTerm(ref inputBytes);
            if (inputBytes.Length > 0)
            {
                while (currentLex.type == Lexer.status[12] && (currentLex.value == "+" || currentLex.value == "-"))
                {
                    Lexeme operation = currentLex;
                    addCurrentLex = true;
                    if (inputBytes.Length > 0)
                    {
                        currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                        addCurrentLex = false;
                    }
                    Node right = ParseTerm(ref inputBytes);
                    left = new Node("BinOperation", operation.value, left, right);
                    if (right.type == "ERROR")
                    {
                        return new Node("ERROR", right.value, null, null);
                    }
                }
            }
            if (!addCurrentLex && firstNode && currentLex.type != Lexer.status[14])
            {
                return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: don't have operation sign", null, null);
            }
            return left;
        }
        public static Node ParseTerm(ref byte[] inputBytes)
        {
            Node left = ParseFactor(ref inputBytes);
            if (inputBytes.Length > 0)
            {
                while (currentLex.type == Lexer.status[12] && (currentLex.value == "*" || currentLex.value == "/"))
                {
                    Lexeme operation = currentLex;
                    addCurrentLex = true;
                    if (inputBytes.Length > 0)
                    {
                        currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                        addCurrentLex = false;
                    }
                    Node right = ParseFactor(ref inputBytes);
                    left = new Node("BinOperation", operation.value, left, right);
                    if(right.type == "ERROR")
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
                addCurrentLex = true;
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                    addCurrentLex = false;
                }
                else
                {
                    return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - 1}) ERROR: don't have ')'", null, null);
                }
                Node e = ParseExpression(ref inputBytes);
                if(currentLex.type == Lexer.status[13] && currentLex.value == ")")
                {
                    addCurrentLex = true;
                    if (inputBytes.Length > 0)
                    {
                        currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                        addCurrentLex = false;
                    }
                }
                else
                {
                    return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - 1}) ERROR: don't have ')'", null, null);
                }
                return e;
            }
            if (currentLex.type == Lexer.status[8] || currentLex.type == Lexer.status[4] || currentLex.type == Lexer.status[3])
            {
                Lexeme factor = currentLex;
                addCurrentLex = true;
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                    addCurrentLex = false;
                }
                return new Node(factor.type, factor.value, null, null);
            }

            return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: don't have factor", null, null);
        }
    }
}
