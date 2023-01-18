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
        mov,
        push,
        pop,
        add,
        sub,
        mul,
        div,
        call,
        fld,
        fstp,
        cmp,
        jmp,
        je,
        jg,
        jl
    }
    public enum Register
    {
        eax,
        ebx,
        ecx,
        edx,
        esp
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
    public class Generator
    {
        string pathOut;
        public bool _mainDef = false;
        public int line = 0;
        public string Mangle(string var)
        {
            return "var_" + var;
        }
        public void AddCommand(string linecommand)
        {
            using (StreamWriter sw = new StreamWriter(pathOut, true, Encoding.Default))
            {
                sw.Write(linecommand + "\r\n");
                line += 1;
            }
        }
        public void Add(Command cmd, params object[] arguments)
        {
            using (StreamWriter sw = new StreamWriter(pathOut, true, Encoding.Default))
            {
                sw.Write(cmd + " ");
                for(int i = 0; i < arguments.Length - 1; i++)
                {
                    sw.Write(arguments[i] + ", ");
                }
                if(arguments.Length > 0)
                {
                    sw.Write(arguments[^1]);
                }
                sw.Write("\r\n");
                line += 1;
            }
        }
        public Generator(string pathOut)
        {
            this.pathOut = pathOut;
        }
    }
}
