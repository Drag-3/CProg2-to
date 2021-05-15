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
        private readonly List<BankCustomer> _customersList;
        private readonly StringBuilder _details;
        private StringBuilder _errors;
        private ulong _numberOfTransactions;
        private short _primeRateMultiplier;

        public Bank()
        {
            _bankPrimeRate = 0;
            _primeRateMultiplier = 100;
            _numberOfTransactions = 0;
            _customersList = new List<BankCustomer>();
            _details = new StringBuilder();
            _errors = new StringBuilder();
            _customersList.Add(new BankCustomer(0, "Placeholder"));
        }

        public Bank(decimal bankPrimeRate)
        {
            _bankPrimeRate = bankPrimeRate;
            _primeRateMultiplier = 100;
            _numberOfTransactions = 0;
            _customersList = new List<BankCustomer>();
            _details = new StringBuilder();
            _errors = new StringBuilder();
            _customersList.Add(new BankCustomer(0, "Placeholder"));
        }

        public Bank(decimal bankPrimeRate, short primeRateMultiplier)
        {
            _bankPrimeRate = bankPrimeRate;
            _primeRateMultiplier = primeRateMultiplier;
            _numberOfTransactions = 0;
            _customersList = new List<BankCustomer>();
            _details = new StringBuilder();
            _errors = new StringBuilder();
            _customersList.Add(new BankCustomer(0, "Placeholder"));
        }

        /// <summary>
        ///     Reads the inputted fileName, processes the text file, and does the stuff in it.
        /// </summary>
        /// <param name="inputFileName"> File to search for</param>
        /// <returns> Number of transactions contained in the file</returns>
        public ulong ProcessTransactionLogs(string inputFileName)
        {
            _numberOfTransactions = 0;
            //Interested about other types of files
            var detailedFileName = inputFileName + "-details.log";
            var transactionLog = new TransactionLog(inputFileName, detailedFileName);
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
            _customersList.Add(new BankCustomer(userNumber, userName));
        }

        private void CreateAccount(int userNumber, char accountType, decimal accountInterest, ulong transactionNumber)
        {
            if (userNumber < _customersList.Count) _customersList[userNumber].AddAccount(accountType, accountInterest);
        }

        private void ChangeCustomerName(int userNumber, string newUserName, ulong transactionNumber)
        {
            if (userNumber < _customersList.Count) _customersList[userNumber].UserName = newUserName;
        }

        private void LinkAccounts(int userNumber, int originUserNumber, bool originPrimary, ulong transactionNumber)
        {
            if (userNumber >= _customersList.Count || originUserNumber >= _customersList.Count) return;
            if (originPrimary)
            {
                _customersList[userNumber].AddAccount(_customersList[originUserNumber].PrimaryAccount);
                return;
            }

            _customersList[userNumber].AddAccount(_customersList[originUserNumber].SecondaryAccount);
        }

        private void Deposit(int userNumber, bool primaryAccount, decimal depositAmount, ulong transactionNumber)
        {
            if (userNumber < _customersList.Count)
            {
                if (primaryAccount)
                {
                    _customersList[userNumber].DepositTo(0, depositAmount);
                    return;
                }

                _customersList[userNumber].DepositTo(1, depositAmount);
            }
        }

        private void Withdraw(int userNumber, bool primaryAccount, decimal withdrawAmount, ulong transactionNumber)
        {
            if (userNumber >= _customersList.Count) return;
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
            if (userNumber >= _customersList.Count || senderNumber >= _customersList.Count) return;
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
            if (userNumber >= _customersList.Count) return;
            _customersList[userNumber].SwapAccounts();
        }

        private void MonthEndProcessing(ulong transactionNumber)
        {
            for (var i = 1; i < _customersList.Count; ++i) _customersList[i].PostAccounts(_bankPrimeRate);
            foreach (var customer in _customersList) customer.EndInterestPosting();
        }

        public void DeleteAccount(int userNumber, bool primaryAccount)
        {
            if (userNumber >= _customersList.Count) return;
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
            if (userNumber >= _customersList.Count) return;
            if (recipientCustomerNumber > 0 && recipientCustomerNumber < _customersList.Count)
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
            }
            else if (recipientCustomerNumber == 0)
            {
                _customersList[userNumber].ProcessCheck(checkNumber, primaryAccount ? (ushort) 0 : (ushort) 1, toName,
                    checkAmount,
                    _customersList[recipientCustomerNumber].PrimaryAccount);
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

            //Title Segment
            output.AppendFormat("{0,-50}", "Banking Interface V 2.1 (C#)");
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
            for (var i = 1; i < _customersList.Count; ++i) // Rn max balance is ~ 9bill before the box is destroyed
            {
                output.Append('║');
                output.AppendFormat("{0,-4}│", i);
                output.AppendFormat("{0,-18}│", _customersList[i].UserName.Truncate(18));
                //output.AppendFormat("{0,-3}│", _customersList[i].PrimaryAccount != null ? _customersList[i].GetAccountPrimaryString(0) : "-");
                output.AppendFormat("{0,-4}│",
                    _customersList[i].PrimaryAccount != null ? _customersList[i].GetAccType(0) : "--");
                output.AppendFormat("{0,-13}│",
                    _customersList[i].PrimaryAccount != null
                        ? PriceString(_customersList[i].PrimaryAccountBalance)
                        : "--");
                output.AppendFormat("{0, -5}│",
                    _customersList[i].PrimaryAccount != null
                        ? _customersList[i].PrimaryAccountAnnualPercentageRate + "%"
                        : "--");
                output.AppendFormat("{0,-11}", GetLinkedString(i, 0));
                output.AppendFormat("{0,3}", i == 1 ? " ╽ " : " ┃ ");
                //output.AppendFormat("{0,-3}│", _customersList[i].SecondaryAccount != null ? _customersList[i].GetAccountPrimaryString(1) : "-");
                output.AppendFormat("{0,-4}│",
                    _customersList[i].SecondaryAccount != null ? _customersList[i].GetAccType(1) : "--");
                output.AppendFormat("{0,-13:}│",
                    _customersList[i].SecondaryAccount != null
                        ? PriceString(_customersList[i].SecondaryAccountBalance)
                        : "--");
                output.AppendFormat("{0, -5}│",
                    _customersList[i].SecondaryAccount != null
                        ? _customersList[i].SecondaryAccountAnnualPercentageRate + "%"
                        : "--");
                output.AppendFormat("{0,-11}", GetLinkedString(i, 1));
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
            for (var i = 1; i < _customersList.Count; i++)
            {
                if (_customersList[i].PrimaryAccount != null && _customersList[i].AccountLinked(0) == 0)
                    totalAssets += _customersList[i].PrimaryAccountBalance;
                if (_customersList[i].SecondaryAccount != null && _customersList[i].AccountLinked(1) == 0)
                    totalAssets += _customersList[i].SecondaryAccountBalance;
            }

            return totalAssets;
        }

        private string GetLinkedString(int userNumber, int accountToTest)
        {
            if (_customersList.Count <= userNumber) return string.Empty;
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
            for (var index = 1; index < _customersList.Count && !notUnique; index++)
            {
                if (index == userNumber) continue; // Linked accs should not be from the users acc so can skip
                var customer = _customersList[index];
                notUnique = customer.NotUniqueAccount(_customersList[userNumber].SecondaryAccount);
                //if (notUnique) break;
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
            for (var index = 1; index < _customersList.Count && !notUnique; index++)
            {
                if (index == userNumber) continue;
                var customer = _customersList[index];
                notUnique = customer.NotUniqueAccount(_customersList[userNumber].PrimaryAccount);
                //if (notUnique) break;
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

        
        private void AddTransactionIDToDetail(ulong transactionID)
        {
            _details.Append($"{transactionID}|".PadLeft(5, '|'));
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