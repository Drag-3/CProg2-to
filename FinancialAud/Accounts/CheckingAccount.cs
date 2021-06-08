
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests-")]
namespace Bank.Accounts
{
    public class CheckingAccount : Account
    {
        public CheckingAccount()
        {
        }

        public CheckingAccount(uint owner) : base(owner)
        {
        }

        public CheckingAccount(uint owner, decimal apr) : base(owner, apr)
        {
        }

        public CheckingAccount(uint owner, decimal apr, long centPrecision, int multiplier) : base(owner, apr, centPrecision, multiplier)
        {
        }
        
        
        public  override bool PostInterest(decimal primeInterest) {
            if (InterestPosted) return InterestPosted;
            decimal multiplier = ((primeInterest / 2) + AnnualPercentageRate) / 100 / 12;
            ApplyInterest(multiplier);
            return InterestPosted;
        }
    }
}