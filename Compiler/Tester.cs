﻿using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Reflection.PortableExecutable;

namespace Compiler
{ 
    class Tester
    {
        public static void StartTest()
        {
            int countOK = 0;
            int countERROR = 0;
            for (int numberTest = 1; numberTest < 3; numberTest++)
            {
                string pathIn = "../../../tests/" + $"00{numberTest}_input.txt";
                string pathOut = "../../../tests/" + $"00{numberTest}_out.txt";
                string pathCheck = "../../../tests/" + $"00{numberTest}_check.txt";
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
                if(checkFile == outFile)
                {
                    Console.WriteLine("OK");
                    countOK += 1;
                }
                else
                {
                    Console.WriteLine("ERROR");
                    countERROR += 1;
                }
            }
            Console.WriteLine($"Result OK: {countOK} ERRORS: {countERROR}");
        }
    }
}