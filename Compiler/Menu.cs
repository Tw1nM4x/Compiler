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
            Lexer.CreateTableDFA();
            Console.WriteLine($"Введите ключ для запуска автоматических тестов связанных с проверяемым режимом работы\nИли нажмите Enter для ручного ввода имени файла и ключа\n");
            Console.WriteLine($"Ключи:\n1 - Лексический анализ\n2 - Анализ простейшего выражения\n3 - Синтаксический анализ");
            string? input = Console.ReadLine();
            if (input == "1" || input == "2" || input == "3")
            {
                Tester.StartTest(input);
            }
            if (input == "")
            {
                Console.WriteLine($"Введите имя файла (файл в формате .txt должен храниться в папке tests)");
                string? fileName = Console.ReadLine();
                string path = $"../../../tests/{fileName}.txt";
                if (!File.Exists(path))
                {
                    Console.WriteLine($"Такого файла не сущестует");
                    return;
                }
                Console.WriteLine($"Введите ключ");
                string? key = Console.ReadLine();
                switch (key)
                {
                    case "1":
                        SkillCompiler.OutputLexemeParsing(path, "console");
                        break;
                    case "2":
                        SkillCompiler.OutputSimpleExpressionsParsing(path, "console");
                        break;
                    case "3":
                        SkillCompiler.OutputSyntaxParsing(path, "console");
                        break;
                    default:
                        Console.WriteLine($"Такого ключа не существует");
                        break;
                }
                Console.WriteLine("\nЧтобы завершить программу нажмите Enter");
                Console.ReadLine();
            }
        }
    }
}
