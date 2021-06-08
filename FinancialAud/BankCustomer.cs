using System;
using System.Collections.Generic;
using System.Text;
using Bank.Accounts;

namespace Bank
{
    internal enum AccountLoc
    {
        PrimaryAccount,
        SecondaryAccount
    }
    public enum AccountType
    {
        Savings,
        Checking
    }
    

    public class BankCustomer
    {
        private readonly List<Account> _accountList;
        private ushort _numberOfAccounts;
        public uint UserIdNumber { get; }
        public string UserName { get; set; }

        public decimal PrimaryAccountBalance
        {
            get
            {
                if (CheckForAccountDeleted((int) AccountLoc.PrimaryAccount)) return 0;
                return _accountList.Count > (int) AccountLoc.PrimaryAccount ?
                    _accountList[(int) AccountLoc.PrimaryAccount].AccountBalance : 0;
            }
        }
        

        public decimal SecondaryAccountBalance
        {
            get
            {
                if (CheckForAccountDeleted((int) AccountLoc.SecondaryAccount)) return 0;
                return _accountList.Count > (int) AccountLoc.SecondaryAccount ?
                    _accountList[(int) AccountLoc.SecondaryAccount].AccountBalance : 0;
            }
        }

        public decimal PrimaryAccountAnnualPercentageRate
        {
            get
            {
                if (CheckForAccountDeleted((int) AccountLoc.PrimaryAccount)) return 0;
                return _accountList.Count > (int) AccountLoc.PrimaryAccount ?
                    _accountList[(int) AccountLoc.PrimaryAccount].AnnualPercentageRate : 0;
            }
            set
            {
                if (_accountList.Count > (int) AccountLoc.PrimaryAccount)
                {
                    _accountList[(int) AccountLoc.PrimaryAccount].AnnualPercentageRate = value;
                }
            }
        }

        public decimal SecondaryAccountAnnualPercentageRate
        {
            get
            {
                if (CheckForAccountDeleted((int) AccountLoc.SecondaryAccount)) return 0;
                return _accountList.Count > (int) AccountLoc.SecondaryAccount ?
                    _accountList[(int) AccountLoc.SecondaryAccount].AnnualPercentageRate : 0;
            }
            set
            {
                var _ = CheckForAccountDeleted((int) AccountLoc.SecondaryAccount); // Maybe this will work
                if (_accountList.Count > (int) AccountLoc.SecondaryAccount)
                {
                    _accountList[(int) AccountLoc.SecondaryAccount].AnnualPercentageRate = value;
                }
            }
        }

        public Account PrimaryAccount // Returns the actual account 
        {
            get
            {
                var _ = CheckForAccountDeleted((int) AccountLoc.PrimaryAccount);
                return _numberOfAccounts > (int) AccountLoc.PrimaryAccount ?
                    _accountList[(int) AccountLoc.PrimaryAccount] : null;

                //throw new IndexOutOfRangeException("The account does not exist");
            }   // Null seems easier to work with & test for.
        }

        public Account SecondaryAccount
        {
            get
            {
                var _ = CheckForAccountDeleted((int) AccountLoc.SecondaryAccount);
                return _numberOfAccounts > (int) AccountLoc.SecondaryAccount ?
                    _accountList[(int) AccountLoc.SecondaryAccount] : null;


                //throw new IndexOutOfRangeException("The account does not exist");
            }
        }

        public BankCustomer(uint userIdNumber, string userName)
        {
            _accountList = new List<Account>();
            _numberOfAccounts = 0;
            UserIdNumber = userIdNumber;
            UserName = userName;
        }

        // Creates Default Account
        public void AddAccount(char accountType)
        {
            if (_numberOfAccounts >= 2) return;
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

        public void AddAccount(char accountType, decimal accountInterest)
        {
            if (_numberOfAccounts >= 2) return;
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

        public void AddAccount(Account accountToLink) // This is the only case where overload is used
        {
            if (_numberOfAccounts >= 2) return;
             _accountList.Add(accountToLink);
            ++_numberOfAccounts;
        }

        public void DepositTo(ushort account, decimal amount)
        {
            if (CheckForAccountDeleted(account) || account >= _numberOfAccounts) return;
            _accountList[account]?.Deposit(amount);
        }

        public bool WithdrawFrom(ushort account, decimal amount)
        {
            if (CheckForAccountDeleted(account))
                return false; // Will not run if acc has been deleted, but what if secondary is deleted
            if (account == (int) AccountLoc.PrimaryAccount && _accountList.Count > 1)
            {
                return !CheckForAccountDeleted(1) && _accountList[account].Withdraw(amount, _accountList[1]);
            }

            return _accountList[account].Withdraw(amount);
        }


        public bool TransferBetween(ushort account, decimal amount, Account toTransfer)
        {
            if (CheckForAccountDeleted(account)) return false;
            if (account == (int) AccountLoc.PrimaryAccount && _accountList.Count > 1)
            {
                return !CheckForAccountDeleted(1) && _accountList[account].Transfer(amount, _accountList[1], toTransfer);
            }

            return _accountList[account].Transfer(amount, toTransfer);
        }

        public bool SwapAccounts()
        {
            if (_numberOfAccounts < 2 || CheckForAccountDeleted(0) || CheckForAccountDeleted(1)) return false;
            _accountList.Reverse(); // IF this reverses the order, should work
            return true;
        }

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

        private bool CheckForAccountDeleted(int account)
        {
            if (account >= _numberOfAccounts || (_accountList[account] != null && !_accountList[account].Deleted)) return false;
            _accountList.RemoveAt(account);
            --_numberOfAccounts;
            return true;
        }

        public bool DeleteAccount(ushort account)
        {
            if (account >= _numberOfAccounts || _accountList[account].IsLinked(UserIdNumber) != 0) return false;
            _accountList[account].SetDelete();
            --_numberOfAccounts;
            return true;

        }

        public bool ProcessCheck(int checkNumber, ushort account, string toName, decimal checkAmount)
        {
            if (CheckForAccountDeleted(account)) return false;
            if (checkAmount > 0)
            {
                bool success;
                if (account == (int) AccountLoc.PrimaryAccount &&
                    _accountList.Count == 2) // If primary and has secondary
                {
                    if (CheckForAccountDeleted(1)) return false;

                    {
                        if (_accountList[account] is CheckingAccount isChecking)
                        {
                            success = isChecking.Withdraw(checkAmount, SecondaryAccount);
                            //if (success) OutboundAccount.Send(checkAmount);
                            return success;
                        }

                        return false;
                    }
                }
                else if (
                    _accountList[account] is CheckingAccount isChecking) // IF secondary or primary w/o secondary
                {
                    success = isChecking.Withdraw(checkAmount);
                    //if (success) OutboundAccount.Send(checkAmount);
                    return success;
                }
            }
            else
            {
                _accountList[account].Deposit(-checkAmount); // Can't Fail, assumes the withdrawl is valid
                return true;
            }

            return false;

        }
        public bool ProcessCheck(int checkNumber, ushort account, string toName, decimal checkAmount,
            Account accountToTransfer)
        {
            //var e = new SavingsAccount(2);
            //var a = (CheckingAccount) e;
            //var x = (Account) e;
            {
                if (CheckForAccountDeleted(account)) return false;
                bool success;
                if (checkAmount > 0)
                {
                    if (account == (int) AccountLoc.PrimaryAccount &&
                        _accountList.Count == 2) // If primary and has secondary
                    {
                        if (CheckForAccountDeleted(1)) return false;

                        {
                            if (_accountList[account] is CheckingAccount isChecking)
                            {
                                success = isChecking.Withdraw(checkAmount, SecondaryAccount);
                                if (success) accountToTransfer.Deposit(checkAmount);
                                return success;
                            }

                            return false;
                        }
                    }
                    else if (
                        _accountList[account] is CheckingAccount isChecking) // IF secondary or primary w/o secondary
                    {
                        success = isChecking.Withdraw(checkAmount);
                        if (success) accountToTransfer.Deposit(checkAmount);
                        return success;
                    }

                    return false;

                }
                else if (accountToTransfer is CheckingAccount isChecking) // opposite check
                {
                    success = isChecking.Withdraw(-checkAmount);
                    if (success) _accountList[account].Deposit(-checkAmount);
                    return success;
                }

                return false;

            }
        }


        public bool ProcessCheck(int checkNumber, ushort account, string toName, decimal checkAmount,
            Account accountToTransfer, Account accountToTransferSecondary)
        {
            if (CheckForAccountDeleted(account)) return false;
            bool success;
            if (checkAmount > 0)
            {
                if (account == (int) AccountLoc.PrimaryAccount &&
                    _accountList.Count == 2) // If primary and has secondary
                {
                    if (CheckForAccountDeleted(1)) return false;

                    {
                        if (_accountList[account] is CheckingAccount isChecking)
                        {
                            success = isChecking.Withdraw(checkAmount, SecondaryAccount);
                            if (success) accountToTransfer.Deposit(checkAmount);
                            return success;
                        }

                        return false;
                    }
                }
                else if (_accountList[account] is CheckingAccount isChecking) // IF secondary or primary w/o secondary
                {
                    success = isChecking.Withdraw(checkAmount);
                    if (success) accountToTransfer.Deposit(checkAmount);
                    return success;
                }

                return false;

            }
            else if (accountToTransfer is CheckingAccount isChecking)
            {
                success = isChecking.Withdraw(-checkAmount, accountToTransferSecondary);
                if (success) _accountList[account].Deposit(-checkAmount);
                return success;
            }

            return false;
        }


        public bool? IsPrimary(short acc){
            var output = new StringBuilder();
            if (acc >= 0 && _numberOfAccounts > acc)
            {
                return acc == 0; // primary
            }

            return null;
        }
        
        public AccountType? GetAccType(short acc){
            
            if (_numberOfAccounts > acc)
            {
                return _accountList[acc] is CheckingAccount ? AccountType.Checking : AccountType.Savings;
            }

            return null;
        }

        public uint AccountLinked(int accountToCheck) // Returns
        {
            if (CheckForAccountDeleted(accountToCheck) || _accountList.Count <= accountToCheck) return UInt32.MaxValue;
            return _accountList[accountToCheck].IsLinked(UserIdNumber);
        }

        public bool NotUniqueAccount(Account accountToCompare)
        {
            bool unique = false;
            foreach (var account in _accountList)
            {
                unique = account == accountToCompare;
            }

            return unique;
        }
    };
    
}