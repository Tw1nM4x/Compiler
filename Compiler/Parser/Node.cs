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
    }
    public class MainProgramNode : Node 
    {
        string? name;
        List<TypesNode?> types;
        BlockStmt body;
        public MainProgramNode(string? name, List<TypesNode?> types, BlockStmt body)
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
            foreach (TypesNode? type in types)
            {
                if(type != null)
                {
                    res += prefix + $"├─── {type.ToString(ListAddLeft(isLeftParents))}\r\n";
                }
            }
            res += prefix + $"└─── {body.ToString(ListAddRight(isLeftParents))}";
            return res;
        }
    }
    public class ProgramNode : Node
    {
        List<TypesNode?> types;
        BlockStmt body;
        public ProgramNode(List<TypesNode?> types, BlockStmt body)
        {
            this.types = types;
            this.body = body;
        }
        public override string ToString(List<bool> isLeftParents)
        {
            string res;
            string prefix = GetPrefixNode(isLeftParents);
            res = $"program\r\n";
            foreach (TypesNode? type in types)
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
