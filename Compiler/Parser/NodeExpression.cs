using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class NodeExpression : Node 
    {
        private protected SymType cachedType = new SymType("");
        public SymType GetCachedType()
        {
            return cachedType;
        }
        public virtual SymType CalcType()
        {
            return new SymType("");
        }
    }
    public class NodeCast : NodeExpression
    {
        SymType cast;
        NodeExpression exp;
        public NodeCast(SymType cast, NodeExpression exp)
        {
            this.cast = cast;
            this.exp = exp;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"{cast.GetName()}\r\n";
            res += prefix + $"└─── {exp.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public class NodeBinOp : NodeExpression
    {
        string opname;
        NodeExpression left;
        NodeExpression right;
        public NodeBinOp(string opname, NodeExpression left, NodeExpression right)
        {
            this.opname = opname;
            this.left = left;
            this.right = right;
            cachedType = CalcType();
        }
        public override SymType CalcType()
        {
            SymType leftType = left.GetCachedType();
            SymType rightType = right.GetCachedType();
            if(leftType.GetType().Name == "SymRecord" || rightType.GetType().Name == "SymRecord" || leftType.GetType().Name == "SymArray" || rightType.GetType().Name == "SymArray")
            {
                throw new Exception("Operator is not overloaded");
            }
            if(leftType.GetType() != rightType.GetType())
            {
                if((leftType.GetType().Name == "SymInteger" || leftType.GetType().Name == "SymReal") &&
                   (rightType.GetType().Name == "SymInteger" || rightType.GetType().Name == "SymReal"))
                {
                    if (leftType.GetType().Name == "SymInteger")
                    {
                        left = new NodeCast(rightType, left);
                    }
                    else
                    {
                        right = new NodeCast(leftType, right);
                    }
                    if((opname == "<" || opname == "<=" || opname == ">" || opname == "=>" || opname == "=" || opname == "<>"))
                    {
                        return new SymBoolean("boolean");
                    }
                    return new SymReal("real");
                }
                else
                {
                    throw new Exception("Incompatible types");
                }
            }
            if((leftType.GetType().Name == "SymString" && rightType.GetType().Name == "SymString" && (opname == "/" || opname == "*" || opname == "-"))||
               ((opname == "or" || opname == "and" || opname == "not") && (leftType.GetType().Name != "SymBoolean" || rightType.GetType().Name != "SymBoolean")))
            {
                throw new Exception("Operator is not overloaded");
            }
            if ((opname == "<" || opname == "<=" || opname == ">" || opname == "=>" || opname == "=" || opname == "<>"))
            {
                return new SymBoolean("boolean");
            }
            return leftType;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"{opname}\r\n";
            res += prefix + $"├─── {left.ToString(ListAddLeft(isLeftParents))}\r\n";
            res += prefix + $"└─── {right.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public class NodeRecordAccess : NodeBinOp
    {
        public NodeRecordAccess(string opname, NodeExpression left, NodeExpression right) : base(opname, left, right) { }
    }
    public class NodeUnOp : NodeExpression
    {
        string opname;
        NodeExpression arg;
        public NodeUnOp(string opname, NodeExpression arg)
        {
            this.opname = opname;
            this.arg = arg;
            cachedType = CalcType();
        }
        public override SymType CalcType()
        {
            return arg.CalcType();
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"{opname}\r\n";
            res += prefix + $"└─── {arg.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public class NodeArrayPosition : NodeExpression
    {
        string name;
        List<NodeExpression?>? args;
        public NodeArrayPosition(string name, List<NodeExpression?>? arg)
        {
            this.name = name;
            this.args = arg;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"{name}\r\n";
            if(args != null)
            {
                int i = 1;
                List<NodeExpression?>? argsLast = new List<NodeExpression?>(args);
                foreach (NodeExpression? arg in args)
                {
                    if (i == args.Count)
                    {
                        if (arg != null)
                        {
                            res += prefix + $"└─── {arg.ToString(ListAddRight(isLeftParents))}";
                        }
                    }
                    else
                    {
                        if (arg != null)
                        {
                            res += prefix + $"├─── {arg.ToString(ListAddLeft(isLeftParents))}\r\n";
                        }
                        i++;
                    }
                }
            }
            return res;
        }
    }
    public class NodeVar : NodeExpression
    {
        SymVar var_;
        public string GetName()
        {
            return var_.GetName();
        }
        public NodeVar(SymVar var_)
        {
            this.var_ = var_;
            cachedType = CalcType();
        }
        public override SymType CalcType()
        {
            return var_.GetTypeVar();
        }
        public override string ToString(List<bool> isLeftParents)
        {
            return $"{var_.GetName()}";
        }
    }
    public class NodeInt : NodeExpression
    {
        int value;
        public NodeInt(int value)
        {
            this.value = value;
            cachedType = CalcType();
        }
        public override SymType CalcType()
        {
            return new SymInteger("integer");
        }
        public override string ToString(List<bool> isLeftParents)
        {
            return $"{value.ToString()}";
        }
    }
    public class NodeReal : NodeExpression
    {
        float value;
        public NodeReal(float value)
        {
            this.value = value;
            cachedType = CalcType();
        }
        public override SymType CalcType()
        {
            return new SymReal("real");
        }
        public override string ToString(List<bool> isLeftParents)
        {
            return $"{value.ToString("E10")}";
        }
    }
    public class NodeString : NodeExpression
    {
        string value;
        public NodeString(string value)
        {
            this.value = value;
            cachedType = CalcType();
        }
        public override SymType CalcType()
        {
            return new SymString("string");
        }
        public override string ToString(List<bool> isLeftParents)
        {
            return $"\'{value}\'";
        }
    }
}
