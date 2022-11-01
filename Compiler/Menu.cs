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
                Console.WriteLine($"Введите имя файла (c расширением, файл должен храниться в папке tests)");
                string? fileName = Console.ReadLine();
                Console.WriteLine($"Введите ключ (ключ Лексического анализа - 1)");
                string? key = Console.ReadLine();
                if (key == "1")
                {
                    string path = $"../../../tests/{fileName}";
                    if (File.Exists(path))
                    {
                        Compiler.CompileFile(path,"console");
                    }
                    else
                    {
                        Console.WriteLine($"Такого файла не сущестует");
                    }
                }
                else
                {
                    Console.WriteLine($"Такого ключа не существует");
                }
                Console.WriteLine("\nЧтобы завершить программу нажмите Enter");
                Console.ReadLine();
            }
        }
    }
}
