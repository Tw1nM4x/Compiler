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
                Console.WriteLine($"Введите имя файла (c расширением, файл должен храниться в папке tests) и ключ (ключ Лексического анализа - 1)");
                string? fileNameAndKey = Console.ReadLine();
                string?[] words = fileNameAndKey.Split(' ');
                if (words[1] == "1" && words.Length > 1)
                {
                    string path = $"../../../tests/{words[0]}";
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
            }
        }
    }
}
