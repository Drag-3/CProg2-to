using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace BankConsole
{
    static class Program
    {
        static void Main(string[] args)
        {
            SetupConsole();

            var test = new Bank.Bank();

            if (args.Length > 0 && args[0] != null)
            {
                test.ProcessTransactionLogs(args[0]);
                Console.WriteLine(Menus.PrintFinalState(test));
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
                Menus.PrintPrettyFinal(test);
                Console.WriteLine(Menus.PrintFinalState(test));
            }

            Console.WriteLine();
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);

        }

        static void SetupConsole()
        {
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