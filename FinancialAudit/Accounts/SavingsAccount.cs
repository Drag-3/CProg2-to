namespace FinancialAudit.Accounts
{

    public class SavingsAccount : Account
    {
        private short _numberOfWithdrawals;
        public SavingsAccount()
        {
            _numberOfWithdrawals = 0;
        }

        public SavingsAccount(int owner) : base(owner)
        {
            _numberOfWithdrawals = 0;
        }

        public SavingsAccount(int owner, decimal apr) : base(owner, apr)
        {
            _numberOfWithdrawals = 0;
        }

        public SavingsAccount(int owner, decimal apr, long centPrecision, int multiplier) : base(owner, apr, centPrecision, multiplier)
        {
            _numberOfWithdrawals = 0;
        }
        
        /// <inheritdoc cref="Account.Withdraw(decimal)"/>
        /// <remarks>Only allows three withdraws or transfers each posting cycle, more than three will fail</remarks>
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


        /// <inheritdoc cref="Account.Withdraw(decimal,Account)"/>
        /// <remarks>Only allows three withdraws or transfers each posting cycle, more than three will fail</remarks>

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
        
        /// <inheritdoc cref="Account.Transfer(decimal,Account)"/>
        /// <remarks>Only allows three withdraws or transfers each posting cycle, more than three will fail</remarks>
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


        /// <inheritdoc cref="Account.Transfer(decimal,Account,Account)"/>
        /// <remarks>Only allows three withdraws or transfers each posting cycle, more than three will fail</remarks>
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
        
        /// <inheritdoc cref="Account.PostInterest(decimal)"/>
        public override bool PostInterest(decimal bankPrimeRate)
        {
            if (InterestPosted) return InterestPosted;
            var multiplier = ((bankPrimeRate + (AnnualPercentageRate)) / 100 / 12);
            ApplyInterest(multiplier);
            return InterestPosted;
        }

        /// <inheritdoc cref="Account.EndPosting"/>
        /// <remarks>Resets withdrawal counter</remarks>
        
        public override void EndPosting()
        {
            _numberOfWithdrawals = 0;
            base.EndPosting();
        }
    }
}