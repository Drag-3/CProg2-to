using Bank.Accounts;
using NUnit.Framework;

namespace Tests
{
    public class AccountAndInheritorsTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void DepositTest()
        {
            var let = new CheckingAccount();
            let.Deposit(1000);
            Assert.AreEqual(1000, let.AccountBalance);
        }

        [Test]
        public void SufficientWithdrawalTest()
        {
            var let = new SavingsAccount();
            let.Deposit(1200);
            let.Withdraw(7.3453M);
            var solution = 1200 - 7.3453M;
            Assert.AreEqual(solution, let.AccountBalance);
        }

        
        [Test]
        public void SufficientWithdrawalTestWithSecondary()
        {
            var primary = new CheckingAccount();
            var secondary = new SavingsAccount();
            
            primary.Deposit(1000);
            secondary.Deposit(1000);

            primary.Withdraw(510, secondary);
            
            Assert.AreEqual(490, primary.AccountBalance);
            Assert.AreEqual(1000, secondary.AccountBalance);
        }
        [Test]
        public void InsufficientWithdrawalTest()
        {
            var let = new CheckingAccount();
            let.Deposit(5);
            let.Withdraw(150);
            Assert.AreEqual(-5, let.AccountBalance);
        }

        [Test]
        public void InsufficientWithdrawalTestWithSecondarySufficient()
        {
            var primary = new CheckingAccount();
            var secondary = new SavingsAccount();
            
            primary.Deposit(10);
            secondary.Deposit(1000);

            primary.Withdraw(510, secondary);
            
            Assert.AreEqual(0, primary.AccountBalance);
            Assert.AreEqual(500, secondary.AccountBalance);
        }
        
        [Test]
        public void InsufficientWithdrawalTestWithSecondaryInsufficient()
        {
            var primary = new CheckingAccount();
            var secondary = new SavingsAccount();
            
            primary.Deposit(10);
            secondary.Deposit(1000);

            primary.Withdraw(2000, secondary);
            
            Assert.AreEqual(0, primary.AccountBalance); // Penalty deducted
            Assert.AreEqual(1000, secondary.AccountBalance); // Unaffecetd
        }

        [Test]
        public void TransferTest()
        {
            var let = new CheckingAccount();
            var transfer = new SavingsAccount();
            
            let.Deposit(2000);

            let.Transfer(1000, transfer);
            
            Assert.AreEqual(1000, let.AccountBalance);
            Assert.AreEqual(1000, transfer.AccountBalance);
        }

        [Test]
        public void TransferTestInsufficientBalance()
        {
            var let = new CheckingAccount();
            var transfer = new SavingsAccount();
            
            let.Deposit(2000);

            let.Transfer(5000, transfer);
            
            Assert.AreEqual(1990, let.AccountBalance);
            Assert.AreEqual(0, transfer.AccountBalance);
        }
        
        [Test]
        public void TransferTestInsufficientBalanceWithSecondarySufficient()
        {
            var let = new CheckingAccount();
            var secondary = new SavingsAccount();
            var transfer = new SavingsAccount();
            
            let.Deposit(2000);
            secondary.Deposit(6000);

            let.Transfer(5000,secondary,  transfer);
            Assert.AreEqual(0, let.AccountBalance);
            Assert.AreEqual(3000, secondary.AccountBalance);
            Assert.AreEqual(5000, transfer.AccountBalance);
        }
        
        [Test]
        public void TransferTestInsufficientBalanceWithSecondaryInsufficient() // so long
        {
            var let = new CheckingAccount();
            var secondary = new SavingsAccount();
            var transfer = new SavingsAccount();
            
            let.Deposit(2000);
            secondary.Deposit(1000);

            let.Transfer(5000,secondary,  transfer);
            
            Assert.AreEqual(1990, let.AccountBalance); // Only penalty should be deducted
            Assert.AreEqual(1000, secondary.AccountBalance); // Should not be affected
            Assert.AreEqual(0, transfer.AccountBalance); 
        }
    }
}