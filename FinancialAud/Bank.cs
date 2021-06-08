using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Bank.Extentions;
using Bank.Logs;

namespace Bank
{
    public class Bank
    {
        private decimal _bankPrimeRate;
        public decimal BankPrimeRate
        {
            get => _bankPrimeRate;
            set => SetPrimeRate(value);
        }
        public ulong NumberOfTransactions { get; private set; }

        public Dictionary<int, BankCustomer> CustomerRepository { get; }

        public Bank()
        {
            _bankPrimeRate = 0;
            NumberOfTransactions = 0;
            CustomerRepository = new Dictionary<int, BankCustomer>();
        }

        public Bank(decimal bankPrimeRate) : this()
        {
            _bankPrimeRate = bankPrimeRate;
        }
        

        /// <summary>
        ///     Reads the inputted fileName, processes the text file, and does the stuff in it.
        /// </summary>
        /// <param name="inputFileName"> File to search for</param>
        /// <returns> Number of transactions contained in the file</returns>
        public ulong ProcessTransactionLogs(string inputFileName)
        {
            NumberOfTransactions = 0;
            //Interested about other types of files like .xml for storing a bank after processing
            var transactionLog = new TransactionLog(inputFileName);
            if (transactionLog.OpenInputFile())
                while (transactionLog.HasTransactions)
                {
                    var currentTransaction = transactionLog.GetNextTransaction();
                    switch (currentTransaction.action)
                    {
                        case 'A':
                            AddCustomer(currentTransaction.customerName,
                                (uint) currentTransaction.customerNumber);
                            break;

                        case 'C':
                        case 'S':
                            CreateAccount(currentTransaction.customerNumber,
                                currentTransaction.action,
                                Convert.ToDecimal(currentTransaction.amountOrRate));
                            break;

                        case 'N':
                            ChangeCustomerName(currentTransaction.customerNumber,
                                currentTransaction.customerName);
                            break;

                        case 'P':
                            SetPrimeRate(Convert.ToDecimal(currentTransaction.amountOrRate));
                            break;

                        case 'L':
                            LinkAccounts(currentTransaction.customerNumber,
                                currentTransaction.recipientCustomerNumber,
                                !currentTransaction.recipientPrimary);
                            break;

                        case 'D':
                            Deposit(currentTransaction.customerNumber,
                                !currentTransaction.primaryAccount,
                                Convert.ToDecimal(currentTransaction.amountOrRate));
                            break;

                        case 'W':
                            Withdraw(currentTransaction.customerNumber,
                                !currentTransaction.primaryAccount,
                                Convert.ToDecimal(currentTransaction.amountOrRate));
                            break;

                        case 'K':
                            ProcessCheck(currentTransaction.customerNumber,
                                !currentTransaction.primaryAccount,
                                currentTransaction.checkNumber,
                                currentTransaction.customerName,
                                Convert.ToDecimal(currentTransaction.amountOrRate),
                                currentTransaction.recipientCustomerNumber,
                                !currentTransaction.recipientPrimary);
                            break;

                        case 'X':
                            Transfer(currentTransaction.customerNumber, // Recipient = to
                                !currentTransaction.primaryAccount,
                                Convert.ToDecimal(currentTransaction.amountOrRate),
                                currentTransaction.recipientCustomerNumber, // Originating ID = from
                                !currentTransaction.recipientPrimary);
                            break;
                        case 'E':
                            SwapAccounts(currentTransaction.customerNumber);
                            break;
                        case 'M':
                            MonthEndProcessing();
                            break;
                        case 'R':
                            ChangeAnnualPercentageRate(currentTransaction.customerNumber,
                                !currentTransaction.primaryAccount,
                                Convert.ToDecimal(currentTransaction.amountOrRate));
                            break;
                        case 'Y':
                            DeleteAccount(currentTransaction.customerNumber,
                                !currentTransaction.primaryAccount);
                            break;
                        default:
                            --NumberOfTransactions;
                            break; // Add to Error list if I end up making that
                    }

                    ++NumberOfTransactions;
                }

            transactionLog.CloseFile(MyFileType.InputFile);
            return NumberOfTransactions;
        }

        private void AddCustomer(string userName, uint userNumber)
        {
            if (!CustomerRepository.ContainsKey((int) userNumber))
                CustomerRepository.Add((int) userNumber, new BankCustomer(userNumber, userName));
        }

        private void CreateAccount(int userNumber, char accountType, decimal accountInterest)
        {
            if (CustomerRepository.ContainsKey(userNumber))
                CustomerRepository[userNumber].AddAccount(accountType, accountInterest);
        }

        private void ChangeCustomerName(int userNumber, string newUserName)
        {
            if (CustomerRepository.ContainsKey(userNumber)) CustomerRepository[userNumber].UserName = newUserName;
        }

        private void LinkAccounts(int userNumber, int originUserNumber, bool originPrimary)
        {
            if (!CustomerRepository.ContainsKey(userNumber) || !CustomerRepository.ContainsKey(originUserNumber) ||
                userNumber == originUserNumber) return;

            if (originPrimary)
            {
                //Linking an Account adds the account to the recipients accounts
                CustomerRepository[userNumber].AddAccount(CustomerRepository[originUserNumber].PrimaryAccount);
                return;
            }

            CustomerRepository[userNumber].AddAccount(CustomerRepository[originUserNumber].SecondaryAccount);
        }

        private void Deposit(int userNumber, bool primaryAccount, decimal depositAmount)
        {
            if (!CustomerRepository.ContainsKey(userNumber)) return;
            if (primaryAccount)
            {
                CustomerRepository[userNumber].DepositTo(0, depositAmount);
                return;
            }

            CustomerRepository[userNumber].DepositTo(1, depositAmount);
        }

        private void Withdraw(int userNumber, bool primaryAccount, decimal withdrawAmount)
        {
            if (!CustomerRepository.ContainsKey(userNumber)) return;

            if (primaryAccount)
            {
                CustomerRepository[userNumber].WithdrawFrom(0, withdrawAmount);
                return;
            }

            CustomerRepository[userNumber].WithdrawFrom(1, withdrawAmount);
        }

        private void Transfer(int userNumber, bool primaryAccount, decimal transferAmount, int senderNumber,
            bool senderPrimary)
        {
            if (!CustomerRepository.ContainsKey(userNumber) || !CustomerRepository.ContainsKey(senderNumber)) return;
            if (primaryAccount)
            {
                CustomerRepository[userNumber].TransferBetween(0, transferAmount,
                    senderPrimary
                        ? CustomerRepository[senderNumber].PrimaryAccount
                        : CustomerRepository[senderNumber].SecondaryAccount);
                return;
            }

            CustomerRepository[userNumber].TransferBetween(1, transferAmount,
                senderPrimary
                    ? CustomerRepository[senderNumber].PrimaryAccount
                    : CustomerRepository[senderNumber].SecondaryAccount);
        }

        private void SwapAccounts(int userNumber)
        {
            if (CustomerRepository.ContainsKey(userNumber))
                CustomerRepository[userNumber].SwapAccounts();
        }

        private void MonthEndProcessing()
        {
            foreach (var customer in CustomerRepository) customer.Value.PostAccounts(_bankPrimeRate);
            foreach (var customer in CustomerRepository) customer.Value.EndInterestPosting();
        }

        public void DeleteAccount(int userNumber, bool primaryAccount)
        {
            if (!CustomerRepository.ContainsKey(userNumber)) return;
            CustomerRepository[userNumber].DeleteAccount(primaryAccount ? (ushort) 0 : (ushort) 1);
        }

        private void ChangeAnnualPercentageRate(int userNumber, bool primaryAccount, decimal newAnnualPercentageRate)
        {
            if (userNumber >= CustomerRepository.Count) return;
            if (userNumber == 0)
            {
                _bankPrimeRate = newAnnualPercentageRate;
                return;
            }

            if (!CustomerRepository.ContainsKey(userNumber)) return;
            if (primaryAccount)
            {
                CustomerRepository[userNumber].PrimaryAccountAnnualPercentageRate = newAnnualPercentageRate;
                return;
            }

            CustomerRepository[userNumber].SecondaryAccountAnnualPercentageRate = newAnnualPercentageRate;
        }

        private void SetPrimeRate(decimal newAnnualPercentageRate)
        {
            if (newAnnualPercentageRate > 0) _bankPrimeRate = newAnnualPercentageRate;
        }

        private void ProcessCheck(int userNumber, bool primaryAccount, int checkNumber, string toName,
            decimal checkAmount, int recipientCustomerNumber, bool recipientPrimary)
        {
            if (!CustomerRepository.ContainsKey(userNumber)) return;
            switch (recipientCustomerNumber)
            {
                case > 0 when CustomerRepository.ContainsKey(userNumber):
                {
                    if (primaryAccount)
                    {
                        if (recipientPrimary)
                            CustomerRepository[userNumber].ProcessCheck(checkNumber, 0, toName, checkAmount,
                                CustomerRepository[recipientCustomerNumber].PrimaryAccount,
                                CustomerRepository[recipientCustomerNumber].SecondaryAccount);
                        else
                            CustomerRepository[userNumber].ProcessCheck(checkNumber, 0, toName, checkAmount,
                                CustomerRepository[recipientCustomerNumber].PrimaryAccount);
                    }
                    else
                    {
                        if (recipientPrimary)
                            CustomerRepository[userNumber].ProcessCheck(checkNumber, 1, toName, checkAmount,
                                CustomerRepository[recipientCustomerNumber].PrimaryAccount,
                                CustomerRepository[recipientCustomerNumber].SecondaryAccount);
                        else
                            CustomerRepository[userNumber].ProcessCheck(checkNumber, 1, toName, checkAmount,
                                CustomerRepository[recipientCustomerNumber].PrimaryAccount);
                    }

                    break;
                }
                case 0: // This is a global Check, the recipient is not in the system
                    CustomerRepository[userNumber].ProcessCheck(checkNumber, primaryAccount ? (ushort) 0 : (ushort) 1,
                        toName,
                        checkAmount,
                        CustomerRepository[recipientCustomerNumber].PrimaryAccount);
                    break;
            }
        }

        public string PrintFinalState()
        {
            //var vertical = '║';
            //var hotizontal = '═';
            //var leftCorner = '╔';
            //var rightCorner = '╗';
            //var tBottom = '╤';
            //var xConnector = '╪';
            //var barSeparator =
            //"╔════╦══════════════════╦═══╦════╦══════════╦═════╦═══════════╦════╦════╦══════════╦═════╦══════════╗";
            var output = new StringBuilder();
            var sorted =
                new SortedDictionary<int, BankCustomer>(
                    CustomerRepository); // Sort the dictionary for output based on Id (Key)

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
                var accType = customer.GetAccType(0);
                var type = accType switch
                {
                    AccountType.Checking => "C",
                    AccountType.Savings => "S",
                    _ => "--"
                };

                output.AppendFormat("{0,-4}│", accType switch
                {
                    AccountType.Checking => "C",
                    AccountType.Savings => "S",
                    _ => "--"
                });
                    //customer.PrimaryAccount != null ? customer.GetAccType(0) : "--");
                output.AppendFormat("{0,-13}│",
                    customer.PrimaryAccount != null
                        ? PriceString(customer.PrimaryAccountBalance)
                        : "--");
                output.AppendFormat("{0, -5}│",
                    customer.PrimaryAccount != null
                        ? customer.PrimaryAccountAnnualPercentageRate + "%"
                        : "--");
                output.AppendFormat("{0,-11}", GetLinkedString(userIdNumber, 0));
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
                        ? PriceString(customer.SecondaryAccountBalance)
                        : "--");
                output.AppendFormat("{0, -5}│",
                    customer.SecondaryAccount != null
                        ? customer.SecondaryAccountAnnualPercentageRate + "%"
                        : "--");
                output.AppendFormat("{0,-11}", GetLinkedString(userIdNumber, 1));
                output.AppendLine("║▒");
            }

            //Bottom Section
            var numOfTransactions = "║Number of Transactions: " + NumberOfTransactions;
            var lengthOfNumber = numOfTransactions.Length;

            var totalAssets = $"║Total Assets: {TotalTender():C}";
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

        public decimal TotalTender()
        {
            decimal totalAssets = 0;
            foreach (var (_, customer) in CustomerRepository)
            {
                // Does not count non master linked accounts to avoid overcount
                if (customer.PrimaryAccount != null && customer.AccountLinked(0) == 0)
                    totalAssets += customer.PrimaryAccountBalance;
                if (customer.SecondaryAccount != null && customer.AccountLinked(1) == 0)
                    totalAssets += customer.SecondaryAccountBalance;
            }

            return totalAssets;
        }

        public string GetLinkedString(int userNumber, int accountToTest)
        {
            if (!CustomerRepository.ContainsKey(userNumber)) return string.Empty;
            var customer = CustomerRepository[userNumber];
            var output = new StringBuilder();

            CreateLinkedString(output, customer, accountToTest, userNumber);

            return output.ToString();
        }


        private void CreateLinkedString(StringBuilder builder, BankCustomer customer, int primary, int userNumber)
        {
            switch (primary)
            {
                case 0:
                    GetPrimaryLinkedString(builder, customer, userNumber);
                    break;
                case 1:
                    GetSecondaryLinkedString(builder, customer, userNumber);
                    break;
                default: break;
            }
        }

        private void GetSecondaryLinkedString(StringBuilder builder, BankCustomer customer, int userNumber)
        {
            if (customer.SecondaryAccount == null)
            {
                builder.Append("--");
                return;
            }

            var notUnique = false;
            foreach (var (userId, currentCustomer) in CustomerRepository)
            {
                if (userId == userNumber) continue; // Linked accs should not be from the users acc so can skip
                notUnique = currentCustomer.NotUniqueAccount(customer.SecondaryAccount);
                if (notUnique) break;
            }

            if (notUnique)
            {
                var master = customer.AccountLinked(1);
                if (master == uint.MaxValue) return;
                builder.AppendFormat("{0}", master == 0 ? "Yes: Master" : $"Yes-({master}) ");
                if (master == 0) return;
                if (!CustomerRepository.ContainsKey((int) master))
                {
                    var s = customer.SecondaryAccount == CustomerRepository[(int) master].PrimaryAccount;
                    builder.AppendFormat("{0}", s ? "P" : "S");
                    return;
                }

                builder.AppendFormat("{0}", "E");
            }
            else
            {
                builder.Append("No");
            }
        }

        private void GetPrimaryLinkedString(StringBuilder builder, BankCustomer customer, int userNumber)
        {
            if (CustomerRepository[userNumber].PrimaryAccount == null)
            {
                builder.Append("--");
                return;
            }

            var notUnique = false;
            foreach (var (userId, currentCustomer) in CustomerRepository)
            {
                if (userId == userNumber) continue; // Linked accs should not be from the users acc so can skip
                notUnique = currentCustomer.NotUniqueAccount(CustomerRepository[userNumber].PrimaryAccount);
                if (notUnique) break;
            }

            if (notUnique)
            {
                var master = CustomerRepository[userNumber].AccountLinked(0);
                if (master == uint.MaxValue) return;
                builder.AppendFormat("{0}", master == 0 ? "Yes: Master" : $"Yes-({master}) ");

                if (master == 0) return;
                var s = CustomerRepository[userNumber].PrimaryAccount == CustomerRepository[(int) master].PrimaryAccount;
                builder.AppendFormat("{0}", s ? "P" : "S");
            }
            else
            {
                builder.Append("No");
            }
        }


        private string AddTransactionIdToDetail(ulong transactionID)
        {
            return ($"{transactionID}|".PadLeft(5, '|'));
        }


        public static string PriceString(decimal amount)
        {
            if (amount <= 999999999) return $"{amount:C}";

            var placeValue = amount / 1000000000;

            switch (placeValue)
            {
                case < 1000:
                    return $"${amount / 1000000000:0.#######}B";
                case < 1000000:
                    return $"${amount / 1000000000000:0.#######}T";
            }

            return $"{amount:C}";
        }
    }
}