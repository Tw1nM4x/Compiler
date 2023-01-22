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
        public override string ToString(List<bool> isLeftParents)
        {
            return $"{GetName()}";
        }
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
    public class SymBoolean : SymType
    {
        public SymBoolean(string name) : base(name) { }
    }
    public class SymArray : SymType
    {
        List<OrdinalTypeNode> ordinalTypes;
        SymType type;
        public SymType GetTypeArray()
        {
            return type;
        }
        public List<OrdinalTypeNode> GetOrdinalTypeNode()
        {
            return ordinalTypes;
        }
        public SymArray(string name, List<OrdinalTypeNode> ordinalTypes, SymType type) : base(name)
        {
            this.ordinalTypes = ordinalTypes;
            this.type = type;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"array\r\n";
            foreach (OrdinalTypeNode ordinalType in ordinalTypes)
            {
                res += prefix + $"├─── {ordinalType.ToString(ListAddLeft(isLeftParents))}\r\n";
            }
            res += prefix + $"└─── {type.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public class OrdinalTypeNode : Node
    {
        public NodeExpression from;
        public NodeExpression to;
        public OrdinalTypeNode(NodeExpression from, NodeExpression to)
        {
            this.from = from;
            this.to = to;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"..\r\n";
            res += prefix + $"├─── {from.ToString(ListAddLeft(isLeftParents))}\r\n";
            res += prefix + $"└─── {to.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public class SymRecord : SymType
    {
        SymTable fields;
        public SymTable GetFields()
        {
            return fields;
        }
        public SymRecord(string name, SymTable fields) : base(name)
        {
            this.fields = fields;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"record \r\n";
            List<Symbol> symFields = new List<Symbol>(fields.GetData().Values);
            int i = 1;
            foreach (Symbol symField in symFields)
            {
                SymVar varField = (SymVar)symField;
                if (i == symFields.Count)
                {
                    res += prefix + $"└─── {varField.GetName()}\r\n";
                    res += prefix + $"     └─── {varField.GetOriginalTypeVar().ToString(ListAddRight(ListAddRight(isLeftParents)))}";
                }
                else
                {
                    res += prefix + $"├─── {varField.GetName()}\r\n";
                    res += prefix + $"│    └─── {varField.GetOriginalTypeVar().ToString(ListAddRight(ListAddLeft(isLeftParents)))}\r\n";
                    i++;
                }
            }
            return res;
        }
    }
    public class SymTypeAlias : SymType
    {
        SymType original;
        public SymType GetOriginalType()
        {
            return original;
        }
        public SymTypeAlias(string name, SymType original) : base(name)
        {
            this.original = original;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"{GetName()}\r\n";
            res += prefix + $"└─── {original.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
}
