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
            SymType buildsType = type;
            while (buildsType.GetType().Name == "SymTypeAlias")
            {
                SymTypeAlias symTypeAlias = (SymTypeAlias)type;
                buildsType = symTypeAlias.GetOriginalType();
            }
            return buildsType;
        }
        public SymVar(string name, SymType type) : base(name)
        {
            this.type = type;
        }
    }
    public class SymParamVar : SymVar
    {
        public SymParamVar(SymVar var) : base(var.GetName(), var.GetTypeVar()) { }
    }
    public class SymParamOut : SymVar
    {
        public SymParamOut(SymVar var) : base(var.GetName(), var.GetTypeVar()) { }
    }
    public class SymVarConst : SymVar
    {
        public SymVarConst(string name, SymType type) : base(name, type) { }
    }
    public class SymVarGlobal : SymVar
    {
        public SymVarGlobal(string name, SymType type) : base(name, type) { }
    }
    public class SymVarLocal : SymVar
    {
        public SymVarLocal(string name, SymType type) : base(name, type) { }
    }
    public class SymProc : Symbol
    {
        bool unlimitedParameters = false;
        SymTable params_;
        SymTable locals;
        BlockStmt body;
        public int GetCountParams()
        {
            if (unlimitedParameters)
            {
                return -1;
            }
            else
            {
                return params_.GetSize();
            }
        }
        public BlockStmt GetBody()
        {
            return body;
        }
        public SymProc(string name, SymTable params_, SymTable locals, BlockStmt body) : base(name)
        {
            this.params_ = params_;
            this.locals = locals;
            this.body = body;
        }
        public SymProc(string name) : base(name)
        {
            unlimitedParameters = true;
            this.params_ = new SymTable(new Dictionary<string, Symbol>());
            this.locals = new SymTable(new Dictionary<string, Symbol>());
            this.body = new BlockStmt(new List<NodeStatement>());
        }
    }
}
