using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class TypesNode : Node { }
    public class ConstTypesNode : TypesNode 
    {
        List<ConstDeclarationNode> body;
        public ConstTypesNode(List<ConstDeclarationNode> body)
        {
            this.body = body;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"const\r\n";
            int i = 1;
            foreach (ConstDeclarationNode el in body)
            {
                if (i == body.Count)
                {
                    if (el != null)
                    {
                        res += prefix + $"└─── {el.ToString(ListAddRight(isLeftParents))}";
                    }
                }
                else
                {
                    if (el != null)
                    {
                        res += prefix + $"├─── {el.ToString(ListAddLeft(isLeftParents))}\r\n";
                    }
                    i++;
                }
            }
            return res;
        }
    }
    public class VarTypesNode : TypesNode
    {
        List<VarDeclarationNode> body;
        public VarTypesNode(List<VarDeclarationNode> body)
        {
            this.body = body;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"var\r\n";
            int i = 1;
            foreach (VarDeclarationNode el in body)
            {
                if (i == body.Count)
                {
                    if (el != null)
                    {
                        res += prefix + $"└─── {el.ToString(ListAddRight(isLeftParents))}";
                    }
                }
                else
                {
                    if (el != null)
                    {
                        res += prefix + $"├─── {el.ToString(ListAddLeft(isLeftParents))}\r\n";
                    }
                    i++;
                }
            }
            return res;
        }
    }
    public class TypeTypesNode : TypesNode
    {
        List<DeclarationNode> body;
        public TypeTypesNode(List<DeclarationNode> body)
        {
            this.body = body;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"type\r\n";
            int i = 1;
            foreach (DeclarationNode el in body)
            {
                if (i == body.Count)
                {
                    if (el != null)
                    {
                        res += prefix + $"└─── {el.ToString(ListAddRight(isLeftParents))}";
                    }
                }
                else
                {
                    if (el != null)
                    {
                        res += prefix + $"├─── {el.ToString(ListAddLeft(isLeftParents))}\r\n";
                    }
                    i++;
                }
            }
            return res;
        }
    }
    public class LabelTypesNode : TypesNode
    {
        List<string> body;
        public LabelTypesNode(List<string> body)
        {
            this.body = body;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"label\r\n";
            int i = 1;
            foreach (string el in body)
            {
                if (i == body.Count)
                {
                    if (el != null)
                    {
                        res += prefix + $"└─── {el}";
                    }
                }
                else
                {
                    if (el != null)
                    {
                        res += prefix + $"├─── {el}\r\n";
                    }
                    i++;
                }
            }
            return res;
        }
    }
    public class ProcedureTypesNode : TypesNode
    {
        string name;
        List<DeclarationNode> param;
        Node program;
        public ProcedureTypesNode(string name, List<DeclarationNode> param, Node program)
        {
            this.name = name;
            this.param = param;
            this.program = program;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"procedure\r\n";
            res += prefix + $"├─── {name}\r\n";
            foreach(DeclarationNode el in param)
            {
                res += prefix + $"├─── {el.ToString(ListAddLeft(isLeftParents))}\r\n";
            }
            res += prefix + $"└─── {program.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public class DeclarationNode : Node { }
    public class VarDeclarationNode : DeclarationNode
    {
        List<string> name;
        TypeNode type;
        ExpressionNode? value = null;
        public ExpressionNode? GetValue()
        {
            return value;
        }
        public VarDeclarationNode(List<string> name, TypeNode type, ExpressionNode? value)
        {
            this.name = name;
            this.type = type;
            this.value = value;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $":\r\n";
            foreach (string el in name)
            {
                res += prefix + $"├─── {el}\r\n";
            }
            if(value != null)
            {
                res += prefix + $"├─── {type.ToString(ListAddLeft(isLeftParents))}\r\n";
                res += prefix + $"└─── =\r\n" +
                       prefix + $"     └─── {value.ToString(ListAddRight(ListAddRight(isLeftParents)))}";
            }
            else
            {
                res += prefix + $"└─── {type.ToString(ListAddRight(isLeftParents))}";
            }
            return res;
        }
    }
    public class RefVarDeclarationNode : DeclarationNode
    {
        VarDeclarationNode body;
        public RefVarDeclarationNode(VarDeclarationNode body)
        {
            this.body = body;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"var\r\n";
            res += prefix + $"└─── {body.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public class TypeDeclarationNode : DeclarationNode
    {
        string name;
        TypeNode type;
        public TypeDeclarationNode(string name, TypeNode type)
        {
            this.name = name;
            this.type = type;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"=\r\n";
            res += prefix + $"├─── {name}\r\n";
            res += prefix + $"└─── {type.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public class ConstDeclarationNode : DeclarationNode
    {
        string name;
        ExpressionNode value;
        public ConstDeclarationNode(string name, ExpressionNode value)
        {
            this.name = name;
            this.value = value;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"=\r\n";
            res += prefix + $"├─── {name}\r\n";
            res += prefix + $"└─── {value.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
}
