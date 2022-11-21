using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public enum TypeNode
    {
        ERROR,
        BinOperation,
        UnaryOperation,
        Call,
        Variable,
        Number,
        String,
    }
    internal class Node
    {
        public TypeNode type;
        public string value;
        public List<Node?>? children;
        public Node(TypeNode type, string value, List<Node?>? childrens = null)
        {
            this.type = type;
            this.value = value;
            this.children = childrens;
        }

    }
    internal class Parser
    {
        static int notClosedBrackets = 0;
        public static Lexeme currentLex;
        static void NextLexeme(ref byte[] inputBytes)
        {
            if (inputBytes.Length > 0)
            {
                currentLex = Lexer.GetFirstLexeme(ref inputBytes);
            }
        }
        public static Node Parse(ref byte[] inputBytes)
        {
            Node res;
            List<Node?> var = new List<Node?>{ };
            List<Node?> body = new List<Node?> { };
            NextLexeme(ref inputBytes);
            if (currentLex.type == TypeLexeme.Key_word && currentLex.value == "program")
            {
                body.Add(ParseProgram(ref inputBytes));
                NextLexeme(ref inputBytes);
            }
            while (currentLex.type == TypeLexeme.Key_word && (currentLex.value == "var" || currentLex.value == "procedure" || currentLex.value == "label" || currentLex.value == "const" || currentLex.value == "type"))
            {
                switch (currentLex.value)
                {
                    case "var":
                        body.Add(ParseVar(ref inputBytes));
                        break;
                    case "const":
                        body.Add(ParseConst(ref inputBytes));
                        break;
                    case "type":
                        body.Add(ParseTypes(ref inputBytes));
                        break;
                    case "procedure":
                        body.Add(ParseDeclarationProcedure(ref inputBytes));
                        break;
                    case "label":
                        body.Add(ParseDeclarationLabel(ref inputBytes));
                        break;
                }
            }
            if (currentLex.type == TypeLexeme.Key_word && currentLex.value == "begin")
            {
                body.Add(ParseMainBlock(ref inputBytes));
                res = new Node(TypeNode.Call, "program", body);
            }
            else
            {
                throw new Exception("ERROR: expected 'begin'");
            }
            return res;
        }
        public static Node ParseProgram(ref byte[] inputBytes)
        {
            Node res;
            NextLexeme(ref inputBytes);
            if (currentLex.type == TypeLexeme.Indifier)
            {
                res = new Node(TypeNode.Variable, currentLex.value, null);
            }
            else
            {
                throw new Exception("ERROR: expected Indifier");
            }
            NextLexeme(ref inputBytes);
            if (!(currentLex.type == TypeLexeme.Separator && currentLex.value == ";"))
            {
                throw new Exception("ERROR: expected ';'");
            }
            return res;
        }
        public static Node? ParseTypes(ref byte[] inputBytes)
        {
            Node? res;
            List<Node?> children = new List<Node?>();
            if (inputBytes.Length > 0)
            {
                NextLexeme(ref inputBytes);
                while (currentLex.type == TypeLexeme.Indifier)
                {
                    Node child = ParseType(ref inputBytes);
                    if (!(currentLex.type == TypeLexeme.Separator && currentLex.value == ";"))
                    {
                        throw new Exception("ERROR: expected ';'");
                    }
                    children.Add(child);
                    if (child.type == TypeNode.ERROR)
                    {
                        break;
                    }
                    NextLexeme(ref inputBytes);
                }
            }
            if (children.Count > 0)
            {
                res = new Node(TypeNode.Call, "type", children);
            }
            else
            {
                res = null;
            }
            return res;
        }
        public static Node ParseType(ref byte[] inputBytes)
        {
            Node nameType;
            Node type;
            if (currentLex.type == TypeLexeme.Indifier)
            {
                nameType = new Node(TypeNode.Variable, currentLex.value, null);
                NextLexeme(ref inputBytes);
            }
            else
            {
                throw new Exception("ERROR: expected indifier");
            }
            if (currentLex.type == TypeLexeme.Operation_sign && currentLex.value == "=")
            {
                NextLexeme(ref inputBytes);
                type = ParseTypeVariable(ref inputBytes);
            }
            else
            {
                throw new Exception("ERROR: expected =");
            }
            return new Node(TypeNode.BinOperation, "=", new List<Node?> { nameType, type });
        }
        public static Node? ParseConst(ref byte[] inputBytes)
        {
            List<Node?> children = new List<Node?>();
            NextLexeme(ref inputBytes);
            while (currentLex.type == TypeLexeme.Indifier)
            {
                Node var = new Node(TypeNode.Variable, currentLex.value, null);
                NextLexeme(ref inputBytes);
                if (currentLex.type == TypeLexeme.Operation_sign && currentLex.value == "=")
                {
                    Node value;
                    NextLexeme(ref inputBytes);
                    if (currentLex.type == TypeLexeme.String)
                    {
                        value = new Node(TypeNode.String, currentLex.value, null);
                        NextLexeme(ref inputBytes);
                    }
                    else
                    {
                        value = ParseSimpleExpression(ref inputBytes);
                    }
                    if (currentLex.type == TypeLexeme.Separator && currentLex.value == ";")
                    {
                        children.Add(new Node(TypeNode.Call, "=", new List<Node?> { var, value }));
                        NextLexeme(ref inputBytes);
                    }
                    else
                    {
                        throw new Exception("ERROR: expected ';'");
                    }
                }
                else
                {
                    throw new Exception("ERROR: expected '='");
                }
            }
            return new Node(TypeNode.Call, "const", children);
        }
        public static Node? ParseVar(ref byte[] inputBytes)
        {
            Node? res;
            List<Node?> children = new List<Node?>();
            if(inputBytes.Length > 0)
            {
                NextLexeme(ref inputBytes);
                while (currentLex.type == TypeLexeme.Indifier)
                {
                    Node child = ParseVariable(ref inputBytes);
                    if (!(currentLex.type == TypeLexeme.Separator && currentLex.value == ";"))
                    {
                        throw new Exception("ERROR: expected ';'");
                    }
                    children.Add(child);
                    if(child.type == TypeNode.ERROR)
                    {
                        break;
                    }
                    NextLexeme(ref inputBytes);
                }
            }
            if(children.Count > 0)
            {
                res = new Node(TypeNode.Call, "var", children);
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
            NextLexeme(ref inputBytes);
            if (currentLex.type == TypeLexeme.Indifier)
            {
                name = currentLex.value;
                NextLexeme(ref inputBytes);
                if (currentLex.type == TypeLexeme.Separator && currentLex.value == "(")
                {
                    do
                    {
                        NextLexeme(ref inputBytes);
                        if (currentLex.type == TypeLexeme.Key_word && currentLex.value == "var")
                        {
                            NextLexeme(ref inputBytes);
                            parameters.Add(new Node(TypeNode.UnaryOperation, "var", new List<Node?> { ParseVariable(ref inputBytes) }));
                        }
                        else
                        {
                            parameters.Add(ParseVariable(ref inputBytes));
                        }
                    } 
                    while (currentLex.type == TypeLexeme.Separator && currentLex.value == ";");

                    if (currentLex.type == TypeLexeme.Separator && currentLex.value == ")")
                    {
                        NextLexeme(ref inputBytes);
                        body.Add(new Node(TypeNode.Call, name, parameters));
                        if (!(currentLex.type == TypeLexeme.Separator && currentLex.value == ";"))
                        {
                            throw new Exception("ERROR: expected ';'");
                        }
                        else
                        {
                            NextLexeme(ref inputBytes);
                        }
                    }
                    else
                    {
                        throw new Exception("expected ')'");
                    }
                }
                while(currentLex.type == TypeLexeme.Key_word && currentLex.value == "var")
                {
                    body.Add(ParseVar(ref inputBytes));
                }
                if(currentLex.type == TypeLexeme.Key_word && currentLex.value == "begin")
                {
                    body.Add(ParseBlock(ref inputBytes));
                    res = new Node(TypeNode.Call, "procedure", body);
                    if (!(currentLex.type == TypeLexeme.Separator && currentLex.value == ";"))
                    {
                        throw new Exception("ERROR: expected ';'");
                    }
                    else
                    {
                        NextLexeme(ref inputBytes);
                    }
                }
                else
                {
                    throw new Exception("ERROR: expected 'begin'");
                }
            }
            else
            {
                throw new Exception("ERROR: expected indifier");
            }
            return res;
        }
        public static Node ParseRecord(ref byte[] inputBytes)
        {
            List<Node?> children = new List<Node?>();
            while (currentLex.type == TypeLexeme.Indifier)
            {
                Node child = ParseVariable(ref inputBytes);
                if (!(currentLex.type == TypeLexeme.Separator && currentLex.value == ";"))
                {
                    throw new Exception("ERROR: expected ';'");
                }
                children.Add(child);
                if (child.type == TypeNode.ERROR)
                {
                    break;
                }
                NextLexeme(ref inputBytes);
            }
            if (currentLex.type == TypeLexeme.Key_word && currentLex.value == "end")
            {
                children.Add(new Node(TypeNode.Call, "end", null));
                NextLexeme(ref inputBytes);
            }
            else
            {
                throw new Exception("ERROR: expected 'end'");
            }
            return new Node(TypeNode.Call, "record", children);
        }
        public static Node ParseVariable(ref byte[] inputBytes)
        {
            Node? ifArray = null;
            List<Node?> names = new List<Node?> { };
            Node type;
            if (currentLex.type == TypeLexeme.Indifier)
            {
                names.Add(new Node(TypeNode.Variable, currentLex.value, null));
            }
            else
            {
                throw new Exception("ERROR: expected indifier");
            }
            NextLexeme(ref inputBytes);
            while (currentLex.type == TypeLexeme.Separator && currentLex.value == ",")
            {
                NextLexeme(ref inputBytes);
                if (currentLex.type == TypeLexeme.Indifier)
                {
                    names.Add(new Node(TypeNode.Variable, currentLex.value, null));
                }
                else
                {
                    throw new Exception("ERROR: expected indifier");
                }
                NextLexeme(ref inputBytes);
            }
            if (currentLex.type == TypeLexeme.Operation_sign && currentLex.value == ":")
            {
                NextLexeme(ref inputBytes);
                if (currentLex.type == TypeLexeme.Indifier || currentLex.type == TypeLexeme.Key_word)
                {
                    type = ParseTypeVariable(ref inputBytes);
                }
                else
                {
                    throw new Exception("ERROR: expected type variable");
                }
            }
            else
            {
                throw new Exception("ERROR: expected ':'");
            }
            if (currentLex.type == TypeLexeme.Operation_sign && currentLex.value == "=")
            {
                NextLexeme(ref inputBytes);
                Node value = ParseSimpleExpression(ref inputBytes);
                if (names.Count > 1)
                {
                    throw new Exception("ERROR: Only one variable can be initialized");
                }
                else
                {
                    names[0] = new Node(TypeNode.BinOperation, "=", new List<Node?> { names[0], value });
                }
            }
            List<Node?> body = new List<Node?> { };
            Node? variable;
            if (names.Count > 1)
            {
                variable = new Node(TypeNode.Variable, ",", names);
            }
            else
            {
                variable = names[0];
            }
            body.Add(variable);
            if(ifArray != null)
            {
                body.Add(ifArray);
            }
            else
            {
                body.Add(type);
            }
            return new Node(TypeNode.Variable, ":", body);
        }
        public static Node ParseArray(ref byte[] inputBytes)
        {
            Node type;
            Node sizes;
            List<Node?> body = new List<Node?> { };
            List<Node?> ordinalType = new List<Node?> { };
            if (currentLex.type == TypeLexeme.Separator && currentLex.value == "[")
            {
                NextLexeme(ref inputBytes);
                ordinalType.Add(ParseArrayOrdinalType(ref inputBytes));
                while (currentLex.type == TypeLexeme.Separator && currentLex.value == ",")
                {
                    NextLexeme(ref inputBytes);
                    ordinalType.Add(ParseArrayOrdinalType(ref inputBytes));
                }
                if (currentLex.type == TypeLexeme.Separator && currentLex.value == "]")
                {
                    NextLexeme(ref inputBytes);
                }
                else
                {
                    throw new Exception("ERROR: expected ']'");
                }
            }
            else
            {
                throw new Exception("ERROR: expected '['");
            }
            sizes = new Node(TypeNode.Call, "[]", ordinalType);
            if (currentLex.type == TypeLexeme.Key_word && currentLex.value == "of")
            {
                NextLexeme(ref inputBytes);
                if (currentLex.type == TypeLexeme.Indifier || currentLex.type == TypeLexeme.Key_word)
                {
                    type = ParseTypeVariable(ref inputBytes);
                }
                else
                {
                    throw new Exception("ERROR: expected type");
                }
            }
            else
            {
                throw new Exception("ERROR: expected 'of'");
            }
            body.Add(sizes);
            body.Add(type);
            return new Node(TypeNode.Call, "array", body);
        }
        public static Node ParseTypeVariable(ref byte[] inputBytes)
        {
            Node ifArrayOrProcedure = new Node(TypeNode.ERROR, "");
            string type;
            if (currentLex.type == TypeLexeme.Indifier || currentLex.type == TypeLexeme.Key_word)
            {
                type = currentLex.value;
                if (type == "array")
                {
                    NextLexeme(ref inputBytes);
                    ifArrayOrProcedure = ParseArray(ref inputBytes);
                }
                if (type == "record")
                {
                    NextLexeme(ref inputBytes);
                    ifArrayOrProcedure = ParseRecord(ref inputBytes);
                }
            }
            else
            {
                throw new Exception("ERROR: expected type variable");
            }
            if (type != "array" && type != "record")
            {
                NextLexeme(ref inputBytes);
            }
            if (ifArrayOrProcedure.value == "")
            {
                return new Node(TypeNode.UnaryOperation, type, null);
            }
            else
            {
                return ifArrayOrProcedure;
            }
        }
        public static Node ParseArrayOrdinalType(ref byte[] inputBytes)
        {
            Node left = ParseSimpleExpression(ref inputBytes);
            if (currentLex.type == TypeLexeme.Separator && currentLex.value == "..")
            {
                NextLexeme(ref inputBytes);
            }
            else
            {
                throw new Exception("ERROR: expected '..'");
            }
            Node right = ParseSimpleExpression(ref inputBytes);
            return new Node(TypeNode.BinOperation, "..", new List<Node?> { left, right });
        }
        public static Node ParseMainBlock(ref byte[] inputBytes)
        {
            List<Node?> children = new List<Node?>();
            NextLexeme(ref inputBytes);
            while (currentLex.type != TypeLexeme.End_file)
            {
                children.Add(ParseStatement(ref inputBytes));
                if (currentLex.type == TypeLexeme.Separator && currentLex.value == ";")
                {
                    NextLexeme(ref inputBytes);
                }
                else
                {
                    if(!(currentLex.type == TypeLexeme.End_file))
                    {
                        throw new Exception("ERROR: expected ';'");
                    }
                }
            }
            if (currentLex.type == TypeLexeme.End_file)
            {
                children.Add(new Node(TypeNode.Call, "end.", null));
                NextLexeme(ref inputBytes);
            }
            else
            {
                throw new Exception("ERROR: expected 'end.'");
            }
            return new Node(TypeNode.Call, "begin", children);
        }
        public static Node ParseBlock(ref byte[] inputBytes)
        {
            List<Node?> children = new List<Node?>();
            NextLexeme(ref inputBytes);
            while (!(currentLex.type == TypeLexeme.Key_word && currentLex.value == "end"))
            {
                children.Add(ParseStatement(ref inputBytes));

                if (currentLex.type == TypeLexeme.Separator && currentLex.value == ";")
                {
                    NextLexeme(ref inputBytes);
                }
                else
                {
                    if (!(currentLex.type == TypeLexeme.Key_word))
                    {
                        throw new Exception("ERROR: expected ';'");
                    }
                }
            }
            if (currentLex.type == TypeLexeme.Key_word && currentLex.value == "end")
            {
                children.Add(new Node(TypeNode.Call, "end", null));
                NextLexeme(ref inputBytes);
            }
            else
            {
                throw new Exception("ERROR: expected 'end.'");
            }
            return new Node(TypeNode.Call, "begin", children);
        }
        public static Node? ParseStatement(ref byte[] inputBytes)
        {
            Node ? res = null;
            if (currentLex.type == TypeLexeme.Indifier)
            {
                res = ParseSimpleStatement(ref inputBytes);
            }
            else
            {
                if (currentLex.type == TypeLexeme.Key_word || (currentLex.type == TypeLexeme.Separator && currentLex.value == ";"))
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
                        case "goto":
                            res = ParseGoto(ref inputBytes);
                            break;
                        case ";":
                            res = null;
                            break;
                        case "exit":
                            res = new Node(TypeNode.Call, "exit");
                            NextLexeme(ref inputBytes);
                            break;
                        default:
                            NextLexeme(ref inputBytes);
                            throw new Exception("ERROR: expected statement");
                    }
                }
                else
                {
                    throw new Exception("ERROR: expected statement");
                }
            }
            return res;
        }
        public static Node ParseSimpleStatement(ref byte[] inputBytes)
        {
            string operation = "";
            Node? left = null;
            Node? right = null;
            if (currentLex.type == TypeLexeme.Indifier)
            {
                left = new Node(TypeNode.Variable, currentLex.value, null);
                NextLexeme(ref inputBytes);
                while (currentLex.type == TypeLexeme.Separator && (currentLex.value == "[" || currentLex.value == "."))
                {
                    string separator = currentLex.value;
                    NextLexeme(ref inputBytes);
                    if (left.children == null)
                    {
                        switch (separator)
                        {
                            case "[":
                                left = new Node(left.type, left.value, new List<Node?> { ParsePositionArray(ref inputBytes) });
                                break;
                            case ".":
                                left = new Node(left.type, left.value, new List<Node?> { ParseRecordField(ref inputBytes) });
                                break;
                        }
                    }
                    else
                    {
                        switch (separator)
                        {
                            case "[":
                                left.children.Add(ParsePositionArray(ref inputBytes));
                                break;
                            case ".":
                                left.children.Add(ParseRecordField(ref inputBytes));
                                break;
                        }
                    }
                }
            }
            else
            {
                throw new Exception("ERROR: expected indifier");
            }
            if (currentLex.type == TypeLexeme.Operation_sign && (currentLex.value == ":=" || currentLex.value == "+=" || currentLex.value == "-=" || currentLex.value == "*=" || currentLex.value == "/="))
            {
                operation = currentLex.value;
                if (inputBytes.Length > 0)
                {
                    NextLexeme(ref inputBytes);
                }
                else
                {
                    throw new Exception("ERROR: expected expression");
                }
                if(currentLex.type == TypeLexeme.String)
                {
                    right = new Node(TypeNode.String, currentLex.value, null);
                    NextLexeme(ref inputBytes);
                }
                else
                {
                    right = ParseSimpleExpression(ref inputBytes);
                }
            }
            else
            {
                if(currentLex.type == TypeLexeme.Operation_sign && currentLex.value == ":")
                {
                    return ParseGotoLabel(ref inputBytes, label: left.value);
                }
                else
                {
                    return ParseProcedure(ref inputBytes, name: left.value);
                }
            }
            return new Node(TypeNode.BinOperation, operation, new List<Node?> { left, right });
        }
        public static Node ParseFor(ref byte[] inputBytes)
        {
            string controllVar = "";
            Node? initialValue = null;
            string toOrDownto = "";
            Node? finalValue = null;
            Node? statement = null;
            NextLexeme(ref inputBytes);
            if (currentLex.type == TypeLexeme.Indifier)
            {
                controllVar = currentLex.value;
                NextLexeme(ref inputBytes);
                if (currentLex.type == TypeLexeme.Operation_sign && currentLex.value == ":=")
                {
                    if (inputBytes.Length > 0)
                    {
                        NextLexeme(ref inputBytes);
                        Node exp = ParseSimpleExpression(ref inputBytes);
                        initialValue = new Node(TypeNode.BinOperation, ":=", new List<Node?>() { new Node(TypeNode.Call, controllVar, null), exp });
                    }
                    else
                    {
                        throw new Exception("ERROR: expected simple expression");
                    }
                }
                else
                {
                    throw new Exception("ERROR: expected ':='");
                }
            }
            else
            {
                throw new Exception("ERROR: expected indifier");
            }
            if(currentLex.type == TypeLexeme.Key_word && (currentLex.value == "to" || currentLex.value == "downto"))
            {
                toOrDownto = currentLex.value;
                if (inputBytes.Length > 0)
                {
                    NextLexeme(ref inputBytes);
                    finalValue = ParseSimpleExpression(ref inputBytes);
                    finalValue = new Node(TypeNode.UnaryOperation, toOrDownto, new List<Node?> { finalValue });
                }
                else
                {
                    throw new Exception("ERROR: expected simple expression");
                }
            }
            else
            {
                throw new Exception("ERROR: expected 'to' or 'downto'");
            }
            if (currentLex.type == TypeLexeme.Key_word && currentLex.value == "do")
            {
                if (inputBytes.Length > 0)
                {
                    NextLexeme(ref inputBytes);
                    statement = ParseStatement(ref inputBytes);
                    statement = new Node(TypeNode.Call, "do", new List<Node?> { statement });
                }
                else
                {
                    throw new Exception("ERROR: expected statement");
                }
            }
            else
            {
                throw new Exception("ERROR: expected 'do'");
            }
            return new Node(TypeNode.Call, "for", new List<Node?> { initialValue, finalValue, statement });
        }
        public static Node ParseIf(ref byte[] inputBytes)
        {
            Node condition;
            Node? then = null;
            Node? elseStatement = null;

            NextLexeme(ref inputBytes);
            condition = ParseLogicalExpression(ref inputBytes);

            if(currentLex.type == TypeLexeme.Key_word && currentLex.value == "then")
            {
                NextLexeme(ref inputBytes);
                Node? statement = ParseStatement(ref inputBytes);
                if(statement.type == TypeNode.ERROR && currentLex.type == TypeLexeme.Key_word && currentLex.value == "else")
                {
                    statement = null;
                }
                then = new Node(TypeNode.Call, "then", new List<Node?> { statement });
            }
            else
            {
                throw new Exception("ERROR: expected 'then'");
            }

            if (currentLex.type == TypeLexeme.Key_word && currentLex.value == "else")
            {
                NextLexeme(ref inputBytes);
                elseStatement = new Node(TypeNode.Call, "else", new List<Node?> { ParseStatement(ref inputBytes) });
            }
            if(elseStatement == null)
            {
                return new Node(TypeNode.Call, "if", new List<Node?> { condition, then });
            }
            else
            {
                return new Node(TypeNode.Call, "if", new List<Node?> { condition, then, elseStatement });
            }
        }
        public static Node ParseWhile(ref byte[] inputBytes)
        {
            Node condition;
            Node? statement = null;

            NextLexeme(ref inputBytes);
            condition = ParseLogicalExpression(ref inputBytes);

            if (currentLex.type == TypeLexeme.Key_word && currentLex.value == "do")
            {
                NextLexeme(ref inputBytes);
                statement = new Node(TypeNode.Call, "do", new List<Node?> { ParseStatement(ref inputBytes) });
            }
            else
            {
                throw new Exception("ERROR: expected 'do'");
            }

            return new Node(TypeNode.Call, "while", new List<Node?> { condition, statement });
        }
        public static Node ParseDeclarationLabel(ref byte[] inputBytes)
        {
            List<Node?> children = new List<Node?> { };
            NextLexeme(ref inputBytes);
            if (currentLex.type == TypeLexeme.Indifier)
            {
                children.Add(new Node(TypeNode.Call, currentLex.value, null));
            }
            else
            {
                throw new Exception("ERROR: expected indifier");
            }
            NextLexeme(ref inputBytes);
            while (currentLex.type == TypeLexeme.Separator && currentLex.value == ",")
            {
                children.Add(new Node(TypeNode.Call, currentLex.value, null));
                NextLexeme(ref inputBytes);
            }
            if (currentLex.type == TypeLexeme.Separator && currentLex.value == ";")
            {
                NextLexeme(ref inputBytes);
            }
            else
            {
                throw new Exception("ERROR: expected ';'");
            }
            return new Node(TypeNode.Call, "label", children);
        }
        public static Node ParseGotoLabel(ref byte[] inputBytes, string label)
        {
            Node lab;
            lab = new Node(TypeNode.Call, label, null);
            currentLex.value = ";";
            currentLex.type = TypeLexeme.Separator;
            return new Node(TypeNode.Call, ":", new List<Node?> { lab });
        }
        public static Node ParseGoto(ref byte[] inputBytes)
        {
            Node label;
            NextLexeme(ref inputBytes);
            if (currentLex.type == TypeLexeme.Indifier)
            {
                label = new Node(TypeNode.Call, currentLex.value, null);
                NextLexeme(ref inputBytes);
            }
            else
            {
                throw new Exception("ERROR: expected indifier");
            }
            return new Node(TypeNode.Call,"goto", new List<Node?> { label });
        }
        public static Node ParseRepeat(ref byte[] inputBytes)
        {
            List<Node?> children = new List<Node?>();

            NextLexeme(ref inputBytes);
            if (!(currentLex.type == TypeLexeme.Key_word && currentLex.value == "until"))
            {
                children.Add(ParseStatement(ref inputBytes));
            }

            while (currentLex.type == TypeLexeme.Separator && currentLex.value == ";")
            {
                NextLexeme(ref inputBytes);
                if (currentLex.type == TypeLexeme.Key_word && currentLex.value == "until")
                {
                    break;
                }
                Console.WriteLine(currentLex.value);
                children.Add(ParseStatement(ref inputBytes));
            }

            if (currentLex.type == TypeLexeme.Key_word && currentLex.value == "until")
            {
                NextLexeme(ref inputBytes);
                children.Add(new Node(TypeNode.Call, "until", new List<Node?> { ParseLogicalExpression(ref inputBytes) }));
            }
            else
            {
                throw new Exception("ERROR: expected 'until'");
            }

            return new Node(TypeNode.Call, "repeat", children);
        }
        public static Node ParseProcedure(ref byte[] inputBytes, string name)
        {
            List<Node?> parameter = new List<Node?>() { };
            if (currentLex.type == TypeLexeme.Separator && currentLex.value == "(")
            {
                NextLexeme(ref inputBytes);
                while (currentLex.type == TypeLexeme.String || currentLex.type == TypeLexeme.Indifier || currentLex.type == TypeLexeme.Integer || currentLex.type == TypeLexeme.Real)
                {
                    parameter.Add(new Node(TypeNode.Call, currentLex.value, null));
                    NextLexeme(ref inputBytes);
                    if (currentLex.type == TypeLexeme.Separator && currentLex.value == ",")
                    {
                        NextLexeme(ref inputBytes);
                    }
                    else
                    {
                        break;
                    }
                }
                if (currentLex.type == TypeLexeme.Separator && currentLex.value == ")")
                {
                    NextLexeme(ref inputBytes);
                }
                else
                {
                    throw new Exception("expected ')'");
                }
            }

            return new Node(TypeNode.Call, name, parameter);
        }
        public static Node ParseLogicalExpression(ref byte[] inputBytes, Node? left = null, bool inComparison = false)
        {
            if(left == null)
            {
                left = ParseLogicalTerm(ref inputBytes, inComparison);
            }
            while (currentLex.type == TypeLexeme.Key_word && currentLex.value == "or")
            {
                Lexeme operation = currentLex;
                NextLexeme(ref inputBytes);
                Node right = ParseLogicalTerm(ref inputBytes, inComparison);
                left = new Node(TypeNode.BinOperation, operation.value, new List<Node?> { left, right });
                if (right.type == TypeNode.ERROR)
                {
                    return new Node(TypeNode.ERROR, right.value, null);
                }
            }
            return left;
        }
        public static Node ParseLogicalTerm(ref byte[] inputBytes, bool inComparison = false)
        {
            Node left = ParseLogicalFactor(ref inputBytes, inComparison);
            while (currentLex.type == TypeLexeme.Key_word && currentLex.value == "and")
            {
                Lexeme operation = currentLex;
                NextLexeme(ref inputBytes);
                Node right = ParseLogicalFactor(ref inputBytes, inComparison);
                left = new Node(TypeNode.BinOperation, operation.value, new List<Node?> { left, right });
                if (right.type == TypeNode.ERROR)
                {
                    return new Node(TypeNode.ERROR, right.value, null);
                }
            }
            return left;
        }
        public static Node ParseLogicalFactor(ref byte[] inputBytes, bool inComparison = false)
        {
            Node left;
            if (inComparison)
            {
                if (currentLex.type == TypeLexeme.Key_word && currentLex.value == "not")
                {
                    NextLexeme(ref inputBytes);
                    left = new Node(TypeNode.UnaryOperation, "not", new List<Node?> { ParseSimpleExpression(ref inputBytes) });
                }
                else
                {
                    left = ParseSimpleExpression(ref inputBytes);
                }
            }
            else
            {
                if (currentLex.type == TypeLexeme.Key_word && currentLex.value == "not")
                {
                    NextLexeme(ref inputBytes);
                    Node not = ParseСomparison(ref inputBytes);
                    left = new Node(TypeNode.Call, "not", new List<Node?> { not });
                    if (notClosedBrackets > 0)
                    {
                        left = new Node(TypeNode.Call, "not", new List<Node?> { ParseLogicalExpression(ref inputBytes, not) });
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
            Node left;
            if (currentLex.type == TypeLexeme.String)
            {
                left = new Node(TypeNode.String, currentLex.value, null);
                NextLexeme(ref inputBytes);
            }
            else
            {
                left = ParseLogicalExpression(ref inputBytes, inComparison: true);
            }
            if (currentLex.type == TypeLexeme.Operation_sign && (currentLex.value == "<" || currentLex.value == "<=" || currentLex.value == ">" || currentLex.value == ">=" || currentLex.value == "=" || currentLex.value == "<>"))
            {
                Lexeme operation = currentLex;
                NextLexeme(ref inputBytes);
                Node right;
                if (operation.type == TypeLexeme.Operation_sign && operation.value == "=" && currentLex.type == TypeLexeme.String)
                {
                    right = new Node(TypeNode.String, currentLex.value, null);
                    NextLexeme(ref inputBytes);
                }
                else
                {
                    right = ParseLogicalExpression(ref inputBytes, inComparison: true);
                }
                left = new Node(TypeNode.BinOperation, operation.value, new List<Node?> { left, right });
                if (right.type == TypeNode.ERROR)
                {
                    return new Node(TypeNode.ERROR, right.value, null);
                }
            }
            else
            {
                throw new Exception("ERROR: don't have operation sign of comparison");
            }
            while (notClosedBrackets > 0 && currentLex.type == TypeLexeme.Separator && currentLex.value == ")")
            {
                notClosedBrackets -= 1;
                NextLexeme(ref inputBytes);
            }
            return left;
        }
        public static Node ParseSimpleExpression(ref byte[] inputBytes)
        {
            Node left = ParseTerm(ref inputBytes);
            while (currentLex.type == TypeLexeme.Operation_sign && (currentLex.value == "+" || currentLex.value == "-"))
            {
                Lexeme operation = currentLex;
                NextLexeme(ref inputBytes);
                Node right = ParseTerm(ref inputBytes);
                left = new Node(TypeNode.BinOperation, operation.value, new List<Node?> { left, right });
                if (right.type == TypeNode.ERROR)
                {
                    return new Node(TypeNode.ERROR, right.value, null);
                }
            }
            return left;
        }
        public static Node ParseTerm(ref byte[] inputBytes)
        {
            Node left = ParseFactor(ref inputBytes, withUnOp: true);
            while (left.type == TypeNode.Variable && currentLex.type == TypeLexeme.Separator && (currentLex.value == "[" || currentLex.value == "."))
                        {
                            string separator = currentLex.value;
                            NextLexeme(ref inputBytes);
                            if (left.children == null)
                            {
                                switch (separator)
                                {
                                    case "[":
                                        left = new Node(left.type, left.value, new List<Node?> { ParsePositionArray(ref inputBytes) });
                                        break;
                                    case ".":
                                        left = new Node(left.type, left.value, new List<Node?> { ParseRecordField(ref inputBytes) });
                                        break;
                                }
                            }
                            else
                            {
                                switch (separator)
                                {
                                    case "[":
                                        left.children.Add(ParsePositionArray(ref inputBytes));
                                        break;
                                    case ".":
                                        left.children.Add(ParseRecordField(ref inputBytes));
                                        break;
                                }
                            }
                        }
            if(left.type != TypeNode.ERROR)
            {
                if (inputBytes.Length > 0)
                {
                    Node checkEnd = ParseFactor(ref inputBytes, withUnOp: true);
                    if (checkEnd.type != TypeNode.ERROR || (currentLex.type == TypeLexeme.Separator && currentLex.value == ")"))
                    {
                        NextLexeme(ref inputBytes);
                        Node check = ParseFactor(ref inputBytes);
                        if (check.type != TypeNode.ERROR)
                        {
                            throw new Exception("expected operation sign");
                        }
                    }
                }
                while (currentLex.type == TypeLexeme.Operation_sign && (currentLex.value == "*" || currentLex.value == "/"))
                {
                    Lexeme operation = currentLex;
                    NextLexeme(ref inputBytes);
                    Node right = ParseFactor(ref inputBytes, withUnOp: true);
                    if (right.type == TypeNode.ERROR)
                    {
                        throw new Exception("expected factor");
                    }
                    NextLexeme(ref inputBytes);
                    while (right.type == TypeNode.Variable && currentLex.type == TypeLexeme.Separator && (currentLex.value == "[" || currentLex.value == "."))
                    {
                        string separator = currentLex.value;
                        NextLexeme(ref inputBytes);
                        if (right.children == null)
                        {
                            switch (separator)
                            {
                                case "[":
                                    right = new Node(right.type, right.value, new List<Node?> { ParsePositionArray(ref inputBytes) });
                                    break;
                                case ".":
                                    right = new Node(right.type, right.value, new List<Node?> { ParseRecordField(ref inputBytes) });
                                    break;
                            }
                        }
                        else
                        {
                            switch (separator)
                            {
                                case "[":
                                    right.children.Add(ParsePositionArray(ref inputBytes));
                                    break;
                                case ".":
                                    right.children.Add(ParseRecordField(ref inputBytes));
                                    break;
                            }
                        }
                    }
                    left = new Node(TypeNode.BinOperation, operation.value, new List<Node?> { left, right });
                    if (right.type == TypeNode.ERROR)
                    {
                        return new Node(TypeNode.ERROR, right.value, null);
                    }
                }
            }
            else
            {
                throw new Exception("expected factor");
            }
            return left;
        }
        public static Node ParseFactor(ref byte[] inputBytes, bool withUnOp = false)
        {
            if (currentLex.type == TypeLexeme.Separator && currentLex.value == "(")
            {
                Node e;
                if (inputBytes.Length > 0)
                {
                    NextLexeme(ref inputBytes);
                    e = ParseSimpleExpression(ref inputBytes);
                }
                else
                {
                    throw new Exception("expected ')'");
                }
                if(!(currentLex.type == TypeLexeme.Separator && currentLex.value == ")"))
                {
                    if (inputBytes.Length == 0)
                    {
                        throw new Exception("expected ')'");
                    }
                    else
                    {
                        notClosedBrackets += 1;
                    }
                }
                return e;
            }
            if (currentLex.type == TypeLexeme.Real || currentLex.type == TypeLexeme.Integer)
            {
                Lexeme factor = currentLex;
                return new Node(TypeNode.Number, factor.value, null);
            }
            if (currentLex.type == TypeLexeme.Indifier)
            {
                Lexeme factor = currentLex;
                return new Node(TypeNode.Variable, factor.value, null);
            }

            if (withUnOp && (currentLex.type == TypeLexeme.Operation_sign && (currentLex.value == "+" || currentLex.value == "-")))
            {
                string unOp = currentLex.value;
                NextLexeme(ref inputBytes);
                Node factor = ParseFactor(ref inputBytes);
                if (factor.type == TypeNode.ERROR)
                {
                    throw new Exception("expected factor");
                }
                return new Node(TypeNode.UnaryOperation, unOp, new List<Node?> { factor });
            }

            return new Node(TypeNode.ERROR, $"error");
        }
        public static Node ParsePositionArray(ref byte[] inputBytes)
        {
            List<Node?> body = new List<Node?> { };
            body.Add(ParseSimpleExpression(ref inputBytes));
            bool bracketClose = false;
            while (currentLex.type == TypeLexeme.Separator && (currentLex.value == "," || currentLex.value == "]"))
            {
                switch (currentLex.value)
                {
                    case "]":
                        bracketClose = true;
                        NextLexeme(ref inputBytes);
                        if (currentLex.value == "[")
                        {
                            bracketClose = false;
                            NextLexeme(ref inputBytes);
                            body.Add(ParseSimpleExpression(ref inputBytes));
                        }
                        break;
                    case ",":
                        NextLexeme(ref inputBytes);
                        body.Add(ParseSimpleExpression(ref inputBytes));
                        break;
                }
            }
            if (!bracketClose)
            {
                throw new Exception("ERROR: expected ']'");
            }
            return new Node(TypeNode.Call, "[]", body);
        }
        public static Node ParseRecordField(ref byte[] inputBytes)
        {
            if(currentLex.type == TypeLexeme.Indifier)
            {
                Node field = new Node(TypeNode.Variable, currentLex.value);
                NextLexeme(ref inputBytes);
                return new Node(TypeNode.UnaryOperation, ".", new List<Node?> { field });
            }
            else
            {
                throw new Exception("ERROR: expected indifier");
            }
        }
    }
}
