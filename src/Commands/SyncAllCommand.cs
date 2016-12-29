using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using PocketBookSync.Data;
using PocketBookSync.Exporters;
using PocketBookSync.PocketBook;

namespace PocketBookSync.Commands
{
    public class SyncAllCommand
    {
        public void Build(CommandLineApplication application)
        {
            application.Command("sync", c =>
            {
                var useChrome = c.Option("--use-chrome", "Use chrome instead of PhantomJS", CommandOptionType.NoValue);
                var noDownload = c.Option("--no-download", "Skip downloading new transactions from the bank",
                    CommandOptionType.NoValue);
                var noUpload = c.Option("--no-upload", "Skip uploading new transactions to PocketBook",
                    CommandOptionType.NoValue);
                c.Description = "Sync all bank accounts";
                c.OnExecute(() =>
                {
                    SyncAllAccountsAsync(useChrome.HasValue(), noDownload.HasValue(), noUpload.HasValue()).Wait();
                    return 0;
                });
            });
        }

        public static async Task SyncAllAccountsAsync(bool useChrome, bool noDownload, bool noUpload)
        {
            if (!noDownload)
                await DownloadAsync(useChrome);

            if (!noUpload)
                await UploadAsync();
        }

        private static async Task UploadAsync()
        {
            using (var db = new AppDbContext())
            {
                var uploader = new PocketBookUploader(db);
                foreach (var account in db.Accounts)
                {
                    await uploader.UploadAsync(account);
                }
                await db.SaveChangesAsync();
            }
        }

        private static async Task DownloadAsync(bool useChrome)
        {
            using (var db = new AppDbContext())
            {
                await Migrations.MigrateAsync(db);
                using (var factory = new ExporterFactory(useChrome))
                {
                    foreach (var account in db.Accounts)
                    {
                        await Synchronizer.SynchronizeAccountAsync(db, factory, account);
                    }
                }
                await db.SaveChangesAsync();
            }
        }
    }
}