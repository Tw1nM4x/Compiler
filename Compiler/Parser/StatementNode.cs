using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class StatementNode : Node { }
    public class NullStmt : StatementNode
    {
        public NullStmt() { }
        public override string ToString(List<bool> isLeftParents)
        {
            return "";
        }
    }
    public class LabelStmt : StatementNode
    {
        NodeVar name;
        StatementNode stmt;
        public LabelStmt(NodeVar name, StatementNode stmt)
        {
            this.name = name;
            this.stmt = stmt;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $":\r\n";
            res += prefix + $"├─── {name.ToString(ListAddRight(isLeftParents))}\r\n";
            res += prefix + $"└─── {stmt.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public class AssignmentStmt : StatementNode
    {
        string opname;
        ExpressionNode left;
        ExpressionNode right;
        public AssignmentStmt(string opname, ExpressionNode left, ExpressionNode right)
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
    public class CallStmt : StatementNode
    {
        string name;
        List<ExpressionNode?>? args;
        public CallStmt(string name, List<ExpressionNode?>? arg)
        {
            this.name = name;
            this.args = arg;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"{name}";
            if (args != null && args.Count > 0)
            {
                res += $"\r\n";
                int i = 1;
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
    public class GotoStmt : StatementNode
    {
        ExpressionNode label;
        public GotoStmt(ExpressionNode label)
        {
            this.label = label;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"goto\r\n";
            res += prefix + $"└─── {label.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public class IfStmt : StatementNode
    {
        ExpressionNode condition;
        StatementNode body;
        StatementNode elseBody;
        public IfStmt(ExpressionNode condition, StatementNode body, StatementNode elseBody)
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
    public class WhileStmt : StatementNode
    {
        ExpressionNode condition;
        StatementNode body;
        public WhileStmt(ExpressionNode condition, StatementNode body)
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
    public class ForStmt : StatementNode
    {
        NodeVar controlVar;
        ExpressionNode initialVal;
        string toOrDownto;
        ExpressionNode finalVal;
        StatementNode body;
        public ForStmt(NodeVar controlVar, ExpressionNode initialVal, string toOrDownto, ExpressionNode finalVal, StatementNode body)
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
                   prefix + $"├─── {toOrDownto}\r\n" +
                   prefix + $"│    └─── {finalVal.ToString(ListAddRight(ListAddLeft(isLeftParents)))}\r\n" +
                   prefix + $"└─── {body.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public class RepeatStmt : StatementNode
    {
        ExpressionNode condition;
        List<StatementNode> body;
        public RepeatStmt(List<StatementNode> body, ExpressionNode condition)
        {
            this.condition = condition;
            this.body = body;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"repeat\r\n";
            foreach (StatementNode? stmt in body)
            {
                res += prefix + $"├─── {stmt.ToString(ListAddLeft(isLeftParents))}\r\n";
            }
            res += prefix + $"└─── {condition.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public class CommandStmt : StatementNode
    {
        string name;
        public CommandStmt(string name)
        {
            this.name = name;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            res = $"{name}";
            return res;
        }
    }
    public class BlockStmt : StatementNode
    {
        List<StatementNode> body;
        public BlockStmt(List<StatementNode> body)
        {
            this.body = body;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            isLeftParents.Add(true);
            res = $"begin\r\n";
            foreach (StatementNode? stmt in body)
            {
                res += prefix + $"├─── {stmt.ToString(isLeftParents)}\r\n";
            }
            res += prefix + $"└─── end";
            return res;
        }
    }
}
