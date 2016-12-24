using System;
using System.Collections.Generic;
using PocketBookSync.Data;
using System.Linq;

namespace PocketBookSync.Commands
{
    public static class Synchronizer
    {
        public static IEnumerable<Transaction> Synchronize(IEnumerable<Transaction> currentTransactions, IEnumerable<Transaction> existingTransactions)
        {
            var newTransactions = new List<Transaction>();
            var ctGroups = currentTransactions.GroupBy(x => new { x.Date, x.Amount });
            var etGroups = existingTransactions.GroupBy(x => new { x.Date, x.Amount }).ToDictionary(x => x.Key);

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

                foreach (var x in ctGroup)
                {
                    foreach (var y in etGroup)
                    {
                        Console.WriteLine($"{x.Description}: {y.Description}: {Similar.Compare(x.Description, y.Description)}");
                    }
                    Console.WriteLine("--------------");
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
    }
}
