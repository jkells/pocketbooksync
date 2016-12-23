using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;

namespace PocketBookSync.Commands
{
    public class SyncAccountCommand
    {
        public void Build(CommandLineApplication application)
        {
            application.Command("sync-account", c =>
            {
                c.Description = "Sync a bank account";
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

                    SyncAccountAsync(accountReference.Value()).Wait();
                    return 0;
                });
            });
        }

        public static async Task SyncAccountAsync(string accountReference)
        {
        }
    }
}