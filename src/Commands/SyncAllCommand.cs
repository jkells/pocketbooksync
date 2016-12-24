using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using PocketBookSync.Data;
using PocketBookSync.Exporters;

namespace PocketBookSync.Commands
{
    public class SyncAllCommand
    {
        public void Build(CommandLineApplication application)
        {
            application.Command("sync", c =>
            {
                var useChrome = c.Option("--use-chrome", "Use chrome instead of PhantomJS", CommandOptionType.NoValue);
                c.Description = "Sync all bank accounts";
                c.OnExecute(() =>
                {
                    SyncAllAccountsAsync(useChrome.HasValue()).Wait();
                    return 0;
                });
            });
        }

        public static async Task SyncAllAccountsAsync(bool useChrome)
        {
            using (var db = new AppDbContext())
            {
                await Migrations.MigrateAsync(db);
                var config = await db.GetConfigAsync();

                using (var factory = new ExporterFactory(useChrome))
                {
                    foreach (var account in config.Accounts)
                    {
                        var exporter = factory.Create(account);
                        var currentTransactions = (await exporter.ExportRecent(account)).ToList();
                        if (!currentTransactions.Any())
                            continue;

                        var dates = currentTransactions.Select(x => x.Date);
                        var existingTransactions = db.GetTransactionsForDates(account, dates);
                        var toAdd = Synchronizer.Synchronize(currentTransactions, existingTransactions);

                        Console.WriteLine($"Account: {account.AccountReference}, {existingTransactions.Count()}, {currentTransactions.Count}, {toAdd.Count()}");
                        foreach (var transaction in toAdd)
                        {
                            await db.Transactions.AddAsync(transaction);
                        }
                        await db.SaveChangesAsync();
                    }
                }
            }
        }
    }
}