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
        public List<Node?>? childrens;
        public Node(string type, string value, List<Node?>? childrens)
        {
            this.type = type;
            this.value = value;
            this.childrens = childrens;
        }

    }
    internal class Parser
    {
        static int notClosedBrackets = 0;
        public static Lexeme currentLex;
        public static Node Parse(ref byte[] inputBytes)
        {
            Node res;
            List<Node?> var = new List<Node?>{ };
            List<Node?> body = new List<Node?> { };
            if (inputBytes.Length > 0)
            {
                currentLex = Lexer.GetFirstLexeme(ref inputBytes);
            }
            if (currentLex.type == Lexer.status[10] && currentLex.value == "program")
            {
                body.Add(ParseProgram(ref inputBytes));
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                }
            }
            while (currentLex.type == Lexer.status[10] && (currentLex.value == "var" || currentLex.value == "procedure"))
            {
                switch (currentLex.value)
                {
                    case "var":
                        body.Add(ParseVar(ref inputBytes));
                        break;
                    case "procedure":
                        body.Add(ParseDeclarationProcedure(ref inputBytes));
                        break;
                }
            }
            if (currentLex.type == Lexer.status[10] && currentLex.value == "begin")
            {
                body.Add(ParseMainBlock(ref inputBytes));
                res = new Node("StartProgram", "program", body);
            }
            else
            {
                res = new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected 'begin'", null);
            }
            return res;
        }
        public static Node ParseProgram(ref byte[] inputBytes)
        {
            Node res;
            if (inputBytes.Length > 0)
            {
                currentLex = Lexer.GetFirstLexeme(ref inputBytes);
            }
            if (currentLex.type == Lexer.status[3])
            {
                res = new Node("NameProgram", currentLex.value, null);
            }
            else
            {
                return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected Indifier", null);
            }
            if (inputBytes.Length > 0)
            {
                currentLex = Lexer.GetFirstLexeme(ref inputBytes);
            }
            if (!(currentLex.type == Lexer.status[13] && currentLex.value == ";"))
            {
                return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected ';'", null);
            }
            return res;
        }
        public static Node? ParseVar(ref byte[] inputBytes)
        {
            Node? res;
            List<Node?> children = new List<Node?>();
            if(inputBytes.Length > 0)
            {
                currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                while (currentLex.type == Lexer.status[3])
                {
                    Node child = ParseVariables(ref inputBytes);
                    if (!(currentLex.type == Lexer.status[13] && currentLex.value == ";"))
                    {
                        return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected ';'", null);
                    }
                    children.Add(child);
                    if(child.type == "ERROR")
                    {
                        break;
                    }
                    if (inputBytes.Length > 0)
                    {
                        currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                    }
                }
            }
            if(children.Count > 0)
            {
                res = new Node("Var", "var", children);
            }
            else
            {
                res = null;
            }
            return res;
        }
        public static Node? ParseDeclarationProcedure(ref byte[] inputBytes)
        {
            Node? res = null;
            string name = "";
            List<Node?> body = new List<Node?> { };
            List<Node?> parameters = new List<Node?> { };
            if (inputBytes.Length > 0)
            {
                currentLex = Lexer.GetFirstLexeme(ref inputBytes);
            }
            if(currentLex.type == Lexer.status[3])
            {
                name = currentLex.value;
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                }
                if (currentLex.type == Lexer.status[13] && currentLex.value == "(")
                {
                    do
                    {
                        if (inputBytes.Length > 0)
                        {
                            currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                        }
                        if (currentLex.type == Lexer.status[10] && currentLex.value == "var")
                        {
                            if (inputBytes.Length > 0)
                            {
                                currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                            };
                            parameters.Add(new Node("Ref", "var", new List<Node?> { ParseVariables(ref inputBytes) }));
                        }
                        else
                        {
                            parameters.Add(ParseVariables(ref inputBytes));
                        }
                    } 
                    while (currentLex.type == Lexer.status[13] && currentLex.value == ";");

                    if (currentLex.type == Lexer.status[13] && currentLex.value == ")")
                    {
                        if (inputBytes.Length > 0)
                        {
                            currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                        }
                        body.Add(new Node("ProcedureDeclaration", name, parameters));
                        if (!(currentLex.type == Lexer.status[13] && currentLex.value == ";"))
                        {
                            return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected ';'", null);
                        }
                        else
                        {
                            if (inputBytes.Length > 0)
                            {
                                currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                            }
                        }
                    }
                    else
                    {
                        return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - 1}) ERROR: don't have ')'", null);
                    }
                }
                while(currentLex.type == Lexer.status[10] && currentLex.value == "var")
                {
                    body.Add(ParseVar(ref inputBytes));
                }
                if(currentLex.type == Lexer.status[10] && currentLex.value == "begin")
                {
                    body.Add(ParseBlock(ref inputBytes));
                    res = new Node("Procedure", "procedure", body);
                    if (!(currentLex.type == Lexer.status[13] && currentLex.value == ";"))
                    {
                        return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected ';'", null);
                    }
                    else
                    {
                        if (inputBytes.Length > 0)
                        {
                            currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                        }
                    }
                }
                else
                {
                    return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected 'begin'", null);
                }
            }
            else
            {
                return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected indifier", null);
            }
            return res;
        }
        public static Node ParseVariables(ref byte[] inputBytes)
        {
            Node res;
            List<Node?> names = new List<Node?>() { };
            string type;
            if (currentLex.type == Lexer.status[3])
            {
                names.Add(new Node("NameVar", currentLex.value, null));
            }
            else
            {
                return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected indifier", null);
            }
            if (inputBytes.Length > 0)
            {
                currentLex = Lexer.GetFirstLexeme(ref inputBytes);
            }
            while (currentLex.type == Lexer.status[13] && currentLex.value == ",")
            {
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                }
                if (currentLex.type == Lexer.status[3])
                {
                    names.Add(new Node("NameVar", currentLex.value, null));
                }
                else
                {
                    return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected indifier", null);
                }
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                }
            }
            if (currentLex.type == Lexer.status[12] && currentLex.value == ":")
            {
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                }
                if (currentLex.type == Lexer.status[3] || currentLex.type == Lexer.status[10])
                {
                    type = currentLex.value;
                }
                else
                {
                    return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected type variable", null);
                }
            }
            else
            {
                return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected ':'", null);
            }
            if (inputBytes.Length > 0)
            {
                currentLex = Lexer.GetFirstLexeme(ref inputBytes);
            }
            if (currentLex.type == Lexer.status[12] && currentLex.value == "=")
            {
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                }
                Node value = ParseSimpleExpression(ref inputBytes);
                if (names.Count > 1)
                {
                    return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: Only one variable can be initialized", null);
                }
                else
                {
                    names[0] = new Node("Assignment", "=", new List<Node?> { names[0], value });
                }
            }
            res = new Node("TypeVar", type, names);
            return res;
        }
        public static Node ParseMainBlock(ref byte[] inputBytes)
        {
            List<Node?> children = new List<Node?>();
            if (inputBytes.Length > 0)
            {
                currentLex = Lexer.GetFirstLexeme(ref inputBytes);
            }
            while (currentLex.type != Lexer.status[11])
            {
                children.Add(ParseStatement(ref inputBytes));
                if (currentLex.type == Lexer.status[13] && currentLex.value == ";")
                {
                    if (inputBytes.Length > 0)
                    {
                        currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                    }
                }
                else
                {
                    if(!(currentLex.type == Lexer.status[11]))
                    {
                        return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected ';'", null);
                    }
                }
            }
            if (currentLex.type == Lexer.status[11])
            {
                children.Add(new Node("End", "end.", null));
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                }
            }
            else
            {
                return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected 'end.'", null);
            }
            return new Node("Block", "begin", children);
        }
        public static Node ParseBlock(ref byte[] inputBytes)
        {
            List<Node?> children = new List<Node?>();
            if (inputBytes.Length > 0)
            {
                currentLex = Lexer.GetFirstLexeme(ref inputBytes);
            }
            while (!(currentLex.type == Lexer.status[10] && currentLex.value == "end"))
            {
                children.Add(ParseStatement(ref inputBytes));

                if (currentLex.type == Lexer.status[13] && currentLex.value == ";")
                {
                    if (inputBytes.Length > 0)
                    {
                        currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                    }
                }
                else
                {
                    if (!(currentLex.type == Lexer.status[10]))
                    {
                        return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected ';'", null);
                    }
                }
            }
            if (currentLex.type == Lexer.status[10] && currentLex.value == "end")
            {
                children.Add(new Node("End", "end", null));
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                }
            }
            else
            {
                return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected 'end.'", null);
            }
            return new Node("Block", "begin", children);
        }
        public static Node ParseFor(ref byte[] inputBytes)
        {
            string controllVar = "";
            Node? initialValue = null;
            string toOrDownto = "";
            Node? finalValue = null;
            Node? statement = null;
            if (inputBytes.Length > 0)
            {
                currentLex = Lexer.GetFirstLexeme(ref inputBytes);
            }
            if (currentLex.type == Lexer.status[3])
            {
                controllVar = currentLex.value;
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                }
                if (currentLex.type == Lexer.status[12] && currentLex.value == ":=")
                {
                    if (inputBytes.Length > 0)
                    {
                        currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                        Node exp = ParseSimpleExpression(ref inputBytes);
                        initialValue = new Node("Condition", ":=", new List<Node?>() { new Node("Control Var", controllVar, null), exp });
                    }
                    else
                    {
                        return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected simple expression", null);
                    }
                }
                else
                {
                    return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected ':='", null);
                }
            }
            else
            {
                return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected indifier", null);
            }
            if(currentLex.type == Lexer.status[10] && (currentLex.value == "to" || currentLex.value == "downto"))
            {
                toOrDownto = currentLex.value;
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                    finalValue = ParseSimpleExpression(ref inputBytes);
                    finalValue = new Node("FinalValue", toOrDownto, new List<Node?> { finalValue });
                }
                else
                {
                    return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected simple expression", null);
                }
            }
            else
            {
                return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected 'to' or 'downto'", null);
            }
            if (currentLex.type == Lexer.status[10] && currentLex.value == "do")
            {
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                    statement = ParseStatement(ref inputBytes);
                    statement = new Node("Do", "do", new List<Node?> { statement });
                }
                else
                {
                    return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected statement", null);
                }
            }
            else
            {
                return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected 'do'", null);
            }
            return new Node("For", "for", new List<Node?> { initialValue, finalValue, statement });
        }
        public static Node ParseIf(ref byte[] inputBytes)
        {
            Node condition;
            Node? then = null;
            Node? elseStatement = null;

            if (inputBytes.Length > 0)
            {
                currentLex = Lexer.GetFirstLexeme(ref inputBytes);
            }
            condition = ParseLogicalExpression(ref inputBytes);

            if(currentLex.type == Lexer.status[10] && currentLex.value == "then")
            {
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                }
                Node? statement = ParseStatement(ref inputBytes);
                if(statement.type == "ERROR" && currentLex.type == Lexer.status[10] && currentLex.value == "else")
                {
                    statement = null;
                }
                then = new Node("Then", "then", new List<Node?> { statement });
            }
            else
            {
                return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected 'then'", null);
            }

            if (currentLex.type == Lexer.status[10] && currentLex.value == "else")
            {
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                }
                elseStatement = new Node("Else", "else", new List<Node?> { ParseStatement(ref inputBytes) });
            }
            if(elseStatement == null)
            {
                return new Node("If", "if", new List<Node?> { condition, then });
            }
            else
            {
                return new Node("If", "if", new List<Node?> { condition, then, elseStatement });
            }
        }
        public static Node ParseWhile(ref byte[] inputBytes)
        {
            Node condition;
            Node? statement = null;

            if (inputBytes.Length > 0)
            {
                currentLex = Lexer.GetFirstLexeme(ref inputBytes);
            }
            condition = ParseLogicalExpression(ref inputBytes);

            if (currentLex.type == Lexer.status[10] && currentLex.value == "do")
            {
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                }
                statement = new Node("Do", "do", new List<Node?> { ParseStatement(ref inputBytes) });
            }
            else
            {
                return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected 'do'", null);
            }

            return new Node("While", "while", new List<Node?> { condition, statement });
        }
        public static Node ParseRepeat(ref byte[] inputBytes)
        {
            List<Node?> children = new List<Node?>();

            if (inputBytes.Length > 0)
            {
                currentLex = Lexer.GetFirstLexeme(ref inputBytes);
            }
            if (!(currentLex.type == Lexer.status[10] && currentLex.value == "until"))
            {
                children.Add(ParseStatement(ref inputBytes));
            }

            while (currentLex.type == Lexer.status[13] && currentLex.value == ";")
            {
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                }
                if (currentLex.type == Lexer.status[10] && currentLex.value == "until")
                {
                    break;
                }
                Console.WriteLine(currentLex.value);
                children.Add(ParseStatement(ref inputBytes));
            }

            if (currentLex.type == Lexer.status[10] && currentLex.value == "until")
            {
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                }
                children.Add(new Node("Until", "until", new List<Node?> { ParseLogicalExpression(ref inputBytes) }));
            }
            else
            {
                return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected 'until'", null);
            }

            return new Node("Repeat", "repeat", children);
        }
        public static Node? ParseStatement(ref byte[] inputBytes)
        {
            Node ? res = null;
            if (currentLex.type == Lexer.status[3])
            {
                res = ParseSimpleStatement(ref inputBytes);
            }
            else
            {
                if (currentLex.type == Lexer.status[10] || (currentLex.type == Lexer.status[13] && currentLex.value == ";"))
                {
                    switch (currentLex.value)
                    {
                        case "begin":
                            res = ParseBlock(ref inputBytes);
                            break;
                        case "if":
                            res = ParseIf(ref inputBytes);
                            break;
                        case "for":
                            res = ParseFor(ref inputBytes);
                            break;
                        case "while":
                            res = ParseWhile(ref inputBytes);
                            break;
                        case "repeat":
                            res = ParseRepeat(ref inputBytes);
                            break;
                        case ";":
                            res = null;
                            break;
                        case "exit":
                            res = new Node("Exit", "exit", null);
                            if (inputBytes.Length > 0)
                            {
                                currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                            }
                            break;
                        default:
                            if (inputBytes.Length > 0)
                            {
                                currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                            }
                            return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected statement", null);
                    }
                }
                else
                {
                    return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected statement", null);
                }
            }
            return res;
        }
        public static Node ParseSimpleStatement(ref byte[] inputBytes)
        {
            string operation = "";
            Node? left = null;
            Node? right = null;
            if (currentLex.type == Lexer.status[3])
            {
                left = new Node("Variable", currentLex.value, null);
            }
            else
            {
                return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected indifier", null);
            }
            if (inputBytes.Length > 0)
            {
                currentLex = Lexer.GetFirstLexeme(ref inputBytes);
            }
            if (currentLex.type == Lexer.status[12] && (currentLex.value == ":=" || currentLex.value == "+=" || currentLex.value == "-=" || currentLex.value == "*=" || currentLex.value == "/="))
            {
                operation = currentLex.value;
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                }
                else
                {
                    return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: expected expression", null);
                }
                if(currentLex.type == Lexer.status[2])
                {
                    right = new Node("String", currentLex.value, null);
                    if (inputBytes.Length > 0)
                    {
                        currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                    }
                }
                else
                {
                    right = ParseSimpleExpression(ref inputBytes);
                }
            }
            else
            {
                return ParseProcedure(ref inputBytes, name: left.value);
            }
            return new Node("SimpleStatement", operation, new List<Node?> { left, right });
        }
        public static Node ParseProcedure(ref byte[] inputBytes, string name)
        {
            List<Node?> parameter = new List<Node?>() { };
            if (currentLex.type == Lexer.status[13] && currentLex.value == "(")
            {
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                }
                while (currentLex.type == Lexer.status[2] || currentLex.type == Lexer.status[3] || currentLex.type == Lexer.status[4] || currentLex.type == Lexer.status[8])
                {
                    parameter.Add(new Node("Parameter", currentLex.value, null));
                    if (inputBytes.Length > 0)
                    {
                        currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                    }
                    if (currentLex.type == Lexer.status[13] && currentLex.value == ",")
                    {
                        if (inputBytes.Length > 0)
                        {
                            currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                        }
                    }
                    else
                    {
                        break;
                    }
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
                    return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - 1}) ERROR: don't have ')'", null);
                }
            }

            return new Node("Procedure", name, parameter);
        }
        public static Node ParseLogicalExpression(ref byte[] inputBytes, Node? left = null, bool inComparison = false)
        {
            if(left == null)
            {
                left = ParseLogicalTerm(ref inputBytes, inComparison);
            }
            while (currentLex.type == Lexer.status[10] && currentLex.value == "or")
            {
                Lexeme operation = currentLex;
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                }
                Node right = ParseLogicalTerm(ref inputBytes, inComparison);
                left = new Node("LogicalOperation", operation.value, new List<Node?> { left, right });
                if (right.type == "ERROR")
                {
                    return new Node("ERROR", right.value, null);
                }
            }
            return left;
        }
        public static Node ParseLogicalTerm(ref byte[] inputBytes, bool inComparison = false)
        {
            Node left = ParseLogicalFactor(ref inputBytes, inComparison);
            while (currentLex.type == Lexer.status[10] && currentLex.value == "and")
            {
                Lexeme operation = currentLex;
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                }
                Node right = ParseLogicalFactor(ref inputBytes, inComparison);
                left = new Node("LogicalOperation", operation.value, new List<Node?> { left, right });
                if (right.type == "ERROR")
                {
                    return new Node("ERROR", right.value, null);
                }
            }
            return left;
        }
        public static Node ParseLogicalFactor(ref byte[] inputBytes, bool inComparison = false)
        {
            Node left;
            if (inComparison)
            {
                if (currentLex.type == Lexer.status[10] && currentLex.value == "not")
                {
                    if (inputBytes.Length > 0)
                    {
                        currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                    }
                    left = new Node("Not", "not", new List<Node?> { ParseSimpleExpression(ref inputBytes) });
                }
                else
                {
                    left = ParseSimpleExpression(ref inputBytes);
                }
            }
            else
            {
                if (currentLex.type == Lexer.status[10] && currentLex.value == "not")
                {
                    if (inputBytes.Length > 0)
                    {
                        currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                    }
                    Node not = ParseСomparison(ref inputBytes);
                    left = new Node("Not", "not", new List<Node?> { not });
                    if (notClosedBrackets > 0)
                    {
                        left = new Node("Not", "not", new List<Node?> { ParseLogicalExpression(ref inputBytes, not) });
                    }
                }
                else
                {
                    left = ParseСomparison(ref inputBytes);
                    if (notClosedBrackets > 0)
                    {
                        left = ParseLogicalExpression(ref inputBytes, left);
                    }
                }
            }
            return left;
        }
        public static Node ParseСomparison(ref byte[] inputBytes)
        {
            Node left = ParseLogicalExpression(ref inputBytes, inComparison: true);
            if (currentLex.type == Lexer.status[12] && (currentLex.value == "<" || currentLex.value == "<=" || currentLex.value == ">" || currentLex.value == ">=" || currentLex.value == "=" || currentLex.value == "<>"))
            {
                Lexeme operation = currentLex;
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                }
                Node right = ParseLogicalExpression(ref inputBytes, inComparison: true);
                left = new Node("Comparison", operation.value, new List<Node?> { left, right });
                if (right.type == "ERROR")
                {
                    return new Node("ERROR", right.value, null);
                }
            }
            else
            {
                return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: don't have operation sign of comparison", null);
            }
            while (notClosedBrackets > 0 && currentLex.type == Lexer.status[13] && currentLex.value == ")")
            {
                notClosedBrackets -= 1;
                if (inputBytes.Length > 0)
                {
                    currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                }
            }
            return left;
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
                left = new Node("BinOperation", operation.value, new List<Node?> { left, right });
                if (right.type == "ERROR")
                {
                    return new Node("ERROR", right.value, null);
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
                    Node checkEnd = ParseFactor(ref inputBytes);
                    if (checkEnd.type != "ERROR" || (currentLex.type == Lexer.status[13] && currentLex.value == ")"))
                    {
                        currentLex = Lexer.GetFirstLexeme(ref inputBytes);
                        Node check = ParseFactor(ref inputBytes);
                        if (check.type != "ERROR")
                        {
                            return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - 1}) ERROR: don't have operation sign", null);
                        }
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
                    left = new Node("BinOperation", operation.value, new List<Node?> { left, right });
                    if (right.type == "ERROR")
                    {
                        return new Node("ERROR", right.value, null);
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
                    e = new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - 1}) ERROR: don't have ')'", null);
                }
                if(!(currentLex.type == Lexer.status[13] && currentLex.value == ")"))
                {
                    if (inputBytes.Length != 0)
                    {
                        notClosedBrackets += 1;
                    }
                    else
                    {
                        e = new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - 1}) ERROR: don't have ')'", null);
                    }
                }
                return e;
            }
            if (currentLex.type == Lexer.status[8] || currentLex.type == Lexer.status[4] || currentLex.type == Lexer.status[3])
            {
                Lexeme factor = currentLex;
                return new Node(factor.type, factor.value, null);
            }

            return new Node("ERROR", $"({Lexer.currentLine},{Lexer.currentSymbol - currentLex.lexeme.Length}) ERROR: don't have factor", null);
        }
    }
}
