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
        static Folder[] folders = new Folder[] { };

        public static void StartTest(string key)
        {
            switch (key)
            {
                case "1":
                    folders = new Folder[] { new("string", 14), new("indifier", 9), new("integer", 19), new("real", 28), new("space", 2), new("comment", 9), new("key word", 4), new("end file", 4), new("operation sign", 3), new("separator", 3), new("errors", 3), new("combination", 2) };
                    break;
                case "2":
                    folders = new Folder[] { new("simple expressions", 14) };
                    break;
            }
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
                    switch (key)
                    {
                        case "1":
                            SkillCompiler.OutputLexemeParsing(pathIn, pathOut);
                            break;
                        case "2":
                            SkillCompiler.OutputSimpleExpressionsParsing(pathIn, pathOut);
                            break;
                    }
                    byte[] checkFile;
                    byte[] outFile;
                    using (FileStream fstream = File.OpenRead(pathCheck))
                    {
                        checkFile = new byte[fstream.Length];
                        fstream.Read(checkFile, 0, checkFile.Length);
                    }
                    using (FileStream fstream = File.OpenRead(pathOut))
                    {
                        outFile = new byte[fstream.Length];
                        fstream.Read(outFile, 0, outFile.Length);
                    }
                    bool twin = true;
                    if(outFile.Length != checkFile.Length)
                    {
                        twin = false;
                    }
                    else
                    {
                        for(int i = 0; i < outFile.Length; i++)
                        {
                            if (outFile[i] != checkFile[i])
                            {
                                twin = false;
                            }
                        }
                    }
                    if (twin)
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
            Console.WriteLine("Чтобы запустить проверку тестов с подробным результатом введите 1 \nЧтобы завершить программу нажмите Enter");
            string? input = Console.ReadLine();
            if(input == "1")
            {
                StartDetailTest(key);
            }
        }
        static void StartDetailTest(string key)
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
                    switch (key)
                    {
                        case "1":
                            SkillCompiler.OutputLexemeParsing(pathIn);
                            break;
                        case "2":
                            SkillCompiler.OutputSimpleExpressionsParsing(pathIn);
                            break;
                    }
                    Console.WriteLine("\nЧтобы завершить введите 0\nЧтобы перейти к следующему тесту нажмите Enter");
                    string? input = Console.ReadLine();
                    if(input == "0")
                    {
                        return;
                    }
                }
            }
        }
    }
}