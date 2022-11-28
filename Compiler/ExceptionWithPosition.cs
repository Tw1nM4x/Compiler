using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class ExceptionWithPosition : Exception
    {
        private string _message;
        private int _positionLine;
        private int _positionSymbol;
        public ExceptionWithPosition(int positionLine, int positionSymbol, string message)
        {
            _message = message;
            _positionLine = positionLine;
            _positionSymbol = positionSymbol;
        }
        public override string ToString()
        {
            return $"({_positionLine},{_positionSymbol}) ERROR: {_message}";
        }
    }
}
