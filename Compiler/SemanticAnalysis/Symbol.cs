using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class Symbol
    {
        string name;
        public Symbol(string name)
        {
            this.name = name;
        }
    }
    public class SymVar : Symbol
    {
        SymType type;
        public SymVar(string name, SymType type) : base(name)
        {
            this.type = type;
        }
    }
    public class SymParamVar : SymVar
    {
        public SymParamVar(string name, SymType type) : base(name, type) { }
    }
    public class SymParamOut : SymVar
    {
        public SymParamOut(string name, SymType type) : base(name, type) { }
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
        SymTable parameters;
        SymTable locals;
        BlockStmt body;
        public SymProc(string name, SymTable parameters, SymTable locals, BlockStmt body) : base(name)
        {
            this.parameters = parameters;
            this.locals = locals;
            this.body = body;
        }
    }
}
