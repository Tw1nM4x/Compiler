namespace Compiler
{
    public class Parser
    {
        Lexer lexer;
        Token currentLex;
        SymTableStack symTableStack;
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
        void NextToken()
        {
            currentLex = lexer.GetNextToken();
        }
        private bool Expect(params object[] requires)
        {
            foreach(object require in requires)
            {
                if (Equals(currentLex.Value, require))
                {
                    return true;
                }
            }
            return false;
        }
        private void Require(object require)
        {
            if (!Expect(require))
            {
                if (require.GetType() == typeof(Separator))
                {
                    require = Lexer.GetStrSeparator((Separator)require);
                }
                if (require.GetType() == typeof(OperationSign))
                {
                    require = Lexer.GetStrOperationSign((OperationSign)require);
                }
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, $"expected '{require}'");
            }
            NextToken();
        }
        private bool ExpectType(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (currentLex.Type == type)
                {
                    return true;
                }
            }
            return false;
        }
        private void RequireType(TokenType type)
        {
            if(!ExpectType(type))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, $"expected {type}");
            }
        }
        public Node ParseMainProgram()
        {
            string name = "";
            List<NodeDefs> types = new List<NodeDefs>();
            BlockStmt body;
            if (Expect(KeyWord.PROGRAM))
            {
                name = ParseProgramName();
            }
            types = ParseDefs();
            Require(KeyWord.BEGIN);
            body = ParseBlock();
            Require(Separator.Point);
            return new NodeMainProgram(name, types, body);
        }
        public string ParseProgramName()
        {
            string res;
            NextToken();
            RequireType(TokenType.Identifier);
            res = (string)currentLex.Value;
            NextToken();
            Require(Separator.Semiсolon);
            return res;
        }
        public List<NodeDefs> ParseDefs()
        {
            List<NodeDefs> types = new List<NodeDefs>();
            while (Expect(KeyWord.VAR, KeyWord.CONST, KeyWord.TYPE, KeyWord.PROCEDURE))
            {
                switch (currentLex.Value)
                {
                    case KeyWord.VAR:
                        types.Add(ParseVarDefs());
                        break;
                    case KeyWord.CONST:
                        types.Add(ParseConstDefs());
                        break;
                    case KeyWord.TYPE:
                        types.Add(ParseTypeDefs());
                        break;
                    case KeyWord.PROCEDURE:
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
            RequireType(TokenType.Identifier);
            while (ExpectType(TokenType.Identifier))
            {
                string name = (string)currentLex.Value;
                NextToken();
                Require(OperationSign.Equal);
                NodeExpression value;
                value = ParseExpression();
                Require(Separator.Semiсolon);
                SymVarConst varConst = new SymVarConst(name, new SymType("const"));
                symTableStack.Add(name, varConst);
                body.Add(new ConstDeclarationNode(name, value));
            }
            return new ConstTypesNode(body);
        }
        public NodeDefs ParseVarDefs()
        {
            List<VarDeclarationNode> body = new List<VarDeclarationNode>();
            NextToken();
            RequireType(TokenType.Identifier);
            while (ExpectType(TokenType.Identifier))
            {
                body.Add(ParseVarDef());
                Require(Separator.Semiсolon);
            }
            return new VarTypesNode(body);
        }
        public NodeDefs ParseTypeDefs()
        {
            List<DeclarationNode> body = new List<DeclarationNode>();
            NextToken();
            while (currentLex.Type == TokenType.Identifier)
            {
                body.Add(ParseTypeDef());
                Require(Separator.Semiсolon);
            }
            return new TypeTypesNode(body);
        }
        public NodeDefs ParseProcedureDefs()
        {
            string name;
            List<VarDeclarationNode> paramsNode = new List<VarDeclarationNode>();
            SymTable locals = new SymTable(new Dictionary<string, Symbol>());
            NextToken();
            RequireType(TokenType.Identifier);
            name = (string)currentLex.Value;
            NextToken();
            symTableStack.AddTable(locals);
            if (Expect(Separator.OpenParenthesis))
            {
                do
                {
                    NextToken();
                    VarDeclarationNode varDef;
                    if (Expect(KeyWord.VAR, KeyWord.OUT))
                    {
                        KeyWord param = (KeyWord)currentLex.Value;
                        NextToken();
                        varDef = ParseVarDef(param);
                        paramsNode.Add(varDef);
                    }
                    else
                    {
                        varDef = ParseVarDef();
                        paramsNode.Add(varDef);
                    }
                } 
                while (Expect(Separator.Semiсolon));
                Require(Separator.CloseParenthesis);
            }
            Require(Separator.Semiсolon);

            SymTable params_ = new SymTable(symTableStack.GetBackTable());
            List<NodeDefs> localsTypes = ParseDefs();
            locals = symTableStack.GetBackTable();
            Require(KeyWord.BEGIN);
            BlockStmt body = ParseBlock();
            Require(Separator.Semiсolon);
            symTableStack.PopBack();
            SymProc symProc = new SymProc(name, params_, locals, body);
            symTableStack.Add(name, symProc);
            return new ProcedureTypesNode(paramsNode, localsTypes, symProc);
        }
        public VarDeclarationNode ParseVarDef(KeyWord? param = null)
        {
            List<string> names = new List<string>();
            List<SymVar> vars = new List<SymVar> ();
            SymType type;
            NodeExpression? value = null;
            RequireType(TokenType.Identifier);
            names.Add((string)currentLex.Value);
            NextToken();
            while (Expect(Separator.Comma))
            {
                NextToken();
                RequireType(TokenType.Identifier);
                names.Add((string)currentLex.Value);
                NextToken();
            }
            Require(OperationSign.Colon);
            if (!(ExpectType(TokenType.Identifier) || ExpectType(TokenType.Key_word)))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected type variable");
            }
            type = ParseType();
            if (Expect(OperationSign.Equal))
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
                    case KeyWord.VAR:
                        symTableStack.Add(name, new SymParamVar(var));
                        var = new SymParamVar(var);
                        break;
                    case KeyWord.OUT:
                        symTableStack.Add(name, new SymParamOut(var));
                        var = new SymParamOut(var);
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
            RequireType(TokenType.Identifier);
            nameType = (string)currentLex.Value;
            NextToken();
            Require(OperationSign.Equal);
            type = ParseType();
            SymTypeAlias typeAlias = new SymTypeAlias(type.GetName(), type);
            symTableStack.Add(nameType, type);
            return new TypeDeclarationNode( nameType, typeAlias );
        }
        public SymType ParseType()
        {
            if (!(ExpectType(TokenType.Identifier) || ExpectType(TokenType.Key_word)))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected type variable");
            }
            Token type = currentLex;
            NextToken();
            switch (type.Value)
            {
                case KeyWord.ARRAY:
                    return ParseArrayType();
                case KeyWord.RECORD:
                    return ParseRecordType();
                case KeyWord.STRING:
                    return (SymType)symTableStack.Get("string");
                case "integer":
                    return (SymType)symTableStack.Get("integer");
                case "real":
                    return (SymType)symTableStack.Get("real");
                default:
                    SymType original;
                    try
                    {
                        Symbol sym = symTableStack.Get((string)type.Value);
                        original = (SymType) sym;
                    }
                    catch
                    {
                        throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, $"Identifier not found \"{type.Source}\"");
                    }
                    return new SymTypeAlias((string)type.Value, original);
            }
        }
        public SymType ParseArrayType()
        {
            SymType type;
            List<OrdinalTypeNode> ordinalTypes = new List<OrdinalTypeNode> ();

            if (!Expect(Separator.OpenBracket))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected '['");
            }
            do
            {
                NextToken();
                ordinalTypes.Add(ParseArrayOrdinalType());
            }
            while (Expect(Separator.Comma));
            Require(Separator.CloseBracket);
            Require(KeyWord.OF);
            if (ExpectType(TokenType.Identifier) || ExpectType(TokenType.Key_word))
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
            Require(Separator.DoublePoint);
            NodeExpression to = ParseSimpleExpression();
            return new OrdinalTypeNode(from, to);
        }
        public SymType ParseRecordType()
        {
            SymTable fields = new SymTable(new Dictionary<string, Symbol>());
            symTableStack.AddTable(fields);
            while (currentLex.Type == TokenType.Identifier)
            {
                ParseVarDef();
                if (Expect(KeyWord.END))
                {
                    break;
                }
                Require(Separator.Semiсolon);
            }
            Require(KeyWord.END);
            symTableStack.PopBack();
            return new SymRecord("record", fields);
        }
        public NodeStatement ParseStatement()
        {
            NodeStatement res = new NullStmt();
            if (ExpectType(TokenType.Identifier))
            {
                res = ParseSimpleStatement();
                return res;
            }
            //structStmt
            switch (currentLex.Value)
            {
                case KeyWord.BEGIN:
                    res = ParseBlock();
                    break;
                case KeyWord.IF:
                    res = ParseIf();
                    break;
                case KeyWord.FOR:
                    res = ParseFor();
                    break;
                case KeyWord.WHILE:
                    res = ParseWhile();
                    break;
                case KeyWord.REPEAT:
                    res = ParseRepeat();
                    break;
                case Separator.Semiсolon:
                    break;
                case KeyWord.EXIT:
                    res = new CallStmt(symTableStack.Get("exit"), null);
                    NextToken();
                    break;
                default:
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected statement");
            }
            return res;
        }
        public NodeStatement ParseSimpleStatement()
        {
            string operation;
            string name;
            NodeExpression left;
            NodeExpression right;
            SymVar symVar;

            int lineStart = currentLex.NumberLine;
            int symStart = currentLex.NumberSymbol;
            try
            {
                //assigmentStmt
                symVar = (SymVar)symTableStack.Get((string)currentLex.Value);
                left = new NodeVar(symVar);
                name = (string)currentLex.Value;
                NextToken();
                while (Expect(Separator.OpenBracket, Separator.Point))
                {
                    Separator separator = (Separator)currentLex.Value;
                    NextToken();
                    switch (separator)
                    {
                        case Separator.OpenBracket:
                            left = ParsePositionArray(left, ref symVar);
                            break;
                        case Separator.Point:
                            left = ParseRecordField(left, ref symVar);
                            name = symVar.GetName();
                            break;
                    }
                }
            }
            catch
            {
                //procedureStmt
                name = (string)currentLex.Value;
                NextToken();
                return ParseProcedureStmt(name, lineStart, symStart);
            }
            //assigmentStmt
            if (!Expect(OperationSign.Assignment, OperationSign.Addition, OperationSign.Subtraction, OperationSign.Multiplication, OperationSign.Division))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, $"Expected assigment sign");
            }
            operation = Lexer.GetStrOperationSign((OperationSign)currentLex.Value);
            NextToken();
            int lineStartExp = currentLex.NumberLine;
            int symStartExp = currentLex.NumberSymbol;
            right = ParseExpression();
            if(symVar.GetTypeVar().GetName() != right.GetCachedType().GetName())
            {
                throw new ExceptionWithPosition(lineStartExp, symStartExp, $"Incompatible types: got \"{right.GetCachedType().GetName()}\" expected \"{symVar.GetTypeVar().GetName()}\"");
            }
            return new AssignmentStmt(operation, left, right);
        }
        public BlockStmt ParseBlock()
        {
            List<NodeStatement> body = new List<NodeStatement>();
            while (!Expect(KeyWord.END))
            {
                body.Add(ParseStatement());
                if (Expect(Separator.Semiсolon))
                {
                    NextToken();
                    continue;
                }
                if (!Expect(KeyWord.END))
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected ';'");
                }
            }
            Require(KeyWord.END);
            return new BlockStmt(body);
        }
        public NodeStatement ParseFor()
        {
            KeyWord toOrDownto;
            NodeVar controlVar;
            NodeExpression initialValue;
            NodeExpression finalValue;
            NodeStatement? body;
            NextToken();
            RequireType(TokenType.Identifier);
            controlVar = new NodeVar((SymVar)symTableStack.Get((string)currentLex.Value));
            NextToken();
            Require(OperationSign.Assignment);
            initialValue = ParseSimpleExpression();
            if (!Expect(KeyWord.TO, KeyWord.DOWNTO))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected 'to' or 'downto'");
            }
            toOrDownto = (KeyWord)currentLex.Value;
            NextToken();
            finalValue = ParseSimpleExpression();
            Require(KeyWord.DO);
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
            Require(KeyWord.THEN);
            if (!Expect(KeyWord.ELSE))
            {
                body = ParseStatement();
            }
            if (Expect(KeyWord.ELSE))
            {
                NextToken();
                elseStatement = ParseStatement();
            }
            return new IfStmt(condition, body, elseStatement);
        }
        public NodeStatement ParseWhile()
        {
            NodeExpression condition;
            NodeStatement body;

            NextToken();
            condition = ParseExpression();
            Require(KeyWord.DO);
            body = ParseStatement();
            return new WhileStmt(condition, body);
        }
        public NodeStatement ParseRepeat()
        {
            List<NodeStatement> body = new List<NodeStatement>();
            NodeExpression cond;

            do
            {
                NextToken();
                if (Expect(KeyWord.UNTIL))
                {
                    break;
                }
                body.Add(ParseStatement());
            } while (Expect(Separator.Semiсolon));
            Require(KeyWord.UNTIL);
            cond = ParseExpression();
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
            if (Expect(Separator.OpenParenthesis))
            {
                NextToken();
                while (!Expect(Separator.CloseParenthesis))
                {
                    parameter.Add(ParseFactor());
                    if (Expect(Separator.Comma))
                    {
                        NextToken();
                    }
                    else
                    {
                        break;
                    }
                }
                Require(Separator.CloseParenthesis);
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
            while (Expect(OperationSign.Less, OperationSign.LessOrEqual, OperationSign.Greater, OperationSign.GreaterOrEqual, OperationSign.Equal, OperationSign.NotEqual))
            {
                OperationSign operation = (OperationSign)currentLex.Value;
                NextToken();
                NodeExpression right = ParseSimpleExpression();
                left = new NodeBinOp(operation, left, right);
            }
            return left;
        }
        public NodeExpression ParseSimpleExpression()
        {
            NodeExpression left = ParseTerm();
            while (Expect(OperationSign.Plus, OperationSign.Minus, KeyWord.OR, KeyWord.XOR))
            {
                object operation = currentLex.Value;
                NextToken();
                NodeExpression right = ParseTerm();
                left = new NodeBinOp(operation, left, right );
            }
            return left;
        }
        public NodeExpression ParseTerm()
        {
            NodeExpression left = ParseFactor();
            while (Expect(OperationSign.Multiply, OperationSign.Divide, KeyWord.AND))
            {
                object operation = currentLex.Value;
                NextToken();
                NodeExpression right = ParseFactor();
                left = new NodeBinOp(operation, left, right);
            }
            return left;
        }
        public NodeExpression ParseFactor()
        {
            if (Expect(Separator.OpenParenthesis))
            {
                NodeExpression e;
                NextToken();
                e = ParseExpression();
                Require(Separator.CloseParenthesis);
                return e;
            }
            if (ExpectType(TokenType.Integer))
            {
                Token factor = currentLex;
                NextToken();
                return new NodeInt((int)factor.Value);
            }
            if (ExpectType(TokenType.String))
            {
                Token factor = currentLex;
                NextToken();
                return new NodeString((string)factor.Value);
            }
            if (ExpectType(TokenType.Real))
            {
                Token factor = currentLex;
                NextToken();
                return new NodeReal((double)factor.Value);
            }
            if (ExpectType(TokenType.Identifier))
            {
                NodeExpression ans;
                Token factor = currentLex;
                NextToken();
                SymVar symVar;
                try
                {
                    symVar = (SymVar)symTableStack.Get((string)factor.Value);
                }
                catch
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, $"Identifier not found \"{factor.Value}\"");
                }
                ans = new NodeVar(symVar);
                while (Expect(Separator.OpenBracket, Separator.Point))
                {
                    Separator separator = (Separator)currentLex.Value;
                    NextToken();
                    switch (separator)
                    {
                        case Separator.OpenBracket:
                            ans = ParsePositionArray(ans, ref symVar);
                            break;
                        case Separator.Point:
                            ans = ParseRecordField(ans, ref symVar);
                            break;
                    }
                }
                return ans;
            }
            if (Expect(OperationSign.Plus, OperationSign.Minus, KeyWord.NOT))
            {
                object unOp = currentLex.Value;
                NextToken();
                NodeExpression factor = ParseFactor();
                return new NodeUnOp(unOp, factor);
            }
            throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected factor");
        }
        public NodeExpression ParsePositionArray(NodeExpression node, ref SymVar var_)
        {
            SymArray array;
            List<NodeExpression?> body = new List<NodeExpression?> ();
            bool bracketClose = false;

            body.Add(ParseSimpleExpression());
            while (Expect(Separator.Comma, Separator.CloseBracket))
            {
                switch (currentLex.Value)
                {
                    case Separator.CloseBracket:
                        array = (SymArray)var_.GetTypeVar();
                        var_ = new SymVar(var_.GetName(), array.GetTypeArray());
                        bracketClose = true;
                        NextToken();
                        if (Expect(Separator.OpenBracket))
                        {
                            bracketClose = false;
                            NextToken();
                            body.Add(ParseSimpleExpression());
                        }
                        break;
                    case Separator.Comma:
                        NextToken();
                        body.Add(ParseSimpleExpression());
                        break;
                }
            }
            if (!bracketClose)
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected ']'");
            }
            return new NodeArrayPosition(var_.GetName(), body);
        }
        public NodeExpression ParseRecordField(NodeExpression node, ref SymVar var_)
        {
            if(!ExpectType(TokenType.Identifier))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected Identifier");
            }
            SymRecord record = (SymRecord)var_.GetTypeVar();
            SymTable fields = record.GetFields();
            var_ = (SymVar)fields.Get((string)currentLex.Value);
            NodeExpression field = new NodeVar(var_);
            NextToken();
            return new NodeRecordAccess(OperationSign.PointRecord, node, field);
        }
    }
}
