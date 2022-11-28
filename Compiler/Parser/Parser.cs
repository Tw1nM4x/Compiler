namespace Compiler
{
    public class Parser
    {
        Lexer lexer;
        Token currentLex;
        void NextToken()
        {
            currentLex = lexer.GetNextToken();
        }
        public Parser(Lexer lexer)
        {
            this.lexer = lexer;
            NextToken();
        }
        public Node ParseProgram(bool isMain = false)
        {
            string? name = null;
            List<TypesNode?> types = new List<TypesNode?>();
            BlockStmt body;
            if (isMain)
            {
                if (currentLex.Type == TokenType.Key_word && currentLex.Value == "program")
                {
                    name = ParseNameProgram();
                }
            }
            while (currentLex.Type == TokenType.Key_word && (currentLex.Value == "var" || currentLex.Value == "procedure" || currentLex.Value == "label" || currentLex.Value == "const" || currentLex.Value == "type"))
            {
                switch (currentLex.Value)
                {
                    case "var":
                        types.Add(ParseVarTypes());
                        break;
                    case "const":
                        types.Add(ParseConstTypes());
                        break;
                    case "type":
                        types.Add(ParseTypeTypes());
                        break;
                    case "procedure":
                        types.Add(ParseProcedureTypes());
                        break;
                    case "label":
                        types.Add(ParseLabelTypes());
                        break;
                }
            }
            if (currentLex.Type != TokenType.Key_word || currentLex.Value != "begin")
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected 'begin'");
            }
            body = ParseBlock();
            if (isMain)
            {
                if (currentLex.Type != TokenType.Separator || currentLex.Value != ".")
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected '.'");
                }
                return new MainProgramNode(name, types, body);
            }
            else
            {
                if (currentLex.Type != TokenType.Separator || currentLex.Value != ";")
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected '.'");
                }
                NextToken();
                return new ProgramNode(types, body);
            }
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
        public TypesNode ParseConstTypes()
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
                ExpressionNode value;
                NextToken();
                value = ParseExpression();
                if (!(currentLex.Type == TokenType.Separator && currentLex.Value == ";"))
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected ';'");
                }
                body.Add(new ConstDeclarationNode(name, value));
                NextToken();
            }
            return new ConstTypesNode(body);
        }
        public TypesNode ParseVarTypes()
        {
            List<VarDeclarationNode> body = new List<VarDeclarationNode>();
            NextToken();
            if (currentLex.Type != TokenType.Indifier)
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected indifier");
            }
            while (currentLex.Type == TokenType.Indifier)
            {
                VarDeclarationNode var = ParseVarDecl();
                body.Add(var);
                if (!(currentLex.Type == TokenType.Separator && currentLex.Value == ";"))
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected ';'");
                }
                NextToken();
            }
            return new VarTypesNode(body);
        }
        public TypesNode ParseTypeTypes()
        {
            List<DeclarationNode> body = new List<DeclarationNode>();
            NextToken();
            while (currentLex.Type == TokenType.Indifier)
            {
                body.Add(ParseTypeDecl());
                if (!(currentLex.Type == TokenType.Separator && currentLex.Value == ";"))
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected ';'");
                }
                NextToken();
            }
            return new TypeTypesNode(body);
        }
        public TypesNode ParseLabelTypes()
        {
            List<string> body = new List<string> ();
            NextToken();
            do
            {
                if (currentLex.Type != TokenType.Indifier)
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected indifier");
                }
                body.Add(currentLex.Value);
                NextToken();
            } while (currentLex.Type == TokenType.Separator && currentLex.Value == ",");
            if (!(currentLex.Type == TokenType.Separator && currentLex.Value == ";"))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, $"expected ';'");
            }
            NextToken();
            return new LabelTypesNode(body);
        }
        public TypesNode ParseProcedureTypes()
        {
            string name = "";
            List<DeclarationNode> parameters = new List<DeclarationNode> ();
            Node program;
            NextToken();
            if (currentLex.Type != TokenType.Indifier)
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected indifier");
            }
            name = currentLex.Value;
            NextToken();
            if (currentLex.Type == TokenType.Separator && currentLex.Value == "(")
            {
                do
                {
                    NextToken();
                    if (currentLex.Type == TokenType.Key_word && currentLex.Value == "var")
                    {
                        NextToken();
                        parameters.Add(new RefVarDeclarationNode(ParseVarDecl()));
                    }
                    else
                    {
                        parameters.Add(ParseVarDecl());
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
            program = ParseProgram();
            return new ProcedureTypesNode(name, parameters, program);
        }
        public VarDeclarationNode ParseVarDecl()
        {
            List<string> name = new List<string> ();
            TypeNode type;
            ExpressionNode? value = null;
            if (!(currentLex.Type == TokenType.Indifier))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected indifier");
            }
            name.Add(currentLex.Value);
            NextToken();
            while (currentLex.Type == TokenType.Separator && currentLex.Value == ",")
            {
                NextToken();
                if (currentLex.Type != TokenType.Indifier)
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected indifier");
                }
                name.Add(currentLex.Value);
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
                if (name.Count > 1)
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"Only one variable can be initialized");
                }
                NextToken();
                value = ParseExpression();
            }
            return new VarDeclarationNode(name, type, value);
        }
        public TypeDeclarationNode ParseTypeDecl()
        {
            string nameType;
            TypeNode type;
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
            return new TypeDeclarationNode( nameType, type );
        }
        public TypeNode ParseType()
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
                case "procedure":
                    return ParseProcedureType();
                default:
                    return new SimpleTypeNode(type.Value);
            }
        }
        public TypeNode ParseArrayType()
        {
            TypeNode type;
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
            return new ArrayTypeNode(ordinalTypes, type);
        }
        public OrdinalTypeNode ParseArrayOrdinalType()
        {
            ExpressionNode from = ParseSimpleExpression();
            if (!(currentLex.Type == TokenType.Separator && currentLex.Value == ".."))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected '..'");
            }
            NextToken();
            ExpressionNode to = ParseSimpleExpression();
            return new OrdinalTypeNode(from, to);
        }
        public TypeNode ParseRecordType()
        {
            List<VarDeclarationNode> body = new List<VarDeclarationNode>();
            while (currentLex.Type == TokenType.Indifier)
            {
                body.Add(ParseVarDecl());
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
            NextToken();
            return new RecordTypeNode(body);
        }
        public TypeNode ParseProcedureType()
        {
            List<VarDeclarationNode> formalParameterList = new List<VarDeclarationNode>();
            if (!(currentLex.Type == TokenType.Separator && currentLex.Value == "("))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected '('");
            }
            do
            {
                NextToken();
                formalParameterList.Add(ParseVarDecl());
                if(formalParameterList[^1].GetValue() != null)
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected ')'");
                }
            }
            while (currentLex.Type == TokenType.Separator && currentLex.Value == ";");
            if (!(currentLex.Type == TokenType.Separator && currentLex.Value == ")"))
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected ')'");
            }
            NextToken();
            return new ProceduralTypeNode(formalParameterList);
        }
        public StatementNode ParseStatement()
        {
            StatementNode res = new NullStmt();
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
                        case "goto":
                            res = ParseGoto();
                            break;
                        case ";":
                            break;
                        case "exit":
                            res = new CallStmt("exit", null);
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
        public StatementNode ParseSimpleStatement()
        {
            string operation = "";
            ExpressionNode left;
            ExpressionNode right;
            if (currentLex.Type == TokenType.Indifier)
            {
                left = ParseFactor();
            }
            else
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected indifier");
            }
            //assigmentStmt
            if (currentLex.Type == TokenType.Operation_sign && (currentLex.Value == ":=" || currentLex.Value == "+=" || currentLex.Value == "-=" || currentLex.Value == "*=" || currentLex.Value == "/="))
            {
                operation = currentLex.Value;
                NextToken();
                right = ParseExpression();
                return new AssignmentStmt(operation, left, right);
            }
            else
            {
                //label
                if (left.GetType().Name == "NodeVar")
                {
                    NodeVar var = (NodeVar)left;
                    if (currentLex.Type == TokenType.Operation_sign && currentLex.Value == ":")
                    {
                        NextToken();
                        StatementNode stmt = ParseStatement();
                        return new LabelStmt(var, stmt);
                    }
                    else
                    {
                        return ParseProcedure(name: var.Name);
                    }
                }
                else
                {
                    throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected assignment sign");
                }
            }
        }
        public BlockStmt ParseBlock()
        {
            List<StatementNode> body = new List<StatementNode>();
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
        public StatementNode ParseFor()
        {
            string toOrDownto = "";
            NodeVar controlVar;
            ExpressionNode initialValue;
            ExpressionNode finalValue;
            StatementNode? body;
            NextToken();
            if (currentLex.Type != TokenType.Indifier)
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected indifier");
            }
            controlVar = new NodeVar(currentLex.Value);
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
        public StatementNode ParseIf()
        {
            ExpressionNode condition;
            StatementNode body = new NullStmt();
            StatementNode elseStatement = new NullStmt();

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
        public StatementNode ParseWhile()
        {
            ExpressionNode condition;
            StatementNode? body = null;

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
        public StatementNode ParseGoto()
        {
            ExpressionNode label;
            NextToken();
            if (currentLex.Type != TokenType.Indifier)
            {
                throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected indifier");
            }
            label = ParseFactor();
            NextToken();
            return new GotoStmt( label );
        }
        public StatementNode ParseRepeat()
        {
            List<StatementNode> body = new List<StatementNode>();
            ExpressionNode cond;

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
        public StatementNode ParseProcedure(string name)
        {
            List<ExpressionNode?> parameter = new List<ExpressionNode?>();
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

            return new CallStmt(name, parameter);
        }
        public ExpressionNode ParseExpression()
        {
            ExpressionNode left = ParseSimpleExpression();
            while (currentLex.Type == TokenType.Operation_sign &&
                  (currentLex.Value == "<" || currentLex.Value == "<=" || currentLex.Value == ">" || currentLex.Value == "=>" || currentLex.Value == "=" || currentLex.Value == "<>"))
            {
                string operation = currentLex.Value;
                NextToken();
                ExpressionNode right = ParseSimpleExpression();
                left = new NodeBinOp(operation, left, right);
            }
            return left;
        }
        public ExpressionNode ParseSimpleExpression()
        {
            ExpressionNode left = ParseTerm();
            while ((currentLex.Type == TokenType.Operation_sign && (currentLex.Value == "+" || currentLex.Value == "-")) ||
                   (currentLex.Type == TokenType.Key_word && (currentLex.Value == "or" || currentLex.Value == "xor")))
            {
                string operation = currentLex.Value;
                NextToken();
                ExpressionNode right = ParseTerm();
                left = new NodeBinOp(operation, left, right );
            }
            return left;
        }
        public ExpressionNode ParseTerm()
        {
            ExpressionNode left = ParseFactor(withUnOp: true);
            //throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol,"expected operation sign");
            while ((currentLex.Type == TokenType.Operation_sign && (currentLex.Value == "*" || currentLex.Value == "/")) ||
                   (currentLex.Type == TokenType.Key_word && currentLex.Value == "and"))
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
                ExpressionNode ans;
                Token factor = currentLex;
                NextToken();
                ans = new NodeVar(factor.Value);
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
                ExpressionNode factor = ParseFactor();
                return new NodeUnOp(unOp, factor);
            }

            throw new ExceptionWithPosition(currentLex.NumberLine, currentLex.NumberSymbol, "expected factor");
        }
        public ExpressionNode ParsePositionArray(ExpressionNode node)
        {
            List<ExpressionNode?> body = new List<ExpressionNode?> ();
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
        public ExpressionNode ParseRecordField(ExpressionNode node)
        {
            if(currentLex.Type == TokenType.Indifier)
            {
                ExpressionNode field = new NodeVar(currentLex.Value);
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
