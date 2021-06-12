using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using FinancialAudit.IO;

namespace TimerTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new TextStream(File.Open(Environment.CurrentDirectory + "\\random", FileMode.Create));
            var watch = new Stopwatch();
            var random = new Random();
            
            watch.Start();
            for (var i = 0; i < 3200000; i++)
            {
                for (var j = 0; j < 20; ++j)
                {
                    test.WriteString(new string((char) random.Next(33, 126), 1));
                }
                test.WriteString("\n");
            }
            watch.Stop();
            Console.WriteLine($"Time Elasped FileCreation: {watch.ElapsedMilliseconds} miliseconds");
            Console.WriteLine($"Time Elasped FileCreation: {watch.Elapsed}");
            
            
              //Time Elasped FileCreation: 213909 miliseconds
              //Time Elasped FileCreation: 00:03:33.9095772
              
            watch.Reset();
            test.Dispose();
            
            var test2 = new TextStream(File.Open(Environment.CurrentDirectory + "\\random", FileMode.Open));
            //test3.Open();
            
            watch.Start();
            test2.ReadAllLines();
            watch.Stop();
            
            Console.WriteLine($"Time Elasped Read All Lines 256: {watch.ElapsedMilliseconds} miliseconds");
            Console.WriteLine($"Time Elasped Read All Lines 256: {watch.Elapsed}");
            test2.Dispose();
            watch.Reset();
            
            var lol = new StringBuilder();
            var test3 = new TextStream(File.Open(Environment.CurrentDirectory + "\\random", FileMode.Open));
            //test3.Open();

            Console.WriteLine();
            
            watch.Start();
            for (var i = 0; i < 3200000; ++i)
            { 
                test3.ReadLine();
            }
            watch.Stop();
            Console.WriteLine($"Time Elasped ReadLine: {watch.ElapsedMilliseconds} miliseconds");
            Console.WriteLine($"Time Elasped ReadLines: {watch.Elapsed}");
            test3.Dispose();
            
        }
    }
    
 }