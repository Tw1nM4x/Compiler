using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class NodeDefs : Node { }
    public partial class ConstDefsNode : NodeDefs 
    {
        List<ConstDeclarationNode> body;
        public ConstDefsNode(List<ConstDeclarationNode> body)
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
    public partial class VarDefsNode : NodeDefs
    {
        List<VarDeclarationNode> body;
        public VarDefsNode(List<VarDeclarationNode> body)
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
    public partial class TypeDefsNode : NodeDefs
    {
        List<TypeDeclarationNode> body;
        public TypeDefsNode(List<TypeDeclarationNode> body)
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
    public partial class ProcedureDefNode : NodeDefs
    {
        List<VarDeclarationNode> params_;
        List<NodeDefs> localsTypes;
        SymProc symProc;
        public ProcedureDefNode(List<VarDeclarationNode> params_, List<NodeDefs> localsTypes, SymProc symProc)
        {
            this.params_ = params_;
            this.localsTypes = localsTypes;
            this.symProc = symProc;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"procedure {symProc.GetName()}\r\n";
            foreach (VarDeclarationNode el in params_)
            {
                res += prefix + $"├─── {el.ToString(ListAddLeft(isLeftParents))}\r\n";
            }
            foreach (NodeDefs? el in localsTypes)
            {
                res += prefix + $"├─── {el.ToString(ListAddLeft(isLeftParents))}\r\n";
            }
            res += prefix + $"└─── {symProc.GetBody().ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public class DeclarationNode : Node { }
    public partial class VarDeclarationNode : DeclarationNode
    {
        List<SymVar> vars;
        SymType type;
        NodeExpression? value = null;
        public List<SymVar> GetVars()
        {
            return vars;
        }
        public NodeExpression? GetValue()
        {
            return value;
        }
        public VarDeclarationNode(List<SymVar> name, SymType type, NodeExpression? value)
        {
            this.vars = name;
            this.type = type;
            this.value = value;
            if (value != null)
            {
                if (type.GetType() != value.GetCachedType().GetType())
                {
                    if (type.GetType() == typeof(SymReal) && value.GetCachedType().GetType() == typeof(SymInteger))
                    {
                        this.value = new NodeCast(type, value);
                    }
                    else
                    {
                        throw new Exception($"Incompatible types");
                    }
                }
            }
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $":\r\n";
            if(vars.Count > 1)
            {
                res += prefix + $"├─── \r\n";
                for (int i = 0; i < vars.Count - 1; i++)
                {
                    res += prefix + $"│    ├─── {vars[i].GetName()}\r\n";
                }
                res += prefix + $"│    └─── {vars[^1].GetName()}\r\n";
            }
            else
            {
                res += prefix + $"├─── {vars[0].GetName()}\r\n";
            }
            if (value != null)
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
    public partial class TypeDeclarationNode : DeclarationNode
    {
        string name;
        SymTypeAlias type;
        public TypeDeclarationNode(string name, SymTypeAlias type)
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
            res += prefix + $"└─── {type.GetOriginalType().ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public partial class ConstDeclarationNode : DeclarationNode
    {
        SymVarConst var;
        NodeExpression value;
        public ConstDeclarationNode(SymVarConst var, NodeExpression value)
        {
            this.var = var;
            this.value = value;
            if ((value.GetCachedType().GetType() != typeof(SymInteger)) &&
                (value.GetCachedType().GetType() != typeof(SymReal)) &&
                (value.GetCachedType().GetType() != typeof(SymString)))
            {
                throw new Exception($"Incompatible types");
            }
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"=\r\n";
            res += prefix + $"├─── {var.GetName()}\r\n";
            res += prefix + $"└─── {value.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
}
