using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class SymTable
    {
        public int sizeLocal = 0; 
        public int sizeParam = 0;
        Dictionary<string, Symbol> data;
        public Dictionary<string, Symbol> GetData()
        {
            return data;
        }
        public int GetSizeLocal()
        {
            return sizeLocal;
        }
        public int GetSizeParam()
        {
            return sizeParam;
        }
        public int GetSize()
        {
            return data.Count;
        }
        public void Add(string name, Symbol value)
        {
            if(data.TryAdd(name, value))
            {
                if(value.GetType() == typeof(SymVarLocal))
                {
                    SymVarLocal varLocal = (SymVarLocal)value;
                    if(varLocal.GetOriginalTypeVar().GetType() == typeof(SymInteger))
                    {
                        sizeLocal += 4;
                    }
                    varLocal.offset = sizeLocal;
                }
                if (value.GetType() == typeof(SymVarParam))
                {
                    SymVarParam varParam = (SymVarParam)value;
                    if (varParam.GetOriginalTypeVar().GetType() == typeof(SymInteger))
                    {
                        sizeParam += 4;
                    }
                    varParam.offset = sizeParam;
                }
                return;
            }
            else
            {
                throw new Exception($"Duplicate identifier \"{name}\"");
            }
        }
        public Symbol Get(string name)
        {
            Symbol value;
            Symbol? check;
            if (data.TryGetValue(name, out check))
            {
                value = check;
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
        public SymTable(SymTable original)
        {
            this.data = new Dictionary<string, Symbol>(original.data);
        }
    }
    public class SymTableStack
    {
        List<SymTable> tables;
        public int GetCountTables()
        {
            return tables.Count;
        }
        public SymTable GetTable(int index)
        {
            return tables[index];
        }
        public SymTable GetBackTable()
        {
            return tables[^1];
        }
        public void AddTable(SymTable table)
        {
            tables.Add(table);
        }
        public void PopBack()
        {
            tables.RemoveAt(tables.Count - 1);
        }
        public void Check(string name)
        {
            if (GetBackTable().GetData().ContainsKey(name))
            {
                throw new Exception($"Duplicate identifier \"{name}\"");
            }
        }
        public void Add(string name, Symbol value)
        {
            try
            {
                GetBackTable().Add(name, value);
            }
            catch
            {
                throw new Exception($"Duplicate identifier \"{name}\"");
            }
        }
        public Symbol Get(string name)
        {
            Symbol res = new Symbol("");
            bool decl = false;
            for(int i = tables.Count - 1; i >= 0; i--)
            {
                try
                {
                    res = tables[i].Get(name);
                }
                catch
                {
                    continue;
                }
                finally
                {
                    decl = true;
                }
            }
            if (!decl)
            {
                throw new Exception("Variable not declared");
            }
            return res;
        }
        public SymTableStack()
        {
            tables = new List<SymTable>();
        }
    }
}
