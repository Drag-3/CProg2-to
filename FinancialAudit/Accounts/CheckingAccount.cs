
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests-")]
namespace FinancialAudit.Accounts
{
    public class CheckingAccount : Account
    {
        public CheckingAccount()
        {
        }

        public CheckingAccount(int owner) : base(owner)
        {
        }

        public CheckingAccount(int owner, decimal apr) : base(owner, apr)
        {
        }

        public CheckingAccount(int owner, decimal apr, long centPrecision, int multiplier) : base(owner, apr, centPrecision, multiplier)
        {
        }
        
        
        /// <inheritdoc/>
        public  override bool PostInterest(decimal primeInterest) {
            if (InterestPosted) return InterestPosted;
            decimal multiplier = ((primeInterest / 2) + AnnualPercentageRate) / 100 / 12;
            ApplyInterest(multiplier);
            return InterestPosted;
        }
    }
}