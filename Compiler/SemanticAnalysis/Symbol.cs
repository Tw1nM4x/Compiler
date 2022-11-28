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
    public class SymTable
    {
        Dictionary<string, Symbol> data;
        public void Add(string name, Symbol value)
        {
            data.Add(name, value);
        }
        public Symbol? Get(string name)
        {
            Symbol? value;
            data.TryGetValue(name, out value);
            return value;
        }
        public SymTable(Dictionary<string, Symbol> data)
        {
            this.data = data;
        }
    }
    public class SymTableStack
    {
        List<SymTable> tables;
        public void Add(string name, Symbol value)
        {

        }
        public Symbol? Get(string name)
        {

        }
        public SymTableStack(List<SymTable> tables)
        {
            this.tables = tables;
        }
    }
    public class SymType : Symbol
    {
        public SymType(string name) : base (name)
        {
            
        }
    }
    public class SymInteger : SymType
    {
        public SymInteger(string name) : base(name)
        {

        }
    }
    public class SymReal : SymType
    {
        public SymReal(string name) : base(name)
        {

        }
    }
    public class SymString : SymType
    {
        public SymString(string name) : base(name)
        {

        }
    }
    public class SymArray : SymType
    {
        SymType elem;
        int low;
        int hi;
        public SymArray(string name, SymType elem, int low, int hi) : base(name)
        {
            this.elem = elem;
            this.low = low;
            this.hi = hi;
        }
    }
    public class SymRecord : SymType
    {
        SymTable fields;
        public SymRecord(string name, SymTable fields) : base(name)
        {
            this.fields = fields;
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
        public SymParamVar(string name, SymType type) : base(name, type)
        {

        }
    }
    public class SymParamOut : SymVar
    {
        public SymParamOut(string name, SymType type) : base(name, type)
        {

        }
    }
    public class SymTypeAlias : SymType
    {
        SymType original;
        public SymTypeAlias(string name, SymType original) : base(name)
        {
            this.original = original;
        }
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
