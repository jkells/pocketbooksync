using System;
using Microsoft.Extensions.CommandLineUtils;
using System.Threading.Tasks;
using System.Linq;
using PocketBookSync.Data;
using PocketBookSync.Exporters;

namespace PocketBookSync
{

    public class Program
    {
        private static string ReadPassword()
        {
            string password = "";
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, (password.Length - 1));
                    Console.Write("\b \b");
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    return password;
                }
                else
                {
                    password += key.KeyChar;
                }
            }
        }


        public static async Task AddAccountAsync(string clientNumber, string password, string accountReference, string pocketBookAccountNumber, string type)
        {
            using (var db = new AppDbContext())
            {
                await Migrations.MigrateAsync(db);
                var config = await db.GetConfigAsync();
                if (config.Accounts.Any(x => x.AccountReference == accountReference))
                {
                    Console.WriteLine("Account already added");
                    return;
                }

                config.Accounts = config.Accounts.Concat(new[]{
                    new Account
                    {
                        Username = clientNumber,
                        AccountReference = accountReference,
                        PocketBookAccountNumber = pocketBookAccountNumber,
                        Password = password,
                        Type = type
                    }
                });

                await db.SetConfigAsync(config);                
                await db.SaveChangesAsync();
            }
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

        public static async Task SyncAccountAsync(string accountReference)
        {

        }

        public static async Task SyncAllAccountsAsync()
        {
            using (var db = new AppDbContext())
            {
                await Migrations.MigrateAsync(db);
                var config = await db.GetConfigAsync();
                foreach (var account in config.Accounts)
                {
                    using (var exporter = ExporterFactory.Create(account))
                    {
                        Console.WriteLine($"Account: {account.AccountReference}");
                        Console.WriteLine("==================================");
                        foreach (var transaction in await exporter.ExportRecent())
                        {
                            Console.WriteLine($"{transaction.Date}: {transaction.Amount} - {transaction.Description}");
                        }
                        Console.WriteLine();
                    }
                }
            }
            Console.ReadKey();
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


        public static void Main(string[] args)
        {
            CommandLineApplication commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: false);            
            commandLineApplication.Command("add-account", c =>
            {
                c.Description = "Add a bank account";
                var clientNumber = c.Option("--client-number", "Client Number", CommandOptionType.SingleValue);
                var accountReference = c.Option("--account-reference", "Account Reference", CommandOptionType.SingleValue);
                var pocketBookAccountNumber = c.Option("--pocket-book-account", "Pocket Book Account Number", CommandOptionType.SingleValue);
                var type = c.Option("--type", "Type", CommandOptionType.SingleValue);
                c.HelpOption("-? | -h | --help");
                c.OnExecute(() =>
                {
                    if (!clientNumber.HasValue() || !accountReference.HasValue() || !type.HasValue() || !pocketBookAccountNumber.HasValue())
                    {
                        c.ShowHelp();
                        return 1;
                    }

                    Console.WriteLine("Enter Password: ");
                    var password = ReadPassword();
                    AddAccountAsync(clientNumber.Value(), password, accountReference.Value(), pocketBookAccountNumber.Value(), type.Value()).Wait();
                    return 0;
                });
            });

            commandLineApplication.Command("delete-account", c =>
            {
                c.Description = "Delete a bank account";
                var accountReference = c.Option("--account-reference", "Account Reference", CommandOptionType.SingleValue);
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

            commandLineApplication.Command("sync-account", c =>
            {
                c.Description = "Sync a bank account";
                var accountReference = c.Option("--account-reference", "Account Reference", CommandOptionType.SingleValue);
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

            commandLineApplication.Command("sync", c =>
            {
                c.Description = "Sync all bank accounts";
                c.OnExecute(() =>
                {
                    SyncAllAccountsAsync().Wait();
                    return 0;
                });
            });

            commandLineApplication.Command("list", c =>
            {
                c.Description = "List bank accounts";
                c.OnExecute(() =>
                {
                    ListAccountsAsync().Wait();
                    return 0;
                });
            });

            commandLineApplication.HelpOption("-? | -h | --help");
            commandLineApplication.OnExecute(() =>
            {
                return 0;
            });
            commandLineApplication.Execute(args);
        }
    }
}
