using System;
using System.Collections.Generic;
using System.Text;
using FinancialAudit.Extentions;
using FinancialAudit.Logs;

namespace FinancialAudit.Bank
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

        /// <summary>
        /// Contains the customers keyed to the customer's Id
        /// </summary>
        public Dictionary<int, Customer> CustomerRepository { get; }

        public Bank()
        {
            _bankPrimeRate = 0;
            NumberOfTransactions = 0;
            CustomerRepository = new Dictionary<int, Customer>();
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
                    switch (currentTransaction.Action)
                    {
                        case 'A':
                            AddCustomer(currentTransaction.CustomerName,
                                currentTransaction.CustomerNumber);
                            break;

                        case 'C': // Create Checking
                        case 'S': // Create Savings
                            CreateAccount(currentTransaction.CustomerNumber,
                                currentTransaction.Action,
                                Convert.ToDecimal(currentTransaction.AmountOrRate));
                            break;

                        case 'N':
                            ChangeCustomerName(currentTransaction.CustomerNumber,
                                currentTransaction.CustomerName);
                            break;

                        case 'P':
                            SetPrimeRate(Convert.ToDecimal(currentTransaction.AmountOrRate));
                            break;

                        case 'L':
                            LinkAccounts(currentTransaction.CustomerNumber,
                                currentTransaction.RecipientCustomerNumber,
                                !currentTransaction.RecipientPrimary);
                            break;

                        case 'D':
                            Deposit(currentTransaction.CustomerNumber,
                                !currentTransaction.PrimaryAccount,
                                Convert.ToDecimal(currentTransaction.AmountOrRate));
                            break;

                        case 'W':
                            Withdraw(currentTransaction.CustomerNumber,
                                !currentTransaction.PrimaryAccount,
                                Convert.ToDecimal(currentTransaction.AmountOrRate));
                            break;

                        case 'K':
                            ProcessCheck(currentTransaction.CustomerNumber,
                                !currentTransaction.PrimaryAccount,
                                currentTransaction.CheckNumber,
                                currentTransaction.CustomerName,
                                Convert.ToDecimal(currentTransaction.AmountOrRate),
                                currentTransaction.RecipientCustomerNumber,
                                !currentTransaction.RecipientPrimary);
                            break;

                        case 'X':
                            Transfer(currentTransaction.CustomerNumber, // Recipient = to
                                !currentTransaction.PrimaryAccount,
                                Convert.ToDecimal(currentTransaction.AmountOrRate),
                                currentTransaction.RecipientCustomerNumber, // Originating ID = from
                                !currentTransaction.RecipientPrimary);
                            break;
                        case 'E':
                            SwapAccounts(currentTransaction.CustomerNumber);
                            break;
                        case 'M':
                            MonthEndProcessing();
                            break;
                        case 'R':
                            ChangeAnnualPercentageRate(currentTransaction.CustomerNumber,
                                !currentTransaction.PrimaryAccount,
                                Convert.ToDecimal(currentTransaction.AmountOrRate));
                            break;
                        case 'Y':
                            DeleteAccount(currentTransaction.CustomerNumber,
                                !currentTransaction.PrimaryAccount);
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

        public void AddCustomer(string userName, int userNumber)
        {
            if (!CustomerRepository.ContainsKey(userNumber))
                CustomerRepository.Add(userNumber, new Customer(userNumber, userName));
        }

        public void CreateAccount(int userNumber, char accountType, decimal accountInterest)
        {
            if (CustomerRepository.ContainsKey(userNumber))
                CustomerRepository[userNumber].AddAccount(accountType, accountInterest);
        }

        private void ChangeCustomerName(int userNumber, string newUserName)
        {
            if (CustomerRepository.ContainsKey(userNumber)) 
                CustomerRepository[userNumber].UserName = newUserName;
        }

        private void LinkAccounts(int userNumber, int originUserNumber, bool originPrimary)
        {
            if (!CustomerRepository.ContainsKey(userNumber) || !CustomerRepository.ContainsKey(originUserNumber) ||
                userNumber == originUserNumber)
                return;

            if (originPrimary)
            {
                //Linking an Account adds the account to the recipients accounts
                CustomerRepository[userNumber].AddAccount(CustomerRepository[originUserNumber].PrimaryAccount);
                return;
            }

            CustomerRepository[userNumber].AddAccount(CustomerRepository[originUserNumber].SecondaryAccount);
        }

        public void Deposit(int userNumber, bool primaryAccount, decimal depositAmount)
        {
            if (!CustomerRepository.ContainsKey(userNumber))
                return;
            
            if (primaryAccount)
            {
                CustomerRepository[userNumber].DepositTo(AccountLoc.PrimaryAccount, depositAmount);
                return;
            }

            CustomerRepository[userNumber].DepositTo(AccountLoc.SecondaryAccount, depositAmount);
        }

        private void Withdraw(int userNumber, bool primaryAccount, decimal withdrawAmount)
        {
            if (!CustomerRepository.ContainsKey(userNumber))
                return;
            

            if (primaryAccount)
            {
                CustomerRepository[userNumber].WithdrawFrom(AccountLoc.PrimaryAccount, withdrawAmount);
                return;
            }

            CustomerRepository[userNumber].WithdrawFrom(AccountLoc.SecondaryAccount, withdrawAmount);
        }

        private void Transfer(int userNumber, bool primaryAccount, decimal transferAmount, int senderNumber,
            bool senderPrimary)
        {
            if (!CustomerRepository.ContainsKey(userNumber) || !CustomerRepository.ContainsKey(senderNumber))
                return;
            
            if (primaryAccount)
            {
                CustomerRepository[userNumber].TransferBetween(AccountLoc.PrimaryAccount, transferAmount,
                    senderPrimary
                        ? CustomerRepository[senderNumber].PrimaryAccount
                        : CustomerRepository[senderNumber].SecondaryAccount);
                return;
            }

            CustomerRepository[userNumber].TransferBetween(AccountLoc.SecondaryAccount, transferAmount,
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
            foreach (var (_ ,customer) in CustomerRepository)
                customer.PostAccounts(_bankPrimeRate);
            
            foreach (var (_ ,customer) in CustomerRepository)
                customer.EndInterestPosting();
        }

        private void DeleteAccount(int userNumber, bool primaryAccount)
        {
            if (!CustomerRepository.ContainsKey(userNumber))
                return;
            
            CustomerRepository[userNumber].DeleteAccount(primaryAccount ? AccountLoc.PrimaryAccount : AccountLoc.SecondaryAccount);
        }

        private void ChangeAnnualPercentageRate(int userNumber, bool primaryAccount, decimal newAnnualPercentageRate)
        {
            if (!CustomerRepository.ContainsKey(userNumber))
                return;
            
            if (userNumber == 0) // Change prime rate
            {
                _bankPrimeRate = newAnnualPercentageRate;
                return;
            }

            if (!CustomerRepository.ContainsKey(userNumber))
                return;
            
            //Change the APR of the user's account
            if (primaryAccount)
            {
                CustomerRepository[userNumber].PrimaryAccountAnnualPercentageRate = newAnnualPercentageRate;
                return;
            }

            CustomerRepository[userNumber].SecondaryAccountAnnualPercentageRate = newAnnualPercentageRate;
        }

        private void SetPrimeRate(decimal newAnnualPercentageRate)
        {
            if (newAnnualPercentageRate > 0)
                _bankPrimeRate = newAnnualPercentageRate;
        }

        public void ProcessCheck(int userNumber, bool primaryAccount, int checkNumber, string toName,
            decimal checkAmount, int recipientCustomerNumber, bool recipientPrimary)
        {
            if (!CustomerRepository.ContainsKey(userNumber))
                return;
            
            switch (recipientCustomerNumber)
            {
                case > 0 when CustomerRepository.ContainsKey(userNumber): // When the recipient # is 0 it is a Global check (Recipient not in bank)
                {
                    //if (primaryAccount)
                    //{
                        if (recipientPrimary)
                            CustomerRepository[userNumber].ProcessCheck(checkNumber,
                                accountCheckFrom: primaryAccount ? AccountLoc.PrimaryAccount : AccountLoc.SecondaryAccount,
                                toName,
                                checkAmount,
                                CustomerRepository[recipientCustomerNumber].PrimaryAccount,
                                CustomerRepository[recipientCustomerNumber].SecondaryAccount);
                        else
                            CustomerRepository[userNumber].ProcessCheck(checkNumber,
                                primaryAccount ? AccountLoc.PrimaryAccount : AccountLoc.SecondaryAccount,
                                toName,
                                checkAmount,
                                CustomerRepository[recipientCustomerNumber].SecondaryAccount);
                    //}
                    //else
                    //{
                    /*
                        if (recipientPrimary)
                            CustomerRepository[userNumber].ProcessCheck(checkNumber,
                                AccountLoc.SecondaryAccount,
                                toName,
                                checkAmount,
                                CustomerRepository[recipientCustomerNumber].PrimaryAccount,
                                CustomerRepository[recipientCustomerNumber].SecondaryAccount);
                        else
                            CustomerRepository[userNumber].ProcessCheck(checkNumber,
                                AccountLoc.SecondaryAccount,
                                toName,
                                checkAmount,
                                CustomerRepository[recipientCustomerNumber].PrimaryAccount);*/
                    //}

                    break;
                }
                case 0: // This is a global Check, the recipient is not in the system
                    CustomerRepository[userNumber].ProcessCheck(checkNumber,
                                                                primaryAccount ? AccountLoc.PrimaryAccount : AccountLoc.SecondaryAccount,
                                                                toName,
                                                                checkAmount,
                                                                CustomerRepository[recipientCustomerNumber].PrimaryAccount);
                    break;
            }
        }

        public string PrintFinalState()
        {
            //var vertical = '║';
            //var horizontal = '═';
            //var leftCorner = '╔';
            //var rightCorner = '╗';
            //var tBottom = '╤';
            //var xConnector = '╪';
            //var barSeparator =
            //"╔════╦══════════════════╦═══╦════╦══════════╦═════╦═══════════╦════╦════╦══════════╦═════╦══════════╗";
            var output = new StringBuilder();
            var sorted =
                new SortedDictionary<int, Customer>(
                    CustomerRepository); // Sort the dictionary for output based on Id (Key)

            //Title Segment
            output.AppendFormat("{0,-50}", "Banking Interface V 2.2 (C#)");
            output.AppendFormat("{0,50}", "By: J");
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

            // Body Section -  same sizes as above | 10 rows in table
            foreach (var (userIdNumber, customer) in sorted)
            {
                output.Append('║');
                output.AppendFormat("{0,-4}│", userIdNumber); // user Id | 1
                output.AppendFormat("{0,-18}│", customer.UserName.Truncate(18)); // Gets User Name and Truncates if necessary | 2
                
                var accType = customer.GetAccType(AccountLoc.PrimaryAccount); // Get Type from first account in the List | 3
                output.AppendFormat("{0,-4}│", accType switch
                {
                    AccountType.Checking => "C",
                    AccountType.Savings => "S",
                    _ => "--"
                });
                    
                output.AppendFormat("{0,-13}│", // Get The balance of the primary Account | 4
                    customer.PrimaryAccount != null
                        ? PriceString(customer.PrimaryAccountBalance)
                        : "--");
                
                output.AppendFormat("{0, -5}│",  // Get the APR of the primary Account | 5
                    customer.PrimaryAccount != null
                        ? customer.PrimaryAccountAnnualPercentageRate + "%"
                        : "--");
                
                output.AppendFormat("{0,-11}", GetLinkedString(userIdNumber, (int) AccountLoc.PrimaryAccount)); // Get the link status | 6
                
                output.AppendFormat("{0,3}", userIdNumber == 1 ? " ╽ " : " ┃ "); // center divider
                
                accType = customer.GetAccType(AccountLoc.SecondaryAccount); // Get Type from second account in the List | 7
                output.AppendFormat("{0,-4}│", accType switch
                {
                    AccountType.Checking => "C",
                    AccountType.Savings => "S",
                    _ => "--"
                });
                output.AppendFormat("{0,-13:}│", // Get Balance of Secondary Account | 8
                    customer.SecondaryAccount != null
                        ? PriceString(customer.SecondaryAccountBalance)
                        : "--");
                
                output.AppendFormat("{0, -5}│",  // Get APR of secondary Account | 9
                    customer.SecondaryAccount != null
                        ? customer.SecondaryAccountAnnualPercentageRate + "%"
                        : "--");
                
                output.AppendFormat("{0,-11}", GetLinkedString(userIdNumber, 1)); // Get link status | 10
                output.AppendLine("║▒");
            }

            //Bottom Section
            var numOfTransactions = "║Number of Transactions: " + NumberOfTransactions;
            var lengthOfNumber = numOfTransactions.Length;

            var totalAssets = $"║Total Assets: {GetTotalTender():C}";
            var lengthOfAssets = totalAssets.Length;

            var locationOfT = lengthOfAssets > lengthOfNumber ? lengthOfAssets : lengthOfNumber; // Finds end of Assets string
            
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
            
            output.AppendLine(new string('▒', 102)); // Bottom Shadow


            return output.ToString();
        }

        /// <summary>
        /// Calculates the total amount of money in the bank
        /// </summary>
        /// <returns>The total tender as a decimal</returns>
        public decimal GetTotalTender()
        {
            decimal totalAssets = 0;
            foreach (var (_, customer) in CustomerRepository)
            {
                // Does not count non master linked accounts to avoid overcounting
                if (customer.PrimaryAccount != null && customer.AccountLinked(AccountLoc.PrimaryAccount) == 0) // is owned by self
                    totalAssets += customer.PrimaryAccountBalance;
                if (customer.SecondaryAccount != null && customer.AccountLinked(AccountLoc.SecondaryAccount) == 0)
                    totalAssets += customer.SecondaryAccountBalance;
            }

            return totalAssets;
        }

        public string GetLinkedString(int userNumber, int accountToTest)
        {
            if (!CustomerRepository.ContainsKey(userNumber))
                return string.Empty;
            
            var customer = CustomerRepository[userNumber];
            var output = new StringBuilder();

            CreateLinkedString(output, customer, accountToTest, userNumber);

            return output.ToString();
        }


        private void CreateLinkedString(StringBuilder builder, Customer customer, int primary, int userNumber)
        {
            switch (primary)
            {
                case 0:
                    GetPrimaryLinkedString(builder, customer, userNumber);
                    break;
                case 1:
                    GetSecondaryLinkedString(builder, customer, userNumber);
                    break;
                //default: break;
            }
        }

        private void GetSecondaryLinkedString(StringBuilder builder, Customer customer, int userNumber)
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
                var master = customer.AccountLinked(AccountLoc.SecondaryAccount);
                if (master == int.MaxValue) return;
                builder.AppendFormat("{0}", master == 0 ? "Yes: Master" : $"Yes-({master}) ");
                if (master == 0) //Account is owned by self
                    return;
                if (!CustomerRepository.ContainsKey(master))
                {
                    var s = customer.SecondaryAccount == CustomerRepository[master].PrimaryAccount;
                    builder.AppendFormat("{0}", s ? "P" : "S");
                    return;
                }

                builder.AppendFormat("{0}", "E"); // An error if the master is not in the database
            }
            else
            {
                builder.Append("No");
            }
        }

        private void GetPrimaryLinkedString(StringBuilder builder, Customer customer, int userNumber)
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
                var master = CustomerRepository[userNumber].AccountLinked(AccountLoc.PrimaryAccount);
                if (master == int.MaxValue) return;
                builder.AppendFormat("{0}", master == 0 ? "Yes: Master" : $"Yes-({master}) ");

                if (master == 0) // Account is owned by self 
                    return;
                var s = CustomerRepository[userNumber].PrimaryAccount == CustomerRepository[master].PrimaryAccount;
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


        /// <summary>
        /// Returns a Decimal as a price string with shorted forms for numbers over 1 Billion 
        /// </summary>
        /// <param name="amount">The decimal amount to use</param>
        /// <returns>Price string</returns>
        public static string PriceString(decimal amount)
        {
            if (amount <= 999_999_999) // Only shortens if 1 billion or higher
                return $"{amount:C}";

            var placeValue = amount / 1_000_000_000;

            switch (placeValue)
            {
                case < 1000:
                    return $"${amount / 1_000_000_000:0.#######}B";
                case < 1000000:
                    return $"${amount / 1_000_000_000_000:0.#######}T";
            }

            return $"{amount:C}";
        }
    }
}