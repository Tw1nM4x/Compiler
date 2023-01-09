using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class Node {
        public virtual string ToString(List<bool> isLeftParents) {
            return "node";
        }
        public static string GetPrefixNode(List<bool> isLeftParents)
        {
            string res = "";
            foreach (bool isLeft in isLeftParents)
            {
                if (isLeft)
                {
                    res += $"│    ";
                }
                else
                {
                    res += $"     ";
                }
            }
            return res;
        }
        public List<bool> ListAddLeft(List<bool> List)
        {
            List<bool> newList = new List<bool>(List);
            newList.Add(true);
            return newList;
        }
        public List<bool> ListAddRight(List<bool> List)
        {
            List<bool> newList = new List<bool>(List);
            newList.Add(false);
            return newList;
        }
        public virtual void Generate()
        {

        }
    }
    public partial class NodeMainProgram : Node 
    {
        string name;
        List<NodeDefs> types;
        BlockStmt body;
        public NodeMainProgram(string name, List<NodeDefs> types, BlockStmt body)
        {
            this.name = name;
            this.types = types;
            this.body = body;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"program {name}\r\n";
            foreach (NodeDefs type in types)
            {
                res += prefix + $"├─── {type.ToString(ListAddLeft(isLeftParents))}\r\n";
            }
            res += prefix + $"└─── {body.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public partial class NodeProgram : Node
    {
        List<NodeDefs?> types;
        BlockStmt body;
        public NodeProgram(List<NodeDefs?> types, BlockStmt body)
        {
            this.types = types;
            this.body = body;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"program\r\n";
            foreach (NodeDefs? type in types)
            {
                if (type != null)
                {
                    res += prefix + $"├─── {type.ToString(ListAddLeft(isLeftParents))}\r\n";
                }
            }
            res += prefix + $"└─── {body.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
}
