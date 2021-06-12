using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace FinancialAudit
{
    static class Program
    {
        
        static void Main(string[] args)
        {
            SetupConsole();

            //var yu = new Logger("Yes");
            //yu.Log(Logger.LogPriority.Debug, "First Test");
            //var qw = "Using var";
            //yu.Log(Logger.LogPriority.Info, qw);
            
            var test = new Bank.Bank();
            //Console.WriteLine(test.PriceString(12345678904324));

            if (args.Length > 0 && args[0] != null)
            {
                test.ProcessTransactionLogs(args[0]);
                Console.WriteLine(test.PrintFinalState());
            }
            else
            {
                Console.WriteLine("╔" + new string('═', 100) + "╗");
                Console.Write("║{0,-50}", "Banking Interface V 2.1 (C#)");
                Console.WriteLine("{0,50}║", "By: Justin Erysthee");
                Console.WriteLine("╚" + new string('═', 100) + "╝");
                Console.WriteLine();
                Console.WriteLine("Enter log to Process");
                var fileName = Console.ReadLine();
                Console.WriteLine($"Processing file - {fileName}");
                Thread.Sleep(1000);
                Console.Clear();
                
                test.ProcessTransactionLogs(fileName);
                Console.WriteLine(test.PrintFinalState());
            }
            Console.WriteLine();
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
            
        }

        static void SetupConsole()
        {
            // Got from here -> https://stackoverflow.com/questions/38533903/set-c-sharp-console-application-to-unicode-output
            
            /*
            var note = new Process
            {
                StartInfo =
                {
                    FileName = "C:\\Program Files\\Mozilla Firefox\\firefox.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = false,
                    UseShellExecute = false
                }
            };

            var webTest = new ProcessStartInfo // 0..o
            {
                FileName = "www.google.com",
                UseShellExecute = true
            };
            
            note.Start();
            Process.Start(webTest);
            note.StandardInput.WriteLine("Hehehehehehehheheheheh");
            note.StandardInput.Flush();
            //note.StandardInput.Close();
            */
            
            // Required 
            var cmd = new Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };
            cmd.Start();
            cmd.StandardInput.WriteLine("chcp 65001");
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();

            Console.OutputEncoding = Encoding.UTF8;
        }

    };
}