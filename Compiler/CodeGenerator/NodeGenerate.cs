using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public partial class NodeMainProgram
    {
        public override void Generate(Generator generator)
        {
            generator.Add(Command.@extern, Call._printf);
            generator.Add(Command.@extern, Call._scanf);
            generator.Add(Command.global, "_main");
            foreach (NodeDefs el in types)
            {
                el.Generate(generator);
            }
            generator.Add(Command.section, ".text");
            if (!generator._mainDef)
            {
                generator.AddCommand("_main: ");
                generator._mainDef = true;
            }
            body.Generate(generator);
            generator.Add(Command.section, ".data");
            generator.AddCommand($"finalVal: {NasmType.dd}  0.0");
            generator.AddCommand($"real: {NasmType.dd}  0.0");
            generator.AddCommand($"format: {NasmType.db} '%s',0");
        }
    }
    public partial class NodeProgram
    {
        public override void Generate(Generator generator)
        {

        }
    }
    public partial class ConstDefsNode
    {
        public override void Generate(Generator generator)
        {
            generator.Add(Command.section, ".data");
            foreach (ConstDeclarationNode el in body)
            {
                el.Generate(generator);
            }
        }
    }

    public partial class VarDefsNode
    {
        public override void Generate(Generator generator)
        {
            generator.Add(Command.section, ".data");
            foreach (VarDeclarationNode el in body)
            {
                el.Generate(generator);
            }
        }
    }
    public partial class ProcedureDefsNode
    {
        public override void Generate(Generator generator)
        {

        }
    }
    public partial class VarDeclarationNode
    {
        public override void Generate(Generator generator)
        {
            foreach (SymVar var in vars)
            {
                if (var.GetTypeVar().GetType() == typeof(SymInteger))
                {
                    generator.AddCommand($"{generator.Mangle(var.GetName())}: {NasmType.dd} 0");
                }
                if (var.GetTypeVar().GetType() == typeof(SymReal))
                {
                    generator.AddCommand($"{generator.Mangle(var.GetName())}: {NasmType.dq} 0.0");
                }
                if (var.GetTypeVar().GetType() == typeof(SymString))
                {
                    generator.AddCommand($"{generator.Mangle(var.GetName())}: {NasmType.db} 0");
                }
                if (value != null)
                {
                    generator.Add(Command.section, ".text");
                    if (!generator._mainDef)
                    {
                        generator.AddCommand("_main: ");
                        generator._mainDef = true;
                    }
                    value.Generate(generator);
                    generator.Add(Command.pop, Register.eax);
                    generator.Add(Command.mov, $"[{generator.Mangle(var.GetName())}]", Register.eax);
                }
            }
        }
    }
    public partial class ConstDeclarationNode
    {
        public override void Generate(Generator generator)
        {
            if (var.GetTypeVar().GetType() == typeof(SymInteger))
            {
                generator.AddCommand($"{generator.Mangle(var.GetName())}: {NasmType.dd} 0");
            }
            if (var.GetTypeVar().GetType() == typeof(SymReal))
            {
                generator.AddCommand($"{generator.Mangle(var.GetName())}: {NasmType.dq} 0.0");
            }
            if (var.GetTypeVar().GetType() == typeof(SymString))
            {
                generator.AddCommand($"{generator.Mangle(var.GetName())}: {NasmType.db} 0");
            }
            if (value != null)
            {
                generator.Add(Command.section, ".text");
                if (!generator._mainDef)
                {
                    generator.AddCommand("_main: ");
                    generator._mainDef = true;
                }
                value.Generate(generator);
                generator.Add(Command.pop, Register.eax);
                generator.Add(Command.mov, $"[{generator.Mangle(var.GetName())}]", Register.eax);
            }
        }
    }
    public partial class NodeCast
    {
        public override void Generate(Generator generator)
        {

        }
    }
    public partial class NodeBinOp
    {
        public override void Generate(Generator generator)
        {
            left.Generate(generator);
            right.Generate(generator);
            generator.Add(Command.pop, Register.ebx);
            generator.Add(Command.pop, Register.eax);
            switch (opname)
            {
                case OperationSign.Plus:
                    generator.Add(Command.add, Register.eax, Register.ebx);
                    generator.Add(Command.push, Register.eax);
                    break;
                case OperationSign.Minus:
                    generator.Add(Command.sub, Register.eax, Register.ebx);
                    generator.Add(Command.push, Register.eax);
                    break;
                case OperationSign.Multiply:
                    generator.Add(Command.mul, Register.ebx);
                    generator.Add(Command.push, Register.eax);
                    break;
                case OperationSign.Divide:
                    generator.Add(Command.mov, Register.ecx, 0);
                    generator.Add(Command.mov, Register.edx, 0);
                    generator.Add(Command.div, Register.ebx);
                    generator.Add(Command.push, Register.eax);
                    break;
                case OperationSign.Equal:
                    generator.Add(Command.cmp, Register.eax, Register.ebx);
                    int key = generator.line;
                    generator.Add(Command.je, $"True_{key}");
                    generator.Add(Command.push, $"{NasmType.@byte} 0");
                    generator.Add(Command.jmp, $"False_{key}");
                    generator.AddCommand($"True_{key}:");
                    generator.Add(Command.push, $"{NasmType.@byte} 1");
                    generator.AddCommand($"False_{key}:");
                    break;
            }
        }
    }
    public partial class NodeRecordAccess
    {
        public override void Generate(Generator generator)
        {

        }
    }
    public partial class NodeUnOp
    {
        public override void Generate(Generator generator)
        {

        }
    }
    public partial class NodeArrayPosition
    {
        public override void Generate(Generator generator)
        {

        }
    }
    public partial class NodeVar
    {
        public override void Generate(Generator generator)
        {
            if (var_.GetTypeVar().GetType() == typeof(SymInteger))
            {
                generator.Add(Command.push, $"{NasmType.dword} [{generator.Mangle(var_.GetName())}]");
            }
            if (var_.GetTypeVar().GetType() == typeof(SymReal))
            {
                generator.Add(Command.sub, Register.esp, 8);
                generator.Add(Command.fld, $"{NasmType.dword} [{generator.Mangle(var_.GetName())}]");
                generator.Add(Command.fstp, $"{NasmType.qword} [{Register.esp}]");
            }
            if (var_.GetTypeVar().GetType() == typeof(SymString))
            {
                generator.Add(Command.push, $"[{generator.Mangle(var_.GetName())}]");
            }
        }
    }
    public partial class NodeInt
    {
        public override void Generate(Generator generator)
        {
            generator.Add(Command.push, $"{NasmType.dword} " + value);
        }
    }
    public partial class NodeReal
    {
        public override void Generate(Generator generator)
        {
            string real = value.ToString("F").Replace(',', '.');
            generator.Add(Command.sub, Register.esp, 8);
            generator.Add(Command.mov, "[real]", $"{NasmType.dword} __float32__({real})");
            generator.Add(Command.fld, $"{NasmType.dword} [real]");
            generator.Add(Command.fstp, $"{NasmType.qword} [{Register.esp}]");
        }
    }
    public partial class NodeString
    {
        public override void Generate(Generator generator)
        {
            for (int i = 0; i < value.Length; i++)
            {
                //generator.Add(Command.sub, Register.esp, 1);
                generator.Add(Command.push, $"{NasmType.@byte} \'{value[i]}\'");
            }
            //generator.Add(Command.sub, Register.esp, 1);
            generator.Add(Command.push, $"{NasmType.@byte} 0");
            generator.Add(Command.sub, Register.esp, value.Length);
        }
    }
    public partial class AssignmentStmt
    {
        public override void Generate(Generator generator)
        {
            right.Generate(generator);
            generator.Add(Command.pop, Register.eax);
            switch (opname)
            {
                case OperationSign.Assignment:
                    if (left.GetType() == typeof(NodeVar))
                    {
                        NodeVar var = (NodeVar)left;
                        generator.Add(Command.mov, $"[{generator.Mangle(var.GetName())}]",Register.eax);
                    }
                    break;
            }
        }
    }
    public partial class CallStmt
    {
        public override void Generate(Generator generator)
        {
            if(proc.GetName() == "write")
            {
                string format = "";
                for(int i = args.Count - 1; i > -1; i--)
                {
                    NodeExpression? el = args[i];
                    if (el != null)
                    {
                        if (el.GetCachedType().GetType() == typeof(SymReal))
                        {
                            format = "%g" + format;
                            el.Generate(generator);
                        }
                        if (el.GetCachedType().GetType() == typeof(SymInteger))
                        {
                            format = "%d" + format;
                            el.Generate(generator);
                        }
                        if (el.GetCachedType().GetType() == typeof(SymString))
                        {
                            if (el.GetType() == typeof(NodeString))
                            {
                                NodeString nodeStr = (NodeString)el;
                                format = nodeStr.GetValue() + format;
                            }
                            else if(el.GetType() == typeof(NodeBinOp))
                            {
                                NodeBinOp binOp = (NodeBinOp)el;
                                PrintPartOp(binOp);
                                void PrintPartOp(NodeBinOp part)
                                {
                                    if (part.right.GetType() == typeof(NodeString))
                                    {
                                        NodeString nodeStr = (NodeString)part.right;
                                        format = nodeStr.GetValue() + format;
                                    }
                                    else if (part.right.GetType() == typeof(NodeBinOp))
                                    {
                                        NodeBinOp binOp = (NodeBinOp)part.right;
                                        PrintPartOp(binOp);
                                    }
                                    if(part.left.GetType() == typeof(NodeString))
                                    {
                                        NodeString nodeStr = (NodeString)part.left;
                                        format = nodeStr.GetValue() + format;
                                    }
                                    else if(part.left.GetType() == typeof(NodeBinOp))
                                    {
                                        NodeBinOp binOp = (NodeBinOp)part.left;
                                        PrintPartOp(binOp);
                                    }
                                }
                            }
                        }
                    }
                }
                for (int i = 0; i < format.Length; i++)
                {
                    generator.Add(Command.mov, $"[format + {i}]", $"{NasmType.@byte} \'{format[i]}\'");
                }
                generator.Add(Command.mov, $"[format + {format.Length}]", $"{NasmType.@byte} 0");
                generator.Add(Command.push, "format");
                generator.Add(Command.call, Call._printf);
                generator.Add(Command.add, Register.esp, 4);
                generator.Add(Command.mov, Register.eax, 0);
            }
        }
    }
    public partial class IfStmt
    {
        public override void Generate(Generator generator)
        {
            condition.Generate(generator);
            generator.Add(Command.pop, Register.eax);
            generator.Add(Command.cmp, Register.eax, $"{NasmType.@byte} 1");
            int key = generator.line;
            generator.Add(Command.je, $"Then_{key}");
            elseBody.Generate(generator);
            generator.Add(Command.jmp, $"Else_{key}");
            generator.AddCommand($"Then_{key}:");
            body.Generate(generator);
            generator.AddCommand($"Else_{key}:");
        }
    }
    public partial class WhileStmt
    {
        public override void Generate(Generator generator)
        {
            condition.Generate(generator);
            generator.Add(Command.pop, Register.eax);
            int key = generator.line;
            generator.Add(Command.jmp, $"Check_{key}");
            generator.AddCommand($"Do_{key}:");
            body.Generate(generator);
            generator.AddCommand($"Check_{key}:");
            generator.Add(Command.cmp, Register.eax, $"{NasmType.@byte} 1");
            generator.Add(Command.je, $"Do_{key}");
        }
    }
    public partial class ForStmt
    {
        //NodeVar controlVar;
        //NodeExpression initialVal;
        //KeyWord toOrDownto;
        //NodeExpression finalVal;

        //NodeStatement body;
        public override void Generate(Generator generator)
        {
            initialVal.Generate(generator);
            generator.Add(Command.pop, Register.eax);
            generator.Add(Command.mov, $"[{generator.Mangle(controlVar.GetName())}]", Register.eax);
            finalVal.Generate(generator);
            generator.Add(Command.pop, Register.eax);
            generator.Add(Command.mov, "[finalVal]", Register.eax);

            int key = generator.line;
            generator.AddCommand($"Do_{key}:");
            body.Generate(generator);
            generator.Add(Command.inc, $"{NasmType.dword} [{generator.Mangle(controlVar.GetName())}]");

            generator.Add(Command.mov, Register.eax, $"{NasmType.dword} [{generator.Mangle(controlVar.GetName())}]");
            generator.Add(Command.cmp, Register.eax, $"{NasmType.dword} [finalVal]");
            generator.Add(Command.jle, $"Do_{key}");
        }
    }
    public partial class RepeatStmt
    {
        public override void Generate(Generator generator)
        {
            condition.Generate(generator);
            generator.Add(Command.pop, Register.eax);
            int key = generator.line;
            generator.AddCommand($"Do_{key}:");
            foreach(NodeStatement node in body)
            {
                node.Generate(generator);
            }
            generator.Add(Command.cmp, Register.eax, $"{NasmType.@byte} 1");
            generator.Add(Command.je, $"Do_{key}");
        }
    }
    public partial class BlockStmt
    {
        public override void Generate(Generator generator)
        {
            foreach(var el in body)
            {
                el.Generate(generator);
            }
        }
    }
}
