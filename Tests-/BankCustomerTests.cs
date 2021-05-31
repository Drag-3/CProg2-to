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
        [Test]
        public void CreateSavingsTest()
        {
            var let = new BankCustomer(1, "One");
            let.AddAccount('S');
            Assert.AreEqual(let.PrimaryAccount.GetType(), typeof(Bank.Accounts.SavingsAccount));
        }
        [Test]
        public void CreateCheckingTest()
        {
            var let = new BankCustomer(1, "One");
            let.AddAccount('C');
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
    }
}