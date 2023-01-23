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
            generator.AddCommand(Section.file, Command.@extern, Call._printf);
            generator.AddCommand(Section.file, Command.@extern, Call._scanf);
            generator.AddCommand(Section.file, Command.global, "_main");
            generator.AddCommand(Section.file, Command.section, ".data");
            generator.AddCommand(Section.file, Command.section, ".text");

            foreach (NodeDefs el in types)
            {
                if(el.GetType() == typeof(ProcedureDefNode))
                {
                    el.Generate(generator);
                }
            }
            generator.AddLine(Section.text, "_main :");
            foreach (NodeDefs el in types)
            {
                if (el.GetType() != typeof(ProcedureDefNode))
                {
                    el.Generate(generator);
                }
            }
            body.Generate(generator);
            generator.AddCommand(Section.text, Command.ret);
            generator.AddLine(Section.data, $"real: {NasmType.dd}  0.0");
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
            foreach (VarDeclarationNode el in body)
            {
                el.Generate(generator);
            }
        }
    }
    public partial class TypeDefsNode
    {
        public override void Generate(Generator generator)
        {
            foreach (TypeDeclarationNode el in body)
            {
                el.Generate(generator);
            }
        }
    }
    public partial class ProcedureDefNode
    {
        public override void Generate(Generator generator)
        {
            generator.AddLine(Section.text, $"{Generator.Mangle(symProc.GetName())}:");

            generator.AddCommand(Section.text, Command.push, Register.ebp);
            generator.AddCommand(Section.text, Command.mov, Register.ebp, Register.esp);

            generator.AddCommand(Section.text, Command.sub, Register.esp, symProc.GetLocals().GetSizeLocal());

            foreach (NodeDefs el in localsTypes)
            {
                if (el.GetType() != typeof(ProcedureDefNode))
                {
                    el.Generate(generator);
                }
            }
            symProc.GetBody().Generate(generator);

            generator.AddCommand(Section.text, Command.add, Register.esp, symProc.GetLocals().GetSizeParam());

            generator.AddCommand(Section.text, Command.mov, Register.esp, Register.ebp);
            generator.AddCommand(Section.text, Command.pop, Register.ebp);

            generator.AddCommand(Section.text, Command.ret);
            generator.AddLine(Section.text, "");

            foreach (NodeDefs el in localsTypes)
            {
                if (el.GetType() == typeof(ProcedureDefNode))
                {
                    el.Generate(generator);
                }
            }
        }
    }
    public partial class VarDeclarationNode
    {
        public override void Generate(Generator generator)
        {
            foreach (SymVar var in vars)
            {
                if(var.GetType() == typeof(SymVarGlobal))
                {
                    if (var.GetOriginalTypeVar().GetType() == typeof(SymInteger))
                    {
                        generator.AddLine(Section.data, $"{Generator.Mangle(var.GetName())}: {NasmType.dd} 0");
                    }
                    if (var.GetOriginalTypeVar().GetType() == typeof(SymReal))
                    {
                        generator.AddLine(Section.data, $"{Generator.Mangle(var.GetName())}: {NasmType.dd} 0.0");
                    }
                    if (var.GetOriginalTypeVar().GetType() == typeof(SymString))
                    {
                        generator.AddLine(Section.data, $"{Generator.Mangle(var.GetName())}: {NasmType.dd} 0");
                    }
                    if (var.GetOriginalTypeVar().GetType() == typeof(SymRecord))
                    {
                        SymRecord record = (SymRecord)var.GetOriginalTypeVar();

                        generator.AddLine(Section.data, "");
                        generator.AddLine(Section.data, $"{Generator.Mangle(var.GetName())}:");
                        generator.AddCommand(Section.data, Command.istruc, $"rec_{type.GetName()}");
                        foreach (string el in record.GetFields().GetData().Keys)
                        {
                            generator.AddCommand(Section.data, Command.at, $"rec_{type.GetName()}.{ Generator.Mangle(el)}", $"{NasmType.dd} 0");
                        }
                        generator.AddCommand(Section.data, Command.iend);
                        generator.AddLine(Section.data, "");
                    }
                    if (var.GetOriginalTypeVar().GetType() == typeof(SymArray))
                    {
                        SymArray array = (SymArray)var.GetOriginalTypeVar();
                        string size = "";
                        for(int i = 0; i < array.GetOrdinalTypeNode().Count; i++)
                        {
                            OrdinalTypeNode ordinalTypesNode = array.GetOrdinalTypeNode().ElementAt(i);
                            if (i > 0)
                            {
                                size += " * ";
                            }
                            generator.AddLine(Section.bss, $"{Generator.Mangle(var.GetName())}_from{i} : {Command.equ} {ordinalTypesNode.from.Print()}");
                            string res = $"({(ordinalTypesNode.to.Print())} - {Generator.Mangle(var.GetName())}_from{i} + 1)";
                            generator.AddLine(Section.bss, $"{Generator.Mangle(var.GetName())}_size{i} : {Command.equ} {res}");
                            size += $"{Generator.Mangle(var.GetName())}_size{i}";
                        }
                        generator.AddLine(Section.bss, $"{Generator.Mangle(var.GetName())}: {Command.resd} {size}");
                    }
                    if (value != null)
                    {
                        value.Generate(generator);
                        generator.AddCommand(Section.text, Command.pop, $"{NasmType.dword} [{Generator.Mangle(var.GetName())}]");
                    }
                }
                if (var.GetType() == typeof(SymVarLocal))
                {
                    if (value != null)
                    {
                        SymVarLocal varLocal = (SymVarLocal)var;
                        value.Generate(generator);
                        generator.AddCommand(Section.text, Command.pop, $"{NasmType.dword} [{Register.ebp} - {varLocal.offset}]");
                    }
                }
                if (var.GetType() == typeof(SymVarParam))
                {
                    if (value != null)
                    {
                        SymVarParam varParam = (SymVarParam)var;
                        value.Generate(generator);
                        generator.AddCommand(Section.text, Command.pop, $"{NasmType.dword} [{Register.ebp} + {4 + varParam.offset}]");
                    }
                }
            }
        }
    }
    public partial class ConstDeclarationNode
    {
        public override void Generate(Generator generator)
        {
            if(value.GetCachedType().GetType() == typeof(SymString))
            {
                generator.numberConst += 1;
                generator.AddLine(Section.data, $"string_{generator.numberConst}: {NasmType.db} \'{((NodeString)value).GetValue()}\',0");
                generator.AddLine(Section.file, $"{Generator.Mangle(var.GetName())} {Command.equ} string_{generator.numberConst}");
            }
            else
            {
                generator.AddLine(Section.file, $"{Generator.Mangle(var.GetName())} {Command.equ} {value.Print()}");
            }
        }
    }
    public partial class TypeDeclarationNode
    {
        public override void Generate(Generator generator)
        {
            if (type.GetOriginalType().GetType() == typeof(SymRecord))
            {
                SymRecord record = (SymRecord)type.GetOriginalType();

                generator.AddLine(Section.file, $"");
                generator.AddCommand(Section.file, Command.struc, $"rec_{name}");
                foreach (string el in record.GetFields().GetData().Keys)
                {
                    generator.AddLine(Section.file, $".{Generator.Mangle(el)}: resd 1");
                }
                generator.AddCommand(Section.file, Command.endstruc);
            }
        }
    }
    public partial class NodeCast
    {
        public override void Generate(Generator generator)
        {
            if(cast.GetType() == typeof(SymReal))
            {
                if(exp.GetCachedType().GetType() == typeof(SymInteger))
                {
                    exp.Generate(generator);
                    generator.AddCommand(Section.text, Command.pop, Register.eax);
                    generator.AddCommand(Section.text, Command.mov, $"[real]", Register.eax);
                    generator.AddCommand(Section.text, Command.fild, $"{NasmType.dword} [real]");
                    generator.AddCommand(Section.text, Command.sub, Register.esp, 4);
                    generator.AddCommand(Section.text, Command.fstp, $"{NasmType.dword} [{Register.esp}]");
                }
            }
        }
    }
    public partial class NodeBinOp
    {
        public override void Generate(Generator generator)
        {
            left.Generate(generator);
            right.Generate(generator);
            if (left.GetCachedType().GetType() == typeof(SymReal))
            {
                generator.AddCommand(Section.text, Command.fld, $"{NasmType.dword} [{Register.esp} + 4]");
                generator.AddCommand(Section.text, Command.fld, $"{NasmType.dword} [{Register.esp}]");
                switch (opname)
                {
                    case OperationSign.Plus:
                        generator.AddCommand(Section.text, Command.fadd);
                        break;
                    case OperationSign.Minus:
                        generator.AddCommand(Section.text, Command.fsub);
                        break;
                    case OperationSign.Multiply:
                        generator.AddCommand(Section.text, Command.fmul);
                        break;
                    case OperationSign.Divide:
                        generator.AddCommand(Section.text, Command.fdiv);
                        break;
                }
                generator.AddCommand(Section.text, Command.add, Register.esp, 4);
                generator.AddCommand(Section.text, Command.fstp, $"{NasmType.dword} [{Register.esp}]");

            }
            if (left.GetCachedType().GetType() == typeof(SymInteger))
            {
                generator.AddCommand(Section.text, Command.pop, Register.eax);
                generator.AddCommand(Section.text, Command.pop, Register.ebx);
                switch (opname)
                {
                    case OperationSign.Plus:
                        generator.AddCommand(Section.text, Command.add, Register.eax, Register.ebx);
                        generator.AddCommand(Section.text, Command.push, Register.eax);
                        break;
                    case OperationSign.Minus:
                        generator.AddCommand(Section.text, Command.sub, Register.eax, Register.ebx);
                        generator.AddCommand(Section.text, Command.push, Register.eax);
                        break;
                    case OperationSign.Multiply:
                        generator.AddCommand(Section.text, Command.imul, Register.ebx);
                        generator.AddCommand(Section.text, Command.push, Register.eax);
                        break;
                    case OperationSign.Divide:
                        generator.AddCommand(Section.text, Command.mov, Register.ecx, 0);
                        generator.AddCommand(Section.text, Command.mov, Register.edx, 0);
                        generator.AddCommand(Section.text, Command.idiv, Register.ebx);
                        generator.AddCommand(Section.text, Command.push, Register.eax);
                        break;
                }
                if(opname.GetType() == typeof(OperationSign))
                {
                    if ((OperationSign)opname == OperationSign.Less || (OperationSign)opname == OperationSign.LessOrEqual ||
                        (OperationSign)opname == OperationSign.Equal || (OperationSign)opname == OperationSign.NotEqual ||
                        (OperationSign)opname == OperationSign.GreaterOrEqual || (OperationSign)opname == OperationSign.Greater)
                    {
                        int key = generator.key;
                        generator.AddCommand(Section.text, Command.cmp, Register.eax, Register.ebx);
                        switch (opname)
                        {
                            case OperationSign.Less:
                                generator.AddCommand(Section.text, Command.jl, $"True_{key}");
                                break;
                            case OperationSign.LessOrEqual:
                                generator.AddCommand(Section.text, Command.jle, $"True_{key}");
                                break;
                            case OperationSign.Equal:
                                generator.AddCommand(Section.text, Command.je, $"True_{key}");
                                break;
                            case OperationSign.NotEqual:
                                generator.AddCommand(Section.text, Command.jne, $"True_{key}");
                                break;
                            case OperationSign.GreaterOrEqual:
                                generator.AddCommand(Section.text, Command.jge, $"True_{key}");
                                break;
                            case OperationSign.Greater:
                                generator.AddCommand(Section.text, Command.jg, $"True_{key}");
                                break;
                        }
                        generator.AddCommand(Section.text, Command.push, $"{NasmType.@byte} 0");
                        generator.AddCommand(Section.text, Command.jmp, $"False_{key}");
                        generator.AddLine(Section.text, $"True_{key}:");
                        generator.AddCommand(Section.text, Command.push, $"{NasmType.@byte} 1");
                        generator.AddLine(Section.text, $"False_{key}:");
                    }
                }
            }
            if (left.GetCachedType().GetType() == typeof(SymString))
            {
                generator.AddCommand(Section.text, Command.pop, Register.ebx);
                generator.AddCommand(Section.text, Command.pop, Register.eax);
                switch (opname)
                {
                    case OperationSign.Plus:
                        int key = generator.key;
                        generator.AddLine(Section.data, $"newstr_{key} : times 9 {NasmType.db} 0");
                        generator.AddCommand(Section.text, Command.push, $"{NasmType.dword} [{Register.eax}]");
                        generator.AddCommand(Section.text, Command.pop, $"{NasmType.dword} [newstr_{key}]");
                        generator.AddCommand(Section.text, Command.push, $"{NasmType.dword} [{Register.ebx}]");
                        generator.AddCommand(Section.text, Command.pop, $"{NasmType.dword} [newstr_{key} + 4]");
                        generator.AddCommand(Section.text, Command.push, $"newstr_{key}");
                        break;
                }
            }
        }
        public override string Print()
        {
            string result = "";
            result += "(" + left.Print();
            if (opname.GetType() == typeof(OperationSign))
            {
                result += " " + Lexer.GetStrOperationSign((OperationSign)opname) + " ";
            }
            else
            {
                result += " " + opname + " ";
            }
            result += right.Print() + ")";
            return result;
        }
    }
    public partial class NodeRecordAccess
    {
        public override void Generate(Generator generator)
        {
            NodeVar leftVar = (NodeVar)left;
            NodeVar rightVar = (NodeVar)right;
            generator.AddCommand(Section.text, Command.push, $"{NasmType.dword} [{Generator.Mangle(leftVar.GetName())} + rec_{leftVar.GetSymVar().GetTypeVar().GetName()}.{Generator.Mangle(rightVar.GetName())}]");
        }
    }
    public partial class NodeUnOp
    {
        public override void Generate(Generator generator)
        {
            arg.Generate(generator);
            switch (opname)
            {
                case OperationSign.Minus:
                    generator.AddCommand(Section.text, Command.pop, Register.eax);
                    if (arg.GetCachedType().GetType() == typeof(SymInteger))
                    {
                        generator.AddCommand(Section.text, Command.neg, Register.eax);
                        generator.AddCommand(Section.text, Command.push, Register.eax);
                    }
                    if (arg.GetCachedType().GetType() == typeof(SymReal))
                    {
                        generator.AddCommand(Section.text, Command.mov, $"[real]", $"{NasmType.dword} __float32__(0.0)");
                        generator.AddCommand(Section.text, Command.fld, $"{NasmType.dword} [real]");
                        generator.AddCommand(Section.text, Command.mov, $"[real]", Register.eax);
                        generator.AddCommand(Section.text, Command.fld, $"{NasmType.dword} [real]");
                        generator.AddCommand(Section.text, Command.fsub);
                        generator.AddCommand(Section.text, Command.sub, Register.esp, 4);
                        generator.AddCommand(Section.text, Command.fstp, $"{NasmType.dword} [{Register.esp}]");
                    }
                    break;
                case OperationSign.Plus:
                    break;
            }
        }
        public override string Print()
        {
            string result = "";
            result += "(";
            if (opname.GetType() == typeof(OperationSign))
            {
                result += Lexer.GetStrOperationSign((OperationSign)opname) + " ";
            }
            else
            {
                result += opname + " ";
            }
            result += arg.Print() + ")";
            return result;
        }
    }
    public partial class NodeArrayPosition
    {
        public override void Generate(Generator generator)
        {
            generator.AddCommand(Section.text, Command.mov, Register.ecx, 0);
            for (int i = 0; i < args.Count; i++)
            {
                NodeExpression arg = args.ElementAt(i);
                arg.Generate(generator);
                generator.AddCommand(Section.text, Command.pop, Register.eax);
                generator.AddCommand(Section.text, Command.mov, Register.ebx, $"{Generator.Mangle(name)}_from{i}");
                generator.AddCommand(Section.text, Command.sub, Register.eax, Register.ebx);
                if (i == args.Count - 1)
                {
                    generator.AddCommand(Section.text, Command.add, Register.ecx, Register.eax);
                }
                else
                {
                    generator.AddCommand(Section.text, Command.mov, Register.ebx, $"{Generator.Mangle(name)}_size{i}");
                    generator.AddCommand(Section.text, Command.imul, Register.ebx);
                    generator.AddCommand(Section.text, Command.add, Register.ecx, Register.eax);
                }
            }
            generator.AddCommand(Section.text, Command.push, $"{NasmType.dword} [{Generator.Mangle(name)} + 4 * {Register.ecx}]");
        }
    }
    public partial class NodeVar
    {
        public override void Generate(Generator generator)
        {
            if (var_.GetType() == typeof(SymVarGlobal))
            {
                if (var_.GetOriginalTypeVar().GetType() == typeof(SymInteger))
                {
                    generator.AddCommand(Section.text, Command.push, $"{NasmType.dword} [{Generator.Mangle(var_.GetName())}]");
                }
                if (var_.GetOriginalTypeVar().GetType() == typeof(SymReal))
                {
                    generator.AddCommand(Section.text, Command.sub, Register.esp, 4);
                    generator.AddCommand(Section.text, Command.fld, $"{NasmType.dword} [{Generator.Mangle(var_.GetName())}]");
                    generator.AddCommand(Section.text, Command.fstp, $"{NasmType.dword} [{Register.esp}]");
                }
                if (var_.GetOriginalTypeVar().GetType() == typeof(SymString))
                {
                    generator.AddCommand(Section.text, Command.push, $"dword [{Generator.Mangle(var_.GetName())}]");
                }
            }
            if (var_.GetType() == typeof(SymVarParam))
            {
                SymVarParam varParam = (SymVarParam)var_;
                generator.AddCommand(Section.text, Command.push, $"{NasmType.dword} [{Register.ebp} + {4 + varParam.offset}]");
            }
            if (var_.GetType() == typeof(SymVarLocal))
            {
                if (var_.GetOriginalTypeVar().GetType() == typeof(SymInteger))
                {
                    SymVarLocal varLocal = (SymVarLocal)var_;
                    generator.AddCommand(Section.text, Command.push, $"{NasmType.dword} [{Register.ebp} - {varLocal.offset}]");
                }
            }
            if (var_.GetType() == typeof(SymVarConst))
            {
                generator.AddCommand(Section.text, Command.push, $"{Generator.Mangle(var_.GetName())}");
            }
        }
        public override string Print()
        {
            return $"{Generator.Mangle(var_.GetName())}";
        }
    }
    public partial class NodeInt
    {
        public override void Generate(Generator generator)
        {
            generator.AddCommand(Section.text, Command.push, $"{NasmType.dword} " + value);
        }
        public override string Print()
        {
            return value.ToString();
        }
    }
    public partial class NodeReal
    {
        public override void Generate(Generator generator)
        {
            string real = value.ToString("F").Replace(',', '.');
            generator.AddCommand(Section.text, Command.sub, Register.esp, 4);
            generator.AddCommand(Section.text, Command.mov, $"[real]", $"{NasmType.dword} __float32__({real})");
            generator.AddCommand(Section.text, Command.fld, $"{NasmType.dword} [real]");
            generator.AddCommand(Section.text, Command.fstp, $"{NasmType.dword} [{Register.esp}]");
        }
        public override string Print()
        {
            return $"__float32__({value.ToString("F").Replace(',', '.')})";
        }
    }
    public partial class NodeString
    {
        public override void Generate(Generator generator)
        {
            generator.numberConst += 1;
            generator.AddLine(Section.data, $"string_{generator.numberConst}: {NasmType.db} \'{value}\',0");
            generator.AddCommand(Section.text, Command.push, $"string_{generator.numberConst}");
        }
        public override string Print()
        {
            return "\'" + value.ToString() + "\'";
        }
    }
    public partial class AssignmentStmt
    {
        public override void Generate(Generator generator)
        {
            right.Generate(generator);
            switch (opname)
            {
                case OperationSign.Assignment:
                    if (left.GetType() == typeof(NodeVar))
                    {
                        NodeVar var = (NodeVar)left;
                        if(var.GetSymVar().GetType() == typeof(SymVarGlobal))
                        {
                            if (var.GetSymVar().GetOriginalTypeVar().GetType() == typeof(SymInteger))
                            {
                                generator.AddCommand(Section.text, Command.pop, $"{NasmType.dword} [{Generator.Mangle(var.GetName())}]");
                            }
                            if (var.GetSymVar().GetOriginalTypeVar().GetType() == typeof(SymReal))
                            {
                                generator.AddCommand(Section.text, Command.pop, $"{NasmType.dword} [{Generator.Mangle(var.GetName())}]");
                            }
                            if (var.GetSymVar().GetOriginalTypeVar().GetType() == typeof(SymString))
                            {
                                generator.AddCommand(Section.text, Command.pop, $"{NasmType.dword} [{Generator.Mangle(var.GetName())}]");
                            }
                        }
                        if (var.GetSymVar().GetType() == typeof(SymVarParam))
                        {
                            SymVarParam varParam = (SymVarParam)var.GetSymVar();
                            generator.AddCommand(Section.text, Command.pop, $"{NasmType.dword} [{Register.ebp} + {4 + varParam.offset}]");
                        }
                        if (var.GetSymVar().GetType() == typeof(SymVarLocal))
                        {
                            SymVarLocal varLocal = (SymVarLocal)var.GetSymVar();
                            generator.AddCommand(Section.text, Command.pop, $"{NasmType.dword} [{Register.ebp} - {varLocal.offset}]");
                        }
                    }
                    if (left.GetType() == typeof(NodeRecordAccess))
                    {
                        NodeRecordAccess nodeRec = (NodeRecordAccess)left;
                        NodeVar leftVar = (NodeVar)nodeRec.left;
                        NodeVar rightVar = (NodeVar)nodeRec.right;
                        generator.AddCommand(Section.text, Command.pop, $"{NasmType.dword} [{Generator.Mangle(leftVar.GetName())} + rec_{leftVar.GetSymVar().GetTypeVar().GetName()}.{Generator.Mangle(rightVar.GetName())}]");
                    }
                    if (left.GetType() == typeof(NodeArrayPosition))
                    {
                        NodeArrayPosition nodeArrPos = (NodeArrayPosition)left;
                        generator.AddCommand(Section.text, Command.mov, Register.ecx, 0);
                        for (int i = 0; i < nodeArrPos.args.Count; i++)
                        {
                            NodeExpression arg = nodeArrPos.args.ElementAt(i);
                            arg.Generate(generator);
                            generator.AddCommand(Section.text, Command.pop, Register.eax);
                            generator.AddCommand(Section.text, Command.mov, Register.ebx, $"{Generator.Mangle(nodeArrPos.GetName())}_from{i}");
                            generator.AddCommand(Section.text, Command.sub, Register.eax, Register.ebx);
                            if (i == nodeArrPos.args.Count - 1)
                            {
                                generator.AddCommand(Section.text, Command.add, Register.ecx, Register.eax);
                            }
                            else
                            {
                                generator.AddCommand(Section.text, Command.mov, Register.ebx, $"{Generator.Mangle(nodeArrPos.GetName())}_size{i}");
                                generator.AddCommand(Section.text, Command.imul, Register.ebx);
                                generator.AddCommand(Section.text, Command.add, Register.ecx, Register.eax);
                            }
                        }
                        generator.AddCommand(Section.text, Command.pop, $"{NasmType.dword} [{Generator.Mangle(nodeArrPos.GetName())} + 4 * {Register.ecx}]");
                    }
                    break;
            }
        }
    }
    public partial class CallStmt
    {
        public override void Generate(Generator generator)
        {
            switch (proc.GetName())
            {
                case "write":
                    int size = 0;
                    string format = "";
                    for (int i = args.Count - 1; i > -1; i--)
                    {
                        NodeExpression? el = args[i];
                        if (el != null)
                        {
                            if (el.GetCachedType().GetType() == typeof(SymReal))
                            {
                                format = "%g" + format;
                                el.Generate(generator);
                                generator.AddCommand(Section.text, Command.pop, Register.eax);
                                generator.AddCommand(Section.text, Command.mov, $"[real]", $"{Register.eax}");
                                generator.AddCommand(Section.text, Command.fld, $"{NasmType.dword} [real]");
                                generator.AddCommand(Section.text, Command.sub, Register.esp, 8);
                                generator.AddCommand(Section.text, Command.fstp, $"{NasmType.qword} [{Register.esp}]");
                                size += 8;
                            }
                            if (el.GetCachedType().GetType() == typeof(SymInteger))
                            {
                                format = "%d" + format;
                                el.Generate(generator);
                                size += 4;
                            }
                            if (el.GetCachedType().GetType() == typeof(SymString))
                            {
                                format = "%s" + format;
                                el.Generate(generator);
                                size += 4;
                            }
                        }
                    }
                    generator.numberConst += 1;
                    generator.AddLine(Section.data, $"format_{generator.numberConst}: {NasmType.db} \'{format}\',0");
                    generator.AddCommand(Section.text, Command.push, $"format_{generator.numberConst}");
                    generator.AddCommand(Section.text, Command.call, Call._printf);
                    generator.AddCommand(Section.text, Command.add, Register.esp, size);
                    generator.AddCommand(Section.text, Command.mov, Register.eax, 0);
                    break;
                case "read":
                    break;
                default:
                    for (int i = args.Count - 1; i > -1; i--)
                    {
                        NodeExpression? el = args[i];
                        if (el != null)
                        {
                            el.Generate(generator);
                        }
                    }
                    generator.AddCommand(Section.text, Command.call, Generator.Mangle(proc.GetName()));
                    break;
            }
        }
    }
    public partial class IfStmt
    {
        public override void Generate(Generator generator)
        {
            condition.Generate(generator);
            generator.AddCommand(Section.text, Command.pop, Register.eax);
            generator.AddCommand(Section.text, Command.cmp, Register.eax, $"{NasmType.@byte} 1");
            int key = generator.key;
            generator.AddCommand(Section.text, Command.je, $"Then_{key}");
            elseBody.Generate(generator);
            generator.AddCommand(Section.text, Command.jmp, $"Else_{key}");
            generator.AddLine(Section.text, $"Then_{key}:");
            body.Generate(generator);
            generator.AddLine(Section.text, $"Else_{key}:");
        }
    }
    public partial class WhileStmt
    {
        public override void Generate(Generator generator)
        {
            int key = generator.key;
            generator.AddCommand(Section.text, Command.jmp, $"Check_{key}");
            generator.AddLine(Section.text, $"Do_{key}:");
            body.Generate(generator);
            generator.AddLine(Section.text, $"Check_{key}:");
            condition.Generate(generator);
            generator.AddCommand(Section.text, Command.pop, Register.eax);
            generator.AddCommand(Section.text, Command.cmp, Register.eax, $"{NasmType.@byte} 1");
            generator.AddCommand(Section.text, Command.je, $"Do_{key}");
        }
    }
    public partial class ForStmt
    {
        public override void Generate(Generator generator)
        {
            int key = generator.key;
            initialVal.Generate(generator);
            generator.AddCommand(Section.text, Command.pop, Register.eax);
            generator.AddCommand(Section.text, Command.mov, $"[{Generator.Mangle(controlVar.GetName())}]", Register.eax);
            finalVal.Generate(generator);
            generator.AddCommand(Section.text, Command.pop, Register.eax);
            generator.AddLine(Section.data, $"finalVal{key}: {NasmType.dd}  0");

            generator.AddCommand(Section.text, Command.mov, $"[finalVal{key}]", Register.eax);

            generator.AddLine(Section.text, $"Do_{key}:");
            body.Generate(generator);
            generator.AddCommand(Section.text, Command.inc, $"{NasmType.dword} [{Generator.Mangle(controlVar.GetName())}]");

            generator.AddCommand(Section.text, Command.mov, Register.eax, $"{NasmType.dword} [{Generator.Mangle(controlVar.GetName())}]");
            generator.AddCommand(Section.text, Command.cmp, Register.eax, $"{NasmType.dword} [finalVal{key}]");
            generator.AddCommand(Section.text, Command.jle, $"Do_{key}");
        }
    }
    public partial class RepeatStmt
    {
        public override void Generate(Generator generator)
        {
            int key = generator.key;
            generator.AddLine(Section.text, $"Do_{key}:");
            foreach(NodeStatement node in body)
            {
                node.Generate(generator);
            }
            condition.Generate(generator);
            generator.AddCommand(Section.text, Command.pop, Register.eax);
            generator.AddCommand(Section.text, Command.cmp, Register.eax, $"{NasmType.@byte} 1");
            generator.AddCommand(Section.text, Command.je, $"Do_{key}");
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
