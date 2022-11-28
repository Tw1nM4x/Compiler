using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
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
            if (data.TryGetValue(name, out value))
            {
                return value;
            }
            else
            {
                throw new Exception("Variable not declared");
            }
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
            tables[^1].Add(name, value);
        }
        public Symbol? Get(string name)
        {
            Symbol? res = null;
            foreach (SymTable table in tables)
            {
                try
                {
                    res = table.Get(name);
                }
                catch
                {
                    continue;
                }
            }
            if (res == null)
            {
                throw new Exception("Variable not declared");
            }
            return res;
        }
        public SymTableStack(List<SymTable> tables)
        {
            this.tables = tables;
        }
    }
}
