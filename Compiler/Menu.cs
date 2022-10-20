using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    static class Menu
    {
        public static void Main()
        {
            Console.WriteLine($"Введите 1 для запуска автоматических тестов \nИли нажмите Enter для ввода имени файла и кода");
            string? input = Console.ReadLine();
            if (input == "1")
            {
                Tester.StartTest();
            }
            if (input == "")
            {
                Console.WriteLine($"Введите имя файла и ключ (Лексический анализ - 1)");
                string? fileNameAndKey = Console.ReadLine();
                string?[] words = fileNameAndKey.Split(' ');
                Compiler.CompileFile();
            }
        }
    }
}
