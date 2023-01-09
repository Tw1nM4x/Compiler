using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public enum Command
    {
        Add,
        Move
    }
    public class LineCommand
    {
        Command cmd;
        List<object> arguments;
        public LineCommand(Command cmd, List<object> arguments)
        {
            this.cmd = cmd;
            this.arguments = arguments;
        }
    }
    public class Generator
    {
        public void AddCommand(string linecommand)
        {
            //добавить команду
        }
        public void Add(LineCommand command)
        {
            //добавить команду
        }
        public Generator()
        {

        }
    }
}
