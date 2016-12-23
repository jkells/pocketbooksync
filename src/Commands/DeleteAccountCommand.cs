using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using PocketBookSync.Data;

namespace PocketBookSync.Commands
{
    public class DeleteAccountCommand
    {
        public void Build(CommandLineApplication application)
        {
            application.Command("delete-account", c =>
            {
                c.Description = "Delete a bank account";
                var accountReference = c.Option("--account-reference", "Account Reference",
                    CommandOptionType.SingleValue);
                c.HelpOption("-? | -h | --help");
                c.OnExecute(() =>
                {
                    if (!accountReference.HasValue())
                    {
                        c.ShowHelp();
                        return 1;
                    }

                    DeleteAccountAsync(accountReference.Value()).Wait();
                    return 0;
                });
            });
        }

        public static async Task DeleteAccountAsync(string accountReference)
        {
            using (var db = new AppDbContext())
            {
                await Migrations.MigrateAsync(db);
                var config = await db.GetConfigAsync();
                config.Accounts = config.Accounts.Where(x => x.AccountReference != accountReference);
                await db.SetConfigAsync(config);
                await db.SaveChangesAsync();
            }
        }
    }
}