using System.Runtime.InteropServices;
using FinancialAudit.Accounts;
using NUnit.Framework;
using FinancialAudit.Bank;

namespace Tests
{
    public class BankCustomerTests
    {
        
        [SetUp]
        public void Setup()
        {
        }

        public Customer SetupOneChecking()
        {
            var customer = new Customer(1, "yes");
            customer.AddAccount('C');
            return customer;
        }
        public Customer SetupOneSavings()
        {
            var customer = new Customer(1, "yes");
            customer.AddAccount('S');
            return customer;
        }
        public Customer SetupCheckings()
        {
            var customer = new Customer(1, "yes");
            customer.AddAccount('C');
            customer.AddAccount('C');
            return customer;
        }
        public Customer SetupSavings()
        {
            var customer = new Customer(1, "yes");
            customer.AddAccount('S');
            customer.AddAccount('S');
            return customer;
        }
        public Customer SetupCheckingSavings()
        {
            var customer = new Customer(1, "yes");
            customer.AddAccount('C');
            customer.AddAccount('S');
            return customer;
        }
        [Test]
        public void CreateSavingsTest()
        {
            var let = SetupOneSavings();
            Assert.AreEqual(let.PrimaryAccount.GetType(), typeof(SavingsAccount));
        }
        [Test]
        public void CreateCheckingTest()
        {
            var let = SetupOneChecking();
            Assert.AreEqual(let.PrimaryAccount.GetType(), typeof(CheckingAccount));
        }
        [Test]
        public void LinkTest()
        {
            var let = new Customer(1, "One");
            var accountToLink = new CheckingAccount();
            let.AddAccount(accountToLink);
            Assert.AreEqual(accountToLink, let.PrimaryAccount);
        }
        [Test]
        public void DepositTest()
        {
            var let = SetupOneChecking();
            let.DepositTo(AccountLoc.PrimaryAccount, 12345);
            Assert.AreEqual(12345, let.PrimaryAccountBalance);
        }
        
        [Test]
        public void SufficientWithdrawalTest()
        {
            var let = SetupOneSavings();
            let.DepositTo(0, 1200);
            var e  =let.WithdrawFrom(AccountLoc.PrimaryAccount, 7.3453M);
            var solution = 1200 - 7.3453M;
            Assert.True(e);
            Assert.AreEqual(solution, let.PrimaryAccountBalance);
        }

        
        [Test]
        public void SufficientWithdrawalTestWithSecondary()
        {
            var let = SetupCheckingSavings();

            let.DepositTo(AccountLoc.PrimaryAccount, 1000);
            let.DepositTo(AccountLoc.SecondaryAccount, 1000);

            var e =let.WithdrawFrom(AccountLoc.PrimaryAccount, 510);
            
            Assert.True(e);
            Assert.AreEqual(490, let.PrimaryAccountBalance);
            Assert.AreEqual(1000, let.SecondaryAccountBalance);
        }
        [Test]
        public void InsufficientWithdrawalTest()
        {
            var let = SetupOneChecking();
            let.DepositTo(AccountLoc.PrimaryAccount, 5);
            var e =let.WithdrawFrom(AccountLoc.PrimaryAccount, 150);
            Assert.False(e);
            Assert.AreEqual(-5, let.PrimaryAccountBalance);
        }

        [Test]
        public void InsufficientWithdrawalTestWithSecondarySufficient()
        {
            var let = SetupCheckingSavings();
            
            let.DepositTo(AccountLoc.PrimaryAccount, 10);
            let.DepositTo(AccountLoc.SecondaryAccount, 1000);

            var e = let.WithdrawFrom(AccountLoc.PrimaryAccount, 510);
            
            Assert.True(e);
            Assert.AreEqual(0, let.PrimaryAccountBalance);
            Assert.AreEqual(500, let.SecondaryAccountBalance);
        }

        [Test]
        public void InsufficientWithdrawalTestWithSecondaryInsufficient()
        {
            var let = SetupCheckingSavings();

            let.DepositTo(AccountLoc.PrimaryAccount, 10);
            let.DepositTo(AccountLoc.SecondaryAccount, 1000);

            var e = let.WithdrawFrom(AccountLoc.PrimaryAccount, 2000);

            Assert.False(e);
            Assert.AreEqual(0, let.PrimaryAccountBalance); // Penalty deducted
            Assert.AreEqual(1000, let.SecondaryAccountBalance); // Unaffected
        }
        
        [Test]
        public void GlobalCheckSenderSufficient()
        {
            var let = SetupOneChecking();
            
            let.DepositTo(AccountLoc.PrimaryAccount, 1000);

            var e = let.ProcessCheck(0, AccountLoc.PrimaryAccount, "John", 500);
            
            Assert.True(e);
            Assert.AreEqual(500, let.PrimaryAccountBalance);
        }
        
        [Test]
        public void GlobalCheckSenderPrimaryInsufficient()
        {
            var let = SetupOneChecking();
            
            let.DepositTo(AccountLoc.PrimaryAccount, 5);

            var e = let.ProcessCheck(0, AccountLoc.PrimaryAccount, "John", 500);
            
            Assert.False(e);
            Assert.AreEqual(-5, let.PrimaryAccountBalance);
        }
        [Test]
        public void GlobalCheckSenderPrimaryInsufficientSecondarySufficient()
        {
            var let = SetupCheckingSavings();
            
            let.DepositTo(AccountLoc.SecondaryAccount, 1000);

            var e = let.ProcessCheck(0, AccountLoc.PrimaryAccount, "John", 500);
            
            Assert.True(e);
            Assert.AreEqual(0, let.PrimaryAccountBalance);
            Assert.AreEqual(500, let.SecondaryAccountBalance);
        }
        [Test]
        public void GlobalCheckSenderPrimaryInsufficientSecondaryInsufficient()
        {
            var let = SetupCheckingSavings();
            

            var e = let.ProcessCheck(0, AccountLoc.PrimaryAccount, "John", 500);
            
            Assert.False(e);
            Assert.AreEqual(-10, let.PrimaryAccountBalance);
            Assert.AreEqual(0, let.SecondaryAccountBalance);
        }
        
        [Test]
        public void GlobalCheckSenderSufficientSavings()
        {
            var let = SetupOneSavings();
            
            let.DepositTo(AccountLoc.PrimaryAccount, 1000);

            var e = let.ProcessCheck(0, AccountLoc.PrimaryAccount, "John", 500);
            
            Assert.False(e);
            Assert.AreEqual(1000, let.PrimaryAccountBalance);
        }
        
        [Test]
        public void GlobalCheckSenderSufficientReversed()
        {
            var let = SetupOneChecking();
            
            let.DepositTo(AccountLoc.PrimaryAccount, 1000);

            var e = let.ProcessCheck(0, AccountLoc.PrimaryAccount, "John", -500);
            
            Assert.AreEqual(1500, let.PrimaryAccountBalance);
            Assert.True(e);
            
        }

        [Test]
        public void GlobalCheckSenderSufficientSavingsReversed()
        {
            var let = SetupOneSavings();
            
            let.DepositTo(AccountLoc.PrimaryAccount, 1000);

            var e = let.ProcessCheck(0, AccountLoc.PrimaryAccount, "John", -500);
            
            Assert.True(e);
            Assert.AreEqual(1500, let.PrimaryAccountBalance); // Savings accounts can accept checks but not send them
        }
        [Test]
        public void LocalCheckSenderSufficient()
        {
            var let = SetupOneChecking();
            var checking = new CheckingAccount();
            let.DepositTo(AccountLoc.PrimaryAccount, 1000);

            var e = let.ProcessCheck(0, AccountLoc.PrimaryAccount, "John", 500, checking);
            
            Assert.True(e);
            Assert.AreEqual(500, let.PrimaryAccountBalance);
            Assert.AreEqual(500, checking.AccountBalance);
        }
        
        [Test]
        public void LocalCheckSenderPrimaryInsufficientSecondarySufficient()
        {
            var let = SetupCheckingSavings();
            var checking = new CheckingAccount();
            
            let.DepositTo(AccountLoc.SecondaryAccount, 1000);

            var e = let.ProcessCheck(0, AccountLoc.PrimaryAccount, "John", 500, checking);
            
            Assert.True(e);
            Assert.AreEqual(0, let.PrimaryAccountBalance);
            Assert.AreEqual(500, let.SecondaryAccountBalance);
            Assert.AreEqual(500, checking.AccountBalance);
        }
        
        [Test]
        public void LocalCheckSenderPrimaryInsufficientSecondaryInsufficient()
        {
            var let = SetupCheckingSavings();
            var checking = new CheckingAccount();
            

            var e = let.ProcessCheck(0, AccountLoc.PrimaryAccount, "John", 500, checking);
            
            Assert.False(e);
            Assert.AreEqual(-10, let.PrimaryAccountBalance);
            Assert.AreEqual(0, let.SecondaryAccountBalance);
            Assert.AreEqual(0, checking.AccountBalance);
        }
        
        [Test]
        public void LocalCheckSenderSufficientSavings()
        {
            var let = SetupOneSavings();
            var checking = new CheckingAccount();
            
            let.DepositTo(AccountLoc.PrimaryAccount, 1000);

            var e = let.ProcessCheck(0, AccountLoc.PrimaryAccount, "John", 500, checking);
            
            Assert.False(e);
            Assert.AreEqual(1000, let.PrimaryAccountBalance);
        }
        
        [Test]
        public void LocalCheckSenderSufficientReversed()
        {
            var let = SetupOneChecking();
            var checking = new CheckingAccount();
            checking.Deposit(1000);

            var e = let.ProcessCheck(0, AccountLoc.PrimaryAccount, "John", -500, checking);
            
            Assert.True(e);
            Assert.AreEqual(500, let.PrimaryAccountBalance);
            Assert.AreEqual(500, checking.AccountBalance);
        }
        
        [Test]
        public void LocalCheckSenderPrimaryInsufficientSecondarySufficientReversed()
        {
            var let = SetupCheckingSavings();
            var checking = new CheckingAccount();
            var savingsSecondary = new SavingsAccount();
            
            savingsSecondary.Deposit(1000);

            var e = let.ProcessCheck(0, AccountLoc.PrimaryAccount, "John", -500, checking, savingsSecondary);
            
            Assert.True(e);
            Assert.AreEqual(500, let.PrimaryAccountBalance);
            Assert.AreEqual(0, let.SecondaryAccountBalance);
            Assert.AreEqual(0, checking.AccountBalance);
            Assert.AreEqual(500, savingsSecondary.AccountBalance);
        }
        
        [Test]
        public void LocalCheckSenderPrimaryInsufficientSecondaryInsufficientReversed()
        {
            var let = SetupCheckingSavings();
            var checking = new CheckingAccount();
            var savingsSecondary = new SavingsAccount();
            

            var e = let.ProcessCheck(0, AccountLoc.PrimaryAccount, "John", -500, checking, savingsSecondary);
            
            Assert.False(e);
            Assert.AreEqual(0, let.PrimaryAccountBalance);
            Assert.AreEqual(0, let.SecondaryAccountBalance);
            Assert.AreEqual(-10, checking.AccountBalance);
            Assert.AreEqual(0, savingsSecondary.AccountBalance);
        }
        
        [Test]
        public void LocalCheckSenderSufficientSavingsReversed()
        {
            var let = SetupOneChecking();
            var savings = new SavingsAccount();
            
            let.DepositTo(AccountLoc.PrimaryAccount, 1000);

            var e = let.ProcessCheck(0, AccountLoc.PrimaryAccount, "John", -500, savings);
            
            Assert.False(e);
            Assert.AreEqual(1000, let.PrimaryAccountBalance);
        }
        
    }
    
}