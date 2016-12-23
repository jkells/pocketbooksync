using System;
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
                        Console.WriteLine($"Account: {account.AccountReference}");
                        Console.WriteLine("==================================");
                        var exporter = factory.Create(account);
                        foreach (var transaction in await exporter.ExportRecent(account))
                        {
                            Console.WriteLine($"{transaction.Date}: {transaction.Amount} - {transaction.Description}");
                        }
                        Console.WriteLine();
                    }
                }
            }
            Console.ReadKey();
        }
    }
}