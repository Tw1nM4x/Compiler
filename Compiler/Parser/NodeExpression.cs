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
        public virtual string Print()
        {
            return "";
        }
    }
    public partial class NodeCast : NodeExpression
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
    public partial class NodeBinOp : NodeExpression
    {
        public object opname;
        public NodeExpression left;
        public NodeExpression right;
        public NodeBinOp(object opname, NodeExpression left, NodeExpression right)
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
            if(leftType.GetType() != rightType.GetType())
            {
                if ((leftType.GetType() == typeof(SymInteger) || leftType.GetType() == typeof(SymReal)) &&
                   (rightType.GetType() == typeof(SymInteger) || rightType.GetType() == typeof(SymReal)))
                {
                    if (leftType.GetType() == typeof(SymInteger))
                    {
                        left = new NodeCast(rightType, left);
                    }
                    else
                    {
                        right = new NodeCast(leftType, right);
                    }
                    return new SymReal("real");
                }
                else
                {
                    throw new Exception($"Incompatible types");
                }
            }
            if (opname.GetType() == typeof(OperationSign))
            {
                OperationSign op = (OperationSign) opname;
                if (op == OperationSign.Equal || op == OperationSign.Less || op == OperationSign.LessOrEqual ||
                   op == OperationSign.Greater || op == OperationSign.GreaterOrEqual || op == OperationSign.NotEqual)
                {
                    return new SymBoolean("boolean");
                }
                if ((op == OperationSign.Minus || op == OperationSign.Multiply || op == OperationSign.Divide ||
                    op == OperationSign.Subtraction || op == OperationSign.Multiplication || op == OperationSign.Subtraction) 
                    && leftType.GetType() == typeof(SymString))
                {
                    throw new Exception("Operator is not overloaded");
                }
            }
            return leftType;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            string? opnameStr = opname.ToString();
            if(opnameStr != null)
            {
                opnameStr = opnameStr.ToLower();
            }
            if (opname.GetType() == typeof(OperationSign))
            {
                opnameStr = Lexer.GetStrOperationSign((OperationSign)opname);
            }
            res = $"{opnameStr}\r\n";
            res += prefix + $"├─── {left.ToString(ListAddLeft(isLeftParents))}\r\n";
            res += prefix + $"└─── {right.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public partial class NodeRecordAccess : NodeBinOp
    {
        public NodeRecordAccess(object opname, NodeExpression left, NodeExpression right) : base(opname, left, right) { }

        public override SymType CalcType()
        {
            SymType rightType = right.GetCachedType();
            return rightType;
        }
    }
    public partial class NodeUnOp : NodeExpression
    {
        object opname;
        NodeExpression arg;
        public NodeUnOp(object opname, NodeExpression arg)
        {
            this.opname = opname;
            this.arg = arg;
            cachedType = CalcType();
        }
        public override SymType CalcType()
        {
            if (arg.CalcType().GetType() != typeof(SymInteger) && arg.CalcType().GetType() != typeof(SymReal))
            {
                throw new Exception("Operator is not overloaded (expected Integer or Real)");
            }
            return arg.CalcType();
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            string? opnameStr = opname.ToString();
            if (opnameStr != null)
            {
                opnameStr = opnameStr.ToLower();
            }
            if (opname.GetType() == typeof(OperationSign))
            {
                opnameStr = Lexer.GetStrOperationSign((OperationSign)opname);
            }
            res = $"{opnameStr}\r\n";
            res += prefix + $"└─── {arg.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public partial class NodeArrayPosition : NodeExpression
    {
        string name;
        SymArray symArray;
        public List<NodeExpression> args;
        public string GetName()
        {
            return name;
        }
        public NodeArrayPosition(string name, SymArray symArray, List<NodeExpression> arg)
        {
            this.name = name;
            this.symArray = symArray;
            this.args = arg;
            cachedType = CalcType();
        }
        public override SymType CalcType()
        {
            SymType res = symArray.GetTypeArray();
            while (res.GetType() == typeof(SymArray))
            {
                res = ((SymArray)res).GetTypeArray();
            }
            return res;
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
    public partial class NodeVar : NodeExpression
    {
        public SymVar var_;
        public string GetName()
        {
            return var_.GetName();
        }
        public SymVar GetSymVar()
        {
            return var_;
        }
        public NodeVar(SymVar var_)
        {
            this.var_ = var_;
            cachedType = CalcType();
        }
        public override SymType CalcType()
        {
            return var_.GetOriginalTypeVar();
        }
        public override string ToString(List<bool> isLeftParents)
        {
            return $"{var_.GetName()}";
        }
    }
    public partial class NodeInt : NodeExpression
    {
        public int value;
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
    public partial class NodeReal : NodeExpression
    {
        public double value;
        public NodeReal(double value)
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
            return $"{value.ToString()}";
        }
    }
    public partial class NodeString : NodeExpression
    {
        public string value;
        public string GetValue()
        {
            return value;
        }
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
