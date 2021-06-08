using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using Bank;
using Bank.Extentions;
using PrettyConsoleHelper;

namespace BankConsole
{
    public class Menus
    {
        public string CreateMainMenuNoArg()
        {
            var output = new StringBuilder();

            return output.ToString();
        }

        public static string PrintFinalState(Bank.Bank bank)
        {
            {
                var output = new StringBuilder();
                var sorted = new SortedDictionary<int, BankCustomer>(bank.CustomerRepository); // Sort the dictionary for output or maybe use linq

                //Title Segment
                output.AppendFormat("{0,-50}", "Banking Interface V 2.2 (C#)");
                output.AppendFormat("{0,50}", "By: J");
                output.AppendLine("\n");
                output.AppendFormat("{0,55}\n", "Final State");
                output.AppendLine(
                    "╔════╤══════════════════╤════╤═════════════╤═════╤════════════╤═════╤═════════════╤═════╤═══════════╗");

                //TODO make able for resizing of the table without having to change everything 
                //Header Segment - MUST ADD TO 99 - + Edges = 102 chars
                output.Append('║');
                output.AppendFormat("{0,-4}│", "ID"); //5
                output.AppendFormat("{0,-18}│", "Name"); //19
                output.AppendFormat("{0,-4}│", "Type"); //5
                output.AppendFormat("{0,-13}│", "Balance"); //14
                output.AppendFormat("{0,-5}│", "APR"); //6
                output.AppendFormat("{0,-11}", "Linked"); //11
                output.AppendFormat("{0,3}", " │ "); //3
                output.AppendFormat("{0,-4}│", "Type"); //5
                output.AppendFormat("{0,-13}│", "Balance"); //14
                output.AppendFormat("{0,-5}│", "APR"); //6
                output.AppendFormat("{0,-11}", "Linked"); //11
                output.AppendLine("║▒");
                output.AppendLine(
                    "╠════╪══════════════════╪════╪═════════════╪═════╪════════════╪═════╪═════════════╪═════╪═══════════╣▒");

                // Body Section -  same sizes as above 
                foreach (var (userIdNumber, customer) in sorted)
                {
                    output.Append('║');
                    output.AppendFormat("{0,-4}│", userIdNumber);
                    output.AppendFormat("{0,-18}│", customer.UserName.Truncate(18));

                    //output.AppendFormat("{0,-3}│", _customersList[i].PrimaryAccount != null ? _customersList[i].GetAccountPrimaryString(0) : "-");
                    var accType = customer.GetAccType(0);
                
                    output.AppendFormat("{0,-4}│", accType switch
                    {
                        AccountType.Checking => "C",
                        AccountType.Savings => "S",
                        _ => "--"
                    });
                    //customer.PrimaryAccount != null ? customer.GetAccType(0) : "--");
                    output.AppendFormat("{0,-13}│",
                        customer.PrimaryAccount != null
                            ? Bank.Bank.PriceString(customer.PrimaryAccountBalance)
                            : "--");
                    output.AppendFormat("{0, -5}│",
                        customer.PrimaryAccount != null
                            ? customer.PrimaryAccountAnnualPercentageRate + "%"
                            : "--");
                    output.AppendFormat("{0,-11}", bank.GetLinkedString(userIdNumber, 0));
                    output.AppendFormat("{0,3}", userIdNumber == 1 ? " ╽ " : " ┃ ");
                    //output.AppendFormat("{0,-3}│", _customersList[i].SecondaryAccount != null ? _customersList[i].GetAccountPrimaryString(1) : "-");
                    accType = customer.GetAccType(1);
                    output.AppendFormat("{0,-4}│", accType switch
                    {
                        AccountType.Checking => "C",
                        AccountType.Savings => "S",
                        _ => "--"
                    });
                    output.AppendFormat("{0,-13:}│",
                        customer.SecondaryAccount != null
                            ? Bank.Bank.PriceString(customer.SecondaryAccountBalance)
                            : "--");
                    output.AppendFormat("{0, -5}│",
                        customer.SecondaryAccount != null
                            ? customer.SecondaryAccountAnnualPercentageRate + "%"
                            : "--");
                    output.AppendFormat("{0,-11}", bank.GetLinkedString(userIdNumber, 1));
                    output.AppendLine("║▒");
                }

                //Bottom Section
                var numOfTransactions = "║Number of Transactions: " + bank.NumberOfTransactions;
                var lengthOfNumber = numOfTransactions.Length;

                var totalAssets = $"║Total Assets: {bank.TotalTender():C}";
                var lengthOfAssets = totalAssets.Length;

                var locationOfT = lengthOfAssets > lengthOfNumber ? lengthOfAssets : lengthOfNumber;
                var bottomMidLine =
                    "╟────┴──────────────────┴────┴─────────────┴─────┴────────────┸─────┴─────────────┴─────┴───────────╢▒";
                var charAtReplace = bottomMidLine[locationOfT];

                // make sure connection is correct
                output.AppendLine(
                    bottomMidLine.Remove(locationOfT, 1).Insert(locationOfT, charAtReplace == '┴' ? "┼" : "┬"));

                output.AppendFormat("{0}", numOfTransactions + "│".PadLeft(locationOfT - lengthOfNumber + 1));
                output.Append(new string('▓', 99 - locationOfT));
                output.AppendLine("║▒");
                output.AppendFormat("{0}", totalAssets + "│".PadLeft(locationOfT - lengthOfAssets + 1));
                output.Append(new string('▓', 99 - locationOfT));
                output.AppendLine("║▒");

                output.AppendFormat("{0}\n",
                    "╚══════════════════════════════════════════════════════════════════════════════════════════════════╝▒"
                        .Insert(locationOfT, "╧"));
                output.AppendLine(new string('▒', 102));


                return output.ToString();
            }
            
        }

        public static void PrintPrettyFinal(Bank.Bank bank)
        {
            var headers = new[] {"ID", "Name", "Type", "Balance", "APR", "Linked", "Type", "Balance", "APR", "Linked"};
            var table = new PrettyTable("|", ConsoleColor.Gray, headers);
            var customerList = bank.CustomerRepository.Select(x => x.Value)
                                                                                .OrderBy(x => x.UserIdNumber).ToList();
            var rows = new List<object>();
            foreach (var customer in customerList)
            {
                rows.Add(customer.UserIdNumber);
                rows.Add(customer.UserName);
                rows.Add(customer.GetAccType(0));
                rows.Add(customer.PrimaryAccountBalance);
                rows.Add(customer.PrimaryAccountAnnualPercentageRate);
                rows.Add(bank.GetLinkedString((int) customer.UserIdNumber, 0));
                rows.Add(customer.GetAccType(1));
                rows.Add(customer.SecondaryAccountBalance);
                rows.Add(customer.SecondaryAccountAnnualPercentageRate);
                rows.Add(bank.GetLinkedString((int) customer.UserIdNumber, 1));
                table.AddRow(rows.ToArray());
                rows.Clear();
            }
            
            table.Write();
        }
    }
}