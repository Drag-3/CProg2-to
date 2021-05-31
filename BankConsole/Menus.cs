using System.Collections.Generic;
using System.Text;
using System.Xml;
using Bank;
using Bank.Extentions;

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
                var sorted = new SortedDictionary<int, BankCustomer>(bank.CustomerRepository); // Sort the dictionary for output

                //Title Segment
                output.AppendFormat("{0,-50}", "Banking Interface V 2.2 (C#)");
                output.AppendFormat("{0,50}", "By: Justin Erysthee");
                output.AppendLine("\n");
                output.AppendFormat("{0,55}\n", "Final State");
                output.AppendLine(
                    "╔════╤══════════════════╤════╤═════════════╤═════╤════════════╤═════╤═════════════╤═════╤═══════════╗");

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
                    output.AppendFormat("{0,-4}│",
                        customer.PrimaryAccount != null ? customer.GetAccType(0) : "--");
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
                    output.AppendFormat("{0,-4}│",
                        customer.SecondaryAccount != null ? customer.GetAccType(1) : "--");
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
    }
}