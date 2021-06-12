using System.Collections.Generic;
using FinancialAudit.Accounts;
using Microsoft.VisualBasic.CompilerServices;

namespace FinancialAudit.Bank
{
    /// <summary>
    /// Represents a customer and their owned accounts. Has methods to interact with owned accounts
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// List of user owned accounts. Max size is 2
        /// </summary>
        private readonly List<Account> _accountList;

        private const short MaxAccounts = 2;
        
        private ushort _numberOfAccounts;
        public int UserIdNumber { get; }
        public string UserName { get; set; }

        public decimal PrimaryAccountBalance =>
            !IsAccountDeleted(AccountLoc.PrimaryAccount) ?
                _accountList[(int) AccountLoc.PrimaryAccount].AccountBalance : 0;


        public decimal SecondaryAccountBalance =>
            !IsAccountDeleted(AccountLoc.SecondaryAccount) ?
                _accountList[(int) AccountLoc.SecondaryAccount].AccountBalance : 0;

        public decimal PrimaryAccountAnnualPercentageRate
        {
            get =>
                !IsAccountDeleted(AccountLoc.PrimaryAccount) ?
                    _accountList[(int) AccountLoc.PrimaryAccount].AnnualPercentageRate : 0;
            set
            {
                if (!IsAccountDeleted(AccountLoc.PrimaryAccount))
                {
                    _accountList[(int) AccountLoc.PrimaryAccount].AnnualPercentageRate = value;
                }
            }
        }

        public decimal SecondaryAccountAnnualPercentageRate
        {
            get =>
                !IsAccountDeleted(AccountLoc.SecondaryAccount) ?
                    _accountList[(int) AccountLoc.SecondaryAccount].AnnualPercentageRate : 0;
            set
            {
                if (!IsAccountDeleted(AccountLoc.PrimaryAccount))
                {
                    _accountList[(int) AccountLoc.SecondaryAccount].AnnualPercentageRate = value;
                }
            }
        }

        public Account PrimaryAccount => !IsAccountDeleted(AccountLoc.PrimaryAccount) ? 
                        _accountList[(int) AccountLoc.PrimaryAccount] : null; // Returns the actual account or null
        

        public Account SecondaryAccount => !IsAccountDeleted(AccountLoc.SecondaryAccount) ? 
            _accountList[(int) AccountLoc.SecondaryAccount] : null; // Returns the actual account or null
        
        public Customer(int userIdNumber, string userName)
        {
            _accountList = new List<Account>();
            _numberOfAccounts = 0;
            UserIdNumber = userIdNumber;
            UserName = userName;
        }

        /// <summary>
        /// Creates Account and add to list
        /// </summary>
        /// <param name="accountType">'C' creates checking account
        ///     <para>'S' creates savings account</para>
        /// </param>
        public void AddAccount(char accountType)
        {
            if (_numberOfAccounts >= MaxAccounts) // Max accounts already reached
                return;
            
            switch (accountType)
            {
                case 'S':
                    _accountList.Add(new SavingsAccount(UserIdNumber));
                    ++_numberOfAccounts;
                    break;
                case 'C':
                    _accountList.Add(new CheckingAccount(UserIdNumber));
                    ++_numberOfAccounts;
                    break;
            }
        }

        /// <summary>
        /// Creates a new Account with the specified interest rate and adds it to the list
        /// </summary>
        /// <param name="accountType">'C' creates checking account
        ///     <para>'S' creates savings account</para>
        /// </param>
        /// <param name="accountInterest">The interest rate of the new account</param>
        public void AddAccount(char accountType, decimal accountInterest)
        {
            if (_numberOfAccounts >= MaxAccounts)
                return;
            
            switch (accountType)
            {
                case 'S':
                    _accountList.Add(new SavingsAccount(UserIdNumber, accountInterest));
                    ++_numberOfAccounts;
                    break;
                case 'C':
                    _accountList.Add(new CheckingAccount(UserIdNumber, accountInterest));
                    ++_numberOfAccounts;
                    break;
            }
        }

        /// <summary>
        /// Adds an already existing account to the customer
        /// </summary>
        /// <param name="accountToLink">The account to add to the list</param>
        public void AddAccount(Account accountToLink) // This is the only case where overload is used
        {
            if (_numberOfAccounts >= MaxAccounts)
                return;
            
            _accountList.Add(accountToLink);
            ++_numberOfAccounts;
        }

        /// <summary>
        /// Deposits the specified amount to the account
        /// </summary>
        /// <param name="accountToDeposit"></param>
        /// <param name="amount"></param>
        public void DepositTo(AccountLoc accountToDeposit, decimal amount)
        {
            var accountIndex = (int) accountToDeposit;
            
            if (IsAccountDeleted(accountToDeposit))
                return;
            
            _accountList[accountIndex]?.Deposit(amount);
        }

        /// <summary>
        /// Withdraws money from the selected account
        /// </summary>
        /// <param name="accountToWithdraw"></param>
        /// <param name="amount"></param>
        /// <returns>Bool representing success of withdrawal</returns>
        public bool WithdrawFrom(AccountLoc accountToWithdraw, decimal amount)
        {
            var accountIndex = (int) accountToWithdraw;
            if (IsAccountDeleted(accountToWithdraw))
                return false; // Will not run if acc has been deleted, but what if secondary is deleted
            if (accountIndex == (int) AccountLoc.PrimaryAccount && _accountList.Count > 1)
            {
                return !IsAccountDeleted(AccountLoc.SecondaryAccount) && _accountList[accountIndex].Withdraw(amount, _accountList[1]);
            }

            return _accountList[accountIndex].Withdraw(amount);
        }


        /// <summary>
        /// Transfers money between the selected account and a supplied one
        /// </summary>
        /// <param name="accountFromTransfer">The account the transfer originates from</param>
        /// <param name="amount">The amount that will be transferred</param>
        /// <param name="toTransfer">The account the money will be transferred to</param>
        /// <returns>Bool representing success of transfer</returns>
        public bool TransferBetween(AccountLoc accountFromTransfer, decimal amount, Account toTransfer)
        {
            var accountIndex = (int) accountFromTransfer;
            
            if (IsAccountDeleted(accountFromTransfer))
                return false;
            
            if (accountIndex == (int) AccountLoc.PrimaryAccount && _accountList.Count > 1)
            {
                return !IsAccountDeleted(AccountLoc.SecondaryAccount) && _accountList[accountIndex].Transfer(amount, _accountList[1], toTransfer);
            }

            return _accountList[accountIndex].Transfer(amount, toTransfer);
        }

        /// <summary>
        /// Swap the primary and secondary accounts if both exist
        /// </summary>
        /// <returns>bool representing if the swap was successful.</returns>
        public bool SwapAccounts()
        {
            if (_numberOfAccounts < MaxAccounts || IsAccountDeleted(AccountLoc.PrimaryAccount) || IsAccountDeleted(AccountLoc.SecondaryAccount))
                return false;
            
            _accountList.Reverse(); // IF this reverses the order, should work
            return true;
        }

        /// <summary>
        /// Posts Interest on contained accounts
        /// </summary>
        /// <param name="bankPrimeRate"></param>
        public void PostAccounts(decimal bankPrimeRate)
        {
            foreach (var account in _accountList)
            {
                account?.PostInterest(bankPrimeRate); //
            }
        }

        public void EndInterestPosting()
        {
            foreach (var account in _accountList)
            {
                account?.EndPosting();
            }
        }

        /// Returns false if Account exists (delete flag counts as not exist)
        private bool IsAccountDeleted(AccountLoc accountToCheck) 
        {
            var accountIndex = (int) accountToCheck;

            if (accountIndex >= _numberOfAccounts) // Account is out of range so counts as deleted
                return true;
            
            if (accountIndex < _numberOfAccounts || (_accountList[accountIndex] != null && !_accountList[accountIndex].Deleted))
                return false;
            
            //Account exists but has delete flag so remove it
            _accountList.RemoveAt(accountIndex); // remove the deleted account
            --_numberOfAccounts;
            return true;
        }

        /// <summary>
        /// Sets the Delete flag in an account
        /// </summary>
        /// <param name="accountToDelete"></param>
        /// <returns></returns>
        public bool DeleteAccount(AccountLoc accountToDelete)
        {
            var accountIndex = (int) accountToDelete;
            
            if (accountIndex >= _numberOfAccounts || _accountList[accountIndex].IsLinked(UserIdNumber) != 0) // Account must exist and be owned by customer
                return false;
            
            _accountList[accountIndex].SetDelete();
            --_numberOfAccounts;
            return true;

        }

        /// <summary>
        /// Processes a check from the selected account
        /// </summary>
        /// <param name="checkNumber"></param>
        /// <param name="accountCheckFrom"></param>
        /// <param name="toName"></param>
        /// <param name="checkAmount"></param>
        /// <returns></returns>
        public bool ProcessCheck(int checkNumber, AccountLoc accountCheckFrom, string toName, decimal checkAmount)
        {
            var accountIndex = (int) accountCheckFrom;
            
            if (IsAccountDeleted(accountCheckFrom))
                return false;
            
            if (checkAmount > 0)
            {
                bool success;
                if (accountCheckFrom == AccountLoc.PrimaryAccount &&
                    _accountList.Count == MaxAccounts) // If primary and has secondary (has max)
                {
                    if (IsAccountDeleted(AccountLoc.SecondaryAccount))
                        return false;

                    {
                        if (_accountList[accountIndex] is CheckingAccount isChecking)
                        {
                            success = isChecking.Withdraw(checkAmount, SecondaryAccount);
                            //if (success)
                            //   OutboundAccount.Send(checkAmount);
                            return success;
                        }

                        return false;
                    }
                }
                else if (
                    _accountList[accountIndex] is CheckingAccount isChecking) // IF secondary or primary w/o secondary
                {
                    success = isChecking.Withdraw(checkAmount);
                    //if (success) OutboundAccount.Send(checkAmount);
                    return success;
                }
            }
            else
            {
                _accountList[accountIndex].Deposit(-checkAmount); // Can't Fail, assumes the withdrawal was valid
                return true;
            }

            return false;

        }
        /// <summary>
        /// Processes a check from the selected account
        /// </summary>
        /// <param name="checkNumber"></param>
        /// <param name="accountCheckFrom"></param>
        /// <param name="toName"></param>
        /// <param name="checkAmount"></param>
        /// <param name="accountToTransfer"></param>
        /// <returns></returns>
        public bool ProcessCheck(int checkNumber, AccountLoc accountCheckFrom, string toName, decimal checkAmount,
            Account accountToTransfer)
        {
            //var e = new SavingsAccount(2);
            //var a = (CheckingAccount) e;
            //var x = (Account) e;
            {
                
                var accountIndex = (int) accountCheckFrom;
                
                if (IsAccountDeleted(accountCheckFrom))
                    return false;
                
                bool success;
                if (checkAmount > 0)
                {
                    if (accountCheckFrom == AccountLoc.PrimaryAccount &&
                        _accountList.Count == MaxAccounts) // If primary and has secondary (Has max)
                    {
                        if (IsAccountDeleted(AccountLoc.SecondaryAccount))
                            return false;

                        {
                            if (_accountList[accountIndex] is CheckingAccount isChecking)
                            {
                                success = isChecking.Withdraw(checkAmount, SecondaryAccount);
                                if (success)
                                    accountToTransfer.Deposit(checkAmount);
                                return success;
                            }

                            return false;
                        }
                    }
                    else if (
                        _accountList[accountIndex] is CheckingAccount isChecking) // IF secondary or primary w/o secondary
                    {
                        success = isChecking.Withdraw(checkAmount);
                        if (success)
                            accountToTransfer.Deposit(checkAmount);
                        return success;
                    }

                    return false;

                }
                else if (accountToTransfer is CheckingAccount isChecking) // opposite check
                {
                    success = isChecking.Withdraw(-checkAmount);
                    if (success) _accountList[accountIndex].Deposit(-checkAmount);
                    return success;
                }

                return false;

            }
        }


        /// <summary>
        /// Processes a check from the selected account
        /// </summary>
        /// <param name="checkNumber"></param>
        /// <param name="accountCheckFrom"></param>
        /// <param name="toName"></param>
        /// <param name="checkAmount"></param>
        /// <param name="accountToTransfer"></param>
        /// <param name="accountToTransferSecondary"></param>
        /// <returns>true if check passes, false if check flops</returns>
        public bool ProcessCheck(int checkNumber, AccountLoc accountCheckFrom, string toName, decimal checkAmount,
            Account accountToTransfer, Account accountToTransferSecondary)
        {
            
            var accountIndex = (int) accountCheckFrom;
            
            if (IsAccountDeleted(accountCheckFrom))
                return false;
            
            bool success;
            if (checkAmount > 0)
            {
                if (accountCheckFrom == AccountLoc.PrimaryAccount &&
                    _accountList.Count == MaxAccounts) // If primary and has secondary (Has max)
                {
                    if (IsAccountDeleted(AccountLoc.SecondaryAccount)) return false;

                    {
                        if (_accountList[accountIndex] is CheckingAccount isChecking)
                        {
                            success = isChecking.Withdraw(checkAmount, SecondaryAccount);
                            if (success) accountToTransfer.Deposit(checkAmount);
                            return success;
                        }

                        return false;
                    }
                }
                else if (_accountList[accountIndex] is CheckingAccount isChecking) // IF secondary or primary w/o secondary
                {
                    success = isChecking.Withdraw(checkAmount);
                    if (success) accountToTransfer.Deposit(checkAmount);
                    return success;
                }

                return false;

            }
            else if (accountToTransfer is CheckingAccount isChecking)
            {
                success = accountToTransferSecondary != null ? isChecking.Withdraw(-checkAmount, accountToTransferSecondary) : isChecking.Withdraw(-checkAmount);
                if (success) _accountList[accountIndex].Deposit(-checkAmount);
                return success;
            }

            return false;
        }


        public bool? IsPrimaryAccount(short acc){ // Unused
            if (acc >= 0 && _numberOfAccounts > acc)
            {
                return acc == 0; // primary
            }

            return null;
        }
        
        /// <summary>
        /// Get the account type of the selected account
        /// </summary>
        /// <param name="accountToCheck"></param>
        /// <returns>type of account or null</returns>
        public AccountType? GetAccType(AccountLoc accountToCheck)
        {

            var accountIndex = (int) accountToCheck;
            if (!IsAccountDeleted(accountToCheck))
            {
                return _accountList[accountIndex] is CheckingAccount ? AccountType.Checking : AccountType.Savings;
            }

            return null;
        }

        /// <summary>
        /// Returns 0 or the userNumber of the customer that owns the account.
        /// Returns int.MaxValue if account does not exist.
        /// </summary>
        /// <param name="accountToCheck"></param>
        /// <returns></returns>
        public int AccountLinked(AccountLoc accountToCheck) // Returns
        {
            var accountIndex = (int) accountToCheck;
            
            if (IsAccountDeleted(accountToCheck))
                return int.MaxValue; // Shows that account does not exist
            
            return _accountList[accountIndex].IsLinked(UserIdNumber);
        }

        /// <summary>
        /// Returns true if the account is NOT unique when compared to the owned accounts
        /// </summary>
        /// <param name="accountToCompare"></param>
        /// <returns></returns>
        public bool NotUniqueAccount(Account accountToCompare)
        {
            var notUnique = false;
            foreach (var account in _accountList)
            {
                notUnique = account == accountToCompare;
                
                if (notUnique)
                    break;
            }

            return notUnique;
        }
    };
    
}