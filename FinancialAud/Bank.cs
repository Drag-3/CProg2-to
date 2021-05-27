using System;
using System.Collections.Generic;
using System.Text;
using Bank.Extentions;
using Bank.Logs;

namespace Bank
{
    public class Bank
    {
        private decimal _bankPrimeRate;
        private readonly Dictionary<int, BankCustomer> _customersList;
        private ulong _numberOfTransactions;
        private short _primeRateMultiplier;

        public Bank()
        {
            _bankPrimeRate = 0;
            _primeRateMultiplier = 100;
            _numberOfTransactions = 0;
            _customersList = new Dictionary<int, BankCustomer>();
        }

        public Bank(decimal bankPrimeRate) : this()
        {
            _bankPrimeRate = bankPrimeRate;
        }

        public Bank(decimal bankPrimeRate, short primeRateMultiplier) : this(bankPrimeRate)
        {
            _primeRateMultiplier = primeRateMultiplier;
        }

        /// <summary>
        ///     Reads the inputted fileName, processes the text file, and does the stuff in it.
        /// </summary>
        /// <param name="inputFileName"> File to search for</param>
        /// <returns> Number of transactions contained in the file</returns>
        public ulong ProcessTransactionLogs(string inputFileName)
        {
            _numberOfTransactions = 0;
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
                                (uint) currentTransaction.customerNumber,
                                currentTransaction.transactionID);
                            break;

                        case 'C':
                        case 'S':
                            CreateAccount(currentTransaction.customerNumber,
                                currentTransaction.action,
                                Convert.ToDecimal(currentTransaction.amountOrRate),
                                currentTransaction.transactionID);
                            break;

                        case 'N':
                            ChangeCustomerName(currentTransaction.customerNumber,
                                currentTransaction.customerName,
                                currentTransaction.transactionID);
                            break;

                        case 'P':
                            SetPrimeRate(Convert.ToDecimal(currentTransaction.amountOrRate),
                                currentTransaction.transactionID);
                            break;

                        case 'L':
                            LinkAccounts(currentTransaction.customerNumber,
                                currentTransaction.recipientCustomerNumber,
                                !currentTransaction.recipientPrimary, // is primary is zero index, always flip
                                currentTransaction.transactionID);
                            break;

                        case 'D':
                            Deposit(currentTransaction.customerNumber,
                                !currentTransaction.primaryAccount,
                                Convert.ToDecimal(currentTransaction.amountOrRate),
                                currentTransaction.transactionID);
                            break;

                        case 'W':
                            Withdraw(currentTransaction.customerNumber,
                                !currentTransaction.primaryAccount,
                                Convert.ToDecimal(currentTransaction.amountOrRate),
                                currentTransaction.transactionID);
                            break;

                        case 'K':
                            ProcessCheck(currentTransaction.customerNumber,
                                !currentTransaction.primaryAccount,
                                currentTransaction.checkNumber,
                                currentTransaction.customerName,
                                Convert.ToDecimal(currentTransaction.amountOrRate),
                                currentTransaction.transactionID,
                                currentTransaction.recipientCustomerNumber,
                                !currentTransaction.recipientPrimary);
                            break;

                        case 'X':
                            Transfer(currentTransaction.customerNumber, // Recipient = to
                                !currentTransaction.primaryAccount,
                                Convert.ToDecimal(currentTransaction.amountOrRate),
                                currentTransaction.recipientCustomerNumber, // Originating ID = from
                                !currentTransaction.recipientPrimary,
                                currentTransaction.transactionID);
                            break;
                        case 'E':
                            SwapAccounts(currentTransaction.customerNumber,
                                currentTransaction.transactionID);
                            break;
                        case 'M':
                            MonthEndProcessing(currentTransaction.transactionID);
                            break;
                        case 'R':
                            ChangeAnnualPercentageRate(currentTransaction.customerNumber,
                                !currentTransaction.primaryAccount,
                                Convert.ToDecimal(currentTransaction.amountOrRate),
                                currentTransaction.transactionID);
                            break;
                        case 'Y':
                            DeleteAccount(currentTransaction.customerNumber,
                                !currentTransaction.primaryAccount);
                            break;
                        default:
                            --_numberOfTransactions;
                            break; // Add to Error list if I end up making that
                    }

                    ++_numberOfTransactions;
                }

            transactionLog.CloseFile(MyFileType.InputFile);
            return _numberOfTransactions;
        }

        private void AddCustomer(string userName, uint userNumber, ulong transactionNumber)
        {
            if (!_customersList.ContainsKey((int) userNumber))
                _customersList.Add((int) userNumber, new BankCustomer(userNumber, userName));
        }

        private void CreateAccount(int userNumber, char accountType, decimal accountInterest, ulong transactionNumber)
        {
            if (_customersList.ContainsKey(userNumber)) _customersList[userNumber].AddAccount(accountType, accountInterest);
        }

        private void ChangeCustomerName(int userNumber, string newUserName, ulong transactionNumber)
        {
            if (_customersList.ContainsKey(userNumber)) _customersList[userNumber].UserName = newUserName;
        }

        private void LinkAccounts(int userNumber, int originUserNumber, bool originPrimary, ulong transactionNumber)
        {
            if (!_customersList.ContainsKey(userNumber) || !_customersList.ContainsKey(originUserNumber) ||
                userNumber == originUserNumber) return;
            
            if (originPrimary)
            {
                //Linking an Account adds the account to the recipients accounts
                _customersList[userNumber].AddAccount(_customersList[originUserNumber].PrimaryAccount);
                return;
            }
            
            _customersList[userNumber].AddAccount(_customersList[originUserNumber].SecondaryAccount);
        }

        private void Deposit(int userNumber, bool primaryAccount, decimal depositAmount, ulong transactionNumber)
        {
            if (!_customersList.ContainsKey(userNumber)) return;
            if (primaryAccount)
            {
                _customersList[userNumber].DepositTo(0, depositAmount);
                return;
            }
                
            _customersList[userNumber].DepositTo(1, depositAmount);
        }

        private void Withdraw(int userNumber, bool primaryAccount, decimal withdrawAmount, ulong transactionNumber)
        {
            if (!_customersList.ContainsKey(userNumber)) return;

            if (primaryAccount)
            {
                _customersList[userNumber].WithdrawFrom(0, withdrawAmount);
                return;
            }

            _customersList[userNumber].WithdrawFrom(1, withdrawAmount);
        }

        private void Transfer(int userNumber, bool primaryAccount, decimal transferAmount, int senderNumber,
            bool senderPrimary,
            ulong transactionNumber)
        {
            if (!_customersList.ContainsKey(userNumber) || !_customersList.ContainsKey(senderNumber)) return;
            if (primaryAccount)
            {
                _customersList[userNumber].TransferBetween(0, transferAmount,
                    senderPrimary
                        ? _customersList[senderNumber].PrimaryAccount
                        : _customersList[senderNumber].SecondaryAccount);
                return;
            }
            
            _customersList[userNumber].TransferBetween(1, transferAmount,
                senderPrimary
                    ? _customersList[senderNumber].PrimaryAccount
                    : _customersList[senderNumber].SecondaryAccount);
        }

        private void SwapAccounts(int userNumber, ulong transactionNumber)
        {
            if (_customersList.ContainsKey(userNumber))
                _customersList[userNumber].SwapAccounts();
        }

        private void MonthEndProcessing(ulong transactionNumber)
        {
            foreach ( var customer in _customersList) customer.Value.PostAccounts(_bankPrimeRate);
            foreach (var customer in _customersList) customer.Value.EndInterestPosting();
        }

        public void DeleteAccount(int userNumber, bool primaryAccount)
        {
            if (!_customersList.ContainsKey(userNumber)) return;
            _customersList[userNumber].DeleteAccount(primaryAccount ? (ushort) 0 : (ushort) 1);
        }

        private void ChangeAnnualPercentageRate(int userNumber, bool primaryAccount, decimal newAnnualPercentageRate,
            ulong transactionNumber)
        {
            if (userNumber >= _customersList.Count) return;
            if (userNumber == 0)
            {
                _bankPrimeRate = newAnnualPercentageRate;
                return;
            }

            if (!_customersList.ContainsKey(userNumber)) return;
            if (primaryAccount)
            {
                
                _customersList[userNumber].PrimaryAccountAnnualPercentageRate = newAnnualPercentageRate;
                return;
            }
            
            _customersList[userNumber].SecondaryAccountAnnualPercentageRate = newAnnualPercentageRate;
        }

        private void SetPrimeRate(decimal newAnnualPercentageRate, ulong transactionNum)
        {
            if (newAnnualPercentageRate > 0) _bankPrimeRate = newAnnualPercentageRate;
        }

        private void ProcessCheck(int userNumber, bool primaryAccount, int checkNumber, string toName,
            decimal checkAmount, ulong transactionNumber, int recipientCustomerNumber, bool recipientPrimary)
        {
            if (!_customersList.ContainsKey(userNumber)) return;
            switch (recipientCustomerNumber)
            {
                case > 0 when _customersList.ContainsKey(userNumber):
                {
                    if (primaryAccount)
                    {
                        if (recipientPrimary)
                            _customersList[userNumber].ProcessCheck(checkNumber, 0, toName, checkAmount,
                                _customersList[recipientCustomerNumber].PrimaryAccount,
                                _customersList[recipientCustomerNumber].SecondaryAccount);
                        else
                            _customersList[userNumber].ProcessCheck(checkNumber, 0, toName, checkAmount,
                                _customersList[recipientCustomerNumber].PrimaryAccount);
                    }
                    else 
                    {
                        if (recipientPrimary)
                            _customersList[userNumber].ProcessCheck(checkNumber, 1, toName, checkAmount,
                                _customersList[recipientCustomerNumber].PrimaryAccount,
                                _customersList[recipientCustomerNumber].SecondaryAccount);
                        else
                            _customersList[userNumber].ProcessCheck(checkNumber, 1, toName, checkAmount,
                                _customersList[recipientCustomerNumber].PrimaryAccount);
                    }

                    break;
                }
                case 0: // This is a global Check, the recipient is not in the system
                    _customersList[userNumber].ProcessCheck(checkNumber, primaryAccount ? (ushort) 0 : (ushort) 1, toName,
                        checkAmount,
                        _customersList[recipientCustomerNumber].PrimaryAccount);
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
            var sorted = new SortedDictionary<int, BankCustomer>(_customersList); // Sort the dictionary for output

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
                        ? PriceString(customer.PrimaryAccountBalance)
                        : "--");
                output.AppendFormat("{0, -5}│",
                    customer.PrimaryAccount != null
                        ? customer.PrimaryAccountAnnualPercentageRate + "%"
                        : "--");
                output.AppendFormat("{0,-11}", GetLinkedString(userIdNumber, 0));
                output.AppendFormat("{0,3}", userIdNumber == 1 ? " ╽ " : " ┃ ");
                //output.AppendFormat("{0,-3}│", _customersList[i].SecondaryAccount != null ? _customersList[i].GetAccountPrimaryString(1) : "-");
                output.AppendFormat("{0,-4}│",
                    customer.SecondaryAccount != null ? customer.GetAccType(1) : "--");
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
            var numOfTransactions = "║Number of Transactions: " + _numberOfTransactions;
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
            foreach (var (_, customer) in _customersList)
            {
                // Does not count non master linked accounts to avoid overcount
                if (customer.PrimaryAccount != null && customer.AccountLinked(0) == 0)
                    totalAssets += customer.PrimaryAccountBalance;
                if (customer.SecondaryAccount != null && customer.AccountLinked(1) == 0)
                    totalAssets += customer.SecondaryAccountBalance;
            }

            return totalAssets;
        }

        private string GetLinkedString(int userNumber, int accountToTest)
        {
            if (!_customersList.ContainsKey(userNumber)) return string.Empty;
            var output = new StringBuilder();

                CreateLinkedString(output, accountToTest, userNumber);

                return output.ToString();
        }


        private void CreateLinkedString(StringBuilder builder, int primary, int userNumber)
        {
            switch (primary)
            {
                case 0: GetPrimaryLinkedString(builder, userNumber);
                    break;
                case 1: GetSecondaryLinkedString(builder, userNumber);
                    break;
                default: break;
            }
        }

        private void GetSecondaryLinkedString(StringBuilder builder, int userNumber)
        {
            if (_customersList[userNumber].SecondaryAccount == null)
            {
                builder.Append("--");
                return;
            }
            var notUnique = false;
            foreach (var (userId, customer) in _customersList)
            {
                if (userId == userNumber) continue; // Linked accs should not be from the users acc so can skip
                notUnique = customer.NotUniqueAccount(_customersList[userNumber].SecondaryAccount);
                if (notUnique) break;
            }

            if (notUnique)
            {
                var master = _customersList[userNumber].AccountLinked(1);
                if (master == uint.MaxValue) return;
                builder.AppendFormat("{0}", master == 0 ? "Yes: Master" : $"Yes-({master}) ");
                if (master == 0) return;
                var s = _customersList[userNumber].SecondaryAccount == _customersList[(int) master].PrimaryAccount;
                builder.AppendFormat("{0}", s ? "P" : "S");
            }
            else
            {
                builder.Append("No");
            }
        }

        private void GetPrimaryLinkedString(StringBuilder builder, int userNumber)
        {
            
            if (_customersList[userNumber].PrimaryAccount == null)
            {
                builder.Append("--");
                return;
            }
            var notUnique = false;
            foreach (var (userId, customer) in _customersList)
            {
                if (userId == userNumber) continue; // Linked accs should not be from the users acc so can skip
                notUnique = customer.NotUniqueAccount(_customersList[userNumber].PrimaryAccount);
                if (notUnique) break;
            }

            if (notUnique)
            {
                var master = _customersList[userNumber].AccountLinked(0);
                if (master == uint.MaxValue) return;
                builder.AppendFormat("{0}", master == 0 ? "Yes: Master" : $"Yes-({master}) ");

                if (master == 0) return;
                var s = _customersList[userNumber].PrimaryAccount == _customersList[(int) master].PrimaryAccount;
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