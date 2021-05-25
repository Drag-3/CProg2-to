
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AccountTests")]
namespace Bank.Accounts
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
        private uint Owner;

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
        public Account(uint owner) : this()
        {
            Owner = owner;
        }
        public Account(uint owner, decimal apr) :this(owner)
        {
            _annualPercentageRate = (int) (apr * _annualPercentageRateMultiplier);
        }
        public Account(uint owner, decimal apr, long centPrecision, int multiplier) : this(owner, apr)
        {
            _centPrecision = centPrecision;
            _annualPercentageRateMultiplier = multiplier;
        }
        
        public void Deposit(decimal amt)
        {
            _cents += (long) (amt * _centPrecision);
        }

        public void SetDelete() // Cannot undo this action
        {
            Deleted = true;
        }
        
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
        public virtual bool Withdraw(decimal amt, Account secondary ) // Overload for Accounts w/ Sec
        {
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

        public virtual bool Transfer(decimal amount, Account toTransfer)// Primary Account w/o Sec or Secondary acc
        {
            var success = Withdraw(amount);
            if (success)
            {
                toTransfer.Deposit(amount);
            }

            return success;
        }

        public virtual bool Transfer(decimal amount, Account secondary, Account toTransfer) // Overload for Accounts w/ Sec
        {
            var success = Withdraw(amount, secondary);
            if (success)
            {
                toTransfer.Deposit(amount);
            }

            return success;
        }

        public abstract bool PostInterest(decimal bankPrimeRate);

        public virtual void EndPosting() => InterestPosted = false;

        public decimal GetBalance() 
        {
            return (decimal) _cents / _centPrecision;
        }

        

        public uint IsLinked(uint toCompare)
        {
            return toCompare == Owner ? 0 : Owner;
        }

        public bool IsDeleted()
        {
            return Deleted;
        }
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
        

        protected void ApplyInterest(decimal multiplier)
        {
            if (_cents <= 0) return;
            var addedValue = _cents * multiplier;
            _cents += (long) addedValue;
            InterestPosted = true;
        }
        
    }
    
}