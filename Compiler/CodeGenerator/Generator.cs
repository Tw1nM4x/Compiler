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
        mul,
        div,
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
        string pathOut;
        public int line = 0;
        public int numberConst = 0;
        List<string> file = new List<string>();
        List<string> data = new List<string>();
        List<string> bss = new List<string>();
        List<string> text = new List<string>();
        public static string Mangle(string var)
        {
            return "_" + var;
        }
        public void Write()
        {
            using (StreamWriter sw = new StreamWriter(pathOut, true, Encoding.Default))
            {
                foreach (string el in file)
                {
                    sw.Write(el + "\r\n");
                }
                sw.Write($"\r\n {Command.section} .{Section.bss} \r\n");
                foreach (string el in bss)
                {
                    sw.Write("\t" + el + "\r\n");
                }
                sw.Write($"\r\n {Command.section} .{Section.data} \r\n");
                foreach (string el in data)
                {
                    sw.Write("\t" + el + "\r\n");
                }
                sw.Write($"\r\n {Command.section} .{Section.text} \r\n");
                foreach (string el in text)
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
            line += 1;
        }
        public void AddCommand(Section section, Command cmd, params object[] arguments)
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
            line += 1;
        }
        public Generator(string pathOut)
        {
            this.pathOut = pathOut;
        }
    }
}
