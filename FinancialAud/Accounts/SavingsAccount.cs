namespace Bank.Accounts
{

    public class SavingsAccount : Account
    {
        private short _numberOfWithdrawals;
        public SavingsAccount()
        {
            _numberOfWithdrawals = 0;
        }

        public SavingsAccount(uint owner) : base(owner)
        {
            _numberOfWithdrawals = 0;
        }

        public SavingsAccount(uint owner, decimal apr) : base(owner, apr)
        {
            _numberOfWithdrawals = 0;
        }

        public SavingsAccount(uint owner, decimal apr, long centPrecision, int multiplier) : base(owner, apr, centPrecision, multiplier)
        {
            _numberOfWithdrawals = 0;
        }

        public override bool Withdraw(decimal amt)
        {
            if (_numberOfWithdrawals >= 3)
            {
                ApplyPenalty(5);
                return false;
            }
            ++_numberOfWithdrawals;
            return base.Withdraw(amt);
        }

        public override bool Withdraw(decimal amt, Account secondary)
        {
            if (_numberOfWithdrawals >= 3)
            {
                ApplyPenalty(5);
                return false;
            }
            ++_numberOfWithdrawals;
            return base.Withdraw(amt, secondary);
        }

        public override bool Transfer(decimal amount, Account toTransfer)
        {
            if (_numberOfWithdrawals >= 3)
            {
                ApplyPenalty(5);
                return false;
            }
            
            ++_numberOfWithdrawals;
            return base.Transfer(amount, toTransfer);
        }

        public override bool Transfer(decimal amount, Account secondary, Account toTransfer)
        {
            if (_numberOfWithdrawals >= 3)
            {
                ApplyPenalty(5);
                return false;
            }
            ++_numberOfWithdrawals;
            return base.Transfer(amount,secondary, toTransfer);
        }

        public override bool PostInterest(decimal bankPrimeRate)
        {
            if (InterestPosted) return InterestPosted;
            var multiplier = ((bankPrimeRate + (AnnualPercentageRate)) / 100 / 12);
            ApplyInterest(multiplier);
            return InterestPosted;
        }

        public override void EndPosting()
        {
            _numberOfWithdrawals = 0;
            base.EndPosting();
        }
    }
}