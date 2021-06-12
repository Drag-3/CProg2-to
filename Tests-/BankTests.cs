using FinancialAudit.Bank;
using NUnit;
using NUnit.Framework;

namespace Tests
{
    public class BankTests
    {
        public Bank bank;
        
        [SetUp]
        public void SetUp()
        {
            bank = new Bank();
            bank.AddCustomer("John", 1);
            bank.AddCustomer("Max", 2);
            bank.CreateAccount(1, 'C', 0.02m);
            bank.CreateAccount(1, 'S', 0.002m);
            bank.CreateAccount(2, 'C', 0.12m);
            //bank.CreateAccount(2, 'S', 0.00543m);
        }

        [Test]
        public void LocalCheckSenderSufficient()
        {
            bank.Deposit(1, true, 1000);
            bank.ProcessCheck(1, true, 1, "Max", 456, 2, true);

            var exp = 1000 - 456;
            var bal = bank.CustomerRepository[2].PrimaryAccountBalance;

            Assert.AreEqual(456, bal);
            Assert.AreEqual(exp, bank.CustomerRepository[1].PrimaryAccountBalance);
        }
        
        [Test]
        public void LocalCheckSenderInsufficient()
        {
            //bank.Deposit(1, true, 1000);
            bank.ProcessCheck(1, true, 1, "Max", 456, 2, true);
            
            var bal = bank.CustomerRepository[2].PrimaryAccountBalance;

            Assert.AreEqual(0, bal);
            Assert.AreEqual(-10, bank.CustomerRepository[1].PrimaryAccountBalance);
        }
    }
}