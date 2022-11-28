using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class SymType : Symbol
    {
        public SymType(string name) : base(name) { }
    }
    public class SymInteger : SymType
    {
        public SymInteger(string name) : base(name) { }
    }
    public class SymReal : SymType
    {
        public SymReal(string name) : base(name) { }
    }
    public class SymString : SymType
    {
        public SymString(string name) : base(name) { }
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
    public class SymTypeAlias : SymType
    {
        SymType original;
        public SymTypeAlias(string name, SymType original) : base(name)
        {
            this.original = original;
        }
    }
}
