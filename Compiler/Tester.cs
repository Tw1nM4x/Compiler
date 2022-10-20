using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Reflection.PortableExecutable;

namespace Compiler
{ 
    class Tester
    {
        static void Main()
        {
            int numberTest = 1;
            string pathIn = "../../../" + $"00{numberTest}_input.txt";
            string pathOut = "../../../" + $"00{numberTest}_out.txt";
            Console.WriteLine($"Нажмите на Enter для запуска теста {numberTest}");
            string? input = Console.ReadLine();
            if(input == null)
            {
                Compiler.Main(pathIn, pathOut);
            }
        }
    }
}