using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public enum Command
    {
        @extern,
        global,
        section,
        equ,
        mov,
        push,
        pop,
        add,
        sub,
        imul,
        idiv,
        fadd,
        fsub,
        fmul,
        fdiv,
        call,
        fld,
        fild,
        fstp,
        cmp,
        jmp,
        je,
        jne,
        jle,
        jge,
        jg,
        jl,
        inc,
        ret,
        proc,
        endp,
        struc,
        endstruc,
        istruc,
        iend,
        at,
        resd,
        neg,
        fneg
    }
    public enum Register
    {
        eax,
        ebx,
        ecx,
        edx,
        esp,
        ebp
    }
    public enum Call
    {
        _printf,
        _scanf
    }
    public enum NasmType
    {
        @byte,
        word,
        dword,
        qword,
        db,
        dd,
        dq
    }
    public enum Section
    {
        file,
        data,
        bss,
        text
    }
    public class Generator
    {
        public int key = 0;
        public int numberConst = 0;
        List<object> file = new List<object>();
        List<object> data = new List<object>();
        List<object> bss = new List<object>();
        List<object> text = new List<object>();
        public Generator() { }
        public static string Mangle(string var)
        {
            return "_" + var;
        }
        public void WriteInFile(string pathOut)
        {
            using (StreamWriter sw = new StreamWriter(pathOut, false, Encoding.Default))
            {
                foreach (object el in file)
                {
                    sw.Write(el + "\r\n");
                }
                sw.Write($"\r\n {Command.section} .{Section.bss} \r\n");
                foreach (object el in bss)
                {
                    sw.Write("\t" + el + "\r\n");
                }
                sw.Write($"\r\n {Command.section} .{Section.data} \r\n");
                foreach (object el in data)
                {
                    sw.Write("\t" + el + "\r\n");
                }
                sw.Write($"\r\n {Command.section} .{Section.text} \r\n");
                foreach (object el in text)
                {
                    sw.Write("\t" + el + "\r\n");
                }
            }
        }
        public void AddLine(Section section, string linecommand)
        {
            switch (section)
            {
                case Section.file:
                    file.Add(linecommand);
                    break;
                case Section.data:
                    data.Add(linecommand);
                    break;
                case Section.bss:
                    bss.Add(linecommand);
                    break;
                case Section.text:
                    text.Add(linecommand);
                    break;
            }
            key += 1;
        }
        class CommandLine
        {
            public Command cmd;
            public object[] arguments;
            public CommandLine(Command cmd, params object[] arguments)
            {
                this.cmd = cmd;
                this.arguments = arguments;
            }
            public override string ToString()
            {
                string linecommand = "";
                linecommand += cmd + " ";
                for (int i = 0; i < arguments.Length - 1; i++)
                {
                    linecommand += arguments[i] + ", ";
                }
                if (arguments.Length > 0)
                {
                    linecommand += arguments[^1];
                }
                return linecommand;
            }
        }
        public void AddCommand(Section section, Command cmd, params object[] arguments)
        {
            CommandLine cmdline = new CommandLine(cmd, arguments);
            switch (section)
            {
                case Section.file:
                    file.Add(cmdline);
                    break;
                case Section.data:
                    data.Add(cmdline);
                    break;
                case Section.bss:
                    bss.Add(cmdline);
                    break;
                case Section.text:
                    text.Add(cmdline);
                    break;
            }
            key += 1;
        }

        public void PeepholeOptimization(int rounds)
        {
            for (int r = 0; r < rounds; r ++)
            {
                for (int i = 1; i < text.Count; i++)
                {
                    object line = text[i];
                    object beforeLine = text[i - 1];
                    if (line.GetType() == typeof(CommandLine) && beforeLine.GetType() == typeof(CommandLine))
                    {
                        CommandLine cmdBeforeLine = (CommandLine)beforeLine;
                        CommandLine cmdLine = (CommandLine)line;
                        if(cmdBeforeLine.cmd == Command.push && cmdLine.cmd == Command.pop)
                        {
                            if(cmdBeforeLine.arguments.First().GetType() == typeof(Register) && cmdLine.arguments.First().GetType() == typeof(Register))
                            {
                                if ((Register)cmdBeforeLine.arguments.First() == (Register)cmdLine.arguments.First())
                                {
                                    Console.WriteLine($"desription {cmdBeforeLine.arguments.First()} {cmdLine.arguments.First()}");
                                    text.RemoveAt(i);
                                    text.RemoveAt(i - 1);
                                    i -= 2;
                                    continue;
                                }
                            }
                            CommandLine newCmdline = new CommandLine(Command.mov, new object[] { cmdLine.arguments.First(), cmdBeforeLine.arguments.First() });
                            Console.WriteLine(newCmdline);
                            text[i] = newCmdline;
                            text.RemoveAt(i - 1);
                            i -= 1;
                        }
                    }
                }
            }
        }
    }
}
