using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler;

namespace Tester
{
    static class Menu
    {
        public static void Main()
        {
            Lexer.CreateTableDFA();
            Console.WriteLine($"тесты");
            Console.WriteLine($"Ключи:\n1 - Лексический анализ\n2 - Анализ простейшего выражения\n3 - Синтаксический анализ");
            string? input = Console.ReadLine();
        }
    }
}
