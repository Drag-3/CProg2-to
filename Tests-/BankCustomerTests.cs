using System.Runtime.InteropServices;
using NUnit.Framework;
using Bank;

namespace Tests
{
    public class BankCustomerTests
    {
        
        [SetUp]
        public void Setup()
        {
        }

        public BankCustomer SetupOneChecking()
        {
            var customer = new BankCustomer(1, "yes");
            customer.AddAccount('C');
            return customer;
        }
        public BankCustomer SetupOneSavings()
        {
            var customer = new BankCustomer(1, "yes");
            customer.AddAccount('S');
            return customer;
        }
        public BankCustomer SetupCheckings()
        {
            var customer = new BankCustomer(1, "yes");
            customer.AddAccount('C');
            customer.AddAccount('C');
            return customer;
        }
        public BankCustomer SetupSavings()
        {
            var customer = new BankCustomer(1, "yes");
            customer.AddAccount('S');
            customer.AddAccount('S');
            return customer;
        }
        public BankCustomer SetupCheckingSavings()
        {
            var customer = new BankCustomer(1, "yes");
            customer.AddAccount('C');
            customer.AddAccount('S');
            return customer;
        }
        [Test]
        public void CreateSavingsTest()
        {
            var let = SetupOneSavings();
            Assert.AreEqual(let.PrimaryAccount.GetType(), typeof(Bank.Accounts.SavingsAccount));
        }
        [Test]
        public void CreateCheckingTest()
        {
            var let = SetupOneChecking();
            Assert.AreEqual(let.PrimaryAccount.GetType(), typeof(Bank.Accounts.CheckingAccount));
        }
        [Test]
        public void LinkTest()
        {
            var let = new BankCustomer(1, "One");
            var accountToLink = new Bank.Accounts.CheckingAccount();
            let.AddAccount(accountToLink);
            Assert.AreEqual(accountToLink, let.PrimaryAccount);
        }
        [Test]
        public void DepositTest()
        {
            var let = SetupOneChecking();
            let.DepositTo(0, 12345);
            Assert.AreEqual(12345, let.PrimaryAccountBalance);
        }
        
        [Test]
        public void SufficientWithdrawalTest()
        {
            var let = SetupOneSavings();
            let.DepositTo(0, 1200);
            var e  =let.WithdrawFrom(0, 7.3453M);
            var solution = 1200 - 7.3453M;
            Assert.True(e);
            Assert.AreEqual(solution, let.PrimaryAccountBalance);
        }

        
        [Test]
        public void SufficientWithdrawalTestWithSecondary()
        {
            var let = SetupCheckingSavings();

            let.DepositTo(0, 1000);
            let.DepositTo(1, 1000);

            var e =let.WithdrawFrom(0, 510);
            
            Assert.True(e);
            Assert.AreEqual(490, let.PrimaryAccountBalance);
            Assert.AreEqual(1000, let.SecondaryAccountBalance);
        }
        [Test]
        public void InsufficientWithdrawalTest()
        {
            var let = SetupOneChecking();
            let.DepositTo(0, 5);
            var e =let.WithdrawFrom(0, 150);
            Assert.False(e);
            Assert.AreEqual(-5, let.PrimaryAccountBalance);
        }

        [Test]
        public void InsufficientWithdrawalTestWithSecondarySufficient()
        {
            var let = SetupCheckingSavings();
            
            let.DepositTo(0, 10);
            let.DepositTo(1, 1000);

            var e = let.WithdrawFrom(0, 510);
            
            Assert.True(e);
            Assert.AreEqual(0, let.PrimaryAccountBalance);
            Assert.AreEqual(500, let.SecondaryAccountBalance);
        }

        [Test]
        public void InsufficientWithdrawalTestWithSecondaryInsufficient()
        {
            var let = SetupCheckingSavings();

            let.DepositTo(0, 10);
            let.DepositTo(1, 1000);

            var e = let.WithdrawFrom(0, 2000);

            Assert.AreEqual(0, let.PrimaryAccountBalance); // Penalty deducted
            Assert.AreEqual(1000, let.SecondaryAccountBalance); // Unaffecetd
        }
        
        [Test]
        public void GlobalCheckSenderSufficent()
        {
            var let = SetupOneChecking();
            
            let.DepositTo(0, 1000);

            var e = let.ProcessCheck(0, 0, "John", 500);
            
            Assert.True(e);
            Assert.AreEqual(500, let.PrimaryAccountBalance);
        }
        
        [Test]
        public void GlobalCheckSenderPrimaryInsuffient()
        {
            var let = SetupOneChecking();
            
            let.DepositTo(0, 5);

            var e = let.ProcessCheck(0, 0, "John", 500);
            
            Assert.False(e);
            Assert.AreEqual(-5, let.PrimaryAccountBalance);
        }
        [Test]
        public void GlobalCheckSenderPrimaryInsuffientSecondarySufficent()
        {
            var let = SetupCheckingSavings();
            
            let.DepositTo(1, 1000);

            var e = let.ProcessCheck(0, 0, "John", 500);
            
            Assert.True(e);
            Assert.AreEqual(0, let.PrimaryAccountBalance);
            Assert.AreEqual(500, let.SecondaryAccountBalance);
        }
        [Test]
        public void GlobalCheckSenderPrimaryInsuffientSecondaryInsufficent()
        {
            var let = SetupCheckingSavings();
            

            var e = let.ProcessCheck(0, 0, "John", 500);
            
            Assert.False(e);
            Assert.AreEqual(-10, let.PrimaryAccountBalance);
            Assert.AreEqual(0, let.SecondaryAccountBalance);
        }
        
        [Test]
        public void GlobalCheckSenderSufficentSavings()
        {
            var let = SetupOneSavings();
            
            let.DepositTo(0, 1000);

            var e = let.ProcessCheck(0, 0, "John", 500);
            
            Assert.False(e);
            Assert.AreEqual(1000, let.PrimaryAccountBalance);
        }
        
        [Test]
        public void GlobalCheckSenderSufficentReversed()
        {
            var let = SetupOneChecking();
            
            let.DepositTo(0, 1000);

            var e = let.ProcessCheck(0, 0, "John", -500);
            
            Assert.AreEqual(1500, let.PrimaryAccountBalance);
            Assert.True(e);
            
        }

        [Test]
        public void GlobalCheckSenderSufficentSavingsReversed()
        {
            var let = SetupOneSavings();
            
            let.DepositTo(0, 1000);

            var e = let.ProcessCheck(0, 0, "John", -500);
            
            Assert.True(e);
            Assert.AreEqual(1500, let.PrimaryAccountBalance); // Savings accounts can accept checks but not send them
        }
        [Test]
        public void LocalCheckSenderSufficent()
        {
            var let = SetupOneChecking();
            var checking = new Bank.Accounts.CheckingAccount();
            let.DepositTo(0, 1000);

            var e = let.ProcessCheck(0, 0, "John", 500, checking);
            
            Assert.True(e);
            Assert.AreEqual(500, let.PrimaryAccountBalance);
            Assert.AreEqual(500, checking.AccountBalance);
        }
        
        [Test]
        public void LocalCheckSenderPrimaryInsufficentSecondarySufficent()
        {
            var let = SetupCheckingSavings();
            var checking = new Bank.Accounts.CheckingAccount();
            
            let.DepositTo(1, 1000);

            var e = let.ProcessCheck(0, 0, "John", 500, checking);
            
            Assert.True(e);
            Assert.AreEqual(0, let.PrimaryAccountBalance);
            Assert.AreEqual(500, let.SecondaryAccountBalance);
            Assert.AreEqual(500, checking.AccountBalance);
        }
        
        [Test]
        public void LocalCheckSenderPrimaryInsufficentSecondaryInsufficent()
        {
            var let = SetupCheckingSavings();
            var checking = new Bank.Accounts.CheckingAccount();
            

            var e = let.ProcessCheck(0, 0, "John", 500, checking);
            
            Assert.False(e);
            Assert.AreEqual(-10, let.PrimaryAccountBalance);
            Assert.AreEqual(0, let.SecondaryAccountBalance);
            Assert.AreEqual(0, checking.AccountBalance);
        }
        
        [Test]
        public void LocalCheckSenderSufficentSavings()
        {
            var let = SetupOneSavings();
            var checking = new Bank.Accounts.CheckingAccount();
            
            let.DepositTo(0, 1000);

            var e = let.ProcessCheck(0, 0, "John", 500, checking);
            
            Assert.False(e);
            Assert.AreEqual(1000, let.PrimaryAccountBalance);
        }
        
        [Test]
        public void LocalCheckSenderSufficentReversed()
        {
            var let = SetupOneChecking();
            var checking = new Bank.Accounts.CheckingAccount();
            checking.Deposit(1000);

            var e = let.ProcessCheck(0, 0, "John", -500, checking);
            
            Assert.True(e);
            Assert.AreEqual(500, let.PrimaryAccountBalance);
            Assert.AreEqual(500, checking.AccountBalance);
        }
        
        [Test]
        public void LocalCheckSenderPrimaryInsufficentSecondarySufficentReversed()
        {
            var let = SetupCheckingSavings();
            var checking = new Bank.Accounts.CheckingAccount();
            var savingsSecondary = new Bank.Accounts.SavingsAccount();
            
            savingsSecondary.Deposit(1000);

            var e = let.ProcessCheck(0, 0, "John", -500, checking, savingsSecondary);
            
            Assert.True(e);
            Assert.AreEqual(500, let.PrimaryAccountBalance);
            Assert.AreEqual(0, let.SecondaryAccountBalance);
            Assert.AreEqual(0, checking.AccountBalance);
            Assert.AreEqual(500, savingsSecondary.AccountBalance);
        }
        
        [Test]
        public void LocalCheckSenderPrimaryInsufficentSecondaryInsufficentReversed()
        {
            var let = SetupCheckingSavings();
            var checking = new Bank.Accounts.CheckingAccount();
            var savingsSecondary = new Bank.Accounts.SavingsAccount();
            

            var e = let.ProcessCheck(0, 0, "John", -500, checking, savingsSecondary);
            
            Assert.False(e);
            Assert.AreEqual(0, let.PrimaryAccountBalance);
            Assert.AreEqual(0, let.SecondaryAccountBalance);
            Assert.AreEqual(-10, checking.AccountBalance);
            Assert.AreEqual(0, savingsSecondary.AccountBalance);
        }
        
        [Test]
        public void LocalCheckSenderSufficentSavingsReversed()
        {
            var let = SetupOneChecking();
            var savings = new Bank.Accounts.SavingsAccount();
            
            let.DepositTo(0, 1000);

            var e = let.ProcessCheck(0, 0, "John", -500, savings);
            
            Assert.False(e);
            Assert.AreEqual(1000, let.PrimaryAccountBalance);
        }
        
    }
    
}