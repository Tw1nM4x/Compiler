using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class TypeNode : Node { }
    public class SimpleTypeNode : TypeNode
    {
        string type;
        public SimpleTypeNode(string type)
        {
            this.type = type;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            return $"{type}";
        }
    }
    public class StructuredTypeNode : TypeNode { }
    public class ArrayTypeNode : StructuredTypeNode
    {
        List<OrdinalTypeNode> ordinalTypes;
        TypeNode type;
        public ArrayTypeNode (List<OrdinalTypeNode> ordinalTypes, TypeNode type)
        {
            this.ordinalTypes = ordinalTypes;
            this.type = type;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"array\r\n";
            foreach(OrdinalTypeNode ordinalType in ordinalTypes)
            {
                res += prefix + $"├─── {ordinalType.ToString(ListAddLeft(isLeftParents))}\r\n";
            }
            res += prefix + $"└─── {type.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public class OrdinalTypeNode : Node
    {
        ExpressionNode from;
        ExpressionNode to;
        public OrdinalTypeNode(ExpressionNode from, ExpressionNode to)
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
    public class RecordTypeNode : StructuredTypeNode
    {
        List<VarDeclarationNode> types;
        public RecordTypeNode(List<VarDeclarationNode> types)
        {
            this.types = types;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"record\r\n";
            int i = 1;
            foreach (VarDeclarationNode type in types)
            {
                if (i == types.Count)
                {
                    if (type != null)
                    {
                        res += prefix + $"└─── {type.ToString(ListAddRight(isLeftParents))}";
                    }
                }
                else
                {
                    if (type != null)
                    {
                        res += prefix + $"├─── {type.ToString(ListAddLeft(isLeftParents))}\r\n";
                    }
                    i++;
                }
            }
            return res;
        }
    }
    public class ProceduralTypeNode : StructuredTypeNode
    {
        List<VarDeclarationNode> formalParameterList;
        public ProceduralTypeNode(List<VarDeclarationNode> formalParameterList)
        {
            this.formalParameterList = formalParameterList;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"procedure\r\n";
            int i = 1;
            foreach (VarDeclarationNode par in formalParameterList)
            {
                if (i == formalParameterList.Count)
                {
                    if (par != null)
                    {
                        res += prefix + $"└─── {par.ToString(ListAddRight(isLeftParents))}";
                    }
                }
                else
                {
                    if (par != null)
                    {
                        res += prefix + $"├─── {par.ToString(ListAddLeft(isLeftParents))}\r\n";
                    }
                    i++;
                }
            }
            return res;
        }
    }
}
