using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class NodeStatement : Node { }
    public class NullStmt : NodeStatement
    {
        public NullStmt() { }
        public override string ToString(List<bool> isLeftParents)
        {
            return "";
        }
    }
    public partial class AssignmentStmt : NodeStatement
    {
        string opname;
        NodeExpression left;
        NodeExpression right;
        public AssignmentStmt(string opname, NodeExpression left, NodeExpression right)
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
    public partial class CallStmt : NodeStatement
    {
        SymProc proc;
        List<NodeExpression?>? args;
        public CallStmt(Symbol proc, List<NodeExpression?>? arg)
        {
            this.proc = (SymProc) proc;
            this.args = arg;
        }
        public CallStmt(SymProc proc, List<NodeExpression?>? arg)
        {
            this.proc = proc;
            this.args = arg;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"{proc.GetName()}";
            if (args != null && args.Count > 0)
            {
                res += $"\r\n";
                int i = 1;
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
    public partial class IfStmt : NodeStatement
    {
        NodeExpression condition;
        NodeStatement body;
        NodeStatement elseBody;
        public IfStmt(NodeExpression condition, NodeStatement body, NodeStatement elseBody)
        {
            this.condition = condition;
            this.body = body;
            this.elseBody = elseBody;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"if\r\n";
            res += prefix + $"├─── {condition.ToString(ListAddLeft(isLeftParents))}\r\n";
            res += prefix + $"├─── {body.ToString(ListAddLeft(isLeftParents))}\r\n";
            res += prefix + $"└─── {elseBody.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public partial class WhileStmt : NodeStatement
    {
        NodeExpression condition;
        NodeStatement body;
        public WhileStmt(NodeExpression condition, NodeStatement body)
        {
            this.condition = condition;
            this.body = body;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"while\r\n";
            res += prefix + $"├─── {condition.ToString(ListAddLeft(isLeftParents))}\r\n";
            res += prefix + $"└─── {body.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public partial class ForStmt : NodeStatement
    {
        NodeVar controlVar;
        NodeExpression initialVal;
        KeyWord toOrDownto;
        NodeExpression finalVal;
        NodeStatement body;
        public ForStmt(NodeVar controlVar, NodeExpression initialVal, KeyWord toOrDownto, NodeExpression finalVal, NodeStatement body)
        {
            this.controlVar = controlVar;
            this.initialVal = initialVal;
            this.toOrDownto = toOrDownto;
            this.finalVal = finalVal;
            this.body = body;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"for\r\n";
            res += prefix + $"├─── :=\r\n" +
                   prefix + $"│    ├─── {controlVar.ToString(isLeftParents)}\r\n" +
                   prefix + $"│    └─── {initialVal.ToString(ListAddRight(ListAddLeft(isLeftParents)))}\r\n" +
                   prefix + $"├─── {toOrDownto.ToString().ToLower()}\r\n" +
                   prefix + $"│    └─── {finalVal.ToString(ListAddRight(ListAddLeft(isLeftParents)))}\r\n" +
                   prefix + $"└─── {body.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public partial class RepeatStmt : NodeStatement
    {
        NodeExpression condition;
        List<NodeStatement> body;
        public RepeatStmt(List<NodeStatement> body, NodeExpression condition)
        {
            this.condition = condition;
            this.body = body;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"repeat\r\n";
            foreach (NodeStatement? stmt in body)
            {
                res += prefix + $"├─── {stmt.ToString(ListAddLeft(isLeftParents))}\r\n";
            }
            res += prefix + $"└─── {condition.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public partial class BlockStmt : NodeStatement
    {
        List<NodeStatement> body;
        public BlockStmt(List<NodeStatement> body)
        {
            this.body = body;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            isLeftParents.Add(true);
            res = $"begin\r\n";
            foreach (NodeStatement? stmt in body)
            {
                res += prefix + $"├─── {stmt.ToString(isLeftParents)}\r\n";
            }
            res += prefix + $"└─── end";
            return res;
        }
    }
}
