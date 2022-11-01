using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Reflection.PortableExecutable;

namespace Compiler
{
    class Tester
    {
        struct Folder{
            public string name;
            public int countTest;
            public Folder(string name = "string", int countTest = 0)
            {
                this.name = name;
                this.countTest = countTest;
            }
        }
        static Folder[] folders = new Folder[] { new("string", 14), new("indifier", 9), new("integer", 19), new("real", 28), new("space", 2), 
            new("comment", 9), new("key word", 4), new("end file", 4), new("operation sign", 3), new("separator", 3), new("errors", 3), new("combination", 2) };

        static void StartDetailTest()
        {
            for (int numberFolder = 0; numberFolder < folders.Length; numberFolder++)
            {
                Console.WriteLine($"----------{folders[numberFolder].name}----------");
                for (int numberTest = 1; numberTest <= folders[numberFolder].countTest; numberTest++)
                {
                    Console.WriteLine($"{numberTest})\n");
                    string numberTestStr = numberTest.ToString();
                    if (numberTestStr.Length < 2)
                    {
                        numberTestStr = "0" + numberTestStr;
                    }
                    string pathIn = $"../../../tests/{folders[numberFolder].name}/" + $"{numberTestStr}_input.txt";
                    using (StreamReader sr = new StreamReader(pathIn, Encoding.Default))
                    {
                        Console.WriteLine("In:\n" + sr.ReadToEnd() + "\n");
                    }
                    Console.WriteLine("Out:");
                    Compiler.CompileFile(pathIn, "console");
                    Console.WriteLine("\nЧтобы завершить введите 0\nЧтобы перейти к следующему тесту нажмите Enter");
                    string? input = Console.ReadLine();
                    if(input == "0")
                    {
                        return;
                    }
                }
            }
        }
        public static void StartTest()
        {
            int countOK = 0;
            int countERROR = 0;
            for (int numberFolder = 0; numberFolder < folders.Length; numberFolder++)
            {
                Console.WriteLine($"----------{folders[numberFolder].name}----------");
                for (int numberTest = 1; numberTest <= folders[numberFolder].countTest; numberTest++)
                {
                    string numberTestStr = numberTest.ToString();
                    if (numberTestStr.Length < 2)
                    {
                        numberTestStr = "0" + numberTestStr;
                    }
                    string pathIn = $"../../../tests/{folders[numberFolder].name}/" + $"{numberTestStr}_input.txt";
                    string pathOut = $"../../../tests/{folders[numberFolder].name}/" + $"{numberTestStr}_out.txt";
                    string pathCheck = $"../../../tests/{folders[numberFolder].name}/" + $"{numberTestStr}_correct.txt";
                    Compiler.CompileFile(pathIn, pathOut);
                    string? checkFile;
                    string? outFile;
                    using (StreamReader sr = new StreamReader(pathCheck, Encoding.Default))
                    {
                        checkFile = sr.ReadToEnd();
                    }
                    using (StreamReader sr = new StreamReader(pathOut, Encoding.Default))
                    {
                        outFile = sr.ReadToEnd();
                    }
                    if (checkFile == outFile)
                    {
                        Console.WriteLine($"{numberTest}-OK");
                        countOK += 1;
                    }
                    else
                    {
                        Console.WriteLine($"{numberTest}-ERROR");
                        countERROR += 1;
                    }
                }
            }
            Console.WriteLine($"OK: {countOK}  ERRORS: {countERROR}");
            Console.WriteLine("-------------------------------");
            Console.WriteLine("Чтобы запустить проверку тестов с подробным результатом нажмите 1 \nЧтобы завершить программу нажмите Enter");
            string? input = Console.ReadLine();
            if(input == "1")
            {
                StartDetailTest();
            }
        }
    }
}