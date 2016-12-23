using System;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using PocketBookSync.Data;

namespace PocketBookSync.Commands
{
    public class ListAccountsCommand
    {
        public void Build(CommandLineApplication application)
        {
            application.Command("list", c =>
            {
                c.Description = "List bank accounts";
                c.OnExecute(() =>
                {
                    ListAccountsAsync().Wait();
                    return 0;
                });
            });
        }

        public static async Task ListAccountsAsync()
        {
            using (var db = new AppDbContext())
            {
                await Migrations.MigrateAsync(db);
                var config = await db.GetConfigAsync();

                foreach (var account in config.Accounts)
                {
                    Console.WriteLine($"Type:                       {account.Type}");
                    Console.WriteLine($"Account Reference:          {account.AccountReference}");
                    Console.WriteLine($"Pocket Book Account Number: {account.PocketBookAccountNumber}");
                    Console.WriteLine();
                }
            }
        }
    }
}
