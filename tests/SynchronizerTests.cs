using System;
using PocketBookSync.Commands;
using PocketBookSync.Data;
using Xunit;
using Enumerable = System.Linq.Enumerable;
using System.Linq;

namespace PocketBookSync.tests
{
    public class SynchronizerTests
    {
        [Fact]
        public void only_new_transactions_return_all()
        {
            var date = DateTime.UtcNow;
            var currentTransactions = new[]
            {
                new Transaction {AccountId = 1, Amount = 5, Date = date, Description = "Transaction 1"},
                new Transaction {AccountId = 1, Amount = 5, Date = date, Description = "Transaction 2"},
                new Transaction {AccountId = 1, Amount = 6, Date = date, Description = "Transaction 3"}
            };

            var newTransactions = Synchronizer.FindNewTransactions(currentTransactions, new Transaction[0]);
            Assert.Equal(3, Enumerable.Count(newTransactions));
        }

        [Fact]
        public void no_new_transactions_return_none()
        {
            var date = DateTime.UtcNow;
            var currentTransactions = new[]
            {
                new Transaction {AccountId = 1, Amount = 5, Date = date, Description = "Transaction 1"},
                new Transaction {AccountId = 1, Amount = 5, Date = date, Description = "Transaction 2"},
                new Transaction {AccountId = 1, Amount = 6, Date = date, Description = "Transaction 3"}
            };
            var existingTransactions = new[]
            {
                new Transaction {AccountId = 1, Amount = 5, Date = date, Description = "Transaction 1"},
                new Transaction {AccountId = 1, Amount = 5, Date = date, Description = "Transaction 2"},
                new Transaction {AccountId = 1, Amount = 6, Date = date, Description = "Transaction 3"}
            };


            var newTransactions = Synchronizer.FindNewTransactions(currentTransactions, existingTransactions);
            Assert.Equal(0, Enumerable.Count(newTransactions));
        }

        [Fact]
        public void finds_new_transactions_with_different_ammounts()
        {
            var date = DateTime.UtcNow;
            var currentTransactions = new[]
            {
                new Transaction {AccountId = 1, Amount = 5, Date = date, Description = "Transaction 1"},
                new Transaction {AccountId = 1, Amount = 5, Date = date, Description = "Transaction 2"},
                new Transaction {AccountId = 1, Amount = 6, Date = date, Description = "Transaction 3"},
                new Transaction {AccountId = 1, Amount = 7, Date = date, Description = "Transaction 4"},
                new Transaction {AccountId = 1, Amount = 8, Date = date, Description = "Transaction 5"}
            };
            var existingTransactions = new[]
            {
                new Transaction {AccountId = 1, Amount = 5, Date = date, Description = "Transaction 1"},
                new Transaction {AccountId = 1, Amount = 5, Date = date, Description = "Transaction 2"},
                new Transaction {AccountId = 1, Amount = 6, Date = date, Description = "Transaction 3"}
            };


            var newTransactions =
                Enumerable.ToList(Synchronizer.FindNewTransactions(currentTransactions, existingTransactions));
            Assert.Equal(2, newTransactions.Count());
            Assert.Equal(1, newTransactions.Count(x => x.Amount == 7));
            Assert.Equal(1, newTransactions.Count(x => x.Amount == 8));
        }


        [Fact]
        public void finds_new_transactions_with_different_dates()
        {
            var date = DateTime.UtcNow;
            var currentTransactions = new[]
            {
                new Transaction {AccountId = 1, Amount = 5, Date = date, Description = "Transaction 1"}
            };
            var existingTransactions = new[]
            {
                new Transaction
                {
                    AccountId = 1,
                    Amount = 5,
                    Date = date.Subtract(TimeSpan.FromDays(2)),
                    Description = "Transaction 1"
                }
            };


            var newTransactions = Synchronizer.FindNewTransactions(currentTransactions, existingTransactions);
            Assert.Equal(1, Enumerable.Count(newTransactions));
        }

        [Fact]
        public void chooses_most_different_transaction_description_1()
        {
            var date = DateTime.UtcNow;
            var currentTransactions = new[]
            {
                new Transaction {AccountId = 1, Amount = 5, Date = date, Description = "Transaction 1"},
                new Transaction
                {
                    AccountId = 1,
                    Amount = 5,
                    Date = date,
                    Description = "Transaction 2 - Now with more info"
                },
                new Transaction {AccountId = 1, Amount = 5, Date = date, Description = "Transact i on 3 // 220"},
                new Transaction {AccountId = 1, Amount = 5, Date = date, Description = "Netflix Pty ltd"},
                new Transaction {AccountId = 1, Amount = 5, Date = date, Description = "Stan"}
            };

            var existingTransactions = new[]
            {
                new Transaction {AccountId = 1, Amount = 5, Date = date, Description = "Transaction 1"},
                new Transaction {AccountId = 1, Amount = 5, Date = date, Description = "Transaction 2"},
                new Transaction {AccountId = 1, Amount = 5, Date = date, Description = "Transaction 3"},
                new Transaction {AccountId = 1, Amount = 5, Date = date, Description = "Netflix"}
            };

            var newTransactions =
                Enumerable.ToList(Synchronizer.FindNewTransactions(currentTransactions, existingTransactions));
            Assert.Equal(1, newTransactions.Count());
            Assert.Equal((string) "Stan", (string) newTransactions.First().Description);
        }
    }
}