using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using PocketBookSync.Data;
using PocketBookSync.Exporters;
using PocketBookSync.PocketBook;

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
                var noDownload = c.Option("--no-download", "Skip downloading new transactions from the bank",
                    CommandOptionType.NoValue);
                var noUpload = c.Option("--no-upload", "Skip uploading new transactions to PocketBook",
                    CommandOptionType.NoValue);
                var useChrome = c.Option("--use-chrome", "Use chrome instead of PhantomJS", CommandOptionType.NoValue);
                c.HelpOption("-? | -h | --help");
                c.OnExecute(() =>
                {
                    if (!accountReference.HasValue())
                    {
                        c.ShowHelp();
                        return 1;
                    }

                    SyncAccountAsync(accountReference.Value(), useChrome.HasValue(), noDownload.HasValue(),
                        noUpload.HasValue()).Wait();
                    return 0;
                });
            });
        }

        public static async Task SyncAccountAsync(string accountReference, bool useChrome, bool noDownload,
            bool noUpload)
        {
            Account account;
            using (var db = new AppDbContext())
            {
                await Migrations.MigrateAsync(db);
                account = db.Accounts.FirstOrDefault(x => x.AccountReference == accountReference);
                if (account == null)
                    throw new AppException("Account not found");
            }

            if (!noDownload)
                await DownloadAsync(useChrome, account);

            if (!noUpload)
                await UploadAsync(useChrome, account);
        }

        private static async Task UploadAsync(bool useChrome, Account account)
        {
            using (var db = new AppDbContext())
            {
                var uploader = new PocketBookUploader(db);
                await uploader.UploadAsync(account);
                await db.SaveChangesAsync();
            }
        }

        private static async Task DownloadAsync(bool useChrome, Account account)
        {
            using (var db = new AppDbContext())
            {
                using (var factory = new ExporterFactory(useChrome))
                {
                    await Synchronizer.SynchronizeAccountAsync(db, factory, account);
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}