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
            builtins.Add("exit", new SymProc("exit"));
            symTableStack.AddTable(new SymTable(builtins));
            symTableStack.AddTable(new SymTable(new Dictionary<string, Symbol>()));
            NextToken();
        }
        public string PrintSymTable()
        {
            string PrintSymProc(Dictionary<string, Symbol> dic, int index, int depth)
            {
                string res = "";
                SymProc symProc = (SymProc)dic.ElementAt(index).Value;
                Dictionary<string, Symbol> dicLocals = symProc.GetLocals().GetData();
                if(dicLocals.Count > 0)
                {
                    for (int i = 0; i < depth; i++)
                    {
                        res += "\t";
                    }
                    res += $"locals of procedure \"{dic.ElementAt(index).Key}\": \r\n";
                    for (int z = 0; z < dicLocals.Count; z++)
                    {
                        for (int i = 0; i < depth; i++)
                        {
                            res += "\t";
                        }
                        res += dicLocals.ElementAt(z).Key.ToString() + ": " + dicLocals.ElementAt(z).Value.GetType().Name + "\r\n";
                        if (dicLocals.ElementAt(z).Value.GetType() == typeof(SymProc))
                        {
                            res += PrintSymProc(dicLocals, z, depth + 1);
                        }
                    }
                }
                return res;
            }
            string PrintSymRecord(Dictionary<string, Symbol> dic, int index, int depth)
            {
                string res = "";
                SymRecord symRecord = (SymRecord)dic.ElementAt(index).Value;
                Dictionary<string, Symbol> dicLocals = symRecord.GetFields().GetData();
                if (dicLocals.Count > 0)
                {
                    for (int i = 0; i < depth; i++)
                    {
                        res += "\t";
                    }
                    res += $"locals of record \"{dic.ElementAt(index).Key}\": \r\n";
                    for (int z = 0; z < dicLocals.Count; z++)
                    {
                        for (int i = 0; i < depth; i++)
                        {
                            res += "\t";
                        }
                        res += dicLocals.ElementAt(z).Key.ToString() + ": " + dicLocals.ElementAt(z).Value.GetType().Name + "\r\n";
                        if (dicLocals.ElementAt(z).Value.GetType() == typeof(SymRecord))
                        {
                            res += PrintSymRecord(dicLocals, z, depth + 1);
                        }
                    }
                }
                return res;
            }
            string res = "\r\nSymbol Tables:\r\n";
            for (int i = 0; i < symTableStack.GetCountTables(); i++)
            {
                switch (i)
                {
                    case 0:
                        res += $"builtins:\r\n";
                        break;
                    case 1:
                        res += $"globals:\r\n";
                        break;
                    default:
                        res += $"table #{i}\r\n";
                        break;
                }
                Dictionary<string, Symbol> dic = symTableStack.GetTable(i).GetData();
                for (int j = 0; j < dic.Count; j++)
                {
                    res += "\t" + dic.ElementAt(j).Key.ToString() + ": " + dic.ElementAt(j).Value.GetType().Name + "\r\n";
                    if (dic.ElementAt(j).Value.GetType() == typeof(SymProc))
                    {
                        res += PrintSymProc(dic, j, 2);
                    }
                    if (dic.ElementAt(j).Value.GetType() == typeof(SymRecord))
                    {
                        res += PrintSymRecord(dic, j, 2);
                    }
                }
            }
            return res;
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
                if (require is Separator r)
                {
                    require = Lexer.GetStrSeparator(r);
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
        public List<NodeDefs> ParseDefs(VarType varType = VarType.Global)
        {
            List<NodeDefs> types = new List<NodeDefs>();
            while (Expect(KeyWord.VAR, KeyWord.CONST, KeyWord.TYPE, KeyWord.PROCEDURE))
            {
                switch (currentLex.Value)
                {
                    case KeyWord.VAR:
                        types.Add(ParseVarDefs(varType: varType));
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
                symTableStack.Check(name);
                NextToken();
                Require(OperationSign.Equal);
                NodeExpression value;
                value = ParseExpression();
                Require(Separator.Semiсolon);
                SymVarConst varConst = new SymVarConst(name, value.GetCachedType(), value);
                symTableStack.Add(name, varConst);
                body.Add(new ConstDeclarationNode(varConst, value));
            }
            return new ConstDefsNode(body);
        }
        public NodeDefs ParseVarDefs(VarType varType = VarType.Global)
        {
            List<VarDeclarationNode> body = new List<VarDeclarationNode>();
            NextToken();
            RequireType(TokenType.Identifier);
            while (ExpectType(TokenType.Identifier))
            {
                body.Add(ParseVarDef(varType: varType));
                Require(Separator.Semiсolon);
            }
            return new VarDefsNode(body);
        }
        public NodeDefs ParseTypeDefs()
        {
            List<TypeDeclarationNode> body = new List<TypeDeclarationNode>();
            NextToken();
            while (currentLex.Type == TokenType.Identifier)
            {
                body.Add(ParseTypeDef());
                Require(Separator.Semiсolon);
            }
            return new TypeDefsNode(body);
        }
        public NodeDefs ParseProcedureDefs()
        {
            string name;
            List<VarDeclarationNode> paramsNode = new List<VarDeclarationNode>();
            SymTable locals = new SymTable(new Dictionary<string, Symbol>());
            List<SymVar> args = new List<SymVar>();
            NextToken();
            RequireType(TokenType.Identifier);
            name = (string)currentLex.Value;
            symTableStack.Check(name);
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
                        varDef = ParseVarDef(param: param, varType: VarType.Param);
                        paramsNode.Add(varDef);
                    }
                    else
                    {
                        varDef = ParseVarDef(varType: VarType.Param);
                        paramsNode.Add(varDef);
                    }
                } 
                while (Expect(Separator.Semiсolon));
                Require(Separator.CloseParenthesis);
            }
            Require(Separator.Semiсolon);

            foreach (VarDeclarationNode varDeclNode in paramsNode)
            {
                foreach (SymVar var in varDeclNode.GetVars())
                {
                    args.Add(var);
                }
            }

            List<NodeDefs> localsTypes = ParseDefs(varType: VarType.Local);
            locals = symTableStack.GetBackTable();
            Require(KeyWord.BEGIN);
            BlockStmt body = ParseBlock();
            Require(Separator.Semiсolon);
            symTableStack.PopBack();
            SymProc symProc = new SymProc(name, args, locals, body);
            symTableStack.Add(name, symProc);
            return new ProcedureDefNode(paramsNode, localsTypes, symProc);
        }
        public VarDeclarationNode ParseVarDef(KeyWord? param = null, VarType varType = VarType.Global)
        {
            List<string> names = new List<string>();
            List<SymVar> vars = new List<SymVar> ();
            SymType type;
            NodeExpression? value = null;
            RequireType(TokenType.Identifier);
            names.Add((string)currentLex.Value);
            symTableStack.Check(names[^1]);
            NextToken();
            while (Expect(Separator.Comma))
            {
                NextToken();
                RequireType(TokenType.Identifier);
                names.Add((string)currentLex.Value);
                symTableStack.Check(names[^1]);
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
                value = ParseExpression(inDef: true);
            }
            foreach(string name in names)
            {
                SymVar var = new SymVar(name, type);
                switch (varType)
                {
                    case VarType.Global:
                        var = new SymVarGlobal(var);
                        symTableStack.Add(name, var);
                        break;
                    case VarType.Local:
                        var = new SymVarLocal(var);
                        symTableStack.Add(name, var);
                        break;
                    case VarType.Param:
                        switch (param)
                        {
                            case KeyWord.VAR:
                                var = new SymVarParamVar(var);
                                symTableStack.Add(name, var);
                                break;
                            case KeyWord.OUT:
                                var = new SymVarParamOut(var);
                                symTableStack.Add(name, var);
                                break;
                            default:
                                var = new SymVarParam(var);
                                symTableStack.Add(name, var);
                                break;
                        }
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
            symTableStack.Check(nameType);
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
            NodeExpression from = ParseSimpleExpression(inDef: true);
            Require(Separator.DoublePoint);
            NodeExpression to = ParseSimpleExpression(inDef: true);
            return new OrdinalTypeNode(from, to);
        }
        public SymType ParseRecordType()
        {
            SymTable fields = new SymTable(new Dictionary<string, Symbol>());
            symTableStack.AddTable(fields);
            while (ExpectType(TokenType.Identifier))
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
                    NextToken();
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
                    res = new CallStmt(symTableStack.Get("exit"), new List<NodeExpression?>());
                    NextToken();
                    break;
                default:
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected statement");
            }
            return res;
        }
        public NodeStatement ParseSimpleStatement()
        {
            OperationSign operation;
            string name;
            NodeExpression left;
            NodeExpression right;
            SymVar symVar;

            int lineStart = currentLex.NumberLine;
            int symStart = currentLex.NumberSymbol;

            //assigmentStmt
            name = (string)currentLex.Value;
            Symbol sym = symTableStack.Get((string)currentLex.Value);
            NextToken();
            if(sym.GetType() != typeof(SymVarGlobal) && sym.GetType() != typeof(SymVarLocal) && sym.GetType() != typeof(SymVarParam) && sym.GetType() != typeof(SymVarParamVar) && sym.GetType() != typeof(SymVarParamOut))
            {
                if(sym.GetType() == typeof(SymProc))
                {
                    //procedureStmt
                    return ParseProcedureStmt(name, lineStart, symStart);
                }
                throw new ExceptionWithPosition(lineStart, symStart, $"Expected variable identifier {sym.GetType()}");
            }
            symVar = (SymVar)sym;
            left = new NodeVar(symVar);
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
                        if (((NodeRecordAccess)left).CalcType().GetType() == typeof(SymProc))
                        {
                            //procedureStmt
                            return ParseProcedureStmt(name, lineStart, symStart);
                        }
                        break;
                }
            }
            if (!Expect(OperationSign.Assignment, OperationSign.Addition, OperationSign.Subtraction, OperationSign.Multiplication, OperationSign.Division))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, $"Expected assigment sign");
            }
            operation = (OperationSign)currentLex.Value;
            NextToken();
            int lineStartExp = currentLex.NumberLine;
            int symStartExp = currentLex.NumberSymbol;
            right = ParseExpression();
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
            }
            Require(KeyWord.END);
            return HighLevelOptimization.RemoveUnreachebleCode(new BlockStmt(body));
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
            if (controlVar.CalcType().GetType() != typeof(SymInteger))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, $"Ordinal expression expected");
            }
            NextToken();
            Require(OperationSign.Assignment);
            initialValue = ParseSimpleExpression();
            if (initialValue.CalcType().GetType() != controlVar.GetCachedType().GetType())
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, $"Incompatible types");
            }
            if (!Expect(KeyWord.TO, KeyWord.DOWNTO))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected 'to' or 'downto'");
            }
            toOrDownto = (KeyWord)currentLex.Value;
            NextToken();
            finalValue = ParseSimpleExpression();
            if (finalValue.CalcType().GetType() != controlVar.GetCachedType().GetType())
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, $"Incompatible types");
            }
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
            if (condition.CalcType().GetType() != typeof(SymBoolean))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, $"Incompatible types: expected \"Boolean\"");
            }
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
            if (condition.CalcType().GetType() != typeof(SymBoolean))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, $"Incompatible types: expected \"Boolean\"");
            }
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
            if (cond.CalcType().GetType() != typeof(SymBoolean))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, $"Incompatible types: expected \"Boolean\"");
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
                throw new ExceptionWithPosition(lineProc, symProc, $"Procedure not found \"{name}\"");
            }
            if (Expect(Separator.OpenParenthesis))
            {
                NextToken();
                int i = 0;
                while (!Expect(Separator.CloseParenthesis))
                {
                    NodeExpression param = ParseSimpleExpression();
                    if(proc.GetName() != "read" && proc.GetName() != "write")
                    {
                        if (param.GetCachedType().GetType() != proc.GetParams()[i].GetOriginalTypeVar().GetType())
                        {
                            throw new ExceptionWithPosition(lineProc, symProc, $"Incompatible type for arg no. {i + 1}");
                        }
                        i += 1;
                    }
                    parameter.Add(param);
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
            if (proc.GetName() != "read" && proc.GetName() != "write")
            {
                if (proc.GetCountParams() != -1 && parameter.Count != proc.GetCountParams())
                {
                    throw new ExceptionWithPosition(lineProc, symProc, $"Wrong number of parameters specified for call to \"{proc.GetName()}\"");
                }
            }
            return new CallStmt(proc, parameter);
        }
        public NodeExpression ParseExpression(bool inDef = false)
        {
            NodeExpression left = ParseSimpleExpression(inDef);
            while (Expect(OperationSign.Less, OperationSign.LessOrEqual, OperationSign.Greater, OperationSign.GreaterOrEqual, OperationSign.Equal, OperationSign.NotEqual))
            {
                OperationSign operation = (OperationSign)currentLex.Value;
                NextToken();
                NodeExpression right = ParseSimpleExpression(inDef);
                left = new NodeBinOp(operation, left, right);
            }
            return left;
        }
        public NodeExpression ParseSimpleExpression(bool inDef = false)
        {
            NodeExpression left = ParseTerm(inDef);
            while (Expect(OperationSign.Plus, OperationSign.Minus, KeyWord.OR, KeyWord.XOR))
            {
                object operation = currentLex.Value;
                NextToken();
                NodeExpression right = ParseTerm(inDef);
                left = new NodeBinOp(operation, left, right );
                left = HighLevelOptimization.ConstantFolding((NodeBinOp)left);
            }
            return left;
        }
        public NodeExpression ParseTerm(bool inDef = false)
        {
            NodeExpression left = ParseFactor(inDef);
            while (Expect(OperationSign.Multiply, OperationSign.Divide, KeyWord.AND))
            {
                object operation = currentLex.Value;
                NextToken();
                NodeExpression right = ParseFactor(inDef);
                left = new NodeBinOp(operation, left, right);
                left = HighLevelOptimization.ConstantFolding((NodeBinOp)left); //optimization
            }
            return left;
        }
        public NodeExpression ParseFactor(bool inDef = false)
        {
            if (Expect(Separator.OpenParenthesis))
            {
                NodeExpression e;
                NextToken();
                e = ParseExpression(inDef);
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
                if (inDef && symVar.GetType() != typeof(SymVarConst))
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, $"Illegal expression");
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
            SymArray array = new SymArray("", new List<OrdinalTypeNode>(), new SymInteger(""));
            List<NodeExpression> body = new List<NodeExpression> ();
            bool bracketClose = false;
            bool end = false;

            body.Add(ParseSimpleExpression());
            while (Expect(Separator.Comma, Separator.CloseBracket))
            {
                if (end)
                {
                    break;
                }
                switch (currentLex.Value)
                {
                    case Separator.CloseBracket:
                        array = (SymArray)((NodeVar)node).GetSymVar().GetOriginalTypeVar();
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
                        if (!bracketClose)
                        {
                            NextToken();
                            body.Add(ParseSimpleExpression());
                        }
                        else
                        {
                            end = true;
                        }
                        break;
                }
            }
            if (!bracketClose)
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected ']'");
            }
            return new NodeArrayPosition(var_.GetName(), array, body);
        }
        public NodeExpression ParseRecordField(NodeExpression node, ref SymVar var_)
        {
            if(!ExpectType(TokenType.Identifier))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected Identifier");
            }
            SymRecord record = (SymRecord)var_.GetOriginalTypeVar();
            SymTable fields = record.GetFields();
            var_ = (SymVar)fields.Get((string)currentLex.Value);
            NodeExpression field = new NodeVar(var_);
            NextToken();
            return new NodeRecordAccess(OperationSign.PointRecord, node, field);
        }
    }
}
