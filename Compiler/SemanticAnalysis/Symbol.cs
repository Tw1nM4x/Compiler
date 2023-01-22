using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class Symbol : Node
    {
        string name;
        public string GetName()
        {
            return name;
        }
        public Symbol(string name)
        {
            this.name = name;
        }
    }
    public class SymVar : Symbol
    {
        SymType type;
        public SymType GetTypeVar()
        {
            return type;
        }
        public SymType GetOriginalTypeVar()
        {
            SymType buildsType = type;
            while (buildsType.GetType().Name == "SymTypeAlias")
            {
                SymTypeAlias symTypeAlias = (SymTypeAlias)buildsType;
                buildsType = symTypeAlias.GetOriginalType();
            }
            return buildsType;
        }
        public SymVar(string name, SymType type) : base(name)
        {
            this.type = type;
        }
    }
    public class SymVarParamVar : SymVar
    {
        public SymVarParamVar(SymVar var) : base("var " + var.GetName(), var.GetTypeVar()) { }
    }
    public class SymVarParamOut : SymVar
    {
        public SymVarParamOut(SymVar var) : base("out " + var.GetName(), var.GetTypeVar()) { }
    }
    public enum VarType
    {
        Param,
        Const,
        Global,
        Local
    }
    public class SymVarParam : SymVar
    {
        public int offset = 0;
        public SymVarParam(SymVar var) : base(var.GetName(), var.GetTypeVar()) { }
    }
    public class SymVarConst : SymVar
    {
        public SymVarConst(string name, SymType type) : base(name, type) { }
    }
    public class SymVarGlobal : SymVar
    {
        public SymVarGlobal(SymVar var) : base(var.GetName(), var.GetTypeVar()) { }
    }
    public class SymVarLocal : SymVar
    {
        public int offset = 0;
        public SymVarLocal(SymVar var) : base(var.GetName(), var.GetTypeVar()) { }
    }
    public class SymProc : Symbol
    {
        bool unlimitedParameters = false;
        List<SymVar> args;
        SymTable locals;
        BlockStmt body;
        public List<SymVar> GetParams()
        {
            return args;
        }
        public SymTable GetLocals()
        {
            return locals;
        }
        public int GetCountParams()
        {
            if (unlimitedParameters)
            {
                return -1;
            }
            else
            {
                return args.Count();
            }
        }
        public BlockStmt GetBody()
        {
            return body;
        }
        public SymProc(string name, List<SymVar> args, SymTable locals, BlockStmt body) : base(name)
        {
            this.args = args;
            this.locals = locals;
            this.body = body;
        }
        public SymProc(string name) : base(name)
        {
            unlimitedParameters = true;
            this.args = new List<SymVar>();
            this.locals = new SymTable(new Dictionary<string, Symbol>());
            this.body = new BlockStmt(new List<NodeStatement>());
        }
    }
}
