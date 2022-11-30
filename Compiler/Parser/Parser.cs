namespace Compiler
{
    public class Parser
    {
        Lexer lexer;
        Token currentLex;
        SymTableStack symTableStack;
        public string PrintSymTable()
        {
            string ans = "";

            return ans;
        }
        void NextToken()
        {
            currentLex = lexer.GetNextToken();
        }
        public Parser(Lexer lexer)
        {
            this.lexer = lexer;
            symTableStack = new SymTableStack();
            Dictionary<string, Symbol> builtins = new Dictionary<string, Symbol>();
            builtins.Add("integer", new SymInteger("integer"));
            builtins.Add("real", new SymReal("real"));
            builtins.Add("string", new SymString("string"));
            builtins.Add("write", new SymProc("write"));
            builtins.Add("read", new SymProc("read"));
            symTableStack.AddTable(new SymTable(builtins));
            symTableStack.AddTable(new SymTable(new Dictionary<string, Symbol>()));
            NextToken();
        }
        public Node ParseProgram()
        {
            string? name = null;
            List<NodeDefs> types = new List<NodeDefs>();
            BlockStmt body;
            if (currentLex.Type == TokenType.Key_word && currentLex.Value == "program")
            {
                name = ParseNameProgram();
            }
            types = ParseDefs();
            if (currentLex.Type != TokenType.Key_word || currentLex.Value != "begin")
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected 'begin'");
            }
            body = ParseBlock();
            if (currentLex.Type != TokenType.Separator || currentLex.Value != ".")
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected '.'");
            }
            return new NodeMainProgram(name, types, body);
        }
        public string ParseNameProgram()
        {
            string res;
            NextToken();
            if (currentLex.Type != TokenType.Indifier)
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected Indifier");
            }
            res = currentLex.Value;
            NextToken();
            if (!(currentLex.Type == TokenType.Separator && currentLex.Value == ";"))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected ';'");
            }
            NextToken();
            return res;
        }
        public List<NodeDefs> ParseDefs()
        {
            List<NodeDefs> types = new List<NodeDefs>();
            while (currentLex.Type == TokenType.Key_word && (currentLex.Value == "var" || currentLex.Value == "procedure" || currentLex.Value == "label" || currentLex.Value == "const" || currentLex.Value == "type"))
            {
                switch (currentLex.Value)
                {
                    case "var":
                        types.Add(ParseVarDefs());
                        break;
                    case "const":
                        types.Add(ParseConstDefs());
                        break;
                    case "type":
                        types.Add(ParseTypeDefs());
                        break;
                    case "procedure":
                        types.Add(ParseProcedureDefs());
                        break;
                }
            }
            return types;
        }
        public NodeDefs ParseConstDefs()
        {
            List<ConstDeclarationNode> body = new List<ConstDeclarationNode>();
            NextToken();
            if (currentLex.Type != TokenType.Indifier)
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected indifier");
            }
            while (currentLex.Type == TokenType.Indifier)
            {
                string name = currentLex.Value;
                NextToken();
                if (!(currentLex.Type == TokenType.Operation_sign && currentLex.Value == "="))
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected '='");
                }
                NodeExpression value;
                NextToken();
                value = ParseExpression();
                if (!(currentLex.Type == TokenType.Separator && currentLex.Value == ";"))
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected ';'");
                }
                SymVarConst varConst = new SymVarConst(name, new SymType("const"));
                symTableStack.Add(name, varConst);
                body.Add(new ConstDeclarationNode(name, value));
                NextToken();
            }
            return new ConstTypesNode(body);
        }
        public NodeDefs ParseVarDefs()
        {
            List<VarDeclarationNode> body = new List<VarDeclarationNode>();
            NextToken();
            if (currentLex.Type != TokenType.Indifier)
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected indifier");
            }
            while (currentLex.Type == TokenType.Indifier)
            {
                VarDeclarationNode var = ParseVarDef();
                body.Add(var);
                if (!(currentLex.Type == TokenType.Separator && currentLex.Value == ";"))
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected ';'");
                }
                NextToken();
            }
            return new VarTypesNode(body);
        }
        public NodeDefs ParseTypeDefs()
        {
            List<DeclarationNode> body = new List<DeclarationNode>();
            NextToken();
            while (currentLex.Type == TokenType.Indifier)
            {
                body.Add(ParseTypeDef());
                if (!(currentLex.Type == TokenType.Separator && currentLex.Value == ";"))
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected ';'");
                }
                NextToken();
            }
            return new TypeTypesNode(body);
        }
        public NodeDefs ParseProcedureDefs()
        {
            string name;
            List<VarDeclarationNode> paramsNode = new List<VarDeclarationNode>();
            SymTable locals = new SymTable(new Dictionary<string, Symbol>());
            NextToken();
            if (currentLex.Type != TokenType.Indifier)
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected indifier");
            }
            name = currentLex.Value;
            NextToken();
            symTableStack.AddTable(locals);
            if (currentLex.Type == TokenType.Separator && currentLex.Value == "(")
            {
                do
                {
                    NextToken();
                    VarDeclarationNode varDef;
                    if (currentLex.Type == TokenType.Key_word)
                    {
                        if (currentLex.Value == "var" || currentLex.Value == "out")
                        {
                            string param = currentLex.Value;
                            NextToken();
                            varDef = ParseVarDef(param);
                            paramsNode.Add(varDef);
                        }
                        else
                        {
                            throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected indifier");
                        }
                    }
                    else
                    {
                        varDef = ParseVarDef();
                        paramsNode.Add(varDef);
                    }
                } 
                while (currentLex.Type == TokenType.Separator && currentLex.Value == ";");

                if (!(currentLex.Type == TokenType.Separator && currentLex.Value == ")"))
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected ')'");
                }
                NextToken();
            }
            if (!(currentLex.Type == TokenType.Separator && currentLex.Value == ";"))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected ';'");
            }
            NextToken();

            SymTable params_ = new SymTable(symTableStack.GetBackTable());
            List<NodeDefs> localsTypes = ParseDefs();
            locals = symTableStack.GetBackTable();
            if (!(currentLex.Type == TokenType.Key_word && currentLex.Value == "begin"))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected 'begin'");
            }
            BlockStmt body = ParseBlock();
            if (!(currentLex.Type == TokenType.Separator && currentLex.Value == ";"))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected ';'");
            }
            NextToken();
            symTableStack.PopBack();
            SymProc symProc = new SymProc(name, params_, locals, body);
            symTableStack.Add(name, symProc);
            return new ProcedureTypesNode(paramsNode, localsTypes, symProc);
        }
        public VarDeclarationNode ParseVarDef(string param = "")
        {
            List<string> names = new List<string>();
            List<SymVar> vars = new List<SymVar> ();
            SymType type;
            NodeExpression? value = null;
            if (!(currentLex.Type == TokenType.Indifier))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected indifier");
            }
            names.Add(currentLex.Value);
            NextToken();
            while (currentLex.Type == TokenType.Separator && currentLex.Value == ",")
            {
                NextToken();
                if (currentLex.Type != TokenType.Indifier)
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected indifier");
                }
                names.Add(currentLex.Value);
                NextToken();
            }
            if (!(currentLex.Type == TokenType.Operation_sign && currentLex.Value == ":"))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected ':'");
            }
            NextToken();
            if (!(currentLex.Type == TokenType.Indifier || currentLex.Type == TokenType.Key_word))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected type variable");
            }
            type = ParseType();
            if (currentLex.Type == TokenType.Operation_sign && currentLex.Value == "=")
            {
                if (names.Count > 1)
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"Only one variable can be initialized");
                }
                NextToken();
                value = ParseExpression();
            }
            foreach(string name in names)
            {
                SymVar var = new SymVar(name, type);
                switch (param)
                {
                    case "var":
                        symTableStack.Add(name, new SymParamVar(var));
                        break;
                    case "out":
                        symTableStack.Add(name, new SymParamOut(var));
                        break;
                    default:
                        symTableStack.Add(name, var);
                        break;
                }
                vars.Add(var);
            }
            return new VarDeclarationNode(vars, type, value);
        }
        public TypeDeclarationNode ParseTypeDef()
        {
            string nameType;
            SymType type;
            if (currentLex.Type != TokenType.Indifier)
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected indifier");
            }
            nameType = currentLex.Value;
            NextToken();
            if (!(currentLex.Type == TokenType.Operation_sign && currentLex.Value == "="))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected =");
            }
            NextToken();
            type = ParseType();
            SymTypeAlias typeAlias = new SymTypeAlias(type.GetName(), type);
            symTableStack.Add(nameType, type);
            return new TypeDeclarationNode( nameType, typeAlias );
        }
        public SymType ParseType()
        {
            if (!(currentLex.Type == TokenType.Indifier || currentLex.Type == TokenType.Key_word))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected type variable");
            }
            Token type = currentLex;
            NextToken();
            switch (type.Value)
            {
                case "array":
                    return ParseArrayType();
                case "record":
                    return ParseRecordType();
                case "string":
                    return (SymType)symTableStack.Get(type.Value);
                case "integer":
                    return (SymType)symTableStack.Get(type.Value);
                case "real":
                    return (SymType)symTableStack.Get(type.Value);
                default:
                    SymType original;
                    Symbol sym = symTableStack.Get(type.Value);
                    try
                    {
                        original = (SymType) sym;
                    }
                    catch
                    {
                        throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, $"Identifier not found \"{type.Source}\"");
                    }
                    return new SymTypeAlias(type.Value, original);
            }
        }
        public SymType ParseArrayType()
        {
            SymType type;
            List<OrdinalTypeNode> ordinalTypes = new List<OrdinalTypeNode> ();

            if (!(currentLex.Type == TokenType.Separator && currentLex.Value == "["))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected '['");
            }
            do
            {
                NextToken();
                ordinalTypes.Add(ParseArrayOrdinalType());
            }
            while (currentLex.Type == TokenType.Separator && currentLex.Value == ",");
            if (!(currentLex.Type == TokenType.Separator && currentLex.Value == "]"))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected ']'");
            }
            NextToken();
            if (!(currentLex.Type == TokenType.Key_word && currentLex.Value == "of"))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected 'of'");
            }
            NextToken();
            if (currentLex.Type == TokenType.Indifier || currentLex.Type == TokenType.Key_word)
            {
                type = ParseType();
            }
            else
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected type");
            }
            return new SymArray("array", ordinalTypes, type);
        }
        public OrdinalTypeNode ParseArrayOrdinalType()
        {
            NodeExpression from = ParseSimpleExpression();
            if (!(currentLex.Type == TokenType.Separator && currentLex.Value == ".."))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected '..'");
            }
            NextToken();
            NodeExpression to = ParseSimpleExpression();
            return new OrdinalTypeNode(from, to);
        }
        public SymType ParseRecordType()
        {
            SymTable fields = new SymTable(new Dictionary<string, Symbol>());
            symTableStack.AddTable(fields);
            while (currentLex.Type == TokenType.Indifier)
            {
                ParseVarDef();
                if (currentLex.Type == TokenType.Key_word && currentLex.Value == "end")
                {
                    break;
                }
                if (!(currentLex.Type == TokenType.Separator && currentLex.Value == ";"))
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected ';'");
                }
                NextToken();
            }
            if (!(currentLex.Type == TokenType.Key_word && currentLex.Value == "end"))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected 'end'");
            }
            symTableStack.PopBack();
            NextToken();
            return new SymRecord("record", fields);
        }
        public NodeStatement ParseStatement()
        {
            NodeStatement res = new NullStmt();
            if (currentLex.Type == TokenType.Indifier)
            {
                res = ParseSimpleStatement();
            }
            else
            {
                //structStmt
                if (currentLex.Type == TokenType.Key_word || (currentLex.Type == TokenType.Separator && currentLex.Value == ";"))
                {
                    switch (currentLex.Value)
                    {
                        case "begin":
                            res = ParseBlock();
                            break;
                        case "if":
                            res = ParseIf();
                            break;
                        case "for":
                            res = ParseFor();
                            break;
                        case "while":
                            res = ParseWhile();
                            break;
                        case "repeat":
                            res = ParseRepeat();
                            break;
                        case ";":
                            break;
                        case "exit":
                            res = new CallStmt(symTableStack.Get("exit"), null);
                            NextToken();
                            break;
                        default:
                            throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected statement");
                    }
                }
                else
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected statement");
                }
            }
            return res;
        }
        public NodeStatement ParseSimpleStatement()
        {
            string operation;
            string name;
            NodeExpression left;
            NodeExpression right;

            int lineStart = currentLex.NumberLine;
            int symStart = currentLex.NumberSymbol;
            if (currentLex.Type == TokenType.Indifier)
            {
                name = currentLex.Value;
                NextToken();
            }
            else
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected indifier");
            }
            //assigmentStmt
            if (currentLex.Type == TokenType.Operation_sign && (currentLex.Value == ":=" || currentLex.Value == "+=" || currentLex.Value == "-=" || currentLex.Value == "*=" || currentLex.Value == "/="))
            {
                SymVar var_ = (SymVar)symTableStack.Get(name);
                left = new NodeVar(var_);
                operation = currentLex.Value;
                if ((var_.GetTypeVar().GetType().Name == "SymRecord" || var_.GetTypeVar().GetType().Name == "SymRecord" || var_.GetTypeVar().GetType().Name == "SymArray" || var_.GetTypeVar().GetType().Name == "SymArray") &&
                    (operation == "+=" || operation == "-=" || operation == "*=" || operation == "/="))
                {
                    throw new ExceptionWithPosition(lineStart, symStart, $"Operator is not overloaded");
                }
                if (var_.GetTypeVar().GetType().Name == "SymString" && (operation == "*=" || operation == "/=" || operation == "-="))
                {
                    throw new ExceptionWithPosition(lineStart, symStart, $"Operator is not overloaded");
                }
                NextToken();
                int lineStartExp = currentLex.NumberLine;
                int symStartExp = currentLex.NumberSymbol;
                right = ParseExpression();
                if(var_.GetTypeVar().GetType().Name != right.GetCachedType().GetType().Name)
                {
                    throw new ExceptionWithPosition(lineStartExp, symStartExp, $"Incompatible types: got \"{right.GetCachedType().GetName()}\" expected \"{var_.GetTypeVar().GetName()}\"");
                }
                return new AssignmentStmt(operation, left, right);
            }
            else
            {
                return ParseProcedureStmt(name, lineStart, symStart);
            }
        }
        public BlockStmt ParseBlock()
        {
            List<NodeStatement> body = new List<NodeStatement>();
            NextToken();
            while (!(currentLex.Type == TokenType.Key_word && currentLex.Value == "end"))
            {
                body.Add(ParseStatement());

                if (currentLex.Type == TokenType.Separator && currentLex.Value == ";")
                {
                    NextToken();
                }
                else
                {
                    if (!(currentLex.Type == TokenType.Key_word))
                    {
                        throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected ';'");
                    }
                }
            }
            if (currentLex.Type == TokenType.Key_word && currentLex.Value == "end")
            {
                NextToken();
            }
            else
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected 'end'");
            }
            return new BlockStmt(body);
        }
        public NodeStatement ParseFor()
        {
            string toOrDownto = "";
            NodeVar controlVar;
            NodeExpression initialValue;
            NodeExpression finalValue;
            NodeStatement? body;
            NextToken();
            if (currentLex.Type != TokenType.Indifier)
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected indifier");
            }
            controlVar = new NodeVar((SymVar)symTableStack.Get(currentLex.Value));
            NextToken();
            if (currentLex.Type != TokenType.Operation_sign || currentLex.Value != ":=")
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected ':='");
            }
            NextToken();
            initialValue = ParseSimpleExpression();
            if (currentLex.Type == TokenType.Key_word && (currentLex.Value == "to" || currentLex.Value == "downto"))
            {
                toOrDownto = currentLex.Value;
                NextToken();
                finalValue = ParseSimpleExpression();
            }
            else
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected 'to' or 'downto'");
            }
            if (currentLex.Type != TokenType.Key_word || currentLex.Value != "do")
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected 'do'");
            }
            NextToken();
            body = ParseStatement();
            return new ForStmt( controlVar, initialValue, toOrDownto, finalValue, body );
        }
        public NodeStatement ParseIf()
        {
            NodeExpression condition;
            NodeStatement body = new NullStmt();
            NodeStatement elseStatement = new NullStmt();

            NextToken();
            condition = ParseExpression();

            if(currentLex.Type != TokenType.Key_word || currentLex.Value != "then")
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected 'then'");
            }
            NextToken();
            if (!(currentLex.Type == TokenType.Key_word && currentLex.Value == "else"))
            {
                body = ParseStatement();
            }
            if (currentLex.Type == TokenType.Key_word && currentLex.Value == "else")
            {
                NextToken();
                elseStatement = ParseStatement();
            }
            return new IfStmt(condition, body, elseStatement);
        }
        public NodeStatement ParseWhile()
        {
            NodeExpression condition;
            NodeStatement? body = null;

            NextToken();
            condition = ParseExpression();
            if (currentLex.Type != TokenType.Key_word || currentLex.Value != "do")
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected 'do'");
            }
            NextToken();
            body = ParseStatement();

            return new WhileStmt( condition, body );
        }
        public NodeStatement ParseRepeat()
        {
            List<NodeStatement> body = new List<NodeStatement>();
            NodeExpression cond;

            NextToken();
            if (!(currentLex.Type == TokenType.Key_word && currentLex.Value == "until"))
            {
                body.Add(ParseStatement());
            }

            while (currentLex.Type == TokenType.Separator && currentLex.Value == ";")
            {
                NextToken();
                if (currentLex.Type == TokenType.Key_word && currentLex.Value == "until")
                {
                    break;
                }
                body.Add(ParseStatement());
            }

            if (currentLex.Type == TokenType.Key_word && currentLex.Value == "until")
            {
                NextToken();
                cond = ParseExpression();
            }
            else
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected 'until'");
            }

            return new RepeatStmt(body, cond);
        }
        public NodeStatement ParseProcedureStmt(string name, int lineProc, int symProc)
        {
            List<NodeExpression?> parameter = new List<NodeExpression?>();
            SymProc proc;
            try
            {
                proc = (SymProc)symTableStack.Get(name);
            }
            catch
            {
                throw new ExceptionWithPosition(lineProc, symProc, $"Identifier not found \"{name}\"");
            }
            if (currentLex.Type == TokenType.Separator && currentLex.Value == "(")
            {
                NextToken();
                while (currentLex.Type == TokenType.String || currentLex.Type == TokenType.Indifier || currentLex.Type == TokenType.Integer || currentLex.Type == TokenType.Real)
                {
                    parameter.Add(ParseFactor());
                    if (currentLex.Type == TokenType.Separator && currentLex.Value == ",")
                    {
                        NextToken();
                    }
                    else
                    {
                        break;
                    }
                }
                if (currentLex.Type == TokenType.Separator && currentLex.Value != ")")
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected ')'");
                }
                NextToken();
            }
            if (proc.GetCountParams() != -1 && parameter.Count != proc.GetCountParams())
            {
                throw new ExceptionWithPosition(lineProc, symProc, $"Wrong number of parameters specified for call to \"{proc.GetName()}\"");
            }
            return new CallStmt(proc, parameter);
        }
        public NodeExpression ParseExpression()
        {
            NodeExpression left = ParseSimpleExpression();
            while (currentLex.Type == TokenType.Operation_sign &&
                  (currentLex.Value == "<" || currentLex.Value == "<=" || currentLex.Value == ">" || currentLex.Value == "=>" || currentLex.Value == "=" || currentLex.Value == "<>"))
            {
                string operation = currentLex.Value;
                NextToken();
                NodeExpression right = ParseSimpleExpression();
                left = new NodeBinOp(operation, left, right);
            }
            return left;
        }
        public NodeExpression ParseSimpleExpression()
        {
            NodeExpression left = ParseTerm();
            while ((currentLex.Type == TokenType.Operation_sign && (currentLex.Value == "+" || currentLex.Value == "-")) ||
                   (currentLex.Type == TokenType.Key_word && (currentLex.Value == "or" || currentLex.Value == "xor")))
            {
                string operation = currentLex.Value;
                NextToken();
                NodeExpression right = ParseTerm();
                left = new NodeBinOp(operation, left, right );
            }
            return left;
        }
        public NodeExpression ParseTerm()
        {
            NodeExpression left = ParseFactor(withUnOp: true);
            while ((currentLex.Type == TokenType.Operation_sign && (currentLex.Value == "*" || currentLex.Value == "/")) ||
                   (currentLex.Type == TokenType.Key_word && currentLex.Value == "and"))
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
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected ')'");
                }
                if(!(currentLex.Type == TokenType.Separator && currentLex.Value == ")"))
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
                SymVar symVar;
                try
                {
                    symVar = (SymVar)symTableStack.Get(factor.Value);
                }
                catch
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, $"Identifier not found \"{factor.Value}\"");
                }
                ans = new NodeVar(symVar);
                while (currentLex.Type == TokenType.Separator && (currentLex.Value == "[" || currentLex.Value == "."))
                {
                    string separator = currentLex.Value;
                    NextToken();
                    switch (separator)
                    {
                        case "[":
                            ans = ParsePositionArray(ans);
                            break;
                        case ".":
                            ans = ParseRecordField(ans);
                            break;
                    }
                }
                return ans;
            }

            if (withUnOp && ((currentLex.Type == TokenType.Operation_sign && (currentLex.Value == "+" || currentLex.Value == "-")) || (currentLex.Type == TokenType.Key_word && currentLex.Value == "not")))
            {
                string unOp = currentLex.Value;
                NextToken();
                NodeExpression factor = ParseFactor();
                return new NodeUnOp(unOp, factor);
            }

            throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected factor");
        }
        public NodeExpression ParsePositionArray(NodeExpression node)
        {
            List<NodeExpression?> body = new List<NodeExpression?> ();
            body.Add(ParseSimpleExpression());
            bool bracketClose = false;
            while (currentLex.Type == TokenType.Separator && (currentLex.Value == "," || currentLex.Value == "]"))
            {
                switch (currentLex.Value)
                {
                    case "]":
                        bracketClose = true;
                        NextToken();
                        if (currentLex.Value == "[")
                        {
                            bracketClose = false;
                            NextToken();
                            body.Add(ParseSimpleExpression());
                        }
                        break;
                    case ",":
                        NextToken();
                        body.Add(ParseSimpleExpression());
                        break;
                }
            }
            if (!bracketClose)
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected ']'");
            }
            return new NodeArrayPosition("[]", body);
        }
        public NodeExpression ParseRecordField(NodeExpression node)
        {
            if(currentLex.Type == TokenType.Indifier)
            {
                NodeExpression field = new NodeVar((SymVar)symTableStack.Get(currentLex.Value));
                NextToken();
                return new NodeRecordAccess(".", node, field);
            }
            else
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected indifier");
            }
        }
    }
}
