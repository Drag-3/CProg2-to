
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests-")]
namespace FinancialAudit.Accounts
{
    
    public abstract class Account
    {
        //Account(decimal, uint);
        private long _cents;
        private long _centPrecision;
        
        private int _annualPercentageRate;
        private int _annualPercentageRateMultiplier;

        public decimal AnnualPercentageRate
        {
            get => (decimal) _annualPercentageRate / _annualPercentageRateMultiplier;
            set
            {
                if (value > 0)
                {
                    _annualPercentageRate = (int) (value * _annualPercentageRateMultiplier);
                }
            }
        }

        public decimal AccountBalance => (decimal) _cents /  _centPrecision;

        public bool InterestPosted { get; private set; }
        private int Owner;

        public bool Deleted
        {
            get;
            private set;
        }

         public Account()
        {
            _cents = 0;
            _centPrecision = 1000000000;
            _annualPercentageRateMultiplier = 100;
            _annualPercentageRate = 0;
            InterestPosted = false;
            Owner = 0;
            Deleted = false;
        }
        public Account(int owner) : this()
        {
            Owner = owner;
        }
        public Account(int owner, decimal apr) :this(owner)
        {
            _annualPercentageRate = (int) (apr * _annualPercentageRateMultiplier);
        }
        public Account(int owner, decimal apr, long centPrecision, int multiplier) : this(owner, apr)
        {
            _centPrecision = centPrecision;
            _annualPercentageRateMultiplier = multiplier;
        }
        
        /// <summary>
        /// Deposits the amount to the account
        /// </summary>
        /// <param name="amt">Amount to deposit</param>
        public void Deposit(decimal amt)
        {
            _cents += (long) (amt * _centPrecision);
        }

        /// <summary>
        /// Sets the delete flag to true CAN'T UNDO RIGHT NOW
        /// </summary>
        public void SetDelete() // Cannot undo this action
        {
            Deleted = true;
        }
        
        /// <summary>
        /// Withdraws the amount from the account is able
        /// </summary>
        /// <param name="amt">Amount to withdraw</param>
        /// <returns>Success of the withdrawal</returns>
        public virtual bool Withdraw(decimal amt) // Primary Account w/o Sec or Secondary acc
        {
            var amountToWithdraw = (long) (amt * _centPrecision);
            var success = false;
            if (_cents - amountToWithdraw < 0)
            {
                ApplyPenalty(10);
            }
            else
            {
                _cents -= amountToWithdraw;
                success = true;
            }
            return success;
        }
        /// <summary>
        /// Tries to withdraw the amount from this account or from secondary if insufficient
        /// </summary>
        /// <param name="amt">Amount to Withdraw</param>
        /// <param name="secondary">Secondary Account to draw from</param>
        /// <returns>Success of the withdrawal</returns>
        public virtual bool Withdraw(decimal amt, Account secondary ) // Overload for Accounts w/ Sec
        {
            if (secondary == null) //if a null value is passed treat it as a normal withdrawal - Maybe add something to let know
                return Withdraw(amt);
            
            
            var amountToWithdraw = (long) amt * _centPrecision;
            var success = false;
            if (_cents - amountToWithdraw < 0)
            {
                if (secondary.SecondaryWithdrawal(amountToWithdraw - _cents))
                {
                    success = true;
                    _cents = 0;
                }
                else
                {
                    ApplyPenalty(10);
                }
            }
            else
            {
                _cents -= amountToWithdraw;
                success = true;
            }
            return success;
        }

        /// <summary>
        /// If able transfers amount from this account to toTransfer
        /// </summary>
        /// <param name="amount">Amount to Transfer</param>
        /// <param name="toTransfer">Account to Transfer To</param>
        /// <returns>Success of the transfer</returns>
        public virtual bool Transfer(decimal amount, Account toTransfer)// Primary Account w/o Sec or Secondary acc
        {
            var success = Withdraw(amount);
            if (success)
            {
                toTransfer.Deposit(amount);
            }

            return success;
        }

        /// <summary>
        /// Tries to transfer money from this account, takes difference from secondary if insufficient.
        /// </summary>
        /// <param name="amount">Amount to Transfer</param>
        /// <param name="secondary">Secondary Account to draw from</param>
        /// <param name="toTransfer">Account to Transfer to</param>
        /// <returns>Success of the transfer</returns>
        public virtual bool Transfer(decimal amount, Account secondary, Account toTransfer) // Overload for Accounts w/ Sec
        {
            var success = Withdraw(amount, secondary);
            if (success)
            {
                toTransfer.Deposit(amount);
            }

            return success;
        }

        
        /// <summary>
        /// Posts Interest on this account using prime and stored interest rate
        /// </summary>
        /// <param name="bankPrimeRate">The prime rate of the bank</param>
        /// <returns>Success of  the posting</returns>
        public abstract bool PostInterest(decimal bankPrimeRate);

        /// <summary>
        /// Resets the InterestPosted flag
        /// </summary>
        public virtual void EndPosting() => InterestPosted = false;

        public decimal GetBalance() 
        {
            return (decimal) _cents / _centPrecision;
        }

        

        /// <summary>
        /// Compares entered int against owner If it does not match returns the owner's number else 0
        /// </summary>
        /// <param name="toCompare">The user Id to compare against</param>
        /// <returns>entered id is owner - 0 OR
        ///     <para>owner's id</para>
        /// </returns>
        public int IsLinked(int toCompare)
        {
            return toCompare == Owner ? 0 : Owner;
        }
        public bool IsDeleted()
        {
            return Deleted;
        }
        
        /// <summary>
        /// Applies a penalty to the account (direct subtracts money ignores balance)
        /// </summary>
        /// <param name="penalty">The penalty in whole dollars to add</param>
        protected void ApplyPenalty(int penalty)
        {
            _cents -= penalty * _centPrecision;
        }
        
        private bool SecondaryWithdrawal(long amount) // To be done when taking money from secondary account 
        {
            var success = (_cents - amount) >= 0;
            if (success)
            {
                _cents -= amount;
            }

            return success;
        }
        

        /// <summary>
        /// Applies interest to the account given a multiplier
        /// </summary>
        /// <param name="multiplier"></param>
        protected void ApplyInterest(decimal multiplier)
        {
            if (_cents <= 0) return;
            var addedValue = _cents * multiplier;
            _cents += (long) addedValue;
            InterestPosted = true;
        }
        
    }
    
}