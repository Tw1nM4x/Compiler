using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class ExpressionNode : Node { }
    public class NodeBinOp : ExpressionNode
    {
        string opname;
        ExpressionNode left;
        ExpressionNode right;
        public NodeBinOp(string opname, ExpressionNode left, ExpressionNode right)
        {
            this.opname = opname;
            this.left = left;
            this.right = right;
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
        public NodeRecordAccess(string opname, ExpressionNode left, ExpressionNode right) : base(opname, left, right) { }
    }
    public class NodeUnOp : ExpressionNode
    {
        string opname;
        ExpressionNode arg;
        public NodeUnOp(string opname, ExpressionNode arg)
        {
            this.opname = opname;
            this.arg = arg;
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
    public class NodeArrayPosition : ExpressionNode
    {
        string name;
        List<ExpressionNode?>? args;
        public NodeArrayPosition(string name, List<ExpressionNode?>? arg)
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
                List<ExpressionNode?>? argsLast = new List<ExpressionNode?>(args);
                foreach (ExpressionNode? arg in args)
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
    public class NodeVar : ExpressionNode
    {
        public string Name;
        public NodeVar(string name)
        {
            this.Name = name;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            return $"{Name}";
        }
    }
    public class NodeInt : ExpressionNode
    {
        int value;
        public NodeInt(int value)
        {
            this.value = value;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            return $"{value.ToString()}";
        }
    }
    public class NodeReal : ExpressionNode
    {
        float value;
        public NodeReal(float value)
        {
            this.value = value;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            return $"{value.ToString("E10")}";
        }
    }
    public class NodeString : ExpressionNode
    {
        string value;
        public NodeString(string value)
        {
            this.value = value;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            return $"\'{value}\'";
        }
    }
}
