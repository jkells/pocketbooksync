using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PocketBookSync.Data;
using PocketBookSync.Exporters;
using Enumerable = System.Linq.Enumerable;

namespace PocketBookSync.Commands
{
    public static class Synchronizer
    {
        public static async Task SynchronizeAccountAsync(AppDbContext db, ExporterFactory factory, Account account)
        {
            var exporter = factory.Create(account);
            var currentTransactions = Enumerable.ToList((await exporter.ExportRecentAsync(account)));
            if (!currentTransactions.Any())
                return;

            var dates = currentTransactions.Select(x => x.Date);
            var existingTransactions =
                db.Transactions.Where(t => t.AccountId == account.Id && Enumerable.Contains<DateTime>(dates, t.Date));
            var toAdd = FindNewTransactions(currentTransactions, existingTransactions).ToList();

            Log($"{existingTransactions.Count()} already exist, {toAdd.Count()} new transactions found");

            foreach (var transaction in toAdd)
            {
                await db.Transactions.AddAsync(transaction);
            }
        }

        public static IEnumerable<Transaction> FindNewTransactions(IEnumerable<Transaction> currentTransactions,
            IEnumerable<Transaction> existingTransactions)
        {
            var newTransactions = new List<Transaction>();
            var ctGroups = currentTransactions.GroupBy(x => new {x.Date, x.Amount});
            var etGroups = existingTransactions.GroupBy(x => new {x.Date, x.Amount}).ToDictionary(x => x.Key);

            foreach (var ctGroup in ctGroups)
            {
                // No transaction with the same grouping, they're all new.
                if (!etGroups.ContainsKey(ctGroup.Key))
                {
                    newTransactions.AddRange(ctGroup);
                    continue;
                }

                var etGroup = etGroups[ctGroup.Key];
                // Same number of transactions in this group, they're all recorded
                if (etGroup.Count() == ctGroup.Count())
                {
                    continue;
                }

                // We have some new transactions!
                if (ctGroup.Count() > etGroup.Count())
                {
                    var currentTransactionsByEditDistance = ctGroup.Select(ct => new
                    {
                        transaction = ct,
                        maxSimilarity = etGroup.Select(et => Similar.Compare(et.Description, ct.Description)).Max()
                    });

                    // Take the transactions on the same day with the same ammount that have a higher edit distance.

                    var newestTransactionsByEditDistance = currentTransactionsByEditDistance
                        .OrderBy(x => x.maxSimilarity)
                        .Take(ctGroup.Count() - etGroup.Count())
                        .Select(x => x.transaction);

                    newTransactions.AddRange(newestTransactionsByEditDistance);
                }
            }

            return newTransactions;
        }

        private static void Log(string message)
        {
            Console.WriteLine($"Synchronizer:       {message}");
        }
    }
}